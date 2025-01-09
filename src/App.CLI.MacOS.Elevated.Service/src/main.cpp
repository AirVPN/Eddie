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

#include <fstream>
#include <sstream>
#include <iostream>
#include <string>
#include <vector>
#include <algorithm>

#include <dirent.h>
#include <unistd.h>
#include <sys/stat.h>
#include <sys/types.h>
#include <sys/wait.h>

#include "../../../dependencies/sha256/sha256.h"

#include "../../../dependencies/NlohmannJSON/json.hpp"
using json = nlohmann::json;

std::string serviceName = "eddie-elevated";
std::string serviceDesc = "Eddie Elevation";
std::string serviceLauncherPath = "/Library/PrivilegedHelperTools/website.eddie.Helper";
std::string launchdPath = "/Library/LaunchDaemons/org.airvpn.eddie.ui.elevated.plist";

const std::string FsPathSeparator = "/";

std::string StringToLower(const std::string& s)
{
	std::string result = s;
	std::transform(result.begin(), result.end(), result.begin(), ::tolower);
	return result;
}

bool StringEndsWith(const std::string& s, const std::string& f)
{
	return(s.size() >= f.size() && s.compare(s.size() - f.size(), f.size(), f) == 0);
}

bool StringContain(const std::string& s, const std::string& f)
{
	return (s.find(f) != std::string::npos);
}

bool FsFileExists(const std::string& path)
{
	struct stat db;
	return (stat(path.c_str(), &db) == 0);
}

bool FsFileIsExecutable(std::string path)
{
	struct stat sb;
	if (stat(path.c_str(), &sb) == 0 && S_ISREG(sb.st_mode) && (sb.st_mode & S_IXUSR))
		return true;
	else
		return false;
}

std::string FsFileGetDirectory(const std::string& path)
{
	return path.substr(0, path.find_last_of("/\\"));
}

std::vector<std::string> FsFilesInPath(const std::string& path)
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

std::string FsFileSHA256Sum(const std::string& path)
{
	std::ifstream ifs(path, std::ios::binary | std::ios::ate);
	std::ifstream::pos_type pos = ifs.tellg();

	std::vector<char> buf((const unsigned int)pos);

	ifs.seekg(0, std::ios::beg);
	if (pos > 0)
		ifs.read(&buf[0], pos);

	sha256_context ctx;
	sha256_starts(&ctx);
	sha256_update(&ctx, (unsigned char*)buf.data(), (unsigned long)buf.size());
	unsigned char sha256sum[32];
	sha256_finish(&ctx, sha256sum);

	const char hex_chars[] = "0123456789abcdef";
	std::string result;
	for (size_t i = 0; i < sizeof(sha256sum); ++i) {
		result.push_back(hex_chars[(sha256sum[i] >> 4) & 0x0F]);
		result.push_back(hex_chars[sha256sum[i] & 0x0F]);
	}

	return result;
}

bool FsFileIsRootOnly(std::string path)
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

std::string SystemWideDataPath()
{
	return "/Library/PrivilegedHelperTools/website.eddie.Helper.dat";
}

std::string SystemWideDataGet(const std::string& key, const std::string& def)
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

std::string IntegrityCheckRead(const std::string mode)
{
	return SystemWideDataGet("integrity_" + mode, "");
}

std::string IntegrityCheckBuild(const std::string elevatedPath)
{
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

		if (include)
		{
			std::string sha256 = FsFileSHA256Sum(filePathFull);
			output += sha256 + ";" + filePathFull + "\n";
		}
	}
	return output;
}

void execCommand(const char *const argv[])
{
	pid_t pid = fork();
	if(pid == 0)
		execv(argv[0], (char* const*)argv);
	else
		waitpid(pid, NULL, 0);
}

void cleanService()
{
	// If something goes wrong, make a best-effort attempt to remove the service.
	// For example, if a portable version has been removed without uninstalling the service	

	if (access(launchdPath.c_str(), F_OK) == 0)
	{								
		execCommand((const char *const[]){"/bin/launchctl", "unload", "/Library/LaunchDaemons/org.airvpn.eddie.ui.elevated.plist", NULL});		
		remove(launchdPath.c_str());
	}

	if (access(serviceLauncherPath.c_str(), F_OK) == 0)
	{
		remove(serviceLauncherPath.c_str());
	}

	if (access(SystemWideDataPath().c_str(), F_OK) == 0)
	{
		remove(SystemWideDataPath().c_str());
	}
}

int main(int argc, char* argv[])
{
	std::string messageNotAllowed = "This application can't be run directly, it's used internally by Eddie.";

	try
	{
		if (argc<2)
			throw std::runtime_error(messageNotAllowed);
		
		uid_t euid = geteuid();
		if (euid != 0)
			throw std::runtime_error(messageNotAllowed + " (not root)");

		std::string elevatedPath = argv[1];

		std::string securityHashesStored = IntegrityCheckRead("service");
		std::string securityHashesComputed = IntegrityCheckBuild(elevatedPath);
		
		if (securityHashesStored != securityHashesComputed)
		{
			cleanService();
			throw std::runtime_error(messageNotAllowed + " (check fail)");
		}

		std::string argService = "mode=service";
		std::vector<char*> execArgs;
		execArgs.push_back(argv[1]); // elevatedPath
		execArgs.push_back(&argService[0]); // "mode=service"		
		for (int i = 2; i < argc; ++i) {
			execArgs.push_back(argv[i]);
		}
		execArgs.push_back(nullptr);

		pid_t pid = fork();
		if (pid == 0)
		{
			execvp(execArgs[0], execArgs.data());
			return 1;
		}
		else if (pid > 0) 
		{
			int status;
			waitpid(pid, &status, 0);			
			
			return WIFEXITED(status);
		} 
		else 
		{
			cleanService();
			throw std::runtime_error(messageNotAllowed + " (unexpected)");
		}

		return 1;
	}
	catch(const std::exception& e)
	{
		std::cout << e.what() << std::endl;

		return 1;
	}
}

