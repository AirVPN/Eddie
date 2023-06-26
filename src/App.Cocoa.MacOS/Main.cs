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
using System.Reflection;
using Foundation;
using AppKit;
using ObjCRuntime;
using Eddie.Core;

namespace Eddie.UI.Cocoa.Osx
{
	class MainClass
	{
        static void Main(string[] args)
		{
			Core.Platform.Instance = new Eddie.Platform.MacOS.Platform();

			if (new CommandLine(Environment.CommandLine, true, false).Exists("cli")) // TOFIX, not need anymore when every OS have a CLI executable.
			{
                Core.ConsoleEdition.UiClient client = new Core.ConsoleEdition.UiClient();
                AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) => {
                    Exception ex = (Exception)e.ExceptionObject;
                    client.OnUnhandledException("CurrentDomain", ex);
                };
                client.Init(Environment.CommandLine);
			}
			else
			{
                NSApplication.Init();

                NSApplication.Main(args);
			}
		}
    }
}

