// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace AirVPN.UI.Osx
{
	[Register ("MainWindowController")]
	partial class MainWindowController
	{
		[Outlet]
		MonoMac.AppKit.NSPopUpButton CboServersScoringRule { get; set; }

		[Outlet]
		MonoMac.AppKit.NSPopUpButton CboSpeedResolutions { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton ChkRemember { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton ChkServersLockCurrent { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton ChkServersShowAll { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdConnect { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdDisconnect { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdLogin { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdLogsSupport { get; set; }

		[Outlet]
		MonoMac.AppKit.NSProgressIndicator ImgProgress { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField LblConnect { get; set; }

		[Outlet]
		MonoMac.AppKit.NSBox PanelConnected { get; set; }

		[Outlet]
		MonoMac.AppKit.NSBox PanelWaiting { get; set; }

		[Outlet]
		MonoMac.AppKit.NSBox PanelWelcome { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTableView TableAreas { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTableView TableLogs { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTableView TableServers { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTableView TableStats { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField TxtLogin { get; set; }

		[Outlet]
		MonoMac.AppKit.NSSecureTextField TxtPassword { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CboSpeedResolutions != null) {
				CboSpeedResolutions.Dispose ();
				CboSpeedResolutions = null;
			}

			if (ChkRemember != null) {
				ChkRemember.Dispose ();
				ChkRemember = null;
			}

			if (CmdConnect != null) {
				CmdConnect.Dispose ();
				CmdConnect = null;
			}

			if (CmdLogin != null) {
				CmdLogin.Dispose ();
				CmdLogin = null;
			}

			if (CmdLogsSupport != null) {
				CmdLogsSupport.Dispose ();
				CmdLogsSupport = null;
			}

			if (ImgProgress != null) {
				ImgProgress.Dispose ();
				ImgProgress = null;
			}

			if (LblConnect != null) {
				LblConnect.Dispose ();
				LblConnect = null;
			}

			if (PanelConnected != null) {
				PanelConnected.Dispose ();
				PanelConnected = null;
			}

			if (PanelWaiting != null) {
				PanelWaiting.Dispose ();
				PanelWaiting = null;
			}

			if (CmdDisconnect != null) {
				CmdDisconnect.Dispose ();
				CmdDisconnect = null;
			}

			if (PanelWelcome != null) {
				PanelWelcome.Dispose ();
				PanelWelcome = null;
			}

			if (TableAreas != null) {
				TableAreas.Dispose ();
				TableAreas = null;
			}

			if (TableLogs != null) {
				TableLogs.Dispose ();
				TableLogs = null;
			}

			if (TableServers != null) {
				TableServers.Dispose ();
				TableServers = null;
			}

			if (ChkServersShowAll != null) {
				ChkServersShowAll.Dispose ();
				ChkServersShowAll = null;
			}

			if (CboServersScoringRule != null) {
				CboServersScoringRule.Dispose ();
				CboServersScoringRule = null;
			}

			if (ChkServersLockCurrent != null) {
				ChkServersLockCurrent.Dispose ();
				ChkServersLockCurrent = null;
			}

			if (TableStats != null) {
				TableStats.Dispose ();
				TableStats = null;
			}

			if (TxtLogin != null) {
				TxtLogin.Dispose ();
				TxtLogin = null;
			}

			if (TxtPassword != null) {
				TxtPassword.Dispose ();
				TxtPassword = null;
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
