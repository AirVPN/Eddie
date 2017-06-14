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
	public class IpAddresses
	{
		public List<IpAddress> IPs = new List<IpAddress>();

		public IpAddresses()
		{

		}

		public IpAddresses(string value)
		{
			Add(value);
		}

		public bool Contains(IpAddress ip)
		{
			return (IPs.Contains(ip));
		}

		public void Add(string v)
		{
			if (v.Trim() != "")
			{
				IpAddress ip = new IpAddress(v);
				if (ip.Valid) // ClodoTemp
				{
					Add(ip);
				}
				else
				{
					// Try resolve
					Add(Platform.Instance.ResolveDNS(v));
				}
			}
		}

		public void Add(IpAddresses addresses)
		{
			lock (addresses)
			{
				foreach (IpAddress ip in addresses.IPs)
					Add(ip);
			}
		}

		public void Add(IpAddress ip)
		{
			if (Contains(ip) == false)
			{
				lock (IPs)
				{
					IPs.Add(ip);
				}
			}
		}

		public int Count
		{
			get
			{
				return IPs.Count;
			}
		}

		public void Clear()
		{
			lock (IPs)
			{
				IPs.Clear();
			}
		}

		public override string ToString()
		{
			string result = "";
			foreach (IpAddress ip in IPs)
			{
				if (result != "")
					result += ",";
				result += ip.ToCIDR();
			}
			return result;
		}
	}
}
