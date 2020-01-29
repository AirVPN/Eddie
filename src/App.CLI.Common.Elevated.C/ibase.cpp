
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
#include <stdio.h>
#include <stdlib.h>
#include <fstream>
#include <sstream>
#include <string.h>
#include <cstring>
#include <iomanip>

// Posix-specific
// Work in progress: move any posix/linux/macos code here in iposix.cpp,
// for cleanup and allow us to develop a C++ elevated in Windows (and deprecate the current C#).
#include <netdb.h>
#include <dirent.h>
#include <arpa/inet.h>

#include "pstream.h"
#include "base64.h"
#include "sha256.h"
#include "ibase.h"

using namespace redi; // related to pstream.h

// --------------------------
// Engine
// --------------------------

int IBase::AppMain(int argc, char* argv[])
{
    m_cmdline = ParseCommandLine(argc, argv);
    
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
                if(params.find("version") != params.end())
                    clientVersion = params["version"];
                
                if(clientVersion != m_elevatedVersion)
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
    SendMessage("ee:log:" + base64_encode(msg));
}

void IBase::LogLocal(const std::string& msg)
{
    std::cout << msg << std::endl;
}

void IBase::LogDebug(const std::string& msg)
{
#ifdef Debug
    std::cout << "Elevated Debug: " << msg << std::endl;
#endif
    if(m_debug)
        LogRemote("Elevated: " + msg);
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
    int nWrite = write(m_sockClient, sendBuffer.c_str(), sendBuffer.length());
    m_mutex_inout.unlock();
    
    // LogDebug("Write socket " + std::to_string(nWrite) + " bytes.");
    
    if (nWrite < 0) {
        ThrowException("Error writing to socket");
    }
}

// --------------------------
// Virtual
// --------------------------

// Must be derived
int IBase::Main()
{
    // Checkings
    
    uid_t euid = geteuid();
    if(euid != 0)
    {
        uid_t uid = getuid();
        LogLocal("This application can't be run directly, it's used internally by Eddie. (uid:" + std::to_string(uid) + ",euid:" + std::to_string(euid) + ")");
        return 1;
    }
    
    m_lastModified = GetProcessModTimeCurrent();
    
    int nMaxAccepted = -1;
    int port = 0;
    
    if ( (m_cmdline.find("mode") != m_cmdline.end()) && (m_cmdline["mode"] == "spot") )
    {
        nMaxAccepted = 1;
        port = 9345;
        m_serviceMode = false;
    }
    else if ( (m_cmdline.find("mode") != m_cmdline.end()) && (m_cmdline["mode"] == "service") )
    {
        nMaxAccepted = -1;
        port = 9346;
        m_serviceMode = true;
    }
    else
    {
        LogLocal("This application can't be run directly, it's used internally by Eddie.");
        return 1;
    }
    
    if (m_cmdline.find("port") != m_cmdline.end())
        port = atoi(m_cmdline["port"].c_str());
    
    int nAccepted = 0;
    
    int sockServer;
    struct sockaddr_in addrServer;
    
    int option = 1;
    sockServer = socket(AF_INET, SOCK_STREAM, 0);
    setsockopt(sockServer, SOL_SOCKET, SO_REUSEADDR, &option, sizeof(option));

    if (sockServer < 0) {
        ThrowException("Error on opening socket");
    }
    
    /* Initialize socket structure */
    bzero((char *) &addrServer, sizeof(addrServer));

    addrServer.sin_family = AF_INET;
    addrServer.sin_addr.s_addr = htonl(INADDR_LOOPBACK);
    addrServer.sin_port = htons(port);

    if (bind(sockServer, (struct sockaddr *) &addrServer, sizeof(addrServer)) < 0) {
        ThrowException("Error on binding socket");
    }
    
    listen(sockServer,5);
    
    int resultSocketListenBlock = fcntl(sockServer, F_SETFL, fcntl(sockServer, F_GETFL, 0) | O_NONBLOCK);
    if (resultSocketListenBlock == -1){
        ThrowException("Error on fcntl listen socket");
    }
    
    for(;;)
    {
        if( (nMaxAccepted != -1) && (nAccepted>=nMaxAccepted) )
        {
            LogDebug("Exit from spot mode");
            break;
        }
     
        m_session_key = "";       
        m_sockClient = 0;
        struct sockaddr_in addrClient;
        socklen_t addrClientLen = sizeof(addrClient);
        
        LogDebug("Waiting for client");
            
        try
        {
            for(;;)
            {
                m_sockClient = accept(sockServer, (struct sockaddr *)&addrClient, &addrClientLen);
                if(m_sockClient == -1)
                {
                    if (errno == EWOULDBLOCK) {
                        Idle();
                        Sleep(1000);
                    }
                    else
                    {
                        ThrowException("Error on accept socket");
                    }
                }
                else
                {
                    // Remove if inherit
                    int resultSocketAcceptedBlock = fcntl(m_sockClient, F_SETFL, fcntl(m_sockClient, F_GETFL, 0) & ~O_NONBLOCK);
                    if (resultSocketAcceptedBlock == -1){
                        ThrowException("Error on fcntl client socket");
                    }
    
                    break;
                }
            }
            
            nAccepted++;

            // Check allowed
            {
                int clientProcessId = GetProcessIdMatchingIPEndPoints(addrClient, addrServer);

                if(clientProcessId == 0)
                    ThrowException("Client not allowed: Cannot detect client process");
                    
                std::string clientProcessPath = GetProcessPathOfId(clientProcessId);
                if(clientProcessPath == "")
                    ThrowException("Client not allowed: Cannot detect client process path");
                    
                // If spot mode, must be the parent
                if(m_serviceMode == false)
                {
                    int parentPID = GetParentProcessId();
                    
                    if(clientProcessId != parentPID)
                    {
                        int parentPID2 = GetParentProcessId(parentPID); // Grandparent, for example if launched via 'sudo'
                        
                        if(clientProcessId != parentPID2)
                        {
                            ThrowException("Client not allowed: Connection not from parent process (spot mode)");
                        }
                    }
                }

                // In service mode, hash must match
                if(m_serviceMode)
                {
                    std::string allowedHash = "";
                    if(m_cmdline.find("allowed_hash") != m_cmdline.end())
                        allowedHash = m_cmdline["allowed_hash"];
                    std::string clientHash = FileSHA256Sum(clientProcessPath);
                    if(allowedHash != clientHash)
                        ThrowException("Client not allowed: Hash mismatch (client " + clientHash + " != expected " + allowedHash + ") (service mode)");
                }
                
                std::string allowed = CheckIfClientPathIsAllowed(clientProcessPath);
                if(allowed != "ok")
                    ThrowException("Client not allowed: " + allowed);
            }

            ReplyPID(getpid());            
            
            uint bufSize = 256*256*32; // Note: Maximum size of command
            char buffer[bufSize];
            bzero(buffer,sizeof(buffer));
            uint bufferPos = 0;
            bool clientStop = false;
            
            for(;;)
            {
                if(clientStop)
                    break;
                    
                int nMaxRead = sizeof(buffer)-bufferPos;
                if(nMaxRead <= 0) {
                    ThrowException("Unexpected, command too big");
                }
                
                int n = read(m_sockClient,buffer + bufferPos,nMaxRead);
                if (n < 0) {
                    ThrowException("Error reading from socket");
                }
                
                // LogDebug("Read socket " + std::to_string(n) + " bytes.");
                
                if(n == 0)
                {
                    break;
                }
                
                bufferPos += n;
                
                for(;;)
                {
                    uint bufferPosEndLine = 0;
                    bool bufferPosEndLineFound = false;
                    for(uint c=0;c<bufferPos;c++)
                    {
                        if(buffer[c] == '\n')
                        {
                            bufferPosEndLine = c;
                            bufferPosEndLineFound = true;
                            break;
                        }
                    }
                    
                    if(bufferPosEndLineFound == false)
                    {
                        break;
                    }
                    else
                    {
                        std::string line(buffer, bufferPosEndLine);
                        memcpy(&buffer[0], &buffer[0]+bufferPosEndLine+1, bufferPos-(bufferPosEndLine+1));
                        bufferPos -= (bufferPosEndLine+1);
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
                            
                        if(command == "exit")
                        {
                            clientStop = true;
                        }
                        else if(command != "")
                        {
                            LogDebug("Command:" + command);
                            
                            std::thread t = std::thread(ThreadCommand, this, id, command, params);
                            t.detach();
                        }
                    }
                }
            }
        }
        catch (const std::exception& e) 
        {
            LogFatal(std::string(e.what()));
        }
        catch(...)
        {
            LogFatal(std::string("Unknown exception"));
        }
        
        LogDebug("Client soft disconnected");
            
        shutdown(m_sockClient, 2);
        close(m_sockClient); 
    }
    
    shutdown(sockServer, 2);
    close(sockServer);
    
    return 0;
}

void IBase::Idle()
{
    // Scenario: macOS with portable app in /Application, create launchd daemon, replace the app with a new version
    if(IsServiceMode())
    {
        time_t start = GetProcessModTimeStart();
        time_t cur = GetProcessModTimeCurrent();
        if(start != cur)
        {
            LogLocal("Executable changed/upgraded, exit");
            kill(getpid(), SIGTERM);
        }    
    }
}

void IBase::Do(const std::string& commandId, const std::string& command, std::map<std::string, std::string>& params)
{
    if (command == "exit")
    {
    }
    else if (command == "process_openvpn")
    {
        CheckIfExecutableIsAllowed(params["path"]);
        
        const pstreams::pmode mode = pstreams::pstdout | pstreams::pstderr;
        pstreams::argv_type argv;
        argv.push_back(params["path"]);
        
        argv.push_back("--config");
        argv.push_back(params["config"]);
        
        std::string checkResult = CheckValidOpenVpnConfig(params["config"]);
        if(checkResult != "")
        {
            ReplyException(commandId, "Not supported OpenVPN config: " + checkResult);
        }
        else
        {
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
        
        // Workaround. In any case, hummingbird called from Eddie don't perform any action that need a recovery.
        if(FileExists("/etc/airvpn/hummingbird.lock"))
            FileDelete("/etc/airvpn/hummingbird.lock");
        
        std::string checkResult = CheckValidOpenVpnConfig(params["config"]);
        if(checkResult != "")
        {
            ReplyException(commandId, "Not supported Hummingbird config: " + checkResult);
        }
        else
        {
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
            
        if (FileExists(processPath) == false)
            processPath = "";
            
        if(processPath == "")
        {
            pid_t id = GetProcessIdOfName(processName);
            if(id != 0)
                processPath = GetProcessPathOfId(id);
        }

        if(processPath == "") // Alternative name, TorBrowser under macOS
        {
            pid_t id = GetProcessIdOfName(processName + ".real");
            if(id != 0)
                processPath = GetProcessPathOfId(id);
        }

        std::vector<std::string> paths;
        AddTorCookiePaths(processPath, username, paths);
        
        // Other custom paths to check
        if (params.find("cookie-path1") != params.end())
            paths.push_back(params["cookie-path1"]);
        if (params.find("cookie-path2") != params.end())
            paths.push_back(params["cookie-path2"]);
        
        for (std::vector<std::string>::const_iterator i = paths.begin(); i != paths.end(); ++i)
        {
            std::string path = *i;
            if(FileExists(path))
            {
                std::vector<char> chars = FileReadBytes(path);
                if(chars.size() > 0)
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

void IBase::CheckIfExecutableIsAllowed(const std::string& path)
{
    std::string issues = "";

    {
        // Exception: Maybe the bundle openvpn or hummingbird (if not, doesn't matter).
        // Portable or AppImage edition can't have the next checks.
        // We hardcoded hash (updated by build scripts) in sources, like HTTP Content-Security-Policy approach.
        // If match, trust it, otherwise other checks are performed.
        std::string computedHash = FileSHA256Sum(path);
        
        std::string expectedOpenvpnHash = "304254d86ad0609e3389345f0158f7326bf1a21c291c125b7e06de74aa263c05";
        if(expectedOpenvpnHash == computedHash) 
            return;
            
        std::string expectedHummingbirdHash = "91fc77a7e137f0fc1b0fce0d35e5e15b3d5cee2789961aa98d05c9afae071f41";
        if(expectedHummingbirdHash == computedHash)
            return;
    }
    
    struct stat st;
    memset(&st, 0, sizeof(struct stat));
    if(stat(path.c_str(), &st) != 0)
    {
        issues += "Not readable;";
    }
    else
    {		
        if(st.st_uid != 0)
        {
            issues += "Not owned by root;";
        }
        else if((st.st_mode & S_ISUID) == 0)
        {
            if((st.st_mode & S_IXUSR) == 0)
            {
                issues += "Not executable by owner;";
            }
            
            if((st.st_mode & S_IWGRP) != 0)
            {
                issues += "Writable by group";
            }
            
            if((st.st_mode & S_IWOTH) != 0)
            {
                issues += "Writable by other;";
            }
        }	
    }

    if(issues != "")
        ThrowException("Executable '" + path + "' not allowed: " + issues);
}

std::string IBase::GetProcessPathCurrent()
{
    return GetProcessPathOfId(getpid());
}

std::string IBase::GetProcessPathCurrentDir()
{
    std::string procPath = GetProcessPathCurrent();
    size_t pos = procPath.find_last_of("/");
    if(pos != std::string::npos)
        procPath = procPath.substr(0,pos);
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
    if(stat(path.c_str(), &st) == 0)
    {
        return st.st_mtime;
    }
    else
    {
        return 0;
    }
}

std::string IBase::GetTempPath()
{
    // Same env sequence performed by boost tmpdir
    // First version are the same app dir (elevated is always run with root, so no permission issues).
    // But on macOS, an App in Downloads start with AppTranslocation, and file written here are not accessible (for example pf config).
    
    const char* path;
    path = getenv("TMP");
    if(path != NULL)
        return path;
       
    path = getenv("TMPDIR");
    if(path != NULL)
        return path;
        
    path = getenv("TEMP");
    if(path != NULL)
        return path;
        
    if(FileExists("/tmp"))
        return "/tmp";
        
    return GetProcessPathCurrentDir();
}

std::string IBase::GetTempPath(const std::string& filename)
{
    return GetTempPath() + "/eddie_tmp_" + filename;
}

void IBase::AddTorCookiePaths(const std::string& torPath, const std::string& username, std::vector<std::string>& result)
{
}






// --------------------------
// Utils filesystem
// --------------------------

bool IBase::FileExists(const std::string& path)
{
    struct stat db;
    return (stat(path.c_str(), &db) == 0);
}

void IBase::FileDelete(const std::string& path)
{
    if (FileExists(path))
        unlink(path.c_str());
}

bool IBase::FileMove(const std::string& source, const std::string& destination)
{
    return (rename(source.c_str(), destination.c_str()) == 0);
}

bool IBase::FileWriteText(const std::string& path, const std::string& body)
{
    FILE* f = fopen(path.c_str(), "w");
    if (f == NULL)
        return false;
    fprintf(f, "%s", body.c_str());
    fclose(f);
    return true;
}

bool IBase::FileAppendText(const std::string& path, const std::string& body)
{
    FILE* f = fopen(path.c_str(), "a");
    if (f == NULL)
        return false;
    fprintf(f, "%s", body.c_str());
    fclose(f);
    return true;
}

std::string IBase::FileGetDirectory(const std::string& path)
{
    //std::filesystem::path p = path;
    //return p.parent_path();
    
    return path.substr(0, path.find_last_of("/\\"));
}

std::string IBase::FileReadText(const std::string& path)
{
    std::ifstream t(path);
    std::stringstream buffer;
    buffer << t.rdbuf();
    return buffer.str();
}

std::vector<char> IBase::FileReadBytes(const std::string& path)
{
    /*
    std::ifstream input(path, std::ios::binary);

    std::vector<unsigned char> bytes(
         (std::istreambuf_iterator<char>(input)),
         (std::istreambuf_iterator<char>()));

    input.close();
    
    return bytes;
    */
    std::ifstream ifs(path.c_str(), std::ios::binary|std::ios::ate);
    std::ifstream::pos_type pos = ifs.tellg();

    std::vector<char>  result(pos);

    ifs.seekg(0, std::ios::beg);
    ifs.read(&result[0], pos);

    return result;
}

std::vector<std::string> IBase::FilesInPath(const std::string& path)
{
    std::vector<std::string> result;
    
    DIR* dirp = opendir(path.c_str());
    if(dirp == NULL)
        return result;
    struct dirent * dp;
    while ((dp = readdir(dirp)) != NULL) {
        std::string filename = dp->d_name;
        if(filename == ".") continue;
        if(filename == "..") continue;
        result.push_back(filename);
    }
    closedir(dirp);
    return result;
}

std::string IBase::FileSHA256Sum(const std::string& path)
{
    FILE *f;
    int i, j;
    unsigned char buf[1000];
    sha256_context ctx;
    unsigned char sha256sum[32];
    
    sha256_starts( &ctx );
    
    if( ! ( f = fopen( path.c_str(), "rb" ) ) )
    {
        ThrowException("Unable to open for sha256 hash, path: " + path);
    }

    sha256_starts( &ctx );

    while( ( i = fread( buf, 1, sizeof( buf ), f ) ) > 0 )
    {
        sha256_update( &ctx, buf, i );
    }
    
    fclose(f);

    sha256_finish( &ctx, sha256sum );
    
    char result[65];
    for( j = 0; j < 32; j++ )
    {
        sprintf(&result[j*2], "%02x", sha256sum[j]);
    }
    result[64]=0;
    
    return result;
}

int IBase::Shell(const std::string& path, const std::vector<std::string>& args, const bool stdinWrite, const std::string& stdinBody, std::string& stdOut, std::string& stdErr)
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
        
        if(child.rdbuf()->exited())
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

std::string IBase::LocateExecutable(const std::string& name)
{
    std::vector<std::string> paths = StringToVector(getenv("PATH"),':',false);
    for (std::vector<std::string>::const_iterator i = paths.begin(); i != paths.end(); ++i)
    {
        std::string fullPath = *i + "/" + name;
        if(FileExists(fullPath))
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
    if(start_pos == std::string::npos)
        return "";
    size_t end_pos = str.find(to, start_pos + from.length());
    if(end_pos == std::string::npos)
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
        if(autoTrim)
        {
            item = StringTrim(item);
            if(item == "")
                continue;
        }
        
        result.push_back(item);
    } while (0 != *str++);

    return result;
}

std::string IBase::StringFromVector(const std::vector<std::string>& v, const std::string& delimiter)
{
    std::string output;
    for(uint i=0;i<v.size();i++)
    {
        if(i!=0)
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
    if(v1.size() != v2.size())
        return false;
        
    for(uint i=0;i<v1.size();i++)
    {
        if(v1[i] != v2[i])
            return false;
    }

    return true;
}

bool IBase::StringVectorsEqualOrdered(const std::vector<std::string>& v1, const std::vector<std::string>& v2)
{
    if(v1.size() != v2.size())
        return false;

    std::vector<std::string> v1o(v1);
    std::vector<std::string> v2o(v2);

    std::sort(v1o.begin(), v1o.end());
    std::sort(v2o.begin(), v2o.end());
    
    return StringVectorsEqual(v1o, v2o);
}

std::string IBase::StringTrim(const std::string& s)
{
    const std::string& chars = "\t\n\v\f\r ";
    std::string result = s;
    result.erase(0, result.find_first_not_of(chars));
    result.erase(result.find_last_not_of(chars) + 1);
    return result;
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
    return StringPruneCharsNotIn(str, ":-_0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ");
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

std::string IBase::StringHexEncode(const std::vector<char>& bytes)
{
    uint len = bytes.size();
    char result[len*2+1];
    
    for(uint i=0;i<bytes.size();i++)
        sprintf((char*)&result+i*2, "%02x", (unsigned char)bytes[i]);
    result[len*2] = 0;
        
    return result;
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

std::map<std::string, std::string> IBase::ParseCommandLine(int argc, char* argv[])
{
    // Note: accept "key1 key2=value2 key3" syntax. No "--key", no "--key='value'", no quotes.
    std::map<std::string, std::string> result;
    
    for(int i=0;i<argc;i++)
    {
        std::string f = argv[i];
        
        std::string k = "";
        std::string v = "";
        
        if(i == 0)
        {
            k = "path";
            v = f;
        }
        else
        {
            size_t posKV = f.find('=');
            if(posKV != std::string::npos)
            {
                k = f.substr(0, posKV);
                v = f.substr(posKV+1);
            }
            else
            {
                k = f;
                v = "";
            }
        }
        
        if( (k == "service") && (v == "") ) // Compatibility >=2.18.1 and <=2.18.4
        {
            k = "mode";
            v = "service";
        }
        
        if(k != "")
            result[k] = v;
    }
    
    return result;
}

std::string IBase::CheckValidOpenVpnConfig(const std::string& path)
{
    if(FileExists(path) == false)
        return "file not found";

    std::string body = FileReadText(path);
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
        // But this validation is only for additional security, Eddie already parse and prune with a better parse the config
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
        
        if(lineAllowed == false)
            return "directive '" + lineNormalized + "' not allowed";
    }

    return "";
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
    if(err == "")
        return out;
    if(out == "")
        return err;
    return "out:" + out + ";err:" + err;
}

// --------------------------
// Outside class
// --------------------------

void ThreadCommand(IBase* impl, const std::string id, const std::string command, std::map<std::string, std::string> params)
{
    impl->MainDo(id, command, params);
}
