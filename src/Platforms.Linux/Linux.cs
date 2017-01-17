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
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Xml;
using Eddie.Lib.Common;
using Eddie.Core;
// using Mono.Unix.Native; // Removed in 2.11

namespace Eddie.Platforms
{
    public class Linux : Platform
    {
		private string m_architecture = "";
        private UInt32 m_uid;
        private string m_logname;

        // Override
        public Linux()
		{
 			m_architecture = NormalizeArchitecture(ShellPlatformIndipendent("sh", "-c 'uname -m'", "", true, false).Trim());            
            m_uid = 9999;
            UInt32.TryParse(ShellCmd("id -u"), out m_uid);


            // Debian, Environment.UserName == 'root', $SUDO_USER == '', $LOGNAME == 'root', whoami == 'root', logname == 'myuser'
            // Ubuntu, Environment.UserName == 'root', $SUDO_USER == 'myuser', $LOGNAME == 'root', whoami == 'root', logname == 'no login name'
            // Manjaro, same as Ubuntu
            m_logname = ShellCmd("echo $SUDO_USER").Trim();
            if (m_logname == "")
                m_logname = ShellCmd("logname");
            if (m_logname.Contains("no login name"))
                m_logname = Environment.UserName;
            
            TrustCertificatePolicy.Activate();
		}

		public override string GetCode()
		{
			return "Linux";
		}

		public override string GetName()
		{
			if (Platform.Instance.FileExists("/etc/issue"))
				return FileContentsReadText("/etc/issue").Replace("\n","").Replace("\r"," - ").Trim();
			else
				return base.GetName();
		}

        public override string GetOsArchitecture()
		{
			return m_architecture;
		}

        public override bool IsAdmin()
        {
            return (m_uid == 0);
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

        public override bool FileImmutableGet(string path)
        {   
            // We don't find a better direct method in Mono/Posix without adding ioctl references
            // The list of flags can be different between Linux distro (for example 16 on Debian, 19 on Manjaro)
                     
            if (FileExists(path) == false)
                return false;

            string result = ShellCmd("lsattr \"" + Utils.StringNormalizePath(path) + "\"");
                        
            /* // < 2.11.11
            if (result.IndexOf(' ') != 16) 
                return false;
            if (result[4] == 'i')
                return true;
            */

            if (result.StartsWith("lsattr: ")) // Generic error
                return false;

            if (result.IndexOf(' ') != -1)
                result = result.Substring(0, result.IndexOf(' '));

            return (result.IndexOf("-i-") != -1);            
        }

        public override void FileImmutableSet(string path, bool value)
        {
            if (FileExists(path))
            {
                string flag = (value ? "+i" : "-i");
                ShellCmd("chattr " + flag + " \"" + Utils.StringNormalizePath(path) + "\"");
            }
        }
        
        public override string GetExecutableReport(string path)
		{
			return ShellCmd("ldd \"" + Utils.StringNormalizePath(path) + "\"");
		}

		public override string GetExecutablePathEx()
		{
            // We use this because querying .Net Assembly (what the base class do) doesn't work within Mkbundle.

            // TOFIX: Linux and OS X version are different, merge. Probably OS X it's more a clean approach.

            // Removed in 2.11 to avoid dependencies with libMonoPosixHelper.so
            // Useless, still required, but at least it's an external requirement.
            /*
			string output = "";
			StringBuilder builder = new StringBuilder(8192);
			if (Syscall.readlink("/proc/self/exe", builder) >= 0)
				output = builder.ToString();
            */
            int pid = Process.GetCurrentProcess().Id;
            string output = Platform.Instance.ShellCmd("readlink /proc/" + pid.ToString() + "/exe");

            if ((output != "") && (new FileInfo(output).Name.ToLowerInvariant().StartsWith("mono")))
			{
				// Exception: Assembly directly load by Mono
				output = base.GetExecutablePathEx();
			}

			return output;
		}

        public override string GetUserPathEx()
        {
            return Environment.GetEnvironmentVariable("HOME") + DirSep + ".airvpn";
        }

        public override string ShellCmd(string Command)
        {
            return Shell("sh", String.Format("-c '{0}'", Command));
        }

        public override string GetSystemFont()
        {
            if(Platform.Instance.FileExists("/usr/bin/gsettings")) // gnome
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
            if (Platform.Instance.FileExists("/usr/bin/gsettings")) // gnome
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
            // Too much issues to find a method available on all Linux platform.
            /*
            ShellCmd("/etc/rc.d/init.d/nscd restart");
            if (Platform.Instance.FileExists("/usr/bin/systemctl"))
                ShellCmd("systemctl restart nscd");
            */
        }

        protected override void OpenDirectoryInFileManagerEx(string path)
        {
            // TOFIX Don't work well on all distro
            string args = " - " + m_logname + " -c 'xdg-open \"" + path + "\"'";
            Shell("su", args, false);
        }

        public override long Ping(string host, int timeoutSec)
        {
            string cmd = "ping -c 1 -w " + timeoutSec.ToString() + " -q -n " + Utils.SafeStringHost(host);
            string result = ShellCmd(cmd);
            
            string sMS = Utils.ExtractBetween(result.ToLowerInvariant(), "min/avg/max/mdev = ", "/");
            float iMS;
            /*
            if (float.TryParse(sMS, out iMS))
                return (Int64)iMS;
            else
                return -1;
            */
            if (float.TryParse(sMS, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out iMS) == false)
                iMS = -1;
            
            return (long) iMS;
        }

        public override void EnsureExecutablePermissions(string path)
		{
			if ((path == "") || (Platform.Instance.FileExists(path) == false))
				return;

			ShellCmd("chmod +x \"" + Utils.StringNormalizePath(path) + "\"");			
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

        public override void ResolveWithoutAnswer(string host)
        {
            // Base method with Dns.GetHostEntry have cache issue, for example on Fedora.
            if (Platform.Instance.FileExists("/usr/bin/host"))
                ShellCmd("host -W 5 -t A " + Utils.SafeStringHost(host));
            else
                base.ResolveWithoutAnswer(host);
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

            t += "\n\n-- Linux\n";

            t += "UID: " + m_uid + "\n";
            t += "LogName: " + m_logname + "\n";

            t += "\n-- ifconfig\n";
            t += ShellCmd("ifconfig");

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
                    Engine.Instance.Logs.Log(LogType.Verbose, Messages.DnsResolvConfScript);
                    ovpn.AppendDirective("script-security", "2", "");
                    ovpn.AppendDirective("up", dnsScriptPath, "");
                    ovpn.AppendDirective("down", dnsScriptPath, "");
                }
            }

            ovpn.AppendDirective("route-delay", "5", ""); // 2.8, to resolve some issue on some distro, ex. Fedora 21
		}

		public override bool OnCheckEnvironment()
		{
			if (Engine.Instance.Storage.GetLower("ipv6.mode") == "disable")
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
					Engine.Instance.Logs.Log(LogType.Verbose, Messages.IpV6WarningUnableToDetect);
				}
			}

			return true;
		}

        public override bool OnCheckSingleInstance()
        {
            try
            {
                int currentId = Process.GetCurrentProcess().Id;
                string path = Utils.GetTempPath() + "/" + Constants.Name2 + "_" + Constants.AppID + ".pid";
                if (File.Exists(path) == false)
                {                    
                }
                else
                {
                    int otherId;
                    if(int.TryParse(Platform.Instance.FileContentsReadText(path), out otherId))
                    {
                        string procFile = "/proc/" + otherId.ToString() + "/cmdline";
                        if(File.Exists(procFile))
                        {
                            return false;
                        }
                    }
                }

                Platform.Instance.FileContentsWriteText(path, currentId.ToString());

                return true;
            }
            catch(Exception e)
            {
                Engine.Instance.Logs.Log(e);
                return true;
            }            
        }

        public override void OnCheckSingleInstanceClear()
        {
            try
            {
                int currentId = Process.GetCurrentProcess().Id;
                string path = Utils.GetTempPath() + "/" + Constants.Name2 + "_" + Constants.AppID + ".pid";
                if (File.Exists(path))
                {
                    int otherId;
                    if (int.TryParse(Platform.Instance.FileContentsReadText(path), out otherId))
                    {
                        if (otherId == currentId)
                            Platform.Instance.FileDelete(path);
                    }
                }
            }
            catch (Exception e)
            {
                Engine.Instance.Logs.Log(e);
            }
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
                if (FileExists("/etc/resolv.conf.eddie") == false)
				{
                    if (FileExists("/etc/resolv.conf"))
                    {
                        Engine.Instance.Logs.Log(LogType.Verbose, Messages.DnsRenameBackup);
                        FileMove("/etc/resolv.conf", "/etc/resolv.conf.eddie");
                    }
				}

                Engine.Instance.Logs.Log(LogType.Verbose, Messages.DnsRenameDone);

				string text = "# " + Engine.Instance.GenerateFileHeader() + "\n\n";

				string[] dnsArray = dns.Split(',');

				foreach(string dnsSingle in dnsArray)
					text += "nameserver " + dnsSingle + "\n";

                FileContentsWriteText("/etc/resolv.conf", text);                
            }

			base.OnDnsSwitchDo(dns);

			return true;
		}

		public override bool OnDnsSwitchRestore()
		{
			// Cleaning rename method if pending
			if (FileExists("/etc/resolv.conf.eddie") == true)
			{
                if (FileExists("/etc/resolv.conf"))
                    FileDelete("/etc/resolv.conf");
                
                Engine.Instance.Logs.Log(LogType.Verbose, Messages.DnsRenameRestored);

				FileMove("/etc/resolv.conf.eddie", "/etc/resolv.conf");
			}

			base.OnDnsSwitchRestore();

			return true;
		}

		public override string GetDriverAvailable()
		{
			if (Platform.Instance.FileExists("/dev/net/tun"))
				return "Found, /dev/net/tun";
			else if (Platform.Instance.FileExists("/dev/tun"))
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
			string current = Engine.Instance.Storage.GetLower("dns.mode");

			if (current == "auto")
			{
				// 2.10.1
				current = "rename";
                /*
				if (Platform.Instance.FileExists("/sbin/resolvconf"))
					current = "resolvconf";
				else
					current = "rename";
				*/
            }

            // Fallback
            if ( (current == "resolvconv") && (Software.FindResource("update-resolv-conf") == "") )
				current = "rename";

			if ((current == "resolvconv") && (Platform.Instance.FileExists("/sbin/resolvconf") == false))
				current = "rename";


			return current;			
		}

        public bool DetectLinuxHostNameIssue()
        {
            // https://bugzilla.xamarin.com/show_bug.cgi?id=42249
            // If hostname can't be resolved (missing entry in /etc/hosts), Mono::Ping throw an exception.
            // Never really called, because in >2.11.4 we use a shell for pinging under Linux.
            try
            {
                foreach (IPAddress addr in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {

                }
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }        
    }
}
