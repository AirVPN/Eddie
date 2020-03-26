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

#include "../../App.CLI.Common.Elevated/ibase.h"

class IWindows: public IBase
{
protected:
	
	virtual void Do(const std::string& id, const std::string& command, std::map<std::string, std::string>& params);

	// Virtual
protected:
	virtual void AddTorCookiePaths(const std::string& torPath, const std::string& username, std::vector<std::string>& result);

	// Virtual Pure, OS
protected:
	virtual bool IsRoot();
	virtual void Sleep(int ms);
	pid_t GetCurrentProcessId();
	virtual pid_t GetParentProcessId();
	virtual pid_t GetParentProcessId(pid_t pid);
	virtual std::string GetProcessPathOfId(pid_t pid);
	virtual pid_t GetProcessIdOfName(const std::string& name);
	virtual std::string GetCmdlineOfProcessId(pid_t pid);
	virtual int Shell(const std::string& path, const std::vector<std::string>& args, const bool stdinWrite, const std::string& stdinBody, std::string& stdOut, std::string& stdErr);
	virtual void FsDirectoryCreate(const std::string& path);
	virtual bool FsFileExists(const std::string& path);
	virtual bool FsDirectoryExists(const std::string& path);
	virtual void FsFileDelete(const std::string& path);
	virtual void FsDirectoryDelete(const std::string& path, bool recursive);
	virtual bool FsFileMove(const std::string& source, const std::string& destination);
	virtual std::string FsFileReadText(const std::string& path);
	virtual std::vector<std::string> FsFilesInPath(const std::string& path);
	virtual std::string FsGetTempPath();
	virtual std::vector<std::string> FsGetEnvPath();
	virtual bool SocketIsValid(HSOCKET s);
	virtual void SocketMarkReuseAddr(HSOCKET s);
	virtual void SocketBlockMode(HSOCKET s, bool block);
	virtual void SocketClose(HSOCKET s);

	// Protected
protected:

	typedef struct ShellInfo {
		std::string lastError = "";
		DWORD lastErrorCode = 0;
		pid_t pid = 0;
		PROCESS_INFORMATION processInfo;
		STARTUPINFOW startupInfo;
		HANDLE stdoutReadHandle = NULL;
		HANDLE stderrReadHandle = NULL;
	} t_shellinfo;

	std::string GetLastErrorAsString();
	t_shellinfo ShellStart(const std::string path, const std::vector<std::string> args);
	DWORD ShellEnd(t_shellinfo info);
	void ShellCleanup(t_shellinfo info);
    
	// Public
public:
    static std::wstring StringUTF8ToWString(std::string str);
	static std::string StringWStringToUTF8(std::wstring str);
};
