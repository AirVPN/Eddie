using System;
using System.Collections.Generic;
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
			CboServersScoringRule.ToolTip = Messages.TooltipServersScoreType;
			ChkServersLockCurrent.ToolTip = Messages.TooltipServersLockCurrent;
			ChkServersShowAll.ToolTip = Messages.TooltipServersShowAll;

			CmdLogin.Activated += (object sender, EventArgs e) =>
			{


				WindowTosController tos = new WindowTosController();
				tos.PazzoTest = "gamma";
				tos.Window.ReleasedWhenClosed = true;
				NSApplication.SharedApplication.RunModalForWindow(tos.Window);
				tos.Window.Close();

				// Sample MessageBox
				NSAlert alert = new NSAlert();
				alert.MessageText = "Sample alert title";
				if(tos.Accepted)
					alert.InformativeText = "Sample alert body, ACCEPTED";
				else 
					alert.InformativeText = "Sample alert body, REJECTED";
				alert.RunModal();
			};	

			CmdConnect.Activated += (object sender, EventArgs e) =>
			{
				Engine.LogDebug ("pazzo");

				ImgProgress.StartAnimation(this);

				Notification("Connected","to Castor!");
			};

			CmdLogsSupport.Activated += (object sender, EventArgs e) =>
			{
				WindowAboutController about = new WindowAboutController();

				NSApplication.SharedApplication.RunModalForWindow(about.Window);
			};			
			/*
			ClickMeButton.Activated += (object sender, EventArgs e) =>
			{


				WindowAboutController about = new WindowAboutController();
				NSApplication.SharedApplication.RunModalForWindow(about.Window);
			};			
			*/
			TableServersController = new TableServersController (this.TableServers);
			TableAreasController = new TableAreasController (this.TableAreas);
			TableLogsController = new TableLogsController (this.TableLogs);
			TableStatsController = new TableStatsController (this.TableStats);
		
			TableStats.DoubleClick += (object sender, EventArgs e) => {
				TableStatsController.DoubleClickItem();
			};
		}

		public void RefreshUi (Engine.RefreshUiMode mode)
		{
			TableServersController.RefreshUI ();
			TableAreasController.RefreshUI ();
		}

		public void RequestAttention()
		{
			NSApplication.SharedApplication.RequestUserAttention (NSRequestUserAttentionType.InformationalRequest);


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

		public UI.Osx.Engine Engine
		{
			get {
				return Core.Engine.Instance as UI.Osx.Engine;
			}
		}
	}
}

