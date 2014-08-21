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
    
      
    public class RoutesManager
    {
        public static RoutesManager Instance = new RoutesManager();

		private IpAddress DefaultGateway = new IpAddress();
		private string DefaultInterface = "";

        private Dictionary<string ,RouteEntry> EntryRemoved = new Dictionary<string,RouteEntry>();
        private Dictionary<string ,RouteEntry> EntryAdded = new Dictionary<string,RouteEntry>();

		public static bool IsIP(string v) // TOCLEAN: where it's used and why
		{
			if (v == "On-link")
				return true;
			if (Utils.IsIP(v))
				return true;
			return false;
		}

		public static bool IsIP(IpAddress v)
		{
			return IsIP(v.Value);
		}

		public static string Key(IpAddress address, IpAddress mask)
        {
            return address.Value + "-" + mask.Value;
        }

		public RoutesManager()
		{
			Instance = this;
		}

        public bool GetLockActive()
        {
            return (DefaultGateway.Empty == false);
        }

        public bool LockActivate()
        {
            lock(this)
            {
				if (GetLockActive() == true)
                    return true;

                bool Failed = false;

                List<RouteEntry> EntryList = Platform.Instance.RouteList();

                DefaultGateway = "";
				DefaultInterface = "";
                EntryRemoved.Clear();
                EntryAdded.Clear();

                foreach (RouteEntry Entry in EntryList)
                {
					if (IsIP(Entry.Gateway))
					{
						if (DefaultGateway.Empty)
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

                if (DefaultGateway.Empty)
                    Failed = true;

				if (Failed)
                {
					DefaultGateway = "";
					DefaultInterface = "";
                    Engine.Instance.Log(Engine.LogType.Fatal, Messages.NetworkLockActivationFailed);
                    foreach (RouteEntry Entry in EntryList)
                    {
						Engine.Instance.Log(Engine.LogType.Verbose, Entry.ToString());
                    }

					Engine.Instance.Storage.SetBool("advanced.netlock.active", false);

                    return false;
                }
                else
                {
					Engine.Instance.Log(Engine.LogType.Info, Messages.Format(Messages.NetworkLockActivationSuccess, DefaultGateway.Value));
                }

				// pazzo
				/*
                foreach (RouteEntry Entry in EntryList)
                {
                    if (IsIP(Entry.Gateway))
                    {
                        EntryRemoved[Entry.Key] = Entry;
						Engine.Instance.Log(Engine.LogType.Verbose, Messages.Format(Messages.NetworkLockRouteRemoved, Entry.ToString()));
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
				
				Recovery.Save();

                return true;
            }
        }

        public void LockDeactivate()
        {
            lock (this)
            {
				if (GetLockActive() == false)
                    return;

                foreach (RouteEntry Entry in EntryAdded.Values)
                    Entry.Remove();

				foreach (RouteEntry Entry in EntryRemoved.Values)
				{
					Entry.Add();
					Engine.Instance.Log(Engine.LogType.Verbose, Messages.Format(Messages.NetworkLockRouteRestored, Entry.ToString()));
				}

                DefaultGateway = "";
				DefaultInterface = "";

				EntryAdded.Clear();
				EntryRemoved.Clear();

				Recovery.Save();

				Engine.Instance.Log(Engine.LogType.Info, Messages.NetworkLockDeactivationSuccess);
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
					DefaultInterface = node.GetAttribute("interface");

					XmlElement nodeAdded = node.GetElementsByTagName("added")[0] as XmlElement;
					foreach (XmlElement nodeEntry in nodeAdded.ChildNodes)
					{
						RouteEntry entry = new RouteEntry();
						entry.ReadXML(nodeEntry);
						EntryAdded[entry.Key] = entry;
					}

					XmlElement nodeRemoved = node.GetElementsByTagName("removed")[0] as XmlElement;
					foreach (XmlElement nodeEntry in nodeRemoved.ChildNodes)
					{
						RouteEntry entry = new RouteEntry();
						entry.ReadXML(nodeEntry);
						EntryRemoved[entry.Key] = entry;
					}

					LockDeactivate();
				}
			}
			catch (Exception e)
			{
				Engine.Instance.Log(e);
			}
		}

		public void OnRecoverySave(XmlElement root)
		{
			if (GetLockActive() == false)
				return;

			try
			{
				if ((EntryAdded.Count != 0) || (EntryRemoved.Count != 0))
				{
					XmlDocument doc = root.OwnerDocument;
					XmlElement el = (XmlElement)root.AppendChild(doc.CreateElement("NetworkLocking"));
					el.SetAttribute("gateway", DefaultGateway.Value);
					el.SetAttribute("interface", DefaultInterface);

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


		public void RouteAdd(IpAddress address, IpAddress mask)
		{
			RouteAdd(address, mask, DefaultGateway, DefaultInterface);
		}

		public void RouteAdd(IpAddress address, IpAddress mask, IpAddress gateway, string iface)
        {
			lock (this)
            {
				if (GetLockActive() == false)
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
                    entry.Gateway = gateway;
					entry.Interface = iface;
                    EntryAdded[key] = entry;
                    entry.Add();
                }

				Recovery.Save();
            }			
        }

		public void RouteRemove(IpAddress address, IpAddress mask)
        {
			lock (this)
            {
				if (GetLockActive() == false)
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
        }

    }
}
