using System;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;

namespace AirVPN.UI.Osx
{
	public partial class AppDelegate : NSApplicationDelegate
	{
		MainWindowController mainWindowController;

		NSStatusItem m_statusItem;

		WindowAboutController windowAbout;
		WindowPreferencesController windowPreferences;

		public AppDelegate ()
		{
		}

		public override void FinishedLaunching (NSObject notification)
		{
			//CreateMenu (); // TOCLEAN

			mainWindowController = new MainWindowController ();
			mainWindowController.Window.MakeKeyAndOrderFront (this);

			MenuEvents ();

			MenuBarIcon ();
		}

		void MenuBarIcon ()
		{
			NSMenu notifyMenu = new NSMenu ();
			NSMenuItem exitMenuItem = new NSMenuItem ("Quit", (a,b) => {
				System.Environment.Exit (0); });
			notifyMenu.AddItem (exitMenuItem);

			m_statusItem = NSStatusBar.SystemStatusBar.CreateStatusItem (30);
			m_statusItem.Menu = notifyMenu;
			m_statusItem.Image = NSImage.ImageNamed ("speed.png");
			m_statusItem.HighlightMode = true;
		}

		void MenuEvents()
		{
			MnuMainAbout.Activated += (object sender, EventArgs e) =>
			{
				if(windowAbout == null)
					windowAbout = new WindowAboutController();
				windowAbout.ShowWindow(this);
				//NSApplication.SharedApplication.RunModalForWindow(about.Window);
			};

			MnuMainPreferences.Activated += (object sender, EventArgs e) =>
			{
				if(windowPreferences == null)
					windowPreferences = new WindowPreferencesController();
				windowPreferences.ShowWindow(this);
			};

			MnuMainHome.Activated += (object sender, EventArgs e) =>
			{
				AirVPN.Core.UI.Actions.OpenUrlWebsite();
			};

			MnuMainClientArea.Activated += (object sender, EventArgs e) =>
			{
				AirVPN.Core.UI.Actions.OpenUrlClient();
			};

			MnuMainForwardingPorts.Activated += (object sender, EventArgs e) =>
			{
				AirVPN.Core.UI.Actions.OpenUrlPorts();
			};

			MnuMainSpeedTest.Activated += (object sender, EventArgs e) =>
			{
				AirVPN.Core.UI.Actions.OpenUrlSpeedTest();
			};
		}

		/* // TOCLEAN
		void CreateMenu()
		{
			NSMenu menuMain = new NSMenu ();

			NSApplication.SharedApplication.MainMenu = menuMain;

			NSMenuItem menuItemApp = new NSMenuItem ();
			menuMain.AddItem (menuItemApp);

			NSMenu menuApp = new NSMenu ("AirVPN");
			menuItemApp.Submenu = menuApp;

			NSMenuItem menuWebsite = new NSMenuItem ("AirVPN Web Site", "w", delegate {
			});
			menuApp.AddItem (menuWebsite);

			NSMenuItem menuPreferences = new NSMenuItem ("Preferences", "p", delegate {
			});
			menuApp.AddItem (menuPreferences);

			NSMenuItem menuDetails = new NSMenuItem ("Details", "l", delegate {
			});
			menuApp.AddItem (menuDetails);

			// TODO: Developers
			// TODO: Tools

			NSMenuItem menuAbout = new NSMenuItem ("About", "a", delegate {
			});
			menuApp.AddItem (menuAbout);

			menuApp.AddItem (NSMenuItem.SeparatorItem);

			NSMenuItem menuUser = new NSMenuItem ("Your details and statistics (Web)", "d", delegate {
			});
			menuApp.AddItem (menuUser);

			NSMenuItem menuPorts = new NSMenuItem ("Forwarding Ports (Web)", "p", delegate {
			});
			menuApp.AddItem (menuPorts);

			NSMenuItem menuSpeedTest = new NSMenuItem ("Speed Test (Web)", "s", delegate {
			});
			menuApp.AddItem (menuSpeedTest);

			menuApp.AddItem (NSMenuItem.SeparatorItem);

			NSMenuItem menuItemQuit = new NSMenuItem("Quit", "q", delegate {
				NSApplication.SharedApplication.Terminate (menuMain);
			});
			menuApp.AddItem(menuItemQuit);


		}
		*/
	}
}

