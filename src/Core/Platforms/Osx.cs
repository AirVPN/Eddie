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
using System.Collections.Generic;
using System.Text;

namespace AirVPN.Core.Platforms
{
    public class Osx : Core.Platform
    {
        // Override
		public override string GetCode()
		{
			return "OSX";
		}

        public override bool IsAdmin()
        {
            return (Environment.UserName == "root");
        }

		public override string VersionDescription()
        {
			string o = base.VersionDescription();
            o += " - " + ShellCmd("uname -a").Trim();
            return o;
        }

        public override string DirSep
        {
            get
            {
                return "/";
            }
        }

        public override string GetUserFolder()
        {
            return Environment.GetEnvironmentVariable("HOME") + DirSep + ".airvpn";
        }

        public override string ShellCmd(string Command)
        {
            return Shell("sh", String.Format("-c '{0}'", Command), true);
        }

        public override void FlushDNS()
        {
            // Leopard
            ShellCmd("lookupd -flushcache");
            // Other
            ShellCmd("dscacheutil -flushcache");
        }

		public override string GetDriverAvailable()
		{
			return "Unknown";
		}

		public override bool CanUnInstallDriver()
		{
			return true;
		}

		public override void InstallDriver()
		{
		}

		public override void UnInstallDriver()
		{
		}
    }
}
