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
	public class NetworkLockWindowsFirewallProfile
	{
		public string id;
		public bool State = false;
		public string Inbound = "";
		public string Outbound = "";

		public NetworkLockWindowsFirewallProfile(string name)
		{
			id = name;
		}

		public void Fetch()
		{
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
		}

		public void StateOn()
		{
			Platform.Instance.ShellCmd("netsh advfirewall set " + id + " state on");
		}

		public void StateOff()
		{
			Platform.Instance.ShellCmd("netsh advfirewall set " + id + " state off");
		}

		public void RestorePolicy()
		{
			Platform.Instance.ShellCmd("netsh advfirewall set " + id + " firewallpolicy " + Inbound + "," + Outbound);
		}
	}

	public class NetworkLockWindowsFirewall : NetworkLockPlugin
	{
		private List<NetworkLockWindowsFirewallProfile> Profiles = new List<NetworkLockWindowsFirewallProfile>();
		private bool m_activated;
		private string m_lastestIpList;

		public override string GetCode()
		{
			return "windows_firewall";
		}

		public override string GetName()
		{
			return "Windows Firewall";
		}
			
		public override void Activation()
		{
			base.Activation();

			// If 'backup.wfw' doesn't exists, create it. It's a general backup of the first time.
			string rulesBackupFirstTime = Storage.DataPath + Platform.Instance.DirSep + "winfirewallrulesorig.wfw";
			if (File.Exists(rulesBackupFirstTime) == false)
				Platform.Instance.ShellCmd("netsh advfirewall export \"" + rulesBackupFirstTime + "\"");

			string rulesBackupSession = Storage.DataPath + Platform.Instance.DirSep + "winfirewallrules.wfw";
			if (File.Exists(rulesBackupSession))
				File.Delete(rulesBackupSession);
			Platform.Instance.ShellCmd("netsh advfirewall export \"" + rulesBackupSession + "\"");
			if (File.Exists(rulesBackupSession) == false)
				throw new Exception(Messages.NetworkLockWindowsFirewallBackupFailed);

			Profiles.Clear();
			Profiles.Add(new NetworkLockWindowsFirewallProfile("domain"));
			Profiles.Add(new NetworkLockWindowsFirewallProfile("private"));
			Profiles.Add(new NetworkLockWindowsFirewallProfile("public"));

			foreach (NetworkLockWindowsFirewallProfile profile in Profiles)
				profile.Fetch();

			foreach (NetworkLockWindowsFirewallProfile profile in Profiles)
			{
				if (profile.State == false)
					profile.StateOn();
			}

			Platform.Instance.ShellCmd("netsh advfirewall firewall delete rule name=all");
			
			Platform.Instance.ShellCmd("netsh advfirewall firewall add rule name=\"AirVPN - Out - AllowLocal\" dir=out action=allow remoteip=LocalSubnet");
			Platform.Instance.ShellCmd("netsh advfirewall firewall add rule name=\"AirVPN - Out - AllowVPN\" dir=out action=allow localip=10.4.0.0-10.9.255.255");

			Platform.Instance.ShellCmd("netsh advfirewall set allprofiles firewallpolicy BlockInbound,BlockOutbound");

			m_activated = true; // To avoid OnUpdateIps before this moment

			OnUpdateIps();
		}

		public override void Deactivation()
		{
			base.Deactivation();

			foreach (NetworkLockWindowsFirewallProfile profile in Profiles)
				profile.RestorePolicy();

			Platform.Instance.ShellCmd("netsh advfirewall firewall delete rule name=\"AirVPN - Out - AllowLocal\"");
			Platform.Instance.ShellCmd("netsh advfirewall firewall delete rule name=\"AirVPN - Out - AllowVPN\"");
			Platform.Instance.ShellCmd("netsh advfirewall firewall delete rule name=\"AirVPN - Out - AllowAirIPS\"");

			string rulesBackupSession = Storage.DataPath + Platform.Instance.DirSep + "winfirewallrules.wfw";
			if (File.Exists(rulesBackupSession))
			{
				Platform.Instance.ShellCmd("netsh advfirewall import \"" + rulesBackupSession + "\"");
				File.Delete(rulesBackupSession);
			}

			foreach (NetworkLockWindowsFirewallProfile profile in Profiles)
			{
				if (profile.State == false)
					profile.StateOff();
			}			
		}

		public override void OnUpdateIps()
		{
			if (m_activated == false)
				return;

			List<IpAddress> ipsFirewalled = GetAllIps();
			string ipList = "";
			foreach (IpAddress ip in ipsFirewalled)
			{
				if (ipList != "")
					ipList += ",";
				ipList += ip.ToString();
			}

			if (ipList != m_lastestIpList)
			{
				m_lastestIpList = ipList;

				if(m_lastestIpList != "")
					Platform.Instance.ShellCmd("netsh advfirewall firewall delete rule name=\"AirVPN - Out - AllowAirIPS\"");
				Platform.Instance.ShellCmd("netsh advfirewall firewall add rule name=\"AirVPN - Out - AllowAirIPS\" dir=out action=allow remoteip=" + ipList);
			}
		}
	}
}
