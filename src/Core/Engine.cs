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
using Eddie.Lib.Common;

namespace Eddie.Core
{
	public class Engine : Eddie.Core.Thread
	{
		public static Engine Instance;

		public bool ConsoleMode = false;

		public bool Terminated = false;
		public delegate void TerminateHandler();
		public event TerminateHandler TerminateEvent;

		public delegate void CommandEventHandler(XmlItem data);
		public event CommandEventHandler CommandEvent;

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
		private TcpServer m_tcpServer;

		private Dictionary<string, ConnectionInfo> m_connections = new Dictionary<string, ConnectionInfo>();
		private Dictionary<string, AreaInfo> m_areas = new Dictionary<string, AreaInfo>();
		private bool m_serversInfoUpdated = false;
		private bool m_areasInfoUpdated = false;
		private List<string> m_frontMessages = new List<string>();
		private int m_breakRequests = 0;
		//private TimeDelta m_tickDeltaUiRefreshQuick = new TimeDelta();
		private TimeDelta m_tickDeltaUiRefreshFull = new TimeDelta();

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
			MainMessage = 3, // Clodo, TOCLEAN?
			Log = 4 // Clodo, TOCLEAN?
		}

		private List<ActionService> m_actionsList = new List<ActionService>();
		private string m_waitMessage = Messages.AppStarting;
		private bool m_waitCancel = false;
		private bool m_connected = false;

		public ConnectionInfo CurrentServer;
		public ConnectionInfo NextServer;
		public bool SwitchServer;

		public DateTime ConnectedSince = DateTime.MinValue;
		public OvpnBuilder ConnectedOVPN; // TOCLEAN?
		public string ConnectedProtocol = "";
		public IpAddress ConnectedEntryIP = "";
		public IpAddresses ConnectedExitIP = new IpAddresses();
		public int ConnectedPort = 0;
		public Int64 ConnectedServerTime = 0;
		public Int64 ConnectedClientTime = 0;
		public string ConnectedRealIp = "";
		public string ConnectedControlChannel = "";
		public string ConnectedVpnInterfaceName = "";
		public string ConnectedVpnInterfaceId = "";
		public Int64 ConnectedVpnStatsRead = 0;
		public Int64 ConnectedVpnStatsWrite = 0;
		public Int64 ConnectedSessionStatsRead = 0;
		public Int64 ConnectedSessionStatsWrite = 0;
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

		public TcpServer TcpServer
		{
			get
			{
				return m_tcpServer;
			}
		}

		public Dictionary<string, ConnectionInfo> Connections
		{
			get
			{
				return m_connections;
			}
		}

		public Dictionary<string, AreaInfo> Areas
		{
			get
			{
				return m_areas;
			}
		}

		public bool DevelopmentEnvironment
		{
			get
			{
				return CommandLine.SystemEnvironment.Exists("development");
			}
		}

		public Providers.Service AirVPN // TOFIX, for compatibility
		{
			get
			{
				foreach (Provider provider in ProvidersManager.Providers)
				{
					if ((provider.Enabled) && (provider.Code == "AirVPN"))
						return provider as Providers.Service;
				}
				return null;
			}
		}

		public bool Initialization(bool cli)
		{
			Platform.Instance.OnInit(cli);

			if (ResourcesFiles.Count() == 0)
			{
				ResourcesFiles.SetString("AirVPN.xml", Core.Properties.Resources.AirVPN); // TOCLEAN with Eddie3
				ResourcesFiles.SetString("OpenVPN.xml", Core.Properties.Resources.OpenVPN); // TOCLEAN with Eddie3
				ResourcesFiles.SetString("license.txt", Core.Properties.Resources.License);
				ResourcesFiles.SetString("thirdparty.txt", Core.Properties.Resources.ThirdParty);
				ResourcesFiles.SetString("tos.txt", Core.Properties.Resources.TOS); // TOCLEAN
			}

			m_logsManager = new LogsManager();

			m_storage = new Core.Storage();

			if (cli)
			{
				if (Storage.GetBool("version"))
				{
					Console.WriteLine(Constants.Name + " - version " + Constants.VersionDesc);
					RequestStop();
					return false;
				}
				else if (Storage.GetBool("version.short"))
				{
					Console.WriteLine(Lib.Common.Constants.VersionDesc);
					RequestStop();
					return false;
				}
				else if (Storage.GetBool("help"))
				{
					Engine.Instance.Logs.Log(LogType.Info, Storage.GetMan(Storage.Get("help.format")));
					RequestStop();
					return false;
				}
			}

			Logs.Log(LogType.Info, "Eddie version: " + Constants.VersionDesc + " / " + Platform.Instance.GetSystemCode() + ", System: " + Platform.Instance.GetCode() + ", Name: " + Platform.Instance.GetName() + ", Version: " + Platform.Instance.GetVersion() + ", Mono/.Net Framework: " + Platform.Instance.GetMonoVersion());

			if (DevelopmentEnvironment)
				Logs.Log(LogType.Info, "Development environment.");

			// This is before the Storage.Load, because non-root can't read option (chmod)
			if (Storage.GetBool("advanced.skip_privileges") == false)
			{
				if (Platform.Instance.IsAdmin() == false)
				{
					if (ConsoleMode)
					{
						Logs.Log(LogType.Fatal, Messages.AdminRequiredStop);
					}
					else if (Platform.Instance.RestartAsRoot() == false)
					{
						Logs.Log(LogType.Fatal, Messages.AdminRequiredStop);
					}

					RequestStop();
					return false;
				}
			}

			if (Storage.Get("console.mode") == "keys")
				Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);

			CountriesManager.Init();

			// Providers
			m_providersManager = new ProvidersManager();
			m_providersManager.Init();

			m_storage.Load();

			if (Storage.GetBool("tcpserver.enabled") == true)
			{
				m_tcpServer = new TcpServer();
				m_tcpServer.Start();

				/*
				string pathControl = Storage.Get("console.control.path");
				if (pathControl != "")
				{
					Platform.Instance.FileContentsWriteText(pathControl, Storage.Get("tcpserver.port"));

					m_tcpServer.SignalConnection.WaitOne(); // Clodo, TOCHECK; CTRL+C stop it?
				}
				*/

				if (Storage.Get("console.mode") == "tcp")
				{
					// Start requested by an UI, wait it.
					m_tcpServer.SignalConnection.WaitOne(); // Clodo, TOCHECK; CTRL+C stop it?
				}
			}

			m_providersManager.Load();

			if (Storage.GetBool("cli"))
				ConsoleMode = true;

			if (Storage.Get("paramtest") != "") // Look comment in storage.cs
				Logs.Log(LogType.Warning, "Param test:-" + Storage.Get("paramtest") + "-");

			if (Storage.GetBool("os.single_instance") == true)
			{
				if (Platform.Instance.OnCheckSingleInstance() == false)
				{
					Logs.Log(LogType.Fatal, Messages.OsInstanceAlreadyRunning);
					RequestStop();
					return false;
				}
			}

			m_stats = new Core.Stats();

			if ((WebServer.GetPath() != "") && (Storage.GetBool("webui.enabled") == true))
			{
				m_webServer = new WebServer();
				m_webServer.Start();
			}

			m_networkLockManager = new NetworkLockManager();
			m_networkLockManager.Init();

			CompatibilityManager.Init();

			Platform.Instance.OnStart();

			return true;
		}

		public override void OnRun()
		{
			bool initResult = OnInit();

			// Not the best, but under Mono/Linux allow showing the 'Starting...' that give a feedback of running.
			System.Threading.Thread.Sleep(1000);

			if (initResult == true)
			{
				Software.Checking();
				Software.Log();

				CheckEnvironmentApp();

				Platform.Instance.OnRecovery();

				Recovery.Load();

				RunEventCommand("app.start");

				if (Engine.Storage.GetLower("netlock.mode") != "none")
				{
					if (Engine.Storage.GetBool("netlock")) // 2.8
						m_networkLockManager.Activation();
				}

				WaitMessageClear();

				m_threadPinger = new Threads.Pinger();
				m_threadPenalities = new Threads.Penalities();
				m_threadDiscover = new Threads.Discover();
				m_threadManifest = new Threads.Manifest();

				PostManifestUpdate();

				bool autoStart = false;
				if (ConsoleMode)
				{
					Auth();
				}

				// Clodo: Try to remove: if not logged, no servers, fatal error and exit.
				if ((CanConnect()) && (Engine.Storage.GetBool("connect")))
					autoStart = true;
				if (autoStart)
					Connect();
				else
					Logs.Log(LogType.InfoImportant, Messages.Ready);

				for (;;)
				{
					OnWork();

					if (ConsoleMode)
					{
						if (Storage.Get("console.mode") == "keys")
						{
							try
							{
								// try/catch because throw an exception if stdin is redirected.
								// May in future we can use Console.IsInputRedirected, but require .Net 4.5
								if (Console.KeyAvailable)
								{
									ConsoleKeyInfo key = Console.ReadKey();
									OnConsoleKey(key);
								}
							}
							catch
							{
							}
						}
					}

					if (CancelRequested)
						break;
				}
			}

			Logs.Log(LogType.Verbose, Messages.AppShutdownStart);

			OnDeInit();

			RunEventCommand("app.stop");

			Command("shutdown.done"); // Clodo, new Eddie3

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

					// 2.11.11 - Used to avoid a Mono crash if pressed Win key
					if (commandLineParamKey == "verify-all")
						continue;

					if (Storage.Exists(commandLineParamKey) == false)
					{
						Logs.Log(LogType.Error, MessagesFormatter.Format(Messages.CommandLineUnknownOption, commandLineParamKey));
					}
				}

				// 2.13.4 // TOCLEAN, useless, already in stats tab.
				//Logs.Log(LogType.Verbose, "Data path: " + Storage.DataPath);
				//Logs.Log(LogType.Verbose, "Application path: " + Platform.Instance.GetApplicationPath());
				//Logs.Log(LogType.Verbose, "Executable path: " + Platform.Instance.GetExecutablePath());
				Logs.Log(LogType.Verbose, "Command line arguments (" + CommandLine.SystemEnvironment.Params.Count.ToString() + "): " + CommandLine.SystemEnvironment.GetFull());

				if (Storage.Get("profile") != "AirVPN.xml")
					Logs.Log(LogType.Verbose, "Profile path: " + Storage.GetProfilePath());

				//return OnInit2(); // 2.11, now called on ConsoleStart and UiStart
				return true;
			}
		}

		public virtual bool OnInit2()
		{
			//Software.Checking(); // TOCLEAN, moved in 2.13.4

			Start();

			//LoggedUpdate(); // TOCLEAN, removed in 2.13.4

			return true;
		}

		public virtual void OnDeInit()
		{
			SessionStop();

			WaitMessageSet(Messages.AppExiting, false);

			if (m_threadManifest != null)
				m_threadManifest.RequestStopSync();

			if (m_threadPenalities != null)
				m_threadPenalities.RequestStopSync();

			if (m_threadDiscover != null)
				m_threadDiscover.RequestStopSync();

			if (m_threadPinger != null)
				m_threadPinger.RequestStopSync();

			m_networkLockManager.Deactivation(true);
			m_networkLockManager = null;

			TemporaryFiles.Clean();

			Platform.Instance.OnCheckSingleInstanceClear();

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

			if (m_webServer != null)
				m_webServer.Stop();

			if (m_tcpServer != null)
				m_tcpServer.Stop();
		}

		public virtual void OnWork()
		{
			ActionService currentAction = ActionService.None;

			lock (m_actionsList)
			{
				if (m_actionsList.Count > 0)
				{
					currentAction = m_actionsList[0];
					m_actionsList.RemoveAt(0);
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
			if (m_tickDeltaUiRefreshFull.Elapsed(1000))
			{
				if (m_serversInfoUpdated)
				{
					m_serversInfoUpdated = false;
					OnRefreshUi(RefreshUiMode.Full);
				}

				if (m_areasInfoUpdated)
				{
					m_areasInfoUpdated = false;
					RecomputeAreas();
				}
			}

			Sleep(100);
		}

		public virtual void OnConsoleKey(ConsoleKeyInfo key)
		{
			if (Engine.Storage.Get("console.mode") == "keys")
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
					if ((Engine.CanConnect()) && (Engine.IsConnected() == false) && (Engine.IsWaiting() == false))
						Connect();
				}
			}
		}

		public virtual void OnConsoleBreak()
		{
			Logs.Log(LogType.Info, Messages.ConsoleKeyBreak);

			OnExit();
		}

		public virtual XmlItem OnCommand(XmlItem xml, bool ignoreIfNotExists)
		{
			XmlItem ret = new XmlItem();

			string action = xml.GetAttribute("action").ToLowerInvariant();
			if (action == "exit")
			{
				OnExit();
			}
			else if (action == "openvpn")
			{
				SendManagementCommand(xml.GetAttribute("command"));
			}
			else if (action == "ui.show.text")
			{
				OnShowText(xml.GetAttribute("title", ""), xml.GetAttribute("body", ""));
			}
			else if (action == "ui.show.url")
			{
				Platform.Instance.OpenUrl(xml.GetAttribute("url"));
			}
			else if (action == "ui.get.license") // Eddie3
			{
				ret.SetAttribute("body", ResourcesFiles.GetString("license.txt"));
			}
			else if (action == "ui.show.license")
			{
				XmlItem xml2 = new XmlItem("command");
				xml2.SetAttribute("action", "ui.show.text");
				xml2.SetAttribute("title", "License");
				xml2.SetAttribute("body", ResourcesFiles.GetString("license.txt"));
				Command(xml2);
			}
			else if (action == "ui.show.libraries")
			{
				XmlItem xml2 = new XmlItem("command");
				xml2.SetAttribute("action", "ui.show.text");
				xml2.SetAttribute("title", "Libraries and Tools");
				xml2.SetAttribute("body", ResourcesFiles.GetString("thirdparty.txt"));
				Command(xml2);
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
			else if (action == "ui.stats.vpngeneratedovpn")
			{
				if ((IsConnected()) && (ConnectedOVPN != null))
				{
					OnShowText(Messages.StatsVpnGeneratedOVPN, ConnectedOVPN.Get());
				}
			}
			else if (action == "ui.stats.pinger")
			{
				InvalidateConnections();
			}
			else if (action == "ui.stats.manifestlastupdate")
			{
				RefreshConnections();
			}
			else if (action == "ui.stats.pathapp")
			{
				Platform.Instance.OpenDirectoryInFileManager(Stats.Get("PathApp").Value);
			}
			else if (action == "ui.stats.pathprofile")
			{
				Platform.Instance.OpenDirectoryInFileManager(Stats.Get("PathProfile").Value);
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
			else if (action == "ui.hello")
			{
				Engine.Instance.Command("ui.show.ready");
			}
			else if (action == "ui.start")
			{
				UiSendStartInfo();
				UiSendOsInfo();
				UiSendStatusInfo();
				UiSendQuickInfo();
			}
			else
			{
				if (ignoreIfNotExists == false)
					throw new Exception(MessagesFormatter.Format(Messages.CommandUnknown, xml.ToString()));
			}

			return ret;
		}

		public virtual void OnSettingsChanged()
		{
			OnCheckConnections();

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

		public virtual void OnSignal(string signal)
		{
			Engine.Instance.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.ReceivedOsSignal, signal));
			m_breakRequests++;
			OnExit();
		}

		public virtual void OnExitRejected()
		{
			m_breakRequests = 0;
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

			string login = Storage.Get("login").Trim();
			string password = Storage.Get("password").Trim();

			if ((login == "") || (password == ""))
			{
				Engine.Instance.Logs.Log(LogType.Fatal, Messages.ConsoleHelp);
				return false;
			}

			if (Storage.Get("console.mode") == "keys")
				Engine.Instance.Logs.Log(LogType.Info, Messages.ConsoleKeyboardHelp);

			if (Storage.GetBool("connect") == false)
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

		public bool AskExitConfirm()
		{
			if (m_breakRequests > 0)
				return false;

			if (Engine.Storage.GetBool("gui.exit_confirm") == true)
				return true;

			return false;
		}

		public void Connect()
		{
			lock (m_actionsList)
			{
				m_actionsList.Add(Engine.ActionService.Connect);
			}
		}

		public void Login()
		{
			lock (m_actionsList)
			{
				m_actionsList.Add(Engine.ActionService.Login);
			}
		}

		public void Logout()
		{
			lock (m_actionsList)
			{
				m_actionsList.Add(Engine.ActionService.Logout);
			}
		}

		public void NetLockIn()
		{
			lock (m_actionsList)
			{
				m_actionsList.Add(Engine.ActionService.NetLockIn);
			}
		}

		public void NetLockOut()
		{
			lock (m_actionsList)
			{
				m_actionsList.Add(Engine.ActionService.NetLockOut);
			}
		}

		public void Disconnect()
		{
			lock (m_actionsList)
			{
				m_actionsList.Add(Engine.ActionService.Disconnect);
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

		public void GenerateSystemReport()
		{
			Report report = new Report();
			report.Start();
		}

		public void SetConnected(bool connected)
		{
			lock (this)
			{
				if ((connected == true) && (m_connected == false))
				{
					// OnConnected
					m_connected = true;
					ConnectedSince = DateTime.UtcNow;

					WaitMessageClear();

					OnRefreshUi(RefreshUiMode.Full);
				}

				if ((connected == false) && (m_connected == true))
				{
					// OnDisconnected
					m_connected = false;
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
				if (m_connected)
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
			return m_connected;
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
			if (m_threadSession == null)
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

			if (l.Type != LogType.Realtime)
			{
				string lines = l.GetStringLines().Trim();

				if (Storage != null)
				{
					lock (Storage)
					{
						if (Storage.Get("console.mode") == "batch")
							Console.WriteLine(lines);
						if (Storage.Get("console.mode") == "keys")
							Console.WriteLine(lines);

						if (Storage.GetBool("log.file.enabled"))
						{
							try
							{
								string logPath = Storage.Get("log.file.path").Trim();
								Encoding encoding = Storage.GetEncoding("log.file.encoding");

								List<string> paths = Logs.ParseLogFilePath(logPath);
								foreach (string path in paths)
								{
									Directory.CreateDirectory(Path.GetDirectoryName(path));
									string text = Platform.Instance.NormalizeString(lines + "\n");
									Platform.Instance.FileContentsAppendText(path, text, encoding);
								}
							}
							catch (Exception e)
							{
								Logs.Log(LogType.Warning, MessagesFormatter.Format("Log to file disabled due to error, {1}", e.Message));
								Storage.SetBool("log.file.enabled", false);
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
		
		public virtual void OnShowText(string title, string data)
		{
		}

		public virtual bool OnAskYesNo(string message)
		{
			return true;
		}

		public virtual Credentials OnAskCredentials()
		{
			return null;
		}

		public virtual void OnSystemReport(string step, string text, int perc)
		{

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
					if ((msg != "") && (m_frontMessages.Contains(msg) == false))
					{
						OnFrontMessage(msg);
						m_frontMessages.Add(msg);
					}
				}
			}

			lock (m_connections)
			{
				foreach (ConnectionInfo infoServer in m_connections.Values)
				{
					infoServer.Deleted = true;
				}

				foreach (Provider provider in ProvidersManager.Providers)
				{
					if (provider.Enabled)
					{
						provider.OnBuildConnections();
					}
				}

				for (;;)
				{
					bool restart = false;
					foreach (ConnectionInfo infoServer in m_connections.Values)
					{
						if (infoServer.Deleted)
						{
							m_connections.Remove(infoServer.Code);
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
				foreach (ConnectionInfo infoConnection in m_connections.Values)
				{
					string code = infoConnection.Code;
					string displayName = infoConnection.DisplayName;

					/*
					if (serversWhiteList.Contains(code))
						infoServer.UserList = ServerInfo.UserListType.WhiteList;
					else if (serversBlackList.Contains(code))
						infoServer.UserList = ServerInfo.UserListType.BlackList;
					else
						infoServer.UserList = ServerInfo.UserListType.None;
					*/
					// 2.11.5
					if (serversWhiteList.Contains(code))
						infoConnection.UserList = ConnectionInfo.UserListType.WhiteList;
					else if (serversWhiteList.Contains(displayName))
						infoConnection.UserList = ConnectionInfo.UserListType.WhiteList;
					else if (serversBlackList.Contains(code))
						infoConnection.UserList = ConnectionInfo.UserListType.BlackList;
					else if (serversBlackList.Contains(displayName))
						infoConnection.UserList = ConnectionInfo.UserListType.BlackList;
					else
						infoConnection.UserList = ConnectionInfo.UserListType.None;
				}

				RecomputeAreas();

				OnCheckConnections();
			}

			m_threadDiscover.CheckNow();
		}

		public virtual void OnCheckConnections()
		{
			lock (m_connections)
			{
				foreach (ConnectionInfo connection in m_connections.Values)
				{
					connection.Warnings.Clear();
					if (connection.WarningOpen != "")
						connection.WarningAdd(connection.WarningOpen, ConnectionInfoWarning.WarningType.Warning);
					if (connection.WarningClosed != "")
						connection.WarningAdd(connection.WarningClosed, ConnectionInfoWarning.WarningType.Error);

					if ((Engine.Instance.Storage.Get("protocol.ip.entry") == "ipv6-only") && (connection.IpsEntry.CountIPv6 == 0))
						connection.WarningAdd(Messages.ConnectionWarningNoEntryIPv6, ConnectionInfoWarning.WarningType.Error);
					if ((Engine.Instance.Storage.Get("protocol.ip.entry") == "ipv4-only") && (connection.IpsEntry.CountIPv4 == 0))
						connection.WarningAdd(Messages.ConnectionWarningNoEntryIPv4, ConnectionInfoWarning.WarningType.Error);
					if ((Engine.Instance.Storage.Get("protocol.ipv4.route") == "in-always") && (connection.SupportIPv4 == false))
						connection.WarningAdd(Messages.ConnectionWarningNoExitIPv4, ConnectionInfoWarning.WarningType.Error);
					if ((Engine.Instance.Storage.Get("protocol.ipv6.route") == "in-always") && (connection.SupportIPv6 == false))
						connection.WarningAdd(Messages.ConnectionWarningNoExitIPv6, ConnectionInfoWarning.WarningType.Error);

					OvpnBuilder ovpn = connection.BuildOVPN(true);

					if (ovpn != null)
						if ((ovpn.ExistsDirective("<tls-crypt>")) && (Software.GetTool("openvpn").VersionUnder("2.4")))
							connection.WarningAdd(Messages.ConnectionWarningOlderOpenVpnTlsCrypt, ConnectionInfoWarning.WarningType.Error);
				}

				foreach (Provider provider in ProvidersManager.Providers)
				{
					if (provider.Enabled)
						provider.OnCheckConnections();
				}
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

					Stats.UpdateValue("SessionTotalDownload", Core.Utils.FormatBytes(Engine.ConnectedSessionStatsRead, false, true));
					Stats.UpdateValue("SessionTotalUpload", Core.Utils.FormatBytes(Engine.ConnectedSessionStatsWrite, false, true));

					Stats.UpdateValue("VpnTotalDownload", Core.Utils.FormatBytes(Engine.ConnectedVpnStatsRead, false, true));
					Stats.UpdateValue("VpnTotalUpload", Core.Utils.FormatBytes(Engine.ConnectedVpnStatsWrite, false, true));

					Stats.UpdateValue("VpnSpeedDownload", Core.Utils.FormatBytes(Engine.ConnectedLastDownloadStep, true, true));
					Stats.UpdateValue("VpnSpeedUpload", Core.Utils.FormatBytes(Engine.ConnectedLastUploadStep, true, true));
				}
				else
				{
					Stats.UpdateValue("VpnConnectionStart", Messages.StatsNotConnected);

					Stats.UpdateValue("SessionTotalDownload", Messages.StatsNotConnected);
					Stats.UpdateValue("SessionTotalUpload", Messages.StatsNotConnected);

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
					if (Core.Threads.Manifest.Instance.Refresh != Threads.RefreshType.None)
						manifestLastUpdate += " (" + Messages.ManifestUpdateForce + ")";
				Stats.UpdateValue("ManifestLastUpdate", manifestLastUpdate);

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

					Stats.UpdateValue("VpnEntryIP", Engine.ConnectedEntryIP.ToString());
					if (Engine.ConnectedExitIP.CountIPv4 != 0)
						Stats.UpdateValue("VpnExitIPv4", Engine.ConnectedExitIP.OnlyIPv4.ToString());
					else
						Stats.UpdateValue("VpnExitIPv4", Messages.NotAvailable);
					if (Engine.ConnectedExitIP.CountIPv6 != 0)
						Stats.UpdateValue("VpnExitIPv6", Engine.ConnectedExitIP.OnlyIPv6.ToString());
					else
						Stats.UpdateValue("VpnExitIPv6", Messages.NotAvailable);
					Stats.UpdateValue("VpnProtocol", Engine.ConnectedProtocol);
					Stats.UpdateValue("VpnPort", Engine.ConnectedPort.ToString());
					if (Engine.ConnectedRealIp != "")
						Stats.UpdateValue("VpnRealIp", Engine.ConnectedRealIp);
					else if (Engine.CurrentServer.SupportCheck)
						Stats.UpdateValue("VpnRealIp", Messages.CheckingRequired);
					else
						Stats.UpdateValue("VpnRealIp", Messages.NotAvailable);
					Stats.UpdateValue("VpnIp", Engine.ConnectedOVPN.ExtractVpnIPs().ToString());
					Stats.UpdateValue("VpnDns", Engine.ConnectedOVPN.ExtractDns().ToString());
					Stats.UpdateValue("VpnInterface", Engine.ConnectedVpnInterfaceName);
					Stats.UpdateValue("VpnGateway", Engine.ConnectedOVPN.ExtractGateway().ToString());
					Stats.UpdateValue("VpnCipher", Engine.ConnectedOVPN.ExtractCipher());
					Stats.UpdateValue("VpnControlChannel", Engine.ConnectedControlChannel);
					Stats.UpdateValue("VpnGeneratedOVPN", "");

					if (Engine.ConnectedServerTime != 0)
						Stats.UpdateValue("SystemTimeServerDifference", (Engine.ConnectedServerTime - Engine.ConnectedClientTime).ToString() + " seconds");
					else if (Engine.CurrentServer.SupportCheck)
						Stats.UpdateValue("SystemTimeServerDifference", Messages.CheckingRequired);
					else
						Stats.UpdateValue("SystemTimeServerDifference", Messages.NotAvailable);
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

					Stats.UpdateValue("VpnEntryIP", Messages.StatsNotConnected);
					Stats.UpdateValue("VpnExitIPv4", Messages.StatsNotConnected);
					Stats.UpdateValue("VpnExitIPv6", Messages.StatsNotConnected);
					Stats.UpdateValue("VpnProtocol", Messages.StatsNotConnected);
					Stats.UpdateValue("VpnPort", Messages.StatsNotConnected);
					Stats.UpdateValue("VpnRealIp", Messages.StatsNotConnected);
					Stats.UpdateValue("VpnIp", Messages.StatsNotConnected);
					Stats.UpdateValue("VpnDns", Messages.StatsNotConnected);
					Stats.UpdateValue("VpnInterface", Messages.StatsNotConnected);
					Stats.UpdateValue("VpnGateway", Messages.StatsNotConnected);
					Stats.UpdateValue("VpnCipher", Messages.StatsNotConnected);
					Stats.UpdateValue("VpnControlChannel", Messages.StatsNotConnected);
					Stats.UpdateValue("VpnGeneratedOVPN", Messages.StatsNotConnected);
					Stats.UpdateValue("SystemTimeServerDifference", Messages.StatsNotConnected);
				}
			}

			if (mode == RefreshUiMode.Stats)
			{
				UiSendQuickInfo();
			}

			if (mode == RefreshUiMode.Full)
			{
				UiSendStatusInfo();
			}

			if (mode == RefreshUiMode.MainMessage)
			{
				UiSendStatusInfo();
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

			if (ignore == false)
				Logs.Log(LogType.Fatal, Messages.UnhandledException + " - " + e.Message + " - " + e.StackTrace.ToString());
		}

		// ----------------------------------------------------
		// Misc
		// ----------------------------------------------------

		public ConnectionInfo GetConnectionInfo(string code, Provider provider)
		{
			ConnectionInfo infoConnection = null;
			if (m_connections.ContainsKey(code))
				infoConnection = m_connections[code];
			if (infoConnection == null)
			{
				// Create
				infoConnection = new ConnectionInfo();
				infoConnection.Provider = provider;
				infoConnection.Code = code;
				m_connections[code] = infoConnection;
			}
			infoConnection.Deleted = false;
			return infoConnection;
		}

		// Return the list of selected servers, based on settings, filter, ordered by score.
		// Filter can be "earth","europe","nl","castor".
		public List<ConnectionInfo> GetConnections(bool all)
		{
			List<ConnectionInfo> list = new List<ConnectionInfo>();

			lock (m_connections)
			{
				bool existsWhiteListAreas = false;
				bool existsWhiteListServers = false;
				foreach (ConnectionInfo server in m_connections.Values)
				{
					if (server.UserList == ConnectionInfo.UserListType.WhiteList)
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

				foreach (ConnectionInfo server in m_connections.Values)
				{
					bool skip = false;

					if (all == false)
					{
						ConnectionInfo.UserListType serverUserList = server.UserList;
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

						if (serverUserList == ConnectionInfo.UserListType.BlackList)
						{
							skip = true;
						}
						else if (serverUserList == ConnectionInfo.UserListType.WhiteList)
						{
							skip = false;
						}
						else
						{
							if (countryUserList == AreaInfo.UserListType.BlackList)
							{
								skip = true;
							}
							else if ((existsWhiteListServers) && (serverUserList == ConnectionInfo.UserListType.None))
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

		public ConnectionInfo PickConnectionByName(string name)
		{
			lock (m_connections)
			{
				foreach (ConnectionInfo s in m_connections.Values)
				{
					if (s.DisplayName == name)
						return s;
				}

				Engine.Instance.Logs.Log(LogType.Fatal, MessagesFormatter.Format(Messages.ServerByNameNotFound, name));
				return null;
			}
		}

		public ConnectionInfo PickConnection()
		{
			return PickConnection("");
		}

		public ConnectionInfo PickConnection(string preferred)
		{
			lock (m_connections)
			{
				if (preferred != "")
				{
					if (m_connections.ContainsKey(preferred))
						return m_connections[preferred];
				}

				List<ConnectionInfo> list = GetConnections(false);
				if (list.Count > 0)
					return list[0];
			}

			return null;
		}

		public void RefreshConnections()
		{
			// Refresh each provider
			Core.Threads.Manifest.Instance.Refresh = Threads.RefreshType.Refresh;
		}

		public void RefreshInvalidateConnections()
		{
			// Refresh each provider AND invalidate all ping and discovery info
			Core.Threads.Manifest.Instance.Refresh = Threads.RefreshType.RefreshInvalidate;
		}

		public void InvalidateConnections()
		{
			m_threadPinger.InvalidateAll();
			m_threadDiscover.InvalidateAll();

			OnRefreshUi(Core.Engine.RefreshUiMode.Full);
		}

		public string WaitManifestUpdate()
		{
			m_threadManifest.Refresh = Threads.RefreshType.Refresh;
			m_threadManifest.Updated.WaitOne();
			return m_threadManifest.GetLastResult();
		}

		public void PostManifestUpdate()
		{
			LoggedUpdate(); // ClodoTemp. Start with AirVPN disabled, enable it, crash with a key='Default' without this. // TOFIX

			OnPostManifestUpdate();

			OnRefreshUi(Core.Engine.RefreshUiMode.Full);

			if (m_networkLockManager != null)
				m_networkLockManager.OnUpdateIps();
		}

		public void LoggedUpdate()
		{
			XmlDocument xmlDoc = new XmlDocument();
			XmlElement xmlKeys = xmlDoc.CreateElement("keys");

			if ((AirVPN != null) && (AirVPN.User != null))
			{
				string key = Engine.Instance.Storage.Get("key");
				string firstKey = "";
				bool found = false;
				foreach (XmlElement xmlKey in AirVPN.User.SelectNodes("keys/key"))
				{
					if (key == xmlKey.GetAttribute("name"))
					{
						found = true;
						break;
					}
					if (firstKey == "")
						firstKey = xmlKey.GetAttribute("name");
				}
				if (found == false)
					Engine.Instance.Storage.Set("key", firstKey);
			}

			if ((AirVPN != null) && (AirVPN.User != null))
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
				string message = MessagesFormatter.Format(Messages.AppEvent, name);
				WaitMessageSet(message, false);
				Logs.Log(LogType.Info, message);

				//Log(LogType.Verbose, "Start Running event '" + name + "', Command: '" + filename + "', Arguments: '" + arguments + "'");
				SystemShell.ShellUserEvent(filename, arguments, waitEnd);
				//Log(LogType.Verbose, "End Running event '" + name + "', Command: '" + filename + "', Arguments: '" + arguments + "'");
			}
		}

		public void SendManagementCommand(string command)
		{
			if (m_threadSession != null)
				m_threadSession.SendManagementCommand(command);
		}

		public XmlDocument FetchUrlXml(HttpRequest request)
		{
			HttpResponse response = FetchUrl(request);
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(response.GetAscii());
			return doc;
		}

		public HttpResponse FetchUrl(HttpRequest request)
		{
			Tools.Curl curl = Software.GetTool("curl") as Tools.Curl;

			return curl.Fetch(request);
		}

		public int PingerInvalid()
		{
			return m_threadPinger.GetStats().Invalid;
		}

		public PingerStats PingerStats()
		{
			return m_threadPinger.GetStats();
		}

		public string GenerateFileHeader()
		{
			return MessagesFormatter.Format(Messages.GeneratedFileHeader, Constants.VersionDesc);
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
			Command(cmd, true);
		}

		public XmlItem Command(string cmd, bool ignoreIfNotExists)
		{
			return Command(new XmlItem(cmd), ignoreIfNotExists);
		}

		public XmlItem Command(XmlItem xml)
		{
			return Command(xml, true);
		}

		public XmlItem Command(XmlItem xml, bool ignoreIfNotExists)
		{
			try
			{
				if ((Engine.Storage != null) && (Engine.Storage.Get("console.mode") == "backend"))
					Console.WriteLine(xml.ToString());

				if (CommandEvent != null)
					CommandEvent(xml);

				return OnCommand(xml, ignoreIfNotExists);
			}
			catch (Exception e)
			{
				Logs.Log(LogType.Error, e);
				return null;
			}
		}

		private void Auth()
		{
			if (AirVPN == null)
				return;

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

					OnCheckConnections();

					OnRefreshUi(RefreshUiMode.Full);
				}
			}
			catch (Exception e)
			{
				Logs.Log(LogType.Fatal, MessagesFormatter.Format(Messages.AuthorizeLoginFailed, e.Message));
			}

			SaveSettings(); // 2.8

			Engine.Instance.WaitMessageClear();
		}

		private void DeAuth()
		{
			if (AirVPN == null)
				return;

			if (IsLogged())
			{
				Engine.Instance.WaitMessageSet(Messages.AuthorizeLogout, false);

				AirVPN.DeAuth();

				OnCheckConnections();

				OnRefreshUi(RefreshUiMode.Full);

				Logs.Log(LogType.InfoImportant, Messages.AuthorizeLogoutDone);

				Engine.Instance.WaitMessageClear();

				SaveSettings(); // 2.8
			}
		}

		public void ReAuth()
		{
			if (Engine.Instance.IsLogged())
			{
				DeAuth();
				Auth();
			}
		}

		public bool CanConnect()
		{
			lock (m_connections)
				foreach (ConnectionInfo server in m_connections.Values)
				{
					if (server.CanConnect())
						return true;
				}
			return false;
		}

		private void SessionStart()
		{
			try
			{
				Logs.Log(LogType.Info, Messages.SessionStart);

				Engine.Instance.WaitMessageSet(Messages.CheckingEnvironment, true);

				if (CheckEnvironmentSession() == false)
				{
					WaitMessageClear();
				}
				else
				{
					// Check Driver
					if (Engine.Instance.Storage.GetBool("advanced.skip_tun_detect") == false)
					{
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
							NextServer = Engine.PickConnection(Engine.Storage.Get("servers.last"));
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
					m_threadSession.SetReset("CANCEL"); // New 2.11.10
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

				foreach (ConnectionInfo server in m_connections.Values)
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
			List<string> connectionsWhiteList = new List<string>();
			List<string> connectionsBlackList = new List<string>();
			foreach (ConnectionInfo info in m_connections.Values)
				if (info.UserList == ConnectionInfo.UserListType.WhiteList)
					connectionsWhiteList.Add(info.Code);
				else if (info.UserList == ConnectionInfo.UserListType.BlackList)
					connectionsBlackList.Add(info.Code);
			Storage.SetList("servers.whitelist", connectionsWhiteList);
			Storage.SetList("servers.blacklist", connectionsBlackList);

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

		public bool CheckEnvironmentApp()
		{
			if(Platform.Instance.HasAccessToWrite(Storage.GetDataPath()) == false)
				Engine.Instance.Logs.Log(LogType.Fatal, "Unable to write in path '" + Storage.GetDataPath() + "'");

			// Local Time in the past
			if (DateTime.UtcNow < Constants.dateForPastChecking)
				Engine.Instance.Logs.Log(LogType.Fatal, Messages.WarningLocalTimeInPast);

			return Platform.Instance.OnCheckEnvironmentApp();
		}

		public bool CheckEnvironmentSession()
		{
			Software.ExceptionForRequired();

			if (Platform.Instance.HasAccessToWrite(Storage.GetDataPath()) == false)
				throw new Exception("Unable to write in path '" + Storage.GetDataPath() + "'");

			string protocol = Storage.Get("mode.protocol").ToUpperInvariant();

			if ((protocol != "AUTO") && (protocol != "UDP") && (protocol != "TCP") && (protocol != "SSH") && (protocol != "SSL"))
				throw new Exception(Messages.CheckingProtocolUnknown);

			// TOCLEAN : Must be moved at provider level
			if ((protocol == "SSH") && (Software.GetTool("ssh").Available() == false))
				throw new Exception("SSH " + Messages.NotFound);

			if ((protocol == "SSL") && (Software.GetTool("ssl").Available() == false))
				throw new Exception("SSL " + Messages.NotFound);

			if (Storage.GetBool("advanced.skip_alreadyrun") == false)
			{
				Dictionary<int, string> processes = Platform.Instance.GetProcessesList();

				foreach (string cmd in processes.Values)
				{
					if (cmd.StartsWith(Software.GetTool("openvpn").Path))
						throw new Exception(MessagesFormatter.Format(Messages.AlreadyRunningOpenVPN, cmd));

					if ((protocol == "SSL") && (cmd.StartsWith(Software.GetTool("ssl").Path)))
					{
						throw new Exception(MessagesFormatter.Format(Messages.AlreadyRunningSTunnel, cmd));
					}

					if ((protocol == "SSH") && (cmd.StartsWith(Software.GetTool("ssh").Path)))
						throw new Exception(MessagesFormatter.Format(Messages.AlreadyRunningSsh, cmd));
				}
			}

			if( (Storage.GetLower("proxy.mode") != "none") && (Storage.GetLower("proxy.when") != "none") )
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

			if (Engine.Instance.Storage.GetBool("routes.remove_default"))
				Logs.Log(LogType.Warning, Messages.DeprecatedRemoveDefaultGateway);

			return Platform.Instance.OnCheckEnvironmentSession();
		}

		public IpAddresses DiscoverExit()
		{
			return m_threadDiscover.DiscoverExit();
		}

		public void LogOpenvpnConfig()
		{
			string t = "-- Start OpenVPN config dump\n" + Engine.Instance.ConnectedOVPN + "\n-- End OpenVPN config dump";
			t = Regex.Replace(t, "<ca>(.*?)</ca>", "<ca>omissis</ca>", RegexOptions.Singleline);
			t = Regex.Replace(t, "<key>(.*?)</key>", "<key>omissis</key>", RegexOptions.Singleline);
			t = Regex.Replace(t, "<cert>(.*?)</cert>", "<cert>omissis</cert>", RegexOptions.Singleline);
			t = Regex.Replace(t, "<tls-auth>(.*?)</tls-auth>", "<tls-auth>omissis</tls-auth>", RegexOptions.Singleline);
			t = Regex.Replace(t, "<tls-crypt>(.*?)</tls-crypt>", "<tls-auth>omissis</tls-auth>", RegexOptions.Singleline);
			Engine.Logs.Log(LogType.Verbose, t);
		}

		public string GetConnectedTrayText(bool speed, bool server)
		{
			if (CurrentServer == null)
				return "";

			string text = "";

			if (speed)
			{
				text += "D:" + Core.Utils.FormatBytes(ConnectedLastDownloadStep, true, false) + " U:" + Core.Utils.FormatBytes(ConnectedLastUploadStep, true, false);
			}
			if (server)
			{
				if (text != "")
					text += " - ";
				string serverName = CurrentServer.DisplayName;
				string country = CountriesManager.GetNameFromCode(CurrentServer.CountryCode);
				if (country != "")
					serverName += " (" + country + ")";
				text += serverName;
				if (ConnectedExitIP.Count != 0)
					text += " - IP:" + ConnectedExitIP.ToString();
			}
			return text;
		}

		public void UiSendStartInfo()
		{
			// Data sended at connection, never changed
			XmlItem xml = new XmlItem("command");
			xml.SetAttribute("action", "ui.info.start");

			xml.SetAttribute("eddie.version.text", Constants.VersionDesc);
			xml.SetAttributeInt("eddie.version.int", Constants.VersionInt);
			xml.SetAttributeBool("eddie.development", DevelopmentEnvironment);
			xml.SetAttribute("mono.version", Platform.Instance.GetMonoVersion());
			xml.SetAttribute("os.code", Platform.Instance.GetSystemCode());
			xml.SetAttribute("os.name", Platform.Instance.GetName());

			//XmlItem xmlOptions = xml.CreateChild("options");

			Command(xml);
		}

		public void UiSendOsInfo()
		{
			// Data sended at connection, never changed, at least until options changes.
			XmlItem xml = new XmlItem("command");
			xml.SetAttribute("action", "ui.info.os");

			xml.SetAttribute("eddie.windows.tap.driver", Constants.WindowsDriverVersion);
			xml.SetAttribute("eddie.windows-xp.tap.driver", Constants.WindowsXpDriverVersion);
			xml.SetAttribute("tools.tap-driver.version", Software.OpenVpnDriver);
			xml.SetAttribute("tools.curl.version", Software.GetTool("curl").Version);
			xml.SetAttribute("tools.curl.path", Software.GetTool("curl").Path);
			xml.SetAttribute("tools.openvpn.version", Software.GetTool("openvpn").Version);
			xml.SetAttribute("tools.openvpn.path", Software.GetTool("openvpn").Path);
			xml.SetAttribute("tools.ssh.version", Software.GetTool("ssh").Version);
			xml.SetAttribute("tools.ssh.path", Software.GetTool("ssh").Path);
			xml.SetAttribute("tools.ssl.version", Software.GetTool("ssl").Version);
			xml.SetAttribute("tools.ssl.path", Software.GetTool("ssl").Path);

			Command(xml);
		}

		public void UiSendQuickInfo()
		{
			// Data sended every second
			/*
			XmlItem xml = new XmlItem("command");
			xml.SetAttribute("action", "ui.info.os");

			xml.SetAttributeInt64("stats.vpn.download.bytes", Engine.ConnectedLastDownloadStep);
			xml.SetAttribute("stats.vpn.download.text", Core.Utils.FormatBytes(Engine.ConnectedLastDownloadStep, true, false));
			xml.SetAttributeInt64("stats.vpn.upload.bytes", Engine.ConnectedLastUploadStep);
			xml.SetAttribute("stats.vpn.upload.text", Core.Utils.FormatBytes(Engine.ConnectedLastUploadStep, true, false));

			xml.SetAttribute("status.text", Engine.Instance.GetConnectedTrayText(true, true));

			Command(xml);
			*/
		}

		public void UiSendStatusInfo()
		{
			// Data sended in case of big event, like connect/disconnect.
			XmlItem xml = new XmlItem("command");
			xml.SetAttribute("action", "ui.info.status");
			xml.SetAttributeBool("waiting", IsWaiting());
			xml.SetAttribute("waiting.message", WaitMessage);
			xml.SetAttributeBool("connected", IsConnected());
			xml.SetAttributeBool("netlock", ((Engine.Instance.NetworkLockManager != null) && (Engine.Instance.NetworkLockManager.IsActive())));
			if (CurrentServer != null)
			{
				xml.SetAttribute("server.country.code", CurrentServer.CountryCode);
				xml.SetAttribute("server.display-name", CurrentServer.DisplayName);
			}
			Command(xml);
		}
	}
}
