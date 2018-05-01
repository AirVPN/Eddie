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
	[Register ("WindowProviderNoBootstrapController")]
	partial class WindowProviderNoBootstrapController
	{
		[Outlet]
		AppKit.NSButton CmdCancel { get; set; }

		[Outlet]
		AppKit.NSButton CmdOk { get; set; }

		[Outlet]
		AppKit.NSTextField LblBody { get; set; }

		[Outlet]
		AppKit.NSTextField TxtManualUrls { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (LblBody != null) {
				LblBody.Dispose ();
				LblBody = null;
			}

			if (CmdOk != null) {
				CmdOk.Dispose ();
				CmdOk = null;
			}

			if (CmdCancel != null) {
				CmdCancel.Dispose ();
				CmdCancel = null;
			}

			if (TxtManualUrls != null) {
				TxtManualUrls.Dispose ();
				TxtManualUrls = null;
			}
		}
	}
}
