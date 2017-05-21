// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2016 AirVPN (support@airvpn.org) / https://airvpn.org )
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


#pragma comment(lib, "iphlpapi.lib")
#pragma comment(lib, "fwpuclnt.lib")
#pragma comment(lib, "rpcrt4.lib")
#define MALLOC(x) HeapAlloc(GetProcessHeap(), 0, (x))
#define FREE(x) HeapFree(GetProcessHeap(), 0, (x))

/******************************************************************************
Utils
*******************************************************************************/

int GetInterfaceMetric(int index, const char* layer)
{
	std::string layerS = layer;
	DWORD err = 0;
	MIB_IPINTERFACE_ROW ipiface;
	InitializeIpInterfaceEntry(&ipiface);
	if(layerS == "ipv4")
		ipiface.Family = AF_INET;
	else if(layerS == "ipv6")
		ipiface.Family = AF_INET6;
	else
		return -1;
	ipiface.InterfaceIndex = index;
	err = GetIpInterfaceEntry(&ipiface);
	if (err == NO_ERROR)
	{
		if(ipiface.UseAutomaticMetric)
			return 0;
		else
			return ipiface.Metric;		
	}
	else
		return -1;
}

int SetInterfaceMetric(int index, const char* layer, int value)
{
	std::string layerS = layer;
	DWORD err = 0;
	MIB_IPINTERFACE_ROW ipiface;
	InitializeIpInterfaceEntry(&ipiface);
	if(layerS == "ipv4")
		ipiface.Family = AF_INET;
	else if (layerS == "ipv6")
		ipiface.Family = AF_INET6;
	else
		return -1;
	ipiface.InterfaceIndex = index;
	err = GetIpInterfaceEntry(&ipiface);
	if (err == NO_ERROR)
	{
		if (ipiface.Family == AF_INET)
			ipiface.SitePrefixLength = 0; // required for IPv4 as per MSDN
		ipiface.Metric = value;
		if (value == 0)
			ipiface.UseAutomaticMetric = TRUE;
		else
			ipiface.UseAutomaticMetric = FALSE;
		err = SetIpInterfaceEntry(&ipiface);
		if (err == NO_ERROR)
			return 0;
	}
	return err;
}

/******************************************************************************
WFP - CreateDeleteInterface - This method creates or deletes a packet filter interface.
*******************************************************************************/
DWORD InterfaceCreate(bool bDynamic)
{
	DWORD dwFwAPiRetCode = ERROR_BAD_COMMAND;
	try
	{
		FWPM_SESSION0 session = {0};
		if(bDynamic)
			session.flags = FWPM_SESSION_FLAG_DYNAMIC;

		// Create packet filter interface.
		dwFwAPiRetCode =  ::FwpmEngineOpen0( NULL,
												RPC_C_AUTHN_WINNT,
												NULL,
												&session,
												&m_hEngineHandle );
	}
	catch(...)
	{
	}
	return dwFwAPiRetCode;
}

DWORD InterfaceDelete()
{
	DWORD dwFwAPiRetCode = ERROR_BAD_COMMAND;
	try
	{
		if (NULL != m_hEngineHandle)
		{
			// Close packet filter interface.
			dwFwAPiRetCode = ::FwpmEngineClose0(m_hEngineHandle);
			m_hEngineHandle = NULL;
		}
	}
	catch (...)
	{
	}
	return dwFwAPiRetCode;
}

DWORD BindInterface(const char* name, std::string weight, bool persistent)
{
	DWORD dwFwAPiRetCode = ERROR_BAD_COMMAND;
	try
	{
		RPC_STATUS rpcStatus = {0};
		FWPM_SUBLAYER0 SubLayer = {0};

		// Create a GUID for our packet filter layer.
		rpcStatus = ::UuidCreate( &SubLayer.subLayerKey );
		if( NO_ERROR == rpcStatus )
		{
			// Save GUID.
			::CopyMemory( &m_subLayerGUID,
							&SubLayer.subLayerKey,
							sizeof( SubLayer.subLayerKey ) );

			// Populate packet filter layer information.
			SubLayer.displayData.name = &subLayerWName[0];
			SubLayer.displayData.description = &subLayerWName[0];

			if(persistent)
				SubLayer.flags = FWPM_SUBLAYER_FLAG_PERSISTENT;

			//SubLayer.flags = 0;
			SubLayer.weight = 0;

			if (weight == "auto")
				SubLayer.weight = 0x100;
			else if(weight == "max")
				SubLayer.weight = maxSubLayerWeight;
			else
			{
				SubLayer.weight = atoi(weight.c_str());
			}

			// Add packet filter to our interface.
			dwFwAPiRetCode = ::FwpmSubLayerAdd0( m_hEngineHandle,
													&SubLayer,
													NULL );
		}
	}
	catch(...)
	{
	}
	return dwFwAPiRetCode;
}

DWORD UnbindInterface()
{
	DWORD dwFwAPiRetCode = ERROR_BAD_COMMAND;
	try
	{
		// Delete packet filter layer from our interface.
		dwFwAPiRetCode = ::FwpmSubLayerDeleteByKey0(m_hEngineHandle,
			&m_subLayerGUID);
		::ZeroMemory(&m_subLayerGUID, sizeof(GUID));
	}
	catch (...)
	{
	}
	return dwFwAPiRetCode;
}

void LibPocketFirewallInit(const char* name)
{
	try
	{
		// Initialize member variables.
		m_hEngineHandle = NULL;
		::ZeroMemory(&m_subLayerGUID, sizeof(GUID));

		std::string fullName = name;

		std::wstring fullWName(fullName.begin(), fullName.end());

		subLayerName = fullName;
		subLayerWName = fullWName;
		serviceWName = fullWName;
	}
	catch (...)
	{
	}
}

BOOL LibPocketFirewallStart(const char* xml)
{
	BOOL bStarted = FALSE;
	std::string description;
	std::string weight;
	bool persistent = false;
	bool dynamic = false;

	std::string attrValue = "";
	void *buf = malloc(XMLBUFSIZE);
	yxml_t x;
	yxml_init(&x, buf, XMLBUFSIZE);
	const char* xmlE = xml;

	for (; *xmlE; xmlE++)
	{
		yxml_ret_t r = yxml_parse(&x, *xmlE);
		if (r < 0)
		{
			lastError = "XML syntax error";
			lastErrorCode = 0;
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
					lastError = "Unknown attribute '" + attrName + "'";
					lastErrorCode = 0;
					return 0;
				}

				attrValue = "";
			}
		}
	}

	try
	{
		lastError = "Failed to Create packet filter interface.";
		// Create packet filter interface.
		lastErrorCode = InterfaceCreate(dynamic);
		if(lastErrorCode == ERROR_SUCCESS)
		{
			lastError = "Failed to bind to packet filter interface.";
			// Bind to packet filter interface.
			lastErrorCode = BindInterface(description.c_str(), weight.c_str(), persistent);
			if(lastErrorCode == ERROR_SUCCESS)
			{
				lastError = "";
				lastErrorCode = 0;
				bStarted = TRUE;
			}
		}
	}
	catch(...)
	{
		lastError = "Unexpected error.";
		lastErrorCode = 0;
	}
	return bStarted;
}

HHOOK _hook;

LRESULT __stdcall HookCallback(int nCode, WPARAM wParam, LPARAM lParam)
{
	if (nCode == WM_INITMENUPOPUP)
	{
		return 0;
	}
	if (nCode >= 0)
	{
		// the action is valid: HC_ACTION.
		if (wParam == WM_INITMENU)
		{
			return 0;
		}
	}

	// call the next hook in the hook chain. This is nessecary or your hook chain will break and the hook stops
	return CallNextHookEx(_hook, nCode, wParam, lParam);
}

BOOL LibPocketFirewallStop()
{
	BOOL bStopped = FALSE;
	try
	{
		DWORD dwFwAPiRetCode = ERROR_BAD_COMMAND;

		// Remove all filters.

		for (unsigned int i = 0; i < filterids.size(); i++)
		{
			dwFwAPiRetCode = ::FwpmFilterDeleteById0(m_hEngineHandle, filterids[i]);
		}
		filterids.clear();

		lastError = "Failed to unbind from packet filter interface.";
		// Unbind from packet filter interface.
		lastErrorCode = UnbindInterface();
		if(lastErrorCode == ERROR_SUCCESS)
		{
			lastError = "Failed to delete packet filter interface.";
			lastErrorCode = InterfaceDelete();
			// Delete packet filter interface.
			if(lastErrorCode == ERROR_SUCCESS)
			{
				lastError = "";
				lastErrorCode = 0;
				bStopped = TRUE;
			}
		}
	}
	catch(...)
	{
		lastError = "Unexpected error.";
		lastErrorCode = 0;
	}
	return bStopped;
}


UINT64 LibPocketFirewallAddRule(const char* xml)
{
	DWORD dwFwAPiRetCode = ERROR_BAD_COMMAND;
	UINT64 filterId = 0;

	// Init
	FWPM_FILTER0 Filter;
	FWPM_FILTER_CONDITION0 Condition[100] = { 0 };

	// Prepare filter.
	RtlZeroMemory(&Filter, sizeof(FWPM_FILTER0));

	Filter.subLayerKey = m_subLayerGUID;
	Filter.displayData.name = &serviceWName[0];
	Filter.displayData.description = &serviceWName[0];
	Filter.weight.type = FWP_EMPTY;
	Filter.numFilterConditions = 0;
	Filter.filterCondition = Condition;
	Filter.flags = FWPM_FILTER_FLAG_NONE;

	std::string attrValue = "";
	void *buf = malloc(XMLBUFSIZE);
	yxml_t x;
	yxml_init(&x, buf, XMLBUFSIZE);
	const char* xmlE = xml;

	int conditionIndex = -1;

	std::wstring ruleWName;

	// Temporary vars for multiple value fields
	FWP_V4_ADDR_AND_MASK ipAddressV4;
	::ZeroMemory(&ipAddressV4, sizeof(FWP_V4_ADDR_AND_MASK));
	FWP_V6_ADDR_AND_MASK ipAddressV6;
	::ZeroMemory(&ipAddressV6, sizeof(FWP_V6_ADDR_AND_MASK));
	FWP_RANGE0 portRange;
	UINT64 filterWeight;

	for (; *xmlE; xmlE++) {
		yxml_ret_t r = yxml_parse(&x, *xmlE);
		if (r < 0)
		{
			lastError = "XML syntax error";
			lastErrorCode = 0;
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
						lastError = "Don't pass disabled rule.";
						lastErrorCode = 0;
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
						lastError = "Unknown layer '" + attrValue + "'";
						lastErrorCode = 0;
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
						lastError = "Unknown action '" + attrValue + "'";
						lastErrorCode = 0;
						return 0;
					}
				}
				else if (attrName == "weight")
				{
					if(attrValue == "auto")
						Filter.weight.type = FWP_EMPTY;
					else if (attrValue == "max")
					{
						Filter.weight.type = FWP_UINT64;
						Filter.weight.uint64 = &maxFilterWeight;
					}
					else
					{
						Filter.weight.type = FWP_UINT64;
						filterWeight = atoi(attrValue.c_str());
						Filter.weight.uint64 = &filterWeight; // TODO : not sure if safe to retain pointer here
					}
				}
				else
				{
					lastError = "Unknown attribute '" + attrName + "'";
					lastErrorCode = 0;
					return 0;
				}
			}
			else if (elemName == "if")
			{
				if (attrName == "field")
				{
					conditionIndex++;

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
						lastError = "Unknown field '" + attrValue + "'";
						lastErrorCode = 0;
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
						lastError = "Unknown match '" + attrValue + "'";
						lastErrorCode = 0;
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
						FWP_BYTE_BLOB *appblob = NULL;
						if (FwpmGetAppIdFromFileName0(wAttrValue.c_str(), &appblob) != ERROR_SUCCESS)
						{
							lastError = "App not found";
							lastErrorCode = 0;
							return 0;
						}

						Condition[conditionIndex].conditionValue.type = FWP_BYTE_BLOB_TYPE;
						Condition[conditionIndex].conditionValue.byteBlob = appblob;
					}
					else
					{
						lastError = "Unexpected 'path' attribute.";
						lastErrorCode = 0;
						return 0;
					}
				}
				else if (attrName == "address")
				{
					if ((Filter.layerKey == FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V4) ||
						(Filter.layerKey == FWPM_LAYER_ALE_AUTH_CONNECT_V4) ||
						(Filter.layerKey == FWPM_LAYER_ALE_FLOW_ESTABLISHED_V4))
					{
						UINT32 IpAddr;
						inet_pton(AF_INET, attrValue.c_str(), &IpAddr);
						ipAddressV4.addr = ntohl(IpAddr);

						Condition[conditionIndex].conditionValue.type = FWP_V4_ADDR_MASK;
						Condition[conditionIndex].conditionValue.v4AddrMask = &ipAddressV4;
					}
					else if ((Filter.layerKey == FWPM_LAYER_ALE_AUTH_CONNECT_V6) ||
						(Filter.layerKey == FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V6) ||
						(Filter.layerKey == FWPM_LAYER_ALE_FLOW_ESTABLISHED_V6))
					{
						// TOFIX, missing IPv6 implementation
					}


				}
				else if (attrName == "mask")
				{
					if ((Filter.layerKey == FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V4) ||
						(Filter.layerKey == FWPM_LAYER_ALE_AUTH_CONNECT_V4) ||
						(Filter.layerKey == FWPM_LAYER_ALE_FLOW_ESTABLISHED_V4))
					{
						UINT32 Mask;
						inet_pton(AF_INET, attrValue.c_str(), &Mask);
						ipAddressV4.mask = ntohl(Mask);

						Condition[conditionIndex].conditionValue.type = FWP_V4_ADDR_MASK;
						Condition[conditionIndex].conditionValue.v4AddrMask = &ipAddressV4;
					}
					else if ((Filter.layerKey == FWPM_LAYER_ALE_AUTH_CONNECT_V6) ||
						(Filter.layerKey == FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V6) ||
						(Filter.layerKey == FWPM_LAYER_ALE_FLOW_ESTABLISHED_V6))
					{
						// TOFIX, missing IPv6 implementation
					}

				}
				else if (attrName == "port_from")
				{
					portRange.valueLow.type = FWP_UINT16;
					portRange.valueLow.uint16 = atoi(attrValue.c_str());

					Condition[conditionIndex].conditionValue.type = FWP_RANGE_TYPE;
					Condition[conditionIndex].conditionValue.rangeValue = &portRange;
				}
				else if (attrName == "port_to")
				{
					portRange.valueHigh.type = FWP_UINT16;
					portRange.valueHigh.uint16 = atoi(attrValue.c_str());

					Condition[conditionIndex].conditionValue.type = FWP_RANGE_TYPE;
					Condition[conditionIndex].conditionValue.rangeValue = &portRange;
				}
				else if (attrName == "protocol")
				{
					Condition[conditionIndex].conditionValue.type = FWP_UINT8;
					if(attrValue == "tcp")
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

							pAddresses = (IP_ADAPTER_ADDRESSES *)MALLOC(outBufLen);
							if (pAddresses == NULL) {
								printf
									("Memory allocation failed for IP_ADAPTER_ADDRESSES struct\n");
								exit(1);
							}

							dwRetVal =
								GetAdaptersAddresses(family, flags, NULL, pAddresses, &outBufLen);

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

								//log << pCurrAddresses->AdapterName;
								//log << "(" << pCurrAddresses->IfIndex << "," << pCurrAddresses->Ipv6IfIndex << "); ";

								if (found)
								{
									NET_LUID tapluid;

									if (ConvertInterfaceIndexToLuid(index, &tapluid) == NO_ERROR)
									{
										Condition[conditionIndex].conditionValue.uint64 = &tapluid.Value;
									}
									else
									{
										lastError = "Unable to obtain interface luid";
										lastErrorCode = 0;
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
							lastError = "Interface ID '" + attrValue + "' unknown or disabled for the layer.";
							lastErrorCode = 0;
							return 0;
						}
					}
					else
					{
						lastError = "Unexpected 'interface' attribute.";
						lastErrorCode = 0;
						return 0;
					}
				}
				else
				{
					lastError = "Unknown attribute '" + attrName + "'";
					lastErrorCode = 0;
					return 0;
				}
			}
			else
			{
				lastError = "Unknown element '" + elemName + "'";
				lastErrorCode = 0;
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
		lastError = "XML syntax error";
		lastErrorCode = 0;
		return 0; // Syntax error
	}

	Filter.numFilterConditions = conditionIndex + 1;

	// Add filter condition to our interface. Save filter id in filterids.
	lastErrorCode = ::FwpmFilterAdd0(m_hEngineHandle,
		&Filter,
		NULL,
		&filterId);

	if (lastErrorCode != ERROR_SUCCESS)
	{
		lastError = "WFP Error.";
		return 0;
	}

	filterids.push_back(filterId);

	lastError = "OK";
	lastErrorCode = 0;
	return filterId;
}

BOOL LibPocketFirewallRemoveRule(const UINT64 id)
{
	for (unsigned int i = 0; i < filterids.size(); i++)
	{
		if (id == filterids[i])
		{
			filterids.erase(filterids.begin() + i);

			return LibPocketFirewallRemoveRuleDirect(id);
		}
	}

	lastError = "Not found";
	lastErrorCode = 0;
	return FALSE;
}

BOOL LibPocketFirewallRemoveRuleDirect(const UINT64 id)
{
	lastErrorCode = ::FwpmFilterDeleteById0(m_hEngineHandle, id);

	if (lastErrorCode != ERROR_SUCCESS)
	{
		lastError = "WFP Error.";
		return FALSE;
	}

	lastError = "OK";
	lastErrorCode = 0;
	return TRUE;
}

const char* LibPocketFirewallGetLastError()
{
	return lastError.c_str();
}

DWORD LibPocketFirewallGetLastErrorCode()
{
	return lastErrorCode;
}
