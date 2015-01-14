// <airvpn_source_header>
// This file is part of AirVPN Client software.
// Copyright (C)2014-2014 AirVPN (support@airvpn.org) / https://airvpn.org )
//
// AirVPN Client is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// AirVPN Client is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with AirVPN Client. If not, see <http://www.gnu.org/licenses/>.
// </airvpn_source_header>

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

namespace AirVPN.Core
{
	public class Engine : AirVPN.Core.Thread
    {
		public static Engine Instance;

		public bool DevelopmentEnvironment = false;
        
		public bool ConsoleMode = false;

		public bool Terminated = false;
		public delegate void TerminateHandler();
		public event TerminateHandler TerminateEvent;

        private Threads.Pinger m_threadPinger;
        private Threads.Penalities m_threadPenalities;
		private Threads.Manifest m_threadManifest;
        private Threads.Session m_threadSession;
        private Storage m_storage;
		private Stats m_stats;
		private NetworkLockManager m_networkLockManager;
		private Dictionary<string, ServerInfo> m_servers = new Dictionary<string, ServerInfo>();
        private Dictionary<string, AreaInfo> m_areas = new Dictionary<string, AreaInfo>();
		private List<string> FrontMessages = new List<string>();
		private int m_breakRequests = 0;

		private String m_lastLogMessage;
		private int m_logDotCount = 0;

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
			Quick = 5
        }

		private List<ActionService> ActionsList = new List<ActionService>();        
		public string m_waitMessage = "Starting";
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

		public NetworkLockManager NetworkLockManager
		{
			get
			{
				return m_networkLockManager;
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

		public bool Initialization()
		{	
			if(ResourcesFiles.Count() == 0)			
			{
				ResourcesFiles.SetString("auth.xml", Lib.Core.Properties.Resources.Auth);
				ResourcesFiles.SetString("manifest.xml", Lib.Core.Properties.Resources.Manifest);
				ResourcesFiles.SetString("license.txt", Lib.Core.Properties.Resources.License);
				ResourcesFiles.SetString("thirdparty.txt", Lib.Core.Properties.Resources.ThirdParty);
				ResourcesFiles.SetString("tos.txt", Lib.Core.Properties.Resources.TOS);
			}
			
			Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);

			DevelopmentEnvironment = File.Exists(Platform.Instance.NormalizePath(Platform.Instance.GetProgramFolder() + "/dev.txt"));

			bool manMode = (CommandLine.SystemEnvironment.Exists("help"));
			if (manMode == false)
			{
				Log(LogType.Info, "AirVPN client version: " + Constants.VersionDesc + ", System: " + Platform.Instance.GetCode() + ", Name: " + Platform.Instance.GetName() + ", Architecture: " + Platform.Instance.GetArchitecture());
				if (DevelopmentEnvironment)
					Log(LogType.Info, "Development environment.");
			}


			m_storage = new Core.Storage();

			m_storage.Load(manMode);

			if (Storage.GetBool("cli"))
				ConsoleMode = true;

			if(Storage.Get("paramtest") != "") // Look comment in storage.cs
				Log(LogType.Warning, "Param test:-" + Storage.Get("paramtest") + "-");			

			m_stats = new Core.Stats();

			m_networkLockManager = new NetworkLockManager();
			m_networkLockManager.Init();

			if (Storage.GetBool("advanced.skip_privileges") == false)
			{
				if (Platform.Instance.IsAdmin() == false)
				{
					if (OnNoRoot() == false)
						Log(LogType.Fatal, Messages.AdminRequiredStop);

					return false;
				}
			}

			return true;
		}

        public override void OnRun()
        {
			bool initResult = OnInit();

			if(initResult == true)            
            {
				Platform.Instance.LogSystemInfo();

				Software.Checking(); // TOTEST messa qui, cosa comporta?

				Software.Log();

				Recovery.Load();

                RunEventCommand("app.start");

				Platform.Instance.OnAppStart();

				if (Engine.Storage.Get("netlock.mode") != "none")
				{
					if (Engine.Storage.GetBool("netlock")) // 2.8
						m_networkLockManager.Activation();					
				}

                WaitMessageClear();

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
				if ((IsLogged()) && (Engine.Storage.GetBool("connect")))
					autoStart = true;
				if (autoStart)
					Connect();
				else if(CancelRequested == false)
					Log(LogType.InfoImportant, Messages.Ready);
                
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

                RunEventCommand("app.stop");                
            }

			OnDeInit();
			OnDeInit2();

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
					if (Storage.Exists(commandLineParamKey) == false)
					{
						Log(LogType.Error, Messages.Format(Messages.CommandLineUnknownOption, commandLineParamKey));						
					}
				}

                Engine.Instance.Log(Engine.LogType.Verbose, "Data Path: " + Storage.DataPath);
				Engine.Instance.Log(Engine.LogType.Verbose, "App Path: " + Platform.Instance.GetProgramFolder());
				Engine.Instance.Log(Engine.LogType.Verbose, "Executable Path: " + Platform.Instance.GetExecutablePath());
				Engine.Instance.Log(Engine.LogType.Verbose, "Command line arguments (" + CommandLine.SystemEnvironment.Params.Count.ToString() + "): " + CommandLine.SystemEnvironment.GetFull());

                if (Storage.Get("profile") != "AirVPN")
                    Engine.Instance.Log(Engine.LogType.Verbose, "Profile: " + Storage.Get("profile"));

				return OnInit2();
            }
        }

		public virtual bool OnInit2()
        {
			// Software.Checking(); // TOTEST spostata, cosa comporta?

            TrustCertificatePolicy.Activate();

            PostManifestUpdate();

			m_threadPinger = new Threads.Pinger();
			m_threadPenalities = new Threads.Penalities();
			m_threadManifest = new Threads.Manifest();

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

			if (m_threadPinger != null)
				m_threadPinger.RequestStopSync();

			m_networkLockManager.Deactivation(true);
			m_networkLockManager = null;
			
			TemporaryFiles.Clean();

            Platform.DeInit();
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
				
			if (TickDeltaUiRefreshQuick.Elapsed(1000))
				OnRefreshUi(RefreshUiMode.Quick);

			if (TickDeltaUiRefreshFull.Elapsed(10000))
				OnRefreshUi(RefreshUiMode.Full);

			Sleep(100);
		}

		public virtual void OnConsoleKey(ConsoleKeyInfo key)
		{
			if (key.KeyChar == 'x')
			{
				Log(LogType.Info, Messages.ConsoleKeyCancel);
				OnExit();
			}
			else if (key.KeyChar == 'n')
			{
				Log(LogType.Info, Messages.ConsoleKeySwitch);
				SwitchServer = true;
				if ((Engine.IsLogged()) && (Engine.IsConnected() == false) && (Engine.IsWaiting() == false))
					Connect();
			}
		}

		public virtual void OnConsoleBreak()
		{
			Log(LogType.Info, Messages.ConsoleKeyBreak);

			OnExit();	
		}

		public virtual void OnCommand(CommandLine command)
		{
			string action = command.Get("action","").ToLowerInvariant();
			if (action == "exit")
			{
				OnExit();
			}
			else if (action == "openvpn")
			{
				SendManagementCommand(command.Get("command",""));
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
		}

		public virtual void OnSessionStop()
		{
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

		public void ConsoleStart()
		{
			ConsoleMode = true;

			string login = Storage.Get("login").Trim();
			string password = Storage.Get("password").Trim();

			if (Storage.GetBool("help"))
			{
				Engine.Instance.Log(Engine.LogType.Info, Core.UI.Actions.GetMan(Storage.Get("help_format")));				
			}
			else if ((login == "") || (password == ""))
			{
				Engine.Instance.Log(Engine.LogType.Fatal, Messages.ConsoleHelp);
			}
			else
			{
				Engine.Instance.Log(Engine.LogType.Info, Messages.ConsoleKeyboardHelp);				

				if(Storage.GetBool("connect") == false)
					Engine.Instance.Log(Engine.LogType.Info, Messages.ConsoleKeyboardHelpNoConnect);									
				
				Start();
			}
		}

		public void UiStart()
		{
			Start();
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

        public enum LogType
        {
            Realtime = 0, // Only tray or status label, no logs.
            Verbose = 1, // Only log, no tray
            Info = 2, // Tray
            InfoImportant = 3,
            Warning = 4,
            Error = 5,
            Fatal = 6 // MsgBox
        }

		public void Log(Exception e)
		{
			Log(LogType.Error, e);
		}

        public void Log(LogType Type, Exception e)
        {
			string msg = e.Message;
			if (DevelopmentEnvironment)
				msg += " - Stack: " + e.StackTrace.ToString ();
			Log(Type, msg, 1000, e);
        }

        public void LogDebug(string Message)
        {
            Log(LogType.Verbose, Message, 1000, null);
        }

        public void Log(LogType Type, string Message)
        {
            Log(Type, Message, 1000, null);
        }

		public void Log(LogType Type, string Message, int BalloonTime)
		{
			Log(Type, Message, BalloonTime, null);
		}

        public void Log(LogType Type, string Message, int BalloonTime, Exception e)
        {
            LogEntry l = new LogEntry();
            l.Type = Type;
            l.Message = Message;
            l.BalloonTime = BalloonTime;
			l.Exception = e;

			if (l.Type > Engine.LogType.Realtime)
			{
				m_lastLogMessage = l.Message;
				m_logDotCount += 1;
				m_logDotCount = m_logDotCount % 10;
			}

            OnLog(l);
        }

		public string GetLogDetailTitle()
		{
			string output = "";
			if ((Storage != null) && (Storage.GetBool("advanced.expert")))
			{
				if (WaitMessage == m_lastLogMessage)
					output = "";
				else
					output = m_lastLogMessage;
			}
			else
			{
				for (int i = 0; i < m_logDotCount; i++)
					output += ".";
			}
			return output;
		}

		public string GetLogSuggestedFileName()
		{
			return "AirVPN_" + DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + ".txt";
		}

		public List<string> ParseLogFilePath(string paths)
		{	
			string logPaths = paths;				
			DateTime now = DateTime.Now;
			logPaths = logPaths.Replace("%d", now.ToString("dd"));
			logPaths = logPaths.Replace("%m", now.ToString("MM"));
			logPaths = logPaths.Replace("%y", now.ToString("yyyy"));
			logPaths = logPaths.Replace("%w", now.ToString("dddd"));
			logPaths = logPaths.Replace("%H", now.ToString("HH"));
			logPaths = logPaths.Replace("%M", now.ToString("mm"));
			logPaths = logPaths.Replace("%S", now.ToString("ss"));

			List<string> results = new List<string>();

			string[] logPathsArray = logPaths.Split(';');
			foreach (string path in logPathsArray)
			{
				string logPath = path;
				if (System.IO.Path.IsPathRooted(path) == false)
				{
					logPath = Storage.DataPath + "/" + logPath;
				}
				logPath = Platform.Instance.NormalizePath(logPath).Trim();
				if (logPath != "")
					results.Add(logPath);
			}
			
			return results;			
		}

		public string GetParseLogFilePaths(string paths)
		{
			string output = "";
			List<string> results = ParseLogFilePath(paths);
			foreach (string path in results)
			{
				output += path + "\r\n";
			}
			return output.Trim();
		}

        public virtual void OnLog(LogEntry l)
        {
			// An exception, to have a clean, standard 'man' output without logging header.
			if (CommandLine.SystemEnvironment.Exists("help"))
			{
				Console.WriteLine(l.Message);
				return;
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
							string logPath = Storage.Get("log.file.path").Trim();

							List<string> paths = ParseLogFilePath(logPath);
							foreach (string path in paths)
							{
								Directory.CreateDirectory(Path.GetDirectoryName(path));
								File.AppendAllText(path, lines + "\n");
							}
						}
					}
				}
			}
        }

		public virtual void OnFrontMessage(string message)
		{
			Log(LogType.Warning, message);
		}

		public virtual void OnPostManifestUpdate()
		{
		}

        public virtual void OnRefreshUi()
        {
            OnRefreshUi(RefreshUiMode.Full);
        }

        public virtual void OnRefreshUi(RefreshUiMode mode)
        {
			if ((mode == Core.Engine.RefreshUiMode.Quick) || (mode == Core.Engine.RefreshUiMode.Full))
			{
				string manifestLastUpdate = Utils.FormatTime(Utils.XmlGetAttributeInt64(Engine.Storage.Manifest, "time", 0));
				if(Core.Threads.Manifest.Instance != null)
					if(Core.Threads.Manifest.Instance.ForceUpdate)
						manifestLastUpdate += " (" + Messages.ManifestUpdateForce + ")";
				Stats.UpdateValue("ManifestLastUpdate", manifestLastUpdate);
			}

			if ((mode == Core.Engine.RefreshUiMode.Stats) || (mode == Core.Engine.RefreshUiMode.Full))
			{
				if (Engine.IsConnected())
				{
					{
						DateTime DT1 = Engine.Instance.ConnectedSince;
						DateTime DT2 = DateTime.UtcNow;
						TimeSpan TS = DT2 - DT1;
						string TSText = string.Format("{0:00}:{1:00}:{2:00} - {3}", (int)TS.TotalHours, TS.Minutes, TS.Seconds, DT1.ToLocalTime().ToLongDateString() + " " + DT1.ToLocalTime().ToLongTimeString());
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
				Stats.UpdateValue("SystemReport", Messages.DoubleClickToView);
				if (m_threadPinger != null)
				{
					Stats.UpdateValue("Pinger", PingerStats().ToString());					
				}

				if (Engine.IsConnected())
				{
					Stats.UpdateValue("ServerName", Engine.CurrentServer.Name);
					Stats.UpdateValue("ServerLatency", Engine.CurrentServer.Ping.ToString() + " ms");
					Stats.UpdateValue("ServerLocation", Engine.CurrentServer.CountryName + " - " + Engine.CurrentServer.Location);
					Stats.UpdateValue("ServerLoad", Engine.CurrentServer.Load().ToString());
					Stats.UpdateValue("ServerUsers", Engine.CurrentServer.Users.ToString());

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
				Log(LogType.Fatal, Messages.UnhandledException + " - " + e.Message + " - " + e.StackTrace.ToString());
		}
        
        // ----------------------------------------------------
        // Misc
        // ----------------------------------------------------
		   

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
                
				//bool existsWhiteList = (existsWhiteListAreas || existsWhiteListServers);

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

		public ServerInfo PickServer(string preferred)
        {
            lock (m_servers)
            {
                if ((preferred != null) && (m_servers.ContainsKey(preferred)))
                    return m_servers[preferred];
                else
                {
                    List<ServerInfo> list = GetServers(false);
                    if (list.Count > 0)
                        return list[0];
                }
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
			if (Storage.Manifest.Attributes["front_message"] != null)
			{
				string msg = Storage.Manifest.Attributes["front_message"].Value;
				if (FrontMessages.Contains(msg) == false)
				{
					OnFrontMessage(msg);
					FrontMessages.Add(msg);
				}
			}

            lock (m_servers)
            {
                foreach (ServerInfo infoServer in m_servers.Values)
                {
                    infoServer.Deleted = true;
                }

                List<string> whiteList = Storage.GetList("servers.whitelist");
                List<string> blackList = Storage.GetList("servers.blacklist");

                if (Storage.Manifest != null)
                {
                    lock (Storage.Manifest)
                    {
                        foreach (XmlNode nodeServer in Storage.Manifest.SelectNodes("//servers/server"))
                        {
							string name = nodeServer.Attributes["name"].Value;

                            ServerInfo infoServer = null;
                            if (m_servers.ContainsKey(name))
                                infoServer = m_servers[name];
                            if (infoServer == null)
                            {
                                // Create
                                infoServer = new ServerInfo();
                                infoServer.Name = name;
                                m_servers[name] = infoServer;
                            }

                            {
                                // Update info
                                infoServer.Deleted = false;

                                infoServer.PublicName = Utils.XmlGetAttributeString(nodeServer, "public_name", "");
                                infoServer.IpEntry = Utils.XmlGetAttributeString(nodeServer, "ip_entry", ""); ;
                                infoServer.IpEntry2 = Utils.XmlGetAttributeString(nodeServer, "ip_entry2", "");
                                infoServer.IpExit = Utils.XmlGetAttributeString(nodeServer, "ip_exit", "");
                                infoServer.CountryCode = Utils.XmlGetAttributeString(nodeServer, "country_code", "");
                                infoServer.CountryName = Utils.XmlGetAttributeString(nodeServer, "country_name", "");
                                infoServer.Location = Utils.XmlGetAttributeString(nodeServer, "location", "");
                                infoServer.ScoreBase = Utils.XmlGetAttributeInt64(nodeServer, "scorebase", 0);
                                infoServer.Bandwidth = Utils.XmlGetAttributeInt64(nodeServer, "bw", 0);
                                infoServer.BandwidthMax = Utils.XmlGetAttributeInt64(nodeServer, "bw_max", 1);
                                infoServer.Users = Utils.XmlGetAttributeInt64(nodeServer, "users", 0);
                                infoServer.WarningOpen = Utils.XmlGetAttributeString(nodeServer, "warning_open", "");
                                infoServer.WarningClosed = Utils.XmlGetAttributeString(nodeServer, "warning_closed", "");
								infoServer.ServerType = Utils.XmlGetAttributeInt64(nodeServer, "server_type", -1);
								infoServer.Public = Utils.XmlGetAttributeBool(nodeServer, "public", false);								

                                if (whiteList.Contains(name))
                                    infoServer.UserList = ServerInfo.UserListType.WhiteList;
                                else if (blackList.Contains(name))
                                    infoServer.UserList = ServerInfo.UserListType.BlackList;
                            }
                        }
                    }
                }

                for (; ; )
                {
                    bool restart = false;
                    foreach (ServerInfo infoServer in m_servers.Values)
                    {
                        if (infoServer.Deleted)
                        {
                            m_servers.Remove(infoServer.Name);
							restart = true;
                            break;
                        }
                    }

                    if (restart == false)
                        break;
                }
            }

            lock (m_areas)
            {
                foreach (AreaInfo infoArea in m_areas.Values)
                {
                    infoArea.Deleted = true;
                }

                List<string> whiteList = Storage.GetList("areas.whitelist");
                List<string> blackList = Storage.GetList("areas.blacklist");

                if (Storage.Manifest != null)
                {
                    lock (Storage.Manifest)
                    {
                        foreach (XmlNode nodeServer in Storage.Manifest.SelectNodes("//areas/area"))
                        {
							string code = nodeServer.Attributes["code"].Value;

                            AreaInfo infoArea = null;
                            if (m_areas.ContainsKey(code))
                                infoArea = m_areas[code];
                            if (infoArea == null)
                            {
                                // Create
                                infoArea = new AreaInfo();
                                infoArea.Code = code;
                                m_areas[code] = infoArea;
                            }

                            {
                                // Update info
                                infoArea.Deleted = false;

								infoArea.PublicName = Utils.XmlGetAttributeString(nodeServer, "public_name", "");
								infoArea.Bandwidth = Utils.XmlGetAttributeInt64(nodeServer, "bw", 0);
								infoArea.BandwidthMax = Utils.XmlGetAttributeInt64(nodeServer, "bw_max", 1);
								infoArea.Users = Utils.XmlGetAttributeInt64(nodeServer, "users", 0);
								infoArea.Servers = Utils.XmlGetAttributeInt64(nodeServer, "servers", 0);                                

                                if(whiteList.Contains(code))
                                    infoArea.UserList = AreaInfo.UserListType.WhiteList;
                                else if(blackList.Contains(code))
                                    infoArea.UserList = AreaInfo.UserListType.BlackList;
                            }
                        }
                    }
                }

                for (; ; )
                {
                    bool restart = false;
                    foreach (AreaInfo infoArea in m_areas.Values)
                    {
                        if (infoArea.Deleted)
                        {
                            m_areas.Remove(infoArea.Code);
                            break;
                        }
                    }

                    if (restart == false)
                        break;
                }
            }

			if (m_networkLockManager != null)
				m_networkLockManager.OnUpdateIps();

            OnRefreshUi(Core.Engine.RefreshUiMode.Full);

			OnPostManifestUpdate();
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
				Engine.Log(Engine.LogType.Info, message);

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
		
		public byte[] FetchUrl(string url, string title, int ntry, bool bypassProxy)
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

						if (Storage.Get("mode.protocol").ToUpperInvariant() == "TOR")
						{
							proxyMode = "socks";
							proxyHost = Storage.Get("mode.tor.host");
							proxyPort = Storage.GetInt("mode.tor.port");
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
								TemporaryFile fileOutput = new TemporaryFile("bin");
								string args = " \"" + url + "\" --socks4a " + proxyHost + ":" + proxyPort;
								if (proxyAuth != "none")
								{
									args += " -U " + proxyLogin + ":" + proxyPassword;
								}
								args += " -o \"" + fileOutput.Path + "\"";
								args += " --progress-bar";
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

					return wc.DownloadData(url);
				}
				catch (Exception e)
				{
					if(ntry == 1) // AirAuth have it's catch errors retry logic.
						throw e;
					else
					{
						lastException = e.Message;
						
						Engine.Instance.Log(Engine.LogType.Warning, Messages.Format(Messages.ExchangeTryFailed, title, (t+1).ToString(), lastException));
					}
				}
			}

			throw new Exception(lastException);
		}

		public XmlDocument XmlFromUrl(string url, string title, bool bypassProxy)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(System.Text.Encoding.ASCII.GetString(FetchUrl(url, title, 5, bypassProxy)));
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

        public void BuildOVPN(string protocol, int port, int alt, int proxyPort)
        {
            DateTime now = DateTime.UtcNow;
            Storage s = Engine.Instance.Storage;
						
            string ip = CurrentServer.IpEntry;
            if (alt == 1)
                ip = CurrentServer.IpEntry2;

			string ovpn = "";
			ovpn += "# " + Messages.Format(Messages.GeneratedFileHeader, Constants.VersionDesc) + "\n";
            ovpn += "# " + now.ToLongDateString() + " " + now.ToLongTimeString() + " UTC\n";
            if (s.GetBool("openvpn.skip_defaults") == false)
                ovpn += s.Manifest.Attributes["openvpn_directives_common"].Value.Replace("\t", "").Trim() + "\n";

            if (protocol == "UDP")
            {
                ovpn += "proto udp\n";
                if (s.GetBool("openvpn.skip_defaults") == false)
                    ovpn += s.Manifest.Attributes["openvpn_directives_udp"].Value.Replace("\t", "").Trim() + "\n";
            }
            else // TCP, SSH, SSL, TOR
            {
                ovpn += "proto tcp\n";
                if (s.GetBool("openvpn.skip_defaults") == false)
                    ovpn += s.Manifest.Attributes["openvpn_directives_tcp"].Value.Replace("\t", "").Trim() + "\n";
            }

            if (protocol == "SSH")
				ovpn += "remote 127.0.0.1 " + Conversions.ToString(proxyPort) + "\n";
            else if (protocol == "SSL")
				ovpn += "remote 127.0.0.1 " + Conversions.ToString(proxyPort) + "\n";
            else
                ovpn += "remote " + ip + " " + port.ToString() + "\n";

			if (protocol == "TOR")
			{
				ovpn += "socks-proxy " + s.Get("mode.tor.host") + " " + s.Get("mode.tor.port") + "\n";
			}
			else
			{
				string proxyMode = s.GetLower("proxy.mode");
				if (proxyMode == "http")
				{
					ovpn += "http-proxy " + s.Get("proxy.host") + " " + s.Get("proxy.port");
				}
				else if (proxyMode == "socks")
				{
					ovpn += "socks-proxy " + s.Get("proxy.host") + " " + s.Get("proxy.port");
				}

				if (s.GetLower("proxy.mode") != "none")
				{
					if (s.Get("proxy.auth") != "None")
					{
						string fileNameAuth = s.GetPath("AirVPN.ppw");
						string fileNameAuthOvpn = fileNameAuth.Replace("\\", "\\\\"); // 2.6, Escaping for Windows
						string fileNameData = s.Get("proxy.login") + "\n" + s.Get("proxy.password") + "\n";
						Utils.SaveFile(fileNameAuth, fileNameData);
						ovpn += " \"" + fileNameAuthOvpn + "\" " + s.Get("proxy.auth").ToLowerInvariant() + "\n"; // 2.6 Auth Fix
					}
					else
					{
						ovpn += "\n";
					}
				}
			}

            string routesDefault = s.Get("routes.default");
            if (routesDefault == "out")
            {
				ovpn += "route-nopull\n";

				// For Checking
				ovpn += "route " + CurrentServer.IpExit + " 255.255.255.255 vpn_gateway # For Checking Route\n";

				// For DNS
				ovpn += "dhcp-option DNS " + Constants.DnsVpn + "\n"; // Manually because route-nopull skip it
				ovpn += "route 10.4.0.1 255.255.255.255 vpn_gateway # AirDNS\n";
				ovpn += "route 10.5.0.1 255.255.255.255 vpn_gateway # AirDNS\n";
				ovpn += "route 10.6.0.1 255.255.255.255 vpn_gateway # AirDNS\n";
				ovpn += "route 10.7.0.1 255.255.255.255 vpn_gateway # AirDNS\n";
				ovpn += "route 10.8.0.1 255.255.255.255 vpn_gateway # AirDNS\n";
				ovpn += "route 10.9.0.1 255.255.255.255 vpn_gateway # AirDNS\n";
				ovpn += "route 10.30.0.1 255.255.255.255 vpn_gateway # AirDNS\n";
				ovpn += "route 10.50.0.1 255.255.255.255 vpn_gateway # AirDNS\n"; 
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
					Log(LogType.Warning, Messages.Format(Messages.CustomRouteInvalid, ipCustomRoute.ToString()));
				else
				{
					string action = routeEntries[1];
					string notes = routeEntries[2];

					if ((routesDefault == "out") && (action == "in"))
						ovpn += "route " + ipCustomRoute.ToOpenVPN() + " vpn_gateway # " + Utils.SafeString(notes) + "\n";
					if ((routesDefault == "in") && (action == "out"))
						ovpn += "route " + ipCustomRoute.ToOpenVPN() + " net_gateway # " + Utils.SafeString(notes) + "\n";
				}
            }

			if (routesDefault == "in")
			{
				if ((protocol == "SSH") || (protocol == "SSL"))
				{
					ovpn += "route " + ip + " 255.255.255.255 net_gateway # VPN Entry IP\n";
				}

				if (protocol == "TOR")
				{
					List<string> torNodeIps = TorControl.GetGuardIps();
					foreach (string torNodeIp in torNodeIps)
					{
						ovpn += "route " + torNodeIp + " 255.255.255.255 net_gateway # Tor Circuit\n";
					}
				}
			}
	
			ovpn += "management localhost " + Engine.Instance.Storage.Get("openvpn.management_port") + "\n";

			Platform.Instance.OnBuildOvpn(ref ovpn);			
						
            ovpn += "\n";
            ovpn += s.Get("openvpn.custom").Replace("\t", "").Trim();
            ovpn += "\n";

			// Experimental - Allow identification as Public Network in Windows. Advanced Option?
			// ovpn += "route-metric 512\n";
			// ovpn += "route 0.0.0.0 0.0.0.0\n";


			//XmlNode nodeUser = s.Manifest.SelectSingleNode("//user");
			XmlNode nodeUser = s.User;
			ovpn += "<ca>\n" + nodeUser.Attributes["ca"].Value + "</ca>\n";
            XmlNode nodeUserKey = s.User.SelectSingleNode("keys/key[@id='default']"); // TODO
			ovpn += "<cert>\n" + nodeUserKey.Attributes["crt"].Value + "</cert>\n";
			ovpn += "<key>\n" + nodeUserKey.Attributes["key"].Value + "</key>\n";
            ovpn += "key-direction 1\n";
			ovpn += "<tls-auth>\n" + nodeUser.Attributes["ta"].Value + "</tls-auth>\n";

			

			// Custom replacement, useful to final adjustment of generated OVPN by server-side rules.
			// Never used yet, available for urgent maintenance.
			int iCustomReplaces = 0;
			for(;;)
			{
				if(s.Manifest.Attributes["openvpn_replace" + iCustomReplaces.ToString() + "_pattern"] == null)
					break;

				string pattern = s.Manifest.Attributes["openvpn_replace" + iCustomReplaces.ToString() + "_pattern"].Value;
				string replacement = s.Manifest.Attributes["openvpn_replace" + iCustomReplaces.ToString() + "_replacement"].Value;

				ovpn = Regex.Replace(ovpn, pattern, replacement);

				iCustomReplaces++;
			}
			
            ConnectedOVPN = ovpn;
            ConnectedEntryIP = ip;
            ConnectedPort = port;
            ConnectedProtocol = protocol;
        }

		public bool IsLogged()
		{
			return ( (Storage.User != null) && (Storage.User.Attributes["login"] != null) );			
		}

		public void Command(string cmd)
		{
			CommandLine command = new CommandLine(cmd, false, true);
			
			try
			{
				OnCommand(command);
				Log(LogType.Info, "Command '" + command.GetFull() + "' executed");
			}
			catch (Exception e)
			{
				Log(LogType.Error, e);
			}
		}

		private void Auth()
		{
			Engine.Instance.WaitMessageSet(Messages.AuthorizeLogin, false);
			Engine.Log(Engine.LogType.Info, Messages.AuthorizeLogin);

			Dictionary<string, string> parameters = new Dictionary<string, string>();
			parameters["act"] = "user";

			try
			{
				XmlDocument xmlDoc = AirExchange.Fetch(Messages.AuthorizeLogin, parameters);
				string userMessage = Utils.XmlGetAttributeString(xmlDoc.DocumentElement, "message", "");

				if (userMessage != "")
				{
					Log(LogType.Fatal, userMessage);
				}
				else
				{
					Log(LogType.InfoImportant, Messages.AuthorizeLoginDone);
					Storage.User = xmlDoc.DocumentElement;
				}
			}
			catch (Exception e)
			{
				Log(LogType.Fatal, Messages.Format(Messages.AuthorizeLoginFailed, e.Message));				
			}

			SaveSettings(); // 2.8

			Engine.Instance.WaitMessageClear();
		}

		private void DeAuth()
		{
			if (IsLogged())
			{
				Engine.Instance.WaitMessageSet(Messages.AuthorizeLogout, false);

				Storage.User = null;

				Log(LogType.InfoImportant, Messages.AuthorizeLogoutDone);

				Engine.Instance.WaitMessageClear();
			}

			SaveSettings(); // 2.8
		}
        
        private void SessionStart()
        {
            try
            {
				Engine.Log(Engine.LogType.Info, Messages.SessionStart);

				Engine.Instance.WaitMessageSet(Messages.CheckingEnvironment, true);
				Engine.Log(Engine.LogType.Info, Messages.OsDriverInstall);
                
                CheckEnvironment();

				// Check Driver				
				if (Platform.Instance.GetDriverAvailable() == "")
				{
					if (Platform.Instance.CanInstallDriver())
					{
						Engine.Instance.WaitMessageSet(Messages.OsDriverInstall, false);
						Engine.Log(Engine.LogType.InfoImportant, Messages.OsDriverInstall);

						Platform.Instance.InstallDriver();

						if (Platform.Instance.GetDriverAvailable() == "")
							throw new Exception(Messages.OsDriverFailed);
					}
					else
						throw new Exception(Messages.OsDriverFailed);
				}

				if (m_threadSession != null)
                    throw new Exception("Daemon already running.");

                if (Storage.UpdateManifestNeed(true))
                {
					Engine.Instance.WaitMessageSet(Messages.RetrievingManifest, true);
					Engine.Log(Engine.LogType.Info, Messages.RetrievingManifest);

					string result = Engine.WaitManifestUpdate();

					if (result != "")
                    {
						if (Storage.UpdateManifestNeed(false))
                        {
                            throw new Exception(result);
                        }
                        else
                        {
                            Log(LogType.Warning, Messages.ManifestFailedContinue);
                        }
                    }
                }

				OnSessionStart();

				if (NextServer == null)
				{
					if (Engine.Storage.Get("server") != "")
						NextServer = Engine.PickServer(Engine.Storage.Get("server"));
					else if (Engine.Storage.GetBool("servers.startlast"))
						NextServer = Engine.PickServer(Engine.Storage.Get("servers.last"));
				}

				m_threadSession = new Threads.Session();                
            }
            catch (Exception e)
            {
                Log(LogType.Fatal, e);

                WaitMessageClear();

				//ConsoleExit();			
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
                    Log(LogType.Fatal, e);
                }

				OnSessionStop();
				
                WaitMessageClear();

                Engine.Log(Engine.LogType.InfoImportant, Messages.SessionStop);

				//ConsoleExit();			
            }
			
        }

		public void SaveSettings()
		{
			if (m_storage != null)
			{
				List<string> serversWhiteList = new List<string>();
				List<string> serversBlackList = new List<string>();
				foreach (ServerInfo info in m_servers.Values)
					if (info.UserList == ServerInfo.UserListType.WhiteList)
						serversWhiteList.Add(info.Name);
					else if (info.UserList == ServerInfo.UserListType.BlackList)
						serversBlackList.Add(info.Name);
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


				Storage.Save();
			}
		}

		public void CheckEnvironment()
		{			
			bool installed = (Software.OpenVpnVersion != "");
			if (installed == false)
				throw new Exception("OpenVPN " + Messages.NotFound);

			string protocol = Storage.Get("mode.protocol").ToUpperInvariant();

			if ((protocol != "AUTO") && (protocol != "UDP") && (protocol != "TCP") && (protocol != "SSH") && (protocol != "SSL") && (protocol != "TOR"))
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
								
				if (protocol != "TCP")
					throw new Exception(Messages.CheckingProxyNoTcp);
			}
		}
    }
}
