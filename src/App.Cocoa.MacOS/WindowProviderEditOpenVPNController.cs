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
	public partial class WindowProviderEditOpenVPNController : AppKit.NSWindowController
	{
		public Core.Providers.OpenVPN Provider;

		#region Constructors

		// Called when created from unmanaged code
		public WindowProviderEditOpenVPNController(IntPtr handle) : base(handle)
		{
			Initialize();
		}

		// Called when created directly from a XIB file
		[Export("initWithCoder:")]
		public WindowProviderEditOpenVPNController(NSCoder coder) : base(coder)
		{
			Initialize();
		}

		// Call to load from the XIB/NIB file
		public WindowProviderEditOpenVPNController() : base("WindowProviderEditOpenVPN")
		{
			Initialize();
		}

		// Shared initialization code
		void Initialize()
		{
		}

		#endregion

		//strongly typed window accessor
		public new WindowProviderEditOpenVPN Window
		{
			get
			{
				return (WindowProviderEditOpenVPN)base.Window;
			}
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();

			Window.Title = Constants.Name + " - " + LanguageManager.GetText("WindowsProviderEditOpenVPNTitle");

			GuiUtils.SetButtonCancel(Window, CmdCancel);
            GuiUtils.SetButtonDefault(Window, CmdOk);

            LblTitle.Title = Provider.DefinitionTitle;
			LblSubtitle.StringValue = Provider.DefinitionSubTitle;

			GuiUtils.SetCheck(ChkEnabled, Provider.Enabled);
			TxtTitle.StringValue = Provider.Title;
			TxtPath.StringValue = Provider.Path;

			ChkSupportIPv6.Title = LanguageManager.GetText("WindowsProviderEditOpenVPNSupportIPv6");
			GuiUtils.SetCheck(ChkSupportIPv6, Provider.SupportIPv6);

			TxtAuthPassUsername.StringValue = Provider.AuthPassUsername;
			TxtAuthPassPassword.StringValue = Provider.AuthPassPassword;

			LblTitle.Activated += (object sender, EventArgs e) =>
			{
				Core.Platform.Instance.OpenUrl(Provider.DefinitionHref);
			};

			CmdPathBrowse.Activated += (object sender, EventArgs e) =>
			{
				GuiUtils.SelectPath(this.Window, TxtPath);
			};

			CmdOk.Activated += (object sender, EventArgs e) =>
			{
				Provider.Enabled = GuiUtils.GetCheck(ChkEnabled);
				Provider.Title = TxtTitle.StringValue;
				Provider.Path = TxtPath.StringValue;
				Provider.SupportIPv6 = GuiUtils.GetCheck(ChkSupportIPv6);

				Provider.AuthPassUsername = TxtAuthPassUsername.StringValue;
				Provider.AuthPassPassword = TxtAuthPassPassword.StringValue;

				Window.Close();
				NSApplication.SharedApplication.StopModal();
			};

			CmdCancel.Activated += (object sender, EventArgs e) =>
			{
				Provider = null;

				Window.Close();
				NSApplication.SharedApplication.StopModal();
			};
		}
	}
}
