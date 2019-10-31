// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2016 AirVPN (support@airvpn.org) / https://airvpn.org )
//
// Eddie is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Eddie is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Eddie. If not, see <http://www.gnu.org/licenses/>.
// </eddie_source_header>

using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;
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

			Window.Title = Constants.Name + " - " + LanguageManager.GetText("WindowsReportTitle");

			GuiUtils.SetButtonCancel(Window, CmdClose);
            GuiUtils.SetButtonDefault(Window, CmdSave);

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
				GuiUtils.MessageBoxInfo(LanguageManager.GetText("LogsCopyClipboardDone"));
			};

			CmdSave.Activated += (object sender, EventArgs e) =>
			{
				string t = TxtBody.Value;

				NSSavePanel panel = new NSSavePanel();
				panel.CanCreateDirectories = true;
				nint result = panel.RunModal();
				if (result == 1)
				{
                    Core.Platform.Instance.FileContentsWriteText(panel.Url.Path, t, System.Text.Encoding.UTF8);

					GuiUtils.MessageBoxInfo(LanguageManager.GetText("LogsSaveToFileDone"));
				}
			};

			CmdCopyClipboard.ToolTip = LanguageManager.GetText("TooltipLogsCopy");
			CmdSave.ToolTip = LanguageManager.GetText("TooltipLogsSave");
		}

		public void SetStep(string step, string text, int perc)
		{
			LblStep.StringValue = step;
			PgrStep.DoubleValue = perc;
			TxtBody.Value = text;
		}
	}
}
