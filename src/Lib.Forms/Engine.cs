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
using System.IO;
using System.Threading;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Eddie.Core;

//using ExceptionReporting;

namespace Eddie.Gui
{
	public class Engine : Eddie.Core.Engine
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
				reporter.Config.AppName = "Eddie";
				reporter.Config.TitleText = "Eddie Error Report";
				reporter.Config.EmailReportAddress = "support@airvpn.org";
				reporter.Config.ShowSysInfoTab = true;   // all tabs are shown by default
				reporter.Config.ShowFlatButtons = true;   // this particular config is code-only
				reporter.Config.TakeScreenshot = true;   // attached if sending email				

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

        public override void OnCommand(XmlItem xml, bool ignoreIfNotExists)
        {
            string action = xml.GetAttribute("action").ToLowerInvariant();

            if(action == "ui.show.preferences")
            {
                Forms.Settings Dlg = new Forms.Settings();
                Dlg.ShowDialog();

                FormMain.EnabledUi();
            }            
            else if (action == "ui.show.about")
            {
                Forms.About dlg = new Forms.About();
                dlg.ShowDialog();
            }
            else if (action == "ui.show.menu")
            {
                FormMain.ShowMenu();
            }
            else
                base.OnCommand(xml, ignoreIfNotExists);
        }

        public override bool OnNoRoot()
		{
			if (ConsoleMode == false) // GUI Only
			{
				if (Platform.Instance.IsWindowsSystem())
				{
					// Never occur, root granted by Windows Manifest
					return false;
				}
				else if (Platform.Instance.IsLinuxSystem())
				{
					string command = "";
					string arguments = "";


					string command2 = "";
					string executablePath = Platform.Instance.GetExecutablePath();
					string cmdline = CommandLine.SystemEnvironment.GetFull();
					if (executablePath.Substring(executablePath.Length - 4).ToLowerInvariant() == ".exe")
						command2 += "mono ";						
					command2 += Platform.Instance.GetExecutablePath();
					command2 += " ";
					command2 += cmdline;
					bool waitEnd = false;
					
					if (Platform.Instance.FileExists("/usr/bin/kdesudo"))
					{
						command = "kdesudo";
						arguments = "";
						arguments += " -u root"; // Administrative privileges
						arguments += " -d"; // Don't show commandline
						arguments += " --comment \"" + Messages.AdminRequiredPasswordPrompt + "\"";
						arguments += " -c "; // The command
						//arguments += " \"" + command2 + "\"";
						arguments += " \"" + command2 + "\"";
					}
					else if (Platform.Instance.FileExists("/usr/bin/kdesu"))
					{
						command = "kdesu";
						arguments = "";
						arguments += " -u root"; // Administrative privileges
						arguments += " -d"; // Don't show commandline
						//arguments += " --comment \"" + Messages.AdminRequiredPasswordPrompt + "\"";
						arguments += " -c "; // The command
						//arguments += " \"" + command2 + "\"";
						arguments += " \"" + command2 + "\"";
					}
                    /*
					 * Under Debian, gksudo don't work, gksu work...
					if (Platform.Instance.FileExists("/usr/bin/gksudo"))
					{
						command = "gksudo";
						arguments = "";
						arguments += " -u root"; // Administrative privileges
						arguments += " -m \"" + Messages.AdminRequiredPasswordPrompt + "\"";
						arguments += " \"" + command2 + "\"";
					}
					else 
					*/
                    else if (Platform.Instance.FileExists("/usr/bin/gksu"))
					{
						command = "gksu";
						arguments = "";
						arguments += " -u root"; // Administrative privileges
						arguments += " -m \"" + Messages.AdminRequiredPasswordPrompt + "\"";
						arguments += " \"" + command2 + "\"";
					}
					else if (Platform.Instance.FileExists("/usr/bin/xdg-su")) // OpenSUSE
					{
						command = "xdg-su";
						arguments = "";
						arguments += " -u root"; // Administrative privileges
						arguments += " -c "; // The command
						arguments += " \"" + command2 + "\"";
					}
					else if (Platform.Instance.FileExists("/usr/bin/beesu")) // Fedora
					{
						command = "beesu";
						arguments = "";
						arguments += " " + command2 + "";
					}
                    /*
					else if (Platform.Instance.FileExists("/usr/bin/pkexec"))
					{
						// Different behiavour on different platforms
						command = "pkexec";
						arguments = "";
						arguments = " env DISPLAY=$DISPLAY XAUTHORITY=$XAUTHORITY";
						arguments += " " + command2 + "";

						// For this bug: https://lists.ubuntu.com/archives/foundations-bugs/2012-July/100103.html
						// We need to keep alive the current process, otherwise 'Refusing to render service to dead parents.'.
						waitEnd = true;

						// Still don't work.
					}
					*/

                    if (command != "")
					{
                        Logs.Log(LogType.Verbose, Messages.AdminRequiredRestart);

                        //Logs.Log(LogType.Verbose, "Command:'" + command + "', Args:'" + arguments + "'");

						Platform.Instance.Shell(command, arguments, waitEnd);
					}
					else
					{
                        Logs.Log(LogType.Fatal, Messages.AdminRequiredRestartFailed);						
					}

					return true;
				}
			}
			
			return false;
		}
        
        public override void OnRefreshUi(RefreshUiMode mode)
        {
			base.OnRefreshUi(mode);

			if(Engine.Storage.GetBool("cli") == false)
				if (FormMain != null)
					FormMain.RefreshUi(mode);            
        }

		public override void OnStatsChange(StatsEntry entry)
		{
			if (FormMain != null)
				FormMain.StatsChange(entry);
		}
		
        public override void OnLog(LogEntry l)
        {
			base.OnLog(l);
			
			if( (Engine.Storage == null) || (Engine.Storage.GetBool("cli") == false) )
			{
				lock (LogEntries)
				{
					LogEntries.Add(l);
				}
				if (FormMain != null)
					FormMain.RefreshUi(RefreshUiMode.Log);            
				
				if (FormMain == null) // Otherwise it's showed from the RefreshUI in the same UI Thread
				{
					if (l.Type == LogType.Fatal)
					{
						MessageBox.Show(FormMain, l.Message, Constants.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
        }

		public override void OnFrontMessage(string message)
		{
			base.OnFrontMessage(message);

			if (FormMain != null)
				FormMain.ShowFrontMessage(message);
		}

        public override void OnMessageInfo(string message)
        {
            base.OnMessageInfo(message);

            if (FormMain != null)
                MessageBox.Show(FormMain, message, Constants.Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public override void OnMessageError(string message)
        {
            base.OnMessageError(message);

            if (FormMain != null)
                MessageBox.Show(FormMain, message, Constants.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public override void OnShowText(string title, string data)
        {
            base.OnShowText(title, data);

            Forms.TextViewer Dlg = new Forms.TextViewer();
            Dlg.Title = title;
            Dlg.Body = data;
            Dlg.ShowDialog();
        }

        public override bool OnAskYesNo(string message)
		{
			if (FormMain != null)
				return MessageBox.Show(message, Constants.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
			else
				return true;
		}

        public override void OnLoggedUpdate(XmlElement xmlKeys)
        {
            base.OnLoggedUpdate(xmlKeys);

            if (FormMain != null)
                FormMain.LoggedUpdate(xmlKeys);
        }

        public override void OnPostManifestUpdate()
		{
			base.OnPostManifestUpdate();

			if (FormMain != null)
				FormMain.PostManifestUpdate();
		}

    }
}
