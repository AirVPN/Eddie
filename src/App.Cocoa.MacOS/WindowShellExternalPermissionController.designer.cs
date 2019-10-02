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
	[Register ("WindowShellExternalPermissionController")]
	partial class WindowShellExternalPermissionController
	{
		[Outlet]
		AppKit.NSButton CmdNo { get; set; }

		[Outlet]
		AppKit.NSButton CmdRuleAll { get; set; }

		[Outlet]
		AppKit.NSButton CmdRuleHash { get; set; }

		[Outlet]
		AppKit.NSButton CmdRulePath { get; set; }

		[Outlet]
		AppKit.NSButton CmdRuleSign { get; set; }

		[Outlet]
		AppKit.NSButton CmdYes { get; set; }

		[Outlet]
		AppKit.NSTextField LblMessage { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CmdNo != null) {
				CmdNo.Dispose ();
				CmdNo = null;
			}

			if (CmdYes != null) {
				CmdYes.Dispose ();
				CmdYes = null;
			}

			if (CmdRuleAll != null) {
				CmdRuleAll.Dispose ();
				CmdRuleAll = null;
			}

			if (CmdRuleHash != null) {
				CmdRuleHash.Dispose ();
				CmdRuleHash = null;
			}

			if (CmdRulePath != null) {
				CmdRulePath.Dispose ();
				CmdRulePath = null;
			}

			if (CmdRuleSign != null) {
				CmdRuleSign.Dispose ();
				CmdRuleSign = null;
			}

			if (LblMessage != null) {
				LblMessage.Dispose ();
				LblMessage = null;
			}
		}
	}
}
