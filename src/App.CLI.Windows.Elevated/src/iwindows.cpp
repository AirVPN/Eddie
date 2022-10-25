// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2016 AirVPN (support@airvpn.org) / https://airvpn.org
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

#include "iwindows.h"

#include <fstream>
#include <sstream>

#include "psapi.h"
#include "tlhelp32.h"

#include <iphlpapi.h>

#include <codecvt> // For StringUTF8ToWString

#include "sddl.h"	// For ConvertSidToStringSid

#include "accctrl.h" // for GetSecurityInfo
#include "aclapi.h" // for GetSecurityInfo

void IWindows::Do(const std::string& commandId, const std::string& command, std::map<std::string, std::string>& params)
{
	if (command == "dns-flush")
	{
		std::string mode = params["mode"];

		std::string netPath = FsLocateExecutable("net.exe", false);
		std::string ipconfigPath = FsLocateExecutable("net.exe", false);

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
			ThrowException(GetExecResultDump(shellResult));
	}
	else if (command == "openvpn")
	{
		std::string id = params["id"];
		std::string action = params["action"];

		if (action == "stop")
		{
			std::string signal = params["signal"];

			if (m_keypair.find("openvpn_" + id + "_pid") != m_keypair.end())
			{
				pid_t pid = atoi(m_keypair["openvpn_" + id + "_pid"].c_str());

				if (GetParentProcessId(pid) != GetCurrentProcessId())
					ThrowException("Requested a kill to a non-child elevated process");

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

				m_keypair["openvpn_" + id + "_pid"] = std::to_string(info.pid);
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

				m_keypair.erase("openvpn_" + id + "_pid");

				int exitCode = ExecEnd(info);
				ReplyCommand(commandId, "return:" + std::to_string(exitCode));
			}
		}
	}
	else if (command == "hummingbird")
	{
		// At 2020/02/23, Hummingbird Windows is not public available.
	}
	else if (command == "wintun-version")
	{
		ReplyCommand(commandId, StringFrom(WintunVersion()));
	}
	else if (command == "wintun-adapter-ensure")
	{
		std::wstring pool = StringUTF8ToWString(params["pool"]);
		std::wstring name = StringUTF8ToWString(params["name"]);

		WintunAdapterEnsure(pool, name);

		ReplyCommand(commandId, std::to_string(WintunVersion()));
	}
	else if (command == "wintun-adapter-removepool")
	{
		std::wstring pool = StringUTF8ToWString(params["pool"]);
		WintunAdapterRemovePool(pool);
	}
	else if (command == "wireguard-version")
	{
		std::string version = "";
		if (IsWin8OrGreater()) // see WintunEnsureLibrary
			version = "0.5.2"; // Embedded, wgtunnel.dll
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
			//ServiceDelete(serviceId);
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
					ServiceDelete(serviceId); // Delete if already exists

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
						serviceDependsW = NULL; // Removed, cause issue. Anyway Elevated itself have this depends. // TOFIX?
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
				ServiceDelete(serviceId);


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

	std::string path = FsFileGetDirectory(elevatedPath) + FsPathSeparator + "Eddie-Service-Elevated.exe";

	std::string elevatedArgs = "mode=service";
	std::string integrity = ComputeIntegrityHash(GetProcessPathCurrent(), "");
	elevatedArgs += " integrity=" + StringEnsureIntegrity(integrity);

	if (m_cmdline.find("service_port") != m_cmdline.end())
		elevatedArgs += " service_port=" + std::to_string(port);

	// Can be active but old version that don't accept new client
	ServiceUninstallDirect();

	SC_HANDLE serviceControlManager = OpenSCManager(0, 0, SC_MANAGER_ALL_ACCESS); // GENERIC_WRITE is not enough
	if (serviceControlManager)
	{
		std::wstring serviceServiceNameW = StringUTF8ToWString(GetServiceId());
		std::wstring serviceDisplayNameW = StringUTF8ToWString(GetServiceName());
		std::wstring servicePathW = StringUTF8ToWString("\"" + path + "\"");
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
					// Write Args
					std::wstring serviceArgs = StringUTF8ToWString(elevatedArgs);
					LONG setResA = RegSetValueEx(hKey, TEXT("EddieArgs"), 0, REG_SZ, (LPBYTE)serviceArgs.c_str(), (DWORD)(serviceArgs.size() + 1) * sizeof(wchar_t));
					if (setResA != ERROR_SUCCESS)
						success = false;

					// Write Description
					std::wstring serviceDisplayDescW = StringUTF8ToWString(GetServiceDesc());
					LONG setResD = RegSetValueEx(hKey, TEXT("Description"), 0, REG_SZ, (LPBYTE)serviceDisplayDescW.c_str(), (DWORD)(serviceDisplayDescW.size() + 1) * sizeof(wchar_t));
					if (setResD != ERROR_SUCCESS)
						success = false;
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
				ServiceUninstallDirect();
		}
		CloseServiceHandle(serviceControlManager);
	}

	return success;
}

bool IWindows::ServiceUninstall()
{
	if (IsServiceInstalled())
	{
		std::string serviceId = GetServiceId();

		if (GetLaunchMode() == "service")
		{
			// This is performed 'async' without wait the result, because launched from service itself.
			// See comment in IBase::ServiceUninstallSupportRealtime
			std::string path = FsLocateExecutable("sc.exe");
			std::vector<std::string> args;
			args.push_back("delete \"" + serviceId + "\"");
			t_shellinfo info = ExecStart(path, args);

			return true;
		}
		else
		{
			return ServiceUninstallDirect();
		}
	}
	else
		return false;
}

bool IWindows::ServiceUninstallSupportRealtime()
{
	// See comment in base
	return true;
}

bool IWindows::FullUninstall()
{
	// Remove any adapter created from Eddie
	WintunAdapterRemovePool(StringUTF8ToWString("Eddie"));

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
	BOOL fIsElevated = FALSE;
	HANDLE hToken = NULL;
	TOKEN_ELEVATION elevation;
	DWORD dwSize;

	if (!OpenProcessToken(GetCurrentProcess(), TOKEN_QUERY, &hToken))
	{
		// Failed to get Process Token
		goto Cleanup; // if Failed, we treat as False
	}

	if (!GetTokenInformation(hToken, TokenElevation, &elevation, sizeof(elevation), &dwSize))
	{
		// Failed to get Token Information
		goto Cleanup; // if Failed, we treat as False
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

void IWindows::Sleep(int ms)
{
	::Sleep(ms);
}

uint64_t IWindows::GetTimestampUnixUsec()
{
	uint64_t EPOCH = ((uint64_t)116444736000000000ULL);
	SYSTEMTIME  system_time;
	FILETIME    file_time;
	uint64_t    time;
	GetSystemTime(&system_time);
	SystemTimeToFileTime(&system_time, &file_time);
	time = ((uint64_t)file_time.dwLowDateTime);
	time += ((uint64_t)file_time.dwHighDateTime) << 32;
	return (time - EPOCH) / 10 + uint64_t(system_time.wMilliseconds) * 1000;
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
	WCHAR filename[MAX_PATH];

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

int IWindows::Exec(const std::string& path, const std::vector<std::string>& args, const bool stdinWrite, const std::string& stdinBody, std::string& stdOut, std::string& stdErr, const bool log)
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
	if (INVALID_FILE_ATTRIBUTES == ::GetFileAttributes(pathW.c_str()) && ::GetLastError() == ERROR_FILE_NOT_FOUND)
		return false;
	else
		return true;
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

std::string IWindows::CheckIfClientPathIsAllowed(const std::string& path)
{
#ifdef Debug
	return "ok";
#else
	/*
	// MacOS have equivalent.
	// Linux don't have any signature check (and can't have, because in distro like Arch, binary are builded client-side from sources
	// This is probably a superfluous check, and can cause issue for who compile from sources.
	// If implemented, need a conversion from C# to C++ of the code below.

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
#endif
}

bool IWindows::CheckIfExecutableIsAllowed(const std::string& path, const bool& throwException)
{
	std::string issues = "";

	if (CheckIfExecutableIsWhitelisted(path)) // If true, skip other checks.
		return true;

	return true; // Commented, cause crash, and it's not used anyway right now

	// Windows Security Descriptor
	DWORD dwRtnCode = 0;
	PSID pSidOwner = NULL;
	BOOL bRtnBool = TRUE;
	LPTSTR AcctName = NULL;
	LPTSTR DomainName = NULL;
	DWORD dwAcctName = 1, dwDomainName = 1;
	SID_NAME_USE eUse = SidTypeUnknown;
	HANDLE hFile;
	PSECURITY_DESCRIPTOR pSD = NULL;

	hFile = CreateFile(StringUTF8ToWString(path).c_str(), GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);

	if (hFile == INVALID_HANDLE_VALUE)
	{
		DWORD dwErrorCode = 0;
		dwErrorCode = GetLastError();
		issues += "CheckIfExecutableIsAllowed - CreateFile error (" + StringFrom(dwErrorCode) + ");";
	}
	else
	{
		dwRtnCode = GetSecurityInfo(hFile, SE_FILE_OBJECT, OWNER_SECURITY_INFORMATION, &pSidOwner, NULL, NULL, NULL, &pSD);

		if (dwRtnCode != ERROR_SUCCESS)
		{
			DWORD dwErrorCode = 0;
			dwErrorCode = GetLastError();
			issues += "CheckIfExecutableIsAllowed - GetSecurityInfo error (" + StringFrom(dwErrorCode) + ");";
		}
		else
		{
			CloseHandle(hFile);

			bRtnBool = LookupAccountSid(NULL, pSidOwner, AcctName, (LPDWORD)&dwAcctName, DomainName, (LPDWORD)&dwDomainName, &eUse);

			AcctName = (LPTSTR)GlobalAlloc(GMEM_FIXED, dwAcctName);

			if (AcctName == NULL)
			{
				DWORD dwErrorCode = 0;
				dwErrorCode = GetLastError();
				issues += "CheckIfExecutableIsAllowed - GlobalAlloc error (" + StringFrom(dwErrorCode) + ");";
			}
			else
			{
				DomainName = (LPTSTR)GlobalAlloc(GMEM_FIXED, dwDomainName);

				if (DomainName == NULL)
				{
					DWORD dwErrorCode = 0;
					dwErrorCode = GetLastError();
					issues += "CheckIfExecutableIsAllowed - GlobalAlloc error (" + StringFrom(dwErrorCode) + ");";
				}
				else
				{
					bRtnBool = LookupAccountSid(NULL, pSidOwner, AcctName, (LPDWORD)&dwAcctName, DomainName, (LPDWORD)&dwDomainName, &eUse);

					if (bRtnBool == FALSE)
					{
						DWORD dwErrorCode = 0;
						dwErrorCode = GetLastError();
						if (dwErrorCode == ERROR_NONE_MAPPED)
							issues = "Account owner not found for specified SID;";
						else
							issues = "Error in LookupAccountSid;";
					}
					else
					{
						// TOFIX, issues for who use custom path...
						/*
						std::string sDomainName = StringWStringToUTF8(DomainName);
						if (StringWStringToUTF8(DomainName) != "NT SERVICE")
							issues = "not under NT SERVICE";
						*/
					}
				}
			}
		}
	}

	if (issues != "")
	{
		if (throwException)
			ThrowException("Executable '" + path + "' not allowed: " + issues);
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

bool IWindows::IsWin8OrGreater()
{
	NTSTATUS(WINAPI * RtlGetVersion)(LPOSVERSIONINFOEXW);
	OSVERSIONINFOEXW osInfo;

	//*(FARPROC*)&RtlGetVersion = GetProcAddress(GetModuleHandleA("ntdll"), "RtlGetVersion");
	*reinterpret_cast<FARPROC*>(&RtlGetVersion) = GetProcAddress(GetModuleHandleA("ntdll"), "RtlGetVersion");

	if (RtlGetVersion != NULL)
	{
		osInfo.dwOSVersionInfoSize = sizeof(osInfo);
		RtlGetVersion(&osInfo);
		return (osInfo.dwMajorVersion >= 6 && osInfo.dwMinorVersion >= 3) || osInfo.dwMajorVersion >= 10;
	}

	return false;
}

std::string IWindows::GetLastErrorAsString()
{
	DWORD errorMessageID = ::GetLastError();
	if (errorMessageID == 0)
		return std::string();

	/*
	LPSTR messageBuffer = nullptr;
	size_t size = FormatMessageA(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
		NULL, errorMessageID, MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), (LPSTR)&messageBuffer, 0, NULL);
	std::string message(messageBuffer, size);
	LocalFree(messageBuffer);
	*/
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

	//std::wstring cmdline = L""; // <2.21.4
	std::wstring cmdline = L"\"" + pathW + L"\""; // >2.21.4
	for (std::vector<std::string>::const_iterator i = args.begin(); i != args.end(); ++i)
		cmdline += L" " + StringUTF8ToWString(*i);

	const int lMaxCmdLine = 1024;
	TCHAR pcmdline[lMaxCmdLine + 1];

	//strcpy_s(pcmdline, sizeof(pcmdline), cmdline.c_str());
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

bool IWindows::ServiceDelete(const std::string& id)
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

bool IWindows::ServiceUninstallDirect()
{
	return ServiceDelete(GetServiceId());
}

bool IWindows::WintunEnsureLibrary()
{
	if (m_wintunLibrary == 0)
	{
		// wintun.dll don't work (LoadLibrary crash) on older Win7 not updated.
		// Maybe because it's signed with SHA256 (not supported). KB3033929 may resolve, but fail to install on our lab win7 clean (and don't exists x86 version).
		// Maybe because perform dependencies delay load.
		// Anyway, with Win7 almost updated, fail to create adapter
		if (IsWin8OrGreater())
		{
			std::string path = GetProcessPathCurrentDir() + FsPathSeparator + "wintun.dll";
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

	WINTUN_GET_RUNNING_DRIVER_VERSION_FUNC funcWintunGetRunningDriverVersion = (WINTUN_GET_RUNNING_DRIVER_VERSION_FUNC)GetProcAddress(m_wintunLibrary, "WintunGetRunningDriverVersion");
	if (funcWintunGetRunningDriverVersion == 0)
		ThrowException("wintun.dll WintunGetRunningDriverVersion not found: " + GetLastErrorAsString());
	else
		return funcWintunGetRunningDriverVersion();

	return 0;
}

void IWindows::WintunAdapterAdd(const std::wstring& pool, const std::wstring& name)
{
	if (WintunEnsureLibrary() == false)
		return;

	WINTUN_CREATE_ADAPTER_FUNC funcWintunCreateAdapter = (WINTUN_CREATE_ADAPTER_FUNC)GetProcAddress(m_wintunLibrary, "WintunCreateAdapter");
	if (funcWintunCreateAdapter == 0)
		ThrowException("wintun.dll WintunCreateAdapter not found");
	else
	{
		BOOL needReboot = false;

		WINTUN_ADAPTER_HANDLE hAdapter = funcWintunCreateAdapter(pool.c_str(), name.c_str(), NULL, &needReboot);
		
		if (hAdapter == 0)
		{
			ThrowException("wintun.dll WintunCreateAdapter fail, pool:'" + StringWStringToUTF8(pool) +"', name:'" + StringWStringToUTF8(name) + "', nr:" + (needReboot ? "Y":"N") + ", error : '" + GetLastErrorAsString() + "'");
		}
		else
			WintunAdapterClose(hAdapter);
	}
}

void IWindows::WintunAdapterEnsure(const std::wstring& pool, const std::wstring& name)
{
	if (WintunEnsureLibrary() == false)
		return;

	WINTUN_ADAPTER_HANDLE hAdapter = WintunAdapterOpen(pool, name);
	if (hAdapter == 0)
	{
		WintunAdapterAdd(pool, name);
	}
	else
	{
		WintunAdapterClose(hAdapter);
	}
}

void IWindows::WintunAdapterRemove(const std::wstring& pool, const std::wstring& name)
{
	if (WintunEnsureLibrary() == false)
		return;

	WINTUN_ADAPTER_HANDLE hAdapter = WintunAdapterOpen(pool, name);
	if (hAdapter != 0)
	{
		WintunAdapterRemove(hAdapter);

		WintunAdapterClose(hAdapter);
	}
}

WINTUN_ADAPTER_HANDLE IWindows::WintunAdapterOpen(const std::wstring& pool, const std::wstring& name)
{
	WINTUN_OPEN_ADAPTER_FUNC funcWintunOpenAdapter = (WINTUN_OPEN_ADAPTER_FUNC)GetProcAddress(m_wintunLibrary, "WintunOpenAdapter");
	if (funcWintunOpenAdapter == 0)
	{
		ThrowException("wintun.dll WintunOpenAdapter not found");
		return 0;
	}
	else
	{
		WINTUN_ADAPTER_HANDLE hAdapter = funcWintunOpenAdapter(pool.c_str(), name.c_str());
		return hAdapter;
	}
}

void IWindows::WintunAdapterClose(WINTUN_ADAPTER_HANDLE hAdapter)
{
	WINTUN_FREE_ADAPTER_FUNC funcWintunFreeAdapter = (WINTUN_FREE_ADAPTER_FUNC)GetProcAddress(m_wintunLibrary, "WintunFreeAdapter");
	if (funcWintunFreeAdapter == 0)
		ThrowException("wintun.dll WintunFreeAdapter not found");
	else
		funcWintunFreeAdapter(hAdapter);
}

void IWindows::WintunAdapterRemove(WINTUN_ADAPTER_HANDLE hAdapter)
{
	WINTUN_DELETE_ADAPTER_FUNC funcWintunDeleteAdapter = (WINTUN_DELETE_ADAPTER_FUNC)GetProcAddress(m_wintunLibrary, "WintunDeleteAdapter");
	if (funcWintunDeleteAdapter == 0)
		ThrowException("wintun.dll WintunDeleteAdapter not found");
	else
	{
		BOOL needReboot = false;
		funcWintunDeleteAdapter(hAdapter, true, &needReboot);
	}
}

BOOL WintunAdapterRemovePoolCallback(WINTUN_ADAPTER_HANDLE hAdapter, LPARAM param) // TOFIX: convert to member function
{
	IWindows* pImpl = (IWindows*)param;
	pImpl->WintunAdapterRemove(hAdapter);
	return TRUE;
}

void IWindows::WintunAdapterRemovePool(const std::wstring& pool)
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
		funcWintunEnumAdapters(pool.c_str(), &WintunAdapterRemovePoolCallback, (LPARAM)this);
	}
#else
	// TOFIX, the code above don't compile on x86
	WintunAdapterRemove(pool, StringUTF8ToWString("Eddie OpenVPN"));
#endif
}

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

	HINSTANCE hInstanceTunnel = LoadLibrary(TEXT("wgtunnel.dll"));
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
	std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> conv;
	return conv.from_bytes(str);
}

std::string IWindows::StringWStringToUTF8(const std::wstring& str)
{
	std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> conv;
	return conv.to_bytes(str);
}

