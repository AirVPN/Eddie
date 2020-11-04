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

#include <iostream> 
#include <string>
#include <map>
#include <mutex>
#include <thread>
#include <vector>

// Platform specific
// Note: path are always std::string utf8. In Windows, are converted to wstring when need.
#if defined(WIN32) || defined(_WIN32) || defined(__WIN32__) || defined(__NT__)
#include "Windows.h"
typedef long pid_t;
typedef unsigned int uint;
typedef SOCKET HSOCKET;
typedef int socklen_t;
const std::string FsPathSeparator = "\\";
#else
typedef int HSOCKET;
const std::string FsPathSeparator = "/";
#endif

class ShellResult;

class IBase
{
private:
	std::string m_elevatedVersion = "v1375";
	int m_elevatedPortDefault = 9349;

	std::string m_session_key;
	std::mutex m_mutex_inout;
	HSOCKET m_sockClient;
	bool m_debug = false;
	time_t m_lastModified;
	//bool m_serviceMode = false;
	std::map<pid_t, bool> m_pidManaged;
	std::string m_launchMode;
	bool m_singleConnMode = false;

protected:
	std::map<std::string, std::string> m_cmdline;

	// Engine
public:
	int AppMain(int argc, char* argv[]);
	virtual int AppMain(const std::vector<std::string>& args);
	void MainDo(const std::string& commandId, const std::string& command, std::map<std::string, std::string>& params);

	// Engine, Protected
protected:
	std::string GetLaunchMode();
	void LogFatal(const std::string& msg);
	void LogRemote(const std::string& msg);
	void LogLocal(const std::string& msg);
	void LogDebug(const std::string& msg);
	void LogDevDebug(const std::string& msg);
	void ReplyPID(int pid);
	void ReplyCommand(const std::string& commandId, const std::string& data);

	// Engine, Private
private:
	void ReplyException(const std::string& id, const std::string& message);
	void EndCommand(const std::string& id);
	void SendMessage(const std::string& message);


	// Virtual
protected:
	virtual int Main();
	virtual void Idle();
	virtual void Do(const std::string& id, const std::string& command, std::map<std::string, std::string>& params);
	virtual bool IsStopRequested();
	
	virtual std::string GetServiceId();
	virtual std::string GetServiceName();
	virtual std::string GetServiceDesc();
	virtual bool IsServiceInstalled();
	virtual bool ServiceInstall();
	virtual bool ServiceUninstall();
	virtual bool ServiceReinstall();
    virtual bool ServiceUninstallSupportRealtime();

	virtual std::string GetProcessPathCurrent();
	virtual std::string GetProcessPathCurrentDir();
	virtual time_t GetProcessModTimeStart();
	virtual time_t GetProcessModTimeCurrent();

	virtual std::string GetTempPath(const std::string& filename);
	virtual void AddTorCookiePaths(const std::string& torPath, const std::string& username, std::vector<std::string>& result);

	// Virtual Pure, OS
protected:
	virtual bool IsRoot() = 0;
	virtual void Sleep(int ms) = 0;
	virtual pid_t GetCurrentProcessId() = 0;
	virtual pid_t GetParentProcessId() = 0;
	virtual pid_t GetParentProcessId(pid_t pid) = 0;
	virtual std::string GetProcessPathOfId(pid_t pid) = 0;
	virtual pid_t GetProcessIdOfName(const std::string& name) = 0;
	virtual std::string GetCmdlineOfProcessId(pid_t pid) = 0;
	virtual std::string GetWorkingDirOfProcessId(pid_t pid) = 0;
	virtual int Shell(const std::string& path, const std::vector<std::string>& args, const bool stdinWrite, const std::string& stdinBody, std::string& stdOut, std::string& stdErr) = 0;
	virtual void FsDirectoryCreate(const std::string& path) = 0;
	virtual bool FsFileExists(const std::string& path) = 0;
	virtual bool FsDirectoryExists(const std::string& path) = 0;
	virtual void FsFileDelete(const std::string& path) = 0;
	virtual void FsDirectoryDelete(const std::string& path, bool recursive) = 0;
	virtual bool FsFileMove(const std::string& source, const std::string& destination) = 0;
	virtual std::string FsFileReadText(const std::string& path) = 0;
	virtual std::vector<char> FsFileReadBytes(const std::string& path) = 0;
	virtual std::vector<std::string> FsFilesInPath(const std::string& path) = 0;
	virtual std::string FsGetTempPath() = 0;
	virtual std::vector<std::string> FsGetEnvPath() = 0;
	virtual std::string FsGetRealPath(std::string path) = 0;
	virtual bool SocketIsValid(HSOCKET s) = 0;
	virtual void SocketMarkReuseAddr(HSOCKET s) = 0;
	virtual void SocketBlockMode(HSOCKET s, bool block) = 0;
	virtual void SocketClose(HSOCKET s) = 0;

	// Virtual Pure, Other
protected:
	virtual std::string CheckIfClientPathIsAllowed(const std::string& path) = 0;
	virtual void CheckIfExecutableIsAllowed(const std::string& path) = 0;
	virtual int GetProcessIdMatchingIPEndPoints(struct sockaddr_in& addrClient, struct sockaddr_in& addrServer) = 0;

	// Utils filesystem
protected:
	bool FsFileWriteText(const std::string& path, const std::string& body);
	bool FsFileAppendText(const std::string& path, const std::string& body);
	std::string FsFileGetDirectory(const std::string& path);	

	std::string FsFileSHA256Sum(const std::string& path);
	std::string FsLocateExecutable(const std::string& name);

	// Utils string
protected:
	std::string StringReplaceAll(const std::string& str, const std::string& from, const std::string& to);
	std::string StringExtractBetween(const std::string& str, const std::string& from, const std::string& to);
	std::vector<std::string> StringToVector(const std::string& s, const char c, bool autoTrim = true);
	std::string StringFromVector(const std::vector<std::string>& v, const std::string& delimiter);
	bool StringStartsWith(const std::string& s, const std::string& f);
	bool StringEndsWith(const std::string& s, const std::string& f);
	bool StringContain(const std::string& s, const std::string& f);
	bool StringVectorsEqual(const std::vector<std::string>& v1, const std::vector<std::string>& v2);
	bool StringVectorsEqualOrdered(const std::vector<std::string>& v1, const std::vector<std::string>& v2);
	std::string StringTrim(const std::string& s, const std::string& chars);
	std::string StringTrim(const std::string& s);
	std::string StringToLower(const std::string& s);
	std::string StringPruneCharsNotIn(const std::string& str, const std::string& allowed);
	std::string StringEnsureSecure(const std::string& str);
	std::string StringEnsureCidr(const std::string& str);
	std::string StringEnsureInterfaceName(const std::string& str);
	std::string StringEnsureIpAddress(const std::string& str);
	std::string StringEnsureNumericInt(const std::string& str);
	std::string StringBase64Encode(const std::string& str);
	std::string StringBase64Decode(const std::string& str);
	std::string StringXmlEncode(const std::string& str);
	std::string StringHexEncode(const unsigned char* buf, const size_t s);
	std::string StringHexEncode(const std::vector<char>& bytes);
	std::string StringHexEncode(const int v, const int chars);
    
	// Utils other
	std::map<std::string, std::string> ParseCommandLine(const std::vector<std::string>& args);
	std::string CheckValidOpenVpnConfig(const std::string& path);
	std::string CheckValidHummingbirdConfig(const std::string& path);
	std::string ComputeIntegrityHash(const std::string& elevatedPath, const std::string& clientPath);
	bool CheckIfExecutableIsWhitelisted(const std::string& path);
	void PidAdd(pid_t pid);
	void PidRemove(pid_t pid);
	bool PidManaged(pid_t pid);

	// Helper
	void ThrowException(const std::string& message);
	ShellResult ShellEx(const std::string& path, const std::vector<std::string>& args);
	ShellResult ShellEx1(const std::string& path, const std::string& arg1);
	ShellResult ShellEx2(const std::string& path, const std::string& arg1, const std::string& arg2);
	ShellResult ShellEx3(const std::string& path, const std::string& arg1, const std::string& arg2, const std::string& arg3);
};

class ShellResult
{
public:

	std::string out;
	std::string err;
	int exit;

	std::string output();
	std::string dump();
};


void ThreadCommand(IBase* impl, const std::string id, const std::string command, std::map<std::string, std::string> params);