// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2023 AirVPN (support@airvpn.org) / https://airvpn.org )
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
//using System.Drawing;
using Foundation;
using AppKit;
using ObjCRuntime;
using Eddie.Core;

namespace Eddie.UI.Cocoa.Osx
{
	public partial class AppDelegate : NSApplicationDelegate
	{
		public AppDelegate()
		{
        }

        public override bool ApplicationShouldHandleReopen(NSApplication sender, bool hasVisibleWindows)
        {
			if (UiClient.Instance.Data != null)
				UiClient.Instance.MainWindow.Restore(sender);
			return true;
		}

        public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender)
		{
			return false; // 2.8
		}

		public override void DidFinishLaunching(NSNotification notification)
		{
            NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);

            NSProcessInfo.ProcessInfo.DisableSuddenTermination(); // Already disabled by default

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            UiClient client = new UiClient();
            client.AppDelegate = this;
            client.Init(Environment.CommandLine);

            MenuEvents();

		}

        public override NSApplicationTerminateReply ApplicationShouldTerminate(NSApplication sender)
		{
            if (UiClient.Instance.Engine.Terminated == false)
			{
                if (UiClient.Instance.MainWindow == null)
                    return NSApplicationTerminateReply.Now;
                else if (UiClient.Instance.MainWindow.ShutdownConfirmed)
					return NSApplicationTerminateReply.Later;
				else if (UiClient.Instance.MainWindow.Shutdown() == false)
					return NSApplicationTerminateReply.Cancel;
				else
					return NSApplicationTerminateReply.Later;
			}
			else
			{
				return NSApplicationTerminateReply.Now;
			}
		}

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (UiClient.Instance != null)
            {
                Exception ex = (Exception)e.ExceptionObject;
                UiClient.Instance.OnUnhandledException("CurrentDomain", ex);
            }
        }



		void MenuEvents()
		{
			MnuMainAbout.Activated += (object sender, EventArgs e) =>
			{
                if (UiClient.Instance.Data != null)
                    UiClient.Instance.MainWindow.ShowAbout();
			};

			MnuMainPreferences.Activated += (object sender, EventArgs e) =>
			{
                if (UiClient.Instance.Data != null)
                    UiClient.Instance.MainWindow.ShowPreferences();
			};

			MnuMainHome.Activated += (object sender, EventArgs e) =>
			{
                if (UiClient.Instance.Data != null)
                    UiClient.Instance.MainWindow.ShowHome();
			};

			MnuMainClientArea.Activated += (object sender, EventArgs e) =>
			{
                if (UiClient.Instance.Data != null)
                    UiClient.Instance.MainWindow.ShowClientArea();
			};

			MnuMainForwardingPorts.Activated += (object sender, EventArgs e) =>
			{
                if (UiClient.Instance.Data != null)
                    UiClient.Instance.MainWindow.ShowForwardingPorts();
			};

			MnuMainQuit.Activated += (object sender, EventArgs e) =>
			{
                UiClient.Instance.MainWindow.Shutdown();
			};
		}
	}
}

