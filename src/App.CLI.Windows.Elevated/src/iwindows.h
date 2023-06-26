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

#include "../../App.CLI.Common.Elevated/ibase.h"

#include "wintun.h"

class IWindows : public IBase
{
protected:

	virtual void Do(const std::string& id, const std::string& command, std::map<std::string, std::string>& params);
	virtual bool IsServiceInstalled();
	virtual bool ServiceInstall();
	virtual bool ServiceUninstall();
	virtual bool ServiceUninstallSupportRealtime();
	virtual bool FullUninstall();

	// Virtual
protected:
	virtual void AddTorCookiePaths(const std::string& torPath, const std::string& username, std::vector<std::string>& result);

	// Virtual Pure, OS
protected:
	virtual bool IsRoot();
	virtual void Sleep(int ms);
	virtual uint64_t GetTimestampUnixUsec();
	pid_t GetCurrentProcessId();
	virtual pid_t GetParentProcessId();
	virtual pid_t GetParentProcessId(pid_t pid);
	virtual std::string GetProcessPathOfId(pid_t pid);
	virtual pid_t GetProcessIdOfName(const std::string& name);
	virtual void KillProcess(const std::string& signal, pid_t pid);
	virtual std::string GetCmdlineOfProcessId(pid_t pid);
	virtual std::string GetWorkingDirOfProcessId(pid_t pid);
	virtual void SetEnv(const std::string& name, const std::string& value);
	virtual int Exec(const std::string& path, const std::vector<std::string>& args, const bool stdinWrite, const std::string& stdinBody, std::string& stdOut, std::string& stdErr, const bool log);
	virtual bool FsDirectoryCreate(const std::string& path);
	virtual bool FsFileExists(const std::string& path);
	virtual bool FsDirectoryExists(const std::string& path);
	virtual bool FsFileDelete(const std::string& path);
	virtual bool FsDirectoryDelete(const std::string& path, bool recursive);
	virtual bool FsFileMove(const std::string& source, const std::string& destination);
	virtual std::string FsFileReadText(const std::string& path);
	virtual std::vector<char> FsFileReadBytes(const std::string& path);
	virtual std::vector<std::string> FsFilesInPath(const std::string& path);
	virtual std::string FsGetTempPath();
	virtual std::vector<std::string> FsGetEnvPath();
	virtual std::string FsGetRealPath(std::string path);
	virtual bool SocketIsValid(HSOCKET s);
	virtual void SocketMarkReuseAddr(HSOCKET s);
	virtual void SocketBlockMode(HSOCKET s, bool block);
	virtual void SocketClose(HSOCKET s);
	virtual int SocketGetLastErrorCode();

	// Virtual Pure, Other
protected:
	virtual std::string CheckIfClientPathIsAllowed(const std::string& path);
	virtual bool CheckIfExecutableIsAllowed(const std::string& path, const bool& throwException);
	virtual int GetProcessIdMatchingIPEndPoints(struct sockaddr_in& addrClient, struct sockaddr_in& addrServer);

	// Protected
protected:

	typedef struct ExecInfo {
		std::string lastError = "";
		DWORD lastErrorCode = 0;
		pid_t pid = 0;
		PROCESS_INFORMATION processInfo{};
		STARTUPINFOW startupInfo{};
		HANDLE stdoutReadHandle = NULL;
		HANDLE stderrReadHandle = NULL;
	} t_shellinfo;
		
	std::string GetLastErrorAsString();
	t_shellinfo ExecStart(const std::string& path, const std::vector<std::string>& args);
	DWORD ExecEnd(t_shellinfo info);
	void ExecCleanup(t_shellinfo info);
	int SetInterfaceMetric(const int index, const std::string layer, const int value);

	bool ServiceDelete(const std::string& id);
	bool ServiceUninstallDirect();

	// Wintun	
	HMODULE m_wintunLibrary = 0;
	bool WintunEnsureLibrary();
	DWORD WintunVersion();
	void WintunAdapterAdd(const std::wstring& pool, const std::wstring& name);
	void WintunAdapterEnsure(const std::wstring& pool, const std::wstring& name);
	void WintunAdapterRemove(const std::wstring& pool, const std::wstring& name);
	void WintunAdapterRemovePool(const std::wstring& pool);

	// Tap	
	void TapCreateInterface(const std::wstring& hwid, const std::string& name);
	void TapDeleteInterface(const std::wstring& id);

public:
	WINTUN_ADAPTER_HANDLE WintunAdapterOpen(const std::wstring& pool, const std::wstring& name);
	void WintunAdapterClose(WINTUN_ADAPTER_HANDLE hAdapter);
	void WintunAdapterRemove(WINTUN_ADAPTER_HANDLE hAdapter);

protected:

	// WireGuard
	int WireGuardTunnel(const std::string& configName);

	// Public
public:	
	static std::wstring StringUTF8ToWString(const std::string& str);
	static std::string StringWStringToUTF8(const std::wstring& str);
};
