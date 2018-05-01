// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2016 AirVPN (support@airvpn.org) / https://airvpn.org
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
using System.Linq;
using System.Text;

namespace Eddie.Deploy
{
	public class Package
	{
		public string Platform;
		public string Architecture;
		public string UI; // TOFIX: Not yet used
		public bool Bundles; // TOFIX: Not yet used
		public string NetFramework = "";
		public string Format;

		public string ArchitectureCompile;

		public Package(string platform, string architecture, string ui, bool bundles, string netFramework, string format)
		{
			Platform = platform;
			Architecture = architecture;
			UI = ui;
			Bundles = bundles;
			NetFramework = netFramework;
			Format = format;

			ArchitectureCompile = Architecture;
			if (ArchitectureCompile == "armhf") // Arm pick x64 executabled (that are anyway CIL).
				ArchitectureCompile = "x64";
		}
	}
}
