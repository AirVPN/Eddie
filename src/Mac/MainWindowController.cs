using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace AirVPN.Mac
{
	public partial class MainWindowController : MonoMac.AppKit.NSWindowController
	{
		protected int numberOfTimesClicked = 0;
		private TableLogsController tableLogsController;

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
			ClickMeButton.Activated += (object sender, EventArgs e) =>
			{
				/*
				numberOfTimesClicked++;
				OutputLabel.StringValue = "Clicked " + numberOfTimesClicked + " times.";

				tableLogsController.list.Add (numberOfTimesClicked.ToString ());
				tableLogsController.tableView.ReloadData();
				*/

				WindowAboutController about = new WindowAboutController();
				NSApplication.SharedApplication.RunModalForWindow(about.Window);
			};			

			this.tableLogsController = new TableLogsController (this.TableLogs);
		}
	}
}

