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
using System.Text;
using System.Xml;

/*
 * NOT USED
 * 
 * Known limits:
 * 
 * - Set all profiles firewallpolicy
 * - Lock only outgoing packets
 * - Already exists outgoing rules are not disabled
*/

namespace Eddie.Core.NetworkLocks
{
	public class RoutingTable : NetworkLockPlugin
	{
		public override string GetCode()
		{
			return "routing_table";
		}

		public override string GetName()
		{
			return "Routing table";
		}

		public override string GetDescription()
		{
			return "Not supported yet";
		}

		public override void Activation()
		{
			base.Activation();


			List<RouteEntry> EntryList = Platform.Instance.RouteList();

			DefaultGateway = "";
			DefaultInterface = "";
			EntryRemoved.Clear();
			EntryAdded.Clear();

			foreach (RouteEntry Entry in EntryList)
			{
				if (Entry.Gateway.Valid)
				{
					if (DefaultGateway.Valid == false)
					{
						DefaultGateway = Entry.Gateway;
						DefaultInterface = Entry.Interface;
					}
					else if (DefaultGateway != Entry.Gateway)
					{
						Failed = true;
						break;
					}
				}
			}

			if (DefaultGateway.Valid == false)
				Failed = true;

			if (Failed)
			{
				DefaultGateway = "";
				DefaultInterface = "";				
				foreach (RouteEntry Entry in EntryList)
				{
					Engine.Instance.Logs.Log(LogType.Verbose, Entry.ToString());
				}

				throw new Exception("error");
			}
			
			/*
			foreach (RouteEntry Entry in EntryList)
			{
				if (IsIP(Entry.Gateway))
				{
					EntryRemoved[Entry.Key] = Entry;
					Engine.Instance.Logs.Log(LogType.Verbose, Messages.Format(Messages.NetworkLockRouteRemoved, Entry.ToString()));
					Entry.Remove();
				}
			}
			*/

			IpAddress destinationHole = DefaultGateway;
			string interfaceHole = "1";
			int routesHoleN = 4;
			for (int i = 0; i < routesHoleN; i++)
			{
				string maskHole = "192.0.0.0";
				string ipHole = Conversions.ToString((256 / routesHoleN * i)) + ".0.0.0";

				RouteAdd(ipHole, maskHole, destinationHole, interfaceHole);
			}
		}

		public override void Deactivation()
		{
			base.Deactivation();

			foreach (RouteEntry Entry in EntryAdded.Values)
				Entry.Remove();

			foreach (RouteEntry Entry in EntryRemoved.Values)
			{
				Entry.Add();
				Engine.Instance.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.NetworkLockRouteRestored, Entry.ToString()));
			}

			DefaultGateway = "";
			DefaultInterface = "";

			EntryAdded.Clear();
			EntryRemoved.Clear();
		}

		public override void AllowIP(IpAddress ip)
		{
			base.AllowIP(ip);

			RouteAdd(ip);
		}

		public override void DeallowIP(IpAddress ip)
		{
			base.DeallowIP(ip);

			RouteRemove(ip);
		}

		public override void OnRecoveryLoad(XmlElement root)
		{
			base.OnRecoveryLoad(root);

			DefaultGateway = root.GetAttribute("gateway");
			DefaultInterface = root.GetAttribute("interface");

			XmlElement nodeAdded = root.GetElementsByTagName("added")[0] as XmlElement;
			foreach (XmlElement nodeEntry in nodeAdded.ChildNodes)
			{
				RouteEntry entry = new RouteEntry();
				entry.ReadXML(nodeEntry);
				EntryAdded[entry.Key] = entry;
			}

			XmlElement nodeRemoved = root.GetElementsByTagName("removed")[0] as XmlElement;
			foreach (XmlElement nodeEntry in nodeRemoved.ChildNodes)
			{
				RouteEntry entry = new RouteEntry();
				entry.ReadXML(nodeEntry);
				EntryRemoved[entry.Key] = entry;
			}
				
		}

		public override void OnRecoverySave(XmlElement root)
		{
			base.OnRecoverySave(root);

			if ((EntryAdded.Count != 0) || (EntryRemoved.Count != 0))
			{
				XmlDocument doc = root.OwnerDocument;				
				root.SetAttribute("gateway", DefaultGateway.Address);
				root.SetAttribute("interface", DefaultInterface);

				XmlElement nodeAdded = root.AppendChild(doc.CreateElement("added")) as XmlElement;
				foreach (RouteEntry entry in EntryAdded.Values)
				{
					XmlElement nodeEntry = nodeAdded.AppendChild(doc.CreateElement("entry")) as XmlElement;
					entry.WriteXML(nodeEntry);
				}

				XmlElement nodeRemoved = root.AppendChild(doc.CreateElement("removed")) as XmlElement;
				foreach (RouteEntry entry in EntryRemoved.Values)
				{
					XmlElement nodeEntry = nodeRemoved.AppendChild(doc.CreateElement("entry")) as XmlElement;
					entry.WriteXML(nodeEntry);
				}
			}
		}


		private IpAddress DefaultGateway = new IpAddress();
		private string DefaultInterface = "";

		private Dictionary<string, RouteEntry> EntryRemoved = new Dictionary<string, RouteEntry>();
		private Dictionary<string, RouteEntry> EntryAdded = new Dictionary<string, RouteEntry>();

		public static bool IsIP(string v)
		{
			if (v == "On-link")
				return true;
			if (IpAddress.IsIP(v))
				return true;
			return false;
		}

		bool Failed = false;

		public void RouteAdd(IpAddress address)
		{
			RouteAdd(address, new IpAddress("255.255.255.255"), DefaultGateway, DefaultInterface);
		}

		public void RouteAdd(IpAddress address, IpAddress mask, IpAddress gateway, string iface)
		{
			lock (this)
			{
				string key = RouteEntry.ToKey(address, mask);
				if (EntryAdded.ContainsKey(key))
				{
					RouteEntry entry = EntryAdded[key];
					entry.RefCount++;
				}
				else
				{
					RouteEntry entry = new RouteEntry();
					entry.Address = address;
					entry.Mask = mask;
					entry.Gateway = gateway;
					entry.Interface = iface;
					EntryAdded[key] = entry;
					entry.Add();
				}

				Recovery.Save();
			}
		}

		public void RouteRemove(IpAddress address)
		{
			RouteRemove(address, new IpAddress("255.255.255.255"));
		}

		public void RouteRemove(IpAddress address, IpAddress mask)
		{
			lock (this)
			{
				string key = RouteEntry.ToKey(address, mask);
				if (EntryAdded.ContainsKey(key))
				{
					RouteEntry entry = EntryAdded[key];
					entry.RefCount--;
					if (entry.RefCount > 0)
					{
					}
					else
					{
						entry.Remove();
						EntryAdded.Remove(key);
					}
				}

				Recovery.Save();
			}
		}

	}
}
