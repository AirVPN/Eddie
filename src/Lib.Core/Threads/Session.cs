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
using Eddie.Common;
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
		private Process m_processOpenVpn;
		private Process m_processProxy;

		private Socket m_openVpnManagementSocket;
		private List<string> m_openVpnManagementCommands = new List<string>();
		private List<string> m_openVpnManagementStatisticsLines = new List<string>();

		private string m_reset = "";
		private List<SessionLogEvent> m_logEvents = new List<SessionLogEvent>();
		List<ConnectionActiveRoute> m_routes = new List<ConnectionActiveRoute>();
		private NetworkInterface m_interfaceTun;
		private int m_timeLastStatus = 0;
		private TemporaryFile m_fileSshKey;
		private TemporaryFile m_fileSslCrt;
		private TemporaryFile m_fileSslConfig;
		//private TemporaryFile m_fileOvpn;
		private ProgramScope m_programScope = null;
		private InterfaceScope m_interfaceScope = null;

		private Int64 m_interfaceTunBytesReadInitial = -1;
		private Int64 m_interfaceTunBytesWriteInitial = -1;
		private Int64 m_interfaceTunBytesLastRead = -1;
		private Int64 m_interfaceTunBytesLastWrite = -1;
		private TimeDelta m_interfaceTunBytesLastTick = new TimeDelta();

		private ConnectionActive m_connectionActive = null;

		public override void OnRun()
		{
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

				m_routes.Clear();

				try
				{
					// -----------------------------------
					// Phase 1: Initialization and start
					// -----------------------------------


					if ((Engine.NextServer == null) && (Pinger.Instance.GetEnabled()) && (Engine.PingerInvalid() != 0))
					{
						string lastWaitingMessage = "";
						for (; ; )
						{
							if (CancelRequested)
								break;

							int i = Engine.PingerInvalid();
							if (i == 0)
								break;

							string nextWaitingMessage = Messages.WaitingLatencyTestsTitle + " " + MessagesFormatter.Format(Messages.WaitingLatencyTestsStep, i.ToString());
							if (lastWaitingMessage != nextWaitingMessage)
							{
								lastWaitingMessage = nextWaitingMessage;
								Engine.WaitMessageSet(nextWaitingMessage, true);
							}

							Sleep(100);
						}
					}

					if (CancelRequested)
						continue;

					m_openVpnManagementCommands.Clear();

					Engine.CurrentServer = Engine.NextServer;
					Engine.NextServer = null;

					string forceServer = Engine.Storage.Get("server");
					if (forceServer != "")
					{
						Engine.CurrentServer = Engine.PickConnectionByName(forceServer);
					}
					else
					{
						if (Engine.CurrentServer == null)
							if (Engine.Storage.GetBool("servers.locklast"))
								Engine.CurrentServer = Engine.PickConnection(sessionLastServer);

						if (Engine.CurrentServer == null)
							Engine.CurrentServer = Engine.PickConnection();
					}

					if (Engine.CurrentServer == null)
					{
						allowed = false;
						Engine.Logs.Log(LogType.Fatal, "No server available.");
						RequestStop();
					}

					// Checking auth user status.
					// Only to avoid a generic AUTH_FAILED. For that we don't report here for ex. the sshtunnel keys.
					if (allowed)
					{
						if (Engine.CurrentServer.Provider is Providers.Service)
						{
							Providers.Service service = Engine.CurrentServer.Provider as Providers.Service;
							if (service.SupportConnect)
							{
								Engine.WaitMessageSet(Messages.AuthorizeConnect, true);
								Engine.Logs.Log(LogType.Info, Messages.AuthorizeConnect);

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
									xmlDoc = service.Fetch(Messages.AuthorizeConnect, parameters);
								}
								catch (Exception e)
								{
									// Note: If failed, continue anyway.
									Engine.Logs.Log(LogType.Warning, MessagesFormatter.Format(Messages.AuthorizeConnectFailed, e.Message));
								}

								if (xmlDoc != null)
								{
									string userMessage = UtilsXml.XmlGetAttributeString(xmlDoc.DocumentElement, "message", "");
									if (userMessage != "")
									{
										allowed = false;
										string userMessageAction = UtilsXml.XmlGetAttributeString(xmlDoc.DocumentElement, "message_action", "");
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
							Engine.Instance.Logs.LogWarning(Messages.IPv6NotSupportedByOS);
							if ((Engine.Instance.Storage.GetBool("network.ipv6.autoswitch")) && (Engine.Instance.Storage.Get("network.ipv6.mode") != "block"))
							{
								Engine.Instance.Logs.LogWarning(Messages.IPv6NotSupportedByNetworkAdapterAutoSwitch);
								Engine.Instance.Storage.Set("network.ipv6.mode", "block");
							}
						}	
					}

					try
					{
						m_connectionActive = Engine.CurrentServer.BuildConnectionActive(false);
						Engine.WaitMessageSet(Messages.ConnectionCredentials, true);
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
							if (Common.Constants.FeatureIPv6ControlOptions)
							{
								if (Engine.Instance.GetNetworkIPv6Mode() == "block")
									m_connectionActive.BlockedIPv6 = true;
							}
							else
							{
								if (Engine.Instance.Storage.GetLower("ipv6.mode") == "disable")
									m_connectionActive.BlockedIPv6 = true;
							}

							if ((Engine.CurrentServer.SupportIPv6 == false) && (Engine.Instance.GetNetworkIPv6Mode() == "in-block"))
								m_connectionActive.BlockedIPv6 = true;

							if (m_connectionActive.BlockedIPv6)
								Platform.Instance.OnIPv6Block();
						}

						m_connectionActive.ExitIPs.Add(Engine.CurrentServer.IpsExit.Clone());

						sessionLastServer = Engine.CurrentServer.Code;
						Engine.Storage.Set("servers.last", Engine.CurrentServer.Code);

						routeScope = new RouteScope(m_connectionActive.EntryIP); // Clodo: Urgent, may not work under some OS with NetLock active. Try to add the RouteScope when detecting protocol from OpenVPN logs.

						Engine.RunEventCommand("vpn.pre");

						string connectingMessage = MessagesFormatter.Format(Messages.ConnectionConnecting, Engine.CurrentServer.GetNameWithLocation());
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
						//else if ( (Engine.ConnectedProtocol == "UDP") || (Engine.ConnectedProtocol == "TCP") )
						else // 2.11.4
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

								int timeNow = UtilsCore.UnixTimeStamp();

								if (Engine.IsConnected() == false)
									throw new Exception("Unexpected.");

								ProcessOpenVpnManagement();

								if (timeNow - m_timeLastStatus >= 1)
								{
									m_timeLastStatus = timeNow;

									// Update traffic stats
									if (Storage.Simulate)
									{
										Engine.Instance.Stats.Charts.Hit(15354, 2525);

										Engine.OnRefreshUi(Core.Engine.RefreshUiMode.Stats);
									}
									else if (Platform.Instance.GetTunStatsMode() == "NetworkInterface")
									{
										if (m_interfaceTun != null)
										{
											Int64 tunRead = 0;
											Int64 tunWrite = 0;

											tunRead += m_interfaceTun.GetIPv4Statistics().BytesReceived;
											tunWrite += m_interfaceTun.GetIPv4Statistics().BytesSent;

											if (m_interfaceTunBytesReadInitial == -1)
											{
												m_interfaceTunBytesReadInitial = tunRead;
												m_interfaceTunBytesWriteInitial = tunWrite;
											}
											tunRead -= m_interfaceTunBytesReadInitial;
											tunWrite -= m_interfaceTunBytesWriteInitial;

											UpdateBytesStats(tunRead, tunWrite);
										}
									}
									else if (Platform.Instance.GetTunStatsMode() == "OpenVpnManagement")
									{
										SendManagementCommand("status");
									}
								}

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

						Engine.WaitMessageSet(Messages.ConnectionDisconnecting, false);
						Engine.Logs.Log(LogType.InfoImportant, Messages.ConnectionDisconnecting);

						if (Storage.Simulate)
						{
							if(m_processOpenVpn != null)
								if (m_processOpenVpn.ReallyExited == false)
									m_processOpenVpn.Kill();
						}

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

								int now = UtilsCore.UnixTimeStamp();

								// As explained here: http://stanislavs.org/stopping-command-line-applications-programatically-with-ctrl-c-events-from-net/
								// there isn't any .Net/Mono clean method to send a signal term to a Windows console-only application. So a brutal Kill is performed when there isn't any alternative.
								// TODO: Maybe optimized under Linux.

								// Simulation process
								if ((Storage.Simulate) && (m_processOpenVpn != null) && (m_processOpenVpn.ReallyExited == false))
									m_processOpenVpn.Kill();

								// OpenVPN process completed, but management socket still opened. Strange, but happen. Closing socket.
								if ((m_processOpenVpn != null) && (m_openVpnManagementSocket != null) && (m_processOpenVpn.ReallyExited == true) && (m_openVpnManagementSocket.Connected))
									m_openVpnManagementSocket.Close();

								// OpenVPN process still exists, but management socket is not connected. We can't tell to OpenVPN to do a plain disconnection, force killing.
								if ((m_processOpenVpn != null) && (m_processOpenVpn.ReallyExited == false))
								{
									if ((m_openVpnManagementSocket == null) || (m_openVpnManagementSocket.Connected == false))
									{
										if (now - lastSignalTime >= 10)
										{
											lastSignalTime = now;

											if ((lastSignalType == "none") || (lastSignalType == "management"))
											{
												lastSignalType = "soft";
												Engine.Instance.Logs.Log(LogType.Verbose, Messages.KillWithSoft);
												Platform.Instance.ProcessKillSoft(m_processOpenVpn);
											}
											else if ((lastSignalType == "soft") || (lastSignalType == "hard"))
											{
												lastSignalType = "hard";
												Engine.Instance.Logs.Log(LogType.Verbose, Messages.KillWithHard);
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
											Engine.Instance.Logs.Log(LogType.Verbose, Messages.KillWithSoft);
											Platform.Instance.ProcessKillSoft(m_processProxy);
										}
										else if ((lastSignalType == "soft") || (lastSignalType == "hard"))
										{
											lastSignalType = "hard";
											Engine.Instance.Logs.Log(LogType.Verbose, Messages.KillWithHard);
											m_processProxy.Kill();
										}
									}
								}

								// Start a clean disconnection
								if ((m_processOpenVpn != null) && (m_openVpnManagementSocket != null) && (m_processOpenVpn.ReallyExited == false) && (m_openVpnManagementSocket.Connected))
								{
									if (now - lastSignalTime >= 10)
									{
										lastSignalTime = now;
										lastSignalType = "management";
										Engine.Instance.Logs.Log(LogType.Verbose, Messages.KillWithManagement);
										SendManagementCommand("signal SIGTERM");
										ProcessOpenVpnManagement();
									}
								}
							}
							catch (Exception e)
							{
								Engine.Logs.Log(LogType.Warning, e);
							}

							bool exit = true;

							if ((m_openVpnManagementSocket != null) && (m_openVpnManagementSocket.Connected))
								exit = false;

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

						Engine.Logs.Log(LogType.Verbose, Messages.ConnectionStop);

						Engine.RunEventCommand("vpn.down");

						if (m_connectionActive.BlockedIPv6)
						{
							Platform.Instance.OnIPv6Restore();
							m_connectionActive.BlockedIPv6 = false;
						}

						Platform.Instance.OnRouteDefaultRemoveRestore();

						Platform.Instance.OnDnsSwitchRestore();

						Platform.Instance.OnInterfaceRestore();

						if (m_processOpenVpn != null)
						{
							m_processOpenVpn.Dispose();
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

						

						if (m_openVpnManagementSocket != null)
						{
							(m_openVpnManagementSocket as IDisposable).Dispose();
							m_openVpnManagementSocket = null;
						}

						ProcessLogsEvents();
					}

				}
				catch (Exception e)
				{
					// Warning: Avoid to reach this catch: unpredicable status of running processes.
					Engine.SetConnected(false);

					Engine.Logs.Log(LogType.Error, MessagesFormatter.Format(Messages.FatalUnexpected, e.Message + " - " + e.StackTrace));
				}

				if (routeScope != null)
					routeScope.End();

				if (m_programScope != null)
					m_programScope.End();

				if (m_interfaceScope != null)
					m_interfaceScope.End();

				if (Engine.Instance.Storage.GetBool("batch"))
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
						Engine.WaitMessageSet(MessagesFormatter.Format(waitingMessage, (waitingSecs - i).ToString()), true);
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
					Engine.Logs.Log(LogType.Info, Messages.SessionCancel);
				}
				else
				{
					Engine.Logs.Log(LogType.Error, Messages.SessionFailed);
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
			bool isPlink = (sshToolPath.ToLowerInvariant().EndsWith("plink.exe"));

			Providers.Service service = Engine.CurrentServer.Provider as Providers.Service;
			if (service == null) // Unexpected
				return;

			if (m_processProxy != null) // Unexpected
				return;

			string fileKeyExtension = "key";			
			if(isPlink)
				fileKeyExtension = "ppk";

			m_fileSshKey = new TemporaryFile(fileKeyExtension);
			Platform.Instance.FileContentsWriteText(m_fileSshKey.Path, UtilsXml.XmlGetAttributeString(service.User, "ssh_" + fileKeyExtension, ""), Encoding.ASCII);

			if (Platform.Instance.IsWindowsSystem())
				Platform.Instance.FileEnsureRootOnly(m_fileSshKey.Path);

			if (Platform.Instance.IsUnixSystem())
			{
				// under macOS, SSH change it's UID to normal user.
				if (Platform.Instance.GetCode() != "MacOS")
				{
					Platform.Instance.FileEnsurePermission(m_fileSshKey.Path, "600");
				}
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

			if (Platform.Instance.IsUnixSystem())
				arguments += " -o UserKnownHostsFile=/dev/null -o StrictHostKeyChecking=no"; // TOOPTIMIZE: To bypass key confirmation. Not the best approach.
			arguments += " -N -T -v";

			m_programScope = new ProgramScope(Software.GetTool("ssh").Path, "SSH Tunnel");

			m_processProxy = new Process();
			m_processProxy.StartInfo.FileName = sshToolPath;
			m_processProxy.StartInfo.Arguments = arguments;
			m_processProxy.StartInfo.WorkingDirectory = UtilsCore.GetTempPath();

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
			Platform.Instance.FileContentsWriteText(m_fileSslCrt.Path, UtilsXml.XmlGetAttributeString(service.User, "ssl_crt", ""), Encoding.ASCII);

			m_fileSslConfig = new TemporaryFile("ssl");

			string sslConfig = "";

			if (Platform.Instance.IsUnixSystem())
			{
				//sslConfig += "output = /dev/stdout\n"; // With this, with new stunnel 5.01, we have duplicated output dump.
				sslConfig += "foreground = yes\n";	// Without this, the process fork and it's exit can't be detected.
				//sslConfig += "pid = /tmp/" + RandomGenerator.GetHash() + ".pid\n"; // 2.2
			}
			if (Engine.Instance.Storage.Get("ssl.options") != "")
				sslConfig += "options = " + Engine.Instance.Storage.Get("ssl.options") + "\n";
			sslConfig += "client = yes\n";
			sslConfig += "debug = 6\n";
			sslConfig += "\n";
			sslConfig += "[openvpn]\n";
			sslConfig += "accept = 127.0.0.1:" + Conversions.ToString(m_connectionActive.SslLocalPort) + "\n";
			sslConfig += "connect = " + m_connectionActive.Address + ":" + m_connectionActive.SslRemotePort + "\n";
			sslConfig += "TIMEOUTclose = 0\n";
			if (Engine.Instance.Storage.GetInt("ssl.verify") != -1)
				sslConfig += "verify = " + Engine.Instance.Storage.GetInt("ssl.verify").ToString() + "\n";
			//sslConfig += "CAfile = \"" + m_fileSslCrt.Path + "\"\n";
			sslConfig += "CAfile = " + m_fileSslCrt.Path + "\n";
			sslConfig += "\n";

			string sslConfigPath = m_fileSslConfig.Path;
			Platform.Instance.FileContentsWriteText(sslConfigPath, sslConfig, Encoding.UTF8);

			m_programScope = new ProgramScope(Software.GetTool("ssl").Path, "SSL Tunnel");

			m_processProxy = new Process();
			m_processProxy.StartInfo.FileName = Software.GetTool("ssl").Path;			
			m_processProxy.StartInfo.Arguments = "\"" + Encoding.Default.GetString(Encoding.UTF8.GetBytes(sslConfigPath)) + "\""; // encoding workaround, stunnel expect utf8
			m_processProxy.StartInfo.WorkingDirectory = UtilsCore.GetTempPath();

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

			if (m_processProxy == null)
				m_programScope = new ProgramScope(Software.GetTool("openvpn").Path, "OpenVPN Tunnel");

			m_processOpenVpn = new Process();
			m_processOpenVpn.StartInfo.FileName = Software.GetTool("openvpn").Path;
			m_processOpenVpn.StartInfo.Arguments = "";
			m_processOpenVpn.StartInfo.WorkingDirectory = UtilsCore.GetTempPath();

			if (Storage.Simulate)
			{
				m_processOpenVpn.StartInfo.FileName = "Simulate.exe";
				Sleep(1000);
				Engine.SetConnected(true);
			}

			m_processOpenVpn.StartInfo.Arguments = "--config \"" + m_connectionActive.OvpnFile.Path + "\" ";

			m_processOpenVpn.StartInfo.Verb = "run";
			m_processOpenVpn.StartInfo.CreateNoWindow = true;
			m_processOpenVpn.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			m_processOpenVpn.StartInfo.UseShellExecute = false;
			m_processOpenVpn.StartInfo.RedirectStandardInput = true;
			m_processOpenVpn.StartInfo.RedirectStandardError = true;
			m_processOpenVpn.StartInfo.RedirectStandardOutput = true;
			m_processOpenVpn.StartInfo.StandardErrorEncoding = Encoding.UTF8; // 2.11.10
			m_processOpenVpn.StartInfo.StandardOutputEncoding = Encoding.UTF8; // 2.11.10

			m_processOpenVpn.OutputDataReceived += new DataReceivedEventHandler(ProcessOpenVpnOutputDataReceived);
			m_processOpenVpn.ErrorDataReceived += new DataReceivedEventHandler(ProcessOpenVpnOutputDataReceived);
			m_processOpenVpn.Exited += new EventHandler(ProcessOpenVpnExited);
			
			m_processOpenVpn.Start();

			m_processOpenVpn.BeginOutputReadLine();
			m_processOpenVpn.BeginErrorReadLine();
		}

		void ProcessOpenVpnExited(object sender, EventArgs e)
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

		void ProcessSshOutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			// TOCHECK: Must wait until a \n ?
			if (e.Data != null)
			{
				string message = e.Data.ToString();

				lock (m_logEvents)
					AddLogEvent("SSH", message);
			}
		}

		void ProcessSslOutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			// TOCHECK: Must wait until a \n ?
			if (e.Data != null)
			{
				string message = e.Data.ToString();

				// Remove STunnel timestamp
				message = System.Text.RegularExpressions.Regex.Replace(message, "^\\d{4}\\.\\d{2}\\.\\d{2}\\s+\\d{2}:\\d{2}:\\d{2}\\s+LOG\\d{1}\\[\\d{0,6}:\\d{0,60}\\]:\\s+", "");

				lock (m_logEvents)
					AddLogEvent("SSL", message);
			}
		}

		void ProcessOpenVpnOutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			// TOCHECK: Must wait until a \n ?
			if (e.Data != null)
			{
				string message = e.Data.ToString();

				// Remove OpenVPN timestamp
				message = System.Text.RegularExpressions.Regex.Replace(message, "^\\w{3}\\s+\\w{3}\\s+\\d{1,2}\\s+\\d{1,2}:\\d{1,2}:\\d{1,2}\\s+\\d{2,4}\\s+", "");

				lock (m_logEvents)
					AddLogEvent("OpenVPN", message);
			}
		}

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

		void AddLogEvent(string source, string message)
		{
			string[] lines = message.Split('\n');
			foreach (string line in lines)
			{
				if(line.Trim() != "")
					m_logEvents.Add(new SessionLogEvent(source, line.Trim()));
			}
		}

		void ProcessLogsEvents()
		{
			lock (m_logEvents)
			{
				foreach (SessionLogEvent logEvent in m_logEvents)
					ProcessLogEvent(logEvent);
				m_logEvents.Clear();
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
				if (source == "OpenVPN")
				{
					bool log = true;
					LogType logType = LogType.Verbose;

					if( (m_connectionActive != null) && (m_connectionActive.OvpnFile != null) )
					{
						// First feedback from OpenVPN process. We can remove temporary files.
						m_connectionActive.CleanAfterStart();
					}

					// Ignore
					if (message.IndexOf("MANAGEMENT: CMD 'status'") != -1)
						return;

					// Level
					if (messageLower.StartsWith("warning:"))
						logType = LogType.Warning;
					if (messageLower.StartsWith("warn:"))
						logType = LogType.Warning;
					if (messageLower.StartsWith("error:"))
						logType = LogType.Error;
					if (messageLower.StartsWith("fatal:"))
						logType = LogType.Error;
					if (messageLower.StartsWith("options error:"))
						logType = LogType.Warning;

					// Exception
					if (Platform.Instance.GetCode() != "MacOS")
					{
						// Unresolved issue, but don't want to warn users.
						// Under OpenVPN provider, file hash.tmp.ppw for auth, any permission flags cause denied or this warning. // TOFIX
						if ((messageLower.StartsWith("warning:")) && (messageLower.Contains("is group or others accessible")))
							logType = LogType.Verbose;
					}

					if (Software.GetTool("openvpn").VersionAboveOrEqual("2.4") == false)
					{
						// Don't warning users for correct behiavour (outside tunnel)
						if (
							(message.IndexOf("Options error: option 'redirect-gateway' cannot be used in this context ([PUSH-OPTIONS])") != -1) ||
							(message.IndexOf("Options error: option 'dhcp-option' cannot be used in this context ([PUSH-OPTIONS])") != -1)
							)
							log = false;
					}

					// Useless
					if (message.Contains("Use —help for more information."))
						log = false;

					// Ignore, caused by Windows method GenerateConsoleCtrlEvent to soft-kill
					if (message.Contains("win_trigger_event: WriteConsoleInput: The handle is invalid."))
						log = false;

					// Eddie delete any ovpn/crt/key/pwd files after the reading by OpenVPN.
					// If a connection drop, it's not OpenVPN that need to retry the connection, must performed by Eddie.
					// But unfortunately there isn't any method in OpenVPN to avoid the behiavour of SIGHUP (process restarting).
					// Neither the directive 'single-session'.
					// Anyway, when happen, OpenVPN can't find the .ovpn config, so effectively stop and Eddie will perform the retry.
					if ( (message.StartsWith("Options error:")) && (message.Contains("Error opening configuration file")) )
					{
						log = false;
					}

					if (message.StartsWith("Options error:"))
					{
						if (log)
						{
							List<string> matches = UtilsString.RegExMatchSingle(message, "Options error\\: Unrecognized option or missing parameter\\(s\\) in (.*?)\\:\\d+\\:(.*?)\\(.*\\)");
							if ((matches != null) && (matches.Count == 2))
							{
								string context = matches[0].Trim();
								string unrecognizedOption = matches[1].Trim();

								if (context != "[PUSH-OPTIONS]")
								{
									Engine.Logs.Log(LogType.Fatal, MessagesFormatter.Format(Messages.DirectiveError, unrecognizedOption));
									Engine.LogOpenvpnConfig();
									SetReset("FATAL");
								}
							}
						}
					}

					if (message.StartsWith("Control Channel: "))
					{
						m_connectionActive.ControlChannel = message.Substring("Control Channel: ".Length);
					}

					if (message.IndexOf("Connection reset, restarting") != -1)
					{
						SetReset("ERROR");
					}

					if (message.IndexOf("Exiting due to fatal error") != -1)
					{
						SetReset("ERROR");
					}

					if (message.IndexOf("SIGTERM[soft,ping-exit]") != -1) // 2.2
					{
						SetReset("ERROR");
					}

					if (message.IndexOf("SIGUSR1[soft,tls-error] received, process restarting") != -1)
					{
						SetReset("ERROR");
					}

					if (message.IndexOf("SIGUSR1[soft,tls-error] received, process restarting") != -1)
					{
						SetReset("ERROR");
					}

					Match matchSigReceived = Regex.Match(message, "SIG(.*?)\\[(.*?),(.*?)\\] received");
					if (matchSigReceived.Success)
					{
						SetReset("ERROR");
					}

					if (message.IndexOf("MANAGEMENT: Socket bind failed on local address") != -1)
					{
						Engine.Logs.Log(LogType.Verbose, Messages.AutoPortSwitch);

						Engine.Storage.SetInt("openvpn.management_port", Engine.Storage.GetInt("openvpn.management_port") + 1);

						SetReset("RETRY");
					}

					if (message.IndexOf("AUTH_FAILED") != -1)
					{
						Engine.Instance.CurrentServer.Provider.OnAuthFailed();

						SetReset("AUTH_FAILED");
					}

					if (message.IndexOf("MANAGEMENT: TCP Socket listening on") != -1)
					{
					}

					if (message.IndexOf("TLS: tls_process: killed expiring key") != -1)
					{
						Engine.Logs.Log(LogType.Verbose, Messages.RenewingTls);
					}

					if (message.IndexOf("Initialization Sequence Completed With Errors") != -1)
					{
						SetReset("ERROR");
					}

					// Detect connection (OpenVPN >2.4)
					if (UtilsString.RegExMatchOne(messageLower, "peer connection initiated with \\[af_inet6?\\]([0-9a-f\\.\\:]+?):(\\d+?)") != "")
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
							string t = messageLower;
							t = t.Replace("[nonblock]", "").Trim();
							List<string> fields = UtilsString.RegExMatchSingle(t, "\\[af_inet6?\\]([a-z90-9\\.\\:]+?):(\\d+?)$");
							if (fields.Count == 2)
							{
								m_connectionActive.EntryIP = fields[0];
								m_connectionActive.Port = Conversions.ToInt32(fields[1]);
							}
						}
					}

					// Detect TCP connection (OpenVPN <2.4)
					if (message.IndexOf("Attempting to establish TCP connection with [AF_INET]") != -1)
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
					if (message.IndexOf("UDPv4 link remote: [AF_INET]") != -1)
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

					if (UtilsString.RegExMatchOne(messageLower, "^management: tcp socket listening on \\[af_inet6?\\]([0-9a-f\\.\\:]+?)$") != "")
					{
						ConnectManagementSocket();
					}

					if (message.IndexOf("Initialization Sequence Completed") != -1)
					{
						ConnectedStep();
					}

					if (message.IndexOf("Client connected from [AF_INET]127.0.0.1") != -1)
					{
					}

					// Windows
					if (Platform.Instance.IsUnixSystem() == false)
					{
						List<string> matchInterface = UtilsString.RegExMatchSingle(message, "TAP-.*\\\\(.+?).tap");
						if (matchInterface != null)
						{
							m_connectionActive.InterfaceId = matchInterface[0];
							
							CheckTunNetworkInterface();							
						}

						// Match name - 2.11.10
						// Note: Windows allow [] chars in interface name, but OpenVPN use ] to close the name and don't escape it, so "\\sopened" it's required for lazy regex.
						List<string> matchName = UtilsString.RegExMatchSingle(message, "TAP-.*\\sdevice\\s\\[(.*?)\\]\\sopened");
						if (matchName != null)
						{
							m_connectionActive.InterfaceName = matchName[0];
						}
					}

					// Unix
					if (Platform.Instance.IsUnixSystem())
					{
						Match match = Regex.Match(message, "TUN/TAP device (.*?) opened");
						if (match.Success)
						{
							m_connectionActive.InterfaceName = match.Groups[1].Value;
							m_connectionActive.InterfaceId = match.Groups[1].Value;
							CheckTunNetworkInterface();
						}
					}

					// OSX
					{
						Match match = Regex.Match(message, "Opened utun device (.*?)$");
						if (match.Success)
						{
							m_connectionActive.InterfaceName = match.Groups[1].Value;
							m_connectionActive.InterfaceId = match.Groups[1].Value;
							CheckTunNetworkInterface();							
						}
					}

					if (Platform.Instance.IsWindowsSystem())
					{
						// Workaround (2018/01/28) for this bug: https://airvpn.org/topic/25139-why-exists-pull-filter-ignore-dhcp-option-dns6/
						if (message.IndexOf("metric 0") != -1) // To catch only one, the main
						{
							IpAddress ipv6rangeIp = new IpAddress(UtilsString.RegExMatchOne(message, "add_route_ipv6\\((.*?)\\s"));
							if (ipv6rangeIp.Valid)
							{
								string routeIPv6 = SystemShell.ShellCmd("route -6 print");
								string iFace = UtilsString.RegExMatchOne(UtilsString.StringCleanSpace(routeIPv6).ToLowerInvariant(), "(\\d+?)\\s\\d+\\s" + ipv6rangeIp.ToCIDR() + "\\son-link");
								if (iFace != "")
								{
									Engine.Instance.Logs.LogVerbose("Detected an OpenVPN bug (On-Link route on VPN range), autofix.");
									SystemShell.Shell1(Platform.Instance.LocateExecutable("netsh.exe"), "interface ipv6 del route \"" + ipv6rangeIp.ToCIDR() + "\" interface=\"" + SystemShell.EscapeInt(iFace) + "\"");
								}
							}
						}
					}

					// Push directives management
					if (m_connectionActive != null)
					{
						bool pushLog = false;
						string pushDirectivesLine = UtilsString.RegExMatchOne(message, "PUSH: Received control message: '(.*?)'");
						if (pushDirectivesLine != "")
						{
							pushLog = true;
							List<string> pushDirectives = UtilsString.StringToList(pushDirectivesLine, ",");
							foreach (string directive in pushDirectives)
							{
								if (directive == "PUSH_REPLY")
									continue;
								m_connectionActive.PendingPushDetected.Add(directive);
							}

						}

						string pushDirectivesFiltered = UtilsString.RegExMatchOne(message, "Pushed option removed by filter: '(.*?)'");
						if (pushDirectivesFiltered != "")
						{
							pushLog = true;
							m_connectionActive.PendingPushDetected.Remove(pushDirectivesFiltered);
						}

						// OpenVPN log "PUSH: Received control message:" and after a series of "Pushed option removed by filter:".
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
					if (message.IndexOf("enter \"y\" to update PuTTY's cache and continue connecting") != -1)
						m_processProxy.StandardInput.WriteLine("y");

					if (message.IndexOf("If you trust this host, enter \"y\" to add the key to") != -1)
						m_processProxy.StandardInput.WriteLine("y");

					if (message == "Access granted") // PLink Windows
					{
						StartOpenVpnProcess();
					}

					if (message.StartsWith("Authenticated to")) // SSH Linux
					{
						StartOpenVpnProcess();
					}

					if (log)
						Engine.Logs.Log(LogType.Verbose, source + " > " + message);
				}
				else if (source == "SSL")
				{
					bool log = true;

					if (message.IndexOf("Configuration successful") != -1)
					{
						StartOpenVpnProcess();
					}

					if (log)
						Engine.Logs.Log(LogType.Verbose, source + " > " + message);
				}
				else if (source == "Management")
				{
					ProcessOutputManagement(source, message);
				}
			}
			catch (Exception ex)
			{
				Engine.Logs.Log(LogType.Warning, ex);

				SetReset("ERROR");
			}
		}

		public void ConnectManagementSocket()
		{
			if (m_openVpnManagementSocket == null)
			{
				Engine.Logs.Log(LogType.Verbose, Messages.ConnectionStartManagement);

				m_openVpnManagementSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				m_openVpnManagementSocket.Connect("127.0.0.1", Engine.Storage.GetInt("openvpn.management_port"));
				m_openVpnManagementSocket.SendTimeout = 5000;
				m_openVpnManagementSocket.ReceiveTimeout = 5000;

				SendManagementCommand(m_connectionActive.ManagementPassword);
			}
		}

		public void CheckTunNetworkInterface()
		{
			m_interfaceScope = new InterfaceScope(m_connectionActive.InterfaceId);

			NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface adapter in interfaces)
			{
				if (adapter.Id == m_connectionActive.InterfaceId)
				{
					m_interfaceTun = adapter;
					m_interfaceTunBytesReadInitial = -1;
					m_interfaceTunBytesWriteInitial = -1;
					m_interfaceTunBytesLastRead = -1;
					m_interfaceTunBytesLastWrite = -1;

					Json jInfo = Engine.Instance.FindNetworkInterfaceInfo(m_connectionActive.InterfaceId);

					if ((m_connectionActive.TunnelIPv4) && (jInfo != null) && (jInfo.HasKey("support_ipv4")) && (Conversions.ToBool(jInfo["support_ipv4"].Value) == false))
					{
						Engine.Instance.Logs.LogWarning(Messages.IPv4NotSupportedByNetworkAdapter);
						if( (Engine.Instance.Storage.GetBool("network.ipv4.autoswitch")) && (Engine.Instance.Storage.Get("network.ipv4.mode") != "block") )
						{
							Engine.Instance.Logs.LogWarning(Messages.IPv4NotSupportedByNetworkAdapterAutoSwitch);
							Engine.Instance.Storage.Set("network.ipv4.mode", "block");
						}
					}
					if ((m_connectionActive.TunnelIPv6) && (jInfo != null) && (jInfo.HasKey("support_ipv6")) && (Conversions.ToBool(jInfo["support_ipv6"].Value) == false))
					{
						Engine.Instance.Logs.LogWarning(Messages.IPv6NotSupportedByNetworkAdapter);
						if ((Engine.Instance.Storage.GetBool("network.ipv6.autoswitch")) && (Engine.Instance.Storage.Get("network.ipv6.mode") != "block"))
						{
							Engine.Instance.Logs.LogWarning(Messages.IPv6NotSupportedByNetworkAdapterAutoSwitch);
							Engine.Instance.Storage.Set("network.ipv6.mode", "block");
						}
					}
				}
			}
		}

		public void ConnectedStep()
		{
			Platform.Instance.OnInterfaceDo(m_connectionActive.InterfaceId);

			IpAddresses dns = new IpAddresses(Engine.Instance.Storage.Get("dns.servers"));
			if (dns.Count == 0)
				dns = Engine.ConnectionActive.OpenVpnProfileWithPush.ExtractDns();

			if (dns.Count != 0)
				Platform.Instance.OnDnsSwitchDo(m_connectionActive, dns);

			if (Engine.Instance.Storage.GetBool("routes.remove_default"))
				Platform.Instance.OnRouteDefaultRemoveDo();

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

			Engine.WaitMessageSet(Messages.ConnectionFlushDNS, true);

			Platform.Instance.FlushDNS();

			// 2.4: Sometime (only under Windows) Interface is not really ready...
			if (Platform.Instance.WaitTunReady() == false)
				SetReset("ERROR");

			if (m_connectionActive.ExitIPs.Count == 0)
			{
				Engine.WaitMessageSet(Messages.ConnectionDetectExit, true);
				Engine.Logs.Log(LogType.Verbose, Messages.ConnectionDetectExit);
				m_connectionActive.ExitIPs.Add(Engine.Instance.DiscoverExit());
			}

			Engine.Instance.NetworkLockManager.OnVpnEstablished();

			if (Engine.CurrentServer.Provider is Providers.Service)
			{
				Providers.Service service = Engine.CurrentServer.Provider as Providers.Service;

				if (Engine.CurrentServer.SupportCheck == false)
				{
					Engine.Logs.Log(LogType.Warning, Messages.ConnectionCheckingRouteNotAvailable);
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
							Engine.WaitMessageSet(Messages.ConnectionCheckingRouteIPv4, true);
							for (int t = 0; t < nTry; t++)
							{
								if (m_reset != "")
									break;

								if (t == 0)
									Engine.Logs.Log(LogType.Info, Messages.ConnectionCheckingRouteIPv4);
								else
								{
									Engine.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.ConnectionCheckingTryRoute, (t + 1).ToString()));
									System.Threading.Thread.Sleep(t * 1000);
								}

								try
								{
									string checkDomain = Engine.CurrentServer.ProviderName.ToLowerInvariant() + "_exit." + service.GetKeyValue("check_domain", "");
									string checkUrl = "https://" + checkDomain + "/check_tun/";
									/* 2.14.0
									HttpRequest httpRequest = new HttpRequest();
									httpRequest.Url = checkUrl;
									httpRequest.BypassProxy = true;								
									httpRequest.ForceResolve = checkDomain + ":" + Engine.CurrentServer.IpsExit.ToStringFirstIPv4();
									XmlDocument xmlDoc = Engine.FetchUrlXml(httpRequest);

									string answer = xmlDoc.DocumentElement.Attributes["ip"].Value;

									if (m_connectionActive.OpenVpnProfileWithPush.ExtractVpnIPs().ContainsAddress(answer) == false)
										throw new Exception(MessagesFormatter.Format(Messages.ConnectionCheckingTryRouteFail, answer));

									Engine.ConnectedServerTime = Conversions.ToInt64(xmlDoc.DocumentElement.Attributes["time"].Value);
									Engine.ConnectedClientTime = UtilsCore.UnixTimeStamp();
									*/

									HttpRequest httpRequest = new HttpRequest();
									httpRequest.Url = checkUrl;
									httpRequest.BypassProxy = true;
									httpRequest.IpLayer = "4";
									httpRequest.ForceResolve = checkDomain + ":" + Engine.CurrentServer.IpsExit.OnlyIPv4.First.Address;
									XmlDocument xmlDoc = Engine.FetchUrlXml(httpRequest);

									string answer = xmlDoc.DocumentElement.Attributes["ip"].Value;

									if (m_connectionActive.OpenVpnProfileWithPush.ExtractVpnIPs().OnlyIPv4.ContainsAddress(answer) == false)
										throw new Exception(MessagesFormatter.Format(Messages.ConnectionCheckingTryRouteFail, answer));

									m_connectionActive.TimeServer = Conversions.ToInt64(xmlDoc.DocumentElement.Attributes["time"].Value);
									m_connectionActive.TimeClient = UtilsCore.UnixTimeStamp();

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
								Engine.Logs.Log(LogType.Error, Messages.ConnectionCheckingRouteIPv4Failed);
								SetReset("ERROR");
							}
						}

						if ((m_connectionActive.TunnelIPv6) && (Engine.CurrentServer.SupportIPv6))
						{
							bool ok = false;
							Engine.WaitMessageSet(Messages.ConnectionCheckingRouteIPv6, true);
							for (int t = 0; t < nTry; t++)
							{
								if (m_reset != "")
									break;

								if (t == 0)
									Engine.Logs.Log(LogType.Info, Messages.ConnectionCheckingRouteIPv6);
								else
								{
									Engine.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.ConnectionCheckingTryRoute, (t + 1).ToString()));
									System.Threading.Thread.Sleep(t * 1000);
								}

								try
								{
									string checkDomain = Engine.CurrentServer.ProviderName.ToLowerInvariant() + "_exit." + service.GetKeyValue("check_domain", "");
									string checkUrl = "https://" + checkDomain + "/check_tun/";
									/* 2.14.0
									HttpRequest httpRequest = new HttpRequest();
									httpRequest.Url = checkUrl;
									httpRequest.BypassProxy = true;								
									httpRequest.ForceResolve = checkDomain + ":" + Engine.CurrentServer.IpsExit.ToStringFirstIPv4();
									XmlDocument xmlDoc = Engine.FetchUrlXml(httpRequest);

									string answer = xmlDoc.DocumentElement.Attributes["ip"].Value;

									if (m_connectionActive.OpenVpnProfileWithPush.ExtractVpnIPs().ContainsAddress(answer) == false)
										throw new Exception(MessagesFormatter.Format(Messages.ConnectionCheckingTryRouteFail, answer));

									Engine.ConnectedServerTime = Conversions.ToInt64(xmlDoc.DocumentElement.Attributes["time"].Value);
									Engine.ConnectedClientTime = UtilsCore.UnixTimeStamp();
									*/

									HttpRequest httpRequest = new HttpRequest();
									httpRequest.Url = checkUrl;
									httpRequest.BypassProxy = true;
									httpRequest.IpLayer = "6";
									httpRequest.ForceResolve = checkDomain + ":" + Engine.CurrentServer.IpsExit.OnlyIPv6.First.Address;
									XmlDocument xmlDoc = Engine.FetchUrlXml(httpRequest);

									string answer = xmlDoc.DocumentElement.Attributes["ip"].Value;

									if (m_connectionActive.OpenVpnProfileWithPush.ExtractVpnIPs().OnlyIPv6.ContainsAddress(answer) == false)
										throw new Exception(MessagesFormatter.Format(Messages.ConnectionCheckingTryRouteFail, answer));

									m_connectionActive.TimeServer = Conversions.ToInt64(xmlDoc.DocumentElement.Attributes["time"].Value);
									m_connectionActive.TimeClient = UtilsCore.UnixTimeStamp();

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
								Engine.Logs.Log(LogType.Error, Messages.ConnectionCheckingRouteIPv6Failed);
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
								m_connectionActive.RealIp = Messages.NotAvailable;
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
									m_connectionActive.TimeClient = UtilsCore.UnixTimeStamp();

									m_connectionActive.RealIp = xmlDoc.DocumentElement.Attributes["ip"].Value;
								}
								catch (Exception e)
								{
									Engine.Logs.Log(e);

									m_connectionActive.RealIp = Messages.NotAvailable;
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
						Engine.WaitMessageSet(Messages.ConnectionCheckingDNS, true);

						bool ok = false;
						int nTry = 3;
						if (Engine.Instance.Storage.GetBool("windows.workarounds"))
							nTry = 10;

						for (int t = 0; t < nTry; t++)
						{
							if (m_reset != "")
								break;

							if (t == 0)
								Engine.Logs.Log(LogType.Info, Messages.ConnectionCheckingDNS);
							else
							{
								Engine.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.ConnectionCheckingTryDNS, (t + 1).ToString()));
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
								//	throw new Exception(MessagesFormatter.Format(Messages.ConnectionCheckingTryDNSFail, "No DNS answer"));								

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
									throw new Exception(MessagesFormatter.Format(Messages.ConnectionCheckingTryDNSFail, answer));

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
							Engine.Logs.Log(LogType.Error, Messages.ConnectionCheckingDNSFailed);
							SetReset("ERROR");
						}
					}
				}
			}

			if (m_reset == "")
			{
				Engine.RunEventCommand("vpn.up");

				Engine.Logs.Log(LogType.InfoImportant, Messages.ConnectionConnected);
				Engine.SetConnected(true);
				m_connectionActive.TimeStart = DateTime.UtcNow;

				if (Engine.Instance.Storage.GetBool("advanced.testonly"))
					Engine.RequestStop();
			}
		}

		public void ProcessOutputManagement(string source, string message)
		{
			// Ignore, useless			
			if (message.Contains("OpenVPN Management Interface Version 1 -- type 'help' for more info"))
				return;
			if (message.StartsWith("ENTER PASSWORD:"))
				return;
			if (message.StartsWith("SUCCESS: password is correct"))
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

		public void UpdateBytesStats(Int64 read, Int64 write)
		{
			if (read < 0)
				read = 0; // Unexpected, ignore if it happens
			if (write < 0)
				write = 0; // Unexpected, ignore if it happens
			Int64 deltaRead = read;
			if (m_interfaceTunBytesLastRead != -1)
			{
				deltaRead = read - m_interfaceTunBytesLastRead;
				if (deltaRead < 0)
					deltaRead = 0; // Unexpected, ignore if it happens
			}
			Engine.SessionStatsRead += deltaRead;
			m_connectionActive.BytesRead += deltaRead;

			Int64 deltaWrite = write;
			if (m_interfaceTunBytesLastWrite != -1)
			{
				deltaWrite = write - m_interfaceTunBytesLastWrite;
				if (deltaWrite < 0)
					deltaWrite = 0; // Unexpected, ignore if it happens
			}
			Engine.SessionStatsWrite += deltaWrite;
			m_connectionActive.BytesWrite += deltaWrite;

			if (m_interfaceTunBytesLastRead != -1)
			{
				int delta = m_interfaceTunBytesLastTick.Reset();
				if (delta > 0)
				{
					m_connectionActive.BytesLastDownloadStep = (1000 * (deltaRead)) / delta;
					m_connectionActive.BytesLastUploadStep = (1000 * (deltaWrite)) / delta;
				}
			}

			m_interfaceTunBytesLastRead = read;
			m_interfaceTunBytesLastWrite = write;

			Engine.Instance.Stats.Charts.Hit(m_connectionActive.BytesLastDownloadStep, m_connectionActive.BytesLastUploadStep);

			Engine.OnRefreshUi(Core.Engine.RefreshUiMode.Stats);

			Engine.RaiseStatus();
		}
	}
}