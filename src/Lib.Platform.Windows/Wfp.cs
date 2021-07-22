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
using Microsoft.Win32;
using Eddie.Core;

namespace Eddie.Platform.Windows
{
	public class Wfp
	{
		private static Dictionary<string, WfpItem> Items = new Dictionary<string, WfpItem>();

		public static string GetName()
		{
			return Constants.Name + "-" + Constants.AppID;
		}

		public static bool GetDynamicMode()
		{
			return Engine.Instance.Options.GetBool("windows.wfp.dynamic");
		}

		public static void Start()
		{
			Engine.Instance.Elevated.DoCommandSync("wfp", "action", "init", "name", GetName());

			XmlDocument xmlStart = new XmlDocument();
			XmlElement xmlInfo = xmlStart.CreateElement("firewall");
			xmlInfo.SetAttribute("description", Constants.Name);
			xmlInfo.SetAttribute("weight", "max");
			xmlInfo.SetAttribute("dynamic", GetDynamicMode() ? "true" : "false");

			if (Conversions.ToBool(Engine.Instance.Elevated.DoCommandSync("wfp", "action", "start", "xml", xmlInfo.OuterXml)) == false)
			{
				string wfpLastError = Engine.Instance.Elevated.DoCommandSync("wfp", "action", "last-error");
				throw new Exception(LanguageManager.GetText("WfpStartFail", wfpLastError));
			}
		}

		public static void Stop()
		{
			if (Engine.Instance.Elevated != null) // May have failed the elevation
				Engine.Instance.Elevated.DoCommandSync("wfp", "action", "stop");
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
			lock (Items)
			{
				if (Items.ContainsValue(item) == false)
					throw new Exception("Windows WFP, unexpected: Rule '" + item.Code + "' not exists");

				foreach (UInt64 id in item.FirewallIds)
				{
					bool result = RemoveItemId(id);
					if (result == false)
					{
						string wfpLastError = Engine.Instance.Elevated.DoCommandSync("wfp", "action", "last-error");
						throw new Exception(LanguageManager.GetText("WfpRuleRemoveFail", wfpLastError));
					}

				}

				Items.Remove(item.Code);
			}

			return true;
		}

		public static bool RemoveItemId(ulong id)
		{
			return Conversions.ToBool(Engine.Instance.Elevated.DoCommandSync("wfp", "action", "rule-remove", "id", id.ToString()));
		}

		public static WfpItem AddItem(string code, XmlElement xml)
		{
			lock (Items)
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
				else if (xml.GetAttribute("layer") == "all-in")
				{
					layers.Add("ale_auth_recv_accept_v4");
					layers.Add("ale_auth_recv_accept_v6");
				}
				else if (xml.GetAttribute("layer") == "all-out")
				{
					layers.Add("ale_auth_connect_v4");
					layers.Add("ale_auth_connect_v6");
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
				else if (xml.GetAttribute("layer") == "ipv4-in")
				{
					layers.Add("ale_auth_recv_accept_v4");
				}
				else if (xml.GetAttribute("layer") == "ipv6-in")
				{
					layers.Add("ale_auth_recv_accept_v6");
				}
				else if (xml.GetAttribute("layer") == "ipv4-out")
				{
					layers.Add("ale_auth_connect_v4");
				}
				else if (xml.GetAttribute("layer") == "ipv6-out")
				{
					layers.Add("ale_auth_connect_v6");
				}
				else
					layers.Add(xml.GetAttribute("layer"));

				if (xml.HasAttribute("weight") == false)
				{
					xml.SetAttribute("weight", "1000");
				}

				foreach (string layer in layers)
				{
					XmlElement xmlClone = xml.CloneNode(true) as XmlElement;
					xmlClone.SetAttribute("layer", layer);
					string xmlStr = xmlClone.OuterXml;

					UInt64 id1 = Conversions.ToUInt64(Engine.Instance.Elevated.DoCommandSync("wfp", "action", "rule-add", "xml", xmlStr));

					if (id1 == 0)
					{
						string wfpLastError = Engine.Instance.Elevated.DoCommandSync("wfp", "action", "last-error");
						throw new Exception(LanguageManager.GetText("WfpRuleAddFail", wfpLastError));
					}
					else
					{
						// Only used for debugging WFP issue with rules in some system
						// Engine.Instance.Logs.Log(LogType.Verbose, Messages.Format(Messages.WfpRuleAddSuccess, xmlStr));
						item.FirewallIds.Add(id1);
					}
				}

				Items[item.Code] = item;

				return item;
			}
		}

		public static XmlElement CreateItemAllowAddress(string title, IpAddress range)
		{
			string address = range.Address;
			string mask = range.Mask;

			XmlDocument xmlDocRule = new XmlDocument();
			XmlElement xmlRule = xmlDocRule.CreateElement("rule");
			xmlRule.SetAttribute("name", title);
			if (range.IsV4)
				xmlRule.SetAttribute("layer", "ipv4");
			else if (range.IsV6)
				xmlRule.SetAttribute("layer", "ipv6");
			xmlRule.SetAttribute("action", "permit");
			XmlElement XmlIf1 = xmlDocRule.CreateElement("if");
			xmlRule.AppendChild(XmlIf1);
			XmlIf1.SetAttribute("field", "ip_remote_address");
			XmlIf1.SetAttribute("match", "equal");
			XmlIf1.SetAttribute("address", address);
			XmlIf1.SetAttribute("mask", mask);

			return xmlRule;
		}

		public static XmlElement CreateItemAllowProgram(string title, string path)
		{
			XmlDocument xmlDocRule = new XmlDocument();
			XmlElement xmlRule = xmlDocRule.CreateElement("rule");
			xmlRule.SetAttribute("name", title);
			xmlRule.SetAttribute("layer", "all");
			xmlRule.SetAttribute("action", "permit");
			XmlElement XmlIf1 = xmlDocRule.CreateElement("if");
			xmlRule.AppendChild(XmlIf1);
			XmlIf1.SetAttribute("field", "ale_app_id");
			XmlIf1.SetAttribute("match", "equal");
			XmlIf1.SetAttribute("path", path);

			return xmlRule;
		}

		public static XmlElement CreateItemAllowInterface(string title, string id, string layers)
		{
			XmlDocument xmlDocRule = new XmlDocument();
			XmlElement xmlRule = xmlDocRule.CreateElement("rule");
			xmlRule.SetAttribute("name", title);
			xmlRule.SetAttribute("layer", layers);
			xmlRule.SetAttribute("action", "permit");
			XmlElement XmlIf1 = xmlDocRule.CreateElement("if");
			xmlRule.AppendChild(XmlIf1);
			XmlIf1.SetAttribute("field", "ip_local_interface");
			XmlIf1.SetAttribute("match", "equal");
			XmlIf1.SetAttribute("interface", id);

			return xmlRule;
		}

		public static bool ClearPendingRules()
		{
			return Conversions.ToBool(Engine.Instance.Elevated.DoCommandSync("wfp", "action", "pending-remove", "name", GetName()));
		}
	}
}
