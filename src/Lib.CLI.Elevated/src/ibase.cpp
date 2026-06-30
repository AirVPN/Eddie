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
#include <unistd.h> // read, write, close
#endif

#include "../include/ibase.h"

#include "../../../dependencies/base64/base64.h"
#include "../../../dependencies/sha256/sha256.h"

#define IPC_MAX_REPLY_SIZE (4 * 1024 * 1024)

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
				{
					LogLocal("Handshake rejected: version mismatch (elevated " + m_elevatedVersion + ", client " + clientVersion + ")");
					ThrowException("Version mismatch");
				}

				if (params.find("path") != params.end()) // 2.24.5
				{
					if (GetProcessPathCurrentDir() != params["path"])
					{
						// This occur for example if Eddie in installed and service active, but another portable is running.
						// Basically this is not a problem, if ElevatedVersion above match.
						// But Security Hashes Checks will fail.
						LogLocal("Handshake rejected: path mismatch (elevated " + GetProcessPathCurrentDir() + ", client " + params["path"] + ")");
						ThrowException("Path mismatch");
					}
				}

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

void IBase::SetLaunchMode(const std::string& mode)
{
	m_launchMode = mode;
}

int IBase::GetSpotClientPid()
{
	return m_spotClientPid;
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
#if defined(Debug) || defined(_DEBUG)
	std::string logPath = GetDevLogPath();
	if (FsFileExists(logPath))
	{
		FILE* f = fopen(logPath.c_str(), "a");
		if (f != NULL)
		{
			fprintf(f, "%lu - Elevated,PID: %d - %s%s", (unsigned long)GetTimestampUnix(), (int)GetCurrentProcessId(), msg.c_str(), FsEndLine.c_str());
			fclose(f);
		}
	}
#endif
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
	if (m_clientConnected == false)
		return;

	std::string sendBuffer = message + "\n";

	m_mutex_inout.lock();

	if (m_clientConnected == false)
	{
		m_mutex_inout.unlock();
		return;
	}

	if (sendBuffer.length() > IPC_MAX_REPLY_SIZE)
	{
		TransportClientClose();
		m_clientConnected = false;
		m_mutex_inout.unlock();
		LogLocal("Outgoing IPC message exceeds cap (" + std::to_string(sendBuffer.length()) + " bytes), client connection closed");
		return;
	}

	int nWrite = TransportWrite(sendBuffer.c_str(), (int)sendBuffer.length());

	m_mutex_inout.unlock();

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
		SetLaunchMode("spot");

		if (m_cmdline.find("spot_port") != m_cmdline.end())
			port = StringToInt(m_cmdline["spot_port"]);

		// Bind this one-time elevation to the client instance that launched it.
		if (m_cmdline.find("spot_client_pid") != m_cmdline.end())
			m_spotClientPid = StringToInt(m_cmdline["spot_client_pid"]);

		m_singleConnMode = true;

		// If launched in SPOT mode, if service was active, they not accept for some reason (generally upgrade and fail integrity check), so reinstall.
		ServiceReinstall();

		// Update Security Info
		IntegrityCheckUpdate("spot");
	}
	else if ((m_cmdline.find("mode") != m_cmdline.end()) && (m_cmdline["mode"] == "service"))
	{
		SetLaunchMode("service");

		if (m_cmdline.find("service_port") != m_cmdline.end())
			port = StringToInt(m_cmdline["service_port"]);

		m_singleConnMode = false;
	}
	else
	{
		LogLocal("This application can't be run directly, it's used internally by Eddie. (unknown mode)");
		return 1;
	}

	// Stage privileged tools into a root-only directory when running from a writable
	// location, so every later exec/load resolves to a copy an unprivileged user cannot swap.
	// Fail-closed: if staging is required (writable layout) but cannot be secured, refuse to
	// start rather than run tools from a swappable directory (privilege-escalation race).
	if (StagingPrepare() == false)
	{
		LogLocal("Elevated startup aborted: privileged-tool staging failed");
		return 1;
	}

	TransportListen(port);

	for (;;)
	{
		if (IsStopRequested())
			break;

		m_keypair.clear();
		m_session_key = "";
		m_clientConnected = false;
		char* buffer = (char*)malloc(NETBUFSIZE);
		if (buffer == NULL)
			break;

		LogDebug("Waiting for client");

		try
		{
			if (TransportAccept())
			{
				m_clientConnected = true;

				// Check allowed
				if(true)
				{
					int clientProcessId = TransportGetClientProcessId();

					if (clientProcessId == 0)
						ThrowException("Client not allowed: Cannot detect client process");

					std::string clientProcessPath = GetProcessPathOfId(clientProcessId);
					if (clientProcessPath == "")
						ThrowException("Client not allowed: Cannot detect client process path");

					// Spot mode: the one-time elevation is bound to the client instance that launched it.
					// The expected client PID is passed at launch (spot_client_pid) and compared against the
					// kernel-attested peer PID of the connecting socket. This replaces the previous parent-chain
					// walk, which is unreliable when the launcher is not in the elevated process ancestry
					// (e.g. macOS AuthorizationExecuteWithPrivileges routes through security_authtrampoline).
					if (GetLaunchMode() == "spot")
					{
						if (GetSpotClientPid() == 0)
							ThrowException("Client not allowed: Spot mode, missing client PID binding");

						if (clientProcessId != GetSpotClientPid())
							ThrowException("Client not allowed: Spot mode, client PID mismatch");
					}

#if defined(Debug) || defined(_DEBUG)
#else
					if (true)
					{
						if (IntegrityCheckFileKnown(clientProcessPath) == false)
							ThrowException("Client not allowed: Integrity check failed ");
					}
#endif
				}

				ReplyPID(GetCurrentProcessId());

				LogRemote("Privileged tools run from: " + (m_stagingDir == "" ? "install directory (secure, no staging)" : "staging directory " + m_stagingDir));

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

					int n = TransportRead(buffer + bufferPos, nMaxRead);
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

							line = StringTrim(line);
							if (line.empty() || !StringStartsWith(line, "command:"))
							{
								LogLocal("Client protocol rejected");
								clientStop = true;
								break;
							}

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

		TransportClientClose();
		m_clientConnected = false;

		free(buffer);

		if (m_singleConnMode)
			break;
	}

	LogDebug("Closing");

	TransportServerClose();

	StagingCleanup();

	if (m_singleConnMode)
	{
		if (GetLaunchMode() == "service")
		{
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
	else if (command == "tor-get-info")
	{
		// Eddie need to talk with Tor Control to ask for new circuits or obtain guard IPs.
		std::string processName;
		std::string processPath;
		std::string username;
		std::string cookieFoundPath;
		std::string cookiePasswordHex;

		if (params.find("name") != params.end())
			processName = params["name"];

		if (params.find("path") != params.end())
			processPath = params["path"];

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

	// Secure install (root-only file + dir): the co-located client cannot have been planted or
	// swapped by an unprivileged user, so the full-directory SHA256 recompute adds nothing. Skip it.
	if (CanUseDirectPath(GetProcessPathCurrent()))
		return true;

	std::string integrityKnown = IntegrityCheckRead(GetLaunchMode());

	// Writable layout: recompute and compare the whole-directory hash against the stored snapshot
	// to detect a swapped client binary or an injected library since startup/install.
	std::string integrityComputed = IntegrityCheckBuild();

	return (integrityKnown == integrityComputed);
}

// Executable security

bool IBase::IsRunnableExecutable(const std::string& path)
{
	// Safe to execute/load as root iff the file AND its parent directory are root-only, so an
	// unprivileged user cannot swap it (TOCTOU). Bundled tools are guaranteed this by staging
	// (or a secure install dir); system tools resolve from root-only $PATH dirs. The integrity
	// snapshot is not needed here: in a root-only directory only root can change the bytes.
	return CanUseDirectPath(path);
}

bool IBase::IsSecureServiceLocation(const std::string& path)
{
	// The service repeatedly launches this binary as root: both the file and its directory
	// must be root-only, otherwise the binary could be swapped between launches.
	return CanUseDirectPath(path);
}

bool IBase::CanUseDirectPath(const std::string& path)
{
	// True when both the file and its directory are root-only: the executable can be run
	// in place without staging, with no writable-path TOCTOU window.
	return FsFileIsRootOnly(path) && FsDirectoryIsRootOnly(FsFileGetDirectory(path));
}

bool IBase::StagingPrepare()
{
	// Per-launch-mode staging dir (.../stage/spot, .../stage/service): spot and service never share
	// a directory, so installing/uninstalling the service from a connected spot (or vice versa) cannot
	// purge or delete the other's staged tools. Only one spot per machine at a time is supported.
	std::string stagingBase = GetStagingDir();
	std::string stagingDir = stagingBase + FsPathSeparator + GetLaunchMode();

	// Purge only our own leftovers first: covers crashes and persistent backends (e.g. Windows ProgramData).
	FsDirectoryDelete(stagingDir, true);

	m_stagingDir = "";

	// Secure install (root-only elevated directory): run privileged tools in place, no staging needed.
	if (CanUseDirectPath(GetProcessPathCurrent()))
		return true;

	std::string srcDir = GetProcessPathCurrentDir();
	FsDirectoryCreate(FsFileGetDirectory(stagingBase)); // .../Eddie-VPN | .../eddie-vpn
	FsDirectoryCreate(stagingBase);                     // .../stage
	FsDirectoryCreate(stagingDir);                      // .../stage/<mode>
	if (FsDirectoryEnsureRootOnly(stagingDir) == false)
	{
		LogLocal("Staging: unable to secure directory " + stagingDir + ", refusing to start (running tools from a writable directory would re-open a privilege-escalation race)");
		FsDirectoryDelete(stagingDir, true);
		return false;
	}

	std::vector<std::string> files = FsFilesInPath(srcDir);
	int copied = 0;
	for (std::vector<std::string>::const_iterator i = files.begin(); i != files.end(); ++i)
	{
		const std::string& file = *i;

		// Whitelist of privileged tools and their bundled libraries/drivers, matched by name prefix
		// (OS-agnostic: covers .exe, extensionless POSIX names, .so/.dylib/.dll and driver files alike).
		bool needCopy = false;
		if (StringStartsWith(file, "openvpn")) needCopy = true;
		if (StringStartsWith(file, "hummingbird")) needCopy = true;
		if (StringStartsWith(file, "wireguard")) needCopy = true;
		if (StringStartsWith(file, "wg")) needCopy = true;
		if (StringStartsWith(file, "amnezia")) needCopy = true;
		if (StringStartsWith(file, "awg")) needCopy = true;
		if (StringStartsWith(file, "tapctl")) needCopy = true;
		if (StringStartsWith(file, "libssl")) needCopy = true;
		if (StringStartsWith(file, "libpkcs11")) needCopy = true;
		if (StringStartsWith(file, "liblzo")) needCopy = true;
		if (StringStartsWith(file, "liblz4")) needCopy = true;
		if (StringStartsWith(file, "libcrypto")) needCopy = true;
		if (StringStartsWith(file, "libnl")) needCopy = true;
		if (StringStartsWith(file, "libssp")) needCopy = true;
		if (StringStartsWith(file, "libcap")) needCopy = true;
		if (StringStartsWith(file, "ovpn-dco")) needCopy = true;
		if (StringStartsWith(file, "tap0901")) needCopy = true;
		if (needCopy == false)
			continue;

		std::string src = srcDir + FsPathSeparator + file;
		std::string dst = stagingDir + FsPathSeparator + file;
		if (FsFileCopy(src, dst) == false)
		{
			LogLocal("Staging: copy failed for " + file + ", refusing to start (running tools from a writable directory would re-open a privilege-escalation race)");
			FsDirectoryDelete(stagingDir, true);
			return false;
		}
		FsFileMakeRunnable(dst);
		copied++;
	}

	m_stagingDir = stagingDir;
	return true;
}

void IBase::StagingCleanup()
{
	if (m_stagingDir == "")
		return;

	std::string stagingDir = m_stagingDir;
	m_stagingDir = "";
	FsDirectoryDelete(stagingDir, true); // .../stage/<mode>

	// Best-effort removal of the now-possibly-empty parents (non-recursive delete only succeeds when
	// empty): the staging base (.../stage) is removed once the other mode's subdir is also gone, and
	// the shared runtime parent (POSIX dir holding the socket) is left untouched while still in use.
	std::string stagingBase = FsFileGetDirectory(stagingDir);
	FsDirectoryDelete(stagingBase, false);
	FsDirectoryDelete(FsFileGetDirectory(stagingBase), false);
}

std::string IBase::GetProcessPathCurrent()
{
	return GetProcessPathOfId(GetCurrentProcessId());
}

std::string IBase::GetProcessPathCurrentDir()
{
	return FsFileGetDirectory(GetProcessPathCurrent());
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

std::string IBase::FsLocateExecutable(const std::string& name, const bool throwException, const bool includeElevatedPath)
{
	std::vector<std::string> paths;

	if (includeElevatedPath)
	{
		// When staging is active the bundled tools have been copied into a root-only
		// directory; prefer those secured copies over the original writable bundle.
		if (m_stagingDir != "")
			paths.push_back(m_stagingDir);

		// Bundled tools (e.g. tapctl.exe on Windows, wg/wireguard-go on macOS)
		// live next to the elevated executable in production and dev layouts.
		paths.push_back(GetProcessPathCurrentDir());
	}

	{
		std::vector<std::string> envPaths = FsGetEnvPath();
		paths.insert(std::end(paths), std::begin(envPaths), std::end(envPaths));
	}

	std::string foundButNotAllowed = "";
	for (std::vector<std::string>::const_iterator i = paths.begin(); i != paths.end(); ++i)
	{
		std::string fullPath = *i + FsPathSeparator + name;
		if (FsFileExists(fullPath) == false)
			continue;
		if (IsRunnableExecutable(fullPath))
			return fullPath;
		foundButNotAllowed = fullPath;
	}

	if (throwException)
	{
		if (foundButNotAllowed != "")
			ThrowException("Executable '" + name + "' not allowed: " + CheckExecutablePathPermissions(foundButNotAllowed));
		else
			ThrowException("Executable '" + name + "' not found");
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

std::string IBase::StringEnsureAsciiName(const std::string& str)
{
	return StringPruneCharsNotIn(str, "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_-");
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
		lineNormalized = StringReplaceAll(lineNormalized, "\t", " ");

		if (StringStartsWith(lineNormalized, "#"))
			continue;

		bool lineAllowed = true;

		// The below list directive can be incompleted with newest OpenVPN version, 
		// but any new dangerous directive is expected to be blocked by script-security
		// Note: TABs are collapsed to spaces before prefix matching below.
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
		if (StringStartsWith(lineNormalized, "config ")) lineAllowed = false;
		if (StringStartsWith(lineNormalized, "include ")) lineAllowed = false;

		if (lineAllowed == false)
			return "directive '" + lineNormalized + "' not allowed";
	}

	return "";
}

std::string IBase::CheckValidHummingbirdConfigFile(const std::string& path)
{
	return CheckValidOpenVpnConfigFile(path);
}

std::string IBase::CheckValidWireGuardConfig(const std::string& config)
{
	std::vector<std::string> lines = StringToVector(config, '\n');
	for (std::vector<std::string>::const_iterator i = lines.begin(); i != lines.end(); ++i)
	{
		std::string lineNormalized = StringTrim(*i);

		size_t posComment = lineNormalized.find("#");
		if (posComment != std::string::npos)
			lineNormalized = StringTrim(lineNormalized.substr(0, posComment));

		if (lineNormalized == "")
			continue;

		if (StringStartsWith(lineNormalized, "["))
			continue;

		size_t posSep = lineNormalized.find('=');
		if (posSep == std::string::npos)
			continue;

		std::string key = StringToLower(StringTrim(lineNormalized.substr(0, posSep)));
		std::string value = StringTrim(lineNormalized.substr(posSep + 1));

		if (key == "preup" || key == "postup" || key == "predown" || key == "postdown")
			return "directive '" + key + "' not allowed";

		if (key == "saveconfig")
			return "directive '" + key + "' not allowed";

		if (key == "table")
		{
			if (StringToLower(value) != "off")
				return "directive 'table' not allowed (only 'off' permitted)";
		}
	}

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
