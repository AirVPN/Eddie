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
using System.Xml;
using System.Text;

namespace Eddie.Core
{
    public class Option
    {
		public string Code = "";
		public string Type = "";
		public string Default = "";
		public string Man = "";
		public string Value = "";

        public bool Important = false; // Dump in support log
		public bool Omissis = false; // Dump in support log with omissis
		//public bool CommandLineOnly = false;
		public bool InternalOnly = false; // Don't show in UI
        public bool DontUserReset = false; // If true, the 'Reset All' launched by user don't clean this option.

		public Json GetJson()
		{
			Json j = new Json();

			j["type"].Value = Type;
			j["default"].Value = Default;
			j["man"].Value = Man;
			// Note: no value
			j["important"].Value = Important;
			j["omissis"].Value = Omissis;
			j["internalonly"].Value = InternalOnly;
			j["dontuserreset"].Value = DontUserReset;

			return j;
		}
	}
}
