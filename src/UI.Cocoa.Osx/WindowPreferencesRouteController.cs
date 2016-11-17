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
using Eddie.Core;

namespace Eddie.UI.Cocoa.Osx
{
	public partial class WindowPreferencesRouteController : MonoMac.AppKit.NSWindowController
	{
		public bool Accepted = false;
		public static TableRoutingControllerItem Item;



		#region Constructors

		// Called when created from unmanaged code
		public WindowPreferencesRouteController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public WindowPreferencesRouteController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		
		// Call to load from the XIB/NIB file
		public WindowPreferencesRouteController () : base ("WindowPreferencesRoute")
		{
			Initialize ();
		}
		
		// Shared initialization code
		void Initialize ()
		{
		}

		#endregion

		//strongly typed window accessor
		public new WindowPreferencesRoute Window {
			get {
				return (WindowPreferencesRoute)base.Window;
			}
		}

		public override void AwakeFromNib ()
		{
			base.AwakeFromNib ();

			Window.Title = Constants.Name + " - " + Messages.WindowsSettingsRouteTitle;
		
			LblHelp.StringValue = Messages.WindowsSettingsRouteTitle;

			CboAction.RemoveAllItems ();
			CboAction.AddItem (WindowPreferencesController.RouteDirectionToDescription ("in"));
			CboAction.AddItem (WindowPreferencesController.RouteDirectionToDescription ("out"));

			TxtIP.Changed += (object sender, EventArgs e) => {
				EnableIde();
			};
			CmdOk.Activated += (object sender, EventArgs e) => {

				Accepted = true;
				Item.Ip = TxtIP.StringValue;
				Item.Action = WindowPreferencesController.RouteDescriptionToDirection(GuiUtils.GetSelected(CboAction));
				Item.Icon = Item.Action;
				Item.Notes = TxtNotes.StringValue;

				Window.Close ();
				NSApplication.SharedApplication.StopModal ();
			};

			CmdCancel.Activated += (object sender, EventArgs e) => {

				Accepted = false;

				Window.Close ();
				NSApplication.SharedApplication.StopModal ();
			};

			TxtIP.StringValue = Item.Ip;
			GuiUtils.SetSelected (CboAction, WindowPreferencesController.RouteDirectionToDescription (Item.Action));
			TxtNotes.StringValue = Item.Notes;

			EnableIde ();
		}

		public void EnableIde()
		{
			if (new IpAddressRange (TxtIP.StringValue).Valid == false) {
				LblHelp.StringValue = Messages.WindowsSettingsRouteInvalid + "\n" + Messages.WindowsSettingsRouteEditIp;
				CmdOk.Enabled = false;
			} else {
				LblHelp.StringValue = Messages.WindowsSettingsRouteEditIp;
				CmdOk.Enabled = true;
			}

		}
	}
}

