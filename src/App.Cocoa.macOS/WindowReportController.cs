
using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;
using Eddie.Lib.Common;
using Eddie.Core;

namespace Eddie.UI.Cocoa.Osx
{
	public partial class WindowReportController : AppKit.NSWindowController
	{
		#region Constructors

		// Called when created from unmanaged code
		public WindowReportController(IntPtr handle) : base(handle)
		{
			Initialize();
		}

		// Called when created directly from a XIB file
		[Export("initWithCoder:")]
		public WindowReportController(NSCoder coder) : base(coder)
		{
			Initialize();
		}

		// Call to load from the XIB/NIB file
		public WindowReportController() : base("WindowReport")
		{
			Initialize();
		}

		// Shared initialization code
		void Initialize()
		{
		}

		#endregion

		//strongly typed window accessor
		public new WindowReport Window
		{
			get
			{
				return (WindowReport)base.Window;
			}
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();

			Window.Title = Constants.Name + " - " + Messages.WindowsReportTitle;

			CmdClose.Activated += (object sender, EventArgs e) =>
			{
				Window.Close();
				NSApplication.SharedApplication.StopModal();
			};

			CmdCopyClipboard.Activated += (object sender, EventArgs e) =>
			{
				string t = TxtBody.Value;

				string[] pboardTypes = new string[] { "NSStringPboardType" };
				NSPasteboard.GeneralPasteboard.DeclareTypes(pboardTypes, null);
				NSPasteboard.GeneralPasteboard.SetStringForType(t, pboardTypes[0]);
				GuiUtils.MessageBoxInfo(Messages.LogsCopyClipboardDone);
			};

			CmdSave.Activated += (object sender, EventArgs e) =>
			{
				string t = TxtBody.Value;

				NSSavePanel panel = new NSSavePanel();
				panel.CanCreateDirectories = true;
				nint result = panel.RunModal();
				if (result == 1)
				{
					Core.Platform.Instance.FileContentsWriteText(panel.Url.Path, t);

					GuiUtils.MessageBoxInfo(Messages.LogsSaveToFileDone);
				}
			};

			CmdCopyClipboard.ToolTip = Messages.TooltipLogsCopy;
			CmdSave.ToolTip = Messages.TooltipLogsSave;
		}

		public void SetStep(string step, string text, int perc)
		{
			LblStep.StringValue = step;
			PgrStep.DoubleValue = perc;
			TxtBody.Value = text;
		}
	}
}
