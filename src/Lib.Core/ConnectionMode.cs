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
using System.Text;
using System.Xml;

namespace Eddie.Core
{
	public class ConnectionMode
	{
		public string Title;
		public string Protocol;
		public int Port;
		public int EntryIndex;
		public string Specs;
		public string MinVersion;
		public string Directives;

		public void ReadXML(XmlElement node)
		{
			Title = Utils.XmlGetAttributeString(node, "title", "");
			Protocol = Utils.XmlGetAttributeString(node, "protocol", "").ToUpperInvariant();
			Port = Utils.XmlGetAttributeInt(node, "port", 0);			
			EntryIndex = Utils.XmlGetAttributeInt(node, "entry_index", 0);
			Specs = Utils.XmlGetAttributeString(node, "specs", "");
			MinVersion = Utils.XmlGetAttributeString(node, "openvpn_minversion", "");
			Directives = Utils.XmlGetAttributeString(node, "openvpn_directives", "");
		}

		public bool Available
		{
			get
			{
				if (MinVersion == "")
					return true;

				if (Software.GetTool("openvpn").VersionAboveOrEqual(MinVersion))
					return true;

				return false;
			}
		}
	}
}
