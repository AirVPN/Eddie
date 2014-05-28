
using System;
using System.Collections.Generic;
using MonoMac.Foundation;
using MonoMac.AppKit;
using AirVPN.Core;

namespace AirVPN.UI.Osx
{
	public class Engine : AirVPN.Core.Engine
	{
		public MainWindowController MainWindow;
		public List<LogEntry> LogsEntries = new List<LogEntry>();

		public Engine ()
		{
		}

		public override void OnLog (LogEntry l)
		{
			base.OnLog (l);

			LogsEntries.Add (l);

			if (MainWindow != null)
			if (MainWindow.TableLogsController != null) {
				new NSObject ().InvokeOnMainThread (() => {
					MainWindow.TableLogsController.RefreshUI ();
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

		public override void OnRefreshUi (RefreshUiMode mode)
		{
			base.OnRefreshUi (mode);

			if (MainWindow != null) {
				new NSObject ().InvokeOnMainThread (() => {
					MainWindow.RefreshUi (mode);
				});
			}
		}
	}
}