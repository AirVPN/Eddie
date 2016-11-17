// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2016 AirVPN (support@airvpn.org) / https://airvpn.org )
//
// Eddie is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Eddie is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Eddie. If not, see <http://www.gnu.org/licenses/>.
// </eddie_source_header>

using System;
using System.Drawing;
using System.Reflection;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using Eddie.Core;

namespace Eddie.UI.Cocoa.Osx
{
	class MainClass
	{
		static void Main (string[] args)
		{
			Core.Platform.Instance = new Eddie.Platforms.Osx();

			CommandLine.InitSystem(Environment.CommandLine);

			// Due to a bug in Xamarin, that don't recognize resources inside Core library if Mono is bundled, we embed some resources in entry assembly.

			Core.ResourcesFiles.LoadString (Assembly.GetEntryAssembly (), "license.txt", "License.txt");
			Core.ResourcesFiles.LoadString (Assembly.GetEntryAssembly (), "thirdparty.txt", "ThirdParty.txt");
			Core.ResourcesFiles.LoadString (Assembly.GetEntryAssembly (), "tos.txt", "TOS.txt");

			Core.ResourcesFiles.Count ();

			if (CommandLine.SystemEnvironment.Exists ("cli")) {
				Core.Engine engine = new Core.Engine ();

				if (engine.Initialization (true)) {
					engine.ConsoleStart ();
					engine.Join ();
				}

			} else {
				Engine engine = new Engine ();

				if (engine.Initialization (false) == false)
					return;

				NSApplication.Init ();
				NSApplication.Main (args);
			}
		}
	}
}	

