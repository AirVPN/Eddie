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
using System.Web;
using System.Xml;
using Eddie.Core;

namespace Eddie.Core.Threads
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
		private Elevated.Process m_processOpenVpn;
		private Process m_processProxy;

		// TOCLEAN_OPENVPNMANAGEMENT
		/*
		private Socket m_openVpnManagementSocket;
		private List<string> m_openVpnManagementCommands = new List<string>();
		private List<string> m_openVpnManagementStatisticsLines = new List<string>();
		*/

		private string m_reset = "";
		private List<SessionLogEvent> m_logEvents = new List<SessionLogEvent>();
		List<ConnectionActiveRoute> m_routes = new List<ConnectionActiveRoute>();
		
		private TemporaryFile m_fileSshKey;
		private TemporaryFile m_fileSslCrt;
		private TemporaryFile m_fileSslConfig;		
		private ProgramScope m_programScope = null;
		private InterfaceScope m_interfaceScope = null;

		private ConnectionActive m_connectionActive = null;

        private string m_specialAirParseStep = "";
        private int m_specialAirParseStepInt = -1;

        private bool m_isHummingbird = false;

        public override void OnRun()
		{
			m_isHummingbird = (Engine.Instance.GetOpenVpnTool() is Tools.Hummingbird);

			CancelRequested = false;

			Engine.SessionStatsStart = DateTime.UtcNow;
			Engine.SessionStatsRead = 0;
			Engine.SessionStatsWrite = 0;

			string sessionLastServer = "";

			bool oneConnectionReached = false;

			for (; CancelRequested == false;)
			{
				RouteScope routeScope = null;

				bool allowed = true;
				string waitingMessage = "";
				int waitingSecs = 0;
				m_processOpenVpn = null;
				m_processProxy = null;

                m_specialAirParseStep = "";
                m_specialAirParseStepInt = 0;

                m_routes.Clear();

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

                    string forceServer = Engine.Storage.Get("server");
                    if ((Engine.NextServer == null) && (forceServer != ""))
                    {
                        Engine.NextServer = Engine.PickConnectionByName(forceServer);
                    }

                    if ((Engine.NextServer == null) && (Engine.Storage.GetBool("servers.startlast")))
                    {
                        Engine.NextServer = Engine.PickConnection(Engine.Storage.Get("servers.last"));
                    }

                    if ((Engine.NextServer == null) && (Engine.Storage.GetBool("servers.locklast")) && (sessionLastServer != ""))
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

					// TOCLEAN_OPENVPNMANAGEMENT
					// m_openVpnManagementCommands.Clear();					

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
								catch (Exception e)
								{
									// Note: If failed, continue anyway.
									Engine.Logs.Log(LogType.Warning, LanguageManager.GetText("AuthorizeConnectFailed", e.Message));
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
											Engine.CurrentServer.Penality += Engine.Storage.GetInt("advanced.penality_on_error");
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

                    if(Engine.CurrentServer.SupportIPv6)
					{
						bool osSupport = Conversions.ToBool(Engine.Instance.Manifest["network_info"]["support_ipv6"].Value);
						if( (osSupport == false) && (Engine.Instance.Storage.GetLower("network.ipv6.mode") != "block"))
						{
							Engine.Instance.Logs.LogWarning(LanguageManager.GetText("IPv6NotSupportedByOS"));
							if ((Engine.Instance.Storage.GetBool("network.ipv6.autoswitch")) && (Engine.Instance.Storage.Get("network.ipv6.mode") != "block"))
							{
								Engine.Instance.Logs.LogWarning(LanguageManager.GetText("IPv6NotSupportedByNetworkAdapterAutoSwitch"));
								Engine.Instance.Storage.Set("network.ipv6.mode", "block");
							}
						}	
					}

					try
					{
						m_connectionActive = Engine.CurrentServer.BuildConnectionActive(false);

						if (Engine.Instance.Storage.GetBool("advanced.skip_tun_detect") == false)
						{
							string driverRequested = Platform.Instance.GetOvpnDriverRequested(m_connectionActive.OpenVpnProfileStartup);
							Engine.Instance.WaitMessageSet(LanguageManager.GetText("OsDriverInstall", driverRequested), false);
							Platform.Instance.EnsureDriverAndAdapterAvailable(driverRequested);
						}

						Engine.WaitMessageSet(LanguageManager.GetText("ConnectionCredentials"), true);
						if (Engine.CurrentServer.Provider.ApplyCredentials(m_connectionActive) == false)
						{
							allowed = false;
							CancelRequested = true;
							SetReset("FATAL");
						}
						else
						{
							m_connectionActive.FillProfileWithPush();
							Engine.Instance.ConnectionActive = m_connectionActive;
						}

                        if (m_connectionActive.Protocol == "SSH")
                        {
                            m_connectionActive.EntryIP = m_connectionActive.Address;
                            m_connectionActive.Port = m_connectionActive.SshRemotePort;
                        }
                        else if (m_connectionActive.Protocol == "SSL")
                        {
                            m_connectionActive.EntryIP = m_connectionActive.Address;
                            m_connectionActive.Port = m_connectionActive.SslRemotePort;
                        }
						else
						{
							// Detected after from logs
						}
                    }
					catch (Exception e)
					{
						Engine.Logs.Log(e);
						allowed = false;
						SetReset("ERROR");
					}

					if (allowed)
					{
						// Need IPv6 block?
						{
							if (Engine.Instance.GetNetworkIPv6Mode() == "block")
								m_connectionActive.BlockedIPv6 = true;
							
							if ((Engine.CurrentServer.SupportIPv6 == false) && (Engine.Instance.GetNetworkIPv6Mode() == "in-block"))
								m_connectionActive.BlockedIPv6 = true;

							if ((Engine.Instance.GetOpenVpnTool().VersionUnder("2.4")) && (Engine.Instance.GetNetworkIPv6Mode() == "in-block"))
								m_connectionActive.BlockedIPv6 = true;

							if (m_connectionActive.BlockedIPv6)
								Platform.Instance.OnIPv6Block();
						}

						m_connectionActive.ExitIPs.Add(Engine.CurrentServer.IpsExit.Clone());

						sessionLastServer = Engine.CurrentServer.Code;
						Engine.Storage.Set("servers.last", Engine.CurrentServer.Code);

						routeScope = new RouteScope(m_connectionActive.EntryIP); // Clodo: Urgent, may not work under some OS with NetLock active. Try to add the RouteScope when detecting protocol from OpenVPN logs.

						Engine.RunEventCommand("vpn.pre");

						string connectingMessage = LanguageManager.GetText("ConnectionConnecting", Engine.CurrentServer.GetNameWithLocation());
						Engine.WaitMessageSet(connectingMessage, true);
						Engine.Logs.Log(LogType.InfoImportant, connectingMessage);

						// 2.14.0
						foreach (ConnectionActiveRoute route in m_connectionActive.Routes)
						{
							if ((route.Address.IsV6) || (Constants.FeatureAlwaysBypassOpenvpnRoute))
							{
								if (route.Gateway != "vpn_gateway")
								{
									if (route.Add(m_connectionActive))
										m_routes.Add(route);
								}
							}
						}

						if (m_connectionActive.Protocol == "SSH")
						{
							StartSshProcess();
						}
						else if (m_connectionActive.Protocol == "SSL")
						{
							StartSslProcess();
						}
						else
						{
							StartOpenVpnProcess();
						}

						int waitingSleep = 100; // To avoid CPU stress

						SetReset("");

						// -----------------------------------
						// Phase 2: Waiting connection
						// -----------------------------------

						for (; ; )
						{
							ProcessLogsEvents();

							if ((m_processOpenVpn != null) && (m_processOpenVpn.ReallyExited)) // 2.2
								SetReset("ERROR");
							if ((m_processProxy != null) && (m_processProxy.ReallyExited)) // 2.2
								SetReset("ERROR");

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

								int timeNow = Utils.UnixTimeStamp();

								if (Engine.IsConnected() == false)
									throw new Exception("Unexpected.");

								// TOCLEAN_OPENVPNMANAGEMENT
								// ProcessOpenVpnManagement();

								// Need stop?
								bool StopRequest = false;

								if (m_reset == "RETRY")
								{
									StopRequest = true;
								}
								else if (m_reset == "ERROR")
								{
									//Engine.CurrentServer.Penality += Engine.Storage.GetInt("advanced.penality_on_error"); // Removed in 2.11
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

						if (m_reset == "ERROR") // Added in 2.11
						{
							Engine.CurrentServer.Penality += Engine.Storage.GetInt("advanced.penality_on_error");
						}

						// -----------------------------------
						// Phase 4 - Start disconnection
						// -----------------------------------

						Engine.SetConnected(false);

						Engine.WaitMessageSet(LanguageManager.GetText("ConnectionDisconnecting"), false);
						Engine.Logs.Log(LogType.InfoImportant, LanguageManager.GetText("ConnectionDisconnecting"));

						// 2.14.0
						foreach (ConnectionActiveRoute route in m_routes)
						{
							route.Remove(m_connectionActive);
						}
						m_routes.Clear();

						// -----------------------------------
						// Phase 5 - Waiting disconnection
						// -----------------------------------


						int lastSignalTime = 0;
						string lastSignalType = "none";

						for (; ; )
						{
							try
							{
								ProcessLogsEvents();

								int now = Utils.UnixTimeStamp();

								// TOCLEAN_OPENVPNMANAGEMENT
								/*
								// OpenVPN process completed, but management socket still opened. Strange, but happen. Closing socket.
								if ((m_processOpenVpn != null) && (m_openVpnManagementSocket != null) && (m_processOpenVpn.ReallyExited == true) && (m_openVpnManagementSocket.Connected))
									m_openVpnManagementSocket.Close();
								*/

								// OpenVPN process still exists, but management socket is not connected. We can't tell to OpenVPN to do a plain disconnection, force killing.
								if ((m_processOpenVpn != null) && (m_processOpenVpn.ReallyExited == false))
								{
									// TOCLEAN_OPENVPNMANAGEMENT
									//if ((m_openVpnManagementSocket == null) || (m_openVpnManagementSocket.Connected == false))
									if(true)
									{
										if (now - lastSignalTime >= 10)
										{
											lastSignalTime = now;

											if ((lastSignalType == "none") || (lastSignalType == "management"))
											{
												lastSignalType = "soft";
												Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("KillWithSoft"));												
												m_processOpenVpn.KillSoft();
											}
											else if ((lastSignalType == "soft") || (lastSignalType == "hard"))
											{
												lastSignalType = "hard";
												Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("KillWithHard"));
												m_processOpenVpn.Kill();
											}
										}
									}
								}

								// Proxy (SSH/SSL) process
								if ((m_processProxy != null) && (m_processOpenVpn != null) && (m_processProxy.ReallyExited == false) && (m_processOpenVpn.ReallyExited == true))
								{
									if (now - lastSignalTime >= 10)
									{
										lastSignalTime = now;

										if ((lastSignalType == "none") || (lastSignalType == "management"))
										{
											lastSignalType = "soft";
											Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("KillWithSoft"));
											Platform.Instance.ProcessKillSoft(m_processProxy);
										}
										else if ((lastSignalType == "soft") || (lastSignalType == "hard"))
										{
											lastSignalType = "hard";
											Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("KillWithHard"));
											m_processProxy.Kill();
										}
									}
								}

								// TOCLEAN_OPENVPNMANAGEMENT
								/*
								// Start a clean disconnection
								if ((m_processOpenVpn != null) && (m_openVpnManagementSocket != null) && (m_processOpenVpn.ReallyExited == false) && (m_openVpnManagementSocket.Connected))
								{
									if (now - lastSignalTime >= 10)
									{
										lastSignalTime = now;
										lastSignalType = "management";
										Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("KillWithManagement"));
										SendManagementCommand("signal SIGTERM");
										ProcessOpenVpnManagement();
									}
								}
								*/
							}
							catch (Exception e)
							{
								Engine.Logs.Log(LogType.Warning, e);
							}

							bool exit = true;

							// TOCLEAN_OPENVPNMANAGEMENT
							/*
							if ((m_openVpnManagementSocket != null) && (m_openVpnManagementSocket.Connected))
								exit = false;
							*/

							if ((m_processProxy != null) && (m_processProxy.ReallyExited == false))
								exit = false;

							if ((m_processOpenVpn != null) && (m_processOpenVpn.ReallyExited == false))
								exit = false;

							if (exit)
								break;

							Sleep(waitingSleep);
						}

						// -----------------------------------
						// Phase 6: Cleaning, waiting before retry.
						// -----------------------------------

						Engine.Logs.Log(LogType.Verbose, LanguageManager.GetText("ConnectionStop"));

						Engine.RunEventCommand("vpn.down");

						if (m_connectionActive.BlockedIPv6)
						{
							Platform.Instance.OnIPv6Restore();
							m_connectionActive.BlockedIPv6 = false;
						}

						//Platform.Instance.OnRouteDefaultRemoveRestore();

						Platform.Instance.OnDnsSwitchRestore();

						Platform.Instance.OnInterfaceRestore();

						if (m_processOpenVpn != null)
						{
							m_processOpenVpn.Stop();
							m_processOpenVpn = null;
						}

						if (m_processProxy != null)
						{
							m_processProxy.Dispose();
							m_processProxy = null;
						}

						// Closing temporary files
						if (m_fileSshKey != null)
						{
							m_fileSshKey.Close();
							m_fileSshKey = null;
						}
						if (m_fileSslCrt != null)
						{
							m_fileSslCrt.Close();
							m_fileSslCrt = null;
						}
						if (m_fileSslConfig != null)
						{
							m_fileSslConfig.Close();
							m_fileSslConfig = null;
						}
						if (m_connectionActive != null)
						{
							m_connectionActive.Close();
							m_connectionActive = null;
						}

						Engine.Instance.ConnectionActive = null;


						// TOCLEAN_OPENVPNMANAGEMENT
						/*
						if (m_openVpnManagementSocket != null)
						{
							(m_openVpnManagementSocket as IDisposable).Dispose();
							m_openVpnManagementSocket = null;
						}
						*/

						ProcessLogsEvents();
					}

				}
				catch (Exception e)
				{
					// Warning: Avoid to reach this catch: unpredicable status of running processes.
					Engine.SetConnected(false);

					Engine.Logs.Log(LogType.Error, LanguageManager.GetText("FatalUnexpected", e.Message + " - " + e.StackTrace));
				}

				if (routeScope != null)
					routeScope.End();

				if (m_programScope != null)
					m_programScope.End();

				if (m_interfaceScope != null)
					m_interfaceScope.End();

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
						//Engine.Log(Engine.LogType.Verbose, waitingMessage);
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

		private void StartSshProcess()
		{
			string sshToolPath = Software.GetTool("ssh").Path;
			bool isPlink = (sshToolPath.ToLowerInvariant().EndsWith("plink.exe", StringComparison.InvariantCulture));

			Providers.Service service = Engine.CurrentServer.Provider as Providers.Service;
			if (service == null) // Unexpected
				return;

			if (m_processProxy != null) // Unexpected
				return;

			string fileKeyExtension = "key";			
			if(isPlink)
				fileKeyExtension = "ppk";

			m_fileSshKey = new TemporaryFile(fileKeyExtension);
			Platform.Instance.FileContentsWriteText(m_fileSshKey.Path, service.User.GetAttributeString("ssh_" + fileKeyExtension, ""), Encoding.ASCII);
						
			if( (Platform.Instance.IsWindowsSystem()) && (fileKeyExtension == "key") )
			{
				// For Windows10 SSH
				Platform.Instance.FileEnsureCurrentUserOnly(m_fileSshKey.Path);				
			}				

			if (Platform.Instance.IsUnixSystem())
			{
				Platform.Instance.FileEnsurePermission(m_fileSshKey.Path, "600");
			}

			string arguments = "";
			arguments += " -i \"" + m_fileSshKey.Path + "\"";
			arguments += " -L " + Conversions.ToString(m_connectionActive.SshLocalPort);
			if (m_connectionActive.Address.IsV6)
				arguments += ":[::1]";
			else
				arguments += ":127.0.0.1";
			arguments += ":" + Conversions.ToString(m_connectionActive.SshPortDestination);
			arguments += " sshtunnel@" + m_connectionActive.Address;

			if (isPlink == false)
				arguments += " -p"; // ssh use -p
			else
				arguments += " -P"; // plink use -P
			arguments += " " + m_connectionActive.SshRemotePort;

			// TOOPTIMIZE: To bypass key confirmation. Not the best approach.
			// TOFIX: Maybe provide a UserKnownHostsFile...
			if (fileKeyExtension == "key")
				arguments += " -o UserKnownHostsFile=/dev/null -o StrictHostKeyChecking=no"; 

			arguments += " -N -T -v";

			m_programScope = new ProgramScope(Software.GetTool("ssh").Path, "SSH Tunnel");

			m_processProxy = new Process();
			m_processProxy.StartInfo.FileName = sshToolPath;
			m_processProxy.StartInfo.Arguments = arguments;
			m_processProxy.StartInfo.WorkingDirectory = Platform.Instance.DirectoryTemp();

			m_processProxy.StartInfo.Verb = "run";
			m_processProxy.StartInfo.CreateNoWindow = true;
			m_processProxy.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			m_processProxy.StartInfo.UseShellExecute = false;
			m_processProxy.StartInfo.RedirectStandardInput = true;
			m_processProxy.StartInfo.RedirectStandardError = true;
			m_processProxy.StartInfo.RedirectStandardOutput = true;

			m_processProxy.ErrorDataReceived += ProcessSshOutputDataReceived;
			m_processProxy.OutputDataReceived += ProcessSshOutputDataReceived;
			m_processProxy.Exited += ProcessSshExited;

			m_processProxy.Start();

			m_processProxy.BeginOutputReadLine();
			m_processProxy.BeginErrorReadLine();
		}

		void ProcessSshExited(object sender, EventArgs e)
		{
			if (m_reset == "")
				SetReset("ERROR");
		}

		private void StartSslProcess()
		{
			Providers.Service service = Engine.CurrentServer.Provider as Providers.Service;
			if (service == null) // Unexpected
				return;

			if (m_processProxy != null) // Unexpected
				return;

			m_fileSslCrt = new TemporaryFile("crt");
			Platform.Instance.FileContentsWriteText(m_fileSslCrt.Path, service.User.GetAttributeString("ssl_crt", ""), Encoding.ASCII);

			m_fileSslConfig = new TemporaryFile("ssl");

			string sslConfig = "";

			if (Platform.Instance.IsUnixSystem())
			{
				//sslConfig += "output = /dev/stdout\n"; // With this, with new stunnel 5.01, we have duplicated output dump.
				sslConfig += "foreground = yes\n";	// Without this, the process fork and it's exit can't be detected.
			}
			if (Engine.Instance.Storage.Get("ssl.options") != "")
				sslConfig += "options = " + Engine.Instance.Storage.Get("ssl.options") + "\n";
			sslConfig += "client = yes\n";
			sslConfig += "debug = 6\n";
			if (Platform.Instance.IsUnixSystem())
				sslConfig += "pid = " + Engine.Instance.GetPathInData("stunnel.pid"); // Added 2.18.3. Note: don't like quoted path
			sslConfig += "\n";
			sslConfig += "[openvpn]\n";
			sslConfig += "accept = 127.0.0.1:" + Conversions.ToString(m_connectionActive.SslLocalPort) + "\n";
			sslConfig += "connect = " + m_connectionActive.Address + ":" + m_connectionActive.SslRemotePort + "\n";
			sslConfig += "TIMEOUTclose = 0\n";
			if (Engine.Instance.Storage.GetInt("ssl.verify") != -1)
				sslConfig += "verify = " + Engine.Instance.Storage.GetInt("ssl.verify").ToString() + "\n";
			sslConfig += "CAfile = " + m_fileSslCrt.Path + "\n"; // Note: don't like quoted path
            sslConfig += "\n";

			string sslConfigPath = m_fileSslConfig.Path;
			Platform.Instance.FileContentsWriteText(sslConfigPath, sslConfig, Encoding.UTF8);

			m_programScope = new ProgramScope(Software.GetTool("ssl").Path, "SSL Tunnel");

			m_processProxy = new Process();
			m_processProxy.StartInfo.FileName = Software.GetTool("ssl").Path;			
			m_processProxy.StartInfo.Arguments = "\"" + Encoding.Default.GetString(Encoding.UTF8.GetBytes(sslConfigPath)) + "\""; // encoding workaround, stunnel expect utf8
			m_processProxy.StartInfo.WorkingDirectory = Platform.Instance.DirectoryTemp();

			m_processProxy.StartInfo.Verb = "run";
			m_processProxy.StartInfo.CreateNoWindow = true;
			m_processProxy.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			m_processProxy.StartInfo.UseShellExecute = false;
			m_processProxy.StartInfo.RedirectStandardInput = true;
			m_processProxy.StartInfo.RedirectStandardError = true;
			m_processProxy.StartInfo.RedirectStandardOutput = true;

			m_processProxy.ErrorDataReceived += ProcessSslOutputDataReceived;
			m_processProxy.OutputDataReceived += ProcessSslOutputDataReceived;
			m_processProxy.Exited += ProcessSslExited;

			m_processProxy.Start();

			m_processProxy.BeginOutputReadLine();
			m_processProxy.BeginErrorReadLine();
		}

		void ProcessSslExited(object sender, EventArgs e)
		{
			if (m_reset == "")
				SetReset("ERROR");
		}

		private void StartOpenVpnProcess()
		{
			if (m_processOpenVpn != null) // Unexpected
				return;

			m_connectionActive.InitStart();

            string path = Engine.Instance.GetOpenVpnTool().Path;

            if (m_processProxy == null)
				m_programScope = new ProgramScope(path, "OpenVPN Tunnel");

			m_processOpenVpn = new Elevated.Process();
            if(m_isHummingbird)
			{
				m_processOpenVpn.Command.Parameters["command"] = "hummingbird";
				m_processOpenVpn.Command.Parameters["gui-version"] = Constants.Name + Constants.VersionDesc;
			}
			else
			{
				m_processOpenVpn.Command.Parameters["command"] = "process_openvpn";
			}
			m_processOpenVpn.Command.Parameters["path"] = path;
			m_processOpenVpn.Command.Parameters["config"] = m_connectionActive.OvpnFile.Path;
			m_processOpenVpn.StdOut.LineEvent += ProcessOpenVpnOutputDataReceived;
			m_processOpenVpn.StdErr.LineEvent += ProcessOpenVpnOutputDataReceived;
			m_processOpenVpn.EndEvent += ProcessOpenVpnExited;
			m_processOpenVpn.Start();
		}
		
		void ProcessOpenVpnExited()
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

			// 2.11.7 version, for reference
			//m_reset = level;
		}

		public bool InReset
		{
			get
			{
				return (m_reset != "");
			}
		}

		// TOCLEAN_OPENVPNMANAGEMENT
		/*
		public bool SendManagementCommand(string cmd)
		{
			if (m_openVpnManagementSocket == null)
				return false;

			if (m_openVpnManagementSocket.Connected == false)
				return false;

			lock (this)
			{
				m_openVpnManagementCommands.Add(cmd);
				return true;
			}
		}
		*/

		void ProcessSshOutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (e.Data != null)
			{
				string message = e.Data.ToString();

				AddLogEvent("SSH", message);
			}
		}

		void ProcessSslOutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (e.Data != null)
			{
				string message = e.Data.ToString();

				// Remove STunnel timestamp
				message = System.Text.RegularExpressions.Regex.Replace(message, "^\\d{4}\\.\\d{2}\\.\\d{2}\\s+\\d{2}:\\d{2}:\\d{2}\\s+LOG\\d{1}\\[\\d{0,6}:\\d{0,60}\\]:\\s+", "");

				AddLogEvent("SSL", message);
			}
		}

		void ProcessOpenVpnOutputDataReceived(string data)
		{
            if (m_isHummingbird)
            {
                // Remove Hummingbird timestamp
                // Example: "Sat Oct 12 10:15:54.795 2019"
                data = System.Text.RegularExpressions.Regex.Replace(data, "^\\w{3}\\s+\\w{3}\\s+\\d{1,2}\\s+\\d{1,2}:\\d{1,2}:\\d{1,2}\\.\\d{0,3}\\s+\\d{2,4}\\s+", "");
                AddLogEvent("Hummingbird", data);
            }
            else
            {
                // Remove OpenVPN timestamp
                // Example OpenVPN<3: "Sat Oct 12 10:13:38 2019 /sbin/ip route add 0.0.0.0/1 via 10.20.6.1"
                data = System.Text.RegularExpressions.Regex.Replace(data, "^\\w{3}\\s+\\w{3}\\s+\\d{1,2}\\s+\\d{1,2}:\\d{1,2}:\\d{1,2}\\s+\\d{2,4}\\s+", "");
                AddLogEvent("OpenVPN", data);
            }
		}

		// TOCLEAN_OPENVPNMANAGEMENT
		/*
		void ProcessOpenVpnManagement()
		{
			try
			{
				// Fetch OpenVPN Management
				if (m_openVpnManagementSocket != null)
				{
					if (m_openVpnManagementSocket.Connected == false)
						throw new Exception("OpenVPN Management disconnected.");

					lock (this)
					{
						foreach (string command in m_openVpnManagementCommands)
						{
							bool log = true;
							if (command == "status")
								log = false;
							if (command == m_connectionActive.ManagementPassword)
								log = false;

							if (log)
								Engine.Logs.Log(LogType.Verbose, "Management - Send '" + command + "'");

							string MyCmd = command + "\n";
							Byte[] bufS = new byte[1024 * 16];
							int lenS = Encoding.ASCII.GetBytes(MyCmd, 0, MyCmd.Length, bufS, 0);

							m_openVpnManagementSocket.Send(bufS, lenS, SocketFlags.None);
						}
						m_openVpnManagementCommands.Clear();
					}

					// Fetch OpenVPN Management
					if (m_openVpnManagementSocket.Available != 0)
					{
						Byte[] buf = new byte[1024 * 16];
						int bytes = m_openVpnManagementSocket.Receive(buf, buf.Length, 0);

						string data = Encoding.ASCII.GetString(buf, 0, bytes);

						AddLogEvent("Management", data);
					}
				}
			}
			catch (Exception ex)
			{
				Engine.Logs.Log(LogType.Warning, ex);

				SetReset("ERROR");
			}
		}
		*/

		void AddLogEvent(string source, string message)
		{
            lock(m_logEvents)
            {
                string[] lines = message.Split('\n');
                foreach (string line in lines)
                {
                    if (line.Trim() != "")
                        m_logEvents.Add(new SessionLogEvent(source, line.Trim()));
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
			string messageLower = message.ToLowerInvariant(); // Try to match lower/insensitive case when possible.

			if (messageLower.Trim() == "") // 2.10.1
				return;

			try
			{
                if (source == "Hummingbird")
                {
                    bool log = true;
                    LogType logType = LogType.Verbose;

                    // Level
                    if (messageLower.StartsWithInv("warning:"))
                        logType = LogType.Warning;
                    if (messageLower.StartsWithInv("warn:"))
                        logType = LogType.Warning;
                    if (messageLower.StartsWithInv("error:"))
                        logType = LogType.Error;
                    if (messageLower.StartsWithInv("fatal:"))
                        logType = LogType.Error;
                    if (messageLower.StartsWithInv("options error:"))
                        logType = LogType.Warning;

                    if (message.Contains("WARNING: Network filter and lock is disabled"))
                    {
                        // Don't warn user, are managed by Eddie.
                        log = false;
                    }
                    else if (message.Contains("ERROR: cannot detect IPv6 default gateway"))
                    {
                        // Don't want user, a client can not have IPv6 connectivity (default gateway) but VPN provide it.
                        log = false;
                    }
                    else if (message.RegExMatch("Pushed DNS Server (.+?) ignored"))
                    {
                        // Don't warn user, not true, are managed by Eddie.
                        log = false;
                    }
                    else if (message.StartsWithInv("OpenVPN core"))
                    {
                        // First feedback from Hummingbird process. We can remove temporary files.
                        m_connectionActive.CleanAfterStart();
                    }
                    else if (m_specialAirParseStep == "options")
                    {
                        string[] fields = message.Split(' '); // Note: Not best approach, but output is temporary/test
                        if (fields.Length > 0)
                        {
                            int x = Conversions.ToInt32(fields[0]);
                            if (x != m_specialAirParseStepInt)
                            {
                                m_specialAirParseStep = "";
                                m_specialAirParseStepInt = 0;
                            }
                            else
                            {
                                m_specialAirParseStepInt++;

                                string directive = "";
                                for (int f = 1; f < fields.Length; f++)
                                {
                                    string v = fields[f];
                                    v = v.TrimStart('[');
                                    v = v.TrimEnd(']');
                                    if (directive != "")
                                        directive += " ";
                                    directive += v;
                                }

                                m_connectionActive.OpenVpnProfileWithPush.AppendDirectives(directive, "Push");
                            }
                        }
                    }
                    else if (message.EndsWithInv("OPTIONS:"))
                    {
                        m_specialAirParseStep = "options";
                    }
                    else if (message.ContainsInv("EVENT: CONNECTED"))
                    {
                        ConnectedStep();
                    }
                    else if (message.StartsWithInv("SSL Handshake: "))
                    {
                        m_connectionActive.ControlChannel = message.Substring("SSL Handshake: ".Length);
                    }
                    else if (message.StartsWithInv("open ")) // MacOS
                    {
                        Match match = Regex.Match(message, "open (.+?) SUCCEEDED");
                        if (match.Success)
                            CheckTunNetworkInterface(match.Groups[1].Value);
                    }
                    else if (message.StartsWithInv("net_iface_up: ")) // Linux
                    {
                        Match match = Regex.Match(message, "net_iface_up: set (.+?) up");
                        if (match.Success)
                            CheckTunNetworkInterface(match.Groups[1].Value);
                    }
                    else if (message.StartsWithInv("Contacting "))
                    {
                        if ((m_connectionActive.Protocol != "SSH") && (m_connectionActive.Protocol != "SSL")) // Otherwise report 127.0.0.1
                        {
                            List<string> fields = message.RegExMatchSingle("Contacting ([a-z90-9\\.\\:]+?):(\\d+?)\\s");
                            if ((fields != null) && (fields.Count == 2))
                            {
                                m_connectionActive.EntryIP = fields[0];
                                m_connectionActive.Port = Conversions.ToInt32(fields[1]);
                            }
                        }
                    }

                    if (log)
                        Engine.Logs.Log(logType, source + " > " + message);
                }
                else if (source == "OpenVPN")
				{
					bool log = true;
					LogType logType = LogType.Verbose;

                    if ((m_connectionActive != null) && (m_connectionActive.OvpnFile != null))
                    {
                        // First feedback from OpenVPN process. We can remove temporary files.
                        m_connectionActive.CleanAfterStart();
                    }

                    // Ignore
                    if (message.IndexOfInv("MANAGEMENT: CMD 'status'") != -1)
						return;

					// Level
					if (messageLower.StartsWithInv("warning:"))
						logType = LogType.Warning;
					if (messageLower.StartsWithInv("warn:"))
						logType = LogType.Warning;
					if (messageLower.StartsWithInv("error:"))
						logType = LogType.Error;
					if (messageLower.StartsWithInv("fatal:"))
						logType = LogType.Error;
					if (messageLower.StartsWithInv("options error:"))
						logType = LogType.Warning;

					// Exception
					if (Platform.Instance.GetCode() != "MacOS")
					{
						// Unresolved issue, but don't want to warn users.
						// Under OpenVPN provider, file hash.tmp.ppw for auth, any permission flags cause denied or this warning. // TOFIX
						if ((messageLower.StartsWithInv("warning:")) && (messageLower.Contains("is group or others accessible")))
							logType = LogType.Verbose;
					}

					if (Engine.Instance.GetOpenVpnTool().VersionAboveOrEqual("2.4") == false)
					{
						// Don't warning users for correct behiavour (outside tunnel)
						if (
							(message.IndexOfInv("Options error: option 'redirect-gateway' cannot be used in this context ([PUSH-OPTIONS])") != -1) ||
							(message.IndexOfInv("Options error: option 'dhcp-option' cannot be used in this context ([PUSH-OPTIONS])") != -1)
							)
							log = false;
					}

					// Useless
					if (message.Contains("Use --help for more information."))
						log = false;

					// Ignore, caused by Windows method GenerateConsoleCtrlEvent to soft-kill
					if (message.Contains("win_trigger_event: WriteConsoleInput: The handle is invalid."))
						log = false;

					// Eddie delete any ovpn/crt/key/pwd files after the reading by OpenVPN.
					// If a connection drop, it's not OpenVPN that need to retry the connection, must performed by Eddie.
					// But unfortunately there isn't any method in OpenVPN to avoid the behiavour of SIGHUP (process restarting).
					// Neither the directive 'single-session'.
					// Anyway, when happen, OpenVPN can't find the .ovpn config, so effectively stop and Eddie will perform the retry.
					if ( (message.StartsWithInv("Options error:")) && (message.Contains("Error opening configuration file")) )
					{
						log = false;
					}

					// Under Windows, kind of errors when i programmatically send CTRL+C. Ignore it.
					if (message.Contains("win_trigger_event: WriteConsoleInput"))
						log = false;

					// If ncp-disable is used, this warnings are useless.
					if ((message.Contains("WARNING: 'cipher' is used inconsistently")) && (m_connectionActive.OpenVpnProfileWithPush.ExistsDirective("ncp-disable")))
						log = false;
					if ((message.Contains("WARNING: 'keysize' is used inconsistently")) && (m_connectionActive.OpenVpnProfileWithPush.ExistsDirective("ncp-disable")))
						log = false;

					// OpenVPN <2.4 don't have ncp-disable, and throw the same warnings if cipher specified client-side it's different from the server-side.
					if ((message.Contains("WARNING: 'cipher' is used inconsistently")) && (Engine.Instance.GetOpenVpnTool().VersionUnder("2.4")))
						log = false;
					if ((message.Contains("WARNING: 'keysize' is used inconsistently")) && (Engine.Instance.GetOpenVpnTool().VersionUnder("2.4")))
						log = false;

					if (message.StartsWithInv("Options error:"))
					{
						if (log)
						{
							List<string> matches = message.RegExMatchSingle("Options error\\: Unrecognized option or missing parameter\\(s\\) in (.*?)\\:\\d+\\:(.*?)\\(.*\\)");
							if ((matches != null) && (matches.Count == 2))
							{
								string context = matches[0].Trim();
								string unrecognizedOption = matches[1].Trim();

								if (context != "[PUSH-OPTIONS]")
								{
									Engine.Logs.Log(LogType.Fatal, LanguageManager.GetText("DirectiveError", unrecognizedOption));
									Engine.LogOpenvpnConfig();
									SetReset("FATAL");
								}
							}
						}
					}

					if (message.StartsWithInv("Control Channel: "))
					{
						m_connectionActive.ControlChannel = message.Substring("Control Channel: ".Length);
					}

					if (message.IndexOfInv("Connection reset, restarting") != -1)
					{
						SetReset("ERROR");
					}

					if (message.IndexOfInv("Exiting due to fatal error") != -1)
					{
						SetReset("ERROR");
					}

					if (message.IndexOfInv("SIGTERM[soft,ping-exit]") != -1) // 2.2
					{
						SetReset("ERROR");
					}

					if (message.IndexOfInv("SIGUSR1[soft,tls-error] received, process restarting") != -1)
					{
						SetReset("ERROR");
					}

					if (message.IndexOfInv("SIGUSR1[soft,tls-error] received, process restarting") != -1)
					{
						SetReset("ERROR");
					}

					Match matchSigReceived = Regex.Match(message, "SIG(.*?)\\[(.*?),(.*?)\\] received");
					if (matchSigReceived.Success)
					{
						SetReset("ERROR");
					}

					if (message.IndexOfInv("MANAGEMENT: Socket bind failed on local address") != -1)
					{
						Engine.Logs.Log(LogType.Verbose, LanguageManager.GetText("AutoPortSwitch"));

						Engine.Storage.SetInt("openvpn.management_port", Engine.Storage.GetInt("openvpn.management_port") + 1);

						SetReset("RETRY");
					}

					if (message.IndexOfInv("AUTH_FAILED") != -1)
					{
						Engine.Instance.CurrentServer.Provider.OnAuthFailed();

						SetReset("AUTH_FAILED");
					}

					if (message.IndexOfInv("MANAGEMENT: TCP Socket listening on") != -1)
					{
					}

					if (message.IndexOfInv("TLS: tls_process: killed expiring key") != -1)
					{
						Engine.Logs.Log(LogType.Verbose, LanguageManager.GetText("RenewingTls"));
					}

					if (message.IndexOfInv("Initialization Sequence Completed With Errors") != -1)
					{
						SetReset("ERROR");
					}

					// Detect connection (OpenVPN >2.4)
					if (messageLower.RegExMatchOne("peer connection initiated with \\[af_inet6?\\]([0-9a-f\\.\\:]+?):(\\d+?)") != "")
					{						
						if ((m_connectionActive.Protocol != "SSH") && (m_connectionActive.Protocol != "SSL"))
						{
							string t = messageLower;
							t = t.Replace("[nonblock]", "").Trim();
							List<string> fields = t.RegExMatchSingle("\\[af_inet6?\\]([a-z90-9\\.\\:]+?):(\\d+?)$");
							if( (fields != null) && (fields.Count == 2) )
							{
								m_connectionActive.EntryIP = fields[0];
								m_connectionActive.Port = Conversions.ToInt32(fields[1]);
							}
						}
					}

					// Detect TCP connection (OpenVPN <2.4)
					if (message.IndexOfInv("Attempting to establish TCP connection with [AF_INET]") != -1)
					{
						if (m_connectionActive.Protocol == "SSH")
						{
							m_connectionActive.EntryIP = m_connectionActive.Address;
							m_connectionActive.Port = m_connectionActive.SshRemotePort;
						}
						else if (m_connectionActive.Protocol == "SSL")
						{
							m_connectionActive.EntryIP = m_connectionActive.Address;
							m_connectionActive.Port = m_connectionActive.SslRemotePort;
						}
						else
						{
							string t = message;
							t = t.Replace("Attempting to establish TCP connection with", "").Trim();
							t = t.Replace("[nonblock]", "").Trim();
							t = t.Replace("[AF_INET]", "").Trim();
							string[] parts = t.Split(':');
							if (parts.Length == 2)
							{
								m_connectionActive.EntryIP = parts[0];
								m_connectionActive.Port = Convert.ToInt32(parts[1]);
							}
						}
					}

					// Detect UDP connection (OpenVPN <2.4)
					if (message.IndexOfInv("UDPv4 link remote: [AF_INET]") != -1)
					{
						string t = message;
						t = t.Replace("UDPv4 link remote:", "").Trim();
						t = t.Replace("[AF_INET]", "").Trim();
						string[] parts = t.Split(':');
						if (parts.Length == 2)
						{
							m_connectionActive.EntryIP = parts[0];
							m_connectionActive.Port = Convert.ToInt32(parts[1]);
						}
					}

					// TOCLEAN_OPENVPNMANAGEMENT
					/*
					if (messageLower.RegExMatchOne("^management: tcp socket listening on \\[af_inet6?\\]([0-9a-f\\.\\:]+?)$") != "")
					{
						ConnectManagementSocket();
					}
					*/

					if (message.IndexOfInv("Initialization Sequence Completed") != -1)
					{
						ConnectedStep();
					}

                    if (message.IndexOfInv("Client connected from [AF_INET]127.0.0.1") != -1)
					{
					}

					// Windows
					if (Platform.Instance.IsUnixSystem() == false)
					{
						// Note: Windows allow [] chars in interface name, but OpenVPN use ] to close the name and don't escape it, so "\\sopened" it's required for lazy regex.

						// TAP-WIN32 - OpenVPN 2.5
						{
							Match match = Regex.Match(message, "TAP-WIN32 device \\[(.*?)\\] opened");
							if (match.Success)
								CheckTunNetworkInterface(match.Groups[1].Value); // Note: Name, not ID.
						}

						// Wintap - OpenVPN 2.5
						{
							Match match = Regex.Match(message, "Wintun device \\[(.*?)\\] opened");
							if(match.Success)
								CheckTunNetworkInterface(match.Groups[1].Value); // Note: Name, not ID.
						}

						// Compatibility, can be probably removed. Ininfluent if rule above already match.
						{
							List<string> matchInterface = message.RegExMatchSingle("TAP-.*\\\\(.+?).tap");
							if (matchInterface != null)
								CheckTunNetworkInterface(matchInterface[0]);
						}

						// Compatibility, can be probably removed. Ininfluent if rule above already match.
						{
							List<string> matchName = message.RegExMatchSingle("TAP-.*\\sdevice\\s\\[(.*?)\\]\\sopened");
							if (matchName != null)
								CheckTunNetworkInterface(matchName[0]);
						}
					}

					// Unix
					if (Platform.Instance.IsUnixSystem())
					{
						Match match = Regex.Match(message, "TUN/TAP device (.*?) opened");
						if (match.Success)
							CheckTunNetworkInterface(match.Groups[1].Value);
					}

					// OSX
					{
						Match match = Regex.Match(message, "Opened utun device (.*?)$");
						if (match.Success)
							CheckTunNetworkInterface(match.Groups[1].Value);
					}

					if (Platform.Instance.IsWindowsSystem())
					{
						// Workaround (2018/01/28) for this bug: https://airvpn.org/topic/25139-why-exists-pull-filter-ignore-dhcp-option-dns6/
						if (message.IndexOfInv("metric 0") != -1) // To catch only one, the main
						{
							IpAddress ipv6rangeIp = new IpAddress(message.RegExMatchOne("add_route_ipv6\\((.*?)\\s"));
							if (ipv6rangeIp.Valid)
							{
								string routeIPv6 = SystemShell.Shell2(Platform.Instance.LocateExecutable("route.exe"), "-6", "PRINT");								
								string iFace = routeIPv6.CleanSpace().ToLowerInv().RegExMatchOne("(\\d+?)\\s\\d+\\s" + ipv6rangeIp.ToCIDR() + "\\son-link");
								if (iFace != "")
								{
									Engine.Instance.Logs.LogVerbose("Detected an OpenVPN bug (On-Link route on VPN range), autofix.");
									Engine.Instance.Elevated.DoCommandSync("windows-workaround-25139", "cidr", ipv6rangeIp.ToCIDR(), "iface", iFace);									
								}
							}
						}
					}

					// Push directives management
					if (m_connectionActive != null)
					{
						bool pushLog = false;
						string pushDirectivesLine = message.RegExMatchOne("PUSH: Received control message: '(.*?)'");
						if (pushDirectivesLine != "")
						{
							pushLog = true;
							List<string> pushDirectives = pushDirectivesLine.StringToList(",");
							foreach (string directive in pushDirectives)
							{
								if (directive == "PUSH_REPLY")
									continue;
								m_connectionActive.PendingPushDetected.Add(directive);
							}

						}

						string pushDirectivesFiltered = message.RegExMatchOne("Pushed option removed by filter: '(.*?)'");
						if (pushDirectivesFiltered != "")
						{
							pushLog = true;
							m_connectionActive.PendingPushDetected.Remove(pushDirectivesFiltered);
						}

						// OpenVPN2 log "PUSH: Received control message:" and after a series of "Pushed option removed by filter:".
						// So, if the series of "Pushed option removed by filter:" ends, accept the push list as definitive.
						if ((pushLog == false) && (m_connectionActive.PendingPushDetected.Count != 0))
						{
							foreach (string directive in m_connectionActive.PendingPushDetected)
								m_connectionActive.OpenVpnProfileWithPush.AppendDirectives(directive, "Push");
							m_connectionActive.PendingPushDetected.Clear();
						}
					}

                    // End

                    if (log)
						Engine.Logs.Log(logType, source + " > " + message);

					Platform.Instance.OnDaemonOutput(source, message);
				}
				else if (source == "SSH")
				{
					bool log = true;

					// Windows PuTTY
					if (message.IndexOfInv("enter \"y\" to update PuTTY's cache and continue connecting") != -1)
						m_processProxy.StandardInput.WriteLine("y");

					// Linux/MacOS SSH - Not sure if used really
					if (message.IndexOfInv("If you trust this host, enter \"y\" to add the key to") != -1)
						m_processProxy.StandardInput.WriteLine("y");

					if (message == "Access granted") // PLink Windows
					{
						StartOpenVpnProcess();
					}

					if (message.StartsWithInv("Authenticated to")) // SSH Linux
					{
						StartOpenVpnProcess();
					}

					if (log)
						Engine.Logs.Log(LogType.Verbose, source + " > " + message);
				}
				else if (source == "SSL")
				{
					bool log = true;

					if (message.IndexOfInv("Configuration successful") != -1)
					{
						StartOpenVpnProcess();
					}

					if (log)
						Engine.Logs.Log(LogType.Verbose, source + " > " + message);
				}
				/* TOCLEAN
				else if (source == "Management")
				{

					ProcessOutputManagement(source, message);
				}
				*/
			}
			catch (Exception ex)
			{
				Engine.Logs.Log(LogType.Warning, ex);

				SetReset("ERROR");
			}
		}

		// TOCLEAN_OPENVPNMANAGEMENT
		/*
		public void ConnectManagementSocket()
		{
            if(Engine.Instance.GetUseOpenVpnManagement() == false)
                return;

			if (m_openVpnManagementSocket == null)
			{
				Engine.Logs.Log(LogType.Verbose, LanguageManager.GetText("ConnectionStartManagement"));

				m_openVpnManagementSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				m_openVpnManagementSocket.Connect("127.0.0.1", Engine.Storage.GetInt("openvpn.management_port"));
				m_openVpnManagementSocket.SendTimeout = 5000;
				m_openVpnManagementSocket.ReceiveTimeout = 5000;

				SendManagementCommand(m_connectionActive.ManagementPassword);
			}
		}
		*/

		public void CheckTunNetworkInterface(string id)
		{
			if (m_connectionActive.Interface != null)
				return; // Already detected

			// Search NetworkInterface
			

			NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

			// Search by ID
			foreach (NetworkInterface adapter in interfaces)
			{
				if (adapter.Id == id)
					m_connectionActive.Interface = adapter;
			}

			// Search by Name
			if (m_connectionActive.Interface == null)
			{				
				foreach (NetworkInterface adapter in interfaces)
				{
					if (adapter.Name == id)
						m_connectionActive.Interface = adapter;
				}
			}

			if (m_connectionActive.Interface == null)
				throw new Exception("Unexpected: Network interface unknown");

			m_interfaceScope = new InterfaceScope(m_connectionActive.Interface.Id);

			Json jInfo = Engine.Instance.FindNetworkInterfaceInfo(m_connectionActive.Interface.Id);

			if ((m_connectionActive.TunnelIPv4) && (jInfo != null) && (jInfo.HasKey("support_ipv4")) && (Conversions.ToBool(jInfo["support_ipv4"].Value) == false))
			{
				Engine.Instance.Logs.LogWarning(LanguageManager.GetText("IPv4NotSupportedByNetworkAdapter"));
				if ( (Engine.Instance.Storage.GetBool("network.ipv4.autoswitch")) && (Engine.Instance.Storage.Get("network.ipv4.mode") != "block") )
				{
					Engine.Instance.Logs.LogWarning(LanguageManager.GetText("IPv4NotSupportedByNetworkAdapterAutoSwitch"));
					Engine.Instance.Storage.Set("network.ipv4.mode", "block");
				}
			}
			if ((m_connectionActive.TunnelIPv6) && (jInfo != null) && (jInfo.HasKey("support_ipv6")) && (Conversions.ToBool(jInfo["support_ipv6"].Value) == false))
			{
				Engine.Instance.Logs.LogWarning(LanguageManager.GetText("IPv6NotSupportedByNetworkAdapter"));
				if ((Engine.Instance.Storage.GetBool("network.ipv6.autoswitch")) && (Engine.Instance.Storage.Get("network.ipv6.mode") != "block"))
				{
					Engine.Instance.Logs.LogWarning(LanguageManager.GetText("IPv6NotSupportedByNetworkAdapterAutoSwitch"));
					Engine.Instance.Storage.Set("network.ipv6.mode", "block");
				}
			}
		}

		public void ConnectedStep()
		{
			Platform.Instance.OnInterfaceDo(m_connectionActive.Interface.Id);

			IpAddresses dns = new IpAddresses(Engine.Instance.Storage.Get("dns.servers"));
			if (dns.Count == 0)
				dns = Engine.ConnectionActive.OpenVpnProfileWithPush.ExtractDns();

			if (dns.Count != 0)
				Platform.Instance.OnDnsSwitchDo(m_connectionActive, dns);

			// 2.14.0
			foreach (ConnectionActiveRoute route in m_connectionActive.Routes)
			{
				if ((route.Address.IsV6) || (Constants.FeatureAlwaysBypassOpenvpnRoute))
				{
					if (route.Gateway == "vpn_gateway")
					{
						if (route.Add(m_connectionActive))
							m_routes.Add(route);
					}
				}
			}

			Engine.WaitMessageSet(LanguageManager.GetText("ConnectionFlushDNS"), true);

			Platform.Instance.FlushDNS();

			// 2.4: Sometime (only under Windows) Interface is not really ready...
			if (Platform.Instance.WaitTunReady(m_connectionActive) == false)
				SetReset("ERROR");

			if (m_connectionActive.ExitIPs.Count == 0)
			{
				Engine.WaitMessageSet(LanguageManager.GetText("ConnectionDetectExit"), true);
				Engine.Logs.Log(LogType.Verbose, LanguageManager.GetText("ConnectionDetectExit"));
				m_connectionActive.ExitIPs.Add(Engine.Instance.DiscoverExit());
			}

			Engine.Instance.NetworkLockManager.OnVpnEstablished();

			if (Engine.CurrentServer.Provider is Providers.Service)
			{
				Providers.Service service = Engine.CurrentServer.Provider as Providers.Service;

				if (Engine.CurrentServer.SupportCheck == false)
				{
					Engine.Logs.Log(LogType.Warning, LanguageManager.GetText("ConnectionCheckingRouteNotAvailable"));
				}
				else
				{
					if ((m_reset == "") && (service.CheckTunnel))
					{
						int nTry = 3;
						if (Engine.Instance.Storage.GetBool("windows.workarounds"))
							nTry = 10;

						if ((m_connectionActive.TunnelIPv4) && (Engine.CurrentServer.SupportIPv4))
						{
							bool ok = false;
							Engine.WaitMessageSet(LanguageManager.GetText("ConnectionCheckingRouteIPv4"), true);
							for (int t = 0; t < nTry; t++)
							{
								if (m_reset != "")
									break;

								if (t == 0)
									Engine.Logs.Log(LogType.Info, LanguageManager.GetText("ConnectionCheckingRouteIPv4"));
								else
								{
									Engine.Logs.Log(LogType.Verbose, LanguageManager.GetText("ConnectionCheckingTryRoute", (t + 1).ToString()));
									System.Threading.Thread.Sleep(t * 1000);
								}

								try
								{
									string checkDomain = Engine.CurrentServer.ProviderName.ToLowerInvariant() + "_exit." + service.GetKeyValue("check_domain", "");
									string checkUrl = "https://" + checkDomain + "/check_tun/";
									
									HttpRequest httpRequest = new HttpRequest();
									httpRequest.Url = checkUrl;
									httpRequest.BypassProxy = true;
									httpRequest.IpLayer = "4";
									httpRequest.ForceResolve = checkDomain + ":" + Engine.CurrentServer.IpsExit.OnlyIPv4.First.Address;
									XmlDocument xmlDoc = Engine.FetchUrlXml(httpRequest);

									string answer = xmlDoc.DocumentElement.Attributes["ip"].Value;

									if (m_connectionActive.OpenVpnProfileWithPush.ExtractVpnIPs().OnlyIPv4.ContainsAddress(answer) == false)
										throw new Exception(LanguageManager.GetText("ConnectionCheckingTryRouteFail", answer));

									m_connectionActive.TimeServer = Conversions.ToInt64(xmlDoc.DocumentElement.Attributes["time"].Value);
									m_connectionActive.TimeClient = Utils.UnixTimeStamp();

									ok = true;
									break;
								}
								catch (Exception e)
								{
									Engine.Logs.Log(LogType.Verbose, e);
								}
							}

							if ((m_reset == "") && (ok == false))
							{
								Engine.Logs.Log(LogType.Error, LanguageManager.GetText("ConnectionCheckingRouteIPv4Failed"));
								SetReset("ERROR");
							}
						}

						if ((m_connectionActive.TunnelIPv6) && (Engine.CurrentServer.SupportIPv6))
						{
							bool ok = false;
							Engine.WaitMessageSet(LanguageManager.GetText("ConnectionCheckingRouteIPv6"), true);
							for (int t = 0; t < nTry; t++)
							{
								if (m_reset != "")
									break;

								if (t == 0)
									Engine.Logs.Log(LogType.Info, LanguageManager.GetText("ConnectionCheckingRouteIPv6"));
								else
								{
									Engine.Logs.Log(LogType.Verbose, LanguageManager.GetText("ConnectionCheckingTryRoute", (t + 1).ToString()));
									System.Threading.Thread.Sleep(t * 1000);
								}

								try
								{
									string checkDomain = Engine.CurrentServer.ProviderName.ToLowerInvariant() + "_exit." + service.GetKeyValue("check_domain", "");
									string checkUrl = "https://" + checkDomain + "/check_tun/";
									
									HttpRequest httpRequest = new HttpRequest();
									httpRequest.Url = checkUrl;
									httpRequest.BypassProxy = true;
									httpRequest.IpLayer = "6";
									httpRequest.ForceResolve = checkDomain + ":" + Engine.CurrentServer.IpsExit.OnlyIPv6.First.Address;
									XmlDocument xmlDoc = Engine.FetchUrlXml(httpRequest);

									string answer = xmlDoc.DocumentElement.Attributes["ip"].Value;

									if (m_connectionActive.OpenVpnProfileWithPush.ExtractVpnIPs().OnlyIPv6.ContainsAddress(answer) == false)
										throw new Exception(LanguageManager.GetText("ConnectionCheckingTryRouteFail", answer));

									m_connectionActive.TimeServer = Conversions.ToInt64(xmlDoc.DocumentElement.Attributes["time"].Value);
									m_connectionActive.TimeClient = Utils.UnixTimeStamp();

									ok = true;
									break;
								}
								catch (Exception e)
								{
									Engine.Logs.Log(LogType.Verbose, e);
								}
							}

							if ((m_reset == "") && (ok == false))
							{
								Engine.Logs.Log(LogType.Error, LanguageManager.GetText("ConnectionCheckingRouteIPv6Failed"));
								SetReset("ERROR");
							}
						}

						if (m_reset == "")
						{
							// Real IP are detected with a request over the server entry IP.
							// Normally this is routed by openvpn outside the tunnel.
							// But if a proxy is active, don't work.
							if ((Engine.Instance.ConnectionActive.OpenVpnProfileWithPush.ExistsDirective("socks-proxy")) ||
								(Engine.Instance.ConnectionActive.OpenVpnProfileWithPush.ExistsDirective("http-proxy"))
							  )
							{
								m_connectionActive.RealIp = LanguageManager.GetText("NotAvailable");
								m_connectionActive.TimeServer = 0;
							}
							else
							{
								try
								{
									string checkDomain = Engine.CurrentServer.ProviderName.ToLowerInvariant() + "." + service.GetKeyValue("check_domain", "");
									string checkUrl = "https://" + checkDomain + "/check_tun/";
									HttpRequest httpRequest = new HttpRequest();
									httpRequest.Url = checkUrl;
									httpRequest.BypassProxy = true;
									if (m_connectionActive.EntryIP.IsV4)
										httpRequest.IpLayer = "4";
									else
										httpRequest.IpLayer = "6";
									httpRequest.ForceResolve = checkDomain + ":" + m_connectionActive.EntryIP;
									XmlDocument xmlDoc = Engine.FetchUrlXml(httpRequest);

									m_connectionActive.TimeServer = Conversions.ToInt64(xmlDoc.DocumentElement.Attributes["time"].Value);
									m_connectionActive.TimeClient = Utils.UnixTimeStamp();

									m_connectionActive.RealIp = xmlDoc.DocumentElement.Attributes["ip"].Value;
								}
								catch (Exception ex)
								{
									Engine.Logs.Log(ex);

									m_connectionActive.RealIp = LanguageManager.GetText("NotAvailable");
									m_connectionActive.TimeServer = 0;
								}
							}
						}
					}
					else
					{
						m_connectionActive.RealIp = "";
						m_connectionActive.TimeServer = 0;
					}

					// DNS test
					if ((m_reset == "") && (service.CheckDns) && (Engine.Storage.Get("dns.servers") == ""))
					{
						Engine.WaitMessageSet(LanguageManager.GetText("ConnectionCheckingDNS"), true);

						bool ok = false;
						int nTry = 3;
						if (Engine.Instance.Storage.GetBool("windows.workarounds"))
							nTry = 10;

						for (int t = 0; t < nTry; t++)
						{
							if (m_reset != "")
								break;

							if (t == 0)
								Engine.Logs.Log(LogType.Info, LanguageManager.GetText("ConnectionCheckingDNS"));
							else
							{
								Engine.Logs.Log(LogType.Verbose, LanguageManager.GetText("ConnectionCheckingTryDNS", (t + 1).ToString()));
								System.Threading.Thread.Sleep(t * 1000);
							}

							try
							{
								// Don't use a real hash, it's too long.
								string hash = RandomGenerator.GetRandomToken();

								// Query a inexistent domain with the hash
								string dnsQuery = service.GetKeyValue("check_dns_query", "");
								string dnsHost = dnsQuery.Replace("{hash}", hash);
								IpAddresses result = DnsManager.ResolveDNS(dnsHost, true);

								// Note1: AirVPN generation 1 servers don't provide any answer. 
								// So we check SupportIPv6 for generation 2.
								// When all AirVPN servers become gen2, assert(result.Count != 0)
								// Note2: Sometime (with pull-filter DNS6 for example) the server receive the request 
								//        but the DNS fail with WSANO_RECOVERY under Windows.
								//        So result is empty, but CheckDNS below works (parallel DNS execution?)
								//if(result.Count == 0)
								//if ((Engine.CurrentServer.SupportIPv6) && (result.Count == 0))
								//	throw new Exception(LanguageManager.GetText("ConnectionCheckingTryDNSFail, "No DNS answer"));								

								// Check if the server has received the above DNS query
								string checkDomain = Engine.CurrentServer.ProviderName.ToLowerInvariant() + "_exit." + service.GetKeyValue("check_domain", "");
								string checkUrl = "https://" + checkDomain + "/check_dns/";
								HttpRequest httpRequest = new HttpRequest();
								httpRequest.Url = checkUrl;
								httpRequest.BypassProxy = true;
								if (result.CountIPv6 != 0) // Note: Use the same IP layer of the dns-result
								{
									httpRequest.IpLayer = "6";
									httpRequest.ForceResolve = checkDomain + ":" + Engine.CurrentServer.IpsExit.OnlyIPv6.First;
								}
								else
								{
									httpRequest.IpLayer = "4";
									httpRequest.ForceResolve = checkDomain + ":" + Engine.CurrentServer.IpsExit.OnlyIPv4.First;
								}
								XmlDocument xmlDoc = Engine.FetchUrlXml(httpRequest);								

								string answer = xmlDoc.DocumentElement.Attributes["hash"].Value;

								if (hash != answer)
									throw new Exception(LanguageManager.GetText("ConnectionCheckingTryDNSFail", answer));

								ok = true;
								break;
							}
							catch (Exception e)
							{
								Engine.Logs.Log(LogType.Verbose, e);
							}
						}

						if ((m_reset == "") && (ok == false))
						{
							Engine.Logs.Log(LogType.Error, LanguageManager.GetText("ConnectionCheckingDNSFailed"));
							SetReset("ERROR");
						}
					}
				}
			}

			if (m_reset == "")
			{
				Engine.RunEventCommand("vpn.up");

				Engine.Logs.Log(LogType.InfoImportant, LanguageManager.GetText("ConnectionConnected"));
				Engine.SetConnected(true);
				m_connectionActive.TimeStart = DateTime.UtcNow;

				if (Engine.Instance.Storage.GetBool("advanced.testonly"))
					Engine.RequestStop();
			}
		}

		/* TOCLEAN
		public void ProcessOutputManagement(string source, string message)
		{
			// Ignore, useless			
			if (message.ContainsInv("OpenVPN Management Interface Version 1 -- type 'help' for more info"))
				return;
			if (message.StartsWithInv("ENTER PASSWORD:"))
				return;
			if (message.StartsWithInv("SUCCESS: password is correct"))
				return;

			string[] lines = message.Split('\n');
			for (int i = 0; i < lines.Length; i++)
			{
				lines[i] = lines[i].Trim();

				string line = lines[i];

				if (line == "")
				{
				}
				else if (line == "OpenVPN STATISTICS")
				{
					m_openVpnManagementStatisticsLines.Add(line);
				}
				else if (line == "END")
				{
					if (m_openVpnManagementStatisticsLines.Count != 0) // If 0, 'END' refer to another command.
					{
						// Process statistics
						Int64 read = 0;
						Int64 write = 0;
						String[] readArray = m_openVpnManagementStatisticsLines[4].Split(',');
						String[] writeArray = m_openVpnManagementStatisticsLines[5].Split(',');
						if (readArray.Length == 2)
							read = Conversions.ToInt64(readArray[1]);
						if (writeArray.Length == 2)
							write = Conversions.ToInt64(writeArray[1]);

						UpdateBytesStats(read, write);

						m_openVpnManagementStatisticsLines.Clear();
					}
				}
				else if (m_openVpnManagementStatisticsLines.Count != 0)
				{
					m_openVpnManagementStatisticsLines.Add(lines[i]);
				}
				else
				{
					Engine.Logs.Log(LogType.Verbose, "OpenVpn Management > " + line);
				}
			}
		}
		*/
	}
}