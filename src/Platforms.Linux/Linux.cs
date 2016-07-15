﻿// <eddie_source_header>
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
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Xml;
using Eddie.Core;
using Mono.Unix.Native;

namespace Eddie.Platforms
{
    public class Linux : Platform
    {
		private string m_architecture = "";

		// Override
		public Linux()
		{
 			m_architecture = NormalizeArchitecture(ShellPlatformIndipendent("sh", "-c 'uname -m'", "", true, false).Trim());

			TrustCertificatePolicy.Activate();
		}

		public override string GetCode()
		{
			return "Linux";
		}

		public override string GetName()
		{
			if (File.Exists("/etc/issue"))
				return File.ReadAllText("/etc/issue").Replace("\n","").Replace("\r"," - ").Trim();
			else
				return base.GetName();
		}

		public override string GetOsArchitecture()
		{
			return m_architecture;
		}

        public override bool IsAdmin()
        {
            return (Environment.UserName == "root");
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

        public override void OpenUrl(string url)
        {
            System.Diagnostics.Process.Start("xdg-open", url);
        }

        public override string DirSep
        {
            get
            {
                return "/";
            }
        }

		public override string GetExecutableReport(string path)
		{
			return ShellCmd("ldd \"" + path + "\"");
		}

		public override string GetExecutablePath()
		{
			// We use this because querying .Net Assembly (what the base class do) doesn't work within Mkbundle.

			string output = "";
			StringBuilder builder = new StringBuilder(8192);
			if (Syscall.readlink("/proc/self/exe", builder) >= 0)
				output = builder.ToString();

			if ((output != "") && (new FileInfo(output).Name.ToLowerInvariant().StartsWith("mono")))
			{
				// Exception: Assembly directly load by Mono
				output = base.GetExecutablePath();
			}

			return output;
		}

        public override string GetUserFolder()
        {
            return Environment.GetEnvironmentVariable("HOME") + DirSep + ".airvpn";
        }

        public override string ShellCmd(string Command)
        {
            return Shell("sh", String.Format("-c '{0}'", Command));
        }

        public override string GetSystemFont()
        {
            if(File.Exists("/usr/bin/gsettings")) // gnome
            {
                string detected = Shell("/usr/bin/gsettings", "get org.gnome.desktop.interface font-name").Trim('\'');
                int posSize = detected.LastIndexOf(" ");
                if (posSize != -1)
                    detected = detected.Substring(0, posSize) + "," + detected.Substring(posSize + 1);
                // if (IsFontInstalled(detected)) // Don't work under Debian7
                    return detected;                
            }
                
            return base.GetSystemFont();
        }

        public override string GetSystemFontMonospace()
        {
            if (File.Exists("/usr/bin/gsettings")) // gnome
            {
                string detected = Shell("/usr/bin/gsettings", "get org.gnome.desktop.interface monospace-font-name").Trim('\'');
                int posSize = detected.LastIndexOf(" ");
                if (posSize != -1)
                    detected = detected.Substring(0, posSize) + "," + detected.Substring(posSize + 1);
                // if (IsFontInstalled(detected)) // Don't work under Debian7
                return detected;
            }

            return base.GetSystemFontMonospace();
        }

        public override void FlushDNS()
        {
            ShellCmd("/etc/rc.d/init.d/nscd restart");
        }

		public override void EnsureExecutablePermissions(string path)
		{
			if ((path == "") || (File.Exists(path) == false))
				return;

			ShellCmd("chmod +x \"" + path + "\"");			
		}


		public override void RouteAdd(RouteEntry r)
		{
			string cmd = "route add";

			cmd += " -net " + r.Address.Value;
			cmd += " netmask " + r.Mask.Value;
			cmd += " gw " + r.Gateway.Value;
			if(r.Metrics != "")
				cmd += " metric " + r.Metrics;
			if( (r.Mss != "") && (r.Mss != "0") )
				cmd += " mss " + r.Mss;
			if( (r.Window != "") && (r.Window != "0") ) 
				cmd += " window " + r.Window;
			if (r.Irtt != "")
				cmd += " irtt " + r.Irtt;

			if (r.Flags.Contains("!"))
				cmd += " reject";
			if (r.Flags.Contains("M"))
				cmd += " mod";
			if (r.Flags.Contains("D"))
				cmd += " dyn";
			if (r.Flags.Contains("R"))
				cmd += " reinstate";

			if(r.Interface != "")
				cmd += " dev " + r.Interface;

			ShellCmd(cmd);
		}

		public override void RouteRemove(RouteEntry r)
		{
			string cmd = "route del";

			cmd += " -net " + r.Address.Value;
			cmd += " gw " + r.Gateway.Value;
			cmd += " netmask " + r.Mask.Value;			
			/*
			if(r.Metrics != "")
				cmd += " metric " + r.Metrics;			
			*/
			if(r.Interface != "")
				cmd += " dev " + r.Interface;
			
			ShellCmd(cmd);
		}
		
		public override List<RouteEntry> RouteList()
		{	
			List<RouteEntry> entryList = new List<RouteEntry>();

			string result = ShellCmd("route -n -ee");

			string[] lines = result.Split('\n');
			foreach (string line in lines)
			{
				string[] fields = Utils.StringCleanSpace(line).Split(' ');

				if (fields.Length == 11)
				{
					RouteEntry e = new RouteEntry();
					e.Address = fields[0];
					e.Gateway = fields[1];
					e.Mask = fields[2];
					e.Flags = fields[3].ToUpperInvariant();
					e.Metrics = fields[4];
					// Ref, Use ignored
					e.Interface = fields[7];
					e.Mss = fields[8];
					e.Window = fields[9];
					e.Irtt = fields[10];
					
					if (e.Address.Valid == false)
						continue;
					if (e.Gateway.Valid == false)
						continue;
					if (e.Mask.Valid == false)
						continue;

					entryList.Add(e);
				}
			}

			return entryList;
		}

		public override string GenerateSystemReport()
		{
			string t = base.GenerateSystemReport();

			
			return t;
		}

		public override void OnAppStart()
		{
			base.OnAppStart();

			string dnsScriptPath = Software.FindResource("update-resolv-conf");
			if (dnsScriptPath == "")
			{
				Engine.Instance.Logs.Log(LogType.Error, "update-resolv-conf " + Messages.NotFound);
			}
		}

		public override void OnBuildOvpn(OvpnBuilder ovpn)
		{
			base.OnBuildOvpn(ovpn);
            
            if (GetDnsSwitchMode() == "resolvconf")
            {
                string dnsScriptPath = Software.FindResource("update-resolv-conf");
                if (dnsScriptPath != "")
                {
                    EnsureExecutablePermissions(dnsScriptPath);
                    Engine.Instance.Logs.Log(LogType.Info, Messages.DnsResolvConfScript);
                    ovpn.AppendDirective("script-security", "2", "");
                    ovpn.AppendDirective("up", dnsScriptPath, "");
                    ovpn.AppendDirective("down", dnsScriptPath, "");
                }
            }

            ovpn.AppendDirective("route-delay", "5", ""); // 2.8, to resolve some issue on some distro, ex. Fedora 21
		}

		public override bool OnCheckEnvironment()
		{
			if (Engine.Instance.Storage.Get("ipv6.mode") == "disable")
			{
				string sysctlName = "sysctl net.ipv6.conf.all.disable_ipv6";
				string ipV6 = ShellCmd(sysctlName).Replace(sysctlName, "").Trim().Trim(new char[] { '=', ' ', '\n', '\r' }); // 2.10.1

				if (ipV6 == "0")
				{
					if (Engine.Instance.OnAskYesNo(Messages.IpV6Warning))
					{
						Engine.Instance.Storage.Set("ipv6.mode", "none");
					}
					else
					{
						return false;
					}
				}
				else if (ipV6 == "1")
				{
					// Already disabled
				}
				else
				{
					Engine.Instance.Logs.Log(LogType.Warning, Messages.IpV6WarningUnableToDetect);
				}
			}

			return true;
		}

		public override void OnNetworkLockManagerInit()
		{
			base.OnNetworkLockManagerInit();

			Engine.Instance.NetworkLockManager.AddPlugin(new NetworkLockIptables());
		}

        public override string OnNetworkLockRecommendedMode()
        {
            return "linux_iptables";
        }

        public override bool OnDnsSwitchDo(string dns)
		{
			if (GetDnsSwitchMode() == "rename")
			{
				if (File.Exists("/etc/resolv.conf.airvpn") == false)
				{
					Engine.Instance.Logs.Log(LogType.Info, Messages.DnsRenameBackup);
					File.Copy("/etc/resolv.conf", "/etc/resolv.conf.airvpn");
				}

				Engine.Instance.Logs.Log(LogType.Info, Messages.DnsRenameDone);

				string text = "# " + Engine.Instance.GenerateFileHeader() + "\n\n";

				string[] dnsArray = dns.Split(',');

				foreach(string dnsSingle in dnsArray)
					text += "nameserver " + dnsSingle + "\n";

				File.WriteAllText("/etc/resolv.conf", text);
			}

			base.OnDnsSwitchDo(dns);

			return true;
		}

		public override bool OnDnsSwitchRestore()
		{
			// Cleaning rename method if pending
			if (File.Exists("/etc/resolv.conf.airvpn") == true)
			{
				Engine.Instance.Logs.Log(LogType.Info, Messages.DnsRenameRestored);

				File.Copy("/etc/resolv.conf.airvpn", "/etc/resolv.conf", true);
				File.Delete("/etc/resolv.conf.airvpn");
			}

			base.OnDnsSwitchRestore();

			return true;
		}

		public override string GetDriverAvailable()
		{
			if (File.Exists("/dev/net/tun"))
				return "Found, /dev/net/tun";
			else if (File.Exists("/dev/tun"))
				return "Found, /dev/tun";

			return "";
		}

		public override bool CanInstallDriver()
		{
			return false;
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





		public string GetDnsSwitchMode()
		{
			string current = Engine.Instance.Storage.Get("dns.mode").ToLowerInvariant();

			if (current == "auto")
			{
				// 2.10.1
				current = "rename";
				/*
				if (File.Exists("/sbin/resolvconf"))
					current = "resolvconf";
				else
					current = "rename";
				*/
			}
			
			// Fallback
			if( (current == "resolvconv") && (Software.FindResource("update-resolv-conf") == "") )
				current = "rename";

			if ((current == "resolvconv") && (File.Exists("/sbin/resolvconf") == false))
				current = "rename";


			return current;			
		}
    }
}
