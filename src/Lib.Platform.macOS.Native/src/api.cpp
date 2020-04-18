// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2017 AirVPN (support@airvpn.org) / https://airvpn.org
//
// Eddie is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Eddie is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Eddie. If not, see <http://www.gnu.org/licenses/>.
// </eddie_source_header>

#include "stdafx.h"
#include "api.h"

#include <arpa/inet.h>
#include <memory>
#include <mutex>
#include <netinet/ip.h>
#include <netinet/ip_icmp.h>
#include <netinet/icmp6.h>
#include <signal.h>
#include <stdio.h>
#include <stdlib.h>
#include <string>
#include <string.h>
#include <sys/ioctl.h>
#include <sys/stat.h>
#include <sys/time.h>
#include <time.h>
#include <unistd.h>

#include <sys/sysctl.h> // used by eddie_get_realtime_network_stats
#include <netinet/in.h> // used by eddie_get_realtime_network_stats
#include <net/if.h> // used by eddie_get_realtime_network_stats
#include <net/route.h> // used by eddie_get_realtime_network_stats

#include <curl/curl.h> // Debian: libcurl4-openssl-dev
#include "json.hpp"
using json = nlohmann::json;

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#define EDDIE_PING_BUFFER_SIZE 256

typedef std::lock_guard<std::mutex> mutex_lock;

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifdef __cplusplus
extern "C" {
#endif

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

static std::mutex g_eddie_ping_cs;
static uint16_t g_eddie_ping_id = (uint16_t) time(NULL);

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

static uint16_t eddie_ip_checksum(const uint16_t *addr, uint32_t len)
{
	uint32_t sum = 0;

	while(len > 1)
	{
		sum += *addr++;
		len -= 2;
	}

	if(len > 0)
		sum += * (unsigned char *) addr;

	sum = (sum >> 16) + (sum & 0xffff);
	sum += (sum >> 16);

	return ~sum;
}

static double eddie_get_time()
{
	struct timeval now;
	gettimeofday(&now, NULL);
	return (double)((now.tv_sec * 1000) + (now.tv_usec / 1000));
}

static uint16_t eddie_generate_icmp_id()
{
    mutex_lock lock(g_eddie_ping_cs);
    return g_eddie_ping_id++;
}
    
static int eddie_file_get_flags(const char *filename)
{
    struct stat s;
    EDDIE_ZEROMEMORY(&s, sizeof(struct stat));
    
    if(stat(filename, &s) == -1)
        return -1;
    
    return (int) s.st_flags;
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

class IPinger
{
// Construction
protected:
    IPinger(const std::string &address, int timeout) : m_address(address),
                                                       m_timeout(timeout),
                                                       m_socket(-1)
    {
        EDDIE_ZEROMEMORY(m_responseBuffer, EDDIE_PING_BUFFER_SIZE);
    }
        
public:
    virtual ~IPinger()
    {
        if(m_socket != -1)
        {
            close(m_socket);
            m_socket = -1;
        }
    }
        
// Attributes
public:
    inline const std::string & getAddress() const { return m_address; }
        
// Operations
public:
    int ping()
    {
        if(init() == false)
            return -1;
            
        u_int16_t requestID = eddie_generate_icmp_id();
        if(createRequest(requestID) == false)
            return -1;
            
        return sendRequest(requestID);
    }
        
private:
    int sendRequest(u_int16_t requestID)
    {
        fd_set read_set;
        EDDIE_ZEROMEMORY(&read_set, sizeof(read_set));
            
        double start_time = eddie_get_time();
            
        // Sends the echo request to the destination address
        if(sendto(m_socket, getRequestData(), getRequestSize(), 0, getAddressData(), (socklen_t) getAddressSize()) <= 0)
            return -1;
            
        struct timeval receive_timeout;
        int available_timeout = m_timeout;
            
        for(;;)
        {
            if(available_timeout == 0)
                return -1;
                
            // Read set and receive_timeout must be reset at each loop
            FD_SET(m_socket, &read_set);
            receive_timeout.tv_sec = available_timeout / 1000;
            receive_timeout.tv_usec = (available_timeout % 1000) * 1000;
                
            // Wait for a response
            int result = select(m_socket + 1, &read_set, NULL, NULL, &receive_timeout);
            if(result == -1)
                return -1;   // If we got a socket error avoid looping uselessy
                
            int delta = (int) (eddie_get_time() - start_time);
            if(delta < m_timeout)
                available_timeout = m_timeout - delta;
            else
                available_timeout = 0;
                
            if(result <= 0)
                continue;
                
            EDDIE_ZEROMEMORY(m_responseBuffer, EDDIE_PING_BUFFER_SIZE);
            result = (int) recvfrom(m_socket, m_responseBuffer, EDDIE_PING_BUFFER_SIZE, 0, NULL, NULL);
            if(result == -1)
                return -1;
                
            if(parseResponse(requestID, m_responseBuffer, result))
                // Got it, save the elapsed milliseconds
                return delta;
                
            // Here we got an ICMP packet of different type (i.e.: ICMP_DEST_UNREACH) or a response to another ICMP request (just ignore it and wait again until the timeout)
        }
            
        return -1;
    }
        
// Interface
protected:
    virtual const struct sockaddr * getAddressData() const = 0;
    virtual size_t getAddressSize() const = 0;
        
    virtual const void * getRequestData() const = 0;
    virtual size_t getRequestSize() const = 0;
        
    virtual bool init() = 0;
    virtual bool createRequest(u_int16_t requestID) = 0;
    virtual bool parseResponse(u_int16_t requestID, const char *data, size_t size) = 0;
        
protected:
    std::string m_address;
    int m_timeout;
    int m_socket;
        
private:
    char m_responseBuffer[EDDIE_PING_BUFFER_SIZE];
};
    
class PingerV4 : public IPinger
{
// Construction
public:
    PingerV4(const std::string &address, int timeout) : IPinger(address, timeout)
    {
        EDDIE_ZEROMEMORY(&m_requestAddress, sizeof(struct sockaddr_in));
        EDDIE_ZEROMEMORY(&m_requestHeader, sizeof(struct icmp));
    }
        
    virtual ~PingerV4()
    {
            
    }
        
// IPinger interface
public:
    virtual const struct sockaddr * getAddressData() const override
    {
        return (const struct sockaddr *) &m_requestAddress;
    }
        
    virtual size_t getAddressSize() const override
    {
        return sizeof(m_requestAddress);
    }
        
    virtual const void * getRequestData() const override
    {
        return &m_requestHeader;
    }
        
    virtual size_t getRequestSize() const override
    {
        return sizeof(m_requestHeader);
    }
        
    virtual bool init() override
    {
        m_socket = socket(PF_INET, SOCK_RAW, IPPROTO_ICMP);
        if(m_socket < 0)
            return false;
            
        int hdrincl = 0;
        if(setsockopt(m_socket, IPPROTO_IP, IP_HDRINCL, &hdrincl, sizeof(hdrincl)) == -1)
            return false;
            
        m_requestAddress.sin_family = AF_INET;
        m_requestAddress.sin_port = 0;
        m_requestAddress.sin_addr.s_addr = inet_addr(getAddress().c_str());   // The result is already in network byte order
        if(m_requestAddress.sin_addr.s_addr == INADDR_NONE)
            return -1;
            
        return true;
    }
        
    virtual bool createRequest(u_int16_t requestID) override
    {
        EDDIE_ZEROMEMORY(&m_requestHeader, sizeof(struct icmp));
        m_requestHeader.icmp_hun.ih_idseq.icd_id = htons(requestID);        // Creates a random ID for the echo request
        m_requestHeader.icmp_hun.ih_idseq.icd_seq = 0;
        m_requestHeader.icmp_type = ICMP_ECHO;
        m_requestHeader.icmp_cksum = eddie_ip_checksum((const uint16_t *) (&m_requestHeader), sizeof(struct icmp));
            
        return true;
    }
        
    virtual bool parseResponse(u_int16_t requestID, const char *data, size_t size) override
    {
        if(size < (sizeof(struct ip) + sizeof(struct icmp)))
            return false;
            
        struct icmp *response_header = (struct icmp *)(data + sizeof(struct ip));
        // Check that we got a reply for our echo request
        return ((response_header->icmp_type == ICMP_ECHOREPLY) && (ntohs(response_header->icmp_hun.ih_idseq.icd_id) == requestID));
    }
        
private:
    struct sockaddr_in m_requestAddress;
    struct icmp m_requestHeader;
};
    
class PingerV6 : public IPinger
{
// Construction
public:
    PingerV6(const std::string &address, int timeout) : IPinger(address, timeout)
    {
        EDDIE_ZEROMEMORY(&m_requestAddress, sizeof(struct sockaddr_in6));
        EDDIE_ZEROMEMORY(&m_requestHeader, sizeof(struct icmp6_hdr));
    }
        
    virtual ~PingerV6()
    {
            
    }
        
// IPinger interface
public:
    virtual const struct sockaddr * getAddressData() const override
    {
        return (const struct sockaddr *) &m_requestAddress;
    }
        
    virtual size_t getAddressSize() const override
    {
        return sizeof(m_requestAddress);
    }
        
    virtual const void * getRequestData() const override
    {
        return &m_requestHeader;
    }
        
    virtual size_t getRequestSize() const override
    {
        return sizeof(m_requestHeader);
    }
        
    virtual bool init() override
    {
        m_socket = socket(PF_INET6, SOCK_RAW, IPPROTO_ICMPV6);
        if(m_socket < 0)
            return false;
            
        // It doesn't work on MacOS
        //int hdrincl = 0;
        //if(setsockopt(m_socket, IPPROTO_IPV6, IP_HDRINCL, &hdrincl, sizeof(hdrincl)) == -1)
        //return false;
            
        m_requestAddress.sin6_family = AF_INET6;
        m_requestAddress.sin6_port = 0;
        return inet_pton(AF_INET6, getAddress().c_str(), &m_requestAddress.sin6_addr) > 0;
    }
        
    virtual bool createRequest(u_int16_t requestID) override
    {
        EDDIE_ZEROMEMORY(&m_requestHeader, sizeof(struct icmp6_hdr));
        m_requestHeader.icmp6_id = htons(requestID);
        m_requestHeader.icmp6_seq = 0;
        m_requestHeader.icmp6_type = ICMP6_ECHO_REQUEST;
        m_requestHeader.icmp6_code = 0;
        m_requestHeader.icmp6_cksum = eddie_ip_checksum((const uint16_t *) (&m_requestHeader), sizeof(struct icmp6_hdr));
            
        return true;
    }
        
    virtual bool parseResponse(u_int16_t requestID, const char *data, size_t size) override
    {
        /*
        if(size < (sizeof(struct ip6_hdr) + sizeof(struct icmp6_hdr)))
            return false;
             
        struct icmp6_hdr *response_header = (struct icmp6_hdr *)(data + sizeof(struct ip6_hdr));
        // Check that we got a reply for our echo request
        return ((response_header->icmp6_type == ICMP6_ECHO_REPLY) && (ntohs(response_header->icmp6_id) == requestID));
        */
            
        // TODO: here the buffer doesn't seem to include the ip6_hdr while in ipv4 is included (is there an issue with IP_HDRINCL?)
            
        if(size < sizeof(struct icmp6_hdr))
            return false;
            
        struct icmp6_hdr *response_header = (struct icmp6_hdr *)(data);
        // Check that we got a reply for our echo request
        return ((response_header->icmp6_type == ICMP6_ECHO_REPLY) && (ntohs(response_header->icmp6_id) == requestID));
    }
        
private:
    struct sockaddr_in6 m_requestAddress;
    struct icmp6_hdr m_requestHeader;
};

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
int eddie_init()
{
	return 0;
}

int eddie_file_get_mode(const char *filename)
{
    struct stat s;
    EDDIE_ZEROMEMORY(&s, sizeof(struct stat));

    if(stat(filename, &s) == -1)
        return -1;

    return (int) s.st_mode;
}

int eddie_file_set_mode(const char *filename, int mode)
{
    return chmod(filename, (mode_t) mode);
}

int eddie_file_set_mode_str(const char *filename, const char *mode)
{
    return eddie_file_set_mode(filename, (int) strtol(mode, NULL, 8));
}

int eddie_file_get_immutable(const char *filename)
{
    int result = eddie_file_get_flags(filename);
    if(result == -1)
        return -1;
    
    return (result & SF_IMMUTABLE) == SF_IMMUTABLE;
}

/*
int eddie_file_set_immutable(const char *filename, int flag)
{
    // sudo chflags schg /path/to/file
    // sudo chflags noschg /path/to/file
    
    int result = eddie_file_get_flags(filename);
    if(result == -1)
        return -1;
    
    result = flag ? (result | SF_IMMUTABLE) : (result & ~SF_IMMUTABLE);
    if(chflags(filename, result) == -1)
        return -1;
    
    return 0;
}
*/

bool eddie_file_get_runasroot(const char *filename)
{
    struct stat s;
    memset(&s, 0, sizeof(struct stat));

    if(stat(filename, &s) == -1)
        return false;

    bool ownedByRoot = (s.st_uid == 0);
    bool haveSetUID = (s.st_mode & S_ISUID);

    return (ownedByRoot && haveSetUID);
}

/*
int eddie_ip_ping(const char *address, int timeout)
{
    if(address == NULL)
        return -1;

    // Looks for ':' char instead of '.' for compatibility with the notation "::a.b.c.d"
    bool isv6 = strchr(address, ':') != NULL;
    
    struct sockaddr_in request_address4;
    EDDIE_ZEROMEMORY(&request_address4, sizeof(struct sockaddr_in));
    
    struct sockaddr_in6 request_address6;
    EDDIE_ZEROMEMORY(&request_address6, sizeof(struct sockaddr_in6));
    
    struct sockaddr *request_address = NULL;
    socklen_t request_address_len = 0;
    
    if(isv6)
    {
        request_address6.sin6_family = AF_INET6;
        request_address6.sin6_port = 0;
        if(inet_pton(AF_INET6, address, &request_address6.sin6_addr) <= 0)  // The result is already in network byte order
            return -1;
        
        request_address = (struct sockaddr *) &request_address6;
        request_address_len = sizeof(request_address6);
    }
    else
    {
        request_address4.sin_family = AF_INET;
        request_address4.sin_port = 0;
        request_address4.sin_addr.s_addr = inet_addr(address);   // The result is already in network byte order
        if(request_address4.sin_addr.s_addr == INADDR_NONE)
            return -1;
        
        request_address = (struct sockaddr *) &request_address4;
        request_address_len = sizeof(request_address4);
    }
    
    // Allocate a raw ICMP socket (requires root)
    int sock = socket(PF_INET, SOCK_RAW, IPPROTO_ICMP);
    if(sock < 0)
        return -1;

    // We do not include the IP header in the ICMP packet (just let the OS to do that).
    // "Some operating systems set IP_HDRINCL implicitly when IPPROTO_RAW is selected but others require an explicit call to setsockopt".
    int hdrincl = 0;
    if(setsockopt(sock, IPPROTO_IP, IP_HDRINCL, &hdrincl, sizeof(hdrincl)) == -1)
    {
        close(sock);
        return -1;
    }

    uint16_t request_id = eddie_generate_icmp_id();

    struct icmp request_header;
    EDDIE_ZEROMEMORY(&request_header, sizeof(struct icmp));
    request_header.icmp_hun.ih_idseq.icd_id = htons(request_id);        // Creates a random ID for the echo request
    request_header.icmp_hun.ih_idseq.icd_seq = 0;
    request_header.icmp_type = ICMP_ECHO;
    request_header.icmp_cksum = eddie_ip_checksum((const uint16_t *) (&request_header), sizeof(struct icmp));

    char response_buffer[EDDIE_PING_BUFFER_SIZE];

    fd_set read_set;
    EDDIE_ZEROMEMORY(&read_set, sizeof(read_set));

    int retval = -1;

    double start_time = eddie_get_time();

    // Sends the echo request to the destination address
    if(sendto(sock, &request_header, sizeof(struct icmp), 0, request_address, request_address_len) > 0)
    {
        struct timeval receive_timeout;
        int available_timeout = timeout;

        for(;;)
        {
            if(available_timeout == 0)
                break;

            // Read set and receive_timeout must be reset at each loop
            FD_SET(sock, &read_set);
            receive_timeout.tv_sec = available_timeout / 1000;
            receive_timeout.tv_usec = (available_timeout % 1000) * 1000;

            // Wait for a response
            int result = select(sock + 1, &read_set, NULL, NULL, &receive_timeout);
            if(result == -1)
                break;   // If we got a socket error avoid looping uselessy

            int delta = (int) (eddie_get_time() - start_time);
            if(delta < timeout)
                available_timeout = timeout - delta;
            else
                available_timeout = 0;

            if(result <= 0)
                continue;

            result = recvfrom(sock, response_buffer, EDDIE_PING_BUFFER_SIZE, 0, NULL, NULL);
            if(result < (int)(sizeof(struct ip) + sizeof(struct icmp)))
                continue;

            struct icmp *response_header = (struct icmp *)(response_buffer + sizeof(struct ip));
            // Check that we got a reply for our echo request
            if((response_header->icmp_type == ICMP_ECHOREPLY) && (ntohs(response_header->icmp_hun.ih_idseq.icd_id) == request_id))
            {
                // Got it, save the elapsed milliseconds
                retval = delta;
                break;
            }

            // Here we got an ICMP packet of different type (i.e.: ICMP_DEST_UNREACH) or a response to another ICMP request (just ignore it and wait again until the timeout)
        }
    }

    close(sock);

    return retval;
}
*/
    
int eddie_ip_ping(const char *address, int timeout)
{
    if(address == NULL)
        return -1;
    
    // Looks for ':' char instead of '.' for compatibility with the notation "::a.b.c.d"
    bool isv6 = strchr(address, ':') != NULL;
    
    std::unique_ptr<IPinger> pinger;
    if(isv6)
        pinger.reset(new PingerV6(address, timeout));
    else
        pinger.reset(new PingerV4(address, timeout));
    
    return pinger->ping();
}
    
void eddie_signal(int signum, eddie_sighandler_t handler)
{
    signal(signum, handler);
}
  
int eddie_kill(int pid, int sig)
{
    return kill((pid_t) pid, sig);
}

void eddie_get_realtime_network_stats(char* buf, int bufMaxLen)
{
    std::string jsonStr = "";

    jsonStr += "[";

    int nRecords=0;
    
    int mib[] = {
        CTL_NET,
        PF_ROUTE,
        0,
        0,
        NET_RT_IFLIST2,
        0
    };
    size_t len;
    if (sysctl(mib, 6, NULL, &len, NULL, 0) < 0) {
        //fprintf(stderr, "sysctl: %s\n", strerror(errno));
    }
    else
    {
        char *buf = (char *)malloc(len);
        if (sysctl(mib, 6, buf, &len, NULL, 0) < 0) {
            //fprintf(stderr, "sysctl: %s\n", strerror(errno));
        }
        else
        {
            char *lim = buf + len;
            char *next = NULL;
            u_int64_t totalibytes = 0;
            u_int64_t totalobytes = 0;
            for (next = buf; next < lim; ) 
            {
                struct if_msghdr *ifm = (struct if_msghdr *)next;
                next += ifm->ifm_msglen;
                if (ifm->ifm_type == RTM_IFINFO2) 
                {
                    struct if_msghdr2 *if2m = (struct if_msghdr2 *)ifm;
                    totalibytes += if2m->ifm_data.ifi_ibytes;
                    totalobytes += if2m->ifm_data.ifi_obytes;

                    if(nRecords>0)
                        jsonStr += ",";
                    jsonStr += "{ \"id\":" + std::to_string(if2m->ifm_index) + ",\"rcv\":" + std::to_string(if2m->ifm_data.ifi_ibytes) + ",\"snd\":" + std::to_string(if2m->ifm_data.ifi_obytes) + " }";
                }
            }
        }
        free(buf);
    }
    
    if(jsonStr.size()+3 > bufMaxLen) // Avoid buffer overflow
        jsonStr = "[";
    
    jsonStr += "]";
    strcpy(buf, jsonStr.c_str());
}

static size_t eddie_curl_headercallback(void *contents, size_t size, size_t nmemb, void *userp)
{
    ((std::string*)userp)->append((char*)contents, size * nmemb);
    return size * nmemb;
}

static size_t eddie_curl_writecallback(void *data, size_t size, size_t nmemb, std::string *buffer)
{
    int result = 0;
    if (buffer != NULL) 
    {
        buffer->append((char*)data, size * nmemb);
        result = size * nmemb;
    }
    return result;
}

void eddie_curl(const char* jRequest, unsigned int resultMaxLen, char* jResult)
{
    json jsonRequest = json::parse(jRequest);
    
    json jsonResponse;
    
    CURL *hcurl;
    //struct curl_slist *headersList = NULL;
    struct curl_slist *resolveList = NULL;
    CURLcode res;
    
    hcurl = curl_easy_init();
    if(hcurl) 
    {
        try
        {
            std::string bufferHeaders;
            std::string bufferBody;
            
            curl_easy_setopt(hcurl, CURLOPT_URL, std::string(jsonRequest["url"]).c_str());
            
            std::string postfields = jsonRequest["postfields"];
            if(postfields != "")
            {
                jsonResponse["debug-post"] = jsonRequest["postfields"];
                curl_easy_setopt(hcurl, CURLOPT_POSTFIELDSIZE, (long)postfields.size());
                curl_easy_setopt(hcurl, CURLOPT_POSTFIELDS, postfields.c_str());
            }
            
            curl_easy_setopt(hcurl, CURLOPT_HEADERFUNCTION, eddie_curl_headercallback);
            curl_easy_setopt(hcurl, CURLOPT_HEADERDATA, &bufferHeaders);
            curl_easy_setopt(hcurl, CURLOPT_WRITEFUNCTION, eddie_curl_writecallback);
            curl_easy_setopt(hcurl, CURLOPT_WRITEDATA, &bufferBody);
            curl_easy_setopt(hcurl, CURLOPT_NOPROGRESS, 1L);
            
            curl_easy_setopt(hcurl, CURLOPT_USERAGENT, std::string(jsonRequest["useragent"]).c_str());
            
            curl_easy_setopt(hcurl, CURLOPT_CAINFO, std::string(jsonRequest["cacert"]).c_str());
            
            if(jsonRequest["iplayer"] == "4")
                curl_easy_setopt(hcurl, CURLOPT_IPRESOLVE, CURL_IPRESOLVE_V4);
            else if (jsonRequest["iplayer"] == "6")
                curl_easy_setopt(hcurl, CURLOPT_IPRESOLVE, CURL_IPRESOLVE_V6);
            
            if (jsonRequest["resolve-single"] != "")
            {               
                resolveList = curl_slist_append(NULL, std::string(jsonRequest["resolve-single"]).c_str());
                curl_easy_setopt(hcurl, CURLOPT_RESOLVE, resolveList);
            }           

            if (jsonRequest["proxy"] != "")
            {
                curl_easy_setopt(hcurl, CURLOPT_PROXY, std::string(jsonRequest["proxy"]).c_str());
                
                if (jsonRequest["proxyauth"] == "basic")
                    curl_easy_setopt(hcurl, CURLOPT_PROXYAUTH, (long)CURLAUTH_BASIC);
                else if (jsonRequest["proxyauth"] == "ntlm")
                    curl_easy_setopt(hcurl, CURLOPT_PROXYAUTH, (long)CURLAUTH_NTLM);

                if (jsonRequest["proxyuserpwd"] != "")
                    curl_easy_setopt(hcurl, CURLOPT_PROXYUSERPWD, std::string(jsonRequest["proxyuserpwd"]).c_str());
            }
            
            //curl_easy_setopt(hcurl, CURLOPT_HTTPHEADER, headersList);
            
            res = curl_easy_perform(hcurl);
            if(res != CURLE_OK)
                throw std::runtime_error(std::string(curl_easy_strerror(res)));
                
            long response_code;
            curl_easy_getinfo(hcurl, CURLINFO_RESPONSE_CODE, &response_code);
            jsonResponse["response_code"] = response_code;
            
            jsonResponse["headers"] = bufferHeaders;
            
            // Encode in hex string the binary body
            const char digitshex[] = "0123456789ABCDEF";
            std::string hexBody;
            hexBody.reserve(bufferBody.size()*2);
            for (size_t i = 0; i < bufferBody.size(); i++)
            {
                unsigned char c = bufferBody[i];
                hexBody.push_back(digitshex[c >> 4]);
                hexBody.push_back(digitshex[c & 15]);
            }
            jsonResponse["body"] = hexBody;
        }
        catch(const std::exception& ex)
        {
            jsonResponse["error"] = std::string(ex.what());
        }
        catch(...)
        {
            jsonResponse["error"] = "Unexpected internal error";
        }
        
        if(hcurl)
            curl_easy_cleanup(hcurl);
        //if(headersList)
        //    curl_slist_free_all(headersList);
        if(resolveList)
            curl_slist_free_all(resolveList);
    }
    
    std::string jResultStr = jsonResponse.dump();
    if(jResultStr.size() > resultMaxLen)
        return;
        
    strcpy(jResult, jResultStr.c_str());
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifdef __cplusplus
}
#endif

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
