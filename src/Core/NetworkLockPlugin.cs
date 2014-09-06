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

		public virtual bool IsDnsResolutionAvailable(string host)
		{
			return false;
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

		public List<IpAddress> GetAllIps()
		{
			List<IpAddress> result = new List<IpAddress>();

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
					
					IpAddress ip3 = new IpAddress(ip2);
					if (ip3.Valid == false)
					{
						Engine.Instance.Log(Engine.LogType.Error, Messages.Format(Messages.NetworkLockAllowedIpInvalid, ip2));
						continue;
					}

					if (result.Contains(ip3))
					{
						Engine.Instance.Log(Engine.LogType.Warning, Messages.Format(Messages.NetworkLockAllowedIpDuplicated, ip2));
						continue;
					}

					result.Add(ip3);
				}
			}

			// Hosts
			if (Engine.Instance.Storage.Manifest != null)
			{
				XmlNodeList nodesHosts = Engine.Instance.Storage.Manifest.SelectNodes("//hosts/host");
				foreach (XmlNode nodeHost in nodesHosts)
				{
					IpAddress ip = new IpAddress(nodeHost.Attributes["address"].Value);
					if ((ip.Valid) && (result.Contains(ip) == false))
						result.Add(ip);
				}
			}

			// Servers
			lock (Engine.Instance.Servers)
			{
				Dictionary<string, ServerInfo> servers = new Dictionary<string, ServerInfo>(Engine.Instance.Servers);

				foreach (ServerInfo infoServer in servers.Values)
				{
					if (result.Contains(infoServer.IpEntry) == false)
						result.Add(infoServer.IpEntry);
					if (infoServer.IpEntry2.Trim() != "")
						if (result.Contains(infoServer.IpEntry2) == false)
							result.Add(infoServer.IpEntry2);
				}
			}

			return result;
		}
	}
}
