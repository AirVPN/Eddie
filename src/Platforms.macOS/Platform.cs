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
using MonoMac.AppKit;
using MonoMac.Foundation;

namespace Eddie.Platforms.MacOS
{
	public class Platform : Core.Platform
	{
		private string m_version = "";
		private string m_architecture = "";

		private List<DnsSwitchEntry> m_listDnsSwitch = new List<DnsSwitchEntry>();
		private List<IpV6ModeEntry> m_listIpV6Mode = new List<IpV6ModeEntry>();

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
			return SystemShell.ShellCmd("sw_vers -productVersion");
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
			string u = SystemShell.Shell("/usr/bin/whoami", new string[] { }).ToLowerInvariant().Trim();
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

		public override void FileEnsurePermission(string path, string mode)
		{
			if ((path == "") || (Platform.Instance.FileExists(path) == false))
				return;

			// 'mode' not escaped, called hard-coded.
			SystemShell s = new SystemShell();
			s.Path = "/bin/chmod";
			s.Arguments.Add(mode);
			s.Arguments.Add(SystemShell.EscapePath(path));
			s.NoDebugLog = true;
			s.Run();
		}

		public override void FileEnsureExecutablePermission(string path)
		{
			FileEnsurePermission(path, "+x");
		}

		public override string GetExecutableReport(string path)
		{
			return SystemShell.ShellCmd("otool -L \"" + SystemShell.EscapePath(path) + "\"");
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
			Engine.Instance.Logs.Log(LogType.Verbose, Messages.ConnectionFlushDNS);

			// 10.9
			SystemShell.ShellCmd("dscacheutil -flushcache");
			SystemShell.ShellCmd("killall -HUP mDNSResponder");

			// 10.10
			SystemShell.ShellCmd("discoveryutil udnsflushcaches");
			SystemShell.ShellCmd("discoveryutil mdnsflushcache"); // 2.11
		}

		public override void ShellCommandDirect(string command, out string path, out string[] arguments)
		{
			path = "/bin/sh";
			arguments = new string[] { "-c", command };
		}

		public override void ShellSync(string path, string[] arguments, out string stdout, out string stderr)
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
			}
			catch (Exception ex)
			{
				stdout = "";
				stderr = "Error: " + ex.Message;
			}
		}

		public override bool SearchTool(string name, string relativePath, ref string path, ref string location)
		{
			string pathBin = "/usr/bin/" + name;
			if (Platform.Instance.FileExists(pathBin))
			{
				path = pathBin;
				location = "system";
				return true;
			}

			string pathSBin = "/usr/sbin/" + name;
			if (Platform.Instance.FileExists(pathSBin))
			{
				path = pathSBin;
				location = "system";
				return true;
			}

			// Look in application bundle resources
			string resPath = NormalizePath(relativePath) + "/../Resources/" + name;
			if (File.Exists(resPath))
			{
				path = resPath;
				location = "bundle";
				return true;
			}

			return base.SearchTool(name, relativePath, ref path, ref location);
		}

		// Encounter Mono issue about the .Net method on OS X, similar to Mono issue under Linux. Use shell instead, like Linux
		public override long Ping(string host, int timeoutSec)
		{
			// Note: Linux timeout is -w, OS X timeout is -t
			/*
            string args = "-c 1 -t " + SystemShell.EscapeInt(timeoutSec) + " -q -n " + SystemShell.EscapeHost(host);
            string result = Shell("/sbin/ping", args);
            */
			/*
            string result = "";
            try
            {
				var pipeOut = new NSPipe();

				var t = new NSTask();
                //t.LaunchPath = "/sbin/ping";
                //t.Arguments = new string[] { "-c 1 -t " + SystemShell.EscapeInt(timeoutSec) + " -q -n " + SystemShell.EscapeHost(host) };

                t.LaunchPath = "/bin/sh";
                t.Arguments = new string[] { "-c", "/sbin/ping -c 1 -t " + SystemShell.EscapeInt(timeoutSec) + " -q -n " + SystemShell.EscapeHost(host) };
				t.StandardOutput = pipeOut;

				t.Launch();
				t.WaitUntilExit();
				//t.Release();
				t.Dispose();

                NSFileHandle f = pipeOut.ReadHandle;
                result = f.ReadDataToEndOfFile().ToString();
                f.CloseFile();
            }
            catch(Exception e)
            {
                Engine.Instance.Logs.Log(e);
            }
            */

			SystemShell s = new SystemShell();
			s.Path = "/sbin/ping";
			s.Arguments.Add("-c 1");
			s.Arguments.Add("-t " + timeoutSec.ToString());
			s.Arguments.Add("-q");
			s.Arguments.Add("-n");
			s.Arguments.Add(SystemShell.EscapeHost(host));
			s.NoDebugLog = true;

			float iMS = -1;
			if (s.Run())
			{
				// Note: Linux have mdev, OS X have stddev
				string sMS = Utils.ExtractBetween(result, "min/avg/max/stddev = ", "/");
				if (float.TryParse(sMS, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out iMS) == false)
					iMS = -1;
			}

			return (long)iMS;
		}

		public override string GetSystemFont()
		{
			// Crash with Xamarin 6.1.2
			return "";
		}

		public override string GetSystemFontMonospace()
		{
			// Crash with Xamarin 6.1.2
			return "";
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

		public override void RouteAdd(RouteEntry r)
		{
			base.RouteAdd(r);
		}

		public override void RouteRemove(RouteEntry r)
		{
			base.RouteRemove(r);
		}

		public override IpAddresses ResolveDNS(string host)
		{
			// Base method with Dns.GetHostEntry have cache issue, for example on Fedora. OS X it's based on Mono.
			// Also, base methods with Dns.GetHostEntry sometime don't fetch AAAA IPv6 addresses.

			IpAddresses result = new IpAddresses();

			// Note: CNAME record are automatically followed.
			SystemShell s = new SystemShell();
			s.Path = "/usr/bin/host";
			s.Arguments.Add("-W 5");
			s.Arguments.Add(SystemShell.EscapeHost(host));
			if(s.Run())
			{
				string hostout = s.Output;
				foreach (string line in hostout.Split('\n'))
				{
					string ipv4 = Utils.RegExMatchOne(line, "^.*? has address (.*?)$");
					if (ipv4 != "")
						result.Add(ipv4.Trim());

					string ipv6 = Utils.RegExMatchOne(line, "^.*? has IPv6 address (.*?)$");
					if (ipv6 != "")
						result.Add(ipv6.Trim());
				}
			}
			
			return result;
		}

		public override IpAddresses DetectDNS()
		{
			IpAddresses list = new IpAddresses();
			string[] interfaces = GetInterfaces();
			foreach (string i in interfaces)
			{
				string i2 = i.Trim();

				//string current = SystemShell.ShellCmd("networksetup -getdnsservers \"" + SystemShell.EscapeInsideQuote(i2) + "\"");
				string current = SystemShell.Shell("/usr/sbin/networksetup", new string[] { "-getdnsservers", SystemShell.EscapeInsideQuote(i2) });

				list.Add(current);
			}
			return list;
		}

		public override List<RouteEntry> RouteList()
		{
			List<RouteEntry> entryList = new List<RouteEntry>();

			string result = SystemShell.ShellCmd("netstat -rnl");

			string[] lines = result.Split('\n');
			foreach (string line in lines)
			{
				if (line == "Routing tables")
					continue;
				if (line == "Internet:")
					continue;
				if (line == "Internet6:")
					continue;

				string[] fields = Utils.StringCleanSpace(line).Split(' ');

				if (fields.Length == 8)
				{
					if (fields[0] == "Destination")
						continue;

					RouteEntry e = new RouteEntry();
					e.Address = fields[0];
					e.Gateway = fields[1];
					e.Flags = fields[2];
					// Refs
					// Use
					// Mtu
					e.Interface = fields[6];
					// Expire

					if (e.Address.Valid == false)
						continue;
					if (e.Gateway.Valid == false)
						continue;

					entryList.Add(e);
				}
			}

			return entryList;
		}

		public override void OnReport(Report report)
		{
			base.OnReport(report);

			report.Add("ifconfig", SystemShell.ShellCmd("ifconfig"));
			report.Add("netstat /rnl", SystemShell.ShellCmd("netstat /rnl"));
		}

		public override Dictionary<int, string> GetProcessesList()
		{
			// We experience some crash under OSX with the base method.

			Dictionary<int, string> result = new Dictionary<int, string>();
			String resultS = SystemShell.ShellCmd("ps -eo pid,command");
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

			return result;
		}

		public override bool OnCheckEnvironment()
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
			XmlElement nodeDns = Utils.XmlGetFirstElementByTagName(root, "DnsSwitch");
			if (nodeDns != null)
			{
				foreach (XmlElement nodeEntry in nodeDns.ChildNodes)
				{
					DnsSwitchEntry entry = new DnsSwitchEntry();
					entry.ReadXML(nodeEntry);
					m_listDnsSwitch.Add(entry);
				}
			}

			XmlElement nodeIpV6 = Utils.XmlGetFirstElementByTagName(root, "IpV6");
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

		public override bool OnIpV6Do()
		{
			if (Engine.Instance.Storage.GetLower("ipv6.mode") == "disable")
			{
				string[] interfaces = GetInterfaces();
				foreach (string i in interfaces)
				{
					string getInfo = SystemShell.Shell("/usr/sbin/networksetup", new string[] { "-getinfo", SystemShell.EscapeInsideQuote(i) });
					
					string mode = Utils.RegExMatchOne(getInfo, "^IPv6: (.*?)$");
					string address = Utils.RegExMatchOne(getInfo, "^IPv6 IP address: (.*?)$");

					if ((mode == "") && (address != ""))
						mode = "LinkLocal";

					if (mode != "Off")
					{
						Engine.Instance.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.NetworkAdapterIpV6Disabled, i));

						IpV6ModeEntry entry = new IpV6ModeEntry();
						entry.Interface = i;
						entry.Mode = mode;
						entry.Address = address;
						if (mode == "Manual")
						{
							entry.Router = Utils.RegExMatchOne(getInfo, "^IPv6 IP Router: (.*?)$");
							entry.PrefixLength = Utils.RegExMatchOne(getInfo, "^IPv6 Prefix Length: (.*?)$");
						}
						m_listIpV6Mode.Add(entry);

						SystemShell.Shell("/usr/sbin/networksetup", new string[] { "-setv6off", SystemShell.EscapeInsideQuote(i) });
					}
				}

				Recovery.Save();
			}

			base.OnIpV6Do();

			return true;
		}

		public override bool OnIpV6Restore()
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

				Engine.Instance.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.NetworkAdapterIpV6Restored, entry.Interface));
			}

			m_listIpV6Mode.Clear();

			Recovery.Save();

			base.OnIpV6Restore();

			return true;
		}

		public override bool OnDnsSwitchDo(IpAddresses dns)
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
						current.Add(ip);
					}

					if (dns.Equals(current) == false)
					{
						// Switch
						Engine.Instance.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.NetworkAdapterDnsDone, i2, ((current.Count == 0) ? "Automatic" : current.Addresses), dns.Addresses));

						DnsSwitchEntry e = new DnsSwitchEntry();
						e.Name = i2;
						e.Dns = current.Addresses;
						m_listDnsSwitch.Add(e);

						string dns2 = dns.Addresses.Replace(",", "\" \"");
						SystemShell.Shell("/usr/sbin/networksetup", new string[] { "-setdnsservers", SystemShell.EscapeInsideQuote(i2), dns2 });
					}
				}

				Recovery.Save();
			}

			base.OnDnsSwitchDo(dns);

			return true;
		}

		public override bool OnDnsSwitchRestore()
		{
			foreach (DnsSwitchEntry e in m_listDnsSwitch)
			{
				string v = e.Dns;
				if (v == "")
					v = "empty";
				v = v.Replace(",", "\" \"");

				Engine.Instance.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.NetworkAdapterDnsRestored, e.Name, ((e.Dns == "") ? "Automatic" : e.Dns)));
				SystemShell.Shell("/usr/sbin/networksetup", new string[] { "-setdnsservers", SystemShell.EscapeInsideQuote(e.Name), v });
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

		public string[] GetInterfaces()
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
			Name = Utils.XmlGetAttributeString(node, "name", "");
			Dns = Utils.XmlGetAttributeString(node, "dns", "");
		}

		public void WriteXML(XmlElement node)
		{
			Utils.XmlSetAttributeString(node, "name", Name);
			Utils.XmlSetAttributeString(node, "dns", Dns);
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
			Interface = Utils.XmlGetAttributeString(node, "interface", "");
			Mode = Utils.XmlGetAttributeString(node, "mode", "");
			Address = Utils.XmlGetAttributeString(node, "address", "");
			Router = Utils.XmlGetAttributeString(node, "router", "");
			PrefixLength = Utils.XmlGetAttributeString(node, "prefix_length", "");
		}

		public void WriteXML(XmlElement node)
		{
			Utils.XmlSetAttributeString(node, "interface", Interface);
			Utils.XmlSetAttributeString(node, "mode", Mode);
			Utils.XmlSetAttributeString(node, "address", Address);
			Utils.XmlSetAttributeString(node, "router", Router);
			Utils.XmlSetAttributeString(node, "prefix_length", PrefixLength);
		}
	}
}

