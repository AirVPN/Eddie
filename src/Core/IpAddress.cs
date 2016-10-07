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
using System.Net;
using System.Text;
using System.Xml;

namespace Eddie.Core
{
	public class IpAddress
	{
		private IPAddress m_ip;

        public static bool IsIP(string ip)
        {
            IPAddress ip2;
            bool valid = IPAddress.TryParse(ip, out ip2);
            return valid;
        }

		public IpAddress()
		{
			m_ip = null;
        }

		public IpAddress(string value)
		{
			if (IPAddress.TryParse(value.ToLowerInvariant().Trim(), out m_ip) == false)
                m_ip = null;

            // Only IPv4 or IPv6 address
            if (m_ip != null)
            {
                if ((m_ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) &&
                    (m_ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6))
                    m_ip = null;
            }
        }

		public static implicit operator IpAddress(string value)
		{
			return new IpAddress(value);
		}
        
		public bool Valid
		{
			get
			{
                return m_ip != null;
			}
		}

        public bool IsV4
        {
            get
            {
                if (Valid == false)
                    return false;
                
                return m_ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
            }
        }

        public bool IsV6
        {
            get
            {
                if (Valid == false)
                    return false;

                return m_ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6;
            }
        }

		public void Clear()
		{
			m_ip = null;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			IpAddress two = obj as IpAddress;
			if (two == null)
				return false;

			return ToString() == two.ToString();
		}

		public override int GetHashCode()
		{
			return m_ip.ToString().GetHashCode();
		}

		public override string ToString()
		{
			return m_ip.ToString();
		}

        public string Value
        {
            get
            {
                if (Valid == false)
                    return "";
                else
                    return m_ip.ToString();
            }
        }
	}
}
