// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace AirVPN.Mac
{
	[Register ("WindowAboutController")]
	partial class WindowAboutController
	{
		[Outlet]
		MonoMac.AppKit.NSButton CmdOk { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextView TxtLicense { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CmdOk != null) {
				CmdOk.Dispose ();
				CmdOk = null;
			}

			if (TxtLicense != null) {
				TxtLicense.Dispose ();
				TxtLicense = null;
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
