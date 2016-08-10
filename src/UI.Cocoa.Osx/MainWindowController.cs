// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2016 AirVPN (support@airvpn.org) / https://airvpn.org )
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
using System.Globalization;
using System.Linq;
using System.Xml;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Eddie.Core;

namespace Eddie.UI.Osx
{
	public partial class MainWindowController : MonoMac.AppKit.NSWindowController
	{
		public TableServersController TableServersController;
		public TableAreasController TableAreasController;
		public TableLogsController TableLogsController;
		public TableStatsController TableStatsController;

		public bool ShutdownConfirmed = false;

		public NSStatusItem StatusItem;

		private WindowAboutController windowAbout;
		private WindowPreferencesController windowPreferences;

		#region Constructors
		// Called when created from unmanaged code
		public MainWindowController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public MainWindowController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		// Call to load from the XIB/NIB file
		public MainWindowController () : base ("MainWindow")
		{
			Initialize ();
		}	
		// Shared initialization code
		void Initialize ()
		{
		}
		#endregion
		//strongly typed window accessor
		public new MainWindow Window {
			get {
				return (MainWindow)base.Window;
			}
		}

		
		public UI.Osx.Engine Engine
		{
			get {
				return Core.Engine.Instance as UI.Osx.Engine;
			}
		}
			
		public override void AwakeFromNib()
		{
			base.AwakeFromNib ();

			Window.Delegate = new MainWindowDelegate (this);

			Window.AcceptsMouseMovedEvents = true;

			CreateMenuBarIcon ();

			ChkRemember.State = Engine.Storage.GetBool("remember") ? NSCellStateValue.On : NSCellStateValue.Off; 
			ChkServersShowAll.State = NSCellStateValue.Off;
			GuiUtils.SetCheck (ChkServersLockCurrent, Engine.Storage.GetBool ("servers.locklast"));
			GuiUtils.SetSelected (CboServersScoringRule, Engine.Storage.Get ("servers.scoretype"));

			CboSpeedResolutions.RemoveAllItems ();
			CboSpeedResolutions.AddItem (Messages.WindowsMainSpeedResolution1);
			CboSpeedResolutions.AddItem (Messages.WindowsMainSpeedResolution2);
			CboSpeedResolutions.AddItem (Messages.WindowsMainSpeedResolution3);
			CboSpeedResolutions.AddItem (Messages.WindowsMainSpeedResolution4);
			CboSpeedResolutions.AddItem (Messages.WindowsMainSpeedResolution5);
			CboSpeedResolutions.SelectItem (0);

			CmdConnect.Title = Messages.CommandConnect;
			LblConnect.StringValue = Messages.CommandConnectSubtitle;
			CmdDisconnect.Title = Messages.CommandDisconnect;
			CmdCancel.Title = Messages.CommandCancel;

			CboServersScoringRule.ToolTip = Messages.TooltipServersScoreType;
			ChkServersLockCurrent.ToolTip = Messages.TooltipServersLockCurrent;
			ChkServersShowAll.ToolTip = Messages.TooltipServersShowAll;
			CmdServersConnect.ToolTip = Messages.TooltipServersConnect;
			CmdServersUndefined.ToolTip = Messages.TooltipServersUndefined;
			CmdServersBlackList.ToolTip = Messages.TooltipServersBlackList;
			CmdServersWhiteList.ToolTip = Messages.TooltipServersWhiteList;
			CmdAreasUndefined.ToolTip = Messages.TooltipAreasUndefined;
			CmdAreasBlackList.ToolTip = Messages.TooltipAreasBlackList;
			CmdAreasWhiteList.ToolTip = Messages.TooltipAreasWhiteList;
			CmdLogsOpenVpnManagement.ToolTip = Messages.TooltipLogsOpenVpnManagement;
			CmdLogsClean.ToolTip = Messages.TooltipLogsClean;
			CmdLogsCopy.ToolTip = Messages.TooltipLogsCopy;
			CmdLogsSave.ToolTip = Messages.TooltipLogsSave;
			CmdLogsSupport.ToolTip = Messages.TooltipLogsSupport;

			if (Engine.Storage.GetBool ("remember")) {
				ChkRemember.State = NSCellStateValue.On;
				TxtLogin.StringValue = Engine.Storage.Get ("login");
				TxtPassword.StringValue = Engine.Storage.Get ("password");

			}



			ChkRemember.Activated += (object sender, EventArgs e) =>
			{
				Engine.Storage.SetBool ("remember", ChkRemember.State == NSCellStateValue.On);
			};

			CmdLogin.Activated += (object sender, EventArgs e) =>
			{
				if(Engine.IsLogged() == false)
					Login();
				else
					Logout();
			};	

			TxtLogin.Activated += (object sender, EventArgs e) => {
				EnabledUI();
			};

			TxtPassword.Activated += (object sender, EventArgs e) => {
				EnabledUI();
			};

			CboKey.Activated += (object sender, EventArgs e) => {
				Engine.Instance.Storage.Set("key", CboKey.SelectedItem.Title);
			};

			CmdConnect.Activated += (object sender, EventArgs e) =>
			{
				Connect();
			};

			CmdDisconnect.Activated += (object sender, EventArgs e) => {
				Disconnect();
			};

			CmdCancel.Activated += (object sender, EventArgs e) => {				
				Disconnect ();
			};

			CmdNetworkLock.Activated += (object sender, EventArgs e) => {
				if(Engine.Instance.NetworkLockManager.IsActive())
				{
					NetworkLockDeactivation();
				}
				else
				{
					NetworkLockActivation();
				}
			};

			TableServers.DoubleClick += (object sender, EventArgs e) => {
				ConnectManual();
			};

			CmdServersConnect.Activated += (object sender, EventArgs e) => {
				ConnectManual();
			};

			CmdServersWhiteList.Activated += (object sender, EventArgs e) => {
				ServersWhiteList();
			};

			CmdServersBlackList.Activated += (object sender, EventArgs e) => {
				ServersBlackList();
			};

			CmdServersUndefined.Activated += (object sender, EventArgs e) => {
				ServersUndefinedList ();
			};

			CmdServersRefresh.Activated += (object sender, EventArgs e) => {
				ServersRefresh ();
			};

			MnuServersConnect.Activated += (object sender, EventArgs e) => {
				ConnectManual();
			};

			MnuServersWhitelist.Activated += (object sender, EventArgs e) => {
				ServersWhiteList();
			};

			MnuServersBlacklist.Activated += (object sender, EventArgs e) => {
				ServersBlackList();
			};

			MnuServersUndefined.Activated += (object sender, EventArgs e) => {
				ServersUndefinedList ();
			};

			MnuServersRefresh.Activated += (object sender, EventArgs e) => {
				ServersRefresh ();
			};

			CmdAreasWhiteList.Activated += (object sender, EventArgs e) => {
				AreasWhiteList();
			};

			CmdAreasBlackList.Activated += (object sender, EventArgs e) => {
				AreasBlackList();
			};

			CmdAreasUndefined.Activated += (object sender, EventArgs e) => {
				AreasUndefinedList ();
			};

			MnuAreasWhitelist.Activated += (object sender, EventArgs e) => {
				AreasWhiteList();
			};

			MnuAreasBlacklist.Activated += (object sender, EventArgs e) => {
				AreasBlackList();
			};

			MnuAreasUndefined.Activated += (object sender, EventArgs e) => {
				AreasUndefinedList ();
			};

			ChkServersShowAll.Activated += (object sender, EventArgs e) => {
				TableServersController.ShowAll = (ChkServersShowAll.State == NSCellStateValue.On);
				TableServersController.RefreshUI();
			};

			/*
			 * Xamarin Bug: https://bugzilla.xamarin.com/show_bug.cgi?id=12467
			 * Resolved with delegate
			 * 
			TableServers.SelectionDidChange += (object sender, EventArgs e) => {
				EnabledUI();
			};

			TableAreas.SelectionDidChange += (object sender, EventArgs e) => {
				EnabledUI();
			};
			*/

			ChkServersLockCurrent.Activated += (object sender, EventArgs e) => {
				Engine.Storage.SetBool ("servers.locklast", ChkServersLockCurrent.State == NSCellStateValue.On);
			};

			CboServersScoringRule.Activated += (object sender, EventArgs e) => {
				Engine.Storage.Set ("servers.scoretype", GuiUtils.GetSelected(CboServersScoringRule));

				RefreshUi (Engine.RefreshUiMode.Full);
			};

			CboSpeedResolutions.Activated += (object sender, EventArgs e) => {
				(PnlChart as ChartView).Switch(CboSpeedResolutions.IndexOfItem(CboSpeedResolutions.SelectedItem));
			};
		

			CmdLogsClean.Activated += (object sender, EventArgs e) => {
				TableLogsController.Clear();
			};

			CmdLogsSave.Activated += (object sender, EventArgs e) => {
				LogsDoSave(false);
			};

			CmdLogsCopy.Activated += (object sender, EventArgs e) => {
				LogsDoCopy(false);
			};

			CmdLogsSupport.Activated += (object sender, EventArgs e) =>
			{
				SupportReport();
			};		

			MnuLogsCopyAll.Activated += (object sender, EventArgs e) =>
			{
				LogsDoCopy(false);
			};		

			MnuLogsSaveAll.Activated += (object sender, EventArgs e) =>
			{
				LogsDoSave(false);
			};			
			MnuLogsCopySelected.Activated += (object sender, EventArgs e) =>
			{
				LogsDoCopy(true);
			};			
			MnuLogsSaveSelected.Activated += (object sender, EventArgs e) =>
			{
				LogsDoSave(true);
			};			

			CmdLogsOpenVpnManagement.Activated += (object sender, EventArgs e) => {
				if(Engine.IsConnected())
				{
					WindowOpenVpnManagementCommandController w = new WindowOpenVpnManagementCommandController();
					NSApplication.SharedApplication.RunModalForWindow(w.Window);
					if(w.Command != "")
						Core.UI.Actions.SendOpenVpnManagementCommand(w.Command);
				}
			};
				
			TableServersController = new TableServersController (this.TableServers);
			this.TableServers.Delegate = new TableServersDelegate (this);

			TableAreasController = new TableAreasController (this.TableAreas);
			this.TableAreas.Delegate = new TableAreasDelegate (this);

			TableLogsController = new TableLogsController (this.TableLogs);
			TableStatsController = new TableStatsController (this.TableStats);
		
			TableStats.DoubleClick += (object sender, EventArgs e) => {
				TableStatsController.DoubleClickItem();
			};

			// Topbar Menu

			MnuTrayStatus.Activated += (object sender, EventArgs e) =>
			{
				Restore(sender);
			};

			MnuTrayConnect.Activated += (object sender, EventArgs e) =>
			{
				if (Engine.IsWaiting())
				{
					Disconnect();
				}
				else if (Engine.IsConnected())
				{
					Disconnect();
				}
				else if (Engine.IsLogged())
				{
					Connect();
				}
				else
				{
					Restore(sender);
				}
			};

			MnuTrayAbout.Activated += (object sender, EventArgs e) =>
			{
				ShowAbout();
			};

			MnuTrayPreferences.Activated += (object sender, EventArgs e) =>
			{
				ShowPreferences();
			};

			MnuTrayHome.Activated += (object sender, EventArgs e) =>
			{
				ShowHome();
			};

			MnuTrayClientArea.Activated += (object sender, EventArgs e) =>
			{
				ShowClientArea();
			};

			MnuTrayForwardingPorts.Activated += (object sender, EventArgs e) =>
			{
				ShowForwardingPorts();
			};

			MnuTraySpeedTest.Activated += (object sender, EventArgs e) =>
			{
				ShowSpeedTest();
			};

			MnuTrayRestore.Activated += (object sender, EventArgs e) => {
				/* // 2.8
				if(Window.IsVisible)
					Minimize();
				else
					*/
					Restore(sender);
			};

			MnuTrayQuit.Activated += (object sender, EventArgs e) =>
			{
				Shutdown();
			};

			CboServersScoringRule.ToolTip = Messages.TooltipServersScoreType;
			CmdAreasBlackList.ToolTip = Messages.TooltipAreasBlackList;

			Engine.MainWindow = this;
			Engine.UiStart ();

			Engine.OnRefreshUi ();

			SettingsChanged ();

			RequestAttention ();
		}

		public bool Shutdown()
		{
			if (Engine.Instance.Storage.GetBool ("gui.exit_confirm") == true)
			{
				bool result = GuiUtils.MessageYesNo (Messages.ExitConfirm);
				if (result == false) {
					return false;
				}
			}
			ShutdownConfirmed = true;
			if (windowAbout != null)
				windowAbout.Close ();
			if (windowPreferences != null)
				windowPreferences.Close ();

			Engine.Instance.RequestStop ();
			return true;
		}

		public void RefreshUi (Engine.RefreshUiMode mode)
		{
			try
			{
				if ((mode == Engine.RefreshUiMode.MainMessage) || (mode == Engine.RefreshUiMode.Full)) {

					if (Engine.CurrentServer != null) {
						ImgTopFlag.Image = NSImage.ImageNamed("flag_" + Engine.CurrentServer.CountryCode.ToLowerInvariant() + ".png");
					}
					else {
						ImgTopFlag.Image = NSImage.ImageNamed ("notconnected.png");
					}

					LblWaiting1.StringValue = Engine.WaitMessage;

					if(Engine.IsWaiting())
					{
						ImgProgress.StartAnimation(this);
						ImgTopPanel.Image = NSImage.ImageNamed ("topbar_osx_yellow.png");
						MnuTrayStatus.Image = NSImage.ImageNamed ("status_yellow_16.png");
						LblTopStatus.StringValue = Engine.WaitMessage;

						TabOverview.SelectAt(1);

						CmdCancel.Hidden = (Engine.IsWaitingCancelAllowed() == false);
						CmdCancel.Enabled = (Engine.IsWaitingCancelPending() == false);
						MnuTrayConnect.Enabled = CmdCancel.Enabled;
					}
					else if (Engine.IsConnected())
					{
						ImgProgress.StopAnimation (this);
						ImgTopPanel.Image = NSImage.ImageNamed ("topbar_osx_green.png");
						MnuTrayStatus.Image = NSImage.ImageNamed ("status_green_16.png");
						LblTopStatus.StringValue = Messages.Format(Messages.TopBarConnected, Engine.CurrentServer.DisplayName);

						TabOverview.SelectAt(2);

						LblConnectedServerName.StringValue = Engine.CurrentServer.DisplayName;
						LblConnectedLocation.StringValue = Engine.CurrentServer.GetLocationForList();
						TxtConnectedExitIp.StringValue = Engine.CurrentServer.IpExit;
						ImgConnectedCountry.Image = NSImage.ImageNamed ("flag_" + Engine.CurrentServer.CountryCode.ToLowerInvariant () + ".png");
					}
					else
					{
						ImgProgress.StopAnimation (this);
						ImgTopPanel.Image = NSImage.ImageNamed ("topbar_osx_red.png");
						MnuTrayStatus.Image = NSImage.ImageNamed ("status_red_16.png");
						if(Engine.Instance.NetworkLockManager.IsActive())
							LblTopStatus.StringValue = Messages.TopBarNotConnectedLocked;
						else
							LblTopStatus.StringValue = Messages.TopBarNotConnectedExposed;

						TabOverview.SelectAt(0);
					}

					// Icon update
					if(StatusItem != null)
					{
						//string colorMode = GuiUtils.InterfaceColorMode ();
						string colorMode = Engine.Storage.Get ("gui.osx.style");
				
						if(Engine.IsConnected())
						{
							StatusItem.Image = NSImage.ImageNamed("menubar_" + colorMode.ToLowerInvariant() + "_green.png");
							//NSApplication.SharedApplication.DockTile. =  DateTime.Now.ToString ();
							NSApplication.SharedApplication.ApplicationIconImage = NSImage.ImageNamed("icon.png");
						}
						else
						{
							StatusItem.Image = NSImage.ImageNamed("menubar_" + colorMode.ToLowerInvariant() + "_red.png");
							//NSApplication.SharedApplication.DockTile.Description =  DateTime.Now.ToString ();
							NSApplication.SharedApplication.ApplicationIconImage = NSImage.ImageNamed("icon_gray.png");
						}
					}

					EnabledUI();
				}

				if ((mode == Engine.RefreshUiMode.Log) || (mode == Engine.RefreshUiMode.Full)) {

					lock (Engine.LogsPending) {
						while (Engine.LogsPending.Count > 0) {
							LogEntry l = Engine.LogsPending [0];
							Engine.LogsPending.RemoveAt (0);

							Log (l);
						}
					}
					LblWaiting2.StringValue = Engine.Logs.GetLogDetailTitle();
				}

				if ((mode == Engine.RefreshUiMode.Stats) || (mode == Engine.RefreshUiMode.Full)) {
					if (Engine.IsConnected ()) {
						TxtConnectedSince.StringValue = Engine.Stats.GetValue ("VpnConnectionStart");

						TxtConnectedDownload.StringValue = Core.Utils.FormatBytes (Engine.ConnectedLastDownloadStep, true, false);
						TxtConnectedUpload.StringValue = Core.Utils.FormatBytes (Engine.ConnectedLastUploadStep, true, false);

						string msg = Engine.Instance.GetConnectedTrayText (true, true);
						string tmsg = Constants.Name + " - " + msg;
						this.Window.Title = tmsg;
						MnuTrayStatus.Title = "> " + msg;

						StatusItem.ToolTip = msg;
						StatusItem.Title = Engine.Instance.GetConnectedTrayText(Engine.Instance.Storage.GetBool("gui.osx.sysbar.show_speed"), Engine.Instance.Storage.GetBool("gui.osx.sysbar.show_server"));

					}
				}

				if ((mode == Engine.RefreshUiMode.Full)) {
					if(TableServersController != null)
						TableServersController.RefreshUI ();
					if(TableAreasController != null)
						TableAreasController.RefreshUI ();
				}
			 


			}
			catch(Exception) {
				// TOFIX: OS X sometime throw an useless exception in closing phase
			}
		}


		public void Log(LogEntry l)
		{
			string msg = l.Message;

			TableLogsController.AddLog (l);

			if ((msg != "") && (l.Type != LogType.Verbose)) {
				if(Engine.IsConnected() == false)
				{
					Window.Title = Constants.Name + " - " + msg;
					MnuTrayStatus.Title = "> " + msg;
				}
			}

			if (l.Type >= LogType.InfoImportant) {
				StatusItem.ToolTip = msg;
				if(Engine.Instance.Storage.GetBool("gui.osx.sysbar.show_info"))
					StatusItem.Title = msg;
			}

			if (l.Type >= LogType.InfoImportant)
				Notification (msg, "");

			if (l.Type >= LogType.InfoImportant)
				RequestAttention ();

			if (l.Type == LogType.Fatal)
				GuiUtils.MessageBox (msg);
		}

		public void EnabledUI()
		{
			bool logged = Engine.IsLogged ();
			bool connected = Engine.IsConnected ();
			bool waiting = Engine.IsWaiting ();

			MnuTrayRestore.Hidden = false;
			/* // 2.8
			if (this.Window.IsVisible)
				MnuTrayRestore.Title = Messages.WindowsMainHide;
			else
				*/
			MnuTrayRestore.Title = Messages.WindowsMainShow;

			if (logged == false)
				CmdLogin.Title = Messages.CommandLoginButton;
			else
				CmdLogin.Title = Messages.CommandLogout;

			if (waiting)
			{
				MnuTrayConnect.Title = Messages.CommandCancel;
			}
			else if (connected)
			{
				MnuTrayConnect.Enabled = true;
				MnuTrayConnect.Title = Messages.CommandDisconnect;
			}
			else if (logged)
			{
				MnuTrayConnect.Enabled = true;
				MnuTrayConnect.Title = Messages.CommandConnect;
			}
			else
			{
				MnuTrayConnect.Enabled = true;
				MnuTrayConnect.Title = Messages.CommandLoginMenu;
			}

			CmdLogin.Enabled = ((waiting == false) && (connected == false) && (TxtLogin.StringValue.Trim () != "") && (TxtPassword.StringValue.Trim () != ""));

			TxtLogin.Enabled = (logged == false);
			TxtPassword.Enabled = (logged == false);
			LblKey.Hidden = ( (logged == false) || (CboKey.ItemCount < 2) );
			CboKey.Hidden = LblKey.Hidden;

			if (logged) {
				CmdConnect.Enabled = true;
			} else {
				CmdConnect.Enabled = false;
			}

			CmdServersConnect.Enabled = ((logged) && (TableServers.SelectedRowCount == 1));
			CmdServersWhiteList.Enabled = (TableServers.SelectedRowCount > 0);
			CmdServersBlackList.Enabled = CmdServersWhiteList.Enabled;
			CmdServersUndefined.Enabled = CmdServersWhiteList.Enabled;
			MnuServersConnect.Enabled = CmdServersConnect.Enabled;
			MnuServersWhitelist.Enabled = CmdServersWhiteList.Enabled;
			MnuServersBlacklist.Enabled = CmdServersBlackList.Enabled;
			MnuServersUndefined.Enabled = CmdServersUndefined.Enabled;

			CmdAreasWhiteList.Enabled = (TableAreas.SelectedRowCount > 0);
			CmdAreasBlackList.Enabled = CmdAreasWhiteList.Enabled;
			CmdAreasUndefined.Enabled = CmdAreasWhiteList.Enabled;
			MnuAreasWhitelist.Enabled = CmdAreasWhiteList.Enabled;
			MnuAreasBlacklist.Enabled = CmdAreasBlackList.Enabled;
			MnuAreasUndefined.Enabled = CmdAreasUndefined.Enabled;

			CmdLogsOpenVpnManagement.Hidden = (Engine.Storage.GetBool("advanced.expert") == false);
			CmdLogsOpenVpnManagement.Enabled = connected;

			if (Engine.Instance.NetworkLockManager != null) {
				CmdNetworkLock.Hidden = (Engine.Instance.NetworkLockManager.CanEnabled () == false);
				ImgNetworkLock.Hidden = CmdNetworkLock.Hidden;
				if (Engine.Instance.NetworkLockManager.IsActive ()) {
					CmdNetworkLock.Title = Messages.NetworkLockButtonActive;
					ImgNetworkLock.Image = NSImage.ImageNamed ("netlock_on.png");

					LblNetLockStatus.Image = NSImage.ImageNamed ("netlock_status_on.png");
					LblNetLockStatus.ToolTip = Messages.NetworkLockStatusActive;

				} else {
					CmdNetworkLock.Title = Messages.NetworkLockButtonDeactive;
					ImgNetworkLock.Image = NSImage.ImageNamed ("netlock_off.png");
				
					LblNetLockStatus.Image = NSImage.ImageNamed ("netlock_status_off.png");
					LblNetLockStatus.ToolTip = Messages.NetworkLockStatusDeactive;
				}
			}


		}

		public void SettingsChanged()
		{
			// Commented in 2.8, see this FAQ: https://airvpn.org/topic/13331-its-possible-to-hide-the-icon-in-dock-bar-under-os-x/
			/*
			bool showInDock = Engine.Storage.GetBool ("gui.osx.dock");
			if (showInDock)
				SwitchToRegular ();
			else
				SwitchToAccessory ();
			*/
		}

		/*
		public void SwitchToRegular()
		{
			NSApplication.SharedApplication.ActivationPolicy = NSApplicationActivationPolicy.Regular;
		}

		public void SwitchToAccessory()
		{
			NSApplication.SharedApplication.ActivationPolicy = NSApplicationActivationPolicy.Accessory;
		}
		*/

		public void FrontMessage(string message)
		{
			WindowFrontMessageController w = new WindowFrontMessageController ();
			(Engine.Instance as Engine).WindowsOpen.Add (w);
			w.Message = message;
			GuiUtils.ShowWindowWithFocus (w, this);
		}

		public void PostManifestUpdate()
		{
			MnuServersRefresh.Enabled = true;
			CmdServersRefresh.Enabled = true;
		}

		public void LoggedUpdate(XmlElement xmlKeys)
		{
			CboKey.RemoveAllItems ();
			foreach (XmlElement xmlKey in xmlKeys.ChildNodes) {
				string keyName = xmlKey.GetAttribute ("name");
				CboKey.AddItem(keyName);
			}
			string currentKey = Engine.Instance.Storage.Get ("key");
			int currentIndex = CboKey.IndexOfItem (currentKey);
			if (currentIndex != -1) {
				CboKey.SelectItem (currentIndex);
			} else {
				if (CboKey.ItemCount > 0) {
					CboKey.SelectItem (0);
					Engine.Instance.Storage.Set ("key", CboKey.ItemAtIndex(0).Title);
				} else {
					Engine.Instance.Storage.Set ("key", "");
				}
			}
			
		}

		public void RequestAttention()
		{
			NSApplication.SharedApplication.RequestUserAttention (NSRequestUserAttentionType.InformationalRequest);


		}


		public bool NetworkLockKnowledge()
		{
			string msg = Messages.NetworkLockWarning;

			return (GuiUtils.MessageYesNo (msg));
		}

		public void Notification(string title, string notes)
		{
			RequestAttention ();

			if (Engine.Instance.Storage.GetBool ("gui.osx.notifications") == false)
				return;

			// First we create our notification and customize as needed
			NSUserNotification not = null;

			try {
				not = new NSUserNotification();
			} catch {
				// This API was introduced in OS X Mountain Lion (10.8)
				return;
			}

			not.Title = title;
			not.InformativeText = notes;
			not.DeliveryDate = DateTime.Now;
			not.SoundName = NSUserNotification.NSUserNotificationDefaultSoundName;

			// We get the Default notification Center
			NSUserNotificationCenter center = NSUserNotificationCenter.DefaultUserNotificationCenter;

			// If we return true here, Notification will show up even if your app is TopMost.
			center.ShouldPresentNotification = (c, n) => { return true; };

			center.ScheduleNotification(not);
		}

		public void CreateMenuBarIcon ()
		{
			StatusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(NSStatusItemLength.Variable);
			//StatusItem.Menu = notifyMenu;
			StatusItem.Menu = MnuTray;
			StatusItem.Image = NSImage.ImageNamed ("menubar_light_red.png");
			StatusItem.HighlightMode = true;
		}



		void Login()
		{
			Engine.Storage.Set ("login", TxtLogin.StringValue);
			Engine.Storage.Set ("password", TxtPassword.StringValue);

			if (TermsOfServiceCheck (false) == false)
				return;

			Engine.Login ();
		}

		void Logout()
		{
			Engine.Logout ();
		}

		void Connect()
		{
			if((Engine.IsLogged() == true) && (Engine.IsConnected() == false) && (Engine.IsWaiting() == false))
			{
				TabMain.SelectAt (0);
				Engine.Connect ();
			}
		}

		void ConnectManual()
		{
			if (TableServers.SelectedRows.Count == 1) {
				ServerInfo s = TableServersController.GetRelatedItem(TableServers.SelectedRow);
				Engine.NextServer = s;
				Connect ();
			}
		}

		void Disconnect()
		{
			CmdCancel.Enabled = false;
			MnuTrayConnect.Enabled = false;

			Engine.Disconnect ();
		}

		void NetworkLockActivation()
		{
			if(NetworkLockKnowledge())
			{
				Engine.Instance.NetLockIn ();
			}

		}

		void NetworkLockDeactivation()
		{
			Engine.NetLockOut ();
		}

		bool TermsOfServiceCheck(bool force)
		{
			bool show = force;
			if(show == false)
				show = (Engine.Storage.GetBool ("gui.tos") == false);

			if (show) {
				WindowTosController tos = new WindowTosController ();
				tos.Window.ReleasedWhenClosed = true;
				NSApplication.SharedApplication.RunModalForWindow (tos.Window);
				tos.Window.Close ();

				if (tos.Accepted) {
					Engine.Storage.SetBool ("gui.tos", true);
					return true;
				} else {
					return false;
				}
			} else {
				return true;
			}
		}


		void ServersWhiteList()
		{
			foreach(int i in TableServers.SelectedRows)
			{
				TableServersController.GetRelatedItem(i).UserList = ServerInfo.UserListType.WhiteList;
			}
			Engine.UpdateSettings();
			TableServersController.RefreshUI ();
		}

		void ServersBlackList()
		{
			foreach(int i in TableServers.SelectedRows)
			{
				TableServersController.GetRelatedItem(i).UserList = ServerInfo.UserListType.BlackList;
			}
			Engine.UpdateSettings();
			TableServersController.RefreshUI ();
		}

		void ServersUndefinedList()
		{
			foreach(int i in TableServers.SelectedRows)
			{
				TableServersController.GetRelatedItem(i).UserList = ServerInfo.UserListType.None;
			}
			Engine.UpdateSettings();
			TableServersController.RefreshUI ();
		}

		void ServersRefresh()
		{
			MnuServersRefresh.Enabled = false;
			CmdServersRefresh.Enabled = false;

			Core.Threads.Manifest.Instance.ForceUpdate = true;
		}

		void AreasWhiteList()
		{
			foreach(int i in TableAreas.SelectedRows)
			{
				TableAreasController.GetRelatedItem(i).UserList = AreaInfo.UserListType.WhiteList;
			}
			Engine.UpdateSettings();
			TableServersController.RefreshUI ();
		}

		void AreasBlackList()
		{
			foreach(int i in TableAreas.SelectedRows)
			{
				TableAreasController.GetRelatedItem(i).UserList = AreaInfo.UserListType.BlackList;
			}
			Engine.UpdateSettings();
			TableAreasController.RefreshUI ();
		}

		void AreasUndefinedList()
		{
			foreach(int i in TableAreas.SelectedRows)
			{
				TableAreasController.GetRelatedItem(i).UserList = AreaInfo.UserListType.None;
			}
			Engine.UpdateSettings();
			TableAreasController.RefreshUI ();
		}

		void SupportReport()
		{
			//string report = Engine.Instance.GetSupportReport (TableLogsController.GetBody (false));
			string report = "Pazzo";

			string [] pboardTypes = new string[] { "NSStringPboardType" };
			NSPasteboard.GeneralPasteboard.DeclareTypes (pboardTypes, null);
			NSPasteboard.GeneralPasteboard.SetStringForType (report, pboardTypes [0]);
			GuiUtils.MessageBox (Messages.LogsCopyClipboardDone);
		}

		void LogsDoCopy(bool selectedOnly)
		{
			string t = TableLogsController.GetBody (selectedOnly);
			if (t != "") {
				string [] pboardTypes = new string[] { "NSStringPboardType" };
				NSPasteboard.GeneralPasteboard.DeclareTypes (pboardTypes, null);
				NSPasteboard.GeneralPasteboard.SetStringForType (t, pboardTypes [0]);
				GuiUtils.MessageBox (Messages.LogsCopyClipboardDone);
			}
		}

		void LogsDoSave(bool selectedOnly)
		{
			string t = TableLogsController.GetBody (selectedOnly);
			if (t.Trim () != "") {
				//string filename = "AirVPN_" + DateTime.Now.ToString ("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + ".txt"; // TOCLEAN
				string filename = Engine.Logs.GetLogSuggestedFileName();

				NSSavePanel panel = new NSSavePanel ();
				panel.NameFieldStringValue = filename;
				panel.CanCreateDirectories = true;
				int result = panel.RunModal ();
				if (result == 1) {
					System.IO.File.WriteAllText (panel.Url.Path, t);
			
					GuiUtils.MessageBox (Messages.LogsSaveToFileDone);
				}
			}
		}

		public void ShowAbout()
		{
			if ( (windowAbout == null) || (windowAbout.Window.IsVisible == false) )
				windowAbout = new WindowAboutController();
			GuiUtils.ShowWindowWithFocus (windowAbout, this);
		}

		public void ShowPreferences()
		{
			if ( (windowPreferences == null) || (windowPreferences.Window.IsVisible == false) )
				windowPreferences = new WindowPreferencesController();
			GuiUtils.ShowWindowWithFocus (windowPreferences,this);
		}

		public void ShowHome()
		{
			Engine.Instance.Command ("ui.show.website");
		}

		public void ShowClientArea()
		{
			Engine.Instance.Command ("ui.show.clientarea");
		}

		public void ShowForwardingPorts()
		{
			Engine.Instance.Command ("ui.show.ports");
		}

		public void ShowSpeedTest()
		{
			Engine.Instance.Command ("ui.show.speedtest");
		}

		public void Minimize()
		{
			Window.Miniaturize (this);
			EnabledUI ();
		}

		public void Restore(object sender)
		{

			NSApplication.SharedApplication.ActivateIgnoringOtherApps (true);

			GuiUtils.ShowWindowWithFocus (this, this);
			Window.MakeMainWindow ();
			EnabledUI ();
			/*
			ShowWindow (this);
			Window.MakeMainWindow ();
			Window.Deminiaturize (this);
			EnabledUI ();
			Window.MakeKeyAndOrderFront (this);
			*/

		}
	}
}


