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
using Eddie.Common;
using Eddie.Core;

namespace Eddie.UI.Cocoa.Osx
{
	public partial class WindowCredentialsController : AppKit.NSWindowController
	{
		public Credentials Credentials = null;

		#region Constructors

		// Called when created from unmanaged code
		public WindowCredentialsController(IntPtr handle) : base(handle)
		{
			Initialize();
		}

		// Called when created directly from a XIB file
		[Export("initWithCoder:")]
		public WindowCredentialsController(NSCoder coder) : base(coder)
		{
			Initialize();
		}

		// Call to load from the XIB/NIB file
		public WindowCredentialsController() : base("WindowCredentials")
		{
			Initialize();
		}

		// Shared initialization code
		void Initialize()
		{
		}

		#endregion

		//strongly typed window accessor
		public new WindowCredentials Window
		{
			get
			{
				return (WindowCredentials)base.Window;
			}
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();

			Window.Title = Constants.Name + " - " + Messages.WindowsLoginTitle;

			CboRemember.RemoveAllItems();
			CboRemember.AddItem(Messages.WindowsCredentialsRememberNo);
			CboRemember.AddItem(Messages.WindowsCredentialsRememberRun);
			CboRemember.AddItem(Messages.WindowsCredentialsRememberPermanent);
			GuiUtils.SetSelected(CboRemember, Messages.WindowsCredentialsRememberRun);

			TxtUsername.Changed += (object sender, EventArgs e) =>
			{
				EnableIde();
			};
			TxtPassword.Changed += (object sender, EventArgs e) =>
			{
				EnableIde();
			};

			CmdLogin.Activated += (object sender, EventArgs e) =>
			{
				Credentials = new Credentials();
				Credentials.Username = TxtUsername.StringValue;
				Credentials.Password = TxtPassword.StringValue;
				string rememberText = GuiUtils.GetSelected(CboRemember);
				if (rememberText == Messages.WindowsCredentialsRememberNo)
					Credentials.Remember = "no";
				else if (rememberText == Messages.WindowsCredentialsRememberRun)
					Credentials.Remember = "run";
				else if (rememberText == Messages.WindowsCredentialsRememberPermanent)
					Credentials.Remember = "permanent";
				else
					Credentials.Remember = "no";

				Window.Close();
				NSApplication.SharedApplication.StopModal();
			};

			CmdCancel.Activated += (object sender, EventArgs e) =>
			{
				Credentials = null;

				Window.Close();
				NSApplication.SharedApplication.StopModal();
			};

			EnableIde();
		}

		public void EnableIde()
		{
			bool acceptable = true;
			if (TxtUsername.StringValue.Trim() == "")
				acceptable = false;
			if (TxtPassword.StringValue.Trim() == "")
				acceptable = false;
			CmdLogin.Enabled = acceptable;
		}
	}
}
