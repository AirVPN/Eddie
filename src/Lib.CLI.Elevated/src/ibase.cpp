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

#include <sys/stat.h>
#include <sys/types.h>

#include <algorithm>
#include <cstring>
#include <fcntl.h>
#include <fstream>
#include <iomanip>
#include <signal.h>
#include <sstream>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#if defined(WIN32) || defined(_WIN32) || defined(__WIN32__) || defined(__NT__)	
#pragma warning(disable:4996)
#else
#include <unistd.h> // read, write, close (all socket related)
#include <netdb.h> // htons, listen, sockaddr_in

#endif

#include "../include/ibase.h"

#include "../../../dependencies/base64/base64.h"
#include "../../../dependencies/sha256/sha256.h"

#define NETBUFSIZE 256*256*10 // Note: Maximum size of command

// --------------------------
// Engine
// --------------------------

int IBase::AppMain(int argc, char* argv[])
{
	std::vector<std::string> args;
	for (int c = 0; c < argc; c++)
		args.push_back(argv[c]);
	return AppMain(args);
}


int IBase::AppMain(const std::vector<std::string>& args)
{
	m_cmdline = ParseCommandLine(args);

	try
	{
		return Main();
	}
	catch (std::exception& ex)
	{
		LogLocal("Main exception: " + std::string(ex.what()));
		return 1;
	}
	catch (...)
	{
		LogLocal("Main exception: Unknown");
		return 1;
	}
}

void IBase::MainDo(const std::string& commandId, const std::string& command, std::map<std::string, std::string>& params)
{
	try
	{
		if (m_session_key == "")
		{
			if (command == "session-key")
			{
				std::string clientVersion = "";
				if (params.find("version") != params.end())
					clientVersion = params["version"];

				if (clientVersion != m_elevatedVersion)
					ThrowException("Unexpected version, elevated: " + m_elevatedVersion + ", client: " + clientVersion);

				m_session_key = params["key"];
			}
			else
			{
				ReplyException(commandId, "Not init.");
			}
		}
		else if ((params.find("_token") == params.end()) || (m_session_key != params["_token"]))
		{
			ReplyException(commandId, "Not auth.");
		}
		else
		{
			m_debug = ((params.find("_debug") != params.end()) && (params["_debug"] == "1"));

			Do(commandId, command, params);
		}
	}
	catch (std::exception& ex)
	{
		ReplyException(commandId, "Exception: " + std::string(ex.what()));
	}
	catch (...)
	{
		ReplyException(commandId, "Internal exception.");
	}

	EndCommand(commandId);
}



// --------------------------
// Engine, Protected
// --------------------------

std::string IBase::GetLaunchMode()
{
	return m_launchMode;
}

void IBase::LogFatal(const std::string& msg)
{
	LogDebug("Fatal: " + msg);
	SendMessage("ee:fatal:" + base64_encode(msg));
}

void IBase::LogRemote(const std::string& msg)
{
	LogDevDebug("Remote:" + msg);

	SendMessage("ee:log:" + base64_encode(msg));
}

void IBase::LogLocal(const std::string& msg)
{
	LogDevDebug("Local:" + msg);

	// Removed in 2.19.7, otherwise dumped in CLI edition with sudoers user
	// Re-Added in 2.24.0
	std::cout << msg << std::endl;
}

void IBase::LogDebug(const std::string& msg)
{
	LogDevDebug("Debug:" + msg);

	/*
#if defined(Debug) || defined(_DEBUG)
	m_mutex_cout.lock();
	std::cout << "Elevated Debug: " << msg << std::endl;
	m_mutex_cout.unlock();
#endif
	*/

	if (m_debug)
	{
		try
		{
			LogRemote("Elevated: " + msg);
		}
		catch (const std::exception&)
		{
			// Ignore
		}
	}
}

void IBase::LogDevDebug(const std::string& msg)
{
	std::string logPath = "/tmp/eddie-elevated.log";
	#if defined(WIN32) || defined(_WIN32) || defined(__WIN32__) || defined(__NT__)	
		logPath = "C:\\eddie-elevated.log";
	#endif
	if (FsFileExists(logPath))
	{
		FILE* f = fopen(logPath.c_str(), "a");
		fprintf(f, "%lu - Elevated,PID: %d - %s%s", (unsigned long)GetTimestampUnix(), (int)GetCurrentProcessId(), msg.c_str(), FsEndLine.c_str());
		fclose(f);
	}
}

void IBase::ReplyPID(int pid)
{
	SendMessage("ee:pid:" + base64_encode(std::to_string(pid)));
}

void IBase::ReplyCommand(const std::string& commandId, const std::string& data)
{
	SendMessage("ee:data:" + commandId + ":" + base64_encode(data));
}

// --------------------------
// Engine, Private
// --------------------------

void IBase::ReplyException(const std::string& commandId, const std::string& message)
{
	SendMessage("ee:exception:" + commandId + ":" + base64_encode(message));
}

void IBase::EndCommand(const std::string& commandId)
{
	SendMessage("ee:end:" + commandId);
}

void IBase::SendMessage(const std::string& message)
{
	if (m_sockClient == 0)
		return;

	std::string sendBuffer = message + "\n";
	m_mutex_inout.lock();
	int nWrite = send(m_sockClient, sendBuffer.c_str(), (int)sendBuffer.length(), 0);
	m_mutex_inout.unlock();

	// LogDebug("Write socket " + std::to_string(nWrite) + " bytes.");

	if (nWrite < 0)
	{
		ThrowException("Error writing to socket");
	}
}

// --------------------------
// Virtual
// --------------------------

int IBase::Main()
{
	// Checkings
	if (IsRoot() == false)
	{
		LogLocal("This application can't be run directly, it's used internally by Eddie. (not root)");
		return 1;
	}

	// Service install/uninstall request
	if (m_cmdline.find("service") != m_cmdline.end())
	{
		if (m_cmdline["service"] == "install")
		{
			if (ServiceInstall())
				return 0;
			else
			{
				LogLocal("Service install fail.");
				return 1;
			}
		}
		else if (m_cmdline["service"] == "uninstall-full") // Full uninstall request
		{
			if (FullUninstall())
				return 0;
			else
			{
				LogLocal("Service uninstall fail.");
				return 1;
			}
		}
		else if (m_cmdline["service"] == "uninstall") // Service uninstall request
		{
			if (ServiceUninstall())
				return 0;
			else
			{
				LogLocal("Service uninstall fail.");
				return 1;
			}
		}
		else
		{
			LogLocal("Unknown service request");
			return 1;
		}
	}

	m_lastModified = GetProcessModTimeCurrent();

	int port = m_elevatedPortDefault;

	if ((m_cmdline.find("mode") != m_cmdline.end()) && (m_cmdline["mode"] == "spot"))
	{
		m_launchMode = "spot";

		if (m_cmdline.find("spot_port") != m_cmdline.end())
			port = StringToInt(m_cmdline["spot_port"]);

		m_singleConnMode = true;

		// If launched in SPOT mode, if service was active, they not accept for some reason (generally upgrade and fail integrity check), so reinstall.
		ServiceReinstall();

		// Update Security Info
		IntegrityCheckUpdate("spot");
	}
	else if ((m_cmdline.find("mode") != m_cmdline.end()) && (m_cmdline["mode"] == "service"))
	{
		m_launchMode = "service";

		if (m_cmdline.find("service_port") != m_cmdline.end())
			port = StringToInt(m_cmdline["service_port"]);

		m_singleConnMode = false;
	}
	else
	{
		LogLocal("This application can't be run directly, it's used internally by Eddie. (unknown mode)");
		return 1;
	}

	HSOCKET sockServer;
	struct sockaddr_in addrServer;

	sockServer = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);

	SocketMarkReuseAddr(sockServer);

	if (SocketIsValid(sockServer) == false)
	{
		ThrowException("Error on opening socket");
	}

	std::memset(&addrServer, 0, sizeof(addrServer));

	addrServer.sin_family = AF_INET;
	addrServer.sin_addr.s_addr = htonl(INADDR_LOOPBACK);
	addrServer.sin_port = htons(port);

	LogDevDebug("Listening on port " + std::to_string(port));

	if (bind(sockServer, (struct sockaddr*)&addrServer, sizeof(addrServer)) != 0) {
		ThrowException("Error on binding socket (" + std::to_string(SocketGetLastErrorCode()) + ")");
	}

	if (listen(sockServer, 1) != 0) {
		ThrowException("Error on listen socket (" + std::to_string(SocketGetLastErrorCode()) + ")");
	}

	SocketBlockMode(sockServer, false);

	for (;;)
	{
		if (IsStopRequested())
			break;

		m_keypair.clear();
		m_session_key = "";
		m_sockClient = 0;
		struct sockaddr_in addrClient;
		socklen_t addrClientLen = sizeof(addrClient);
		char* buffer = (char*)malloc(NETBUFSIZE);
		if (buffer == NULL)
			break;

		LogDebug("Waiting for client");

		try
		{
			for (;;)
			{
				m_sockClient = accept(sockServer, (struct sockaddr*)&addrClient, &addrClientLen);

				// TOFIX. Under Linux, errno==EWOULDBLOCK. Under Windows, i expect WSAEWOULDBLOCK but there are something not understanding.
				if (SocketIsValid(m_sockClient) == false)
				{
					if (IsStopRequested())
						break;

					Idle();

					Sleep(1000);
				}
				else
				{
					break;
				}
			}


			if (SocketIsValid(m_sockClient))
			{
				// Remove if nonblock is inherit
				SocketBlockMode(m_sockClient, true);

				// Check allowed
				if(true)
				{					
					int clientProcessId = GetProcessIdMatchingIPEndPoints(addrClient, addrServer);

					if (clientProcessId == 0)
						ThrowException("Client not allowed: Cannot detect client process");

					std::string clientProcessPath = GetProcessPathOfId(clientProcessId);
					if (clientProcessPath == "")
						ThrowException("Client not allowed: Cannot detect client process path");

					// If spot mode, must be a parent
					if (GetLaunchMode() == "spot")
					{
#if defined(Debug) || defined(_DEBUG)
						// When UI launch both Eddie-CLI and eddie-cli-elevated (currently only App.UI.MacOS)
						// Under _DEBUG until tested
						if(GetParentProcessId() == GetParentProcessId(clientProcessId))
							break;
#endif

						int parentPid = GetParentProcessId();
						for(;;)
						{
							if(parentPid == 0)
								ThrowException("Client not allowed: Connection not from parent process (spot mode)");
							
							if (clientProcessId == parentPid)
								break;
							else
								parentPid = GetParentProcessId(parentPid);
						}
					}
					
#if defined(Debug) || defined(_DEBUG)
					// For example, launched with VSCode detect dotnet.exe as client
#else
					std::string allowed = CheckIfClientPathIsAllowed(clientProcessPath);
					if (allowed != "ok")
						ThrowException("Client not allowed: " + allowed);
#endif
				}

				ReplyPID(GetCurrentProcessId());

				//char buffer[NETBUFSIZE];
				std::memset(buffer, 0, NETBUFSIZE);
				uint bufferPos = 0;
				bool clientStop = false;

				for (;;)
				{
					if (clientStop)
						break;

					if (IsStopRequested())
						break;

					int nMaxRead = NETBUFSIZE - bufferPos;
					if (nMaxRead <= 0) {
						ThrowException("Unexpected, command too big");
					}

					int n = recv(m_sockClient, buffer + bufferPos, nMaxRead, 0);
					if (n < 0) {
						ThrowException("Error reading from socket");
					}

					// LogDebug("Read socket " + std::to_string(n) + " bytes.");

					if (n == 0)
					{
						break;
					}

					bufferPos += n;

					for (;;)
					{
						uint bufferPosEndLine = 0;
						bool bufferPosEndLineFound = false;
						for (uint c = 0; c < bufferPos; c++)
						{
							if (buffer[c] == '\n')
							{
								bufferPosEndLine = c;
								bufferPosEndLineFound = true;
								break;
							}
						}

						if (bufferPosEndLineFound == false)
						{
							break;
						}
						else
						{
							std::string line(buffer, bufferPosEndLine);
							memcpy(&buffer[0], &buffer[0] + bufferPosEndLine + 1, bufferPos - (bufferPosEndLine + 1));
							bufferPos -= (bufferPosEndLine + 1);
							buffer[bufferPos] = 0;

							// Process command
							std::map<std::string, std::string> params;

							std::string::size_type pos = 0;

							for (;;)
							{
								std::string::size_type keyEnd = line.find(':', pos);
								if (keyEnd == std::string::npos)
									break;
								std::string::size_type valueEnd = line.find(';', keyEnd + 1);
								if (valueEnd == std::string::npos)
									break;

								std::string key = line.substr(pos, keyEnd - pos);
								std::string value = line.substr(keyEnd + 1, valueEnd - keyEnd - 1);
								value = base64_decode(value);

								params[key] = value;

								pos = valueEnd + 1;
							}

							std::string id = "";
							if (params.find("_id") != params.end())
								id = params["_id"];
							std::string command = "";
							if (params.find("command") != params.end())
								command = params["command"];

							if (command == "exit")
							{
								clientStop = true;
							}
							else if (command != "")
							{
								bool logDebug = true;
								if (command == "ping") logDebug = false; // Too much
								if (command == "dns-switch-rename-do") logDebug = false; // Too much
								if (logDebug)
									LogDebug("Command:" + command);

								std::thread t = std::thread(ThreadCommand, this, id, command, params);
								t.detach();
							}
						}
					}
				}
			}
		}
		catch (const std::exception& ex)
		{
			LogFatal(std::string(ex.what()));
		}
		catch (...)
		{
			LogFatal(std::string("Unknown exception"));
		}

		// Try to close pending VPN, new in 2.23
		// For example if UI is terminated.		
		{			
			for (std::map<std::string, std::string>::iterator it = m_keypair.begin(); it != m_keypair.end(); ++it)
			{
				std::string k = it->first;
				std::string v = it->second;

				if (StringStartsWith(k, "openvpn_pid_"))
				{
					pid_t pid = atoi(v.c_str());
					KillProcess("sigint", pid);
				}

				if (StringStartsWith(k, "hummingbird_pid_"))
				{
					pid_t pid = atoi(v.c_str());
					KillProcess("sigint", pid);
				}				

				if (StringStartsWith(k, "wireguard_stop_"))
				{
					m_keypair[k] = "stop"; // WIP
				}
			}
		}

		LogDebug("Client soft disconnected");

		SocketClose(m_sockClient);

		free(buffer);

		if (m_singleConnMode)
			break;
	}

	LogDebug("Closing");

	SocketClose(sockServer);

	if (m_singleConnMode)
	{
		if (GetLaunchMode() == "service")
		{
			// Windows only, because only on Windows, Elevated is always a service also in "spot" mode.
			ServiceUninstall();
		}

		IntegrityCheckClean(GetLaunchMode());
	}

	return 0;
}

void IBase::Idle()
{
}

void IBase::Do(const std::string& commandId, const std::string& command, std::map<std::string, std::string>& params)
{
	if (command == "exit")
	{
	}
	else if (command == "service-conn-mode")
	{
		if (params.find("mode") != params.end())
			m_singleConnMode = (params["mode"] == "single");
	}
	else if (command == "ping-request")
	{
		uint16_t id = StringToInt(params["id"]);
		std::string ip = params["ip"];
		int timeoutMs = StringToInt(params["timeout"]);

		m_pinger.Request(id, ip, timeoutMs, "");
	}
	else if (command == "ping-engine")
	{
		m_pinger.m_pBase = this;
		m_pinger.m_commandId = commandId;
		m_pinger.Start();

		for (;;)
		{
			int nPending = m_pinger.Check();

			int sleepMs = 1000; // one second
			if (nPending > 0)
				sleepMs = 1;
			Sleep(sleepMs);

			if (IsStopRequested())
				break;
		}

		m_pinger.Stop();
	}
	else if (command == "bin-path-add")
	{
		// Add a path of a list of paths used by FsLocateExecutable
		m_binPaths.push_back(params["path"]);
	}
	else if (command == "tor-get-info")
	{
		// Eddie need to talk with Tor Control to ask for new circuits or obtain guard IPs.
		std::string processName;
		std::string processPath;
		std::string username;
		std::string cookieCustomPath;
		std::string cookieFoundPath;
		std::string cookiePasswordHex;

		if (params.find("name") != params.end())
			processName = params["name"];

		if (params.find("path") != params.end())
			processPath = params["path"];

		if (params.find("cookie_path") != params.end())
			cookieCustomPath = params["cookie_path"];

		if (params.find("username") != params.end())
			username = params["username"];

		if (FsFileExists(processPath) == false)
			processPath = "";

		if (processPath == "")
		{
			pid_t id = GetProcessIdOfName(processName);
			if (id != 0)
				processPath = GetProcessPathOfId(id);
		}

		if (processPath == "") // Alternative name, TorBrowser under macOS
		{
			pid_t id = GetProcessIdOfName(processName + ".real");
			if (id != 0)
				processPath = GetProcessPathOfId(id);
		}

		std::vector<std::string> paths;

		if (cookieCustomPath != "")
			paths.push_back(cookieCustomPath);
		else
			AddTorCookiePaths(processPath, username, paths);

		for (std::vector<std::string>::const_iterator i = paths.begin(); i != paths.end(); ++i)
		{
			std::string path = *i;
			if (FsFileExists(path))
			{
				std::vector<char> chars = FsFileReadBytes(path);
				if (chars.size() > 0)
				{
					cookieFoundPath = path;
					cookiePasswordHex = StringHexEncode(chars);
					break;
				}
			}
		}

		ReplyCommand(commandId, "Name:" + processName);
		ReplyCommand(commandId, "Path:" + processPath);
		ReplyCommand(commandId, "CookiePath:" + cookieFoundPath);
		ReplyCommand(commandId, "CookiePasswordHex:" + cookiePasswordHex);
	}
	else
	{
		ReplyException(commandId, "Unknown elevated command: " + command);
	}
}

bool IBase::IsStopRequested()
{
	return false;
}

std::string IBase::GetServiceId()
{
	return "EddieElevationService";
}

std::string IBase::GetServiceName()
{
	return "Eddie Elevation Service";
}

std::string IBase::GetServiceDesc()
{
	return "Eddie Elevation Service";
}

bool IBase::IsServiceInstalled()
{
	return false;
}

bool IBase::ServiceInstall()
{
	return false;
}

bool IBase::ServiceUninstall()
{
	return false;
}

bool IBase::ServiceReinstall()
{
	if (IsServiceInstalled())
	{
		return ServiceInstall();
	}
	else
	{
		return false;
	}
}

bool IBase::FullUninstall()
{
	bool result = true;
	if (!ServiceUninstall()) result = false;
	if (!SystemWideDataClean()) result = false;
	return result;
}

bool IBase::IntegrityCheckUpdate(const std::string mode)
{
	// When Elevated launched as spot (manual accept of elevation) or when installed as service,
	// compute a list of hashes of important/executable/libraries of the App/Elevated directory.
	// This ensure:
	// - In service mode, Elevation App is not executed by Elevation Launcher if not match the binaries present when service was installed (avoid also DLL-Injection), for portable editions.
	// - Any executable or library launched or loaded by Elevation must be root-only access. If executable is in app directory, the entire hashes list must match (avoid DLL-Injection and check integrity computed when root elevation was asked).
	//   This check is performed to ensure for example the service registration of a portable edition, launch, and AFTER a .dll is placed for DLL-Injection (also for example about a third-party executable like tapctl.exe).

	return SystemWideDataSet("integrity_" + mode, IntegrityCheckBuild());
}

void IBase::IntegrityCheckClean(const std::string mode)
{
	SystemWideDataDel("integrity_" + mode);	
}

std::string IBase::IntegrityCheckRead(const std::string mode)
{
	return SystemWideDataGet("integrity_" + mode, "");
}

std::string IBase::IntegrityCheckBuild()
{
	std::string elevatedPath = GetProcessPathCurrent();
	std::string output;
	std::string checkPath = FsFileGetDirectory(elevatedPath);
	std::vector<std::string> files = FsFilesInPath(checkPath);
	std::sort(files.begin(), files.end());
	for (std::vector<std::string>::const_iterator i = files.begin(); i != files.end(); ++i)
	{
		std::string file = *i;
		std::string filePathFull = checkPath + FsPathSeparator + file;

		std::string ext = "";
		std::string::size_type extPos = file.rfind('.');

		if (extPos != std::string::npos)
			ext = StringToLower(file.substr(extPos + 1));

		bool include = ((FsFileIsExecutable(filePathFull)) || (StringEndsWith(file, ".dll")) || (StringEndsWith(file, ".dylib")) || (StringEndsWith(file, ".so")) || (StringContain(file, ".so.")));

		if (StringStartsWith(file, "System.")) include = false; // Hack, dotnet7, avoid a lots of intermediary files

		if (include)
		{
			std::string sha256 = FsFileSHA256Sum(filePathFull);
			output += sha256 + ";" + filePathFull + "\n";
		}
	}
	return output;
}

bool IBase::IntegrityCheckFileKnown(const std::string& path)
{
	if (FsFileExists(path) == false)
		return false;

	if (FsFileGetDirectory(path) != FsFileGetDirectory(GetProcessPathCurrent()))
	{
		// Not Elevated Path
		return false;
	}

	std::string integrityKnown = IntegrityCheckRead(GetLaunchMode());

	// Seem excessive to check/compute-sha all files every time, but read comment in IntegrityCheckBuild.
	std::string integrityComputed = IntegrityCheckBuild();

	return (integrityKnown == integrityComputed);
}

std::string IBase::GetProcessPathCurrent()
{
	return GetProcessPathOfId(GetCurrentProcessId());
}

std::string IBase::GetProcessPathCurrentDir()
{
	std::string procPath = GetProcessPathCurrent();
	size_t pos = procPath.find_last_of(FsPathSeparator);
	if (pos != std::string::npos)
		procPath = procPath.substr(0, pos);
	return procPath;
}

time_t IBase::GetProcessModTimeStart()
{
	return m_lastModified;
}

time_t IBase::GetProcessModTimeCurrent()
{
	std::string path = GetProcessPathCurrent();

	struct stat st;
	memset(&st, 0, sizeof(struct stat));
	if (stat(path.c_str(), &st) == 0)
	{
		return st.st_mtime;
	}
	else
	{
		return 0;
	}
}

std::vector<std::string> IBase::GetNetworkInterfacesNames()
{
	// Overrided by linux and macos, windows enum at C#
	std::vector<std::string> result;
	return result;
}

std::string IBase::GetTempPath(const std::string& filename)
{
	return FsGetTempPath() + FsPathSeparator + "eddie_tmp_" + filename;
}

void IBase::AddTorCookiePaths(const std::string& torPath, const std::string& username, std::vector<std::string>& result)
{
}

// --------------------------
// Utils filesystem
// --------------------------

bool IBase::FsFileWriteText(const std::string& path, const std::string& body)
{
	std::ofstream f(path);
	if (!f)
		return "";
	f << body;
	f.close();
	return true;
}

bool IBase::FsFileAppendText(const std::string& path, const std::string& body)
{
	FILE* f = fopen(path.c_str(), "a");
	if (f == NULL)
		return false;
	fprintf(f, "%s", body.c_str());
	fclose(f);
	return true;
}

std::string IBase::FsFileGetExtension(const std::string& path)
{
	std::string ext = "";
	std::string::size_type extPos = path.rfind('.');
	if (extPos != std::string::npos)
		ext = StringToLower(path.substr(extPos + 1));
	return ext;
}

std::string IBase::FsFileGetDirectory(const std::string& path)
{
	//std::filesystem::path p = path;
	//return p.parent_path();

	return path.substr(0, path.find_last_of("/\\"));
}

std::string IBase::FsFileSHA256Sum(const std::string& path)
{
	if (FsFileExists(path) == false)
		ThrowException("Unable to find for sha256 hash, path: " + path);
	std::vector<char> buf = FsFileReadBytes(path);
	return SHA256((unsigned char*)buf.data(), (unsigned long)buf.size());
}

std::string IBase::FsLocateExecutable(const std::string& name, const bool throwException, const bool includeTools)
{
	std::vector<std::string> paths;

	if (includeTools)
	{
		// Used only for 'wireguard-go' and 'wg' in macOS
		paths.insert(std::end(paths), std::begin(m_binPaths), std::end(m_binPaths));
	}

	if (true)
	{
		std::vector<std::string> envPaths = FsGetEnvPath();
		paths.insert(std::end(paths), std::begin(envPaths), std::end(envPaths));
	}

	for (std::vector<std::string>::const_iterator i = paths.begin(); i != paths.end(); ++i)
	{
		std::string fullPath = *i + FsPathSeparator + name;
		if (FsFileExists(fullPath))
		{
			if (CheckIfExecutableIsAllowed(fullPath, throwException) == false)
				return "";
			else
				return fullPath;
		}
	}

	return "";
}


// --------------------------
// Utils string
// --------------------------

std::string IBase::StringReplaceAll(const std::string& str, const std::string& from, const std::string& to)
{
	std::string result = str;
	size_t start_pos = 0;
	while ((start_pos = result.find(from, start_pos)) != std::string::npos) {
		result.replace(start_pos, from.length(), to);
		start_pos += to.length();
	}
	return result;
}

std::string IBase::StringExtractBetween(const std::string& str, const std::string& from, const std::string& to)
{
	size_t start_pos = str.find(from);
	if (start_pos == std::string::npos)
		return "";
	size_t end_pos = str.find(to, start_pos + from.length());
	if (end_pos == std::string::npos)
		return "";
	return str.substr(start_pos + from.length(), end_pos - start_pos - from.length());
}

std::vector<std::string> IBase::StringToVector(const std::string& s, const char c, bool autoTrim)
{
	std::vector<std::string> result;
	const char* str = s.c_str();

	do
	{
		const char* begin = str;
		while (*str != c && *str)
			str++;
		std::string item = std::string(begin, str);
		if (autoTrim)
		{
			item = StringTrim(item);
			if (item == "")
				continue;
		}

		result.push_back(item);
	} while (0 != *str++);

	return result;
}

std::string IBase::StringFromVector(const std::vector<std::string>& v, const std::string& delimiter)
{
	std::string output;
	for (uint i = 0; i < v.size(); i++)
	{
		if (i != 0)
			output += delimiter;
		output += v[i];
	}
	return output;
}

std::string IBase::StringFrom(const int& i)
{
	return std::to_string(i);
}

int IBase::StringToInt(const std::string& s)
{
	return strtol(s.c_str(), NULL, 10);
}

unsigned long IBase::StringToULong(const std::string& s)
{
	return strtoul(s.c_str(), NULL, 10);
}

bool IBase::StringStartsWith(const std::string& s, const std::string& f)
{
	size_t pos = s.find(f);
	return (pos == 0);
}

bool IBase::StringEndsWith(const std::string& s, const std::string& f)
{
	return(s.size() >= f.size() && s.compare(s.size() - f.size(), f.size(), f) == 0);
}

bool IBase::StringContain(const std::string& s, const std::string& f)
{
	return (s.find(f) != std::string::npos);
}

bool IBase::StringVectorsEqual(const std::vector<std::string>& v1, const std::vector<std::string>& v2)
{
	if (v1.size() != v2.size())
		return false;

	for (uint i = 0; i < v1.size(); i++)
	{
		if (v1[i] != v2[i])
			return false;
	}

	return true;
}

bool IBase::StringVectorsEqualOrdered(const std::vector<std::string>& v1, const std::vector<std::string>& v2)
{
	if (v1.size() != v2.size())
		return false;

	std::vector<std::string> v1o(v1);
	std::vector<std::string> v2o(v2);

	std::sort(v1o.begin(), v1o.end());
	std::sort(v2o.begin(), v2o.end());

	return StringVectorsEqual(v1o, v2o);
}

std::string IBase::StringTrim(const std::string& s, const std::string& chars)
{
	std::string result = s;
	result.erase(0, result.find_first_not_of(chars));
	result.erase(result.find_last_not_of(chars) + 1);
	return result;
}

std::string IBase::StringTrim(const std::string& s)
{
	const std::string& chars = "\t\n\v\f\r ";
	return StringTrim(s, chars);
}

std::string IBase::StringToLower(const std::string& s)
{
	std::string result = s;
	std::transform(result.begin(), result.end(), result.begin(), ::tolower);
	return result;
}

std::string IBase::StringPruneCharsNotIn(const std::string& str, const std::string& allowed)
{
	std::string result = "";
	for (std::string::const_iterator it = str.begin(); it != str.end(); ++it)
	{
		if (allowed.find(*it) != std::string::npos)
			result += *it;
	}
	return result;
}

std::string IBase::StringDeleteLinesContain(const std::string& str, const std::string& search)
{
	std::string n = str;
	for(int t=0;t<1000;t++)
	{
		size_t p = n.find(search);
		if (p == std::string::npos)
			break;
		
		size_t pStart = n.rfind("\n", p);
		if(pStart == std::string::npos) 
			pStart = 0;
		size_t pEnd = n.find("\n", p);
		if(pEnd == std::string::npos) 
			pEnd = n.length();

		n = n.substr(0, pStart) + n.substr(pEnd);
	}
	return n;
}

std::string IBase::StringEnsureAlphaNumeric(const std::string& str)
{
	return StringPruneCharsNotIn(str, "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ ");
}

std::string IBase::StringEnsureHex(const std::string& str)
{
	return StringPruneCharsNotIn(str, "0123456789abcdefABCDEF");
}

std::string IBase::StringEnsureFileName(const std::string& str)
{
	return StringPruneCharsNotIn(str, " .;:-_0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ()[]{}");
}

std::string IBase::StringEnsureDirectoryName(const std::string& str)
{
	return StringPruneCharsNotIn(str, " .;:-_0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ()[]{}");
}

std::string IBase::StringEnsureInterfaceName(const std::string& str)
{
	return str;
}

std::string IBase::StringEnsureCidr(const std::string& str)
{
	return StringPruneCharsNotIn(str, "0123456789abcdef.:/");
}

std::string IBase::StringEnsureIpAddress(const std::string& str)
{
	return StringPruneCharsNotIn(str, "0123456789abcdef.:");
}

std::string IBase::StringEnsureNumericInt(const std::string& str)
{
	return StringPruneCharsNotIn(str, "-0123456789");
}

std::string IBase::StringEnsureQuote(const std::string& str)
{
	return StringReplaceAll(str, "\"", "\\\"");
}

std::string IBase::StringBase64Encode(const std::string& str)
{
	return base64_encode(str);
}

std::string IBase::StringBase64Decode(const std::string& str)
{
	return base64_decode(str);
}

std::string IBase::StringXmlEncode(const std::string& str)
{
	std::string result = str;
	result = StringReplaceAll(result, "&", "&amp;");
	result = StringReplaceAll(result, "<", "&lt;");
	result = StringReplaceAll(result, ">", "&gt;");
	result = StringReplaceAll(result, "\"", "&quot;");
	result = StringReplaceAll(result, "'", "&apos;");
	return result;
}

std::string IBase::StringHexEncode(const unsigned char* buf, const size_t s)
{
	const char digitshex[] = "0123456789abcdef";
	std::string hex;
	hex.reserve(s * 2);
	for (size_t i = 0; i < s; i++)
	{
		unsigned char c = buf[i];
		hex.push_back(digitshex[c >> 4]);
		hex.push_back(digitshex[c & 15]);
	}
	return hex;
}

std::string IBase::StringHexEncode(const std::vector<char>& bytes)
{
	return StringHexEncode((const unsigned char*)bytes.data(), bytes.size());
}

std::string IBase::StringHexEncode(const int v, const int chars)
{
	std::stringstream ss;
	ss << std::setfill('0') << std::setw(chars) << std::uppercase << std::hex << v;
	return ss.str();
}

std::string IBase::StringSHA256(const std::string& str)
{
	return SHA256((const unsigned char*)str.c_str(), (unsigned long)str.length());
}

bool IBase::StringIsIPv4(const std::string& ip) // TOFIX: can be better
{
	return ip.find('.') != std::string::npos;
}

bool IBase::StringIsIPv6(const std::string& ip) // TOFIX: can be better
{
	return ip.find(':') != std::string::npos;
}

// Normalization and compression (for IPv6), like "0000:0000::1" => "::1";
std::string IBase::StringIpNormalize(const std::string& ip)
{
	if (StringIsIPv4(ip))
	{
		return "todo:" + ip;
	}
	else if (StringIsIPv6(ip))
	{
		return "todo:" + ip;
	}
	else
		return "";
}

std::string IBase::StringIpRemoveInterface(const std::string& ip)
{
	// "ff01::%lo0/32" => "ff01::/32"	
	size_t start_pos = ip.find('%');
	if (start_pos == std::string::npos)
		return ip;
	size_t range_pos = ip.find('/');
	if (range_pos == std::string::npos)
		return ip.substr(0, start_pos);
	else
		return ip.substr(0, start_pos) + ip.substr(range_pos);
}

// --------------------------
// Utils JSON (avoid library until our usage keep simple)
// --------------------------

std::string IBase::JsonEncode(const std::string& v)
{
	return StringReplaceAll(v, "\"", "\\\"");
}

std::string IBase::JsonFromKeyPairs(std::map<std::string, std::string>& kp)
{
	std::string result;
	int f = 0;
	for (std::map<std::string, std::string>::iterator it = kp.begin(); it != kp.end(); ++it)
	{
		std::string k = it->first;
		std::string v = it->second;
		if (f > 0)
			result += ",";
		result += "\"" + JsonEncode(k) + "\":\"" + JsonEncode(v) + "\"";
		f++;
	}
	return "{" + result + "}";
}

// --------------------------
// Utils other
// --------------------------

std::string IBase::SHA256(const unsigned char* pBuf, const unsigned long s)
{
	sha256_context ctx;
	sha256_starts(&ctx);
	sha256_update(&ctx, (unsigned char*)pBuf, s);
	unsigned char sha256sum[32];
	sha256_finish(&ctx, sha256sum);
	return StringHexEncode(&sha256sum[0], 32);
}

unsigned long IBase::GetTimestampUnix()
{
	return (unsigned long)time(NULL);
}

// Convert a standard INI config to a key-value map. Every section will be prefix for a key. Duplicate key adapted as comma-separated.
std::map<std::string, std::string> IBase::IniConfigToMap(const std::string& ini, std::string sectionKeySeparator, bool convertKeyToLower)
{
	std::map<std::string, std::string> result;

	std::string section = "";
	std::vector<std::string> lines = StringToVector(ini, '\n');
	for (std::vector<std::string>::const_iterator i = lines.begin(); i != lines.end(); ++i)
	{
		std::string lineNormalized = *i;
		lineNormalized = StringTrim(lineNormalized);

		// Comment in standard INI
		if (StringStartsWith(lineNormalized, ";")) 
			continue;

		// WireGuard INI consider comment any text after a #
		// Also systemd use INI-style with # as comment
		size_t posComment = lineNormalized.find("#"); 
		if (posComment != std::string::npos)
			lineNormalized = StringTrim(lineNormalized.substr(0, posComment));

		if (lineNormalized == "") // Empty
			continue;

		if (StringStartsWith(lineNormalized, "[")) // Section
			section = StringExtractBetween(lineNormalized, "[", "]");
		else
		{
			size_t posSep = lineNormalized.find('=');
			if (posSep != std::string::npos)
			{
				std::string key = StringTrim(lineNormalized.substr(0, posSep));
				std::string value = StringTrim(lineNormalized.substr(posSep + 1));

				if (section != "")
					key = section + sectionKeySeparator + key;

				if (convertKeyToLower)
					key = StringToLower(key);
		
				if (result.find(key) != result.end())
					result[key] = result[key] + "," + value;
				else
					result[key] = value;
			}
		}
	}

	return result;
}

std::map<std::string, std::string> IBase::ParseCommandLine(const std::vector<std::string>& args)
{
	// Note: accept "key1 key2=value2 key3" syntax. No "--key", no "--key='value'", no quotes.
	std::map<std::string, std::string> result;

	for (size_t i = 0; i < args.size(); i++)
	{
		std::string f = args[i];

		std::string k = "";
		std::string v = "";

		if (i == 0)
		{
			k = "path";
			v = f;
		}
		else
		{
			size_t posKV = f.find('=');
			if (posKV != std::string::npos)
			{
				k = f.substr(0, posKV);
				v = f.substr(posKV + 1);
			}
			else
			{
				k = f;
				v = "";
			}
		}

		if ((k == "service") && (v == "")) // Compatibility >=2.18.1 and <=2.18.4
		{
			k = "mode";
			v = "service";
		}

		// Remove quote
		if ((v.length() >= 2) && (v[0] == '\"') && (v[v.length() - 1] == '\"'))
			v = v.substr(1, v.length() - 2);

		if (k != "")
		{
			LogDevDebug("CmdLine Arg:" + k + "=" + v);
			result[k] = v;
		}
	}

	return result;
}

std::string IBase::CheckValidOpenVpnConfigFile(const std::string& path)
{
	if (FsFileExists(path) == false)
		return "file not found";

	std::string body = FsFileReadText(path);
	std::vector<std::string> lines = StringToVector(body, '\n');
	for (std::vector<std::string>::const_iterator i = lines.begin(); i != lines.end(); ++i)
	{
		std::string lineNormalized = *i;
		lineNormalized = StringTrim(StringToLower(lineNormalized));

		if (StringStartsWith(lineNormalized, "#"))
			continue;

		bool lineAllowed = true;

		// The below list directive can be incompleted with newest OpenVPN version, 
		// but any new dangerous directive is expected to be blocked by script-security
		// Note: parser can be better (any <key> lines are treated as directive, but never match for the space)
		// But this validation is only for additional security, Eddie already parse and prune with a better parser the config
		if (StringStartsWith(lineNormalized, "script-security ")) lineAllowed = false;
		if (StringStartsWith(lineNormalized, "plugin ")) lineAllowed = false;
		if (StringStartsWith(lineNormalized, "up ")) lineAllowed = false;
		if (StringStartsWith(lineNormalized, "down ")) lineAllowed = false;
		if (StringStartsWith(lineNormalized, "client-connect ")) lineAllowed = false;
		if (StringStartsWith(lineNormalized, "client-disconnect ")) lineAllowed = false;
		if (StringStartsWith(lineNormalized, "learn-address ")) lineAllowed = false;
		if (StringStartsWith(lineNormalized, "auth-user-pass-verify ")) lineAllowed = false;
		if (StringStartsWith(lineNormalized, "tls-verify ")) lineAllowed = false;
		if (StringStartsWith(lineNormalized, "ipchange ")) lineAllowed = false;
		if (StringStartsWith(lineNormalized, "iproute ")) lineAllowed = false;
		if (StringStartsWith(lineNormalized, "route-up ")) lineAllowed = false;
		if (StringStartsWith(lineNormalized, "route-pre-down ")) lineAllowed = false;

		if (lineAllowed == false)
			return "directive '" + lineNormalized + "' not allowed";
	}

	return "";
}

std::string IBase::CheckValidHummingbirdConfigFile(const std::string& path)
{
	return CheckValidOpenVpnConfigFile(path);
}

std::string IBase::CheckValidWireGuardConfig(const std::string& path)
{
	// Nothing here about security, anyway any PostUp and similar events are ignored in our implementation.
	return "";
}

// If Elevated launch child process, add to this list.
// Used by some action like 'kill' to allow signal only to child process.

void IBase::PidAdd(pid_t pid)
{
	m_pidManaged[pid] = true;
}

void IBase::PidRemove(pid_t pid)
{
	m_pidManaged.erase(pid);
}

bool IBase::PidManaged(pid_t pid)
{
	if (m_pidManaged.find(pid) != m_pidManaged.end())
		return true;
	else
		return false;
}

// --------------------------
// Helper
// --------------------------

void IBase::ThrowException(const std::string& message)
{
	throw std::runtime_error(message);
}

ExecResult IBase::ExecEx(const std::string& path, const std::vector<std::string>& args)
{
	return ExecEx(path, args, true);
}

ExecResult IBase::ExecEx(const std::string& path, const std::vector<std::string>& args, bool log)
{
	ExecResult result;
	result.path = path;
	result.args = args;
	result.exit = ExecRaw(path, args, false, "", result.out, result.err, log);
	return result;
}

ExecResult IBase::ExecEx(const std::string& path, const std::vector<std::string>& args, const std::string& stdinput)
{
	ExecResult result;
	result.path = path;
	result.args = args;
	result.exit = ExecRaw(path, args, true, stdinput, result.out, result.err, true);
	return result;
}

std::string IBase::GetExecResultReport(const ExecResult& result)
{
	std::string report = "exit:" + std::to_string(result.exit);
	if (StringTrim(result.out) != "")
		report += "; out:" + StringTrim(result.out);
	if (StringTrim(result.err) != "")
		report += "; err:" + StringTrim(result.err);
	if (StringTrim(result.path) != "")
		report += "; path:" + StringTrim(result.path);
	for (std::vector<std::string>::const_iterator i = result.args.begin(); i != result.args.end(); ++i)
	{
		report += "; arg:" + StringTrim(*i);
	}
	return report;
}

ExecResult IBase::ExecEx0(const std::string& path)
{
	std::vector<std::string> args;
	return ExecEx(path, args);
}

ExecResult IBase::ExecEx1(const std::string& path, const std::string& arg1)
{
	std::vector<std::string> args;
	args.push_back(arg1);
	return ExecEx(path, args);
}

ExecResult IBase::ExecEx2(const std::string& path, const std::string& arg1, const std::string& arg2)
{
	std::vector<std::string> args;
	args.push_back(arg1);
	args.push_back(arg2);
	return ExecEx(path, args);
}

ExecResult IBase::ExecEx3(const std::string& path, const std::string& arg1, const std::string& arg2, const std::string& arg3)
{
	std::vector<std::string> args;
	args.push_back(arg1);
	args.push_back(arg2);
	args.push_back(arg3);
	return ExecEx(path, args);
}

ExecResult IBase::ExecEx4(const std::string& path, const std::string& arg1, const std::string& arg2, const std::string& arg3, const std::string& arg4)
{
	std::vector<std::string> args;
	args.push_back(arg1);
	args.push_back(arg2);
	args.push_back(arg3);
	args.push_back(arg4);
	return ExecEx(path, args);
}

ExecResult IBase::ExecEx5(const std::string& path, const std::string& arg1, const std::string& arg2, const std::string& arg3, const std::string& arg4, const std::string& arg5)
{
	std::vector<std::string> args;
	args.push_back(arg1);
	args.push_back(arg2);
	args.push_back(arg3);
	args.push_back(arg4);
	args.push_back(arg5);
	return ExecEx(path, args);
}

ExecResult IBase::ExecEx6(const std::string& path, const std::string& arg1, const std::string& arg2, const std::string& arg3, const std::string& arg4, const std::string& arg5, const std::string& arg6)
{
	std::vector<std::string> args;
	args.push_back(arg1);
	args.push_back(arg2);
	args.push_back(arg3);
	args.push_back(arg4);
	args.push_back(arg5);
	args.push_back(arg6);
	return ExecEx(path, args);
}

ExecResult IBase::ExecEx7(const std::string& path, const std::string& arg1, const std::string& arg2, const std::string& arg3, const std::string& arg4, const std::string& arg5, const std::string& arg6, const std::string& arg7)
{
	std::vector<std::string> args;
	args.push_back(arg1);
	args.push_back(arg2);
	args.push_back(arg3);
	args.push_back(arg4);
	args.push_back(arg5);
	args.push_back(arg6);
	args.push_back(arg7);
	return ExecEx(path, args);
}

ExecResult IBase::ExecEx8(const std::string& path, const std::string& arg1, const std::string& arg2, const std::string& arg3, const std::string& arg4, const std::string& arg5, const std::string& arg6, const std::string& arg7, const std::string& arg8)
{
	std::vector<std::string> args;
	args.push_back(arg1);
	args.push_back(arg2);
	args.push_back(arg3);
	args.push_back(arg4);
	args.push_back(arg5);
	args.push_back(arg6);
	args.push_back(arg7);
	args.push_back(arg8);
	return ExecEx(path, args);
}

void Pinger::OnResponse(const uint16_t& id, const int& result)
{
	std::string r = std::to_string(id) + "," + std::to_string(result);
	m_pBase->ReplyCommand(m_commandId, r);
}

// --------------------------
// Outside class
// --------------------------

void ThreadCommand(IBase* impl, const std::string id, const std::string command, std::map<std::string, std::string> params)
{
	impl->MainDo(id, command, params);
}
