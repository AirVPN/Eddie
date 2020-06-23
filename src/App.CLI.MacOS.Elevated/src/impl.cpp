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
#include <sys/ioctl.h>
#include <sys/stat.h>
#include <unistd.h>
#include <libproc.h>
#include <arpa/inet.h>
//#include <mach-o/dyld.h>

#include <sys/types.h> // for signal()
#include <signal.h> // for signal();
#include <sys/sysctl.h> // for kinfo_proc

// --------------------------
// Virtual
// --------------------------

std::string serviceName = "eddie-elevated";
std::string serviceDesc = "Eddie Elevation";
std::string launchdPath = "/Library/LaunchDaemons/org.airvpn.eddie.ui.elevated.plist";

int Impl::Main()
{
	signal(SIGINT, SIG_IGN); // See comment in Linux implementation

	setuid(0); // RootLauncher::AuthorizationExecuteWithPrivileges run elevated with superuser, but without change uid. Forced here.

	return IBSD::Main();
}

void Impl::Do(const std::string& commandId, const std::string& command, std::map<std::string, std::string>& params)
{
	if (command == "compatibility-profiles")
	{
		const std::string& dataPath = params["path-data"];

		if (FsFileExists(dataPath) == false)
		{
			// Rename old .airvpn to .eddie if exists, and change owner (<2.17.3 are root-only)
			std::string oldPath = dataPath;
			size_t pos = oldPath.find_last_of("/");
			if (pos != std::string::npos)
			{
				oldPath = oldPath.substr(0, pos) + "/../.airvpn";
				if (FsFileExists(oldPath))
				{
					const std::string& newOwner = params["owner"];
					FsFileMove(oldPath, dataPath);
					std::vector<std::string> args;
					args.push_back("-R");
					args.push_back(newOwner);
					args.push_back(dataPath);
					std::string stdout;
					std::string stderr;
					Shell(FsLocateExecutable("chown"), args, false, "", stdout, stderr);
				}
			}
		}
	}
	else if (command == "shortcut-cli")
	{
		std::string action = params["action"];
		std::string pathExecutable = params["path"];
		std::string pathShortcut = "/usr/local/bin/eddie-cli";
		if(action == "set")
		{
			FsFileWriteText(pathShortcut, "#! /bin/bash\n\"" + pathExecutable + "\" -cli $@");
			chmod(pathShortcut.c_str(), S_IRUSR | S_IWUSR | S_IXUSR | S_IRGRP | S_IXGRP | S_IROTH | S_IXOTH);
		}
		else if(action == "del")
		{
			if(FsFileExists(pathShortcut))
				FsFileDelete(pathShortcut);
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
		// 10.5 - 10.6
		std::string dscacheutilPath = FsLocateExecutable("dscacheutil");
		if (dscacheutilPath != "")
		{
			LogRemote("Flush DNS via dscacheutil");
			ShellEx1(dscacheutilPath, "-flushcache");
		}

		// 10.7 - 10.8 - 10.9 - 10.10.4 - 10.11 - Sierra 10.12.0 - 10.14 Mojave
		std::string killallPath = FsLocateExecutable("killall");
		if (killallPath != "")
		{
			LogRemote("Flush DNS via nDNSResponder");
			ShellEx2(killallPath, "-HUP", "mDNSResponder");
		}

		// 10.10.0 - 10.10.3
		std::string discoveryutilPath = FsLocateExecutable("discoveryutil");
		if (discoveryutilPath != "")
		{
			LogRemote("Flush DNS via discoveryutil");
			ShellEx1(discoveryutilPath, "udnsflushcaches");
			ShellEx1(discoveryutilPath, "mdnsflushcache");
		}
	}
	else if (command == "dns-switch-do")
	{
		std::string networksetupPath = FsLocateExecutable("networksetup");

		std::vector<std::string> newDns = StringToVector(params["dns"], ',');

		std::vector<std::string> interfaces = GetNetworkInterfaces();
		for (std::vector<std::string>::const_iterator i = interfaces.begin(); i != interfaces.end(); ++i)
		{
			std::string interfaceName = *i;

			ShellResult outputDns = ShellEx2(networksetupPath, "-getdnsservers", interfaceName);
			if (outputDns.exit == 0)
			{
				std::vector<std::string> outputDnsLines = StringToVector(outputDns.out, '\n');
				std::vector<std::string> interfaceDns;
				for (std::vector<std::string>::const_iterator i = outputDnsLines.begin(); i != outputDnsLines.end(); ++i)
				{
					if (StringStartsWith(*i, "There aren't any"))
						continue;
					std::string ip = StringEnsureIpAddress(*i);
					if (ip != "")
					{
						interfaceDns.push_back(ip);
					}
				}

				if (StringVectorsEqualOrdered(newDns, interfaceDns) == false)
				{
					// Switch
					std::vector<std::string> args;
					args.push_back("-setdnsservers");
					args.push_back(interfaceName);
					if (newDns.size() == 0)
						args.push_back("EMPTY");
					else
					{
						for (std::vector<std::string>::const_iterator iDns = newDns.begin(); iDns != newDns.end(); ++iDns)
							args.push_back(*iDns);
					}
					ShellResult switchResult = ShellEx(networksetupPath, args);
					if (switchResult.exit == 0)
					{
						ReplyCommand(commandId, "SwitchDNS;" + interfaceName + ";" + StringFromVector(interfaceDns, ","));
					}
				}
				else
				{
					// No need
				}
			}
		}
	}
	else if (command == "dns-switch-restore")
	{
		std::string networksetupPath = FsLocateExecutable("networksetup");

		std::string interfaceName = params["interface"];
		std::vector<std::string> newDns = StringToVector(params["dns"], ',');

		std::vector<std::string> args;
		args.push_back("-setdnsservers");
		args.push_back(interfaceName);
		if (newDns.size() == 0)
			args.push_back("EMPTY");
		else
		{
			for (std::vector<std::string>::const_iterator iDns = newDns.begin(); iDns != newDns.end(); ++iDns)
				args.push_back(*iDns);
		}
		ShellEx(networksetupPath, args);
	}
	else if (command == "ipv6-block")
	{
		std::string networksetupPath = FsLocateExecutable("networksetup");

		std::vector<std::string> newDns = StringToVector(params["dns"], ',');

		std::vector<std::string> interfaces = GetNetworkInterfaces();
		for (std::vector<std::string>::const_iterator i = interfaces.begin(); i != interfaces.end(); ++i)
		{
			std::string interfaceName = *i;

			ShellResult infoResult = ShellEx2(networksetupPath, "-getinfo", interfaceName);
			if (infoResult.exit == 0)
			{
				std::string ipv6Mode = StringTrim(StringExtractBetween(infoResult.out, "IPv6: ", "\n"));
				std::string ipv6Address = StringTrim(StringExtractBetween(infoResult.out, "IPv6 IP address: ", "\n"));
				std::string ipv6Router = "";
				std::string ipv6Prefix = "";

				if ((ipv6Mode == "") && (ipv6Address != ""))
					ipv6Mode = "LinkLocal";

				if (ipv6Mode != "Off")
				{
					if (ipv6Mode == "Manual")
					{
						ipv6Router = StringTrim(StringExtractBetween(infoResult.out, "IPv6 IP Router: ", "\n"));
						ipv6Prefix = StringTrim(StringExtractBetween(infoResult.out, "IPv6 Prefix Length: ", "\n"));
					}

					ShellResult switchResult = ShellEx2(networksetupPath, "-setv6off", interfaceName);
					if (switchResult.exit == 0)
					{
						ReplyCommand(commandId, "SwitchIPv6;" + interfaceName + ";" + ipv6Mode + ";" + ipv6Address + ";" + ipv6Router + ";" + ipv6Prefix);
					}
				}
			}
		}
	}
	else if (command == "ipv6-restore")
	{
		std::string networksetupPath = FsLocateExecutable("networksetup");

		std::string interfaceName = params["interface"];
		std::vector<std::string> newDns = StringToVector(params["dns"], ',');

		std::vector<std::string> args;

		if (params["mode"] == "Off")
		{
			args.push_back("-setv6off");
			args.push_back(interfaceName);
		}
		else if (params["mode"] == "Automatic")
		{
			args.push_back("-setv6automatic");
			args.push_back(interfaceName);
		}
		else if (params["mode"] == "LinkLocal")
		{
			args.push_back("-setv6LinkLocal");
			args.push_back(interfaceName);
		}
		else if (params["mode"] == "Manual")
		{
			args.push_back("-setv6manual");
			args.push_back(interfaceName);
			args.push_back(params["address"]);
			args.push_back(params["prefix"]);
			args.push_back(params["router"]);
		}

		ShellEx(networksetupPath, args);
	}
	else if (command == "netlock-pf-activate")
	{
		std::string pfPath = FsLocateExecutable("pfctl");

		std::string status = "";
		{
			std::vector<std::string> args;
			args.push_back("-si");
			ShellResult pfctlStatusResult = ShellEx(pfPath, args);
			if (pfctlStatusResult.exit != 0)
				ThrowException("Unexpected status: " + pfctlStatusResult.dump());
			status = StringToLower(pfctlStatusResult.dump());
		}

		std::string result = "";

		bool prevActive = false;
		if (StringContain(status, "status: enabled"))
			prevActive = true;
		else if (StringContain(status, "status: disabled"))
			prevActive = false;
		else
			ThrowException("Unexpected status");

		if (prevActive == false)
		{
			// Activate
			std::vector<std::string> args;
			args.push_back("-e");
			ShellResult pfctlActivationResult = ShellEx(pfPath, args);
			if (pfctlActivationResult.exit != 0)
				ThrowException("Activation failure: " + pfctlActivationResult.dump());
			if (StringContain(StringToLower(pfctlActivationResult.dump()), "pf enabled") == false)
				ThrowException("Activation failure: " + pfctlActivationResult.dump());
		}

		if (prevActive == false)
		{
			ReplyCommand(commandId, "activated");
		}
		else
		{
			ReplyCommand(commandId, "active");
		}
	}
	else if (command == "netlock-pf-deactivate")
	{
		std::string pfPath = FsLocateExecutable("pfctl");

		// Restore
		{
			std::vector<std::string> args;
			args.push_back("-v");
			args.push_back("-f");
			args.push_back("/etc/pf.conf"); // Maybe better
			ShellResult pfctlRestoreResult = ShellEx(pfPath, args);
			if (pfctlRestoreResult.exit != 0)
				ThrowException("Restore failure: " + pfctlRestoreResult.dump());
		}

		if (params["prev"] == "disabled")
		{
			std::vector<std::string> args;
			args.push_back("-d");
			ShellResult pfctlDeactivationResult = ShellEx(pfPath, args);
			if (pfctlDeactivationResult.exit != 0)
				ThrowException("Deactivation failure: " + pfctlDeactivationResult.dump());
		}
	}
	else if (command == "netlock-pf-update")
	{
		std::string pfPath = FsLocateExecutable("pfctl");
		std::string tmpPath = GetTempPath("netlock_pf.conf");
		std::vector<std::string> args;
		args.push_back("-v");
		args.push_back("-f");
		args.push_back(tmpPath);

		FsFileWriteText(tmpPath, params["config"]);
		ShellResult pfctlApplyResult = ShellEx(pfPath, args);
		FsFileDelete(tmpPath);

		if (pfctlApplyResult.exit != 0)
		{
			LogRemote("Dump pfctl output: " + pfctlApplyResult.dump());
			LogRemote("Dump pfctl conf: " + params["config"]);
			ThrowException("Rules not loaded");
		}
	}
	else if (command == "route")
	{
		std::vector<std::string> args;

		args.push_back("-n");

		if (params["action"] == "add")
			args.push_back("add");
		else if (params["action"] == "delete")
			args.push_back("delete");
		else
			ThrowException("Unexpected route action");

		if (params["layer"] == "ipv6")
			args.push_back("-inet6");

		args.push_back(StringEnsureCidr(params["cidr"]));
		args.push_back(StringEnsureIpAddress(params["gateway"]));

		ShellResult routeResult = ShellEx(FsLocateExecutable("route"), args);
		bool accepted = (routeResult.exit == 0);

		if (params["action"] == "delete")
		{
			// Still accepted: The device are not available anymore, so the route are already deleted.
			if (StringContain(StringToLower(routeResult.dump()), "cannot find device"))
				accepted = true;

			// Still accepted: Already deleted.
			if (StringContain(StringToLower(routeResult.dump()), "no such process"))
				accepted = true;

			// Still accepted: Already deleted.
			if (StringContain(StringToLower(routeResult.dump()), "not in table"))
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

bool Impl::IsServiceInstalled()
{
	return (FsFileExists(launchdPath));
}

bool Impl::ServiceInstall()
{
	std::string elevatedPath = GetProcessPathCurrent();
	
	if (FsFileExists(launchdPath))
	{
		ShellEx2(FsLocateExecutable("launchctl"), "unload", launchdPath);
	}

	std::string launchd = "";
	launchd += "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n";
	launchd += "<!DOCTYPE plist PUBLIC \"-//Apple/DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">\n";
	launchd += "<plist version=\"1.0\">\n";
	launchd += "    <dict>\n";

#ifdef Debug
	launchd += "        <key>StandardOutPath</key>\n";
	launchd += "        <string>/tmp/org.airvpn.eddie.ui.elevated.stdoutx</string>\n";
	launchd += "        <key>StandardErrorPath</key>\n";
	launchd += "        <string>/tmp/org.airvpn.eddie.ui.elevated.stderr</string>\n";
#endif
	launchd += "        <key>Label</key>\n";
	launchd += "        <string>org.airvpn.eddie.ui.elevated</string>\n";
	launchd += "           <key>ProgramArguments</key>\n";
	launchd += "        <array>\n";
	launchd += "            <string>" + StringXmlEncode(elevatedPath) + "</string>\n";
	launchd += "            <string>mode=service</string>\n";
	if (m_cmdline.find("service_port") != m_cmdline.end())
		launchd += "            <string>service_port=" + StringEnsureSecure(m_cmdline["service_port"]) + "</string>\n";
	std::string integrity = ComputeIntegrityHash(GetProcessPathCurrent(), "");
	launchd += "            <string>integrity=" + StringEnsureSecure(integrity) + "</string>\n";
	launchd += "        </array>\n";
	launchd += "        <key>RunAtLoad</key>\n";
	launchd += "        <true/>\n";
	launchd += "        <key>KeepAlive</key>\n";
	//launchd += "        <string>SuccessfulExit</string>\n";
	launchd += "        <true/>\n";
	launchd += "    </dict>\n";
	launchd += "</plist>\n";

	FsFileWriteText(launchdPath, launchd);

	ShellResult launchctlResult = ShellEx2(FsLocateExecutable("launchctl"), "load", launchdPath);
	return launchctlResult.exit;
}

bool Impl::ServiceUninstall()
{
    if (FsFileExists(launchdPath))
    {
	    ShellResult launchctlResult = ShellEx2(FsLocateExecutable("launchctl"), "unload", launchdPath);
        
		FsFileDelete(launchdPath);

	    return launchctlResult.exit;
    }
    else
        return 0;
}

std::string Impl::CheckIfClientPathIsAllowed(const std::string& path)
{
#ifdef Debug
	return "ok";
#else
	std::string localPath = GetProcessPathCurrent();
	std::string remotePath = path;

	std::string codesignPath = "/usr/bin/codesign"; // Note: Absolute path to avoid ENV

	// Check remote signature
	{
		std::vector<std::string> args;
		args.push_back("--verify");
		args.push_back(remotePath);
		std::string stdout;
		std::string stderr;
		ShellResult verifyResult = ShellEx(codesignPath, args);
		if (verifyResult.exit != 0)
		{
			LogLocal("Remote executable '" + remotePath + "' not signed");
			return "Remote executable '" + remotePath + "' not signed";
		}
	}

	// Check if remote signature authority match current
	{
		ShellResult infoLocal = ShellEx3(codesignPath, "-dv", "--verbose=4", localPath);
		if (infoLocal.exit != 0)
		{
			LogLocal("Unable to obtain signature of local");
			return "Unable to obtain signature of local";
		}
		std::string infoLocalFiltered = "";
		std::vector<std::string> infoLocalLines = StringToVector(infoLocal.out, '\n');
		for (std::vector<std::string>::const_iterator i = infoLocalLines.begin(); i != infoLocalLines.end(); ++i)
		{
			std::string line = *i;
			if (StringStartsWith(line, "Authority="))
				infoLocalFiltered += line + "\n";
		}

		ShellResult infoRemote = ShellEx3(codesignPath, "-dv", "--verbose=4", remotePath);
		if (infoRemote.exit != 0)
		{
			LogLocal("Unable to obtain signature of remote");
			return "Unable to obtain signature of remote";
		}
		std::string infoRemoteFiltered = "";
		std::vector<std::string> infoRemoteLines = StringToVector(infoRemote.out, '\n');
		for (std::vector<std::string>::const_iterator i = infoRemoteLines.begin(); i != infoRemoteLines.end(); ++i)
		{
			std::string line = *i;
			if (StringStartsWith(line, "Authority="))
				infoRemoteFiltered += line + "\n";
		}

		if (infoLocalFiltered != infoRemoteFiltered)
		{
			LogLocal("Remote Authority informations don't match");
			return "Remote Authority informations don't match";
		}
	}

	return "ok";
#endif
}

int Impl::GetProcessIdMatchingIPEndPoints(struct sockaddr_in& addrClient, struct sockaddr_in& addrServer)
{
	char sourceIp[256];
	char destIp[256];

	inet_ntop(AF_INET, &addrClient.sin_addr, sourceIp, sizeof(sourceIp));
	inet_ntop(AF_INET, &addrServer.sin_addr, destIp, sizeof(destIp));
	std::string sourceAddr = sourceIp;
	int sourcePort = ntohs(addrClient.sin_port);
	std::string destAddr = destIp;
	int destPort = ntohs(addrServer.sin_port);

	std::vector<std::string> args;
	args.push_back("-F");
	args.push_back("pfn");
	args.push_back("-anPi");
	args.push_back("4tcp@" + destAddr + ":" + std::to_string(destPort));

	std::string lsofPath = FsLocateExecutable("lsof");
	if (lsofPath != "")
	{
		ShellResult lsResult = ShellEx("lsof", args);
		if (lsResult.exit == 0)
		{
			std::vector<std::string> lines = StringToVector(lsResult.out, '\n');
			int lastPid = 0;
			std::string needle = sourceAddr + ":" + std::to_string(sourcePort) + "->" + destAddr + ":" + std::to_string(destPort);
			for (std::vector<std::string>::const_iterator i = lines.begin(); i != lines.end(); ++i)
			{
				std::string line = StringTrim(StringToLower(*i));

				if (line.length() == 0)
					continue;

				char ch = line.at(0);
				if (ch == 'p')
				{
					lastPid = strtol(line.c_str() + 1, NULL, 10);
				}
				else if (ch == 'n')
				{
					if (StringContain(line, needle))
						return lastPid;
				}
			}
		}
	}

	return 0;
}

void Impl::AddTorCookiePaths(const std::string& torPath, const std::string& username, std::vector<std::string>& result)
{
	if (username != "")
	{
		result.push_back("/Users/" + StringEnsureSecure(username) + "/Library/Application Support/TorBrowser-Data/Tor/control_auth_cookie");
	}

	IBSD::AddTorCookiePaths(torPath, username, result);
}

// --------------------------
// Virtual Pure, OS
// --------------------------

std::string Impl::GetProcessPathOfId(int pid)
{
	char buffer[4096];
	int ret = proc_pidpath(pid, buffer, sizeof(buffer));
	if (ret <= 0)
	{
		return "";
	}
	else
	{
		return std::string(buffer);
	}
}

pid_t Impl::GetParentProcessId(pid_t pid)
{
    struct kinfo_proc info;
    size_t length = sizeof(struct kinfo_proc);
    int mib[4] = { CTL_KERN, KERN_PROC, KERN_PROC_PID, pid };
    if (sysctl(mib, 4, &info, &length, NULL, 0) < 0)
        return 0;
    if (length == 0)
        return 0;
    return info.kp_eproc.e_ppid;
}

pid_t Impl::GetProcessIdOfName(const std::string& name)
{
	// TOFIX - Find a method without shell
	ShellResult pidofResult = ShellEx2("pgrep", "-x", name);
	if (pidofResult.exit == 0)
		return atoi(pidofResult.out.c_str());
	else
		return 0;
}

// --------------------------
// Private
// --------------------------

int Impl::FileImmutableSet(const std::string& path, const int flag)
{
	// sudo chflags schg /path/to/file
	// sudo chflags noschg /path/to/file

	int result = FileGetFlags(path);
	if (result == -1)
		return -1;

	result = flag ? (result | SF_IMMUTABLE) : (result & ~SF_IMMUTABLE);
	if (chflags(path.c_str(), result) == -1)
		return -1;

	return 0;
}

int Impl::FileGetFlags(const std::string& path)
{
	struct stat s;
	memset(&s, 0, sizeof(struct stat));

	if (stat(path.c_str(), &s) == -1)
		return -1;

	return (int)s.st_flags;
}

std::vector<std::string> Impl::GetNetworkInterfaces()
{
	ShellResult networksetupListResult = ShellEx1(FsLocateExecutable("networksetup"), "-listallnetworkservices");
	if (networksetupListResult.exit != 0)
		ThrowException("Unable to obtain network services list");

	std::vector<std::string> lines = StringToVector(networksetupListResult.out, '\n');

	std::vector<std::string> output;

	for (std::vector<std::string>::const_iterator i = lines.begin(); i != lines.end(); ++i)
	{
		std::string line = *i;
		line = StringTrim(line);
		if (line == "")
			continue;
		if (StringStartsWith(line, "An asterisk (*)"))
			continue;

		output.push_back(*i);
	}

	return output;
}

