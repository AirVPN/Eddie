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

		private static string ServiceName = "Eddie Elevation Service";

		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			//Application.Run(new MainForm());

			try
			{
				MainConsole(Environment.GetCommandLineArgs());
			}
			catch (Exception ex)
			{
				LocalLog(ex.Message);
			}
		}

		public static void MainConsole(string[] args)
		{	
			if (Utils.IsAdministrator() == false)
			{
				LocalLog("Don't launch this application directly, it's used by Eddie.");
				return;
			}

			string action = "";
			if (Environment.GetCommandLineArgs().Length == 2)
				action = Environment.GetCommandLineArgs()[1];

			if (action == "service-install")
			{
				string path = AppDomain.CurrentDomain.BaseDirectory + "Eddie-Service-Elevated.exe";

				Utils.Shell("sc", "create \"" + ServiceName + "\" start=auto binpath=\"" + path + "\"");
				Utils.Shell("net", "start \"" + ServiceName + "\"");
			}
			else if(action == "service-uninstall")
			{
				Utils.Shell("net", "stop \"" + ServiceName + "\"");
				Utils.Shell("sc", "delete \"" + ServiceName + "\"");
			}
			else if(action == "spot")
			{				
				if (NativeMethods.Init() != 0)
				{
					LocalLog("Unable to initialize native lib.");
					return;
				}

				Engine engine = new Engine();
				engine.Start(false);
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
