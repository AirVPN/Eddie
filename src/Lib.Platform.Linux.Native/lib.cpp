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

#include "lib.h"

#include <arpa/inet.h>
#include <fstream>
#include <iomanip>
#include <linux/fs.h>
#include <memory>
#include <mutex>
#include <netinet/ip_icmp.h>
#include <netinet/icmp6.h>
#include <signal.h>
#include <sstream>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/ioctl.h>
#include <sys/stat.h>
#include <sys/time.h>
#include <time.h>
#include <unistd.h>
#include <fcntl.h>

#ifdef EDDIE_LIBCURL
#include <curl/curl.h> // Debian: libcurl4-openssl-dev
#endif
#include "json.hpp"
using json = nlohmann::json;

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifdef __cplusplus
extern "C" {
#endif

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

	int eddie_file_set_mode_str(const char* filename, const char* mode)
	{
		return eddie_file_set_mode(filename, (int)strtol(mode, NULL, 8));
	}

	int eddie_file_get_immutable(const char* filename)
	{
		FILE* fp;
		if ((fp = fopen(filename, "r")) == NULL)
			return -1;

		int result = -1;

		int attr = 0;
		if (ioctl(fileno(fp), FS_IOC_GETFLAGS, &attr) != -1)
			result = (attr & FS_IMMUTABLE_FL) == FS_IMMUTABLE_FL;

		fclose(fp);

		return result;
	}

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

	int eddie_pipe_write(const char* filename, const char* data)
	{
		int result = -1;

		if (data != NULL)
		{
			ssize_t size = strlen(data);
			if (size > 0)
			{
				int f = open(filename, O_WRONLY);
				if (f != -1)
				{
					if (write(f, data, size) == size)
						result = 0;

					close(f);
				}
			}
		}

		return result;
	}

	void eddie_signal(int signum, eddie_sighandler_t handler)
	{
		signal(signum, handler);
	}

	int eddie_kill(int pid, int sig)
	{
		return kill((pid_t)pid, sig);
	}

#ifdef EDDIE_LIBCURL
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
					jsonResponse["debug-post"] = jsonRequest["postfields"];
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
#endif

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifdef __cplusplus
}
#endif

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
