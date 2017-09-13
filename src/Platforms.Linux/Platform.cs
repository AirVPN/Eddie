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
using Mono.Unix;

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

			m_version = SystemShell.Shell1(LocateExecutable("uname"), "-a");
			m_architecture = NormalizeArchitecture(SystemShell.Shell1(LocateExecutable("uname"),"-m").Trim());
			m_uid = 9999;
			UInt32.TryParse(SystemShell.Shell1(LocateExecutable("id"),"-u"), out m_uid);

			// Debian, Environment.UserName == 'root', $SUDO_USER == '', $LOGNAME == 'root', whoami == 'root', logname == 'myuser'
			// Ubuntu, Environment.UserName == 'root', $SUDO_USER == 'myuser', $LOGNAME == 'root', whoami == 'root', logname == 'no login name'
			// Manjaro, same as Ubuntu
			m_logname = Environment.GetEnvironmentVariable("SUDO_USER");
			if (m_logname == null)
				m_logname = "";
			else
				m_logname = m_logname.Trim();			
			if (m_logname == "")
				m_logname = SystemShell.Shell0(LocateExecutable("logname"));
			if (m_logname.Contains("no login name"))
				m_logname = Environment.UserName;
			if (m_logname == null)
				m_logname = "";

			m_fontSystem = "";
			string gsettingsPath = LocateExecutable("gsettings"); // gnome
			if (gsettingsPath != "")
			{				
				m_fontSystem = SystemShell.Shell1(gsettingsPath, "get org.gnome.desktop.interface font-name").Trim('\'');
				int posSize = m_fontSystem.LastIndexOf(" ");
				if (posSize != -1)
					m_fontSystem = m_fontSystem.Substring(0, posSize) + "," + m_fontSystem.Substring(posSize + 1);			
			}

			m_fontMonoSpace = "";
			if (gsettingsPath != "")
			{
				m_fontMonoSpace = SystemShell.Shell1(gsettingsPath, "get org.gnome.desktop.interface monospace-font-name").Trim('\'');
				int posSize = m_fontMonoSpace.LastIndexOf(" ");
				if (posSize != -1)
					m_fontMonoSpace = m_fontMonoSpace.Substring(0, posSize) + "," + m_fontMonoSpace.Substring(posSize + 1);				
			}

			Native.eddie_signal((int)Native.Signum.SIGINT, SignalCallback);
			Native.eddie_signal((int)Native.Signum.SIGTERM, SignalCallback);
			Native.eddie_signal((int)Native.Signum.SIGUSR1, SignalCallback);
			Native.eddie_signal((int)Native.Signum.SIGUSR2, SignalCallback);
		}

		private static void SignalCallback(int signum)
		{
			Native.Signum sig = (Native.Signum) signum;
			if (sig == Native.Signum.SIGINT)
				Engine.Instance.OnSignal("SIGINT");
			else if (sig == Native.Signum.SIGTERM)
				Engine.Instance.OnSignal("SIGTERM");
			else if (sig == Native.Signum.SIGUSR1)
				Engine.Instance.OnSignal("SIGUSR1");
			else if (sig == Native.Signum.SIGUSR2)
				Engine.Instance.OnSignal("SIGUSR2");
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

		public override string EnvPathSep
		{
			get
			{
				return ":";
			}
		}

		public override bool FileImmutableGet(string path)
        {
			// We don't find a better direct method in Mono/Posix without adding ioctl references
			// The list of flags can be different between Linux distro (for example 16 on Debian, 19 on Manjaro)

			if ((path == "") || (FileExists(path) == false))
				return false;

			int result = Native.eddie_file_get_immutable(path);
			return (result == 1);
			/* // TOCLEAN
			string lsattrPath = LocateExecutable("lsattr");
			if (lsattrPath != "")
			{
				SystemShell s = new SystemShell();
				s.Path = lsattrPath;
				s.Arguments.Add(SystemShell.EscapePath(path));
				s.NoDebugLog = true;

				if (s.Run())
				{
					string result = s.Output;

					if (result.StartsWith("lsattr: ")) // Generic error
						return false;

					if (result.IndexOf(' ') != -1)
						result = result.Substring(0, result.IndexOf(' '));

					return (result.IndexOf("-i-") != -1);
				}
				else
					return false;
			}
			else
				return false;
			*/
        }

        public override void FileImmutableSet(string path, bool value)
        {
			if ((path == "") || (FileExists(path) == false))
				return;

			if (FileImmutableGet(path) == value)
				return;

			Engine.Instance.Logs.LogDebug("eddie_file_set_immutable:" + value.ToString());
			Native.eddie_file_set_immutable(path, value ? 1 : 0);
			/* // TOCLEAN
			string chattrPath = LocateExecutable("chattr");
			if(chattrPath != "")
            {
                string flag = (value ? "+i" : "-i");
				SystemShell s = new SystemShell();
				s.Path = chattrPath;
				s.Arguments.Add(flag);
				s.Arguments.Add(SystemShell.EscapePath(path));
				s.Run();
			}
			*/
        }

		public override bool FileEnsurePermission(string path, string mode)
		{
			if ((path == "") || (FileExists(path) == false))
				return false;

			/* // TOCLEAN
			string chmodPath = LocateExecutable("chmod");
			if (chmodPath != "")
			{
				// 'mode' not escaped, called hard-coded.
				SystemShell s = new SystemShell();
				s.Path = chmodPath;
				s.Arguments.Add(mode);
				s.Arguments.Add(SystemShell.EscapePath(path));
				s.NoDebugLog = true;
				s.Run();
			}
			*/
			int result = Native.eddie_file_get_mode(path);
			if (result == -1)
			{
				Engine.Instance.Logs.Log(LogType.Warning, "Failed to detect permissions on '" + path + "'.");
				return false;
			}
			int newResult = 0;
			if (mode == "600")
				newResult = (int) Native.FileMode.Mode0600;
			else if (mode == "644")
				newResult = (int)Native.FileMode.Mode0644;
			
			if(newResult == 0)
			{
				Engine.Instance.Logs.Log(LogType.Warning, "Unexpected permission '" + mode + "'");
				return false;
			}

			if (newResult != result)
			{
				result = Native.eddie_file_set_mode(path, newResult);
				if (result == -1)
				{
					Engine.Instance.Logs.Log(LogType.Warning, "Failed to set permissions on '" + path + "'.");
					return false;
				}
			}

			return true;
		}

		public override bool FileEnsureExecutablePermission(string path)
		{
			int result = Native.eddie_file_get_mode(path);
			if (result == -1)
			{
				Engine.Instance.Logs.Log(LogType.Warning, "Failed to detect if '" + path + "' is executable");
				return false;
			}
			
			int newResult = result | 73; // +x :<> (S_IXUSR | S_IXGRP | S_IXOTH) 

			if (newResult != result)
			{
				result = Native.eddie_file_set_mode(path, newResult);
				if (result == -1)
				{
					Engine.Instance.Logs.Log(LogType.Warning, "Failed to mark '" + path + "' as executable");
					return false;
				}
			}

			return true;

			// FileEnsurePermission(path, "+x"); // TOCLEAN
		}

		public override string GetExecutableReport(string path)
		{
			string lddPath = LocateExecutable("ldd");
			if (lddPath != "")
				return SystemShell.Shell1(lddPath, SystemShell.EscapePath(path));
			else
				return "'ldd' " + Messages.NotFound;
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
            string output = SystemShell.Shell1(LocateExecutable("readlink"), "/proc/" + pid.ToString() + "/exe");

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

		public override string LocateExecutable(string name)
		{
			if (m_LocateExecutableCache.ContainsKey(name))
				return m_LocateExecutableCache[name];

			// On some system, for example OpenSuse, 'xdg-su' don't reflect the root path env-var,
			// and for this reason a simple shell to 'sysctl' don't work.
			List<string> paths = GetEnvironmentPaths();
			if (paths.Contains("/sbin") == false)
				paths.Add("/sbin");
			if (paths.Contains("/usr/sbin") == false)
				paths.Add("/usr/sbin");
			if (paths.Contains("/usr/local/sbin") == false)
				paths.Add("/usr/local/sbin");			
			if (paths.Contains("/root/bin") == false)
				paths.Add("/root/bin");			
			return LocateExecutable(name, paths);
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
			base.FlushDNS();
			
			string servicePath = LocateExecutable("service");
			if(servicePath != "")
			{
				Dictionary<string, string> services = new Dictionary<string, string>();
				string list = SystemShell.Shell1(servicePath, "--status-all");
				list = Utils.StringCleanSpace(list);
				foreach(string line in list.Split('\n'))
				{
					if (line.Contains(" running "))
					{
						int pos = line.IndexOf(".service");
						if (pos != -1)
						{
							string name = line.Substring(0, pos).Trim();
							services[name] = name;
						}
					}
				}

				if(services.ContainsKey("nscd"))
					SystemShell.Shell2(servicePath, "nscd", "restart");
				if (services.ContainsKey("dnsmasq"))
					SystemShell.Shell2(servicePath, "dnsmasq", "restart");
				if (services.ContainsKey("named"))
					SystemShell.Shell2(servicePath, "named", "restart");
				if (services.ContainsKey("bind9"))
					SystemShell.Shell2(servicePath, "bind9", "restart");
			}
			else
			{
				if (FileExists("/etc/init.d/nscd"))
					SystemShell.Shell1("/etc/init.d/nscd", "restart");
				if (FileExists("/etc/init.d/dnsmasq"))
					SystemShell.Shell1("/etc/init.d/dnsmasq", "restart");
				if (FileExists("/etc/init.d/named"))
					SystemShell.Shell1("/etc/init.d/named", "restart");
				if (FileExists("/etc/init.d/bind9"))
					SystemShell.Shell1("/etc/init.d/bind9", "restart");
			}

			// On some system, for example Fedora, nscd caches are saved to disk,
			// located in /var/db/nscd, and not flushed with a simple restart. 
			string nscdPath = LocateExecutable("nscd");
			if(nscdPath != "")
			{
				SystemShell.Shell1(nscdPath, "--invalidate=hosts");
			}
		}

		protected override void OpenDirectoryInFileManagerEx(string path)
        {
            // TOFIX Don't work well on all distro
            string args = " - " + m_logname + " -c 'xdg-open \"" + SystemShell.EscapePath(path) + "\"'"; // IJTF2 // TOCHECK
            SystemShell.ShellX("su", args, false);
        }

        public override bool SearchTool(string name, string relativePath, ref string path, ref string location)
        {
			string pathBin = LocateExecutable(name);
			if(pathBin != "")
            {
                path = pathBin;
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
			float iMS = -1;

			string pingPath = LocateExecutable("ping");
			if (pingPath != "")
			{
				SystemShell s = new SystemShell();
				s.Path = pingPath;
				s.Arguments.Add("-c 1");
				s.Arguments.Add("-w " + timeoutSec.ToString());
				s.Arguments.Add("-q");
				s.Arguments.Add("-n");
				s.Arguments.Add(SystemShell.EscapeHost(host));
				s.NoDebugLog = true;
				
				if (s.Run())
				{
					string result = s.Output;
					string sMS = Utils.ExtractBetween(result.ToLowerInvariant(), "min/avg/max/mdev = ", "/");
					if (float.TryParse(sMS, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out iMS) == false)
						iMS = -1;
				}
			}
			
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

			string getentPath = LocateExecutable("getent");
			if (getentPath != "")
			{
				// Note: CNAME record are automatically followed.
				SystemShell s = new SystemShell();
				s.Path = getentPath;
				s.Arguments.Add("ahosts");
				s.Arguments.Add(SystemShell.EscapeHost(host));
				s.NoDebugLog = true;
				if (s.Run())
				{
					string o = s.Output;
					o = Utils.StringCleanSpace(o);
					foreach (string line in o.Split('\n'))
					{
						string[] fields = line.Split(' ');
						if (fields.Length < 2)
							continue;
						if (fields[1].Trim() != "STREAM")
							continue;
						result.Add(fields[0].Trim());
					}
				}
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

			// TOFIX: "route" not available on recent Linux systems.
			// Need to be adapted to "ip". But it's used only for "Remove default gateway" feature, useless for a lots of reason, deprecated soon.
			string routePath = LocateExecutable("route");
			if (routePath == "")
			{
				Engine.Instance.Logs.Log(LogType.Error, "'route' " + Messages.NotFound);
			}
			else
			{
				string result = SystemShell.Shell2(routePath, "-n", "-ee");

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
			}

			return entryList;
		}

		public override void OnReport(Report report)
		{
			base.OnReport(report);

			report.Add("TestDll", Native.eddie_linux_get_3().ToString()); // ClodoTemp

			report.Add("UID", Conversions.ToString(m_uid));
			report.Add("LogName", m_logname);
			report.Add("ip addr show", (LocateExecutable("ip") != "") ? SystemShell.Shell2(LocateExecutable("ip"), "addr", "show") : "'ip' " + Messages.NotFound);
			report.Add("ip link show", (LocateExecutable("ip") != "") ? SystemShell.Shell2(LocateExecutable("ip"), "link", "show") : "'ip' " + Messages.NotFound);
			report.Add("ip route show", (LocateExecutable("ip") != "") ? SystemShell.Shell2(LocateExecutable("ip"), "route", "show") : "'ip' " + Messages.NotFound);			
		}

		public override bool RestartAsRoot()
		{
			string command = "";
			string arguments = "";


			string command2 = "";
			string executablePath = Platform.Instance.GetExecutablePath();
			string cmdline = CommandLine.SystemEnvironment.GetFull();
			if (executablePath.Substring(executablePath.Length - 4).ToLowerInvariant() == ".exe")
				command2 += "mono ";
			command2 += Platform.Instance.GetExecutablePath();
			command2 += " ";
			command2 += cmdline;			
			command2 += " console.mode=none"; // 2.13.6, otherwise CancelKeyPress (mono bug?) and stdout fill the non-root non-blocked terminal.
			command2 = command2.Trim(); // 2.11.11
			bool waitEnd = false;

			if (LocateExecutable("kdesudo") != "")
			{
				command = "kdesudo";
				arguments = "";
				arguments += " -u root"; // Administrative privileges
				arguments += " -d"; // Don't show commandline
				arguments += " --comment \"" + Messages.AdminRequiredPasswordPrompt + "\"";
				arguments += " -c "; // The command
									 //arguments += " \"" + command2 + "\"";
				arguments += " \"" + command2 + "\"";
			}
			else if (LocateExecutable("kdesu") != "")
			{
				command = "kdesu";
				arguments = "";
				arguments += " -u root"; // Administrative privileges
				arguments += " -d"; // Don't show commandline
									//arguments += " --comment \"" + Messages.AdminRequiredPasswordPrompt + "\"";
				arguments += " -c "; // The command
									 //arguments += " \"" + command2 + "\"";
				arguments += " \"" + command2 + "\"";
			}
			/*
			 * Under Debian, gksudo don't work, gksu work...
			if (Platform.Instance.FileExists("/usr/bin/gksudo"))
			{
				command = "gksudo";
				arguments = "";
				arguments += " -u root"; // Administrative privileges
				arguments += " -m \"" + Messages.AdminRequiredPasswordPrompt + "\"";
				arguments += " \"" + command2 + "\"";
			}
			else 
			*/
			else if (LocateExecutable("gksu") != "")
			{
				command = "gksu";
				arguments = "";
				arguments += " -u root"; // Administrative privileges
				arguments += " -m \"" + Messages.AdminRequiredPasswordPrompt + "\"";
				arguments += " \"" + command2 + "\"";
			}
			else if (LocateExecutable("xdg-su") != "") // OpenSUSE
			{
				command = "xdg-su";
				arguments = "";
				arguments += " -u root"; // Administrative privileges
				arguments += " -c "; // The command
				arguments += " \"" + command2 + "\"";
			}
			else if (LocateExecutable("beesu") != "") // Fedora
			{
				command = "beesu";
				arguments = "";
				arguments += " " + command2 + "";
			}
			/*
			else if (Platform.Instance.FileExists("/usr/bin/pkexec"))
			{
				// Different behiavour on different platforms
				command = "pkexec";
				arguments = "";
				arguments = " env DISPLAY=$DISPLAY XAUTHORITY=$XAUTHORITY";
				arguments += " " + command2 + "";

				// For this bug: https://lists.ubuntu.com/archives/foundations-bugs/2012-July/100103.html
				// We need to keep alive the current process, otherwise 'Refusing to render service to dead parents.'.
				waitEnd = true;

				// Still don't work.
			}
			*/

			if (command != "")
			{
				Engine.Instance.Logs.Log(LogType.Verbose, Messages.AdminRequiredRestart);
				
				SystemShell.ShellX(command.Trim(), arguments.Trim(), waitEnd); // IJTF2
			}
			else
			{
				Engine.Instance.Logs.Log(LogType.Fatal, Messages.AdminRequiredRestartFailed);
			}

			return true;
		}

		public override Dictionary<int, string> GetProcessesList()
		{	
			Dictionary<int, string> result = new Dictionary<int, string>();
			string psPath = LocateExecutable("ps");
			if (psPath != "")
			{
				string resultS = SystemShell.Shell2(psPath, "-eo", "pid,command");
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
			}

			return result;
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

		public override bool OnCheckEnvironmentApp()
		{
			string dnsScriptPath = Software.FindResource("update-resolv-conf");
			if (dnsScriptPath == "")
				Engine.Instance.Logs.Log(LogType.Error, "'update-resolv-conf' " + Messages.NotFound);

			/* // TOCLEAN
			string lsattrPath = LocateExecutable("lsattr");
			if(lsattrPath == "")
				Engine.Instance.Logs.Log(LogType.Error, "'lsattr' " + Messages.NotFound);

			string chattrPath = LocateExecutable("chattr");
			if (chattrPath == "")
				Engine.Instance.Logs.Log(LogType.Error, "'chattr' " + Messages.NotFound);

			string chmodPath = LocateExecutable("chmod");
			if (chmodPath == "")
				Engine.Instance.Logs.Log(LogType.Error, "'chmod' " + Messages.NotFound);
			*/

			string pingPath = LocateExecutable("ping");
			if (pingPath == "")
				Engine.Instance.Logs.Log(LogType.Error, "'ping' " + Messages.NotFound);

			string getentPath = LocateExecutable("getent");
			if (getentPath == "")
				Engine.Instance.Logs.Log(LogType.Error, "'getent' " + Messages.NotFound);

			string psPath = LocateExecutable("ps");
			if (psPath == "")
				Engine.Instance.Logs.Log(LogType.Error, "'ps' " + Messages.NotFound);

			string ipPath = LocateExecutable("ip");
			if (ipPath == "")
				Engine.Instance.Logs.Log(LogType.Error, "'ip' " + Messages.NotFound);

			/* // Used only by "Remove default gateway" */
			/*
			string routePath = LocateExecutable("route");
			if (routePath == "")
				Engine.Instance.Logs.Log(LogType.Error, "'route' " + Messages.NotFound);
			*/

			return true;
		}

		public override bool OnCheckEnvironmentSession()
		{
			if (Engine.Instance.Storage.GetLower("ipv6.mode") == "disable")
			{
				string keyname = "net.ipv6.conf.all.disable_ipv6";
				string ipV6 = "";
				string sysctlPath = LocateExecutable("sysctl");
				if(sysctlPath != "")
				{
					ipV6 = SystemShell.Shell1(sysctlPath, keyname).Replace(keyname, "").Trim().Trim(new char[] { '=', ' ', '\n', '\r' }); // 2.10.1
				}

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
