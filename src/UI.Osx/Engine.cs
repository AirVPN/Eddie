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
using MonoMac.Foundation;
using MonoMac.AppKit;
using Eddie.Core;

namespace Eddie.UI.Osx
{
	public class Engine : Eddie.Core.Engine
	{
		public MainWindowController MainWindow;
		public List<LogEntry> LogsPending = new List<LogEntry>();

		public List<MonoMac.AppKit.NSWindowController> WindowsOpen = new List<MonoMac.AppKit.NSWindowController>();


		public Engine ()
		{
		}

		public override bool OnInit ()
		{
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

			return base.OnInit ();
		}

		public override void OnDeInit2 ()
		{
			base.OnDeInit2 ();

			if (MainWindow != null) {
				new NSObject ().InvokeOnMainThread (() => {
					MainWindow.Close ();
					MainWindow = null; // 2.10.2
				});
			}
		}

		public override bool OnNoRoot ()
		{
			string path = Platform.Instance.GetExecutablePath();
			List<string> args = CommandLine.SystemEnvironment.GetFullArray ();
			string colorMode = Platform.Instance.ShellCmd ("defaults read -g AppleInterfaceStyle 2>/dev/null");
			if(colorMode == "Dark")
				args.Add("gui.osx.style=\"dark\"");
			RootLauncher.LaunchExternalTool(path, args.ToArray());
			return true;
		}

		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Exception ex = (Exception)e.ExceptionObject;
			Engine.OnUnhandledException(ex);
		}



		public override void OnRefreshUi (RefreshUiMode mode)
		{
			base.OnRefreshUi (mode);

			if (MainWindow != null) {
				new NSObject ().InvokeOnMainThread (() => {
					MainWindow.RefreshUi (mode);
				});
			}
		}

		public override void OnStatsChange (StatsEntry entry)
		{
			base.OnStatsChange (entry);

			if (MainWindow != null)
			if (MainWindow.TableStatsController != null) {
				new NSObject ().InvokeOnMainThread (() => {
					MainWindow.TableStatsController.RefreshUI ();
				});
			}
		}

		public override void OnSettingsChanged ()
		{
			if (MainWindow != null) {
				new NSObject ().InvokeOnMainThread (() => {
					MainWindow.SettingsChanged ();
				});
			}

			base.OnSettingsChanged ();
		}
		
		public override void OnLog (LogEntry l)
		{
			base.OnLog (l);

			lock (LogsPending) {
				LogsPending.Add (l);
			}

			OnRefreshUi (RefreshUiMode.Log);


		}


		public override void OnFrontMessage (string message)
		{
			base.OnFrontMessage (message);

			if (MainWindow != null) {
				new NSObject ().InvokeOnMainThread (() => {
					MainWindow.FrontMessage (message);
				});
			}
		}

		public override bool OnAskYesNo(string message)
		{
			bool result = false;

			if (MainWindow != null)
			{
				new NSObject().InvokeOnMainThread(() =>
				{
						result = GuiUtils.MessageYesNo(message);
				});
			}
			
			return result;
		}

		public override void OnPostManifestUpdate ()
		{
			base.OnPostManifestUpdate ();

			if (MainWindow != null)
			{
				new NSObject().InvokeOnMainThread(() =>
					{
						MainWindow.PostManifestUpdate();
					});
			}
		}

		public override void OnLoggedUpdate (XmlElement xmlKeys)
		{
			base.OnLoggedUpdate (xmlKeys);

			if (MainWindow != null)
			{
				new NSObject().InvokeOnMainThread(() =>
					{
						MainWindow.LoggedUpdate(xmlKeys);
					});
			}
		}
	}
}