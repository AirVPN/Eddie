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
	[Register ("WindowPreferencesController")]
	partial class WindowPreferencesController
	{
		[Outlet]
		AppKit.NSPopUpButton CboAdvancedManifestRefresh { get; set; }

		[Outlet]
		AppKit.NSPopUpButton CboDnsSwitchMode { get; set; }

		[Outlet]
		AppKit.NSPopUpButton CboGeneralOsxInterfaceStyle { get; set; }

		[Outlet]
		AppKit.NSPopUpButton CboIpV6 { get; set; }

		[Outlet]
		AppKit.NSPopUpButton CboLockIncoming { get; set; }

		[Outlet]
		AppKit.NSPopUpButton CboLockMode { get; set; }

		[Outlet]
		AppKit.NSPopUpButton CboLockOutgoing { get; set; }

		[Outlet]
		AppKit.NSPopUpButton CboNetworkEntryInterface { get; set; }

		[Outlet]
		AppKit.NSPopUpButton CboNetworkIPv4Mode { get; set; }

		[Outlet]
		AppKit.NSPopUpButton CboNetworkIPv6Mode { get; set; }

		[Outlet]
		AppKit.NSButton CboOpenVpnDirectivesHelp { get; set; }

		[Outlet]
		AppKit.NSPopUpButton CboOpenVpnDirectivesSkipDefault { get; set; }

		[Outlet]
		AppKit.NSPopUpButton CboOpenVpnRcvBuf { get; set; }

		[Outlet]
		AppKit.NSPopUpButton CboOpenVpnSndBuf { get; set; }

		[Outlet]
		AppKit.NSPopUpButton CboProtocolIPEntry { get; set; }

		[Outlet]
		AppKit.NSPopUpButton CboProxyAuthentication { get; set; }

		[Outlet]
		AppKit.NSPopUpButton CboProxyType { get; set; }

		[Outlet]
		AppKit.NSPopUpButton CboProxyWhen { get; set; }

		[Outlet]
		AppKit.NSPopUpButton CboRoutesOtherwise { get; set; }

		[Outlet]
		AppKit.NSPopUpButton CboUiUnit { get; set; }

		[Outlet]
		AppKit.NSButton ChkAdvancedCheckRoute { get; set; }

		[Outlet]
		AppKit.NSButton ChkAdvancedExpertMode { get; set; }

		[Outlet]
		AppKit.NSButton ChkAdvancedNetworkLocking { get; set; }

		[Outlet]
		AppKit.NSButton ChkAdvancedPingerEnabled { get; set; }

		[Outlet]
		AppKit.NSButton ChkAdvancedProviders { get; set; }

		[Outlet]
		AppKit.NSButton ChkAdvancedSkipAlreadyRun { get; set; }

		[Outlet]
		AppKit.NSButton ChkConnect { get; set; }

		[Outlet]
		AppKit.NSButton ChkDnsCheck { get; set; }

		[Outlet]
		AppKit.NSButton ChkExitConfirm { get; set; }

		[Outlet]
		AppKit.NSButton ChkGeneralOsxNotifications { get; set; }

		[Outlet]
		AppKit.NSButton ChkGeneralOsxVisible { get; set; }

		[Outlet]
		AppKit.NSButton ChkGeneralStartLast { get; set; }

		[Outlet]
		AppKit.NSButton ChkLockAllowDNS { get; set; }

		[Outlet]
		AppKit.NSButton ChkLockAllowPing { get; set; }

		[Outlet]
		AppKit.NSButton ChkLockAllowPrivate { get; set; }

		[Outlet]
		AppKit.NSButton ChkLoggingEnabled { get; set; }

		[Outlet]
		AppKit.NSButton ChkLogLevelDebug { get; set; }

		[Outlet]
		AppKit.NSButton ChkNetLock { get; set; }

		[Outlet]
		AppKit.NSButton ChkProtocolsAutomatic { get; set; }

		[Outlet]
		AppKit.NSButton ChkRouteRemoveDefaultGateway { get; set; }

		[Outlet]
		AppKit.NSButton ChkUiIEC { get; set; }

		[Outlet]
		AppKit.NSButton ChkUiSkipProviderManifestFailed { get; set; }

		[Outlet]
		AppKit.NSButton ChkUiSystemBarShowInfo { get; set; }

		[Outlet]
		AppKit.NSButton ChkUiSystemBarShowServer { get; set; }

		[Outlet]
		AppKit.NSButton ChkUiSystemBarShowSpeed { get; set; }

		[Outlet]
		AppKit.NSButton CmdAdvancedEventsClear { get; set; }

		[Outlet]
		AppKit.NSButton CmdAdvancedEventsEdit { get; set; }

		[Outlet]
		AppKit.NSButton CmdAdvancedHelp { get; set; }

		[Outlet]
		AppKit.NSButton CmdAdvancedOpenVpnPath { get; set; }

		[Outlet]
		AppKit.NSButton CmdCancel { get; set; }

		[Outlet]
		AppKit.NSButton CmdDnsAdd { get; set; }

		[Outlet]
		AppKit.NSButton CmdDnsEdit { get; set; }

		[Outlet]
		AppKit.NSButton CmdDnsRemove { get; set; }

		[Outlet]
		AppKit.NSButton CmdGeneralTos { get; set; }

		[Outlet]
		AppKit.NSButton CmdLockHelp { get; set; }

		[Outlet]
		AppKit.NSButton CmdLoggingOpen { get; set; }

		[Outlet]
		AppKit.NSButton CmdOpenVpnDirectivesCustomPathBrowse { get; set; }

		[Outlet]
		AppKit.NSButton CmdOpenVpnDirectivesHelp { get; set; }

		[Outlet]
		AppKit.NSButton CmdProtocolsHelp1 { get; set; }

		[Outlet]
		AppKit.NSButton CmdProtocolsHelp2 { get; set; }

		[Outlet]
		AppKit.NSButton CmdProxyTorHelp { get; set; }

		[Outlet]
		AppKit.NSButton CmdProxyTorTest { get; set; }

		[Outlet]
		AppKit.NSButton CmdResetToDefault { get; set; }

		[Outlet]
		AppKit.NSButton CmdRouteAdd { get; set; }

		[Outlet]
		AppKit.NSButton CmdRouteEdit { get; set; }

		[Outlet]
		AppKit.NSButton CmdRouteRemove { get; set; }

		[Outlet]
		AppKit.NSButton CmdSave { get; set; }

		[Outlet]
		AppKit.NSTextField LblAdvancedProviders { get; set; }

		[Outlet]
		AppKit.NSTextField LblDnsServers { get; set; }

		[Outlet]
		AppKit.NSTextField LblIPv6 { get; set; }

		[Outlet]
		AppKit.NSTextField LblLockRoutingOutWarning { get; set; }

		[Outlet]
		AppKit.NSTextField LblLoggingHelp { get; set; }

		[Outlet]
		AppKit.NSTextField LblNetworkIPv4Mode { get; set; }

		[Outlet]
		AppKit.NSTextField LblNetworkIPv6Mode { get; set; }

		[Outlet]
		AppKit.NSTextField LblOpenVpnRcvBuf { get; set; }

		[Outlet]
		AppKit.NSTextField LblOpenVpnSndBuf { get; set; }

		[Outlet]
		AppKit.NSTextField LblRoutesNetworkLockWarning { get; set; }

		[Outlet]
		AppKit.NSTextField LblRoutesOtherwise { get; set; }

		[Outlet]
		AppKit.NSTableView TableAdvancedEvents { get; set; }

		[Outlet]
		AppKit.NSTableView TableDnsServers { get; set; }

		[Outlet]
		AppKit.NSTableView TableProtocols { get; set; }

		[Outlet]
		AppKit.NSTableView TableRoutes { get; set; }

		[Outlet]
		AppKit.NSTableView TableTabs { get; set; }

		[Outlet]
		AppKit.NSTabView TabMain { get; set; }

		[Outlet]
		AppKit.NSTextField TxtAdvancedOpenVpnDirectivesCustom { get; set; }

		[Outlet]
		AppKit.NSTextField TxtAdvancedOpenVpnDirectivesDefault { get; set; }

		[Outlet]
		AppKit.NSTextField TxtAdvancedOpenVpnPath { get; set; }

		[Outlet]
		AppKit.NSTextField TxtLockAllowedIPS { get; set; }

		[Outlet]
		AppKit.NSTextField TxtLoggingComputedPath { get; set; }

		[Outlet]
		AppKit.NSTextField TxtLoggingPath { get; set; }

		[Outlet]
		AppKit.NSTextField TxtOpenVpnDirectivesCustomPath { get; set; }

		[Outlet]
		AppKit.NSTextField TxtProxyHost { get; set; }

		[Outlet]
		AppKit.NSTextField TxtProxyLogin { get; set; }

		[Outlet]
		AppKit.NSTextField TxtProxyPassword { get; set; }

		[Outlet]
		AppKit.NSTextField TxtProxyPort { get; set; }

		[Outlet]
		AppKit.NSTextField TxtProxyTorControlPassword { get; set; }

		[Outlet]
		AppKit.NSTextField TxtProxyTorControlPort { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CboAdvancedManifestRefresh != null) {
				CboAdvancedManifestRefresh.Dispose ();
				CboAdvancedManifestRefresh = null;
			}

			if (CboDnsSwitchMode != null) {
				CboDnsSwitchMode.Dispose ();
				CboDnsSwitchMode = null;
			}

			if (CboGeneralOsxInterfaceStyle != null) {
				CboGeneralOsxInterfaceStyle.Dispose ();
				CboGeneralOsxInterfaceStyle = null;
			}

			if (CboIpV6 != null) {
				CboIpV6.Dispose ();
				CboIpV6 = null;
			}

			if (CboLockIncoming != null) {
				CboLockIncoming.Dispose ();
				CboLockIncoming = null;
			}

			if (CboLockMode != null) {
				CboLockMode.Dispose ();
				CboLockMode = null;
			}

			if (CboLockOutgoing != null) {
				CboLockOutgoing.Dispose ();
				CboLockOutgoing = null;
			}

			if (CboNetworkEntryInterface != null) {
				CboNetworkEntryInterface.Dispose ();
				CboNetworkEntryInterface = null;
			}

			if (CboNetworkIPv4Mode != null) {
				CboNetworkIPv4Mode.Dispose ();
				CboNetworkIPv4Mode = null;
			}

			if (CboNetworkIPv6Mode != null) {
				CboNetworkIPv6Mode.Dispose ();
				CboNetworkIPv6Mode = null;
			}

			if (CboOpenVpnDirectivesHelp != null) {
				CboOpenVpnDirectivesHelp.Dispose ();
				CboOpenVpnDirectivesHelp = null;
			}

			if (CboOpenVpnDirectivesSkipDefault != null) {
				CboOpenVpnDirectivesSkipDefault.Dispose ();
				CboOpenVpnDirectivesSkipDefault = null;
			}

			if (ChkUiSkipProviderManifestFailed != null) {
				ChkUiSkipProviderManifestFailed.Dispose ();
				ChkUiSkipProviderManifestFailed = null;
			}

			if (CboOpenVpnRcvBuf != null) {
				CboOpenVpnRcvBuf.Dispose ();
				CboOpenVpnRcvBuf = null;
			}

			if (CboOpenVpnSndBuf != null) {
				CboOpenVpnSndBuf.Dispose ();
				CboOpenVpnSndBuf = null;
			}

			if (CboProtocolIPEntry != null) {
				CboProtocolIPEntry.Dispose ();
				CboProtocolIPEntry = null;
			}

			if (CboProxyAuthentication != null) {
				CboProxyAuthentication.Dispose ();
				CboProxyAuthentication = null;
			}

			if (CboProxyType != null) {
				CboProxyType.Dispose ();
				CboProxyType = null;
			}

			if (CboProxyWhen != null) {
				CboProxyWhen.Dispose ();
				CboProxyWhen = null;
			}

			if (CboRoutesOtherwise != null) {
				CboRoutesOtherwise.Dispose ();
				CboRoutesOtherwise = null;
			}

			if (CboUiUnit != null) {
				CboUiUnit.Dispose ();
				CboUiUnit = null;
			}

			if (ChkAdvancedCheckRoute != null) {
				ChkAdvancedCheckRoute.Dispose ();
				ChkAdvancedCheckRoute = null;
			}

			if (ChkAdvancedExpertMode != null) {
				ChkAdvancedExpertMode.Dispose ();
				ChkAdvancedExpertMode = null;
			}

			if (ChkAdvancedNetworkLocking != null) {
				ChkAdvancedNetworkLocking.Dispose ();
				ChkAdvancedNetworkLocking = null;
			}

			if (ChkAdvancedPingerEnabled != null) {
				ChkAdvancedPingerEnabled.Dispose ();
				ChkAdvancedPingerEnabled = null;
			}

			if (ChkAdvancedProviders != null) {
				ChkAdvancedProviders.Dispose ();
				ChkAdvancedProviders = null;
			}

			if (ChkAdvancedSkipAlreadyRun != null) {
				ChkAdvancedSkipAlreadyRun.Dispose ();
				ChkAdvancedSkipAlreadyRun = null;
			}

			if (ChkConnect != null) {
				ChkConnect.Dispose ();
				ChkConnect = null;
			}

			if (ChkDnsCheck != null) {
				ChkDnsCheck.Dispose ();
				ChkDnsCheck = null;
			}

			if (ChkExitConfirm != null) {
				ChkExitConfirm.Dispose ();
				ChkExitConfirm = null;
			}

			if (ChkGeneralOsxNotifications != null) {
				ChkGeneralOsxNotifications.Dispose ();
				ChkGeneralOsxNotifications = null;
			}

			if (ChkGeneralOsxVisible != null) {
				ChkGeneralOsxVisible.Dispose ();
				ChkGeneralOsxVisible = null;
			}

			if (ChkGeneralStartLast != null) {
				ChkGeneralStartLast.Dispose ();
				ChkGeneralStartLast = null;
			}

			if (ChkLockAllowDNS != null) {
				ChkLockAllowDNS.Dispose ();
				ChkLockAllowDNS = null;
			}

			if (ChkLockAllowPing != null) {
				ChkLockAllowPing.Dispose ();
				ChkLockAllowPing = null;
			}

			if (ChkLockAllowPrivate != null) {
				ChkLockAllowPrivate.Dispose ();
				ChkLockAllowPrivate = null;
			}

			if (ChkLoggingEnabled != null) {
				ChkLoggingEnabled.Dispose ();
				ChkLoggingEnabled = null;
			}

			if (ChkLogLevelDebug != null) {
				ChkLogLevelDebug.Dispose ();
				ChkLogLevelDebug = null;
			}

			if (ChkNetLock != null) {
				ChkNetLock.Dispose ();
				ChkNetLock = null;
			}

			if (ChkProtocolsAutomatic != null) {
				ChkProtocolsAutomatic.Dispose ();
				ChkProtocolsAutomatic = null;
			}

			if (ChkRouteRemoveDefaultGateway != null) {
				ChkRouteRemoveDefaultGateway.Dispose ();
				ChkRouteRemoveDefaultGateway = null;
			}

			if (ChkUiIEC != null) {
				ChkUiIEC.Dispose ();
				ChkUiIEC = null;
			}

			if (ChkUiSystemBarShowInfo != null) {
				ChkUiSystemBarShowInfo.Dispose ();
				ChkUiSystemBarShowInfo = null;
			}

			if (ChkUiSystemBarShowServer != null) {
				ChkUiSystemBarShowServer.Dispose ();
				ChkUiSystemBarShowServer = null;
			}

			if (ChkUiSystemBarShowSpeed != null) {
				ChkUiSystemBarShowSpeed.Dispose ();
				ChkUiSystemBarShowSpeed = null;
			}

			if (CmdAdvancedEventsClear != null) {
				CmdAdvancedEventsClear.Dispose ();
				CmdAdvancedEventsClear = null;
			}

			if (CmdAdvancedEventsEdit != null) {
				CmdAdvancedEventsEdit.Dispose ();
				CmdAdvancedEventsEdit = null;
			}

			if (CmdAdvancedHelp != null) {
				CmdAdvancedHelp.Dispose ();
				CmdAdvancedHelp = null;
			}

			if (CmdAdvancedOpenVpnPath != null) {
				CmdAdvancedOpenVpnPath.Dispose ();
				CmdAdvancedOpenVpnPath = null;
			}

			if (CmdCancel != null) {
				CmdCancel.Dispose ();
				CmdCancel = null;
			}

			if (CmdDnsAdd != null) {
				CmdDnsAdd.Dispose ();
				CmdDnsAdd = null;
			}

			if (CmdDnsEdit != null) {
				CmdDnsEdit.Dispose ();
				CmdDnsEdit = null;
			}

			if (CmdDnsRemove != null) {
				CmdDnsRemove.Dispose ();
				CmdDnsRemove = null;
			}

			if (CmdGeneralTos != null) {
				CmdGeneralTos.Dispose ();
				CmdGeneralTos = null;
			}

			if (CmdLockHelp != null) {
				CmdLockHelp.Dispose ();
				CmdLockHelp = null;
			}

			if (CmdLoggingOpen != null) {
				CmdLoggingOpen.Dispose ();
				CmdLoggingOpen = null;
			}

			if (CmdOpenVpnDirectivesCustomPathBrowse != null) {
				CmdOpenVpnDirectivesCustomPathBrowse.Dispose ();
				CmdOpenVpnDirectivesCustomPathBrowse = null;
			}

			if (CmdOpenVpnDirectivesHelp != null) {
				CmdOpenVpnDirectivesHelp.Dispose ();
				CmdOpenVpnDirectivesHelp = null;
			}

			if (CmdProtocolsHelp1 != null) {
				CmdProtocolsHelp1.Dispose ();
				CmdProtocolsHelp1 = null;
			}

			if (CmdProtocolsHelp2 != null) {
				CmdProtocolsHelp2.Dispose ();
				CmdProtocolsHelp2 = null;
			}

			if (CmdProxyTorHelp != null) {
				CmdProxyTorHelp.Dispose ();
				CmdProxyTorHelp = null;
			}

			if (CmdProxyTorTest != null) {
				CmdProxyTorTest.Dispose ();
				CmdProxyTorTest = null;
			}

			if (CmdResetToDefault != null) {
				CmdResetToDefault.Dispose ();
				CmdResetToDefault = null;
			}

			if (CmdRouteAdd != null) {
				CmdRouteAdd.Dispose ();
				CmdRouteAdd = null;
			}

			if (CmdRouteEdit != null) {
				CmdRouteEdit.Dispose ();
				CmdRouteEdit = null;
			}

			if (CmdRouteRemove != null) {
				CmdRouteRemove.Dispose ();
				CmdRouteRemove = null;
			}

			if (CmdSave != null) {
				CmdSave.Dispose ();
				CmdSave = null;
			}

			if (LblAdvancedProviders != null) {
				LblAdvancedProviders.Dispose ();
				LblAdvancedProviders = null;
			}

			if (LblDnsServers != null) {
				LblDnsServers.Dispose ();
				LblDnsServers = null;
			}

			if (LblIPv6 != null) {
				LblIPv6.Dispose ();
				LblIPv6 = null;
			}

			if (LblLockRoutingOutWarning != null) {
				LblLockRoutingOutWarning.Dispose ();
				LblLockRoutingOutWarning = null;
			}

			if (LblLoggingHelp != null) {
				LblLoggingHelp.Dispose ();
				LblLoggingHelp = null;
			}

			if (LblNetworkIPv4Mode != null) {
				LblNetworkIPv4Mode.Dispose ();
				LblNetworkIPv4Mode = null;
			}

			if (LblNetworkIPv6Mode != null) {
				LblNetworkIPv6Mode.Dispose ();
				LblNetworkIPv6Mode = null;
			}

			if (LblOpenVpnRcvBuf != null) {
				LblOpenVpnRcvBuf.Dispose ();
				LblOpenVpnRcvBuf = null;
			}

			if (LblOpenVpnSndBuf != null) {
				LblOpenVpnSndBuf.Dispose ();
				LblOpenVpnSndBuf = null;
			}

			if (LblRoutesNetworkLockWarning != null) {
				LblRoutesNetworkLockWarning.Dispose ();
				LblRoutesNetworkLockWarning = null;
			}

			if (LblRoutesOtherwise != null) {
				LblRoutesOtherwise.Dispose ();
				LblRoutesOtherwise = null;
			}

			if (TableAdvancedEvents != null) {
				TableAdvancedEvents.Dispose ();
				TableAdvancedEvents = null;
			}

			if (TableDnsServers != null) {
				TableDnsServers.Dispose ();
				TableDnsServers = null;
			}

			if (TableProtocols != null) {
				TableProtocols.Dispose ();
				TableProtocols = null;
			}

			if (TableRoutes != null) {
				TableRoutes.Dispose ();
				TableRoutes = null;
			}

			if (TableTabs != null) {
				TableTabs.Dispose ();
				TableTabs = null;
			}

			if (TabMain != null) {
				TabMain.Dispose ();
				TabMain = null;
			}

			if (TxtAdvancedOpenVpnDirectivesCustom != null) {
				TxtAdvancedOpenVpnDirectivesCustom.Dispose ();
				TxtAdvancedOpenVpnDirectivesCustom = null;
			}

			if (TxtAdvancedOpenVpnDirectivesDefault != null) {
				TxtAdvancedOpenVpnDirectivesDefault.Dispose ();
				TxtAdvancedOpenVpnDirectivesDefault = null;
			}

			if (TxtAdvancedOpenVpnPath != null) {
				TxtAdvancedOpenVpnPath.Dispose ();
				TxtAdvancedOpenVpnPath = null;
			}

			if (TxtLockAllowedIPS != null) {
				TxtLockAllowedIPS.Dispose ();
				TxtLockAllowedIPS = null;
			}

			if (TxtLoggingComputedPath != null) {
				TxtLoggingComputedPath.Dispose ();
				TxtLoggingComputedPath = null;
			}

			if (TxtLoggingPath != null) {
				TxtLoggingPath.Dispose ();
				TxtLoggingPath = null;
			}

			if (TxtOpenVpnDirectivesCustomPath != null) {
				TxtOpenVpnDirectivesCustomPath.Dispose ();
				TxtOpenVpnDirectivesCustomPath = null;
			}

			if (TxtProxyHost != null) {
				TxtProxyHost.Dispose ();
				TxtProxyHost = null;
			}

			if (TxtProxyLogin != null) {
				TxtProxyLogin.Dispose ();
				TxtProxyLogin = null;
			}

			if (TxtProxyPassword != null) {
				TxtProxyPassword.Dispose ();
				TxtProxyPassword = null;
			}

			if (TxtProxyPort != null) {
				TxtProxyPort.Dispose ();
				TxtProxyPort = null;
			}

			if (TxtProxyTorControlPassword != null) {
				TxtProxyTorControlPassword.Dispose ();
				TxtProxyTorControlPassword = null;
			}

			if (TxtProxyTorControlPort != null) {
				TxtProxyTorControlPort.Dispose ();
				TxtProxyTorControlPort = null;
			}
		}
	}

	[Register ("WindowPreferences")]
	partial class WindowPreferences
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
