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
using Eddie.Common;
using Eddie.Core;

//using ExceptionReporting;

namespace Eddie.Forms
{
	public class Engine : Eddie.Core.Engine
	{
		// We have a list of logs, because we process it only when the form are available.
		public List<LogEntry> LogEntries = new List<LogEntry>();

		public Forms.Main FormMain; // ClodoTemp2 - remove?

		//public AutoResetEvent FormsReady = new AutoResetEvent(false);
		public AutoResetEvent InitDone = new AutoResetEvent(false);

		public override bool OnInit()
		{
			Application.ThreadException += new ThreadExceptionEventHandler(ApplicationThreadException);
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

			// Add the event handler for handling non-UI thread exceptions to the event. 
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

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

			if (FormMain != null)
				FormMain.DeInit();
		}

		public override void OnRefreshUi(RefreshUiMode mode)
		{
			base.OnRefreshUi(mode);

			if (Engine.Storage.GetBool("cli") == false)
				if (FormMain != null)
					FormMain.OnRefreshUi(mode);
		}

		public override void OnStatsChange(StatsEntry entry)
		{
			if (FormMain != null)
				FormMain.OnStatsChange(entry);
		}

		public override void OnProviderManifestFailed(Provider provider)
		{
			if (FormMain != null)
				FormMain.OnProviderManifestFailed(provider);
		}

		public override void OnLog(LogEntry l)
		{
			base.OnLog(l);

			if ((Engine.Storage == null) || (Engine.Storage.GetBool("cli") == false))
			{
				lock (LogEntries)
				{
					LogEntries.Add(l);
				}
				if (FormMain != null)
					FormMain.OnRefreshUi(RefreshUiMode.Log);

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
				FormMain.OnFrontMessage(message);
		}
		
		public override void OnShowText(string title, string data)
		{
			base.OnShowText(title, data);

			if (FormMain != null)
				FormMain.OnShowText(title, data);
		}

		public override Credentials OnAskCredentials()
		{
			if (FormMain != null)
				return FormMain.OnAskCredentials();
			return null;
		}

		public override void OnLoggedUpdate(XmlElement xmlKeys)
		{
			base.OnLoggedUpdate(xmlKeys);

			if (FormMain != null)
				FormMain.OnLoggedUpdate(xmlKeys);
		}

		public override void OnPostManifestUpdate()
		{
			base.OnPostManifestUpdate();

			if (FormMain != null)
				FormMain.OnPostManifestUpdate();
		}

		public virtual void OnChangeMainFormVisibility(bool vis)
		{
			
		}

		public virtual bool AllowMinimizeInTray()
		{
			if (Engine.Storage != null)
			{
				if (Engine.Storage.GetBool("gui.tray_minimized") == false)
				{
					return false;
				}
			}
			if (FormMain != null)
				return FormMain.AllowMinimizeInTray ();
			return false;
		}
	}
}
