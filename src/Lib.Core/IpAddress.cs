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
using Eddie.Common;

namespace Eddie.Core
{
	public class IpAddress
	{
		public static IpAddress DefaultIPv4 = new IpAddress("0.0.0.0/0");
		public static IpAddress DefaultIPv6 = new IpAddress("::/0");
		private System.Net.IPAddress m_ip;
		private int m_bitmask = -1;

		public static bool IsIP(string ip)
		{
			IpAddress v = new IpAddress(ip);
			return (v.Valid);
		}

		public IpAddress()
		{
		}

		public IpAddress(IpAddress value)
		{
			m_ip = value.m_ip;
			m_bitmask = value.m_bitmask;
		}

		public IpAddress(string value)
		{
			Parse(value);
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
				if (m_ip != null)
					return m_ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
				else
					return false;
			}
		}

		public bool IsV6
		{
			get
			{
				if (m_ip != null)
					return m_ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6;
				else
					return false;
			}
		}

		public bool IsInAddrAny
		{
			get
			{
				string cidr = ToCIDR();
				if (cidr == DefaultIPv4.ToCIDR())
					return true;
				if (cidr == DefaultIPv6.ToCIDR())
					return true;

				if (Address == DefaultIPv4.Address)
					return true;
				if (Address == DefaultIPv6.Address)
					return true;

				return false;
			}
		}

		public void Parse(string value)
		{
			m_ip = null;
			m_bitmask = -1;

			value = value.Trim().Replace(" ", "/");

			// Clean if there is interface, ex. fe80::21d:aaff:fef3:eb8%en0 . For example netstat output under macOs
			if (value.IndexOf("%") != -1)
				value = value.Substring(0, value.IndexOf("%"));

			string ip = "";
			string mask = "";
			int posS = value.IndexOf('/');
			if (posS == -1)
			{
				ip = value.Trim();
				mask = "";
			}
			else
			{
				ip = value.Substring(0, posS).Trim();
				mask = value.Substring(posS + 1).Trim();
			}

			if (System.Net.IPAddress.TryParse(ip.ToLowerInvariant().Trim(), out m_ip) == false)
				m_ip = null;

			// Only IPv4 or IPv6 address
			if (m_ip != null)
			{
				if ((m_ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) &&
					(m_ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6))
					m_ip = null;
			}

			// Parse Mask
			if (m_ip != null)
			{
				if (mask == "")
				{
					if (m_ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
						m_bitmask = 32;
					else if (m_ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
						m_bitmask = 128;
				}
				else
				{
					int iMask = Conversions.ToInt32(mask);
					if (iMask.ToString() == mask)
						m_bitmask = iMask;
					else if (m_ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) // IPv4 only
					{
						iMask = NetMask2bitMaskV4(mask);
						if (iMask != -1)
							m_bitmask = iMask;
					}
				}
			}

			if (m_bitmask == -1)
			{
				m_ip = null;
				m_bitmask = -1;
			}
		}

		public string ToCIDR()
		{
			return ToCIDR(false);
		}

		public string ToCIDR(bool alwaysWithPrefix)
		{
			if (Valid == false)
				return "";

			if ((alwaysWithPrefix == false) && (IsV4) && (m_bitmask == 32))
				return m_ip.ToString();
			else if ((alwaysWithPrefix == false) && (IsV6) && (m_bitmask == 128))
				return m_ip.ToString();
			else
				return m_ip.ToString() + "/" + m_bitmask.ToString();
		}

		public string ToOpenVPN()
		{
			if (Valid == false)
				return "";

			if (IsV4)
				return m_ip.ToString() + " " + BitMask2netMaskV4(m_bitmask);
			else if (IsV6)
				return ToCIDR();
			else
				return "";
		}

		public string Address
		{
			get
			{
				return m_ip.ToString();
			}
		}

		public string Mask
		{
			get
			{
				if (IsV4)
					return BitMask2netMaskV4(m_bitmask);
				else
					return m_bitmask.ToString();
			}
		}

		public void Clear()
		{
			m_ip = null;
			m_bitmask = 0;
		}

		public IpAddress Clone()
		{
			return new IpAddress(this);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			if (obj is IpAddress == false)
				return false;

			IpAddress two = obj as IpAddress;			

			return ToCIDR() == two.ToCIDR();
		}
		/*
		public static bool operator ==(IpAddress i1, IpAddress i2)
		{
			if (i1 == null)
				return (i2 == null);
			if (i1 is null)
				return (i2 is null);
			return i1.Equals(i2);
		}

		public static bool operator !=(IpAddress i1, IpAddress i2)
		{
			return !(i1 == i2);
		}
		*/
		public override int GetHashCode()
		{
			return ToCIDR().GetHashCode();
		}

		public override string ToString()
		{
			return ToCIDR();
		}

		private int NetMask2bitMaskV4(string ip)
		{
			if (ip == "0.0.0.0")
				return 0;
			else if (ip == "128.0.0.0")
				return 1;
			else if (ip == "192.0.0.0")
				return 2;
			else if (ip == "224.0.0.0")
				return 3;
			else if (ip == "240.0.0.0")
				return 4;
			else if (ip == "248.0.0.0")
				return 5;
			else if (ip == "252.0.0.0")
				return 6;
			else if (ip == "254.0.0.0")
				return 7;
			else if (ip == "255.0.0.0")
				return 8;
			else if (ip == "255.128.0.0")
				return 9;
			else if (ip == "255.192.0.0")
				return 10;
			else if (ip == "255.224.0.0")
				return 11;
			else if (ip == "255.240.0.0")
				return 12;
			else if (ip == "255.248.0.0")
				return 13;
			else if (ip == "255.252.0.0")
				return 14;
			else if (ip == "255.254.0.0")
				return 15;
			else if (ip == "255.255.0.0")
				return 16;
			else if (ip == "255.255.128.0")
				return 17;
			else if (ip == "255.255.192.0")
				return 18;
			else if (ip == "255.255.224.0")
				return 19;
			else if (ip == "255.255.240.0")
				return 20;
			else if (ip == "255.255.248.0")
				return 21;
			else if (ip == "255.255.252.0")
				return 22;
			else if (ip == "255.255.254.0")
				return 23;
			else if (ip == "255.255.255.0")
				return 24;
			else if (ip == "255.255.255.128")
				return 25;
			else if (ip == "255.255.255.192")
				return 26;
			else if (ip == "255.255.255.224")
				return 27;
			else if (ip == "255.255.255.240")
				return 28;
			else if (ip == "255.255.255.248")
				return 29;
			else if (ip == "255.255.255.252")
				return 30;
			else if (ip == "255.255.255.254")
				return 31;
			else if (ip == "255.255.255.255")
				return 32;
			else
				return -1;
		}

		private string BitMask2netMaskV4(int bit)
		{
			if (bit == 0)
				return "0.0.0.0";
			else if (bit == 1)
				return "128.0.0.0";
			else if (bit == 2)
				return "192.0.0.0";
			else if (bit == 3)
				return "224.0.0.0";
			else if (bit == 4)
				return "240.0.0.0";
			else if (bit == 5)
				return "248.0.0.0";
			else if (bit == 6)
				return "252.0.0.0";
			else if (bit == 7)
				return "254.0.0.0";
			else if (bit == 8)
				return "255.0.0.0";
			else if (bit == 9)
				return "255.128.0.0";
			else if (bit == 10)
				return "255.192.0.0";
			else if (bit == 11)
				return "255.224.0.0";
			else if (bit == 12)
				return "255.240.0.0";
			else if (bit == 13)
				return "255.248.0.0";
			else if (bit == 14)
				return "255.252.0.0";
			else if (bit == 15)
				return "255.254.0.0";
			else if (bit == 16)
				return "255.255.0.0";
			else if (bit == 17)
				return "255.255.128.0";
			else if (bit == 18)
				return "255.255.192.0";
			else if (bit == 19)
				return "255.255.224.0";
			else if (bit == 20)
				return "255.255.240.0";
			else if (bit == 21)
				return "255.255.248.0";
			else if (bit == 22)
				return "255.255.252.0";
			else if (bit == 23)
				return "255.255.254.0";
			else if (bit == 24)
				return "255.255.255.0";
			else if (bit == 25)
				return "255.255.255.128";
			else if (bit == 26)
				return "255.255.255.192";
			else if (bit == 27)
				return "255.255.255.224";
			else if (bit == 28)
				return "255.255.255.240";
			else if (bit == 29)
				return "255.255.255.248";
			else if (bit == 30)
				return "255.255.255.252";
			else if (bit == 31)
				return "255.255.255.254";
			else if (bit == 32)
				return "255.255.255.255";
			else
				return "";
		}
	}
}
