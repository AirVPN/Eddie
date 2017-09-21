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
	public class NetworkLockWfp : NetworkLockPlugin
	{
        private Dictionary<string, WfpItem> m_rules = new Dictionary<string, WfpItem>();
        private string m_lastestIpList = "";
        
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

            // Block All
            {
                XmlDocument xmlDocRule = new XmlDocument();
                XmlElement xmlRule = xmlDocRule.CreateElement("rule");
                xmlRule.SetAttribute("name", "NetLock - Block All");
                xmlRule.SetAttribute("layer", "all");
                xmlRule.SetAttribute("action", "block");
                AddRule("netlock_block_all", xmlRule);
            }

            // Allow Eddie / OpenVPN / Stunnel / Plink
            AddRule("netlock_allow_eddie", Wfp.CreateItemAllowProgram("NetLock - Allow Eddie", Platform.Instance.GetExecutablePath()));

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

            if (Engine.Instance.Storage.GetBool("netlock.allow_ping") == true)
            {
                // Allow ICMP
                {
                    XmlDocument xmlDocRule = new XmlDocument();
                    XmlElement xmlRule = xmlDocRule.CreateElement("rule");
                    xmlRule.SetAttribute("name", "NetLock - Allow ICMP");
                    xmlRule.SetAttribute("layer", "all");
                    xmlRule.SetAttribute("action", "permit");                    
                    XmlElement XmlIf1 = xmlDocRule.CreateElement("if");
                    xmlRule.AppendChild(XmlIf1);
                    XmlIf1.SetAttribute("field", "ip_protocol");
                    XmlIf1.SetAttribute("match", "equal");
                    XmlIf1.SetAttribute("protocol", "icmp");
                    AddRule("netlock_allow_icmp", xmlRule);                    
                }
            }

            if (Engine.Instance.Storage.GetBool("netlock.allow_private") == true)
            {
                AddRule("netlock_allow_ipv4_local1", Wfp.CreateItemAllowAddress("NetLock - Private - Allow Local Subnet 1 - IPv4", new IpAddress("192.168.0.0/255.255.0.0")));
                AddRule("netlock_allow_ipv4_local2", Wfp.CreateItemAllowAddress("NetLock - Private - Allow Local Subnet 2 - IPv4", new IpAddress("172.16.0.0/255.240.0.0")));
                AddRule("netlock_allow_ipv4_local3", Wfp.CreateItemAllowAddress("NetLock - Private - Allow Local Subnet 3 - IPv4", new IpAddress("10.0.0.0/255.0.0.0")));
                AddRule("netlock_allow_ipv4_multicast", Wfp.CreateItemAllowAddress("NetLock - Private - Allow Multicast - IPv4", new IpAddress("224.0.0.0/255.255.255.0")));
                AddRule("netlock_allow_ipv4_ssdp", Wfp.CreateItemAllowAddress("NetLock - Private - Allow Simple Service Discovery Protocol address", new IpAddress("239.255.255.250/255.255.255.255")));
                AddRule("netlock_allow_ipv4_slp", Wfp.CreateItemAllowAddress("NetLock - Private - Allow Service Location Protocol", new IpAddress("239.255.255.253/255.255.255.255")));
            }

            // Without this, Windows stay in 'Identifying network...' and OpenVPN in 'Waiting TUN to come up'.
            {
                XmlDocument xmlDocRule = new XmlDocument();
                XmlElement xmlRule = xmlDocRule.CreateElement("rule");
                xmlRule.SetAttribute("name", "NetLock - Allow DHCP");
                xmlRule.SetAttribute("layer", "all");
                xmlRule.SetAttribute("action", "permit");
                
                XmlElement XmlIf1 = xmlDocRule.CreateElement("if");
                xmlRule.AppendChild(XmlIf1);
                XmlIf1.SetAttribute("field", "ip_protocol");
                XmlIf1.SetAttribute("match", "equal");
                XmlIf1.SetAttribute("protocol", "udp");

                XmlElement XmlIf2 = xmlDocRule.CreateElement("if");
                xmlRule.AppendChild(XmlIf2);
                XmlIf2.SetAttribute("field", "ip_local_port");
                XmlIf2.SetAttribute("match", "equal");
                XmlIf2.SetAttribute("port", "68");

                XmlElement XmlIf3 = xmlDocRule.CreateElement("if");
                xmlRule.AppendChild(XmlIf3);
                XmlIf3.SetAttribute("field", "ip_remote_port");
                XmlIf3.SetAttribute("match", "equal");
                XmlIf3.SetAttribute("port", "67");                

                AddRule("netlock_allow_dhcp", xmlRule);
            }            
            
            OnUpdateIps();
        }

		public override void Deactivation()
		{
			base.Deactivation();

            RemoveAllRules();

            m_lastestIpList = "";
        }

        public override void AllowProgram(string path, string name, string guid)
        {
            base.AllowProgram(path, name, guid);

            AddRule("netlock_allow_program_" + guid, Wfp.CreateItemAllowProgram("NetLock - Program - Allow " + name, path));
        }

        public override void DeallowProgram(string path, string name, string guid)
        {
            base.DeallowProgram(path, name, guid);

            RemoveRule("netlock_allow_program_" + guid);
        }

        public override void AllowInterface(string id)
        {
            base.AllowInterface(id);

            AddRule("netlock_allow_interface_" + id + "_ipv4", Wfp.CreateItemAllowInterface("NetLock - Interface - Allow " + id + " - IPv4", id, "ipv4"));

            /*
            // TOFIX: Must be enabled in future when IPv6 support is well tested.
            // Remember: May fail at WFP side with a "Unknown interface" because network interface with IPv6 disabled have Ipv6IfIndex == 0.
            if (Engine.Instance.Storage.GetLower("ipv6.mode") != "disable")
                AddRule("netlock_allow_interface_" + id + "_ipv6", Wfp.CreateItemAllowInterface("NetLock - Interface - Allow " + id + " - IPv6", id, "ipv6"));
            */
        }

        public override void DeallowInterface(string id)
        {
            base.DeallowInterface(id);

            RemoveRule("netlock_allow_interface_" + id + "_ipv4");
            RemoveRule("netlock_allow_interface_" + id + "_ipv6");
        }

        public override void OnUpdateIps()
		{
            base.OnUpdateIps();

			IpAddresses ipsAllowed = GetAllIps(false); // Don't need full ip, because the client it's allowed as program.			
            string ipList = ipsAllowed.ToString();
            
            if (ipList != m_lastestIpList)
            {
                if(ExistsRule("netlock_allow_ips_v4"))
                    RemoveRule("netlock_allow_ips_v4");
                if(ExistsRule("netlock_allow_ips_v6"))
                    RemoveRule("netlock_allow_ips_v6");

                m_lastestIpList = ipList;

                XmlElement xmlRuleV4 = null;
                XmlElement xmlRuleV6 = null;

				foreach (IpAddress ip in ipsAllowed.IPs)
                {
                    XmlElement XmlIf = null;

                    if(ip.Valid)
                    {
                        if(ip.IsV4)
                        {
                            if(xmlRuleV4 == null)
                            {
                                XmlDocument xmlDocRuleV4 = new XmlDocument();
                                xmlRuleV4 = xmlDocRuleV4.CreateElement("rule");
                                xmlRuleV4.SetAttribute("name", "NetLock - Allow IP - IPv4");
                                xmlRuleV4.SetAttribute("layer", "ipv4");
                                xmlRuleV4.SetAttribute("action", "permit");
                            }
                            XmlIf = xmlRuleV4.OwnerDocument.CreateElement("if");
                            xmlRuleV4.AppendChild(XmlIf); // bugfix 2.11.9
                        }
                        else if(ip.IsV6)
                        {
                            if (xmlRuleV6 == null)
                            {
                                XmlDocument xmlDocRuleV6 = new XmlDocument();
                                xmlRuleV6 = xmlDocRuleV6.CreateElement("rule");
                                xmlRuleV6.SetAttribute("name", "NetLock - Allow IP - IPv6");
                                xmlRuleV6.SetAttribute("layer", "ipv6");
                                xmlRuleV6.SetAttribute("action", "permit");
                            }
                            XmlIf = xmlRuleV6.OwnerDocument.CreateElement("if");
                            xmlRuleV6.AppendChild(XmlIf); // bugfix 2.11.9
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
    
                if(xmlRuleV4 != null)
                    AddRule("netlock_allow_ips_v4", xmlRuleV4);
                if(xmlRuleV6 != null)
                    AddRule("netlock_allow_ips_v6", xmlRuleV6);
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
                    throw new Exception("Unexpected: NetLock WFP rule '" + code + "' already exists");
                WfpItem item = Wfp.AddItem(code, xmlRule);
                m_rules[code] = item;
            }
        }

        public void RemoveRule(string code)
        {
            lock (m_rules)
            {
                if (m_rules.ContainsKey(code) == false)
                    return;
                    //throw new Exception("Unexpected: NetLock WFP rule '" + code + "' doesn't exists");
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
            lock(m_rules)
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
