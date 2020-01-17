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
using System.Reflection;
using System.Xml;
using Foundation;
using AppKit;
using Eddie.Core;

namespace Eddie.UI.Cocoa.Osx
{
    public class UiClient : Eddie.Core.UiClient
	{
        public static UiClient Instance;
        public Engine Engine;
        public AppDelegate AppDelegate;
        public MainWindowController MainWindow;
        public WindowSplashController SplashWindow;

        private WindowReportController m_windowReport;

        public override bool Init(string environmentCommandLine)
        {
            Instance = this;

            base.Init(environmentCommandLine);

            SplashWindow = new WindowSplashController();
            SplashWindow.Window.MakeKeyAndOrderFront(AppDelegate);

            Engine = new Engine(environmentCommandLine);
            Engine.UiManager.Add(this);
            Engine.TerminateEvent += delegate ()
            {
                new NSObject().InvokeOnMainThread(() => // BeginInvokeOnMainThread
                {
                    if (MainWindow != null)
                        MainWindow.Close();
                    if (SplashWindow != null)
                        SplashWindow.Close();

                    //NSApplication.SharedApplication.ReplyToApplicationShouldTerminate (true);
                    NSApplication.SharedApplication.Terminate(new NSObject());
                });
            };

            Engine.Start();

            return true;
        }

		public void OnUnhandledException(string source, Exception e)
		{
			if (Engine != null)
				Engine.OnUnhandledException(source, e);
		}

		public override Json Command(Json data)
        {
            return Engine.UiManager.SendCommand(data, this);
        }

        public override void OnReceive(Json data)
        {
            base.OnReceive(data);

            string cmd = data["command"].Value as string;

            if (cmd == "log")
            {
                if(data["type"].Value as string == "fatal")
                {
                    if (SplashWindow != null)
                        SplashWindow.MessageError(data["message"].Value as string);
                    else if (MainWindow != null)
                        MainWindow.MessageError(data["message"].Value as string);
                    else
                        GuiUtils.MessageBoxError(data["message"].Value as string);
                }
            }
            else if (cmd == "init.step")
            {
                if (SplashWindow != null)
                    SplashWindow.SetStatus(data["message"].Value as string);
            }
            else if (cmd == "engine.ui")
            {
                SplashWindow.SetStatus("Loading UI");

                //UpdateInterfaceStyle();

                new NSObject().InvokeOnMainThread(() => // BeginInvokeOnMainThread
                {
                    MainWindow = new MainWindowController();
                    bool startVisible = Engine.Storage.GetBool("gui.osx.visible");
                    if (startVisible)
                    {
                        MainWindow.Window.MakeKeyAndOrderFront(null);
                    }
                    else
                    {
                        MainWindow.Window.IsVisible = false;
                    }
                    UiClient.Instance.SplashWindow.RequestCloseForReady();
                });
            }
            else if (cmd == "ui.notification")
            {
                if (MainWindow != null)
                {
                    new NSObject().InvokeOnMainThread(() => // BeginInvokeOnMainThread
                    {
                        MainWindow.ShowNotification(data["message"].Value as string, data["level"].Value as string);
                    });
                }
            }
            else if (cmd == "ui.status")
            {
                if (MainWindow != null)
                {
                    string textFull = data["full"].Value as string;
                    string textShort = textFull;
                    if (data.HasKey("short"))
                        textShort = data["short"].Value as string;

                    new NSObject().InvokeOnMainThread(() => // BeginInvokeOnMainThread
                    {
                        MainWindow.SetStatus(textFull, textShort);
                    });
                }
            }
            else if (cmd == "ui.main-status")
            {
                string appIcon = data["app_icon"].Value as string;
                string appColor = data["app_color"].Value as string;
                string actionIcon = data["action_icon"].Value as string;
                string actionCommand = data["action_command"].Value as string;
                string actionText = data["action_text"].Value as string;

                if (MainWindow != null)
                {
                    new NSObject().InvokeOnMainThread(() => // BeginInvokeOnMainThread
                    {
                        MainWindow.SetMainStatus(appIcon, appColor, actionIcon, actionCommand, actionText);
                    });
                }
            }
            else if (cmd == "ui.updater.available")
            {
                new NSObject().InvokeOnMainThread(() => // BeginInvokeOnMainThread
                {
                    MainWindow.ShowUpdater();
                });
            }
            else if (cmd == "ui.frontmessage")
            {
                new NSObject().InvokeOnMainThread(() => // BeginInvokeOnMainThread
                {
                    MainWindow.FrontMessage(data["message"].Value as Json);
                });
            }
            else if (cmd == "system.report.progress")
            {
                string step = data["step"].Value as string;
                string text = data["body"].Value as string;
                int perc = Conversions.ToInt32(data["perc"].Value, 0);

                if (MainWindow != null)
                {
                    new NSObject().InvokeOnMainThread(() => // BeginInvokeOnMainThread
                    {
                        if ((m_windowReport == null) || (m_windowReport.Window.IsVisible == false))
                        {
                            m_windowReport = new WindowReportController();
                            GuiUtils.ShowWindowWithFocus(m_windowReport, this.MainWindow);
                        }

                        if (m_windowReport != null)
                            m_windowReport.SetStep(step, text, perc);
                    });
                }
            }
        }

        /* // TOCLEAN
        public void UpdateInterfaceStyle()
        {
            // AppleInterfaceStyle is user-level settings.
            // Setting the 'Dark mode' in preferences, don't change the interface style of the ROOT user, and AirVPN client run as root.
            // We detect the settings when this software relaunch itself, and here we update accordly the settings of the current (ROOT) user.
            string defaultsPath = Core.Platform.Instance.LocateExecutable("defaults");
            if (defaultsPath != "")
            {
                // If 'white', return error in StdErr and empty in StdOut.
                SystemShell s = new SystemShell();
                s.Path = defaultsPath;
                s.Arguments.Add("read");
                s.Arguments.Add("-g");
                s.Arguments.Add("AppleInterfaceStyle");
                s.Run();
                string rootColorMode = s.StdOut.Trim().ToLowerInvariant();
                if (rootColorMode == "")
                    rootColorMode = "light";
                string argsColorMode = Engine.Instance.StartCommandLine.Get("gui.osx.style", "light");
                if (rootColorMode != argsColorMode)
                {
                    if (argsColorMode == "dark")
                        Core.SystemShell.Shell(defaultsPath, new string[] { "write", "-g", "AppleInterfaceStyle", "Dark" });
                    else
                        Core.SystemShell.Shell(defaultsPath, new string[] { "remove", "-g", "AppleInterfaceStyle" });
                }
            }
        }
        */
    }
}