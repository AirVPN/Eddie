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
#include <sstream>
#include <unistd.h>
#include <sys/stat.h>

// --------------------------
// Virtual
// --------------------------

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
	else
	{
		IBase::Do(commandId, command, params);
	}
}

int IPosix::GetProcessIdMatchingIPEndPoints(struct sockaddr_in& addrClient, struct sockaddr_in& addrServer)
{
	if (FileExists("/proc") == false)
		return 0;

	// First, locate inode in /proc/net/tcp. After, scan /proc/*/fd/* to search the inode and identify the pid.

	int pidFound = 0;

	std::string procNetTcp = FileReadText("/proc/net/tcp");

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
		std::string path = FileGetDirectory(torPath) + "/../Data/Tor/control_auth_cookie";
		result.push_back(path);
	}

	result.push_back("/var/run/tor/control.authcookie");
	result.push_back("/var/lib/tor/control_auth_cookie");
}

// --------------------------
// Virtual Pure, OS
// --------------------------

void IPosix::Sleep(int ms)
{
	usleep(ms * 1000);
}

pid_t IPosix::GetParentProcessId()
{
	return getppid();
}

pid_t IPosix::GetParentProcessId(pid_t pid)
{
	// We don't find a better method
	std::string statusPath = "/proc/" + std::to_string(pid) + "/status";
	if (!FileExists(statusPath))
		return 0;

	std::string statusBody = FileReadText(statusPath);
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