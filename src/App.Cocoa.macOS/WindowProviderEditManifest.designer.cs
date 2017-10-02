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
	[Register ("WindowProviderEditManifestController")]
	partial class WindowProviderEditManifestController
	{
		[Outlet]
		AppKit.NSButton ChkEnabled { get; set; }

		[Outlet]
		AppKit.NSButton CmdCancel { get; set; }

		[Outlet]
		AppKit.NSButton CmdOk { get; set; }

		[Outlet]
		AppKit.NSTextField LblSubtitle { get; set; }

		[Outlet]
		AppKit.NSButton LblTitle { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ChkEnabled != null) {
				ChkEnabled.Dispose ();
				ChkEnabled = null;
			}

			if (CmdCancel != null) {
				CmdCancel.Dispose ();
				CmdCancel = null;
			}

			if (CmdOk != null) {
				CmdOk.Dispose ();
				CmdOk = null;
			}

			if (LblTitle != null) {
				LblTitle.Dispose ();
				LblTitle = null;
			}

			if (LblSubtitle != null) {
				LblSubtitle.Dispose ();
				LblSubtitle = null;
			}
		}
	}

	[Register ("WindowProviderEditManifest")]
	partial class WindowProviderEditManifest
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
