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
	public class RouteEntry
	{
		public IpAddress Address;
		public IpAddress Mask;
		public IpAddress Gateway;
		public string Interface;
		public string Metrics;
		
		// Unix only:
		public string Flags;
		public string Mss;
		public string Window;
		public string Irtt;

		public int RefCount = 0;

		public RouteEntry()
		{
			
		}

		public void Add()
		{
			Platform.Instance.RouteAdd(this);
		}

		public void Remove()
		{
			Platform.Instance.RouteRemove(this);
		}

		public void ReadXML(XmlElement node)
		{
			Address = node.GetAttribute("address");
			Mask = node.GetAttribute("mask");
			Gateway = node.GetAttribute("gateway");
			Interface = node.GetAttribute("interface");
			Metrics = node.GetAttribute("metrics");
			RefCount = 1;
		}

		public void WriteXML(XmlElement node)
		{
			node.SetAttribute("address", Address.Value);
			node.SetAttribute("mask", Mask.Value);
			node.SetAttribute("gateway", Gateway.Value);
			node.SetAttribute("interface", Interface);
			node.SetAttribute("metrics", Metrics);
		}

		public string Key
		{
			get
			{
				return NetworkLocking.Key(Address, Mask);
			}
		}

		public override string ToString()
		{
			return "Address: " + Address.Value + ", Mask: " + Mask.Value + ", Gateway: " + Gateway.Value;
		}
	}
}
