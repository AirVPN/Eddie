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
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;

namespace AirVPN.UI.Osx
{
	public partial class AppDelegate : NSApplicationDelegate
	{
		MainWindowController mainWindowController;


		public AppDelegate ()
		{
		}

		public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
		{
			return false; // 2.8
		}

		public override void FinishedLaunching (NSObject notification)
		{

			Engine.Instance.TerminateEvent += delegate() {
				new NSObject ().InvokeOnMainThread (() => {
					NSApplication.SharedApplication.ReplyToApplicationShouldTerminate (true);
				});
			};

			mainWindowController = new MainWindowController ();

			bool startVisible = Engine.Instance.Storage.GetBool("gui.osx.visible");
			if (startVisible) {
				mainWindowController.Window.MakeKeyAndOrderFront (this);
			} else {
				mainWindowController.Window.IsVisible = false;
			}
			NSApplication.SharedApplication.ActivateIgnoringOtherApps (true);

			MenuEvents ();

		}


		public override NSApplicationTerminateReply ApplicationShouldTerminate (NSApplication sender)
		{
			if (Engine.Instance.IsAlive ()) {
				if (mainWindowController.ShutdownConfirmed)
					return NSApplicationTerminateReply.Now;
				else if(mainWindowController.Shutdown() == false)
					return NSApplicationTerminateReply.Cancel;
				else
					return NSApplicationTerminateReply.Later;
			} else {
				return NSApplicationTerminateReply.Now;
			}

		}

		void MenuEvents()
		{
			MnuMainAbout.Activated += (object sender, EventArgs e) =>
			{
				mainWindowController.ShowAbout();
			};

			MnuMainPreferences.Activated += (object sender, EventArgs e) =>
			{
				mainWindowController.ShowPreferences();
			};

			MnuMainHome.Activated += (object sender, EventArgs e) =>
			{
				mainWindowController.ShowHome();
			};

			MnuMainClientArea.Activated += (object sender, EventArgs e) =>
			{
				mainWindowController.ShowClientArea();
			};

			MnuMainForwardingPorts.Activated += (object sender, EventArgs e) =>
			{
				mainWindowController.ShowForwardingPorts();
			};

			MnuMainSpeedTest.Activated += (object sender, EventArgs e) =>
			{
				mainWindowController.ShowSpeedTest();
			};

			MnuMainQuit.Activated += (object sender, EventArgs e) => {
				mainWindowController.Shutdown();
			};
		}
	}
}

