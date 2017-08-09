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

namespace Eddie.Platforms.Linux
{
    public class Platform : Core.Platform
    {
		private string m_version = "";
		private string m_architecture = "";
        private UInt32 m_uid;
        private string m_logname;
		private string m_fontSystem;
		private string m_fontMonoSpace;

        // Override
        public Platform()
		{			
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

		public override string GetVersion()
		{
			return m_version;
		}

		public override void OnInit(bool cli)
		{
			base.OnInit(cli);

			m_version = SystemShell.ShellCmd("uname -a").Trim();
			m_architecture = NormalizeArchitecture(SystemShell.ShellCmd("uname -m").Trim());
			m_uid = 9999;
			UInt32.TryParse(SystemShell.ShellCmd("id -u"), out m_uid);

			// Debian, Environment.UserName == 'root', $SUDO_USER == '', $LOGNAME == 'root', whoami == 'root', logname == 'myuser'
			// Ubuntu, Environment.UserName == 'root', $SUDO_USER == 'myuser', $LOGNAME == 'root', whoami == 'root', logname == 'no login name'
			// Manjaro, same as Ubuntu
			m_logname = SystemShell.ShellCmd("echo $SUDO_USER").Trim(); // IJTF2 // TOCHECK
			if (m_logname == "")
				m_logname = SystemShell.ShellCmd("logname");
			if (m_logname.Contains("no login name"))
				m_logname = Environment.UserName;

			m_fontSystem = "";
			if (Platform.Instance.FileExists("/usr/bin/gsettings")) // gnome
			{
				m_fontSystem = SystemShell.Shell("/usr/bin/gsettings", "get org.gnome.desktop.interface font-name").Trim('\'');
				int posSize = m_fontSystem.LastIndexOf(" ");
				if (posSize != -1)
					m_fontSystem = m_fontSystem.Substring(0, posSize) + "," + m_fontSystem.Substring(posSize + 1);				
			}

			m_fontMonoSpace = "";
			if (Platform.Instance.FileExists("/usr/bin/gsettings")) // gnome
			{
				m_fontMonoSpace = SystemShell.Shell("/usr/bin/gsettings", "get org.gnome.desktop.interface monospace-font-name").Trim('\'');
				int posSize = m_fontMonoSpace.LastIndexOf(" ");
				if (posSize != -1)
					m_fontMonoSpace = m_fontMonoSpace.Substring(0, posSize) + "," + m_fontMonoSpace.Substring(posSize + 1);				
			}
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

			string result = SystemShell.Shell("lsattr","\"" + SystemShell.EscapePath(path) + "\"");

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
				SystemShell.ShellCmd("chattr " + flag + " \"" + SystemShell.EscapePath(path) + "\"");
            }
        }

		public override void FileEnsurePermission(string path, string mode)
		{
			if ((path == "") || (Platform.Instance.FileExists(path) == false))
				return;
			
			// 'mode' not escaped, called hard-coded.
			SystemShell.ShellCmd("chmod " + mode + " \"" + SystemShell.EscapePath(path) + "\"");
		}

		public override void FileEnsureExecutablePermission(string path)
		{
			FileEnsurePermission(path, "+x");
		}

		public override string GetExecutableReport(string path)
		{
			return SystemShell.ShellCmd("ldd \"" + SystemShell.EscapePath(path) + "\"");
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
            string output = SystemShell.ShellCmd("readlink /proc/" + pid.ToString() + "/exe");

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

		public override void ShellCommandDirect(string command, out string path, out string[] arguments)
		{
			path = "sh";
			arguments = new string[] { "-c", "'" + command + "'" };
		}

		public override int GetRecommendedRcvBufDirective()
		{
			return base.GetRecommendedRcvBufDirective();
		}

		public override int GetRecommendedSndBufDirective()
		{
			return base.GetRecommendedSndBufDirective();
		}

		public override string GetSystemFont()
        {
			if (m_fontSystem != "")
				return m_fontSystem;
			else                
				return base.GetSystemFont();
        }

        public override string GetSystemFontMonospace()
        {
			if (m_fontMonoSpace != "")
				return m_fontMonoSpace;
			else
				return base.GetSystemFontMonospace();
        }

        public override void FlushDNS()
        {
			// Under Manjaro for example, restart nscd it's mandatory:
			// - if you change /etc/resolv.conf for DNS queries, nscd will continue to use the old one if you have configured /etc/nsswitch.conf to use DNS for host lookups. In such a case, you need to restart nscd.			
			if (SystemShell.ShellCmd("ps -ef | grep [n]scd").Trim() != "")
			{
				if (Platform.Instance.FileExists("/usr/bin/systemctl"))
					SystemShell.ShellCmd("systemctl restart nscd");
				else
					SystemShell.ShellCmd("/etc/init.d/nscd restart");
			}

			if (SystemShell.ShellCmd("ps -ef | grep [d]nsmasq").Trim() != "")
			{
				if (Platform.Instance.FileExists("/usr/bin/systemctl"))
					SystemShell.ShellCmd("systemctl restart dnsmasq");
				else
					SystemShell.ShellCmd("/etc/init.d/dnsmasq restart");
			}
		}

		protected override void OpenDirectoryInFileManagerEx(string path)
        {
            // TOFIX Don't work well on all distro
            string args = " - " + m_logname + " -c 'xdg-open \"" + SystemShell.EscapePath(path) + "\"'"; // IJTF2 // TOCHECK
            SystemShell.Shell("su", args, false);
        }

        public override bool SearchTool(string name, string relativePath, ref string path, ref string location)
        {
            string pathBin = "/usr/bin/" + name;
            if (Platform.Instance.FileExists(pathBin))
            {
                path = pathBin;
                location = "system";
                return true;
            }

            string pathSBin = "/usr/sbin/" + name;
            if (Platform.Instance.FileExists(pathSBin))
            {
                path = pathSBin;
                location = "system";
                return true;
            }

            string pathShare = "/usr/share/" + Lib.Common.Constants.NameCompatibility + "/" + name;
            if (Platform.Instance.FileExists(pathShare))
            {
                path = pathShare;
                location = "system";
                return true;
            }

            return base.SearchTool(name, relativePath, ref path, ref location);
        }

        public override long Ping(string host, int timeoutSec)
        {
			string result = SystemShell.Shell("ping", "-c 1 -w " + timeoutSec.ToString() + " -q -n " + SystemShell.EscapeHost(host));
            
            string sMS = Utils.ExtractBetween(result.ToLowerInvariant(), "min/avg/max/mdev = ", "/");
            float iMS;
            if (float.TryParse(sMS, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out iMS) == false)
                iMS = -1;
            
            return (long) iMS;
        }
		
		public override void RouteAdd(RouteEntry r)
		{
			string cmd = "route add";

			cmd += " -net " + r.Address.Address;
			cmd += " netmask " + r.Mask.Address;
			cmd += " gw " + r.Gateway.Address;
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

			SystemShell.ShellCmd(cmd); // IJTF2 // TOCHECK
        }
        
		public override void RouteRemove(RouteEntry r)
		{
			string cmd = "route del";

			cmd += " -net " + r.Address.Address;
			cmd += " gw " + r.Gateway.Address;
			cmd += " netmask " + r.Mask.Address;			
			/*
			if(r.Metrics != "")
				cmd += " metric " + r.Metrics;			
			*/
			if(r.Interface != "")
				cmd += " dev " + r.Interface;

			SystemShell.ShellCmd(cmd); // IJTF2 // TOCHECK
        }

		public override IpAddresses ResolveDNS(string host)
		{
			IpAddresses result = new IpAddresses();

			// Note: CNAME record are automatically followed.
			string hostout = SystemShell.ShellCmd("host -W 5 " + SystemShell.EscapeHost(host));
			
			foreach (string line in hostout.Split('\n'))
			{
				string ipv4 = Utils.RegExMatchOne(line, "^.*? has address (.*?)$");
				if (ipv4 != "")
					result.Add(ipv4.Trim());

				string ipv6 = Utils.RegExMatchOne(line, "^.*? has IPv6 address (.*?)$");
				if (ipv6 != "")
					result.Add(ipv6.Trim());
			}
			return result;
		}

		public override IpAddresses DetectDNS()
		{
			IpAddresses list = new IpAddresses();
			if (FileExists("/etc/resolv.conf"))
			{
				string o = FileContentsReadText("/etc/resolv.conf");
				foreach (string line in o.Split('\n'))
				{
					if (line.Trim().StartsWith("#"))
						continue;
					if (line.Trim().StartsWith("nameserver"))
					{
						list.Add(line.Substring(11).Trim());
					}
				}
			}
			return list;
		}

		public override List<RouteEntry> RouteList()
		{	
			List<RouteEntry> entryList = new List<RouteEntry>();

			string result = SystemShell.ShellCmd("route -n -ee");

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

		public override void OnReport(Report report)
		{
			base.OnReport(report);

			report.Add("UID", Conversions.ToString(m_uid));
			report.Add("LogName", m_logname);
			report.Add("ip addr show", SystemShell.ShellCmd("ip addr show"));
			report.Add("ip link show", SystemShell.ShellCmd("ip link show"));
			report.Add("ip route show", SystemShell.ShellCmd("ip route show"));
		}
		
		public override Dictionary<int, string> GetProcessesList()
		{
			Dictionary<int, string> result = new Dictionary<int, string>();
			String resultS = SystemShell.ShellCmd("ps -eo pid,command");
			string[] resultA = resultS.Split('\n');
			foreach (string pS in resultA)
			{
				int posS = pS.IndexOf(' ');
				if (posS != -1)
				{
					int pid = Conversions.ToInt32(pS.Substring(0, posS).Trim());
					string name = pS.Substring(posS).Trim();
					result[pid] = name;
				}
			}

			return result;
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
                    FileEnsureExecutablePermission(dnsScriptPath);
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
				string ipV6 = SystemShell.ShellCmd(sysctlName).Replace(sysctlName, "").Trim().Trim(new char[] { '=', ' ', '\n', '\r' }); // 2.10.1

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
                string path = Utils.GetTempPath() + "/" + Constants.Name + "_" + Constants.AppID + ".pid";
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
                string path = Utils.GetTempPath() + "/" + Constants.Name + "_" + Constants.AppID + ".pid";
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

        public override bool OnDnsSwitchDo(IpAddresses dns)
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

				foreach(IpAddress dnsSingle in dns.IPs)
					text += "nameserver " + dnsSingle.Address + "\n";

                FileContentsWriteText("/etc/resolv.conf", text);
				Platform.Instance.FileEnsurePermission("/etc/resolv.conf", "644");
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
