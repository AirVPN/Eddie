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

namespace Eddie.Platform.MacOS
{
	public class NetworkLockOsxPf : NetworkLockPlugin
	{
		private bool m_prevActive = false;
		private bool m_connected = false;
		private string m_pfctlPath = "";

		public override string GetCode()
		{
			return "osx_pf";
		}

		public override string GetName()
		{
			return "macOS - PF";
		}

		public override bool GetSupport()
		{
			if (Platform.Instance.LocateExecutable("pfctl") == "")
				return false;
			if (Platform.Instance.FileExists("/etc/pf.conf") == false)
				return false;

			return true;
		}

		public override void Init()
		{
			base.Init();

			m_pfctlPath = Platform.Instance.LocateExecutable("pfctl");
		}

		public override void Activation()
		{
			base.Activation();

            try
			{
                string result = Engine.Instance.Elevated.DoCommandSync("netlock-pf-activate");
                if (result == "activated")
                    m_prevActive = false;
                else if (result == "active")
                    m_prevActive = true;
                else
                    throw new Exception("Unexpected PF Firewall activation");

                OnUpdateIps();                
			}
			catch (Exception ex)
			{
				Deactivation();
				throw new Exception(ex.Message);
			}

		}

		public override void Deactivation()
		{
			base.Deactivation();

            string result = Engine.Instance.Elevated.DoCommandSync("netlock-pf-deactivate", "prev", (m_prevActive ? "enabled":"disabled"));
            if (result != "")
                throw new Exception("Unexpected result: " + result);            
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

            string pfConfig = BuildPfConfig();

            // Engine.Instance.Logs.Log(LogType.Verbose, "macOS - PF rules updated, reloading");
            string result = Engine.Instance.Elevated.DoCommandSync("netlock-pf-update","config", pfConfig);
            if (result != "")
                throw new Exception("Unexpected result: " + result);
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

        public string BuildPfConfig()
        {
            // Remember: Rules must be in order: options, normalization, queueing, translation, filtering

            string pf = "";
            pf += "# " + Engine.Instance.GenerateFileHeader() + "\n";

            pf += "# Block policy, RST for quickly notice\n";
            pf += "set block-policy return\n"; // 2.9 

            pf += "# Skip interfaces: lo0 and utun (only when connected)\n"; // 2.9
            if (m_connected)
            {
                pf += "set skip on { lo0 " + Engine.Instance.ConnectionActive.Interface.Id + " }\n";
            }
            else
            {
                pf += "set skip on { lo0 }\n";
            }

            pf += "# Scrub\n";
            pf += "scrub in all\n"; // 2.9

            pf += "# General rule\n";
            if (Engine.Instance.Storage.Get("netlock.incoming") == "allow")
                pf += "pass in all\n";
            else
                pf += "block in all\n";
            if (Engine.Instance.Storage.Get("netlock.outgoing") == "allow")
                pf += "pass out all\n";
            else
                pf += "block out all\n";

            if (Engine.Instance.Storage.GetBool("netlock.allow_private"))
            {
                pf += "# IPv4 - Private networks\n";
                pf += "pass out quick inet from 192.168.0.0/16 to 192.168.0.0/16\n";
                pf += "pass in quick inet from 192.168.0.0/16 to 192.168.0.0/16\n";
                pf += "pass out quick inet from 172.16.0.0/12 to 172.16.0.0/12\n";
                pf += "pass in quick inet from 172.16.0.0/12 to 172.16.0.0/12\n";
                pf += "pass out quick inet from 10.0.0.0/8 to 10.0.0.0/8\n";
                pf += "pass in quick inet from 10.0.0.0/8 to 10.0.0.0/8\n";

                pf += "# IPv4 - Multicast\n";
                pf += "pass out quick inet from 192.168.0.0/16 to 224.0.0.0/24\n";
                pf += "pass out quick inet from 172.16.0.0/12 to 224.0.0.0/24\n";
                pf += "pass out quick inet from 10.0.0.0/8 to 224.0.0.0/24\n";

                pf += "# IPv4 - Simple Service Discovery Protocol address\n";
                pf += "pass out quick inet from 192.168.0.0/16 to 239.255.255.250/32\n";
                pf += "pass out quick inet from 172.16.0.0/12 to 239.255.255.250/32\n";
                pf += "pass out quick inet from 10.0.0.0/8 to 239.255.255.250/32\n";

                pf += "# IPv4 - Service Location Protocol version 2 address\n";
                pf += "pass out quick inet from 192.168.0.0/16 to 239.255.255.253/32\n";
                pf += "pass out quick inet from 172.16.0.0/12 to 239.255.255.253/32\n";
                pf += "pass out quick inet from 10.0.0.0/8 to 239.255.255.253/32\n";

                pf += "# IPv6 - Allow Link-Local addresses\n";
                pf += "pass out quick inet6 from fe80::/10 to fe80::/10\n";
                pf += "pass in quick inet6 from fe80::/10 to fe80::/10\n";

                pf += "# IPv6 - Allow Link-Local addresses\n";
                pf += "pass out quick inet6 from ff00::/8 to ff00::/8\n";
                pf += "pass in quick inet6 from ff00::/8 to ff00::/8\n";
            }

            if (Engine.Instance.Storage.GetBool("netlock.allow_ping"))
            {
                pf += "# Allow ICMP\n";
                pf += "pass quick proto icmp\n"; // 2.9

                // Old macOS throw "unknown protocol icmp6". We don't known from when, so use icmp6 if High Sierra and above.
                if (Platform.Instance.GetVersion().VersionAboveOrEqual("10.13"))
                    pf += "pass quick proto icmp6 all\n"; // 2.14.0
            }

            IpAddresses ipsWhiteListIncoming = GetIpsWhiteListIncoming();
            pf += "# Specific ranges - incoming\n";
            foreach (IpAddress ip in ipsWhiteListIncoming.IPs)
            {
                if (ip.IsV4)
                    pf += "pass in quick inet from " + ip.ToCIDR() + " to any\n";
                else if (ip.IsV6)
                    pf += "pass in quick inet6 from " + ip.ToCIDR() + " to any\n";
            }

            IpAddresses ipsWhiteListOutgoing = GetIpsWhiteListOutgoing(true);
            pf += "# Specific ranges - outgoing\n";
            foreach (IpAddress ip in ipsWhiteListOutgoing.IPs)
            {
                if (ip.IsV4)
                    pf += "pass out quick inet from any to " + ip.ToCIDR() + "\n";
                else if (ip.IsV6)
                    pf += "pass out quick inet6 from any to " + ip.ToCIDR() + "\n";
            }

            return pf;
        }
	}
}
