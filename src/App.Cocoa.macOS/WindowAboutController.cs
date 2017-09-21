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
using MonoMac.Foundation;
using MonoMac.AppKit;
using Eddie.Lib.Common;
using Eddie.Core;

namespace Eddie.UI.Cocoa.Osx
{
	public partial class WindowAboutController : MonoMac.AppKit.NSWindowController
	{
		#region Constructors
		// Called when created from unmanaged code
		public WindowAboutController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public WindowAboutController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		// Call to load from the XIB/NIB file
		public WindowAboutController () : base ("WindowAbout")
		{
			Initialize ();
		}
		// Shared initialization code
		void Initialize ()
		{
		}
		#endregion
		//strongly typed window accessor
		public new WindowAbout Window {
			get {
				return (WindowAbout)base.Window;
			}
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib ();

			Window.Title = Constants.Name + " - " + Messages.WindowsAboutTitle;

			TxtVersion.StringValue = Messages.WindowsAboutVersion + " " + Constants.VersionDesc;

			CmdHomePage.Activated += (object sender, EventArgs e) =>
			{
				Engine.Instance.Command ("ui.show.website");
			};

			CmdSoftware.Activated += (object sender, EventArgs e) =>
			{
				Engine.Instance.Command ("ui.show.docs.general");
			};

			CmdSources.Activated += (object sender, EventArgs e) =>
			{
				Engine.Instance.Command ("ui.show.sources");
			};

			CmdLicense.Activated += (object sender, EventArgs e) =>
			{
				Engine.Instance.Command ("ui.show.license");
			};

			CmdLibraries.Activated += (object sender, EventArgs e) =>
			{
				Engine.Instance.Command ("ui.show.libraries");
			};

			CmdOk.Activated += (object sender, EventArgs e) =>
			{
				this.Close();
				NSApplication.SharedApplication.StopModal();
			};

			CmdSystemReport.Activated += (object sender, EventArgs e) =>
			{
                Engine.Instance.GenerateSystemReport();
			};
		}
	}
}

