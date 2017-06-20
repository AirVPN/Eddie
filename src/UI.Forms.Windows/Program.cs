// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2016 AirVPN (support@airvpn.org) / https://airvpn.org
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
using System.Windows.Forms;
using System.Text;
using Eddie.Lib.Common;
using Eddie.Core;
using Eddie.Gui;

namespace Eddie.UI.Windows
{
	static class Program
	{
		/// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 

		public static IpAddresses ResolveDNSLinux(string host)
		{
			IpAddresses result = new IpAddresses();
			string hostout = System.IO.File.ReadAllText("r:\\l.txt");
			// Note: CNAME record are automatically followed.
			foreach(string line in hostout.Split('\n'))
			{
				string ipv4 = Utils.RegExMatchOne(line, "^.*? has address (.*?)$");
				if (ipv4 != "")
					result.Add(ipv4.Trim());

				string ipv6 = Utils.RegExMatchOne(line, "^.*? has IPv6 address (.*?)$");
				if (ipv6 != "")
					result.Add(ipv6.Trim());
			}
			return result;
		}

		[STAThread]
		static void Main()
        {
            try
            {
				if (Environment.OSVersion.Version.Major >= 6)
                    SetProcessDPIAware();

				IpAddresses test = ResolveDNSLinux("JHJF");

				//Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

                Platform.Instance = new Eddie.Platforms.Windows();
				
				CommandLine.InitSystem(Environment.CommandLine);
                
                if (CommandLine.SystemEnvironment.Exists("cli"))
                {
                    Core.Engine engine = new Core.Engine();

                    if (engine.Initialization(true))
                    {
                        engine.ConsoleStart();
                    }
                }
                else
                {
                    GuiUtils.Init();

                    Gui.Engine engine = new Gui.Engine();

                    engine.TerminateEvent += Engine_TerminateEvent;

                    if (engine.Initialization(false))
                    {
                        engine.FormMain = new Gui.Forms.Main();

                        engine.UiStart();

                        // Application.Run(engine.FormMain); // Removed in 2.11.9                      

                        engine.FormMain.LoadPhase();

                        m_context = new ApplicationContext();                        
                        Application.Run(m_context);
                    }
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, Constants.Name2, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
		}

        static ApplicationContext m_context;

        private static void Engine_TerminateEvent()
        {
            m_context.ExitThread();

            //Application.Exit(); // Removed in 2.12, otherwise lock Core thread. Still required in Linux edition.
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

    }
}