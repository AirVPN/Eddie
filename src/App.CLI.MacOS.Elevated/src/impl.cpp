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

#include <fcntl.h>

#include "../include/impl.h"

#include <sys/ioctl.h>
#include <sys/stat.h>
#include <regex>
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
std::string serviceDesc = "Eddie VPN Elevation";
std::string serviceLauncherPath = "/Library/PrivilegedHelperTools/website.eddie.Helper";
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
					ExecEx(FsLocateExecutable("chown"), args);
				}
			}
		}
	}
	else if (command == "shortcut-cli")
	{
		std::string action = params["action"];
		std::string pathExecutable = params["path"];
		std::string pathShortcut = "/usr/local/bin/eddie-cli";
		if (action == "set")
		{
			FsDirectoryCreate("/usr/local/bin");
			FsFileWriteText(pathShortcut, "#! /bin/bash\n\"" + pathExecutable + "\" $@");
			chmod(pathShortcut.c_str(), S_IRUSR | S_IWUSR | S_IXUSR | S_IRGRP | S_IXGRP | S_IROTH | S_IXOTH);
		}
		else if (action == "del")
		{
			if (FsFileExists(pathShortcut))
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
		std::string dscacheutilPath = FsLocateExecutable("dscacheutil", false);
		if (dscacheutilPath != "")
		{
			// LogRemote("Flush DNS via dscacheutil");
			ExecEx1(dscacheutilPath, "-flushcache");
		}

		// 10.7 - 10.8 - 10.9 - 10.10.4 - 10.11 - Sierra 10.12.0 - 10.14 Mojave
		std::string killallPath = FsLocateExecutable("killall", false);
		if (killallPath != "")
		{
			// LogRemote("Flush DNS via nDNSResponder");
			ExecEx2(killallPath, "-HUP", "mDNSResponder");
		}

		// 10.10.0 - 10.10.3
		std::string discoveryutilPath = FsLocateExecutable("discoveryutil", false);
		if (discoveryutilPath != "")
		{
			// LogRemote("Flush DNS via discoveryutil");
			ExecEx1(discoveryutilPath, "udnsflushcaches");
			ExecEx1(discoveryutilPath, "mdnsflushcache");
		}
	}
	else if (command == "dns-switch-do")
	{
		std::string networksetupPath = FsLocateExecutable("networksetup");

		std::vector<std::string> newDns = StringToVector(params["dns"], ',');

		std::vector<std::string> interfaces = GetNetworkInterfacesNames();
		for (std::vector<std::string>::const_iterator i = interfaces.begin(); i != interfaces.end(); ++i)
		{
			std::string interfaceName = *i;

			ExecResult outputDns = ExecEx2(networksetupPath, "-getdnsservers", interfaceName);
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
					ExecResult switchResult = ExecEx(networksetupPath, args);
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
		ExecEx(networksetupPath, args);
	}
	else if (command == "ipv6-block")
	{
		std::string networksetupPath = FsLocateExecutable("networksetup");

		std::vector<std::string> newDns = StringToVector(params["dns"], ',');

		std::vector<std::string> interfaces = GetNetworkInterfacesNames();
		for (std::vector<std::string>::const_iterator i = interfaces.begin(); i != interfaces.end(); ++i)
		{
			std::string interfaceName = *i;

			ExecResult infoResult = ExecEx2(networksetupPath, "-getinfo", interfaceName);
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

					ExecResult switchResult = ExecEx2(networksetupPath, "-setv6off", interfaceName);
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

		ExecEx(networksetupPath, args);
	}
	else if (command == "netlock-pf-activate")
	{
		std::string pfPath = FsLocateExecutable("pfctl");

		std::string status = "";
		{
			std::vector<std::string> args;
			args.push_back("-si");
			ExecResult pfctlStatusResult = ExecEx(pfPath, args);
			if (pfctlStatusResult.exit != 0)
				ThrowException("Unexpected status: " + GetExecResultReport(pfctlStatusResult));
			status = StringToLower(GetExecResultReport(pfctlStatusResult));
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
			ExecResult pfctlActivationResult = ExecEx(pfPath, args);
			if (pfctlActivationResult.exit != 0)
				ThrowException("Activation failure: " + GetExecResultReport(pfctlActivationResult));
			if (StringContain(StringToLower(GetExecResultReport(pfctlActivationResult)), "pf enabled") == false)
				ThrowException("Activation failure: " + GetExecResultReport(pfctlActivationResult));
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
			ExecResult pfctlRestoreResult = ExecEx(pfPath, args);
			if (pfctlRestoreResult.exit != 0)
				ThrowException("Restore failure: " + GetExecResultReport(pfctlRestoreResult));
		}

		if (params["prev"] == "disabled")
		{
			std::vector<std::string> args;
			args.push_back("-d");
			ExecResult pfctlDeactivationResult = ExecEx(pfPath, args);
			if (pfctlDeactivationResult.exit != 0)
				ThrowException("Deactivation failure: " + GetExecResultReport(pfctlDeactivationResult));
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
		ExecResult pfctlApplyResult = ExecEx(pfPath, args);
		FsFileDelete(tmpPath);

		if (pfctlApplyResult.exit != 0)
		{
			LogRemote("Dump pfctl output: " + GetExecResultReport(pfctlApplyResult));
			LogRemote("Dump pfctl conf: " + params["config"]);
			ThrowException("Rules not loaded");
		}
	}
	else if (command == "network-interface-info")
	{
		ReplyCommand(commandId, GetNetworkInterfaceInfoAsJson(params["id"]));
	}
	else if (command == "route-list")
	{
		std::string json = GetRoutesAsJson();

		ReplyCommand(commandId, json);
	}
	else if (command == "route")
	{
		std::vector<std::string> args;

		args.push_back("-q");
		args.push_back("-n");

		if (params["action"] == "add")
			args.push_back("add");
		else if (params["action"] == "delete")
			args.push_back("delete");
		else
			ThrowException("Unexpected route action");

		if (params["layer"] == "ipv4")
			args.push_back("-inet");
		else if (params["layer"] == "ipv6")
			args.push_back("-inet6");
		else
			ThrowException("Unexpected IP layer");

		args.push_back(StringEnsureCidr(params["destination"]));
		if (params.find("gateway") != params.end())
		{
			args.push_back("-gateway");
			args.push_back(StringEnsureIpAddress(params["gateway"]));
		}
		else if (params.find("interface") != params.end())
		{
			args.push_back("-interface");
			args.push_back(StringEnsureInterfaceName(params["interface"]));
		}

		ExecResult execResult = ExecEx(FsLocateExecutable("route"), args);
		if (execResult.exit != 0)
			ThrowException(GetExecResultReport(execResult));
	}
	else if (command == "workaround-ipv6-dns-lookup-enable")
	{
		std::string iface = StringEnsureInterfaceName(params["iface"]);		

		std::string result = WorkaroundDnsLookupIPv6Enable(iface);		
		ReplyCommand(commandId, result);
	}
	else if (command == "workaround-ipv6-dns-lookup-disable")
	{
		std::string iface = StringEnsureInterfaceName(params["iface"]);

		std::string result = WorkaroundDnsLookupIPv6Disable(iface);		
		ReplyCommand(commandId, result);
	}
	else if (command == "wireguard-version")
	{
		std::string wgPath = FsLocateExecutable("wg", false, true); // Expected in macOS .app bundle
		std::string version = ExecEx1(wgPath, "version").out;
		version = StringReplaceAll(version, "wireguard-tools", "");
		version = StringTrim(version, "v ");
		size_t afterPos = version.find(' ');
		if(afterPos != std::string::npos)
			version = version.substr(0, afterPos);
		ReplyCommand(commandId, version);
	}
	else if (command == "wireguard")
	{
		std::string id = params["id"];
		std::string action = params["action"];
		std::string interfaceId = params["interface"].substr(0, 12);

		std::string keypairStopRequest = "wireguard_stop_" + id;

		if (action == "stop")
		{
			m_keypair[keypairStopRequest] = "stop";
		}
		else if (action == "start")
		{
			std::string config = params["config"];
			unsigned long handshakeTimeoutFirst = StringToULong(params["handshake_timeout_first"]);
			unsigned long handshakeTimeoutConnected = StringToULong(params["handshake_timeout_connected"]);

			FsFileWriteText("/tmp/testwg.conf", config);

			std::string varRunPath = "/var/run/wireguard";
			std::string interfaceAssigned = "";

			try
			{
				std::map<std::string, std::string> configmap = IniConfigToMap(config);

				ReplyCommand(commandId, "log:setup-start");

				FsDirectoryCreate(varRunPath);

				std::string ifConfigPath = FsLocateExecutable("ifconfig");
				std::string wireGuardGoPath = FsLocateExecutable("wireguard-go", true, true);
				std::string wgPath = FsLocateExecutable("wg", true, true);

				// Add interface

				std::string interfaceNamePath = FsGetTempPath() + FsPathSeparator + "eddie_wg_go_interface.tmp";
				SetEnv("WG_TUN_NAME_FILE", interfaceNamePath); // Will be filled with interface name
				FsFileDelete(interfaceNamePath);
				if (ExecEx1(wireGuardGoPath, "utun").exit != 0)
					ThrowException("Unable to add interface");
				interfaceAssigned = StringTrim(FsFileReadText(interfaceNamePath));
				FsFileDelete(interfaceNamePath);
				ReplyCommand(commandId, "log:interface-name:" + interfaceAssigned);

				// Set Config
				if (true)
				{
					std::string configSetConf = "";
					configSetConf += "[Interface]\n";
					if (configmap.find("interface.privatekey") != configmap.end())
						configSetConf += "PrivateKey = " + configmap["interface.privatekey"] + "\n";
					if (configmap.find("interface.listenport") != configmap.end())
						configSetConf += "ListenPort = " + configmap["interface.listenport"] + "\n";
					if (configmap.find("interface.fwmark") != configmap.end())
						configSetConf += "FwMark = " + configmap["interface.fwmark"] + "\n";
					configSetConf += "[Peer]\n";
					if (configmap.find("peer.publickey") != configmap.end())
						configSetConf += "PublicKey = " + configmap["peer.publickey"] + "\n";
					if (configmap.find("peer.presharedkey") != configmap.end())
						configSetConf += "PresharedKey = " + configmap["peer.presharedkey"] + "\n";
					if (configmap.find("peer.allowedips") != configmap.end())
						configSetConf += "AllowedIPs = " + configmap["peer.allowedips"] + "\n";
					if (configmap.find("peer.endpoint") != configmap.end())
						configSetConf += "Endpoint = " + configmap["peer.endpoint"] + "\n";
					if (configmap.find("peer.persistentkeepalive") != configmap.end())
						configSetConf += "PersistentKeepalive = " + configmap["peer.persistentkeepalive"] + "\n";

					std::string configSetConfPath = FsGetTempPath() + FsPathSeparator + "eddie_setconf.tmp.conf";
					FsFileWriteText(configSetConfPath, configSetConf);
					ExecResult result = ExecEx3(wgPath, "setconf", interfaceAssigned, configSetConfPath);
					FsFileDelete(configSetConfPath);
					if (result.exit != 0)
						ThrowException("Failed to configure interface '" + interfaceAssigned + "'");
				}

				// Add interface addresses
				if (configmap.find("interface.address") != configmap.end())
				{
					std::vector<std::string> interfaceAddresses = StringToVector(configmap["interface.address"], ',');
					for (std::vector<std::string>::const_iterator i = interfaceAddresses.begin(); i != interfaceAddresses.end(); ++i)
					{
						std::string address = *i;

						std::string layerArg = "";
						if (StringIsIPv4(address))
						{
							std::string addressWithoutCidr = address;
							std::vector<std::string> parts = StringToVector(address, '/');
							if ((parts.size() >= 1) && (ExecEx5(ifConfigPath, interfaceAssigned, "inet", address, parts[0], "alias").exit != 0))
								ThrowException("Failed to add address '" + address + "'");
						}
						else if (StringIsIPv6(address))
						{
							if (ExecEx4(ifConfigPath, interfaceAssigned, "inet6", address, "alias").exit != 0)
								ThrowException("Failed to add address '" + address + "'");
						}
						else
							ThrowException("Unknown address type '" + address + "'");
					}
				}

				if (configmap.find("interface.mtu") != configmap.end())
				{
					int mtu = StringToInt(configmap["interface.mtu"]);

					if (ExecEx3(ifConfigPath, interfaceAssigned, "mtu", StringFrom(mtu)).exit != 0)
						ThrowException("Failed to set mtu '" + StringFrom(mtu) + "'");
				}

				// Interface up
				if (ExecEx2(ifConfigPath, interfaceAssigned, "up").exit != 0)
					ThrowException("Failed to set interface '" + interfaceAssigned + "' up");

				ReplyCommand(commandId, "log:setup-complete");

				unsigned long handshakeStart = GetTimestampUnix();
				unsigned long handshakeLast = 0;

				for (;;)
				{
					unsigned long handshakeNow = WireGuardLastHandshake(wgPath, interfaceAssigned);

					if (handshakeLast != handshakeNow)
					{
						if (handshakeLast == 0)
						{
							// First
							ReplyCommand(commandId, "log:setup-interface");
							ReplyCommand(commandId, "log:handshake-first");
						}

						handshakeLast = handshakeNow;
					}

					unsigned long timeNow = GetTimestampUnix();
					if (handshakeLast > 0)
					{
						unsigned long handshakeDelta = timeNow - handshakeLast;

						if (handshakeDelta > handshakeTimeoutConnected)
						{
							ReplyCommand(commandId, "log:handshake-out"); // Too much, suggest disconnect
						}
					}
					else
					{
						unsigned long handshakeDelta = timeNow - handshakeStart;

						if (handshakeDelta > handshakeTimeoutFirst)
						{
							ReplyCommand(commandId, "log:handshake-out"); // Too much, suggest disconnect
						}
					}

					// Check stop requested
					if (m_keypair.find(keypairStopRequest) != m_keypair.end())
					{
						ReplyCommand(commandId, "log:stop-requested");
						break;
					}

					Sleep(1000);
				}
			}
			catch (std::exception& e)
			{
				ReplyCommand(commandId, "err:" + std::string(e.what()));
			}
			catch (...)
			{
				ReplyCommand(commandId, "err:Unexpected");
			}

			ReplyCommand(commandId, "log:stop-interface");

			if (interfaceAssigned != "")
			{
				std::string sockPath = varRunPath + "/" + interfaceAssigned + ".sock";
				if (FsFileExists(sockPath))
					FsFileDelete(sockPath);
			}

			m_keypair.erase(keypairStopRequest);

			ReplyCommand(commandId, "log:end");
		}
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
		ExecEx2(FsLocateExecutable("launchctl"), "unload", launchdPath);
	}

	std::string launchd = "";
	launchd += "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n";
	launchd += "<!DOCTYPE plist PUBLIC \"-//Apple/DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">\n";
	launchd += "<plist version=\"1.0\">\n";
	launchd += "    <dict>\n";

#ifdef Debug
	launchd += "        <key>StandardOutPath</key>\n";
	launchd += "        <string>/tmp/org.airvpn.eddie.ui.elevated.stdout</string>\n";
	launchd += "        <key>StandardErrorPath</key>\n";
	launchd += "        <string>/tmp/org.airvpn.eddie.ui.elevated.stderr</string>\n";
#endif
	launchd += "        <key>Label</key>\n";
	launchd += "        <string>org.airvpn.eddie.ui.elevated</string>\n";
	launchd += "        <key>ProgramArguments</key>\n";
	launchd += "        <array>\n";

	if (CheckIfExecutableIsAllowed(elevatedPath, false, true))
	{
	}
	else
	{
		std::string elevatedLauncherOrig = GetProcessPathCurrent() + "-service";
		
		if (FsFileCopy(elevatedLauncherOrig, serviceLauncherPath) == false)
			return false;				
		if (chown(serviceLauncherPath.c_str(), 0, 0) == -1)
			return false;			
		if (chmod(serviceLauncherPath.c_str(), 0700) == -1)
			return false;			
		if (CheckIfExecutableIsAllowed(serviceLauncherPath, false, true) == false)
			return false;		

		if (IntegrityCheckUpdate("service") == false)
			return false;
		
		launchd += "            <string>" + StringXmlEncode(serviceLauncherPath) + "</string>\n";
	}
	launchd += "            <string>" + StringXmlEncode(elevatedPath) + "</string>\n";
	launchd += "            <string>mode=service</string>\n";
	if (m_cmdline.find("service_port") != m_cmdline.end())
		launchd += "            <string>service_port=" + StringEnsureNumericInt(m_cmdline["service_port"]) + "</string>\n";
	launchd += "        </array>\n";
	launchd += "        <key>RunAtLoad</key>\n";
	launchd += "        <true/>\n";
	launchd += "        <key>KeepAlive</key>\n";
	//launchd += "        <string>SuccessfulExit</string>\n";
	launchd += "        <true/>\n";
	launchd += "    </dict>\n";
	launchd += "</plist>\n";

	FsFileWriteText(launchdPath, launchd);

	ExecResult launchctlResult = ExecEx2(FsLocateExecutable("launchctl"), "load", launchdPath);	
	return (launchctlResult.exit == 0);
}

bool Impl::ServiceUninstall()
{
	bool result = true;

	if (FsFileExists(launchdPath))
	{
		ExecResult launchctlResult = ExecEx2(FsLocateExecutable("launchctl"), "unload", launchdPath);
		if (launchctlResult.exit > 0)
			result = false;

		FsFileDelete(launchdPath);
	}

	if (FsFileExists(serviceLauncherPath))
	{
		FsFileDelete(serviceLauncherPath);
	}

	IntegrityCheckClean("service");
	
	return result;
}

std::vector<std::string> Impl::GetNetworkInterfacesNames()
{
	ExecResult networksetupListResult = ExecEx1(FsLocateExecutable("networksetup"), "-listallnetworkservices");
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
		if (StringStartsWith(line, "*")) // An asterisk (*) denotes that a network service is disabled
			continue;

		output.push_back(*i);
	}

	return output;
}

std::string Impl::SystemWideDataPath()
{
	return "/Library/PrivilegedHelperTools/website.eddie.Helper.dat";
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
		ExecResult verifyResult = ExecEx(codesignPath, args);
		if (verifyResult.exit != 0)
		{
			LogLocal("Remote executable '" + remotePath + "' not signed");
			return "Remote executable '" + remotePath + "' not signed";
		}
	}

	// Check if remote signature authority match current
	{
		ExecResult infoLocal = ExecEx3(codesignPath, "-dv", "--verbose=4", localPath);
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

		ExecResult infoRemote = ExecEx3(codesignPath, "-dv", "--verbose=4", remotePath);
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
		ExecResult lsResult = ExecEx("lsof", args);
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
		result.push_back("/Users/" + StringEnsureDirectoryName(username) + "/Library/Application Support/TorBrowser-Data/Tor/control_auth_cookie");
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
	// TOFIX - Find a method without exec
	ExecResult pidofResult = ExecEx2("pgrep", "-x", name);
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

std::string Impl::GetRoutesAsJson()
{
	int n = 0;
	std::string json;

	ExecResult netstatResult = ExecEx1(FsLocateExecutable("netstat"), "-rnl");
	if (netstatResult.exit == 0)
	{
		std::vector<std::string> header;
		std::vector<std::string> lines = StringToVector(netstatResult.out, '\n');
		for (std::vector<std::string>::const_iterator i = lines.begin(); i != lines.end(); ++i)
		{
			std::string line = StringTrim(*i);

			if (StringStartsWith(line, "Routing tables")) continue;
			if (StringStartsWith(line, "Internet:")) continue;
			if (StringStartsWith(line, "Internet6:")) continue;

			if (StringStartsWith(line, "Destination "))
			{
				header = StringToVector(line, ' ', true);
			}
			else
			{
				std::vector<std::string> fields = StringToVector(line, ' ', true);
				if (fields.size() == header.size() - 1) // Expire may missing
					fields.push_back("");
				if (fields.size() == header.size())
				{
					std::map<std::string, std::string> keypairs;					
					for (int f = 0; f < fields.size(); f++)
						keypairs[StringToLower(header[f])] = fields[f];

					// Adapt
					if (keypairs.find("destination") != keypairs.end())
					{
						if (keypairs["destination"] == "default")
						{
							if (keypairs.find("gateway") != keypairs.end())
							{
								if (StringIsIPv4(keypairs["gateway"]))
									keypairs["destination"] = "0.0.0.0/0";
								else if (StringIsIPv6(keypairs["gateway"]))
									keypairs["destination"] = "::/0";
							}
						}
						else if (StringContain(keypairs["destination"], "/") == false)
						{
							// Always full CIDR notation
							if (StringIsIPv4(keypairs["destination"]))
								keypairs["destination"] = keypairs["destination"] + "/32";
							else if (StringIsIPv6(keypairs["destination"]))
								keypairs["destination"] = keypairs["destination"] + "/128";
						}
						keypairs["destination"] = StringIpRemoveInterface(keypairs["destination"]);
					}
					if (keypairs.find("gateway") != keypairs.end())
						keypairs["gateway"] = StringIpRemoveInterface(keypairs["gateway"]);
					if (keypairs.find("netif") != keypairs.end())
					{
						keypairs["interface"] = keypairs["netif"];
						keypairs.erase("netif");
					}

					// Build JSON

					if (n > 0)
						json += ",\n";
					json += JsonFromKeyPairs(keypairs);
					n++;
				}
			}
		}
	}

	return "[" + json + "]";
}

std::string Impl::GetNetworkInterfaceInfoAsJson(const std::string& id)
{
	std::map<std::string, std::string> kp;
	
	// Step1: Set IPv6 support to true by default.
	// From C#/Mono, always 'false'. Missing Mono implementation? 
	// After for interfaces listed by 'networksetup -listallhardwareports' we detect specific support.

	kp["support_ipv4"] = "true";
	kp["support_ipv6"] = "true";

	// Step2: Query 'networksetup -listallhardwareports' to obtain a more accurate device friendly names.
	// TOFIX: Exists a better method?
	ExecResult listHardwareReportExec = ExecEx1(FsLocateExecutable("networksetup"),"-listallhardwareports");
	if(listHardwareReportExec.exit == 0)
	{
		std::vector<std::string> listHardwareReport = StringToVector(listHardwareReportExec.out, '\n');
		std::string name = "";
		for (std::vector<std::string>::const_iterator i = listHardwareReport.begin(); i != listHardwareReport.end(); ++i)
		{
			std::string line = *i;			
			if(StringStartsWith(line, "Hardware Port: "))
				name = StringTrim(line.substr(15));
			else if(StringStartsWith(line, "Device:"))
			{
				std::string deviceId = StringTrim(line.substr(8));
				if(deviceId == id)
				{
					// Found, query other info
					kp["friendly"] = name;

					ExecResult networkSetupGetInfoExec = ExecEx2(FsLocateExecutable("networksetup"), "-getinfo", name);
					if(networkSetupGetInfoExec.exit == 0)
					{
						std::vector<std::string> networkSetupGetInfo = StringToVector(networkSetupGetInfoExec.out, '\n');
						std::string name = "";
						for (std::vector<std::string>::const_iterator i = networkSetupGetInfo.begin(); i != networkSetupGetInfo.end(); ++i)
						{
							std::string linegi = *i;

							if(StringStartsWith(linegi, "IPv6: "))
							{
								std::string ipv6 = StringTrim(linegi.substr(6));								
								if(ipv6 == "Off")
									kp["support_ipv6"] = "false";
							}
						}
					}
					
					break;				
				}
			}
		}
	}
	
	return JsonFromKeyPairs(kp);
}

unsigned long Impl::WireGuardLastHandshake(const std::string& wgPath, const std::string& interfaceId)
{
	std::vector<std::string> dataArgs;
	dataArgs.push_back("show");
	dataArgs.push_back(interfaceId);
	dataArgs.push_back("dump");
	ExecResult result = ExecEx(wgPath, dataArgs, false);

	if (result.exit != 0)
		ThrowException("Unable to fetch status (1), dump: " + GetExecResultReport(result));
	std::vector<std::string> lines = StringToVector(result.out, '\n');
	if (lines.size() < 2)
		ThrowException("Unable to fetch status (2), dump:" + GetExecResultReport(result));
	std::vector<std::string> peerStats = StringToVector(lines[1], '\t');
	if (peerStats.size() < 4)
		ThrowException("Unable to fetch status (3), dump:" + GetExecResultReport(result));

	return StringToULong(peerStats[4]);
}

std::string Impl::WorkaroundDnsLookupIPv6Enable(const std::string& iface)
{
	// Workaround based on
	// https://gist.github.com/smammy/3247b5114d717d12b68c201000ab163d

	try
	{
		std::string serviceName = "eddie_tunnel_" + iface;
		std::string ifconfigPath = FsLocateExecutable("ifconfig");
		std::string scUtilPath = FsLocateExecutable("scutil");


		// Need? 
		ExecResult execCheck = ExecEx1(scUtilPath, "--dns");
		if (execCheck.exit != 0)
			ThrowException("Unexpected scutil --dns output: " + GetExecResultReport(execCheck));
		if (StringContain(execCheck.out, "Request AAAA records"))
			return "not_need_already";
				
		// Fetch data
		ExecResult ifconfigResult;
		ifconfigResult = ExecEx1(ifconfigPath, iface);
		if(ifconfigResult.exit != 0)
			ThrowException("Unexpected ifconfig output: " + GetExecResultReport(ifconfigResult));

		std::vector<std::string> scAddressesIPv4;
		std::vector<std::string> scDescAddressesIPv4;
		std::regex regexIPv4("\\s*inet\\s+(\\S+)\\s+-->\\s+(\\S+)\\s+netmask\\s+\\S+");
		auto regexIPv4begin = std::sregex_iterator(ifconfigResult.out.begin(), ifconfigResult.out.end(), regexIPv4);
		auto regexIPv4end = std::sregex_iterator();
		for (std::sregex_iterator i = regexIPv4begin; i != regexIPv4end; ++i)
		{
			std::smatch match = *i;

			scAddressesIPv4.push_back(match[1]);
			scDescAddressesIPv4.push_back(match[2]);
		}

		std::vector<std::string> scAddressesIPv6;
		std::vector<std::string> scDescAddressesIPv6;
		std::vector<std::string> scFlagsIPv6;
		std::vector<std::string> scPrefixLengthIPv6;
		std::regex regexIPv6("\\s*inet6\\s+(\\S+?)(?:%\\S+)?\\s+prefixlen\\s+(\\S+)");
		auto regexIPv6begin = std::sregex_iterator(ifconfigResult.out.begin(), ifconfigResult.out.end(), regexIPv6);
		auto regexIPv6end = std::sregex_iterator();
		for (std::sregex_iterator i = regexIPv6begin; i != regexIPv6end; ++i)
		{
			std::smatch match = *i;

			scAddressesIPv6.push_back(match[1]);
			if(StringStartsWith(match[1], "fe80")) // ?
				scDescAddressesIPv6.push_back("::ffff:ffff:ffff:ffff:0:0");
			else
				scDescAddressesIPv6.push_back("::");
			scFlagsIPv6.push_back("0");
			scPrefixLengthIPv6.push_back(match[2]);
		}

		if (scAddressesIPv6.size() == 0)
			return "not_need_noipv6";

		// Build scutil
		std::string sc;
		if (scAddressesIPv4.size() > 0)
		{
			sc += "d.init\n";
			sc += "d.add Addresses * " + StringFromVector(scAddressesIPv4, " ") + "\n";
			sc += "d.add DestAddresses * " + StringFromVector(scDescAddressesIPv4, " ") + "\n";
			sc += "d.add InterfaceName " + iface + "\n";
			sc += "set State:/Network/Service/" + serviceName + "/IPv4\n";
			sc += "set Setup:/Network/Service/" + serviceName + "/IPv4\n";
		}
		if (scAddressesIPv6.size() > 0)
		{
			sc += "d.init\n";
			sc += "d.add Addresses * " + StringFromVector(scAddressesIPv6, " ") + "\n";
			sc += "d.add DestAddresses * " + StringFromVector(scDescAddressesIPv6, " ") + "\n";
			sc += "d.add Flags * " + StringFromVector(scFlagsIPv6, " ") + "\n";
			sc += "d.add InterfaceName " + iface + "\n";
			sc += "d.add PrefixLength * " + StringFromVector(scPrefixLengthIPv6, " ") + "\n";
			sc += "set State:/Network/Service/" + serviceName + "/IPv6\n";
			sc += "set Setup:/Network/Service/" + serviceName + "/IPv6\n";
		}

		std::vector<std::string> args;
		ExecResult resultScutil = ExecEx(scUtilPath, args, sc);
		if(resultScutil.exit != 0)
			ThrowException("Unexpected scutil output: " + GetExecResultReport(resultScutil));

		return "ok";
	}
	catch (std::exception& ex)
	{
		std::string msg = "WorkaroundDnsLookupIPv6Enable: " + std::string(ex.what());
		return "err:" + msg;
	}	
}

std::string Impl::WorkaroundDnsLookupIPv6Disable(const std::string& iface)
{
	try
	{
		std::string serviceName = "eddie_tunnel_" + iface;
		std::string scUtilPath = FsLocateExecutable("scutil");

		std::string sc;
		sc += "remove State:/Network/Service/" + serviceName + "/IPv4\n";
		sc += "remove Setup:/Network/Service/" + serviceName + "/IPv4\n";
		sc += "remove State:/Network/Service/" + serviceName + "/IPv6\n";
		sc += "remove Setup:/Network/Service/" + serviceName + "/IPv6\n";

		std::vector<std::string> args;
		ExecResult resultScutil = ExecEx(scUtilPath, args, sc);
		if(resultScutil.exit != 0)
			ThrowException("Unexpected scutil output: " + GetExecResultReport(resultScutil));
		
		return "ok";
	}
	catch (std::exception& ex)
	{
		std::string msg = "WorkaroundDnsLookupIPv6Disable: " + std::string(ex.what());
		return "err:" + msg;
	}
}
