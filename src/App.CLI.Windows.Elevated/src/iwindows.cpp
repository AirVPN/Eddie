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

#include "Windows.h"

#include <fstream>
#include <sstream>

#include "iwindows.h"
#include "psapi.h"
#include "tlhelp32.h"

#include <iphlpapi.h>

#include <codecvt> // For StringUTF8ToWString

#include "sddl.h"	// For ConvertSidToStringSid

#include "accctrl.h" // for GetSecurityInfo
#include "aclapi.h" // for GetSecurityInfo

void IWindows::Do(const std::string& commandId, const std::string& command, std::map<std::string, std::string>& params)
{
	if (command == "openvpn")
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

				for (;;) {
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
	else if (command == "wireguard-version")
	{
		std::string version = "0.3.11"; // Embedded, wireguard.dll		
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

				try
				{
					ServiceDelete(serviceId); // Delete if already exists

					if (FsDirectoryCreate(tempPath) == false)
						ThrowException("Unable to create config directory (" + GetLastErrorAsString() + ")");

					std::string configPath = tempPath + FsPathSeparator + interfaceId + ".conf";

					if (FsFileWriteText(configPath, config) == false)
						ThrowException("Unable to clean previous ring file (" + GetLastErrorAsString() + ")");

					std::string ringPath = tempPath + FsPathSeparator + "log.bin";

					if (FsFileExists(ringPath))
						if (FsFileDelete(ringPath) == false)
							ThrowException("Unable to clean previous ring file (" + GetLastErrorAsString() + ")");

					// Start an ad-hoc service (cannot call wireguard.dll directly here), required by current Windows WireGuard code.
					serviceControlManager = OpenSCManager(0, 0, SC_MANAGER_ALL_ACCESS); // GENERIC_WRITE is not enough
					if (!serviceControlManager)
						ThrowException("Services management failed (" + GetLastErrorAsString() + ")");
					else
					{
						std::string elevatedPath = GetProcessPathCurrent();
						std::string path = FsFileGetDirectory(GetProcessPathCurrent()) + FsPathSeparator + "Eddie-CLI-Elevated.exe";
						std::string pathWithArgs = path + " mode=wireguard config=\"" + configPath + "\"";

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

							unsigned long handshakeStart = GetTimestampUnix();
							unsigned long handshakeLast = 0;

							DWORD ringCursor = 0;
							bool ringStop = false;
							for (;;)
							{
								if (ringStop)
								{
									break;
								}

								bool ringReadAll = (ringCursor == 0);
								DWORD ringNextIndex = 0;
								SetFilePointer(hRingFile, ringPositionNextIndex, NULL, FILE_BEGIN);
								if (!ReadFile(hRingFile, &ringNextIndex, 4, &ringReaded, NULL))
									ThrowException("Unable to read log ring (" + GetLastErrorAsString() + ")");

								if (ringReadAll)
									ringCursor = ringNextIndex;

								for (DWORD l = 0; l < ringMaxLines; l++)
								{
									if (ringReadAll == false)
									{
										if (ringCursor == ringNextIndex)
											break;
									}

									DWORD readOffset = (ringHeaderBytes + ringCursor * ringLineBytes);

									SetFilePointer(hRingFile, readOffset + 8, NULL, FILE_BEGIN);
									bool readResult = ReadFile(hRingFile, &ringBufLine[0], ringMaxLineLength, &ringReaded, NULL);

									if (ringBufLine[0] != 0)
									{
										std::string line = ringBufLine;

										// Normalize
										line = StringReplaceAll(line, "[TUN]", "");
										line = StringReplaceAll(line, "[" + interfaceId + "]", "");
										line = StringTrim(line);

										bool skip = false;
										if (StringStartsWith(line, "Routine:"))
											skip = true;

										// Note: the exact handshake can be obtained with pipe (the only info we need), but not implemented to avoid complexity
										if (StringContain(line, "Received handshake response"))
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

											if (StringContain(line, "Shutting down"))
												ringStop = true;
										}
									}

									ringCursor++;
									ringCursor = ringCursor % ringMaxLines;
								}

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
				if (FsDirectoryExists(tempPath))
					FsDirectoryDelete(tempPath, true);

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
	return (time - EPOCH) / 10 + system_time.wMilliseconds * 1000;
}

int IWindows::Ping(const std::string& host, const int timeout)
{
	return -1; // Not yet implemented
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
	std::wstring pathW(StringUTF8ToWString(path));
	if (INVALID_FILE_ATTRIBUTES == ::GetFileAttributes(pathW.c_str()) && ::GetLastError() == ERROR_FILE_NOT_FOUND)
		return false;
	else
		return true;
}

bool IWindows::FsDirectoryExists(const std::string& path)
{
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

	return message;
}

void IWindows::ExecCleanup(IWindows::t_shellinfo info)
{
	CloseHandle(info.stdoutReadHandle);
	CloseHandle(info.stderrReadHandle);
	CloseHandle(info.processInfo.hProcess);
	CloseHandle(info.processInfo.hThread);
}

IWindows::t_shellinfo IWindows::ExecStart(const std::string& path, const std::vector<std::string>& args)
{
	t_shellinfo info;

	SECURITY_ATTRIBUTES saAttr;
	HANDLE stdoutWriteHandle = NULL;
	HANDLE stderrWriteHandle = NULL;

	std::wstring pathW = StringUTF8ToWString(path);

	std::wstring cmdline = L"";
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

// Note 2021-05-06:
// It's not possible to call directly the wireguard.dll entrypoint, because throw
// "Service run error: An instance of the service is already running."
// seem WG code try to manage the service itself.
// Maybe in future we can submit a patch to WireGuard to establish a tunnel directly from C++ here,
// in the meantime we use a service ad-hoc.
int IWindows::WireGuardTunnel(const std::string& configName)
{
	// Write a file for any fatal error
	std::string error = "";

	HINSTANCE hInstanceTunnel = LoadLibrary(TEXT("wireguard.dll"));
	if (hInstanceTunnel != NULL)
	{
		typedef int(__cdecl* WGTUNNELPROC)(LPWSTR);
		WGTUNNELPROC procWgTunnel = (WGTUNNELPROC)GetProcAddress(hInstanceTunnel, "WireGuardTunnelService");
		if (procWgTunnel != NULL)
		{
			// The entrypoint "WireGuardTunnelService" of "wireguard.dll" from WireGuard dump directly errors in stderr (not a best practice..).
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
		error += "\r\nmodule 'wireguard.dll' not found";
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

