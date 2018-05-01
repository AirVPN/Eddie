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
using System.ServiceProcess;
using System.Text;
using System.Xml;
using Eddie.Core;
using Microsoft.Win32;

namespace Eddie.Platform.Windows
{
	public class NetworkLockWindowsFirewallProfile
	{
		public string id;
		public bool State = false;
		public string Inbound = "";
		public string Outbound = "";
		public bool Notifications = false;

		public NetworkLockWindowsFirewallProfile(string name)
		{
			id = name;
		}

		public void Fetch()
		{
			string regkey = GetRegPath();

			/* < 2.11
			string report = Platform.Instance.ShellCmd("netsh advfirewall show " + id);

			State = report.IndexOf("ON") != -1;

			if (report.IndexOf("AllowInbound") != -1)
				Inbound = "AllowInbound";
			else if (report.IndexOf("BlockInboundAlways") != -1)
				Inbound = "BlockInboundAlways";
			else if (report.IndexOf("BlockInbound") != -1)
				Inbound = "BlockInbound";

			if (report.IndexOf("AllowOutbound") != -1)
				Outbound = "AllowOutbound";
			else if (report.IndexOf("BlockOutbound") != -1)
				Outbound = "BlockOutbound";

            Notifications = (Registry.GetValue(regkey, "DisableNotifications", 0).ToString() == "0");
            */

			int enableFirewall = Convert.ToInt32(Registry.GetValue(regkey, "EnableFirewall", 0));
			int disableNotifications = Convert.ToInt32(Registry.GetValue(regkey, "DisableNotifications", 0));
			int defaultInboundAction = Convert.ToInt32(Registry.GetValue(regkey, "DefaultInboundAction", 1));
			int defaultOutboundAction = Convert.ToInt32(Registry.GetValue(regkey, "DefaultOutboundAction", 0));
			int doNotAllowExceptions = Convert.ToInt32(Registry.GetValue(regkey, "DoNotAllowExceptions", 0));

			State = (enableFirewall == 1);
			Notifications = (disableNotifications == 0);

			if (defaultInboundAction == 0)
				Inbound = "AllowInbound";
			else if (defaultInboundAction == 1)
				Inbound = "BlockInbound";

			if (doNotAllowExceptions == 1)
				Inbound = "BlockInboundAlways";

			if (defaultOutboundAction == 0)
				Outbound = "AllowOutbound";
			else if (defaultOutboundAction == 1)
				Outbound = "BlockOutbound";
		}

		public string GetRegPath()
		{
			string notificationsProfileName = "";
			if (id == "domain")
				notificationsProfileName = "DomainProfile";
			else if (id == "private")
				notificationsProfileName = "StandardProfile";
			else if (id == "public")
				notificationsProfileName = "PublicProfile";
			else
			{
				notificationsProfileName = "StandardProfile";
			}

			return "HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\services\\SharedAccess\\Parameters\\FirewallPolicy\\" + notificationsProfileName;
		}

		/*
		public string GetOldFirewallProfileName()
		{
			if (id == "domain")
				return "DOMAIN";
			else if (id == "private")
				return "STANDARD";
			else if (id == "public")
				return "CURRENT";
			else
				return "ALL";
		}
        */

		public void StateOn()
		{
			SystemShell.ShellCmd("netsh advfirewall set " + UtilsString.StringSafe(id) + " state on");
		}

		public void StateOff()
		{
			SystemShell.ShellCmd("netsh advfirewall set " + UtilsString.StringSafe(id) + " state off");
		}

		public void NotifyOn()
		{
			//Registry.SetValue(GetNotificationRegPath(), "DisableNotifications", 0, RegistryValueKind.DWord);
			//Platform.Instance.ShellCmd("netsh firewall set notifications mode=enable profile=" + GetOldFirewallProfileName());
			SystemShell.ShellCmd("netsh advfirewall set " + UtilsString.StringSafe(id) + " settings inboundusernotification enable");
		}

		public void NotifyOff()
		{
			//Registry.SetValue(GetNotificationRegPath(), "DisableNotifications", 1, RegistryValueKind.DWord);
			//Platform.Instance.ShellCmd("netsh firewall set notifications mode=disable profile=" + GetOldFirewallProfileName());
			SystemShell.ShellCmd("netsh advfirewall set " + UtilsString.StringSafe(id) + " settings inboundusernotification disable");
		}

		public void RestorePolicy()
		{
			SystemShell.ShellCmd("netsh advfirewall set " + UtilsString.StringSafe(id) + " firewallpolicy " + Inbound + "," + Outbound);
		}

		public void ReadXML(XmlElement node)
		{
			State = (node.GetAttribute("state") == "1");
			Inbound = node.GetAttribute("inbound");
			Outbound = node.GetAttribute("outbound");
		}

		public void WriteXML(XmlElement node)
		{
			node.SetAttribute("state", State ? "1" : "0");
			node.SetAttribute("inbound", Inbound);
			node.SetAttribute("outbound", Outbound);
		}
	}

	public class NetworkLockWindowsFirewall : NetworkLockPlugin
	{
		private List<NetworkLockWindowsFirewallProfile> Profiles = new List<NetworkLockWindowsFirewallProfile>();
		private bool m_serviceStatus = false;
		private bool m_activated = false;
		private string m_lastestIpsWhiteListOutgoing = "";

		public override string GetCode()
		{
			return "windows_firewall";
		}

		public override string GetName()
		{
			return "Windows Firewall";
		}

		public override string GetDescription()
		{
			return "Not recommended";
		}

		public override void Init()
		{
			base.Init();

			Profiles.Clear();
			Profiles.Add(new NetworkLockWindowsFirewallProfile("domain"));
			Profiles.Add(new NetworkLockWindowsFirewallProfile("private"));
			Profiles.Add(new NetworkLockWindowsFirewallProfile("public"));
		}

		public override void Activation()
		{
			base.Activation();

			// Service
			{
				ServiceController service = null;
				try
				{
					service = new ServiceController("MpsSvc");
					m_serviceStatus = (service.Status == ServiceControllerStatus.Running);
					if (m_serviceStatus == false)
					{
						TimeSpan timeout = TimeSpan.FromMilliseconds(10000);
						service.Start();
						service.WaitForStatus(ServiceControllerStatus.Running, timeout);
					}
				}
				catch (Exception e)
				{
					if (e.Message.Contains("MpsSvc"))
						throw new Exception(Messages.NetworkLockWindowsFirewallUnableToStartService);
					else
						throw e;
				}
				finally
				{
					if (service != null)
						service.Dispose();
				}
			}

			// If 'winfirewall_rules_original.airvpn' doesn't exists, create it. It's a general backup of the first time.
			// We create this kind of file in Windows System directory, because it's system critical data, and to allow it to survive between re-installation of the software.
			string rulesBackupFirstTime = Engine.Instance.Storage.GetPathInData("winfirewall_rules_original.wfw");
			if (Platform.Instance.FileExists(rulesBackupFirstTime) == false)
				SystemShell.ShellCmd("netsh advfirewall export \"" + SystemShell.EscapePath(rulesBackupFirstTime) + "\"");

			string rulesBackupSession = Engine.Instance.Storage.GetPathInData("winfirewall_rules_backup.wfw");
			if (Platform.Instance.FileExists(rulesBackupSession))
				Platform.Instance.FileDelete(rulesBackupSession);
			SystemShell.ShellCmd("netsh advfirewall export \"" + SystemShell.EscapePath(rulesBackupSession) + "\"");
			if (Platform.Instance.FileExists(rulesBackupSession) == false)
				throw new Exception(Messages.NetworkLockWindowsFirewallBackupFailed);

			foreach (NetworkLockWindowsFirewallProfile profile in Profiles)
				profile.Fetch();

			foreach (NetworkLockWindowsFirewallProfile profile in Profiles)
			{
				if (profile.State == false)
				{
					profile.StateOn();
				}

				/*
				if (profile.Notifications == true)
				{
					profile.NotifyOff();
				}
                */
			}

			// Disable all notifications
			SystemShell.ShellCmd("netsh advfirewall set allprofiles settings inboundusernotification disable");

			SystemShell.ShellCmd("netsh advfirewall firewall delete rule name=all");

			// Windows Firewall don't work with logical path (a path that contain hardlink)
			SystemShell.ShellCmd("netsh advfirewall firewall add rule name=\"Eddie - Out - Program Eddie\" dir=out action=allow program=\"" + SystemShell.EscapePath(Platform.Instance.FileGetPhysicalPath(Platform.Instance.GetExecutablePath())) + "\" enable=yes");

			// Adding rules are slow, so force at least curl
			SystemShell.ShellCmd("netsh advfirewall firewall add rule name=\"Eddie - Out - Program curl\" dir=out action=allow program=\"" + SystemShell.EscapePath(Platform.Instance.FileGetPhysicalPath(Software.GetTool("curl").Path)) + "\" enable=yes");

			if (Engine.Instance.Storage.GetBool("netlock.allow_ping") == true)
			{
				SystemShell.ShellCmd("netsh advfirewall firewall add rule name=\"Eddie - In - ICMP IPv4\" dir=in action=allow protocol=icmpv4:8,any");
				SystemShell.ShellCmd("netsh advfirewall firewall add rule name=\"Eddie - In - ICMP IPv6\" dir=in action=allow protocol=icmpv6:8,any");
				SystemShell.ShellCmd("netsh advfirewall firewall add rule name=\"Eddie - Out - ICMP IPv4\" dir=out action=allow protocol=icmpv4:8,any");
				SystemShell.ShellCmd("netsh advfirewall firewall add rule name=\"Eddie - Out - ICMP IPv6\" dir=out action=allow protocol=icmpv6:8,any");
			}

			// Exec("netsh advfirewall firewall add rule name=\"Eddie - IPv6 Block - Low\" dir=out remoteip=0000::/1 action=allow");
			// Exec("netsh advfirewall firewall add rule name=\"Eddie - IPv6 Block - High\" dir=out remoteip=8000::/1 action=allow");

			if (Engine.Instance.Storage.GetBool("netlock.allow_private") == true)
			{
				SystemShell.ShellCmd("netsh advfirewall firewall add rule name=\"Eddie - In - AllowLocal\" dir=in action=allow remoteip=LocalSubnet");
				SystemShell.ShellCmd("netsh advfirewall firewall add rule name=\"Eddie - Out - AllowLocal\" dir=out action=allow remoteip=LocalSubnet");

				SystemShell.ShellCmd("netsh advfirewall firewall add rule name=\"Eddie - Out - AllowMulticast\" dir=out action=allow remoteip=224.0.0.0/24");
				SystemShell.ShellCmd("netsh advfirewall firewall add rule name=\"Eddie - Out - AllowSimpleServiceDiscoveryProtocol\" dir=out action=allow remoteip=239.255.255.250/32");
				SystemShell.ShellCmd("netsh advfirewall firewall add rule name=\"Eddie - Out - ServiceLocationProtocol\" dir=out action=allow remoteip=239.255.255.253/32");
			}

			// This is not optimal, it maybe also allow LAN traffic, but we can't find a better alternative (interfacetype=ras don't work) and WinFirewall method must be deprecated.
			SystemShell.ShellCmd("netsh advfirewall firewall add rule name=\"Eddie - In - AllowVPN\" dir=in action=allow localip=10.0.0.0/8");
			SystemShell.ShellCmd("netsh advfirewall firewall add rule name=\"Eddie - Out - AllowVPN\" dir=out action=allow localip=10.0.0.0/8");

			// Without this, Windows stay in 'Identifying network...' and OpenVPN in 'Waiting TUN to come up'.
			SystemShell.ShellCmd("netsh advfirewall firewall add rule name=\"Eddie - Out - DHCP\" dir=out action=allow protocol=UDP localport=68 remoteport=67 program=\"%SystemRoot%\\system32\\svchost.exe\" service=\"dhcp\"");

			string cmd = "netsh advfirewall set allprofiles firewallpolicy ";
			if (Engine.Instance.Storage.Get("netlock.incoming") == "allow")
				cmd += "allowinbound";
			else
				cmd += "blockinbound";
			cmd += ",";
			if (Engine.Instance.Storage.Get("netlock.outgoing") == "allow")
				cmd += "allowoutbound";
			else
				cmd += "blockoutbound";
			SystemShell.ShellCmd(cmd);

			m_activated = true; // To avoid OnUpdateIps before this moment

			OnUpdateIps();
		}

		public override void Deactivation()
		{
			base.Deactivation();

			foreach (NetworkLockWindowsFirewallProfile profile in Profiles)
				profile.RestorePolicy();

			// Not need, already restored in after import
			// Exec("netsh advfirewall firewall delete rule name=\"Eddie - In - AllowLocal\"");
			// Exec("netsh advfirewall firewall delete rule name=\"Eddie - Out - AllowLocal\"");
			// Exec("netsh advfirewall firewall delete rule name=\"Eddie - Out - AllowVPN\"");
			// Exec("netsh advfirewall firewall delete rule name=\"Eddie - Out - AllowAirIPS\"");
			// Exec("netsh advfirewall firewall delete rule name=\"Eddie - Out - DHCP\"");

			// >2.9.1 edition
			{
				string rulesBackupSession = Engine.Instance.Storage.GetPathInData("winfirewall_rules_backup.wfw");
				if (Platform.Instance.FileExists(rulesBackupSession))
				{
					SystemShell.ShellCmd("netsh advfirewall import \"" + SystemShell.EscapePath(rulesBackupSession) + "\"");
					Platform.Instance.FileDelete(rulesBackupSession);
				}
			}

			// Old <2.8 edition
			{
				string rulesBackupSession = Engine.Instance.Storage.GetPathInData("winfirewallrules.wfw");
				if (Platform.Instance.FileExists(rulesBackupSession))
				{
					SystemShell.ShellCmd("netsh advfirewall import \"" + SystemShell.EscapePath(rulesBackupSession) + "\"");
					Platform.Instance.FileDelete(rulesBackupSession);
				}
			}

			// Old 2.9.0 edition, recover
			{
				string rulesBackupSession = Environment.SystemDirectory + Platform.Instance.DirSep + "winfirewall_rules_original.airvpn";
				if (Platform.Instance.FileExists(rulesBackupSession))
				{
					SystemShell.ShellCmd("netsh advfirewall import \"" + SystemShell.EscapePath(rulesBackupSession) + "\"");
					Platform.Instance.FileDelete(rulesBackupSession);
				}
			}

			foreach (NetworkLockWindowsFirewallProfile profile in Profiles)
			{
				if (profile.State == false)
					profile.StateOff();
				// <2.11 Not need, already restored in below import
				// >=2.11 Restored, otherwise are not correctly restored in nt-domain environment.
				if (profile.Notifications == true)
					profile.NotifyOn();
			}

			// Service
			if (m_serviceStatus == false)
			{
				ServiceController service = null;
				try
				{
					service = new ServiceController("MpsSvc");
					TimeSpan timeout = TimeSpan.FromMilliseconds(30000);
					service.Stop();
					service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
				}
				finally
				{
					if (service != null)
						service.Dispose();
				}
			}

			m_lastestIpsWhiteListOutgoing = "";
		}

		public override void AllowProgram(string path, string name, string guid)
		{
			base.AllowProgram(path, name, guid);

			if (path == Software.GetTool("curl").Path)
				return;

			// Windows Firewall don't work with logical path (a path that contain hardlink)
			string physicalPath = Platform.Instance.FileGetPhysicalPath(path);

			SystemShell.ShellCmd("netsh advfirewall firewall add rule name=\"Eddie - Out - AllowProgram - " + guid + "\" dir=out action=allow program=\"" + SystemShell.EscapePath(physicalPath) + "\" enable=yes");
		}

		public override void DeallowProgram(string path, string name, string guid)
		{
			base.DeallowProgram(path, name, guid);

			if (path == Software.GetTool("curl").Path)
				return;

			SystemShell.ShellCmd("netsh advfirewall firewall delete rule name=\"Eddie - Out - AllowProgram - " + guid + "\"");
		}

		public override void OnUpdateIps()
		{
			if (m_activated == false)
				return;

			IpAddresses ipsWhiteListOutgoing = GetIpsWhiteListOutgoing(true); // ClodoTemp: can be 'false', but pinger don't work
			string currentIpsWhiteListOutgoing = "";
			foreach (IpAddress ip in ipsWhiteListOutgoing.IPs)
			{
				if (currentIpsWhiteListOutgoing != "")
					currentIpsWhiteListOutgoing += ",";
				currentIpsWhiteListOutgoing += ip.ToCIDR();
			}

			if (currentIpsWhiteListOutgoing != m_lastestIpsWhiteListOutgoing)
			{
				if (m_lastestIpsWhiteListOutgoing != "")
					SystemShell.ShellCmd("netsh advfirewall firewall set rule name=\"Eddie - Out - Allow IPs\" dir=out new action=allow remoteip=\"" + m_lastestIpsWhiteListOutgoing + "\"");
				else
					SystemShell.ShellCmd("netsh advfirewall firewall add rule name=\"Eddie - Out - Allow IPs\" dir=out action=allow remoteip=\"" + m_lastestIpsWhiteListOutgoing + "\"");

				m_lastestIpsWhiteListOutgoing = currentIpsWhiteListOutgoing;
			}
		}

		public override void OnRecoveryLoad(XmlElement root)
		{
			base.OnRecoveryLoad(root);

			m_serviceStatus = (root.GetAttribute("service") != "0");

			foreach (NetworkLockWindowsFirewallProfile profile in Profiles)
			{
				XmlElement node = UtilsXml.XmlGetFirstElementByTagName(root, profile.id);
				if (node != null)
				{
					profile.ReadXML(node);
				}
			}
		}

		public override void OnRecoverySave(XmlElement root)
		{
			base.OnRecoverySave(root);

			root.SetAttribute("service", m_serviceStatus ? "1" : "0");

			foreach (NetworkLockWindowsFirewallProfile profile in Profiles)
			{
				XmlElement el = (XmlElement)root.AppendChild(root.OwnerDocument.CreateElement(profile.id));
				profile.WriteXML(el);
			}
		}
	}
}
