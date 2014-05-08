// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace AirVPN.Mac
{
	[Register ("AppDelegate")]
	partial class AppDelegate
	{
		[Outlet]
		MonoMac.AppKit.NSMenuItem MnuMainAbout { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (MnuMainAbout != null) {
				MnuMainAbout.Dispose ();
				MnuMainAbout = null;
			}
		}
	}
}
