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
	[Register ("WindowProviderEditOpenVPNController")]
	partial class WindowProviderEditOpenVPNController
	{
		[Outlet]
		AppKit.NSButton ChkEnabled { get; set; }

		[Outlet]
		AppKit.NSButton ChkSupportIPv6 { get; set; }

		[Outlet]
		AppKit.NSButton CmdCancel { get; set; }

		[Outlet]
		AppKit.NSButton CmdOk { get; set; }

		[Outlet]
		AppKit.NSButton CmdPathBrowse { get; set; }

		[Outlet]
		AppKit.NSTextField LblSubtitle { get; set; }

		[Outlet]
		AppKit.NSButton LblTitle { get; set; }

		[Outlet]
		AppKit.NSSecureTextField TxtAuthPassPassword { get; set; }

		[Outlet]
		AppKit.NSTextField TxtAuthPassUsername { get; set; }

		[Outlet]
		AppKit.NSTextField TxtPath { get; set; }

		[Outlet]
		AppKit.NSTextField TxtTitle { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ChkEnabled != null) {
				ChkEnabled.Dispose ();
				ChkEnabled = null;
			}

			if (CmdCancel != null) {
				CmdCancel.Dispose ();
				CmdCancel = null;
			}

			if (CmdOk != null) {
				CmdOk.Dispose ();
				CmdOk = null;
			}

			if (CmdPathBrowse != null) {
				CmdPathBrowse.Dispose ();
				CmdPathBrowse = null;
			}

			if (LblSubtitle != null) {
				LblSubtitle.Dispose ();
				LblSubtitle = null;
			}

			if (LblTitle != null) {
				LblTitle.Dispose ();
				LblTitle = null;
			}

			if (TxtAuthPassPassword != null) {
				TxtAuthPassPassword.Dispose ();
				TxtAuthPassPassword = null;
			}

			if (TxtAuthPassUsername != null) {
				TxtAuthPassUsername.Dispose ();
				TxtAuthPassUsername = null;
			}

			if (TxtPath != null) {
				TxtPath.Dispose ();
				TxtPath = null;
			}

			if (ChkSupportIPv6 != null) {
				ChkSupportIPv6.Dispose ();
				ChkSupportIPv6 = null;
			}

			if (TxtTitle != null) {
				TxtTitle.Dispose ();
				TxtTitle = null;
			}
		}
	}

	[Register ("WindowProviderEditOpenVPN")]
	partial class WindowProviderEditOpenVPN
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
