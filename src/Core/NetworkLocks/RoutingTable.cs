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

/*
 * Known limits:
 * 
 * - Set all profiles firewallpolicy
 * - Lock only outgoing packets
 * - Already exists outgoing rules are not disabled
*/

namespace AirVPN.Core.NetworkLocks
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

		public override void Activation()
		{
			base.Activation();

			RoutesManager.Instance.LockActivate();
		}

		public override void Deactivation()
		{
			base.Deactivation();

			RoutesManager.Instance.LockDeactivate();
		}

		public override void AllowIP(IpAddress ip)
		{
			base.AllowIP(ip);

			RoutesManager.Instance.RouteAdd(ip);
		}

		public override void DeallowIP(IpAddress ip)
		{
			base.DeallowIP(ip);

			RoutesManager.Instance.RouteRemove(ip);
		}
	}
}
