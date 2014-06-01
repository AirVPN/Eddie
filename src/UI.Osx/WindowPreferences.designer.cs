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
	[Register ("WindowPreferencesController")]
	partial class WindowPreferencesController
	{
		[Outlet]
		MonoMac.AppKit.NSPopUpButton CboProxyAuthentication { get; set; }

		[Outlet]
		MonoMac.AppKit.NSPopUpButton CboProxyType { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton ChkAutoStart { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton ChkGeneralOsxNotifications { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton ChkGeneralStartLast { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton ChkModeSsh22 { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton ChkModeSsl443 { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton ChkModeTcp443 { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton ChkModeUdp443 { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdAdvancedDocs { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdCancel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdGeneralTos { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdModeHelp { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdSave { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField TxtProxyHost { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField TxtProxyLogin { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField TxtProxyPassword { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField TxtProxyPort { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ChkModeUdp443 != null) {
				ChkModeUdp443.Dispose ();
				ChkModeUdp443 = null;
			}

			if (CmdModeHelp != null) {
				CmdModeHelp.Dispose ();
				CmdModeHelp = null;
			}

			if (ChkModeSsh22 != null) {
				ChkModeSsh22.Dispose ();
				ChkModeSsh22 = null;
			}

			if (ChkModeSsl443 != null) {
				ChkModeSsl443.Dispose ();
				ChkModeSsl443 = null;
			}

			if (CboProxyAuthentication != null) {
				CboProxyAuthentication.Dispose ();
				CboProxyAuthentication = null;
			}

			if (ChkModeTcp443 != null) {
				ChkModeTcp443.Dispose ();
				ChkModeTcp443 = null;
			}

			if (CboProxyType != null) {
				CboProxyType.Dispose ();
				CboProxyType = null;
			}

			if (ChkAutoStart != null) {
				ChkAutoStart.Dispose ();
				ChkAutoStart = null;
			}

			if (ChkGeneralOsxNotifications != null) {
				ChkGeneralOsxNotifications.Dispose ();
				ChkGeneralOsxNotifications = null;
			}

			if (ChkGeneralStartLast != null) {
				ChkGeneralStartLast.Dispose ();
				ChkGeneralStartLast = null;
			}

			if (CmdAdvancedDocs != null) {
				CmdAdvancedDocs.Dispose ();
				CmdAdvancedDocs = null;
			}

			if (CmdCancel != null) {
				CmdCancel.Dispose ();
				CmdCancel = null;
			}

			if (CmdGeneralTos != null) {
				CmdGeneralTos.Dispose ();
				CmdGeneralTos = null;
			}

			if (CmdSave != null) {
				CmdSave.Dispose ();
				CmdSave = null;
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
