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
using System.Text;
using System.Xml;

namespace AirVPN.Core
{
	public class NetworkLockPlugin
	{
		public virtual string GetCode()
		{
			return "Interface class";
		}

		public virtual string GetName()
		{
			return "Interface class";
		}

		public virtual bool GetSupport()
		{
			return true;
		}

		public virtual bool IsDnsResolutionAvailable(string host)
		{
			return false;
		}

		public virtual void Init()
		{
		}

		public virtual void Activation()
		{
		}

		public virtual void Deactivation()
		{
		}

		public virtual void AllowIP(IpAddress ip)
		{
		}

		public virtual void DeallowIP(IpAddress ip)
		{
		}

		public virtual void OnUpdateIps()
		{
			// Called when Ip used by AirVPN (hosts auth or vpn servers) maybe changed.
		}

		public virtual void OnVpnEstablished()
		{
		}

		public virtual void OnVpnDisconnected()
		{
		}

		public virtual void OnRecoveryLoad(XmlElement root)
		{
		}

		public virtual void OnRecoverySave(XmlElement root)
		{
		}

		public string Exec(string cmd)
		{
			return Platform.Instance.ShellCmd(cmd);
		}

		public void AddToIpsList(List<IpAddressRange> result, IpAddressRange ip, bool warning)
		{
			if (ip.Valid == false)
			{
				if(warning == true)
					Engine.Instance.Log(Engine.LogType.Error, Messages.Format(Messages.NetworkLockAllowedIpInvalid, ip.ToString()));
				return;
			}

			if (result.Contains(ip))
			{
				if (warning == true)
					Engine.Instance.Log(Engine.LogType.Warning, Messages.Format(Messages.NetworkLockAllowedIpDuplicated, ip.ToString()));
				return;
			}

			result.Add(ip);
		}

		public List<IpAddressRange> GetAllIps()
		{
			List<IpAddressRange> result = new List<IpAddressRange>();

			// Custom
			{
				string list = Engine.Instance.Storage.Get("netlock.allowed_ips");
				string[] ips = list.Split('\n');
				foreach (string ip in ips)
				{
					string ip2 = ip.Trim();

					int posComment = ip2.IndexOf("#");
					if (posComment != -1)
						ip2 = ip2.Substring(0, posComment).Trim();

					if (ip2 == "")
						continue;

					AddToIpsList(result, new IpAddressRange(ip2), true);					
				}
			}

			// Routes Out
			{
				string routes = Engine.Instance.Storage.Get("routes.custom");
				string[] routes2 = routes.Split(';');
				foreach (string route in routes2)
				{
					string[] routeEntries = route.Split(',');
					if (routeEntries.Length < 2)
						continue;

					string ip = routeEntries[0];					
					string action = routeEntries[1];
					
					if (action == "out")
					{
						AddToIpsList(result, new IpAddressRange(ip), true);					
					}					
				}
			}

			// Hosts
			if (Engine.Instance.Storage.Manifest != null)
			{
				XmlNodeList nodesHosts = Engine.Instance.Storage.Manifest.SelectNodes("//hosts/host");
				foreach (XmlNode nodeHost in nodesHosts)
				{
					IpAddressRange ip = new IpAddressRange(nodeHost.Attributes["address"].Value);
					AddToIpsList(result, ip, false);										
				}
			}

			// Servers
			lock (Engine.Instance.Servers)
			{
				Dictionary<string, ServerInfo> servers = new Dictionary<string, ServerInfo>(Engine.Instance.Servers);

				foreach (ServerInfo infoServer in servers.Values)
				{
					AddToIpsList(result, infoServer.IpEntry, false);										
					if (infoServer.IpEntry2.Trim() != "")
						AddToIpsList(result, infoServer.IpEntry2, false);																
				}
			}

			return result;
		}
	}
}
