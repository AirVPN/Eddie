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
using System.Globalization;
using System.IO;
using System.Threading;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Eddie.Core
{
	public class Engine : Eddie.Core.Thread
    {
		public static Engine Instance;

		public bool DevelopmentEnvironment = false;
        
		public bool ConsoleMode = false;

		public bool Terminated = false;
		public delegate void TerminateHandler();
		public event TerminateHandler TerminateEvent;

        private Threads.Pinger m_threadPinger;
        private Threads.Penalities m_threadPenalities;
        private Threads.Discover m_threadDiscover;
        private Threads.Manifest m_threadManifest;
        private Threads.Session m_threadSession;
        private Storage m_storage;
		private Stats m_stats;
        private ProvidersManager m_providersManager;
        private LogsManager m_logsManager;
        private NetworkLockManager m_networkLockManager;
        private WebServer m_webServer;
        
        public Providers.Service AirVPN; // TOFIX, for compatibility

        private Dictionary<string, ServerInfo> m_servers = new Dictionary<string, ServerInfo>();
        private Dictionary<string, AreaInfo> m_areas = new Dictionary<string, AreaInfo>();
        private bool m_serversInfoUpdated = false;
        private bool m_areasInfoUpdated = false;
		private List<string> FrontMessages = new List<string>();
		private int m_breakRequests = 0;

		public enum ActionService
        {
            None = 0,
            Connect = 1,
            Disconnect = 2,
			Login = 3,
			Logout = 4,
			NetLockIn = 5,
			NetLockOut = 6
        }

        public enum RefreshUiMode
        {
            None = 0,
            Stats = 1,
            Full = 2,
			MainMessage = 3,
			Log = 4,
			QuickX = 5
        }

		private List<ActionService> ActionsList = new List<ActionService>();        
		public string m_waitMessage = Messages.AppStarting;
        public bool m_waitCancel = false;

        private bool Connected = false;
        public ServerInfo CurrentServer;
        public ServerInfo NextServer;
		public bool SwitchServer;

		public TimeDelta TickDeltaUiRefreshQuick = new TimeDelta();
		public TimeDelta TickDeltaUiRefreshFull = new TimeDelta();

        public DateTime ConnectedSince = DateTime.MinValue;
		public string ConnectedEntryIP = "";
        public int ConnectedPort = 0;
		public string ConnectedProtocol = "";
		public string ConnectedOVPN = "";
        public Int64 ConnectedServerTime = 0;
		public Int64 ConnectedClientTime = 0;
		public string ConnectedRealIp = "";
		public string ConnectedVpnIp = "";
		public string ConnectedVpnGateway = "";
		public string ConnectedVpnInterfaceName = "";
		public string ConnectedVpnInterfaceId = "";
		public string ConnectedVpnDns = "";
        public TimeDelta ConnectedLastStatsTick = new TimeDelta();
        public Int64 ConnectedLastRead = -1;
        public Int64 ConnectedLastWrite = -1;
        public Int64 ConnectedLastDownloadStep = -1;
        public Int64 ConnectedLastUploadStep = -1;
        
        public Engine() : base(false)
        {
			Instance = this;
        }
                
        public Storage Storage
        {
            get
            {
                return m_storage;
            }
        }

		public Stats Stats
		{
			get
			{
				return m_stats;
			}
		}

        public ProvidersManager ProvidersManager
        {
            get
            {
                return m_providersManager;
            }
        }

        public LogsManager Logs
        {
            get
            {
                return m_logsManager;
            }
        }

        public NetworkLockManager NetworkLockManager
		{
			get
			{
				return m_networkLockManager;
			}
		}

        public WebServer WebServer
        {
            get
            {
                return m_webServer;
            }
        }

        public Dictionary<string, ServerInfo> Servers
        {
            get
            {
                return m_servers;
            }
        }

        public Dictionary<string, AreaInfo> Areas
        {
            get
            {
                return m_areas;
            }
        }

		public bool Initialization(bool cli)
		{
            if (ResourcesFiles.Count() == 0)			
			{
                ResourcesFiles.SetString("AirVPN.xml", Lib.Core.Properties.Resources.AirVPN); // TOCLEAN with Eddie3                				
                ResourcesFiles.SetString("license.txt", Lib.Core.Properties.Resources.License);
				ResourcesFiles.SetString("thirdparty.txt", Lib.Core.Properties.Resources.ThirdParty);
				ResourcesFiles.SetString("tos.txt", Lib.Core.Properties.Resources.TOS); // TOCLEAN
            }
            
            m_logsManager = new LogsManager();
            
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);

			DevelopmentEnvironment = File.Exists(Platform.Instance.NormalizePath(Platform.Instance.GetProgramFolder() + "/dev.txt"));

            bool manMode = (CommandLine.SystemEnvironment.Exists("help"));
			if (manMode == false)
			{
                Logs.Log(LogType.Info, "Eddie client version: " + Constants.VersionDesc + " / " + Platform.Instance.GetArchitecture() + ", System: " + Platform.Instance.GetCode() + ", Name: " + Platform.Instance.GetName() + " / " + Platform.Instance.GetOsArchitecture());

				if (DevelopmentEnvironment)
                    Logs.Log(LogType.Info, "Development environment.");
			}
            
            m_storage = new Core.Storage();

            if (cli)
            {
                string login = Storage.Get("login").Trim();
                string password = Storage.Get("password").Trim();

                if (Storage.GetBool("help"))
                {
                    Engine.Instance.Logs.Log(LogType.Info, Storage.GetMan(Storage.Get("help_format")));
                    return false;
                }
                else if ((login == "") || (password == ""))
                {
                    Engine.Instance.Logs.Log(LogType.Fatal, Messages.ConsoleHelp);
                    return false;
                }
            }

            // This is before the Storage.Load, because non-root can't read option (chmod)
            if (Storage.GetBool("advanced.skip_privileges") == false)
			{
				if (Platform.Instance.IsAdmin() == false)
				{
					if (OnNoRoot() == false)
                        Logs.Log(LogType.Fatal, Messages.AdminRequiredStop);

					return false;
				}
			}

            CountriesManager.Init();

            // Providers
            m_providersManager = new ProvidersManager();
            m_providersManager.Init();

            m_storage.Load();

            m_providersManager.Load();

            if (Storage.GetBool("cli"))
				ConsoleMode = true;

			if(Storage.Get("paramtest") != "") // Look comment in storage.cs
				Logs.Log(LogType.Warning, "Param test:-" + Storage.Get("paramtest") + "-");			

            if(Storage.GetBool("os.single_instance") == true)
            {
                if(Platform.Instance.OnCheckSingleInstance() == false)
                {
                    Logs.Log(LogType.Fatal, Messages.OsInstanceAlreadyRunning);
                    return false;
                }
            }

            m_stats = new Core.Stats();

            if( (WebServer.GetPath() != "") && (Storage.GetBool("webui.enabled") == true) )
            {
                m_webServer = new WebServer();
                m_webServer.Start();
            }

            m_networkLockManager = new NetworkLockManager();
			m_networkLockManager.Init();			

			CompatibilityManager.Init();

            Platform.Instance.OnInit();

            return true;
		}

        public override void OnRun()
        {
			bool initResult = OnInit();

			if(initResult == true)            
            {
				Platform.Instance.LogSystemInfo();

				Software.Checking();

				Software.Log();

                Platform.Instance.OnRecovery();

				Recovery.Load();

                RunEventCommand("app.start");

				Platform.Instance.OnAppStart();

				if (Engine.Storage.GetLower("netlock.mode") != "none")
				{
					if (Engine.Storage.GetBool("netlock")) // 2.8
						m_networkLockManager.Activation();					
				}

                WaitMessageClear();

                // Start threads
                m_threadPinger = new Threads.Pinger();
                m_threadPenalities = new Threads.Penalities();
                m_threadDiscover = new Threads.Discover();
                m_threadManifest = new Threads.Manifest();


                bool autoStart = false;
				if (ConsoleMode)
				{
					Auth();
					/* // 2.8
					if (IsLogged())
						autoStart = true;
					else
						CancelRequested = true;
					*/
				}
                
				// Clodo: Try to remove: if not logged, no servers, fatal error and exit.
				if ((IsLogged()) && (Engine.Storage.GetBool("connect")))
					autoStart = true;
				if (autoStart)
					Connect();
				else
                    Logs.Log(LogType.InfoImportant, Messages.Ready);
                
                for (; ; )
                {
                    OnWork();

					if (ConsoleMode)
					{
						if (Console.KeyAvailable)
						{
							ConsoleKeyInfo key = Console.ReadKey();
							OnConsoleKey(key);
						}
					}

                    if (CancelRequested)
                        break;
                }
            }

            Logs.Log(LogType.Verbose, Messages.AppShutdownStart);

            OnDeInit();

			RunEventCommand("app.stop");                

			OnDeInit2();

            Logs.Log(LogType.Verbose, Messages.AppShutdownComplete);

            Terminated = true;
			if (TerminateEvent != null)
				TerminateEvent();
        }

		public virtual bool OnInit()
        {
            lock (this)
            {
				foreach (string commandLineParamKey in CommandLine.SystemEnvironment.Params.Keys)
				{
					// 2.10.1
					// OS X sometime pass as command-line arguments a 'psn_0_16920610' or similar. Ignore it.
					if (commandLineParamKey.StartsWith("psn_"))
						continue;

					if (Storage.Exists(commandLineParamKey) == false)
					{
						Logs.Log(LogType.Error, Messages.Format(Messages.CommandLineUnknownOption, commandLineParamKey));						
					}
				}

                Logs.Log(LogType.Verbose, "Data Path: " + Storage.DataPath);
				Logs.Log(LogType.Verbose, "App Path: " + Platform.Instance.GetProgramFolder());
				Logs.Log(LogType.Verbose, "Executable Path: " + Platform.Instance.GetExecutablePath());
				Logs.Log(LogType.Verbose, "Command line arguments (" + CommandLine.SystemEnvironment.Params.Count.ToString() + "): " + CommandLine.SystemEnvironment.GetFull());

                if (Storage.Get("profile") != "AirVPN")
                    Logs.Log(LogType.Verbose, "Profile: " + Storage.Get("profile"));

                //return OnInit2(); // 2.11, now called on ConsoleStart and UiStart
                return true;
            }
        }

		public virtual bool OnInit2()
        {
            Start();

            PostManifestUpdate();

            LoggedUpdate();
            
            return true;
        }

		public virtual void OnDeInit()
        {
            SessionStop();

			WaitMessageSet(Messages.AppExiting, false);
			//Engine.Log(Engine.LogType.InfoImportant, Messages.AppExiting);

            if (m_threadManifest != null)
				m_threadManifest.RequestStopSync();				

			if (m_threadPenalities != null)
				m_threadPenalities.RequestStopSync();

            if (m_threadDiscover != null)
                m_threadDiscover.RequestStopSync();

            if (m_threadPinger != null)
				m_threadPinger.RequestStopSync();

			if (m_webServer != null)
                m_webServer.Stop();

			m_networkLockManager.Deactivation(true);
			m_networkLockManager = null;
			
			TemporaryFiles.Clean();

            Platform.Instance.OnDeInit();
        }

        public virtual void OnDeInit2()
        {
			SaveSettings();

			if (m_storage != null)
			{
				if (Storage.GetBool("remember") == false)
				{
					DeAuth();
				}
			}
        }

		public virtual bool OnNoRoot()
		{
			// Return true if managed differently
			return false;
		}

		public virtual void OnWork()
		{
			ActionService currentAction = ActionService.None;

			lock (ActionsList)
			{
				if (ActionsList.Count > 0)
				{
					currentAction = ActionsList[0];
					ActionsList.RemoveAt(0);
				}
			}

			if (currentAction == ActionService.None)
			{
				/*
				if (m_waitMessage != "")
					WaitMessageSet("", false);
				*/
			}
			else if (currentAction == ActionService.Login)
			{
				Auth();
			}
			else if (currentAction == ActionService.Logout)
			{
				DeAuth();
			}
			else if (currentAction == ActionService.NetLockIn)
			{
				m_networkLockManager.Activation();				
				WaitMessageClear();
			}
			else if (currentAction == ActionService.NetLockOut)
			{
				m_networkLockManager.Deactivation(false);
				WaitMessageClear();
			}
			else if (currentAction == ActionService.Connect)
			{
				if (m_threadSession == null)
					SessionStart();
			}
			else if (currentAction == ActionService.Disconnect)
			{
				if (m_threadSession != null)
					SessionStop();
			}

            /*
            if (TickDeltaUiRefreshQuick.Elapsed(1000))
				OnRefreshUi(RefreshUiMode.Quick);
            */

            // Avoid to refresh UI for every ping, for example
            if (TickDeltaUiRefreshFull.Elapsed(1000))
            {   
                if (m_serversInfoUpdated)
                {
                    m_serversInfoUpdated = false;
                    OnRefreshUi(RefreshUiMode.Full);
                }

                if(m_areasInfoUpdated)
                {
                    m_areasInfoUpdated = false;
                    RecomputeAreas();
                }
            }
            
			Sleep(100);
		}

		public virtual void OnConsoleKey(ConsoleKeyInfo key)
		{
			if (key.KeyChar == 'x')
			{
                Logs.Log(LogType.Info, Messages.ConsoleKeyCancel);
				OnExit();
			}
			else if (key.KeyChar == 'n')
			{
                Logs.Log(LogType.Info, Messages.ConsoleKeySwitch);
				SwitchServer = true;
				if ((Engine.IsLogged()) && (Engine.IsConnected() == false) && (Engine.IsWaiting() == false))
					Connect();
			}
		}

		public virtual void OnConsoleBreak()
		{
            Logs.Log(LogType.Info, Messages.ConsoleKeyBreak);

			OnExit();	
		}

		public virtual void OnCommand(CommandLine command)
		{
            if(m_webServer != null)
            {
                XmlElement xmlCommand = WebServer.CreateMessage();
                xmlCommand.SetAttribute("type", "command");
                command.WriteXML(xmlCommand);
                m_webServer.Send(xmlCommand);
            }   

			string action = command.Get("action","").ToLowerInvariant();
            if (action == "exit")
            {
                OnExit();
            }
            else if (action == "openvpn")
            {
                SendManagementCommand(command.Get("command", ""));
            }
            else if (action == "ui.show.license")
            {
                OnShowText("License", ResourcesFiles.GetString("license.txt"));
            }
            else if (action == "ui.show.libraries")
            {
                OnShowText("Libraries and Tools", ResourcesFiles.GetString("thirdparty.txt"));
            }
            else if (action == "ui.show.website")
            {
                Platform.Instance.OpenUrl(Constants.WebSite + "/" + "");
            }
            else if (action == "ui.show.ports")
            {
                Platform.Instance.OpenUrl(Constants.WebSite + "/" + "ports/");
            }
            else if (action == "ui.show.speedtest")
            {
                Platform.Instance.OpenUrl(Constants.WebSite + "/" + "speedtest/");
            }
            else if (action == "ui.show.clientarea")
            {
                Platform.Instance.OpenUrl(Constants.WebSite + "/" + "client/");
            }
            else if (action == "ui.show.docs.general")
            {
                Platform.Instance.OpenUrl(Constants.WebSite + "/" + "software/");
            }
            else if (action == "ui.show.docs.protocols")
            {
                Platform.Instance.OpenUrl(Constants.WebSite + "/" + "faq/software_protocols/");
            }
            else if (action == "ui.show.docs.udp_vs_tcp")
            {
                Platform.Instance.OpenUrl(Constants.WebSite + "/" + "faq/udp_vs_tcp/");
            }
            else if (action == "ui.show.docs.tor")
            {
                Platform.Instance.OpenUrl(Constants.WebSite + "/" + "tor/");
            }
            else if (action == "ui.show.docs.advanced")
            {
                Platform.Instance.OpenUrl(Constants.WebSite + "/" + "faq/software_advanced/");
            }
            else if (action == "ui.show.docs.directives")
            {
                Platform.Instance.OpenUrl(Constants.WebSite + "/" + "faq/software_directives/");
            }
            else if (action == "ui.show.docs.lock")
            {
                Platform.Instance.OpenUrl(Constants.WebSite + "/" + "faq/software_lock/");
            }
            else if (action == "ui.show.sources")
            {
                string url = "https://github.com/AirVPN/airvpn-client";
                Platform.Instance.OpenUrl(url);
            }
            else if (action == "ui.show.gpl")
            {
                Platform.Instance.OpenUrl("http://www.gnu.org/licenses/gpl.html");
            }
            else if (action == "ui.show.openvpn.management")
            {
                Platform.Instance.OpenUrl("http://openvpn.net/index.php/open-source/documentation/miscellaneous/79-management-interface.html");
            }
            else if (action == "providers.add.airvpn")
            {
                Engine.Instance.ProvidersManager.AddProvider("AirVPN", null);
            }
            else if (action == "providers.add.openvpn")
            {
                Engine.Instance.ProvidersManager.AddProvider("OpenVPN", null);
            }
            else if (action == "testlog")
            {
                Engine.Instance.Logs.Log(LogType.Warning, "Test Log " + Utils.GetRandomToken());
            }
            else
            {
                throw new Exception(Messages.CommandUnknown);
            }
		}

		public virtual void OnSettingsChanged()
		{
			SaveSettings(); // 2.8

			OnRefreshUi(RefreshUiMode.Full);

			if (m_networkLockManager != null)
				m_networkLockManager.OnUpdateIps();
		}

		public virtual void OnSessionStart()
		{
			RunEventCommand("session.start");

			Platform.Instance.OnSessionStart();

			Platform.Instance.OnIpV6Do();				
		}

		public virtual void OnSessionStop()
		{
			Platform.Instance.OnIpV6Restore();

			Platform.Instance.OnSessionStop();

			RunEventCommand("session.stop");
		}

		public virtual void OnExit()
		{
			RequestStop();
		}

		private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
		{
			m_breakRequests++;

			if (m_breakRequests == 1)
			{
				e.Cancel = true;
				
				OnConsoleBreak();
			}			
			
		}

		public bool ConsoleStart()
		{
			ConsoleMode = true;

			Engine.Instance.Logs.Log(LogType.Info, Messages.ConsoleKeyboardHelp);				

			if(Storage.GetBool("connect") == false)
				Engine.Instance.Logs.Log(LogType.Info, Messages.ConsoleKeyboardHelpNoConnect);

            return OnInit2();							
		}

		public bool UiStart()
		{
            return OnInit2();
		}

		public void ConsoleExit()
		{
			if (Storage.GetBool("cli"))
			{
				OnExit();
			}
		}

		public void Connect()
		{
			lock (ActionsList)
			{
				ActionsList.Add(Engine.ActionService.Connect);
			}
		}

		public void Login()
		{
			lock (ActionsList)
			{
				ActionsList.Add(Engine.ActionService.Login);
			}
		}

		public void Logout()
		{
			lock (ActionsList)
			{
				ActionsList.Add(Engine.ActionService.Logout);
			}
		}

		public void NetLockIn()
		{
			lock (ActionsList)
			{
				ActionsList.Add(Engine.ActionService.NetLockIn);
			}
		}

		public void NetLockOut()
		{
			lock (ActionsList)
			{
				ActionsList.Add(Engine.ActionService.NetLockOut);
			}
		}

		public void Disconnect()
		{
			lock (ActionsList)
			{
				ActionsList.Add(Engine.ActionService.Disconnect);
			}
		}

        public void MarkServersListUpdated()
        {
            // Called for minimal changes, like ping
            m_serversInfoUpdated = true;
        }

        public void MarkAreasListUpdated()
        {
            m_areasInfoUpdated = true;
        }

		public void SetConnected(bool connected)
        {
            lock (this)
            {
                if ((connected == true) && (Connected == false))
                {
                    // OnConnected
                    Connected = true;
                    ConnectedSince = DateTime.UtcNow;
					ConnectedLastRead = -1;
					ConnectedLastWrite = -1;
                    
                    WaitMessageClear();

                    OnRefreshUi(RefreshUiMode.Full);
                }

                if ((connected == false) && (Connected == true))
                {
                    // OnDisconnected
                    Connected = false;
                    ConnectedSince = DateTime.MinValue;
					ConnectedLastDownloadStep = 0;
					ConnectedLastUploadStep = 0;

					Engine.NetworkLockManager.OnVpnDisconnected();

                    OnRefreshUi(RefreshUiMode.Full);
                }
            }
        }

        public void WaitMessageClear()
        {
			WaitMessageSet("", false);
        }
		
		public void WaitMessageSet(string message, bool allowCancel)
        {			
			if (message != "")
			{
				if (Connected)
					throw new Exception("Unexpected status.");
			}

            lock (this)
            {
                m_waitMessage = message;
				m_waitCancel = allowCancel;
                
                OnRefreshUi(RefreshUiMode.MainMessage);
            }
        }

		public bool IsConnected()
        {
            return Connected;
        }

        public bool IsWaiting()
        {
            return (m_waitMessage != "");
        }

		public string WaitMessage
        {
            get
            {
                return m_waitMessage;
            }
        }

        public bool IsWaitingCancelAllowed()
        {
            return (m_waitCancel);
        }

		public bool IsWaitingCancelPending()
		{
			if(m_threadSession == null)
				return false;

			lock (m_threadSession)
			{
				return m_threadSession.CancelRequested;
			}
		}
		
        // ----------------------------------------------------
        // Logging
        // ----------------------------------------------------



        public virtual void OnLog(LogEntry l)
        {
			// An exception, to have a clean, standard 'man' output without logging header.
			if (CommandLine.SystemEnvironment.Exists("help"))
			{
				Console.WriteLine(l.Message);
				return;
			}

            if (m_webServer != null)
            {
                XmlElement xmlLog = WebServer.CreateMessage();
                xmlLog.SetAttribute("type", "log");
                l.WriteXML(xmlLog);
                m_webServer.Send(xmlLog);
            }

            if (l.Type != LogType.Realtime)
			{
				string lines = l.GetStringLines().Trim();
				Console.WriteLine(lines);

				// File logging

				if (Storage != null)
				{
					lock (Storage)
					{
						if (Storage.GetBool("log.file.enabled"))
						{
							try
							{
								string logPath = Storage.Get("log.file.path").Trim();

								List<string> paths = Logs.ParseLogFilePath(logPath);
								foreach (string path in paths)
								{
									Directory.CreateDirectory(Path.GetDirectoryName(path));
									File.AppendAllText(path, lines + "\n");
								}
							}
							catch(Exception e) 
							{
                                Logs.Log(LogType.Warning, Messages.Format("Log to file disabled due to error, {1}", e.Message));
								Storage.SetBool ("log.file.enabled", false);
							}
						}
					}
				}
			}
        }

		public virtual void OnFrontMessage(string message)
		{
            Logs.Log(LogType.Warning, message);
		}

        public virtual void OnMessageInfo(string message)
        {
        }

        public virtual void OnMessageError(string message)
        {
        }

        public virtual void OnShowText(string title, string data)
        {
            if (m_webServer != null)
            {
                XmlElement webMessage = WebServer.CreateMessage();
                webMessage.SetAttribute("type", "ui.show.text");
                webMessage.SetAttribute("title", title);
                webMessage.SetAttribute("text", data);
                m_webServer.Send(webMessage);
            }
        }

        public virtual bool OnAskYesNo(string message)
		{
			return true;
		}

        public virtual void OnLoggedUpdate(XmlElement xmlKeys)
        {

        }
        
        public virtual void OnPostManifestUpdate()
		{
            foreach (Provider provider in ProvidersManager.Providers)
            {
                if (provider.Enabled)
                {
                    string msg = provider.GetFrontMessage();
                    if ((msg != "") && (FrontMessages.Contains(msg) == false))
                    {
                        OnFrontMessage(msg);
                        FrontMessages.Add(msg);
                    }
                }
            }

            lock(m_servers)
            {
                foreach (ServerInfo infoServer in m_servers.Values)
                {
                    infoServer.Deleted = true;
                }

                foreach (Provider provider in ProvidersManager.Providers)
                {
                    if (provider.Enabled)
                    {
                        provider.OnBuildServersList();
                    }
                }

                for (;;)
                {
                    bool restart = false;
                    foreach (ServerInfo infoServer in m_servers.Values)
                    {
                        if (infoServer.Deleted)
                        {
                            m_servers.Remove(infoServer.Code);
                            restart = true;
                            break;
                        }
                    }

                    if (restart == false)
                        break;
                }

                // White/black list
				List<string> serversWhiteList = Storage.GetList("servers.whitelist");
				List<string> serversBlackList = Storage.GetList("servers.blacklist");
                foreach (ServerInfo infoServer in m_servers.Values)
                {
                    string code = infoServer.Code;

					if (serversWhiteList.Contains(code))
                        infoServer.UserList = ServerInfo.UserListType.WhiteList;
					else if (serversBlackList.Contains(code))
                        infoServer.UserList = ServerInfo.UserListType.BlackList;
                    else
                        infoServer.UserList = ServerInfo.UserListType.None;
                }

                RecomputeAreas();
            }
        }

        public virtual void OnRefreshUi()
        {
            OnRefreshUi(RefreshUiMode.Full);
        }

        public virtual void OnRefreshUi(RefreshUiMode mode)
        {
            if ((mode == Core.Engine.RefreshUiMode.Stats) || (mode == Core.Engine.RefreshUiMode.Full))
			{
				if (Engine.IsConnected())
				{
					{
						DateTime DT1 = Engine.Instance.ConnectedSince;
						DateTime DT2 = DateTime.UtcNow;
						TimeSpan TS = DT2 - DT1;
						string TSText = string.Format("{0:00}:{1:00}:{2:00} - {3}", (int)TS.TotalHours, TS.Minutes, TS.Seconds, DT1.ToLocalTime().ToShortDateString() + " " + DT1.ToLocalTime().ToShortTimeString());
						Stats.UpdateValue("VpnConnectionStart", TSText);

					}
					Stats.UpdateValue("VpnTotalDownload", Core.Utils.FormatBytes(Engine.ConnectedLastRead, false, true));
					Stats.UpdateValue("VpnTotalUpload", Core.Utils.FormatBytes(Engine.ConnectedLastWrite, false, true));

					Stats.UpdateValue("VpnSpeedDownload", Core.Utils.FormatBytes(Engine.ConnectedLastDownloadStep, true, true));
					Stats.UpdateValue("VpnSpeedUpload", Core.Utils.FormatBytes(Engine.ConnectedLastUploadStep, true, true));
				}
				else
				{
					Stats.UpdateValue("VpnConnectionStart", Messages.StatsNotConnected);
					Stats.UpdateValue("VpnTotalDownload", Messages.StatsNotConnected);
					Stats.UpdateValue("VpnTotalUpload", Messages.StatsNotConnected);

					Stats.UpdateValue("VpnSpeedDownload", Messages.StatsNotConnected);
					Stats.UpdateValue("VpnSpeedUpload", Messages.StatsNotConnected);
				}
			}

			if (mode == Core.Engine.RefreshUiMode.Full)
			{
                string manifestLastUpdate = Utils.FormatTime(ProvidersManager.LastRefreshDone);
                if (Core.Threads.Manifest.Instance != null)
                    if (Core.Threads.Manifest.Instance.ForceUpdate)
                        manifestLastUpdate += " (" + Messages.ManifestUpdateForce + ")";
                Stats.UpdateValue("ManifestLastUpdate", manifestLastUpdate);


                Stats.UpdateValue("SystemReport", Messages.DoubleClickToView);
				if (m_threadPinger != null)
				{
					Stats.UpdateValue("Pinger", PingerStats().ToString());					
				}

				if (Engine.IsConnected())
				{
					Stats.UpdateValue("ServerName", Engine.CurrentServer.DisplayName);
					Stats.UpdateValue("ServerLatency", Engine.CurrentServer.GetLatencyForList());
					Stats.UpdateValue("ServerLocation", Engine.CurrentServer.GetLocationForList());
					Stats.UpdateValue("ServerLoad", Engine.CurrentServer.GetLoadForList());
					Stats.UpdateValue("ServerUsers", Engine.CurrentServer.GetUsersForList());

                    Stats.UpdateValue("AccountLogin", Engine.Storage.Get("login"));
                    Stats.UpdateValue("AccountKey", Engine.Storage.Get("key"));

                    Stats.UpdateValue("VpnIpEntry", Engine.ConnectedEntryIP);
                    Stats.UpdateValue("VpnIpExit", Engine.CurrentServer.IpExit);                    
					Stats.UpdateValue("VpnProtocol", Engine.ConnectedProtocol);
					Stats.UpdateValue("VpnPort", Engine.ConnectedPort.ToString());
					if (Engine.ConnectedRealIp != "")
						Stats.UpdateValue("VpnRealIp", Engine.ConnectedRealIp);
					else
						Stats.UpdateValue("VpnRealIp", Messages.CheckingRequired);
					Stats.UpdateValue("VpnIp", Engine.ConnectedVpnIp);
					Stats.UpdateValue("VpnDns", Engine.ConnectedVpnDns);
					Stats.UpdateValue("VpnInterface", Engine.ConnectedVpnInterfaceName);
					Stats.UpdateValue("VpnGateway", Engine.ConnectedVpnGateway);
					Stats.UpdateValue("VpnGeneratedOVPN", Messages.DoubleClickToView);

					if (Engine.ConnectedServerTime != 0)
						Stats.UpdateValue("SystemTimeServerDifference", (Engine.ConnectedServerTime - Engine.ConnectedClientTime).ToString() + " seconds");
					else
						Stats.UpdateValue("SystemTimeServerDifference", Messages.CheckingRequired);
				}
				else
				{
					Stats.UpdateValue("ServerName", Messages.StatsNotConnected);
					Stats.UpdateValue("ServerLatency", Messages.StatsNotConnected);
					Stats.UpdateValue("ServerLocation", Messages.StatsNotConnected);
					Stats.UpdateValue("ServerLoad", Messages.StatsNotConnected);
					Stats.UpdateValue("ServerUsers", Messages.StatsNotConnected);

                    Stats.UpdateValue("AccountLogin", Messages.StatsNotConnected);
                    Stats.UpdateValue("AccountKey", Messages.StatsNotConnected);

                    Stats.UpdateValue("VpnIpEntry", Messages.StatsNotConnected);
					Stats.UpdateValue("VpnIpExit", Messages.StatsNotConnected);
					Stats.UpdateValue("VpnProtocol", Messages.StatsNotConnected);
					Stats.UpdateValue("VpnPort", Messages.StatsNotConnected);
					Stats.UpdateValue("VpnRealIp", Messages.StatsNotConnected);
					Stats.UpdateValue("VpnIp", Messages.StatsNotConnected);
					Stats.UpdateValue("VpnDns", Messages.StatsNotConnected);
					Stats.UpdateValue("VpnInterface", Messages.StatsNotConnected);
					Stats.UpdateValue("VpnGateway", Messages.StatsNotConnected);
					Stats.UpdateValue("VpnGeneratedOVPN", Messages.StatsNotConnected);
					Stats.UpdateValue("SystemTimeServerDifference", Messages.StatsNotConnected);
				}
			}
        }

		public virtual void OnStatsChange(StatsEntry entry)
		{
		}

		public virtual void OnUnhandledException(Exception e)
		{
			bool ignore = false;
			string stackTrace = e.StackTrace.ToString();

			if (stackTrace.IndexOf("System.Windows.Forms.XplatUIX11") != -1)
			{
				// Mono bug https://bugs.debian.org/cgi-bin/bugreport.cgi?bug=742774
				// Mono bug https://bugs.debian.org/cgi-bin/bugreport.cgi?bug=727651
				ignore = true;
			}

			if (stackTrace.IndexOf("System.Windows.Forms.ToolStripItem.OnParentChanged") != -1)
			{
				// Mono bug https://bugs.debian.org/cgi-bin/bugreport.cgi?bug=742774
				// Mono bug https://bugs.debian.org/cgi-bin/bugreport.cgi?bug=727651
				ignore = true;
			}

			if(ignore == false)
                Logs.Log(LogType.Fatal, Messages.UnhandledException + " - " + e.Message + " - " + e.StackTrace.ToString());
		}

        // ----------------------------------------------------
        // Misc
        // ----------------------------------------------------

        public ServerInfo GetServerInfo(string code, Provider provider)
        {
            ServerInfo infoServer = null;
            if (m_servers.ContainsKey(code))
                infoServer = m_servers[code];
            if (infoServer == null)
            {
                // Create
                infoServer = new ServerInfo();
                infoServer.Provider = provider;
                infoServer.Code = code;
                m_servers[code] = infoServer;
            }
            infoServer.Deleted = false;
            return infoServer;
        }

        // Return the list of selected servers, based on settings, filter, ordered by score.
        // Filter can be "earth","europe","nl","castor".
        public List<ServerInfo> GetServers(bool all)
        {
            List<ServerInfo> list = new List<ServerInfo>();

            lock (m_servers)
            {
                bool existsWhiteListAreas = false;
				bool existsWhiteListServers = false;
                foreach (ServerInfo server in m_servers.Values)
                {
                    if (server.UserList == ServerInfo.UserListType.WhiteList)
                    {
						existsWhiteListServers = true;
                        break;
                    }
                }                
                foreach (AreaInfo area in m_areas.Values)
                {
                    if (area.UserList == AreaInfo.UserListType.WhiteList)
                    {
						existsWhiteListAreas = true;
                        break;
                    }
                }
                
				foreach (ServerInfo server in m_servers.Values)
                {
                    bool skip = false;

                    if (all == false)
                    {
						ServerInfo.UserListType serverUserList = server.UserList;
						AreaInfo.UserListType countryUserList = AreaInfo.UserListType.None;

						/*
						if (m_areas.ContainsKey(server.CountryCode))
							countryUserList = m_areas[server.CountryCode].UserList;
						if (serverUserList == ServerInfo.UserListType.BlackList)
							skip = true;
						else if (countryUserList == AreaInfo.UserListType.BlackList)
							skip = true;
						else if ((serverUserList == ServerInfo.UserListType.None) && (countryUserList == AreaInfo.UserListType.None) && (existsWhiteList))
							skip = true;
						*/
                        if (m_areas.ContainsKey(server.CountryCode))
							countryUserList = m_areas[server.CountryCode].UserList;

						if (serverUserList == ServerInfo.UserListType.BlackList)
						{
							skip = true;
						}
						else if (serverUserList == ServerInfo.UserListType.WhiteList)
						{
							skip = false;
						}
						else
						{
							if (countryUserList == AreaInfo.UserListType.BlackList)
							{
								skip = true;
							}
							else if ((existsWhiteListServers) && (serverUserList == ServerInfo.UserListType.None))
							{
								skip = true;
							}
							else if ((existsWhiteListAreas) && (countryUserList == AreaInfo.UserListType.None))
							{
								skip = true;
							}
							else if (countryUserList == AreaInfo.UserListType.WhiteList)
							{
								skip = false;
							}
						}
                    }

                    if (skip == false)
                        list.Add(server);
                }                                
            }
            
            list.Sort();
            return list;            
        }

        public ServerInfo PickServerByName(string name)
        {
            lock (m_servers)
            {
                foreach (ServerInfo s in m_servers.Values)
                {
                    if (s.DisplayName == name)
                        return s;
                }

                Engine.Instance.Logs.Log(LogType.Fatal, Messages.Format(Messages.ServerByNameNotFound, name));
                return null;
            }
        }

        public ServerInfo PickServer()
		{   
            return PickServer("");
		}

		public ServerInfo PickServer(string preferred)
        {
            lock (m_servers)
            {
				if (preferred != "") 
				{
					if(m_servers.ContainsKey(preferred))
						return m_servers[preferred];					
				}

				List<ServerInfo> list = GetServers(false);
                if (list.Count > 0)
                    return list[0];                
            }            

            return null;
        }
        
        public string WaitManifestUpdate()
		{
			m_threadManifest.ForceUpdate = true;
			m_threadManifest.Updated.WaitOne();
			return m_threadManifest.GetLastResult();
		}		

        public void PostManifestUpdate()
        {
            OnPostManifestUpdate();

            OnRefreshUi(Core.Engine.RefreshUiMode.Full);

            if (m_networkLockManager != null)
				m_networkLockManager.OnUpdateIps();            
        }

        public void LoggedUpdate()
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement xmlKeys = xmlDoc.CreateElement("keys");

            if( (AirVPN != null) && (AirVPN.User != null) )
            {                
                xmlDoc.AppendChild(xmlKeys);

                foreach (XmlElement xmlKey in AirVPN.User.SelectNodes("keys/key"))
                {
                    XmlElement xmlKey2 = xmlDoc.CreateElement("key");
                    xmlKey2.SetAttribute("name", xmlKey.GetAttribute("name"));                    
                    xmlKeys.AppendChild(xmlKey2);
                }
            }

            OnLoggedUpdate(xmlKeys);
        }
        
        public void RunEventCommand(string name)
        {
			string filename = Storage.Get("event." + name + ".filename");
			string arguments = Storage.Get("event." + name + ".arguments");
			bool waitEnd = Storage.GetBool("event." + name + ".waitend");

            if (filename.Trim() != "")
            {
				string message = Messages.Format(Messages.AppEvent, name);
				WaitMessageSet(message, false);
				Logs.Log(LogType.Info, message);

                //Log(LogType.Verbose, "Start Running event '" + name + "', Command: '" + filename + "', Arguments: '" + arguments + "'");
				Platform.Instance.Shell(filename, arguments, waitEnd);
                //Log(LogType.Verbose, "End Running event '" + name + "', Command: '" + filename + "', Arguments: '" + arguments + "'");
            }
        }

		public void SendManagementCommand(string command)
		{
			if (m_threadSession != null)
				m_threadSession.SendManagementCommand(command);
		}

		public byte[] FetchUrlEx(string url, System.Collections.Specialized.NameValueCollection parameters, string title, int ntry, bool bypassProxy)
		{
			string lastException = "";
			for (int t = 0; t < ntry; t++)
			{
				try
				{
					// Note: by default WebClient try to determine the proxy used by IE/Windows
					WebClientEx wc = new WebClientEx();

					if (bypassProxy)
					{
						// Don't use a proxy if connected to the VPN
						wc.Proxy = null;
					}
					else
					{
						string proxyMode = Storage.Get("proxy.mode").ToLowerInvariant();
						string proxyHost = Storage.Get("proxy.host");
						int proxyPort = Storage.GetInt("proxy.port");
						string proxyAuth = Storage.Get("proxy.auth").ToLowerInvariant();
						string proxyLogin = Storage.Get("proxy.login");
						string proxyPassword = Storage.Get("proxy.password");

						if(proxyMode == "Tor")
						{
							proxyMode = "socks";
                            proxyAuth = "none"; 
							proxyLogin = "";
							proxyPassword = "";
						}

                        if (proxyMode == "http")
						{
							System.Net.WebProxy proxy = new System.Net.WebProxy(proxyHost, proxyPort);
							//string proxyUrl = "http://" + Storage.Get("proxy.host") + ":" + Storage.GetInt("proxy.port").ToString() + "/";
							//System.Net.WebProxy proxy = new System.Net.WebProxy(proxyUrl, true);					

							if (proxyAuth != "none")
							{
								//wc.Credentials = new System.Net.NetworkCredential(Storage.Get("proxy.login"), Storage.Get("proxy.password"), Storage.Get("proxy.host"));
								wc.Credentials = new System.Net.NetworkCredential(proxyLogin, proxyPassword, "");
								proxy.Credentials = new System.Net.NetworkCredential(proxyLogin, proxyPassword, "");
								wc.UseDefaultCredentials = false;
							}

							wc.Proxy = proxy;
						}
						else if (proxyMode == "socks")
						{
							// Socks Proxy supported with a curl shell
							if (Software.CurlPath == "")
							{
								throw new Exception(Messages.CUrlRequiredForProxySocks);
							}
							else
							{
								string dataParameters = "";
								if (parameters != null)
								{
									foreach (string k in parameters.Keys)
									{
										if (dataParameters != "")
											dataParameters += "&";
										dataParameters += k + "=" + Uri.EscapeUriString(parameters[k]);
									}
								}

								TemporaryFile fileOutput = new TemporaryFile("bin");
								string args = " \"" + url + "\" --socks4a " + proxyHost + ":" + proxyPort;
								if (proxyAuth != "none")
								{
									args += " -U " + proxyLogin + ":" + proxyPassword;
								}
								args += " -o \"" + fileOutput.Path + "\"";
								args += " --progress-bar";
								if (dataParameters != "")
									args += " --data \"" + dataParameters + "\"";
								string str = Platform.Instance.Shell(Software.CurlPath, args);
								byte[] bytes;
								if (File.Exists(fileOutput.Path))
								{
									bytes = File.ReadAllBytes(fileOutput.Path);
									fileOutput.Close();
									return bytes;
								}
								else
								{
									throw new Exception(str);
								}
							}
						}
						else if (proxyMode != "detect")
						{
							wc.Proxy = null;
						}
					}

					if (parameters == null)
						return wc.DownloadData(url);
					else
						return wc.UploadValues(url, "POST", parameters);
				}
				catch (Exception e)
				{
					if (ntry == 1) // AirAuth have it's catch errors retry logic.
						throw e;
					else
					{
						lastException = e.Message;

						if(Engine.Storage.GetBool("advanced.expert"))
							Engine.Instance.Logs.Log(LogType.Warning, Messages.Format(Messages.FetchTryFailed, title, (t + 1).ToString(), lastException));
					}
				}
			}

			throw new Exception(lastException);			
		}
		
		public XmlDocument XmlFromUrl(string url, System.Collections.Specialized.NameValueCollection parameters, string title, bool bypassProxy)
        {
            string str = System.Text.Encoding.ASCII.GetString(FetchUrlEx(url, parameters, title, 5, bypassProxy));                
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(str);
            return doc;
        }

		public int PingerInvalid()
		{
			return m_threadPinger.GetStats().Invalid;
		}

		public Threads.PingerStats PingerStats()
		{
			return m_threadPinger.GetStats();
		}

		public string GenerateFileHeader()
		{
			return Messages.Format(Messages.GeneratedFileHeader, Constants.VersionDesc);
		}
        
		public bool IsLogged()
		{
            if (AirVPN == null)
                return true; // TOCONTINUE

            if (AirVPN.User == null)
                return false;

            if (AirVPN.User.Attributes["login"] == null)
                return false;

            if (Storage.Get("login") == "")
                return false;

            //return ( (AirVPN.User != null) && (AirVPN.User.Attributes["login"] != null) );			
            return true;
		}

		public void Command(string cmd)
		{
			CommandLine command = new CommandLine(cmd, false, true);

			try
			{
				OnCommand(command);

				//Log(LogType.Verbose, "Command '" + command.GetFull() + "' executed");
			}
			catch (Exception e)
			{
                Logs.Log(LogType.Error, e);
			}
		}

		private void Auth()
		{
			Engine.Instance.WaitMessageSet(Messages.AuthorizeLogin, false);
            Logs.Log(LogType.Info, Messages.AuthorizeLogin);

			Dictionary<string, string> parameters = new Dictionary<string, string>();
			parameters["act"] = "user";

			try
			{
				XmlDocument xmlDoc = AirVPN.Fetch(Messages.AuthorizeLogin, parameters);
				string userMessage = Utils.XmlGetAttributeString(xmlDoc.DocumentElement, "message", "");

				if (userMessage != "")
				{
                    Logs.Log(LogType.Fatal, userMessage);
				}
				else
				{
                    Logs.Log(LogType.InfoImportant, Messages.AuthorizeLoginDone);
					
                    AirVPN.Auth(xmlDoc.DocumentElement);

                    LoggedUpdate();
                }
			}
			catch (Exception e)
			{
                Logs.Log(LogType.Fatal, Messages.Format(Messages.AuthorizeLoginFailed, e.Message));				
			}

			SaveSettings(); // 2.8

			Engine.Instance.WaitMessageClear();
		}

		private void DeAuth()
		{
			if (IsLogged())
			{
				Engine.Instance.WaitMessageSet(Messages.AuthorizeLogout, false);

                AirVPN.DeAuth();

                Logs.Log(LogType.InfoImportant, Messages.AuthorizeLogoutDone);

				Engine.Instance.WaitMessageClear();
			}

			SaveSettings(); // 2.8
		}

		public void ReAuth()
		{
			if (Engine.Instance.IsLogged())
			{
				DeAuth();
				Auth();
			}
		}
        
        private void SessionStart()
        {
            try
            {
				Logs.Log(LogType.Info, Messages.SessionStart);

				Engine.Instance.WaitMessageSet(Messages.CheckingEnvironment, true);
				
				if (CheckEnvironment() == false)
				{
					WaitMessageClear();
				}
				else
				{
					// Check Driver				
					if (Platform.Instance.GetDriverAvailable() == "")
					{
						if (Platform.Instance.CanInstallDriver())
						{
							Engine.Instance.WaitMessageSet(Messages.OsDriverInstall, false);
							Logs.Log(LogType.InfoImportant, Messages.OsDriverInstall);

							Platform.Instance.InstallDriver();

							if (Platform.Instance.GetDriverAvailable() == "")
								throw new Exception(Messages.OsDriverFailed);
						}
						else
							throw new Exception(Messages.OsDriverCannotInstall);
					}

					if (m_threadSession != null)
						throw new Exception("Daemon already running.");

					if (ProvidersManager.NeedUpdate(true))
					{
						Engine.Instance.WaitMessageSet(Messages.RetrievingManifest, true);
                        Logs.Log(LogType.Info, Messages.RetrievingManifest);

						string result = Engine.WaitManifestUpdate();

						if (result != "")
						{
							if (ProvidersManager.NeedUpdate(false))
							{
								throw new Exception(result);
							}
							else
							{
                                Logs.Log(LogType.Warning, Messages.ManifestFailedContinue);
							}
						}
					}

					OnSessionStart();

					if (NextServer == null)
					{
						if (Engine.Storage.GetBool("servers.startlast"))
							NextServer = Engine.PickServer(Engine.Storage.Get("servers.last"));
					}

					m_threadSession = new Threads.Session();
				}
            }
            catch (Exception e)
            {
                Logs.Log(LogType.Fatal, e);

                WaitMessageClear();

                if (Engine.Instance.Storage.GetBool("batch"))
                {
                    Engine.Instance.RequestStop();                    
                }
            }
        }

        private void SessionStop()
        {
			if (m_threadSession != null)
            {
                try
                {
					m_threadSession.RequestStopSync();
					m_threadSession = null;                    
                }
                catch (Exception e)
                {
                    Logs.Log(LogType.Fatal, e);
                }

				OnSessionStop();
				
                WaitMessageClear();

                Logs.Log(LogType.InfoImportant, Messages.SessionStop);                
            }
			
        }

        public void RecomputeAreas()
        {
            // Compute areas
            lock (m_areas)
            {
                foreach (AreaInfo infoArea in m_areas.Values)
                {
                    infoArea.Deleted = true;
                    infoArea.Servers = 0;
                    infoArea.Users = -1;
                    infoArea.Bandwidth = 0;
                    infoArea.BandwidthMax = 0;
                }

                List<string> areasWhiteList = Storage.GetList("areas.whitelist");
                List<string> areasBlackList = Storage.GetList("areas.blacklist");

                foreach (ServerInfo server in m_servers.Values)
                {
                    string countryCode = server.CountryCode;

                    AreaInfo infoArea = null;
                    if (m_areas.ContainsKey(countryCode))
                    {
                        infoArea = m_areas[countryCode];
                        infoArea.Deleted = false;
                    }

                    if (infoArea == null)
                    {
                        // Create
                        infoArea = new AreaInfo();
                        infoArea.Code = countryCode;
                        infoArea.Name = CountriesManager.GetNameFromCode(countryCode);
                        infoArea.Deleted = false;
                        m_areas[countryCode] = infoArea;
                    }

                    if (server.BandwidthMax != 0)
                    {
                        infoArea.Bandwidth += server.Bandwidth;
                        infoArea.BandwidthMax += server.BandwidthMax;
                    }

                    if (server.Users >= 0)
                    {
                        if (infoArea.Users == -1)
                            infoArea.Users = 0;
                        infoArea.Users += server.Users;
                    }

                    infoArea.Servers++;
                }

                for (;;)
                {
                    bool restart = false;
                    foreach (AreaInfo infoArea in m_areas.Values)
                    {
                        if (infoArea.Deleted)
                        {
                            m_areas.Remove(infoArea.Code);
                            restart = true;
                            break;
                        }
                    }

                    if (restart == false)
                        break;
                }

                // White/black list
                foreach (AreaInfo infoArea in m_areas.Values)
                {
                    string code = infoArea.Code;
                    if (areasWhiteList.Contains(code))
                        infoArea.UserList = AreaInfo.UserListType.WhiteList;
                    else if (areasBlackList.Contains(code))
                        infoArea.UserList = AreaInfo.UserListType.BlackList;
                    else
                        infoArea.UserList = AreaInfo.UserListType.None;
                }
            }
        }

		public void UpdateSettings()
		{
			List<string> serversWhiteList = new List<string>();
			List<string> serversBlackList = new List<string>();
			foreach (ServerInfo info in m_servers.Values)
				if (info.UserList == ServerInfo.UserListType.WhiteList)
					serversWhiteList.Add(info.Code);
				else if (info.UserList == ServerInfo.UserListType.BlackList)
					serversBlackList.Add(info.Code);
			Storage.SetList("servers.whitelist", serversWhiteList);
			Storage.SetList("servers.blacklist", serversBlackList);

			List<string> areasWhiteList = new List<string>();
			List<string> areasBlackList = new List<string>();
			foreach (AreaInfo info in m_areas.Values)
				if (info.UserList == AreaInfo.UserListType.WhiteList)
					areasWhiteList.Add(info.Code);
				else if (info.UserList == AreaInfo.UserListType.BlackList)
					areasBlackList.Add(info.Code);
			Storage.SetList("areas.whitelist", areasWhiteList);
			Storage.SetList("areas.blacklist", areasBlackList);
		}

		public void SaveSettings()
		{
			if (m_storage != null)
			{
				UpdateSettings();

				Storage.Save();
			}
		}

		public bool CheckEnvironment()
		{			
			bool installed = (Software.OpenVpnVersion != "");
			if (installed == false)
				throw new Exception("OpenVPN " + Messages.NotFound);

			string protocol = Storage.Get("mode.protocol").ToUpperInvariant();

			if ((protocol != "AUTO") && (protocol != "UDP") && (protocol != "TCP") && (protocol != "SSH") && (protocol != "SSL") )
				throw new Exception(Messages.CheckingProtocolUnknown);

			if( (protocol == "SSH") && (Software.SshVersion == "") )
				throw new Exception("SSH " + Messages.NotFound);

			if( (protocol == "SSL") && (Software.SslVersion == "") )
				throw new Exception("SSL " + Messages.NotFound);

			if (Storage.GetBool("advanced.skip_alreadyrun") == false)
			{
				Dictionary<int, string> processes = Platform.Instance.GetProcessesList();
				
				foreach (string name in processes.Values)
				{
					if (name == "openvpn")
						throw new Exception(Messages.AlreadyRunningOpenVPN);

					if ((protocol == "SSL") && (name == "stunnel"))
					{
						throw new Exception(Messages.AlreadyRunningSTunnel);
					}

					if ((protocol == "SSH") && (name == "plink"))
						throw new Exception(Messages.AlreadyRunningSshPLink);

					if ((protocol == "SSH") && (name == "ssh"))
						throw new Exception(Messages.AlreadyRunningSsh);					
				}
			}

			if (Storage.Get("proxy.mode") != "None")
			{
				string proxyHost = Storage.Get("proxy.host").Trim();
				int proxyPort = Storage.GetInt("proxy.port");
				if (proxyHost == "")
					throw new Exception(Messages.CheckingProxyHostMissing);
				if ((proxyPort <= 0) || (proxyPort >= 256 * 256))
					throw new Exception(Messages.CheckingProxyPortWrong);
								
				if (protocol == "UDP")
					throw new Exception(Messages.CheckingProxyNoUdp);
			}

			return Platform.Instance.OnCheckEnvironment();
		}

        public string GetSupportReport(string logs)
        {
            string report = "";

            report += "AirVPN Support Report - Generated " + DateTime.UtcNow.ToShortDateString() + " " + DateTime.UtcNow.ToShortTimeString() + " " + "UTC\n";

            report += "\n\n-- Important options not at defaults --\n" + Storage.GetReportForSupport();

            report += "\n\n-- Logs --\n" + logs;

            report += "\n\n-- System --\n" + Platform.Instance.GenerateSystemReport();
            
            return report;
        }

        public void LogOpenvpnConfig()
        {
            string t = "-- Start OpenVPN config dump\n" + Engine.Instance.ConnectedOVPN + "\n-- End OpenVPN config dump";
            t = Regex.Replace(t, "<ca>(.*?)</ca>", "<ca>omissis</ca>", RegexOptions.Singleline);
            t = Regex.Replace(t, "<key>(.*?)</key>", "<key>omissis</key>", RegexOptions.Singleline);
            t = Regex.Replace(t, "<cert>(.*?)</cert>", "<cert>omissis</cert>", RegexOptions.Singleline);
            t = Regex.Replace(t, "<tls-auth>(.*?)</tls-auth>", "<tls-auth>omissis</tls-auth>", RegexOptions.Singleline);
            Engine.Logs.Log(LogType.Verbose, t);
        }

		public string GetConnectedTrayText(bool speed, bool server)
        {
            if (CurrentServer == null)
                return "";

			string text = "";

			if (speed) {
				text += "D:" + Core.Utils.FormatBytes (ConnectedLastDownloadStep, true, false) + " U:" + Core.Utils.FormatBytes(ConnectedLastUploadStep, true, false);
			}
			if (server) {
				if (text != "")
					text += " - ";
				string serverName = CurrentServer.DisplayName;
				string country = CountriesManager.GetNameFromCode(CurrentServer.CountryCode);
				if (country != "")
					serverName += " (" + country + ")";
				text += serverName;
				text += " - IP:" + CurrentServer.IpExit; 
			}
			return text;
        }
    }
}
