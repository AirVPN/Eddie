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
using System.Xml;
using System.Text;
using Eddie.Core;
using System.Net.NetworkInformation;

namespace Eddie.Core.ConnectionTypes
{
	public class WireGuard : IConnectionType
	{
		protected string m_interfaceName;
		protected ConfigBuilder.WireGuard m_configStartup;
		protected Elevated.Command m_elevatedCommand;

		protected IpAddresses m_dns = new IpAddresses();
		protected IpAddresses m_gateway = new IpAddresses();

		protected bool m_stopRequested = false;

		public override string GetTypeName()
		{
			return "WireGuard";
		}

		public override void Build()
		{
			base.Build();

			// With OpenVPN can be empty (no manual adapter creation).
			// With WireGuard, must exists (WG create the adapter).
			m_interfaceName = Engine.Instance.Options.Get("network.iface.name");
			if (m_interfaceName == "")
				m_interfaceName = "Eddie";

			m_configStartup = new ConfigBuilder.WireGuard();

			if (Info.Path != "")
			{
				if (Platform.Instance.FileExists(Info.Path))
				{
					string text = Platform.Instance.FileContentsReadText(Info.Path);
					m_configStartup.Parse(text);
				}
			}

			SetupLayers();

			Info.Provider.OnBuildConnection(this);

			EntryIP = m_configStartup.PeerEndpointAddress;
			EntryPort = m_configStartup.PeerEndpointPort;

			if (ConfigIPv4 == false)
				m_configStartup.InterfaceAddresses = m_configStartup.InterfaceAddresses.OnlyIPv6;
			if (ConfigIPv6 == false)
				m_configStartup.InterfaceAddresses = m_configStartup.InterfaceAddresses.OnlyIPv4;

			// DNS
			IpAddresses dnsCustom = new IpAddresses(Engine.Instance.Options.Get("dns.servers"));
			if (dnsCustom.Count > 0)
				m_configStartup.InterfaceDns = dnsCustom;
			if (Engine.Instance.Options.GetBool("dns.delegate") == false)
			{
				m_dns = m_configStartup.InterfaceDns.Clone();
				m_configStartup.InterfaceDns.Clear();
			}

			m_configStartup.Adaptation();
		}

		public override void OnStart()
		{
			base.OnStart();

			m_elevatedCommand = new Elevated.Command();
			m_elevatedCommand.Parameters["command"] = "wireguard";
			m_elevatedCommand.Parameters["action"] = "start";
			m_elevatedCommand.Parameters["id"] = Id;
			m_elevatedCommand.Parameters["interface"] = m_interfaceName;
			m_elevatedCommand.Parameters["config"] = m_configStartup.Build();
			m_elevatedCommand.Parameters["handshake_timeout_first"] = Engine.Instance.Options.GetInt("wireguard.handshake.timeout.first").ToString();
			m_elevatedCommand.Parameters["handshake_timeout_connected"] = Engine.Instance.Options.GetInt("wireguard.handshake.timeout.connected").ToString();

			m_elevatedCommand.ExceptionEvent += delegate (Elevated.Command cmd, string message)
			{
				Session.AddLogEvent("Tunnel", "Error: " + message);
			};

			m_elevatedCommand.ReceiveEvent += delegate (Elevated.Command cmd, string data)
			{
				if (data.StartsWithInv("log:"))
				{
					Session.AddLogEvent("Tunnel", data.Substring(4));
				}
				else if (data.StartsWithInv("err:"))
				{
					Session.AddLogEvent("Tunnel", "Error: " + data.Substring(4));
				}
				else
					Session.AddLogEvent("Tunnel", data);
			};

			m_elevatedCommand.CompleteEvent += delegate (Elevated.Command cmd)
			{
				Session.SetResetError();
			};

			m_elevatedCommand.DoASync();
		}

		public override void OnStop()
		{
			base.OnStop();
		}

		public override bool OnWaitingConnection()
		{
			return false;
		}

		public override bool OnWaitingDisconnection()
		{
			bool result = true;

			if ((m_elevatedCommand != null) && (m_elevatedCommand.IsComplete == false))
			{
				result = false;

				if (m_stopRequested == false)
				{
					m_stopRequested = true;

					Engine.Instance.Elevated.DoCommandSync("wireguard", "action", "stop", "id", Id, "interface", m_interfaceName);
				}
			}

			return result;
		}

		public override void OnLogEvent(string source, string message)
		{
			string messageLower = message.ToLowerInvariant(); // Try to match lower/insensitive case when possible.

			if (source == "Tunnel")
			{
				bool log = true;
				LogType logType = LogType.Verbose;

				if (messageLower.StartsWithInv("error:"))
				{
					logType = LogType.Error;
					Session.SetResetError();
				}
				else if (messageLower == "setup-start")
				{
					log = false;
					Engine.Instance.Logs.Log(LogType.Verbose, "WireGuard > " + LanguageManager.GetText("WireGuardSetupStart"));
				}
				else if (messageLower == "setup-complete")
				{
					log = false;
					Engine.Instance.Logs.Log(LogType.Verbose, "WireGuard > " + LanguageManager.GetText("WireGuardSetupComplete"));
				}
				else if (messageLower == "setup-interface")
				{
					log = false;
					Engine.Instance.Logs.Log(LogType.Verbose, "WireGuard > " + LanguageManager.GetText("WireGuardSetupInterface"));
					SearchTunNetworkInterfaceByName(m_interfaceName);
				}
				else if (messageLower == "handshake-first")
				{
					log = false;
					Engine.Instance.Logs.Log(LogType.Verbose, "WireGuard > " + LanguageManager.GetText("WireGuardFirstHandshake"));

					if (Session.GetConnected() == false)
					{
						// Forced because actually exists only one version of WireGuard protocol, maybe in future detected based on protocol version
						DataChannel = "WireGuard Data Channel"; // Need better description?
						ControlChannel = "WireGuard Control Channel"; // Need better description?

						Session.ConnectedStep();
					}
				}
				else if (messageLower == "handshake-out")
				{
					log = false;

					if (Session.InReset == false)
					{
						Engine.Instance.Logs.Log(LogType.Warning, "WireGuard > " + LanguageManager.GetText("WireGuardTimeoutHandshake"));
						Session.SetResetError();
					}
				}
				else if (messageLower.StartsWithInv("interface-name:"))
				{
					log = false;

					// If the OS can't assign the request name (macOS for example), receive the interface name here.
					m_interfaceName = messageLower.Substring(15);
				}
				else if (messageLower == "stop-requested")
				{
					log = false;
					Engine.Instance.Logs.Log(LogType.Verbose, "WireGuard > " + LanguageManager.GetText("WireGuardStopRequested"));
				}
				else if (messageLower == "stop-interface")
				{
					log = false;
					Engine.Instance.Logs.Log(LogType.Verbose, "WireGuard > " + LanguageManager.GetText("WireGuardStopInterface"));
				}
				else if (messageLower == "stop")
				{
					log = false;
					Engine.Instance.Logs.Log(LogType.Verbose, "WireGuard > " + LanguageManager.GetText("WireGuardStopCompleted"));
				}

				if (log)
					Engine.Instance.Logs.Log(logType, "WireGuard > " + message);
			}
		}

		public override IpAddresses GetDns()
		{
			if (Engine.Instance.Options.GetBool("dns.delegate"))
				return m_configStartup.InterfaceDns;
			else
				return m_dns;
		}

		public override IpAddresses GetVpnIPs()
		{
			return m_configStartup.InterfaceAddresses;
		}

		public override string GetProtocolDescription()
		{
			return "UDP";
		}

		public override string ExportConfigStartup()
		{
			return m_configStartup.Build();
		}

		// -----------------------------------------
		// Public
		// -----------------------------------------

		public ConfigBuilder.WireGuard ConfigStartup
		{
			get
			{
				return m_configStartup;
			}
		}

		// -----------------------------------------
		// Private
		// -----------------------------------------
	}
}
