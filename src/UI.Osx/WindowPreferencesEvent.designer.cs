// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;
using System.CodeDom.Compiler;

namespace Eddie.UI.Osx
{
	[Register ("WindowPreferencesEventController")]
	partial class WindowPreferencesEventController
	{
		[Outlet]
		MonoMac.AppKit.NSButton ChkWaitEnd { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdBrowse { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdCancel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CmdSave { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField TxtArguments { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField TxtFilename { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (TxtFilename != null) {
				TxtFilename.Dispose ();
				TxtFilename = null;
			}

			if (TxtArguments != null) {
				TxtArguments.Dispose ();
				TxtArguments = null;
			}

			if (ChkWaitEnd != null) {
				ChkWaitEnd.Dispose ();
				ChkWaitEnd = null;
			}

			if (CmdSave != null) {
				CmdSave.Dispose ();
				CmdSave = null;
			}

			if (CmdCancel != null) {
				CmdCancel.Dispose ();
				CmdCancel = null;
			}

			if (CmdBrowse != null) {
				CmdBrowse.Dispose ();
				CmdBrowse = null;
			}
		}
	}

	[Register ("WindowPreferencesEvent")]
	partial class WindowPreferencesEvent
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
