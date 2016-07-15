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
		private int m_proxyPort = 0;
		private NetworkInterface m_interfaceTun;
		private int m_timeLastStatus = 0;
		private TemporaryFile m_fileSshKey;
		private TemporaryFile m_fileSslCrt;
		private TemporaryFile m_fileSslConfig;
		private TemporaryFile m_fileOvpn;
        private TemporaryFile m_fileProxyAuth;
        private TemporaryFile m_filePasswordAuth;
        private ProgramScope m_programScope = null;
        private InterfaceScope m_interfaceScope = null;

        public override void OnRun()
		{
			CancelRequested = false;

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

							string nextWaitingMessage = Messages.WaitingLatencyTestsTitle + " " + Messages.Format(Messages.WaitingLatencyTestsStep, i.ToString());
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

					if (Engine.Storage.Get("server") != "")
					{
						Engine.CurrentServer = Engine.PickServer(Engine.Storage.Get("server"), true);
					}
					else
					{
						if (Engine.CurrentServer == null)
							if (Engine.Storage.GetBool("servers.locklast"))
								Engine.CurrentServer = Engine.PickServer(sessionLastServer, false);

						if (Engine.CurrentServer == null)
							Engine.CurrentServer = Engine.PickServer();
					}

					if (Engine.CurrentServer == null)
					{
						allowed = false;
						Engine.Logs.Log(LogType.Fatal, "No server available.");
						RequestStop();
					}

                    string protocol = Engine.Storage.Get("mode.protocol").ToUpperInvariant();
                    int port = Engine.Storage.GetInt("mode.port");
                    int alt = Engine.Storage.GetInt("mode.alt");

                    if (Engine.CurrentServer != null)
                    {
                        if (protocol == "AUTO")
                        {
                            protocol = Engine.CurrentServer.Provider.GetKeyValue("mode_protocol", "UDP");
                            string proxyMode = Engine.Storage.GetLower("proxy.mode");
                            if (proxyMode != "none")
                                protocol = "TCP";
                            port = Conversions.ToInt32(Engine.CurrentServer.Provider.GetKeyValue("mode_port", "443"));
                            alt = Conversions.ToInt32(Engine.CurrentServer.Provider.GetKeyValue("mode_alt", "0"));
                        }
                    }

                    if (protocol == "SSH")
                    {
                        m_proxyPort = Engine.Storage.GetInt("ssh.port");
                        if (m_proxyPort == 0)
                            m_proxyPort = RandomGenerator.GetInt(1024, 64 * 1024);
                    }
                    else if (protocol == "SSL")
                    {
                        m_proxyPort = Engine.Storage.GetInt("ssl.port");
                        if (m_proxyPort == 0)
                            m_proxyPort = RandomGenerator.GetInt(1024, 64 * 1024);
                    }
                    else
                    {
                        m_proxyPort = 0;
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
                                parameters["server"] = Engine.CurrentServer.ProviderNameX;
                                parameters["protocol"] = protocol;
                                parameters["port"] = port.ToString();
                                parameters["alt"] = alt.ToString();

                                XmlDocument xmlDoc = null;
                                try
                                {
                                    xmlDoc = service.Fetch(Messages.AuthorizeConnect, parameters);
                                }
                                catch (Exception e)
                                {
                                    // Note: If failed, continue anyway.
                                    Engine.Logs.Log(LogType.Warning, Messages.Format(Messages.AuthorizeConnectFailed, e.Message));
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

					if (allowed)
					{
						sessionLastServer = Engine.CurrentServer.Code;
						Engine.Storage.Set("servers.last", Engine.CurrentServer.Code);
                        
						routeScope = new RouteScope(Engine.ConnectedEntryIP);

						Engine.RunEventCommand("vpn.pre");

						string connectingMessage = Messages.Format(Messages.ConnectionConnecting, Engine.CurrentServer.GetNameWithLocation());
						Engine.WaitMessageSet(connectingMessage, true);
						Engine.Logs.Log(LogType.InfoImportant, connectingMessage);

						BuildOVPN(protocol, port, alt, m_proxyPort);

						if (protocol == "SSH")
						{
							StartSshProcess();
						}
						else if (protocol == "SSL")
						{
							StartSslProcess();
						}
						else if ((protocol == "TCP") || (protocol == "UDP"))
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
											Int64 read = m_interfaceTun.GetIPv4Statistics().BytesReceived;
											Int64 write = m_interfaceTun.GetIPv4Statistics().BytesSent;

											if (Engine.ConnectedLastRead != -1)
											{
												int delta = Engine.ConnectedLastStatsTick.Reset();
												if (delta > 0)
												{
													Engine.ConnectedLastDownloadStep = (1000 * (read - Engine.ConnectedLastRead)) / delta;
													Engine.ConnectedLastUploadStep = (1000 * (write - Engine.ConnectedLastWrite)) / delta;
												}
											}

											Engine.ConnectedLastRead = read;
											Engine.ConnectedLastWrite = write;

											Engine.Instance.Stats.Charts.Hit(Engine.ConnectedLastDownloadStep, Engine.ConnectedLastUploadStep);

											Engine.OnRefreshUi(Core.Engine.RefreshUiMode.Stats);
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
									StopRequest = true;
								}

								if (Engine.SwitchServer != false)
								{
									Engine.SwitchServer = false;
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
                        if (m_fileProxyAuth != null)
                        {
                            m_fileProxyAuth.Close();
                            m_fileProxyAuth = null;
                        }
                        if (m_filePasswordAuth != null)
                        {
                            m_filePasswordAuth.Close();
                            m_filePasswordAuth = null;
                        }
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

                if (m_reset == "AUTH_FAILED")
				{
					waitingMessage = "Auth failed, retry in {1} sec.";
					waitingSecs = 10;
				}
				else if (m_reset == "ERROR")
				{
					waitingMessage = "Restart in {1} sec.";
					waitingSecs = 3;
				}
                else if (m_reset == "FATAL") 
                {
                    if (Engine.Instance.Storage.GetBool("advanced.testmode"))
                    {
                        Engine.Instance.RequestStop();
                    }
                    else
                    {
                        Engine.Instance.Disconnect();
                    }
                    break;
                }
								
				if (waitingSecs > 0)
				{
					for (int i = 0; i < waitingSecs; i++)
					{
						Engine.WaitMessageSet(Messages.Format(waitingMessage, (waitingSecs - i).ToString()), true);
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
			File.WriteAllText(m_fileSshKey.Path, Utils.XmlGetAttributeString(service.User, "ssh_" + fileKeyExtension, ""));
			
			if (Platform.Instance.IsUnixSystem())
			{
				// TOCHECK: under OSX with chmod 700 fail, need investigation.
				if (Platform.Instance.GetCode() != "OSX") 
				{
					Platform.Instance.ShellCmd("chmod 700 \"" + m_fileSshKey.Path + "\"");
				}
			}
			
			string arguments = "";

			arguments += " -i \"" + m_fileSshKey.Path + "\" -L " + Conversions.ToString(m_proxyPort) + ":127.0.0.1:2018 sshtunnel@" + Engine.ConnectedEntryIP;
			if (Platform.Instance.IsUnixSystem())
				arguments += " -p " + Engine.ConnectedPort; // ssh use -p
			else
				arguments += " -P " + Engine.ConnectedPort; // plink use -P			
				
			if (Platform.Instance.IsUnixSystem())
				arguments += " -o UserKnownHostsFile=/dev/null -o StrictHostKeyChecking=no"; // TOOPTIMIZE: To bypass key confirmation. Not the best approach.
			arguments += " -N -T -v";

            m_programScope = new ProgramScope(Software.SshPath, "SSH Tunnel");

            m_processProxy = new Process();
			m_processProxy.StartInfo.FileName = Software.SshPath;
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
			File.WriteAllText(m_fileSslCrt.Path, Utils.XmlGetAttributeString(service.User, "ssl_crt", ""));

			m_fileSslConfig = new TemporaryFile("ssl");

			string sslConfig = "";

			if (Platform.Instance.IsUnixSystem())
			{
				//sslConfig += "output = /dev/stdout\n"; // With this, with new stunnel 5.01, we have duplicated output dump.
				//sslConfig += "pid = /tmp/stunnel4.pid\n";
				sslConfig += "foreground = yes\n"; // Without this, the process fork and it's exit can't be detected.
				sslConfig += "pid = /tmp/" + RandomGenerator.GetHash() + ".pid\n"; // 2.2
			}
			sslConfig += "options = NO_SSLv2\n";
			sslConfig += "client = yes\n";
			sslConfig += "debug = 6\n";
			sslConfig += "\n";
			sslConfig += "[openvpn]\n";
			sslConfig += "accept = 127.0.0.1:" + Conversions.ToString(m_proxyPort) + "\n";
			sslConfig += "connect = " + Engine.ConnectedEntryIP + ":" + Engine.ConnectedPort + "\n";
			sslConfig += "TIMEOUTclose = 0\n";			
			sslConfig += "verify = 3\n";
			//sslConfig += "CAfile = \"" + m_fileSslCrt.Path + "\"\n";
			sslConfig += "CAfile = " + m_fileSslCrt.Path + "\n";
			sslConfig += "\n";

			string sslConfigPath = m_fileSslConfig.Path;
			Utils.SaveFile(sslConfigPath, sslConfig);

            m_programScope = new ProgramScope(Software.SslPath, "SSL Tunnel");

            m_processProxy = new Process();
			m_processProxy.StartInfo.FileName = Software.SslPath;
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
			Utils.SaveFile(ovpnPath, Engine.ConnectedOVPN);

            if(m_processProxy == null)
                m_programScope = new ProgramScope(Software.OpenVpnPath, "OpenVPN Tunnel");

            m_processOpenVpn = new Process();
			m_processOpenVpn.StartInfo.FileName = Software.OpenVpnPath;
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
            m_reset = level;
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
			if (message.Trim() == "") // 2.10.1
				return;

			try
			{
				if (source == "OpenVPN")
				{
					bool log = true;
                    LogType logType = LogType.Verbose;

					if (message.IndexOf("MANAGEMENT: CMD 'status'") != -1)
						log = false;

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
                        string unrecognizedOption = Utils.RegExMatchOne(message, "Options error\\: Unrecognized option or missing parameter\\(s\\) in .*\\:(.*?)\\(.*\\)");
                        Engine.Logs.Log(LogType.Fatal, Messages.Format(Messages.DirectiveError, unrecognizedOption));
                        Engine.LogOpenvpnConfig();                        
                        SetReset("FATAL");
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
						Engine.Logs.Log(LogType.Warning, Messages.AuthFailed);

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
						if (Engine.Instance.Storage.Get("dns.servers") != "")
							Platform.Instance.OnDnsSwitchDo(Engine.Instance.Storage.Get("dns.servers"));
						else if(Engine.ConnectedVpnDns != "")
							Platform.Instance.OnDnsSwitchDo(Engine.ConnectedVpnDns);

						if (Engine.Instance.Storage.GetBool("routes.remove_default"))
							Platform.Instance.OnRouteDefaultRemoveDo();

						Engine.WaitMessageSet(Messages.ConnectionFlushDNS, true);
						Engine.Logs.Log(LogType.Info, Messages.ConnectionFlushDNS);

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
                                    Engine.Logs.Log(LogType.Info, Messages.ConnectionCheckingRoute);

                                    if (m_reset == "")
                                    {
                                        XmlDocument xmlDoc = Engine.XmlFromUrl("https://" + Engine.CurrentServer.ProviderNameX.ToLowerInvariant() + "_exit." + service.Manifest.Attributes["check_domain"].Value + "/check_tun/", null, Messages.ConnectionCheckingRoute, true);

                                        string VpnIp = xmlDoc.DocumentElement.Attributes["ip"].Value;
                                        Engine.ConnectedServerTime = Conversions.ToInt64(xmlDoc.DocumentElement.Attributes["time"].Value);
                                        Engine.ConnectedClientTime = Utils.UnixTimeStamp();

                                        if (VpnIp != Engine.ConnectedVpnIp)
                                        {
                                            Engine.Logs.Log(LogType.Error, Messages.ConnectionCheckingRouteFailed);
                                            SetReset("ERROR");
                                        }
                                    }

                                    if (m_reset == "")
                                    {
                                        XmlDocument xmlDoc = Engine.XmlFromUrl("https://" + Engine.CurrentServer.ProviderNameX.ToLowerInvariant() + "." + service.Manifest.Attributes["check_domain"].Value + "/check_tun/", null, Messages.ConnectionCheckingRoute2, true);
                                        Engine.ConnectedServerTime = Conversions.ToInt64(xmlDoc.DocumentElement.Attributes["time"].Value);
                                        Engine.ConnectedClientTime = Utils.UnixTimeStamp();

                                        // Real IP are detected with a request over the server entry IP.
                                        // Normally this is routed by openvpn outside the tunnel.
                                        // But if a proxy is active, don't work.
                                        if (Engine.Instance.Storage.Get("proxy.mode").ToLowerInvariant() != "none")
                                        {
                                            Engine.ConnectedRealIp = Messages.NotAvailable;
                                        }
                                        else
                                        {
                                            Engine.ConnectedRealIp = xmlDoc.DocumentElement.Attributes["ip"].Value;
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
                                    Engine.Logs.Log(LogType.Info, Messages.ConnectionCheckingDNS);

                                    string hash = Utils.GetRandomToken();

                                    try
                                    {
                                        // Query a inexistent domain with the hash
                                        IPHostEntry entry = Dns.GetHostEntry(hash + ".airvpn.check_dns");
                                    }
                                    catch (SocketException)
                                    {
                                    }

                                    // Check if the server has received the above DNS query
                                    XmlDocument xmlDoc = Engine.XmlFromUrl("https://" + Engine.CurrentServer.ProviderNameX.ToLowerInvariant() + "_exit." + service.Manifest.Attributes["check_domain"].Value + "/check_dns/", null, Messages.ConnectionCheckingDNS, true);

                                    string hash2 = xmlDoc.DocumentElement.Attributes["hash"].Value;

                                    bool failed = (hash != hash2);

                                    if (failed)
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
						}
					}

					// Windows
					if (Platform.Instance.IsUnixSystem() == false)
					{
						List<string> match = Utils.RegExMatchSingle(message, "TAP-.*? device \\[(.*?)\\] opened: \\\\\\\\\\.\\\\Global\\\\(.*?).tap");
						if(match != null)
						{
							Engine.ConnectedVpnInterfaceName = match[0];
							Engine.ConnectedVpnInterfaceId = match[1];

                            m_interfaceScope = new InterfaceScope(Engine.ConnectedVpnInterfaceId);

							NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
							foreach (NetworkInterface adapter in interfaces)
							{
								if (adapter.Id == Engine.ConnectedVpnInterfaceId)
									m_interfaceTun = adapter;
							}
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
									m_interfaceTun = adapter;
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
						}
					}

					{
						List<List<string>> matches = Utils.RegExMatchMulti(message, "dhcp-option DNS ([0-9\\.]+?),");
						if (matches.Count > 0)
						{
							Engine.ConnectedVpnDns = Utils.ListStringToCommaString(matches);							
						}						
					}

					{
						Match match = Regex.Match(message, "ifconfig ([0-9\\.]+) ([0-9\\.]+)");
						if (match.Success)
						{
							Engine.ConnectedVpnIp = match.Groups[1].Value;
							Engine.ConnectedVpnGateway = match.Groups[2].Value;
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
												
						lock (Engine)
						{
							if (Engine.ConnectedLastRead != -1)
							{
								int delta = Engine.ConnectedLastStatsTick.Reset();
								if (delta > 0)
								{
									Engine.ConnectedLastDownloadStep = (1000 * (read - Engine.ConnectedLastRead)) / delta;
									Engine.ConnectedLastUploadStep = (1000 * (write - Engine.ConnectedLastWrite)) / delta;
								}
							}

							Engine.ConnectedLastRead = read;
							Engine.ConnectedLastWrite = write;

							Engine.Instance.Stats.Charts.Hit(Engine.ConnectedLastDownloadStep, Engine.ConnectedLastUploadStep);
						}

						Engine.OnRefreshUi(Core.Engine.RefreshUiMode.Stats);

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

        public void BuildOVPN(string protocol, int port, int alt, int proxyPort)
        {
            ServerInfo CurrentServer = Engine.Instance.CurrentServer;

            Storage s = Engine.Instance.Storage;

            string ip = CurrentServer.IpEntry;
            if (alt == 1)
                ip = CurrentServer.IpEntry2;

            OvpnBuilder ovpn = new OvpnBuilder();

            if (s.GetBool("openvpn.skip_defaults") == false)
            {
                ovpn.AppendDirectives(Engine.Instance.Storage.Get("openvpn.directives"), "Client level");
                CurrentServer.Provider.OnBuildOvpnDefaults(ovpn, protocol);

                ovpn.AppendDirectives(CurrentServer.OvpnDirectives, "Server level");
            }

            if (s.Get("openvpn.dev_node") != "")
                ovpn.AppendDirective("dev-node", s.Get("openvpn.dev_node"), "");

            int rcvbuf = s.GetInt("openvpn.rcvbuf");
            if ((rcvbuf == -2) && (Platform.IsWindows())) rcvbuf = (256 * 1024);
            if (rcvbuf == -2) rcvbuf = -1;
            if (rcvbuf != -1)
                ovpn.AppendDirective("rcvbuf", rcvbuf.ToString(), "");

            int sndbuf = s.GetInt("openvpn.sndbuf");
            if ((sndbuf == -2) && (Platform.IsWindows())) sndbuf = (256 * 1024);
            if (sndbuf == -2) sndbuf = -1;
            if (sndbuf != -1)
                ovpn.AppendDirective("sndbuf", sndbuf.ToString(), "");

            if (protocol == "UDP")
            {
                ovpn.AppendDirective("proto", "udp", "");
            }
            else // TCP, SSH, SSL, Tor
            {
                ovpn.AppendDirective("proto", "tcp", "");
            }

            if (protocol == "SSH")
                ovpn.AppendDirective("remote", "127.0.0.1 " + Conversions.ToString(proxyPort), "");
            else if (protocol == "SSL")
                ovpn.AppendDirective("remote", "127.0.0.1 " + Conversions.ToString(proxyPort), "");
            else
                ovpn.AppendDirective("remote", ip + " " + port.ToString(), "");

            string proxyDirectiveName = "";
            string proxyDirectiveArgs = "";

            string proxyMode = s.GetLower("proxy.mode");
            if (proxyMode == "tor")
            {
                proxyDirectiveName = "socks-proxy";
            }
            else if (proxyMode == "http")
            {
                proxyDirectiveName = "http-proxy";

            }
            else if (proxyMode == "socks")
            {
                proxyDirectiveName = "socks-proxy";
            }

            if (proxyDirectiveName != "")
            {
                proxyDirectiveArgs += s.Get("proxy.host") + " " + s.Get("proxy.port");

                if ((s.GetLower("proxy.mode") != "none") && (s.GetLower("proxy.mode") != "tor"))
                {
                    if (s.Get("proxy.auth") != "None")
                    {
                        m_fileProxyAuth = new TemporaryFile("ppw");
                        string fileNameAuthOvpn = m_fileProxyAuth.Path.Replace("\\", "\\\\"); // 2.6, Escaping for Windows
                        string fileNameData = s.Get("proxy.login") + "\n" + s.Get("proxy.password") + "\n";
                        Utils.SaveFile(m_fileProxyAuth.Path, fileNameData);
                        proxyDirectiveArgs += " \"" + fileNameAuthOvpn + "\" " + s.Get("proxy.auth").ToLowerInvariant(); // 2.6 Auth Fix
                    }
                }

                ovpn.AppendDirective(proxyDirectiveName, proxyDirectiveArgs, "");
            }

            string routesDefault = s.Get("routes.default");
            if (routesDefault == "out")
            {
                ovpn.AppendDirective("route-nopull", "For Routes Out", "");

                // For Checking
                ovpn.AppendDirective("route", CurrentServer.IpExit + " 255.255.255.255 vpn_gateway", "For Checking Route");


                // For DNS
                // < 2.9. route directive useless, and DNS are forced manually in every supported platform. // TOCLEAN
                /*
				ovpn += "dhcp-option DNS " + Constants.DnsVpn + "\n"; // Manually because route-nopull skip it
				ovpn += "route 10.4.0.1 255.255.255.255 vpn_gateway # AirDNS\n";
				ovpn += "route 10.5.0.1 255.255.255.255 vpn_gateway # AirDNS\n";
				ovpn += "route 10.6.0.1 255.255.255.255 vpn_gateway # AirDNS\n";
				ovpn += "route 10.7.0.1 255.255.255.255 vpn_gateway # AirDNS\n";
				ovpn += "route 10.8.0.1 255.255.255.255 vpn_gateway # AirDNS\n";
				ovpn += "route 10.9.0.1 255.255.255.255 vpn_gateway # AirDNS\n";
				ovpn += "route 10.30.0.1 255.255.255.255 vpn_gateway # AirDNS\n";
				ovpn += "route 10.50.0.1 255.255.255.255 vpn_gateway # AirDNS\n"; 
				*/

                // 2.9, Can be removed when resolv-conf method it's not binded anymore in up/down ovpn directive // TOFIX
                ovpn.AppendDirective("dhcp-option", "DNS " + Constants.DnsVpn, "");
            }
            string routes = s.Get("routes.custom");
            string[] routes2 = routes.Split(';');
            foreach (string route in routes2)
            {
                string[] routeEntries = route.Split(',');
                if (routeEntries.Length != 3)
                    continue;

                IpAddressRange ipCustomRoute = new IpAddressRange(routeEntries[0]);

                if (ipCustomRoute.Valid == false)
                    Engine.Instance.Logs.Log(LogType.Warning, Messages.Format(Messages.CustomRouteInvalid, ipCustomRoute.ToString()));
                else
                {
                    string action = routeEntries[1];
                    string notes = routeEntries[2];

                    if ((routesDefault == "out") && (action == "in"))
                        ovpn.AppendDirective("route", ipCustomRoute.ToOpenVPN() + " vpn_gateway", Utils.SafeString(notes));
                    if ((routesDefault == "in") && (action == "out"))
                        ovpn.AppendDirective("route", ipCustomRoute.ToOpenVPN() + " net_gateway", Utils.SafeString(notes));
                }
            }

            if (routesDefault == "in")
            {
                if ((protocol == "SSH") || (protocol == "SSL"))
                {
                    ovpn.AppendDirective("route", ip + " 255.255.255.255 net_gateway", "VPN Entry IP");
                }

                if (proxyMode == "tor")
                {
                    List<string> torNodeIps = TorControl.GetGuardIps();
                    foreach (string torNodeIp in torNodeIps)
                    {
                        ovpn.AppendDirective("route", torNodeIp + " 255.255.255.255 net_gateway", "Tor Circuit");
                    }
                }
            }

            ovpn.AppendDirective("management", "localhost " + Engine.Instance.Storage.Get("openvpn.management_port"), "");

            ovpn.AppendDirectives(Engine.Instance.Storage.Get("openvpn.custom"), "Custom level");

            if (ovpn.GetDirective("proto").Text.StartsWith("udp") == false)
                ovpn.RemoveDirective("explicit-exit-notify");

            // Experimental - Allow identification as Public Network in Windows. Advanced Option?
            // ovpn.Append("route-metric 512");
            // ovpn.Append("route 0.0.0.0 0.0.0.0");
                        
            // Used by OpenVPN provider
            if (ovpn.ExistsDirective("auth-user-pass"))
            {
                m_filePasswordAuth = new TemporaryFile("ppw");
                string fileNameAuthOvpn = m_filePasswordAuth.Path.Replace("\\", "\\\\");
                string login = CurrentServer.Provider.GetLogin();
                string password = CurrentServer.Provider.GetPassword();
                string fileNameData = login + "\n" + password + "\n";

                Utils.SaveFile(m_filePasswordAuth.Path, fileNameData);
                proxyDirectiveArgs += " \"" + fileNameAuthOvpn + "\" " + s.Get("proxy.auth").ToLowerInvariant(); 

                ovpn.AppendDirective("auth-user-pass", "\"" + fileNameAuthOvpn + "\"", "Auth");
            }

            CurrentServer.Provider.OnBuildOvpnAuth(ovpn);

            Platform.Instance.OnBuildOvpn(ovpn);

            string ovpnText = ovpn.Get();

            CurrentServer.Provider.OnBuildOvpnPost(ref ovpnText);



            Engine.Instance.ConnectedOVPN = ovpnText;
            Engine.Instance.ConnectedEntryIP = ip;
            Engine.Instance.ConnectedPort = port;
            Engine.Instance.ConnectedProtocol = protocol;
        }

    }
}