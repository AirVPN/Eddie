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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Eddie.Core.ConfigBuilder
{
	public class WireGuard : IConfigBuilder
	{
		// Section [Interface]
		public IpAddresses InterfaceAddresses = new IpAddresses();
		public int InterfaceListenPort = 0;
		public string InterfacePrivateKey = "";
		public IpAddresses InterfaceDns = new IpAddresses();
		public int InterfaceTable = 0;
		public int InterfaceMTU = 0;
		public string InterfaceFwMark = "";
		public string InterfacePreUp = "";
		public string InterfacePostUp = "";
		public string InterfacePreDown = "";
		public string InterfacePostDown = "";

		// Section [Peer]
		public string PeerPublicKey = "";
		public string PeerPresharedKey = "";
		public string PeerEndpointAddress = "";
		public int PeerEndpointPort = 0;
		public IpAddresses PeerAllowedIPs = new IpAddresses();
		public int PeerPersistentKeepalive = 0;

		public override string HasError()
		{
			if (InterfaceAddresses.Count == 0)
				return "Missing Interface Address";
			if (InterfacePrivateKey == "")
				return "Missing Interface PrivateKey";
			if (PeerPublicKey == "")
				return "Missing Peer PublicKey";
			if (PeerEndpointAddress == "")
				return "Missing Endpoint Address";
			if (PeerEndpointPort == 0)
				return "Missing Endpoint Port";

			return "";
		}

		public override string Build()
		{
			// Note: we use a single-field (for example Address) with comma separated multivalues, not multiple lines.

			StringBuilder s = new StringBuilder();

			s.Append("# " + Utils.GenerateFileHeader() + Platform.Instance.EndOfLineSep + Platform.Instance.EndOfLineSep);
			s.Append("[Interface]\n");
			s.Append("Address = " + InterfaceAddresses.ToString() + Platform.Instance.EndOfLineSep);
			if (InterfaceListenPort != 0)
				s.Append("ListenPort = " + InterfaceListenPort.ToString() + Platform.Instance.EndOfLineSep);
			s.Append("PrivateKey = " + InterfacePrivateKey + Platform.Instance.EndOfLineSep);
			if (InterfaceDns.Count > 0)
				s.Append("DNS = " + InterfaceDns.ToString() + Platform.Instance.EndOfLineSep);
			if (InterfaceTable != 0)
				s.Append("Table = " + InterfaceTable.ToString() + Platform.Instance.EndOfLineSep);

			int buildInterfaceMTU = InterfaceMTU;
			if (Engine.Instance.Options.GetInt("network.mtu") != -1)
				buildInterfaceMTU = Engine.Instance.Options.GetInt("network.mtu");
			if (buildInterfaceMTU != 0)
				s.Append("MTU = " + buildInterfaceMTU.ToString() + Platform.Instance.EndOfLineSep);

			if (InterfaceFwMark != "")
				s.Append("FwMark = " + InterfaceFwMark + Platform.Instance.EndOfLineSep);

			if (Engine.Instance.Options.GetBool("wireguard.interface.skip_commands") == false)
			{
				if (InterfacePreUp != "")
					s.Append("PreUp = " + InterfacePreUp + Platform.Instance.EndOfLineSep);
				if (InterfacePostUp != "")
					s.Append("PreUp = " + InterfacePostUp + Platform.Instance.EndOfLineSep);
				if (InterfacePreDown != "")
					s.Append("PreUp = " + InterfacePreDown + Platform.Instance.EndOfLineSep);
				if (InterfacePostDown != "")
					s.Append("PreUp = " + InterfacePostDown + Platform.Instance.EndOfLineSep);
			}

			s.Append(Platform.Instance.EndOfLineSep);
			s.Append("[Peer]" + Platform.Instance.EndOfLineSep);
			s.Append("PublicKey = " + PeerPublicKey + Platform.Instance.EndOfLineSep);
			if (PeerPresharedKey != "")
				s.Append("PresharedKey = " + PeerPresharedKey + Platform.Instance.EndOfLineSep);

			string peerEndpointAddress = PeerEndpointAddress;
			if (new IpAddress(peerEndpointAddress).IsV6) peerEndpointAddress = "[" + peerEndpointAddress + "]";
			s.Append("Endpoint = " + peerEndpointAddress + ":" + PeerEndpointPort + Platform.Instance.EndOfLineSep);
			if (PeerAllowedIPs.Count > 0)
				s.Append("AllowedIPs = " + PeerAllowedIPs.ToString() + Platform.Instance.EndOfLineSep);

			int buildPeerPersistentKeepAlive = PeerPersistentKeepalive;
			if (Engine.Instance.Options.GetInt("wireguard.peer.persistentkeepalive") != -1)
				buildPeerPersistentKeepAlive = Engine.Instance.Options.GetInt("wireguard.peer.persistentkeepalive");
			if (buildPeerPersistentKeepAlive != 0)
				s.Append("PersistentKeepalive = " + buildPeerPersistentKeepAlive.ToString() + Platform.Instance.EndOfLineSep);

			s.Append(Platform.Instance.EndOfLineSep);
			return s.ToString();
		}

		public override void Parse(string config)
		{
			base.Parse(config);

			string section = "";

			foreach (string lineOriginal in config.Split('\n'))
			{
				string line = lineOriginal.Trim();

				int posComment = line.IndexOf('#');
				if (posComment != -1)
					line = line.Substring(0, posComment).Trim();

				if (line == "")
					continue;

				if (line.StartsWith("["))
				{
					int posEnd = line.IndexOf("]");
					if (posEnd != -1)
					{
						section = line.Substring(1, posEnd - 1).ToLowerInvariant();
					}
				}
				else
				{
					// Expect keypair
					int posValue = line.IndexOf("=");
					if (posValue != -1)
					{
						string key = line.Substring(0, posValue).ToLowerInvariant().Trim();
						string value = line.Substring(posValue + 1).Trim();

						if ((section == "interface") && (key == "address"))
						{
							foreach (string value2 in value.Split(','))
							{
								InterfaceAddresses.Add(value2.Trim());
							}
						}
						else if ((section == "interface") && (key == "listenport"))
							InterfaceListenPort = Conversions.ToInt32(value);
						else if ((section == "interface") && (key == "privatekey"))
							InterfacePrivateKey = value;
						else if ((section == "interface") && (key == "dns"))
						{
							foreach (string value2 in value.Split(','))
							{
								InterfaceDns.Add(value2.Trim());
							}
						}
						else if ((section == "interface") && (key == "table"))
							InterfaceTable = Conversions.ToInt32(value);
						else if ((section == "interface") && (key == "mtu"))
							InterfaceMTU = Conversions.ToInt32(value);
						else if ((section == "interface") && (key == "fwmark"))
							InterfaceFwMark = value;
						else if ((section == "interface") && (key == "preup"))
							InterfacePreUp = value;
						else if ((section == "interface") && (key == "postup"))
							InterfacePostUp = value;
						else if ((section == "interface") && (key == "predown"))
							InterfacePreDown = value;
						else if ((section == "interface") && (key == "postdown"))
							InterfacePostDown = value;
						else if ((section == "peer") && (key == "publickey"))
							PeerPublicKey = value;
						else if ((section == "peer") && (key == "presharedkey"))
							PeerPresharedKey = value;
						else if ((section == "peer") && (key == "endpoint"))
						{
							int posPort = value.LastIndexOf(':');
							if (posPort != -1)
							{
								PeerEndpointAddress = value.Substring(0, posPort);
								PeerEndpointPort = Conversions.ToInt32(value.Substring(posPort + 1));
							}
						}
						else if ((section == "peer") && (key == "allowedips"))
						{
							foreach (string value2 in value.Split(','))
							{
								PeerAllowedIPs.Add(value2.Trim());
							}
						}
						else if ((section == "peer") && (key == "persistentkeepalive"))
							PeerPersistentKeepalive = Conversions.ToInt32(value);
						else
						{
							Engine.Instance.Logs.LogVerbose("Unknown WireGuard directive '" + line + "'");
						}
					}
					else
						Engine.Instance.Logs.LogVerbose("Unknown WireGuard directive '" + line + "'");
				}
			}
		}

		public override void Adaptation()
		{
			base.Adaptation();

			// Normally WireGuard use AllowedIPs for routing or firewall routes, managed directly by Eddie.
			PeerAllowedIPs.Clear();
			PeerAllowedIPs.Add("0.0.0.0/0");
			PeerAllowedIPs.Add("::/0");

			// Platform specific
			Platform.Instance.AdaptConfigWireGuard(this);
		}
	}
}
