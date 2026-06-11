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
			if ((m_configStartup != null) && m_configStartup.IsAmneziaWG())
				return "AmneziaWG";
			return "WireGuard";
		}

		public override void Build()
		{
			base.Build();

			m_interfaceName = Engine.Instance.ProfileOptions.Get("network.iface.name");
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
			IpAddresses dnsCustom = new IpAddresses(Engine.Instance.ProfileOptions.Get("dns.servers"));
			if (dnsCustom.Count > 0)
				m_configStartup.InterfaceDns = dnsCustom;
			if (Engine.Instance.ProfileOptions.GetBool("dns.delegate") == false)
			{
				m_dns = m_configStartup.InterfaceDns.Clone();
				m_configStartup.InterfaceDns.Clear();
			}

			if (m_configStartup.IsAmneziaWG())
			{
				m_configStartup.AmneziaJc = Engine.Instance.ProfileOptions.GetInt("amneziawg.jc");
				m_configStartup.AmneziaJmin = Engine.Instance.ProfileOptions.GetInt("amneziawg.jmin");
				m_configStartup.AmneziaJmax = Engine.Instance.ProfileOptions.GetInt("amneziawg.jmax");
				m_configStartup.AmneziaS1 = Engine.Instance.ProfileOptions.GetInt("amneziawg.s1");
				m_configStartup.AmneziaS2 = Engine.Instance.ProfileOptions.GetInt("amneziawg.s2");
				m_configStartup.AmneziaS3 = Engine.Instance.ProfileOptions.GetInt("amneziawg.s3");
				m_configStartup.AmneziaS4 = Engine.Instance.ProfileOptions.GetInt("amneziawg.s4");
				m_configStartup.AmneziaH1 = Engine.Instance.ProfileOptions.Get("amneziawg.h1");
				m_configStartup.AmneziaH2 = Engine.Instance.ProfileOptions.Get("amneziawg.h2");
				m_configStartup.AmneziaH3 = Engine.Instance.ProfileOptions.Get("amneziawg.h3");
				m_configStartup.AmneziaH4 = Engine.Instance.ProfileOptions.Get("amneziawg.h4");

				// CPS preset logic: random preset > selected preset > manual I1-I5
				string cpsI1 = "", cpsI2 = "", cpsI3 = "", cpsI4 = "", cpsI5 = "";
				if (Engine.Instance.ProfileOptions.GetBool("amneziawg.cps.random"))
				{
					AmneziaCPSDatabase.CPS randomCps = AmneziaCPSDatabase.GetRandomPreset();
					if (randomCps != null)
					{
						cpsI1 = randomCps.I1;
						cpsI2 = randomCps.I2;
						cpsI3 = randomCps.I3;
						cpsI4 = randomCps.I4;
						cpsI5 = randomCps.I5;
					}
				}
				else
				{
					string presetName = Engine.Instance.ProfileOptions.Get("amneziawg.cps.preset");
					if (!string.IsNullOrEmpty(presetName))
					{
						AmneziaCPSDatabase.CPS preset = AmneziaCPSDatabase.GetPreset(presetName);
						if (preset != null)
						{
							cpsI1 = preset.I1;
							cpsI2 = preset.I2;
							cpsI3 = preset.I3;
							cpsI4 = preset.I4;
							cpsI5 = preset.I5;
						}
					}
				}

				// Manual I1-I5 values override preset values
				m_configStartup.AmneziaI1 = Engine.Instance.ProfileOptions.Get("amneziawg.i1");
				m_configStartup.AmneziaI2 = Engine.Instance.ProfileOptions.Get("amneziawg.i2");
				m_configStartup.AmneziaI3 = Engine.Instance.ProfileOptions.Get("amneziawg.i3");
				m_configStartup.AmneziaI4 = Engine.Instance.ProfileOptions.Get("amneziawg.i4");
				m_configStartup.AmneziaI5 = Engine.Instance.ProfileOptions.Get("amneziawg.i5");

				// Fall back to preset values if manual is empty
				if (string.IsNullOrEmpty(m_configStartup.AmneziaI1)) m_configStartup.AmneziaI1 = cpsI1;
				if (string.IsNullOrEmpty(m_configStartup.AmneziaI2)) m_configStartup.AmneziaI2 = cpsI2;
				if (string.IsNullOrEmpty(m_configStartup.AmneziaI3)) m_configStartup.AmneziaI3 = cpsI3;
				if (string.IsNullOrEmpty(m_configStartup.AmneziaI4)) m_configStartup.AmneziaI4 = cpsI4;
				if (string.IsNullOrEmpty(m_configStartup.AmneziaI5)) m_configStartup.AmneziaI5 = cpsI5;
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
			m_elevatedCommand.Parameters["handshake_timeout_first"] = Engine.Instance.ProfileOptions.GetInt("wireguard.handshake.timeout.first").ToString();
			m_elevatedCommand.Parameters["handshake_timeout_connected"] = Engine.Instance.ProfileOptions.GetInt("wireguard.handshake.timeout.connected").ToString();
			m_elevatedCommand.Parameters["amneziawg"] = m_configStartup.IsAmneziaWG() ? "1" : "0";

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
					Engine.Instance.Logs.Log(LogType.Verbose, "WireGuard > " + LanguageManager.GetText(LanguageItems.WireGuardSetupStart));
				}
				else if (messageLower == "setup-complete")
				{
					log = false;
					Engine.Instance.Logs.Log(LogType.Verbose, "WireGuard > " + LanguageManager.GetText(LanguageItems.WireGuardSetupComplete));
				}
				else if (messageLower == "setup-interface")
				{
					log = false;
					Engine.Instance.Logs.Log(LogType.Verbose, "WireGuard > " + LanguageManager.GetText(LanguageItems.WireGuardSetupInterface));
					SearchTunNetworkInterfaceByName(m_interfaceName);
				}
				else if (messageLower == "handshake-first")
				{
					log = false;
					Engine.Instance.Logs.Log(LogType.Verbose, "WireGuard > " + LanguageManager.GetText(LanguageItems.WireGuardFirstHandshake));

					if (Session.GetConnected() == false)
					{
						// Forced because actually exists only one version of WireGuard protocol, maybe in future detected based on protocol version
						CipherInfo = "Data packets symmetric encryption: ChaCha20 - Poly1305; Perfect Forward Secrecy(PFS): ECDH with Curve25519";

						Session.ConnectedStep();
					}
				}
				else if (messageLower == "handshake-out")
				{
					log = false;

					if (Session.InReset == false)
					{
						Engine.Instance.Logs.Log(LogType.Warning, "WireGuard > " + LanguageManager.GetText(LanguageItems.WireGuardTimeoutHandshake));
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
					Engine.Instance.Logs.Log(LogType.Verbose, "WireGuard > " + LanguageManager.GetText(LanguageItems.WireGuardStopRequested));
				}
				else if (messageLower == "stop-interface")
				{
					log = false;
					Engine.Instance.Logs.Log(LogType.Verbose, "WireGuard > " + LanguageManager.GetText(LanguageItems.WireGuardStopInterface));
				}
				else if (messageLower == "stop")
				{
					log = false;
					Engine.Instance.Logs.Log(LogType.Verbose, "WireGuard > " + LanguageManager.GetText(LanguageItems.WireGuardStopCompleted));
				}

				if (log)
					Engine.Instance.Logs.Log(logType, "WireGuard > " + message);
			}
		}

		public override IpAddresses GetDns()
		{
			if (Engine.Instance.ProfileOptions.GetBool("dns.delegate"))
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
