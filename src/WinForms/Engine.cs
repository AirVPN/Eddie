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
using System.IO;
using System.Threading;
using System.Text;
using System.Windows.Forms;
using AirVPN.Core;

//using ExceptionReporting;

namespace AirVPN.Gui
{
	public class Engine : AirVPN.Core.Engine
    {
		// We have a list of logs, because we process it only when the form are available.
        public List<LogEntry> LogEntries = new List<LogEntry>();

        public Forms.Main FormMain;
        
        //public AutoResetEvent FormsReady = new AutoResetEvent(false);
        public AutoResetEvent InitDone = new AutoResetEvent(false);

        public override bool OnInit()
        {
			// Engine.Log(Core.Engine.LogType.Verbose, "Old Data: " + Application.UserAppDataPath);
            
            Application.ThreadException += new ThreadExceptionEventHandler(ApplicationThreadException);
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

			// Add the event handler for handling non-UI thread exceptions to the event. 
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

			// System.Threading.Thread.Sleep(1000);

            bool result = base.OnInit();

			return result;
        }

		public override void OnUnhandledException(Exception e)
		{
			base.OnUnhandledException(e);

			/*
			if (Platform.IsWindows())
			{
				ExceptionReporter reporter = new ExceptionReporter();

				// Crash Reporting
				reporter.Config.AppName = "AirVPN";
				//reporter.Config.CompanyName = "Fuzz Pty Ltd";
				reporter.Config.TitleText = "AirVPN Client Error Report";
				reporter.Config.EmailReportAddress = "support@airvpn.org";
				reporter.Config.ShowSysInfoTab = true;   // all tabs are shown by default
				reporter.Config.ShowFlatButtons = true;   // this particular config is code-only
				reporter.Config.TakeScreenshot = true;   // attached if sending email
				//reporter.Config.FilesToAttach = new[] { "c:/file.txt" }; // any other files to attach

				reporter.Show(e);
			}
			*/
		}

		public static void ApplicationThreadException(object sender, ThreadExceptionEventArgs e)
		{
			Engine.OnUnhandledException(e.Exception);
		}

		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Exception ex = (Exception)e.ExceptionObject;
			Engine.OnUnhandledException(ex);
		}
        
		public override void OnExit()
		{
			if (FormMain != null)
				FormMain.Close();
			else
				base.OnExit();
		}

        public override void OnDeInit2()
        {
            base.OnDeInit2();

            if(FormMain != null)
                FormMain.DeInit();
        }

		public override bool OnNoRoot()
		{
			if (FormMain != null) // GUI Only
			{
				if (Platform.Instance.IsWindowsSystem())
				{
					// Never occur, root granted by Windows Manifest
					return false;
				}
				else if (Platform.Instance.IsLinuxSystem())
				{
					Log(LogType.Verbose, Messages.AdminRequiredRestart);
					/*
					string cmd;					
					if(File.Exists("/usr/bin/gksu"))
						cmd = "gksu -D \"AirVPN Client\" -u root ";
					else
						cmd = "xdg-su -u root -c ";
					cmd += " \"" + Platform.Instance.GetExecutablePath() + " " + CommandLine.Get() + "\"";
					Console.WriteLine(cmd);
					Platform.Instance.ShellCmd(cmd);					
					*/
					string command = "";
					string arguments = "";
					if (File.Exists("/usr/bin/gksu"))
					{
						command = "gksu";
						arguments = " -D \"AirVPN Client\" -u root ";
					}
					else
					{
						command = "xdg-su";
						arguments = " -u root -c ";
					}
					arguments += " \"" + Platform.Instance.GetExecutablePath() + " " + CommandLine.Get() + "\"";
					Platform.Instance.Shell(command, arguments, false);

					return true;
				}
			}
			
			return false;
		}
        
        public override void OnRefreshUi(RefreshUiMode mode)
        {
			if(Engine.Storage.GetBool("cli") == false)
				if (FormMain != null)
					FormMain.RefreshUi(mode);            
        }
		
        public override void OnLog(LogEntry l)
        {
			base.OnLog(l);
			
			if( (Engine.Storage == null) || (Engine.Storage.GetBool("cli") == false) )
			{
				lock (LogEntries)
				{
					LogEntries.Add(l);
					if (FormMain != null)
						FormMain.RefreshUi(RefreshUiMode.Log);            
				}
			}
        }

		public override void OnFrontMessage(string message)
		{
			base.OnFrontMessage(message);

			if (FormMain != null)
				FormMain.ShowFrontMessage(message);
		}

    }
}
