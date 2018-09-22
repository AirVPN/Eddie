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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Eddie.Core;
using Eddie.Common;

// If errors occur here, probably Xamarin update cause trouble. Remove Xamarin.Mac reference and re-add by browsing to path
// /Library/Frameworks/Xamarin.Mac.framework/Versions/Current/lib/mono/4.5/Xamarin.Mac.dll
using AppKit;
using Foundation;

namespace Eddie.Platform.MacOS
{
	public class Platform : Core.Platform
	{
		private string m_version = "";
		private string m_architecture = "";

		private List<DnsSwitchEntry> m_listDnsSwitch = new List<DnsSwitchEntry>();
		private List<IpV6ModeEntry> m_listIpV6Mode = new List<IpV6ModeEntry>();

		/*
		private UnixSignal[] m_signals = new UnixSignal[] {
			new UnixSignal (Mono.Unix.Native.Signum.SIGTERM),
			new UnixSignal (Mono.Unix.Native.Signum.SIGINT),
			new UnixSignal (Mono.Unix.Native.Signum.SIGUSR1),
			new UnixSignal (Mono.Unix.Native.Signum.SIGUSR2),
		};
		*/

		// Override
		public Platform()
		{
		}

		public override string GetCode()
		{
			return "MacOS";
		}

		public override string GetName()
		{
			string swversPath = LocateExecutable("sw_vers");
			if (swversPath != "")
				return SystemShell.Shell1(swversPath, "-productVersion");
			else
				return "Unknown (no sw_vers)";
		}

		public override string GetVersion()
		{
			return m_version;
		}

		public override void OnInit(bool cli)
		{
			base.OnInit(cli);

			if (cli)
				NSApplication.Init(); // Requested in CLI edition to call NSPipe, NSTask etc.

			m_version = SystemShell.Shell("/usr/bin/uname", new string[] { "-a" }).Trim();
			m_architecture = NormalizeArchitecture(SystemShell.Shell("/usr/bin/uname", new string[] { "-m" }).Trim());

			NativeMethods.eddie_signal((int)NativeMethods.Signum.SIGINT, SignalCallback);
			NativeMethods.eddie_signal((int)NativeMethods.Signum.SIGTERM, SignalCallback);
			NativeMethods.eddie_signal((int)NativeMethods.Signum.SIGUSR1, SignalCallback);
			NativeMethods.eddie_signal((int)NativeMethods.Signum.SIGUSR2, SignalCallback);
		}

		private static void SignalCallback(int signum)
		{
			NativeMethods.Signum sig = (NativeMethods.Signum)signum;
			if (sig == NativeMethods.Signum.SIGINT)
				Engine.Instance.OnSignal("SIGINT");
			else if (sig == NativeMethods.Signum.SIGTERM)
				Engine.Instance.OnSignal("SIGTERM");
			else if (sig == NativeMethods.Signum.SIGUSR1)
				Engine.Instance.OnSignal("SIGUSR1");
			else if (sig == NativeMethods.Signum.SIGUSR2)
				Engine.Instance.OnSignal("SIGUSR2");
		}

		public override string GetOsArchitecture()
		{
			return m_architecture;
		}

		public override string GetDefaultDataPath()
		{
			// Only in OSX, always save in 'home' path also with portable edition.
			return "home";
		}

		public override bool IsAdmin()
		{
			// With root privileges by RootLauncher.cs, Environment.UserName still return the normal username, 'whoami' return 'root'.
			string u = SystemShell.Shell(LocateExecutable("whoami"), new string[] { }).ToLowerInvariant().Trim();
			//return true; // Uncomment for debugging
			return (u == "root");
		}

		public override bool IsUnixSystem()
		{
			return true;
		}

		public override string DirSep
		{
			get
			{
				return "/";
			}
		}

		public override string EnvPathSep
		{
			get
			{
				return ":";
			}
		}

		public override bool NativeInit()
		{
			return (NativeMethods.eddie_init() == 0);
		}

		public override bool FileImmutableGet(string path)
		{
			if ((path == "") || (FileExists(path) == false))
				return false;

			int result = NativeMethods.eddie_file_get_immutable(path);
			return (result == 1);
		}

		public override void FileImmutableSet(string path, bool value)
		{
			if ((path == "") || (FileExists(path) == false))
				return;

			if (FileImmutableGet(path) == value)
				return;

			NativeMethods.eddie_file_set_immutable(path, value ? 1 : 0);
		}

		public override bool FileEnsurePermission(string path, string mode)
		{
			if ((path == "") || (Platform.Instance.FileExists(path) == false))
				return false;

			int result = NativeMethods.eddie_file_get_mode(path);
			if (result == -1)
			{
				Engine.Instance.Logs.Log(LogType.Warning, "Failed to detect permissions on '" + path + "'.");
				return false;
			}
			int newResult = 0;
			if (mode == "600")
				newResult = (int)NativeMethods.FileMode.Mode0600;
			else if (mode == "644")
				newResult = (int)NativeMethods.FileMode.Mode0644;

			if (newResult == 0)
			{
				Engine.Instance.Logs.Log(LogType.Warning, "Unexpected permission '" + mode + "'");
				return false;
			}

			if (newResult != result)
			{
				result = NativeMethods.eddie_file_set_mode(path, newResult);
				if (result == -1)
				{
					Engine.Instance.Logs.Log(LogType.Warning, "Failed to set permissions on '" + path + "'.");
					return false;
				}
			}

			return true;
		}

		public override bool FileEnsureOwner(string path)
		{
            SystemShell.ShellCmd("chown \"" + Environment.GetEnvironmentVariable("USER") + "\" \"" + path + "\""); 
            return true;
		}

		public override bool FileEnsureExecutablePermission(string path)
		{
			if ((path == "") || (FileExists(path) == false))
				return false;

			int result = NativeMethods.eddie_file_get_mode(path);
			if (result == -1)
			{
				Engine.Instance.Logs.Log(LogType.Warning, "Failed to detect if '" + path + "' is executable");
				return false;
			}

			int newResult = result | 73; // +x :<> (S_IXUSR | S_IXGRP | S_IXOTH) 

			if (newResult != result)
			{
				result = NativeMethods.eddie_file_set_mode(path, newResult);
				if (result == -1)
				{
					Engine.Instance.Logs.Log(LogType.Warning, "Failed to mark '" + path + "' as executable");
					return false;
				}
			}

			return true;
		}

		public override string GetExecutableReport(string path)
		{
			string otoolPath = LocateExecutable("otool");
			if (otoolPath != "")
				return SystemShell.Shell2(otoolPath, "-L", SystemShell.EscapePath(path));
			else
				return "'otool' " + Messages.NotFound;
		}

		public override string GetExecutablePathEx()
		{
			string currentPath = System.Reflection.Assembly.GetEntryAssembly().Location;
			if (new FileInfo(currentPath).Directory.Name == "MonoBundle")
			{
				// OSX Bundle detected, use the launcher executable
				currentPath = currentPath.Replace("/MonoBundle/", "/MacOS/").Replace(".exe", "");
			}
			else if (Process.GetCurrentProcess().ProcessName.StartsWith("mono", StringComparison.InvariantCultureIgnoreCase))
			{
				// mono <app>, Entry Assembly path it's ok
			}
			else
			{
				currentPath = Process.GetCurrentProcess().MainModule.FileName;
			}
			return currentPath;
		}

		public override string GetUserPathEx()
		{
			return Environment.GetEnvironmentVariable("HOME") + DirSep + ".airvpn";
		}

		public override bool ProcessKillSoft(Process process)
		{
			return (NativeMethods.eddie_kill(process.Id, (int)NativeMethods.Signum.SIGTERM) == 0);
		}

		public override int GetRecommendedRcvBufDirective()
		{
			return 256 * 1024;
		}

		public override int GetRecommendedSndBufDirective()
		{
			return 256 * 1024;
		}

		public override void FlushDNS()
		{
			base.FlushDNS();

			// 10.5 - 10.6
			string dscacheutilPath = LocateExecutable("dscacheutil");
			if (dscacheutilPath != "")
				SystemShell.Shell1(dscacheutilPath, "-flushcache");

			// 10.7 - 10.8 - 10.9 - 10.10.4 - 10.11 - Sierra 10.12.0
			string killallPath = LocateExecutable("killall");
			if (killallPath != "")
				SystemShell.Shell2(killallPath, "-HUP", "mDNSResponder");

			// 10.10.0 - 10.10.3
			string discoveryutilPath = LocateExecutable("discoveryutil");
			if (discoveryutilPath != "")
			{
				SystemShell.Shell1(discoveryutilPath, "udnsflushcaches");
				SystemShell.Shell1(discoveryutilPath, "mdnsflushcache");
			}
		}

		public override void ShellCommandDirect(string command, out string path, out string[] arguments)
		{
			path = "/bin/sh";
			arguments = new string[] { "-c", command };
		}

		public override void ShellSync(string path, string[] arguments, out string stdout, out string stderr, out int exitCode)
		{
			try
			{
				var pipeOut = new NSPipe();
				var pipeErr = new NSPipe();

				var t = new NSTask();

				t.LaunchPath = path;
				t.Arguments = arguments;
				t.StandardOutput = pipeOut;
				t.StandardError = pipeErr;

				t.Launch();
				t.WaitUntilExit();
				//t.Release();
				t.Dispose();

				NSFileHandle fileOut = pipeOut.ReadHandle;
				stdout = fileOut.ReadDataToEndOfFile().ToString();
				fileOut.CloseFile();

				NSFileHandle fileErr = pipeErr.ReadHandle;
				stderr = fileErr.ReadDataToEndOfFile().ToString();
				fileErr.CloseFile();

				exitCode = t.TerminationStatus;
			}
			catch (Exception ex)
			{
				stdout = "";
				stderr = "Error: " + ex.Message;
				exitCode = -1;
			}
		}

		public override string LocateResource(string relativePath)
		{
			string resPath = NormalizePath(GetApplicationPath() + "/../Resources/" + relativePath);
			if (File.Exists(resPath))
				return resPath;

			return base.LocateResource(relativePath);
		}

		public override long Ping(IpAddress host, int timeoutSec)
		{
			if ((host == null) || (host.Valid == false))
				return -1;

			return NativeMethods.eddie_ip_ping(host.ToString(), timeoutSec * 1000);
		}

		public override string GetDriverAvailable()
		{
			return "Expected";
		}

		public override bool CanInstallDriver()
		{
			return false;
		}

		public override bool CanUnInstallDriver()
		{
			return false;
		}

		public override void InstallDriver()
		{
		}

		public override void UnInstallDriver()
		{
		}

		public override bool RouteAdd(Json jRoute)
		{
			IpAddress ip = jRoute["address"].Value as string;
			if (ip.Valid == false)
				return false;
			IpAddress gateway = jRoute["gateway"].Value as string;
			if (gateway.Valid == false)
				return false;

			SystemShell s = new SystemShell();
			s.Path = LocateExecutable("route");
			s.Arguments.Add("-n");
			s.Arguments.Add("add");
			if (ip.IsV6)
				s.Arguments.Add("-inet6");
			s.Arguments.Add(ip.ToCIDR());
			s.Arguments.Add(gateway.Address);
			s.ExceptionIfFail = true;
			s.Run();

			string result = s.StdErr.Trim();
			if (result == "")
			{
				return base.RouteAdd(jRoute);
			}
			else
			{
				Engine.Instance.Logs.LogWarning(MessagesFormatter.Format(Messages.RouteAddFailed, ip.ToCIDR(), gateway.ToCIDR(), result));
				return false;
			}
		}

		public override bool RouteRemove(Json jRoute)
		{
			IpAddress ip = jRoute["address"].Value as string;
			if (ip.Valid == false)
				return false;
			IpAddress gateway = jRoute["gateway"].Value as string;
			if (gateway.Valid == false)
				return false;

			SystemShell s = new SystemShell();
			s.Path = LocateExecutable("route");
			s.Arguments.Add("-n");
			s.Arguments.Add("delete");
			if (ip.IsV6)
				s.Arguments.Add("-inet6");
			s.Arguments.Add(ip.ToCIDR());
			s.Arguments.Add(gateway.Address);
			s.ExceptionIfFail = true;
			s.Run();

			string result = s.StdErr.Trim();
			if (result == "")
			{
				return base.RouteRemove(jRoute);
			}
			else
			{
				// Remember: Route deletion can occur in a second moment (for example a Recovery phase).

				// Still accepted: The device are not available anymore, so the route are already deleted.

				// Still accepted: Already deleted.
				if (result.ToLowerInvariant().Contains("not in table"))
					return base.RouteRemove(jRoute);

				// Unexpected/unknown error.
				Engine.Instance.Logs.LogWarning(MessagesFormatter.Format(Messages.RouteDelFailed, ip.ToCIDR(), gateway.ToCIDR(), result));
				return false;
			}
		}

		public override IpAddresses ResolveDNS(string host)
		{
			// Base method with Dns.GetHostEntry have cache issue, for example on Fedora. OS X it's based on Mono.
			// Also, base methods with Dns.GetHostEntry sometime don't fetch AAAA IPv6 addresses.

			IpAddresses result = new IpAddresses();

			string hostPath = LocateExecutable("host");
			if (hostPath != "")
			{
				// Note: CNAME record are automatically followed.
				SystemShell s = new SystemShell();
				s.Path = "/usr/bin/host";
				s.Arguments.Add("-W 5");
				s.Arguments.Add(SystemShell.EscapeHost(host));
				s.NoDebugLog = true;
				if (s.Run())
				{
					string hostout = s.Output;
					foreach (string line in hostout.Split('\n'))
					{
						string ipv4 = UtilsString.RegExMatchOne(line, "^.*? has address (.*?)$");
						if (ipv4 != "")
							result.Add(ipv4.Trim());

						string ipv6 = UtilsString.RegExMatchOne(line, "^.*? has IPv6 address (.*?)$");
						if (ipv6 != "")
							result.Add(ipv6.Trim());
					}
				}
			}

			return result;
		}

		public override IpAddresses DetectDNS()
		{
			IpAddresses list = new IpAddresses();

			// Method1: Don't return DHCP DNS
			string networksetupPath = LocateExecutable("networksetup");
			if (networksetupPath != "")
			{
				string[] interfaces = GetInterfaces();
				foreach (string i in interfaces)
				{
					string i2 = i.Trim();

					string current = SystemShell.Shell(networksetupPath, new string[] { "-getdnsservers", SystemShell.EscapeInsideQuote(i2) });

                    foreach(string line in current.Split('\n'))
                    {
                        string field = line.Trim();
                        list.Add(field);
                    }
					
				}
			}

			// Method2 - More info about DHCP DNS
			string scutilPath = LocateExecutable("scutil");
			if(scutilPath != "")
			{
				string scutilOut = SystemShell.Shell1(scutilPath, "--dns");
				List<List<string>> result = UtilsString.RegExMatchMulti(scutilOut.Replace(" ", ""), "nameserver\\[[0-9]+\\]:([0-9:\\.]+)");
				foreach (List<string> match in result)
				{
					foreach (string field in match)
					{
						list.Add(field);
					}
				}
			}

			// Method3 - Compatibility
			if (FileExists("/etc/resolv.conf"))
			{
				string o = FileContentsReadText("/etc/resolv.conf");
				foreach (string line in o.Split('\n'))
				{
					if (line.Trim().StartsWith("#"))
						continue;
					if (line.Trim().StartsWith("nameserver"))
					{
						string field = line.Substring(11).Trim();
						list.Add(field);
					}
				}
			}

			return list;
		}

		public override bool RestartAsRoot()
		{
			string path = Platform.Instance.GetExecutablePath();
			List<string> args = CommandLine.SystemEnvironment.GetFullArray();
			string defaultsPath = Core.Platform.Instance.LocateExecutable("defaults");
			if (defaultsPath != "")
			{
				// If 'white', return error in StdErr and empty in StdOut.
				SystemShell s = new SystemShell();
				s.Path = defaultsPath;
				s.Arguments.Add("read");
				s.Arguments.Add("-g");
				s.Arguments.Add("AppleInterfaceStyle");
				s.Run();
				string colorMode = s.StdOut.Trim().ToLowerInvariant();
				if (colorMode == "dark")
					args.Add("gui.osx.style=\"dark\"");
			}

			RootLauncher.LaunchExternalTool(path, args.ToArray());
			return true;
		}

		public override void OnReport(Report report)
		{
			base.OnReport(report);

			report.Add("ifconfig", (LocateExecutable("ifconfig") != "") ? SystemShell.Shell0(LocateExecutable("ifconfig")) : "'ifconfig' " + Messages.NotFound);

		}

		public override Dictionary<int, string> GetProcessesList()
		{
			// We experience some crash under OSX with the base method.
			Dictionary<int, string> result = new Dictionary<int, string>();
			string psPath = LocateExecutable("ps");
			if (psPath != "")
			{
				string resultS = SystemShell.Shell2(psPath, "-eo", "pid,command");
				string[] resultA = resultS.Split('\n');
				foreach (string pS in resultA)
				{
					int posS = pS.IndexOf(' ');
					if (posS != -1)
					{
						int pid = Conversions.ToInt32(pS.Substring(0, posS).Trim());
						string name = pS.Substring(posS).Trim();
						result[pid] = name;
					}
				}
			}

			return result;
		}

		public override bool OnCheckEnvironmentApp()
		{
			bool fatal = false;
			string networksetupPath = LocateExecutable("networksetup");
			if (networksetupPath == "")
			{
				Engine.Instance.Logs.Log(LogType.Error, "'networksetup' " + Messages.NotFound);
				fatal = true;
			}

			string pfctlPath = LocateExecutable("pfctl");
			if (pfctlPath == "")
				Engine.Instance.Logs.Log(LogType.Error, "'pfctl' " + Messages.NotFound);

			string hostPath = LocateExecutable("host");
			if (hostPath == "")
				Engine.Instance.Logs.Log(LogType.Error, "'host' " + Messages.NotFound);

            string psPath = LocateExecutable("ps");
			if (psPath == "")
				Engine.Instance.Logs.Log(LogType.Error, "'ps' " + Messages.NotFound);

			return (fatal == false);
		}

		public override bool OnCheckEnvironmentSession()
		{
			return true;
		}

		public override void OnNetworkLockManagerInit()
		{
			base.OnNetworkLockManagerInit();

			Engine.Instance.NetworkLockManager.AddPlugin(new NetworkLockOsxPf());
		}

		public override string OnNetworkLockRecommendedMode()
		{
			return "osx_pf";
		}

		public override void OnRecoveryLoad(XmlElement root)
		{
			XmlElement nodeDns = UtilsXml.XmlGetFirstElementByTagName(root, "DnsSwitch");
			if (nodeDns != null)
			{
				foreach (XmlElement nodeEntry in nodeDns.ChildNodes)
				{
					DnsSwitchEntry entry = new DnsSwitchEntry();
					entry.ReadXML(nodeEntry);
					m_listDnsSwitch.Add(entry);
				}
			}

			XmlElement nodeIpV6 = UtilsXml.XmlGetFirstElementByTagName(root, "IpV6");
			if (nodeIpV6 != null)
			{
				foreach (XmlElement nodeEntry in nodeIpV6.ChildNodes)
				{
					IpV6ModeEntry entry = new IpV6ModeEntry();
					entry.ReadXML(nodeEntry);
					m_listIpV6Mode.Add(entry);
				}
			}

			base.OnRecoveryLoad(root);
		}

		public override void OnRecoverySave(XmlElement root)
		{
			base.OnRecoverySave(root);

			XmlDocument doc = root.OwnerDocument;

			if (m_listDnsSwitch.Count != 0)
			{
				XmlElement nodeDns = (XmlElement)root.AppendChild(doc.CreateElement("DnsSwitch"));
				foreach (DnsSwitchEntry entry in m_listDnsSwitch)
				{
					XmlElement nodeEntry = nodeDns.AppendChild(doc.CreateElement("entry")) as XmlElement;
					entry.WriteXML(nodeEntry);
				}
			}

			if (m_listIpV6Mode.Count != 0)
			{
				XmlElement nodeDns = (XmlElement)root.AppendChild(doc.CreateElement("IpV6"));
				foreach (IpV6ModeEntry entry in m_listIpV6Mode)
				{
					XmlElement nodeEntry = nodeDns.AppendChild(doc.CreateElement("entry")) as XmlElement;
					entry.WriteXML(nodeEntry);
				}
			}
		}

		public override bool OnIPv6Block()
		{
			string[] interfaces = GetInterfaces();
			foreach (string i in interfaces)
			{
				string getInfo = SystemShell.Shell("/usr/sbin/networksetup", new string[] { "-getinfo", SystemShell.EscapeInsideQuote(i) });

				string mode = UtilsString.RegExMatchOne(getInfo, "^IPv6: (.*?)$");
				string address = UtilsString.RegExMatchOne(getInfo, "^IPv6 IP address: (.*?)$");

				if ((mode == "") && (address != ""))
					mode = "LinkLocal";

				if (mode != "Off")
				{
					IpV6ModeEntry entry = new IpV6ModeEntry();
					entry.Interface = i;
					entry.Mode = mode;
					entry.Address = address;
					if (mode == "Manual")
					{
						entry.Router = UtilsString.RegExMatchOne(getInfo, "^IPv6 IP Router: (.*?)$");
						entry.PrefixLength = UtilsString.RegExMatchOne(getInfo, "^IPv6 Prefix Length: (.*?)$");
					}
					m_listIpV6Mode.Add(entry);

					SystemShell.Shell("/usr/sbin/networksetup", new string[] { "-setv6off", SystemShell.EscapeInsideQuote(i) });

					Engine.Instance.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.OsMacNetworkAdapterIPv6Disabled, i));
				}
			}

			Recovery.Save();

			base.OnIPv6Block();

			return true;
		}

		public override bool OnIPv6Restore()
		{
			foreach (IpV6ModeEntry entry in m_listIpV6Mode)
			{
				if (entry.Mode == "Off")
				{
					SystemShell.Shell("/usr/sbin/networksetup", new string[] { "-setv6off", SystemShell.EscapeInsideQuote(entry.Interface) });
				}
				else if (entry.Mode == "Automatic")
				{
					SystemShell.Shell("/usr/sbin/networksetup", new string[] { "-setv6automatic", SystemShell.EscapeInsideQuote(entry.Interface) });
				}
				else if (entry.Mode == "LinkLocal")
				{
					SystemShell.Shell("/usr/sbin/networksetup", new string[] { "-setv6LinkLocal", SystemShell.EscapeInsideQuote(entry.Interface) });
				}
				else if (entry.Mode == "Manual")
				{
					SystemShell.Shell("/usr/sbin/networksetup", new string[] { "-setv6manual", SystemShell.EscapeInsideQuote(entry.Interface), entry.Address, entry.PrefixLength, entry.Router });
				}

				Engine.Instance.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.OsMacNetworkAdapterIPv6Restored, entry.Interface));
			}

			m_listIpV6Mode.Clear();

			Recovery.Save();

			base.OnIPv6Restore();

			return true;
		}

		public override bool OnDnsSwitchDo(ConnectionActive connectionActive, IpAddresses dns)
		{
			string mode = Engine.Instance.Storage.GetLower("dns.mode");

			if (mode == "auto")
			{
				string[] interfaces = GetInterfaces();
				foreach (string i in interfaces)
				{
					string i2 = i.Trim();

					string currentStr = SystemShell.Shell("/usr/sbin/networksetup", new string[] { "-getdnsservers", SystemShell.EscapeInsideQuote(i2) });

					// v2
					IpAddresses current = new IpAddresses();
					foreach (string line in currentStr.Split('\n'))
					{
						string ip = line.Trim();
						if (IpAddress.IsIP(ip))
							current.Add(ip);
					}

					if (dns.Equals(current) == false)
					{
						DnsSwitchEntry e = new DnsSwitchEntry();
						e.Name = i2;
						e.Dns = current.Addresses;
						m_listDnsSwitch.Add(e);

						SystemShell s = new SystemShell();
						s.Path = LocateExecutable("networksetup");
						s.Arguments.Add("-setdnsservers");
						s.Arguments.Add(SystemShell.EscapeInsideQuote(i2));
						if (dns.IPs.Count == 0)
							s.Arguments.Add("empty");
						else
							foreach (IpAddress ip in dns.IPs)
								s.Arguments.Add(ip.Address);
						s.Run();

						Engine.Instance.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.OsMacNetworkAdapterDnsDone, i2, ((current.Count == 0) ? "Automatic" : current.Addresses), dns.Addresses));
					}
				}

				Recovery.Save();
			}

			base.OnDnsSwitchDo(connectionActive, dns);

			return true;
		}

		public override bool OnDnsSwitchRestore()
		{
			foreach (DnsSwitchEntry e in m_listDnsSwitch)
			{
				/*
				string v = e.Dns;
				if (v == "")
					v = "empty";
				v = v.Replace(",", "\" \"");
								
				SystemShell.Shell("/usr/sbin/networksetup", new string[] { "-setdnsservers", SystemShell.EscapeInsideQuote(e.Name), v });
				*/
				IpAddresses dns = new IpAddresses();
				dns.Add(e.Dns);

				SystemShell s = new SystemShell();
				s.Path = LocateExecutable("networksetup");
				s.Arguments.Add("-setdnsservers");
				s.Arguments.Add(SystemShell.EscapeInsideQuote(e.Name));
				if (dns.Count == 0)
					s.Arguments.Add("empty");
				else
					foreach (IpAddress ip in dns.IPs)
						s.Arguments.Add(ip.Address);
				s.Run();

				Engine.Instance.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.OsMacNetworkAdapterDnsRestored, e.Name, ((e.Dns == "") ? "Automatic" : e.Dns)));
			}

			m_listDnsSwitch.Clear();

			Recovery.Save();

			base.OnDnsSwitchRestore();

			return true;
		}

		public override string GetTunStatsMode()
		{
			// Mono NetworkInterface::GetIPv4Statistics().BytesReceived always return 0 under OSX.
			return "OpenVpnManagement";
		}

		public override void OnJsonNetworkInfo(Json jNetworkInfo)
		{
			// Step1: Set IPv6 support to true by default.
			// From base virtual, always 'false'. Missing Mono implementation? 
			// After for interfaces listed by 'networksetup -listallhardwareports' we detect specific support.
			foreach (Json jNetworkInterface in jNetworkInfo["interfaces"].Json.GetArray())
			{
				jNetworkInterface["support_ipv6"].Value = true;
			}

			// Step2: Query 'networksetup -listallhardwareports' to obtain a more accurate device friendly names.
			string networksetupPath = LocateExecutable("networksetup");
			if (networksetupPath != "")
			{
				string nsOutput = SystemShell.Shell1(networksetupPath, "-listallhardwareports");
				string lastName = "";
				foreach (string line in nsOutput.Split('\n'))
				{
					if (line.StartsWith("Hardware Port: ", StringComparison.InvariantCulture))
						lastName = line.Substring(15).Trim();
					if (line.StartsWith("Device:", StringComparison.InvariantCulture))
					{
						string deviceId = line.Substring(7).Trim();
						foreach (Json jNetworkInterface in jNetworkInfo["interfaces"].Json.GetArray())
						{
							if (jNetworkInterface["id"].Value as string == deviceId)
							{
								// Set friendly name
								jNetworkInterface["friendly"].Value = lastName;

								// Detect IPv6 support
								string getInfo = SystemShell.Shell(LocateExecutable("networksetup"), new string[] { "-getinfo", SystemShell.EscapeInsideQuote(lastName) });

								string mode = UtilsString.RegExMatchOne(getInfo, "^IPv6: (.*?)$").Trim();

								if (mode == "Off")
									jNetworkInterface["support_ipv6"].Value = false;
								else
									jNetworkInterface["support_ipv6"].Value = true;

								break;
							}
						}
					}
				}
			}
		}

		public override void OnJsonRouteList(Json jRoutesList)
		{
			base.OnJsonRouteList(jRoutesList);

			string netstatPath = LocateExecutable("netstat");
			if (netstatPath != "")
			{
				string result = SystemShell.Shell1(netstatPath, "-rnl");

				string[] lines = result.Split('\n');
				foreach (string line in lines)
				{
					if (line == "Routing tables")
						continue;
					if (line == "Internet:")
						continue;
					if (line == "Internet6:")
						continue;

					string[] fields = UtilsString.StringCleanSpace(line).Split(' ');

					if ((fields.Length > 0) && (fields[0].ToLowerInvariant().Trim() == "destination"))
						continue;

					if (fields.Length >= 7)
					{
						Json jRoute = new Json();
						IpAddress address = new IpAddress();
						if (fields[0].ToLowerInvariant().Trim() == "default")
						{
							IpAddress gateway = new IpAddress(fields[1]);
							if (gateway.Valid == false)
								continue;
							if (gateway.IsV4)
								address = IpAddress.DefaultIPv4;
							else if (gateway.IsV6)
								address = IpAddress.DefaultIPv6;
						}
						else
							address.Parse(fields[0]);
						if (address.Valid == false)
							continue;
						jRoute["address"].Value = address.ToCIDR();
						jRoute["gateway"].Value = fields[1];
						jRoute["flags"].Value = fields[2];
						jRoute["refs"].Value = fields[3];
						jRoute["use"].Value = fields[4];
						jRoute["mtu"].Value = fields[5];
						jRoute["interface"].Value = fields[6];
						if (fields.Length > 7)
							jRoute["expire"].Value = fields[7];
						jRoutesList.Append(jRoute);
					}
				}
			}
		}

		public string[] GetInterfaces() // TOCLEAN can be removed.
		{
			List<string> result = new List<string>();
			foreach (string line in SystemShell.Shell("/usr/sbin/networksetup", new string[] { "-listallnetworkservices" }).Split('\n'))
			{
				if (line.StartsWith("An asterisk", StringComparison.InvariantCultureIgnoreCase))
					continue;
				if (line.Trim() == "")
					continue;
				result.Add(line.Trim());
			}

			return result.ToArray();
		}
	}

	public class DnsSwitchEntry
	{
		public string Name;
		public string Dns;

		public void ReadXML(XmlElement node)
		{
			Name = UtilsXml.XmlGetAttributeString(node, "name", "");
			Dns = UtilsXml.XmlGetAttributeString(node, "dns", "");
		}

		public void WriteXML(XmlElement node)
		{
			UtilsXml.XmlSetAttributeString(node, "name", Name);
			UtilsXml.XmlSetAttributeString(node, "dns", Dns);
		}
	}

	public class IpV6ModeEntry
	{
		public string Interface;
		public string Mode;
		public string Address;
		public string Router;
		public string PrefixLength;

		public void ReadXML(XmlElement node)
		{
			Interface = UtilsXml.XmlGetAttributeString(node, "interface", "");
			Mode = UtilsXml.XmlGetAttributeString(node, "mode", "");
			Address = UtilsXml.XmlGetAttributeString(node, "address", "");
			Router = UtilsXml.XmlGetAttributeString(node, "router", "");
			PrefixLength = UtilsXml.XmlGetAttributeString(node, "prefix_length", "");
		}

		public void WriteXML(XmlElement node)
		{
			UtilsXml.XmlSetAttributeString(node, "interface", Interface);
			UtilsXml.XmlSetAttributeString(node, "mode", Mode);
			UtilsXml.XmlSetAttributeString(node, "address", Address);
			UtilsXml.XmlSetAttributeString(node, "router", Router);
			UtilsXml.XmlSetAttributeString(node, "prefix_length", PrefixLength);
		}
	}
}

