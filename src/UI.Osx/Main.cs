// <airvpn_source_header>
// This file is part of AirVPN Client software.
// Copyright (C)2014-2014 AirVPN (support@airvpn.org) / https://airvpn.org )
//
// AirVPN Client is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// AirVPN Client is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with AirVPN Client. If not, see <http://www.gnu.org/licenses/>.
// </airvpn_source_header>

using System;
using System.Drawing;
using System.Reflection;
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
			Core.Platform.Instance = new AirVPN.Platforms.Osx();

			CommandLine.InitSystem(Environment.CommandLine);

			// Due to a bug in Xamarin, that don't recognize resources inside Core library if Mono is bundled, we embed some resources in entry assembly.

			Core.ResourcesFiles.LoadString (Assembly.GetEntryAssembly (), "manifest.xml", "manifest.xml");
			Core.ResourcesFiles.LoadString (Assembly.GetEntryAssembly (), "auth.xml", "auth.xml");
			Core.ResourcesFiles.LoadString (Assembly.GetEntryAssembly (), "license.txt", "License.txt");
			Core.ResourcesFiles.LoadString (Assembly.GetEntryAssembly (), "thirdparty.txt", "ThirdParty.txt");
			Core.ResourcesFiles.LoadString (Assembly.GetEntryAssembly (), "tos.txt", "TOS.txt");

			Core.ResourcesFiles.Count ();

			if (CommandLine.SystemEnvironment.Exists ("cli")) {
				Core.Engine engine = new Core.Engine ();

				if (engine.Initialization ()) {
					engine.ConsoleStart ();
				}

				engine.Join ();

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

