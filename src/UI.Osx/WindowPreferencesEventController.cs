// <airvpn_source_header>
// This file is part of AirVPN Client software.
// Copyright (C)2014-2014 AirVPN (support@airvpn.org) / https://airvpn.org )
//
// AirVPN Client is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// AirVPN Client is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with AirVPN Client. If not, see <http://www.gnu.org/licenses/>.
// </airvpn_source_header>

using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using AirVPN.Core;

namespace AirVPN.UI.Osx
{
	public partial class WindowPreferencesEventController : MonoMac.AppKit.NSWindowController
	{
		public bool Accepted;

		public static TableAdvancedEventsControllerItem Item;

		#region Constructors

		// Called when created from unmanaged code
		public WindowPreferencesEventController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public WindowPreferencesEventController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		
		// Call to load from the XIB/NIB file
		public WindowPreferencesEventController () : base ("WindowPreferencesEvent")
		{
			Initialize ();
		}
		
		// Shared initialization code
		void Initialize ()
		{
		}

		#endregion

		//strongly typed window accessor
		public new WindowPreferencesEvent Window {
			get {
				return (WindowPreferencesEvent)base.Window;
			}
		}

		public override void AwakeFromNib ()
		{
			base.AwakeFromNib ();
		
			Window.Title = Constants.Name + " - " + Messages.WindowsSettingsEventTitle;

			TxtFilename.StringValue = Item.Filename;
			TxtArguments.StringValue = Item.Arguments;
			GuiUtils.SetCheck (ChkWaitEnd, Item.WaitEnd);

			CmdSave.Activated += (object sender, EventArgs e) => {
			
				Accepted = true;

				Item.Filename = TxtFilename.StringValue;
				Item.Arguments = TxtArguments.StringValue;
				Item.WaitEnd = GuiUtils.GetCheck(ChkWaitEnd);

				Window.Close ();
				NSApplication.SharedApplication.StopModal();
			};

			CmdCancel.Activated += (object sender, EventArgs e) => {

				Accepted = false;

				Window.Close ();
				NSApplication.SharedApplication.StopModal();
			};

			CmdBrowse.Activated += (object sender, EventArgs e) => {
				GuiUtils.SelectFile(this.Window, TxtFilename);
			};
		}
	}
}

