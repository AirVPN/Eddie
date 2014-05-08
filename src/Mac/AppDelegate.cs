using System;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;

namespace AirVPN.Mac
{
	public partial class AppDelegate : NSApplicationDelegate
	{
		MainWindowController mainWindowController;

		public AppDelegate ()
		{
		}

		public override void FinishedLaunching (NSObject notification)
		{
			//CreateMenu (); // Old

			mainWindowController = new MainWindowController ();
			mainWindowController.Window.MakeKeyAndOrderFront (this);

			MenuEvents ();
		}

		void MenuEvents()
		{
			MnuMainAbout.Activated += (object sender, EventArgs e) =>
			{
				WindowAboutController about = new WindowAboutController();
				NSApplication.SharedApplication.RunModalForWindow(about.Window);
			};
		}

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
	}
}

