// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2023 AirVPN (support@airvpn.org) / https://airvpn.org
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

using System.Collections.Generic;

namespace Eddie.Core
{
	public class IpAddresses
	{
		private List<IpAddress> m_list = new List<IpAddress>(); // For ordered list
		private HashSet<IpAddress> m_hashset = new HashSet<IpAddress>(); // For quick check of Contains

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

		public List<IpAddress> IPs
		{
			get
			{
				lock (m_list)
				{
					return new List<IpAddress>(m_list);
				}
			}
		}

		public bool Contains(IpAddress ip)
		{
			return (m_hashset.Contains(ip));
		}

		public bool ContainsAddress(IpAddress ip)
		{
			lock (m_list)
			{
				foreach (IpAddress ipc in m_list)
					if (ipc.Address == ip.Address)
						return true;
			}
			return false;
		}

		public void Set(string v)
		{
			Clear();
			Add(v);
		}

		public void Add(string v)
		{
			if (v == null)
				return;

			v = v.Replace(",", "\n");
			string[] lines = v.Split('\n');
			foreach (string line in lines)
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
			foreach (IpAddress ip in addresses.IPs)
				Add(ip);
		}

		public void Add(IpAddress ip)
		{
			if (Contains(ip) == false)
			{
				lock (m_list)
				{
					m_list.Add(ip);
					m_hashset.Add(ip);
				}
			}
		}

		public void Remove(IpAddress ip)
		{
			if (Contains(ip))
			{
				lock (m_list)
				{
					m_list.Remove(ip);
					m_hashset.Remove(ip);
				}
			}
		}

		public int Count
		{
			get
			{
				return m_list.Count;
			}
		}

		public int CountIPv4
		{
			get
			{
				int n = 0;
				lock (m_list)
				{
					foreach (IpAddress ip in m_list)
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
				lock (m_list)
				{
					foreach (IpAddress ip in m_list)
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
				lock (m_list)
				{
					foreach (IpAddress ip in m_list)
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
				lock (m_list)
				{
					foreach (IpAddress ip in m_list)
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
				return m_list[0];
			}
		}

		public IpAddress FirstPreferIPv4
		{
			get
			{
				IpAddresses list = OnlyIPv4;
				if (list.Count > 0)
					return list.First;
				list = OnlyIPv6;
				if (list.Count > 0)
					return list.First;
				return null;
			}
		}

		public void Clear()
		{
			lock (m_list)
			{
				m_list.Clear();
				m_hashset.Clear();
			}
		}

		public IpAddresses Clone()
		{
			IpAddresses n = new IpAddresses();
			foreach (IpAddress ip in m_list)
				n.Add(ip.Clone());
			return n;
		}

		public IpAddress GetV4ByIndex(int index)
		{
			lock (m_list)
			{
				int i = -1;
				foreach (IpAddress ip in m_list)
				{
					if (ip.IsV4)
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
			lock (m_list)
			{
				int i = -1;
				foreach (IpAddress ip in m_list)
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

		public string Addresses
		{
			get
			{
				string result = "";
				lock (m_list)
				{
					foreach (IpAddress ip in m_list)
					{
						if (result != "")
							result += ", ";
						result += ip.Address;
					}
				}
				return result;
			}
		}

		public string[] AddressesToStringArray()
		{
			List<string> result = new List<string>();
			lock (m_list)
			{
				foreach (IpAddress ip in m_list)
					result.Add(ip.Address);
			}
			return result.ToArray();
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			if (obj is IpAddresses == false)
				return false;

			IpAddresses two = obj as IpAddresses;

			// Note: return false if contains the same IPs but in different order // 2.19.5
			return ToString() == two.ToString();
		}
		/*
		public static bool operator ==(IpAddresses i1, IpAddresses i2)
		{
			if (i1 is null)
				return (i2 is null);
			return i1.Equals(i2);
		}

		public static bool operator !=(IpAddresses i1, IpAddresses i2)
		{
			return !(i1 == i2);
		}
		*/
		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		public override string ToString()
		{
			List<string> items = new List<string>();
			lock (m_list)
			{
				foreach (IpAddress ip in m_list)
					items.Add(ip.ToCIDR());
			}
			// items.Sort(); // Removed in 2.19.5, otherwise restore DNS in wrong order
			return string.Join(", ", items.ToArray());
		}
	}
}
