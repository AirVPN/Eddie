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
using System.IO;
using System.Text;
using AirVPN.Core;

namespace AirVPN.Platforms
{
    public class Osx : Platform
    {
		private string m_architecture = "";

        // Override
		public Osx()
		{
			m_architecture = NormalizeArchitecture(ShellPlatformIndipendent("sh", "-c 'uname -m'", "", true, false).Trim());
		}

		public override string GetCode()
		{
			return "OSX";
		}

		public override string GetArchitecture()
		{
			return m_architecture;
		}

        public override bool IsAdmin()
        {
			return true; // Uncomment for debugging

			// With root privileges by RootLauncher.cs, Environment.UserName still return the normal username, 'whoami' return 'root'.
			string u = ShellCmd ("whoami").ToLowerInvariant().Trim();
			return (u == "root");
        }

		public override bool IsUnixSystem()
		{
			return true;
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

		public override string GetExecutablePath()
		{
			string currentPath = System.Reflection.Assembly.GetEntryAssembly().Location;
			if(new FileInfo(currentPath).Directory.Name == "MonoBundle")
			{
				// OSX Bundle detected, use the launcher executable
				currentPath = currentPath.Replace("/MonoBundle/","/MacOS/").Replace(".exe","");
			}
			return currentPath;
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
			return false;
		}

		public override void InstallDriver()
		{
		}

		public override void UnInstallDriver()
		{
		}

		public override void RouteAdd (string Address, string Mask, string Gateway)
		{
			base.RouteAdd (Address, Mask, Gateway);
		}

		public override void RouteRemove (string Address, string Mask, string Gateway)
		{
			base.RouteRemove (Address, Mask, Gateway);
		}

		public override string RouteList ()
		{
			string cmd = "netstat -nr";
			string output = ShellCmd (cmd);
			return output;
		}

		public override string GetGitDeployPath()
		{
			return GetProgramFolder () + "/../../../../../../../deploy/" + Platform.Instance.GetSystemCode () + "/";
		}
    }
}




/*
 * 
 * 
 *
Last login: Mon Jun  2 00:05:12 on ttys000
clodo@fabrizios-imac.station:~$ sudo -s
Password:
root@fabrizios-imac.station:~$ 
root@fabrizios-imac.station:~$ 
root@fabrizios-imac.station:~$ networksetup -listallnetworkservices | grep -v denotes
Bluetooth DUN
Ethernet
FireWire
Wi-Fi
Bluetooth PAN
Thunderbolt Bridge
root@fabrizios-imac.station:~$ networksetup -getdnsservers Ethernet
There aren't any DNS Servers set on Ethernet.
root@fabrizios-imac.station:~$ networksetup -getdnsservers Bluetooth DUN
Bluetooth is not a recognized network service.
** Error: The parameters were not valid.
root@fabrizios-imac.station:~$ networksetup -getdnsservers "Bluetooth DUN"
There aren't any DNS Servers set on Bluetooth DUN.
root@fabrizios-imac.station:~$ networksetup -getdnsservers "Wi-Fi"
There aren't any DNS Servers set on Wi-Fi.
root@fabrizios-imac.station:~$ networksetup -getdnsservers "FireWire"
There aren't any DNS Servers set on FireWire.
root@fabrizios-imac.station:~$ networksetup -getdnsservers "Ethernet"
There aren't any DNS Servers set on Ethernet.
root@fabrizios-imac.station:~$ networksetup -setdnsservers "Ethernet" "10.4.0.1"
root@fabrizios-imac.station:~$ networksetup -getdnsservers "Ethernet"
10.4.0.1
root@fabrizios-imac.station:~$ networksetup -setdnsservers "Ethernet" empty
root@fabrizios-imac.station:~$ 
*/