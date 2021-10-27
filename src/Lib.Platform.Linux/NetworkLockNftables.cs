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
	public class NetworkLockNftables : NetworkLockPlugin
	{
		private IpAddresses m_ipsAllowlistIncoming = new IpAddresses();
		private IpAddresses m_ipsAllowlistOutgoing = new IpAddresses();
		private bool m_supportIPv4 = true;
		private bool m_supportIPv6 = true;

		public override string GetCode()
		{
			return "linux_nftables";
		}

		public override string GetName()
		{
			return "Linux nftables";
		}

		public override bool GetSupport()
		{
			string result = Engine.Instance.Elevated.DoCommandSync("netlock-nftables-available");
			return (result == "1");
		}

		public override void Init()
		{
			base.Init();
		}

		public void AddRule(System.Text.StringBuilder rules, string layer, string rule)
		{
			if ((layer == "ipv4") && (m_supportIPv4 == false))
				return;
			if ((layer == "ipv6") && (m_supportIPv6 == false))
				return;

			rules.AppendLine(rule);
		}

		public override void Activation()
		{
			base.Activation();

			m_supportIPv4 = true; // IPv4 assumed, if not available, will throw a fatal exception.
			m_supportIPv6 = Conversions.ToBool(Engine.Instance.Manifest["network_info"]["support_ipv6"].Value);

			if (m_supportIPv6 == false)
				Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("NetworkLockLinuxIPv6NotAvailable"));

			try
			{
				IpAddresses ipsAllowlistIncoming = GetIpsAllowlistIncoming();
				IpAddresses ipsAllowlistOutgoing = GetIpsAllowlistOutgoing(true);

				string defaultPolicyInput = "drop";
				string defaultPolicyForward = "drop";
				string defaultPolicyOutput = "drop";
				if (Engine.Instance.Options.Get("netlock.incoming") == "allow")
					defaultPolicyInput = "accept";
				if (Engine.Instance.Options.Get("netlock.outgoing") == "allow")
					defaultPolicyOutput = "accept";

				// Build rules
				var rules = new System.Text.StringBuilder();

				AddRule(rules, "", "flush ruleset");
				AddRule(rules, "ipv4", "add table ip nat");
				AddRule(rules, "ipv6", "add table ip6 nat");
				AddRule(rules, "ipv4", "add chain ip nat PREROUTING { type nat hook prerouting priority -100; policy accept; }");
				AddRule(rules, "ipv6", "add chain ip6 nat PREROUTING { type nat hook prerouting priority -100; policy accept; }");
				AddRule(rules, "ipv4", "add chain ip nat INPUT { type nat hook input priority 100; policy accept; }");
				AddRule(rules, "ipv6", "add chain ip6 nat INPUT { type nat hook input priority 100; policy accept; }");
				AddRule(rules, "ipv4", "add chain ip nat OUTPUT { type nat hook output priority -100; policy accept; }");
				AddRule(rules, "ipv6", "add chain ip6 nat OUTPUT { type nat hook output priority -100; policy accept; }");
				AddRule(rules, "ipv4", "add chain ip nat POSTROUTING { type nat hook postrouting priority 100; policy accept; }");
				AddRule(rules, "ipv6", "add chain ip6 nat POSTROUTING { type nat hook postrouting priority 100; policy accept; }");

				AddRule(rules, "ipv4", "add table ip mangle");
				AddRule(rules, "ipv6", "add table ip6 mangle");
				AddRule(rules, "ipv4", "add chain ip mangle PREROUTING { type filter hook prerouting priority -150; policy accept; }");
				AddRule(rules, "ipv6", "add chain ip6 mangle PREROUTING { type filter hook prerouting priority -150; policy accept; }");
				AddRule(rules, "ipv4", "add chain ip mangle INPUT { type filter hook input priority -150; policy accept; }");
				AddRule(rules, "ipv6", "add chain ip6 mangle INPUT { type filter hook input priority -150; policy accept; }");
				AddRule(rules, "ipv4", "add chain ip mangle FORWARD { type filter hook forward priority -150; policy accept; }");
				AddRule(rules, "ipv6", "add chain ip6 mangle FORWARD { type filter hook forward priority -150; policy accept; }");
				AddRule(rules, "ipv4", "add chain ip mangle OUTPUT { type route hook output priority - 150; policy accept; }");
				AddRule(rules, "ipv6", "add chain ip6 mangle OUTPUT { type route hook output priority -150; policy accept; }");
				AddRule(rules, "ipv4", "add chain ip mangle POSTROUTING { type filter hook postrouting priority -150; policy accept; }");
				AddRule(rules, "ipv6", "add chain ip6 mangle POSTROUTING { type filter hook postrouting priority -150; policy accept; }");

				AddRule(rules, "ipv4", "add table ip filter");
				AddRule(rules, "ipv6", "add table ip6 filter");
				AddRule(rules, "ipv4", "add chain ip filter INPUT { type filter hook input priority 0; policy " + defaultPolicyInput + "; }");
				AddRule(rules, "ipv6", "add chain ip6 filter INPUT { type filter hook input priority 0; policy " + defaultPolicyInput + "; }");
				AddRule(rules, "ipv4", "add chain ip filter FORWARD { type filter hook forward priority 0; policy " + defaultPolicyForward + "; }");
				AddRule(rules, "ipv6", "add chain ip6 filter FORWARD { type filter hook forward priority 0; policy " + defaultPolicyForward + "; }");
				AddRule(rules, "ipv4", "add chain ip filter OUTPUT { type filter hook output priority 0; policy " + defaultPolicyOutput + "; }");
				AddRule(rules, "ipv6", "add chain ip6 filter OUTPUT { type filter hook output priority 0; policy " + defaultPolicyOutput + "; }");

				// Input - Local
				AddRule(rules, "ipv4", "add rule ip filter INPUT iifname \"lo\" counter accept");
				AddRule(rules, "ipv6", "add rule ip6 filter INPUT iifname \"lo\" counter accept");

				// Input - Reject traffic to localhost that does not originate from lo0.
				AddRule(rules, "ipv6", "add rule ip6 filter INPUT iifname != \"lo\" ip6 saddr ::1 counter reject");

				if (Engine.Instance.Options.GetBool("netlock.allow_dhcp") == true)
				{
					AddRule(rules, "ipv4", "add rule ip filter INPUT ip saddr 255.255.255.255 counter accept");
				}

				if (Engine.Instance.Options.GetBool("netlock.allow_private"))
				{
					AddRule(rules, "ipv4", "add rule ip filter INPUT ip saddr 192.168.0.0/16 ip daddr 192.168.0.0/16 counter accept");
					AddRule(rules, "ipv4", "add rule ip filter INPUT ip saddr 10.0.0.0/8 ip daddr 10.0.0.0/8 counter accept");
					AddRule(rules, "ipv4", "add rule ip filter INPUT ip saddr 172.16.0.0/12 ip daddr 172.16.0.0/12 counter accept");

					AddRule(rules, "ipv6", "add rule ip6 filter INPUT ip6 saddr fe80::/10 counter accept");
					AddRule(rules, "ipv6", "add rule ip6 filter INPUT ip6 daddr ff00::/8 counter accept");
				}

				if (Engine.Instance.Options.GetBool("netlock.allow_ping"))
				{
					AddRule(rules, "ipv4", "add rule ip filter INPUT icmp type echo-request counter accept");

					AddRule(rules, "ipv6", "add rule ip6 filter INPUT meta l4proto ipv6-icmp counter accept");
				}

				// Input - Disable processing of any RH0 packet which could allow a ping-pong of packets
				AddRule(rules, "ipv6", "add rule ip6 filter INPUT rt type 0 counter drop");

				// Input - icmpv6-type:router-advertisement - Rules which are required for your IPv6 address to be properly allocated
				AddRule(rules, "ipv6", "add rule ip6 filter INPUT meta l4proto ipv6-icmp icmpv6 type nd-router-advert ip6 hoplimit 255 counter accept");

				// Input - icmpv6-type:neighbor-solicitation - Rules which are required for your IPv6 address to be properly allocated
				AddRule(rules, "ipv6", "add rule ip6 filter INPUT meta l4proto ipv6-icmp icmpv6 type nd-neighbor-solicit ip6 hoplimit 255 counter accept");

				// Input - icmpv6-type:neighbor-advertisement - Rules which are required for your IPv6 address to be properly allocated
				AddRule(rules, "ipv6", "add rule ip6 filter INPUT meta l4proto ipv6-icmp icmpv6 type nd-neighbor-advert ip6 hoplimit 255 counter accept");

				// Input - icmpv6-type:redirect - Rules which are required for your IPv6 address to be properly allocated
				AddRule(rules, "ipv6", "add rule ip6 filter INPUT meta l4proto ipv6-icmp icmpv6 type nd-redirect ip6 hoplimit 255 counter accept");

				// Input - Allow established sessions to receive traffic
				AddRule(rules, "ipv4", "add rule ip filter INPUT ct state related,established  counter accept");
				AddRule(rules, "ipv6", "add rule ip6 filter INPUT ct state related,established  counter accept");

				// Input - Allow TUN
				//AddRule(rules, "ipv4", "add rule ip filter INPUT iifname \"tun*\" counter accept");
				//AddRule(rules, "ipv6", "add rule ip6 filter INPUT iifname \"tun*\" counter accept");

				// Input - Allowlist incoming
				foreach (IpAddress ip in ipsAllowlistIncoming.IPs)
				{
					if (ip.IsV4)
						AddRule(rules, "ipv4", "add rule ip filter INPUT ip saddr " + ip.ToCIDR() + " counter accept");

					if (ip.IsV6)
						AddRule(rules, "ipv6", "add rule ip6 filter INPUT ip6 saddr " + ip.ToCIDR() + " counter accept");
				}

				// Input - Redundand, equal to default policy
				AddRule(rules, "ipv4", "add rule ip filter INPUT counter " + defaultPolicyInput + " comment \"eddie_ip_filter_INPUT_latest_rule\"");
				AddRule(rules, "ipv6", "add rule ip6 filter INPUT counter " + defaultPolicyInput + " comment \"eddie_ip6_filter_INPUT_latest_rule\"");

				// Forward - Disable processing of any RH0 packet which could allow a ping-pong of packets
				AddRule(rules, "ipv6", "add rule ip6 filter FORWARD rt type 0 counter drop");

				// Forward - Allow TUN
				//AddRule(rules, "ipv4", "add rule ip filter FORWARD iifname \"tun*\" counter accept");
				//AddRule(rules, "ipv6", "add rule ip6 filter FORWARD iifname \"tun*\" counter accept");

				// Forward - Redundand, equal to default policy
				AddRule(rules, "ipv4", "add rule ip filter FORWARD counter " + defaultPolicyForward + " comment \"eddie_ip_filter_FORWARD_latest_rule\"");
				AddRule(rules, "ipv6", "add rule ip6 filter FORWARD counter " + defaultPolicyForward + " comment \"eddie_ip6_filter_FORWARD_latest_rule\"");

				// Output - Local
				AddRule(rules, "ipv4", "add rule ip filter OUTPUT oifname \"lo\" counter accept");
				AddRule(rules, "ipv6", "add rule ip6 filter OUTPUT oifname \"lo\" counter accept");

				// Output - Disable processing of any RH0 packet which could allow a ping-pong of packets
				AddRule(rules, "ipv6", "add rule ip6 filter OUTPUT rt type 0 counter drop");

				if (Engine.Instance.Options.GetBool("netlock.allow_dhcp") == true)
				{
					AddRule(rules, "ipv4", "add rule ip filter OUTPUT ip daddr 255.255.255.255 counter accept");
				}

				if (Engine.Instance.Options.GetBool("netlock.allow_private"))
				{
					// Private networks
					AddRule(rules, "ipv4", "add rule ip filter OUTPUT ip saddr 192.168.0.0/16 ip daddr 192.168.0.0/16 counter accept");
					AddRule(rules, "ipv4", "add rule ip filter OUTPUT ip saddr 10.0.0.0/8 ip daddr 10.0.0.0/8 counter accept");
					AddRule(rules, "ipv4", "add rule ip filter OUTPUT ip saddr 172.16.0.0/12 ip daddr 172.16.0.0/12 counter accept");

					// Multicast
					AddRule(rules, "ipv4", "add rule ip filter OUTPUT ip saddr 192.168.0.0/16 ip daddr 224.0.0.0/24 counter accept");
					AddRule(rules, "ipv4", "add rule ip filter OUTPUT ip saddr 10.0.0.0/8 ip daddr 224.0.0.0/24 counter accept");
					AddRule(rules, "ipv4", "add rule ip filter OUTPUT ip saddr 172.16.0.0/12 ip daddr 224.0.0.0/24 counter accept");

					// Simple Service Discovery Protocol address
					AddRule(rules, "ipv4", "add rule ip filter OUTPUT ip saddr 192.168.0.0/16 ip daddr 239.255.255.250 counter accept");
					AddRule(rules, "ipv4", "add rule ip filter OUTPUT ip saddr 10.0.0.0/8 ip daddr 239.255.255.250 counter accept");
					AddRule(rules, "ipv4", "add rule ip filter OUTPUT ip saddr 172.16.0.0/12 ip daddr 239.255.255.250 counter accept");

					// Service Location Protocol version 2 address
					AddRule(rules, "ipv4", "add rule ip filter OUTPUT ip saddr 192.168.0.0/16 ip daddr 239.255.255.253 counter accept");
					AddRule(rules, "ipv4", "add rule ip filter OUTPUT ip saddr 10.0.0.0/8 ip daddr 239.255.255.253 counter accept");
					AddRule(rules, "ipv4", "add rule ip filter OUTPUT ip saddr 172.16.0.0/12 ip daddr 239.255.255.253 counter accept");

					// Allow Link-Local addresses
					AddRule(rules, "ipv6", "add rule ip6 filter OUTPUT ip6 saddr fe80::/10 counter accept");

					// Allow multicast
					AddRule(rules, "ipv6", "add rule ip6 filter OUTPUT ip6 daddr ff00::/8 counter accept");
				}

				if (Engine.Instance.Options.GetBool("netlock.allow_ping"))
				{
					AddRule(rules, "ipv4", "add rule ip filter OUTPUT icmp type echo-reply counter accept");
					AddRule(rules, "ipv6", "add rule ip6 filter OUTPUT meta l4proto ipv6-icmp counter accept");
				}

				// Allow TUN
				//AddRule(rules, "ipv4", "add rule ip filter OUTPUT oifname \"tun*\" counter accept");
				//AddRule(rules, "ipv6", "add rule ip6 filter OUTPUT oifname \"tun*\" counter accept");

				// If incoming=allow, allow packet response to out
				// We avoid a general rules, because in block/block mode don't drop already exists keepalive
				if (defaultPolicyInput == "ACCEPT")
				{
					AddRule(rules, "ipv4", "add rule ip filter OUTPUT ct state established  counter accept");
					AddRule(rules, "ipv6", "add rule ip6 filter OUTPUT ct state established  counter accept");
				}

				// Allowlist incoming
				foreach (IpAddress ip in ipsAllowlistIncoming.IPs)
				{
					if (ip.IsV4)
						AddRule(rules, "ipv4", "add rule ip filter OUTPUT ip daddr " + ip.ToCIDR() + " ct state established  counter accept");
					if (ip.IsV6)
						AddRule(rules, "ipv6", "add rule ip6 filter OUTPUT ip6 daddr " + ip.ToCIDR() + " ct state established  counter accept");
				}

				// Allowlist outgoing
				foreach (IpAddress ip in ipsAllowlistOutgoing.IPs)
				{
					if (ip.IsV4)
						AddRule(rules, "ipv4", "add rule ip filter OUTPUT ip daddr " + ip.ToCIDR() + " counter accept");
					if (ip.IsV6)
						AddRule(rules, "ipv6", "add rule ip6 filter OUTPUT ip6 daddr " + ip.ToCIDR() + " counter accept");
				}

				// Redundand, equal to default policy
				AddRule(rules, "ipv4", "add rule ip filter OUTPUT counter " + defaultPolicyOutput + " comment \"eddie_ip_filter_OUTPUT_latest_rule\"");
				AddRule(rules, "ipv6", "add rule ip6 filter OUTPUT counter " + defaultPolicyOutput + " comment \"eddie_ip6_filter_OUTPUT_latest_rule\"");

				// Apply 
				Core.Elevated.Command c = new Core.Elevated.Command();
				c.Parameters["command"] = "netlock-nftables-activate";
				c.Parameters["support-ipv4"] = (m_supportIPv4 ? "y" : "n");
				c.Parameters["support-ipv6"] = (m_supportIPv6 ? "y" : "n");
				c.Parameters["rules"] = rules.ToString();
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

			string result = Engine.Instance.Elevated.DoCommandSync("netlock-nftables-deactivate");
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
					Engine.Instance.Elevated.DoCommandSync("netlock-nftables-accept-ip", "layer", (ip.IsV4 ? "ipv4" : "ipv6"), "direction", "in", "action", "del", "cidr", ip.ToCIDR());
				}
			}

			// Incoming - Add IP
			foreach (IpAddress ip in ipsAllowlistIncoming.IPs)
			{
				if (((ip.IsV4) && (m_supportIPv4 == false)) || ((ip.IsV6) && (m_supportIPv6 == false)))
					continue;

				if (m_ipsAllowlistIncoming.Contains(ip) == false)
				{
					Engine.Instance.Elevated.DoCommandSync("netlock-nftables-accept-ip", "layer", (ip.IsV4 ? "ipv4" : "ipv6"), "direction", "in", "action", "add", "cidr", ip.ToCIDR());
				}
			}

			// Outgoing - Remove IP not present in the new list
			foreach (IpAddress ip in m_ipsAllowlistOutgoing.IPs)
			{
				if (((ip.IsV4) && (m_supportIPv4 == false)) || ((ip.IsV6) && (m_supportIPv6 == false)))
					continue;

				if (ipsAllowlistOutgoing.Contains(ip) == false)
				{
					Engine.Instance.Elevated.DoCommandSync("netlock-nftables-accept-ip", "layer", (ip.IsV4 ? "ipv4" : "ipv6"), "direction", "out", "action", "del", "cidr", ip.ToCIDR());
				}
			}

			// Outgoing - Add IP
			foreach (IpAddress ip in ipsAllowlistOutgoing.IPs)
			{
				if (((ip.IsV4) && (m_supportIPv4 == false)) || ((ip.IsV6) && (m_supportIPv6 == false)))
					continue;

				if (m_ipsAllowlistOutgoing.Contains(ip) == false)
				{
					Engine.Instance.Elevated.DoCommandSync("netlock-nftables-accept-ip", "layer", (ip.IsV4 ? "ipv4" : "ipv6"), "direction", "out", "action", "add", "cidr", ip.ToCIDR());
				}
			}

			m_ipsAllowlistIncoming = ipsAllowlistIncoming;
			m_ipsAllowlistOutgoing = ipsAllowlistOutgoing;
		}

		public override void AllowInterface(NetworkInterface networkInterface)
		{
			base.AllowInterface(networkInterface); 

			Engine.Instance.Elevated.DoCommandSync("netlock-nftables-interface", "id", networkInterface.Id, "ipv4", m_supportIPv4 ? "1" : "0", "ipv6", m_supportIPv6 ? "1" : "0", "action", "add");
		}

		public override void DeallowInterface(NetworkInterface networkInterface)
		{
			base.DeallowInterface(networkInterface); 

			Engine.Instance.Elevated.DoCommandSync("netlock-nftables-interface", "id", networkInterface.Id, "ipv4", m_supportIPv4 ? "1" : "0", "ipv6", m_supportIPv6 ? "1" : "0", "action", "del");
		}
	}
}
