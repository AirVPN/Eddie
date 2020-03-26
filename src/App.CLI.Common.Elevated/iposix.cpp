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

#include "iposix.h"

#include "signal.h"

#include <cstring>
#include <dirent.h>
#include <fstream>
#include <sstream>
#include <unistd.h>
#include <sys/stat.h>

#include "pstream.h"
using namespace redi; // related to pstream.h

// --------------------------
// Virtual
// --------------------------

void IPosix::Idle()
{
	// Scenario: macOS with portable app in /Application, create launchd daemon, replace the app with a new version
	if (IsServiceMode())
	{
		time_t start = GetProcessModTimeStart();
		time_t cur = GetProcessModTimeCurrent();
		if (start != cur)
		{
			LogLocal("Executable changed/upgraded, exit");
			kill(GetCurrentProcessId(), SIGTERM);
		}
	}

	IBase::Idle();
}

void IPosix::Do(const std::string& commandId, const std::string& command, std::map<std::string, std::string>& params)
{
	if (command == "kill")
	{
		pid_t pid = std::atoi(params["pid"].c_str());
		int signal = SIGINT;
		if (params["signal"] == "sigint")
			signal = SIGINT;
		else if (params["signal"] == "sigterm")
			signal = SIGTERM;

		kill(pid, signal);
	}
	else if (command == "process_openvpn")
	{
		CheckIfExecutableIsAllowed(params["path"]);

		std::string checkResult = CheckValidOpenVpnConfig(params["config"]);
		if (checkResult != "")
		{
			ThrowException("Not supported OpenVPN config: " + checkResult);
		}
		else
		{
			const pstreams::pmode mode = pstreams::pstdout | pstreams::pstderr;
			pstreams::argv_type argv;
			argv.push_back(params["path"]);

			argv.push_back("--config");
			argv.push_back(params["config"]);

			pstream child(argv, mode);
			char buf[1024 * 32];
			std::streamsize n;

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

			child.close();
			int status = child.rdbuf()->status();
			int exitCode = -1;
			if (WIFEXITED(status))
				exitCode = WEXITSTATUS(status);

			std::string exitCodeStr = std::to_string(exitCode);
			ReplyCommand(commandId, "return:" + exitCodeStr);
		}
	}
	else if (command == "hummingbird")
	{
		CheckIfExecutableIsAllowed(params["path"]);

		// Workaround. In any case, hummingbird called from Eddie don't perform any action that need a recovery.
		if (FsFileExists("/etc/airvpn/hummingbird.lock"))
			FsFileDelete("/etc/airvpn/hummingbird.lock");

		std::string checkResult = CheckValidHummingbirdConfig(params["config"]);
		if (checkResult != "")
		{
			ThrowException("Not supported Hummingbird config: " + checkResult);
		}
		else
		{
			const pstreams::pmode mode = pstreams::pstdout | pstreams::pstderr;
			pstreams::argv_type argv;
			argv.push_back(params["path"]);

			argv.push_back(params["config"]);

			argv.push_back("--ignore-dns-push");
			argv.push_back("--network-lock");
			argv.push_back("off");

			if (params.find("gui-version") != params.end())
			{
				argv.push_back("--gui-version");
				argv.push_back(params["gui-version"]);
			}

			pstream child(argv, mode);
			char buf[1024 * 32];
			std::streamsize n;

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

			child.close();
			int status = child.rdbuf()->status();
			int exitCode = -1;
			if (WIFEXITED(status))
				exitCode = WEXITSTATUS(status);

			std::string exitCodeStr = std::to_string(exitCode);
			ReplyCommand(commandId, "return:" + exitCodeStr);
		}
	}
	else
	{
		IBase::Do(commandId, command, params);
	}
}

/* // Old shell edition
int IPosix::GetProcessIdMatchingIPEndPoints(std::string sourceAddr, int sourcePort, std::string destAddr, int destPort)
{
	std::vector<std::string> args;
	args.push_back("-F");
	args.push_back("pfn");
	args.push_back("-anPi");
	args.push_back("4tcp@" + destAddr + ":" + std::to_string(destPort));

	std::string lsofPath = LocateExecutable("lsof");
	if(lsofPath != "")
	{
		ShellResult lsResult = ShellEx("lsof", args);
		if(lsResult.exit == 0)
		{
			std::vector<std::string> lines = StringToVector(lsResult.out, '\n');
			int lastPid = 0;
			std::string needle = sourceAddr + ":" + std::to_string(sourcePort) + "->" + destAddr + ":" + std::to_string(destPort);
			for (std::vector<std::string>::const_iterator i = lines.begin(); i != lines.end(); ++i)
			{
				std::string line = StringTrim(StringToLower(*i));

				if(line.length()==0)
					continue;

				char ch = line.at(0);
				if(ch == 'p')
				{
					lastPid = strtol(line.c_str()+1,NULL,10);
				}
				else if(ch == 'n')
				{
					if(StringContain(line, needle))
						return lastPid;
				}
			}
		}
	}

	return 0;
}
*/

void IPosix::AddTorCookiePaths(const std::string& torPath, const std::string& username, std::vector<std::string>& result)
{
	if (torPath != "") // TorBrowser
	{
		std::string path = FsFileGetDirectory(torPath) + "/../Data/Tor/control_auth_cookie";
		result.push_back(path);
	}

	result.push_back("/var/run/tor/control.authcookie");
	result.push_back("/var/lib/tor/control_auth_cookie");
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
	// TOFIX - Find a method without shell
	ShellResult pidofResult = ShellEx1("pidof", name);
	if (pidofResult.exit == 0)
		return atoi(pidofResult.out.c_str());
	else
		return 0;
}

std::string IPosix::GetCmdlineOfProcessId(pid_t pid)
{
    std::string path = "/proc/" + std::to_string(pid) + "/cmdline";
    
    if(FsFileExists(path) == false)
        return "";
    
    std::string result;
    FILE *f;
    char ch;
    f = fopen(path.c_str(), "rb");
    if(f == NULL)
        return "";
    while ((ch = fgetc(f)) != EOF)
    {
        if(ch == 0)
            result += " ";
        else
            result += ch;
    }
    fclose(f);
    return result;
}

int IPosix::Shell(const std::string& path, const std::vector<std::string>& args, const bool stdinWrite, const std::string& stdinBody, std::string& stdOut, std::string& stdErr)
{
	const pstreams::pmode mode = pstreams::pstdin | pstreams::pstdout | pstreams::pstderr;
	pstreams::argv_type argv;

	LogDebug("__Shell, path: " + path);

	argv.push_back(path);
	for (std::vector<std::string>::const_iterator i = args.begin(); i != args.end(); ++i)
	{
		LogDebug("__Shell, arg: " + *i);
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

	LogDebug("__Shell, exitcode: " + std::to_string(exitCode));
	LogDebug("__Shell, stdout: " + stdOut);
	LogDebug("__Shell, stderr: " + stdErr);

	return exitCode;
}

void IPosix::FsDirectoryCreate(const std::string& path)
{
	// Not implemented, never used.
}

bool IPosix::FsFileExists(const std::string& path)
{
	struct stat db;
	return (stat(path.c_str(), &db) == 0);
}

bool IPosix::FsDirectoryExists(const std::string& path)
{
	return FsFileExists(path);
}

void IPosix::FsFileDelete(const std::string& path)
{
	if (FsFileExists(path))
		unlink(path.c_str());
}

void IPosix::FsDirectoryDelete(const std::string& path, bool recursive)
{
	// Not implemented, never used.
}

bool IPosix::FsFileMove(const std::string& source, const std::string& destination)
{
	return (rename(source.c_str(), destination.c_str()) == 0);
}

std::string IPosix::FsFileReadText(const std::string& path)
{
	std::ifstream f(path);
	if (!f) return "";
	std::stringstream buffer;
	buffer << f.rdbuf();
	return buffer.str();
}

std::vector<std::string> IPosix::FsFilesInPath(const std::string& path)
{
	std::vector<std::string> result;

	DIR* dirp = opendir(path.c_str());
	if (dirp == NULL)
		return result;
	struct dirent * dp;
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
	return StringToVector(getenv("PATH"), ':', false);
}

bool IPosix::SocketIsValid(HSOCKET s)
{
	return (s > 0);
}

void IPosix::SocketMarkReuseAddr(HSOCKET s)
{
	int option = 1;
	setsockopt(s, SOL_SOCKET, SO_REUSEADDR, &option, sizeof(option));
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

void IPosix::CheckIfExecutableIsAllowed(const std::string& path)
{
	std::string issues = "";

	if (CheckIfExecutableIsWhitelisted(path)) // If true, skip other checks.
		return;

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

	if (issues != "")
		ThrowException("Executable '" + path + "' not allowed: " + issues);
}

int IPosix::GetProcessIdMatchingIPEndPoints(struct sockaddr_in& addrClient, struct sockaddr_in& addrServer)
{
	if (FsFileExists("/proc") == false)
		return 0;

	// First, locate inode in /proc/net/tcp. After, scan /proc/*/fd/* to search the inode and identify the pid.

	int pidFound = 0;

	std::string procNetTcp = FsFileReadText("/proc/net/tcp");

	std::string sourceAddrHex = StringHexEncode(addrClient.sin_addr.s_addr, 8);
	std::string sourcePortHex = StringHexEncode(ntohs(addrClient.sin_port), 4);
	std::string destAddrHex = StringHexEncode(addrServer.sin_addr.s_addr, 8);
	std::string destPortHex = StringHexEncode(ntohs(addrServer.sin_port), 4);

	std::vector<std::string> procNetTcpLines = StringToVector(procNetTcp, '\n');
	for (std::vector<std::string>::const_iterator l = procNetTcpLines.begin(); l != procNetTcpLines.end(); ++l)
	{
		std::vector<std::string> procNetTcpFields = StringToVector(*l, ' ', true);

		if ((procNetTcpFields.size() > 9) &&
			(procNetTcpFields[1] == sourceAddrHex + ":" + sourcePortHex) &&
			(procNetTcpFields[2] == destAddrHex + ":" + destPortHex)
			)
		{
			long unsigned int inodeSearch = strtoul(procNetTcpFields[9].c_str(), NULL, 0);

			DIR *procDir = opendir("/proc");
			if (procDir != NULL)
			{
				for (;;)
				{
					struct dirent *procDirFile = readdir(procDir);
					if (procDirFile == NULL)
						break;

					std::string procDirFilePath = "/proc/" + std::string(procDirFile->d_name);
					struct stat procDirFileSt; // Don't trust dirent::d_type
					memset(&procDirFileSt, 0, sizeof(struct stat));
					if (stat(procDirFilePath.c_str(), &procDirFileSt) == 0)
					{
						if ((S_ISDIR(procDirFileSt.st_mode)) && (procDirFile->d_name[0] != '.'))
						{
							std::string procFdDirPath = "/proc/" + std::string(procDirFile->d_name) + "/fd";
							DIR *procFdDir = opendir(procFdDirPath.c_str());
							if (procFdDir != NULL)
							{
								for (;;)
								{
									struct dirent *procFdDirFile = readdir(procFdDir);
									if (procFdDirFile == NULL)
										break;

									std::string procFdDirFilePath = "/proc/" + std::string(procDirFile->d_name) + "/fd/" + std::string(procFdDirFile->d_name);
									struct stat procFdDirFileSt; // Don't trust dirent::d_type
									memset(&procFdDirFileSt, 0, sizeof(struct stat));
									if (stat(procFdDirFilePath.c_str(), &procFdDirFileSt) == 0) // Note: lstat
									{
										if ((S_ISSOCK(procFdDirFileSt.st_mode)) && (inodeSearch == procFdDirFileSt.st_ino))
										{
											if (pidFound == 0)
												pidFound = atoi(procDirFile->d_name);
											else
											{
												return 0; // Unexpected
											}
										}
									}
								}
								closedir(procFdDir);
							}
						}
					}
				}
				closedir(procDir);
			}
		}

	}

	return pidFound;
}