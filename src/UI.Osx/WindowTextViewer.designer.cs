// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace AirVPN.UI.Osx
{
	[Register ("WindowTextViewerController")]
	partial class WindowTextViewerController
	{
		[Outlet]
		MonoMac.AppKit.NSButton CmdOk { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField TxtBody { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CmdOk != null) {
				CmdOk.Dispose ();
				CmdOk = null;
			}

			if (TxtBody != null) {
				TxtBody.Dispose ();
				TxtBody = null;
			}
		}
	}

	[Register ("WindowTextViewer")]
	partial class WindowTextViewer
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
