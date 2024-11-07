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

#include "ibase.h"

#include <netinet/in.h>

class IPosix : public IBase
{
protected:

	virtual void Idle();
	virtual void Do(const std::string& id, const std::string& command, std::map<std::string, std::string>& params);

	// Virtual
protected:
	virtual void AddTorCookiePaths(const std::string& torPath, const std::string& username, std::vector<std::string>& result);

	// Virtual Pure, OS
protected:
	virtual bool IsRoot();
	virtual void Sleep(int ms);
	virtual uint64_t GetTimestampUnixUsec();
	virtual pid_t GetCurrentProcessId();
	virtual pid_t GetParentProcessId();
	virtual pid_t GetParentProcessId(pid_t pid);
	virtual pid_t GetProcessIdOfName(const std::string& name);
	virtual void KillProcess(const std::string& signal, pid_t pid);
	virtual std::string GetCmdlineOfProcessId(pid_t pid);
	virtual std::string GetWorkingDirOfProcessId(pid_t pid);
	virtual void SetEnv(const std::string& name, const std::string& value);
	virtual int ExecRaw(const std::string& path, const std::vector<std::string>& args, const bool stdinWrite, const std::string& stdinBody, std::string& stdOut, std::string& stdErr, const bool log = true);
	virtual bool FsDirectoryCreate(const std::string& path);
	virtual bool FsFileExists(const std::string& path);
	virtual bool FsDirectoryExists(const std::string& path);
	virtual bool FsFileDelete(const std::string& path);
	virtual bool FsDirectoryDelete(const std::string& path, bool recursive);	
	virtual bool FsFileMove(const std::string& source, const std::string& destination);
	virtual bool FsFileCopy(const std::string& source, const std::string& destination);
	virtual std::string FsFileReadText(const std::string& path);
	virtual std::vector<char> FsFileReadBytes(const std::string& path);
	virtual std::vector<std::string> FsFilesInPath(const std::string& path);
	virtual std::string FsGetTempPath();
	virtual std::vector<std::string> FsGetEnvPath();
	virtual std::string FsGetRealPath(std::string path);
	virtual bool FsFileIsExecutable(std::string path);
	virtual bool FsFileEnsureRootOnly(std::string path);
	virtual bool FsFileIsRootOnly(std::string path);
	virtual bool SocketIsValid(HSOCKET s);
	virtual void SocketMarkReuseAddr(HSOCKET s);
	virtual void SocketBlockMode(HSOCKET s, bool block);
	virtual void SocketClose(HSOCKET s);
	virtual int SocketGetLastErrorCode();

	// Virtual Pure, Other
protected:
	virtual bool SystemWideDataSet(const std::string& key, const std::string& value);
	virtual std::string SystemWideDataGet(const std::string& key, const std::string& def);
	virtual bool SystemWideDataDel(const std::string& key);
	virtual bool SystemWideDataClean();
	virtual std::string SystemWideDataPath() = 0;
	virtual bool CheckIfExecutableIsAllowed(const std::string& path, const bool& throwException, const bool ignoreKnown = false);
	virtual int GetProcessIdMatchingIPEndPoints(struct sockaddr_in& addrClient, struct sockaddr_in& addrServer);

	// Utils filesystem
protected:

};
