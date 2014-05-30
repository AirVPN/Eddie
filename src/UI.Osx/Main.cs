using System;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using AirVPN.Core;

namespace AirVPN.UI.Osx
{
	class MainClass
	{
		static void Main (string[] args)
		{
			Platform.Instance = new AirVPN.Platforms.Osx();

			CommandLine.Init(Environment.CommandLine);

			if (CommandLine.Params.ContainsKey ("cli")) {
				Core.Engine engine = new Core.Engine ();

				if (engine.Initialization ()) {
					engine.ConsoleStart ();
				}
			} else {
				Engine engine = new Engine ();

				if (engine.Initialization () == false)
					return;

				engine.UiStart ();

				NSApplication.Init ();
				NSApplication.Main (args);
			}
		}
	}
}	

