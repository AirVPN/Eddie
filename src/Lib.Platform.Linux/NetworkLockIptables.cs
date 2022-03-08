// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2019 AirVPN (support@airvpn.org) / https://airvpn.org
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
using System.Net.NetworkInformation;
using Eddie.Core;

namespace Eddie.Platform.Linux
{
	public class NetworkLockIptables : NetworkLockPlugin
	{
		private IpAddresses m_ipsAllowlistIncoming = new IpAddresses();
		private IpAddresses m_ipsAllowlistOutgoing = new IpAddresses();
		private bool m_supportIPv4 = true;
		private bool m_supportIPv6 = true;

		public virtual string GetCompatibility()
		{
			return "";
		}

		public override string GetCode()
		{
			return "linux_iptables";
		}

		public override string GetName()
		{
			return "Linux iptables";
		}

		public override bool GetSupport()
		{
			Core.Elevated.Command c = new Core.Elevated.Command();
			c.Parameters["command"] = "netlock-iptables-available";
			c.Parameters["compatibility"] = GetCompatibility();
			string result = Engine.Instance.Elevated.DoCommandSync(c);
			return (result == "1");
		}

		public override void Init()
		{
			base.Init();
		}

		public override void Activation()
		{
			base.Activation();

			m_supportIPv4 = Platform.Instance.GetSupportIPv4();
			m_supportIPv6 = Platform.Instance.GetSupportIPv6();

			if (m_supportIPv6 == false)
				Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("NetworkLockLinuxIPv6NotAvailable"));

			try
			{
				IpAddresses ipsAllowlistIncoming = GetIpsAllowlistIncoming();
				IpAddresses ipsAllowlistOutgoing = GetIpsAllowlistOutgoing(true);

				// Build rules
				var rulesIPv4 = new System.Text.StringBuilder();
				var rulesIPv6 = new System.Text.StringBuilder();

				{
					string defaultPolicyInput = "DROP";
					string defaultPolicyForward = "DROP";
					string defaultPolicyOutput = "DROP";
					if (Engine.Instance.Options.Get("netlock.incoming") == "allow")
						defaultPolicyInput = "ACCEPT";
					if (Engine.Instance.Options.Get("netlock.outgoing") == "allow")
						defaultPolicyOutput = "ACCEPT";

					// IPv4
					if (m_supportIPv4)
					{
						rulesIPv4.AppendLine("*mangle");
						rulesIPv4.AppendLine(":PREROUTING ACCEPT [0:0]");
						rulesIPv4.AppendLine(":INPUT ACCEPT [0:0]");
						rulesIPv4.AppendLine(":FORWARD ACCEPT [0:0]");
						rulesIPv4.AppendLine(":OUTPUT ACCEPT [0:0]");
						rulesIPv4.AppendLine(":POSTROUTING ACCEPT [0:0]");
						rulesIPv4.AppendLine("COMMIT");
						rulesIPv4.AppendLine("*nat");
						rulesIPv4.AppendLine(":PREROUTING ACCEPT [0:0]");
						rulesIPv4.AppendLine(":INPUT ACCEPT [0:0]");
						rulesIPv4.AppendLine(":OUTPUT ACCEPT [0:0]");
						rulesIPv4.AppendLine(":POSTROUTING ACCEPT [0:0]");
						rulesIPv4.AppendLine("COMMIT");
						rulesIPv4.AppendLine("*filter");
						rulesIPv4.AppendLine(":INPUT " + defaultPolicyInput + " [0:0]");
						rulesIPv4.AppendLine(":FORWARD " + defaultPolicyForward + " [0:0]");
						rulesIPv4.AppendLine(":OUTPUT " + defaultPolicyOutput + " [0:0]");

						// Local
						rulesIPv4.AppendLine("-A INPUT -i lo -j ACCEPT");

						if (Engine.Instance.Options.GetBool("netlock.allow_dhcp") == true)
						{
							rulesIPv4.AppendLine("-A INPUT -s 255.255.255.255/32 -j ACCEPT");
						}

						if (Engine.Instance.Options.GetBool("netlock.allow_private"))
						{
							// Private networks
							rulesIPv4.AppendLine("-A INPUT -s 192.168.0.0/16 -d 192.168.0.0/16 -j ACCEPT");
							rulesIPv4.AppendLine("-A INPUT -s 10.0.0.0/8 -d 10.0.0.0/8 -j ACCEPT");
							rulesIPv4.AppendLine("-A INPUT -s 172.16.0.0/12 -d 172.16.0.0/12 -j ACCEPT");
						}

						if (Engine.Instance.Options.GetBool("netlock.allow_ping"))
						{
							// icmp-type: echo-request
							rulesIPv4.AppendLine("-A INPUT -p icmp -m icmp --icmp-type 8 -j ACCEPT");
						}

						// Allow established sessions to receive traffic
						rulesIPv4.AppendLine("-A INPUT -m state --state RELATED,ESTABLISHED -j ACCEPT");

						// Allow TUN
						//rulesIPv4.AppendLine("-A INPUT -i tun+ -j ACCEPT");

						// Allowlist incoming
						foreach (IpAddress ip in ipsAllowlistIncoming.IPs)
						{
							if (ip.IsV4)
							{
								//body.AppendLine("-A INPUT -s " + ip.ToCIDR() + " -m state --state NEW,ESTABLISHED -j ACCEPT");
								rulesIPv4.AppendLine("-A INPUT -s " + ip.ToCIDR() + " -j ACCEPT");
							}
						}

						// Redundand, equal to default policy
						rulesIPv4.AppendLine("-A INPUT -j " + defaultPolicyInput);

						// Allow TUN
						//rulesIPv4.AppendLine("-A FORWARD -i tun+ -j ACCEPT");

						// Redundand, equal to default policy
						rulesIPv4.AppendLine("-A FORWARD -j " + defaultPolicyForward);

						// Local
						rulesIPv4.AppendLine("-A OUTPUT -o lo -j ACCEPT");

						if (Engine.Instance.Options.GetBool("netlock.allow_dhcp") == true)
						{
							// Make sure you can communicate with any DHCP server
							rulesIPv4.AppendLine("-A OUTPUT -d 255.255.255.255/32 -j ACCEPT");
						}

						if (Engine.Instance.Options.GetBool("netlock.allow_private"))
						{
							// Private networks
							rulesIPv4.AppendLine("-A OUTPUT -s 192.168.0.0/16 -d 192.168.0.0/16 -j ACCEPT");
							rulesIPv4.AppendLine("-A OUTPUT -s 10.0.0.0/8 -d 10.0.0.0/8 -j ACCEPT");
							rulesIPv4.AppendLine("-A OUTPUT -s 172.16.0.0/12 -d 172.16.0.0/12 -j ACCEPT");

							// Multicast
							rulesIPv4.AppendLine("-A OUTPUT -s 192.168.0.0/16 -d 224.0.0.0/24 -j ACCEPT");
							rulesIPv4.AppendLine("-A OUTPUT -s 10.0.0.0/8 -d 224.0.0.0/24 -j ACCEPT");
							rulesIPv4.AppendLine("-A OUTPUT -s 172.16.0.0/12 -d 224.0.0.0/24 -j ACCEPT");

							// Simple Service Discovery Protocol address
							rulesIPv4.AppendLine("-A OUTPUT -s 192.168.0.0/16 -d 239.255.255.250/32 -j ACCEPT");
							rulesIPv4.AppendLine("-A OUTPUT -s 10.0.0.0/8 -d 239.255.255.250/32 -j ACCEPT");
							rulesIPv4.AppendLine("-A OUTPUT -s 172.16.0.0/12 -d 239.255.255.250/32 -j ACCEPT");

							// Service Location Protocol version 2 address
							rulesIPv4.AppendLine("-A OUTPUT -s 192.168.0.0/16 -d 239.255.255.253/32 -j ACCEPT");
							rulesIPv4.AppendLine("-A OUTPUT -s 10.0.0.0/8 -d 239.255.255.253/32 -j ACCEPT");
							rulesIPv4.AppendLine("-A OUTPUT -s 172.16.0.0/12 -d 239.255.255.253/32 -j ACCEPT");
						}

						if (Engine.Instance.Options.GetBool("netlock.allow_ping"))
						{
							// icmp-type: echo-reply
							rulesIPv4.AppendLine("-A OUTPUT -p icmp -m icmp --icmp-type 0 -j ACCEPT");
						}

						// Allow TUN
						//rulesIPv4.AppendLine("-A OUTPUT -o tun+ -j ACCEPT"); 

						// If incoming=allow, allow packet response to out
						// We avoid a general rules, because in block/block mode don't drop already exists keepalive
						if (defaultPolicyInput == "ACCEPT")
							rulesIPv4.AppendLine("-A OUTPUT -m state --state ESTABLISHED -j ACCEPT");

						// Allowlist incoming
						foreach (IpAddress ip in ipsAllowlistIncoming.IPs)
						{
							if (ip.IsV4)
								rulesIPv4.AppendLine("-A OUTPUT -d " + ip.ToCIDR() + " -m state --state ESTABLISHED -j ACCEPT");
						}

						// Allowlist outgoing
						foreach (IpAddress ip in ipsAllowlistOutgoing.IPs)
						{
							if (ip.IsV4)
								rulesIPv4.AppendLine("-A OUTPUT -d " + ip.ToCIDR() + " -j ACCEPT");
						}

						// Redundand, equal to default policy
						rulesIPv4.AppendLine("-A OUTPUT -j " + defaultPolicyOutput);

						// Commit
						rulesIPv4.AppendLine("COMMIT");
					}

					// IPv6
					if (m_supportIPv6)
					{
						rulesIPv6.AppendLine("*mangle");
						rulesIPv6.AppendLine(":PREROUTING ACCEPT [0:0]");
						rulesIPv6.AppendLine(":INPUT ACCEPT [0:0]");
						rulesIPv6.AppendLine(":FORWARD ACCEPT [0:0]");
						rulesIPv6.AppendLine(":OUTPUT ACCEPT [0:0]");
						rulesIPv6.AppendLine(":POSTROUTING ACCEPT [0:0]");
						rulesIPv6.AppendLine("COMMIT");
						rulesIPv6.AppendLine("*nat");
						rulesIPv6.AppendLine(":PREROUTING ACCEPT [0:0]");
						rulesIPv6.AppendLine(":INPUT ACCEPT [0:0]");
						rulesIPv6.AppendLine(":OUTPUT ACCEPT [0:0]");
						rulesIPv6.AppendLine(":POSTROUTING ACCEPT [0:0]");
						rulesIPv6.AppendLine("COMMIT");
						rulesIPv6.AppendLine("*filter");
						rulesIPv6.AppendLine(":INPUT " + defaultPolicyInput + " [0:0]");
						rulesIPv6.AppendLine(":FORWARD " + defaultPolicyForward + " [0:0]");
						rulesIPv6.AppendLine(":OUTPUT " + defaultPolicyOutput + " [0:0]");

						// Local
						rulesIPv6.AppendLine("-A INPUT -i lo -j ACCEPT");

						// Reject traffic to localhost that does not originate from lo0.
						rulesIPv6.AppendLine("-A INPUT -s ::1/128 ! -i lo -j REJECT --reject-with icmp6-port-unreachable");

						// Disable processing of any RH0 packet which could allow a ping-pong of packets
						rulesIPv6.AppendLine("-A INPUT -m rt --rt-type 0 -j DROP");

						// icmpv6-type:router-advertisement - Rules which are required for your IPv6 address to be properly allocated
						rulesIPv6.AppendLine("-A INPUT -p ipv6-icmp -m icmp6 --icmpv6-type 134 -m hl --hl-eq 255 -j ACCEPT");

						// icmpv6-type:neighbor-solicitation - Rules which are required for your IPv6 address to be properly allocated
						rulesIPv6.AppendLine("-A INPUT -p ipv6-icmp -m icmp6 --icmpv6-type 135 -m hl --hl-eq 255 -j ACCEPT");

						// icmpv6-type:neighbor-advertisement - Rules which are required for your IPv6 address to be properly allocated
						rulesIPv6.AppendLine("-A INPUT -p ipv6-icmp -m icmp6 --icmpv6-type 136 -m hl --hl-eq 255 -j ACCEPT");

						// icmpv6-type:redirect - Rules which are required for your IPv6 address to be properly allocated
						rulesIPv6.AppendLine("-A INPUT -p ipv6-icmp -m icmp6 --icmpv6-type 137 -m hl --hl-eq 255 -j ACCEPT");

						if (Engine.Instance.Options.GetBool("netlock.allow_private"))
						{
							// Allow Link-Local addresses
							rulesIPv6.AppendLine("-A INPUT -s fe80::/10 -j ACCEPT");

							// Allow multicast
							rulesIPv6.AppendLine("-A INPUT -d ff00::/8 -j ACCEPT");
						}

						if (Engine.Instance.Options.GetBool("netlock.allow_ping"))
						{
							rulesIPv6.AppendLine("-A INPUT -p ipv6-icmp -j ACCEPT");
						}

						// Allow established sessions to receive traffic
						rulesIPv6.AppendLine("-A INPUT -m state --state RELATED,ESTABLISHED -j ACCEPT");

						// Allow TUN
						//rulesIPv6.AppendLine("-A INPUT -i tun+ -j ACCEPT");

						// Allowlist incoming
						foreach (IpAddress ip in ipsAllowlistIncoming.IPs)
						{
							if (ip.IsV6)
							{
								rulesIPv6.AppendLine("-A INPUT -s " + ip.ToCIDR() + " -j ACCEPT");
							}
						}

						// Redundand, equal to default policy
						rulesIPv6.AppendLine("-A INPUT -j " + defaultPolicyInput);

						// Disable processing of any RH0 packet which could allow a ping-pong of packets
						rulesIPv6.AppendLine("-A FORWARD -m rt --rt-type 0 -j DROP");

						// Allow TUN
						//rulesIPv6.AppendLine("-A FORWARD -i tun+ -j ACCEPT");

						// Redundand, equal to default policy
						rulesIPv6.AppendLine("-A FORWARD -j " + defaultPolicyForward);

						// Local
						rulesIPv6.AppendLine("-A OUTPUT -o lo -j ACCEPT");

						// Disable processing of any RH0 packet which could allow a ping-pong of packets
						rulesIPv6.AppendLine("-A OUTPUT -m rt --rt-type 0 -j DROP");

						if (Engine.Instance.Options.GetBool("netlock.allow_private"))
						{
							// Allow Link-Local addresses
							rulesIPv6.AppendLine("-A OUTPUT -s fe80::/10 -j ACCEPT");

							// Allow multicast
							rulesIPv6.AppendLine("-A OUTPUT -d ff00::/8 -j ACCEPT");
						}

						if (Engine.Instance.Options.GetBool("netlock.allow_ping"))
						{
							rulesIPv6.AppendLine("-A OUTPUT -p ipv6-icmp -j ACCEPT");
						}

						// Allow TUN
						//rulesIPv6.AppendLine("-A OUTPUT -o tun+ -j ACCEPT");

						// If incoming=allow, allow packet response to out
						// We avoid a general rules, because in block/block mode don't drop already exists keepalive
						if (defaultPolicyInput == "ACCEPT")
							rulesIPv6.AppendLine("-A OUTPUT -m state --state ESTABLISHED -j ACCEPT");

						// Allowlist incoming
						foreach (IpAddress ip in ipsAllowlistIncoming.IPs)
						{
							if (ip.IsV6)
								rulesIPv6.AppendLine("-A OUTPUT -d " + ip.ToCIDR() + " -m state --state ESTABLISHED -j ACCEPT");
						}

						// Allowlist outgoing
						foreach (IpAddress ip in ipsAllowlistOutgoing.IPs)
						{
							if (ip.IsV6)
								rulesIPv6.AppendLine("-A OUTPUT -d " + ip.ToCIDR() + " -j ACCEPT");
						}

						// Redundand, equal to default policy
						rulesIPv6.AppendLine("-A OUTPUT -j " + defaultPolicyOutput);

						// Commit
						rulesIPv6.AppendLine("COMMIT");
					}
				}

				Core.Elevated.Command c = new Core.Elevated.Command();
				c.Parameters["command"] = "netlock-iptables-activate";
				c.Parameters["compatibility"] = GetCompatibility();
				if (m_supportIPv4)
					c.Parameters["rules-ipv4"] = rulesIPv4.ToString();
				if (m_supportIPv6)
					c.Parameters["rules-ipv6"] = rulesIPv6.ToString();
				string result = Engine.Instance.Elevated.DoCommandSync(c);
				if (result != "")
					throw new Exception("Unexpected result: " + result);

				m_ipsAllowlistIncoming = ipsAllowlistIncoming;
				m_ipsAllowlistOutgoing = ipsAllowlistOutgoing;

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

			Core.Elevated.Command c = new Core.Elevated.Command();
			c.Parameters["command"] = "netlock-iptables-deactivate";
			c.Parameters["compatibility"] = GetCompatibility();
			string result = Engine.Instance.Elevated.DoCommandSync(c);
			if (result != "")
				throw new Exception("Unexpected result: " + result);

			// IPS
			m_ipsAllowlistIncoming.Clear();
			m_ipsAllowlistOutgoing.Clear();
		}

		public override void OnUpdateIps()
		{
			base.OnUpdateIps();

			IpAddresses ipsAllowlistIncoming = GetIpsAllowlistIncoming();
			IpAddresses ipsAllowlistOutgoing = GetIpsAllowlistOutgoing(true);

			// Incoming - Remove IP not present in the new list
			foreach (IpAddress ip in m_ipsAllowlistIncoming.IPs)
			{
				if (((ip.IsV4) && (m_supportIPv4 == false)) || ((ip.IsV6) && (m_supportIPv6 == false)))
					continue;

				if (ipsAllowlistIncoming.Contains(ip) == false)
				{
					Core.Elevated.Command c = new Core.Elevated.Command();
					c.Parameters["command"] = "netlock-iptables-accept-ip";
					c.Parameters["compatibility"] = GetCompatibility();
					c.Parameters["action"] = "del";
					c.Parameters["direction"] = "in";
					c.Parameters["layer"] = (ip.IsV4 ? "ipv4" : "ipv6");
					c.Parameters["cidr"] = ip.ToCIDR();
					Engine.Instance.Elevated.DoCommandSync(c);
				}
			}

			// Incoming - Add IP
			foreach (IpAddress ip in ipsAllowlistIncoming.IPs)
			{
				if (((ip.IsV4) && (m_supportIPv4 == false)) || ((ip.IsV6) && (m_supportIPv6 == false)))
					continue;

				if (m_ipsAllowlistIncoming.Contains(ip) == false)
				{
					Core.Elevated.Command c = new Core.Elevated.Command();
					c.Parameters["command"] = "netlock-iptables-accept-ip";
					c.Parameters["compatibility"] = GetCompatibility();
					c.Parameters["action"] = "add";
					c.Parameters["direction"] = "in";
					c.Parameters["layer"] = (ip.IsV4 ? "ipv4" : "ipv6");
					c.Parameters["cidr"] = ip.ToCIDR();
					Engine.Instance.Elevated.DoCommandSync(c);
				}
			}

			// Outgoing - Remove IP not present in the new list
			foreach (IpAddress ip in m_ipsAllowlistOutgoing.IPs)
			{
				if (((ip.IsV4) && (m_supportIPv4 == false)) || ((ip.IsV6) && (m_supportIPv6 == false)))
					continue;

				if (ipsAllowlistOutgoing.Contains(ip) == false)
				{
					Core.Elevated.Command c = new Core.Elevated.Command();
					c.Parameters["command"] = "netlock-iptables-accept-ip";
					c.Parameters["compatibility"] = GetCompatibility();
					c.Parameters["action"] = "del";
					c.Parameters["direction"] = "out";
					c.Parameters["layer"] = (ip.IsV4 ? "ipv4" : "ipv6");
					c.Parameters["cidr"] = ip.ToCIDR();
					Engine.Instance.Elevated.DoCommandSync(c);
				}
			}

			// Outgoing - Add IP
			foreach (IpAddress ip in ipsAllowlistOutgoing.IPs)
			{
				if (((ip.IsV4) && (m_supportIPv4 == false)) || ((ip.IsV6) && (m_supportIPv6 == false)))
					continue;

				if (m_ipsAllowlistOutgoing.Contains(ip) == false)
				{
					Core.Elevated.Command c = new Core.Elevated.Command();
					c.Parameters["command"] = "netlock-iptables-accept-ip";
					c.Parameters["compatibility"] = GetCompatibility();
					c.Parameters["action"] = "add";
					c.Parameters["direction"] = "out";
					c.Parameters["layer"] = (ip.IsV4 ? "ipv4" : "ipv6");
					c.Parameters["cidr"] = ip.ToCIDR();
					Engine.Instance.Elevated.DoCommandSync(c);
				}
			}

			m_ipsAllowlistIncoming = ipsAllowlistIncoming;
			m_ipsAllowlistOutgoing = ipsAllowlistOutgoing;
		}

		public override void AllowInterface(NetworkInterface networkInterface)
		{
			base.AllowInterface(networkInterface);

			Core.Elevated.Command c = new Core.Elevated.Command();
			c.Parameters["command"] = "netlock-iptables-interface";
			c.Parameters["compatibility"] = GetCompatibility();
			c.Parameters["action"] = "add";
			c.Parameters["id"] = networkInterface.Id;
			c.Parameters["ipv4"] = m_supportIPv4 ? "1" : "0";
			c.Parameters["ipv6"] = m_supportIPv6 ? "1" : "0";
			Engine.Instance.Elevated.DoCommandSync(c);
		}

		public override void DeallowInterface(NetworkInterface networkInterface)
		{
			base.DeallowInterface(networkInterface);

			Core.Elevated.Command c = new Core.Elevated.Command();
			c.Parameters["command"] = "netlock-iptables-interface";
			c.Parameters["compatibility"] = GetCompatibility();
			c.Parameters["action"] = "del";
			c.Parameters["id"] = networkInterface.Id;
			c.Parameters["ipv4"] = m_supportIPv4 ? "1" : "0";
			c.Parameters["ipv6"] = m_supportIPv6 ? "1" : "0";
			Engine.Instance.Elevated.DoCommandSync(c);
		}
	}
}
