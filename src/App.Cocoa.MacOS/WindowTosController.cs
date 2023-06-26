// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2023 AirVPN (support@airvpn.org) / https://airvpn.org )
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
//using Foundation;
//using AppKit;
using Foundation;
using AppKit;
using Eddie.Core;

namespace Eddie.UI.Cocoa.Osx
{
	public partial class WindowTosController : AppKit.NSWindowController
	{
		public bool Accepted = false;

		#region Constructors
		// Called when created from unmanaged code
		public WindowTosController(IntPtr handle) : base(handle)
		{
			Initialize();
		}
		// Called when created directly from a XIB file
		[Export("initWithCoder:")]
		public WindowTosController(NSCoder coder) : base(coder)
		{
			Initialize();
		}
		// Call to load from the XIB/NIB file
		public WindowTosController() : base("WindowTos")
		{
			Initialize();
		}
		// Shared initialization code
		void Initialize()
		{
		}
		#endregion
		//strongly typed window accessor
		public new WindowTos Window
		{
			get
			{
				return (WindowTos)base.Window;
			}
		}


		public override void AwakeFromNib()
		{
			base.AwakeFromNib();

			Accepted = false;

			Window.Title = Constants.Name + " - " + LanguageManager.GetText(LanguageItems.WindowsTosTitle);

			GuiUtils.SetButtonCancel(Window, CmdCancel);
            GuiUtils.SetButtonDefault(Window, CmdAccept);

            // TOCLEAN
            /*
            TxtTos.Value = Core.UI.App.GetTos();
            */
			ChkTos1.Title = LanguageManager.GetText(LanguageItems.WindowsTosCheck1);
			ChkTos2.Title = LanguageManager.GetText(LanguageItems.WindowsTosCheck2);
			CmdAccept.Title = LanguageManager.GetText(LanguageItems.WindowsTosAccept);
			CmdCancel.StringValue = LanguageManager.GetText(LanguageItems.WindowsTosReject);

			bool tosAccepted = Engine.Instance.ProfileOptions.GetBool("gui.tos");
			ChkTos1.State = (tosAccepted ? NSCellStateValue.On : NSCellStateValue.Off);
			ChkTos2.State = (tosAccepted ? NSCellStateValue.On : NSCellStateValue.Off);

			CmdAccept.Activated += (object sender, EventArgs e) =>
			{
				Accepted = true;
				Window.Close();
				NSApplication.SharedApplication.StopModal();
			};

			CmdCancel.Activated += (object sender, EventArgs e) =>
			{
				Window.Close();
				NSApplication.SharedApplication.StopModal();
			};
		}
	}
}

