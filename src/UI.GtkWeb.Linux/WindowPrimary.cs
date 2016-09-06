using System;
using Gtk;
using WebKit;
using Eddie.Core;

public partial class WindowPrimary: Gtk.Window
{	
	public bool CanClose = false;

	private static Gtk.StatusIcon notifyIcon;

	WebView webBrowser;

	public WindowPrimary (): base (Gtk.WindowType.Toplevel)
	{
		Build ();

		SetDefaultSize(250, 200);
		SetPosition(WindowPosition.Center);

		/*
		if (Platform.Instance.IsTraySupported ()) {
			notifyIcon = new StatusIcon (GuiUtils.LoadImageFromResource("icon.png"));

			notifyIcon.Visible = true;

			// Show/Hide the window (even from the Panel/Taskbar) when the TrayIcon has been clicked.
			notifyIcon.Activate += OnNotifyIconActivate;
			// Show a pop up menu when the icon has been right clicked.
			notifyIcon.PopupMenu += OnNotifyIconPopup;

			// A Tooltip for the Icon
			notifyIcon.Tooltip = "Hello World Icon";
		}
		*/

		webBrowser = new WebView ();
		webBrowser.Editable = false;
		webBrowser.Open("http://localhost:4649");
		pnlMain.Add (webBrowser);
		//Gtk.Box.BoxChild w = ((Gtk.Box.BoxChild)pnlMain [webBrowser]);
		//w.Position = 0;
		//w.Expand = true;
		//w.Fill = true;
		pnlMain.ShowAll ();

		DeleteEvent += delegate(object o, Gtk.DeleteEventArgs args) {

			if(CanClose)
				return;

			MyEngine.RequestStop();
		};

	}

	public UI.GtkWeb.Linux.Engine MyEngine
	{
		get {
			return Eddie.Core.Engine.Instance as UI.GtkWeb.Linux.Engine;
		}
	}

	public void Restore()
	{
		this.Show ();
	}

	void OnNotifyIconActivate (object sender, EventArgs e)
	{

	}

	void OnNotifyIconPopup (object o, PopupMenuArgs args)
	{
		//ShowMenu ();
	}

	public void DeInit()
	{
		CanClose = true;

		//notifyIcon.Dispose ();

		Destroy ();
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
}
