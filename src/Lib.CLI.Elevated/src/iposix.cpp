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

#include "../include/iposix.h"

#include "signal.h"

#include <algorithm>
#include <cstring>
#include <dirent.h>
#include <errno.h>
#include <fcntl.h>           /* fcntl() */
#include <fstream>
#include <sstream>
#include <stdint.h>
#include <unistd.h>

#include <sys/socket.h>
#include <sys/stat.h>
#include <sys/time.h>
#include <sys/types.h>
#include <sys/un.h> // sockaddr_un (AF_UNIX local transport)

#include "../../../dependencies/PStreams/pstream.h"
using namespace redi; // related to pstream.h

#include "../../../dependencies/NlohmannJSON/json.hpp"
using json = nlohmann::json;

// --------------------------
// Virtual
// --------------------------

void IPosix::Idle()
{
	// Scenario: macOS with portable app in /Application, create launchd daemon, replace the app with a new version
	if (GetLaunchMode() == "service")
	{
		time_t start = GetProcessModTimeStart();
		time_t cur = GetProcessModTimeCurrent();
		if (start != cur)
		{
			LogLocal("Executable changed/upgraded, exit");
			KillProcess("sigterm", GetCurrentProcessId());
		}
	}

	IBase::Idle();
}

void IPosix::Do(const std::string& commandId, const std::string& command, std::map<std::string, std::string>& params)
{
	if (command == "openvpn")
	{
		std::string id = params["id"];
		std::string action = params["action"];

		if (action == "stop")
		{
			std::string signal = params["signal"];

			if (m_keypair.find("openvpn_pid_" + id) != m_keypair.end())
			{
				pid_t pid = std::atoi(m_keypair["openvpn_pid_" + id].c_str());
				KillProcess(signal, pid);				
			}
		}
		else if (action == "start")
		{
			std::string path = FsLocateExecutable("openvpn", true, true);

			std::string checkResult = CheckValidOpenVpnConfigContent(params["config"]);
			if (checkResult != "")
			{
				ThrowException("Not supported OpenVPN config: " + checkResult);
			}
			else
			{
				std::string configPath = FsWriteRootOnlyTempConfig("openvpn", id, "ovpn", params["config"]);
				try
				{
				const pstreams::pmode mode = pstreams::pstdout | pstreams::pstderr;
				pstreams::argv_type argv;
				argv.push_back(path);

				argv.push_back("--config");
				argv.push_back(configPath);

				std::string openvpnDirectory = FsFileGetDirectory(path);
				int chdirResult = chdir(openvpnDirectory.c_str()); // AppImage, our openvpn look ./ for .so
				if (chdirResult != 0)
					ThrowException("Unable to chdir " + openvpnDirectory);

				pstream child(argv, mode);
				if (child.rdbuf()->is_open() == false)
				{
					int childError = child.rdbuf()->error();
					ThrowException("Unable to start OpenVPN: " + std::string(std::strerror(childError)));
				}
				char buf[1024 * 32];
				std::streamsize n;

				m_keypair["openvpn_pid_" + id] = std::to_string(child.pid());
				ReplyCommand(commandId, "procid:" + std::to_string(child.pid()));

				bool finished[2] = { false, false };
				while (!finished[0] || !finished[1])
				{
					if (!finished[0])
					{
						while ((n = child.err().readsome(buf, sizeof(buf))) > 0)
						{
							std::string o = std::string(buf, n);
							ReplyCommand(commandId, "stderr:" + o);
						}
						if (child.eof())
						{
							finished[0] = true;
							if (!finished[1])
								child.clear();
						}
					}

					if (!finished[1])
					{
						while ((n = child.out().readsome(buf, sizeof(buf))) > 0)
						{
							std::string o = std::string(buf, n);
							ReplyCommand(commandId, "stdout:" + o);
						}
						if (child.eof())
						{
							finished[1] = true;
							if (!finished[0])
								child.clear();
						}
					}

					Sleep(100); // 0.1 secs. TODO: Search for alternative...
				}

				m_keypair.erase("openvpn_pid_" + id);

				child.close();
				int status = child.rdbuf()->status();
				int exitCode = -1;
				if (WIFEXITED(status))
					exitCode = WEXITSTATUS(status);

				std::string exitCodeStr = std::to_string(exitCode);
				ReplyCommand(commandId, "return:" + exitCodeStr);
				}
				catch (...)
				{
					FsFileDelete(configPath);
					throw;
				}
				FsFileDelete(configPath);
			}
		}
	}
	else if (command == "hummingbird")
	{
		std::string id = params["id"];
		std::string action = params["action"];

		if (action == "stop")
		{
			std::string signal = params["signal"];

			if (m_keypair.find("hummingbird_pid_" + id) != m_keypair.end())
			{
				pid_t pid = std::atoi(m_keypair["hummingbird_pid_" + id].c_str());
				KillProcess(signal, pid);
			}
		}
		else if (action == "start")
		{
			std::string path = FsLocateExecutable("hummingbird", true, true);

			// Intentional: remove stale lock before start; Eddie-managed Hummingbird does not use HB recovery.
			if (FsFileExists("/etc/airvpn/hummingbird.lock"))
				FsFileDelete("/etc/airvpn/hummingbird.lock");

			std::string checkResult = CheckValidOpenVpnConfigContent(params["config"]);
			if (checkResult != "")
			{
				ThrowException("Not supported Hummingbird config: " + checkResult);
			}
			else
			{
				std::string configPath = FsWriteRootOnlyTempConfig("hummingbird", id, "ovpn", params["config"]);
				try
				{
				const pstreams::pmode mode = pstreams::pstdout | pstreams::pstderr;
				pstreams::argv_type argv;
				argv.push_back(path);

				if (params.find("dns-ignore") != params.end())
				{
					if (params["dns-ignore"] == "true")
						argv.push_back("--ignore-dns-push"); // TOFIX: must be a generic dns ignore, otherwise HB try to apply any dhcp-option in client-side config?
				}

				// Bypass HB network lock
				argv.push_back("--network-lock");
				argv.push_back("off");

				if (params.find("gui-version") != params.end())
				{
					argv.push_back("--gui-version");
					argv.push_back(params["gui-version"]);
				}

				argv.push_back(configPath);

				pstream child(argv, mode);
				char buf[1024 * 32];
				std::streamsize n;

				m_keypair["hummingbird_pid_" + id] = std::to_string(child.pid());
				ReplyCommand(commandId, "procid:" + std::to_string(child.pid()));

				bool finished[2] = { false, false };
				while (!finished[0] || !finished[1])
				{
					if (!finished[0])
					{
						while ((n = child.err().readsome(buf, sizeof(buf))) > 0)
						{
							std::string o = std::string(buf, n);
							ReplyCommand(commandId, "stderr:" + o);
						}
						if (child.eof())
						{
							finished[0] = true;
							if (!finished[1])
								child.clear();
						}
					}

					if (!finished[1])
					{
						while ((n = child.out().readsome(buf, sizeof(buf))) > 0)
						{
							std::string o = std::string(buf, n);
							ReplyCommand(commandId, "stdout:" + o);
						}
						if (child.eof())
						{
							finished[1] = true;
							if (!finished[0])
								child.clear();
						}
					}

					Sleep(100); // 0.1 secs. TOFIX: alternative?
				}

				m_keypair.erase("hummingbird_pid_" + id);

				child.close();
				int status = child.rdbuf()->status();
				int exitCode = -1;
				if (WIFEXITED(status))
					exitCode = WEXITSTATUS(status);

				std::string exitCodeStr = std::to_string(exitCode);
				ReplyCommand(commandId, "return:" + exitCodeStr);
				}
				catch (...)
				{
					FsFileDelete(configPath);
					throw;
				}
				FsFileDelete(configPath);
			}
		}
	}
	else
	{
		IBase::Do(commandId, command, params);
	}
}

void IPosix::AddTorCookiePaths(const std::string& torPath, const std::string& username, std::vector<std::string>& result)
{
	if (torPath != "") // TorBrowser
	{
		std::string path = FsFileGetDirectory(torPath) + "/../Data/Tor/control_auth_cookie";
		result.push_back(path);
	}

	result.push_back("/var/run/tor/control.authcookie");
	result.push_back("/var/lib/tor/control_auth_cookie");
	result.push_back("/run/tor/control.authcookie"); // Default Ubuntu 20.04.1 LTS tor from official repo
	result.push_back("/run/tor/control_auth_cookie"); // Variant of above
}

// --------------------------
// Virtual Pure, OS
// --------------------------

bool IPosix::IsRoot()
{
	uid_t euid = geteuid();
	return (euid == 0);
}

void IPosix::Sleep(int ms)
{
	usleep(ms * 1000);
}

uint64_t IPosix::GetTimestampUnixUsec()
{
	struct timeval now;
	return gettimeofday(&now, NULL) != 0 ? 0 : now.tv_sec * 1000000 + now.tv_usec;
}

pid_t IPosix::GetCurrentProcessId()
{
	return getpid();
}

pid_t IPosix::GetParentProcessId()
{
	return getppid();
}

pid_t IPosix::GetParentProcessId(pid_t pid)
{
	// We don't find a better method
	std::string statusPath = "/proc/" + std::to_string(pid) + "/status";
	if (!FsFileExists(statusPath))
		return 0;

	std::string statusBody = FsFileReadText(statusPath);
	std::string ppidS = StringTrim(StringExtractBetween(statusBody, "PPid:", "\n"));
	return atoi(ppidS.c_str());
}

pid_t IPosix::GetProcessIdOfName(const std::string& name)
{
	// TOFIX - Find a method without exec
	ExecResult pidofResult = ExecEx(FsLocateExecutable("pidof"), { name });
	if (pidofResult.exit == 0)
		return atoi(pidofResult.out.c_str());
	else
		return 0;
}

void IPosix::KillProcess(const std::string& signal, pid_t pid)
{
	int signalUnix = SIGINT;
	if (signal == "sigint")
		signalUnix = SIGINT;
	else if (signal == "sigterm")
		signalUnix = SIGTERM;

	kill(pid, signalUnix);
}

std::string IPosix::GetCmdlineOfProcessId(pid_t pid)
{
	std::string path = "/proc/" + std::to_string(pid) + "/cmdline";

	if (FsFileExists(path) == false)
		return "";

	std::string result;
	FILE* f;
	f = fopen(path.c_str(), "rb");
	if (f == NULL)
		return "";
	for (;;)
	{
		int chI = fgetc(f);
		if (chI == EOF)
			break;

		unsigned char ch = (unsigned char)chI;

		if (ch == 0)
			result += " ";
		else
			result += ch;
	}
	fclose(f);
	return result;
}

std::string IPosix::GetWorkingDirOfProcessId(pid_t pid)
{
	std::string procPath = "/proc/" + std::to_string(pid) + "/cwd";
	if (FsFileExists(procPath) == false)
		return "";

	char resolvedPath[PATH_MAX];
	size_t result = readlink(procPath.c_str(), resolvedPath, PATH_MAX - 1);
	resolvedPath[result] = 0;
	return std::string(resolvedPath);
}

void IPosix::SetEnv(const std::string& name, const std::string& value)
{
	setenv(name.c_str(), value.c_str(), 1);
}

int IPosix::ExecRaw(const std::string& path, const std::vector<std::string>& args, const bool stdinWrite, const std::string& stdinBody, std::string& stdOut, std::string& stdErr, const bool log)
{
	const pstreams::pmode mode = pstreams::pstdin | pstreams::pstdout | pstreams::pstderr;
	pstreams::argv_type argv;
	std::string logMessage;

	if (log)
		logMessage += "Exec, path:'" + path + "'";

	argv.push_back(path);
	for (std::vector<std::string>::const_iterator i = args.begin(); i != args.end(); ++i)
	{
		if (log)
			logMessage += ", arg:'" + *i + "'";
		argv.push_back(*i);
	}

	pstream child(argv, mode);
	char buf[1024];
	std::streamsize n;
	bool finished[2] = { false, false };

	if (stdinWrite)
	{
		if (stdinBody.length() != 0)
		{

			child.write(stdinBody.c_str(), stdinBody.length());
		}

		child.peof();
	}

	while (!finished[0] || !finished[1])
	{
		if (!finished[0])
		{
			while ((n = child.err().readsome(buf, sizeof(buf))) > 0)
			{
				stdErr += std::string(buf, n);
			}
			if (child.eof())
			{
				finished[0] = true;
				if (!finished[1])
					child.clear();
			}
		}

		if (!finished[1])
		{
			while ((n = child.out().readsome(buf, sizeof(buf))) > 0)
			{
				stdOut += std::string(buf, n);
			}
			if (child.eof())
			{
				finished[1] = true;
				if (!finished[0])
					child.clear();
			}
		}

		if (child.rdbuf()->exited())
			break;

		Sleep(10); // 0.01 secs. TODO: Search for alternative...
	}

	child.close();
	int status = child.rdbuf()->status();
	int exitCode = -1;
	if (WIFEXITED(status))
		exitCode = WEXITSTATUS(status);

	if (log)
	{
		logMessage += ", exit:" + std::to_string(exitCode);
		if (StringTrim(stdOut) != "")
		{
			std::string stdOutLog = stdOut;
			if ( (exitCode == 0) && (StringStartsWith(stdOut, "Routing tables")) ) stdOutLog = "<omissis>"; // Too much logs
			logMessage += ", out:'" + StringTrim(stdOutLog) + "'";
		}
		if (StringTrim(stdErr) != "")
			logMessage += ", err:'" + StringTrim(stdErr) + "'";
		LogDebug(logMessage);
	}

	return exitCode;
}

bool IPosix::FsDirectoryCreate(const std::string& path)
{
	if (FsDirectoryExists(path))
		return true;
	else
		return (mkdir(path.c_str(), 0755) == 0);
}

bool IPosix::FsFileExists(const std::string& path)
{
	struct stat db;
	return (stat(path.c_str(), &db) == 0);
}

bool IPosix::FsDirectoryExists(const std::string& path)
{
	struct stat db;
	return (stat(path.c_str(), &db) == 0 && S_ISDIR(db.st_mode));
}

bool IPosix::FsFileDelete(const std::string& path)
{
	if (FsFileExists(path))
		return (unlink(path.c_str()) == 0);
	else
		return true;
}

bool IPosix::FsDirectoryDelete(const std::string& path, bool recursive)
{
	if (FsDirectoryExists(path) == false)
		return true;

	if (recursive)
	{
		DIR* dirp = opendir(path.c_str());
		if (dirp == NULL)
			return false;

		struct dirent* entry;
		while ((entry = readdir(dirp)) != NULL)
		{
			std::string name = entry->d_name;
			if (name == "." || name == "..")
				continue;

			std::string child = path + "/" + name;

			struct stat st;
			if (lstat(child.c_str(), &st) != 0) // lstat: never follow symlinks
			{
				closedir(dirp);
				return false;
			}

			bool ok;
			if (S_ISDIR(st.st_mode))
				ok = FsDirectoryDelete(child, true);
			else
				ok = (unlink(child.c_str()) == 0); // removes the link itself, not its target

			if (ok == false)
			{
				closedir(dirp);
				return false;
			}
		}

		closedir(dirp);
	}

	return (rmdir(path.c_str()) == 0);
}

bool IPosix::FsFileMove(const std::string& source, const std::string& destination)
{
	return (rename(source.c_str(), destination.c_str()) == 0);
}

bool IPosix::FsFileCopy(const std::string& source, const std::string& destination)
{
	if(FsFileExists(destination))
	{
		if (FsFileDelete(destination) == false)
			return false;
	}
	
	char buf[BUFSIZ];
    ssize_t bytesRead;

    int sourceHandle = open(source.c_str(), O_RDONLY, 0);
	if (sourceHandle<0)
		return false;

    int destHandle = open(destination.c_str(), O_WRONLY | O_CREAT | O_TRUNC, 0644);
	if (destHandle<0)
	{
		close(sourceHandle);
		return false;
	}

    while ((bytesRead = read(sourceHandle, buf, BUFSIZ)) > 0)
	{
        ssize_t bytesWritten = write(destHandle, buf, bytesRead);
		if(bytesRead != bytesWritten)
		{
			close(sourceHandle);
			close(destHandle);
			return false;
		}
    }

    close(sourceHandle);
    close(destHandle);

	return true;
}

std::string IPosix::FsFileReadText(const std::string& path)
{
	std::ifstream f(path);
	if (!f) return "";
	std::stringstream buffer;
	buffer << f.rdbuf();
	return buffer.str();
}

std::vector<char> IPosix::FsFileReadBytes(const std::string& path)
{
	// Note: This don't work with a /proc/x/cmdline

	std::ifstream ifs(path.c_str(), std::ios::binary | std::ios::ate);
	std::ifstream::pos_type pos = ifs.tellg();

	std::vector<char>  result(pos);

	ifs.seekg(0, std::ios::beg);
	ifs.read(&result[0], pos);

	return result;
}

std::vector<std::string> IPosix::FsFilesInPath(const std::string& path)
{
	std::vector<std::string> result;

	DIR* dirp = opendir(path.c_str());
	if (dirp == NULL)
		return result;
	struct dirent* dp;
	while ((dp = readdir(dirp)) != NULL) {
		std::string filename = dp->d_name;
		if (filename == ".") continue;
		if (filename == "..") continue;
		result.push_back(filename);
	}
	closedir(dirp);
	return result;
}

std::string IPosix::FsGetTempPath()
{
	// Same env sequence performed by boost tmpdir
	// First version are the same app dir (elevated is always run with root, so no permission issues).
	// But on macOS, an App in Downloads start with AppTranslocation, and file written here are not accessible (for example pf config).

	const char* path;
	path = getenv("TMP");
	if (path != NULL)
		return path;

	path = getenv("TMPDIR");
	if (path != NULL)
		return path;

	path = getenv("TEMP");
	if (path != NULL)
		return path;

	if (FsFileExists("/tmp"))
		return "/tmp";

	return GetProcessPathCurrentDir();
}

std::vector<std::string> IPosix::FsGetEnvPath()
{
	const char* envPath = getenv("PATH");
	std::vector<std::string> paths = StringToVector((envPath != NULL) ? envPath : "", ':', false);

	// Cron and some elevation helpers omit the sbin dirs where tools like modprobe live.
	const char* fallbackDirs[] = { "/sbin", "/usr/sbin", "/usr/local/sbin", "/root/bin" };
	for (size_t i = 0; i < sizeof(fallbackDirs) / sizeof(fallbackDirs[0]); ++i)
	{
		if (std::find(paths.begin(), paths.end(), fallbackDirs[i]) == paths.end())
			paths.push_back(fallbackDirs[i]);
	}

	return paths;
}

std::string IPosix::FsGetRealPath(std::string path)
{
	char resolvedPath[PATH_MAX];
	char* resolvedPathResult = realpath(path.c_str(), resolvedPath);
	if (resolvedPathResult == NULL)
		return "";
	else
		return std::string(resolvedPathResult);
}

bool IPosix::FsFileIsExecutable(std::string path)
{	
	struct stat sb;
	if (stat(path.c_str(), &sb) == 0 && S_ISREG(sb.st_mode) && (sb.st_mode & S_IXUSR))
		return true;
	else
		return false;
}

bool IPosix::FsFileEnsureRootOnly(std::string path)
{
	if (chown(path.c_str(), 0, 0) == -1)
		return false;
	if (chmod(path.c_str(), 0600) == -1)
		return false;
	return true;
}

bool IPosix::FsFileMakeRunnable(const std::string& path)
{
	if (chown(path.c_str(), 0, 0) == -1)
		return false;
	if (chmod(path.c_str(), 0700) == -1)
		return false;
	return true;
}

bool IPosix::FsFileIsRootOnly(std::string path)
{
	struct stat st;
	memset(&st, 0, sizeof(struct stat));
	if (stat(path.c_str(), &st) != 0)
	{
		return false; // Not found
	}
	else
	{
		if (st.st_uid != 0)
		{
			return false; // Not owned by root
		}
		else if ((st.st_mode & S_ISUID) == 0) // If not SUID
		{
			if ((st.st_mode & S_IWGRP) != 0)
			{
				return false;; // Writable by group
			}

			if ((st.st_mode & S_IWOTH) != 0)
			{
				return false;; // Writable by other
			}
		}
	}

	return true;
}

bool IPosix::FsDirectoryEnsureRootOnly(std::string path)
{
	if (chown(path.c_str(), 0, 0) == -1)
		return false;
	if (chmod(path.c_str(), 0700) == -1)
		return false;
	return true;
}

bool IPosix::FsDirectoryIsRootOnly(std::string path)
{
	struct stat st;
	memset(&st, 0, sizeof(struct stat));
	if (stat(path.c_str(), &st) != 0)
	{
		return false; // Not found
	}

	if (S_ISDIR(st.st_mode) == 0)
	{
		return false; // Not a directory
	}

	if (st.st_uid != 0)
	{
		return false; // Not owned by root
	}

	if ((st.st_mode & S_IWGRP) != 0)
	{
		return false; // Writable by group
	}

	if ((st.st_mode & S_IWOTH) != 0)
	{
		return false; // Writable by other
	}

	return true;
}

bool IPosix::SocketIsValid(HSOCKET s)
{
	return (s > 0);
}

void IPosix::SocketBlockMode(HSOCKET s, bool block)
{
	int result = -1;
	if (block == false)
		result = fcntl(s, F_SETFL, fcntl(s, F_GETFL, 0) | O_NONBLOCK);
	else
		result = fcntl(s, F_SETFL, fcntl(s, F_GETFL, 0) & ~O_NONBLOCK);
	if (result == -1) {
		ThrowException("Error on fcntl socket");
	}
}

void IPosix::SocketClose(HSOCKET s)
{
	shutdown(s, 2);
	close(s);
}

int IPosix::SocketGetLastErrorCode()
{
	return errno;
}

// --------------------------
// Virtual Pure, IPC transport
// --------------------------

void IPosix::TransportListen()
{
	// Socket lives in a root-owned runtime directory (created 0755 by root): other users can
	// traverse it but cannot create files there, which removes the /tmp squatting/race surface.
	// Mode 0666 lets any local user connect by design (shared service): client authentication is
	// enforced by the integrity gate in Main(), not by the socket mode or uid.
	std::string sockDir = GetIpcRuntimeDir();
	if (FsDirectoryCreate(sockDir) == false)
		ThrowException("Error on creating IPC directory " + sockDir);

	// Endpoint keyed by launch mode ("spot"/"service").
	m_ipcSockPath = sockDir + "/eddie-elevated-" + GetLaunchMode() + ".sock";
	struct sockaddr_un addrServer;

	m_ipcServer = socket(AF_UNIX, SOCK_STREAM, 0);

	if (SocketIsValid(m_ipcServer) == false)
	{
		ThrowException("Error on opening socket");
	}

	std::memset(&addrServer, 0, sizeof(addrServer));

	addrServer.sun_family = AF_UNIX;
	std::strncpy(addrServer.sun_path, m_ipcSockPath.c_str(), sizeof(addrServer.sun_path) - 1);

	unlink(m_ipcSockPath.c_str()); // remove a stale socket from a previous run

	LogDevDebug("Listening on unix socket " + m_ipcSockPath);

	if (bind(m_ipcServer, (struct sockaddr*)&addrServer, sizeof(addrServer)) != 0) {
		ThrowException("Error on binding socket (" + std::to_string(SocketGetLastErrorCode()) + ")");
	}

	chmod(m_ipcSockPath.c_str(), 0666); // any local user may connect (shared service); authenticated by the integrity gate, not the socket mode

	if (listen(m_ipcServer, 1) != 0) {
		ThrowException("Error on listen socket (" + std::to_string(SocketGetLastErrorCode()) + ")");
	}

	SocketBlockMode(m_ipcServer, false);
}

bool IPosix::TransportAccept()
{
	// Spot mode is a one-shot elevation: if the authorized client never connects (it gave up on a stale
	// socket, crashed, or the prompt was dismissed), do not wait forever. Giving up lets Main() close and
	// unlink the endpoint, so no elevated process is left orphaned and no stale socket file remains.
	unsigned long acceptStartTime = GetTimestampUnix();
	const unsigned long spotAcceptTimeoutSeconds = 5;

	for (;;)
	{
		// Peer address not requested: the client is identified by kernel credentials, not by address.
		m_ipcClient = accept(m_ipcServer, NULL, NULL);

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

	// Remove if nonblock is inherit
	SocketBlockMode(m_ipcClient, true);

	return true;
}

int IPosix::TransportGetClientProcessId()
{
	// Peer pid from kernel-attested socket credentials.
	return GetSocketPeerPid(m_ipcClient);
}

int IPosix::TransportRead(char* buffer, int maxLen)
{
	return recv(m_ipcClient, buffer, maxLen, 0);
}

int IPosix::TransportWrite(const char* buffer, int len)
{
	return send(m_ipcClient, buffer, len, 0);
}

void IPosix::TransportClientClose()
{
	SocketClose(m_ipcClient);
	m_ipcClient = 0;
}

void IPosix::TransportServerClose()
{
	SocketClose(m_ipcServer);
	unlink(m_ipcSockPath.c_str());
}

std::string IPosix::GetIpcRuntimeDir()
{
	return "/run/eddie-vpn";
}

std::string IPosix::GetDevLogPath()
{
	return "/tmp/eddie-elevated.log";
}

std::string IPosix::GetPrivilegedDataDir()
{
	return "/run/eddie-vpn";
}

std::string IPosix::StringEnsureInterfaceName(const std::string& str)
{
	std::string r = StringPruneCharsNotIn(str, "._-0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ");
	if (r.length() > 15) r = r.substr(0, 15);
	return r;
}

bool IPosix::SystemWideDataSet(const std::string& key, const std::string& value)
{
	std::string path = SystemWideDataPath();
	json j;

	std::ifstream inFile(path);
	if (inFile.is_open()) 
	{
		try 
		{
			inFile >> j;
		}
		catch (const json::parse_error&) 
		{
			j = json::object();
		}
		inFile.close();
	}

	j[key] = value;

	std::ofstream outFile(path);
	if (outFile.is_open()) 
	{
		try 
		{
			outFile << j.dump(4);

			if (FsFileEnsureRootOnly(path) == false)
				return false;
		}
		catch (...) 
		{
			outFile.close();
			return false;
		}
		outFile.close();

		return true;
	}
	else 
	{
		return false;
	}
}

std::string IPosix::SystemWideDataGet(const std::string& key, const std::string& def)
{
	std::string path = SystemWideDataPath();

	if (FsFileIsRootOnly(path) == false)
		return "";

	json j;

	std::ifstream inFile(path);
	if (inFile.is_open()) 
	{
		try 
		{
			inFile >> j;
		}
		catch (const json::parse_error&) 
		{
			inFile.close();
			return def;
		}
		inFile.close();
	}
	else 
	{
		return def;
	}

	if (j.contains(key)) 
	{
		return j[key].get<std::string>();
	}
	else 
	{
		return def;
	}
}

bool IPosix::SystemWideDataDel(const std::string& key)
{
	std::string path = SystemWideDataPath();
	json j;

	std::ifstream inFile(path);
	if (inFile.is_open()) 
	{
		try 
		{
			inFile >> j;

			if (FsFileEnsureRootOnly(path) == false)
				return false;
		}
		catch (const json::parse_error&) 
		{
			inFile.close();
			return false;
		}
		inFile.close();
	}
	else 
	{
		return false;
	}

	if (j.contains(key)) 
	{
		j.erase(key);

		if(j.empty())
		{
			FsFileDelete(path);
			return true;
		}
		else
		{
			std::ofstream outFile(path);
			if (outFile.is_open()) 
			{
				try 
				{
					outFile << j.dump(4);
				}
				catch (...) 
				{
					outFile.close();
					return false;
				}
				outFile.close();
				return true;
			}
			else 
			{
				return false;
			}
		}
	}
	else 
	{
		return false;
	}
}

bool IPosix::SystemWideDataClean()
{
	return FsFileDelete(SystemWideDataPath());
}

std::string IPosix::CheckExecutablePathPermissions(const std::string& path)
{
	std::string issues = "";

	if (FsFileExists(path) == false)
		issues += "Not found;";
	else
	{
		struct stat st;
		memset(&st, 0, sizeof(struct stat));
		if (stat(path.c_str(), &st) != 0)
		{
			issues += "Not readable;";
		}
		else
		{
			if (st.st_uid != 0)
			{
				issues += "Not owned by root;";
			}
			else if ((st.st_mode & S_ISUID) == 0)
			{
				if ((st.st_mode & S_IXUSR) == 0)
				{
					issues += "Not executable by owner;";
				}

				if ((st.st_mode & S_IWGRP) != 0)
				{
					issues += "Writable by group";
				}

				if ((st.st_mode & S_IWOTH) != 0)
				{
					issues += "Writable by other;";
				}
			}
		}
	}

	return issues;
}
