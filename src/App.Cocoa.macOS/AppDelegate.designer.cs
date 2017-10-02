// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Eddie.UI.Cocoa.Osx
{
	[Register ("AppDelegate")]
	partial class AppDelegate
	{
		[Outlet]
		AppKit.NSMenuItem MnuMainAbout { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuMainClientArea { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuMainForwardingPorts { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuMainHome { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuMainPreferences { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuMainQuit { get; set; }

		[Outlet]
		AppKit.NSMenuItem MnuMainSpeedTest { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (MnuMainAbout != null) {
				MnuMainAbout.Dispose ();
				MnuMainAbout = null;
			}

			if (MnuMainClientArea != null) {
				MnuMainClientArea.Dispose ();
				MnuMainClientArea = null;
			}

			if (MnuMainForwardingPorts != null) {
				MnuMainForwardingPorts.Dispose ();
				MnuMainForwardingPorts = null;
			}

			if (MnuMainHome != null) {
				MnuMainHome.Dispose ();
				MnuMainHome = null;
			}

			if (MnuMainPreferences != null) {
				MnuMainPreferences.Dispose ();
				MnuMainPreferences = null;
			}

			if (MnuMainQuit != null) {
				MnuMainQuit.Dispose ();
				MnuMainQuit = null;
			}

			if (MnuMainSpeedTest != null) {
				MnuMainSpeedTest.Dispose ();
				MnuMainSpeedTest = null;
			}
		}
	}
}
