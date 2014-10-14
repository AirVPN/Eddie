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
			if (File.Exists("/etc/pf.conf") == false)
				return false;

			return true;
		}

		public override void Activation()
		{
			base.Activation();

			m_prevActive = false;
			string report = Exec("pfctl -si");
			if (report.IndexOf ("Status: Enabled") != -1)
				m_prevActive = true;
			else if (report.IndexOf("Status: Disabled") != -1)
				m_prevActive = false;
			else
				throw new Exception("Unexpected PF Firewall status");

			if (m_prevActive == false) {
				string reportActivation = Exec ("pfctl -e");
				if (reportActivation.IndexOf ("pf enabled") == -1)
					throw new Exception ("Unexpected PF Firewall activation failure");
			}
			m_filePfConf = new TemporaryFile("pf.conf");

			OnUpdateIps();

			if (m_prevActive == false)
			{
				Exec("pfctl -e");
			}
		}

		public override void Deactivation()
		{
			base.Deactivation();

			// Restore system rules
			Exec("pfctl -v -f \"/etc/pf.conf\"");

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
				Exec("pfctl -d");
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

			string pf = "";
			pf += Messages.Format(Messages.GeneratedFileHeader, Storage.GetVersionDesc()) + "\n";
			pf += "# Drop everything that doesn't match a rule\n";
			//pf += "block drop out inet from 192.168.0.0/16 to any\n";
			pf += "block drop out inet from any to any\n";

			List<IpAddressRange> ips = GetAllIps();
			pf += "# AirVPN IP (Auth and VPN)\n";
			foreach (IpAddressRange ip in ips)
			{
				//pf += "pass out quick inet from 192.168.0.0/16 to " + ip.ToString() + " flags S/SA keep state\n";
				pf += "pass out quick inet from any to " + ip.ToCIDR() + " flags S/SA keep state\n";
			}
			pf += "# Private networks\n";
			pf += "pass out quick inet from 192.168.0.0/16 to 192.168.0.0/16 flags S/SA keep state\n";
			pf += "pass out quick inet from 172.16.0.0/12 to 172.16.0.0/12 flags S/SA keep state\n";
			pf += "pass out quick inet from 10.0.0.0/8 to 10.0.0.0/8 flags S/SA keep state\n";
			pf += "# Allow all on lo0\n";			
			//pf += "pass out quick inet from 127.0.0.1/8 to any flags S/SA keep state\n";
			pf += "pass quick on lo0 all\n";
			pf += "# Everything tunneled\n";			
			if (m_connected)
			{
				string ifn = Engine.Instance.ConnectedVpnInterfaceId;
				pf += "pass out quick on " + ifn + " inet from 10.0.0.0/8 to any flags S/SA keep state\n";
				pf += "pass quick on " + ifn + " inet from any to 10.0.0.0/8 flags S/SA keep state\n";
			}
			/*
			pf += "pass out quick inet on tun+ from 10.4.0.0/16 to any flags S/SA keep state\n";
			pf += "pass out quick inet on tun+ from 10.5.0.0/16 to any flags S/SA keep state\n";
			pf += "pass out quick inet on tun+ from 10.6.0.0/16 to any flags S/SA keep state\n";
			pf += "pass out quick inet on tun+ from 10.7.0.0/16 to any flags S/SA keep state\n";
			pf += "pass out quick inet on tun+ from 10.8.0.0/16 to any flags S/SA keep state\n";
			pf += "pass out quick inet on tun+ from 10.9.0.0/16 to any flags S/SA keep state\n";
			pf += "pass out quick inet on tun+ from 10.30.0.0/16 to any flags S/SA keep state\n";
			pf += "pass out quick inet on tun+ from 10.35.0.0/16 to any flags S/SA keep state\n";
			*/

			if (Utils.SaveFile(m_filePfConf.Path, pf))
			{
				Engine.Instance.Log(Engine.LogType.Verbose, "OS X - PF rules updated, reloading");

				Exec("pfctl -v -f \"" + m_filePfConf.Path + "\"");
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
