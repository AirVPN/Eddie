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
using System.IO;
using System.Text;
using System.Xml;
using AirVPN.Core;

namespace AirVPN.Platforms
{
    public class Osx : Platform
    {
		private string m_architecture = "";

		private List<DnsSwitchEntry> m_listDnsSwitch = new List<DnsSwitchEntry>();
		private List<IpV6ModeEntry> m_listIpV6Mode = new List<IpV6ModeEntry>();

        // Override
		public Osx()
		{
			m_architecture = NormalizeArchitecture(ShellPlatformIndipendent("sh", "-c 'uname -m'", "", true, false).Trim());
		}

		public override string GetCode()
		{
			return "OSX";
		}

		public override string GetName()
		{
			return ShellCmd("sw_vers -productVersion");
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
			// return true; // Uncomment for debugging

			// With root privileges by RootLauncher.cs, Environment.UserName still return the normal username, 'whoami' return 'root'.
			string u = ShellCmd ("whoami").ToLowerInvariant().Trim();
			return (u == "root");
        }

		public override bool IsUnixSystem()
		{
			return true;
		}

		public override string VersionDescription()
        {
			string o = base.VersionDescription();
            o += " - " + ShellCmd("uname -a").Trim();
            return o;
        }

        public override string DirSep
        {
            get
            {
                return "/";
            }
        }

		public override string GetExecutableReport(string path)
		{
			return ShellCmd("otool -L \"" + path + "\"");
		}

		public override string GetExecutablePath()
		{
			string currentPath = System.Reflection.Assembly.GetEntryAssembly().Location;
			if(new FileInfo(currentPath).Directory.Name == "MonoBundle")
			{
				// OSX Bundle detected, use the launcher executable
				currentPath = currentPath.Replace("/MonoBundle/","/MacOS/").Replace(".exe","");
			}
			return currentPath;
		}

        public override string GetUserFolder()
        {
            return Environment.GetEnvironmentVariable("HOME") + DirSep + ".airvpn";
        }

        public override string ShellCmd(string Command)
        {
            return Shell("sh", String.Format("-c '{0}'", Command), true);
        }

        public override void FlushDNS()
        {
			// 10.9
			ShellCmd("dscacheutil -flushcache");
			ShellCmd("killall -HUP mDNSResponder");

			// 10.10
			ShellCmd("discoveryutil udnsflushcaches");
            ShellCmd("discoveryutil mdnsflushcache"); // 2.11
        }

		public override void EnsureExecutablePermissions(string path)
		{
			if ((path == "") || (File.Exists(path) == false))
				return;

			ShellCmd("chmod +x \"" + path + "\"");
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
			base.RouteAdd (r);
		}

		public override void RouteRemove(RouteEntry r)
		{
			base.RouteRemove (r);
		}

		public override List<RouteEntry> RouteList()
		{	
			List<RouteEntry> entryList = new List<RouteEntry>();

			string result = ShellCmd("route -n -ee");

			string[] lines = result.Split('\n');
			foreach (string line in lines)
			{
				string[] fields = Utils.StringCleanSpace(line).Split(' ');

				if (fields.Length == 11)
				{
					RouteEntry e = new RouteEntry();
					e.Address = fields[0];
					e.Gateway = fields[1];
					e.Mask = fields[2];
					e.Flags = fields[3].ToUpperInvariant();
					e.Metrics = fields[4];
					// ref
					// use
					e.Interface = fields[7];
					e.Mss = fields[8];
					e.Window = fields[9];
					e.Irtt = fields[10];

					if (e.Address.Valid == false)
						continue;
					if (e.Gateway.Valid == false)
						continue;
					if (e.Mask.Valid == false)
						continue;

					entryList.Add(e);
				}
			}

			return entryList;
		}

        public override string GenerateSystemReport()
        {
            string t = base.GenerateSystemReport();

            t += "\n\n-- OS X specific\n";

            t += "\n-- ifconfig\n";
            t += ShellCmd("ifconfig");

            return t;
        }

        public override Dictionary<int, string> GetProcessesList()
		{
			// We experience some crash under OSX with the base method.
			
			Dictionary<int, string> result = new Dictionary<int,string>();

			String resultS = ShellCmd("top -b -n 1 | awk '{print $1,$12}'");

			string[] resultA = resultS.Split('\n');
			foreach (string pS in resultA)
			{
				int posS = pS.IndexOf(' ');
				if (posS != -1)
				{
					int pid = Conversions.ToInt32(pS.Substring(0, posS).Trim());
					string name = pS.Substring(posS).Trim().ToLowerInvariant();

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
			if (Engine.Instance.Storage.Get("ipv6.mode") == "disable")
			{
				string[] interfaces = GetInterfaces();
				foreach (string i in interfaces)
				{
					string getInfo = ShellCmd("networksetup -getinfo \"" + i + "\"");

					string mode = Utils.RegExMatchOne(getInfo, "^IPv6: (.*?)$");
					string address = Utils.RegExMatchOne(getInfo, "^IPv6 IP address: (.*?)$");
					
					if( (mode == "") && (address != "") )
						mode = "LinkLocal";

					if (mode != "Off")
					{
						Engine.Instance.Log(Engine.LogType.Info, Messages.Format(Messages.NetworkAdapterIpV6Disabled, i));

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

						ShellCmd("networksetup -setv6off \"" + i + "\"");
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
					ShellCmd("networksetup -setv6off \"" + entry.Interface + "\"");
				}
				else if (entry.Mode == "Automatic")
				{
					ShellCmd("networksetup -setv6automatic \"" + entry.Interface + "\"");
				}
				else if (entry.Mode == "LinkLocal")
				{
					ShellCmd("networksetup -setv6LinkLocal \"" + entry.Interface + "\"");
				}
				else if (entry.Mode == "Manual")
				{
					ShellCmd("networksetup -setv6manual \"" + entry.Interface + "\" " + entry.Address + " " + entry.PrefixLength + " " + entry.Router);
				}

				Engine.Instance.Log(Engine.LogType.Info, Messages.Format(Messages.NetworkAdapterIpV6Restored, entry.Interface));
			}

			m_listIpV6Mode.Clear();
			
			Recovery.Save();

			base.OnIpV6Restore();

			return true;
		}

		public override bool OnDnsSwitchDo(string dns)
		{
			string mode = Engine.Instance.Storage.Get("dns.mode").ToLowerInvariant();

			if (mode == "auto")
			{
				string[] interfaces = GetInterfaces();
				foreach (string i in interfaces)
				{
					string i2 = i.Trim();
					
					string current = ShellCmd("networksetup -getdnsservers \"" + i2 + "\"");

                    // v2
                    List<string> ips = new List<string>();
                    foreach(string line in current.Split('\n'))
                    {
                        string ip = line.Trim();
                        if (Utils.IsIP(ip))
                            ips.Add(ip);
                    }

                    if (ips.Count != 0)
                        current = String.Join(",", ips.ToArray());
                    else
                        current = "";
                    if (current != dns)
                    {
                        // Switch
                        Engine.Instance.Log(Engine.LogType.Info, Messages.Format(Messages.NetworkAdapterDnsDone, i2, ((current == "") ? "Automatic" : current), dns));

                        DnsSwitchEntry e = new DnsSwitchEntry();
                        e.Name = i2;
                        e.Dns = current;
                        m_listDnsSwitch.Add(e);

                        string dns2 = dns.Replace(",", "\" \"");
                        ShellCmd("networksetup -setdnsservers \"" + i2 + "\" \"" + dns2 + "\"");
                    }                    
				}

				Recovery.Save ();
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
				v = v.Replace (",", "\" \"");

				Engine.Instance.Log(Engine.LogType.Info, Messages.Format(Messages.NetworkAdapterDnsRestored, e.Name, ((e.Dns == "") ? "Automatic" : e.Dns)));
				ShellCmd("networksetup -setdnsservers \"" + e.Name + "\" \"" + v + "\"");
			}

			m_listDnsSwitch.Clear();

			Recovery.Save ();

			base.OnDnsSwitchRestore();

			return true;
		}

		public override string GetTunStatsMode()
		{
			// Mono NetworkInterface::GetIPv4Statistics().BytesReceived always return 0 under OSX.
			return "OpenVpnManagement";
		}

		public override string GetGitDeployPath()
		{
			// Under OSX, binary is inside a bundle AirVPN.app/Contents/MacOS/
			return GetProgramFolder () + "/../../../../../../../deploy/" + Platform.Instance.GetSystemCode () + "/";
		}

		public string[] GetInterfaces()
		{
			string[] interfaces = ShellCmd("networksetup -listallnetworkservices | grep -v denotes").Split('\n');
			return interfaces;
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

