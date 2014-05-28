using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using AirVPN.Core;

namespace AirVPN.UI.Osx
{
	public partial class MainWindowController : MonoMac.AppKit.NSWindowController
	{
		public TableServersController TableServersController;
		public TableAreasController TableAreasController;
		public TableLogsController TableLogsController;
		public TableStatsController TableStatsController;

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

			Engine.MainWindow = this;


			ChkServersShowAll.State = NSCellStateValue.Off;
			ChkServersLockCurrent.State = Engine.Storage.GetBool ("servers.locklast") ? NSCellStateValue.On : NSCellStateValue.Off;
			CboServersScoringRule.StringValue = Engine.Storage.Get ("servers.scoretype");

			CboSpeedResolutions.RemoveAllItems ();
			CboSpeedResolutions.AddItem (Messages.WindowsMainSpeedResolution1);
			CboSpeedResolutions.AddItem (Messages.WindowsMainSpeedResolution2);
			CboSpeedResolutions.AddItem (Messages.WindowsMainSpeedResolution3);
			CboSpeedResolutions.AddItem (Messages.WindowsMainSpeedResolution4);
			CboSpeedResolutions.AddItem (Messages.WindowsMainSpeedResolution5);
			CboSpeedResolutions.SelectItem (0);

			CmdConnect.ToolTip = Messages.CommandConnect;
			LblConnect.StringValue = Messages.CommandConnectSubtitle;
			CmdDisconnect.ToolTip = Messages.CommandDisconnect;
			//CmdCancel.ToolTip = Messages.CommandCancel; // TOFIX
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

			ChkLockedMode.State = (Engine.Storage.GetBool ("advanced.locked_security") ? NSCellStateValue.On : NSCellStateValue.Off);



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

			CmdConnect.Activated += (object sender, EventArgs e) =>
			{
				Engine.LogDebug ("pazzo");

				ImgProgress.StartAnimation(this);

				Notification("Connected","to Castor!");

				Engine.OnFrontMessage("Front test message");


				Connect();
			};

			CmdDisconnect.Activated += (object sender, EventArgs e) => {
				Disconnect();
			};

			CmdCancel.Activated += (object sender, EventArgs e) => {
				CmdCancel.Enabled = false;
				Disconnect ();
			};

			ChkLockedMode.Activated += (object sender, EventArgs e) => {
				CheckLockedNetwork();
			};
		//K
			TableServers.DoubleClick += (object sender, EventArgs e) => {
				Notification("Dblclick server","to Castor!");

				// TODO
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

			CmdAreasWhiteList.Activated += (object sender, EventArgs e) => {
				AreasWhiteList();
			};

			CmdAreasBlackList.Activated += (object sender, EventArgs e) => {
				AreasBlackList();
			};

			CmdAreasUndefined.Activated += (object sender, EventArgs e) => {
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
				Engine.Storage.Set ("servers.scoretype", CboServersScoringRule.StringValue);

				RefreshUi (AirVPN.Core.Engine.RefreshUiMode.Full);
			};

			CmdLogsClean.Activated += (object sender, EventArgs e) => {
				lock(Engine.LogsEntries)
				{
					Engine.LogsEntries.Clear();
				}
				TableLogsController.RefreshUI();
			};

			CmdLogsSave.Activated += (object sender, EventArgs e) => {
				LogsDoSave(false);
			};

			CmdLogsCopy.Activated += (object sender, EventArgs e) => {
				LogsDoCopy(false);
			};

			CmdLogsSupport.Activated += (object sender, EventArgs e) =>
			{
				LogsDoCopy(false);
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

			CboSpeedResolutions.Activated += (object sender, EventArgs e) => {
				// TODO
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



			Engine.OnRefreshUi ();
		}

		public void RefreshUi (Engine.RefreshUiMode mode)
		{
			if ((mode == Engine.RefreshUiMode.MainMessage) || (mode == Engine.RefreshUiMode.Full)) {
			}

			if ((mode == Engine.RefreshUiMode.Log) || (mode == Engine.RefreshUiMode.Full)) {
			}

			if ((mode == Engine.RefreshUiMode.MainMessage) || (mode == Engine.RefreshUiMode.Full)) {
			}

			if ((mode == Engine.RefreshUiMode.Stats) || (mode == Engine.RefreshUiMode.Full)) {
			}

			if ((mode == Engine.RefreshUiMode.Full)) {
				if(TableServersController != null)
					TableServersController.RefreshUI ();
				if(TableAreasController != null)
					TableAreasController.RefreshUI ();
			}
			 



		}

		public void EnabledUI()
		{
		}

		public void FrontMessage(string message)
		{
			WindowFrontMessageController w = new WindowFrontMessageController ();
			(Engine.Instance as Engine).WindowsOpen.Add (w);
			w.Message = message;
			w.ShowWindow (this);
		}

		public void RequestAttention()
		{
			NSApplication.SharedApplication.RequestUserAttention (NSRequestUserAttentionType.InformationalRequest);


		}

		public void MessageAlert(string message)
		{
			MessageAlert (message, "");
		}

		public void MessageAlert(string message, string title)
		{
			NSAlert alert = new NSAlert();
			alert.MessageText = title;
			alert.InformativeText = message;
			alert.RunModal();
		}

		public void Notification(string title, string notes)
		{
			RequestAttention ();

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
				// TOFIX TabMain.SelectedIndex = 0;

				//Engine.Connect ();
				if (Engine.NextServer == null)
					MessageAlert ("ConnectTitle", "Connect to best");
				else
					MessageAlert ("ConnectTitle", "Connect to " + Engine.NextServer.PublicName);
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
			Engine.Disconnect ();
		}

		void CheckLockedNetwork()
		{
			// TODO
		}

		bool TermsOfServiceCheck(bool force)
		{
			bool show = force;
			if(show == false)
				show = (Engine.Storage.GetBool ("gui.tos") == false);

			if (show) {
				WindowTosController tos = new WindowTosController ();
				tos.PazzoTest = "gamma";
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
				return false;
			}
		}


		void ServersWhiteList()
		{
			foreach(int i in TableServers.SelectedRows)
			{
				TableServersController.GetRelatedItem(i).UserList = ServerInfo.UserListType.WhiteList;
			}
			TableServersController.RefreshUI ();
		}

		void ServersBlackList()
		{
			foreach(int i in TableServers.SelectedRows)
			{
				TableServersController.GetRelatedItem(i).UserList = ServerInfo.UserListType.BlackList;
			}
			TableServersController.RefreshUI ();
		}

		void ServersUndefinedList()
		{
			foreach(int i in TableServers.SelectedRows)
			{
				TableServersController.GetRelatedItem(i).UserList = ServerInfo.UserListType.None;
			}
			TableServersController.RefreshUI ();
		}

		void AreasWhiteList()
		{
			foreach(int i in TableAreas.SelectedRows)
			{
				TableAreasController.GetRelatedItem(i).UserList = AreaInfo.UserListType.WhiteList;
			}
			TableServersController.RefreshUI ();
		}

		void AreasBlackList()
		{
			foreach(int i in TableAreas.SelectedRows)
			{
				TableAreasController.GetRelatedItem(i).UserList = AreaInfo.UserListType.BlackList;
			}
			TableAreasController.RefreshUI ();
		}

		void AreasUndefinedList()
		{
			foreach(int i in TableAreas.SelectedRows)
			{
				TableAreasController.GetRelatedItem(i).UserList = AreaInfo.UserListType.None;
			}
			TableAreasController.RefreshUI ();
		}

		string LogsGetBody(bool selectedOnly)
		{
			System.Text.StringBuilder buffer = new System.Text.StringBuilder ();

			lock(Engine.LogsEntries)
			{
				int i = 0;
				foreach (LogEntry l in Engine.LogsEntries) {
					bool skip = false;

					if (selectedOnly) {
						if (TableLogs.IsRowSelected (i) == false)
							skip = true;
					}

					if (skip == false) {
						buffer.Append (l.GetStringLines () + "\n");
					}

					i++;
				}
			}

			return Platform.Instance.NormalizeString(buffer.ToString());
		}

		void LogsDoCopy(bool selectedOnly)
		{
			string t = LogsGetBody (selectedOnly);
			if (t != "") {
				string [] pboardTypes = new string[] { "NSStringPboardType" };
				NSPasteboard.GeneralPasteboard.DeclareTypes (pboardTypes, null);
				NSPasteboard.GeneralPasteboard.SetStringForType (t, pboardTypes [0]);
				MessageAlert (Messages.LogsCopyClipboardDone);
			}
		}

		void LogsDoSave(bool selectedOnly)
		{
			string t = LogsGetBody (selectedOnly);
			if (t.Trim () != "") {
				string filename = "AirVPN_" + DateTime.Now.ToString ("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + ".txt"; // TOCLEAN
				//string filename = Engine.GetLogSuggestedFileName();

				NSSavePanel panel = new NSSavePanel ();
				panel.NameFieldStringValue = filename;
				panel.CanCreateDirectories = true;
				int result = panel.RunModal ();
				if (result == 1) {
					System.IO.File.WriteAllText (panel.Url.Path, t);
			
					MessageAlert (Messages.LogsSaveToFileDone);
				}
			}
		}
	}
}

