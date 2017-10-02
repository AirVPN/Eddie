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
	[Register ("WindowConnectionController")]
	partial class WindowConnectionController
	{
		[Outlet]
		AppKit.NSButton CmdCancel { get; set; }

		[Outlet]
		AppKit.NSButton CmdOk { get; set; }

		[Outlet]
		AppKit.NSTabView TabMain { get; set; }

		[Outlet]
		AppKit.NSTextView TxtOvpnGenerated { get; set; }

		[Outlet]
		AppKit.NSTextView TxtOvpnOriginal { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (TabMain != null) {
				TabMain.Dispose ();
				TabMain = null;
			}

			if (TxtOvpnGenerated != null) {
				TxtOvpnGenerated.Dispose ();
				TxtOvpnGenerated = null;
			}

			if (TxtOvpnOriginal != null) {
				TxtOvpnOriginal.Dispose ();
				TxtOvpnOriginal = null;
			}

			if (CmdOk != null) {
				CmdOk.Dispose ();
				CmdOk = null;
			}

			if (CmdCancel != null) {
				CmdCancel.Dispose ();
				CmdCancel = null;
			}
		}
	}

	[Register ("WindowConnection")]
	partial class WindowConnection
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
