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

namespace Eddie.Platforms.MacOS
{
	public class NetworkLockOsxPf : NetworkLockPlugin
	{
		private TemporaryFile m_filePfConf;

		private bool m_prevActive = false;

		private bool m_connected = false;

		public override string GetCode()
		{
			return "osx_pf";
		}

		public override string GetName()
		{
			return "OS X - PF";
		}

		public override bool GetSupport()
		{
			if (Platform.Instance.FileExists("/etc/pf.conf") == false)
				return false;

			return true;
		}

		public override void Activation()
		{
			base.Activation();

			m_prevActive = false;
            string report = SystemShell.ShellCmd("pfctl -si");
			if (report.IndexOf ("denied") != -1)
				throw new Exception("Permission denied.");
			else if (report.IndexOf ("Status: Enabled") != -1)
				m_prevActive = true;
			else if (report.IndexOf("Status: Disabled") != -1)
				m_prevActive = false;
			else
				throw new Exception("Unexpected PF Firewall status");

			if (m_prevActive == false) {
				string reportActivation = SystemShell.ShellCmd("pfctl -e");
				if (reportActivation.IndexOf ("pf enabled") == -1)
					throw new Exception ("Unexpected PF Firewall activation failure");
			}
			m_filePfConf = new TemporaryFile("pf.conf");

			OnUpdateIps();

			if (m_prevActive == false)
			{
				SystemShell.ShellCmd("pfctl -e");
			}
		}

		public override void Deactivation()
		{
			base.Deactivation();

			// Restore system rules
			SystemShell.ShellCmd("pfctl -v -f \"/etc/pf.conf\"");

			if (m_filePfConf != null)
			{
				m_filePfConf.Close();
				m_filePfConf = null;
			}

			if (m_prevActive)
			{
			}
			else
			{
				SystemShell.ShellCmd("pfctl -d");
			}
		}

		public override void AllowIP(IpAddress ip)
		{
			base.AllowIP(ip);
		}

		public override void DeallowIP(IpAddress ip)
		{
			base.DeallowIP(ip);
		}

		public override void OnUpdateIps()
		{
			base.OnUpdateIps();

			// Remember: Rules must be in order: options, normalization, queueing, translation, filtering

			string pf = "";
			pf += "# " + Engine.Instance.GenerateFileHeader() + "\n";

			pf += "# Block policy, RST for quickly notice\n";
			pf += "set block-policy return\n"; // 2.9

			pf += "# Skip interfaces: lo0 and utun (only when connected)\n"; // 2.9
			if (m_connected)
			{
				pf += "set skip on { lo0 " + Engine.Instance.ConnectedVpnInterfaceId + " }\n";				
			}
			else
			{
				pf += "set skip on { lo0 }\n";				
			}

			pf += "# Scrub\n";
			pf += "scrub in all\n"; // 2.9

			pf += "# Drop everything that doesn't match a rule\n";			
			pf += "block drop out all\n"; 
			
			if (Engine.Instance.Storage.GetBool("netlock.allow_private"))
			{
				pf += "# Private networks\n";
				pf += "pass out quick inet from 192.168.0.0/16 to 192.168.0.0/16 flags S/SA keep state\n";
				pf += "pass in quick inet from 192.168.0.0/16 to 192.168.0.0/16 flags S/SA keep state\n";
				pf += "pass out quick inet from 172.16.0.0/12 to 172.16.0.0/12 flags S/SA keep state\n";
				pf += "pass in quick inet from 172.16.0.0/12 to 172.16.0.0/12 flags S/SA keep state\n";
				pf += "pass out quick inet from 10.0.0.0/8 to 10.0.0.0/8 flags S/SA keep state\n";
				pf += "pass in quick inet from 10.0.0.0/8 to 10.0.0.0/8 flags S/SA keep state\n";

                // Multicast
                pf += "pass out quick inet from 192.168.0.0/16 to 224.0.0.0/24\n";
                pf += "pass out quick inet from 172.16.0.0/12 to 224.0.0.0/24\n";
                pf += "pass out quick inet from 10.0.0.0/8 to 224.0.0.0/24\n";

                // 239.255.255.250  Simple Service Discovery Protocol address
                pf += "pass out quick inet from 192.168.0.0/16 to 239.255.255.250/32\n";
                pf += "pass out quick inet from 172.16.0.0/12 to 239.255.255.250/32\n";
                pf += "pass out quick inet from 10.0.0.0/8 to 239.255.255.250/32\n";

                // 239.255.255.253  Service Location Protocol version 2 address
                pf += "pass out quick inet from 192.168.0.0/16 to 239.255.255.253/32\n";
                pf += "pass out quick inet from 172.16.0.0/12 to 239.255.255.253/32\n";
                pf += "pass out quick inet from 10.0.0.0/8 to 239.255.255.253/32\n";

                // TOFIX: IPv6 missing
            }

            if (Engine.Instance.Storage.GetBool("netlock.allow_ping"))
			{
				pf += "# Allow ICMP\n";
				pf += "pass quick proto icmp\n"; // 2.9
                pf += "pass in quick proto icmp6 all\n"; // 2.13.2
			}

            IpAddresses ips = GetAllIps(true);
			pf += "# AirVPN IP (Auth and VPN)\n";
            foreach (IpAddress ip in ips.IPs)
			{
                if(ip.IsV4)
                    pf += "pass out quick inet from any to " + ip.ToCIDR() + " flags S/SA keep state\n";
                else if(ip.IsV6)
                    pf += "pass out quick inet6 from any to " + ip.ToCIDR() + " flags S/SA keep state\n";
			}
			
			if (Platform.Instance.FileContentsWriteText(m_filePfConf.Path, pf))
			{
				Engine.Instance.Logs.Log(LogType.Verbose, "OS X - PF rules updated, reloading");

				string result = SystemShell.ShellCmd("pfctl -v -f \"" + SystemShell.EscapePath(m_filePfConf.Path) + "\"");
                if (result.IndexOf("rules not loaded", StringComparison.InvariantCulture) != -1)
                    throw new Exception(Messages.NetworkLockMacOSUnableToStart);
            }
		}

		public override void OnVpnEstablished()
		{
			base.OnVpnEstablished();

			m_connected = true;
			OnUpdateIps();
		}

		public override void OnVpnDisconnected()
		{
			base.OnVpnDisconnected();

			m_connected = false;
			OnUpdateIps();
		}

		public override void OnRecoveryLoad(XmlElement root)
		{
			base.OnRecoveryLoad(root);

			m_prevActive = (root.GetAttribute("prev_active") == "1");			
		}

		public override void OnRecoverySave(XmlElement root)
		{
			base.OnRecoverySave(root);

			root.SetAttribute("prev_active", m_prevActive ? "1" : "0");
		}
	}
}
