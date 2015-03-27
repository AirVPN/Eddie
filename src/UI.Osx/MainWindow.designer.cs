// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;
using System.CodeDom.Compiler;

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
		MonoMac.AppKit.NSButton ChkLockedMode { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton ChkRemember { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton ChkServersLockCurrent { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton ChkServersShowAll { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdAreasBlackList { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdAreasUndefined { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdAreasWhiteList { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdCancel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdConnect { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdDisconnect { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdLogin { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdLogsClean { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdLogsCopy { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdLogsOpenVpnManagement { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdLogsSave { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdLogsSupport { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdNetworkLock { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdServersBlackList { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdServersConnect { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdServersUndefined { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdServersWhiteList { get; set; }

		[Outlet]
		MonoMac.AppKit.NSImageView ImgConnectedCountry { get; set; }

		[Outlet]
		MonoMac.AppKit.NSImageView ImgNetworkLock { get; set; }

		[Outlet]
		MonoMac.AppKit.NSProgressIndicator ImgProgress { get; set; }

		[Outlet]
		MonoMac.AppKit.NSImageView ImgTopFlag { get; set; }

		[Outlet]
		MonoMac.AppKit.NSImageView ImgTopPanel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField LblConnect { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField LblConnectedLocation { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField LblConnectedServerName { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField LblTopStatus { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField LblWaiting1 { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField LblWaiting2 { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem MnuAreasBlacklist { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem MnuAreasUndefined { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem MnuAreasWhitelist { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem MnuLogsCopyAll { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem MnuLogsCopySelected { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem MnuLogsSaveAll { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem MnuLogsSaveSelected { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem MnuServersBlacklist { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem MnuServersConnect { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem MnuServersUndefined { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem MnuServersWhitelist { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenu MnuTray { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem MnuTrayAbout { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem MnuTrayClientArea { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem MnuTrayConnect { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem MnuTrayForwardingPorts { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem MnuTrayHome { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem MnuTrayPreferences { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem MnuTrayQuit { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem MnuTrayRestore { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem MnuTraySpeedTest { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem MnuTrayStatus { get; set; }

		[Outlet]
		MonoMac.AppKit.NSBox PanelConnected { get; set; }

		[Outlet]
		MonoMac.AppKit.NSBox PanelWaiting { get; set; }

		[Outlet]
		MonoMac.AppKit.NSBox PanelWelcome { get; set; }

		[Outlet]
		MonoMac.AppKit.NSView PnlChart { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenu ServersContextMenu { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTableView TableAreas { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTableView TableLogs { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTableView TableServers { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTableView TableStats { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTabView TabMain { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTabView TabOverview { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField TxtCommand { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField TxtConnectedDownload { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField TxtConnectedExitIp { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField TxtConnectedSince { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField TxtConnectedUpload { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField TxtLogin { get; set; }

		[Outlet]
		MonoMac.AppKit.NSSecureTextField TxtPassword { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (TxtCommand != null) {
				TxtCommand.Dispose ();
				TxtCommand = null;
			}

			if (CboServersScoringRule != null) {
				CboServersScoringRule.Dispose ();
				CboServersScoringRule = null;
			}

			if (CboSpeedResolutions != null) {
				CboSpeedResolutions.Dispose ();
				CboSpeedResolutions = null;
			}

			if (ChkLockedMode != null) {
				ChkLockedMode.Dispose ();
				ChkLockedMode = null;
			}

			if (ChkRemember != null) {
				ChkRemember.Dispose ();
				ChkRemember = null;
			}

			if (ChkServersLockCurrent != null) {
				ChkServersLockCurrent.Dispose ();
				ChkServersLockCurrent = null;
			}

			if (ChkServersShowAll != null) {
				ChkServersShowAll.Dispose ();
				ChkServersShowAll = null;
			}

			if (CmdAreasBlackList != null) {
				CmdAreasBlackList.Dispose ();
				CmdAreasBlackList = null;
			}

			if (CmdAreasUndefined != null) {
				CmdAreasUndefined.Dispose ();
				CmdAreasUndefined = null;
			}

			if (CmdAreasWhiteList != null) {
				CmdAreasWhiteList.Dispose ();
				CmdAreasWhiteList = null;
			}

			if (CmdCancel != null) {
				CmdCancel.Dispose ();
				CmdCancel = null;
			}

			if (CmdConnect != null) {
				CmdConnect.Dispose ();
				CmdConnect = null;
			}

			if (CmdDisconnect != null) {
				CmdDisconnect.Dispose ();
				CmdDisconnect = null;
			}

			if (CmdLogin != null) {
				CmdLogin.Dispose ();
				CmdLogin = null;
			}

			if (CmdLogsClean != null) {
				CmdLogsClean.Dispose ();
				CmdLogsClean = null;
			}

			if (CmdLogsCopy != null) {
				CmdLogsCopy.Dispose ();
				CmdLogsCopy = null;
			}

			if (CmdLogsOpenVpnManagement != null) {
				CmdLogsOpenVpnManagement.Dispose ();
				CmdLogsOpenVpnManagement = null;
			}

			if (CmdLogsSave != null) {
				CmdLogsSave.Dispose ();
				CmdLogsSave = null;
			}

			if (CmdLogsSupport != null) {
				CmdLogsSupport.Dispose ();
				CmdLogsSupport = null;
			}

			if (CmdNetworkLock != null) {
				CmdNetworkLock.Dispose ();
				CmdNetworkLock = null;
			}

			if (CmdServersBlackList != null) {
				CmdServersBlackList.Dispose ();
				CmdServersBlackList = null;
			}

			if (CmdServersConnect != null) {
				CmdServersConnect.Dispose ();
				CmdServersConnect = null;
			}

			if (CmdServersUndefined != null) {
				CmdServersUndefined.Dispose ();
				CmdServersUndefined = null;
			}

			if (CmdServersWhiteList != null) {
				CmdServersWhiteList.Dispose ();
				CmdServersWhiteList = null;
			}

			if (ImgConnectedCountry != null) {
				ImgConnectedCountry.Dispose ();
				ImgConnectedCountry = null;
			}

			if (ImgNetworkLock != null) {
				ImgNetworkLock.Dispose ();
				ImgNetworkLock = null;
			}

			if (ImgProgress != null) {
				ImgProgress.Dispose ();
				ImgProgress = null;
			}

			if (ImgTopFlag != null) {
				ImgTopFlag.Dispose ();
				ImgTopFlag = null;
			}

			if (ImgTopPanel != null) {
				ImgTopPanel.Dispose ();
				ImgTopPanel = null;
			}

			if (LblConnect != null) {
				LblConnect.Dispose ();
				LblConnect = null;
			}

			if (LblConnectedLocation != null) {
				LblConnectedLocation.Dispose ();
				LblConnectedLocation = null;
			}

			if (LblConnectedServerName != null) {
				LblConnectedServerName.Dispose ();
				LblConnectedServerName = null;
			}

			if (LblTopStatus != null) {
				LblTopStatus.Dispose ();
				LblTopStatus = null;
			}

			if (LblWaiting1 != null) {
				LblWaiting1.Dispose ();
				LblWaiting1 = null;
			}

			if (LblWaiting2 != null) {
				LblWaiting2.Dispose ();
				LblWaiting2 = null;
			}

			if (MnuAreasBlacklist != null) {
				MnuAreasBlacklist.Dispose ();
				MnuAreasBlacklist = null;
			}

			if (MnuAreasUndefined != null) {
				MnuAreasUndefined.Dispose ();
				MnuAreasUndefined = null;
			}

			if (MnuAreasWhitelist != null) {
				MnuAreasWhitelist.Dispose ();
				MnuAreasWhitelist = null;
			}

			if (MnuLogsCopyAll != null) {
				MnuLogsCopyAll.Dispose ();
				MnuLogsCopyAll = null;
			}

			if (MnuLogsCopySelected != null) {
				MnuLogsCopySelected.Dispose ();
				MnuLogsCopySelected = null;
			}

			if (MnuLogsSaveAll != null) {
				MnuLogsSaveAll.Dispose ();
				MnuLogsSaveAll = null;
			}

			if (MnuLogsSaveSelected != null) {
				MnuLogsSaveSelected.Dispose ();
				MnuLogsSaveSelected = null;
			}

			if (MnuServersBlacklist != null) {
				MnuServersBlacklist.Dispose ();
				MnuServersBlacklist = null;
			}

			if (MnuServersConnect != null) {
				MnuServersConnect.Dispose ();
				MnuServersConnect = null;
			}

			if (MnuServersUndefined != null) {
				MnuServersUndefined.Dispose ();
				MnuServersUndefined = null;
			}

			if (MnuServersWhitelist != null) {
				MnuServersWhitelist.Dispose ();
				MnuServersWhitelist = null;
			}

			if (MnuTray != null) {
				MnuTray.Dispose ();
				MnuTray = null;
			}

			if (MnuTrayAbout != null) {
				MnuTrayAbout.Dispose ();
				MnuTrayAbout = null;
			}

			if (MnuTrayClientArea != null) {
				MnuTrayClientArea.Dispose ();
				MnuTrayClientArea = null;
			}

			if (MnuTrayConnect != null) {
				MnuTrayConnect.Dispose ();
				MnuTrayConnect = null;
			}

			if (MnuTrayForwardingPorts != null) {
				MnuTrayForwardingPorts.Dispose ();
				MnuTrayForwardingPorts = null;
			}

			if (MnuTrayHome != null) {
				MnuTrayHome.Dispose ();
				MnuTrayHome = null;
			}

			if (MnuTrayPreferences != null) {
				MnuTrayPreferences.Dispose ();
				MnuTrayPreferences = null;
			}

			if (MnuTrayQuit != null) {
				MnuTrayQuit.Dispose ();
				MnuTrayQuit = null;
			}

			if (MnuTrayRestore != null) {
				MnuTrayRestore.Dispose ();
				MnuTrayRestore = null;
			}

			if (MnuTraySpeedTest != null) {
				MnuTraySpeedTest.Dispose ();
				MnuTraySpeedTest = null;
			}

			if (MnuTrayStatus != null) {
				MnuTrayStatus.Dispose ();
				MnuTrayStatus = null;
			}

			if (PanelConnected != null) {
				PanelConnected.Dispose ();
				PanelConnected = null;
			}

			if (PanelWaiting != null) {
				PanelWaiting.Dispose ();
				PanelWaiting = null;
			}

			if (PanelWelcome != null) {
				PanelWelcome.Dispose ();
				PanelWelcome = null;
			}

			if (PnlChart != null) {
				PnlChart.Dispose ();
				PnlChart = null;
			}

			if (ServersContextMenu != null) {
				ServersContextMenu.Dispose ();
				ServersContextMenu = null;
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

			if (TableStats != null) {
				TableStats.Dispose ();
				TableStats = null;
			}

			if (TabMain != null) {
				TabMain.Dispose ();
				TabMain = null;
			}

			if (TabOverview != null) {
				TabOverview.Dispose ();
				TabOverview = null;
			}

			if (TxtConnectedDownload != null) {
				TxtConnectedDownload.Dispose ();
				TxtConnectedDownload = null;
			}

			if (TxtConnectedExitIp != null) {
				TxtConnectedExitIp.Dispose ();
				TxtConnectedExitIp = null;
			}

			if (TxtConnectedSince != null) {
				TxtConnectedSince.Dispose ();
				TxtConnectedSince = null;
			}

			if (TxtConnectedUpload != null) {
				TxtConnectedUpload.Dispose ();
				TxtConnectedUpload = null;
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
