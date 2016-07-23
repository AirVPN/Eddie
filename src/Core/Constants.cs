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
using System.Text;

namespace Eddie.Core
{
    public static class Constants
    {
		public static string Name = "AirVPN";
        public static string Name2 = "Eddie";
        public static string AppID = "ed8efc06d5263733167fbbed49230843397c3701";
        public static int VersionInt = 218;
		public static string VersionDesc = "2.11.2";
        public static string WebSite = "https://airvpn.org";
        public static string DnsVpn = "10.4.0.1"; // < 2.9, TOCLEAN
		public static string WindowsDriverVersion = "9.21.2";
		public static string WindowsXpDriverVersion = "9.9.2";
		public static DateTime dateForPastChecking = new DateTime(2014, 08, 26);
    }
}
