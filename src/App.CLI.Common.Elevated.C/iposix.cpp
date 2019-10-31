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

#include <unistd.h>

void IPosix::Do(const std::string& commandId, const std::string& command, std::map<std::string, std::string>& params)
{
    if (command == "kill")
    {
        pid_t pid = std::atoi(params["pid"].c_str());
        int signal = SIGINT;
        if(params["signal"] == "sigint")
            signal = SIGINT;
        else if(params["signal"] == "sigterm")
            signal = SIGTERM;
        
        kill(pid, signal);
    }
    else
    {
        IBase::Do(commandId, command, params);
    }
}

void IPosix::Sleep(int ms)
{
    usleep(ms * 1000);
}

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
