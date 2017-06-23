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
	[Register ("WindowAboutController")]
	partial class WindowAboutController
	{
		[Outlet]
		MonoMac.AppKit.NSButton CmdHomePage { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdLibraries { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdLicense { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdOk { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdSoftware { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdSources { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdSystemReport { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextFieldCell TxtVersion { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CmdHomePage != null) {
				CmdHomePage.Dispose ();
				CmdHomePage = null;
			}

			if (CmdLibraries != null) {
				CmdLibraries.Dispose ();
				CmdLibraries = null;
			}

			if (CmdLicense != null) {
				CmdLicense.Dispose ();
				CmdLicense = null;
			}

			if (CmdOk != null) {
				CmdOk.Dispose ();
				CmdOk = null;
			}

			if (CmdSoftware != null) {
				CmdSoftware.Dispose ();
				CmdSoftware = null;
			}

			if (CmdSources != null) {
				CmdSources.Dispose ();
				CmdSources = null;
			}

			if (CmdSystemReport != null) {
				CmdSystemReport.Dispose ();
				CmdSystemReport = null;
			}

			if (TxtVersion != null) {
				TxtVersion.Dispose ();
				TxtVersion = null;
			}
		}
	}

	[Register ("WindowAbout")]
	partial class WindowAbout
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
