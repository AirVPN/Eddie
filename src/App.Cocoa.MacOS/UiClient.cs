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
using Eddie.Common;
using Eddie.Core;

namespace Eddie.UI.Cocoa.Osx
{
    public class UiClient : Eddie.Common.UiClient
	{
        public static UiClient Instance;
        public Engine Engine;
        public MainWindowController MainWindow;

        private WindowReportController m_windowReport;

        public override bool Init()
        {
            base.Init();

            Instance = this;

            Engine = new Engine();

            if (Engine.Initialization(false) == false)
                return false;

            Engine.UiManager.Add(this);

            return true;
        }

        public override Json Command(Json data)
        {
            return Engine.UiManager.OnCommand(data, this);
        }

        public override void OnReceive(Json data)
        {
            base.OnReceive(data);

            string cmd = data["command"].Value as string;

            if (cmd == "ui.notification")
            {
                if (MainWindow != null)
                {
                    new NSObject().InvokeOnMainThread(() =>
                    {
                        MainWindow.ShowNotification(data["message"].Value as string);
                    });
                }
            }
            else if (cmd == "ui.color")
            {
                if (MainWindow != null)
                {
                    new NSObject().InvokeOnMainThread(() =>
                    {
                        MainWindow.SetColor(data["color"].Value as string);
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

                    new NSObject().InvokeOnMainThread(() =>
                    {
                        MainWindow.SetStatus(textFull, textShort);
                    });
                }
            }
            else if (cmd == "system.report.progress")
            {
                string step = data["step"].Value as string;
                string text = data["body"].Value as string;
                int perc = Conversions.ToInt32(data["perc"].Value, 0);

                if (MainWindow != null)
                {
                    new NSObject().InvokeOnMainThread(() =>
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
    }
}