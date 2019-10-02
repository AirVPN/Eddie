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

namespace Eddie.Common
{
	public static class Constants
	{
		public static string Name = "Eddie";
		public static string NameCompatibility = "AirVPN";
		public static string AppID = "ec80475d661a5f449069818262b08d645c570f8f";
		public static byte[] NotSecretPayload = Encoding.UTF8.GetBytes("4af85e84255b077ad890dba297e811b7d016add1");
		public static string PasswordIfEmpty = "e6552ddf3ac5c8755a82870d91273a63eab0da1e";
		public static string Thanks = "Clodo, PJ, Berserker, ProMIND, zhang888, LZ1, giganerd, Uncle Hunto, go558a83nk, sheivoko, NaDre, pfSense_fan, x0wllaar";
		public static int VersionInt = 260;
		public static string VersionDesc = "2.18.4"; // Used by deploy system also to generate filenames
        public static string VersionShow = VersionDesc + "beta"; // Visible to users
        //public static string VersionShow = VersionDesc; // Visible to users
        public static int VersionElevated = 1373;
        public static bool AlphaFeatures = false;
		public static bool FeatureIPv6ControlOptions = true;
		public static bool FeatureAlwaysBypassOpenvpnRoute = true; // Default for Eddie 2.14		
		public static string Domain = "eddie.website";
		public static string WebSite = "https://eddie.website";
		public static string WebSiteIPv4 = "188.166.41.48"; // Used only in Test Report (Log>Lifebeft)
		public static string WebSiteIPv6 = "2a03:b0c0:2:d0::11b4:6001"; // Used only in Test Report (Log>Lifebeft)
		public static string DnsVpn = "10.4.0.1"; // < 2.9, TOCLEAN
		public static string WindowsDriverVersion = "9.23.3";
		public static string WindowsXpDriverVersion = "9.9.2";
		public static DateTime dateForPastChecking = new DateTime(2014, 08, 26);
		public static int ElevatedServiceTcpPort = 9346;
	}
}
