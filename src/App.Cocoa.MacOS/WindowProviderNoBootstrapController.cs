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
using Eddie.Core;

namespace Eddie.UI.Cocoa.Osx
{
	public partial class WindowProviderNoBootstrapController : NSWindowController
	{
        public static WindowProviderNoBootstrapController Singleton = null;
        public Provider Provider;

		public WindowProviderNoBootstrapController(IntPtr handle) : base(handle)
		{
		}

		[Export("initWithCoder:")]
		public WindowProviderNoBootstrapController(NSCoder coder) : base(coder)
		{
		}

		public WindowProviderNoBootstrapController() : base("WindowProviderNoBootstrap")
		{
            Singleton = this;
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();

			Window.Title = Constants.Name + " - " + LanguageManager.GetText("WindowsProviderNoBootstrapTitle");

			GuiUtils.SetButtonCancel(Window, CmdCancel);
            GuiUtils.SetButtonDefault(Window, CmdOk);

            GuiUtils.SetCheck(ChkDontShowAgain, false);
			LblBody.StringValue = LanguageManager.GetText("WindowsProviderNoBootstrapBody", Provider.Title);
			TxtManualUrls.StringValue = Engine.Instance.Storage.Get("bootstrap.urls");

			CmdOk.Activated += (object sender, EventArgs e) =>
			{
                Engine.Instance.Storage.SetBool("ui.skip.provider.manifest.failed", GuiUtils.GetCheck(ChkDontShowAgain));
				Engine.Instance.Storage.Set("bootstrap.urls", TxtManualUrls.StringValue);
				Engine.Instance.RefreshProvidersInvalidateConnections();

				Window.Close();
				NSApplication.SharedApplication.StopModal();
                Singleton = null;
			};

			CmdCancel.Activated += (object sender, EventArgs e) =>
			{
                Engine.Instance.Storage.SetBool("ui.skip.provider.manifest.failed", GuiUtils.GetCheck(ChkDontShowAgain));

				Window.Close();
				NSApplication.SharedApplication.StopModal();
                Singleton = null;
            };
		}

		public new WindowProviderNoBootstrap Window
		{
			get { return (WindowProviderNoBootstrap)base.Window; }
		}
	}
}
