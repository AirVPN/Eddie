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
	public partial class WindowPreferencesIpController : AppKit.NSWindowController
	{
		public bool Accepted = false;
		public static string Ip;



		#region Constructors

		// Called when created from unmanaged code
		public WindowPreferencesIpController(IntPtr handle) : base(handle)
		{
			Initialize();
		}

		// Called when created directly from a XIB file
		[Export("initWithCoder:")]
		public WindowPreferencesIpController(NSCoder coder) : base(coder)
		{
			Initialize();
		}

		// Call to load from the XIB/NIB file
		public WindowPreferencesIpController() : base("WindowPreferencesIp")
		{
			Initialize();
		}

		// Shared initialization code
		void Initialize()
		{
		}

		#endregion

		//strongly typed window accessor
		public new WindowPreferencesIp Window
		{
			get
			{
				return (WindowPreferencesIp)base.Window;
			}
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();

			Window.Title = Constants.Name + " - " + LanguageManager.GetText("WindowsSettingsIpTitle");

			GuiUtils.SetButtonCancel(Window, CmdCancel);
            GuiUtils.SetButtonDefault(Window, CmdOk);


            TxtIP.Changed += (object sender, EventArgs e) =>
			{
				EnableIde();
			};
			CmdOk.Activated += (object sender, EventArgs e) =>
			{

				Accepted = true;
				Ip = TxtIP.StringValue;

				Window.Close();
				NSApplication.SharedApplication.StopModal();
			};

			CmdCancel.Activated += (object sender, EventArgs e) =>
			{

				Accepted = false;

				Window.Close();
				NSApplication.SharedApplication.StopModal();
			};

			TxtIP.StringValue = Ip;

			EnableIde();
		}

		public void EnableIde()
		{
			if (new IpAddress(TxtIP.StringValue).Valid == false)
			{
				CmdOk.Enabled = false;
			}
			else
			{
				CmdOk.Enabled = true;
			}

		}
	}
}

