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

#include <sys/stat.h>
#include <sys/types.h>

#include <algorithm>
#include <cstring>
#include <fcntl.h>
#include <fstream>
#include <iomanip>
#include <signal.h>
#include <sstream>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#if defined(WIN32) || defined(_WIN32) || defined(__WIN32__) || defined(__NT__)	
#pragma warning(disable:4996)
#else
#include <unistd.h> // read, write, close (all socket related)
#include <netdb.h> // htons, listen, sockaddr_in
#endif

#include "hashes.h"       
#include "base64.h"
#include "sha256.h"
#include "ibase.h"

#define NETBUFSIZE 256*256*10 // Not: Maximum size of command

// --------------------------
// Engine
// --------------------------

int IBase::AppMain(int argc, char* argv[])
{
	std::vector<std::string> args;
	for (int c = 0; c < argc; c++)
		args.push_back(argv[c]);
	return AppMain(args);
}


int IBase::AppMain(const std::vector<std::string>& args)
{
	m_cmdline = ParseCommandLine(args);

	try
	{
		return Main();
	}
	catch (std::exception& e)
	{
		LogLocal("Main exception: " + std::string(e.what()));
		return 1;
	}
	catch (...)
	{
		LogLocal("Main exception: Unknown");
		return 1;
	}
}

void IBase::MainDo(const std::string& commandId, const std::string& command, std::map<std::string, std::string>& params)
{
	try
	{
		if (m_session_key == "")
		{
			if (command == "session-key")
			{
				std::string clientVersion = "";
				if (params.find("version") != params.end())
					clientVersion = params["version"];

				if (clientVersion != m_elevatedVersion)
					ThrowException("Unexpected version, elevated: " + m_elevatedVersion + ", client: " + clientVersion);

				m_session_key = params["key"];
			}
			else
			{
				ReplyException(commandId, "Not init.");
			}
		}
		else if ((params.find("_token") == params.end()) || (m_session_key != params["_token"]))
		{
			ReplyException(commandId, "Not auth.");
		}
		else
		{
			m_debug = ((params.find("_debug") != params.end()) && (params["_debug"] == "1"));

			Do(commandId, command, params);
		}
	}
	catch (std::exception& e)
	{
		ReplyException(commandId, "Exception: " + std::string(e.what()));
	}
	catch (...)
	{
		ReplyException(commandId, "Internal exception.");
	}

	EndCommand(commandId);
}



// --------------------------
// Engine, Protected
// --------------------------

bool IBase::IsServiceMode()
{
	return m_serviceMode;
}

void IBase::LogFatal(const std::string& msg)
{
	LogDebug("Fatal: " + msg);
	SendMessage("ee:fatal:" + base64_encode(msg));
}

void IBase::LogRemote(const std::string& msg)
{
	LogDevDebug("Remote:" + msg);

	SendMessage("ee:log:" + base64_encode(msg));
}

void IBase::LogLocal(const std::string& msg)
{
	LogDevDebug("Local:" + msg);

	std::cout << msg << std::endl;
}

void IBase::LogDebug(const std::string& msg)
{
	LogDevDebug("Debug:" + msg);

#if defined(Debug) || defined(_DEBUG)
	std::cout << "Elevated Debug: " << msg << std::endl;
#endif

	if (m_debug)
		LogRemote("Elevated: " + msg);
}

void IBase::LogDevDebug(const std::string& msg)
{
	/*
	std::string logPath = "C:\\elevated.log";
	FILE* f = fopen(logPath.c_str(), "a");
	fprintf(f, "%s\r\n", msg.c_str());
	fclose(f);
	*/
}

void IBase::ReplyPID(int pid)
{
	SendMessage("ee:pid:" + base64_encode(std::to_string(pid)));
}

void IBase::ReplyCommand(const std::string& commandId, const std::string& data)
{
	SendMessage("ee:data:" + commandId + ":" + base64_encode(data));
}

// --------------------------
// Engine, Private
// --------------------------

void IBase::ReplyException(const std::string& commandId, const std::string& message)
{
	SendMessage("ee:exception:" + commandId + ":" + base64_encode(message));
}

void IBase::EndCommand(const std::string& commandId)
{
	SendMessage("ee:end:" + commandId);
}

void IBase::SendMessage(const std::string& message)
{
	std::string sendBuffer = message + "\n";
	m_mutex_inout.lock();
	int nWrite = send(m_sockClient, sendBuffer.c_str(), (int)sendBuffer.length(), 0);
	m_mutex_inout.unlock();

	// LogDebug("Write socket " + std::to_string(nWrite) + " bytes.");

	if (nWrite < 0) {
		ThrowException("Error writing to socket");
	}
}

// --------------------------
// Virtual
// --------------------------

int IBase::Main()
{
	// Checkings
	if (IsRoot() == false)
	{
		LogLocal("This application can't be run directly, it's used internally by Eddie. (not root)");
		return 1;
	}

	// Service install/uninstall request
	if (m_cmdline.find("service") != m_cmdline.end())
	{
		if (m_cmdline["service"] == "install")
		{
			if (ServiceInstall())
				return 0;
			else
			{
				LogLocal("Service install fail.");
				return 1;
			}
		}
		else if (m_cmdline["service"] == "uninstall")
		{
			if (ServiceUninstall())
				return 0;
			else
			{
				LogLocal("Service uninstall fail.");
				return 1;
			}
		}
		else
		{
			LogLocal("Unknown service request");
			return 1;
		}
	}

	m_lastModified = GetProcessModTimeCurrent();

	int nMaxAccepted = -1;
	int port = m_elevatedPortDefault;

	if ((m_cmdline.find("mode") != m_cmdline.end()) && (m_cmdline["mode"] == "spot"))
	{
		nMaxAccepted = 1;
		m_serviceMode = false;

		if (m_cmdline.find("spot_port") != m_cmdline.end())
			port = atoi(m_cmdline["spot_port"].c_str());
	}
	else if ((m_cmdline.find("mode") != m_cmdline.end()) && (m_cmdline["mode"] == "service"))
	{
		nMaxAccepted = -1;
		m_serviceMode = true;

		if (m_cmdline.find("service_port") != m_cmdline.end())
			port = atoi(m_cmdline["service_port"].c_str());
	}
	else
	{
		LogLocal("This application can't be run directly, it's used internally by Eddie. (unknown mode)");
		return 1;
	}




	// If launched in SPOT mode, if service was active, they not accept, so reinstall.
	if (m_serviceMode == false)
		ServiceReinstall();

	int nAccepted = 0;

	HSOCKET sockServer;
	struct sockaddr_in addrServer;

	sockServer = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);

	SocketMarkReuseAddr(sockServer);

	if (SocketIsValid(sockServer) == false)
	{
		ThrowException("Error on opening socket");
	}

	std::memset(&addrServer, 0, sizeof(addrServer));

	addrServer.sin_family = AF_INET;
	addrServer.sin_addr.s_addr = htonl(INADDR_LOOPBACK);
	addrServer.sin_port = htons(port);

	if (bind(sockServer, (struct sockaddr *) &addrServer, sizeof(addrServer)) != 0) {
		ThrowException("Error on binding socket");
	}

	if (listen(sockServer, 1) != 0) {
		ThrowException("Error on listen socket");
	}

	SocketBlockMode(sockServer, false);

	for (;;)
	{
		if ((nMaxAccepted != -1) && (nAccepted >= nMaxAccepted))
		{
			LogDebug("Exit from spot mode");
			break;
		}

		if (IsStopRequested())
			break;

		m_session_key = "";
		m_sockClient = 0;
		struct sockaddr_in addrClient;
		socklen_t addrClientLen = sizeof(addrClient);

		LogDebug("Waiting for client");

		try
		{
			for (;;)
			{
				m_sockClient = accept(sockServer, (struct sockaddr *)&addrClient, &addrClientLen);

				// TOFIX. Under Linux, errno==EWOULDBLOCK. Under Windows, i expect WSAEWOULDBLOCK but there are something not understanding.
				if (SocketIsValid(m_sockClient) == false)
				{
					if (IsStopRequested())
						break;

					Idle();

					Sleep(1000);
				}
				else
				{
					break;
				}
			}

			if (SocketIsValid(m_sockClient))
			{
				nAccepted++;

				// Remove if nonblock is inherit
				SocketBlockMode(m_sockClient, true);

				// Check allowed
				{
					int clientProcessId = GetProcessIdMatchingIPEndPoints(addrClient, addrServer);

					if (clientProcessId == 0)
						ThrowException("Client not allowed: Cannot detect client process");

					std::string clientProcessPath = GetProcessPathOfId(clientProcessId);
					if (clientProcessPath == "")
						ThrowException("Client not allowed: Cannot detect client process path");

					// If spot mode, must be the parent
					if (m_serviceMode == false)
					{
						int parentPID = GetParentProcessId();

                        if (clientProcessId != parentPID)
						{
							int parentPID2 = GetParentProcessId(parentPID); // Grandparent, for example if launched via 'sudo'
       
							if (clientProcessId != parentPID2)
							{
								ThrowException("Client not allowed: Connection not from parent process (spot mode)");
							}
						}
					}

					// In service mode, hash must match. See description of ComputeIntegrityHash.
					if (m_serviceMode)
					{
						std::string integrityHashComputed = ComputeIntegrityHash(GetProcessPathCurrent(), clientProcessPath);
						std::string integrityHashExpected = "";
						if (m_cmdline.find("integrity") != m_cmdline.end())
							integrityHashExpected = m_cmdline["integrity"];
						if (integrityHashExpected != "")
						{
							if (integrityHashComputed == "")
								ThrowException("Client not allowed: Client unknown (service mode)");
							else if (integrityHashComputed != integrityHashExpected)
								ThrowException("Client not allowed: integrity mismatch (client " + integrityHashComputed + " != expected " + integrityHashExpected + ") (service mode)");
						}
					}

					std::string allowed = CheckIfClientPathIsAllowed(clientProcessPath);
					if (allowed != "ok")
						ThrowException("Client not allowed: " + allowed);
				}

				ReplyPID(GetCurrentProcessId());

				char buffer[NETBUFSIZE];
				std::memset(buffer, 0, NETBUFSIZE);
				uint bufferPos = 0;
				bool clientStop = false;

				for (;;)
				{
					if (clientStop)
						break;

					if (IsStopRequested())
						break;

					int nMaxRead = NETBUFSIZE - bufferPos;
					if (nMaxRead <= 0) {
						ThrowException("Unexpected, command too big");
					}

					int n = recv(m_sockClient, buffer + bufferPos, nMaxRead, 0);
					if (n < 0) {
						ThrowException("Error reading from socket");
					}

					// LogDebug("Read socket " + std::to_string(n) + " bytes.");

					if (n == 0)
					{
						break;
					}

					bufferPos += n;

					for (;;)
					{
						uint bufferPosEndLine = 0;
						bool bufferPosEndLineFound = false;
						for (uint c = 0; c < bufferPos; c++)
						{
							if (buffer[c] == '\n')
							{
								bufferPosEndLine = c;
								bufferPosEndLineFound = true;
								break;
							}
						}

						if (bufferPosEndLineFound == false)
						{
							break;
						}
						else
						{
							std::string line(buffer, bufferPosEndLine);
							memcpy(&buffer[0], &buffer[0] + bufferPosEndLine + 1, bufferPos - (bufferPosEndLine + 1));
							bufferPos -= (bufferPosEndLine + 1);
							buffer[bufferPos] = 0;

							// Process command
							std::map<std::string, std::string> params;

							std::string::size_type pos = 0;

							for (;;)
							{
								std::string::size_type keyEnd = line.find(':', pos);
								if (keyEnd == std::string::npos)
									break;
								std::string::size_type valueEnd = line.find(';', keyEnd + 1);
								if (valueEnd == std::string::npos)
									break;

								std::string key = line.substr(pos, keyEnd - pos);
								std::string value = line.substr(keyEnd + 1, valueEnd - keyEnd - 1);
								value = base64_decode(value);

								params[key] = value;

								pos = valueEnd + 1;
							}

							std::string id = "";
							if (params.find("_id") != params.end())
								id = params["_id"];
							std::string command = "";
							if (params.find("command") != params.end())
								command = params["command"];

							if (command == "exit")
							{
								clientStop = true;
							}
							else if (command != "")
							{
								LogDebug("Command:" + command);

								std::thread t = std::thread(ThreadCommand, this, id, command, params);
								t.detach();
							}
						}
					}
				}
			}
		}
		catch (const std::exception& e)
		{
			LogFatal(std::string(e.what()));
		}
		catch (...)
		{
			LogFatal(std::string("Unknown exception"));
		}

		LogDebug("Client soft disconnected");

		SocketClose(m_sockClient);
	}

	SocketClose(sockServer);

	return 0;
}

void IBase::Idle()
{
}

void IBase::Do(const std::string& commandId, const std::string& command, std::map<std::string, std::string>& params)
{
	if (command == "exit")
	{
	}
	else if (command == "tor-get-info")
	{
		// Eddie need to talk with Tor Control to ask for new circuits or obtain guard IPs.
		std::string processName;
		std::string processPath;
		std::string username;
		std::string cookiePath;
		std::string cookiePasswordHex;

		if (params.find("name") != params.end())
			processName = params["name"];

		if (params.find("path") != params.end())
			processPath = params["path"];

		if (params.find("username") != params.end())
			username = params["username"];

		if (FsFileExists(processPath) == false)
			processPath = "";

		if (processPath == "")
		{
			pid_t id = GetProcessIdOfName(processName);
			if (id != 0)
				processPath = GetProcessPathOfId(id);
		}

		if (processPath == "") // Alternative name, TorBrowser under macOS
		{
			pid_t id = GetProcessIdOfName(processName + ".real");
			if (id != 0)
				processPath = GetProcessPathOfId(id);
		}

		std::vector<std::string> paths;
		AddTorCookiePaths(processPath, username, paths);

		for (std::vector<std::string>::const_iterator i = paths.begin(); i != paths.end(); ++i)
		{
			std::string path = *i;
			if (FsFileExists(path))
			{
				std::vector<char> chars = FsFileReadBytes(path);
				if (chars.size() > 0)
				{
					cookiePath = path;
					cookiePasswordHex = StringHexEncode(chars);
					break;
				}
			}
		}

		ReplyCommand(commandId, "Name:" + processName);
		ReplyCommand(commandId, "Path:" + processPath);
		ReplyCommand(commandId, "CookiePath:" + cookiePath);
		ReplyCommand(commandId, "CookiePasswordHex:" + cookiePasswordHex);
	}
	else
	{
		ReplyException(commandId, "Unknown elevated command: " + command);
	}
}

bool IBase::IsStopRequested()
{
	return false;
}

bool IBase::IsServiceInstalled()
{
	return false;
}

bool IBase::ServiceInstall()
{
	return false;
}

bool IBase::ServiceUninstall()
{
	return false;
}

bool IBase::ServiceReinstall()
{
	if (IsServiceInstalled())
		return ServiceInstall();
	else
		return false;
}

std::string IBase::GetProcessPathCurrent()
{
	return GetProcessPathOfId(GetCurrentProcessId());
}

std::string IBase::GetProcessPathCurrentDir()
{
	std::string procPath = GetProcessPathCurrent();
	size_t pos = procPath.find_last_of(FsPathSeparator);
	if (pos != std::string::npos)
		procPath = procPath.substr(0, pos);
	return procPath;
}

time_t IBase::GetProcessModTimeStart()
{
	return m_lastModified;
}

time_t IBase::GetProcessModTimeCurrent()
{
	std::string path = GetProcessPathCurrent();

	struct stat st;
	memset(&st, 0, sizeof(struct stat));
	if (stat(path.c_str(), &st) == 0)
	{
		return st.st_mtime;
	}
	else
	{
		return 0;
	}
}

std::string IBase::GetTempPath(const std::string& filename)
{
	return FsGetTempPath() + FsPathSeparator + "eddie_tmp_" + filename;
}

void IBase::AddTorCookiePaths(const std::string& torPath, const std::string& username, std::vector<std::string>& result)
{
}

// --------------------------
// Utils filesystem
// --------------------------

bool IBase::FsFileWriteText(const std::string& path, const std::string& body)
{
	std::ofstream f(path);
	if (!f)
		return "";
	f << body;
	f.close();
	return true;
}

bool IBase::FsFileAppendText(const std::string& path, const std::string& body)
{
	FILE* f = fopen(path.c_str(), "a");
	if (f == NULL)
		return false;
	fprintf(f, "%s", body.c_str());
	fclose(f);
	return true;
}

std::string IBase::FsFileGetDirectory(const std::string& path)
{
	//std::filesystem::path p = path;
	//return p.parent_path();

	return path.substr(0, path.find_last_of("/\\"));
}

std::vector<char> IBase::FsFileReadBytes(const std::string& path)
{
	/*
	std::ifstream input(path, std::ios::binary);

	std::vector<unsigned char> bytes(
		 (std::istreambuf_iterator<char>(input)),
		 (std::istreambuf_iterator<char>()));

	input.close();

	return bytes;
	*/

	// Note: This don't work with a /proc/x/cmdline

	std::ifstream ifs(path.c_str(), std::ios::binary | std::ios::ate);
	std::ifstream::pos_type pos = ifs.tellg();

	std::vector<char>  result(pos);

	ifs.seekg(0, std::ios::beg);
	ifs.read(&result[0], pos);

	return result;
}

std::string IBase::FsFileSHA256Sum(const std::string& path)
{
	FILE *f;
	size_t i;
	unsigned char buf[1000];
	sha256_context ctx;
	unsigned char sha256sum[32];

	sha256_starts(&ctx);

	if (!(f = fopen(path.c_str(), "rb")))
	{
		ThrowException("Unable to open for sha256 hash, path: " + path);
	}

	sha256_starts(&ctx);

	while ((i = fread(buf, 1, sizeof(buf), f)) > 0)
	{
		sha256_update(&ctx, buf, (unsigned long)i);
	}

	fclose(f);

	sha256_finish(&ctx, sha256sum);
 
	return StringHexEncode(&sha256sum[0], 32);
}

std::string IBase::FsLocateExecutable(const std::string& name)
{
	std::vector<std::string> paths = FsGetEnvPath();

	for (std::vector<std::string>::const_iterator i = paths.begin(); i != paths.end(); ++i)
	{
		std::string fullPath = *i + FsPathSeparator + name;
		if (FsFileExists(fullPath))
		{
			CheckIfExecutableIsAllowed(fullPath);

			return fullPath;
		}
	}

	return "";
}


// --------------------------
// Utils string
// --------------------------

std::string IBase::StringReplaceAll(const std::string& str, const std::string& from, const std::string& to)
{
	std::string result = str;
	size_t start_pos = 0;
	while ((start_pos = result.find(from, start_pos)) != std::string::npos) {
		result.replace(start_pos, from.length(), to);
		start_pos += to.length();
	}
	return result;
}

std::string IBase::StringExtractBetween(const std::string& str, const std::string& from, const std::string& to)
{
	size_t start_pos = str.find(from);
	if (start_pos == std::string::npos)
		return "";
	size_t end_pos = str.find(to, start_pos + from.length());
	if (end_pos == std::string::npos)
		return "";
	return str.substr(start_pos + from.length(), end_pos - start_pos - from.length());
}

std::vector<std::string> IBase::StringToVector(const std::string& s, const char c, bool autoTrim)
{
	std::vector<std::string> result;
	const char* str = s.c_str();

	do
	{
		const char *begin = str;
		while (*str != c && *str)
			str++;
		std::string item = std::string(begin, str);
		if (autoTrim)
		{
			item = StringTrim(item);
			if (item == "")
				continue;
		}

		result.push_back(item);
	} while (0 != *str++);

	return result;
}

std::string IBase::StringFromVector(const std::vector<std::string>& v, const std::string& delimiter)
{
	std::string output;
	for (uint i = 0; i < v.size(); i++)
	{
		if (i != 0)
			output += delimiter;
		output += v[i];
	}
	return output;
}

bool IBase::StringStartsWith(const std::string& s, const std::string& f)
{
	size_t pos = s.find(f);
	return (pos == 0);
}

bool IBase::StringEndsWith(const std::string& s, const std::string& f)
{
	return(s.size() >= f.size() && s.compare(s.size() - f.size(), f.size(), f) == 0);
}

bool IBase::StringContain(const std::string& s, const std::string& f)
{
	return (s.find(f) != std::string::npos);
}

bool IBase::StringVectorsEqual(const std::vector<std::string>& v1, const std::vector<std::string>& v2)
{
	if (v1.size() != v2.size())
		return false;

	for (uint i = 0; i < v1.size(); i++)
	{
		if (v1[i] != v2[i])
			return false;
	}

	return true;
}

bool IBase::StringVectorsEqualOrdered(const std::vector<std::string>& v1, const std::vector<std::string>& v2)
{
	if (v1.size() != v2.size())
		return false;

	std::vector<std::string> v1o(v1);
	std::vector<std::string> v2o(v2);

	std::sort(v1o.begin(), v1o.end());
	std::sort(v2o.begin(), v2o.end());

	return StringVectorsEqual(v1o, v2o);
}

std::string IBase::StringTrim(const std::string& s, const std::string& chars)
{
	std::string result = s;
	result.erase(0, result.find_first_not_of(chars));
	result.erase(result.find_last_not_of(chars) + 1);
	return result;
}

std::string IBase::StringTrim(const std::string& s)
{
	const std::string& chars = "\t\n\v\f\r ";
	return StringTrim(s, chars);
}

std::string IBase::StringToLower(const std::string& s)
{
	std::string result = s;
	std::transform(result.begin(), result.end(), result.begin(), ::tolower);
	return result;
}

std::string IBase::StringPruneCharsNotIn(const std::string& str, const std::string& allowed)
{
	std::string result = "";
	for (std::string::const_iterator it = str.begin(); it != str.end(); ++it)
	{
		if (allowed.find(*it) != std::string::npos)
			result += *it;
	}
	return result;
}

std::string IBase::StringEnsureSecure(const std::string& str)
{
	return StringPruneCharsNotIn(str, " .;:-_0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ");
}

std::string IBase::StringEnsureInterfaceName(const std::string& str)
{
	return StringPruneCharsNotIn(str, " :-_0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ");
}

std::string IBase::StringEnsureCidr(const std::string& str)
{
	return StringPruneCharsNotIn(str, "0123456789abcdef.:/");
}

std::string IBase::StringEnsureIpAddress(const std::string& str)
{
	return StringPruneCharsNotIn(str, "0123456789abcdef.:");
}

std::string IBase::StringEnsureNumericInt(const std::string& str)
{
	return StringPruneCharsNotIn(str, "-0123456789");
}

std::string IBase::StringBase64Encode(const std::string& str)
{
	return base64_encode(str);
}

std::string IBase::StringBase64Decode(const std::string& str)
{
	return base64_decode(str);
}

std::string IBase::StringXmlEncode(const std::string& str)
{
	std::string result = str;
	result = StringReplaceAll(result, "&", "&amp;");
	result = StringReplaceAll(result, "<", "&lt;");
	result = StringReplaceAll(result, ">", "&gt;");
	result = StringReplaceAll(result, "\"", "&quot;");
	result = StringReplaceAll(result, "'", "&apos;");
	return result;
}

std::string IBase::StringHexEncode(const unsigned char* buf, const size_t s)
{
    const char digitshex[] = "0123456789abcdef";
    std::string hex;
    hex.reserve(s*2);
    for (size_t i = 0; i < s; i++)
    {
        unsigned char c = buf[i];
        hex.push_back(digitshex[c >> 4]);
        hex.push_back(digitshex[c & 15]);
    }
    return hex;
}

std::string IBase::StringHexEncode(const std::vector<char>& bytes)
{
    return StringHexEncode((const unsigned char*)bytes.data(), bytes.size());
}

std::string IBase::StringHexEncode(const int v, const int chars)
{
	std::stringstream ss;
	ss << std::setfill('0') << std::setw(chars) << std::uppercase << std::hex << v;
	return ss.str();
}

// --------------------------
// Utils other
// --------------------------

std::map<std::string, std::string> IBase::ParseCommandLine(const std::vector<std::string>& args)
{
	// Note: accept "key1 key2=value2 key3" syntax. No "--key", no "--key='value'", no quotes.
	std::map<std::string, std::string> result;

	for (size_t i = 0; i < args.size(); i++)
	{
		std::string f = args[i];

		std::string k = "";
		std::string v = "";

		if (i == 0)
		{
			k = "path";
			v = f;
		}
		else
		{
			size_t posKV = f.find('=');
			if (posKV != std::string::npos)
			{
				k = f.substr(0, posKV);
				v = f.substr(posKV + 1);
			}
			else
			{
				k = f;
				v = "";
			}
		}

		if ((k == "service") && (v == "")) // Compatibility >=2.18.1 and <=2.18.4
		{
			k = "mode";
			v = "service";
		}

		if (k != "")
		{
			result[k] = v;
		}
	}

	return result;
}

std::string IBase::CheckValidOpenVpnConfig(const std::string& path)
{
	if (FsFileExists(path) == false)
		return "file not found";

	std::string body = FsFileReadText(path);
	std::vector<std::string> lines = StringToVector(body, '\n');
	for (std::vector<std::string>::const_iterator i = lines.begin(); i != lines.end(); ++i)
	{
		std::string lineNormalized = *i;
		lineNormalized = StringTrim(StringToLower(lineNormalized));

		if (StringStartsWith(lineNormalized, "#"))
			continue;

		bool lineAllowed = true;

		// The below list directive can be incompleted with newest OpenVPN version, 
		// but any new dangerous directive is expected to be blocked by script-security
		// Note: parser can be better (any <key> lines are treated as directive, but never match for the space)
		// But this validation is only for additional security, Eddie already parse and prune with a better parser the config
		if (StringStartsWith(lineNormalized, "script-security ")) lineAllowed = false;
		if (StringStartsWith(lineNormalized, "plugin ")) lineAllowed = false;
		if (StringStartsWith(lineNormalized, "up ")) lineAllowed = false;
		if (StringStartsWith(lineNormalized, "down ")) lineAllowed = false;
		if (StringStartsWith(lineNormalized, "client-connect ")) lineAllowed = false;
		if (StringStartsWith(lineNormalized, "client-disconnect ")) lineAllowed = false;
		if (StringStartsWith(lineNormalized, "learn-address ")) lineAllowed = false;
		if (StringStartsWith(lineNormalized, "auth-user-pass-verify ")) lineAllowed = false;
		if (StringStartsWith(lineNormalized, "tls-verify ")) lineAllowed = false;
		if (StringStartsWith(lineNormalized, "ipchange ")) lineAllowed = false;
		if (StringStartsWith(lineNormalized, "iproute ")) lineAllowed = false;
		if (StringStartsWith(lineNormalized, "route-up ")) lineAllowed = false;
		if (StringStartsWith(lineNormalized, "route-pre-down ")) lineAllowed = false;

		if (lineAllowed == false)
			return "directive '" + lineNormalized + "' not allowed";
	}

	return "";
}

std::string IBase::CheckValidHummingbirdConfig(const std::string& path)
{
	return CheckValidOpenVpnConfig(path);
}

// When the service is installed, we build an hash of executable and library of the software.
// When a client connect to service, the socket client path must be in this list, and the hash of the package (when service was installed) must match.
// This ensure if the client is altered, the service don't accept it anymore, without need a digital signature (not possible because in some OS, the software is builded from sources).
// If clientPath is empty, return the computed hash. If not empty, return the computed hash only if clientPath is one of the files computed.
std::string IBase::ComputeIntegrityHash(const std::string& elevatedPath, const std::string& clientPath)
{
	bool clientPathFound = false;
	std::string checkPath = FsFileGetDirectory(elevatedPath);
	std::string integrity;
	std::vector<std::string> files = FsFilesInPath(checkPath);
	std::sort(files.begin(), files.end());
	for (std::vector<std::string>::const_iterator i = files.begin(); i != files.end(); ++i)
	{
		std::string file = *i;

		std::string ext = "";
		std::string::size_type extPos = file.rfind('.');

		if (extPos != std::string::npos)
			ext = file.substr(extPos + 1);

		bool include = ((ext == "") || (ext == "exe") || (ext == "dll") || (ext == "so") || (ext == "dylib"));

		if (include)
		{
			std::string checkPathFull = checkPath + FsPathSeparator + file;
			integrity += FsFileSHA256Sum(checkPathFull) + ";";

			if (clientPath != "")
			{
				if (clientPath == checkPathFull)
					clientPathFound = true;
			}
		}
	}

	if ((clientPath != "") && (clientPathFound == false))
		return "";

	return integrity;
}

bool IBase::CheckIfExecutableIsWhitelisted(const std::string& path)
{
	// Exception: Maybe the bundle openvpn or hummingbird (if not, doesn't matter).
	// Portable or AppImage edition can't have other checks performed by CheckIfExecutableIsAllowed
	// We hardcoded hash (updated by build scripts) in sources, like HTTP Content-Security-Policy approach.
	// If match, trust it, otherwise other checks are performed.
	std::string computedHash = FsFileSHA256Sum(path);

	if (computedHash == expectedOpenvpnHash)
		return true;

	if (computedHash == expectedHummingbirdHash)
		return true;

	return false;
}

// If Elevated launch child process, add to this list.
// Used by some action like 'kill' to allow signal only to child process.

void IBase::PidAdd(pid_t pid)
{
	m_pidManaged[pid] = true;
}

void IBase::PidRemove(pid_t pid)
{
	m_pidManaged.erase(pid);
}

bool IBase::PidManaged(pid_t pid)
{
	if (m_pidManaged.find(pid) != m_pidManaged.end())
		return true;
	else
		return false;
}

// --------------------------
// Helper
// --------------------------

void IBase::ThrowException(const std::string& message)
{
	throw std::runtime_error(message);
}

ShellResult IBase::ShellEx(const std::string& path, const std::vector<std::string>& args)
{
	ShellResult result;
	result.exit = Shell(path, args, false, "", result.out, result.err);
	return result;
}

ShellResult IBase::ShellEx1(const std::string& path, const std::string& arg1)
{
	std::vector<std::string> args;
	args.push_back(arg1);
	return ShellEx(path, args);
}

ShellResult IBase::ShellEx2(const std::string& path, const std::string& arg1, const std::string& arg2)
{
	std::vector<std::string> args;
	args.push_back(arg1);
	args.push_back(arg2);
	return ShellEx(path, args);
}

ShellResult IBase::ShellEx3(const std::string& path, const std::string& arg1, const std::string& arg2, const std::string& arg3)
{
	std::vector<std::string> args;
	args.push_back(arg1);
	args.push_back(arg2);
	args.push_back(arg3);
	return ShellEx(path, args);
}

// --------------------------
// ShellResult class
// --------------------------

std::string ShellResult::output()
{
	std::string result = out;
	if (err != "")
	{
		if (result != "")
			result += "\n";
		result += err;
	}
	return result;
}

std::string ShellResult::dump()
{
	return "exit:" + std::to_string(exit) + "; out:" + out + "; err:" + err;
}

// --------------------------
// Outside class
// --------------------------

void ThreadCommand(IBase* impl, const std::string id, const std::string command, std::map<std::string, std::string> params)
{
	impl->MainDo(id, command, params);
}
