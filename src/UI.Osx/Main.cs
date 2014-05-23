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

			Engine engine = new Engine();

			//AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

			if (engine.Initialization () == false)
				return;

			engine.UiStart ();

			NSApplication.Init ();
			NSApplication.Main (args);
		}
	}
}	

