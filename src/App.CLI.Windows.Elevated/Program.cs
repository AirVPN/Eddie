// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2019 AirVPN (support@airvpn.org) / https://airvpn.org
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
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Lib.Platform.Windows.Elevated;

namespace App.CLI.Windows.Elevated
{
	static class Program
	{
		// This program is WinForms only to avoid UAC elevation show console when launched with ShellExecute=true.		

		private static string ServiceName = "EddieElevationService";
		private static string ServiceDisplayName = "Eddie Elevation Service";
		//private static string ServiceDisplayDesc = "Eddie Elevation Service"; // Not yet used

		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			//Application.Run(new MainForm());

			try
			{	
				MainConsole(Engine.ParseCommandLine(Environment.GetCommandLineArgs()));
			}
			catch (Exception ex)
			{
				LocalLog(ex.Message);
			}
		}

		public static void MainConsole(Dictionary<string, string> cmdline)
		{	
			if (Utils.IsAdministrator() == false)
			{
				LocalLog("Don't launch this application directly, it's used by Eddie.");
				return;
			}

			if( (cmdline.ContainsKey("service")) && (cmdline["service"] == "install") )
			{
				string path = AppDomain.CurrentDomain.BaseDirectory + "Eddie-Service-Elevated.exe";

				// Can be active but old version that don't accept new client
				Utils.Shell("net", "stop \"" + ServiceName + "\"");
				Utils.Shell("sc", "delete \"" + ServiceName + "\"");

				Utils.Shell("sc", "create \"" + ServiceName + "\" binpath= \"" + path + " allowed_hash=" + cmdline["allowed_hash"] + "\" DisplayName= \"" + ServiceDisplayName + "\" start= auto");				
				Utils.Shell("net", "start \"" + ServiceName + "\"");
			}
			else if ((cmdline.ContainsKey("service")) && (cmdline["service"] == "uninstall"))
			{
				Utils.Shell("net", "stop \"" + ServiceName + "\"");
				Utils.Shell("sc", "delete \"" + ServiceName + "\"");
			}
			else if ((cmdline.ContainsKey("mode")) && (cmdline["mode"] == "spot"))
			{				
				if (NativeMethods.Init() != 0)
				{
					LocalLog("Unable to initialize native lib.");
					return;
				}

				Engine engine = new Engine();
				engine.Start(cmdline);
				engine.Stop(false);
			}
			else
			{
				LocalLog("Don't launch this application directly, it's used by Eddie.");
				return;
			}
		}

		private static void LocalLog(string msg)
		{
			Console.WriteLine(msg);
			MessageBox.Show(msg, "Eddie - Elevated", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}
}
