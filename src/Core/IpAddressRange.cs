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
	public class IpAddressRange
	{
		private string m_Value;
		
		private string m_IP;
		private string m_Mask;

		public IpAddressRange()
		{
			Value = "";
		}

		public IpAddressRange(string value)
		{
			Value = value.Trim();
		}

		public static implicit operator IpAddressRange(string value)
		{
			return new IpAddressRange(value);
		}

		public bool Empty
		{
			get
			{
				return (m_IP == "");
			}
		}

		public bool Valid
		{
			get
			{
				//return System.Text.RegularExpressions.Regex.IsMatch(Value, @"\b((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$\b");
				return (m_IP != "");
			}
		}

		public bool Loopback
		{
			get
			{				
				return Value.StartsWith("127.0.0.1");
			}
		}

		public string Value
		{
			get
			{
				return m_Value;
			}
			set
			{
				m_Value = value;

				// Calculate
				m_IP = "";
				m_Mask = "";
				
				int posS = value.IndexOf('/');
				if(posS == -1)
				{
					m_IP = value;
					m_Mask = "255.255.255.255";
				}
				else
				{
					m_IP = value.Substring(0,posS);
					m_Mask = value.Substring(posS+1);
				}

				bool valid = false;
				if (new IpAddress(m_IP).Valid)
				{
					// Mask is CIDR or IP?
					if (new IpAddress(m_Mask).Valid)
					{
						// Can be converted to CIDR?
						if (NetMask2bitMask(m_Mask) != "")
						{
							valid = true;
						}
					}
					else
					{
						m_Mask = BitMask2netMask(m_Mask);
						if (new IpAddress(m_Mask).Valid)
						{
							valid = true;
						}
					}
				}

				if (valid == false)
				{
					m_IP = "";
					m_Mask = "";
				}
			}
		}

		public string ToCIDR()
		{
			if(Valid == false)
				return "";

			return m_IP + "/" + NetMask2bitMask(m_Mask);
		}

		public string ToOpenVPN()
		{
			if (Valid == false)
				return "";
			
			return m_IP + " " + m_Mask;
		}

		public void Clear()
		{
			Value = "";
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			IpAddress two = obj as IpAddress;
			if (two == null)
				return false;

			return Value == two.Value;
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public override string ToString()
		{
			return Value;
		}

		private string NetMask2bitMask(string ip)
		{
			if(ip == "0.0.0.0")
				return "0";
			else if(ip == "128.0.0.0")
				return "1";
			else if(ip == "192.0.0.0")
				return "2";
			else if(ip == "224.0.0.0")
				return "3";
			else if(ip == "240.0.0.0")
				return "4";
			else if(ip == "248.0.0.0")
				return "5";
			else if(ip == "252.0.0.0")
				return "6";
			else if(ip == "254.0.0.0")
				return "7";
			else if(ip == "255.0.0.0")
				return "8";
			else if(ip == "255.128.0.0")
				return "9";
			else if(ip == "255.192.0.0")
				return "10";
			else if(ip == "255.224.0.0")
				return "11";
			else if(ip == "255.240.0.0")
				return "12";
			else if(ip == "255.248.0.0")
				return "13";
			else if(ip == "255.252.0.0")
				return "14";
			else if(ip == "255.254.0.0")
				return "15";
			else if(ip == "255.255.0.0")
				return "16";
			else if(ip == "255.255.128.0")
				return "17";
			else if(ip == "255.255.192.0")
				return "18";
			else if(ip == "255.255.224.0")
				return "19";
			else if(ip == "255.255.240.0")
				return "20";
			else if(ip == "255.255.248.0")
				return "21";
			else if(ip == "255.255.252.0")
				return "22";
			else if(ip == "255.255.254.0")
				return "23";
			else if(ip == "255.255.255.0")
				return "24";
			else if(ip == "255.255.255.128")
				return "25";
			else if(ip == "255.255.255.192")
				return "26";
			else if(ip == "255.255.255.224")
				return "27";
			else if(ip == "255.255.255.240")
				return "28";
			else if(ip == "255.255.255.248")
				return "29";
			else if(ip == "255.255.255.252")
				return "30";
			else if(ip == "255.255.255.254")
				return "31";
			else if(ip == "255.255.255.255")
				return "32";
			else
				return "";
		}

		private string BitMask2netMask(string bit)
		{
			if(bit == "0")
				return "0.0.0.0";	
			else if(bit == "1")
				return "128.0.0.0";	
			else if(bit == "2")
				return "192.0.0.0";	
			else if(bit == "3")
				return "224.0.0.0";	
			else if(bit == "4")
				return "240.0.0.0";	
			else if(bit == "5")
				return "248.0.0.0";	
			else if(bit == "6")
				return "252.0.0.0";	
			else if(bit == "7")
				return "254.0.0.0";	
			else if(bit == "8")
				return "255.0.0.0";	
			else if(bit == "9")
				return "255.128.0.0";	
			else if(bit == "10")
				return "255.192.0.0";	
			else if(bit == "11")
				return "255.224.0.0";	
			else if(bit == "12")
				return "255.240.0.0";	
			else if(bit == "13")
				return "255.248.0.0";	
			else if(bit == "14")
				return "255.252.0.0";	
			else if(bit == "15")
				return "255.254.0.0";	
			else if(bit == "16")
				return "255.255.0.0";	
			else if(bit == "17")
				return "255.255.128.0";	
			else if(bit == "18")
				return "255.255.192.0";	
			else if(bit == "19")
				return "255.255.224.0";	
			else if(bit == "20")
				return "255.255.240.0";	
			else if(bit == "21")
				return "255.255.248.0";	
			else if(bit == "22")
				return "255.255.252.0";	
			else if(bit == "23")
				return "255.255.254.0";	
			else if(bit == "24")
				return "255.255.255.0";	
			else if(bit == "25")
				return "255.255.255.128";	
			else if(bit == "26")
				return "255.255.255.192";	
			else if(bit == "27")
				return "255.255.255.224";	
			else if(bit == "28")
				return "255.255.255.240";	
			else if(bit == "29")
				return "255.255.255.248";	
			else if(bit == "30")
				return "255.255.255.252";	
			else if(bit == "31")
				return "255.255.255.254";	
			else if(bit == "32")
				return "255.255.255.255";	
			else
				return "";
		}
	}
}
