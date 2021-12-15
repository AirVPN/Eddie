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

using System.Xml;

namespace Eddie.Core
{
	public class ConnectionMode
	{
		public string Title;
		public string Type; // openvpn,wireguard
		public string Protocol; // udp,tcp
		public string Transport; // empty,ssh,ssl
		public int Port;
		public int EntryIndex;
		public string Specs;
		public string OpenVpnMinVersion;
		public string OpenVpnDirectives;
		public int SshPortDestination = 0;

		public void ReadXML(XmlElement node)
		{
			Title = node.GetAttributeString("title", "");
			Type = node.GetAttributeString("type", "");
			Protocol = node.GetAttributeString("protocol", "").ToUpperInvariant();
			Transport = node.GetAttributeString("transport", "").ToUpperInvariant();
			Port = node.GetAttributeInt("port", 0);
			EntryIndex = node.GetAttributeInt("entry_index", 0);
			Specs = node.GetAttributeString("specs", "");
			OpenVpnMinVersion = node.GetAttributeString("openvpn_minversion", "");
			OpenVpnDirectives = node.GetAttributeString("openvpn_directives", "");
			SshPortDestination = node.GetAttributeInt("ssh_destination", 0);

			// Adapt, until manifest upgrade
			if (Protocol == "UDP")
			{
				Transport = "";
			}
			else if (Protocol == "TCP")
			{
				Transport = "";
			}
			else if (Protocol == "SSH")
			{
				Protocol = "TCP";
				Transport = "SSH";
			}
			else if (Protocol == "SSL")
			{
				Protocol = "TCP";
				Transport = "SSL";
			}
		}

		public bool Available
		{
			get
			{
				if (OpenVpnMinVersion == "")
					return true;

				if (Engine.Instance.GetOpenVpnTool().VersionAboveOrEqual(OpenVpnMinVersion))
					return true;

				return false;
			}
		}
	}
}
