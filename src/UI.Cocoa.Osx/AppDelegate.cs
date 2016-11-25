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
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;

namespace Eddie.UI.Cocoa.Osx
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
					//NSApplication.SharedApplication.ReplyToApplicationShouldTerminate (true);
					NSApplication.SharedApplication.Terminate(new NSObject ());
				});
			};

			UpdateInterfaceStyle ();

			mainWindowController = new MainWindowController ();

			bool startVisible = Engine.Instance.Storage.GetBool("gui.osx.visible");
			if (startVisible) {
				mainWindowController.Window.MakeKeyAndOrderFront (this);
			} else {
				mainWindowController.Window.IsVisible = false;
			}
			NSApplication.SharedApplication.ActivateIgnoringOtherApps (true);

			NSProcessInfo.ProcessInfo.DisableSuddenTermination (); // Already disabled by default

			MenuEvents ();

		}


		public override NSApplicationTerminateReply ApplicationShouldTerminate (NSApplication sender)
		{
			if (Engine.Instance.Terminated == false)
			{
				if (mainWindowController.ShutdownConfirmed)
					return NSApplicationTerminateReply.Later;
				else if(mainWindowController.Shutdown() == false)
					return NSApplicationTerminateReply.Cancel;
				else
					return NSApplicationTerminateReply.Later;
			} else {
				return NSApplicationTerminateReply.Now;
			}
		}

		public void UpdateInterfaceStyle()
		{
			// AppleInterfaceStyle is user-level settings.
			// Setting the 'Dark mode' in preferences, don't change the interface style of the ROOT user, and AirVPN client run as root.
			// We detect the settings when this software relaunch itself, and here we update accordly the settings of the current (ROOT) user.
			string rootColorMode = Core.Platform.Instance.ShellCmd ("defaults read -g AppleInterfaceStyle 2>/dev/null").ToString ().ToLowerInvariant ();
			if (rootColorMode == "")
				rootColorMode = "light";
			string argsColorMode = Engine.Instance.Storage.Get ("gui.osx.style");
			if (rootColorMode != argsColorMode) {
				if(argsColorMode == "dark")
					Core.Platform.Instance.ShellCmd ("defaults write -g AppleInterfaceStyle Dark");
				else 
					Core.Platform.Instance.ShellCmd ("defaults remove -g AppleInterfaceStyle");
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

			MnuMainOpenDataDirectory.Activated += (object sender, EventArgs e) =>
			{
				mainWindowController.ShowDataDirectory();
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

