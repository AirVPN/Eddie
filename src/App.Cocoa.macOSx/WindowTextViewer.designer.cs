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
	[Register ("WindowTextViewerController")]
	partial class WindowTextViewerController
	{
		[Outlet]
		AppKit.NSButton CmdOk { get; set; }

		[Outlet]
		AppKit.NSTextView TxtBody2 { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CmdOk != null) {
				CmdOk.Dispose ();
				CmdOk = null;
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
