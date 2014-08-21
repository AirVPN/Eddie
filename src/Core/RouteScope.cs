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
	public class RouteScope
	{
		private IpAddress m_address;

		public RouteScope(IpAddress address)
		{
			Start(address, false);
		}

		public RouteScope(IpAddress address, bool force)
		{
			Start(address, force);
		}

		~RouteScope()
		{
			End();
		}

		public void Start(IpAddress address, bool force)
		{
			if ((force == false) && (RoutesManager.Instance.GetLockActive() == false))
				return;

			if (address.Valid)
			{
				m_address = address;
				RoutesManager.Instance.RouteAdd(m_address, "255.255.255.255");
			}
		}

		public void End()
		{
			if( (m_address != null) && (m_address.Valid) ) // Only one time			
			{
				if (RoutesManager.Instance.GetLockActive() == false)
					return;

				RoutesManager.Instance.RouteRemove(m_address, "255.255.255.255");
				m_address.Clear();
			}
		}
	}
}
