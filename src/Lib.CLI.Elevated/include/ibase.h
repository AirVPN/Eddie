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

#include <iostream> 
#include <string>
#include <map>
#include <mutex>
#include <thread>
#include <vector>

// Platform specific
// Note: path are always std::string utf8. In Windows, are converted to wstring when need.
#if defined(WIN32) || defined(_WIN32) || defined(__WIN32__) || defined(__NT__)
#define EDDIE_PLATFORM_WINDOWS
#include "Windows.h"
typedef long pid_t;
typedef unsigned int uint;
typedef unsigned long ulong;
typedef int socklen_t;
const std::string FsPathSeparator = "\\";
const std::string FsEndLine = "\r\n";
#else
typedef int HSOCKET;
const std::string FsPathSeparator = "/";
const std::string FsEndLine = "\n";
#endif

#define NETBUFSIZE 256*256*10 // Maximum size of a single IPC command/buffer

#include "ping.h"

class IBase;

class Pinger : public PingEngine
{
protected:
	virtual void OnResponse(const uint16_t& id, const int& result);

public:
	IBase* m_pBase = NULL;
	std::string m_commandId;
};

class ExecResult
{
public:

	std::string path;
	std::vector<std::string> args;
	std::string out;
	std::string err;
	int exit = 1;
};

class IBase
{
private:
	std::string m_elevatedVersion = "v1378";

	std::string m_session_key;
	std::mutex m_mutex_inout;
	bool m_clientConnected = false;
	bool m_debug = false;
	time_t m_lastModified = 0;
	std::map<pid_t, bool> m_pidManaged;
	std::string m_launchMode;
	int m_spotClientPid = 0; // spot mode: PID of the authorized client, bound at launch (spot_client_pid arg)
	bool m_singleConnMode = false;
	Pinger m_pinger;
	std::string m_stagingDir;
	std::string m_runtimeDir;
	std::string m_stateDir;

protected:
	std::map<std::string, std::string> m_cmdline;
	std::map<std::string, std::string> m_keypair;

	// Engine
public:
	int AppMain(int argc, char* argv[]);
	virtual int AppMain(const std::vector<std::string>& args);
	void MainDo(const std::string& commandId, const std::string& command, std::map<std::string, std::string>& params);

	// Engine, Protected
protected:
	std::string GetLaunchMode();
	void SetLaunchMode(const std::string& mode);
	int GetSpotClientPid();
	void LogFatal(const std::string& msg);
	void LogRemote(const std::string& msg);
	void LogLocal(const std::string& msg);
	void LogDebug(const std::string& msg);
	void LogDevDebug(const std::string& msg);
	void ReplyPID(int pid);

	// Engine, Public
public:
	void ReplyCommand(const std::string& commandId, const std::string& data);

	// Engine, Private
private:
	void ReplyException(const std::string& id, const std::string& message);
	void EndCommand(const std::string& id);
	void SendMessage(const std::string& message);
	void EnsureRuntimeDirReady();
	void EnsureStateDirReady();
	void StagingCleanup();
	void RuntimeCleanup();
	void StateCleanup();

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
	virtual bool FullUninstall();

	virtual bool IntegrityCheckUpdate(const std::string mode);
	virtual void IntegrityCheckClean(const std::string mode);
	virtual std::string IntegrityCheckRead(const std::string mode);
	virtual std::string IntegrityCheckBuild();
	virtual bool IntegrityCheckFileKnown(const std::string& path);

	// Executable security
	bool IsRunnableExecutable(const std::string& path);
	bool IsSecureServiceLocation(const std::string& path);
	bool CanUseDirectPath(const std::string& path);

	// Root-only machine-wide data directory (stage/, run/, state/ are subdirs).
	// OS-specific: /var/lib/eddie-vpn (Linux), /var/run/eddie-vpn (macOS), C:\ProgramData\Eddie-VPN (Windows).
	bool StagingPrepare();
	void PrivilegedDataCleanup();

	std::string GetStagingDir();

	// Dedicated root-only directory for persistent privileged data (e.g. network lock rule
	// backups). Distinct from run/ (purged on startup) and from the system temp. Created and
	// secured root-only; contents survive elevated restarts until explicitly removed.
	std::string GetStateDir();
	bool StatePrepare();

	// Dedicated root-only directory for transient privileged files (e.g. the OpenVPN config
	// materialized from the body received over IPC). Distinct from the system temp (world-writable)
	// and from the install directory. Created and secured root-only; purged on startup and removed
	// on shutdown so nothing survives a connection or a crash.
	std::string GetRuntimeDir();
	bool RuntimePrepare();

	// Transient files in the dedicated root-only runtime directory (see GetRuntimeDir).
	// Predictable names are safe: unprivileged users cannot access a root-only directory.
	std::string FsRuntimeTempPath(const std::string& filename);
	std::string FsWriteRootOnlyRuntimeFile(const std::string& filename, const std::string& body);

	std::string FsStatePath(const std::string& filename);
	std::string FsWriteRootOnlyStateFile(const std::string& filename, const std::string& body);

	virtual std::string GetProcessPathCurrent();
	virtual std::string GetProcessPathCurrentDir();
	virtual time_t GetProcessModTimeStart();
	virtual time_t GetProcessModTimeCurrent();
	
	virtual std::vector<std::string> GetNetworkInterfacesNames();	

	virtual std::string GetTempPath(const std::string& filename);
	virtual void AddTorCookiePaths(const std::string& torPath, const std::string& username, std::vector<std::string>& result);

	// Virtual Pure, OS
protected:
	virtual bool IsRoot() = 0;
	virtual void Sleep(int ms) = 0;
	virtual uint64_t GetTimestampUnixUsec() = 0;
	virtual std::string GetDevLogPath() = 0; // Debug-only developer log file path

	virtual pid_t GetCurrentProcessId() = 0;
	virtual pid_t GetParentProcessId() = 0;
	virtual pid_t GetParentProcessId(pid_t pid) = 0;
	virtual std::string GetProcessPathOfId(pid_t pid) = 0;
	virtual pid_t GetProcessIdOfName(const std::string& name) = 0;
	virtual void KillProcess(const std::string& signal, pid_t pid) = 0;
	virtual std::string GetCmdlineOfProcessId(pid_t pid) = 0;
	virtual std::string GetWorkingDirOfProcessId(pid_t pid) = 0;
	virtual void SetEnv(const std::string& name, const std::string& value) = 0;
	virtual int ExecRaw(const std::string& path, const std::vector<std::string>& args, const bool stdinWrite, const std::string& stdinBody, std::string& stdOut, std::string& stdErr, const bool log) = 0;
	virtual bool FsDirectoryCreate(const std::string& path) = 0;
	virtual bool FsFileExists(const std::string& path) = 0;
	virtual bool FsDirectoryExists(const std::string& path) = 0;
	virtual bool FsFileDelete(const std::string& path) = 0;
	virtual bool FsDirectoryDelete(const std::string& path, bool recursive) = 0;	
	virtual bool FsFileMove(const std::string& source, const std::string& destination) = 0;
	virtual bool FsFileCopy(const std::string& source, const std::string& destination) = 0;	
	virtual std::string FsFileReadText(const std::string& path) = 0;
	virtual std::vector<char> FsFileReadBytes(const std::string& path) = 0;
	virtual std::vector<std::string> FsFilesInPath(const std::string& path) = 0;
	virtual std::string FsGetTempPath() = 0;
	virtual std::string GetPrivilegedDataDir() = 0;
	virtual std::vector<std::string> FsGetEnvPath() = 0;
	virtual std::string FsGetRealPath(std::string path) = 0;
	virtual bool FsFileIsExecutable(std::string path) = 0;
	virtual bool FsFileEnsureRootOnly(std::string path) = 0;
	virtual bool FsFileMakeRunnable(const std::string& path) = 0; // root-owned + executable (no-op where not applicable)
	virtual bool FsFileIsRootOnly(std::string path) = 0;
	virtual bool FsDirectoryEnsureRootOnly(std::string path) = 0;
	virtual bool FsDirectoryIsRootOnly(std::string path) = 0;

	// IPC transport (AF_UNIX on POSIX, named pipe on Windows). Implemented per-OS so Main() stays transport-agnostic.
	virtual void TransportListen() = 0;                           // create+bind+listen; throws on failure
	virtual bool TransportAccept() = 0;                           // block until a client connects; false if stop requested
	virtual int TransportGetClientProcessId() = 0;               // peer PID of the connected client (0 if unknown)
	virtual int TransportRead(char* buffer, int maxLen) = 0;     // >0 bytes read, 0 peer closed, <0 error
	virtual int TransportWrite(const char* buffer, int len) = 0; // bytes written, <0 error
	virtual void TransportClientClose() = 0;                     // drop the current client connection
	virtual void TransportServerClose() = 0;                     // close the listener and cleanup

	virtual std::string StringEnsureInterfaceName(const std::string& str) = 0;

	// Virtual Pure, Other
protected:
	virtual bool SystemWideDataSet(const std::string& key, const std::string& value) = 0;
	virtual std::string SystemWideDataGet(const std::string& key, const std::string& def) = 0;
	virtual bool SystemWideDataDel(const std::string& key) = 0;
	virtual bool SystemWideDataClean() = 0;
	virtual std::string CheckExecutablePathPermissions(const std::string& path) = 0;

	// Utils filesystem
protected:
	bool FsFileWriteText(const std::string& path, const std::string& body);
	bool FsFileAppendText(const std::string& path, const std::string& body);
	std::string FsFileGetExtension(const std::string& path);
	std::string FsFileGetDirectory(const std::string& path);
	std::string FsFileSHA256Sum(const std::string& path);
	std::string FsLocateExecutable(const std::string& name, const bool throwException = true, const bool includeElevatedPath = false);

	// Utils string - Maybe in some class, but we prefer to leave code much simply as possible
protected:
	std::string StringReplaceAll(const std::string& str, const std::string& from, const std::string& to);
	std::string StringExtractBetween(const std::string& str, const std::string& from, const std::string& to);
	std::vector<std::string> StringToVector(const std::string& s, const char c, bool autoTrim = true);
	std::string StringFromVector(const std::vector<std::string>& v, const std::string& delimiter);
	std::string StringFrom(const int& i);
	int StringToInt(const std::string& s);
	unsigned long StringToULong(const std::string& s);
	bool StringStartsWith(const std::string& s, const std::string& f);
	bool StringEndsWith(const std::string& s, const std::string& f);
	bool StringContain(const std::string& s, const std::string& f);
	bool StringVectorsEqual(const std::vector<std::string>& v1, const std::vector<std::string>& v2);
	bool StringVectorsEqualOrdered(const std::vector<std::string>& v1, const std::vector<std::string>& v2);
	std::string StringTrim(const std::string& s, const std::string& chars);
	std::string StringTrim(const std::string& s);
	std::string StringToLower(const std::string& s);	
	std::string StringPruneCharsNotIn(const std::string& str, const std::string& allowed);
	std::string StringDeleteLinesContain(const std::string& str, const std::string& search);
	std::string StringEnsureAlphaNumeric(const std::string& str);
	std::string StringEnsureAsciiName(const std::string& str);
	std::string StringEnsureHex(const std::string& str);
	std::string StringEnsureCidr(const std::string& str);
	std::string StringEnsureFileName(const std::string& str);
	std::string StringEnsureDirectoryName(const std::string& str);
	std::string StringEnsureIpAddress(const std::string& str);
	std::string StringEnsureNumericInt(const std::string& str);
	std::string StringEnsureQuote(const std::string& str);
	std::string StringBase64Encode(const std::string& str);
	std::string StringBase64Decode(const std::string& str);
	std::string StringXmlEncode(const std::string& str);
	std::string StringHexEncode(const unsigned char* buf, const size_t s);
	std::string StringHexEncode(const std::vector<char>& bytes);
	std::string StringSHA256(const std::string& str);
	bool StringIsIPv4(const std::string& ip);
	bool StringIsIPv6(const std::string& ip);
	std::string StringIpRemoveInterface(const std::string& ip);

	// Utils JSON (avoid library until our usage keep simple)
	std::string JsonEncode(const std::string& v);
	std::string JsonFromKeyPairs(std::map<std::string, std::string>& kp);

	// Utils other
	std::string SHA256(const unsigned char* pBuf, const unsigned long s);
	unsigned long GetTimestampUnix();
	std::map<std::string, std::string> IniConfigToMap(const std::string& ini, std::string sectionKeySeparator = ".", bool convertKeyToLower = true);
	std::map<std::string, std::string> ParseCommandLine(const std::vector<std::string>& args);
	std::string CheckValidOpenVpnConfigFile(const std::string& path);
	std::string CheckValidOpenVpnConfigContent(const std::string& body);
	std::string CheckValidHummingbirdConfigFile(const std::string& path);
	std::string CheckValidWireGuardConfig(const std::string& config);

	// Materialize a config received over IPC into the dedicated root-only runtime directory and
	// return its path. Fail-closed: throws if the runtime directory cannot be secured. The caller
	// is responsible for deleting the returned file when done.
	std::string FsWriteRootOnlyTempConfig(const std::string& prefix, const std::string& id, const std::string& ext, const std::string& body);
	void PidAdd(pid_t pid);
	void PidRemove(pid_t pid);
	bool PidManaged(pid_t pid);

	// Helper
	void ThrowException(const std::string& message);		
	ExecResult ExecEx(const std::string& path, const std::vector<std::string>& args);
	ExecResult ExecEx(const std::string& path, const std::vector<std::string>& args, bool log);
	ExecResult ExecEx(const std::string& path, const std::vector<std::string>& args, const std::string& stdinput);
	std::string GetExecResultReport(const ExecResult& result);
};





void ThreadCommand(IBase* impl, const std::string id, const std::string command, std::map<std::string, std::string> params);
