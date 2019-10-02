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

		//public AutoResetEvent FormsReady = new AutoResetEvent(false);
		public AutoResetEvent InitDone = new AutoResetEvent(false);

		public Engine(string environmentCommandLine) : base(environmentCommandLine)
		{
		}
		
		public override string OnAskProfilePassword(bool authFailed)
		{			
			return UiClient.Instance.SplashWindow.AskUnlockPassword(authFailed);
		}

		private delegate bool OnAskYesNoDelegate(string message);
		public override bool OnAskYesNo(string message)
		{
			Form parentForm = UiClient.Instance.MainWindow;
			if (parentForm == null)
				parentForm = UiClient.Instance.SplashWindow;

			if (parentForm.InvokeRequired)
			{
				OnAskYesNoDelegate inv = new OnAskYesNoDelegate(this.OnAskYesNo);

				return (bool)parentForm.Invoke(inv, new object[] { message });
			}
			else
			{
				return GuiUtils.MessageBoxAskYesNo(parentForm, message);
			}
		}

		private delegate Json OnAskShellExternalPermissionDelegate(Json data);
		public override Json OnAskShellExternalPermission(Json data)
		{
			Form parentForm = UiClient.Instance.MainWindow;
			if (parentForm == null)
				parentForm = UiClient.Instance.SplashWindow;

			if (parentForm.InvokeRequired)
			{
				OnAskShellExternalPermissionDelegate inv = new OnAskShellExternalPermissionDelegate(this.OnAskShellExternalPermission);

				return (Json)parentForm.Invoke(inv, new object[] { data });
			}
			else
			{
				Forms.WindowShellExternalPermission dlg = new Forms.WindowShellExternalPermission();
				dlg.Data = data;
				dlg.ShowDialog(parentForm);
				return dlg.Answer;
			}
		}

		public override void OnRefreshUi(RefreshUiMode mode)
		{
			base.OnRefreshUi(mode);

			if (UiClient.Instance.MainWindow != null)
				UiClient.Instance.MainWindow.OnRefreshUi(mode);
		}

		public override void OnStatsChange(StatsEntry entry)
		{
			if (UiClient.Instance.MainWindow != null)
				UiClient.Instance.MainWindow.OnStatsChange(entry);
		}

		public override void OnProviderManifestFailed(Provider provider)
		{
			if (UiClient.Instance.MainWindow != null)
				UiClient.Instance.MainWindow.OnProviderManifestFailed(provider);
		}

		public override void OnLog(LogEntry l)
		{
			base.OnLog(l);
						
			{
				lock (LogEntries)
				{
					LogEntries.Add(l);
				}
				if (UiClient.Instance.MainWindow != null)
					UiClient.Instance.MainWindow.OnRefreshUi(RefreshUiMode.Log);
			}
		}
				
		public override void OnShowText(string title, string data)
		{
			base.OnShowText(title, data);

			if (UiClient.Instance.MainWindow != null)
				UiClient.Instance.MainWindow.OnShowText(title, data);
		}

		

		public override Credentials OnAskCredentials()
		{
			if (UiClient.Instance.MainWindow != null)
				return UiClient.Instance.MainWindow.OnAskCredentials();
			else
				return null;
		}

		public override void OnPostManifestUpdate()
		{
			base.OnPostManifestUpdate();

			if (UiClient.Instance.MainWindow != null)
				UiClient.Instance.MainWindow.OnPostManifestUpdate();
		}

		public virtual void OnChangeMainFormVisibility(bool vis)
		{
            if (UiClient.Instance.MainWindow != null)
                UiClient.Instance.MainWindow.OnChangeMainFormVisibility(vis);
        }

		public virtual bool AllowMinimizeInTray()
		{
			if (Engine.Storage != null)
			{
				if (Engine.Storage.GetBool("gui.tray_show") == false)
				{
					return false;
				}
			}
			if (UiClient.Instance.MainWindow != null)
				return UiClient.Instance.MainWindow.AllowMinimizeInTray ();
			return false;
		}
	}
}
