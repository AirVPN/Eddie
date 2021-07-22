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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using Eddie.Core;

namespace Eddie.Core
{
	public class SessionLogEvent
	{
		public string Source;
		public string Message;

		public SessionLogEvent(string source, string message)
		{
			Source = source;
			Message = message;
		}
	}

	public class Session : Eddie.Core.Thread
	{
		public DateTime StatsStart = DateTime.MinValue;
		public Int64 StatsRead = 0;
		public Int64 StatsWrite = 0;

		private bool m_connected = false;
		private string m_reset = "";
		private List<SessionLogEvent> m_logEvents = new List<SessionLogEvent>();
		private InterfaceScope m_interfaceScope = null;
		private ConnectionTypes.IConnectionType m_connection = null;

		public ConnectionTypes.IConnectionType Connection
		{
			get
			{
				return m_connection;
			}
		}

		public override void OnRun()
		{
			CancelRequested = false;

			StatsStart = DateTime.UtcNow;
			StatsRead = 0;
			StatsWrite = 0;

			string sessionLastServer = "";

			bool oneConnectionReached = false;

			for (; CancelRequested == false;)
			{
				RouteScope routeScope = null;

				bool allowed = true;
				string waitingMessage = "";
				int waitingSecs = 0;

				try
				{
					// -----------------------------------
					// Phase 1: Initialization and start
					// -----------------------------------

					// The first refresh of providers must be completed
					// Removed in 2.18.9, always performed before
					/*
                    if (Engine.ProvidersManager.LastRefreshDone == 0)
					{
                        Engine.Instance.WaitMessageSet(LanguageManager.GetText("ProvidersWait"), true);
						Engine.Logs.Log(LogType.Verbose, LanguageManager.GetText("ProvidersWait"));
						for (; ; )
						{
                            if (CancelRequested)
								break;

							if (Engine.ProvidersManager.LastRefreshDone != 0)
								break;

							Sleep(100);
						}
					}
					*/

					if (CancelRequested)
						continue;

					string forceServer = Engine.Options.Get("server");
					if ((Engine.NextServer == null) && (forceServer != ""))
					{
						Engine.NextServer = Engine.PickConnectionByName(forceServer);
					}

					if ((Engine.NextServer == null) && (Engine.Options.GetBool("servers.startlast")))
					{
						Engine.NextServer = Engine.PickConnection(Engine.Options.Get("servers.last"));
					}

					if ((Engine.NextServer == null) && (Engine.Options.GetBool("servers.locklast")) && (sessionLastServer != ""))
					{
						Engine.NextServer = Engine.PickConnection(sessionLastServer);
					}

					if (CancelRequested)
						continue;

					if ((Engine.NextServer == null) && (Engine.Instance.JobsManager.Latency.GetEnabled()) && (Engine.PingerInvalid() != 0))
					{
						string lastWaitingMessage = "";
						for (; ; )
						{
							if (CancelRequested)
								break;

							int i = Engine.PingerInvalid();
							if (i == 0)
								break;

							string nextWaitingMessage = LanguageManager.GetText("WaitingLatencyTestsTitle") + " " + LanguageManager.GetText("WaitingLatencyTestsStep", i.ToString());
							if (lastWaitingMessage != nextWaitingMessage)
							{
								lastWaitingMessage = nextWaitingMessage;
								Engine.Logs.LogVerbose(nextWaitingMessage);
								Engine.WaitMessageSet(nextWaitingMessage, true);
							}

							Sleep(1000);
						}
					}

					if (CancelRequested)
						continue;

					if (Engine.NextServer == null)
						Engine.NextServer = Engine.PickConnection();

					if (Engine.NextServer == null)
					{
						allowed = false;
						Engine.Logs.Log(LogType.Fatal, "No server available.");
						RequestStop();
					}

					Engine.CurrentServer = Engine.NextServer;
					Engine.NextServer = null;

					// Checking auth user status.
					// Only to avoid a generic AUTH_FAILED. For that we don't report here for ex. the sshtunnel keys.
					if (allowed)
					{
						if (Engine.CurrentServer.Provider is Providers.Service)
						{
							Providers.Service service = Engine.CurrentServer.Provider as Providers.Service;
							if (service.SupportConnect)
							{
								Engine.WaitMessageSet(LanguageManager.GetText("AuthorizeConnect"), true);
								Engine.Logs.Log(LogType.Info, LanguageManager.GetText("AuthorizeConnect"));

								Dictionary<string, string> parameters = new Dictionary<string, string>();
								parameters["act"] = "connect";
								parameters["server"] = Engine.CurrentServer.ProviderName;
								/* // 2.11.4
								parameters["protocol"] = protocol;
								parameters["port"] = port.ToString();
								parameters["alt"] = alt.ToString();
								*/

								XmlDocument xmlDoc = null;
								try
								{
									xmlDoc = service.Fetch(LanguageManager.GetText("AuthorizeConnect"), parameters);
								}
								catch (Exception ex)
								{
									// Note: If failed, continue anyway.
									Engine.Logs.Log(LogType.Warning, LanguageManager.GetText("AuthorizeConnectFailed", ex.Message));
								}

								if (xmlDoc != null)
								{
									string userMessage = xmlDoc.DocumentElement.GetAttributeString("message", "");
									if (userMessage != "")
									{
										allowed = false;
										string userMessageAction = xmlDoc.DocumentElement.GetAttributeString("message_action", "");
										if (userMessageAction == "stop")
										{
											Engine.Logs.Log(LogType.Fatal, userMessage);
											Engine.Disconnect(); // 2.8
											RequestStop();
										}
										else if (userMessageAction == "next")
										{
											Engine.CurrentServer.Penality += Engine.Options.GetInt("advanced.penality_on_error");
											waitingMessage = userMessage + ", next in {1} sec.";
											waitingSecs = 5;
										}
										else
										{
											waitingMessage = userMessage + ", retry in {1} sec.";
											waitingSecs = 10;
										}
									}
								}
							}
						}
					}

					if (CancelRequested)
						continue;

					if (Engine.CurrentServer.SupportIPv4)
					{
						bool osSupport = Conversions.ToBool(Engine.Instance.Manifest["network_info"]["support_ipv4"].Value);
						if ((osSupport == false) && (Engine.Instance.Options.GetLower("network.ipv4.mode") != "block"))
						{
							Engine.Instance.Logs.LogWarning(LanguageManager.GetText("IPv4NotSupportedByOS"));
							if ((Engine.Instance.Options.GetBool("network.ipv4.autoswitch")) && (Engine.Instance.Options.Get("network.ipv4.mode") != "block"))
							{
								Engine.Instance.Logs.LogWarning(LanguageManager.GetText("IPv4NotSupportedByNetworkAdapterAutoSwitch"));
								Engine.Instance.Options.Set("network.ipv4.mode", "block");
							}
						}
					}

					if (Engine.CurrentServer.SupportIPv6)
					{
						bool osSupport = Conversions.ToBool(Engine.Instance.Manifest["network_info"]["support_ipv6"].Value);
						if ((osSupport == false) && (Engine.Instance.Options.GetLower("network.ipv6.mode") != "block"))
						{
							Engine.Instance.Logs.LogWarning(LanguageManager.GetText("IPv6NotSupportedByOS"));
							if ((Engine.Instance.Options.GetBool("network.ipv6.autoswitch")) && (Engine.Instance.Options.Get("network.ipv6.mode") != "block"))
							{
								Engine.Instance.Logs.LogWarning(LanguageManager.GetText("IPv6NotSupportedByNetworkAdapterAutoSwitch"));
								Engine.Instance.Options.Set("network.ipv6.mode", "block");
							}
						}
					}

					try
					{
						m_connection = Engine.CurrentServer.BuildConnection(this);

						if (Engine.Instance.Options.GetBool("advanced.skip_tun_detect") == false)
						{
							string driverRequested = Platform.Instance.GetConnectionTunDriver(m_connection);
							Engine.Instance.WaitMessageSet(LanguageManager.GetText("OsDriverInstall", driverRequested), false);
							Platform.Instance.EnsureDriverAndAdapterAvailable(driverRequested);
						}

						Engine.WaitMessageSet(LanguageManager.GetText("ConnectionCredentials"), true);
						if (Engine.CurrentServer.Provider.ApplyCredentials(m_connection) == false)
						{
							allowed = false;
							CancelRequested = true;
							SetReset("FATAL");
						}
					}
					catch (Exception ex)
					{
						Engine.Logs.Log(ex);
						allowed = false;
						SetReset("ERROR");
					}

					if (allowed)
					{
						m_connection.SetupRoutes();

						m_connection.OnInit();

						// Need IPv6 block?
						{
							if (Engine.Instance.GetNetworkIPv6Mode() == "block")
								m_connection.BlockedIPv6 = true;

							if ((Engine.CurrentServer.SupportIPv6 == false) && (Engine.Instance.GetNetworkIPv6Mode() == "in-block"))
								m_connection.BlockedIPv6 = true;

							if ((Engine.Instance.GetOpenVpnTool().VersionUnder("2.4")) && (Engine.Instance.GetNetworkIPv6Mode() == "in-block"))
								m_connection.BlockedIPv6 = true;

							if (m_connection.BlockedIPv6)
								Platform.Instance.OnIPv6Block();
						}

						m_connection.ExitIPs.Add(Engine.CurrentServer.IpsExit.Clone());

						sessionLastServer = Engine.CurrentServer.Code;
						Engine.Options.Set("servers.last", Engine.CurrentServer.Code);

						routeScope = new RouteScope(m_connection.EntryIP); // Clodo: Urgent, may not work under some OS with NetLock active. Try to add the RouteScope when detecting protocol from OpenVPN logs.

						Engine.RunEventCommand("vpn.pre");

						string connectingMessage = LanguageManager.GetText("ConnectionConnecting", Engine.CurrentServer.GetNameWithLocation());
						Engine.WaitMessageSet(connectingMessage, true);
						Engine.Logs.Log(LogType.InfoImportant, connectingMessage);

						// 2.14.0
						RoutesApply("pre");

						m_connection.OnStart();

						int waitingSleep = 100; // To avoid CPU stress

						SetReset("");

						// -----------------------------------
						// Phase 2: Waiting connection
						// -----------------------------------

						for (; ; )
						{
							ProcessLogsEvents();

							if (m_connection.OnWaitingConnection())
								break;

							if (Engine.IsConnected())
								break;

							if (m_reset != "")
								break;

							Sleep(waitingSleep);
						}

						ProcessLogsEvents();

						if (m_reset == "")
							oneConnectionReached = true;

						// -----------------------------------
						// Phase 3 - Running
						// -----------------------------------

						if (m_reset == "")
						{
							for (; ; )
							{
								ProcessLogsEvents();

								Int64 timeNow = Utils.UnixTimeStamp();

								if (Engine.IsConnected() == false)
									throw new Exception("Unexpected.");

								// Need stop?
								bool StopRequest = false;

								if (m_reset == "RETRY")
								{
									StopRequest = true;
								}
								else if (m_reset == "ERROR")
								{
									StopRequest = true;
								}
								else if (m_reset == "FATAL")
								{
									StopRequest = true;
								}

								if (Engine.NextServer != null)
								{
									SetReset("SWITCH"); // 2.11.8
									StopRequest = true;
								}

								if (Engine.SwitchServer != false)
								{
									Engine.SwitchServer = false;
									SetReset("SWITCH"); // 2.11.8
									StopRequest = true;
								}

								if (CancelRequested)
									StopRequest = true;

								if (StopRequest)
									break;

								Sleep(waitingSleep);
							}
						}

						if (m_reset == "ERROR")
						{
							Engine.CurrentServer.Penality += Engine.Options.GetInt("advanced.penality_on_error");
						}

						// -----------------------------------
						// Phase 4 - Start disconnection
						// -----------------------------------

						SetConnected(false);

						Engine.WaitMessageSet(LanguageManager.GetText("ConnectionDisconnecting"), false);
						Engine.Logs.Log(LogType.InfoImportant, LanguageManager.GetText("ConnectionDisconnecting"));

						m_connection.OnStop();

						// -----------------------------------
						// Phase 5 - Waiting disconnection
						// -----------------------------------

						for (; ; )
						{
							try
							{
								ProcessLogsEvents();

								if (m_connection.OnWaitingDisconnection())
									break;

								Sleep(waitingSleep);
							}
							catch (Exception ex)
							{
								Engine.Logs.Log(LogType.Warning, ex);
								break;
							}
						}

						// -----------------------------------
						// Phase 6: Cleaning, waiting before retry.
						// -----------------------------------

						ProcessLogsEvents(); // Latest flush of openvpn disconnection messages

						RoutesApply("post"); // moved here in 2.21, before are removed during disconnection above						

						Engine.RunEventCommand("vpn.down");

						if (m_connection.BlockedIPv6)
						{
							Platform.Instance.OnIPv6Restore();
							m_connection.BlockedIPv6 = false;
						}

						//Platform.Instance.OnRouteDefaultRemoveRestore();

						if (Engine.Instance.Options.GetBool("dns.delegate") == false)
							Platform.Instance.OnDnsSwitchRestore();

						Platform.Instance.OnInterfaceRestore();

						if (m_connection != null)
						{
							m_connection.OnClose();
							m_connection = null;
						}

						Engine.Logs.Log(LogType.Verbose, LanguageManager.GetText("ConnectionStop"));
					}
				}
				catch (Exception ex)
				{
					// Warning: Avoid to reach this catch: unpredicable status of running processes.
					SetConnected(false);

					Engine.Logs.Log(LogType.Error, LanguageManager.GetText("FatalUnexpected", ex.Message + " - " + ex.StackTrace));

					SetReset("FATAL");
				}

				if (routeScope != null)
				{
					routeScope.End();
					routeScope = null;
				}

				if (m_interfaceScope != null)
				{
					m_interfaceScope.End();
					m_interfaceScope = null;
				}

				if (Engine.StartCommandLine.Get("console.mode") == "batch")
				{
					Engine.Instance.RequestStop();
					break;
				}

				if (m_reset == "AUTH_FAILED")
				{
					waitingMessage = "Auth failed, retry in {1} sec.";
					waitingSecs = 3;
				}
				else if (m_reset == "ERROR")
				{
					waitingMessage = "Restart in {1} sec.";
					waitingSecs = 3;
				}
				else if (m_reset == "FATAL")
				{
					Engine.Instance.Disconnect();
					break;
				}

				if (waitingSecs > 0)
				{
					for (int i = 0; i < waitingSecs; i++)
					{
						Engine.WaitMessageSet(LanguageManager.FormatText(waitingMessage, (waitingSecs - i).ToString()), true);
						if (CancelRequested)
							break;

						Sleep(1000);
					}
				}
			}

			if (oneConnectionReached == false)
			{
				if (CancelRequested)
				{
					Engine.Logs.Log(LogType.Info, LanguageManager.GetText("SessionCancel"));
				}
				else
				{
					Engine.Logs.Log(LogType.Error, LanguageManager.GetText("SessionFailed"));
				}
			}

			if (oneConnectionReached == true)
			{
				Platform.Instance.FlushDNS();
			}

			Engine.Instance.WaitMessageClear();

			Engine.CurrentServer = null;
		}



		public void SetResetError()
		{
			if (m_reset == "")
				SetReset("ERROR");
		}

		public void SetReset(string level)
		{
			// 2.11.8
			if (level == "")
				m_reset = "";
			else if (m_reset == "")
				m_reset = level;
		}

		public bool InReset
		{
			get
			{
				return (m_reset != "");
			}
		}

		public bool GetConnected()
		{
			return m_connected;
		}

		public void SetConnected(bool connected)
		{
			if (connected == m_connected)
				return;

			m_connected = connected;

			Engine.UpdateConnectedStatus(connected);
		}


		public void AddLogEvent(string source, string message)
		{
			lock (m_logEvents)
			{
				string[] lines = message.Split('\n');
				foreach (string line in lines)
				{
					string lineN = line.TrimChars("\t\r\n ");

					if (m_connection != null)
						lineN = m_connection.AdaptMessage(lineN).Trim();

					if (lineN != "")
						m_logEvents.Add(new SessionLogEvent(source, lineN));
				}
			}
		}

		void ProcessLogsEvents()
		{
			List<SessionLogEvent> events = null; // Avoid running ProcessLogEvent with m_logEvents locked

			lock (m_logEvents)
			{
				events = new List<SessionLogEvent>(m_logEvents);
				m_logEvents.Clear();
			}

			foreach (SessionLogEvent logEvent in events)
			{
				ProcessLogEvent(logEvent);
			}
		}

		void ProcessLogEvent(SessionLogEvent logEvent)
		{
			ProcessLogEvent(logEvent.Source, logEvent.Message);
		}

		void ProcessLogEvent(string source, string message)
		{
			try
			{
				Platform.Instance.OnSessionLogEvent(source, message);

				if (m_connection != null)
					m_connection.OnLogEvent(source, message);
			}
			catch (Exception ex)
			{
				Engine.Logs.Log(LogType.Warning, ex);

				SetReset("ERROR");
			}
		}

		public void SetTunNetworkInterface(NetworkInterface adapter)
		{
			m_connection.Interface = adapter;

			m_interfaceScope = new InterfaceScope(m_connection.Interface.Id);

			Json jInfo = Engine.Instance.FindNetworkInterfaceInfo(m_connection.Interface.Id);

			if ((m_connection.ConfigIPv4) && (jInfo != null) && (jInfo.HasKey("support_ipv4")) && (Conversions.ToBool(jInfo["support_ipv4"].Value) == false))
			{
				Engine.Instance.Logs.LogWarning(LanguageManager.GetText("IPv4NotSupportedByNetworkAdapter"));
				if ((Engine.Instance.Options.GetBool("network.ipv4.autoswitch")) && (Engine.Instance.Options.Get("network.ipv4.mode") != "block"))
				{
					Engine.Instance.Logs.LogWarning(LanguageManager.GetText("IPv4NotSupportedByNetworkAdapterAutoSwitch"));
					Engine.Instance.Options.Set("network.ipv4.mode", "block");
				}
			}

			if ((m_connection.ConfigIPv6) && (jInfo != null) && (jInfo.HasKey("support_ipv6")) && (Conversions.ToBool(jInfo["support_ipv6"].Value) == false))
			{
				Engine.Instance.Logs.LogWarning(LanguageManager.GetText("IPv6NotSupportedByNetworkAdapter"));
				if ((Engine.Instance.Options.GetBool("network.ipv6.autoswitch")) && (Engine.Instance.Options.Get("network.ipv6.mode") != "block"))
				{
					Engine.Instance.Logs.LogWarning(LanguageManager.GetText("IPv6NotSupportedByNetworkAdapterAutoSwitch"));
					Engine.Instance.Options.Set("network.ipv6.mode", "block");
				}
			}
		}

		public void RoutesApply(string phase)
		{
			foreach (ConnectionRoute route in m_connection.Routes)
			{
				string action = "skip";
				if (phase == "pre")
				{
					if (route.Gateway != "vpn_gateway")
					{
						action = "add";
					}
				}
				else if (phase == "up")
				{
					if (route.Gateway == "vpn_gateway")
					{
						action = "add";
					}
				}
				else if (phase == "post")
				{
					action = "remove";
				}

				if (action != "skip")
				{
					Json jRoute = RouteCompute(route);
					if (jRoute != null)
						Platform.Instance.Route(jRoute, action);
				}
			}
		}

		public Json RouteCompute(ConnectionRoute route)
		{
			Json jRoute = new Json();
			jRoute["destination"].Value = route.Destination.ToCIDR(true);

			if (route.Gateway == "vpn_gateway")
			{
				if (m_connection.Interface == null) // If don't reach connection
					return null;

				jRoute["interface"].Value = m_connection.Interface.Id;
			}
			else if (route.Gateway == "net_gateway")
			{
				if (route.Destination.IsV4)
				{
					IpAddress netGateway = Engine.Instance.GetDefaultGatewayIPv4();
					if (netGateway == null)
					{
						Engine.Instance.Logs.LogVerbose("Routes, skipped for " + route.Destination.ToCIDR() + ": IPv4 Net gateway not available.");
						return null;
					}
					else
					{
						jRoute["gateway"].Value = netGateway.Address;
						jRoute["interface"].Value = Engine.Instance.GetDefaultInterfaceIPv4();
					}
				}
				else if (route.Destination.IsV6)
				{
					IpAddress netGateway = Engine.Instance.GetDefaultGatewayIPv6();
					if (netGateway == null)
					{
						Engine.Instance.Logs.LogVerbose("Routes, skipped for " + route.Destination.ToCIDR() + ": IPv6 Net gateway not available.");
						return null;
					}
					else
					{
						jRoute["gateway"].Value = netGateway.Address;
						jRoute["interface"].Value = Engine.Instance.GetDefaultInterfaceIPv6();
					}
				}
				else
					return null;
			}
			else
			{
				// ClodoTemp: Unsupported on Windows for now, we need the interface.
				IpAddress ip = new IpAddress(route.Gateway);
				if (ip.Valid == false)
				{
					Engine.Instance.Logs.LogWarning("Gateway " + route.Gateway + " invalid.");
					return null;
				}
				else if ((route.Destination.IsV4) && (ip.IsV6))
				{
					Engine.Instance.Logs.LogWarning("Gateway " + route.Gateway + " is IPv6 but used for IPv4 address.");
					return null;
				}
				else if ((route.Destination.IsV6) && (ip.IsV4))
				{
					Engine.Instance.Logs.LogWarning("Gateway " + route.Gateway + " is IPv4 but used for IPv6 address.");
					return null;
				}
				else
				{
					jRoute["gateway"].Value = ip.Address;
				}
			}

			jRoute["metric"].Value = 0; // Lowest

			return jRoute;
		}

		public void ConnectedStep()
		{
			if (m_connection.Interface == null)
				throw new Exception(LanguageManager.GetText("UnexpectedInterfaceNotRecognized"));

			Platform.Instance.OnInterfaceDo(m_connection.Interface);

			// DNS Switch
			if (Engine.Instance.Options.GetBool("dns.delegate") == false)
			{
				IpAddresses dns = m_connection.GetDns();

				if (m_connection.ConfigIPv4 == false) dns = dns.OnlyIPv6;
				if (m_connection.ConfigIPv6 == false) dns = dns.OnlyIPv4;

				if (dns.Count != 0)
					Platform.Instance.OnDnsSwitchDo(m_connection, dns);
			}

			RoutesApply("up");

			Engine.WaitMessageSet(LanguageManager.GetText("ConnectionFlushDNS"), true);

			Platform.Instance.FlushDNS();

			// 2.4: Sometime (only under Windows) Interface is not really ready...
			if (Platform.Instance.WaitTunReady(m_connection) == false)
				SetReset("ERROR");

			if (m_connection.ExitIPs.Count == 0)
			{
				Engine.WaitMessageSet(LanguageManager.GetText("ConnectionDetectExit"), true);
				Engine.Logs.Log(LogType.Verbose, LanguageManager.GetText("ConnectionDetectExit"));
				m_connection.ExitIPs.Add(Engine.Instance.DiscoverExit());
			}

			Engine.Instance.NetworkLockManager.OnVpnEstablished();

			m_connection.Info.Provider.OnVpnEstablished(this);

			if (m_reset == "")
			{
				Engine.RunEventCommand("vpn.up");

				Engine.Logs.Log(LogType.InfoImportant, LanguageManager.GetText("ConnectionConnected"));
				SetConnected(true);
				m_connection.TimeStart = DateTime.UtcNow;

				if (Engine.Instance.Options.GetBool("advanced.testonly"))
					Engine.RequestStop();
			}
		}
	}
}