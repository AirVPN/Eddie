// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;

namespace Eddie.UI.Cocoa.Osx
{
	[Register ("WindowTosController")]
	partial class WindowTosController
	{
		[Outlet]
		AppKit.NSButton ChkTos1 { get; set; }

		[Outlet]
		AppKit.NSButton ChkTos2 { get; set; }

		[Outlet]
		AppKit.NSButton CmdAccept { get; set; }

		[Outlet]
		AppKit.NSButton CmdCancel { get; set; }

		[Outlet]
		AppKit.NSTextView TxtTos { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (TxtTos != null) {
				TxtTos.Dispose ();
				TxtTos = null;
			}

			if (ChkTos1 != null) {
				ChkTos1.Dispose ();
				ChkTos1 = null;
			}

			if (ChkTos2 != null) {
				ChkTos2.Dispose ();
				ChkTos2 = null;
			}

			if (CmdAccept != null) {
				CmdAccept.Dispose ();
				CmdAccept = null;
			}

			if (CmdCancel != null) {
				CmdCancel.Dispose ();
				CmdCancel = null;
			}
		}
	}

	[Register ("WindowTos")]
	partial class WindowTos
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
