// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;
using System.CodeDom.Compiler;

namespace Eddie.UI.Osx
{
	[Register ("WindowAboutController")]
	partial class WindowAboutController
	{
		[Outlet]
		MonoMac.AppKit.NSButton CmdHomePage { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdOk { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdSoftware { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdSources { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextView TxtLibraries { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextView TxtLicense { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextFieldCell TxtVersion { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CmdSources != null) {
				CmdSources.Dispose ();
				CmdSources = null;
			}

			if (CmdHomePage != null) {
				CmdHomePage.Dispose ();
				CmdHomePage = null;
			}

			if (CmdOk != null) {
				CmdOk.Dispose ();
				CmdOk = null;
			}

			if (CmdSoftware != null) {
				CmdSoftware.Dispose ();
				CmdSoftware = null;
			}

			if (TxtLibraries != null) {
				TxtLibraries.Dispose ();
				TxtLibraries = null;
			}

			if (TxtLicense != null) {
				TxtLicense.Dispose ();
				TxtLicense = null;
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
