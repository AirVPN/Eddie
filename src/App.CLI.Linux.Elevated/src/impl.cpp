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

#include <fcntl.h>

#include "impl.h"
#include <linux/fs.h>
#include <sys/ioctl.h>
#include <sys/stat.h>
#include <unistd.h>

#include <sys/prctl.h> // for prctl()
#include <sys/types.h> // for signal()
#include <signal.h> // for signal()

int Impl::Main(int argc, char* argv[])
{
    std::string serviceName = "eddie-elevated";
    std::string serviceDesc = "Eddie Elevation";
    std::string systemdPath = "/usr/lib/systemd/system";
    std::string systemdUnitName = serviceName + ".service";
    std::string systemdUnitPath = systemdPath + "/" + systemdUnitName;
    
    signal(SIGINT, SIG_IGN); // If Eddie is executed as terminal, and receive a Ctrl+C, elevated are terminated before child process (if 'spot'). Need a better solution.
    signal(SIGHUP, SIG_IGN); // Signal of reboot, ignore, container will manage it
    
    prctl(PR_SET_PDEATHSIG, SIGHUP); // Any child process will be killed if this process died, Linux specific
    
    // fd = Inhibit("shutdown:idle", "Package Manager", "Upgrade in progress...", "block");
    
    if (argc != 2)
    {
        LogLocal("This application can't be run directly, it's used internally by Eddie.");
        return 1;
    }
    else if (std::string(argv[1]) == "service-install")
    {
		std::string elevatedPath = GetProcessPathCurrent();
        
        if(FileExists(systemdPath))
        {
            std::string unit = "";
            unit += "[Unit]\n";
            unit += "Description=" + serviceDesc + "\n";
            unit += "Requires=network.target\n";
            unit += "After=network.target\n";
            unit += "\n";
            unit += "[Service]\n";
            unit += "Type=simple\n";
            unit += "ExecStart=\"" + elevatedPath + "\" service\n";
            unit += "Restart=always\n";
            unit += "RestartSec=5s\n";
            unit += "TimeoutStopSec=5s\n";
            unit += "User=root\n";
            unit += "Group=root\n";
            unit += "\n";
            unit += "[Install]\n";
            unit += "WantedBy=multi-user.target\n";
            
            FileWriteText(systemdUnitPath, unit);
        
            ShellResult enableResult = ShellEx2(LocateExecutable("systemctl"), "enable", systemdUnitName);        
            if(enableResult.exit != 0)
            {
                LogLocal("Enable " + systemdUnitName + " failed");
                return 1;
            }
            
            ShellResult startResult = ShellEx2(LocateExecutable("systemctl"), "start", systemdUnitName);        
            if(startResult.exit != 0)
            {
                LogLocal("Start " + systemdUnitName + " failed");
                return 1;
            }
            
            return 0;
        }
        else
        {
            LogLocal("Can't create service in this OS");
        }
        
        return 1;
    }
    else if (std::string(argv[1]) == "service-uninstall")
    {
        if(FileExists(systemdUnitPath))
        {
            ShellEx2(LocateExecutable("systemctl"), "stop", systemdUnitName);        
            ShellEx2(LocateExecutable("systemctl"), "disable", systemdUnitName);        
            FileDelete(systemdUnitPath);
            
            return 0;
        }
        else
        {
            LogLocal("Can't create service in this OS");
        }
        
        return 1;
    }
    else
    {
        return IPosix::Main(argc, argv);
    }
}

void Impl::Do(const std::string& commandId, const std::string& command, std::map<std::string, std::string>& params)
{
	if (command == "compatibility-profiles")
    {
        const std::string& dataPath = params["path-data"];
        
        if(FileExists(dataPath) == false)
        {
            // Rename old .airvpn to .eddie if exists, and change owner (<2.17.3 are root-only)
            std::string oldPath = dataPath;
            size_t pos = oldPath.find_last_of("/");
            if(pos != std::string::npos)
            {
                oldPath = oldPath.substr(0, pos) + "/../.airvpn";
                if(FileExists(oldPath))
                {
                    const std::string& newOwner = params["owner"];
                    FileMove(oldPath, dataPath);
                    std::vector<std::string> args;
                    args.push_back("-R");
                    args.push_back(newOwner);
                    args.push_back(dataPath);
                    std::string stdout;
                    std::string stderr;
                    Shell("chown", args, false, "", stdout, stderr);
                }
            }
        }
    }
	else if (command == "file-immutable-set")
	{
		std::string path = params["path"];
		int flag = atoi(params["flag"].c_str());

		int result = FileImmutableSet(path, flag);

		ReplyCommand(commandId, std::to_string(result));
	}
	else if (command == "dns-flush")
	{
        int pidSystemD = 0;
        ShellResult pidofResult = ShellEx1("pidof", "systemd");
        if(pidofResult.exit == 0)
		    pidSystemD = atoi(pidofResult.out.c_str());
        
        std::map<std::string, int> restarted;
		std::vector<std::string> services = StringToVector(params["services"], ';');

		if (pidSystemD != 0)
		{
			std::string servicePath = LocateExecutable("service");
			std::string systemctlPath = LocateExecutable("systemctl");

			if ((servicePath != "") && (systemctlPath != ""))
			{
                ShellResult systemCtlListUnits = ShellEx2(systemctlPath, "list-units", "--no-pager");
                if(systemCtlListUnits.exit != 0)
                {
                    std::vector<std::string> lines = StringToVector(systemCtlListUnits.out, '\n');
                    for (std::vector<std::string>::const_iterator l = lines.begin(); l != lines.end(); ++l)
                    {
                        std::string line = *l;
                        for (std::vector<std::string>::const_iterator iS = services.begin(); iS != services.end(); ++iS)
                        {
                            std::string service = *iS;
                            if (restarted.find(service) != restarted.end())
                                continue;
                            if (StringStartsWith(line, service + ".service") == false)
                                continue;
                            if (StringContain(line, " running ") == false)
                                continue;
                            
                            LogRemote("Flush DNS - " + service + " via systemd");
                            ShellEx2(servicePath, StringEnsureSecure(service), "restart");
                            restarted[service] = 1;
                        }
                    }
                }
			}
            
            // Special case - Not need, restarted above
            /*
            std::string systemdResolvePath = LocateExecutable("systemd-resolve");
            if (systemdResolvedPath != "")
			    ShellOut1(systemdResolvedPath, "--flush-caches");
            */
		}
        
        // InitD
		{
			for (std::vector<std::string>::const_iterator i = services.begin(); i != services.end(); ++i)
			{
				std::string service = *i;
				if (restarted.find(service) == restarted.end())
				{
					if (FileExists("/etc/init.d/" + service))
					{
                        LogRemote("Flush DNS - " + service + " via init.d");
						ShellEx1("/etc/init.d/" + service, "restart");
					}
				}
			}
		}
        
        // Other specific approach
        {
            for (std::vector<std::string>::const_iterator i = services.begin(); i != services.end(); ++i)
            {
                std::string service = *i;
                if (restarted.find(service) == restarted.end())
                {
                    if(service == "nscd")
                    {
                        // Special case
                        // On some system, for example Fedora, nscd caches are saved to disk,
                        // located in /var/db/nscd, and not flushed with a simple restart. 
                        std::string nscdPath = LocateExecutable("nscd");
                        if (nscdPath != "")
                        {
                            LogRemote("Flush DNS - nscd");
                            ShellEx1(nscdPath, "--invalidate=hosts");
                        }
                    }
                }
            }
        }

		ReplyCommand(commandId, "1");
	}
	else if (command == "dns-switch-rename-do")
	{
		std::string resolvconf = params["text"];

		if (FileExists("/etc/resolv.conf.eddie") == false)
		{
			if (FileExists("/etc/resolv.conf"))
			{
				if (FileMove("/etc/resolv.conf", "/etc/resolv.conf.eddie") == false)
					ThrowException("Move fail");
			}
		}
		if (FileWriteText("/etc/resolv.conf", resolvconf) == false)
			ThrowException("Write fail");
		chmod("/etc/resolv.conf", S_IRUSR | S_IWUSR | S_IRGRP | S_IROTH);
	}
	else if (command == "dns-switch-rename-restore")
	{
		if (FileExists("/etc/resolv.conf.eddie"))
		{
			if (FileExists("/etc/resolv.conf"))
				FileDelete("/etc/resolv.conf");
			FileMove("/etc/resolv.conf.eddie", "/etc/resolv.conf");
			ReplyCommand(commandId, "1");
		}
		else
			ReplyCommand(commandId, "0");
	}
    else if (command == "ipv6-block")
    {   
        std::vector<std::string> interfaces = FilesInPath("/proc/sys/net/ipv6/conf");
        for (std::vector<std::string>::const_iterator i = interfaces.begin(); i != interfaces.end(); ++i)
        {
            std::string interfaceName = *i;
            
            if(interfaceName == "all") // When switch all, all others autoswitch. Prefer a specific approach.
                continue;
                
            if(interfaceName == "lo")
                continue;
                
            std::string curVal = StringTrim(FileReadText("/proc/sys/net/ipv6/conf/" + interfaceName + "/disable_ipv6"));
            
            if(curVal == "1")
            {
                // Nothing to do
            }
            else if(curVal == "0")
            {
                ShellResult switchResult = ShellEx2(LocateExecutable("sysctl"), "-w", "net.ipv6.conf." + interfaceName + ".disable_ipv6=1");
                if(switchResult.exit == 0)
                {
                    ReplyCommand(commandId, interfaceName);
                }
                else
                {
                    ThrowException("Unexpected error when disable IPv6 on interface " + interfaceName);
                }
            }
            else
            {
                // Unexpected, nothing to do
                LogRemote("Unexpected status " + curVal + " of disable_ipv6 for interface " + interfaceName);
            }
        }
    }
    else if (command == "ipv6-restore")
    {
        std::string interfaceName = params["interface"];
        ShellResult switchResult = ShellEx2(LocateExecutable("sysctl"), "-w", "net.ipv6.conf." + interfaceName + ".disable_ipv6=0");
        if(switchResult.exit == 0)
        {
        }
        else
        {
            // Ignore
        }
    }
    else if (command == "netlock-iptables-available")
    {
        bool available = true;
        if(IptablesExecutable("ipv4","") == "")
            available = false;
        if(IptablesExecutable("ipv4","save") == "")
            available = false;
        if(IptablesExecutable("ipv4","restore") == "")
            available = false;
        if(IptablesExecutable("ipv6","") == "")
            available = false;
        if(IptablesExecutable("ipv6","save") == "")
            available = false;
        if(IptablesExecutable("ipv6","restore") == "")
            available = false;
        ReplyCommand(commandId, available ? "1":"0");
    }
    else if (command == "netlock-iptables-activate")
    {
        std::string pathIPv4 = GetTempPath("netlock_iptables_backup_ipv4.txt");
        std::string pathIPv6 = GetTempPath("netlock_iptables_backup_ipv6.txt");
        
        std::string result = "";
        
        if( (FileExists(pathIPv4)) || (FileExists(pathIPv6)) )
        {
            ThrowException("Unexpected: Already active");
        }
        else
        {
            // Try to up kernel module - For example standard Debian 8 KDE don't have it at boot
            std::string modprobePath = LocateExecutable("modprobe");
            if(modprobePath != "")
            {
                if(params.count("rules-ipv4")>0)
                {
                    ShellResult modprobeIptable4FilterResult = ShellEx1(modprobePath, "iptable_filter");
                    if(modprobeIptable4FilterResult.exit != 0)
                        ThrowException("Unable to initialize iptable_filter module");
                }
                
                if(params.count("rules-ipv6")>0)
                {
                    ShellResult modprobeIptable6FilterResult = ShellEx1(modprobePath, "ip6table_filter");
                    if(modprobeIptable6FilterResult.exit != 0)
                        ThrowException("Unable to initialize ip6table_filter module");
                }
            }
                
            // Backup of current
            std::vector<std::string> args;
            
            if(params.count("rules-ipv4")>0)
            {
                std::string backupIPv4 = IptablesExec(IptablesExecutable("ipv4","save"), args, false, "");
                if(StringContain(backupIPv4, "*filter") == false)
                    ThrowException("iptables don't reply, probabily kernel modules issue");
                FileWriteText(pathIPv4,backupIPv4);
            }
            if(params.count("rules-ipv6")>0)
            {
                std::string backupIPv6 = IptablesExec(IptablesExecutable("ipv6","save"), args, false, "");
                if(StringContain(backupIPv6, "*filter") == false)
                    ThrowException("ip6tables don't reply, probabily kernel modules issue");
                FileWriteText(pathIPv6,backupIPv6);
            }
            
            // Apply new
            if(params.count("rules-ipv4")>0)
                result += IptablesExec(IptablesExecutable("ipv4","restore"), args, true, params["rules-ipv4"]);
            if(params.count("rules-ipv6")>0)
                result += IptablesExec(IptablesExecutable("ipv6","restore"), args, true, params["rules-ipv6"]);
        }
        
        ReplyCommand(commandId, StringTrim(result));
    }
    else if (command == "netlock-iptables-deactivate")
    {
        std::string pathIPv4 = GetTempPath("netlock_iptables_backup_ipv4.txt");
        std::string pathIPv6 = GetTempPath("netlock_iptables_backup_ipv6.txt");
        
        //bool wasActive = false;
        std::string result = "";
        
        if(FileExists(pathIPv4))
        {
            //wasActive = true;
            
            std::vector<std::string> args;
            std::string body = FileReadText(pathIPv4);
            result += IptablesExec(IptablesExecutable("ipv4","restore"), args, true, body);
            
            FileDelete(pathIPv4);
        }
        
        if(FileExists(pathIPv6))
        {
            //wasActive = true;
            
            std::vector<std::string> args;
            std::string body = FileReadText(pathIPv6);
            result += IptablesExec(IptablesExecutable("ipv6","restore"), args, true, body);
            
            FileDelete(pathIPv6);
        }
        
        ReplyCommand(commandId, StringTrim(result));
    }
	else if (command == "netlock-iptables-accept-ip")
	{
		std::string path = "";
		std::vector<std::string> args1;
		std::vector<std::string> args2;
		bool stdinWrite = false;
		std::string stdinBody = "";
		int nCommands = 1;
        
        if (params["layer"] == "ipv4")
			path = IptablesExecutable("ipv4","");
		else if (params["layer"] == "ipv6")
			path = IptablesExecutable("ipv6","");
		else
			ThrowException("Unknown layer");

		std::string directionName = "";
		std::string directionIp = "";
		if (params["direction"] == "in")
		{
			directionName = "INPUT";
			directionIp = "-s";
		}
		else if (params["direction"] == "out")
		{
			directionName = "OUTPUT";
			directionIp = "-d";
		}
		else
			ThrowException("Unknown direction");

		if (params["action"] == "add")
		{
			args1.push_back("-I");
			args1.push_back(directionName);
			args1.push_back("1");
			args1.push_back(directionIp);
		}
		else if (params["action"] == "del")
		{
			args1.push_back("-D");
			args1.push_back(directionName);
			args1.push_back(directionIp);
		}
		else
			ThrowException("Unknown action");

		args1.push_back(StringEnsureCidr(params["cidr"]));
		args1.push_back("-j");
		args1.push_back("ACCEPT");

		// Additional rule for incoming
		if ( (directionName == "INPUT") && (directionIp == "-s") )
		{
			nCommands++;
			if (params["action"] == "add")
			{
				args2.push_back("-I");
				args2.push_back("OUTPUT");
				args2.push_back("1");
				args2.push_back("-d");
			}
			else if (params["action"] == "del")
			{
				args2.push_back("-D");
				args2.push_back("OUTPUT");
				args2.push_back("-d");
			}
			else
				ThrowException("Unknown action");

			args2.push_back(StringEnsureCidr(params["cidr"]));
			args2.push_back("-m");
			args2.push_back("state");
			args2.push_back("--state");
			args2.push_back("ESTABLISHED");
			args2.push_back("-j");
			args2.push_back("ACCEPT");
		}
		
		std::string output = "";

		for (int l = 0; l < nCommands; l++)
		{
            std::vector<std::string>* args;
			if(l == 0)
				args = &args1;
			else
				args = &args2;

			output += IptablesExec(path, *args, stdinWrite, stdinBody);
		}
        
        ReplyCommand(commandId, StringTrim(output));
	}
	else if (command == "route")
	{
		std::vector<std::string> args;

		if (params["layer"] == "ipv4")
			args.push_back("-4");
		else if (params["layer"] == "ipv6")
			args.push_back("-6");
		args.push_back("route");
		args.push_back(StringEnsureSecure(params["action"]));
		args.push_back(StringEnsureCidr(params["cidr"]));
		args.push_back("via");
		args.push_back(StringEnsureIpAddress(params["gateway"]));
		if (params["interface"] != "")
		{
			args.push_back("dev");
			args.push_back(StringEnsureSecure(params["interface"]));
		}
		if (params["metric"] != "")
		{
			args.push_back("metric");
			args.push_back(StringEnsureNumericInt(params["metric"]));
		}
        
        ShellResult routeResult = ShellEx(LocateExecutable("ip"), args);
        bool accepted = (routeResult.exit == 0);
        
		if (params["action"] == "delete")
		{
			// Still accepted: The device are not available anymore, so the route are already deleted.
			if (StringContain(StringToLower(routeResult.output()), "cannot find device"))
                accepted = true;

			// Still accepted: Already deleted.
			if (StringContain(StringToLower(routeResult.output()), "no such process"))
				accepted = true;
		}
        
        if (accepted == false)
			ThrowException(routeResult.err);
	}
	else
	{
		IPosix::Do(commandId, command, params);
	}
}

std::string Impl::CheckIfClientPathIsAllowed(const std::string& path)
{
    // Missing under Linux: in other platform (Windows, macOS) check if signature of client match.
    // LocalLog("Checking if " + path + " is allowed");
    return "ok";
}

std::string Impl::GetProcessPathCurrent()
{
	char buffer[4096];
	int n = readlink("/proc/self/exe", buffer, sizeof(buffer));
    if(n == -1)
    {
        ThrowException("Fail to detect exe path");
        return "";
    }
    else
    {
	    std::string path = std::string(buffer, n);
        return path;
    }
}

std::string Impl::GetProcessPathOfID(int pid)
{
    char fullPath[FILENAME_MAX];
    std::string procExePath = "/proc/" + std::to_string(pid) + "/exe";
    int length = readlink(procExePath.c_str(), fullPath, sizeof(fullPath));
    if (length < 0)
        return "";
    else if (length >= FILENAME_MAX)
        return "";
    else
    {
        std::string path = std::string(fullPath, length);
        
        /*
        // Exception: If mono, detect Assembly
        bool isMono = false;
        std::string pathMono1 = LocateExecutable("mono");
        if( (pathMono1 != "") && (StringStartsWith(path, pathMono1)) )
            isMono = true;
        std::string pathMono2 = LocateExecutable("mono-sgen");
        if( (pathMono2 != "") && (StringStartsWith(path, pathMono2)) )
            isMono = true;
        if(isMono)
        {
            std::string procCmdLinePath = "/proc/" + std::to_string(pid) + "/cmdline";
            if(FileExists(procCmdLinePath))
            {
                std::string cmdline = FileReadText(procCmdLinePath);
                std::vector<std::string> args = StringToVector(cmdline, '\0', false);
                // TOFIX: don't work, cmdline arguments are separated by null terminator.
                for (std::vector<std::string>::const_iterator l = args.begin(); l != args.end(); ++l)
                {
                }
            }
        }
        */
        
        return path;
    }
}

int Impl::FileImmutableSet(const std::string& path, const int flag)
{
	const char* filename = path.c_str();
	int result = -1;
	FILE *fp;

	if ((fp = fopen(filename, "r")) != NULL)
	{
		int fd = fileno(fp);

		int attr = 0;
		if (ioctl(fd, FS_IOC_GETFLAGS, &attr) != -1)
		{
			attr = flag ? (attr | FS_IMMUTABLE_FL) : (attr & ~FS_IMMUTABLE_FL);

			if (ioctl(fd, FS_IOC_SETFLAGS, &attr) != -1)
				result = 0;
		}

		fclose(fp);
	}
    return result;
}

std::string Impl::IptablesExecutable(const std::string& layer, const std::string& action)
{
    std::string name = "";
    
    if(layer == "ipv4")
        name += "iptables";
    else if(layer == "ipv6")
        name += "ip6tables";
    else
        return "";
        
    if(action != "")
        name += "-" + action;
        
    std::string legacyName = StringReplaceAll(name, "tables", "tables-legacy");
    std::string legacyPath = LocateExecutable(legacyName);
    if(legacyPath != "")
        return legacyPath;
    std::string path = LocateExecutable(name);
    return path;
}

std::string Impl::IptablesExec(const std::string& path, const std::vector<std::string>& args, const bool stdinWrite, const std::string stdinBody)
{
    std::string stdout;
    std::string stderr;
    bool done = false;
    std::string output;

    for (int t = 0; t < 10; t++)
    {
        int exitCode = Shell(path, args, stdinWrite, stdinBody, stdout, stderr);
        
        if (StringContain(StringToLower(stderr), "temporarily unavailable")) // Older Debian (iptables without --wait)
        {
            usleep(500000);
            continue;
        }

        if (StringContain(StringToLower(stderr), "xtables lock")) // Newest Debian (iptables with --wait but not automatic)
        {
            usleep(500000);
            continue;
        }

        if (exitCode != 0)
            ThrowException(stderr);
        else
        {
            done = true;
            output += stdout + "\n";
            break;
        }
    }
    
    if(done == false)
        ThrowException(stderr);
        
    return output;
}