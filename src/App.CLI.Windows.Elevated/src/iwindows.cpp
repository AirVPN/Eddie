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

// Note the order: must be included before Windows.h
#include <winsock2.h>
#include <Ws2tcpip.h>
#include <ws2ipdef.h>
#include <iphlpapi.h>

#include "Windows.h"

#include "..\include\iwindows.h"

#include <cstring> // memset
#include <fstream>
#include <sstream>

#include "psapi.h"
#include "tlhelp32.h"

#include <codecvt> // For StringUTF8ToWString
#include "sddl.h"	// For ConvertSidToStringSid
#include "accctrl.h" // For GetSecurityInfo
#include "aclapi.h" // For GetSecurityInfo
#include <VersionHelpers.h> // For IsWindows8OrGreater
#pragma comment(lib, "version.lib") // For GetFileVersionInfoSizeW / GetFileVersionInfoW / VerQueryValueW

#include "..\include\wireguard_nt.h" // WireGuardNT adapter API (wireguard.dll)

void IWindows::Do(const std::string& commandId, const std::string& command, std::map<std::string, std::string>& params)
{
	if (command == "dns-flush")
	{
		std::string ipconfigPath = FsLocateExecutable("ipconfig.exe", false);

		if (ipconfigPath != "")
			ExecEx(ipconfigPath, { "/flushdns" });
	}
	else if (command == "windows-dns")
	{
		std::string interfaceName = params["interface"];
		std::string layer = ((params["layer"] == "ipv4") ? "ipv4" : "ipv6");
		std::string mode = params["mode"];
		std::string netsh = FsLocateExecutable("netsh.exe");
		if (mode == "dhcp")
		{
			ExecEx(netsh, { "interface", layer, "set", "dns", "name=" + StringEnsureInterfaceName(interfaceName), "source=dhcp", "register=primary", "validate=no" });
		}
		else if (mode == "static")
		{
			std::string ipaddress = params["ipaddress"];
			ExecEx(netsh, { "interface", layer, "set", "dns", "name=" + StringEnsureInterfaceName(interfaceName), "source=static", "address=" + StringEnsureIpAddress(ipaddress), "register=primary", "validate=no" });
		}
		else if (mode == "add")
		{
			std::string ipaddress = params["ipaddress"];
			ExecEx(netsh, { "interface", layer, "add", "dnsserver", "name=" + StringEnsureInterfaceName(interfaceName), "address=" + StringEnsureIpAddress(ipaddress), "validate=no" });
		}
	}
	else if (command == "windows-workaround-25139")
	{
		std::string cidr = StringTrim(params["cidr"]);
		std::string iface = StringTrim(params["iface"]);
		ExecResult result = ExecEx(FsLocateExecutable("netsh.exe"), { "interface", "ipv6", "del", "route", StringEnsureCidr(cidr), "interface=" + StringEnsureNumericInt(iface) });
	}
	else if (command == "windows-workaround-interface-up")
	{
		std::string name = StringTrim(params["name"]);
		ExecResult result = ExecEx(FsLocateExecutable("netsh.exe"), { "interface", "set", "interface", StringEnsureInterfaceName(name), "ENABLED" });
	}
	else if (command == "set-interface-metric")
	{
		int idx = atoi(params["idx"].c_str());
		int value = atoi(params["value"].c_str());
		std::string layer = params["layer"];
		SetInterfaceMetric(idx, layer, value);
	}
	else if (command == "route-list")
	{
		int n = 0;
		std::string json = "";

		// Adapters
		// Return an UNC path as name, we need LUID in hex OR friendly-name, unresolved here, done at C# side
		// ConvertInterfaceIndexToLuid -> ConvertInterfaceLuidToNameW don't return anyway the friendly-name
		/*
		std::map<ULONG, std::string> interfacesIndexToName;
		{
			// Declare and initialize variables
			PIP_INTERFACE_INFO pInfo = NULL;
			ULONG ulOutBufLen = 0;
			DWORD dwRetVal = 0;
			int iReturn = 1;
			int i;

			dwRetVal = GetInterfaceInfo(NULL, &ulOutBufLen);
			if (dwRetVal == ERROR_INSUFFICIENT_BUFFER) {
				pInfo = (IP_INTERFACE_INFO*)MALLOC(ulOutBufLen);
				if (pInfo != NULL)
				{
					dwRetVal = GetInterfaceInfo(pInfo, &ulOutBufLen);
					if (dwRetVal == NO_ERROR)
					{
						for (i = 0; i < pInfo->NumAdapters; i++)
						{
							interfacesIndexToName[pInfo->Adapter[i].Index] = StringWStringToUTF8(pInfo->Adapter[i].Name);
						}
						iReturn = 0;
					}
				}
			}

			FREE(pInfo);
		}
		*/

		// IPv4
		if (true)
		{
			DWORD retval;
			MIB_IPFORWARD_TABLE2* routes = NULL;
			MIB_IPFORWARD_ROW2* route;
			ULONG idx;

			retval = GetIpForwardTable2(AF_INET, &routes);
			if (retval == ERROR_SUCCESS)
			{
				for (idx = 0; idx < routes->NumEntries; idx++)
				{
					route = routes->Table + idx;

					char buf[INET_ADDRSTRLEN];

					InetNtopA(AF_INET, &route->NextHop.Ipv4.sin_addr, (PSTR)buf, sizeof(buf));
					std::string gateway = buf;

					InetNtopA(AF_INET, &route->DestinationPrefix.Prefix.Ipv4.sin_addr, (PSTR)buf, sizeof(buf));
					std::string destination = buf;
					destination += "/" + std::to_string(route->DestinationPrefix.PrefixLength);

					if (n > 0)
						json += ",";
					json += "{\"destination\":\"" + destination + "\",\"gateway\":\"" + gateway + "\",\"interface_index\":" + std::to_string(route->InterfaceIndex) + ",\"metric\":" + std::to_string(route->Metric) + "}";
					n++;
				}
			}

			FreeMibTable(routes);
		}

		// IPv6
		if (true)
		{
			DWORD retval;
			MIB_IPFORWARD_TABLE2* routes = NULL;
			MIB_IPFORWARD_ROW2* route;
			ULONG idx;

			retval = GetIpForwardTable2(AF_INET6, &routes);
			if (retval == ERROR_SUCCESS)
			{
				for (idx = 0; idx < routes->NumEntries; idx++)
				{
					route = routes->Table + idx;

					char buf[INET6_ADDRSTRLEN];

					InetNtopA(AF_INET6, &route->NextHop.Ipv6.sin6_addr, (PSTR)buf, sizeof(buf));
					std::string gateway = buf;

					InetNtopA(AF_INET6, &route->DestinationPrefix.Prefix.Ipv6.sin6_addr, (PSTR)buf, sizeof(buf));
					std::string destination = buf;
					destination += "/" + std::to_string(route->DestinationPrefix.PrefixLength);

					if (n > 0)
						json += ",";
					json += "{\"destination\":\"" + destination + "\",\"gateway\":\"" + gateway + "\",\"interface_index\":" + std::to_string(route->InterfaceIndex) + ",\"metric\":" + std::to_string(route->Metric) + "}";
					n++;
				}
			}

			FreeMibTable(routes);
		}

		ReplyCommand(commandId, "[" + json + "]");
	}
	else if (command == "route")
	{
		std::vector<std::string> args;
		args.push_back("interface");
		if (StringIsIPv4(params["destination"]))
			args.push_back("ipv4");
		else if (StringIsIPv6(params["destination"]))
			args.push_back("ipv6");
		else
			ThrowException("Unknown layer");
		if (params["action"] == "add")
			args.push_back("add");
		else if (params["action"] == "remove")
			args.push_back("del");
		else
			ThrowException("Unknown action");
		args.push_back("route");
		args.push_back("prefix=" + StringEnsureCidr(params["destination"]));
		args.push_back("interface=" + StringEnsureNumericInt(params["iface"]));
		if (params.find("gateway") != params.end())
		{
			// Remember: Win7 need also nexthop, >Win7 no.
			args.push_back("nexthop=" + StringEnsureIpAddress(params["gateway"]));
		}
		if (params["action"] == "add")
		{
			if (params.find("metric") != params.end())
				args.push_back("metric=" + StringEnsureNumericInt(params["metric"]));
		}
		args.push_back("store=active");

		ExecResult shellResult = ExecEx(FsLocateExecutable("netsh.exe"), args);
		if (shellResult.exit != 0)
			ThrowException(GetExecResultReport(shellResult));
	}
	else if (command == "openvpn")
	{
		std::string id = params["id"];
		std::string action = params["action"];

		if (action == "stop")
		{
			std::string signal = params["signal"];

			if (m_keypair.find("openvpn_pid_" + id) != m_keypair.end())
			{
				pid_t pid = atoi(m_keypair["openvpn_pid_" + id].c_str());

				if (GetParentProcessId(pid) != GetCurrentProcessId())
					ThrowException("Requested a kill to a non-child elevated process");

				KillProcess(signal, pid);
			}
		}
		else if (action == "start")
		{
			std::string path = FsLocateExecutable("openvpn.exe", true, true);
			std::string config = params["config"];

			std::string checkResult = CheckValidOpenVpnConfigFile(config);
			if (checkResult != "")
			{
				ThrowException("Not supported OpenVPN config: " + checkResult);
			}
			else
			{
				std::vector<std::string> args;

				args.push_back("--config");
				args.push_back(params["config"]);

				t_shellinfo info = ExecStart(path, args);

				if (info.lastErrorCode != 0)
					ThrowException(info.lastError);

				m_keypair["openvpn_pid_" + id] = std::to_string(info.pid);				
				ReplyCommand(commandId, "procid:" + std::to_string(info.pid));

				for (;;)
				{
					DWORD bytes_read = 0;
					char tBuf[4096];

					bool stderrEnd = false;
					bool stdoutEnd = false;

					{
						// Don't yet know why, but seem hang with stderr (openvpn only). Doesn't seem really matter.
						stderrEnd = true;
						/*
						// stderr
						if (!ReadFile(info.stderrReadHandle, tBuf, 4096 - 1, &bytes_read, NULL))
						{
							//printf("ReadFile: %u %s\n", GetLastError(), GetLastErrorAsString().c_str());
							stderrEnd = true;
						}
						if (bytes_read > 0)
						{
							tBuf[bytes_read] = '\0';
							ReplyCommand(commandId, "stderr:" + std::string(tBuf));
						}
						*/
					}

					{
						// stdout
						if (!ReadFile(info.stdoutReadHandle, tBuf, 4096 - 1, &bytes_read, NULL))
						{
							//printf("ReadFile: %u %s\n", GetLastError(), GetLastErrorAsString().c_str());
							stdoutEnd = true;
						}
						if (bytes_read > 0)
						{
							tBuf[bytes_read] = '\0';
							ReplyCommand(commandId, "stdout:" + std::string(tBuf));
						}

						if ((stderrEnd) && (stdoutEnd))
							break;
					}
				}

				m_keypair.erase("openvpn_pid_" + id);
				
				int exitCode = ExecEnd(info);
				ReplyCommand(commandId, "return:" + std::to_string(exitCode));
			}
		}
	}
	else if (command == "hummingbird")
	{
		// At 2020/02/23, Hummingbird Windows is not public available.
	}		
	else if (command == "network-adapter-create")
	{		
		std::string result = NetworkAdapterCreate(params["driver"], params["name"]);

		ReplyCommand(commandId, result);		
	}
	else if (command == "network-adapter-delete")
	{
		NetworkAdapterDelete(params["id"]);
	}
	else if (command == "network-adapter-clear-all")
	{
		NetworkAdapterDeleteAll();
	}
	else if (command == "driver-install")
	{
		std::string result = DriverInstall(params["driver"]);
		ReplyCommand(commandId, result);
	}
	else if (command == "driver-uninstall")
	{
		std::string result = DriverUninstall(params["driver"]);
		ReplyCommand(commandId, result);
	}
	else if (command == "wireguard-version")
	{
		// Reads the Major.Minor of wireguard.dll (VS_FIXEDFILEINFO::dwFileVersionMS)
		// directly from the deployed file, so the version cannot drift from the
		// binary across upgrades. wireguard.dll (driver loader, signed by WireGuard
		// LLC) carries the canonical "WireGuard for Windows" release tag. Returns ""
		// when the OS floor is not met or the DLL is missing/unreadable: callers
		// treat empty as "WireGuard unavailable".
		std::string version = "";
		if (IsWindows10OrGreater())
		{
			std::string dllPath = GetProcessPathCurrentDir() + FsPathSeparator + "wireguard.dll";
			std::wstring dllPathW = StringUTF8ToWString(dllPath);
			DWORD handle = 0;
			DWORD size = GetFileVersionInfoSizeW(dllPathW.c_str(), &handle);
			if (size > 0)
			{
				std::vector<BYTE> buffer(size);
				if (GetFileVersionInfoW(dllPathW.c_str(), 0, size, buffer.data()))
				{
					VS_FIXEDFILEINFO* info = nullptr;
					UINT infoLen = 0;
					if (VerQueryValueW(buffer.data(), L"\\", reinterpret_cast<LPVOID*>(&info), &infoLen) && info != nullptr && infoLen >= sizeof(VS_FIXEDFILEINFO))
					{
						char buf[32];
						snprintf(buf, sizeof(buf), "%u.%u",
							HIWORD(info->dwFileVersionMS),
							LOWORD(info->dwFileVersionMS));
						version = buf;
					}
				}
			}
		}
		ReplyCommand(commandId, version);
	}
	else if (command == "wireguard")
	{
		std::string id = params["id"];
		std::string action = params["action"];
		std::string interfaceId = StringEnsureAsciiName(params["interface"]);
		if (interfaceId.empty() || interfaceId.length() > 32)
			ThrowException("Invalid WireGuard interface id");

		std::string keypairStopRequest = "wireguard_stop_" + id;

		if (action == "stop")
		{
			m_keypair[keypairStopRequest] = "stop";
		}
		else if (action == "start")
		{
			std::string config = params["config"];
			std::string checkResult = CheckValidWireGuardConfig(config);
			if (checkResult != "")
			{
				ThrowException("Not supported WireGuard config: " + checkResult);
			}
			else
			{
				unsigned long handshakeTimeoutFirst = StringToULong(params["handshake_timeout_first"]);
				unsigned long handshakeTimeoutConnected = StringToULong(params["handshake_timeout_connected"]);

				ReplyCommand(commandId, "log:setup-start");

				std::map<std::string, std::string> cfg = IniConfigToMap(config, ".", true);

				HMODULE hWireGuard = NULL;
				WIREGUARD_ADAPTER_HANDLE adapter = NULL;
				bool adapterOwned = false;

				try
				{
					std::string dllPath = FsLocateExecutable("wireguard.dll", true, true);
					hWireGuard = LoadLibraryExW(StringUTF8ToWString(dllPath).c_str(), NULL, LOAD_WITH_ALTERED_SEARCH_PATH);
					if (hWireGuard == NULL)
						ThrowException("Unable to load wireguard.dll (" + GetLastErrorAsString() + ")");

					WIREGUARD_CREATE_ADAPTER_FUNC* WgCreateAdapter = (WIREGUARD_CREATE_ADAPTER_FUNC*)GetProcAddress(hWireGuard, "WireGuardCreateAdapter");
					WIREGUARD_OPEN_ADAPTER_FUNC* WgOpenAdapter = (WIREGUARD_OPEN_ADAPTER_FUNC*)GetProcAddress(hWireGuard, "WireGuardOpenAdapter");
					WIREGUARD_CLOSE_ADAPTER_FUNC* WgCloseAdapter = (WIREGUARD_CLOSE_ADAPTER_FUNC*)GetProcAddress(hWireGuard, "WireGuardCloseAdapter");
					WIREGUARD_GET_ADAPTER_LUID_FUNC* WgGetAdapterLuid = (WIREGUARD_GET_ADAPTER_LUID_FUNC*)GetProcAddress(hWireGuard, "WireGuardGetAdapterLUID");
					WIREGUARD_SET_CONFIGURATION_FUNC* WgSetConfiguration = (WIREGUARD_SET_CONFIGURATION_FUNC*)GetProcAddress(hWireGuard, "WireGuardSetConfiguration");
					WIREGUARD_GET_CONFIGURATION_FUNC* WgGetConfiguration = (WIREGUARD_GET_CONFIGURATION_FUNC*)GetProcAddress(hWireGuard, "WireGuardGetConfiguration");
					WIREGUARD_SET_ADAPTER_STATE_FUNC* WgSetAdapterState = (WIREGUARD_SET_ADAPTER_STATE_FUNC*)GetProcAddress(hWireGuard, "WireGuardSetAdapterState");

					if (WgCreateAdapter == NULL || WgOpenAdapter == NULL || WgCloseAdapter == NULL || WgGetAdapterLuid == NULL || WgSetConfiguration == NULL || WgGetConfiguration == NULL || WgSetAdapterState == NULL)
						ThrowException("wireguard.dll is missing the expected WireGuardNT exports");

					std::wstring interfaceIdW = StringUTF8ToWString(interfaceId);

					// Drop any stale adapter from a previous unclean run.
					adapter = WgOpenAdapter(interfaceIdW.c_str());
					if (adapter != NULL)
					{
						WgCloseAdapter(adapter);
						adapter = NULL;
					}

					adapter = WgCreateAdapter(interfaceIdW.c_str(), L"Eddie", NULL);
					if (adapter == NULL)
						ThrowException("Unable to create WireGuard adapter (" + GetLastErrorAsString() + ")");
					adapterOwned = true;

					// Interface keys / listen port
					BYTE interfacePrivateKey[WIREGUARD_KEY_LENGTH];
					{
						std::string rawKey = StringBase64Decode(cfg["interface.privatekey"]);
						if (rawKey.size() != WIREGUARD_KEY_LENGTH)
							ThrowException("Invalid interface private key");
						memcpy(interfacePrivateKey, rawKey.data(), WIREGUARD_KEY_LENGTH);
					}

					// Peer keys
					BYTE peerPublicKey[WIREGUARD_KEY_LENGTH];
					{
						std::string rawKey = StringBase64Decode(cfg["peer.publickey"]);
						if (rawKey.size() != WIREGUARD_KEY_LENGTH)
							ThrowException("Invalid peer public key");
						memcpy(peerPublicKey, rawKey.data(), WIREGUARD_KEY_LENGTH);
					}

					bool hasPresharedKey = false;
					BYTE peerPresharedKey[WIREGUARD_KEY_LENGTH];
					if (cfg.find("peer.presharedkey") != cfg.end() && cfg["peer.presharedkey"] != "")
					{
						std::string rawKey = StringBase64Decode(cfg["peer.presharedkey"]);
						if (rawKey.size() != WIREGUARD_KEY_LENGTH)
							ThrowException("Invalid peer preshared key");
						memcpy(peerPresharedKey, rawKey.data(), WIREGUARD_KEY_LENGTH);
						hasPresharedKey = true;
					}

					SOCKADDR_INET peerEndpoint;
					if (WireGuardParseEndpoint(cfg["peer.endpoint"], peerEndpoint) == false)
						ThrowException("Invalid peer endpoint");

					std::vector<std::string> allowedIps = StringToVector(cfg["peer.allowedips"], ',', true);

					// Build the contiguous WireGuardNT configuration buffer.
					size_t configSize = sizeof(WIREGUARD_INTERFACE) + sizeof(WIREGUARD_PEER) + allowedIps.size() * sizeof(WIREGUARD_ALLOWED_IP);
					std::vector<BYTE> configBuffer(configSize, 0);

					WIREGUARD_INTERFACE* wgInterface = (WIREGUARD_INTERFACE*)configBuffer.data();
					wgInterface->Flags = (WIREGUARD_INTERFACE_FLAG)(WIREGUARD_INTERFACE_HAS_PRIVATE_KEY | WIREGUARD_INTERFACE_REPLACE_PEERS);
					memcpy(wgInterface->PrivateKey, interfacePrivateKey, WIREGUARD_KEY_LENGTH);
					if (cfg.find("interface.listenport") != cfg.end() && cfg["interface.listenport"] != "")
					{
						wgInterface->Flags = (WIREGUARD_INTERFACE_FLAG)(wgInterface->Flags | WIREGUARD_INTERFACE_HAS_LISTEN_PORT);
						wgInterface->ListenPort = (WORD)StringToULong(cfg["interface.listenport"]);
					}
					wgInterface->PeersCount = 1;

					WIREGUARD_PEER* wgPeer = (WIREGUARD_PEER*)(configBuffer.data() + sizeof(WIREGUARD_INTERFACE));
					wgPeer->Flags = (WIREGUARD_PEER_FLAG)(WIREGUARD_PEER_HAS_PUBLIC_KEY | WIREGUARD_PEER_HAS_ENDPOINT | WIREGUARD_PEER_REPLACE_ALLOWED_IPS);
					memcpy(wgPeer->PublicKey, peerPublicKey, WIREGUARD_KEY_LENGTH);
					if (hasPresharedKey)
					{
						wgPeer->Flags = (WIREGUARD_PEER_FLAG)(wgPeer->Flags | WIREGUARD_PEER_HAS_PRESHARED_KEY);
						memcpy(wgPeer->PresharedKey, peerPresharedKey, WIREGUARD_KEY_LENGTH);
					}
					if (cfg.find("peer.persistentkeepalive") != cfg.end() && cfg["peer.persistentkeepalive"] != "")
					{
						wgPeer->Flags = (WIREGUARD_PEER_FLAG)(wgPeer->Flags | WIREGUARD_PEER_HAS_PERSISTENT_KEEPALIVE);
						wgPeer->PersistentKeepalive = (WORD)StringToULong(cfg["peer.persistentkeepalive"]);
					}
					wgPeer->Endpoint = peerEndpoint;
					wgPeer->AllowedIPsCount = (DWORD)allowedIps.size();

					WIREGUARD_ALLOWED_IP* wgAllowedIp = (WIREGUARD_ALLOWED_IP*)(configBuffer.data() + sizeof(WIREGUARD_INTERFACE) + sizeof(WIREGUARD_PEER));
					for (size_t i = 0; i < allowedIps.size(); i++)
					{
						ADDRESS_FAMILY family = 0;
						IN_ADDR v4;
						IN6_ADDR v6;
						BYTE cidr = 0;
						if (WireGuardParseInetCidr(allowedIps[i], family, v4, v6, cidr) == false)
							ThrowException("Invalid allowed IP '" + allowedIps[i] + "'");
						wgAllowedIp[i].AddressFamily = family;
						wgAllowedIp[i].Cidr = cidr;
						if (family == AF_INET)
							wgAllowedIp[i].Address.V4 = v4;
						else
							wgAllowedIp[i].Address.V6 = v6;
					}

					if (WgSetConfiguration(adapter, wgInterface, (DWORD)configSize) == FALSE)
						ThrowException("Unable to set WireGuard configuration (" + GetLastErrorAsString() + ")");

					NET_LUID luid;
					WgGetAdapterLuid(adapter, &luid);

					// Assign interface addresses.
					std::vector<std::string> interfaceAddresses = StringToVector(cfg["interface.address"], ',', true);
					bool hasV4 = false;
					bool hasV6 = false;
					for (std::vector<std::string>::const_iterator i = interfaceAddresses.begin(); i != interfaceAddresses.end(); ++i)
					{
						ADDRESS_FAMILY family = 0;
						IN_ADDR v4;
						IN6_ADDR v6;
						BYTE cidr = 0;
						if (WireGuardParseInetCidr(*i, family, v4, v6, cidr) == false)
							ThrowException("Invalid interface address '" + *i + "'");

						MIB_UNICASTIPADDRESS_ROW addressRow;
						InitializeUnicastIpAddressEntry(&addressRow);
						addressRow.InterfaceLuid = luid;
						addressRow.OnLinkPrefixLength = cidr;
						addressRow.DadState = IpDadStatePreferred;
						if (family == AF_INET)
						{
							addressRow.Address.Ipv4.sin_family = AF_INET;
							addressRow.Address.Ipv4.sin_addr = v4;
							hasV4 = true;
						}
						else
						{
							addressRow.Address.Ipv6.sin6_family = AF_INET6;
							addressRow.Address.Ipv6.sin6_addr = v6;
							hasV6 = true;
						}

						DWORD addressResult = CreateUnicastIpAddressEntry(&addressRow);
						if (addressResult != NO_ERROR && addressResult != ERROR_OBJECT_ALREADY_EXISTS)
							ThrowException("Unable to assign interface address '" + *i + "' (" + std::to_string(addressResult) + ")");
					}

					// Apply MTU on the address families in use.
					if (cfg.find("interface.mtu") != cfg.end() && cfg["interface.mtu"] != "")
					{
						unsigned long mtu = StringToULong(cfg["interface.mtu"]);
						if (mtu > 0)
						{
							for (int pass = 0; pass < 2; pass++)
							{
								ADDRESS_FAMILY family = (pass == 0) ? AF_INET : AF_INET6;
								if (family == AF_INET && hasV4 == false)
									continue;
								if (family == AF_INET6 && hasV6 == false)
									continue;

								MIB_IPINTERFACE_ROW ipInterface;
								InitializeIpInterfaceEntry(&ipInterface);
								ipInterface.InterfaceLuid = luid;
								ipInterface.Family = family;
								if (GetIpInterfaceEntry(&ipInterface) == NO_ERROR)
								{
									ipInterface.NlMtu = (ULONG)mtu;
									if (family == AF_INET)
										ipInterface.SitePrefixLength = 0; // required for IPv4 as per MSDN
									SetIpInterfaceEntry(&ipInterface);
								}
							}
						}
					}

					if (WgSetAdapterState(adapter, WIREGUARD_ADAPTER_STATE_UP) == FALSE)
						ThrowException("Unable to bring WireGuard adapter up (" + GetLastErrorAsString() + ")");

					ReplyCommand(commandId, "log:setup-complete");

					// Triggers SearchTunNetworkInterfaceByName() on the C# side.
					ReplyCommand(commandId, "log:setup-interface");

					unsigned long handshakeStart = GetTimestampUnix();
					unsigned long handshakeLast = 0;
					DWORD64 handshakeFiletimePrev = 0;

					std::vector<BYTE> pollBuffer(configSize);

					for (;;)
					{
						// Read live configuration to detect handshakes.
						DWORD bytes = (DWORD)pollBuffer.size();
						BOOL pollResult = WgGetConfiguration(adapter, (WIREGUARD_INTERFACE*)pollBuffer.data(), &bytes);
						if (pollResult == FALSE && GetLastError() == ERROR_MORE_DATA)
						{
							pollBuffer.resize(bytes);
							pollResult = WgGetConfiguration(adapter, (WIREGUARD_INTERFACE*)pollBuffer.data(), &bytes);
						}

						if (pollResult != FALSE)
						{
							WIREGUARD_INTERFACE* liveInterface = (WIREGUARD_INTERFACE*)pollBuffer.data();
							if (liveInterface->PeersCount >= 1)
							{
								WIREGUARD_PEER* livePeer = (WIREGUARD_PEER*)(pollBuffer.data() + sizeof(WIREGUARD_INTERFACE));
								if (livePeer->LastHandshake != 0 && livePeer->LastHandshake != handshakeFiletimePrev)
								{
									handshakeFiletimePrev = livePeer->LastHandshake;
									if (handshakeLast == 0)
										ReplyCommand(commandId, "log:handshake-first");
									handshakeLast = GetTimestampUnix();
								}
							}
						}

						unsigned long timeNow = GetTimestampUnix();
						if (handshakeLast > 0)
						{
							if (timeNow - handshakeLast > handshakeTimeoutConnected)
								ReplyCommand(commandId, "log:handshake-out");
						}
						else
						{
							if (timeNow - handshakeStart > handshakeTimeoutFirst)
								ReplyCommand(commandId, "log:handshake-out");
						}

						// Check stop requested
						if (m_keypair.find(keypairStopRequest) != m_keypair.end())
						{
							ReplyCommand(commandId, "log:stop-requested");
							break;
						}

						Sleep(100);
					}
				}
				catch (std::exception& e)
				{
					ReplyCommand(commandId, "err:" + StringTrim(std::string(e.what())));
				}
				catch (...)
				{
					ReplyCommand(commandId, "err:Unknown exception");
				}

				ReplyCommand(commandId, "log:stop-interface");

				if (adapter != NULL)
				{
					if (adapterOwned && hWireGuard != NULL)
					{
						WIREGUARD_SET_ADAPTER_STATE_FUNC* WgSetAdapterStateDown = (WIREGUARD_SET_ADAPTER_STATE_FUNC*)GetProcAddress(hWireGuard, "WireGuardSetAdapterState");
						if (WgSetAdapterStateDown != NULL)
							WgSetAdapterStateDown(adapter, WIREGUARD_ADAPTER_STATE_DOWN);
					}

					WIREGUARD_CLOSE_ADAPTER_FUNC* WgCloseAdapterFinal = (hWireGuard != NULL) ? (WIREGUARD_CLOSE_ADAPTER_FUNC*)GetProcAddress(hWireGuard, "WireGuardCloseAdapter") : NULL;
					if (WgCloseAdapterFinal != NULL)
						WgCloseAdapterFinal(adapter);
					adapter = NULL;
				}

				if (hWireGuard != NULL)
				{
					FreeLibrary(hWireGuard);
					hWireGuard = NULL;
				}

				ReplyCommand(commandId, "log:stop");
			}
		}
	}
	else
	{
		IBase::Do(commandId, command, params);
	}
}

// --------------------------
// Virtual
// --------------------------

bool IWindows::IsServiceInstalled()
{
	std::string serviceId = GetServiceId();

	HKEY hKey;
	std::string regKey = "SYSTEM\\CurrentControlSet\\Services\\" + serviceId;
	std::wstring regKeyW = StringUTF8ToWString(regKey);
	LONG openRes = RegOpenKeyEx(HKEY_LOCAL_MACHINE, regKeyW.c_str(), 0, KEY_ALL_ACCESS, &hKey);
	LONG closeOut = RegCloseKey(hKey);

	return (openRes == ERROR_SUCCESS);
}

bool IWindows::ServiceInstall()
{
	bool success = true;

	int port = 0;
	if (m_cmdline.find("service_port") != m_cmdline.end())
		port = atoi(m_cmdline["service_port"].c_str());

	std::string elevatedPath = GetProcessPathCurrent();

	std::string servicePath = FsFileGetDirectory(elevatedPath) + FsPathSeparator + "Eddie-CLI-Elevated-Service.exe";

	std::string elevatedArgs = "mode=service";
	
	if (m_cmdline.find("service_port") != m_cmdline.end())
		elevatedArgs += " service_port=" + std::to_string(port);

	// Can be active but old version that don't accept new client
	ServiceGenericDelete(GetServiceId());

	// Copy in a secure directory the launcher
	bool securePath = false;
	if (IsSecureServiceLocation(servicePath))
	{

	}
	else
	{	
		wchar_t systemDirectory[MAX_PATH];
		UINT size = GetSystemDirectoryW(systemDirectory, MAX_PATH);
		std::wstring servicePathSecure = std::wstring(systemDirectory) + L"\\Eddie-VPN-Elevated-Service.exe";
		CopyFileW(StringUTF8ToWString(servicePath).c_str(), servicePathSecure.c_str(), FALSE);
		servicePath = StringWStringToUTF8(servicePathSecure);
		securePath = true;
	}	

	SC_HANDLE serviceControlManager = OpenSCManager(0, 0, SC_MANAGER_ALL_ACCESS); // GENERIC_WRITE is not enough
	if (serviceControlManager)
	{
		std::wstring serviceServiceNameW = StringUTF8ToWString(GetServiceId());
		std::wstring serviceDisplayNameW = StringUTF8ToWString(GetServiceName());
		std::wstring servicePathW = StringUTF8ToWString("\"" + servicePath + "\"");
		LPCWSTR serviceDependsW = TEXT("nsi\0Tcpip\0"); // Added in 2.21.0
		SC_HANDLE service = CreateService(serviceControlManager, serviceServiceNameW.c_str(), serviceDisplayNameW.c_str(), SC_MANAGER_ALL_ACCESS, SERVICE_WIN32_OWN_PROCESS, SERVICE_AUTO_START, SERVICE_ERROR_NORMAL, servicePathW.c_str(), NULL, NULL, serviceDependsW, NULL, NULL);
		if (service)
		{
			if (success)
			{
				HKEY hKey;
				std::string regKey = "SYSTEM\\CurrentControlSet\\Services\\" + GetServiceId();
				std::wstring regKeyW = StringUTF8ToWString(regKey);
				LONG openRes = RegOpenKeyEx(HKEY_LOCAL_MACHINE, regKeyW.c_str(), 0, KEY_ALL_ACCESS, &hKey);
				if (openRes != ERROR_SUCCESS)
					success = false;
				else
				{
					// Write Path
					std::wstring serviceArgElevatedPath = StringUTF8ToWString(GetProcessPathCurrent());
					if (RegSetValueEx(hKey, TEXT("EddieElevatedPath"), 0, REG_SZ, (LPBYTE)serviceArgElevatedPath.c_str(), (DWORD)(serviceArgElevatedPath.size() + 1) * sizeof(wchar_t)) != ERROR_SUCCESS)
						success = false;

					// Write Args
					std::wstring serviceArgs = StringUTF8ToWString(elevatedArgs);
					if ( RegSetValueEx(hKey, TEXT("EddieArgs"), 0, REG_SZ, (LPBYTE)serviceArgs.c_str(), (DWORD)(serviceArgs.size() + 1) * sizeof(wchar_t)) != ERROR_SUCCESS)
						success = false;

					// Write Description
					std::wstring serviceDisplayDescW = StringUTF8ToWString(GetServiceDesc());
					if (RegSetValueEx(hKey, TEXT("Description"), 0, REG_SZ, (LPBYTE)serviceDisplayDescW.c_str(), (DWORD)(serviceDisplayDescW.size() + 1) * sizeof(wchar_t)) != ERROR_SUCCESS)
						success = false;

					// Write Security Info
					if (IntegrityCheckUpdate("service") == false)
						return false;					
				}
			}

			if (success)
			{
				// Required by WireGuard
				SERVICE_SID_INFO svcSidInfo;
				svcSidInfo.dwServiceSidType = SERVICE_SID_TYPE_UNRESTRICTED;
				if (!ChangeServiceConfig2(service, SERVICE_CONFIG_SERVICE_SID_INFO, &svcSidInfo))
					success = false;
			}

			/* // Done via registry above
			if(success)
			{
				SERVICE_DESCRIPTION svcDescription;
				svcDescription.lpDescription = &(StringUTF8ToWString(GetServiceDesc()))[0];
				if (!ChangeServiceConfig2(service, SERVICE_CONFIG_DESCRIPTION, &svcDescription))
					success = false;
			}
			*/

			if (success)
			{
				if (!StartService(service, 0, NULL))
					success = false;
			}

			CloseServiceHandle(service);

			if (success == false)
			{
				ServiceGenericDelete(GetServiceId());
				if (securePath)
					FsFileDelete(servicePath);
			}
		}
		CloseServiceHandle(serviceControlManager);
	}

	if (success)
	{
		LogLocal("Service installed");
		LogRemote("Service installed");
	}

	return success;
}

bool IWindows::ServiceUninstall()
{
	if (IsServiceInstalled())
	{
		IntegrityCheckClean("service");

		LogLocal("Service uninstalled");
		LogRemote("Service uninstalled");

		wchar_t systemDirectory[MAX_PATH];
		UINT size = GetSystemDirectoryW(systemDirectory, MAX_PATH);
		std::wstring servicePathSecure = std::wstring(systemDirectory) + L"\\Eddie-VPN-Elevated-Service.exe";

		if (GetLaunchMode() == "service")
		{
			// in Windows, we cannot delete the same running process
			std::wstring servicePathDeleteScript = std::wstring(systemDirectory) + L"\\Eddie-VPN-Elevated-Service-Delete.bat";

			std::string scriptDelete = "";			
			scriptDelete += ":loop\r\n";
			scriptDelete += "sc delete EddieElevationService\r\n";
			scriptDelete += "if '%errorlevel%' == '1072' (\r\n"; // If 'marked for deletion', still in execution...
			scriptDelete += "	timeout / t 1 / nobreak > null\r\n";
			scriptDelete += "	goto loop\r\n";
			scriptDelete += ")\r\n";
			if (FsFileExists(StringWStringToUTF8(servicePathSecure)))
				scriptDelete += "del " + StringWStringToUTF8(servicePathSecure) + "\r\n";
			scriptDelete += "del " + StringWStringToUTF8(servicePathDeleteScript) + "\r\n";
			
			FsFileWriteText(StringWStringToUTF8(servicePathDeleteScript), scriptDelete);

			std::string commandDelete = "cmd /c " + StringWStringToUTF8(servicePathDeleteScript);
			STARTUPINFOA si = { sizeof(si) };
			PROCESS_INFORMATION pi;
			if (CreateProcessA(NULL, const_cast<char*>(commandDelete.c_str()), NULL, NULL, FALSE, CREATE_NO_WINDOW, NULL, NULL, &si, &pi))
			{
				CloseHandle(pi.hProcess);
				CloseHandle(pi.hThread);
			}		

			return true;
		}
		else
		{
			FsFileDelete(StringWStringToUTF8(servicePathSecure));
			return ServiceGenericDelete(GetServiceId());
		}
	}
	else
		return false;
}

bool IWindows::FullUninstall()
{
	// Remove any adapter created from Eddie
	NetworkAdapterDeleteAll();
	
	return IBase::FullUninstall();
}

void IWindows::AddTorCookiePaths(const std::string& torPath, const std::string& username, std::vector<std::string>& result)
{
	if (torPath != "") // TorBrowser
	{
		std::string path = FsFileGetDirectory(torPath) + "\\..\\Data\\Tor\\control_auth_cookie";
		result.push_back(path);
	}
}

// --------------------------
// Virtual Pure, OS
// --------------------------

std::string IWindows::GetDevLogPath()
{
	return "C:\\eddie-elevated.log";
}

bool IWindows::IsRoot()
{
	HANDLE hToken = NULL;
	TOKEN_ELEVATION elevation;
	DWORD dwSize;

	if (!OpenProcessToken(GetCurrentProcess(), TOKEN_QUERY, &hToken))
		return false;
	
	if (!GetTokenInformation(hToken, TokenElevation, &elevation, sizeof(elevation), &dwSize))
	{
		CloseHandle(hToken);
		return false;
	}

	bool fIsElevated = elevation.TokenIsElevated;

	CloseHandle(hToken);

	return fIsElevated;
}

void IWindows::Sleep(int ms)
{
	::Sleep(ms);
}

uint64_t IWindows::GetTimestampUnixUsec()
{
	uint64_t EPOCH = ((uint64_t)116444736000000000ULL);
	SYSTEMTIME  systemTime;
	FILETIME    fileTime;
	uint64_t    time;
	GetSystemTime(&systemTime);
	SystemTimeToFileTime(&systemTime, &fileTime);
	time = ((uint64_t)fileTime.dwLowDateTime);
	time += ((uint64_t)fileTime.dwHighDateTime) << 32;
	return (time - EPOCH) / 10 + uint64_t(systemTime.wMilliseconds) * 1000;
}

pid_t IWindows::GetCurrentProcessId()
{
	return ::GetCurrentProcessId();
}

pid_t IWindows::GetParentProcessId()
{
	return GetParentProcessId(GetCurrentProcessId());
}

pid_t IWindows::GetParentProcessId(pid_t pid)
{
	HANDLE hSnapshot;
	PROCESSENTRY32 pe32;
	DWORD ppid = 0;

	hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
	__try {
		if (hSnapshot == INVALID_HANDLE_VALUE) __leave;

		ZeroMemory(&pe32, sizeof(pe32));
		pe32.dwSize = sizeof(pe32);
		if (!Process32First(hSnapshot, &pe32)) __leave;

		do {
			if (pe32.th32ProcessID == pid) {
				ppid = pe32.th32ParentProcessID;
				break;
			}
		} while (Process32Next(hSnapshot, &pe32));

	}
	__finally {
		if (hSnapshot != INVALID_HANDLE_VALUE)
			if (hSnapshot != 0) // Avoid C6387
				CloseHandle(hSnapshot);
	}
	return ppid;
}

std::string IWindows::GetProcessPathOfId(pid_t pid)
{
	HANDLE processHandle = NULL;
	WCHAR filename[MAX_PATH+1];

	processHandle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, FALSE, pid);
	if (processHandle != NULL) {
		std::string result;
		if (GetModuleFileNameExW(processHandle, NULL, filename, MAX_PATH) == 0)
		{
			result = ""; // Failed to get module filename.
		}
		else
		{
			result = StringWStringToUTF8(filename);
		}
		CloseHandle(processHandle);
		return result;
	}
	else
	{
		return ""; // Failed to open process.
	}
}

pid_t IWindows::GetProcessIdOfName(const std::string& name)
{
	HANDLE snap = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);

	PROCESSENTRY32W entry;
	entry.dwSize = sizeof entry;

	if (!Process32FirstW(snap, &entry))
		return 0;

	do {
		std::string exeCurrent = StringWStringToUTF8(entry.szExeFile);
		if (exeCurrent == name)
			return entry.th32ProcessID;
		if (exeCurrent == name + ".exe")
			return entry.th32ProcessID;
	} while (Process32NextW(snap, &entry));

	return 0;
}

void IWindows::KillProcess(const std::string& signal, pid_t pid)
{
	if (signal == "sigint")
	{
		if (AttachConsole(pid))
		{
			SetConsoleCtrlHandler(NULL, true);
			try
			{
				GenerateConsoleCtrlEvent(CTRL_C_EVENT, 0);
				Sleep(3000);
			}
			catch (...)
			{
			}
			FreeConsole();
			SetConsoleCtrlHandler(NULL, false);
		}
	}
	else if (signal == "sigterm")
	{
		HANDLE processHandle;
		processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, pid);
		TerminateProcess(processHandle, 1);
		CloseHandle(processHandle);
	}
}

std::string IWindows::GetCmdlineOfProcessId(pid_t pid)
{
	// Never used in Windows
	return "";
}

std::string IWindows::GetWorkingDirOfProcessId(pid_t pid)
{
	// Never used in Windows
	return "";
}

void IWindows::SetEnv(const std::string& name, const std::string& value)
{
	// Never used in Windows
}

int IWindows::ExecRaw(const std::string& path, const std::vector<std::string>& args, const bool stdinWrite, const std::string& stdinBody, std::string& stdOut, std::string& stdErr, const bool log)
{
	// stdinWrite, stdinBody not yet supported, not need right now

	std::string logMessage;

	if (log)
	{
		logMessage += "Exec, path:'" + path + "'";
		for (std::vector<std::string>::const_iterator i = args.begin(); i != args.end(); ++i)
		{
			logMessage += ", arg:'" + *i + "'";
		}
	}

	t_shellinfo info = ExecStart(path, args);

	if (info.lastErrorCode != 0)
		return info.lastErrorCode;

	for (;;)
	{
		DWORD bytes_read;
		char tBuf[4096];

		bool stderrEnd = false;
		bool stdoutEnd = false;

		{
			if (!ReadFile(info.stderrReadHandle, tBuf, 4096 - 1, &bytes_read, NULL))
			{
				stderrEnd = true;
			}
			if (bytes_read > 0)
			{
				tBuf[bytes_read] = '\0';
				stdErr += tBuf;
			}
		}

		{
			if (!ReadFile(info.stdoutReadHandle, tBuf, 4096 - 1, &bytes_read, NULL))
			{
				stdoutEnd = true;
			}
			if (bytes_read > 0)
			{
				tBuf[bytes_read] = '\0';
				stdOut += tBuf;
			}

			if ((stderrEnd) && (stdoutEnd))
				break;
		}
	}

	DWORD exitCode = ExecEnd(info);

	if (log)
	{
		logMessage += ", exit:" + std::to_string(exitCode);
		if (StringTrim(stdOut) != "")
			logMessage += ", out:'" + StringTrim(stdOut) + "'";
		if (StringTrim(stdErr) != "")
			logMessage += ", err:'" + StringTrim(stdErr) + "'";
		LogDebug(logMessage);
	}

	return exitCode;
}

bool IWindows::FsDirectoryCreate(const std::string& path)
{
	if (FsDirectoryExists(path) == false)
		return ::CreateDirectoryW(StringUTF8ToWString(path).c_str(), NULL);
	else
		return true;
}

bool IWindows::FsFileExists(const std::string& path)
{
	if (path == "")
		return false;

	std::wstring pathW(StringUTF8ToWString(path));
	DWORD fileAttributes = GetFileAttributes(pathW.c_str());
	return (fileAttributes != INVALID_FILE_ATTRIBUTES) && !(fileAttributes & FILE_ATTRIBUTE_DIRECTORY);
}

bool IWindows::FsDirectoryExists(const std::string& path)
{
	if (path == "")
		return false;

	std::wstring pathW(StringUTF8ToWString(path));
	DWORD ftype = GetFileAttributes(pathW.c_str());
	if (ftype == INVALID_FILE_ATTRIBUTES)
		return false;

	if (ftype & FILE_ATTRIBUTE_DIRECTORY)
		return true;

	return false;
}

bool IWindows::FsFileDelete(const std::string& path)
{
	if (FsFileExists(path))
	{
		std::wstring pathW(StringUTF8ToWString(path));
		return ::DeleteFile(pathW.c_str());
	}
	else
		return true;
}

bool IWindows::FsDirectoryDelete(const std::string& path, bool recursive)
{
	if (FsDirectoryExists(path) == false)
		return true;

	std::wstring pathW = StringUTF8ToWString(path);

	if (recursive)
	{
		WIN32_FIND_DATAW findData;
		HANDLE hFind = ::FindFirstFileW((pathW + L"\\*").c_str(), &findData);
		if (hFind == INVALID_HANDLE_VALUE)
			return false;

		bool ok = true;
		do
		{
			std::wstring name = findData.cFileName;
			if (name == L"." || name == L"..")
				continue;

			std::wstring childW = pathW + L"\\" + name;

			if (findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
			{
				if (findData.dwFileAttributes & FILE_ATTRIBUTE_REPARSE_POINT)
					ok = (::RemoveDirectoryW(childW.c_str()) != FALSE); // junction/symlink: remove the link, never recurse into target
				else
					ok = FsDirectoryDelete(StringWStringToUTF8(childW), true);
			}
			else
			{
				if (findData.dwFileAttributes & FILE_ATTRIBUTE_READONLY)
					::SetFileAttributesW(childW.c_str(), FILE_ATTRIBUTE_NORMAL);
				ok = (::DeleteFileW(childW.c_str()) != FALSE);
			}
		} while (ok && (::FindNextFileW(hFind, &findData) != FALSE));

		::FindClose(hFind);

		if (ok == false)
			return false;
	}

	return ::RemoveDirectoryW(pathW.c_str());
}

bool IWindows::FsFileMove(const std::string& source, const std::string& destination)
{
	return (MoveFileW(StringUTF8ToWString(source).c_str(), StringUTF8ToWString(destination).c_str()));
}

bool IWindows::FsFileCopy(const std::string& source, const std::string& destination)
{
	return (CopyFileW(StringUTF8ToWString(source).c_str(), StringUTF8ToWString(destination).c_str(), FALSE));
}

std::string IWindows::FsFileReadText(const std::string& path)
{
	std::wstring pathW(StringUTF8ToWString(path));
	std::ifstream f(pathW);
	if (!f) return "";
	std::stringstream buffer;
	buffer << f.rdbuf();
	return buffer.str();
}

std::vector<char> IWindows::FsFileReadBytes(const std::string& path)
{
	std::wstring pathW(StringUTF8ToWString(path));

	std::ifstream ifs(pathW, std::ios::binary | std::ios::ate);
	std::ifstream::pos_type pos = ifs.tellg();

	std::vector<char>  result((const unsigned int)pos); // 2.21.0 cast added

	ifs.seekg(0, std::ios::beg);
	if (pos > 0)
		ifs.read(&result[0], pos);

	return result;
}

std::vector<std::string> IWindows::FsFilesInPath(const std::string& path)
{
	std::vector<std::string> files;

	std::string pathU = path;

	pathU += FsPathSeparator + "*.*";

	std::wstring pathW(StringUTF8ToWString(pathU));

	WIN32_FIND_DATAW ffd;
	HANDLE hFind = INVALID_HANDLE_VALUE;
	hFind = FindFirstFileW(pathW.c_str(), &ffd);
	if (INVALID_HANDLE_VALUE == hFind)
		return files;

	do
	{
		std::wstring fileW = ffd.cFileName;
		std::string file = StringWStringToUTF8(fileW);
		if (ffd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
			continue;
		files.push_back(file);

	} while (FindNextFile(hFind, &ffd) != 0);

	return files;
}

std::string IWindows::FsGetTempPath()
{
	WCHAR path[MAX_PATH];
	::GetTempPathW(MAX_PATH, path);
	return StringWStringToUTF8(path);
}

std::string IWindows::GetStagingDir()
{
	WCHAR programData[MAX_PATH];
	DWORD len = ::GetEnvironmentVariableW(L"ProgramData", programData, MAX_PATH);
	std::string base = (len > 0 && len < MAX_PATH) ? StringWStringToUTF8(programData) : "C:\\ProgramData";
	return base + "\\Eddie-VPN\\stage";
}

std::vector<std::string> IWindows::FsGetEnvPath()
{
	// In Linux/macOS, we use the path. Anyway, we check if root-owned.
	// In Windows, admin-only check is not yet implemented, but we use only system directory below.
	std::vector<std::string> paths;
	/*
	// ENV edition
	char *pValue;
	size_t len;
	errno_t err = _dupenv_s(&pValue, &len, "PATH");
	if (err)
		return paths;

	paths = StringToVector(pValue, ';', false);
	*/
	WCHAR pathSystem[MAX_PATH];
	::GetSystemDirectoryW(pathSystem, MAX_PATH);
	paths.push_back(StringWStringToUTF8(pathSystem));

	return paths;
}

std::string IWindows::FsGetRealPath(std::string path)
{
	// Never used in Windows
	return "";
}

bool IWindows::FsFileIsExecutable(std::string path)
{
	if (FsFileExists(path) == false)
		return false;
	std::string ext = FsFileGetExtension(path);
	return (ext == "exe" || ext == "msi" || ext == "bat" || ext == "ps1" || ext == "cmd" || ext == "com");
}

bool IWindows::FsFileEnsureRootOnly(std::string path)
{
	// Not used in Windows
	return true;
}

bool IWindows::FsFileMakeRunnable(const std::string& path)
{
	// No-op on Windows: staged files inherit the staging directory's protected DACL
	// (SYSTEM + Administrators only), and there is no POSIX-style executable bit.
	return true;
}

bool IWindows::FsFileIsRootOnly(std::string path)
{
	if (FsFileExists(path) == false)
		return false;
	return CheckExecutablePathPermissions(path).empty();
}

bool IWindows::FsDirectoryEnsureRootOnly(std::string path)
{
	std::wstring pathW = StringUTF8ToWString(path);

	PSID pSidSystem = NULL;
	PSID pSidAdmins = NULL;
	if (ConvertStringSidToSidW(L"S-1-5-18", &pSidSystem) == FALSE) // SYSTEM
		return false;
	if (ConvertStringSidToSidW(L"S-1-5-32-544", &pSidAdmins) == FALSE) // Administrators
	{
		LocalFree(pSidSystem);
		return false;
	}

	EXPLICIT_ACCESSW ea[2];
	ZeroMemory(&ea, sizeof(ea));

	ea[0].grfAccessPermissions = GENERIC_ALL;
	ea[0].grfAccessMode = SET_ACCESS;
	ea[0].grfInheritance = SUB_CONTAINERS_AND_OBJECTS_INHERIT;
	ea[0].Trustee.TrusteeForm = TRUSTEE_IS_SID;
	ea[0].Trustee.TrusteeType = TRUSTEE_IS_USER;
	ea[0].Trustee.ptstrName = (LPWSTR)pSidSystem;

	ea[1].grfAccessPermissions = GENERIC_ALL;
	ea[1].grfAccessMode = SET_ACCESS;
	ea[1].grfInheritance = SUB_CONTAINERS_AND_OBJECTS_INHERIT;
	ea[1].Trustee.TrusteeForm = TRUSTEE_IS_SID;
	ea[1].Trustee.TrusteeType = TRUSTEE_IS_GROUP;
	ea[1].Trustee.ptstrName = (LPWSTR)pSidAdmins;

	PACL pDacl = NULL;
	bool ok = (SetEntriesInAclW(2, ea, NULL, &pDacl) == ERROR_SUCCESS);

	if (ok && FsDirectoryExists(path) == false)
	{
		if (::CreateDirectoryW(pathW.c_str(), NULL) == FALSE)
			ok = false;
	}

	if (ok)
	{
		// PROTECTED_DACL_SECURITY_INFORMATION drops inherited ACEs, so a permissive parent cannot loosen this directory.
		DWORD dwRes = SetNamedSecurityInfoW((LPWSTR)pathW.c_str(), SE_FILE_OBJECT,
			DACL_SECURITY_INFORMATION | PROTECTED_DACL_SECURITY_INFORMATION,
			NULL, NULL, pDacl, NULL);
		if (dwRes != ERROR_SUCCESS)
			ok = false;
	}

	if (pDacl != NULL)
		LocalFree(pDacl);
	LocalFree(pSidSystem);
	LocalFree(pSidAdmins);

	return ok;
}

bool IWindows::FsDirectoryIsRootOnly(std::string path)
{
	if (FsDirectoryExists(path) == false)
		return false;

	std::wstring pathW = StringUTF8ToWString(path);

	PACL pDacl = NULL;
	PSECURITY_DESCRIPTOR pSD = NULL;
	if (GetNamedSecurityInfoW(pathW.c_str(), SE_FILE_OBJECT, DACL_SECURITY_INFORMATION, NULL, NULL, &pDacl, NULL, &pSD) != ERROR_SUCCESS)
		return false;

	bool rootOnly = true;

	PSID pSidSystem = NULL;
	PSID pSidAdmins = NULL;
	PSID pSidTrustedInstaller = NULL;
	ConvertStringSidToSidW(L"S-1-5-18", &pSidSystem); // SYSTEM
	ConvertStringSidToSidW(L"S-1-5-32-544", &pSidAdmins); // Administrators
	ConvertStringSidToSidW(L"S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464", &pSidTrustedInstaller); // TrustedInstaller

	if (pDacl == NULL)
	{
		rootOnly = false; // NULL DACL means unrestricted access
	}
	else
	{
		for (DWORD i = 0; (i < pDacl->AceCount) && rootOnly; i++)
		{
			LPVOID pAce;
			if (GetAce(pDacl, i, &pAce))
			{
				ACE_HEADER* aceHeader = (ACE_HEADER*)pAce;
				if (aceHeader->AceType == ACCESS_ALLOWED_ACE_TYPE)
				{
					ACCESS_ALLOWED_ACE* allowedAce = (ACCESS_ALLOWED_ACE*)pAce;
					if ((allowedAce->Mask & FILE_ADD_FILE) == FILE_ADD_FILE)
					{
						PSID pAceSid = (PSID)&allowedAce->SidStart;

						if ((pSidSystem != NULL) && (EqualSid(pAceSid, pSidSystem)))
							continue;
						if ((pSidAdmins != NULL) && (EqualSid(pAceSid, pSidAdmins)))
							continue;
						if ((pSidTrustedInstaller != NULL) && (EqualSid(pAceSid, pSidTrustedInstaller)))
							continue;

						rootOnly = false; // A non-privileged trustee can create entries
					}
				}
			}
		}
	}

	if (pSidSystem != NULL)
		FreeSid(pSidSystem);
	if (pSidAdmins != NULL)
		FreeSid(pSidAdmins);
	if (pSidTrustedInstaller != NULL)
		FreeSid(pSidTrustedInstaller);
	if (pSD != NULL)
		LocalFree(pSD);

	return rootOnly;
}

bool IWindows::SocketIsValid(HSOCKET s)
{
	return (s != INVALID_SOCKET);
}

void IWindows::SocketMarkReuseAddr(HSOCKET s)
{
	// TOFIX, understand security issues between SO_REUSEADDR and SO_EXCLUSIVEADDRUSE
	/* throw WSAEACCES */
	BOOL bOptVal1 = TRUE;
	int bOptLen1 = sizeof(BOOL);
	setsockopt(s, SOL_SOCKET, SO_REUSEADDR, (char*)&bOptVal1, bOptLen1);
}

void IWindows::SocketBlockMode(HSOCKET s, bool block)
{
	unsigned long m = (block ? 0 : 1);
	if (0 != ioctlsocket(s, FIONBIO, &m))
	{
		ThrowException("Error on SocketBlockMode");
	}
}

void IWindows::SocketClose(HSOCKET s)
{
	closesocket(s);
}

int IWindows::SocketGetLastErrorCode()
{
	return WSAGetLastError();
}

// --------------------------
// Virtual Pure, IPC transport
// --------------------------

void IWindows::TransportListen(int port)
{
#ifdef EDDIE_IPC_NAMEDPIPE
	// Named pipe transport. The NULL DACL lets any local user reach the SYSTEM-created pipe;
	// connecting clients are authenticated by the integrity gate in Main(), not by the pipe ACL.
	// TODO: restrict pipe-instance creation (FILE_FLAG_FIRST_PIPE_INSTANCE / create-instance ACE
	// for SYSTEM+Administrators) to prevent pipe-name squatting by another local user.
	// Pipe keyed by launch mode ("spot"/"service"); the port is only used by the TCP fallback below.
	(void)port;
	std::string pipeName = "\\\\.\\pipe\\eddie-elevated-" + GetLaunchMode();

	SECURITY_DESCRIPTOR pipeSd;
	InitializeSecurityDescriptor(&pipeSd, SECURITY_DESCRIPTOR_REVISION);
	SetSecurityDescriptorDacl(&pipeSd, TRUE, NULL, FALSE); // NULL DACL = any local user may connect
	SECURITY_ATTRIBUTES pipeSa;
	pipeSa.nLength = sizeof(pipeSa);
	pipeSa.lpSecurityDescriptor = &pipeSd;
	pipeSa.bInheritHandle = FALSE;

	m_ipcPipe = CreateNamedPipeA(
		pipeName.c_str(),
		PIPE_ACCESS_DUPLEX | FILE_FLAG_OVERLAPPED, // overlapped: read and write must not serialize on the same handle
		PIPE_TYPE_BYTE | PIPE_READMODE_BYTE | PIPE_WAIT,
		PIPE_UNLIMITED_INSTANCES,
		NETBUFSIZE, NETBUFSIZE, 0, &pipeSa);

	if (m_ipcPipe == INVALID_HANDLE_VALUE)
		ThrowException("Error on creating named pipe (" + std::to_string(GetLastError()) + ")");

	LogDevDebug("Listening on named pipe " + pipeName);
#else
	m_ipcServer = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);

	SocketMarkReuseAddr(m_ipcServer);

	if (SocketIsValid(m_ipcServer) == false)
	{
		ThrowException("Error on opening socket");
	}

	std::memset(&m_ipcAddrServer, 0, sizeof(m_ipcAddrServer));

	m_ipcAddrServer.sin_family = AF_INET;
	m_ipcAddrServer.sin_addr.s_addr = htonl(INADDR_LOOPBACK);
	m_ipcAddrServer.sin_port = htons(port);

	LogDevDebug("Listening on port " + std::to_string(port));

	if (bind(m_ipcServer, (struct sockaddr*)&m_ipcAddrServer, sizeof(m_ipcAddrServer)) != 0) {
		ThrowException("Error on binding socket (" + std::to_string(SocketGetLastErrorCode()) + ")");
	}

	if (listen(m_ipcServer, 1) != 0) {
		ThrowException("Error on listen socket (" + std::to_string(SocketGetLastErrorCode()) + ")");
	}

	SocketBlockMode(m_ipcServer, false);
#endif
}

bool IWindows::TransportAccept()
{
#ifdef EDDIE_IPC_NAMEDPIPE
	// Spot mode is a one-shot elevation: if the authorized client never connects, do not wait forever.
	// Giving up lets Main() close the pipe so no elevated process is left orphaned.
	unsigned long acceptStartTime = GetTimestampUnix();
	const unsigned long spotAcceptTimeoutSeconds = 5;

	for (;;)
	{
		if (IsStopRequested())
			return false;

		if ((GetLaunchMode() == "spot") && (GetTimestampUnix() - acceptStartTime >= spotAcceptTimeoutSeconds))
		{
			LogDebug("Spot mode: no client connected within timeout, giving up");
			return false;
		}

		OVERLAPPED ovConnect;
		memset(&ovConnect, 0, sizeof(ovConnect));
		ovConnect.hEvent = CreateEvent(NULL, TRUE, FALSE, NULL);

		BOOL pipeConnected = ConnectNamedPipe(m_ipcPipe, &ovConnect);
		DWORD connectErr = GetLastError();

		if ((pipeConnected == FALSE) && (connectErr == ERROR_PIPE_CONNECTED))
			pipeConnected = TRUE;
		else if ((pipeConnected == FALSE) && (connectErr == ERROR_IO_PENDING))
		{
			for (;;)
			{
				DWORD waitResult = WaitForSingleObject(ovConnect.hEvent, 1000);
				if (waitResult == WAIT_OBJECT_0)
				{
					DWORD dummy = 0;
					pipeConnected = GetOverlappedResult(m_ipcPipe, &ovConnect, &dummy, FALSE);
					break;
				}
				if (IsStopRequested())
				{
					CancelIo(m_ipcPipe);
					pipeConnected = FALSE;
					break;
				}
				if ((GetLaunchMode() == "spot") && (GetTimestampUnix() - acceptStartTime >= spotAcceptTimeoutSeconds))
				{
					CancelIo(m_ipcPipe);
					CloseHandle(ovConnect.hEvent);
					LogDebug("Spot mode: no client connected within timeout, giving up");
					return false;
				}
				Idle();
			}
		}

		CloseHandle(ovConnect.hEvent);

		if (pipeConnected)
			return true;

		if (IsStopRequested())
			return false;

		Idle();

		Sleep(1000);
	}
#else
	struct sockaddr_in addrClient;
	socklen_t addrClientLen = sizeof(addrClient);

	// Spot mode is a one-shot elevation: if the authorized client never connects, do not wait forever.
	unsigned long acceptStartTime = GetTimestampUnix();
	const unsigned long spotAcceptTimeoutSeconds = 5;

	for (;;)
	{
		m_ipcClient = accept(m_ipcServer, (struct sockaddr*)&addrClient, &addrClientLen);

		// TOFIX. Under Linux, errno==EWOULDBLOCK. Under Windows, i expect WSAEWOULDBLOCK but there are something not understanding.
		if (SocketIsValid(m_ipcClient) == false)
		{
			if (IsStopRequested())
				break;

			if ((GetLaunchMode() == "spot") && (GetTimestampUnix() - acceptStartTime >= spotAcceptTimeoutSeconds))
			{
				LogDebug("Spot mode: no client connected within timeout, giving up");
				break;
			}

			Idle();

			Sleep(1000);
		}
		else
		{
			break;
		}
	}

	if (SocketIsValid(m_ipcClient) == false)
		return false;

	m_ipcAddrClient = addrClient; // kept for TCP peer-pid resolution

	// Remove if nonblock is inherit
	SocketBlockMode(m_ipcClient, true);

	return true;
#endif
}

int IWindows::TransportGetClientProcessId()
{
#ifdef EDDIE_IPC_NAMEDPIPE
	// Peer pid from the kernel-attested named pipe (race-free, replaces GetExtendedTcpTable).
	int clientProcessId = 0;
	DWORD clientPidDword = 0;
	if (GetNamedPipeClientProcessId(m_ipcPipe, &clientPidDword))
		clientProcessId = (int)clientPidDword;
	return clientProcessId;
#else
	return GetProcessIdMatchingIPEndPoints(m_ipcAddrClient, m_ipcAddrServer);
#endif
}

int IWindows::TransportRead(char* buffer, int maxLen)
{
#ifdef EDDIE_IPC_NAMEDPIPE
	OVERLAPPED ovRead;
	memset(&ovRead, 0, sizeof(ovRead));
	ovRead.hEvent = CreateEvent(NULL, TRUE, FALSE, NULL);

	DWORD pipeRead = 0;
	BOOL pipeReadOk = ReadFile(m_ipcPipe, buffer, maxLen, NULL, &ovRead);
	if ((pipeReadOk == FALSE) && (GetLastError() == ERROR_IO_PENDING))
		pipeReadOk = GetOverlappedResult(m_ipcPipe, &ovRead, &pipeRead, TRUE);
	else if (pipeReadOk)
		GetOverlappedResult(m_ipcPipe, &ovRead, &pipeRead, TRUE);

	DWORD pipeReadErr = GetLastError();
	CloseHandle(ovRead.hEvent);

	if (pipeReadOk)
		return (int)pipeRead;
	else if ((pipeReadErr == ERROR_BROKEN_PIPE) || (pipeReadErr == ERROR_PIPE_NOT_CONNECTED))
		return 0; // client disconnected
	else
		return -1;
#else
	return recv(m_ipcClient, buffer, maxLen, 0);
#endif
}

int IWindows::TransportWrite(const char* buffer, int len)
{
#ifdef EDDIE_IPC_NAMEDPIPE
	OVERLAPPED ovWrite;
	memset(&ovWrite, 0, sizeof(ovWrite));
	ovWrite.hEvent = CreateEvent(NULL, TRUE, FALSE, NULL);

	DWORD pipeWritten = 0;
	BOOL pipeWriteOk = WriteFile(m_ipcPipe, buffer, (DWORD)len, NULL, &ovWrite);
	if ((pipeWriteOk == FALSE) && (GetLastError() == ERROR_IO_PENDING))
		pipeWriteOk = GetOverlappedResult(m_ipcPipe, &ovWrite, &pipeWritten, TRUE);
	else if (pipeWriteOk)
		GetOverlappedResult(m_ipcPipe, &ovWrite, &pipeWritten, TRUE);

	CloseHandle(ovWrite.hEvent);

	return pipeWriteOk ? (int)pipeWritten : -1;
#else
	return send(m_ipcClient, buffer, len, 0);
#endif
}

void IWindows::TransportClientClose()
{
#ifdef EDDIE_IPC_NAMEDPIPE
	// Keep the pipe instance for the next client; just drop the current connection.
	DisconnectNamedPipe(m_ipcPipe);
#else
	SocketClose(m_ipcClient);
	m_ipcClient = 0;
#endif
}

void IWindows::TransportServerClose()
{
#ifdef EDDIE_IPC_NAMEDPIPE
	CloseHandle(m_ipcPipe);
#else
	SocketClose(m_ipcServer);
#endif
}

std::string IWindows::StringEnsureInterfaceName(const std::string& str)
{
	std::string r;
	r.reserve(str.size());
	for (size_t i = 0; i < str.size(); ++i)
	{
		unsigned char c = static_cast<unsigned char>(str[i]);
		if (c < 0x20 || c == 0x7F) continue;
		if (c == '"') continue;
		r.push_back(static_cast<char>(c));
	}
	if (r.length() > 256) r = r.substr(0, 256);
	return r;
}

bool IWindows::SystemWideDataSet(const std::string& key, const std::string& value)
{
	std::wstring keyW = StringUTF8ToWString(key);
	std::wstring valueW = StringUTF8ToWString(value);
	HKEY hKey;
	
	if (RegCreateKeyEx(HKEY_LOCAL_MACHINE, L"SOFTWARE\\Eddie-VPN", 0, NULL, 0, KEY_WRITE, NULL, &hKey, NULL) != ERROR_SUCCESS)
		return false;
	DWORD dataSize = static_cast<DWORD>((value.size() + 1) * sizeof(wchar_t));
	bool result = (RegSetValueEx(hKey, keyW.c_str(), 0, REG_SZ, (const BYTE*)valueW.c_str(), dataSize) == ERROR_SUCCESS);
	RegCloseKey(hKey);
	return result;
}

std::string IWindows::SystemWideDataGet(const std::string& key, const std::string& def)
{
	std::wstring keyW = StringUTF8ToWString(key);
	HKEY hKey;
	wchar_t buffer[4096];
	DWORD bufferSize = sizeof(buffer);

	if (RegOpenKeyEx(HKEY_LOCAL_MACHINE, L"SOFTWARE\\Eddie-VPN", 0, KEY_READ, &hKey) != ERROR_SUCCESS)
		return def;
	bool result = (RegQueryValueEx(hKey, keyW.c_str(), NULL, NULL, reinterpret_cast<BYTE*>(buffer), &bufferSize) == ERROR_SUCCESS);
	RegCloseKey(hKey);
	if (result)
		return StringWStringToUTF8(std::wstring(buffer));
	else
		return def;
}

bool IWindows::SystemWideDataDel(const std::string& key)
{
	std::wstring keyW = StringUTF8ToWString(key);
	HKEY hKey;

	if (RegOpenKeyEx(HKEY_LOCAL_MACHINE, L"SOFTWARE\\Eddie-VPN", 0, KEY_SET_VALUE, &hKey) != ERROR_SUCCESS)
		return false;

	bool result = (RegDeleteValue(hKey, keyW.c_str()) == ERROR_SUCCESS);
	RegCloseKey(hKey);
	return result;
}

bool IWindows::SystemWideDataClean()
{
	return (RegDeleteKeyW(HKEY_LOCAL_MACHINE, L"SOFTWARE\\Eddie-VPN") == ERROR_SUCCESS);
}

std::string IWindows::CheckIfClientPathIsAllowed(const std::string& path)
{	
	/*
	// MacOS have equivalent.
	// Linux don't have any signature check (and can't have, because in distro like Arch, binary are builded client-side from sources
	// This is probably a superfluous check, and can cause issue for who compile from sources.
	// If implemented, need a conversion from C# to C++ of the code below.
	// Not yet implemented, redundant.

	// Check signature (optional)
	{
		try
		{
			System.Security.Cryptography.X509Certificates.X509Certificate c1 = System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromSignedFile(System.Reflection.Assembly.GetEntryAssembly().Location);

			// If above don't throw exception, Elevated it's signed, so it's mandatory that client is signed from same subject.
			try
			{
				System.Security.Cryptography.X509Certificates.X509Certificate c2 = System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromSignedFile(clientPath);

				bool match = (
					(c1.Issuer == c2.Issuer) &&
					(c1.Subject == c2.Subject) &&
					(c1.GetCertHashString() == c2.GetCertHashString()) &&
					(c1.GetEffectiveDateString() == c2.GetEffectiveDateString()) &&
					(c1.GetPublicKeyString() == c2.GetPublicKeyString()) &&
					(c1.GetRawCertDataString() == c2.GetRawCertDataString()) &&
					(c1.GetSerialNumberString() == c2.GetSerialNumberString())
					);

				if (match == false)
					return "Client not allowed: digital signature mismatch";
			}
			catch (Exception)
			{
				return "Client not allowed: digital signature not available";
			}
		}
		catch (Exception)
		{
			// Not signed, but maybe compiled from source, it's an optional check.
		}
	}
	*/

	return "ok";
}

std::string IWindows::CheckExecutablePathPermissions(const std::string& path)
{
	std::string issues = "";

	if (FsFileExists(path) == false)
	{
		issues += "not found;";
	}
	else
	{
		std::wstring pathW = StringUTF8ToWString(path);

		DWORD dwRes;
		PACL pDacl = NULL;
		PSECURITY_DESCRIPTOR pSD = NULL;

		dwRes = GetNamedSecurityInfoW(pathW.c_str(), SE_FILE_OBJECT, DACL_SECURITY_INFORMATION, NULL, NULL, &pDacl, NULL, &pSD);
		if (dwRes != ERROR_SUCCESS)
		{
			issues += "GetNamedSecurityInfoW error;";
		}
		else
		{
			PSID pSidSystem = NULL;
			PSID pSidAdmins = NULL;
			PSID pSidTrustedInstaller = NULL;
			ConvertStringSidToSidW(L"S-1-5-18", &pSidSystem); // SYSTEM
			ConvertStringSidToSidW(L"S-1-5-32-544", &pSidAdmins); // SID Administrators
			ConvertStringSidToSidW(L"S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464", &pSidTrustedInstaller); // SID TrustedInstaller

			for (DWORD i = 0; i < pDacl->AceCount; i++)
			{
				LPVOID pAce;
				if (GetAce(pDacl, i, &pAce))
				{
					ACE_HEADER* aceHeader = (ACE_HEADER*)pAce;
					if (aceHeader->AceType == ACCESS_ALLOWED_ACE_TYPE)
					{
						ACCESS_ALLOWED_ACE* allowedAce = (ACCESS_ALLOWED_ACE*)pAce;
						if ((allowedAce->Mask & FILE_WRITE_DATA) == FILE_WRITE_DATA)
						{
							PSID pAceSid = (PSID)&allowedAce->SidStart;

							if ((pSidSystem != NULL) && (EqualSid(pAceSid, pSidSystem)))
								continue;
							if ((pSidAdmins != NULL) && (EqualSid(pAceSid, pSidAdmins)))
								continue;
							if ((pSidTrustedInstaller != NULL) && (EqualSid(pAceSid, pSidTrustedInstaller)))
								continue;

							char name[256], domain[256];
							DWORD nameSize = sizeof(name), domainSize = sizeof(domain);
							SID_NAME_USE sidType;

							if (LookupAccountSidA(NULL, pAceSid, name, &nameSize, domain, &domainSize, &sidType))
							{
								issues += std::string(domain) + "\\" + std::string(name) + " has write;";
							}
							else
							{
								issues += "unknown has write;";
							}
						}
					}
				}
			}

			if (pSidSystem != NULL)
				FreeSid(pSidSystem);
			if (pSidAdmins != NULL)
				FreeSid(pSidAdmins);
			if (pSidTrustedInstaller != NULL)
				FreeSid(pSidTrustedInstaller);
		}

		if (pSD != NULL)
			LocalFree(pSD);
	}

	return issues;
}

#ifndef EDDIE_IPC_LOCAL
int IWindows::GetProcessIdMatchingIPEndPoints(struct sockaddr_in& addrClient, struct sockaddr_in& addrServer)
{
	std::vector<unsigned char> buffer;
	DWORD dwSize = sizeof(MIB_TCPTABLE_OWNER_PID);
	DWORD dwRetValue = 0;

	do {
		buffer.resize(dwSize, 0);
		dwRetValue = GetExtendedTcpTable(buffer.data(), &dwSize, TRUE, AF_INET, TCP_TABLE_OWNER_PID_ALL, 0);
	} while (dwRetValue == ERROR_INSUFFICIENT_BUFFER);
	if (dwRetValue == ERROR_SUCCESS)
	{
		PMIB_TCPTABLE_OWNER_PID ptTable = reinterpret_cast<PMIB_TCPTABLE_OWNER_PID>(buffer.data());
		for (DWORD i = 0; i < ptTable->dwNumEntries; i++) {
			DWORD pid = ptTable->table[i].dwOwningPid;

			if ((ptTable->table[i].dwLocalAddr == addrClient.sin_addr.s_addr) &&
				(ptTable->table[i].dwRemoteAddr == addrServer.sin_addr.s_addr) &&
				(ptTable->table[i].dwLocalPort == addrClient.sin_port) &&
				(ptTable->table[i].dwRemotePort == addrServer.sin_port)
				)
			{
				return pid;
			}
		}
	}

	return 0;
}
#endif // !EDDIE_IPC_LOCAL


// --------------------------
// Protected
// --------------------------

std::string IWindows::GetLastErrorAsString()
{
	DWORD errorMessageID = ::GetLastError();
	if (errorMessageID == 0)
		return std::string();

	wchar_t buf[2048];
	FormatMessageW(FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS, NULL, GetLastError(), MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), buf, (sizeof(buf) / sizeof(wchar_t)), NULL);
	std::string message = StringWStringToUTF8(buf);

	return StringTrim(message);
}

void IWindows::ExecArg(const std::wstring& arg, std::wstring& cmdline)
{
	if (!arg.empty() && arg.find_first_of(L" \t\n\v\"") == std::wstring::npos)
	{
		cmdline.append(arg);
		return;
	}

	cmdline.push_back(L'"');
	for (std::wstring::const_iterator it = arg.begin(); ; ++it)
	{
		unsigned backslashes = 0;
		while (it != arg.end() && *it == L'\\')
		{
			++it;
			++backslashes;
		}

		if (it == arg.end())
		{
			cmdline.append(backslashes * 2, L'\\');
			break;
		}
		else if (*it == L'"')
		{
			cmdline.append(backslashes * 2 + 1, L'\\');
			cmdline.push_back(*it);
		}
		else
		{
			cmdline.append(backslashes, L'\\');
			cmdline.push_back(*it);
		}
	}
	cmdline.push_back(L'"');
}

IWindows::t_shellinfo IWindows::ExecStart(const std::string& path, const std::vector<std::string>& args)
{
	t_shellinfo info;

	SECURITY_ATTRIBUTES saAttr;
	HANDLE stdoutWriteHandle = NULL;
	HANDLE stderrWriteHandle = NULL;

	std::wstring pathW = StringUTF8ToWString(path);

	std::wstring cmdline = L"\"" + pathW + L"\"";
	for (std::vector<std::string>::const_iterator i = args.begin(); i != args.end(); ++i)
	{
		cmdline += L" ";
		ExecArg(StringUTF8ToWString(*i), cmdline);
	}

	// CreateProcessW may modify lpCommandLine in place, so it requires a writable buffer.
	std::vector<wchar_t> pcmdline(cmdline.begin(), cmdline.end());
	pcmdline.push_back(L'\0');

	memset(&saAttr, 0, sizeof(saAttr));
	saAttr.nLength = sizeof(SECURITY_ATTRIBUTES);
	saAttr.bInheritHandle = TRUE;
	saAttr.lpSecurityDescriptor = NULL;

	// Create a pipe for the child process's STDOUT. 
	if (!CreatePipe(&info.stdoutReadHandle, &stdoutWriteHandle, &saAttr, 5000))
	{
		info.lastErrorCode = GetLastError();
		info.lastError = GetLastErrorAsString();
		return info;
	}

	// Ensure the read handle to the pipe for STDOUT is not inherited.
	if (!SetHandleInformation(info.stdoutReadHandle, HANDLE_FLAG_INHERIT, 0))
	{
		info.lastErrorCode = GetLastError();
		info.lastError = GetLastErrorAsString();
		return info;
	}

	// Create a pipe for the child process's STDERR. 
	if (!CreatePipe(&info.stderrReadHandle, &stderrWriteHandle, &saAttr, 5000))
	{
		info.lastErrorCode = GetLastError();
		info.lastError = GetLastErrorAsString();
		return info;
	}

	// Ensure the read handle to the pipe for STDOUT is not inherited.
	if (!SetHandleInformation(info.stderrReadHandle, HANDLE_FLAG_INHERIT, 0))
	{
		info.lastErrorCode = GetLastError();
		info.lastError = GetLastErrorAsString();
		return info;
	}

	memset(&info.startupInfo, 0, sizeof(info.startupInfo));
	info.startupInfo.cb = sizeof(info.startupInfo);
	info.startupInfo.hStdError = stderrWriteHandle;
	info.startupInfo.hStdOutput = stdoutWriteHandle;
	//info.startupInfo.hStdInput = GetStdHandle(STD_INPUT_HANDLE);
	info.startupInfo.dwFlags |= STARTF_USESTDHANDLES;
	info.startupInfo.dwFlags |= STARTF_USESHOWWINDOW;
	//info.startupInfo.dwFlags |= CREATE_NEW_PROCESS_GROUP | DETACHED_PROCESS;	
	info.startupInfo.wShowWindow = SW_HIDE;

	memset(&info.processInfo, 0, sizeof(info.processInfo));

	// VS2019 raise a C6335 warning, false positive.
	if (!CreateProcessW(pathW.c_str(), pcmdline.data(), NULL, NULL, TRUE, CREATE_NO_WINDOW, NULL, 0, &info.startupInfo, &info.processInfo))
	{
		ExecCleanup(info);
		info.lastErrorCode = GetLastError();
		info.lastError = GetLastErrorAsString();
		return info;
	}

	info.pid = info.processInfo.dwProcessId;

	CloseHandle(stdoutWriteHandle);
	CloseHandle(stderrWriteHandle);

	return info;
}

DWORD IWindows::ExecEnd(t_shellinfo info)
{
	DWORD exitcode;

	if (WaitForSingleObject(info.processInfo.hProcess, INFINITE) != WAIT_OBJECT_0)
	{
		ExecCleanup(info);
		return GetLastError();
	}

	if (!GetExitCodeProcess(info.processInfo.hProcess, &exitcode))
	{
		ExecCleanup(info);
		return GetLastError();
	}

	ExecCleanup(info);

	return exitcode;
}

void IWindows::ExecCleanup(IWindows::t_shellinfo info)
{
	CloseHandle(info.stdoutReadHandle);
	CloseHandle(info.stderrReadHandle);
	CloseHandle(info.processInfo.hProcess);
	CloseHandle(info.processInfo.hThread);
}

int IWindows::SetInterfaceMetric(const int index, const std::string layer, const int value)
{
	DWORD err = 0;

	MIB_IPINTERFACE_ROW ipiface;
	memset(&ipiface, 0, sizeof(MIB_IPINTERFACE_ROW));
	InitializeIpInterfaceEntry(&ipiface);
	if (layer == "ipv4")
		ipiface.Family = AF_INET;
	else if (layer == "ipv6")
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

// Note: used both for Eddie Elevated service AND Wireguard service
bool IWindows::ServiceGenericDelete(const std::string& id)
{
	bool success = false;

	SC_HANDLE serviceControlManager = OpenSCManager(0, 0, SC_MANAGER_ALL_ACCESS); // GENERIC_WRITE is not enough

	if (serviceControlManager)
	{
		std::wstring serviceIdW = StringUTF8ToWString(id);
		SC_HANDLE service = OpenService(serviceControlManager, serviceIdW.c_str(), SC_MANAGER_ALL_ACCESS);
		if (service)
		{
			SERVICE_STATUS serviceStatus;

			for (int t = 0; t < 100; t++) // 10 seconds timeout
			{
				if (QueryServiceStatus(service, &serviceStatus))
				{
					if (serviceStatus.dwCurrentState == SERVICE_RUNNING)
					{
						ControlService(service, SERVICE_CONTROL_STOP, &serviceStatus);
						::Sleep(1000);
					}
					else if (serviceStatus.dwCurrentState == SERVICE_STOPPED)
					{
						if (DeleteService(service))
							success = true;

						break;
					}
					else
					{
						::Sleep(100);
					}
				}
				else
					break;
			}

			CloseServiceHandle(service);
		}

		CloseServiceHandle(serviceControlManager);

		return success;
	}
	else
		return false;
}

std::string IWindows::GetBundledInfPath(const std::string& driver)
{
	std::string infName;
	if (driver == "ovpn-dco")
		infName = "ovpn-dco.inf";
	else if (driver == "tap-windows6")
		infName = "tap0901.inf";
	else
		return "";

	std::string tapctl = FsLocateExecutable("tapctl.exe", false, true);
	if (tapctl == "")
		return "";

	std::string path = FsFileGetDirectory(tapctl) + FsPathSeparator + infName;
	if (FsFileExists(path))
		return path;

	return "";
}

std::string IWindows::DriverInstall(const std::string& driver)
{
	if (driver != "ovpn-dco" && driver != "tap-windows6")
		ThrowException("Unknown driver: " + driver);

	std::string infPath = GetBundledInfPath(driver);
	if (infPath == "")
		ThrowException("Driver INF not found for: " + driver);

	std::string pnputil = FsLocateExecutable("pnputil.exe");

	ExecResult result = ExecEx(pnputil, { "/add-driver", infPath, "/install" });

	// exit 0 = new install, exit 259 = already up-to-date, exit 5 = already exists in store
	if (result.exit != 0 && result.exit != 259 && result.exit != 5)
	{
		ThrowException("pnputil /add-driver failed for " + driver +
			": exit=" + std::to_string(result.exit) +
			" out=" + result.out +
			" err=" + result.err);
	}

	// Extract "Published Name:" from output and store for future uninstall
	std::string publishedName = StringTrim(StringExtractBetween(result.out, "Published Name:", "\n"));
	if (publishedName != "")
		SystemWideDataSet("driver_oem_" + driver, publishedName);

	if (result.exit == 259 || result.exit == 5)
		return "already-installed";

	return "installed";
}

std::string IWindows::DriverUninstall(const std::string& driver)
{
	if (driver != "ovpn-dco" && driver != "tap-windows6")
		ThrowException("Unknown driver: " + driver);

	std::string oemInf = SystemWideDataGet("driver_oem_" + driver, "");
	if (oemInf == "")
		return "not-found";

	std::string pnputil = FsLocateExecutable("pnputil.exe");

	ExecResult result = ExecEx(pnputil, { "/delete-driver", oemInf, "/uninstall" });

	// exit 0 = uninstalled, exit -536870340 = already gone
	if (result.exit != 0 && result.exit != -536870340)
	{
		ThrowException("pnputil /delete-driver failed for " + driver +
			" (" + oemInf + "): exit=" + std::to_string(result.exit) +
			" out=" + result.out +
			" err=" + result.err);
	}

	SystemWideDataDel("driver_oem_" + driver);

	return "uninstalled";
}

std::string IWindows::NetworkAdapterCreate(const std::string& driver, const std::string& name)
{
	if (driver != "ovpn-dco" && driver != "tap-windows6")
		ThrowException("Unknown driver: " + driver);

	std::string nameSafe = StringEnsureInterfaceName(name);

	std::string guid = "";

	if (driver == "ovpn-dco")
	{
		std::string tapctl = FsLocateExecutable("tapctl.exe", false, true);
		if (tapctl != "")
		{
			ExecResult result = ExecEx(tapctl, { "create", "--hwid", "ovpn-dco", "--name", nameSafe });
			if (result.exit == 0)
				guid = StringTrim(result.out);
		}
	}
	else if (driver == "tap-windows6")
	{
		std::string tapctl = FsLocateExecutable("tapctl.exe", false, true);
		if (tapctl != "")
		{
			ExecResult result = ExecEx(tapctl, { "create", "--hwid", "root\\tap0901", "--name", nameSafe });
			if (result.exit == 0)
				guid = StringTrim(result.out);
		}
	}

	// tapctl prints "{GUID}\t<name>\t<hwid>"; keep only the GUID
	size_t tab = guid.find('\t');
	if (tab != std::string::npos)
		guid = StringTrim(guid.substr(0, tab));

	if (guid != "")
	{
		std::string eddieList = SystemWideDataGet("network_adapters", "");
		eddieList += guid + ";";
		SystemWideDataSet("network_adapters", eddieList);

		return guid;
	}
	else
		return "";
}

bool IWindows::NetworkAdapterDelete(const std::string& id)
{
	std::string idSafe = StringPruneCharsNotIn(id, "0123456789abcdefABCDEF-{}");

	std::string tapctl = FsLocateExecutable("tapctl.exe", false, true);
	if (tapctl != "")
	{
		ExecResult result = ExecEx(tapctl, { "delete", idSafe });
		if (result.exit == 0)
		{
			std::string eddieList = SystemWideDataGet("network_adapters", "");
			eddieList = StringReplaceAll(eddieList, idSafe + ";", "");
			SystemWideDataSet("network_adapters", eddieList);
			return true;
		}
	}

	return false;
}

void IWindows::NetworkAdapterDeleteAll()
{
	std::vector<std::string> list = StringToVector(SystemWideDataGet("network_adapters", ""), ';', true);
	for (std::vector<std::string>::const_iterator i = list.begin(); i != list.end(); ++i)
	{
		std::string id = *i;
		NetworkAdapterDelete(id);
	}
}

// Parse an "address/cidr" token (IPv4 or IPv6) into family, raw address and prefix length.
bool IWindows::WireGuardParseInetCidr(const std::string& token, ADDRESS_FAMILY& family, IN_ADDR& v4, IN6_ADDR& v6, BYTE& cidr)
{
	std::string value = token;
	std::string prefix = "";
	size_t posSlash = value.find('/');
	if (posSlash != std::string::npos)
	{
		prefix = value.substr(posSlash + 1);
		value = value.substr(0, posSlash);
	}

	if (inet_pton(AF_INET, value.c_str(), &v4) == 1)
	{
		family = AF_INET;
		cidr = (BYTE)(prefix.empty() ? 32 : atoi(prefix.c_str()));
		return true;
	}

	if (inet_pton(AF_INET6, value.c_str(), &v6) == 1)
	{
		family = AF_INET6;
		cidr = (BYTE)(prefix.empty() ? 128 : atoi(prefix.c_str()));
		return true;
	}

	return false;
}

// Parse a "host:port" / "[ipv6]:port" endpoint into a SOCKADDR_INET, resolving host names.
bool IWindows::WireGuardParseEndpoint(const std::string& endpoint, SOCKADDR_INET& out)
{
	std::string host = endpoint;
	std::string port = "";
	size_t posColon = endpoint.rfind(':');
	if (posColon == std::string::npos)
		return false;
	host = endpoint.substr(0, posColon);
	port = endpoint.substr(posColon + 1);
	if (host.size() >= 2 && host.front() == '[' && host.back() == ']')
		host = host.substr(1, host.size() - 2);

	addrinfo hints;
	memset(&hints, 0, sizeof(hints));
	hints.ai_family = AF_UNSPEC;
	hints.ai_socktype = SOCK_DGRAM;
	hints.ai_protocol = IPPROTO_UDP;

	addrinfo* result = NULL;
	if (getaddrinfo(host.c_str(), port.c_str(), &hints, &result) != 0 || result == NULL)
		return false;

	bool found = false;
	for (addrinfo* it = result; it != NULL; it = it->ai_next)
	{
		if (it->ai_family == AF_INET && it->ai_addrlen >= sizeof(sockaddr_in))
		{
			memset(&out, 0, sizeof(out));
			memcpy(&out.Ipv4, it->ai_addr, sizeof(sockaddr_in));
			found = true;
			break;
		}
		if (it->ai_family == AF_INET6 && it->ai_addrlen >= sizeof(sockaddr_in6))
		{
			memset(&out, 0, sizeof(out));
			memcpy(&out.Ipv6, it->ai_addr, sizeof(sockaddr_in6));
			found = true;
			break;
		}
	}

	freeaddrinfo(result);
	return found;
}

// Public

std::wstring IWindows::StringUTF8ToWString(const std::string& str)
{
	// Pre C++17
	//std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> conv;
	//return conv.from_bytes(str);
	if (str.empty()) return std::wstring();
	int sizeNeeded = MultiByteToWideChar(CP_UTF8, 0, &str[0], (int)str.size(), NULL, 0);
	std::wstring wstrTo(sizeNeeded, 0);
	MultiByteToWideChar(CP_UTF8, 0, &str[0], (int)str.size(), &wstrTo[0], sizeNeeded);
	return wstrTo;
}

std::string IWindows::StringWStringToUTF8(const std::wstring& wstr)
{
	// Pre C++17
	//std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> conv;
	//return conv.to_bytes(str);
	if (wstr.empty()) return std::string();
	int sizeNeeded = WideCharToMultiByte(CP_UTF8, 0, &wstr[0], (int)wstr.size(), NULL, 0, NULL, NULL);
	std::string strTo(sizeNeeded, 0);
	WideCharToMultiByte(CP_UTF8, 0, &wstr[0], (int)wstr.size(), &strTo[0], sizeNeeded, NULL, NULL);
	return strTo;
}
