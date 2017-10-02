// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Eddie.UI.Cocoa.Osx
{
	[Register ("WindowReportController")]
	partial class WindowReportController
	{
		[Outlet]
		AppKit.NSButton CmdClose { get; set; }

		[Outlet]
		AppKit.NSButton CmdCopyClipboard { get; set; }

		[Outlet]
		AppKit.NSButton CmdSave { get; set; }

		[Outlet]
		AppKit.NSTextField LblStep { get; set; }

		[Outlet]
		AppKit.NSProgressIndicator PgrStep { get; set; }

		[Outlet]
		AppKit.NSTextView TxtBody { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CmdClose != null) {
				CmdClose.Dispose ();
				CmdClose = null;
			}

			if (CmdCopyClipboard != null) {
				CmdCopyClipboard.Dispose ();
				CmdCopyClipboard = null;
			}

			if (CmdSave != null) {
				CmdSave.Dispose ();
				CmdSave = null;
			}

			if (TxtBody != null) {
				TxtBody.Dispose ();
				TxtBody = null;
			}

			if (LblStep != null) {
				LblStep.Dispose ();
				LblStep = null;
			}

			if (PgrStep != null) {
				PgrStep.Dispose ();
				PgrStep = null;
			}
		}
	}

	[Register ("WindowReport")]
	partial class WindowReport
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
