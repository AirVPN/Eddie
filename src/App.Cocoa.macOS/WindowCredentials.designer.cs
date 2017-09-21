// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;
using System.CodeDom.Compiler;

namespace Eddie.UI.Cocoa.Osx
{
	[Register ("WindowCredentialsController")]
	partial class WindowCredentialsController
	{
		[Outlet]
		MonoMac.AppKit.NSPopUpButton CboRemember { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdCancel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdLogin { get; set; }

		[Outlet]
		MonoMac.AppKit.NSSecureTextField TxtPassword { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField TxtUsername { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (TxtUsername != null) {
				TxtUsername.Dispose ();
				TxtUsername = null;
			}

			if (TxtPassword != null) {
				TxtPassword.Dispose ();
				TxtPassword = null;
			}

			if (CboRemember != null) {
				CboRemember.Dispose ();
				CboRemember = null;
			}

			if (CmdLogin != null) {
				CmdLogin.Dispose ();
				CmdLogin = null;
			}

			if (CmdCancel != null) {
				CmdCancel.Dispose ();
				CmdCancel = null;
			}
		}
	}

	[Register ("WindowCredentials")]
	partial class WindowCredentials
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
