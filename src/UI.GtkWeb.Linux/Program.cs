using System;
using System.Threading;
using System.Diagnostics;
using Gtk;
using Gdk;
using Eddie.Lib.Common;
using Eddie.Lib.UiWeb;

namespace UI.GtkWeb.Linux
{	
	public static class MainClass
	{
		public static Eddie.Lib.UiWeb.ProcessCore Core;

		public static WindowSplash Splash;
		public static Gtk.Window MainForm;

		private static Gtk.Image m_imageIconNormal;
		private static Gtk.Image m_imageIconGray;
		private static Gtk.ScrolledWindow m_scrolledWindow;
		private static WebView m_webBrowser;
		private static StatusIcon m_trayIcon;
		private static Menu m_menu;
		private static bool m_canClose = false;


		public static void Main (string[] args)
		{			
			Application.Init ();

			// Init CLI

			Core = new Eddie.Lib.UiWeb.ProcessCore ();

			Splash = new WindowSplash ();
			Splash.Show ();

			m_imageIconNormal = Gtk.Image.LoadFromResource ("UI.GtkWeb.Linux.Resources.icon.png");
			m_imageIconGray = Gtk.Image.LoadFromResource ("UI.GtkWeb.Linux.Resources.icon_gray.png");

			Core.ReceiveEvent += OnReceiveEvent;
			Core.MessageEvent += OnMessageEvent;
			Core.ShowReadyEvent += OnShowReadyEvent;
			Core.LaunchCoreEvent += OnLaunchCoreEvent;
			Core.ExitEvent += OnExitEvent;



			// Init UI
			//BuildMainForm();

			BuildTray ();

			Core.Start ();

			Application.Run ();
		}


		static void OnReceiveEvent (string data)
		{
			Console.WriteLine ("ReceiveEvent: " + data);
			Cli2Html (data);
		}

		static void OnMessageEvent(string type, string message)
		{
			if (type == "splash") {
				Splash.SetAction (message);
			} else if (type == "fatal") {
				// pazzo TODO
			}
		}

		static void OnShowReadyEvent ()
		{			
			Gtk.Application.Invoke (delegate {
				BuildMainForm();
				MainForm.SetDefaultSize (1280, 800);
				MainForm.SetPosition (WindowPosition.Center);					
				MainForm.Icon = m_imageIconGray.Pixbuf;

				m_scrolledWindow = new ScrolledWindow ();

				m_webBrowser = new WebView ();
				m_webBrowser.Editable = false;
				m_webBrowser.Open ("http://localhost:4649/index.html?mode=UI.Linux");
				m_scrolledWindow.Add (m_webBrowser);
				MainForm.Add (m_scrolledWindow);

				MainForm.ShowAll ();

				Splash.Destroy();
			});
		}

		static Process OnLaunchCoreEvent(string args)
		{
			Process process = new Process();

			process.StartInfo.FileName = "/usr/bin/gksu";
			process.StartInfo.Arguments = " -u root -m \"SuperEddie\" \"mono CLI.Linux.Exe " + args + "\"";
			process.StartInfo.WorkingDirectory = "";

			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			process.StartInfo.UseShellExecute = false;
			//Core.Start ("/usr/bin/gksu", " -u root -m \"SuperEddie\" \"mono CLI.Linux.Exe -console.mode=backend\"");

			return process;
		}

		static void OnExitEvent ()
		{
			m_canClose = true;

			Application.Quit ();
		}

		static void Cli2Html(string data)
		{
			Gtk.Application.Invoke (delegate {
				string jsStr = CommonUtils.JsonEncode(data);
				string js = "Eddie.dataReceive(\"" + jsStr + "\")";
				m_webBrowser.ExecuteScript (js);
			}); 
		}

		public static void HtmlToCli(string data)
		{			
			XmlItem xml = new XmlItem (data);

			string action = xml.GetAttribute ("action");

			// Look WebView.cs comment about the Mono bug
			// Core.Send (data);
			// Alternative workaround, send to JS for direct delivery via /api/command/
			// Maybe totally TOCLEAN if i always use a TCP method.
			Gtk.Application.Invoke (delegate {
				string jsStr = CommonUtils.JsonEncode(data);
				string js = "Eddie.command(\"" + jsStr + "\")";
				m_webBrowser.ExecuteScript (js);
			}); 
		}


		public static void BuildTray()
		{			
			m_trayIcon = new StatusIcon(new Pixbuf ("../../../../webui/images/icon.png"));
			m_trayIcon.Visible = true;

			// Show/Hide the window (even from the Panel/Taskbar) when the TrayIcon has been clicked.
			m_trayIcon.Activate += delegate { MainForm.Visible = !MainForm.Visible; };
			// Show a pop up menu when the icon has been right clicked.
			m_trayIcon.PopupMenu += OnTrayIconPopup;

			// A Tooltip for the Icon
			m_trayIcon.Tooltip = "Hello World Icon";
		}

		public static void BuildMainForm()
		{
			MainForm = new Gtk.Window ("Eddie3");
			MainForm.DeleteEvent += delegate(object o, Gtk.DeleteEventArgs args) 
			{ 				
				if(m_canClose)
					return;
				
				args.RetVal = true; // Avoid

				HtmlToCli("exit"); 
			};
		}

		static void FormRestore()
		{
		}

		static void FormHide()
		{
		}

		static void OnTrayIconPopup (object o, EventArgs args) 
		{
			m_menu.ShowAll ();
			m_menu.Popup();
		}

	}
}
