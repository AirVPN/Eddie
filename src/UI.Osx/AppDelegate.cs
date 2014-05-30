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

			(Engine.Instance as Engine).CreateMenuBarIcon ();
		}



		void MenuEvents()
		{
			MnuMainAbout.Activated += (object sender, EventArgs e) =>
			{
				if(windowAbout == null)
					windowAbout = new WindowAboutController();
				windowAbout.ShowWindow(this);
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
	}
}

