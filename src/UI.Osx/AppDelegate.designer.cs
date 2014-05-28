// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace AirVPN.UI.Osx
{
	[Register ("AppDelegate")]
	partial class AppDelegate
	{
		[Outlet]
		MonoMac.AppKit.NSMenuItem MnuMainAbout { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem MnuMainClientArea { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem MnuMainForwardingPorts { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem MnuMainHome { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem MnuMainPreferences { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem MnuMainSpeedTest { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (MnuMainAbout != null) {
				MnuMainAbout.Dispose ();
				MnuMainAbout = null;
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

			if (MnuMainClientArea != null) {
				MnuMainClientArea.Dispose ();
				MnuMainClientArea = null;
			}

			if (MnuMainSpeedTest != null) {
				MnuMainSpeedTest.Dispose ();
				MnuMainSpeedTest = null;
			}
		}
	}
}
