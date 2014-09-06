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
using System.Xml;
using AirVPN.Core;

namespace AirVPN.Platforms
{
	public class NetworkLockOsxPf : NetworkLockPlugin
	{
		private TemporaryFile m_filePfConf;

		private bool m_prevActive = false;

		public override string GetCode()
		{
			return "osx_pf";
		}

		public override string GetName()
		{
			return "OSX PF";
		}

		public override void Activation()
		{
			base.Activation();

			m_prevActive = false;
			string report = Platform.Instance.ShellCmd("pfctl -si");
			if (report.IndexOf("Status: Enabled") != -1)
				m_prevActive = true;
			else if (report.IndexOf("Status: Disabled") != -1)
				m_prevActive = false;
			else
				throw new Exception("Unexpected PF Firewall status");

			string reportActivation = Platform.Instance.ShellCmd("pfctl -e");
			if(reportActivation.IndexOf("pf enabled") == -1)
				throw new Exception("Unexpected PF Firewall activation failure");

			m_filePfConf = new TemporaryFile("pf.conf");

			OnUpdateIps();

			if (m_prevActive == false)
			{
				Platform.Instance.ShellCmd("pfctl -e");
			}
		}

		public override void Deactivation()
		{
			base.Deactivation();

			// Restore system rules
			Platform.Instance.ShellCmd("sudo pfctl -v -f \"/etc/pf.conf\"");

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
				Platform.Instance.ShellCmd("pfctl -d");
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
			pf += "block drop out inet from 192.168.0.0/16 to any\n";

			List<IpAddress> ips = GetAllIps();
			pf += "# AirVPN IP (Auth and VPN)\n";
			foreach (IpAddress ip in ips)
			{
				pf += "pass out quick inet from 192.168.0.0/16 to " + ip.ToString() + " flags S/SA keep state\n";
			}
			pf += "# Local network\n";
			pf += "pass out quick inet from 192.168.0.0/16 to 192.168.0.0/16 flags S/SA keep state\n";
			pf += "# Allow all on lo0\n";
			pf += "pass out quick inet from 127.0.0.1/8 to any flags S/SA keep state\n";
			pf += "# Everything tunneled\n";
			pf += "pass out quick inet from 10.0.0.0/8 to any flags S/SA keep state\n";

			if (Utils.SaveFile(m_filePfConf.Path, pf))
			{
				Engine.Instance.Log(Engine.LogType.Verbose, "PF rules updated, reloading");

				Platform.Instance.ShellCmd("sudo pfctl -v -f \"" + m_filePfConf.Path + "\"");
			}
		}

		public override void OnRecoveryLoad(XmlElement root)
		{
			base.OnRecoveryLoad(root);

			XmlElement node = Utils.XmlGetFirstElementByTagName(root, "osx_pf");
			if (node != null)
			{
				m_prevActive = (node.GetAttribute("prev_active") == "1");
			}
		}

		public override void OnRecoverySave(XmlElement root)
		{
			base.OnRecoverySave(root);

			XmlDocument doc = root.OwnerDocument;
			XmlElement el = (XmlElement)root.AppendChild(doc.CreateElement("osx_pf"));

			el.SetAttribute("prev_active", m_prevActive ? "1" : "0");
		}
	}
}
