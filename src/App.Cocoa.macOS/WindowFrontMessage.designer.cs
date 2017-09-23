// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
//using Foundation;
//using AppKit;
using Foundation;
using AppKit;

namespace Eddie.UI.Cocoa.Osx
{
	[Register("WindowFrontMessageController")]
	partial class WindowFrontMessageController
	{
		[Outlet]
		AppKit.NSButton CmdClose { get; set; }

		[Outlet]
		AppKit.NSButton CmdMore { get; set; }

		[Outlet]
		AppKit.NSTextField TxtMessage { get; set; }

		void ReleaseDesignerOutlets()
		{
			if (CmdClose != null)
			{
				CmdClose.Dispose();
				CmdClose = null;
			}

			if (CmdMore != null)
			{
				CmdMore.Dispose();
				CmdMore = null;
			}

			if (TxtMessage != null)
			{
				TxtMessage.Dispose();
				TxtMessage = null;
			}
		}
	}

	[Register("WindowFrontMessage")]
	partial class WindowFrontMessage
	{

		void ReleaseDesignerOutlets()
		{
		}
	}
}
