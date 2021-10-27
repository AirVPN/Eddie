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
using System.Net.NetworkInformation;
using System.Text;
using System.Xml;

namespace Eddie.Core
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

		public virtual string GetDescription()
		{
			return "";
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

		/* // Not used
		public virtual void AllowIP(IpAddress ip)
		{
		}

		public virtual void DeallowIP(IpAddress ip)
		{
		}
		*/

		public virtual void AllowProgram(string path)
		{
		}

		public virtual void DeallowProgram(string path)
		{
		}

		public virtual void AllowInterface(NetworkInterface networkInterface)
		{
		}

		public virtual void DeallowInterface(NetworkInterface networkInterface)
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

		public string GetTitleForList()
		{
			string title = GetName();
			if (GetDescription() != "")
				title += " (" + GetDescription() + ")";
			return title;
		}

		public IpAddresses GetIpsAllowlistIncoming()
		{
			IpAddresses result = new IpAddresses();

			// Allowlist
			{
				string list = Engine.Instance.Options.Get("netlock.allowlist.incoming.ips");
				list = list.Replace("\u2028", ","); // macOS Hack  // TOCLEAN
				List<string> hosts = list.StringToList();
				foreach (string host in hosts)
				{
					string host2 = host;
					int posComment = host2.IndexOf("#");
					if (posComment != -1)
						host2 = host2.Substring(0, posComment).Trim();

					result.Add(host2);
				}
			}

			return result;
		}

		public IpAddresses GetIpsAllowlistOutgoing(bool includeIpUsedByClient)
		{
			IpAddresses result = new IpAddresses();

			// Allowlist
			{
				string list = Engine.Instance.Options.Get("netlock.allowlist.outgoing.ips");
				list = list.Replace("\u2028", ","); // macOS Hack  // TOCLEAN
				List<string> hosts = list.StringToList();
				foreach (string host in hosts)
				{
					string host2 = host;
					int posComment = host2.IndexOf("#");
					if (posComment != -1)
						host2 = host2.Substring(0, posComment).Trim();

					result.Add(host2);
				}
			}

			// Routes Out
			{
				string routes = Engine.Instance.Options.Get("routes.custom");
				string[] routes2 = routes.Split(';');
				foreach (string route in routes2)
				{
					string[] routeEntries = route.Split(',');
					if (routeEntries.Length < 2)
						continue;

					string host = routeEntries[0];
					string action = routeEntries[1];

					if (action == "out")
					{
						result.Add(host);
					}
				}
			}

			// DNS
			if (Engine.Instance.Options.GetBool("netlock.allow_dns"))
				result.Add(Platform.Instance.DetectDNS());

			if (includeIpUsedByClient)
			{
				// Providers
				foreach (Providers.IProvider provider in Engine.Instance.ProvidersManager.Providers)
				{
					result.Add(provider.GetNetworkLockAllowlistOutgoingIPs());
				}

				// Servers
				lock (Engine.Instance.Connections)
				{
					Dictionary<string, ConnectionInfo> servers = new Dictionary<string, ConnectionInfo>(Engine.Instance.Connections);

					foreach (ConnectionInfo infoServer in servers.Values)
					{
						result.Add(infoServer.IpsEntry);
					}
				}
			}

			return result;
		}
	}
}
