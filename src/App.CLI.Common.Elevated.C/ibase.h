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
#include <thread>
#include <mutex>
#include <string>
#include <map>
#include <vector>

class ShellResult
{
    public:

        std::string out;
        std::string err;
        int exit;

        std::string output();
};

class IBase
{
private:
    int m_versionElevated = 1373;

	std::string m_session_key;
	std::mutex m_mutex_inout;
    int m_sockClient;
    bool m_debug = false;
    time_t m_lastModified;
    bool m_serviceMode = false;

public:
	virtual int AppMain(int argc, char* argv[]);

private:

	// Internal method
public:
	void MainDo(const std::string& commandId, const std::string& command, std::map<std::string, std::string>& params);

protected:
    bool IsServiceMode();
    void LogFatal(const std::string& msg);
    void LogRemote(const std::string& msg);
	void LogLocal(const std::string& msg);
    void LogDebug(const std::string& msg);
	void ReplyPID(int pid);
	void ReplyCommand(const std::string& commandId, const std::string& data);

	// Utils
	bool FileExists(const std::string& path);
	void FileDelete(const std::string& path);
	bool FileMove(const std::string& source, const std::string& destination);
	bool FileWriteText(const std::string& path, const std::string& body);
    bool FileAppendText(const std::string& path, const std::string& body);
	std::string FileReadText(const std::string& path);
    std::vector<std::string> FilesInPath(const std::string& path);
    std::string FileSHA256Sum(const std::string& path);
    int Shell(const std::string& path, const std::vector<std::string>& args, const bool stdinWrite, const std::string& stdinBody, std::string& stdOut, std::string& stdErr);
	std::string LocateExecutable(const std::string& name);
	std::string CheckValidOpenVpnConfig(const std::string& path);

	// Utils string
	std::string StringReplaceAll(const std::string& str, const std::string& from, const std::string& to);
    std::string StringExtractBetween(const std::string& str, const std::string& from, const std::string& to);
	std::vector<std::string> StringToVector(const std::string& s, const char c, bool autoTrim = true);
    std::string StringFromVector(const std::vector<std::string>& v, const std::string& delimiter);
	bool StringStartsWith(const std::string& s, const std::string& f);
    bool StringEndsWith(const std::string& s, const std::string& f);
	bool StringContain(const std::string& s, const std::string& f);
    bool StringVectorsEqual(const std::vector<std::string>& v1, const std::vector<std::string>& v2);
    bool StringVectorsEqualOrdered(const std::vector<std::string>& v1, const std::vector<std::string>& v2);
	std::string StringTrim(const std::string& s);
	std::string StringToLower(const std::string& s);
	std::string StringPruneCharsNotIn(const std::string& str, const std::string& allowed);
	std::string StringEnsureSecure(const std::string& str);
	std::string StringEnsureCidr(const std::string& str);
	std::string StringEnsureIpAddress(const std::string& str);
	std::string StringEnsureNumericInt(const std::string& str);
	std::string StringBase64Encode(const std::string& str);
	std::string StringBase64Decode(const std::string& str);
	std::string StringXmlEncode(const std::string& str);

	// Helper
    void ThrowException(const std::string& path);
    ShellResult ShellEx(const std::string& path, const std::vector<std::string>& args);
    ShellResult ShellEx1(const std::string& path, const std::string& arg1);
    ShellResult ShellEx2(const std::string& path, const std::string& arg1, const std::string& arg2);
    ShellResult ShellEx3(const std::string& path, const std::string& arg1, const std::string& arg2, const std::string& arg3);
    
	// Virtual
protected:
    virtual int Main(int argc, char* argv[]);
    virtual void Idle();
	virtual void Do(const std::string& id, const std::string& command, std::map<std::string, std::string>& params);
    virtual std::string CheckIfClientPathIsAllowed(const std::string& path);
    virtual void CheckIfExecutableIsAllowed(const std::string& path);
    
    virtual void Sleep(int ms) = 0;
    virtual int GetProcessIdMatchingIPEndPoints(std::string sourceAddr, int sourcePort, std::string destAddr, int destPort) = 0;
    virtual std::string GetProcessPathCurrent();
    virtual std::string GetProcessPathCurrentDir();
    virtual time_t GetProcessModTimeStart();
    virtual time_t GetProcessModTimeCurrent();
    virtual std::string GetProcessPathOfID(pid_t pid);
    virtual std::string GetTempPath(const std::string& filename);

	// Private
private:
	void ReplyException(const std::string& id, const std::string& message);
	void EndCommand(const std::string& id);
	void SendMessage(const std::string& message);
};

void ThreadCommand(IBase* impl, const std::string id, const std::string command, std::map<std::string, std::string> params);