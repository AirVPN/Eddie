// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;
using System.CodeDom.Compiler;

namespace Eddie.UI.Cocoa.Osx
{
	[Register ("WindowPreferencesRouteController")]
	partial class WindowPreferencesRouteController
	{
		[Outlet]
		MonoMac.AppKit.NSPopUpButton CboAction { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdCancel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdOk { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField LblHelp { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField TxtIP { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField TxtNotes { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (TxtIP != null) {
				TxtIP.Dispose ();
				TxtIP = null;
			}

			if (LblHelp != null) {
				LblHelp.Dispose ();
				LblHelp = null;
			}

			if (CboAction != null) {
				CboAction.Dispose ();
				CboAction = null;
			}

			if (TxtNotes != null) {
				TxtNotes.Dispose ();
				TxtNotes = null;
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

	[Register ("WindowPreferencesRoute")]
	partial class WindowPreferencesRoute
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
