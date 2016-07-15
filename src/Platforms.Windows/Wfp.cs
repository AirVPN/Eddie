﻿// <eddie_source_header>
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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.NetworkInformation;
using System.Management;
using System.Security.Principal;
using System.Xml;
using System.Text;
using System.Threading;
using Eddie.Core;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;

namespace Eddie.Platforms
{
    public class Wfp
    {
        private static Dictionary<string, WfpItem> Items = new Dictionary<string, WfpItem>();

        [DllImport("LibPocketFirewall.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void LibPocketFirewallInit(string name);

        [DllImport("LibPocketFirewall.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool LibPocketFirewallStart(string xml);

        [DllImport("LibPocketFirewall.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool LibPocketFirewallStop();

        [DllImport("LibPocketFirewall.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 LibPocketFirewallAddRule(string xml);

        [DllImport("LibPocketFirewall.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool LibPocketFirewallRemoveRule(UInt64 id);

        [DllImport("LibPocketFirewall.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LibPocketFirewallRemoveRuleDirect(UInt64 id);

        [DllImport("LibPocketFirewall.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr LibPocketFirewallGetLastError();

        public static string LibPocketFirewallGetLastError2()
        {
            IntPtr result = LibPocketFirewallGetLastError();
            string s = Marshal.PtrToStringAnsi(result);
            return s;
        }

        public static string GetName()
        {
            return Constants.Name2 + "-" + Constants.AppID;
        }

        public static void Start()
        {
            LibPocketFirewallInit(GetName());

            XmlDocument xmlStart = new XmlDocument();
            XmlElement xmlInfo = xmlStart.CreateElement("firewall");
            xmlInfo.SetAttribute("description", Constants.Name2);
            xmlInfo.SetAttribute("weight", "max");

            if (LibPocketFirewallStart(xmlInfo.OuterXml) == false)
                throw new Exception(Messages.Format(Messages.WfpStartFail, LibPocketFirewallGetLastError2()));
        }

        public static void Stop()
        {
            LibPocketFirewallStop();
        }

        public static bool RemoveItem(string code)
        {
            if (Items.ContainsKey(code) == false)
                return false;

            WfpItem item = Items[code];

            return RemoveItem(item);
        }

        public static bool RemoveItem(WfpItem item)
        {
            lock(Items)
            {
                if (Items.ContainsValue(item) == false)
                    throw new Exception("Windows WFP, unexpected: Rule '" + item.Code + "' not exists");

                foreach (UInt64 id in item.FirewallIds)
                {
                    bool result = RemoveItemId(id);
                    if (result == false)
                        throw new Exception(Messages.Format(Messages.WfpRuleRemoveFail, LibPocketFirewallGetLastError2()));
                }

                Items.Remove(item.Code);

                if (Items.Count == 0)
                {
                    
                }
            }

            return true;
        }

        public static bool RemoveItemId(ulong id)
        {
            return LibPocketFirewallRemoveRule(id);
        }

        public static WfpItem AddItem(string code, XmlElement xml)
        {
            lock(Items)
            {
                if (Items.ContainsKey(code))
                    throw new Exception("Windows WFP, unexpected: Rule '" + code + "' already exists");

                WfpItem item = new WfpItem();
                item.Code = code;

                List<string> layers = new List<string>();

                if (xml.GetAttribute("layer") == "all")
                {
                    layers.Add("ale_auth_recv_accept_v4");
                    layers.Add("ale_auth_recv_accept_v6");
                    layers.Add("ale_auth_connect_v4");
                    layers.Add("ale_auth_connect_v6");
                    layers.Add("ale_flow_established_v4");
                    layers.Add("ale_flow_established_v6");
                }
                else if (xml.GetAttribute("layer") == "ipv4")
                {
                    layers.Add("ale_auth_recv_accept_v4");
                    layers.Add("ale_auth_connect_v4");
                    layers.Add("ale_flow_established_v4");
                }
                else if (xml.GetAttribute("layer") == "ipv6")
                {
                    layers.Add("ale_auth_recv_accept_v6");
                    layers.Add("ale_auth_connect_v6");
                    layers.Add("ale_flow_established_v6");
                }
                else
                    layers.Add(xml.GetAttribute("layer"));

                if (xml.HasAttribute("weight") == false)
                    xml.SetAttribute("weight", "auto");

                foreach (string layer in layers)
                {
                    XmlElement xmlClone = xml.CloneNode(true) as XmlElement;
                    xmlClone.SetAttribute("layer", layer);                    
                    string xmlStr = xmlClone.OuterXml;

                    UInt64 id1 = LibPocketFirewallAddRule(xmlStr);

                    if (id1 == 0)
                    {
                        throw new Exception(Messages.Format(Messages.WfpRuleAddFail, LibPocketFirewallGetLastError2()));
                    }
                    else
                    {
                        item.FirewallIds.Add(id1);
                    }
                }

                Items[item.Code] = item;

                return item;
            }            
        }

        // Shortcuts

        public static XmlElement CreateItemAllowAddress(string title, bool persistent, string address, string mask)
        {            
            XmlDocument xmlDocRule = new XmlDocument();
            XmlElement xmlRule = xmlDocRule.CreateElement("rule");
            xmlRule.SetAttribute("name", title);
            xmlRule.SetAttribute("layer", "ipv4");
            xmlRule.SetAttribute("action", "permit");
            if (persistent)
                xmlRule.SetAttribute("persistent", "true");
            XmlElement XmlIf1 = xmlDocRule.CreateElement("if");
            xmlRule.AppendChild(XmlIf1);
            XmlIf1.SetAttribute("field", "ip_remote_address");
            XmlIf1.SetAttribute("match", "equal");
            XmlIf1.SetAttribute("address", address);
            XmlIf1.SetAttribute("mask", mask);

            return xmlRule;
        }

        public static XmlElement CreateItemAllowProgram(string title, bool persistent, string path)
        {
            XmlDocument xmlDocRule = new XmlDocument();
            XmlElement xmlRule = xmlDocRule.CreateElement("rule");
            xmlRule.SetAttribute("name", title);
            xmlRule.SetAttribute("layer", "all");
            xmlRule.SetAttribute("action", "permit");
            if(persistent)
                xmlRule.SetAttribute("persistent", "true");
            XmlElement XmlIf1 = xmlDocRule.CreateElement("if");
            xmlRule.AppendChild(XmlIf1);
            XmlIf1.SetAttribute("field", "ale_app_id");
            XmlIf1.SetAttribute("match", "equal");
            XmlIf1.SetAttribute("path", path);

            return xmlRule;
        }

        public static XmlElement CreateItemAllowInterface(string title, bool persistent, string id)
        {
            XmlDocument xmlDocRule = new XmlDocument();
            XmlElement xmlRule = xmlDocRule.CreateElement("rule");
            xmlRule.SetAttribute("name", title);
            xmlRule.SetAttribute("layer", "all");
            xmlRule.SetAttribute("action", "permit");
            if (persistent)
                xmlRule.SetAttribute("persistent", "true");
            XmlElement XmlIf1 = xmlDocRule.CreateElement("if");
            xmlRule.AppendChild(XmlIf1);
            XmlIf1.SetAttribute("field", "ip_local_interface");
            XmlIf1.SetAttribute("match", "equal");
            XmlIf1.SetAttribute("interface", id);

            return xmlRule;
        }

        public static bool ClearPendingRules()
        {
            bool found = false;
            string wfpName = GetName();
            string path = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".xml";

            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "NetSh.exe";
            p.StartInfo.Arguments = "WFP Show Filters file=\"" + path + "\"";
            p.StartInfo.WorkingDirectory = Path.GetTempPath();
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            System.Xml.XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            foreach (XmlElement xmlFilter in xmlDoc.DocumentElement.GetElementsByTagName("filters"))
            {
                foreach (XmlElement xmlItem in xmlFilter.GetElementsByTagName("item"))
                {
                    foreach (XmlElement xmlName in xmlItem.SelectNodes("displayData/name"))
                    {
                        string name = xmlName.InnerText;
                        if (name == wfpName)
                        {
                            foreach (XmlNode xmlFilterId in xmlItem.GetElementsByTagName("filterId"))
                            {
                                ulong id;
                                if (ulong.TryParse(xmlFilterId.InnerText, out id))
                                {
                                    LibPocketFirewallRemoveRuleDirect(id);
                                    found = true;
                                }
                            }
                        }
                    }
                }
            }
            
            File.Delete(path);

            return found;
        }
    }
}
