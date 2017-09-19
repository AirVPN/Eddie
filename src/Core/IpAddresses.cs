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

		public IpAddresses(string[] values)
		{
			Add(string.Join(",", values));
		}

		public bool Contains(IpAddress ip)
		{
			return (IPs.Contains(ip));
		}

		public bool ContainsAddress(IpAddress ip)
		{
			foreach (IpAddress ipc in IPs)
				if (ipc.Address == ip.Address)
					return true;
			return false;
		}

		public void Set(string v)
		{
			IPs.Clear();
			Add(v);
		}

		public void Add(string v)
		{
			v = v.Replace(",", "\n");
			string[] lines = v.Split('\n');
			foreach(string line in lines)
			{
				if (line.Trim() != "")
				{
					IpAddress ip = new IpAddress(line.Trim());
					if (ip.Valid)
					{
						Add(ip);
					}
					else
					{
						// Resolve
						Add(DnsManager.ResolveDNS(line.Trim()));
					}
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

		public int CountIPv4
		{
			get
			{
				int n = 0;
				lock(IPs)
				{
					foreach (IpAddress ip in IPs)
						if (ip.IsV4)
							n++;
				}
				return n;
			}
		}

		public int CountIPv6
		{
			get
			{
				int n = 0;
				lock (IPs)
				{
					foreach (IpAddress ip in IPs)
						if (ip.IsV6)
							n++;
				}
				return n;
			}
		}

		public IpAddresses OnlyIPv4
		{
			get
			{
				IpAddresses r = new IpAddresses();
				lock (IPs)
				{
					foreach (IpAddress ip in IPs)
						if (ip.IsV4)
							r.Add(ip);
				}
				return r;
			}
		}

		public IpAddresses OnlyIPv6
		{
			get
			{
				IpAddresses r = new IpAddresses();
				lock (IPs)
				{
					foreach (IpAddress ip in IPs)
						if (ip.IsV6)
							r.Add(ip);
				}
				return r;
			}
		}

		public IpAddress First
		{
			get
			{
				if (Count == 0)
					return null;
				return IPs[0];
			}
		}
		
		public void Clear()
		{
			lock (IPs)
			{
				IPs.Clear();
			}
		}

		public IpAddress GetV4ByIndex(int index)
		{
			lock (IPs)
			{
				int i = -1;
				foreach (IpAddress ip in IPs)
				{
					if(ip.IsV4)
					{
						i++;
						if (i == index)
							return ip;
					}
				}
				return null;
			}
		}

		public IpAddress GetV6ByIndex(int index)
		{
			lock (IPs)
			{
				int i = -1;
				foreach (IpAddress ip in IPs)
				{
					if (ip.IsV6)
					{
						i++;
						if (i == index)
							return ip;
					}
				}
				return null;
			}
		}

		public string ToStringFirstIPv4() // TOCLEAN
		{
			foreach (IpAddress ip in IPs)
			{
				if (ip.IsV4)
					return ip.Address;
			}
			return "";
		}		

		public string Addresses
		{
			get
			{
				string result = "";
				foreach (IpAddress ip in IPs)
				{
					if (result != "")
						result += ", ";
					result += ip.Address;
				}
				return result;
			}
		}

		public string[] AddressesToStringArray()
		{
			List<string> result = new List<string>();
			foreach (IpAddress ip in IPs)
				result.Add(ip.Address);
			return result.ToArray();
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			IpAddresses two = obj as IpAddresses;
			if (two == null)
				return false;
						
			// Works because ToString sort items.
			return ToString() == two.ToString();
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		public override string ToString()
		{
			List<string> items = new List<string>();
			foreach (IpAddress ip in IPs)
				items.Add(ip.ToCIDR());
			items.Sort();
			return string.Join(", ", items.ToArray());
		}
	}
}
