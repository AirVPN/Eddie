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
	[Register ("WindowPreferencesIpController")]
	partial class WindowPreferencesIpController
	{
		[Outlet]
		AppKit.NSButton CmdCancel { get; set; }

		[Outlet]
		AppKit.NSButton CmdOk { get; set; }

		[Outlet]
		AppKit.NSTextField TxtIP { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CmdCancel != null) {
				CmdCancel.Dispose ();
				CmdCancel = null;
			}

			if (CmdOk != null) {
				CmdOk.Dispose ();
				CmdOk = null;
			}

			if (TxtIP != null) {
				TxtIP.Dispose ();
				TxtIP = null;
			}
		}
	}

	[Register ("WindowPreferencesIp")]
	partial class WindowPreferencesIp
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
