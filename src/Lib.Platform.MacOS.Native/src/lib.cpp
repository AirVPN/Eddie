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

#include "../include/lib.h"

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

#include <Security/Security.h> // used by credential keyring

#include <curl/curl.h> // Debian: libcurl4-openssl-dev

#include "../../../dependencies/NlohmannJSON/json.hpp"
using json = nlohmann::json;

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

typedef std::lock_guard<std::mutex> mutex_lock;

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifdef __cplusplus
extern "C" {
#endif

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	static int eddie_file_get_flags(const char* filename)
	{
		struct stat s;
		EDDIE_ZEROMEMORY(&s, sizeof(struct stat));

		if (stat(filename, &s) == -1)
			return -1;

		return (int)s.st_flags;
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	int eddie_init()
	{
		return 0;
	}

	int eddie_file_get_mode(const char* filename)
	{
		struct stat s;
		EDDIE_ZEROMEMORY(&s, sizeof(struct stat));

		if (stat(filename, &s) == -1)
			return -1;

		return (int)s.st_mode;
	}

	int eddie_file_set_mode(const char* filename, int mode)
	{
		return chmod(filename, (mode_t)mode);
	}

	/*
	int eddie_file_set_mode_str(const char* filename, const char* mode)
	{
		return eddie_file_set_mode(filename, (int)strtol(mode, NULL, 8));
	}
	*/

	int eddie_file_get_immutable(const char* filename)
	{
		int result = eddie_file_get_flags(filename);
		if (result == -1)
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

	bool eddie_file_get_runasroot(const char* filename)
	{
		struct stat s;
		memset(&s, 0, sizeof(struct stat));

		if (stat(filename, &s) == -1)
			return false;

		bool ownedByRoot = (s.st_uid == 0);
		bool haveSetUID = (s.st_mode & S_ISUID);

		return (ownedByRoot && haveSetUID);
	}

	void eddie_signal(int signum, eddie_sighandler_t handler)
	{
		signal(signum, handler);
	}

	int eddie_kill(int pid, int sig)
	{
		return kill((pid_t)pid, sig);
	}

	void eddie_get_realtime_network_stats(char* buf, int bufMaxLen)
	{
		std::string jsonStr = "";

		jsonStr += "[";

		int nRecords = 0;

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
			char* buf = (char*)malloc(len);
			if (sysctl(mib, 6, buf, &len, NULL, 0) < 0) {
				//fprintf(stderr, "sysctl: %s\n", strerror(errno));
			}
			else
			{
				char* lim = buf + len;
				char* next = NULL;
				//u_int64_t totalibytes = 0;
				//u_int64_t totalobytes = 0;
				for (next = buf; next < lim; )
				{
					struct if_msghdr* ifm = (struct if_msghdr*)next;
					next += ifm->ifm_msglen;
					if (ifm->ifm_type == RTM_IFINFO2)
					{
						struct if_msghdr2* if2m = (struct if_msghdr2*)ifm;
						//totalibytes += if2m->ifm_data.ifi_ibytes;
						//totalobytes += if2m->ifm_data.ifi_obytes;

						char ifname[IF_NAMESIZE];
        				if_indextoname(if2m->ifm_index, ifname);

						if (nRecords > 0)
							jsonStr += ",";
						jsonStr += "{ \"id\":\"" + std::string(ifname) + "\",\"rcv\":" + std::to_string(if2m->ifm_data.ifi_ibytes) + ",\"snd\":" + std::to_string(if2m->ifm_data.ifi_obytes) + " }";
						nRecords++;
					}
				}
			}
			free(buf);
		}

		if (jsonStr.size() + 3 > bufMaxLen) // Avoid buffer overflow
			jsonStr = "[";

		jsonStr += "]";
		strcpy(buf, jsonStr.c_str());
	}

	static size_t eddie_curl_headercallback(void* contents, size_t size, size_t nmemb, void* userp)
	{
		((std::string*)userp)->append((char*)contents, size * nmemb);
		return size * nmemb;
	}

	static size_t eddie_curl_writecallback(void* data, size_t size, size_t nmemb, std::string* buffer)
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

		CURL* hcurl;
		struct curl_slist* resolveList = NULL;
		CURLcode res;

		hcurl = curl_easy_init();
		if (hcurl)
		{
			try
			{
				std::string bufferHeaders;
				std::string bufferBody;

				curl_easy_setopt(hcurl, CURLOPT_URL, std::string(jsonRequest["url"]).c_str());

				std::string postfields = jsonRequest["postfields"];
				if (postfields != "")
				{
					curl_easy_setopt(hcurl, CURLOPT_POSTFIELDSIZE, (long)postfields.size());
					curl_easy_setopt(hcurl, CURLOPT_POSTFIELDS, postfields.c_str());
				}

				curl_easy_setopt(hcurl, CURLOPT_HEADERFUNCTION, eddie_curl_headercallback);
				curl_easy_setopt(hcurl, CURLOPT_HEADERDATA, &bufferHeaders);
				curl_easy_setopt(hcurl, CURLOPT_WRITEFUNCTION, eddie_curl_writecallback);
				curl_easy_setopt(hcurl, CURLOPT_WRITEDATA, &bufferBody);
				curl_easy_setopt(hcurl, CURLOPT_NOPROGRESS, 1L);

				curl_easy_setopt(hcurl, CURLOPT_TIMEOUT, jsonRequest["timeout"].get<int>());

				curl_easy_setopt(hcurl, CURLOPT_USERAGENT, std::string(jsonRequest["useragent"]).c_str());

				curl_easy_setopt(hcurl, CURLOPT_CAINFO, std::string(jsonRequest["cacert"]).c_str());

				if (jsonRequest["iplayer"] == "4")
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

				res = curl_easy_perform(hcurl);
				if (res != CURLE_OK)
					throw std::runtime_error(std::string(curl_easy_strerror(res)));

				long response_code;
				curl_easy_getinfo(hcurl, CURLINFO_RESPONSE_CODE, &response_code);
				jsonResponse["response_code"] = response_code;

				jsonResponse["headers"] = bufferHeaders;

				// Encode in hex string the binary body
				const char digitshex[] = "0123456789ABCDEF";
				std::string hexBody;
				hexBody.reserve(bufferBody.size() * 2);
				for (size_t i = 0; i < bufferBody.size(); i++)
				{
					unsigned char c = bufferBody[i];
					hexBody.push_back(digitshex[c >> 4]);
					hexBody.push_back(digitshex[c & 15]);
				}
				jsonResponse["body"] = hexBody;
			}
			catch (const std::exception& ex)
			{
				jsonResponse["error"] = std::string(ex.what());
			}
			catch (...)
			{
				jsonResponse["error"] = "Unexpected internal error";
			}

			if (hcurl)
				curl_easy_cleanup(hcurl);
			
			if (resolveList)
				curl_slist_free_all(resolveList);
		}

		std::string jResultStr = jsonResponse.dump();
		if (jResultStr.size() > resultMaxLen)
			return;

		strcpy(jResult, jResultStr.c_str());
	}

	void eddie_credential_system_read(const char* serviceName, const char* accountName, unsigned int outputMax, char* output)
	{
		UInt32 passwordLength = 0;
		void* passwordData = NULL;

		OSStatus status = SecKeychainFindGenericPassword(
			NULL,                                // default keychain
			strlen(serviceName),                 // length of service name
			serviceName,                 		 // service name
			strlen(accountName),                 // length of account name
			accountName,                         // account name
			&passwordLength,                     // length of password
			&passwordData,                       // pointer to password data
			NULL                                 // the item reference
		);

		if (status == errSecSuccess && passwordData != NULL) {
			size_t copyLength = (passwordLength < outputMax) ? passwordLength : outputMax - 1;
			strncpy(output, (char*)passwordData, copyLength);
			output[copyLength] = '\0';
			SecKeychainItemFreeContent(NULL, passwordData);
		}		
	}

	bool eddie_credential_system_write(const char* serviceName, const char* accountName, const char* value)
	{
		OSStatus status = SecKeychainAddGenericPassword(
			NULL, 
			strlen(serviceName),
			serviceName,
			strlen(accountName),
			accountName,
			strlen(value),
			value,
			NULL
		);

		return (status == errSecSuccess);
	}

	bool eddie_credential_system_delete(const char* serviceName, const char* accountName)
	{		
		OSStatus status;
		SecKeychainItemRef item = nullptr;

		CFMutableDictionaryRef query = CFDictionaryCreateMutable(kCFAllocatorDefault, 0, &kCFTypeDictionaryKeyCallBacks, &kCFTypeDictionaryValueCallBacks);
		CFDictionaryAddValue(query, kSecClass, kSecClassGenericPassword);
		CFDictionaryAddValue(query, kSecAttrService, CFStringCreateWithCString(kCFAllocatorDefault, serviceName, kCFStringEncodingUTF8));
		CFDictionaryAddValue(query, kSecAttrAccount, CFStringCreateWithCString(kCFAllocatorDefault, accountName, kCFStringEncodingUTF8));
		CFDictionaryAddValue(query, kSecReturnRef, kCFBooleanTrue);

		status = SecItemCopyMatching(query, (CFTypeRef*)&item);
		
		if (status == errSecSuccess) 
		{
			status = SecKeychainItemDelete(item);
			CFRelease(item);
			if (status == errSecSuccess) {
				return true;
			} else {
				return false;
			}
		} 
		else if (status == errSecItemNotFound) 
		{
			return true;
		} 
		else 
		{
			return false;
		}

		CFRelease(query);
	}
		
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifdef __cplusplus
}
#endif

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
