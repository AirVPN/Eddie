// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace AirVPN.Mac
{
	[Register ("MainWindowController")]
	partial class MainWindowController
	{
		[Outlet]
		MonoMac.AppKit.NSButton ClickMeButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField OutputLabel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTableView TableLogs { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ClickMeButton != null) {
				ClickMeButton.Dispose ();
				ClickMeButton = null;
			}

			if (OutputLabel != null) {
				OutputLabel.Dispose ();
				OutputLabel = null;
			}

			if (TableLogs != null) {
				TableLogs.Dispose ();
				TableLogs = null;
			}
		}
	}

	[Register ("MainWindow")]
	partial class MainWindow
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
