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

namespace Eddie.Platforms
{
	public class NetworkLockWfp : NetworkLockPlugin
	{
        private Dictionary<string, WfpItem> m_rules = new Dictionary<string, WfpItem>();
        private string m_lastestIpList = "";

        public bool GetPersistentMode()
        {
            return Engine.Instance.Storage.GetBool("windows.wfp.persistent");
        }

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

            // Allow Eddie / OpenVPN / Stunnel / Plink
            AddRule("netlock_allow_eddie", Wfp.CreateItemAllowProgram("NetLock - Private - Allow Eddie", GetPersistentMode(), Platform.Instance.GetExecutablePath()));

            // Allow loopback
            {
                XmlDocument xmlDocRule = new XmlDocument();
                XmlElement xmlRule = xmlDocRule.CreateElement("rule");
                xmlRule.SetAttribute("name", "NetLock - Allow loopback");
                xmlRule.SetAttribute("layer", "all");                
                xmlRule.SetAttribute("action", "permit");
                if(GetPersistentMode())
                    xmlRule.SetAttribute("persistent", "true");
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
                    if (GetPersistentMode())
                        xmlRule.SetAttribute("persistent", "true");
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
                AddRule("netlock_allow_ipv4_local1", Wfp.CreateItemAllowAddress("NetLock - Private - Allow Local Subnet 1 - IPv4", GetPersistentMode(), "192.168.0.0", "255.255.0.0"));
                AddRule("netlock_allow_ipv4_local2", Wfp.CreateItemAllowAddress("NetLock - Private - Allow Local Subnet 2 - IPv4", GetPersistentMode(), "172.16.0.0", "255.240.0.0"));
                AddRule("netlock_allow_ipv4_local3", Wfp.CreateItemAllowAddress("NetLock - Private - Allow Local Subnet 3 - IPv4", GetPersistentMode(), "10.0.0.0", "255.0.0.0"));
                AddRule("netlock_allow_ipv4_multicast", Wfp.CreateItemAllowAddress("NetLock - Private - Allow Multicast - IPv4", GetPersistentMode(), "224.0.0.0", "255.255.255.0"));
                AddRule("netlock_allow_ipv4_ssdp", Wfp.CreateItemAllowAddress("NetLock - Private - Allow Simple Service Discovery Protocol address", GetPersistentMode(), "239.255.255.250", "255.255.255.255"));
                AddRule("netlock_allow_ipv4_slp", Wfp.CreateItemAllowAddress("NetLock - Private - Allow Service Location Protocol", GetPersistentMode(), "239.255.255.253", "255.255.255.255"));
            }

            // Without this, Windows stay in 'Identifying network...' and OpenVPN in 'Waiting TUN to come up'.
            {
                XmlDocument xmlDocRule = new XmlDocument();
                XmlElement xmlRule = xmlDocRule.CreateElement("rule");
                xmlRule.SetAttribute("name", "NetLock - Allow ICMP");
                xmlRule.SetAttribute("layer", "all");
                xmlRule.SetAttribute("action", "permit");
                if (GetPersistentMode())
                    xmlRule.SetAttribute("persistent", "true");

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

            // Block All
            {
                XmlDocument xmlDocRule = new XmlDocument();
                XmlElement xmlRule = xmlDocRule.CreateElement("rule");
                xmlRule.SetAttribute("name", "NetLock - Block All");
                xmlRule.SetAttribute("layer", "all");
                xmlRule.SetAttribute("action", "block");
                if (GetPersistentMode())
                    xmlRule.SetAttribute("persistent", "true");
                AddRule("netlock_block_all", xmlRule);
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

            AddRule("netlock_allow_program_" + guid, Wfp.CreateItemAllowProgram("NetLock - Program - Allow " + name, GetPersistentMode(), path));
        }

        public override void DeallowProgram(string path, string name, string guid)
        {
            base.DeallowProgram(path, name, guid);

            RemoveRule("netlock_allow_program_" + guid);
        }

        public override void AllowInterface(string id)
        {
            base.AllowInterface(id);

            AddRule("netlock_allow_interface_" + id, Wfp.CreateItemAllowInterface("NetLock - Interface - Allow " + id, GetPersistentMode(), id));
        }

        public override void DeallowInterface(string id)
        {
            base.DeallowInterface(id);

            RemoveRule("netlock_allow_interface_" + id);
        }

        public override void OnUpdateIps()
		{
            base.OnUpdateIps();

            List<IpAddressRange> ipsFirewalled = GetAllIps(false); // Don't need full ip, because the client it's allowed as program.
            string ipList = "";
            foreach (IpAddressRange ip in ipsFirewalled)
            {
                if (ipList != "")
                    ipList += ",";
                ipList += ip.ToCIDR();
            }

            if (ipList != m_lastestIpList)
            {
                if(ExistsRule("netlock_allow_ips"))
                    RemoveRule("netlock_allow_ips");
                
                m_lastestIpList = ipList;

                XmlDocument xmlDocRule = new XmlDocument();
                XmlElement xmlRule = xmlDocRule.CreateElement("rule");
                xmlRule.SetAttribute("name", "NetLock - Allow IP");
                xmlRule.SetAttribute("layer", "ipv4");
                xmlRule.SetAttribute("action", "permit");
                if (GetPersistentMode())
                    xmlRule.SetAttribute("persistent", "true");

                foreach (IpAddressRange ip in ipsFirewalled)
                {
                    XmlElement XmlIf1 = xmlDocRule.CreateElement("if");
                    xmlRule.AppendChild(XmlIf1);
                    XmlIf1.SetAttribute("field", "ip_remote_address");
                    XmlIf1.SetAttribute("match", "equal");
                    XmlIf1.SetAttribute("address", ip.GetAddress());
                    XmlIf1.SetAttribute("mask", ip.GetMask());
                }
    
                AddRule("netlock_allow_ips", xmlRule);
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
                    throw new Exception("Unexpected: NetLock WFP rule already exists");
                WfpItem item = Wfp.AddItem(code, xmlRule);
                m_rules[code] = item;
            }
        }

        public void RemoveRule(string code)
        {
            lock (m_rules)
            {
                if (m_rules.ContainsKey(code) == false)
                    throw new Exception("Unexpected: NetLock WFP rule doesn't exists");
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
