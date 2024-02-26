// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2023 AirVPN (support@airvpn.org) / https://airvpn.org
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

/******************************************************************************
Original WFP source code by Mahesh S - swatkat_thinkdigit@yahoo.co.in - http://swatrant.blogspot.com/
Some code by ValdikSS
******************************************************************************/

#define _WINSOCK_DEPRECATED_NO_WARNINGS

// Note the order: must be included before Windows.h
#include <winsock2.h>
#include <Ws2tcpip.h>
#include <ws2ipdef.h>
#include <iphlpapi.h>

#include "Windows.h"

#include "..\include\impl.h"

#include <fcntl.h>

#include <sys/stat.h>

#include <sys/types.h> // for signal()
#include <signal.h> // for signal();
#include <psapi.h>

#include <fcntl.h> 



#define XMLBUFSIZE 4096 // WFP
#define MALLOC(x) HeapAlloc(GetProcessHeap(), 0, (x)) // WFP
#define FREE(x) HeapFree(GetProcessHeap(), 0, (x)) // WFP
#include <fwpmu.h> // WFP

#pragma comment(lib, "wsock32.lib")
#pragma comment(lib, "Ws2_32.lib")
#pragma comment(lib, "IPHLPAPI.lib")
#pragma comment(lib, "fwpuclnt.lib")
#pragma comment(lib, "rpcrt4.lib")

#include "../../../dependencies/yxml/yxml.h" // WFP utils, maybe converted in JSON in future

#include "../../../dependencies/wintun/wintun.h"

#include <regex>

int Impl::Main()
{
	signal(SIGINT, SIG_IGN); // See comment in Linux implementation

	if ((m_cmdline.find("mode") != m_cmdline.end()) && (m_cmdline["mode"] == "wireguard"))
		return WireGuardTunnel(m_cmdline["config"]);

	WSADATA	WSAData;
	if (WSAStartup(MAKEWORD(2, 2), &WSAData))
	{
		ThrowException("Error in sockets initialization");
	}

	int result = IWindows::Main();

	WSACleanup();

	return result;
}

void Impl::Do(const std::string& commandId, const std::string& command, std::map<std::string, std::string>& params)
{
	if (command == "compatibility-remove-task")
	{
		// Remove old <2.17.3 task
		std::string schtasksPath = FsLocateExecutable("schtasks.exe", false);
		if (schtasksPath != "")
		{
			ExecEx1(schtasksPath, "/delete /tn AirVPN /f");
			ExecEx1(schtasksPath, "/delete /tn Eddie /f");
		}
	}
	else if (command == "compatibility-profiles")
	{
		std::string dataPath = params["path-data"];
		if (FsDirectoryExists(dataPath) == false)
		{
			std::string appPath = params["path-app"];

			std::vector<std::string> filesToMove;
			filesToMove.push_back("AirVPN.xml");
			filesToMove.push_back("default.xml");
			filesToMove.push_back("Recovery.xml");
			filesToMove.push_back("winfirewall_rules_original.wfw");
			filesToMove.push_back("winfirewall_rules_backup.wfw");
			filesToMove.push_back("winfirewallrules.wfw");
			filesToMove.push_back("winfirewall_rules_original.airvpn");

			FsDirectoryCreate(dataPath);

			if (appPath != dataPath)
			{
				// Old Eddie <2.17.3 save data in C:\Program Files\... . Move now.
				for (std::vector<std::string>::const_iterator i = filesToMove.begin(); i != filesToMove.end(); ++i)
				{
					std::string filename = *i;
					std::string fileOldPath = appPath + FsPathSeparator + filename;
					std::string fileNewPath = dataPath + FsPathSeparator + filename;
					if (FsFileExists(fileOldPath))
					{
						FsFileMove(fileOldPath, fileNewPath);
					}
				}
			}

			std::size_t posLastSlash = dataPath.find_last_of("\\");
			if (posLastSlash != std::string::npos)
			{
				std::string oldDataPath = dataPath.substr(0, posLastSlash) + FsPathSeparator + "AirVPN";

				if (FsDirectoryExists(oldDataPath))
				{
					// Old Eddie <2.17.3 save data in AirVPN folder... . Move now.
					for (std::vector<std::string>::const_iterator i = filesToMove.begin(); i != filesToMove.end(); ++i)
					{
						std::string filename = *i;
						std::string fileOldPath = oldDataPath + FsPathSeparator + filename;
						std::string fileNewPath = dataPath + FsPathSeparator + filename;
						if (FsFileExists(fileOldPath))
						{
							FsFileMove(fileOldPath, fileNewPath);
						}
					}

					FsDirectoryDelete(oldDataPath, true);
				}
			}
		}
	}
	else if (command == "wfp")
	{
		std::string action = params["action"];
		if (action == "init")
		{
			std::string name = params["name"];
			WfpInit(name);
		}
		else if (action == "start")
		{
			std::string xml = params["xml"];
			bool result = WfpStart(xml);
			ReplyCommand(commandId, (result ? "1" : "0"));
		}
		else if (action == "stop")
		{
			WfpStop();
		}
		else if (action == "rule-add")
		{
			std::string xml = params["xml"];
			UINT64 ruleId = WfpRuleAdd(xml.c_str());
			ReplyCommand(commandId, std::to_string(ruleId));
		}
		else if (action == "rule-remove")
		{
			UINT64 ruleId = _atoi64(params["id"].c_str());
			bool result = WfpRuleRemove(ruleId);
			ReplyCommand(commandId, (result ? "1" : "0"));
		}
		else if (action == "pending-remove")
		{
			std::string pathTemp = GetTempPath("wfp_rules.xml");

			ExecResult result = ExecEx1(FsLocateExecutable("netsh.exe"), "WFP Show Filters file=\"" + pathTemp + "\"");
			std::string xml = FsFileReadText(pathTemp);
			FsFileDelete(pathTemp);

			bool found = WfpRemovePending(params["name"], xml);

			ReplyCommand(commandId, (found ? "1" : "0"));
		}
		else if (action == "last-error")
		{
			ReplyCommand(commandId, m_wfpLastError);
		}
	}
	else
	{
		IWindows::Do(commandId, command, params);
	}
}

DWORD Impl::WfpInterfaceCreate(bool bDynamic)
{
	DWORD dwFwAPiRetCode = ERROR_BAD_COMMAND;
	try
	{
		FWPM_SESSION0 session = { 0 };
		if (bDynamic)
			session.flags = FWPM_SESSION_FLAG_DYNAMIC;

		// Create packet filter interface.
		dwFwAPiRetCode = ::FwpmEngineOpen0(NULL,
			RPC_C_AUTHN_WINNT,
			NULL,
			&session,
			&m_wfpEngineHandle);
	}
	catch (...)
	{
	}
	return dwFwAPiRetCode;
}

DWORD Impl::WfpInterfaceDelete()
{
	DWORD dwFwAPiRetCode = ERROR_BAD_COMMAND;
	try
	{
		if (NULL != m_wfpEngineHandle)
		{
			// Close packet filter interface.
			dwFwAPiRetCode = ::FwpmEngineClose0(m_wfpEngineHandle);
			m_wfpEngineHandle = NULL;
		}
	}
	catch (...)
	{
	}
	return dwFwAPiRetCode;
}

DWORD Impl::WfpBindInterface(const char* name, std::string weight, bool persistent)
{
	DWORD dwFwAPiRetCode = ERROR_BAD_COMMAND;
	try
	{
		RPC_STATUS rpcStatus = { 0 };
		FWPM_SUBLAYER0 SubLayer = { 0 };

		// Create a GUID for our packet filter layer.
		rpcStatus = ::UuidCreate(&SubLayer.subLayerKey);
		if (NO_ERROR == rpcStatus)
		{
			// Save GUID.
			::CopyMemory(&m_wfpSubLayerGUID,
				&SubLayer.subLayerKey,
				sizeof(SubLayer.subLayerKey));

			// Populate packet filter layer information.
			SubLayer.displayData.name = &m_wfpSubLayerWName[0];
			SubLayer.displayData.description = &m_wfpSubLayerWName[0];

			if (persistent)
				SubLayer.flags = FWPM_SUBLAYER_FLAG_PERSISTENT;

			//SubLayer.flags = 0;
			SubLayer.weight = 0;

			if (weight == "auto")
				SubLayer.weight = 0x100;
			else if (weight == "max")
				SubLayer.weight = m_wfpMaxSubLayerWeight;
			else
			{
				SubLayer.weight = atoi(weight.c_str());
			}

			// Add packet filter to our interface.
			dwFwAPiRetCode = ::FwpmSubLayerAdd0(m_wfpEngineHandle, &SubLayer, NULL);
		}
	}
	catch (...)
	{
	}
	return dwFwAPiRetCode;
}

DWORD Impl::WfpUnbindInterface()
{
	DWORD dwFwAPiRetCode = ERROR_BAD_COMMAND;
	try
	{
		// Delete packet filter layer from our interface.
		dwFwAPiRetCode = ::FwpmSubLayerDeleteByKey0(m_wfpEngineHandle, &m_wfpSubLayerGUID);
		::ZeroMemory(&m_wfpSubLayerGUID, sizeof(GUID));
	}
	catch (...)
	{
	}
	return dwFwAPiRetCode;
}

void Impl::WfpInit(const std::string& name)
{
	// Initialize member variables.
	m_wfpEngineHandle = NULL;
	::ZeroMemory(&m_wfpSubLayerGUID, sizeof(GUID));

	std::string fullName = name;

	std::wstring fullWName(fullName.begin(), fullName.end());

	m_wfpSubLayerName = fullName;
	m_wfpSubLayerWName = fullWName;
	m_wfpServiceWName = fullWName;
}

bool Impl::WfpStart(const std::string& xml)
{
	BOOL bStarted = FALSE;
	std::string description;
	std::string weight;
	bool persistent = false;
	bool dynamic = false;

	std::string attrValue = "";
	void* buf = malloc(XMLBUFSIZE);
	yxml_t x;
	yxml_init(&x, buf, XMLBUFSIZE);
	const char* xmlE = xml.c_str();

	for (; *xmlE; xmlE++)
	{
		yxml_ret_t r = yxml_parse(&x, *xmlE);
		if (r < 0)
		{
			m_wfpLastError = "XML syntax error";
			m_wfpLastErrorCode = 0;
			return 0; // Syntax error
		}

		if (r == YXML_ATTRVAL)
			attrValue.append(x.data);

		switch (r)
		{
		case YXML_ATTREND:
		{
			std::string elemName = x.elem;
			std::string attrName = x.attr;

			if (attrName == "description")
				description = attrValue;
			else if (attrName == "weight")
				weight = attrValue;
			else if (attrName == "persistent")
				persistent = (attrValue == "true");
			else if (attrName == "dynamic")
				dynamic = (attrValue == "true");
			else
			{
				m_wfpLastError = "Unknown attribute '" + attrName + "'";
				m_wfpLastErrorCode = 0;
				return 0;
			}

			attrValue = "";
		}
		}
	}

	try
	{
		m_wfpLastError = "Failed to Create packet filter interface.";
		// Create packet filter interface.
		m_wfpLastErrorCode = WfpInterfaceCreate(dynamic);
		if (m_wfpLastErrorCode == ERROR_SUCCESS)
		{
			m_wfpLastError = "Failed to bind to packet filter interface.";
			// Bind to packet filter interface.
			m_wfpLastErrorCode = WfpBindInterface(description.c_str(), weight.c_str(), persistent);
			if (m_wfpLastErrorCode == ERROR_SUCCESS)
			{
				m_wfpLastError = "";
				m_wfpLastErrorCode = 0;
				bStarted = TRUE;
			}
		}
	}
	catch (...)
	{
		m_wfpLastError = "Unexpected error.";
		m_wfpLastErrorCode = 0;
	}
	return bStarted;
}

bool Impl::WfpStop()
{
	bool bStopped = false;
	try
	{
		DWORD dwFwAPiRetCode = ERROR_BAD_COMMAND;

		// Remove all filters.

		for (unsigned int i = 0; i < m_wfpFilters.size(); i++)
		{
			dwFwAPiRetCode = ::FwpmFilterDeleteById0(m_wfpEngineHandle, m_wfpFilters[i]);
		}
		m_wfpFilters.clear();

		m_wfpLastError = "Failed to unbind from packet filter interface.";
		// Unbind from packet filter interface.
		m_wfpLastErrorCode = WfpUnbindInterface();
		if (m_wfpLastErrorCode == ERROR_SUCCESS)
		{
			m_wfpLastError = "Failed to delete packet filter interface.";
			m_wfpLastErrorCode = WfpInterfaceDelete();
			// Delete packet filter interface.
			if (m_wfpLastErrorCode == ERROR_SUCCESS)
			{
				m_wfpLastError = "";
				m_wfpLastErrorCode = 0;
				bStopped = true;
			}
		}
	}
	catch (...)
	{
		m_wfpLastError = "Unexpected error.";
		m_wfpLastErrorCode = 0;
	}
	return bStopped;
}

struct WfpRuleData
{
	WfpRuleData()
	{
		::ZeroMemory(&ipAddressV4, sizeof(FWP_V4_ADDR_AND_MASK));
		::ZeroMemory(&ipAddressV6, sizeof(FWP_V6_ADDR_AND_MASK));
		::ZeroMemory(&portRange, sizeof(FWP_RANGE0));
		ipAddressV4.mask = ntohl(4294967295); // 255.255.255.255
		ipAddressV6.prefixLength = 128;
		filterWeight = 0;
	}

	FWP_V4_ADDR_AND_MASK ipAddressV4;
	FWP_V6_ADDR_AND_MASK ipAddressV6;
	FWP_RANGE0 portRange;
	UINT64 filterWeight;
};


UINT64 Impl::WfpRuleAdd(const std::string& xml)
{
	DWORD dwFwAPiRetCode = ERROR_BAD_COMMAND;
	UINT64 filterId = 0;

	// Init
	FWPM_FILTER0 Filter;
	FWPM_FILTER_CONDITION0 Condition[100] = { 0 };

	// Prepare filter.
	RtlZeroMemory(&Filter, sizeof(FWPM_FILTER0));

	Filter.subLayerKey = m_wfpSubLayerGUID;
	Filter.displayData.name = &m_wfpServiceWName[0];
	Filter.displayData.description = &m_wfpServiceWName[0];
	Filter.weight.type = FWP_EMPTY;
	Filter.numFilterConditions = 0;
	Filter.filterCondition = Condition;
	Filter.flags = FWPM_FILTER_FLAG_NONE;

	std::string attrValue = "";
	void* buf = malloc(XMLBUFSIZE);
	yxml_t x;
	yxml_init(&x, buf, XMLBUFSIZE);
	const char* xmlE = xml.c_str();

	int conditionIndex = -1;
	std::map<int, WfpRuleData> rulesMap;

	std::wstring ruleWName;

	for (; *xmlE; xmlE++) {
		yxml_ret_t r = yxml_parse(&x, *xmlE);
		if (r < 0)
		{
			m_wfpLastError = "XML syntax error";
			m_wfpLastErrorCode = 0;
			return 0; // Syntax error
		}

		if (r == YXML_ATTRVAL)
			attrValue.append(x.data);

		switch (r)
		{
		case YXML_ATTREND:
		{
			std::string elemName = x.elem;
			std::string attrName = x.attr;

			if (elemName == "rule")
			{
				if (attrName == "name")
				{
					ruleWName = std::wstring(attrValue.begin(), attrValue.end());
					Filter.displayData.description = &ruleWName[0];
				}
				else if (attrName == "persistent")
				{
					if (attrValue == "true")
					{
						Filter.flags = FWPM_FILTER_FLAG_PERSISTENT;
					}
				}
				else if (attrName == "enabled")
				{
					if (attrValue != "true")
					{
						m_wfpLastError = "Don't pass disabled rule.";
						m_wfpLastErrorCode = 0;
						return 0;
					}
				}
				else if (attrName == "layer")
				{
					if (attrValue == "ale_auth_recv_accept_v4")
						Filter.layerKey = FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V4;
					else if (attrValue == "ale_auth_recv_accept_v6")
						Filter.layerKey = FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V6;
					else if (attrValue == "ale_auth_connect_v4")
						Filter.layerKey = FWPM_LAYER_ALE_AUTH_CONNECT_V4;
					else if (attrValue == "ale_auth_connect_v6")
						Filter.layerKey = FWPM_LAYER_ALE_AUTH_CONNECT_V6;
					else if (attrValue == "ale_flow_established_v4")
						Filter.layerKey = FWPM_LAYER_ALE_FLOW_ESTABLISHED_V4;
					else if (attrValue == "ale_flow_established_v6")
						Filter.layerKey = FWPM_LAYER_ALE_FLOW_ESTABLISHED_V6;
					else
					{
						m_wfpLastError = "Unknown layer '" + attrValue + "'";
						m_wfpLastErrorCode = 0;
						return 0;
					}
				}
				else if (attrName == "action")
				{
					if (attrValue == "block")
					{
						Filter.action.type = FWP_ACTION_BLOCK;
					}
					else if (attrValue == "permit")
					{
						Filter.action.type = FWP_ACTION_PERMIT;
					}
					else
					{
						m_wfpLastError = "Unknown action '" + attrValue + "'";
						m_wfpLastErrorCode = 0;
						return 0;
					}
				}
				else if (attrName == "weight")
				{
					if (attrValue == "auto")
						Filter.weight.type = FWP_EMPTY;
					else if (attrValue == "max")
					{
						Filter.weight.type = FWP_UINT64;
						Filter.weight.uint64 = &m_wfpMaxFilterWeight;
					}
					else
					{
						WfpRuleData& ruleData = rulesMap[conditionIndex];

						Filter.weight.type = FWP_UINT64;
						ruleData.filterWeight = atoi(attrValue.c_str());
						Filter.weight.uint64 = &ruleData.filterWeight;
					}
				}
				else
				{
					m_wfpLastError = "Unknown attribute '" + attrName + "'";
					m_wfpLastErrorCode = 0;
					return 0;
				}
			}
			else if (elemName == "if")
			{
				if (attrName == "field")
				{
					conditionIndex++;
					rulesMap[conditionIndex] = WfpRuleData();
				}

				if ((conditionIndex >= 0) && (conditionIndex < 100))
				{
					if (attrName == "field")
					{
						// https://msdn.microsoft.com/en-us/library/windows/desktop/aa363996(v=vs.85).aspx

						if (attrValue == "ale_app_id")
						{
							Condition[conditionIndex].fieldKey = FWPM_CONDITION_ALE_APP_ID;
						}
						else if (attrValue == "ip_local_address")
						{
							Condition[conditionIndex].fieldKey = FWPM_CONDITION_IP_LOCAL_ADDRESS;
						}
						else if (attrValue == "ip_local_interface")
						{
							Condition[conditionIndex].fieldKey = FWPM_CONDITION_IP_LOCAL_INTERFACE;
						}
						else if (attrValue == "ip_local_port")
						{
							Condition[conditionIndex].fieldKey = FWPM_CONDITION_IP_LOCAL_PORT;
						}
						else if (attrValue == "ip_protocol")
						{
							Condition[conditionIndex].fieldKey = FWPM_CONDITION_IP_PROTOCOL;
						}
						else if (attrValue == "ip_remote_address")
						{
							Condition[conditionIndex].fieldKey = FWPM_CONDITION_IP_REMOTE_ADDRESS;
						}
						else if (attrValue == "ip_remote_port")
						{
							Condition[conditionIndex].fieldKey = FWPM_CONDITION_IP_REMOTE_PORT;
						}
						else
						{
							m_wfpLastError = "Unknown field '" + attrValue + "'";
							m_wfpLastErrorCode = 0;
							return 0;
						};
					}
					else if (attrName == "match")
					{
						if (attrValue == "equal")
						{
							Condition[conditionIndex].matchType = FWP_MATCH_EQUAL;
						}
						else if (attrValue == "greater")
						{
							Condition[conditionIndex].matchType = FWP_MATCH_GREATER;
						}
						else if (attrValue == "less")
						{
							Condition[conditionIndex].matchType = FWP_MATCH_LESS;
						}
						else if (attrValue == "greater_or_equal")
						{
							Condition[conditionIndex].matchType = FWP_MATCH_GREATER_OR_EQUAL;
						}
						else if (attrValue == "less_or_equal")
						{
							Condition[conditionIndex].matchType = FWP_MATCH_LESS_OR_EQUAL;
						}
						else if (attrValue == "range")
						{
							Condition[conditionIndex].matchType = FWP_MATCH_RANGE;
						}
						else if (attrValue == "flags_all_set")
						{
							Condition[conditionIndex].matchType = FWP_MATCH_FLAGS_ALL_SET;
						}
						else if (attrValue == "flags_any_set")
						{
							Condition[conditionIndex].matchType = FWP_MATCH_FLAGS_ANY_SET;
						}
						else if (attrValue == "flags_none_set")
						{
							Condition[conditionIndex].matchType = FWP_MATCH_FLAGS_NONE_SET;
						}
						else if (attrValue == "equal_case_insensitive")
						{
							Condition[conditionIndex].matchType = FWP_MATCH_EQUAL_CASE_INSENSITIVE;
						}
						else if (attrValue == "not_equal")
						{
							Condition[conditionIndex].matchType = FWP_MATCH_NOT_EQUAL;
						}
						else if (attrValue == "type_max")
						{
							Condition[conditionIndex].matchType = FWP_MATCH_TYPE_MAX;
						}
						else
						{
							m_wfpLastError = "Unknown match '" + attrValue + "'";
							m_wfpLastErrorCode = 0;
							return 0;
						}
					}
					else if (attrName == "port")
					{
						Condition[conditionIndex].conditionValue.type = FWP_UINT16;
						Condition[conditionIndex].conditionValue.uint16 = atoi(attrValue.c_str());
					}
					else if (attrName == "path")
					{
						if (Condition[conditionIndex].fieldKey == FWPM_CONDITION_ALE_APP_ID)
						{
							std::wstring wAttrValue = std::wstring(attrValue.begin(), attrValue.end());
							FWP_BYTE_BLOB* appblob = NULL;
							if (FwpmGetAppIdFromFileName0(wAttrValue.c_str(), &appblob) != ERROR_SUCCESS)
							{
								m_wfpLastError = "App not found";
								m_wfpLastErrorCode = 0;
								return 0;
							}

							Condition[conditionIndex].conditionValue.type = FWP_BYTE_BLOB_TYPE;
							Condition[conditionIndex].conditionValue.byteBlob = appblob;
						}
						else
						{
							m_wfpLastError = "Unexpected 'path' attribute.";
							m_wfpLastErrorCode = 0;
							return 0;
						}
					}
					else if (attrName == "address")
					{
						if ((Filter.layerKey == FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V4) ||
							(Filter.layerKey == FWPM_LAYER_ALE_AUTH_CONNECT_V4) ||
							(Filter.layerKey == FWPM_LAYER_ALE_FLOW_ESTABLISHED_V4))
						{
							UINT32 IpAddr = 0;
							if (!inet_pton(AF_INET, attrValue.c_str(), &IpAddr))
							{
								m_wfpLastError = "inet_pton ipv4 address failed with '" + attrValue + "'";
								m_wfpLastErrorCode = 0;
								return 0;
							}

							WfpRuleData& ruleData = rulesMap[conditionIndex];
							ruleData.ipAddressV4.addr = ntohl(IpAddr);

							Condition[conditionIndex].conditionValue.type = FWP_V4_ADDR_MASK;
							Condition[conditionIndex].conditionValue.v4AddrMask = &ruleData.ipAddressV4;
						}
						else if ((Filter.layerKey == FWPM_LAYER_ALE_AUTH_CONNECT_V6) ||
							(Filter.layerKey == FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V6) ||
							(Filter.layerKey == FWPM_LAYER_ALE_FLOW_ESTABLISHED_V6))
						{
							in6_addr IpAddr6;
							::ZeroMemory(&IpAddr6, sizeof(in6_addr));

							if (!inet_pton(AF_INET6, attrValue.c_str(), &IpAddr6))
							{
								m_wfpLastError = "inet_pton ipv6 address failed with '" + attrValue + "'";
								m_wfpLastErrorCode = 0;
								return 0;
							}

							WfpRuleData& ruleData = rulesMap[conditionIndex];
							memcpy(&ruleData.ipAddressV6.addr, &IpAddr6.u.Byte, 16);

							Condition[conditionIndex].conditionValue.type = FWP_V6_ADDR_MASK;
							Condition[conditionIndex].conditionValue.v6AddrMask = &ruleData.ipAddressV6;
						}
					}
					else if (attrName == "mask")
					{
						if (attrValue != "")
						{
							if ((Filter.layerKey == FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V4) ||
								(Filter.layerKey == FWPM_LAYER_ALE_AUTH_CONNECT_V4) ||
								(Filter.layerKey == FWPM_LAYER_ALE_FLOW_ESTABLISHED_V4))
							{
								UINT32 Mask;
								if (!inet_pton(AF_INET, attrValue.c_str(), &Mask))
								{
									m_wfpLastError = "inet_pton ipv4 mask failed with '" + attrValue + "'";
									m_wfpLastErrorCode = 0;
									return 0;
								}

								WfpRuleData& ruleData = rulesMap[conditionIndex];
								ruleData.ipAddressV4.mask = ntohl(Mask);

								Condition[conditionIndex].conditionValue.type = FWP_V4_ADDR_MASK;
								Condition[conditionIndex].conditionValue.v4AddrMask = &ruleData.ipAddressV4;
							}
							else if ((Filter.layerKey == FWPM_LAYER_ALE_AUTH_CONNECT_V6) ||
								(Filter.layerKey == FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V6) ||
								(Filter.layerKey == FWPM_LAYER_ALE_FLOW_ESTABLISHED_V6))
							{
								UINT8 prefixLength = atoi(attrValue.c_str());

								WfpRuleData& ruleData = rulesMap[conditionIndex];
								ruleData.ipAddressV6.prefixLength = prefixLength;

								Condition[conditionIndex].conditionValue.type = FWP_V6_ADDR_MASK;
								Condition[conditionIndex].conditionValue.v6AddrMask = &ruleData.ipAddressV6;
							}
						}
					}
					else if (attrName == "port_from")
					{
						WfpRuleData& ruleData = rulesMap[conditionIndex];

						ruleData.portRange.valueLow.type = FWP_UINT16;
						ruleData.portRange.valueLow.uint16 = atoi(attrValue.c_str());

						Condition[conditionIndex].conditionValue.type = FWP_RANGE_TYPE;
						Condition[conditionIndex].conditionValue.rangeValue = &ruleData.portRange;
					}
					else if (attrName == "port_to")
					{
						WfpRuleData& ruleData = rulesMap[conditionIndex];

						ruleData.portRange.valueHigh.type = FWP_UINT16;
						ruleData.portRange.valueHigh.uint16 = atoi(attrValue.c_str());

						Condition[conditionIndex].conditionValue.type = FWP_RANGE_TYPE;
						Condition[conditionIndex].conditionValue.rangeValue = &ruleData.portRange;
					}
					else if (attrName == "protocol")
					{
						Condition[conditionIndex].conditionValue.type = FWP_UINT8;
						if (attrValue == "tcp")
							Condition[conditionIndex].conditionValue.uint8 = IPPROTO_TCP;
						else if (attrValue == "udp")
							Condition[conditionIndex].conditionValue.uint8 = IPPROTO_UDP;
						else if (attrValue == "icmp")
							Condition[conditionIndex].conditionValue.uint8 = IPPROTO_ICMP;
					}
					else if (attrName == "interface")
					{
						if (Condition[conditionIndex].fieldKey == FWPM_CONDITION_IP_LOCAL_INTERFACE)
						{
							Condition[conditionIndex].conditionValue.type = FWP_UINT64;

							BOOL found = FALSE;

							//std::ostringstream log;

							PIP_ADAPTER_ADDRESSES pAddresses = NULL;
							PIP_ADAPTER_ADDRESSES pCurrAddresses = NULL;
							ULONG outBufLen = 0;
							ULONG Iterations = 0;
							DWORD dwRetVal = 0;
							ULONG family = AF_UNSPEC;
							ULONG flags = GAA_FLAG_INCLUDE_PREFIX;

#define WORKING_BUFFER_SIZE 15000
#define MAX_TRIES 3

							// Allocate a 15 KB buffer to start with.
							outBufLen = WORKING_BUFFER_SIZE;

							do {

								pAddresses = (IP_ADAPTER_ADDRESSES*)MALLOC(outBufLen);
								if (pAddresses == NULL) {
									printf
									("Memory allocation failed for IP_ADAPTER_ADDRESSES struct\n");
									exit(1);
								}

								dwRetVal = GetAdaptersAddresses(family, flags, NULL, pAddresses, &outBufLen);

								if (dwRetVal == ERROR_BUFFER_OVERFLOW) {
									FREE(pAddresses);
									pAddresses = NULL;
								}
								else {
									break;
								}

								Iterations++;

							} while ((dwRetVal == ERROR_BUFFER_OVERFLOW) && (Iterations < MAX_TRIES));

							if (dwRetVal == NO_ERROR) {
								// If successful, output some information from the data we received
								pCurrAddresses = pAddresses;
								while (pCurrAddresses) {

									DWORD index = 0;

									if (Filter.layerKey == FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V4)
										index = pCurrAddresses->IfIndex;
									else if (Filter.layerKey == FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V6)
										index = pCurrAddresses->Ipv6IfIndex;
									else if (Filter.layerKey == FWPM_LAYER_ALE_AUTH_CONNECT_V4)
										index = pCurrAddresses->IfIndex;
									else if (Filter.layerKey == FWPM_LAYER_ALE_AUTH_CONNECT_V6)
										index = pCurrAddresses->Ipv6IfIndex;
									else if (Filter.layerKey == FWPM_LAYER_ALE_FLOW_ESTABLISHED_V4)
										index = pCurrAddresses->IfIndex;
									else if (Filter.layerKey == FWPM_LAYER_ALE_FLOW_ESTABLISHED_V6)
										index = pCurrAddresses->Ipv6IfIndex;

									if (index != 0)
									{
										if ((attrValue == "loopback") && (pCurrAddresses->IfType == IF_TYPE_SOFTWARE_LOOPBACK))
											found = true;

										if (strcmp(pCurrAddresses->AdapterName, attrValue.c_str()) == 0)
											found = true;
									}

									if (found)
									{
										NET_LUID tapluid;

										if (ConvertInterfaceIndexToLuid(index, &tapluid) == NO_ERROR)
										{
											Condition[conditionIndex].conditionValue.uint64 = &tapluid.Value;
										}
										else
										{
											m_wfpLastError = "Unable to obtain interface luid";
											m_wfpLastErrorCode = 0;
											return 0;
										}

										break;
									}

									pCurrAddresses = pCurrAddresses->Next;
								}
							}
							else {
							}

							if (pAddresses) {
								FREE(pAddresses);
							}

							if (found == FALSE)
							{
								m_wfpLastError = "Interface ID '" + attrValue + "' unknown or disabled for the layer.";
								m_wfpLastErrorCode = 0;
								return 0;
							}
						}
						else
						{
							m_wfpLastError = "Unexpected 'interface' attribute.";
							m_wfpLastErrorCode = 0;
							return 0;
						}
					}
					else
					{
						m_wfpLastError = "Unknown attribute '" + attrName + "'";
						m_wfpLastErrorCode = 0;
						return 0;
					}
				}
			}
			else
			{
				m_wfpLastError = "Unknown element '" + elemName + "'";
				m_wfpLastErrorCode = 0;
				return 0;
			}

			attrValue.clear();
		} break;
		case YXML_ELEMEND:
		{

		} break;
		}
	}

	yxml_ret_t r = yxml_eof(&x);
	if (r < 0)
	{
		m_wfpLastError = "XML syntax error";
		m_wfpLastErrorCode = 0;
		return 0; // Syntax error
	}

	Filter.numFilterConditions = conditionIndex + 1;

	// Add filter condition to our interface. Save filter id in filterids.
	m_wfpLastErrorCode = ::FwpmFilterAdd0(m_wfpEngineHandle,
		&Filter,
		NULL,
		&filterId);

	if (m_wfpLastErrorCode != ERROR_SUCCESS)
	{
		m_wfpLastError = "WFP Error.";
		return 0;
	}

	m_wfpFilters.push_back(filterId);

	m_wfpLastError = "OK";
	m_wfpLastErrorCode = 0;
	return filterId;
}

bool Impl::WfpRuleRemove(const UINT64 id)
{
	for (unsigned int i = 0; i < m_wfpFilters.size(); i++)
	{
		if (id == m_wfpFilters[i])
		{
			m_wfpFilters.erase(m_wfpFilters.begin() + i);

			return WfpRuleRemoveDirect(id);
		}
	}

	return false;
}

bool Impl::WfpRuleRemoveDirect(const UINT64 id)
{
	m_wfpLastErrorCode = ::FwpmFilterDeleteById0(m_wfpEngineHandle, id);

	return (m_wfpLastErrorCode == ERROR_SUCCESS);
}

bool Impl::WfpRemovePending(const std::string& filterName, const std::string& xml)
{
	int nFound = 0;
	std::string e = xml;
	std::string searchStart = "<name>" + filterName + "</name>";
	std::string searchEnd = "</filterId>";
	for (;;)
	{
		size_t start_pos = e.find(searchStart);
		if (start_pos == std::string::npos)
			break;
		size_t end_pos = e.find(searchEnd, start_pos);
		if (end_pos == std::string::npos)
			break;

		std::string item = e.substr(start_pos, end_pos - start_pos + searchEnd.length());

		std::string filterIdStr = StringExtractBetween(item, "<filterId>", "</filterId>");

		UINT64 filterId = _atoi64(filterIdStr.c_str());
		if (filterId != 0)
		{
			if (WfpRuleRemoveDirect(filterId))
			{
				nFound++;
			}
		}

		e = StringReplaceAll(e, item, "");
	}
	return (nFound != 0);
}

const char* Impl::WfpGetLastError()
{
	return m_wfpLastError.c_str();
}

DWORD Impl::WfpGetLastErrorCode()
{
	return m_wfpLastErrorCode;
}