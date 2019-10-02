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

using Foundation;
using AppKit;

using Eddie.Common;
using Eddie.Core;

namespace Eddie.UI.Cocoa.Osx
{
    public partial class WindowUnlockController : NSWindowController
    {
        public bool AuthFailed = false;
        public string Body = "";

        public WindowUnlockController(IntPtr handle) : base(handle)
        {
        }

        [Export("initWithCoder:")]
        public WindowUnlockController(NSCoder coder) : base(coder)
        {
        }

        public WindowUnlockController() : base("WindowUnlock")
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            Window.Title = Constants.Name + " - " + LanguageManager.GetText("WindowsUnlockTitle");

			GuiUtils.SetButtonCancel(Window, CmdCancel);
            GuiUtils.SetButtonDefault(Window, CmdOk);

            if(AuthFailed)
            {
                LblMessage.StringValue = LanguageManager.GetText("WindowsUnlockFailedAuth");
				LblMessage.TextColor = NSColor.Red;
            }
            else
            {
                LblMessage.StringValue = LanguageManager.GetText("WindowsUnlockFirstAuth");
			}

            CmdCancel.Activated += (object sender, EventArgs e) =>
            {
                Body = "";
                Window.Close();
                NSApplication.SharedApplication.StopModal();
            };

            CmdOk.Activated += (object sender, EventArgs e) =>
            {
                Body = TxtPassword.StringValue;
                Window.Close();
                NSApplication.SharedApplication.StopModal();
            };

            TxtPassword.Changed += (object sender, EventArgs e) => 
            {
                EnableIde();
            };

            EnableIde();
        }

        public new WindowUnlock Window
        {
            get { return (WindowUnlock)base.Window; }
        }

        public void EnableIde()
        {
            CmdOk.Enabled = (TxtPassword.StringValue != "");
        }
    }
}
