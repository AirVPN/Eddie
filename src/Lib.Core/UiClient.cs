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

using System.Collections.Generic;

namespace Eddie.Core
{
	public class UiClient
	{
		public Json Data;

		public virtual bool Init(string environmentCommandLine)
		{
			return false;
		}

		public virtual Json Command(Json data)
		{
			return null;
		}

		public virtual void OnReceive(Json data)
		{
			string cmd = data["command"].Value as string;
			if (cmd == "ui.manifest")
				Data = data;
		}

		// Helpers

		public Json Command(string command)
		{
			Json j = new Json();

			CommandLine cmd = new CommandLine(command.Trim(), false, true);
			j["command"].Value = cmd.Get("action", "");
			foreach (KeyValuePair<string, string> kp in cmd.Params)
				if (kp.Key != "action")
					j[kp.Key].Value = kp.Value;

			return Command(j);
		}
	}
}
