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
#include <linux/fs.h>
#include <mutex>
#include <netinet/ip_icmp.h>
#include <signal.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/ioctl.h>
#include <sys/stat.h>
#include <sys/time.h>
#include <time.h>
#include <unistd.h>

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

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

int eddie_test_native(int pid, int sig)
{
    return 3;
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
    FILE *fp;
    if((fp = fopen(filename, "r")) == NULL)
        return -1;

    int result = -1;

    int attr = 0;
    if(ioctl(fileno(fp), FS_IOC_GETFLAGS, &attr) != -1)
        result = (attr & FS_IMMUTABLE_FL) == FS_IMMUTABLE_FL;

    fclose(fp);

    return result;
}

int eddie_file_set_immutable(const char *filename, int flag)
{
    FILE *fp;
    if((fp = fopen(filename, "r")) == NULL)
        return -1;

    int fd = fileno(fp);

    int result = -1;

    int attr = 0;
    if(ioctl(fd, FS_IOC_GETFLAGS, &attr) != -1)
    {
        attr = flag ? (attr | FS_IMMUTABLE_FL) : (attr & ~FS_IMMUTABLE_FL);

        if(ioctl(fd, FS_IOC_SETFLAGS, &attr) != -1)
            result = 0;
    }

    fclose(fp);

    return result;
}

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
        if(inet_pton(AF_INET6, address, &request_address6.sin6_addr) <= 0)
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

    struct icmphdr request_header;
    EDDIE_ZEROMEMORY(&request_header, sizeof(struct icmphdr));
    request_header.un.echo.id = htons(request_id);        // Creates a random ID for the echo request
    request_header.un.echo.sequence = 0;
    request_header.type = ICMP_ECHO;
    request_header.checksum = eddie_ip_checksum((const uint16_t *) (&request_header), sizeof(struct icmphdr));

    char response_buffer[EDDIE_PING_BUFFER_SIZE];

    fd_set read_set;
    EDDIE_ZEROMEMORY(&read_set, sizeof(read_set));

    int retval = -1;

    double start_time = eddie_get_time();

    // Sends the echo request to the destination address
    if(sendto(sock, &request_header, sizeof(struct icmphdr), 0, request_address, request_address_len) > 0)
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
            if(result < (int)(sizeof(struct iphdr) + sizeof(struct icmphdr)))
                continue;

            struct icmphdr *response_header = (struct icmphdr *)(response_buffer + sizeof(struct iphdr));
            // Check that we got a reply for our echo request
            if((response_header->type == ICMP_ECHOREPLY) && (ntohs(response_header->un.echo.id) == request_id))
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

void eddie_signal(int signum, eddie_sighandler_t handler)
{
    signal(signum, handler);
}

int eddie_kill(int pid, int sig)
{
    return kill((pid_t) pid, sig);
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifdef __cplusplus
}
#endif

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
