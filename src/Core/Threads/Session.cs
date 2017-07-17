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
using Eddie.Lib.Common;
using Eddie.Core;

namespace Eddie.Core.Threads
{
	public class Session : Eddie.Core.Thread
	{
		private Process m_processOpenVpn;
		private Process m_processProxy;

		private Socket m_openVpnManagementSocket;
		private List<string> m_openVpnManagementCommands = new List<string>();
		private List<string> m_openVpnManagementStatisticsLines = new List<string>();

		private string m_reset = "";
		private NetworkInterface m_interfaceTun;
		private int m_timeLastStatus = 0;
		private TemporaryFile m_fileSshKey;
		private TemporaryFile m_fileSslCrt;
		private TemporaryFile m_fileSslConfig;
		private TemporaryFile m_fileOvpn;		
		private ProgramScope m_programScope = null;
		private InterfaceScope m_interfaceScope = null;

		private Int64 m_interfaceTunBytesReadInitial = -1;
		private Int64 m_interfaceTunBytesWriteInitial = -1;
		private Int64 m_interfaceTunBytesLastRead = -1;
		private Int64 m_interfaceTunBytesLastWrite = -1;
		private TimeDelta m_interfaceTunBytesLastTick = new TimeDelta();

		private OvpnBuilder m_ovpnStartup;
		private OvpnBuilder m_ovpnWithPush;

		public override void OnRun()
		{
			CancelRequested = false;

			Engine.ConnectedSessionStatsRead = 0;
			Engine.ConnectedSessionStatsWrite = 0;

			string sessionLastServer = "";

			bool oneConnectionReached = false;

			for (; CancelRequested == false; )
			{
				RouteScope routeScope = null;

				bool allowed = true;
				string waitingMessage = "";
				int waitingSecs = 0;
				m_processOpenVpn = null;
				m_processProxy = null;

				Engine.ConnectedProtocol = "";
				Engine.ConnectedEntryIP.Clear();
				Engine.ConnectedPort = 0;
				Engine.ConnectedServerTime = 0;
				Engine.ConnectedClientTime = 0;
				Engine.ConnectedRealIp = Messages.NotAvailable;
				Engine.ConnectedControlChannel = Messages.NotAvailable;
				Engine.ConnectedVpnInterfaceName = "";
				Engine.ConnectedVpnInterfaceId = "";
				Engine.ConnectedVpnStatsRead = 0;
				Engine.ConnectedVpnStatsWrite = 0;
				Engine.ConnectedLastDownloadStep = -1;
				Engine.ConnectedLastUploadStep = -1;

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
									string userMessage = Utils.XmlGetAttributeString(xmlDoc.DocumentElement, "message", "");
									if (userMessage != "")
									{
										allowed = false;
										string userMessageAction = Utils.XmlGetAttributeString(xmlDoc.DocumentElement, "message_action", "");
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

					try
					{	
						m_ovpnStartup = Engine.CurrentServer.BuildOVPN(false);
						Engine.WaitMessageSet(Messages.ConnectionCredentials, true);
						if (Engine.CurrentServer.Provider.ApplyCredentials(m_ovpnStartup) == false)
						{
							allowed = false;
							CancelRequested = true;
							SetReset("FATAL");
						}
						else
						{
							m_ovpnWithPush = m_ovpnStartup.Clone();
							Engine.Instance.ConnectedOVPN = m_ovpnWithPush;							
						}
					}
					catch(Exception e)
					{
						Engine.Logs.Log(e);
						allowed = false;
						SetReset("ERROR");
					}

					if (allowed)
					{
						Engine.Instance.ConnectedProtocol = m_ovpnStartup.Protocol;

						sessionLastServer = Engine.CurrentServer.Code;
						Engine.Storage.Set("servers.last", Engine.CurrentServer.Code);

						routeScope = new RouteScope(Engine.ConnectedEntryIP); // Clodo: Urgent, may not work under some OS with NetLock active. Try to add the RouteScope when detecting protocol from OpenVPN logs.

						Engine.RunEventCommand("vpn.pre");

						string connectingMessage = MessagesFormatter.Format(Messages.ConnectionConnecting, Engine.CurrentServer.GetNameWithLocation());
						Engine.WaitMessageSet(connectingMessage, true);
						Engine.Logs.Log(LogType.InfoImportant, connectingMessage);


						if (m_ovpnStartup.Protocol == "SSH")
						{
							StartSshProcess();
						}
						else if (m_ovpnStartup.Protocol == "SSL")
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
							if ((m_processOpenVpn != null) && (m_processOpenVpn.ReallyExited)) // 2.2
								SetReset("ERROR");
							if( (m_processProxy != null) && (m_processProxy.ReallyExited) ) // 2.2
								SetReset("ERROR");

							if (Engine.IsConnected())
								break;

							if (m_reset != "")
								break;

							Sleep(waitingSleep);
						}


						if(m_reset == "")
							oneConnectionReached = true;

						// -----------------------------------
						// Phase 3 - Running
						// -----------------------------------

						if (m_reset == "")
						{
							for (; ; )
							{
								int timeNow = Utils.UnixTimeStamp();

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
									else if(Platform.Instance.GetTunStatsMode() == "NetworkInterface")
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
							if (m_processOpenVpn.ReallyExited == false)
								m_processOpenVpn.Kill();
						}

						// -----------------------------------
						// Phase 5 - Waiting disconnection
						// -----------------------------------

						TimeDelta DeltaSigTerm = null;

						for (; ; )
						{
							try
							{
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
										m_processOpenVpn.Kill();
								}

								// Proxy (SSH/SSL) process
								if ((m_processProxy != null) && (m_processOpenVpn != null) && (m_processProxy.ReallyExited == false) && (m_processOpenVpn.ReallyExited == true))
								{
									m_processProxy.Kill();
								}

								// Start a clean disconnection
								if ((m_processOpenVpn != null) && (m_openVpnManagementSocket != null) && (m_processOpenVpn.ReallyExited == false) && (m_openVpnManagementSocket.Connected))
								{
									bool sendSignal = false;
									if (DeltaSigTerm == null)
									{
										DeltaSigTerm = new TimeDelta();
										sendSignal = true;
									}
									else if (DeltaSigTerm.Elapsed(10000)) // Try a SIGTERM every 10 seconds
										sendSignal = true;

									if(sendSignal)
									{
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

						Platform.Instance.OnRouteDefaultRemoveRestore();

						Platform.Instance.OnDnsSwitchRestore();

						Platform.Instance.OnInterfaceRestore();

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
						if (m_fileOvpn != null)
						{
							m_fileOvpn.Close();
							m_fileOvpn = null;
						}						
						if (m_ovpnStartup != null)
						{
							m_ovpnStartup.Close();
							m_ovpnStartup = null;
							m_ovpnWithPush = null;
						}
						Engine.Instance.ConnectedOVPN = null;
					}



				}
				catch (Exception e)
				{
					// Warning: Avoid to reach this catch: unpredicable status of running processes.
					Engine.SetConnected(false);

					Engine.Logs.Log(LogType.Warning, e);
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
			Providers.Service service = Engine.CurrentServer.Provider as Providers.Service;
			if (service == null) // Unexpected
				return;

			if (m_processProxy != null) // Unexpected
				return;

			string fileKeyExtension = "";
			if (Platform.Instance.IsUnixSystem())
				fileKeyExtension = "key";
			else
				fileKeyExtension = "ppk";


			m_fileSshKey = new TemporaryFile(fileKeyExtension);
			Platform.Instance.FileContentsWriteText(m_fileSshKey.Path, Utils.XmlGetAttributeString(service.User, "ssh_" + fileKeyExtension, ""));

			if (Platform.Instance.IsUnixSystem())
			{
				// under OS X, SSH change it's UID to normal user.
				if (Platform.Instance.GetCode() != "MacOS")
				{
					Platform.Instance.FileEnsurePermission(m_fileSshKey.Path, "600");
				}
			}

			string arguments = "";

			arguments += " -i \"" + m_fileSshKey.Path + "\" -L " + Conversions.ToString(m_ovpnStartup.ProxyPort) + ":127.0.0.1:2018 sshtunnel@" + m_ovpnStartup.Address;
			if (Platform.Instance.IsUnixSystem())
				arguments += " -p " + m_ovpnStartup.Port; // ssh use -p
			else
				arguments += " -P " + m_ovpnStartup.Port; // plink use -P

			if (Platform.Instance.IsUnixSystem())
				arguments += " -o UserKnownHostsFile=/dev/null -o StrictHostKeyChecking=no"; // TOOPTIMIZE: To bypass key confirmation. Not the best approach.
			arguments += " -N -T -v";

			m_programScope = new ProgramScope(Software.GetTool("ssh").Path, "SSH Tunnel");

			m_processProxy = new Process();
			m_processProxy.StartInfo.FileName = Software.GetTool("ssh").Path;
			m_processProxy.StartInfo.Arguments = arguments;
			m_processProxy.StartInfo.WorkingDirectory = Utils.GetTempPath();

			m_processProxy.StartInfo.Verb = "run";
			m_processProxy.StartInfo.CreateNoWindow = true;
			m_processProxy.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			m_processProxy.StartInfo.UseShellExecute = false;
			m_processProxy.StartInfo.RedirectStandardInput = true;
			m_processProxy.StartInfo.RedirectStandardError = true;
			m_processProxy.StartInfo.RedirectStandardOutput = true;

			m_processProxy.ErrorDataReceived += new DataReceivedEventHandler(ProcessSshOutputDataReceived);
			m_processProxy.OutputDataReceived += new DataReceivedEventHandler(ProcessSshOutputDataReceived);
			m_processProxy.Exited += new EventHandler(ProcessSshExited);

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
			Platform.Instance.FileContentsWriteText(m_fileSslCrt.Path, Utils.XmlGetAttributeString(service.User, "ssl_crt", ""));

			m_fileSslConfig = new TemporaryFile("ssl");

			string sslConfig = "";

			if (Platform.Instance.IsUnixSystem())
			{
				//sslConfig += "output = /dev/stdout\n"; // With this, with new stunnel 5.01, we have duplicated output dump.
				//sslConfig += "pid = /tmp/stunnel4.pid\n";
				sslConfig += "foreground = yes\n"; // Without this, the process fork and it's exit can't be detected.
				sslConfig += "pid = /tmp/" + RandomGenerator.GetHash() + ".pid\n"; // 2.2
			}
			if(Engine.Instance.Storage.Get("ssl.options") != "")
				sslConfig += "options = " + Engine.Instance.Storage.Get("ssl.options") + "\n";
			sslConfig += "client = yes\n";
			sslConfig += "debug = 6\n";
			sslConfig += "\n";
			sslConfig += "[openvpn]\n";
			sslConfig += "accept = 127.0.0.1:" + Conversions.ToString(m_ovpnStartup.ProxyPort) + "\n";
			sslConfig += "connect = " + m_ovpnStartup.Address + ":" + m_ovpnStartup.Port + "\n";
			sslConfig += "TIMEOUTclose = 0\n";
			if (Engine.Instance.Storage.GetInt("ssl.verify") != -1)
				sslConfig += "verify = " + Engine.Instance.Storage.GetInt("ssl.verify").ToString() + "\n";
			//sslConfig += "CAfile = \"" + m_fileSslCrt.Path + "\"\n";
			sslConfig += "CAfile = " + m_fileSslCrt.Path + "\n";
			sslConfig += "\n";

			string sslConfigPath = m_fileSslConfig.Path;
			Platform.Instance.FileContentsWriteText(sslConfigPath, sslConfig);

			m_programScope = new ProgramScope(Software.GetTool("ssl").Path, "SSL Tunnel");

			m_processProxy = new Process();
			m_processProxy.StartInfo.FileName = Software.GetTool("ssl").Path;
			m_processProxy.StartInfo.Arguments = "\"" + sslConfigPath + "\"";
			m_processProxy.StartInfo.WorkingDirectory = Utils.GetTempPath();

			m_processProxy.StartInfo.Verb = "run";
			m_processProxy.StartInfo.CreateNoWindow = true;
			m_processProxy.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			m_processProxy.StartInfo.UseShellExecute = false;
			m_processProxy.StartInfo.RedirectStandardInput = true;
			m_processProxy.StartInfo.RedirectStandardError = true;
			m_processProxy.StartInfo.RedirectStandardOutput = true;

			m_processProxy.ErrorDataReceived += new DataReceivedEventHandler(ProcessSslOutputDataReceived);
			m_processProxy.OutputDataReceived += new DataReceivedEventHandler(ProcessSslOutputDataReceived);
			m_processProxy.Exited += new EventHandler(ProcessSslExited);

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

			m_fileOvpn = new TemporaryFile("ovpn");
			string ovpnPath = m_fileOvpn.Path;
			Platform.Instance.FileContentsWriteText(ovpnPath, m_ovpnStartup.Get());

			if(m_processProxy == null)
				m_programScope = new ProgramScope(Software.GetTool("openvpn").Path, "OpenVPN Tunnel");

			m_processOpenVpn = new Process();
			m_processOpenVpn.StartInfo.FileName = Software.GetTool("openvpn").Path;
			m_processOpenVpn.StartInfo.Arguments = "";
			m_processOpenVpn.StartInfo.WorkingDirectory = Utils.GetTempPath();

			if (Storage.Simulate)
			{
				m_processOpenVpn.StartInfo.FileName = "Simulate.exe";
				Sleep(1000);
				Engine.SetConnected(true);
			}

			m_processOpenVpn.StartInfo.Arguments = "--config \"" + ovpnPath + "\" ";

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
			if(m_reset == "")
				SetReset("ERROR");
		}

		public void SetReset(string level)
		{
			// 2.11.8
			if (level == "")
				m_reset = "";
			else if(m_reset == "")
				m_reset = level;

			// 2.11.7 version, for reference
			//m_reset = level;
		}

		public void SendManagementCommand(string Cmd)
		{
			if (Cmd == "k1")
			{
				m_openVpnManagementSocket.Close();
			}
			else if (Cmd == "k2")
			{
				m_processOpenVpn.Kill();
			}
			else if (Cmd == "k3")
			{
				m_processProxy.Kill();
			}

			if (m_openVpnManagementSocket == null)
				return;

			if (m_openVpnManagementSocket.Connected == false)
				return;

			lock (this)
			{
				m_openVpnManagementCommands.Add(Cmd);
			}
		}

		void ProcessSshOutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			// TOCHECK: Must wait until a \n ?
			if (e.Data != null)
			{
				string message = e.Data.ToString();

				ProcessOutput("SSH", message);
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

				ProcessOutput("SSL", message);
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

				ProcessOutput("OpenVPN", message);
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
							if (command != "status")
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

						ProcessOutput("Management", data);
					}
				}
			}
			catch (Exception ex)
			{
				Engine.Logs.Log(LogType.Warning, ex);

				SetReset("ERROR");
			}
		}

		void ProcessOutput(string source, string message)
		{
			// Try to match lower/insensitive case when possible.
			string messageLower = message.ToLowerInvariant();

			if (messageLower.Trim() == "") // 2.10.1
				return;

			try
			{
				if (source == "OpenVPN")
				{
					bool log = true;
					LogType logType = LogType.Verbose;

					if (message.IndexOf("MANAGEMENT: CMD 'status'") != -1)
						log = false;

					if (messageLower.StartsWith("warning:"))
						logType = LogType.Warning;

					if (messageLower.StartsWith("error:"))
						logType = LogType.Error;

					if (messageLower.StartsWith("fatal:"))
						logType = LogType.Error;

					if (Engine.Instance.Storage.Get("routes.default") == "out")
					{
						// Don't warning users for correct behiavour.
						if (
							(message.IndexOf("Options error: option 'redirect-gateway' cannot be used in this context ([PUSH-OPTIONS])") != -1) ||
							(message.IndexOf("Options error: option 'dhcp-option' cannot be used in this context ([PUSH-OPTIONS])") != -1)
						   )
							log = false;
					}

					if(message.StartsWith("Options error:"))
					{
						if (log)
						{
							List<string> matches = Utils.RegExMatchSingle(message, "Options error\\: Unrecognized option or missing parameter\\(s\\) in (.*?)\\:\\d+\\:(.*?)\\(.*\\)");
							if( (matches != null) && (matches.Count == 2) )
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

					// Detect connection (OpenVPN >2.4)
					if (Utils.RegExMatchOne(messageLower, "peer connection initiated with \\[af_inet6?\\]([0-9a-f\\.\\:]+?):(\\d+?)") != "")
					{
						//ClodoTemp Engine.Instance.ConnectedProtocol = m_ovpnStartup.Protocol;
						if (m_ovpnStartup.Protocol == "SSH")
						{
							Engine.Instance.ConnectedEntryIP = m_ovpnStartup.Address;
							Engine.Instance.ConnectedPort = m_ovpnStartup.Port;
						}
						else if (m_ovpnStartup.Protocol == "SSL")
						{
							Engine.Instance.ConnectedEntryIP = m_ovpnStartup.Address;
							Engine.Instance.ConnectedPort = m_ovpnStartup.Port;
						}
						else
						{
							string t = messageLower;
							t = t.Replace("[nonblock]", "").Trim();							
							List<string> fields = Utils.RegExMatchSingle(t, "\\[af_inet6?\\]([a-z90-9\\.\\:]+?):(\\d+?)$");
							if (fields.Count == 2)
							{
								Engine.Instance.ConnectedEntryIP = fields[0];
								Engine.Instance.ConnectedPort = Conversions.ToInt32(fields[1]);
							}
						}
					}

					// Detect TCP connection (OpenVPN <2.4)
					if (message.IndexOf("Attempting to establish TCP connection with [AF_INET]") != -1)
					{
						if (m_ovpnStartup.Protocol == "SSH")
						{
							//ClodoTemp Engine.Instance.ConnectedProtocol = "SSH";
							Engine.Instance.ConnectedEntryIP = m_ovpnStartup.Address;
							Engine.Instance.ConnectedPort = m_ovpnStartup.Port;
						}
						else if (m_ovpnStartup.Protocol == "SSL")
						{
							//ClodoTemp Engine.Instance.ConnectedProtocol = "SSL";
							Engine.Instance.ConnectedEntryIP = m_ovpnStartup.Address;
							Engine.Instance.ConnectedPort = m_ovpnStartup.Port;
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
								//ClodoTemp Engine.Instance.ConnectedProtocol = "TCP";
								Engine.Instance.ConnectedEntryIP = parts[0];
								Engine.Instance.ConnectedPort = Convert.ToInt32(parts[1]);
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
							//ClodoTemp Engine.Instance.ConnectedProtocol = "UDP";
							Engine.Instance.ConnectedEntryIP = parts[0];
							Engine.Instance.ConnectedPort = Convert.ToInt32(parts[1]);
						}
					}

					if (message.StartsWith("Control Channel: "))
					{
						Engine.Instance.ConnectedControlChannel = message.Substring("Control Channel: ".Length);
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
						Engine.Logs.Log(LogType.Info, Messages.RenewingTls);
					}

					if (message.IndexOf("Initialization Sequence Completed With Errors") != -1)
					{
						SetReset("ERROR");
					}

					if (message.IndexOf("Initialization Sequence Completed") != -1)
					{
						Engine.Logs.Log(LogType.Verbose, Messages.ConnectionStartManagement);

						m_openVpnManagementSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
						m_openVpnManagementSocket.Connect("127.0.0.1", Engine.Storage.GetInt("openvpn.management_port"));
						m_openVpnManagementSocket.SendTimeout = 5000;
						m_openVpnManagementSocket.ReceiveTimeout = 5000;						
					}

					if (message.IndexOf("Client connected from [AF_INET]127.0.0.1") != -1)
					{
						Platform.Instance.OnInterfaceDo(Engine.Instance.ConnectedVpnInterfaceId);

						IpAddresses dns = new IpAddresses(Engine.Instance.Storage.Get("dns.servers"));
						if(dns.Count == 0)
							dns = Engine.ConnectedOVPN.ExtractDns();
						
						if(dns.Count != 0)
							Platform.Instance.OnDnsSwitchDo(dns);

						if (Engine.Instance.Storage.GetBool("routes.remove_default"))
							Platform.Instance.OnRouteDefaultRemoveDo();

						Engine.WaitMessageSet(Messages.ConnectionFlushDNS, true);

						Platform.Instance.FlushDNS();

						// 2.4: Sometime (only under Windows) Interface is not really ready...
						if (Platform.Instance.WaitTunReady() == false)
							SetReset("ERROR");

						Engine.Instance.NetworkLockManager.OnVpnEstablished();

						if (Engine.CurrentServer.Provider is Providers.Service)
						{
							Providers.Service service = Engine.CurrentServer.Provider as Providers.Service;

							if(Engine.CurrentServer.SupportCheck == false)
							{
								Engine.Logs.Log(LogType.Warning, Messages.ConnectionCheckingRouteNotAvailable);
							}
							else
							{
								if ((m_reset == "") && (service.CheckTunnel))
								{
									Engine.WaitMessageSet(Messages.ConnectionCheckingRoute, true);

									bool ok = false;
									int nTry = 3;
									if (Engine.Instance.Storage.GetBool("windows.workarounds"))
										nTry = 10;

									for (int t = 0; t < nTry; t++)
									{
										if (m_reset != "")
											break;

										if (t == 0)
											Engine.Logs.Log(LogType.Info, Messages.ConnectionCheckingRoute);
										else
										{
											Engine.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.ConnectionCheckingTryRoute, (t + 1).ToString()));
											System.Threading.Thread.Sleep(t * 1000);
										}

										try
										{
											// <2.11
											// string checkUrl = "https://" + Engine.CurrentServer.ProviderName.ToLowerInvariant() + "_exit." + service.GetKeyValue("check_domain", "") + "/check_tun/";

											// 2.11
											// string checkUrl = "https://" + Engine.CurrentServer.IpExit + ":" + service.GetKeyValue("check_port", "89") + "/check_tun/";
											// string checkDomain = Engine.CurrentServer.ProviderName.ToLowerInvariant() + "_exit." + service.GetKeyValue("check_domain", "");
											// XmlDocument xmlDoc = Engine.XmlFromUrl(checkUrl, checkDomain, null, Messages.ConnectionCheckingRoute, true);

											// 2.12
											string checkDomain = Engine.CurrentServer.ProviderName.ToLowerInvariant() + "_exit." + service.GetKeyValue("check_domain", "");
											string checkUrl = "https://" + checkDomain + "/check_tun/";
											XmlDocument xmlDoc = Engine.FetchUrlXml(checkUrl, null, Messages.ConnectionCheckingRoute, true, checkDomain + ":" + Engine.CurrentServer.IpsExit.ToStringFirstIPv4());

											string answer = xmlDoc.DocumentElement.Attributes["ip"].Value;

											if (Engine.ConnectedOVPN.ExtractVpnIPs().ContainsAddress(answer) == false)
												throw new Exception(MessagesFormatter.Format(Messages.ConnectionCheckingTryRouteFail, answer));

											Engine.ConnectedServerTime = Conversions.ToInt64(xmlDoc.DocumentElement.Attributes["time"].Value);
											Engine.ConnectedClientTime = Utils.UnixTimeStamp();

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
										Engine.Logs.Log(LogType.Error, Messages.ConnectionCheckingRouteFailed);
										SetReset("ERROR");
									}

									if (m_reset == "")
									{
										// Real IP are detected with a request over the server entry IP.
										// Normally this is routed by openvpn outside the tunnel.
										// But if a proxy is active, don't work.
										if (Engine.Instance.Storage.Get("proxy.mode").ToLowerInvariant() != "none")
										{
											Engine.ConnectedRealIp = Messages.NotAvailable;
											Engine.ConnectedServerTime = 0;
										}
										else
										{
											try
											{
												// 2.11
												// string checkUrl = "https://" + Engine.ConnectedEntryIP + ":" + service.GetKeyValue("check_port", "89") + "/check_tun/";
												// string checkDomain = Engine.CurrentServer.ProviderName.ToLowerInvariant() + "." + service.GetKeyValue("check_domain", "");
												// XmlDocument xmlDoc = Engine.XmlFromUrl(checkUrl, checkDomain, null, Messages.ConnectionCheckingRoute2, true);

												// 2.12
												string checkDomain = Engine.CurrentServer.ProviderName.ToLowerInvariant() + "." + service.GetKeyValue("check_domain", "");
												string checkUrl = "https://" + checkDomain + "/check_tun/";
												XmlDocument xmlDoc = Engine.FetchUrlXml(checkUrl, null, Messages.ConnectionCheckingRoute, true, checkDomain + ":" + Engine.ConnectedEntryIP);

												Engine.ConnectedServerTime = Conversions.ToInt64(xmlDoc.DocumentElement.Attributes["time"].Value);
												Engine.ConnectedClientTime = Utils.UnixTimeStamp();

												Engine.ConnectedRealIp = xmlDoc.DocumentElement.Attributes["ip"].Value;
											}
											catch (Exception e)
											{
												Engine.Logs.Log(e);

												Engine.ConnectedRealIp = Messages.NotAvailable;
												Engine.ConnectedServerTime = 0;
											}
										}
									}
								}
								else
								{
									Engine.ConnectedRealIp = "";
									Engine.ConnectedServerTime = 0;
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
											string hash = Utils.GetRandomToken();
											string randomIp = RandomGenerator.GetInt(1, 255) + "." + RandomGenerator.GetInt(1, 255) + "." + RandomGenerator.GetInt(1, 255) + "." + RandomGenerator.GetInt(1, 255);

											// Query a inexistent domain with the hash
											string dnsQuery = service.GetKeyValue("check_dns_query", "");
											string dnsHost = dnsQuery.Replace("{hash}", hash);
											IpAddresses result = Platform.Instance.ResolveDNS(dnsHost);

											// Check if the server has received the above DNS query

											// 2.11
											// string checkUrl = "https://" + Engine.CurrentServer.IpExit + ":" + service.GetKeyValue("check_port", "89") + "/check_dns/";
											// string checkDomain = Engine.CurrentServer.ProviderName.ToLowerInvariant() + "_exit." + service.GetKeyValue("check_domain", "");
											// XmlDocument xmlDoc = Engine.XmlFromUrl(checkUrl, checkDomain, null, Messages.ConnectionCheckingDNS, true);

											// 2.12
											string checkDomain = Engine.CurrentServer.ProviderName.ToLowerInvariant() + "_exit." + service.GetKeyValue("check_domain", "");
											string checkUrl = "https://" + checkDomain + "/check_dns/";
											XmlDocument xmlDoc = Engine.FetchUrlXml(checkUrl, null, Messages.ConnectionCheckingRoute, true, checkDomain + ":" + Engine.CurrentServer.IpsExit.ToStringFirstIPv4());

											string answer = xmlDoc.DocumentElement.Attributes["hash"].Value;

											if (hash != answer)
												throw new Exception(MessagesFormatter.Format(Messages.ConnectionCheckingTryDNSFail, answer));

											ok = true;
											break;
										}
										catch(Exception e)
										{
											Engine.Logs.Log(LogType.Verbose, e);
										}
									}

									if( (m_reset == "") && (ok == false) )
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

							if (Engine.Instance.Storage.GetBool("advanced.testonly"))
								Engine.RequestStop();
						}
					}

					// Windows
					if (Platform.Instance.IsUnixSystem() == false)
					{
						// Old 2.11.9
						/*
						List<string> match = Utils.RegExMatchSingle(message, "TAP-.*? device \\[(.*?)\\] opened: \\\\\\\\\\.\\\\Global\\\\(.*?).tap");
						if (match != null)
						{
							Engine.ConnectedVpnInterfaceName = match[0];
							Engine.ConnectedVpnInterfaceId = match[1];

							m_interfaceScope = new InterfaceScope(Engine.ConnectedVpnInterfaceId);

							NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
							foreach (NetworkInterface adapter in interfaces)
							{
								if (adapter.Id == Engine.ConnectedVpnInterfaceId)
								{
									m_interfaceTun = adapter;
									InterfaceTunBytesReadInitial = -1;
									InterfaceTunBytesWriteInitial = -1;
									InterfaceTunBytesLastRead = -1;
									InterfaceTunBytesLastWrite = -1;
								}
							}
						}
						*/

						// Match interface - 2.11.10
						List<string> matchInterface = Utils.RegExMatchSingle(message, "TAP-.*\\\\(.+?).tap");
						if(matchInterface != null)
						{
							Engine.ConnectedVpnInterfaceId = matchInterface[0];

							m_interfaceScope = new InterfaceScope(Engine.ConnectedVpnInterfaceId);

							NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
							foreach (NetworkInterface adapter in interfaces)
							{
								if (adapter.Id == Engine.ConnectedVpnInterfaceId)
								{
									m_interfaceTun = adapter;
									m_interfaceTunBytesReadInitial = -1;
									m_interfaceTunBytesWriteInitial = -1;
									m_interfaceTunBytesLastRead = -1;
									m_interfaceTunBytesLastWrite = -1;
								}
							}
						}

						// Match name - 2.11.10
						// Note: Windows allow [] chars in interface name, but OpenVPN use ] to close the name and don't escape it, so "\\sopened" it's required for lazy regex.
						List<string> matchName = Utils.RegExMatchSingle(message, "TAP-.*\\sdevice\\s\\[(.*?)\\]\\sopened");
						if(matchName != null)
						{
							Engine.ConnectedVpnInterfaceName = matchName[0];
						}
					}

					// Unix
					if (Platform.Instance.IsUnixSystem())
					{
						Match match = Regex.Match(message, "TUN/TAP device (.*?) opened");
						if (match.Success)
						{
							Engine.ConnectedVpnInterfaceName = match.Groups[1].Value;
							Engine.ConnectedVpnInterfaceId = match.Groups[1].Value;

							NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
							foreach (NetworkInterface adapter in interfaces)
							{
								if (adapter.Id == Engine.ConnectedVpnInterfaceId)
								{
									m_interfaceTun = adapter;
									m_interfaceTunBytesReadInitial = -1;
									m_interfaceTunBytesWriteInitial = -1;
									m_interfaceTunBytesLastRead = -1;
									m_interfaceTunBytesLastWrite = -1;
								}
							}
						}
					}

					// OSX
					{
						Match match = Regex.Match(message, "Opened utun device (.*?)$");
						if (match.Success)
						{
							Engine.ConnectedVpnInterfaceName = match.Groups[1].Value;
							Engine.ConnectedVpnInterfaceId = match.Groups[1].Value;

							m_interfaceTun = null; // Not used under OSX, see Platforms.Osx.GetTunStatsMode comment
							m_interfaceTunBytesLastRead = -1;
							m_interfaceTunBytesLastWrite = -1;
						}
					}

					{
						string pushDirectivesLine = Utils.RegExMatchOne(message, "PUSH: Received control message: '(.*?)'");
						if (pushDirectivesLine != "")
						{
							List<string> pushDirectives = Utils.StringToList(pushDirectivesLine, ",");
							foreach (string directive in pushDirectives)
							{
								if (directive == "PUSH_REPLY")
									continue;
								m_ovpnWithPush.AppendDirectives(directive, "Push");								
							}
						}
					}
					
					if (message.StartsWith("Warning:") && logType < LogType.Warning)
						logType = LogType.Warning;

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

		public void ProcessOutputManagement(string source, string message)
		{
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
			if (m_interfaceTunBytesLastRead != -1) {
				deltaRead = read - m_interfaceTunBytesLastRead;
				if (deltaRead < 0)
					deltaRead = 0; // Unexpected, ignore if it happens
			}
			Engine.ConnectedSessionStatsRead += deltaRead;
			Engine.ConnectedVpnStatsRead += deltaRead;

			Int64 deltaWrite = write;
			if (m_interfaceTunBytesLastWrite != -1) {
				deltaWrite = write - m_interfaceTunBytesLastWrite;
				if (deltaWrite < 0)
					deltaWrite = 0; // Unexpected, ignore if it happens
			}
			Engine.ConnectedSessionStatsWrite += deltaWrite;
			Engine.ConnectedVpnStatsWrite += deltaWrite;

			if (m_interfaceTunBytesLastRead != -1)
			{
				int delta = m_interfaceTunBytesLastTick.Reset();
				if (delta > 0)
				{
					Engine.ConnectedLastDownloadStep = (1000 * (deltaRead)) / delta;
					Engine.ConnectedLastUploadStep = (1000 * (deltaWrite)) / delta;
				}
			}

			m_interfaceTunBytesLastRead = read;
			m_interfaceTunBytesLastWrite = write;

			Engine.Instance.Stats.Charts.Hit(Engine.ConnectedLastDownloadStep, Engine.ConnectedLastUploadStep);

			Engine.OnRefreshUi(Core.Engine.RefreshUiMode.Stats);
		}
	}
}