
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

		public List<MonoMac.AppKit.NSWindowController> WindowsOpen = new List<MonoMac.AppKit.NSWindowController>();

		public NSStatusItem StatusItem;
		public NSMenuItem StatusMenuItem;

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

			System.Environment.Exit (0);
		}

		public override bool OnNoRoot ()
		{
			string path = Platform.Instance.GetExecutablePath();
			RootLauncher.LaunchExternalTool(path);
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


		
		public override void OnLog (LogEntry l)
		{
			base.OnLog (l);

			lock (LogsEntries) {
				LogsEntries.Add (l);
				if(Engine.Storage != null)
					if (LogsEntries.Count >= Engine.Storage.GetInt ("gui.log_limit"))
						LogsEntries.RemoveAt (0);
			}

			if (MainWindow != null)
				if (MainWindow.TableLogsController != null) {
				new NSObject ().InvokeOnMainThread (() => {
					MainWindow.Log(l);
					MainWindow.TableLogsController.RefreshUI ();
				});
			}
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



		public void CreateMenuBarIcon ()
		{
			NSMenu notifyMenu = new NSMenu ();
			StatusMenuItem = new NSMenuItem ("");
			notifyMenu.AddItem (StatusMenuItem);
			NSMenuItem exitMenuItem = new NSMenuItem ("Quit", (a,b) => {
				//System.Environment.Exit (0);
				Engine.RequestStop();
			});
			notifyMenu.AddItem (exitMenuItem);

			StatusItem = NSStatusBar.SystemStatusBar.CreateStatusItem (22);
			StatusItem.Menu = notifyMenu;
			StatusItem.Image = NSImage.ImageNamed ("statusbar_black_gray.png");
			StatusItem.HighlightMode = true;
		}
	}
}