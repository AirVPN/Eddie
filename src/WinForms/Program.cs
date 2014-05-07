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
using System.Windows.Forms;
using AirVPN.Core;

namespace AirVPN.Gui
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        [STAThread]
        static void Main()
        {			
			//Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            CommandLine.Init(Environment.CommandLine);

			if(CommandLine.Params.ContainsKey("cli"))
			{
				Core.Engine engine = new Core.Engine();

				engine.Initialization();

				engine.ConsoleStart();
			}
			else
			{
				GuiUtils.Init();

				Gui.Engine engine = new Gui.Engine();
			
				engine.Initialization();

				engine.FormMain = new Gui.Forms.Main();

				engine.Start();

				Application.Run(engine.FormMain);
			}
        }
    }
}