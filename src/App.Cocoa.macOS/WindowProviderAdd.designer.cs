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
	[Register ("WindowProviderAddController")]
	partial class WindowProviderAddController
	{
		[Outlet]
		AppKit.NSPopUpButton CboProvider { get; set; }

		[Outlet]
		AppKit.NSButton CmdCancel { get; set; }

		[Outlet]
		AppKit.NSButton CmdOk { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CboProvider != null) {
				CboProvider.Dispose ();
				CboProvider = null;
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

	[Register ("WindowProviderAdd")]
	partial class WindowProviderAdd
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
