// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2019 AirVPN (support@airvpn.org) / https://airvpn.org
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
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Eddie.Common;

namespace Eddie.Core
{
	public class EngineCommandLine : CommandLine
	{
		public Dictionary<string, string> KnownCommands = new Dictionary<string, string>();

		public EngineCommandLine(string line) : base(line, true, false)
		{			
		}

		public void Init()
		{
			KnownCommands["cli"] = "";
			KnownCommands["version"] = LanguageManager.GetText("ManCommandLineVersion");
			KnownCommands["version.short"] = LanguageManager.GetText("ManCommandLineVersionShort");
			KnownCommands["help"] = LanguageManager.GetText("ManCommandLineHelp");
			KnownCommands["help.format"] = LanguageManager.GetText("ManCommandLineHelpFormat");

			KnownCommands["linux.xhost"] = ""; // Internal
			KnownCommands["linux.dbus"] = ""; // Internal
			KnownCommands["console.mode"] = ""; // Internal	
			KnownCommands["path.exec"] = "";  // Internal
			//KnownCommands["gui.osx.style"] = "";  // Internal
			KnownCommands["advanced.skip_privileges"] = "";  // Internal

			KnownCommands["profile"] = LanguageManager.GetText("ManCommandLineProfile");
			KnownCommands["path"] = LanguageManager.GetText("ManCommandLinePath");

			KnownCommands["path.resources"] = "";
			KnownCommands["path.tools"] = "";
		}

		public bool Check()
		{
			bool result = true;

			foreach (string commandLineParamKey in Params.Keys)
			{
				// 2.10.1
				// macOS sometime pass as command-line arguments a 'psn_0_16920610' or similar. Ignore it.
				if (commandLineParamKey.StartsWith("psn_", StringComparison.InvariantCultureIgnoreCase))
					continue;

				// 2.11.11 - Used to avoid a Mono crash if pressed Win key
				if (commandLineParamKey == "verify-all")
					continue;

				if (KnownCommands.ContainsKey(commandLineParamKey))
					continue;

                // No warning, because can override profile options
				//Engine.Instance.Logs.Log(LogType.Error, LanguageManager.GetText("CommandLineUnknownOption", commandLineParamKey));
				//result = false;
			}

			return result;
		}
	}
}
