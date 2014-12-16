// <airvpn_source_header>
// This file is part of AirVPN Client software.
// Copyright (C)2014-2014 AirVPN (support@airvpn.org) / https://airvpn.org )
//
// AirVPN Client is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// AirVPN Client is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with AirVPN Client. If not, see <http://www.gnu.org/licenses/>.
// </airvpn_source_header>

using System;
using System.Collections.Generic;
using System.Text;

namespace AirVPN.Core
{
    public static class Messages
    {
		public static string NotImplemented = "Not yet implemented. Contact our support staff.";
		public static string UnhandledException = "Unexpected error. Please contact our support staff.";
		public static string Available = "Available";
		public static string NotAvailable = "Not available";
		public static string NotFound = "Not found";
		public static string Ready = "Ready";
		public static string StatsNotConnected = "Not connected.";
		public static string DoubleClickToView = "(Double-Click to View)";
		public static string ExitConfirm = "Do you really want to exit?";
		public static string WarningLocalTimeInPast = "We have detected a local date/time set to the past.\nThis may cause issues with security verifications.\n\nContinue at your own risk.\n\nIf you're a time traveller, you have been warned.";
		public static string CheckingRequired = "Unavailable (Check required)";
		public static string WaitingLatencyTestsTitle = "Waiting for latency tests";
		public static string WaitingLatencyTestsStep = "({1} to go)";
		public static string ExchangeTryFailed = "{1}, {2}° try failed ({3})";
		public static string AuthorizeLogin = "Checking login ...";
		public static string AuthorizeLoginFailed = "Cannot login. ({1})";
		public static string AuthorizeLoginDone = "Logged in.";
		public static string AuthorizeLogout = "Logout ...";
		public static string AuthorizeLogoutDone = "Logged out.";
		public static string AuthorizeConnect = "Checking authorization ...";
		public static string AuthorizeConnectFailed = "Authorization check failed, continue anyway ({1])";
		public static string AppExiting = "Exiting";
		public static string AppEvent = "Running event {1}";		
		public static string AutoPortSwitch = "Auto retry with another port.";
		public static string AuthFailed = "Authorization failed. Look at the client area to discover the reason.";
		public static string ConsoleKeyboardHelp = "Press 'X' to Cancel, 'B' to reconnect to the best available server.";
		public static string ConsoleKeyboardHelpNoConnect = "Use -connect to start connection when application start"; // TOTRANSLATE
		public static string ConsoleKeyBreak = "Break signal received. Shutdown. Hit again to force break.";
		public static string ConsoleKeyCancel = "Cancel requested from keyboard.";
		public static string ConsoleKeySwitch = "Server switch requested from keyboard.";
		public static string OsDriverInstall = "Installing tunnel driver"; 
		public static string OsDriverNotAvailable = "Driver installer not available.";
		public static string OsDriverFailed = "Driver installation failed.";
		public static string OsDriverUninstallDone = "Tunnel driver uninstalled.";
		public static string ManifestUpdate = "Updating systems & servers data ...";
		public static string ManifestDone = "Systems & servers data update completed";
		public static string ManifestFailed = "Cannot retrieve systems & servers data. Please retry later or contact us for help. ({1})";
		public static string ManifestFailedContinue = "Unable to retrieve systems & servers data. Continue anyway with the old data.";
		public static string CommandLineUnknownOption = "Unknown option in command-line: {1}";
		public static string OptionsRead = "Reading options from {1}";
		public static string OptionsNotFound = "Profile options not found, using defaults.";
		public static string OptionsUnknown = "Skipped unknown option '{1}'";
		public static string OptionsReverted = "Sorry, an error occurred during loading options. Options reverted to default.";
		public static string AdminRequiredStop = "You need root access for this program (to alter routing table)";
		public static string AdminRequiredRestart = "Restarting with admin privileges";
		public static string AdminRequiredRestartFailed = "You need root access for this program (to alter routing table) and a program to obtain administrative privileges was NOT found.\nTry to install packages like 'gksu' or 'kdesudo'";
		public static string AdminRequiredPasswordPrompt = "AirVPN Client needs administrative privileges. Please enter your password.";
		
		public static string GeneratedFileHeader = "# Automatically generated by AirVPN client v{1} | https://airvpn.org . Any manual change will be overridden.";

		public static string AlreadyRunningOpenVPN = "OpenVPN is already running.";
		public static string AlreadyRunningSTunnel = "STunnel is already running.";
		public static string AlreadyRunningSshPLink = "SSH tunnel (plink) is already running.";
		public static string AlreadyRunningSsh = "SSH tunnel is already running.";

		public static string CUrlRequiredForProxySocks = "CUrl is not installed on this system, and it's required for SOCKS proxy.";
		public static string SocksProxyError = "Socks proxy connection error.";

		public static string FetchTryFailed = "{1}, {2}° try failed ({3})";
		
		public static string CheckingEnvironment = "Checking environment";
		public static string CheckingProtocolUnknown = "Unknown protocol.";
		public static string CheckingProxyHostMissing = "Specify a host in the proxy settings.";
		public static string CheckingProxyPortWrong = "Invalid port in the proxy settings.";
		public static string CheckingProxyNoTcp = "Only TCP protocol is allowed with a proxy.";
		public static string CustomRouteInvalid = "Invalid custom route: {1}"; 
		public static string RetrievingManifest = "Retrieving manifest";
		public static string SessionStart = "Session starting.";
		public static string SessionStop = "Session terminated.";
		public static string SessionCancel = "Cancel requested.";
		public static string SessionFailed = "Failed to start.";
		public static string ConnectionStop = "Connection terminated.";
		public static string RenewingTls = "Renewing TLS key";
		
		public static string ConnectionStartManagement = "Starting Management Interface";
		public static string ConnectionCheckingRoute = "Checking route";
		public static string ConnectionCheckingRoute2 = "Checking info";
		public static string ConnectionCheckingRouteNotAvailable = "Checking route not available on this server.";
		public static string ConnectionCheckingRouteFailed = "Routing checking failed.";
		public static string ConnectionCheckingDNS = "Checking DNS";
		public static string ConnectionCheckingDNSFailed = "DNS checking failed.";
		public static string ConnectionFlushDNS = "Flushing DNS";
		public static string ConnectionConnected = "Connected.";
		public static string ConnectionConnecting = "Connecting to {1} ({2}, {3})";
		public static string ConnectionDisconnecting = "Disconnecting";

		public static string NetworkLockActivation = "Activation of Network Lock";
		public static string NetworkLockDeactivation = "Deactivation of Network Lock"; 
		public static string NetworkLockButtonActive = "Network Lock Active. Click to deactivate"; 
		public static string NetworkLockButtonDeactive = "Network Lock Inactive. Click to activate";

		public static string NetworkLockNoMode = "There is no available or enabled Network Lock mode, sorry.";
		public static string NetworkLockAllowedIpDuplicated = "Allowed IP '{1}' in custom lock duplicated"; 
		public static string NetworkLockAllowedIpInvalid = "Allowed IP '{1}' in custom lock invalid"; 
		public static string NetworkLockRecoveryWhenActive = "Unexpected: Recovery Load with Network Lock active";
		public static string NetworkLockRecoveryUnknownMode = "Unknown mode in Network Lock Recovery"; 
				
		public static string NetworkLockRouteRemoved = "Route removed: {1}";
		public static string NetworkLockRouteRestored = "Route restored: {1}";
		public static string NetworkLockWarning = "Network Lock Mode\n\nIn this state, all network connections outside AirVPN service & tunnel are unavailable,\nwhether this system is connected to the VPN or not.\n\nWarning: Any active connection will be dropped.\n\nAre you sure you want to activate this mode?";
		public static string NetworkLockNotAvailableWithRouteOut = "You set the default routing outside the tunnel,\nincompatible with Network Lock feature"; 

		public static string NetworkLockUnexpectedAlreadyActive = "Unexpected already active network lock";
		public static string NetworkLockUnableToStartService = "Unable to start Windows Firewall service. Try to switch it from 'Disabled' to 'Manual'."; // TOTRANSLATE
		public static string NetworkLockWindowsFirewallBackupFailed = "Backup of current rules failed.";

		public static string TorControlAuth = "TOR Control authentication method: {1}"; 
		public static string TorControlGuardIp = "TOR Control Guard IP detected: {1} ({2})"; 
		public static string TorControlNoPath = "Unable to find your TOR path."; 
		public static string TorControlNoIps = "Unable to find IP address of TOR first node of an established circuit.";
		public static string TorControlException = "Unable to communicate with TOR ({1}). Is TOR up and running?";
		public static string TorControlTest = "Successful test. TOR Version: ";
		
		public static string RecoveryDetected = "Recovery. Unexpected crash?";
		
		public static string TopBarConnected = "Connected to {1}";
		public static string TopBarNotConnectedLocked = "Not connected. Network locked.";
		public static string TopBarNotConnectedExposed = "Not connected. Network exposed.";
		public static string StatusTextConnected = "Down: {1} Up: {2} - {3} ({4})";
				
		public static string ChartRange = "Range";
		public static string ChartGrid = "Grid";
		public static string ChartStep = "Step";
		public static string ChartClickToChangeResolution = "Click to change resolution";
		public static string ChartDownload = "Download";
		public static string ChartUpload = "Upload";

		public static string AreasName = "Name";
		public static string AreasServers = "Servers";
		public static string AreasLoad = "Load";
		public static string AreasUsers = "Users";

		public static string ServersName = "Name";
		public static string ServersScore = "Score";
		public static string ServersLocation = "Location";
		public static string ServersLatency = "Latency";
		public static string ServersLoad = "Load";
		public static string ServersUsers = "Users";

		public static string WindowsAboutTitle = "About";
		public static string WindowsAboutVersion = "Version";
		public static string WindowsTosTitle = "Terms of Service";
		public static string WindowsTosCheck1 = "I have read and I accept the Terms of Service ";
		public static string WindowsTosCheck2 = "I HEREBY EXPLICITLY ACCEPT POINTS 8, 10, 11";
		public static string WindowsTosAccept = "Accept"; 
		public static string WindowsTosReject = "Reject";
		public static string WindowsFrontMessageTitle = "Important Message";
		public static string WindowsFrontMessageAccept = "Ok";
		public static string WindowsFrontMessageMore = "Look at https://airvpn.org for more informations";
		public static string WindowsSettingsTitle = "Preferences";
		public static string WindowsSettingsRouteTitle = "Preferences - Route";
		public static string WindowsSettingsEventTitle = "Preferences - Event";
		public static string WindowsSettingsLoggingHelp = "Use %d, %m, %y or %w for day, month, year or day of week. Useful for log rotation.\nRelative to data path. For multiple logs with different paths, separe it with a semicolon ;\n\nAdvanced example:\nlogs/single.log;logs/months/airvpn_%d.log;logs/week/airvpn_%w.log";
		public static string WindowsSettingsRouteLockHelp = "IP in 'Outside Tunnel' are also unlocked when Network Lock feature is active."; 
		public static string WindowsSettingsRouteEditIp = "Specify single IP address (ex. 1.2.3.4) or\nCIDR range (ex. 1.2.3.4/24) or\nsubnet range (ex. 1.2.3.4/255.255.255.128)";
		public static string WindowsSettingsRouteInvalid = "Invalid IP address or range."; 
		public static string WindowsOpenVpnManagementCommandTitle = "OpenVpn Management Command";
		public static string WindowsPortForwardingTitle = "Tools - Port Forwarding Tester";
		public static string WindowsMainSpeedResolution1 = "Range: 1 minute, Grid: 10 seconds, Step: 1 second"; 
		public static string WindowsMainSpeedResolution2 = "Range: 10 minutes, Grid: 1 minute, Step: 1 second"; 
		public static string WindowsMainSpeedResolution3 = "Range: 1 hour, Grid: 10 minutes, Step: 1 second"; 
		public static string WindowsMainSpeedResolution4 = "Range: 1 day, Grid: 1 hour, Step: 30 seconds"; 
		public static string WindowsMainSpeedResolution5 = "Range: 30 days, Grid: 1 day, Step: 10 minutes";
		public static string WindowsMainHide = "Hide Main Window"; 
		public static string WindowsMainShow = "Show Main Window";

		public static string LogsCopyClipboardDone = "Copy to clipboard done.";
		public static string LogsSaveToFileDone = "Save to file done.";
		public static string TooltipServersScoreType = "Choose whether you prefer highest speed (ex. file-sharing) or low-latency speed (ex. gaming)";
		public static string TooltipServersLockCurrent = "Never leave the current server. \r\nFor example if you don\'t want to change your IP for port forwarding reasons.";
		public static string TooltipServersConnect = "Connect to the selected server now";
		public static string TooltipServersShowAll = "Show all servers if checked, or show only selected servers based on whitelist & blacklist";
		public static string TooltipServersUndefined = "Clear the selected servers from whitelist and blacklist";
		public static string TooltipServersBlackList = "Add the selected servers to blacklist. \r\nThe system will never connect to blacklisted servers.";
		public static string TooltipServersWhiteList = "Add the selected server to whitelist.\r\nThe system will only connect to whitelisted servers.";
		public static string TooltipAreasUndefined = "Clear the selected areas from whitelist and blacklist";
		public static string TooltipAreasBlackList = "Add the selected areas to blacklist. \r\nThe system will never connect to servers in blacklisted areas.";
		public static string TooltipAreasWhiteList = "Add the selected area to whitelist.\r\nThe system will only connect to servers in whitelisted areas.";
		public static string TooltipLogsOpenVpnManagement = "Run an OpenVPN Management command";
		public static string TooltipLogsClean = "Clear logs";
		public static string TooltipLogsCopy = "Copy to clipboard";
		public static string TooltipLogsSave = "Save to file";
		public static string TooltipLogsSupport = "Log system information and copy to clipboard. Useful for support requests";
		public static string CommandLogin = "Login action required."; // TOTRANSLATE: It's a menu item, showed when a 'Connect' shortcut can't be available because the user is not logged. At click simply show the main interface.
		public static string CommandLogout = "Logout";
		public static string CommandConnect = "Connect to a recommended server";
		public static string CommandConnectSubtitle = "or choose a specific server in 'Servers' tab.";
		public static string CommandDisconnect = "Disconnect";
		public static string CommandCancel = "Cancel";
		public static string CommandUnknown = "Unknown command"; // TOTRANSLATE

		public static string FilterAllFiles = "All files (*.*)|*.*";
		public static string FilterTextFiles = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

		public static string ScoreUnknown = "NC";

		public static string ConsoleHelp = "Run the program with login & password of your AirVPN account. For example:\nairvpn -cli -login=mynick -password=mypassword\n\nSee https://airvpn.org/software/ for more information, or run with -help for inline manual.";

		public static string TimeJustNow = "Just now";
		public static string TimeAgo = "ago";
		public static string TimeRemain = "remain";

		public static string StatsServerName = "Server Name";
		public static string StatsServerLatency = "Server Latency";
		public static string StatsServerLocation = "Server Location";
		public static string StatsServerLoad = "Server Load";
		public static string StatsServerUsers = "Server Users";
		public static string StatsVpnSpeedDownload = "VPN Speed Download";
		public static string StatsVpnSpeedUpload = "VPN Speed Upload";
		public static string StatsVpnConnectionStart = "VPN Start";
		public static string StatsVpnTotalDownload = "VPN Total Download";
		public static string StatsVpnTotalUpload = "VPN Total Upload";
		public static string StatsVpnIpEntry = "IP Entry";
		public static string StatsVpnIpExit = "IP Exit";
		public static string StatsVpnProtocol = "VPN Protocol";
		public static string StatsVpnPort = "VPN Port";
		public static string StatsVpnRealIp = "Real IP detected";
		public static string StatsVpnIp = "VPN IP";
		public static string StatsVpnDns = "VPN DNS";
		public static string StatsVpnInterface = "VPN Interface";
		public static string StatsVpnGateway = "VPN Gateway";
		public static string StatsVpnGeneratedOVPN = "Generated OVPN";
		public static string StatsManifestLastUpdate = "Latest Manifest Update";
		public static string StatsPinger = "Pinger stats";
		public static string StatsSystemTimeServerDifference = "Server Time Difference";
		public static string StatsSystemReport = "System Report";



		public static string ManName = "airvpn -- AirVPN Client, console edition";
		public static string ManSynopsis = "airvpn -cli [-option=\"value\"] ...";
		public static string ManDescription = "See AirVPN website for more information: https://airvpn.org";
		public static string ManCopyright = "Copyright (C) AirVPN - Released under GNU General Public License - http://www.gnu.org/licenses/gpl.html";

		public static string ManOptionCli = "Don't show the graphics user interface. Connect directly.";
		public static string ManOptionHelp = "Show help manual";
		public static string ManOptionLogin = "Login of your AirVPN account";
		public static string ManOptionPassword = "Password of your AirVPN account";
		public static string ManOptionRemember = "'True' if login data persist after exit.";
		public static string ManOptionServer = "Server to connect to. Leave empty to pick recommended server";
		public static string ManOptionConnect = "Connect automatically at startup. Only for GUI, command-line always starts directly.";
		public static string ManOptionProfile = "Profile name. Use it to store different set of options";
		public static string ManOptionPath = "Data path. May be a full path or special value 'program' or 'home'";
		public static string ManOptionServersWhiteList = "List of servers available for connection. Leave empty for all servers. Separate values with comma. Example: 'Canopus,Syrma,Taygeta'";
		public static string ManOptionServersBlackList = "List of servers to avoid in connection. Same syntax of whitelist.";
		public static string ManOptionServersStartLast = "'True' if you want to connect to the last used server. 'False' to choose it automatically.";
		public static string ManOptionServersLockLast = "'True' if you never leave the selected server, not even in case of disconnection.";
		public static string ManOptionServersScoreType = "May be 'Speed' or 'Latency'. Affects scoring of servers, indicates if you prefer a better speed or a better latency";
		public static string ManOptionAreasWhiteList = "List of areas available for connection. Same as server whitelist syntax. Example: 'nl,de'";
		public static string ManOptionAreasBlackList = "List of areas to avoid in connection. Same as whitelist syntax.";

		public static string ManOptionModeProtocol = "Protocol for connection. 'UDP', 'TCP' for direct openvpn connection. 'SSH', 'SSL' for additional tunneling";
		public static string ManOptionModePort = "Port for connection. Currently available: 443, 80, 53, 2018";
		public static string ManOptionModeAlt = "0 to use the default entry IP, 1 or more for additional entry IP";
		public static string ManOptionModeTorHost = "TOR host";
		public static string ManOptionModeTorPort = "TOR port";
		public static string ManOptionModeTorControlPort = "TOR Control port";
		public static string ManOptionModeTorControlAuth = "TOR Control needs authentication. Normally the TOR Browser Bundle requires authentication and accepts a file cookie password";
		public static string ManOptionModeTorControlPassword = "TOR Control password. If empty, the software tries to detect the file cookie password";
		
		public static string ManOptionProxyMode = "Proxy mode: 'none', 'http' or 'socks'. 'protocol' option must be 'TCP'.";
		public static string ManOptionProxyHost = "Proxy host";
		public static string ManOptionProxyPort = "Proxy port";
		public static string ManOptionProxyAuth = "Proxy authentication method: 'None', 'Basic' or 'NTLM'";
		public static string ManOptionProxyLogin = "Proxy login, for authentication";
		public static string ManOptionProxyPassword = "Proxy password, for authentication";

		public static string ManOptionRoutesCustom = "Custom routes. Format: '{ip or range},{in/out},{note (optional)};...'. Separate multiple routes with ;. Example: '1.2.3.4,in;2.3.4.5/23,out,'test'";
		public static string ManOptionRoutesDefault = "Specify whether routes that don't match the custom route must be inside ('in') or outside ('out') the tunnel.";

		public static string ManOptionExecutablesOpenVpn = "Path to a custom OpenVPN executable";
		public static string ManOptionExecutablesSsh = "Path to a custom SSH tunnel executable";
		public static string ManOptionExecutablesSsl = "Path to a custom SSL tunnel executable";
		public static string ManOptionExecutablesCurl = "Path to a custom curl executable, used only with socks proxy";

		public static string ManOptionOpenVpnCustom = "Allows you to specify a path to OpenVPN executable, to skip the executable bundled with AirVPN Client.";
		public static string ManOptionOpenVpnSkipDefaults = "If 'false' the custom directives are appended to the default directive.";
		public static string ManOptionOpenVpnManagementPort = "Default port of OpenVPN Management Interface.";
		public static string ManOptionSshPort = "Default port of SSH Tunnel. If 0, a random port is used.";
		public static string ManOptionSslPort = "Default port of SSL Tunnel. If 0, a random port is used.";

		public static string ManOptionAdvancedExpert = "Activate some expert information and features.\n- Allows sending commands to OpenVPN Management Interface via Logs window.\n- Show verbose logs message in main windows";
		public static string ManOptionAdvancedCheckDns = "True/False. When the connection is established, try to resolve domains that are resolved only by our DNS server, to ensure that system is correctly using our DNS server.\nIt's not mandatory to use our DNS server, but it's recommended to enjoy our Geolocation Routing service and avoid DNS blocks of your provider.";
		public static string ManOptionAdvancedCheckRoute = "True/False. Send a request to the server, that check it come from within the tunnel, and reply with an acknowledgement.";
		public static string ManOptionAdvancedDnsSwitch = "True/False. Under Linux, run 'update-resolv-conf' script to allow openresolv to update system DNS settings according to pushed DNS settings from server.\nUnder Windows, this currently doesn't have any effect: DNS settings pushed from server are always applied. ";
		public static string ManOptionAdvancedPingerEnabled = "If 'true' the software pings AirVPN server to determine latency score. Pings are not performed during VPN connection.";
		public static string ManOptionAdvancedPingerAlways = "If 'true' pings are performed also during VPN connection. Pings are always performed outside the tunnel.";
		public static string ManOptionAdvancedPingerDelay = "Do a ping on every server every X seconds. If 0, the recommended values are used.";
		public static string ManOptionAdvancedPingerRetry = "Do a ping on every server that don't have ping results every X seconds. If 0, the recommended values are used."; // TOTRANSLATE
		public static string ManOptionAdvancedPingerJobs = "Maximum parallel jobs/thread for pinging purpose.";
		public static string ManOptionAdvancedPingerValid = "Global pinger results valid if all ping reply are maximum X seconds ago. If 0, the recommended values are used.";

		public static string ManOptionAdvancedWindowsTapUp = "Force the TAP interface to come UP.";
		public static string ManOptionAdvancedWindowsDnsForce = "In Windows, every network adapter may have DNS settings.\nIf this option is active, when the connection starts the client empties any DNS settings for every network adapter.\nThe TUN/TAP adapter will be updated when the connection is estabilished.\nWhen the connection is closed, the client resets every DNS settings to the original state.\nDNS settings are changed during connection, not during the disconnection/connection to another server.\n\nThis option is equivalent to one feature of the https://www.dnsleaktest.com/ scripts.";
		public static string ManOptionAdvancedWindowsDhcpDisable = "If a DHCP adapter is renewed during connection, Windows may reset the original DNS settings.\nIf this option is active, when the connection starts the client changes any DHCP adapter to Static.\nWhen the connection is closed, the client resets every adapter to the original state.\nNo action is performed if there isn't any adapter in DHCP.\n\nThis option is equivalent to one feature of the https://www.dnsleaktest.com/ scripts.";

		public static string ManOptionEventFileName = "Filename of the script/executable to launch on event";
		public static string ManOptionEventArguments = "Arguments";
		public static string ManOptionEventWaitEnd = "'true' if the software needs to wait the end (synchronous) or 'false' to be asynchronous";

		// Platform Windows		
		public static string NetworkAdapterDhcpDone = "Network adapter DHCP switched to static ({1})";
		public static string NetworkAdapterDhcpRestored = "DHCP of a network adapter restored to original settings ({1})";
		public static string NetworkAdapterDnsDone = "DNS of a network adapter forced ({1})";
		public static string NetworkAdapterDnsRestored = "DNS of a network adapter restored to original settings ({1})";
		public static string HackInterfaceUpDone = "AirVPN Windows Interface Hack executed ({1})";

		// Platform Linux
		public static string DnsResolvConfScript = "DNS of the system will be updated to VPN DNS (ResolvConf method)"; 
		public static string DnsRenameBackup = "/etc/resolv.conf renamed to /etc/resolv.conf.airvpn as backup";
		public static string DnsRenameDone = "DNS of the system updated to VPN DNS (Rename method: /etc/resolv.conf generated)";
		public static string DnsRenameRestored = "DNS of the system restored to original settings (Rename method)";

		

		public static string Format(string format, string param1)
		{
			return format.Replace("{1}", param1);
		}

		public static string Format(string format, string param1, string param2)
		{
			string o = format;
			o = o.Replace("{1}", param1);
			o = o.Replace("{2}", param2);
			return o;
		}

		public static string Format(string format, string param1, string param2, string param3)
		{
			string o = format;
			o = o.Replace("{1}", param1);
			o = o.Replace("{2}", param2);
			o = o.Replace("{3}", param3);
			return o;
		}

		public static string Format(string format, string param1, string param2, string param3, string param4)
		{
			string o = format;
			o = o.Replace("{1}", param1);
			o = o.Replace("{2}", param2);
			o = o.Replace("{3}", param3);
			o = o.Replace("{4}", param4);
			return o;
		}

		public static string Format(string format, string param1, string param2, string param3, string param4, string param5)
		{
			string o = format;
			o = o.Replace("{1}", param1);
			o = o.Replace("{2}", param2);
			o = o.Replace("{3}", param3);
			o = o.Replace("{4}", param4);
			o = o.Replace("{5}", param5);
			return o;
		}
    }
}
