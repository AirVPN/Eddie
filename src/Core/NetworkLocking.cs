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
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Xml;
using AirVPN.Core;

namespace AirVPN.Core
{
    class RouteEntry
    {
        public enum ActionMode
        {
            Nothing = 0, // Do nothing, list only, never added/removed/changed
            Suspended = 1, // If AirLock is active, remove it
            Temporary = 2, // Added by AirLock
        }
        public string Address;
        public string Mask;
        public string Gateway;
        public string Interface;
        public string Metrics;
        public int RefCount = 0;
        //public ActionMode Action;

        public RouteEntry()
        {
            //Action = ActionMode.Nothing;
        }

        public void Add()
        {
            Platform.Instance.RouteAdd(Address, Mask, Gateway);
        }

        public void Remove()
        {
            Platform.Instance.RouteRemove(Address, Mask, Gateway);
        }

		public void ReadXML(XmlElement node)
		{
			Address = node.GetAttribute("address");
			Mask = node.GetAttribute("mask");
			Gateway = node.GetAttribute("gateway");
			Interface = node.GetAttribute("interface");
			Metrics = node.GetAttribute("metrics");
			RefCount = 1;			
		}

		public void WriteXML(XmlElement node)
		{			
			node.SetAttribute("address", Address);
			node.SetAttribute("mask", Mask);
			node.SetAttribute("gateway", Gateway);
			node.SetAttribute("interface", Interface);
			node.SetAttribute("metrics", Metrics);			
		}

        public string Key
        {
            get
            {
				return NetworkLocking.Key(Address, Mask);
            }
        }
    }

    public class RouteScope
    {
        private string m_address = "";

        public RouteScope(string address)
        {
			Start(address, false);
        }

		public RouteScope(string address, bool force)
		{
			Start(address, force);
		}

        ~RouteScope()
        {
            End();
        }

		public void Start(string address, bool force)
		{
			if( (force == false) && (NetworkLocking.Instance.GetEnabled() == false) )
				return;

			if (Utils.IsIP(address) == false)
				return;

			m_address = address;
			NetworkLocking.Instance.RouteAdd(m_address, "255.255.255.255");
		}

        public void End()
        {
            if (m_address != "") // Only one time
            {
				if (NetworkLocking.Instance.GetEnabled() == false)
                    return;

				NetworkLocking.Instance.RouteRemove(m_address, "255.255.255.255");
                m_address = "";
            }
        }
    }
      
    public class NetworkLocking
    {
        public static NetworkLocking Instance = new NetworkLocking();

		private string DefaultGateway = "";

        private Dictionary<string ,RouteEntry> EntryRemoved = new Dictionary<string,RouteEntry>();
        private Dictionary<string ,RouteEntry> EntryAdded = new Dictionary<string,RouteEntry>();

		public static bool IsIP(string v)
        {
            if (v == "On-link")
                return true;
            if (Utils.IsIP(v))
                return true;
            return false;
        }

        public static string Key(string address, string mask)
        {
            return address + "-" + mask;
        }

		public NetworkLocking()
		{
			Instance = this;
		}

        public bool GetEnabled()
        {
            return (DefaultGateway != "");
        }

        public bool Enable()
        {
            lock(this)
            {
                if (GetEnabled() == true)
                    return true;

                bool Failed = false;

                List<RouteEntry> EntryList = GetRouteList();

                DefaultGateway = "";
                EntryRemoved.Clear();
                EntryAdded.Clear();

                foreach (RouteEntry Entry in EntryList)
                {
                    if ((DefaultGateway != "") && (IsIP(Entry.Gateway)) && (DefaultGateway != Entry.Gateway))
                    {
                        Failed = true;
                        break;
                    }

                    if ((DefaultGateway == "") && (IsIP(Entry.Gateway)))
                        DefaultGateway = Entry.Gateway;
                }

                if (DefaultGateway == "")
                    Failed = true;

                if (Failed)
                {
					DefaultGateway = "";
                    Engine.Instance.Log(Engine.LogType.Warning, "Unable to activate locked network. Logging routes.");
                    foreach (RouteEntry Entry in EntryList)
                    {
                        Engine.Instance.Log(Engine.LogType.Verbose, String.Format("Address: {0}, Mask: {1}, Gateway: {2}, Interface: {3}, Metrics: {4}", Entry.Address, Entry.Mask, Entry.Gateway, Entry.Interface, Entry.Metrics));
                    }
                    return false;
                }
                else
                {
					Engine.Instance.Log(Engine.LogType.Verbose, "Locked network activated. Default gateway: " + DefaultGateway);
                }

                foreach (RouteEntry Entry in EntryList)
                {
                    if (IsIP(Entry.Gateway))
                    {
                        EntryRemoved[Entry.Key] = Entry;
                        Entry.Remove();
                    }
                }

				Recovery.Save();

                return true;
            }
        }

        public void Disable()
        {
            lock (this)
            {
                if (GetEnabled() == false)
                    return;

                foreach (RouteEntry Entry in EntryAdded.Values)
                    Entry.Remove();

                foreach (RouteEntry Entry in EntryRemoved.Values)
                    Entry.Add();

                DefaultGateway = "";

				Engine.Instance.Log(Engine.LogType.Verbose, "Locked network de-activated.");
            }
        }

		public void OnRecoveryLoad(XmlElement root)
		{
			try
			{
				XmlElement node = Utils.XmlGetFirstElementByTagName(root, "NetworkLocking");
				if (node != null)
				{
					DefaultGateway = node.GetAttribute("gateway");

					XmlElement nodeAdded = node.GetElementsByTagName("added")[0] as XmlElement;
					foreach (XmlElement nodeEntry in nodeAdded.ChildNodes)
					{
						RouteEntry entry = new RouteEntry();
						entry.ReadXML(nodeEntry);
						EntryAdded[entry.Key] = entry;
					}

					XmlElement nodeRemoved = node.GetElementsByTagName("removed")[0] as XmlElement;
					foreach (XmlElement nodeEntry in nodeAdded.ChildNodes)
					{
						RouteEntry entry = new RouteEntry();
						entry.ReadXML(nodeEntry);
						EntryRemoved[entry.Key] = entry;
					}

					Disable();
				}
			}
			catch (Exception e)
			{
				Engine.Instance.Log(e);
			}
		}

		public void OnRecoverySave(XmlElement root)
		{
			if (GetEnabled() == false)
				return;

			try
			{
				if ((EntryAdded.Count != 0) || (EntryRemoved.Count != 0))
				{
					XmlDocument doc = root.OwnerDocument;
					XmlElement el = (XmlElement)root.AppendChild(doc.CreateElement("NetworkLocking"));
					el.SetAttribute("gateway", DefaultGateway);

					XmlElement nodeAdded = el.AppendChild(doc.CreateElement("added")) as XmlElement;
					foreach (RouteEntry entry in EntryAdded.Values)
					{
						XmlElement nodeEntry = nodeAdded.AppendChild(doc.CreateElement("entry")) as XmlElement;
						entry.WriteXML(nodeEntry);
					}

					XmlElement nodeRemoved = el.AppendChild(doc.CreateElement("removed")) as XmlElement;
					foreach (RouteEntry entry in EntryRemoved.Values)
					{
						XmlElement nodeEntry = nodeRemoved.AppendChild(doc.CreateElement("entry")) as XmlElement;
						entry.WriteXML(nodeEntry);
					}
				}
			}
			catch (Exception e)
			{
				Engine.Instance.Log(e);
			}
		}

                
        private List<RouteEntry> GetRouteList()
        {
            List<RouteEntry> EntryList = new List<RouteEntry>();

			string Result = Platform.Instance.RouteList();
			string[] Lines = Result.Split('\n');
			foreach (string Line in Lines)
            {
				string[] Fields = Utils.StringCleanSpace(Line).Split(' ');

                if ((Fields.Length == 5) &&
                    (IsIP(Fields[0])) &&
                    (IsIP(Fields[1])) &&
                    (IsIP(Fields[2])) &&
                    (IsIP(Fields[3])))
                {
                    // Route line.
                    RouteEntry E = new RouteEntry();
                    E.Address = Fields[0];
                    E.Mask = Fields[1];
                    E.Gateway = Fields[2];
                    E.Interface = Fields[3];
                    E.Metrics = Fields[4];

                    if(E.Gateway != "On-link")
                        EntryList.Add(E);
                }
            }

            return EntryList;
        }

		public void RouteAdd(string address, string mask)
        {
			lock (this)
            {				
                if (GetEnabled() == false)
                    return;

				string key = Key(address, mask);
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
                    entry.Gateway = DefaultGateway;
                    EntryAdded[key] = entry;
                    entry.Add();
                }

				Recovery.Save();
            }
			Engine.Instance.LogDebug("RouteAdd out");
        }

		public void RouteRemove(string address, string mask)
        {
			Engine.Instance.LogDebug("RouteRemove in");
            lock (this)
            {
                if (GetEnabled() == false)
                    return;

				string key = Key(address, mask);
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
			Engine.Instance.LogDebug("RouteRemove out");
        }

    }
}
