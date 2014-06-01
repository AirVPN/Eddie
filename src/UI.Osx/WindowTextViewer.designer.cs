// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;
using System.CodeDom.Compiler;

namespace AirVPN.UI.Osx
{
	[Register ("WindowTextViewerController")]
	partial class WindowTextViewerController
	{
		[Outlet]
		MonoMac.AppKit.NSButton CmdOk { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField TxtBody { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextView TxtBody2 { get; set; }
		
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

			if (TxtBody2 != null) {
				TxtBody2.Dispose ();
				TxtBody2 = null;
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
