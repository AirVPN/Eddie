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
	public class RouteScope
	{
		private string m_guid;
		private IpAddress m_address;

		public RouteScope(IpAddress address)
		{
			Start(address);
		}

		~RouteScope()
		{
			End();
		}

		public string Guid
		{
			get
			{
				if (m_guid == "")
					m_guid = RandomGenerator.GetHash();
				return m_guid;
			}
		}

		public void Start(IpAddress address)
		{
			if (address.Valid)
			{
				m_address = address;
				if (Engine.Instance.NetworkLockManager != null)
					Engine.Instance.NetworkLockManager.AllowIP(m_address);
			}
			else
			{
			}
		}

		public void End()
		{
			if ((m_address != null) && (m_address.Valid)) // Only one time			
			{
				if (Engine.Instance.NetworkLockManager != null)
					Engine.Instance.NetworkLockManager.DeallowIP(m_address);
				m_address.Clear();
			}
		}
	}
}
