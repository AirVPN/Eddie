using System;
using Gtk;
using Eddie.Core;

namespace UI.GtkWeb.Linux
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Platform.Instance = new Eddie.Platforms.Linux();

			CommandLine.InitSystem(Environment.CommandLine);

			Engine engine = new Engine ();

			if (engine.Initialization (false) == false)
				return;

			engine.TerminateEvent += delegate() {
				Application.Quit();
			};

			Application.Init ();

			engine.WindowMain = new WindowPrimary ();

			engine.UiStart ();


			engine.WindowMain.Show ();

			Application.Run ();
		}
	}
}
