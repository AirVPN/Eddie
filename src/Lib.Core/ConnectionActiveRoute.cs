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
using System.Text;
using System.Xml;

namespace Eddie.Core
{
	public class ConnectionActiveRoute
	{
		public IpAddress Address;
		public string Gateway;
		public string Interface;
		public string Notes;

		public bool Add(ConnectionActive connectionActive)
		{
			Json jRoute = Compute(connectionActive);
			if (jRoute != null)
				return Platform.Instance.RouteAdd(jRoute);
			else
				return false;
		}

		public bool Remove(ConnectionActive connectionActive)
		{
			Json jRoute = Compute(connectionActive);
			if (jRoute != null)
				return Platform.Instance.RouteRemove(jRoute);
			else
				return false;
		}

		public Json Compute(ConnectionActive connectionActive)
		{
			Json jRoute = new Json();
			jRoute["address"].Value = Address.ToCIDR();

			if (Gateway == "vpn_gateway")
			{
				jRoute["interface"].Value = connectionActive.InterfaceId;
				IpAddresses vpnGateways = connectionActive.OpenVpnProfileWithPush.ExtractGateway();
				if (Address.IsV4)
				{
					if (vpnGateways.OnlyIPv4.Count == 0)
					{
						Engine.Instance.Logs.LogWarning("Unable to compute route for " + Address.ToCIDR() + ": IPv4 VPN gateway not available.");
						return null;
					}
					else
					{
						jRoute["gateway"].Value = vpnGateways.OnlyIPv4.First.Address;
					}
				}
				else if (Address.IsV6)
				{
					if (vpnGateways.OnlyIPv6.Count == 0)
					{
						Engine.Instance.Logs.LogVerbose("Unable to compute route for " + Address.ToCIDR() + ": IPv6 VPN gateway not available.");
						return null;
					}
					else
					{
						jRoute["gateway"].Value = vpnGateways.OnlyIPv6.First.Address;
					}
				}
				else
					return null;
			}
			else if(Gateway == "net_gateway")
			{
				if (Address.IsV4)
				{
					IpAddress netGateway = Engine.Instance.GetDefaultGatewayIPv4();
					if(netGateway == null)
					{
						Engine.Instance.Logs.LogWarning("Unable to compute route for " + Address.ToCIDR() + ": IPv4 Net gateway not available.");
						return null;
					}
					else
					{
						jRoute["gateway"].Value = netGateway.Address;
						jRoute["interface"].Value = Engine.Instance.GetDefaultInterfaceIPv4();
					}
				}
				else if (Address.IsV6)
				{
					IpAddress netGateway = Engine.Instance.GetDefaultGatewayIPv6();
					if (netGateway == null)
					{
						Engine.Instance.Logs.LogVerbose("Unable to compute route for " + Address.ToCIDR() + ": IPv6 Net gateway not available.");
						return null;
					}
					else
					{
						jRoute["gateway"].Value = netGateway.Address;
						jRoute["interface"].Value = Engine.Instance.GetDefaultInterfaceIPv6();
					}					
				}
				else
					return null;
			}
			else
			{
				// ClodoTemp: Unsupported on Windows for now, we need the interface.
				IpAddress ip = new IpAddress(Gateway);
				if (ip.Valid == false)
				{
					Engine.Instance.Logs.LogWarning("Gateway " + Gateway + " invalid.");
					return null;
				}
				else if ((Address.IsV4) && (ip.IsV6))
				{					
					Engine.Instance.Logs.LogWarning("Gateway " + Gateway + " is IPv6 but used for IPv4 address.");
					return null;
				}
				else if ((Address.IsV6) && (ip.IsV4))
				{
					Engine.Instance.Logs.LogWarning("Gateway " + Gateway + " is IPv4 but used for IPv6 address.");
					return null;
				}
				else
				{
					jRoute["gateway"].Value = ip.Address;
				}
			}

			return jRoute;
		}
	}	
}
