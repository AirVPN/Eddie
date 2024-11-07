// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2023 AirVPN (support@airvpn.org) / https://airvpn.org
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

using Eddie.Core;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Xml;

namespace Eddie.Platform.Windows
{
	public class NetworkLockWfp : NetworkLockPlugin
	{
		private Dictionary<string, WfpItem> m_rules = new Dictionary<string, WfpItem>();
		private string m_lastestIpsAllowlistIncoming = "";
		private string m_lastestIpsAllowlistOutgoing = "";

		public override string GetCode()
		{
			return "windows_wfp";
		}

		public override string GetName()
		{
			return "Windows Filtering Platform";
		}

		public override void Init()
		{
			base.Init();
		}

		public override void Activation()
		{
			base.Activation();

			try
			{
				// Block All
				if (Engine.Instance.ProfileOptions.Get("netlock.incoming") == "block")
				{
					XmlDocument xmlDocRule = new XmlDocument();
					XmlElement xmlRule = xmlDocRule.CreateElement("rule");
					xmlRule.SetAttribute("name", "NetLock - In - Block All");
					xmlRule.SetAttribute("layer", "all-in");
					xmlRule.SetAttribute("action", "block");
					XmlElement XmlIf1 = xmlDocRule.CreateElement("if"); // Allow Elevated
					xmlRule.AppendChild(XmlIf1);
					XmlIf1.SetAttribute("field", "ip_local_interface");
					XmlIf1.SetAttribute("match", "not_equal");
					XmlIf1.SetAttribute("interface", "loopback");
					AddRule("netlock_in_block_all", xmlRule);
				}
				if (Engine.Instance.ProfileOptions.Get("netlock.outgoing") == "block")
				{
					XmlDocument xmlDocRule = new XmlDocument();
					XmlElement xmlRule = xmlDocRule.CreateElement("rule");
					xmlRule.SetAttribute("name", "NetLock - Out - Block All");
					xmlRule.SetAttribute("layer", "all-out");
					xmlRule.SetAttribute("action", "block");
					XmlElement XmlIf1 = xmlDocRule.CreateElement("if"); // Allow Elevated
					xmlRule.AppendChild(XmlIf1);
					XmlIf1.SetAttribute("field", "ip_local_interface");
					XmlIf1.SetAttribute("match", "not_equal");
					XmlIf1.SetAttribute("interface", "loopback");
					AddRule("netlock_out_block_all", xmlRule);
				}

				// Allow loopback
				{
					XmlDocument xmlDocRule = new XmlDocument();
					XmlElement xmlRule = xmlDocRule.CreateElement("rule");
					xmlRule.SetAttribute("name", "NetLock - Allow loopback");
					xmlRule.SetAttribute("layer", "all");
					xmlRule.SetAttribute("action", "permit");
					XmlElement XmlIf1 = xmlDocRule.CreateElement("if");
					xmlRule.AppendChild(XmlIf1);
					XmlIf1.SetAttribute("field", "ip_local_interface");
					XmlIf1.SetAttribute("match", "equal");
					XmlIf1.SetAttribute("interface", "loopback");
					AddRule("netlock_allow_loopback", xmlRule);
				}

				// Allow Eddie UI
				AddRule("netlock_allow_eddie_ui", Wfp.CreateItemAllowProgram("NetLock - Allow Eddie UI", Platform.Instance.GetExecutablePath()));

				// Allow Eddie Elevated
				AddRule("netlock_allow_eddie_elevated", Wfp.CreateItemAllowProgram("NetLock - Allow Eddie Elevated", Platform.Instance.GetElevatedHelperPath()));

				if (Engine.Instance.ProfileOptions.GetLower("proxy.mode") == "tor")
				{
					string path = TorControl.GetTorExecutablePath();
					if (path != "")
					{
						AddRule("netlock_allow_tor", Wfp.CreateItemAllowProgram("NetLock - Allow Tor", path));
					}
				}

				if (Engine.Instance.ProfileOptions.GetBool("netlock.allow_ping") == true)
				{
					// Allow ICMP IPv4
					{
						XmlDocument xmlDocRule = new XmlDocument();
						XmlElement xmlRule = xmlDocRule.CreateElement("rule");
						xmlRule.SetAttribute("name", "NetLock - Allow ICMP - IPv4");
						xmlRule.SetAttribute("layer", "all");
						xmlRule.SetAttribute("action", "permit");
						XmlElement XmlIf1 = xmlDocRule.CreateElement("if");
						xmlRule.AppendChild(XmlIf1);
						XmlIf1.SetAttribute("field", "ip_protocol");
						XmlIf1.SetAttribute("match", "equal");
						XmlIf1.SetAttribute("protocol", "icmp");
						AddRule("netlock_allow_ping_ipv4", xmlRule);
					}

					// Allow ICMP IPv6 - new in 2.23.0
					{
						XmlDocument xmlDocRule = new XmlDocument();
						XmlElement xmlRule = xmlDocRule.CreateElement("rule");
						xmlRule.SetAttribute("name", "NetLock - Allow ICMP - IPv6");
						xmlRule.SetAttribute("layer", "all");
						xmlRule.SetAttribute("action", "permit");
						XmlElement XmlIf1 = xmlDocRule.CreateElement("if");
						xmlRule.AppendChild(XmlIf1);
						XmlIf1.SetAttribute("field", "ip_protocol");
						XmlIf1.SetAttribute("match", "equal");
						XmlIf1.SetAttribute("protocol", "icmp6");
						AddRule("netlock_allow_ping_ipv6", xmlRule);
					}
				}

				if (Engine.Instance.ProfileOptions.GetBool("netlock.allow_ipv4ipv6translation") == true)
				{
					AddRule("netlock_allow_ipv4ipv6translation1", Wfp.CreateItemAllowAddress("NetLock - IPv4-IPv6 Translation - Allow Subnet 1 - IPv6", new IpAddress("64:ff9b::/96"))); // RFC 6052 // 2.24.3
					AddRule("netlock_allow_ipv4ipv6translation2", Wfp.CreateItemAllowAddress("NetLock - IPv4-IPv6 Translation - Allow Subnet 2 - IPv6", new IpAddress("64:ff9b:1::/48"))); // RFC 6052 // 2.24.3
				}

				if (Engine.Instance.ProfileOptions.GetBool("netlock.allow_private") == true)
				{
					AddRule("netlock_allow_private_ipv4_local1", Wfp.CreateItemAllowAddress("NetLock - Private - Allow Local Subnet 1 - IPv4", new IpAddress("192.168.0.0/255.255.0.0")));
					AddRule("netlock_allow_private_ipv4_local2", Wfp.CreateItemAllowAddress("NetLock - Private - Allow Local Subnet 2 - IPv4", new IpAddress("172.16.0.0/255.240.0.0")));
					AddRule("netlock_allow_private_ipv4_local3", Wfp.CreateItemAllowAddress("NetLock - Private - Allow Local Subnet 3 - IPv4", new IpAddress("10.0.0.0/255.0.0.0")));
					AddRule("netlock_allow_private_ipv4_multicast", Wfp.CreateItemAllowAddress("NetLock - Private - Allow Multicast - IPv4", new IpAddress("224.0.0.0/255.255.255.0")));
					AddRule("netlock_allow_private_ipv4_ssdp", Wfp.CreateItemAllowAddress("NetLock - Private - Allow Simple Service Discovery Protocol address", new IpAddress("239.255.255.250/255.255.255.255")));
					AddRule("netlock_allow_private_ipv4_slp", Wfp.CreateItemAllowAddress("NetLock - Private - Allow Service Location Protocol", new IpAddress("239.255.255.253/255.255.255.255")));

					AddRule("netlock_allow_private_ipv6_local1", Wfp.CreateItemAllowAddress("NetLock - Private - Allow Local Subnet 1 - IPv6", new IpAddress("fe80::/10"))); // New in 2.23.0
					AddRule("netlock_allow_private_ipv6_local2", Wfp.CreateItemAllowAddress("NetLock - Private - Allow Local Subnet 2 - IPv6", new IpAddress("ff00::/8"))); // New in 2.23.0
					AddRule("netlock_allow_private_ipv6_local3", Wfp.CreateItemAllowAddress("NetLock - Private - Allow Local Subnet 2 - IPv6", new IpAddress("fc00::/7"))); // New in 2.24.3
				}

				// Without this, Windows stay in 'Identifying network...' and OpenVPN in 'Waiting TUN to come up'. // Note 2018: don't occur in Win10?
				if (Engine.Instance.ProfileOptions.GetBool("netlock.allow_dhcp") == true)
				{
					// New in 2.23.0
					AddRule("netlock_allow_dhcp_ipv4_rule1", Wfp.CreateItemAllowAddress("NetLock - DHCP - Allow - IPv4 rule 1", new IpAddress("255.255.255.255/255.255.255.255")));
					AddRule("netlock_allow_dhcp_ipv6_rule1", Wfp.CreateItemAllowAddress("NetLock - DHCP - Allow - IPv6 rule 1", new IpAddress("ff02::1:2/128")));
					AddRule("netlock_allow_dhcp_ipv6_rule2", Wfp.CreateItemAllowAddress("NetLock - DHCP - Allow - IPv6 rule 2", new IpAddress("ff05::1:3/128")));

					/*
					XmlDocument xmlDocRule = new XmlDocument();

					XmlElement xmlRuleIPv4 = xmlDocRule.CreateElement("rule");
					xmlRuleIPv4.SetAttribute("name", "NetLock - Allow DHCP - IPv4");
					xmlRuleIPv4.SetAttribute("layer", "all");
					xmlRuleIPv4.SetAttribute("action", "permit");

					XmlElement xmlRuleIPv4If1 = xmlDocRule.CreateElement("if");
					xmlRuleIPv4.AppendChild(xmlRuleIPv4If1);
					xmlRuleIPv4If1.SetAttribute("field", "ip_protocol");
					xmlRuleIPv4If1.SetAttribute("match", "equal");
					xmlRuleIPv4If1.SetAttribute("protocol", "udp");

					XmlElement xmlRuleIPv4If3 = xmlDocRule.CreateElement("if");
					xmlRuleIPv4.AppendChild(xmlRuleIPv4If3);
					xmlRuleIPv4If3.SetAttribute("field", "ip_remote_port");
					xmlRuleIPv4If3.SetAttribute("match", "equal");
					xmlRuleIPv4If3.SetAttribute("port", "67");

					XmlElement xmlRuleIPv4If2 = xmlDocRule.CreateElement("if");
					xmlRuleIPv4.AppendChild(xmlRuleIPv4If2);
					xmlRuleIPv4If2.SetAttribute("field", "ip_local_port");
					xmlRuleIPv4If2.SetAttribute("match", "equal");
					xmlRuleIPv4If2.SetAttribute("port", "68");

					AddRule("netlock_allow_ipv4_dhcp", xmlRuleIPv4);

					XmlElement xmlRuleIPv6 = xmlDocRule.CreateElement("rule");
					xmlRuleIPv6.SetAttribute("name", "NetLock - Allow DHCP - IPv6");
					xmlRuleIPv6.SetAttribute("layer", "all");
					xmlRuleIPv6.SetAttribute("action", "permit");

					XmlElement xmlRuleIPv6If1 = xmlDocRule.CreateElement("if");
					xmlRuleIPv6.AppendChild(xmlRuleIPv6If1);
					xmlRuleIPv6If1.SetAttribute("field", "ip_protocol");
					xmlRuleIPv6If1.SetAttribute("match", "equal");
					xmlRuleIPv6If1.SetAttribute("protocol", "udp");

					XmlElement xmlRuleIPv6If2 = xmlDocRule.CreateElement("if");
					xmlRuleIPv6.AppendChild(xmlRuleIPv6If2);
					xmlRuleIPv6If2.SetAttribute("field", "ip_local_port");
					xmlRuleIPv6If2.SetAttribute("match", "equal");
					xmlRuleIPv6If2.SetAttribute("port", "546");

					XmlElement xmlRuleIPv6If3 = xmlDocRule.CreateElement("if");
					xmlRuleIPv6.AppendChild(xmlRuleIPv6If3);
					xmlRuleIPv6If3.SetAttribute("field", "ip_remote_port");
					xmlRuleIPv6If3.SetAttribute("match", "equal");
					xmlRuleIPv6If3.SetAttribute("port", "547");

					AddRule("netlock_allow_ipv6_dhcp", xmlRuleIPv6);
					*/
				}

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

			RemoveAllRules();

			m_lastestIpsAllowlistIncoming = "";
			m_lastestIpsAllowlistOutgoing = "";
		}

		public override void AllowProgram(string path)
		{
			base.AllowProgram(path);

			string hash = path.HashSHA256();

			AddRule("netlock_allow_program_" + hash, Wfp.CreateItemAllowProgram("NetLock - Program - Allow " + path, path));
		}

		public override void DeallowProgram(string path)
		{
			base.DeallowProgram(path);

			string hash = path.HashSHA256();

			RemoveRule("netlock_allow_program_" + hash);
		}

		public override void AllowInterface(NetworkInterface networkInterface)
		{
			base.AllowInterface(networkInterface);

			Json jInfo = Engine.Instance.NetworkInterfaceInfoBuild(networkInterface);

			string id = networkInterface.Id;

			// Remember: Fail at WFP side with a "Unknown interface" if the network interface have IPv4 or IPv6 disabled (Ipv6IfIndex == 0).

			if ((jInfo != null) && (jInfo.HasKey("support_ipv4")) && (Conversions.ToBool(jInfo["support_ipv4"].Value)))
				AddRule("netlock_allow_interface_" + id + "_ipv4", Wfp.CreateItemAllowInterface("NetLock - Interface - Allow " + id + " - IPv4", id, "ipv4"));

			if ((jInfo != null) && (jInfo.HasKey("support_ipv6")) && (Conversions.ToBool(jInfo["support_ipv6"].Value)))
				AddRule("netlock_allow_interface_" + id + "_ipv6", Wfp.CreateItemAllowInterface("NetLock - Interface - Allow " + id + " - IPv6", id, "ipv6"));
		}

		public override void DeallowInterface(NetworkInterface networkInterface)
		{
			base.DeallowInterface(networkInterface);

			string id = networkInterface.Id;

			RemoveRule("netlock_allow_interface_" + id + "_ipv4");
			RemoveRule("netlock_allow_interface_" + id + "_ipv6");
		}

		public override void OnUpdateIps()
		{
			base.OnUpdateIps();

			// TOFIX: Crash with a lots of IPs, simulate with GetIpsAllowlistOutgoing(true)

			IpAddresses ipsAllowlistIncoming = GetIpsAllowlistIncoming();
			IpAddresses ipsAllowlistOutgoing = GetIpsAllowlistOutgoing(false); // Don't need full ip, because the client it's allowed as program.

			string currentIpsAllowlistIncoming = ipsAllowlistIncoming.ToString();

			if (currentIpsAllowlistIncoming != m_lastestIpsAllowlistIncoming)
			{
				if (ExistsRule("netlock_allow_incoming_ips_v4"))
					RemoveRule("netlock_allow_incoming_ips_v4");
				if (ExistsRule("netlock_allow_incoming_ips_v6"))
					RemoveRule("netlock_allow_incoming_ips_v6");

				m_lastestIpsAllowlistIncoming = currentIpsAllowlistIncoming;

				XmlElement xmlRuleIncomingV4 = null;
				XmlElement xmlRuleIncomingV6 = null;

				foreach (IpAddress ip in ipsAllowlistIncoming.IPs)
				{
					XmlElement XmlIf = null;

					if (ip.Valid)
					{
						if (ip.IsV4)
						{
							if (xmlRuleIncomingV4 == null)
							{
								XmlDocument xmlDocRuleIncomingV4 = new XmlDocument();
								xmlRuleIncomingV4 = xmlDocRuleIncomingV4.CreateElement("rule");
								xmlRuleIncomingV4.SetAttribute("name", "NetLock - Allow IP - Incoming - IPv4");
								xmlRuleIncomingV4.SetAttribute("layer", "ipv4");
								xmlRuleIncomingV4.SetAttribute("action", "permit");
							}
							XmlIf = xmlRuleIncomingV4.OwnerDocument.CreateElement("if");
							xmlRuleIncomingV4.AppendChild(XmlIf);
						}
						else if (ip.IsV6)
						{
							if (xmlRuleIncomingV6 == null)
							{
								XmlDocument xmlDocRuleIncomingV6 = new XmlDocument();
								xmlRuleIncomingV6 = xmlDocRuleIncomingV6.CreateElement("rule");
								xmlRuleIncomingV6.SetAttribute("name", "NetLock - Allow IP - Incoming - IPv6");
								xmlRuleIncomingV6.SetAttribute("layer", "ipv6");
								xmlRuleIncomingV6.SetAttribute("action", "permit");
							}
							XmlIf = xmlRuleIncomingV6.OwnerDocument.CreateElement("if");
							xmlRuleIncomingV6.AppendChild(XmlIf);
						}
					}

					if (XmlIf != null)
					{
						XmlIf.SetAttribute("field", "ip_remote_address");
						XmlIf.SetAttribute("match", "equal");
						XmlIf.SetAttribute("address", ip.Address);
						XmlIf.SetAttribute("mask", ip.Mask);
					}
				}

				if (xmlRuleIncomingV4 != null)
					AddRule("netlock_allow_incoming_ips_v4", xmlRuleIncomingV4);
				if (xmlRuleIncomingV6 != null)
					AddRule("netlock_allow_incoming_ips_v6", xmlRuleIncomingV6);
			}

			string currentIpsAllowlistOutgoing = ipsAllowlistOutgoing.ToString();
			if (currentIpsAllowlistOutgoing != m_lastestIpsAllowlistOutgoing)
			{
				if (ExistsRule("netlock_allow_outgoing_ips_v4"))
					RemoveRule("netlock_allow_outgoing_ips_v4");
				if (ExistsRule("netlock_allow_outgoing_ips_v6"))
					RemoveRule("netlock_allow_outgoing_ips_v6");

				m_lastestIpsAllowlistOutgoing = currentIpsAllowlistOutgoing;

				XmlElement xmlRuleOutgoingV4 = null;
				XmlElement xmlRuleOutgoingV6 = null;

				foreach (IpAddress ip in ipsAllowlistOutgoing.IPs)
				{
					XmlElement XmlIf = null;

					if (ip.Valid)
					{
						if (ip.IsV4)
						{
							if (xmlRuleOutgoingV4 == null)
							{
								XmlDocument xmlDocRuleOutgoingV4 = new XmlDocument();
								xmlRuleOutgoingV4 = xmlDocRuleOutgoingV4.CreateElement("rule");
								xmlRuleOutgoingV4.SetAttribute("name", "NetLock - Allow IP - Outgoing - IPv4");
								xmlRuleOutgoingV4.SetAttribute("layer", "ipv4-out");
								xmlRuleOutgoingV4.SetAttribute("action", "permit");
							}
							XmlIf = xmlRuleOutgoingV4.OwnerDocument.CreateElement("if");
							xmlRuleOutgoingV4.AppendChild(XmlIf);
						}
						else if (ip.IsV6)
						{
							if (xmlRuleOutgoingV6 == null)
							{
								XmlDocument xmlDocRuleOutgoingV6 = new XmlDocument();
								xmlRuleOutgoingV6 = xmlDocRuleOutgoingV6.CreateElement("rule");
								xmlRuleOutgoingV6.SetAttribute("name", "NetLock - Allow IP - Outgoing - IPv6");
								xmlRuleOutgoingV6.SetAttribute("layer", "ipv6-out");
								xmlRuleOutgoingV6.SetAttribute("action", "permit");
							}
							XmlIf = xmlRuleOutgoingV6.OwnerDocument.CreateElement("if");
							xmlRuleOutgoingV6.AppendChild(XmlIf);
						}
					}

					if (XmlIf != null)
					{
						XmlIf.SetAttribute("field", "ip_remote_address");
						XmlIf.SetAttribute("match", "equal");
						XmlIf.SetAttribute("address", ip.Address);
						XmlIf.SetAttribute("mask", ip.Mask);
					}
				}

				if (xmlRuleOutgoingV4 != null)
					AddRule("netlock_allow_outgoing_ips_v4", xmlRuleOutgoingV4);
				if (xmlRuleOutgoingV6 != null)
					AddRule("netlock_allow_outgoing_ips_v6", xmlRuleOutgoingV6);
			}
		}

		/*
		public override void OnRecoveryLoad(XmlElement root)
		{
			base.OnRecoveryLoad(root);

            if (root.HasAttribute("ids"))
            {
                string list = root.GetAttribute("ids");
                string[] ids = list.Split(';');
                foreach(string id in ids)
                {
                    ulong nid;
                    if(ulong.TryParse(id, out nid))
                        Wfp.RemoveItemId(nid);
                }
            }
		}

		public override void OnRecoverySave(XmlElement root)
		{
			base.OnRecoverySave(root);

            lock (m_rules)
            {
                string list = "";
                foreach (WfpItem item in m_rules.Values)
                {
                    foreach (ulong id in item.FirewallIds)
                        list += id.ToString() + ";";                    
                }
                root.SetAttributeNode("ids", list);
            }
        }
        */

		public void AddRule(string code, XmlElement xmlRule)
		{
			lock (m_rules)
			{
				if (m_rules.ContainsKey(code))
				{
					Engine.Instance.Logs.LogVerbose("NetLock WFP rule '" + code + "' already exists");
				}
				else
				{
					WfpItem item = Wfp.AddItem(code, xmlRule);
					m_rules[code] = item;
				}
			}
		}

		public void RemoveRule(string code)
		{
			lock (m_rules)
			{
				if (m_rules.ContainsKey(code) == false)
					return;

				WfpItem item = m_rules[code];
				m_rules.Remove(code);
				Wfp.RemoveItem(item);
			}
		}

		public bool ExistsRule(string code)
		{
			return m_rules.ContainsKey(code);
		}

		public void RemoveAllRules()
		{
			lock (m_rules)
			{
				foreach (WfpItem item in m_rules.Values)
				{
					Wfp.RemoveItem(item);
				}
				m_rules.Clear();
			}
		}
	}
}
