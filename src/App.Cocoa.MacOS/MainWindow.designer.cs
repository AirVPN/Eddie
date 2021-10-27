// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Eddie.UI.Cocoa.Osx
{
	[Register ("MainWindowController")]
	partial class MainWindowController
	{
		[Outlet]
		AppKit.NSPopUpButton CboKey { get; set; }

		[Outlet]
		AppKit.NSPopUpButton CboServersScoringRule { get; set; }

		[Outlet]
		AppKit.NSPopUpButton CboSpeedResolutions { get; set; }

		[Outlet]
		AppKit.NSButton ChkLockedMode { get; set; }

		[Outlet]
		AppKit.NSButton ChkRemember { get; set; }

		[Outlet]
		AppKit.NSButton ChkServersLockCurrent { get; set; }

		[Outlet]
		AppKit.NSButton ChkServersShowAll { get; set; }

		[Outlet]
		AppKit.NSButton CmdAreasDenylist { get; set; }

		[Outlet]
		AppKit.NSButton CmdAreasUndefined { get; set; }

		[Outlet]
		AppKit.NSButton CmdAreasAllowlist { get; set; }

		[Outlet]
		AppKit.NSButton CmdCancel { get; set; }

		[Outlet]
		AppKit.NSButton CmdConnect { get; set; }

		[Outlet]
		AppKit.NSButton CmdDisconnect { get; set; }

		[Outlet]
		AppKit.NSButton CmdLogin { get; set; }

		[Outlet]
		AppKit.NSButton CmdLogsClean { get; set; }

		[Outlet]
		AppKit.NSButton CmdLogsCommand { get; set; }

		[Outlet]
		AppKit.NSButton CmdLogsCopy { get; set; }

		[Outlet]
		AppKit.NSButton CmdLogsOpenVpnManagement { get; set; }

		[Outlet]
		AppKit.NSButton CmdLogsSave { get; set; }

		[Outlet]
		AppKit.NSButton CmdLogsSupport { get; set; }

		[Outlet]
		AppKit.NSButton CmdMainMenu { get; set; }

		[Outlet]
		AppKit.NSButton CmdNetworkLock { get; set; }

		[Outlet]
		AppKit.NSButton CmdProviderAdd { get; set; }

		[Outlet]
		AppKit.NSButton CmdProviderEdit { get; set; }

		[Outlet]
		AppKit.NSButton CmdProviderRemove { get; set; }

		[Outlet]
		AppKit.NSButton CmdServersDenylist { get; set; }

		[Outlet]
		AppKit.NSButton CmdServersConnect { get; set; }

		[Outlet]
		AppKit.NSButton CmdServersMore { get; set; }

		[Outlet]
		AppKit.NSButton CmdServersRefresh { get; set; }

		[Outlet]
		AppKit.NSButton CmdServersRename { get; set; }

		[Outlet]
		AppKit.NSButton CmdServersUndefined { get; set; }

		[Outlet]
		AppKit.NSButton CmdServersAllowlist { get; set; }

		[Outlet]
		AppKit.NSButton CmdUpdater { get; set; }

		[Outlet]
		AppKit.NSImageView ImgConnectedCountry { get; set; }

		[Outlet]
		AppKit.NSImageView ImgNetworkLock { get; set; }

		[Outlet]
		AppKit.NSProgressIndicator ImgProgress { get; set; }

		[Outlet]
		AppKit.NSImageView ImgTopFlag { get; set; }

		[Outlet]
		AppKit.NSImageView ImgTopPanel { get; set; }

		[Outlet]
		AppKit.NSTextField LblConnect { get; set; }

		[Outlet]
		AppKit.NSTextField LblConnectedLocation { get; set; }

		[Outlet]
		AppKit.NSTextField LblConnectedServerName { get; set; }

		[Outlet]
		AppKit.NSTextField LblDevice { get; set; }

		[Outlet]
		AppKit.NSTextField LblKey { get; set; }

		[Outlet]
		AppKit.NSTextField LblLogin { get; set; }

		[Outlet]
		AppKit.NSImageView LblLoginIcon { get; set; }

		[Outlet]
		AppKit.NSImageView LblNetLockStatus { get; set; }

		[Outlet]
		AppKit.NSTextField LblPassword { get; set; }

		[Outlet]
		AppKit.NSTextField LblTopStatus { get; set; }

		[Outlet]
		AppKit.NSTextField LblVersion { get; set; }

		[Outlet]
		AppKit.NSTextField LblWaiting1 { get; set; }

		[Outlet]
		AppKit.NSTextField LblWaiting2 { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuAreasDenylist { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuAreasUndefined { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuAreasAllowlist { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuLogsCopyAll { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuLogsCopySelected { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuLogsSaveAll { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuLogsSaveSelected { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuServersDenylist { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuServersConnect { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuServersMore { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuServersRefresh { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuServersRename { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuServersUndefined { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuServersAllowlist { get; set; }

		[Outlet]
		AppKit.NSMenu MnuTray { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuTrayAbout { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuTrayClientArea { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuTrayConnect { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuTrayForwardingPorts { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuTrayHome { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuTrayPreferences { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuTrayQuit { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuTrayRestore { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuTraySpeedTest { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuTrayStatus { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuTrayUpdate { get; set; }

		[Outlet]
		AppKit.NSBox PanelConnected { get; set; }

		[Outlet]
		AppKit.NSBox PanelWaiting { get; set; }

		[Outlet]
		AppKit.NSBox PanelWelcome { get; set; }

		[Outlet]
		AppKit.NSView PnlChart { get; set; }

		[Outlet]
		AppKit.NSMenu ServersContextMenu { get; set; }

		[Outlet]
		AppKit.NSTableView TableAreas { get; set; }

		[Outlet]
		AppKit.NSTableView TableLogs { get; set; }

		[Outlet]
		AppKit.NSTableView TableProviders { get; set; }

		[Outlet]
		AppKit.NSTableView TableServers { get; set; }

		[Outlet]
		AppKit.NSTableView TableStats { get; set; }

		[Outlet]
		AppKit.NSTabView TabMain { get; set; }

		[Outlet]
		AppKit.NSTabView TabOverview { get; set; }

		[Outlet]
		AppKit.NSTextField TxtCommand { get; set; }

		[Outlet]
		AppKit.NSTextField TxtConnectedDownload { get; set; }

		[Outlet]
		AppKit.NSTextField TxtConnectedExitIp { get; set; }

		[Outlet]
		AppKit.NSTextField TxtConnectedSince { get; set; }

		[Outlet]
		AppKit.NSTextField TxtConnectedUpload { get; set; }

		[Outlet]
		AppKit.NSTextField TxtLogin { get; set; }

		[Outlet]
		AppKit.NSSecureTextField TxtPassword { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CboKey != null) {
				CboKey.Dispose ();
				CboKey = null;
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

			if (CmdAreasDenylist != null) {
				CmdAreasDenylist.Dispose ();
				CmdAreasDenylist = null;
			}

			if (CmdAreasUndefined != null) {
				CmdAreasUndefined.Dispose ();
				CmdAreasUndefined = null;
			}

			if (CmdAreasAllowlist != null) {
				CmdAreasAllowlist.Dispose ();
				CmdAreasAllowlist = null;
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

			if (CmdLogsCommand != null) {
				CmdLogsCommand.Dispose ();
				CmdLogsCommand = null;
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

			if (CmdMainMenu != null) {
				CmdMainMenu.Dispose ();
				CmdMainMenu = null;
			}

			if (CmdNetworkLock != null) {
				CmdNetworkLock.Dispose ();
				CmdNetworkLock = null;
			}

			if (CmdProviderAdd != null) {
				CmdProviderAdd.Dispose ();
				CmdProviderAdd = null;
			}

			if (CmdProviderEdit != null) {
				CmdProviderEdit.Dispose ();
				CmdProviderEdit = null;
			}

			if (CmdProviderRemove != null) {
				CmdProviderRemove.Dispose ();
				CmdProviderRemove = null;
			}

			if (CmdServersDenylist != null) {
				CmdServersDenylist.Dispose ();
				CmdServersDenylist = null;
			}

			if (CmdServersConnect != null) {
				CmdServersConnect.Dispose ();
				CmdServersConnect = null;
			}

			if (CmdServersMore != null) {
				CmdServersMore.Dispose ();
				CmdServersMore = null;
			}

			if (CmdServersRefresh != null) {
				CmdServersRefresh.Dispose ();
				CmdServersRefresh = null;
			}

			if (CmdServersRename != null) {
				CmdServersRename.Dispose ();
				CmdServersRename = null;
			}

			if (CmdServersUndefined != null) {
				CmdServersUndefined.Dispose ();
				CmdServersUndefined = null;
			}

			if (CmdServersAllowlist != null) {
				CmdServersAllowlist.Dispose ();
				CmdServersAllowlist = null;
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

			if (LblDevice != null) {
				LblDevice.Dispose ();
				LblDevice = null;
			}

			if (LblKey != null) {
				LblKey.Dispose ();
				LblKey = null;
			}

			if (LblLogin != null) {
				LblLogin.Dispose ();
				LblLogin = null;
			}

			if (LblLoginIcon != null) {
				LblLoginIcon.Dispose ();
				LblLoginIcon = null;
			}

			if (LblNetLockStatus != null) {
				LblNetLockStatus.Dispose ();
				LblNetLockStatus = null;
			}

			if (LblPassword != null) {
				LblPassword.Dispose ();
				LblPassword = null;
			}

			if (LblTopStatus != null) {
				LblTopStatus.Dispose ();
				LblTopStatus = null;
			}

			if (LblVersion != null) {
				LblVersion.Dispose ();
				LblVersion = null;
			}

			if (LblWaiting1 != null) {
				LblWaiting1.Dispose ();
				LblWaiting1 = null;
			}

			if (LblWaiting2 != null) {
				LblWaiting2.Dispose ();
				LblWaiting2 = null;
			}

			if (MnuAreasDenylist != null) {
				MnuAreasDenylist.Dispose ();
				MnuAreasDenylist = null;
			}

			if (MnuAreasUndefined != null) {
				MnuAreasUndefined.Dispose ();
				MnuAreasUndefined = null;
			}

			if (MnuAreasAllowlist != null) {
				MnuAreasAllowlist.Dispose ();
				MnuAreasAllowlist = null;
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

			if (MnuServersDenylist != null) {
				MnuServersDenylist.Dispose ();
				MnuServersDenylist = null;
			}

			if (MnuServersConnect != null) {
				MnuServersConnect.Dispose ();
				MnuServersConnect = null;
			}

			if (MnuServersMore != null) {
				MnuServersMore.Dispose ();
				MnuServersMore = null;
			}

			if (MnuServersRefresh != null) {
				MnuServersRefresh.Dispose ();
				MnuServersRefresh = null;
			}

			if (MnuServersRename != null) {
				MnuServersRename.Dispose ();
				MnuServersRename = null;
			}

			if (MnuServersUndefined != null) {
				MnuServersUndefined.Dispose ();
				MnuServersUndefined = null;
			}

			if (MnuServersAllowlist != null) {
				MnuServersAllowlist.Dispose ();
				MnuServersAllowlist = null;
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

			if (MnuTrayUpdate != null) {
				MnuTrayUpdate.Dispose ();
				MnuTrayUpdate = null;
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

			if (TableProviders != null) {
				TableProviders.Dispose ();
				TableProviders = null;
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

			if (TxtCommand != null) {
				TxtCommand.Dispose ();
				TxtCommand = null;
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

			if (CmdUpdater != null) {
				CmdUpdater.Dispose ();
				CmdUpdater = null;
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
