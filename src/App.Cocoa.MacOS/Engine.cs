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
	public class Engine : Eddie.Core.Engine
	{
		//public MainWindowController MainWindow;
		public List<LogEntry> LogsPending = new List<LogEntry>();

		public List<AppKit.NSWindowController> WindowsOpen = new List<AppKit.NSWindowController>();

		//private WindowReportController m_windowReport;

        public Engine(string environmentCommandLine) : base(environmentCommandLine)
		{
		}

        /*
		public override void OnDeInit2()
		{
			base.OnDeInit2();

			if (MainWindow != null)
			{
				new NSObject().InvokeOnMainThread(() =>
				{
					MainWindow.Close();
					MainWindow = null; // 2.10.2
				});
			}
		}
		*/

        public override string OnAskProfilePassword(bool authFailed)
        {
            return UiClient.Instance.SplashWindow.AskUnlockPassword(authFailed);
        }

		public override bool OnAskYesNo(string message)
		{
			bool answer = false;
			new NSObject().InvokeOnMainThread(() =>
			{
				answer = GuiUtils.MessageYesNo(message);
			});
			return answer;
		}


		public override Json OnAskShellExternalPermission(Json data)
        {
            Json answer = null;
            new NSObject().InvokeOnMainThread(() =>
            {
                WindowShellExternalPermissionController w = new WindowShellExternalPermissionController();
                w.Data = data;
                NSApplication.SharedApplication.RunModalForWindow(w.Window);
                answer = w.Answer;
            });
            return answer;
        }

        public override void OnRefreshUi(RefreshUiMode mode)
		{
			base.OnRefreshUi(mode);

            if (UiClient.Instance.MainWindow != null)
			{
				new NSObject().InvokeOnMainThread(() =>
				{
                    UiClient.Instance.MainWindow.RefreshUi(mode);
				});
			}
		}

		public override void OnStatsChange(StatsEntry entry)
		{
			base.OnStatsChange(entry);

			if (UiClient.Instance.MainWindow != null)
				if (UiClient.Instance.MainWindow.TableStatsController != null)
				{
					new NSObject().InvokeOnMainThread(() =>
					{
                        UiClient.Instance.MainWindow.TableStatsController.RefreshUI();
					});
				}
		}

		public override void OnProviderManifestFailed(Provider provider)
		{
			base.OnProviderManifestFailed(provider);

			if (UiClient.Instance.MainWindow != null)
				new NSObject().InvokeOnMainThread(() =>
				{
                    UiClient.Instance.MainWindow.ProviderManifestFailed(provider);
				});
		}

		public override void OnSettingsChanged()
		{
			if (UiClient.Instance.MainWindow != null)
			{
				new NSObject().InvokeOnMainThread(() =>
				{
                    UiClient.Instance.MainWindow.SettingsChanged();
				});
			}

			base.OnSettingsChanged();
		}

		public override void OnLog(LogEntry l)
		{
			base.OnLog(l);

			lock (LogsPending)
			{
				LogsPending.Add(l);
			}

			OnRefreshUi(RefreshUiMode.Log);
		}
		
		public override void OnShowText(string title, string data)
		{
			if (UiClient.Instance.MainWindow != null)
			{
				new NSObject().InvokeOnMainThread(() =>
					{
                        UiClient.Instance.MainWindow.ShowText(UiClient.Instance.MainWindow.Window, title, data);
					});
			}
		}

		public override Credentials OnAskCredentials()
		{
			Credentials cred = null;
			if (UiClient.Instance.MainWindow != null)
			{
				new NSObject().InvokeOnMainThread(() =>
				{
					WindowCredentialsController dlg = new WindowCredentialsController();
					dlg.Window.ReleasedWhenClosed = true;
					NSApplication.SharedApplication.RunModalForWindow(dlg.Window);
					dlg.Window.Close();

					if (dlg.Credentials != null)
						cred = dlg.Credentials;
				});
			}
			return cred;
		}

		public override void OnPostManifestUpdate()
		{
			base.OnPostManifestUpdate();

			if (UiClient.Instance.MainWindow != null)
			{
				new NSObject().InvokeOnMainThread(() =>
					{
                        UiClient.Instance.MainWindow.PostManifestUpdate();
					});
			}
		}


	}
}