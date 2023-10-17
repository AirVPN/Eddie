// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2023 AirVPN (support@airvpn.org) / https://airvpn.org )
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
#include "main.h"

#include <time.h>
#include "yxml.h"
#include <Objbase.h>
#include <Ws2tcpip.h>
#include <sstream>
#include <ws2def.h>
#include <ws2ipdef.h>
#include <iphlpapi.h>
#include <map>

#pragma comment(lib, "iphlpapi.lib")
#pragma comment(lib, "fwpuclnt.lib")
#pragma comment(lib, "rpcrt4.lib")

#include "json.hpp"
using json = nlohmann::json;

#define CURL_STATICLIB
#include <curl\curl.h>
#pragma comment(lib, "Crypt32.lib")
#pragma comment(lib, "Wldap32.lib")
#pragma comment(lib, "Normaliz.lib")
#pragma comment(lib, "libcurl_a.lib")

int eddie_init()
{
	return 0;
}

int eddie_get_interface_metric(int index, const char* layer)
{
	std::string layerS = layer;
	DWORD err = 0;
	MIB_IPINTERFACE_ROW ipiface;
	InitializeIpInterfaceEntry(&ipiface);
	if (layerS == "ipv4")
		ipiface.Family = AF_INET;
	else if (layerS == "ipv6")
		ipiface.Family = AF_INET6;
	else
		return -1;
	ipiface.InterfaceIndex = index;
	err = GetIpInterfaceEntry(&ipiface);
	if (err == NO_ERROR)
	{
		if (ipiface.UseAutomaticMetric)
			return 0;
		else
			return ipiface.Metric;
	}
	else
		return -1;
}

DWORD eddie_service_status(const char* serviceId)
{
	SC_HANDLE serviceControlManager = OpenSCManagerA(0, 0, GENERIC_READ);

	DWORD result = 0;

	if (serviceControlManager)
	{
		try
		{
			SC_HANDLE service = OpenServiceA(serviceControlManager, serviceId, SERVICE_QUERY_STATUS);
			if (service)
			{
				try
				{
					SERVICE_STATUS serviceStatus2;
					if (QueryServiceStatus(service, &serviceStatus2))
					{
						result = serviceStatus2.dwCurrentState;
					}
				}
				catch (...)
				{
				}
				CloseServiceHandle(service);
			}
		}
		catch (...)
		{
		}

		CloseServiceHandle(serviceControlManager);
	}

	return result;
}

BOOL eddie_is_process_elevated()
{
	BOOL fIsElevated = FALSE;
	HANDLE hToken = NULL;
	TOKEN_ELEVATION elevation;
	DWORD dwSize;

	if (!OpenProcessToken(GetCurrentProcess(), TOKEN_QUERY, &hToken))
	{
		printf("\n Failed to get Process Token :%d.", GetLastError());
		goto Cleanup;  // if Failed, we treat as False
	}


	if (!GetTokenInformation(hToken, TokenElevation, &elevation, sizeof(elevation), &dwSize))
	{
		printf("\nFailed to get Token Information :%d.", GetLastError());
		goto Cleanup;// if Failed, we treat as False
	}

	fIsElevated = elevation.TokenIsElevated;

Cleanup:
	if (hToken)
	{
		CloseHandle(hToken);
		hToken = NULL;
	}
	return fIsElevated;
}

static size_t eddie_curl_headercallback(void* contents, size_t size, size_t nmemb, void* userp)
{
	((std::string*)userp)->append((char*)contents, size * nmemb);
	return size * nmemb;
}

static size_t eddie_curl_writecallback(void* data, size_t size, size_t nmemb, std::string* buffer)
{
	size_t result = 0;
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
	//struct curl_slist *headersList = NULL;
	struct curl_slist* resolveList = NULL;
	CURLcode res;

	hcurl = curl_easy_init();
	if (hcurl)
	{
		try
		{
			std::string bufferHeaders;
			std::string bufferBody;

			curl_easy_setopt(hcurl, CURLOPT_URL, jsonRequest["url"].get<std::string>().c_str());

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

			curl_easy_setopt(hcurl, CURLOPT_USERAGENT, jsonRequest["useragent"].get<std::string>().c_str());

			curl_easy_setopt(hcurl, CURLOPT_CAINFO, jsonRequest["cacert"].get<std::string>().c_str());

			if (jsonRequest["iplayer"] == "4")
				curl_easy_setopt(hcurl, CURLOPT_IPRESOLVE, CURL_IPRESOLVE_V4);
			else if (jsonRequest["iplayer"] == "6")
				curl_easy_setopt(hcurl, CURLOPT_IPRESOLVE, CURL_IPRESOLVE_V6);

			if (jsonRequest["resolve-single"] != "")
			{
				resolveList = curl_slist_append(NULL, jsonRequest["resolve-single"].get<std::string>().c_str());
				curl_easy_setopt(hcurl, CURLOPT_RESOLVE, resolveList);
			}

			if (jsonRequest["proxy"] != "")
			{
				curl_easy_setopt(hcurl, CURLOPT_PROXY, jsonRequest["proxy"].get<std::string>().c_str());

				if (jsonRequest["proxyauth"] == "basic")
					curl_easy_setopt(hcurl, CURLOPT_PROXYAUTH, (long)CURLAUTH_BASIC);
				else if (jsonRequest["proxyauth"] == "ntlm")
					curl_easy_setopt(hcurl, CURLOPT_PROXYAUTH, (long)CURLAUTH_NTLM);

				if (jsonRequest["proxyuserpwd"] != "")
					curl_easy_setopt(hcurl, CURLOPT_PROXYUSERPWD, jsonRequest["proxyuserpwd"].get<std::string>().c_str());
			}

			res = curl_easy_perform(hcurl);
			if (res != CURLE_OK)
				throw std::runtime_error(std::string(curl_easy_strerror(res)));

			long response_code;
			curl_easy_getinfo(hcurl, CURLINFO_RESPONSE_CODE, &response_code);
			jsonResponse["response_code"] = response_code;

			jsonResponse["headers"] = bufferHeaders;

			curl_version_info_data* version_info = curl_version_info(CURLVERSION_NOW);
			jsonResponse["curl_version"] = std::string{version_info->version};

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

	strcpy_s(jResult, resultMaxLen, jResultStr.c_str());
}
