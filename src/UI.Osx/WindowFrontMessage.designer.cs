// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace AirVPN.UI.Osx
{
	[Register ("WindowFrontMessageController")]
	partial class WindowFrontMessageController
	{
		[Outlet]
		MonoMac.AppKit.NSButton CmdClose { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdMore { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField TxtMessage { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CmdClose != null) {
				CmdClose.Dispose ();
				CmdClose = null;
			}

			if (CmdMore != null) {
				CmdMore.Dispose ();
				CmdMore = null;
			}

			if (TxtMessage != null) {
				TxtMessage.Dispose ();
				TxtMessage = null;
			}
		}
	}

	[Register ("WindowFrontMessage")]
	partial class WindowFrontMessage
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
