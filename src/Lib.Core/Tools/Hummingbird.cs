// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2023 AirVPN (support@airvpn.org) / https://airvpn.org
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

namespace Eddie.Core.Tools
{
	public class Hummingbird : ITool
	{
		public override void OnNormalizeVersion()
		{
			if (Version == "")
				return;

			if (Version.IndexOfInv("\n") != -1)
				Version = Version.Substring(0, Version.IndexOfInv("\n")).Trim();

			// Workaround until Hummingbird report also the original OpenVPN3 version (needed by other checks).
			Version = "3.3.2 - " + Version;
		}

		public override string GetFileName()
		{
			if (Platform.Instance.IsWindowsSystem())
			{
				return "hummingbird.exe";
			}
			else
				return base.GetFileName();
		}

		public override string GetVersionArgument()
		{
			return "--version";
		}
	}
}
