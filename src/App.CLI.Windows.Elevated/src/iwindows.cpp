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

#include <fstream>
#include <sstream>

#include "psapi.h"
#include "tlhelp32.h"

#include <codecvt> // For StringUTF8ToWString
#include "sddl.h"	// For ConvertSidToStringSid
#include "accctrl.h" // For GetSecurityInfo
#include "aclapi.h" // For GetSecurityInfo
#include <VersionHelpers.h> // For IsWindows8OrGreater

void IWindows::Do(const std::string& commandId, const std::string& command, std::map<std::string, std::string>& params)
{
	if (command == "dns-flush")
	{
		std::string mode = params["mode"];

		std::string netPath = FsLocateExecutable("net.exe", false);
		std::string ipconfigPath = FsLocateExecutable("ipconfig.exe", false);

		if (mode == "max")
		{
			if (netPath != "")
			{
				ExecEx1(netPath, "stop dnscache");
				ExecEx1(netPath, "start dnscache");
			}
			if (ipconfigPath != "")
				ExecEx1(ipconfigPath, "/registerdns");
		}

		if (ipconfigPath != "")
			ExecEx1(ipconfigPath, "/flushdns");
	}
	else if (command == "windows-dns")
	{
		std::string interfaceName = params["interface"];
		std::string layer = ((params["layer"] == "ipv4") ? "ipv4" : "ipv6");
		std::string mode = params["mode"];
		if (mode == "dhcp")
		{
			ExecEx1(FsLocateExecutable("netsh.exe"), "interface " + layer + " set dns name=\"" + StringEnsureQuote(StringEnsureInterfaceName(interfaceName)) + "\" source=dhcp register=primary validate=no");
		}
		else if (mode == "static")
		{
			std::string ipaddress = params["ipaddress"];
			ExecEx1(FsLocateExecutable("netsh.exe"), "interface " + layer + " set dns name=\"" + StringEnsureQuote(StringEnsureInterfaceName(interfaceName)) + "\" source=static address=" + StringEnsureIpAddress(ipaddress) + " register=primary validate=no");
		}
		else if (mode == "add")
		{
			std::string ipaddress = params["ipaddress"];
			ExecEx1(FsLocateExecutable("netsh.exe"), "interface " + layer + " add dnsserver name=\"" + StringEnsureQuote(StringEnsureInterfaceName(interfaceName)) + "\" address=" + StringEnsureIpAddress(ipaddress) + " validate=no");
		}
	}
	else if (command == "windows-firewall")
	{
		std::string args = StringTrim(params["args"]);
		ExecResult result = ExecEx1(FsLocateExecutable("netsh.exe"), "advfirewall " + args);
	}
	else if (command == "windows-workaround-25139")
	{
		std::string cidr = StringTrim(params["cidr"]);
		std::string iface = StringTrim(params["iface"]);
		ExecResult result = ExecEx1(FsLocateExecutable("netsh.exe"), "interface ipv6 del route \"" + StringEnsureCidr(cidr) + "\" interface=\"" + StringEnsureNumericInt(iface) + "\"");
	}
	else if (command == "windows-workaround-interface-up")
	{
		std::string name = StringTrim(params["name"]);
		ExecResult result = ExecEx1(FsLocateExecutable("netsh.exe"), "interface set interface \"" + StringEnsureQuote(StringEnsureInterfaceName(name)) + "\" ENABLED");
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
		std::string args = "interface ";
		if (StringIsIPv4(params["destination"]))
			args += " ipv4";
		else if (StringIsIPv6(params["destination"]))
			args += " ipv6";
		else
			ThrowException("Unknown layer");
		if (params["action"] == "add")
			args += " add";
		else if (params["action"] == "remove")
			args += " del";
		else
			ThrowException("Unknown action");
		args += " route";
		args += " prefix=\"" + StringEnsureCidr(params["destination"]) + "\"";
		args += " interface=\"" + StringEnsureNumericInt(params["iface"]) + "\"";
		if (params.find("gateway") != params.end())
		{
			// Remember: Win7 need also nexthop, >Win7 no.
			args += " nexthop=\"" + StringEnsureIpAddress(params["gateway"]) + "\"";
		}
		if (params["action"] == "add")
		{
			if (params.find("metric") != params.end())
				args += " metric=" + StringEnsureNumericInt(params["metric"]);
		}
		args += " store=active";

		ExecResult shellResult = ExecEx1(FsLocateExecutable("netsh.exe"), args);
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
			std::string path = params["path"];
			std::string config = params["config"];

			CheckIfExecutableIsAllowed(path, true);

			std::string checkResult = CheckValidOpenVpnConfigFile(config);
			if (checkResult != "")
			{
				ThrowException("Not supported OpenVPN config: " + checkResult);
			}
			else
			{
				std::vector<std::string> args;

				args.push_back("--config");
				args.push_back("\"" + params["config"] + "\"");

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
		std::string driver = params["driver"];
		std::string name = params["name"];

		std::string result = NetworkAdapterCreate(driver, name);

		ReplyCommand(commandId, result);		
	}
	else if (command == "network-adapter-delete")
	{
		std::string id = params["id"];

		NetworkAdapterDelete(id);
	}
	else if (command == "network-adapter-clear-all")
	{
		NetworkAdapterDeleteAll();
	}
	else if (command == "wireguard-version")
	{
		std::string version = "";
		if (IsWindows8OrGreater()) // see WintunEnsureLibrary
			version = "0.10.1"; // Embedded, wgtunnel.dll
		ReplyCommand(commandId, version);
	}
	else if (command == "wireguard")
	{
		std::string id = params["id"];
		std::string action = params["action"];
		std::string interfaceId = params["interface"];

		std::string keypairStopRequest = "wireguard_stop_" + id;

		if (action == "stop")
		{
			m_keypair[keypairStopRequest] = "stop";
			//std::string serviceId = "WireGuardTunnel_" + interfaceId;
			//ServiceGenericDelete(serviceId);
		}
		else if (action == "start")
		{
			std::string config = params["config"];
			unsigned long handshakeTimeoutFirst = StringToULong(params["handshake_timeout_first"]);
			unsigned long handshakeTimeoutConnected = StringToULong(params["handshake_timeout_connected"]);

			std::string checkResult = CheckValidWireGuardConfig(config);
			if (checkResult != "")
			{
				ThrowException("Not supported WireGuard config: " + checkResult);
			}
			else
			{
				ReplyCommand(commandId, "log:setup-start");

				SC_HANDLE serviceControlManager = 0;
				SC_HANDLE service = 0;
				HANDLE hPipe = 0;

				std::string serviceId = "WireGuardTunnel_" + interfaceId;
				std::string tempPath = FsGetTempPath() + "EddieTemp_" + interfaceId;
				std::string configPath = "";
				std::string ringPath = "";

				try
				{
					ServiceGenericDelete(serviceId); // Delete if already exists

					if (FsDirectoryCreate(tempPath) == false)
						ThrowException("Unable to create config directory (" + GetLastErrorAsString() + ")");

					configPath = tempPath + FsPathSeparator + interfaceId + ".conf";

					if (FsFileWriteText(configPath, config) == false)
						ThrowException("Unable to write config file (" + GetLastErrorAsString() + ")");

					ringPath = tempPath + FsPathSeparator + "log.bin";

					if (FsFileExists(ringPath))
						if (FsFileDelete(ringPath) == false)
							ThrowException("Unable to clean previous ring file (" + GetLastErrorAsString() + ")");

					// Start an ad-hoc service (cannot call wgtunnel.dll directly here), required by current Windows WireGuard code.
					serviceControlManager = OpenSCManager(0, 0, SC_MANAGER_ALL_ACCESS); // GENERIC_WRITE is not enough
					if (!serviceControlManager)
						ThrowException("Services management failed (" + GetLastErrorAsString() + ")");
					else
					{
						std::string elevatedPath = GetProcessPathCurrent();
						std::string path = FsFileGetDirectory(GetProcessPathCurrent()) + FsPathSeparator + "Eddie-CLI-Elevated.exe";
						std::string pathWithArgs = "\"" + path + "\" mode=wireguard config=\"" + configPath + "\"";

						std::wstring serviceIdW = StringUTF8ToWString(serviceId);
						std::wstring serviceNameW = TEXT("WireGuard Eddie - Interface ") + StringUTF8ToWString(interfaceId);
						std::wstring servicePathW = StringUTF8ToWString(pathWithArgs);
						LPCWSTR serviceDependsW = TEXT("Nsi\0TcpIp"); // Added in 2.21.0
						serviceDependsW = NULL; // Removed, cause issue. Anyway Elevated itself have this depends.
						service = CreateService(serviceControlManager, serviceIdW.c_str(), serviceNameW.c_str(), SC_MANAGER_ALL_ACCESS, SERVICE_WIN32_OWN_PROCESS, SERVICE_DEMAND_START, SERVICE_ERROR_NORMAL, servicePathW.c_str(), NULL, NULL, serviceDependsW, NULL, NULL);
						if (!service)
							ThrowException("Service creation failed (" + GetLastErrorAsString() + ")");
						else
						{
							// Init service
							SERVICE_SID_INFO svcSidInfo;
							svcSidInfo.dwServiceSidType = SERVICE_SID_TYPE_UNRESTRICTED;
							if (!ChangeServiceConfig2(service, SERVICE_CONFIG_SERVICE_SID_INFO, &svcSidInfo))
								ThrowException("Change type failed (" + GetLastErrorAsString() + ")");

							// Start service
							if (!StartService(service, 0, NULL))
							{
								// If the service fail to start, it write reason in a temporary file.
								std::string errorPath = configPath + ".fatal";
								if (FsFileExists(errorPath))
									ThrowException("Failed to start: " + FsFileReadText(errorPath));
								else
									ThrowException("Failed to start: " + GetLastErrorAsString());
							}

							// Wait running
							bool waitSuccess = false;
							SERVICE_STATUS serviceStatusWait;
							for (int t = 0; t < 100; t++) // 10 seconds timeout
							{
								if (QueryServiceStatus(service, &serviceStatusWait))
								{
									if (serviceStatusWait.dwCurrentState == SERVICE_RUNNING)
									{
										waitSuccess = true;
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
							if (!waitSuccess)
								ThrowException("Failed to start: not running");

							/*
							// Data Pipe - Not yet implemented, not need
							const int BUFSIZE = 512;

							TCHAR  chBuf[BUFSIZE];
							BOOL   fSuccess = FALSE;
							DWORD  cbRead, cbToWrite, cbWritten, dwMode;
							std::wstring pipeCommandW = StringUTF8ToWString("get=1\n\n");
							std::wstring pipeNameW = StringUTF8ToWString("\\\\.\\pipe\\ProtectedPrefix\\Administrators\\WireGuard\\Eddie_WireGuard");
							LPCTSTR lpvMessage = pipeCommandW.c_str();
							while (1)
							{
								hPipe = CreateFile(pipeNameW.c_str(), GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_EXISTING, 0, NULL);
								if (hPipe != INVALID_HANDLE_VALUE)
									break;
								if (GetLastError() != ERROR_PIPE_BUSY)
									ThrowException("Could not open pipe (" + GetLastErrorAsString() + ")");
								if (!WaitNamedPipe(pipeNameW.c_str(), 10000))
									ThrowException("Could not open pipe: 20 second wait timed out");
							}

							dwMode = PIPE_READMODE_MESSAGE;
							fSuccess = SetNamedPipeHandleState(hPipe, &dwMode, NULL, NULL);
							if (!fSuccess)
								ThrowException("SetNamedPipeHandleState failed (" + GetLastErrorAsString() + ")");

							for(;;)
							{
								{
									cbToWrite = (lstrlen(lpvMessage) + 1) * sizeof(TCHAR);
									fSuccess = WriteFile(hPipe, pipeCommandW.c_str(), cbToWrite, &cbWritten, NULL);
									if (!fSuccess)
										ThrowException("WriteFile to pipe failed (" + GetLastErrorAsString() + ")");
								}

								{
									do
									{
										fSuccess = ReadFile(hPipe, chBuf, BUFSIZE * sizeof(TCHAR), &cbRead, NULL);
										if (!fSuccess && GetLastError() != ERROR_MORE_DATA)
											break;
										ReplyCommand(commandId, "mypipe:" + StringWStringToUTF8(chBuf));
									} while (!fSuccess);
								}
							}
							*/

							// Ring Log
							DWORD ringMagicExpected = 0xbadbabe;
							DWORD ringPositionMagic = 0;
							DWORD ringPositionNextIndex = 4;
							DWORD ringMaxLineLength = 512;
							DWORD ringHeaderBytes = 8;
							DWORD ringMaxLines = 2048;
							DWORD ringLineBytes = ringMaxLineLength + ringHeaderBytes;
							char ringBufLine[512];

							DWORD ringReaded = 0;
							HANDLE hRingFile = CreateFileA(ringPath.c_str(), GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
							if (hRingFile == INVALID_HANDLE_VALUE)
								ThrowException("Unable to open log ring (" + GetLastErrorAsString() + ")");

							// Read Magic
							DWORD ringMagic = 0;
							SetFilePointer(hRingFile, ringPositionMagic, NULL, FILE_BEGIN);
							if (!ReadFile(hRingFile, &ringMagic, 4, &ringReaded, NULL))
								ThrowException("Unable to read log ring (" + GetLastErrorAsString() + ")");
							if (ringMagic != ringMagicExpected)
								ThrowException("Magic not match, " + std::to_string(ringMagicExpected) + " vs " + std::to_string(ringMagic));

							ReplyCommand(commandId, "log:setup-complete");

							std::vector<std::string> logs;

							unsigned long handshakeStart = GetTimestampUnix();
							unsigned long handshakeLast = 0;

							DWORD ringCursor = 0;
							DWORD ringCurrent = 0;
							bool stop = false;
							for (;;)
							{
								if (stop)
									break;

								// Ring read
								if (true)
								{
									DWORD ringNextIndex = 0;
									SetFilePointer(hRingFile, ringPositionNextIndex, NULL, FILE_BEGIN);
									if (!ReadFile(hRingFile, &ringNextIndex, 4, &ringReaded, NULL))
										ThrowException("Unable to read log ring (" + GetLastErrorAsString() + ")");

									for (;;)
									{
										if (ringCurrent == ringNextIndex)
											break;

										if (ringCurrent > ringNextIndex) // Never occur unless unexpected situation
											break;

										DWORD ringCursor = ringCurrent % ringMaxLines;

										DWORD readOffset = (ringHeaderBytes + ringCursor * ringLineBytes);

										SetFilePointer(hRingFile, readOffset + 8, NULL, FILE_BEGIN);
										bool readResult = ReadFile(hRingFile, &ringBufLine[0], ringMaxLineLength, &ringReaded, NULL);

										if (ringBufLine[0] != 0)
										{
											std::string line = ringBufLine;

											logs.push_back(line);
										}

										ringCurrent++;
									}
								}

								for (std::vector<std::string>::const_iterator i = logs.begin(); i != logs.end(); ++i)
								{
									std::string line = *i;

									// Normalize
									line = StringReplaceAll(line, "[TUN]", "");
									line = StringReplaceAll(line, "[" + interfaceId + "]", "");
									line = StringTrim(line);

									bool skip = false;
									//if (StringStartsWith(line, "Routine:")) skip = true; // pre WireGuard-NT

									if (StringStartsWith(line, "Receiving handshake response from peer")) skip = true; // Useless, too much
									if (StringStartsWith(line, "Receiving keepalive packet from peer")) skip = true; // Useless, too much
									if (StringStartsWith(line, "Sending keepalive packet to peer")) skip = true; // Useless, too much
									if (StringStartsWith(line, "Sending handshake initiation to peer")) skip = true; // Useless, too much
									if (StringContain(line, "created for peer")) skip = true; // Useless, too much // 'Keypair 1 created for peer 1'
									if (StringContain(line, "destroyed for peer")) skip = true; // Useless, too much // 'Keypair 1 destroyed for peer 1'

									//if (StringContain(line, "Bringing peers up")) // pre WireGuard-NT						
									if (StringStartsWith(line, "Setting interface configuration"))
										ReplyCommand(commandId, "log:setup-interface");

									//if (StringContain(line, "Shutting down")) stop = true; // pre WireGuard-NT
									if (StringStartsWith(line, "Completed")) stop = true;

									// Note: the exact handshake can be obtained with pipe (the only info we need), but not implemented to avoid complexity
									// Note: there is also "Received first handshake" from WireGuard-NT, not used.
									//if (StringStartsWith(line, "Received handshake response")) // pre WireGuard-NT
									if (StringStartsWith(line, "Receiving handshake response")) // WireGuard-NT
									{
										unsigned long handshakeNow = GetTimestampUnix();

										if (handshakeLast != handshakeNow)
										{
											if (handshakeLast == 0)
												ReplyCommand(commandId, "log:handshake-first");

											//ReplyCommand(commandId, "log:last-handshake:" + StringFrom(handshakeNow));
											handshakeLast = handshakeNow;
										}
									}

									if (skip == false)
									{
										ReplyCommand(commandId, "log:" + line);
									}
								}
								logs.clear();

								unsigned long timeNow = GetTimestampUnix();
								if (handshakeLast > 0)
								{
									unsigned long handshakeDelta = timeNow - handshakeLast;

									if (handshakeDelta > handshakeTimeoutConnected)
									{
										// Too much, suggest disconnect
										ReplyCommand(commandId, "log:handshake-out");
									}
								}
								else
								{
									unsigned long handshakeDelta = timeNow - handshakeStart;

									if (handshakeDelta > handshakeTimeoutFirst)
									{
										// Too much, suggest disconnect
										ReplyCommand(commandId, "log:handshake-out");
									}
								}

								// Check stop requested
								if (m_keypair.find(keypairStopRequest) != m_keypair.end())
								{
									ReplyCommand(commandId, "log:stop-requested");
									break;
								}

								Sleep(100);
							}

							CloseHandle(hRingFile);
						}
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

				// Close Data Pipe
				if (hPipe != 0)
					CloseHandle(hPipe);

				// Stop and delete service
				ServiceGenericDelete(serviceId);


				// Stop and delete temp files
				FsFileDelete(configPath);
				FsFileDelete(ringPath);
				FsDirectoryDelete(tempPath, false);

				// Close service handlers
				if (service != 0)
					CloseServiceHandle(service);
				if (serviceControlManager != 0)
					CloseServiceHandle(serviceControlManager);

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
	if (CheckIfExecutableIsAllowed(servicePath, false, true))
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

	return success;
}

bool IWindows::ServiceUninstall()
{
	if (IsServiceInstalled())
	{
		IntegrityCheckClean("service");

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

	if (recursive == false)
	{
		return ::RemoveDirectoryW(StringUTF8ToWString(path).c_str());
	}
	else
	{
		// Not need yet
		return false;
	}
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

bool IWindows::FsFileIsRootOnly(std::string path)
{
	// Not used in Windows
	return true;
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

bool IWindows::CheckIfExecutableIsAllowed(const std::string& path, const bool& throwException, const bool ignoreKnown)
{
	std::string issues = "";

	if ( (ignoreKnown == false) && (IntegrityCheckFileKnown(path)) ) // If true, skip other checks.
		return true;

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

			bool hasOtherWritePermissions = false;
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

	if (issues != "")
	{
		if (throwException)
		{
			ThrowException("Executable '" + path + "' not allowed: " + issues);
			return false;
		}
		else
			return false;
	}
	else
		return true;
}

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

IWindows::t_shellinfo IWindows::ExecStart(const std::string& path, const std::vector<std::string>& args)
{
	t_shellinfo info;

	SECURITY_ATTRIBUTES saAttr;
	HANDLE stdoutWriteHandle = NULL;
	HANDLE stderrWriteHandle = NULL;

	std::wstring pathW = StringUTF8ToWString(path);

	std::wstring cmdline = L"\"" + pathW + L"\"";
	for (std::vector<std::string>::const_iterator i = args.begin(); i != args.end(); ++i)
		cmdline += L" " + StringUTF8ToWString(*i);

	const int lMaxCmdLine = 1024;
	TCHAR pcmdline[lMaxCmdLine + 1];

	wcscpy_s(pcmdline, lMaxCmdLine, cmdline.c_str());

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
	if (!CreateProcessW(pathW.c_str(), pcmdline, NULL, NULL, TRUE, CREATE_NO_WINDOW, NULL, 0, &info.startupInfo, &info.processInfo))
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

#ifdef WINTUNLIB
bool IWindows::WintunEnsureLibrary()
{
	if (m_wintunLibrary == 0)
	{
		// wintun.dll don't work (LoadLibrary crash) on older Win7 not updated.
		// Maybe because it's signed with SHA256 (not supported). KB3033929 may resolve, but fail to install on our lab win7 clean (and don't exists x86 version).
		// Maybe because perform dependencies delay load.
		// Anyway, with Win7 almost updated, fail to create adapter
		if (IsWindows8OrGreater())
		{
			std::string path = GetProcessPathCurrentDir() + FsPathSeparator + "wintun.dll";
			CheckIfExecutableIsAllowed(path, true);
			std::wstring pathW = StringUTF8ToWString(path);
			m_wintunLibrary = LoadLibraryExW(pathW.c_str(), NULL, LOAD_WITH_ALTERED_SEARCH_PATH);
		}
	}

	return (m_wintunLibrary != 0);
}

DWORD IWindows::WintunVersion()
{
	if (WintunEnsureLibrary() == false)
		return 0;

#ifdef WIP_WINTUN14
	return WintunLibraryGetRunningDriverVersion();
#else

	WINTUN_GET_RUNNING_DRIVER_VERSION_FUNC funcWintunGetRunningDriverVersion = (WINTUN_GET_RUNNING_DRIVER_VERSION_FUNC)GetProcAddress(m_wintunLibrary, "WintunGetRunningDriverVersion");
	if (funcWintunGetRunningDriverVersion == 0)
		ThrowException("wintun.dll WintunGetRunningDriverVersion not found: " + GetLastErrorAsString());
	else
		return funcWintunGetRunningDriverVersion();
#endif

	return 0;
}

std::string IWindows::WintunAdapterAdd(const std::wstring& name)
{
	if (WintunEnsureLibrary() == false)
		return "";
#ifdef WIP_WINTUN14
	WINTUN_ADAPTER_HANDLE hAdapter = WintunLibraryCreateAdapter(pool.c_str(), name.c_str(), NULL);

	if (hAdapter == 0)
	{
		ThrowException("wintun.dll WintunCreateAdapter fail, pool:'" + StringWStringToUTF8(pool) + "', name:'" + StringWStringToUTF8(name) + "', error : '" + GetLastErrorAsString() + "'");
	}
	else
		WintunAdapterClose(hAdapter);
#else
	WINTUN_CREATE_ADAPTER_FUNC funcWintunCreateAdapter = (WINTUN_CREATE_ADAPTER_FUNC)GetProcAddress(m_wintunLibrary, "WintunCreateAdapter");
	if (funcWintunCreateAdapter == 0)
	{
		ThrowException("wintun.dll WintunCreateAdapter not found");
		return "";
	}
	else
	{
		BOOL needReboot = false;

		std::wstring pool = L"Eddie";
		WINTUN_ADAPTER_HANDLE hAdapter = funcWintunCreateAdapter(pool.c_str(), name.c_str(), NULL, &needReboot);
		
		if (hAdapter == 0)
		{
			ThrowException("wintun.dll WintunCreateAdapter fail, pool:'" + StringWStringToUTF8(pool) +"', name:'" + StringWStringToUTF8(name) + "', nr:" + (needReboot ? "Y":"N") + ", error : '" + GetLastErrorAsString() + "'");
			return "";
		}
		else
		{
			std::string guid = WintunAdapterGetGuid(hAdapter);
			WintunAdapterClose(hAdapter);
			return guid;
		}
	}
#endif
}

std::string IWindows::WintunAdapterEnsure(const std::wstring& name)
{
	if (WintunEnsureLibrary() == false)
		return "";

	WINTUN_ADAPTER_HANDLE hAdapter = WintunAdapterOpen(name);
	if (hAdapter == 0)
	{
		return WintunAdapterAdd(name);
	}
	else
	{
		std::string guid = WintunAdapterGetGuid(hAdapter);
		WintunAdapterClose(hAdapter);
		return guid;
	}
}

void IWindows::WintunAdapterRemove(const std::wstring& name)
{
	if (WintunEnsureLibrary() == false)
		return;

	WINTUN_ADAPTER_HANDLE hAdapter = WintunAdapterOpen(name);
	if (hAdapter != 0)
	{
		WintunAdapterRemove(hAdapter);

		WintunAdapterClose(hAdapter);
	}
}

WINTUN_ADAPTER_HANDLE IWindows::WintunAdapterOpen(const std::wstring& name)
{
#ifdef WIP_WINTUN14
	WINTUN_ADAPTER_HANDLE hAdapter = WintunLibraryOpenAdapter(pool.c_str(), name.c_str());
	return hAdapter;
#else
	WINTUN_OPEN_ADAPTER_FUNC funcWintunOpenAdapter = (WINTUN_OPEN_ADAPTER_FUNC)GetProcAddress(m_wintunLibrary, "WintunOpenAdapter");
	if (funcWintunOpenAdapter == 0)
	{
		ThrowException("wintun.dll WintunOpenAdapter not found");
		return 0;
	}
	else
	{
		std::wstring pool = L"Eddie";
		WINTUN_ADAPTER_HANDLE hAdapter = funcWintunOpenAdapter(pool.c_str(), name.c_str());
		return hAdapter;
	}
#endif
}

void IWindows::WintunAdapterClose(WINTUN_ADAPTER_HANDLE hAdapter)
{
#ifdef WIP_WINTUN14
	WintunLibraryCloseAdapter(hAdapter);
#else
	WINTUN_FREE_ADAPTER_FUNC funcWintunFreeAdapter = (WINTUN_FREE_ADAPTER_FUNC)GetProcAddress(m_wintunLibrary, "WintunFreeAdapter");
	if (funcWintunFreeAdapter == 0)
		ThrowException("wintun.dll WintunFreeAdapter not found");
	else
		funcWintunFreeAdapter(hAdapter);
#endif
}

void IWindows::WintunAdapterRemove(WINTUN_ADAPTER_HANDLE hAdapter)
{
#ifdef WIP_WINTUN14
	#error WIP
#else
	WINTUN_DELETE_ADAPTER_FUNC funcWintunDeleteAdapter = (WINTUN_DELETE_ADAPTER_FUNC)GetProcAddress(m_wintunLibrary, "WintunDeleteAdapter");
	if (funcWintunDeleteAdapter == 0)
		ThrowException("wintun.dll WintunDeleteAdapter not found");
	else
	{
		BOOL needReboot = false;
		funcWintunDeleteAdapter(hAdapter, true, &needReboot);
	}
#endif
}

/*
BOOL WintunAdapterRemovePoolCallback(WINTUN_ADAPTER_HANDLE hAdapter, LPARAM param) // TOFIX: convert to member function
{
	IWindows* pImpl = (IWindows*)param;
	pImpl->WintunAdapterRemove(hAdapter);
	return TRUE;
}
*/

/*
void IWindows::WintunAdapterRemovePool()
{
	if (WintunEnsureLibrary() == false)
		return;

#if _WIN64
	WINTUN_ENUM_ADAPTERS_FUNC funcWintunEnumAdapters = (WINTUN_ENUM_ADAPTERS_FUNC)GetProcAddress(m_wintunLibrary, "WintunEnumAdapters");
	if (funcWintunEnumAdapters == 0)
	{
		ThrowException("wintun.dll WintunEnumAdapters not found");
	}
	else
	{
		std::wstring pool = L"Eddie";
		funcWintunEnumAdapters(pool.c_str(), &WintunAdapterRemovePoolCallback, (LPARAM)this);
	}
#else
	// TOFIX, the code above don't compile on x86
	WintunAdapterRemove(pool, StringUTF8ToWString("Eddie OpenVPN"));
#endif
}
*/
#endif // WINTUNLIB

std::string IWindows::WintunAdapterGetGuid(WINTUN_ADAPTER_HANDLE hAdapter)
{
#ifdef WIP_WINTUN14
#error WIP
#else
	WINTUN_GET_ADAPTER_LUID_FUNC funcWintunGetAdapterLUID = (WINTUN_GET_ADAPTER_LUID_FUNC)GetProcAddress(m_wintunLibrary, "WintunGetAdapterLUID");
	if (funcWintunGetAdapterLUID == 0)
	{
		ThrowException("wintun.dll WintunGetAdapterLUID not found");
		return "";
	}
	else
	{
		NET_LUID luid;
		funcWintunGetAdapterLUID(hAdapter, &luid);

		GUID guid;
		if (ConvertInterfaceLuidToGuid(&luid, &guid) == NO_ERROR)
		{
			wchar_t guidString[39];
			StringFromGUID2(guid, guidString, 39);

			return StringWStringToUTF8(guidString);
		}
		else
		{
			ThrowException("wintun.dll WintunGetAdapterLUID fail");
			return "";
		}
	}
#endif
}

std::string IWindows::NetworkAdapterCreate(const std::string& driver, const std::string& name)
{
	// We keep a list of network adapter created by Eddie	

	std::string guid = "";

	if (driver == "ovpn-dco")
	{
		std::string tapctl = FsLocateExecutable("tapctl.exe", false, true);
		if (tapctl != "")
		{
			ExecResult result = ExecEx3(tapctl, "create", "--hwid \"ovpn-dco\"", "--name \"" + name + "\"");
			if (result.exit == 0)
				guid = StringTrim(result.out);
		}
	}
	else if (driver == "wintun")
	{
#ifdef WINTUNLIB
		guid = WintunAdapterEnsure(StringUTF8ToWString(name));
#else
		// Note 2.24.3: tapctl don't create wintun if driver is not installed, so we absolutely need to use wintun.h (12.0 or 14.1, for now 12.0)
		std::string tapctl = FsLocateExecutable("tapctl.exe", false, true);
		if (tapctl != "")
		{
			ExecResult result = ExecEx3(tapctl, "create", "--hwid \"wintun\"", "--name \"" + name + "\"");
			if (result.exit == 0)
				guid = StringTrim(result.out);
		}
#endif
	}
	else if (driver == "tap-windows6")
	{
		std::string tapctl = FsLocateExecutable("tapctl.exe", false, true);
		if (tapctl != "")
		{
			ExecResult result = ExecEx3(tapctl, "create", "--hwid \"root\\tap0901\"", "--name \"" + name + "\"");
			if (result.exit == 0)
				guid = StringTrim(result.out);
		}
	}

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
	std::string tapctl = FsLocateExecutable("tapctl.exe", false, true);
	if (tapctl != "")
	{
		ExecResult result = ExecEx2(tapctl, "delete", id);
		if (result.exit == 0)
		{
			std::string eddieList = SystemWideDataGet("network_adapters", "");
			eddieList = StringReplaceAll(eddieList, id + ";", "");
			SystemWideDataSet("network_adapters", eddieList);
			return true;
		}
	}

	return false;
}

void IWindows::NetworkAdapterDeleteAll()
{
/*
#ifdef WINTUNLIB
	// Only wintun for now
	WintunAdapterRemovePool();
#else
*/
	std::vector<std::string> list = StringToVector(SystemWideDataGet("network_adapters", ""), ';', true);
	for (std::vector<std::string>::const_iterator i = list.begin(); i != list.end(); ++i)
	{
		std::string id = *i;
		NetworkAdapterDelete(id);
	}
//#endif
}
	
/* TOCLEAN
void EnumerateNetworkAdapters()
{
	ULONG flags = GAA_FLAG_INCLUDE_ALL_INTERFACES;
	ULONG family = AF_UNSPEC;
	PIP_ADAPTER_ADDRESSES pAddresses = nullptr;
	ULONG outBufLen = 0;

	ULONG ret = GetAdaptersAddresses(family, flags, nullptr, pAddresses, &outBufLen);
	if (ret == ERROR_BUFFER_OVERFLOW)
	{
		pAddresses = (IP_ADAPTER_ADDRESSES*)malloc(outBufLen);
		if (pAddresses == nullptr)
		{
			return;
		}
	}
	else if (ret != ERROR_BUFFER_OVERFLOW)
	{
		return;
	}

	ret = GetAdaptersAddresses(family, flags, nullptr, pAddresses, &outBufLen);
	if (ret == NO_ERROR)
	{
		PIP_ADAPTER_ADDRESSES pCurrAddresses = pAddresses;
		while (pCurrAddresses) {

			std::string id(pCurrAddresses->AdapterName);
			std::wstring name(pCurrAddresses->FriendlyName);
			std::wstring desc(pCurrAddresses->Description);
			
			pCurrAddresses = pCurrAddresses->Next;
		}
	}

	if (pAddresses)
	{
		free(pAddresses);
	}
}
*/

// Note 2021-05-06:
// It's not possible to call directly the wgtunnel.dll entrypoint, because throw
// "Service run error: An instance of the service is already running."
// seem WG code try to manage the service itself.
// Maybe in future we can submit a patch to WireGuard to establish a tunnel directly from C++ here,
// in the meantime we use a service ad-hoc.
int IWindows::WireGuardTunnel(const std::string& configName)
{
	// Write a file for any fatal error
	std::string error = "";

	std::string path = GetProcessPathCurrentDir() + FsPathSeparator + "wgtunnel.dll";
	CheckIfExecutableIsAllowed(path, true);
	std::wstring pathW = StringUTF8ToWString(path);
	HINSTANCE hInstanceTunnel = LoadLibraryExW(pathW.c_str(), NULL, LOAD_WITH_ALTERED_SEARCH_PATH);

	if (hInstanceTunnel != NULL)
	{
		typedef int(__cdecl* WGTUNNELPROC)(LPWSTR);
		WGTUNNELPROC procWgTunnel = (WGTUNNELPROC)GetProcAddress(hInstanceTunnel, "WireGuardTunnelService");
		if (procWgTunnel != NULL)
		{
			// The entrypoint "WireGuardTunnelService" of "tunnel.dll" from WireGuard dump directly errors in stderr (not a best practice..).
			// We need to override standard output handle to catch it.
			DWORD pipeMode = PIPE_READMODE_BYTE | PIPE_NOWAIT;
			DWORD pipeMaxCollectionCount = 0;
			DWORD pipeCollectDataTimeout = 0;
			HANDLE hOut = GetStdHandle(STD_OUTPUT_HANDLE);
			HANDLE hErr = GetStdHandle(STD_ERROR_HANDLE);

			HANDLE hPipeOutRead, hPipeOutWrite;
			CreatePipe(&hPipeOutRead, &hPipeOutWrite, 0, 0);
			SetNamedPipeHandleState(hPipeOutRead, &pipeMode, &pipeMaxCollectionCount, &pipeCollectDataTimeout);
			SetNamedPipeHandleState(hPipeOutWrite, &pipeMode, &pipeMaxCollectionCount, &pipeCollectDataTimeout);
			SetStdHandle(STD_OUTPUT_HANDLE, hPipeOutWrite);


			HANDLE hPipeErrRead, hPipeErrWrite;
			CreatePipe(&hPipeErrRead, &hPipeErrWrite, 0, 0);
			SetNamedPipeHandleState(hPipeErrRead, &pipeMode, &pipeMaxCollectionCount, &pipeCollectDataTimeout);
			SetNamedPipeHandleState(hPipeErrWrite, &pipeMode, &pipeMaxCollectionCount, &pipeCollectDataTimeout);
			SetStdHandle(STD_ERROR_HANDLE, hPipeErrWrite);
						
			std::wstring configNameW = StringUTF8ToWString(configName);			
			int result = procWgTunnel(&configNameW[0]);

			// Revert previous handlers
			SetStdHandle(STD_OUTPUT_HANDLE, hOut);
			SetStdHandle(STD_ERROR_HANDLE, hErr);

			// Get output
			std::string outputOut = "";
			std::string outputErr = "";
			const DWORD BUFFER_SIZE = 2000;
			char buffer[BUFFER_SIZE];
			DWORD nRead = 0;

			nRead = 0;
			if (ReadFile(hPipeOutRead, buffer, BUFFER_SIZE - 1, &nRead, 0))
			{
				buffer[nRead] = 0;
				outputOut = buffer;
			}
			CloseHandle(hPipeOutRead);
			CloseHandle(hPipeOutWrite);

			nRead = 0;
			if (ReadFile(hPipeErrRead, buffer, BUFFER_SIZE - 1, &nRead, 0))
			{
				buffer[nRead] = 0;
				outputErr = buffer;
			}
			CloseHandle(hPipeErrRead);
			CloseHandle(hPipeErrWrite);

			error += "\r\n" + outputOut;
			error += "\r\n" + outputErr;
		}
		else
		{
			error += "\r\nentrypoint 'WireGuardTunnelService' not found";
		}

		FreeLibrary(hInstanceTunnel);
	}
	else
	{
		error += "\r\nmodule 'wgtunnel.dll' not found";
	}

	error = StringTrim(error);
	if (error != "")
	{
		FsFileWriteText(configName + ".fatal", error);
		return 1;
	}
	else
	{
		return 0;
	}
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
