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
using Eddie.Common;
using Eddie.Core;
// using Mono.Unix.Native; // Removed in 2.11
// using Mono.Unix; // Removed in 2.14.4

namespace Eddie.Platform.Linux
{
	public class Platform : Core.Platform
	{
		private string m_version = "";
		private string m_architecture = "";
		private UInt32 m_uid;		
		private string m_fontSystem;
		private string m_fontMonoSpace;
		private string m_monoVersion = "2-generic";

		private UInt32 m_userId = 0;
		private string m_userName = "";

		private string m_sudoPath = "";

		// Override
		public Platform()
		{
			try
			{
				m_monoVersion = NativeMethods.GetMonoVersion();
			}
			catch
			{
				m_monoVersion = "2-generic";
			}
			if (UtilsCore.CompareVersions(m_monoVersion, "5.10.1.45") < 0)
			{
				// Workaround for https://github.com/mono/mono/issues/6752
				Environment.SetEnvironmentVariable("TERM", "XTERM", EnvironmentVariableTarget.Process);
			}
		}

		public override string GetCode()
		{
			return "Linux";
		}

		public override string GetName()
		{
			if (Platform.Instance.FileExists("/etc/issue"))
				return FileContentsReadText("/etc/issue").Replace("\n", "").Replace("\r", " - ").Trim();
			else
				return base.GetName();
		}

		public override string GetVersion()
		{
			return m_version;
		}

		public override string GetNetFrameworkVersion()
		{			
			return m_monoVersion + "; Framework: " + System.Reflection.Assembly.GetExecutingAssembly().ImageRuntimeVersion;
		}

		public override void OnInit(bool cli)
		{
			base.OnInit(cli);

			m_version = SystemShell.Shell1(LocateExecutable("uname"), "-a");
			m_architecture = NormalizeArchitecture(SystemShell.Shell1(LocateExecutable("uname"), "-m").Trim());
			m_uid = 9999;
			UInt32.TryParse(SystemShell.Shell1(LocateExecutable("id"), "-u"), out m_uid);
			
			m_sudoPath = LocateExecutable("sudo");

			// Obtain user ID and Name. Almost the same methods used by synaptic package manager GUI.
			{
				if (Environment.GetEnvironmentVariable("PKEXEC_UID") != null)
				{
					m_userId = Conversions.ToUInt32(Environment.GetEnvironmentVariable("PKEXEC_UID"));
				}
				else if (Environment.GetEnvironmentVariable("SUDO_UID") != null)
				{
					m_userId = Conversions.ToUInt32(Environment.GetEnvironmentVariable("SUDO_UID"));
				}
				else if (Environment.GetEnvironmentVariable("SUDO_USER") != null)
				{
					m_userName = Environment.GetEnvironmentVariable("SUDO_USER");
				}
				else if (Environment.GetEnvironmentVariable("USER") != null)
				{
					m_userName = Environment.GetEnvironmentVariable("USER");
				}

				if( (m_userName == "") && (m_userId != 0) )
				{
					m_userName = SystemShell.Shell2(LocateExecutable("getent"), "passwd", m_userId.ToString());
					if (m_userName.IndexOf(":") != -1)
						m_userName = m_userName.Substring(0, m_userName.IndexOf(":"));
				}

				m_userName = UtilsString.StringPruneCharsNotIn(m_userName, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789._-");

				if (m_userName == "root")
					m_userName = "";

				if ((m_userName != "") && (m_userId == 0))
					m_userId = Conversions.ToUInt32("id -u " + m_userName);
			}

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

			NativeMethods.Signal((int)NativeMethods.Signum.SIGINT, SignalCallback);
			NativeMethods.Signal((int)NativeMethods.Signum.SIGTERM, SignalCallback);
			NativeMethods.Signal((int)NativeMethods.Signum.SIGUSR1, SignalCallback);
			NativeMethods.Signal((int)NativeMethods.Signum.SIGUSR2, SignalCallback);
		}

		private static void SignalCallback(int signum)
		{
			NativeMethods.Signum sig = (NativeMethods.Signum)signum;
			if (sig == NativeMethods.Signum.SIGINT)
				Engine.Instance.OnSignal("SIGINT");
			else if (sig == NativeMethods.Signum.SIGTERM)
				Engine.Instance.OnSignal("SIGTERM");
			else if (sig == NativeMethods.Signum.SIGUSR1)
				Engine.Instance.OnSignal("SIGUSR1");
			else if (sig == NativeMethods.Signum.SIGUSR2)
				Engine.Instance.OnSignal("SIGUSR2");
		}

		public override string GetOsArchitecture()
		{
			return m_architecture;
		}

		public override bool IsAdmin()
		{
			// return true; // Decomment for debugging			
			return (m_uid == 0);
		}

		public override bool IsUnixSystem()
		{
			return true;
		}

		public override void OpenUrl(string url)
		{
			if (CanShellAsNormalUser() == false)
				return;

			SystemShell s = new SystemShell();
			s.Path = LocateExecutable("xdg-open");
			s.Arguments.Add(url);
			s.RunAsNormalUser = true;
			s.WaitEnd = false;
			s.Run();
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

		public override bool NativeInit()
		{
			return (NativeMethods.Init() == 0);
		}

		public override bool FileImmutableGet(string path)
		{
			if ((path == "") || (FileExists(path) == false))
				return false;

			int result = NativeMethods.GetFileImmutable(path);
			return (result == 1);
			/* // TOCLEAN
			// We don't find a better direct method in Mono/Posix without adding ioctl references
			// The list of flags can be different between Linux distro (for example 16 on Debian, 19 on Manjaro)
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

			NativeMethods.SetFileImmutable(path, value ? 1 : 0);

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
			int result = NativeMethods.GetFileMode(path);
			if (result == -1)
			{
				Engine.Instance.Logs.Log(LogType.Warning, "Failed to detect permissions on '" + path + "'.");
				return false;
			}
			int newResult = 0;
			if (mode == "600")
				newResult = (int)NativeMethods.FileMode.Mode0600;
			else if (mode == "644")
				newResult = (int)NativeMethods.FileMode.Mode0644;

			if (newResult == 0)
			{
				Engine.Instance.Logs.Log(LogType.Warning, "Unexpected permission '" + mode + "'");
				return false;
			}

			if (newResult != result)
			{
				result = NativeMethods.SetFileMode(path, newResult);
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
			if ((path == "") || (FileExists(path) == false))
				return false;

			int result = NativeMethods.GetFileMode(path);
			if (result == -1)
			{
				Engine.Instance.Logs.Log(LogType.Warning, "Failed to detect if '" + path + "' is executable");
				return false;
			}

			int newResult = result | 73; // +x :<> (S_IXUSR | S_IXGRP | S_IXOTH) 

			if (newResult != result)
			{
				result = NativeMethods.SetFileMode(path, newResult);
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

		public override bool CanShellAsNormalUser()
		{
			if( (m_userName != "") && (m_sudoPath != "") )
				return true;

			return false;
		}

		public override bool ShellAdaptNormalUser(ref string path, ref string[] arguments)
		{
			if (IsAdmin() == false) // Still not Admin
				return false;

			if (CanShellAsNormalUser() == false)
				return false;

			//arguments = new string[] { "-u " + m_userName, path + " " + String.Join(" ", arguments) };

			// DBUS_SESSION_BUS_ADDRESS is required only for notify-send. 
			// Debian7 don't work with it. Arch works anyway without it, Debian>7 and Fedora not.
			string dbusSessionBusAddress = Engine.Instance.Storage.Get("linux.dbus");
			if ( (dbusSessionBusAddress == "") && (Platform.Instance.FileExists("/run/user/" + m_userId.ToString() + "/bus")) )
				dbusSessionBusAddress = "unix:path=/run/user/" + m_userId.ToString() + "/bus";
			
			if(dbusSessionBusAddress != "")
				arguments = new string[] { "-u " + m_userName, "DBUS_SESSION_BUS_ADDRESS=" + dbusSessionBusAddress, path + " " + String.Join(" ", arguments) };
			else
				arguments = new string[] { "-u " + m_userName, path + " " + String.Join(" ", arguments) };

			path = m_sudoPath;

			// 2.14.3
			/*
			string a = " - " + m_userName + " -c '" + path + " " + String.Join(" ", arguments) + "'";
			path = "su";
			arguments = new string[] { a };
			*/
			
			return true;
		}

		public override void ShellASync(string path, string[] arguments)
		{
			try
			{
				using (System.Diagnostics.Process p = new System.Diagnostics.Process())
				{
					p.StartInfo.FileName = path;
					p.StartInfo.Arguments = String.Join(" ", arguments);
					p.StartInfo.WorkingDirectory = "";
					p.StartInfo.CreateNoWindow = true;
					p.StartInfo.UseShellExecute = false;
					p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
					p.Start();
				}
			}
			catch (Exception ex)
			{
				Engine.Instance.Logs.Log(ex);
			}
		}

		public override bool ProcessKillSoft(Process process)
		{
			return (NativeMethods.Kill(process.Id, (int)NativeMethods.Signum.SIGTERM) == 0);
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
			if (servicePath != "")
			{
				Dictionary<string, string> services = new Dictionary<string, string>();
				string list = SystemShell.Shell1(servicePath, "--status-all");
				list = UtilsString.StringCleanSpace(list);
				foreach (string line in list.Split('\n'))
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

				if (services.ContainsKey("nscd"))
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
			if (nscdPath != "")
			{
				SystemShell.Shell1(nscdPath, "--invalidate=hosts");
			}
		}

		protected override void OpenDirectoryInFileManagerEx(string path)
		{
			if (CanShellAsNormalUser() == false)
				return;

			SystemShell s = new SystemShell();
			s.Path = LocateExecutable("xdg-open");
			s.Arguments.Add(SystemShell.EscapePath(path));
			s.RunAsNormalUser = true;
			s.WaitEnd = false;
			s.Run();
		}

		public override long Ping(IpAddress host, int timeoutSec)
		{
			if ((host == null) || (host.Valid == false))
				return -1;

			return NativeMethods.PingIP(host.ToString(), timeoutSec * 1000);
			/* < 2.13.6 // TOCLEAN
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

				return (long)iMS;
			}
			*/
		}

		public override bool RouteAdd(Json jRoute)
		{
			IpAddress ip = jRoute["address"].Value as string;
			if (ip.Valid == false)
				return false;
			IpAddress gateway = jRoute["gateway"].Value as string;
			if (gateway.Valid == false)
				return false;

			List<string> arguments = new List<string>();
			if (ip.IsV6)
				arguments.Add("-6");
			arguments.Add("route");
			arguments.Add("add");
			arguments.Add(ip.ToCIDR());
			arguments.Add("via");
			arguments.Add(gateway.ToCIDR());
			if (jRoute.HasKey("interface"))
			{
				arguments.Add("dev");
				arguments.Add(jRoute["interface"].Value as string);
			}
			if (jRoute.HasKey("metric"))
			{
				arguments.Add("metric");
				arguments.Add(SystemShell.EscapeInt(jRoute["metric"].Value as string));
			}

			string result = SystemShell.Shell(LocateExecutable("ip"), arguments.ToArray());
			result = result.Trim();
			if (result.ToLowerInvariant() == "")
			{
				return base.RouteAdd(jRoute);
			}
			else
			{
				Engine.Instance.Logs.LogWarning(MessagesFormatter.Format(Messages.RouteAddFailed, ip.ToCIDR(), gateway.ToCIDR(), result));
				return false;
			}
		}

		public override bool RouteRemove(Json jRoute)
		{
			IpAddress ip = jRoute["address"].Value as string;
			if (ip.Valid == false)
				return false;
			IpAddress gateway = jRoute["gateway"].Value as string;
			if (gateway.Valid == false)
				return false;

			List<string> arguments = new List<string>();
			if (ip.IsV6)
				arguments.Add("-6");
			arguments.Add("route");
			arguments.Add("delete");
			arguments.Add(ip.ToCIDR());
			arguments.Add("via");
			arguments.Add(gateway.ToCIDR());
			if (jRoute.HasKey("interface"))
			{
				arguments.Add("dev");
				arguments.Add(jRoute["interface"].Value as string);
			}
			string result = SystemShell.Shell(LocateExecutable("ip"), arguments.ToArray());
			result = result.Trim();
			if (result.ToLowerInvariant() == "")
			{
				return base.RouteRemove(jRoute);
			}
			else
			{
				// Remember: Route deletion can occur in a second moment (for example a Recovery phase).

				// Still accepted: The device are not available anymore, so the route are already deleted.
				if (result.ToLowerInvariant().Contains("cannot find device"))
					return base.RouteRemove(jRoute);

				// Still accepted: Already deleted.
				if (result.ToLowerInvariant().Contains("no such process"))
					return base.RouteRemove(jRoute);

				// Unexpected/unknown error
				Engine.Instance.Logs.LogWarning(MessagesFormatter.Format(Messages.RouteDelFailed, ip.ToCIDR(), gateway.ToCIDR(), result));
				return false;
			}
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
					o = UtilsString.StringCleanSpace(o);
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

		public override void OnReport(Report report)
		{
			base.OnReport(report);

			report.Add("UID", Conversions.ToString(m_uid));
			report.Add("Run as normal user", CanShellAsNormalUser() + "; ID:" + m_userId.ToString() + "; Name:" + m_userName.ToString());
			report.Add("ip addr show", (LocateExecutable("ip") != "") ? SystemShell.Shell2(LocateExecutable("ip"), "addr", "show") : "'ip' " + Messages.NotFound);
			report.Add("ip link show", (LocateExecutable("ip") != "") ? SystemShell.Shell2(LocateExecutable("ip"), "link", "show") : "'ip' " + Messages.NotFound);
			report.Add("ip -4 route show", (LocateExecutable("ip") != "") ? SystemShell.Shell3(LocateExecutable("ip"), "-4", "route", "show") : "'ip' " + Messages.NotFound);
			report.Add("ip -6 route show", (LocateExecutable("ip") != "") ? SystemShell.Shell3(LocateExecutable("ip"), "-6", "route", "show") : "'ip' " + Messages.NotFound);
		}

		public override bool RestartAsRoot()
		{
			string method = "";
			string command = "";
			string arguments = "";
			string preCommand = "";
			string preArguments = "";
			string postCommand = "";
			string postArguments = "";

			bool isWayland = false;

			string waylandDisplay = Environment.GetEnvironmentVariable("WAYLAND_DISPLAY");
			if (waylandDisplay == null)
				waylandDisplay = "";
			waylandDisplay = waylandDisplay.ToLowerInvariant().Trim();
			if (waylandDisplay != "")
				isWayland = true;

			string xdgSessionType = Environment.GetEnvironmentVariable("XDG_SESSION_TYPE");
			if (xdgSessionType == null)
				xdgSessionType = "";
			xdgSessionType = xdgSessionType.ToLowerInvariant().Trim();
			if (xdgSessionType.Contains("wayland"))
				isWayland = true;

			bool isX = !isWayland;

			string command2 = "";
			string executablePath = Platform.Instance.GetExecutablePath();
			
			string args = CommandLine.SystemEnvironment.GetFull();
			args += " console.mode=none"; // 2.13.6, otherwise CancelKeyPress (mono bug?) and stdout fill the non-root non-blocked terminal.
			{
				string dbusSessionBusAddress = Environment.GetEnvironmentVariable("DBUS_SESSION_BUS_ADDRESS");
				if (dbusSessionBusAddress != null)
					args += " linux.dbus=\"" + dbusSessionBusAddress + "\"";
			}

			if (executablePath.Substring(executablePath.Length - 4).ToLowerInvariant() == ".exe")
				command2 += "mono ";
			command2 += Platform.Instance.GetExecutablePath();
			command2 += " ";
			command2 += args;
			command2 = command2.Trim(); // 2.11.11
			bool waitEnd = false;
			
			if ( (LocateExecutable("pkexec") != "") && (Platform.Instance.FileExists("/usr/share/polkit-1/actions/org.airvpn.eddie.ui.policy")) )
			{
				method = "pkexec-policy";

				/*
				command = "sh";
				arguments = " -c 'pkexec /usr/bin/eddie-ui " + cmdline + " console.mode=none" + " " + Engine.Instance.Storage.Get("pktest") + "'";
				*/
				
				command = LocateExecutable("pkexec");
				string exe = Engine.Instance.Storage.Get("path.exec");
				if (exe == "")
					exe = Platform.Instance.GetExecutablePath();
				arguments = exe + " " + args;
				
				// Without this, don't work on Debian9
				// It is not allowed to run pkexec in the background by fork and exec and then terminating the parent. 
				// The process becomes an orphan and belongs to init (ppid == 1).
				waitEnd = true;
								
				if(Engine.Instance.Storage.GetBool("linux.xhost"))
				{
					string xhost = LocateExecutable("xhost");					
					if (xhost != "")
					{
						preCommand = xhost;
						preArguments = "+SI:localuser:root";

						postCommand = xhost;
						postArguments = "-SI:localuser:root";
					}					
				}
			}
			else if ((isX) && (LocateExecutable("kdesudo") != ""))
			{
				method = "kdesudo";
				command = LocateExecutable("kdesudo");
				arguments = "";
				arguments += " -u root"; // Administrative privileges
				arguments += " -d"; // Don't show commandline
				arguments += " --comment \"" + Messages.AdminRequiredPasswordPrompt + "\"";
				arguments += " -c "; // The command
									 //arguments += " \"" + command2 + "\"";
				arguments += " \"" + command2 + "\"";
			}
			else if ((isX) && (LocateExecutable("kdesu") != ""))
			{
				method = "kdesu";
				command = LocateExecutable("kdesu");
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
			else if ((isX) && (LocateExecutable("gksu") != ""))
			{
				method = "gksu";
				command = LocateExecutable("gksu");
				arguments = "";
				arguments += " -u root"; // Administrative privileges
				arguments += " -m \"" + Messages.AdminRequiredPasswordPrompt + "\"";
				arguments += " \"" + command2 + "\"";
			}
			else if ((isX) && (LocateExecutable("xdg-su") != "")) // OpenSUSE
			{
				method = "xdg-su";
				command = LocateExecutable("xdg-su");
				arguments = "";
				arguments += " -u root"; // Administrative privileges
				arguments += " -c "; // The command
				arguments += " \"" + command2 + "\"";
			}
			else if ((isX) && (LocateExecutable("beesu") != "")) // Fedora
			{
				method = "beesu";
				command = LocateExecutable("beesu");
				arguments = "";
				arguments += " " + command2 + "";
			}
			else if ((LocateExecutable("xhost") != "") && (LocateExecutable("pkexec") != "")) // 2.14.0
			{
				method = "pkexec";
				command = "sh";
				arguments = "-c 'xhost si:localuser:root && pkexec env DISPLAY=$DISPLAY XAUTHORITY=$XAUTHORITY " + command2 + "; xhost -si:localuser:root'";
				//arguments = "+local: && pkexec env DISPLAY=$DISPLAY XAUTHORITY=$XAUTHORITY " + command2;				
			}

			if (command != "")
			{
				Engine.Instance.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.AdminRequiredRestart, method));
				
				if (preCommand != "")
					Engine.Instance.Logs.Log(LogType.Verbose, SystemShell.ShellX(preCommand.Trim(), preArguments.Trim(), true)); // IJTF2
				
				SystemShell.ShellX(command.Trim(), arguments.Trim(), waitEnd); // IJTF2

				if (postCommand != "")
					Engine.Instance.Logs.Log(LogType.Verbose, SystemShell.ShellX(postCommand.Trim(), postArguments.Trim(), true)); // IJTF2				
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
					Engine.Instance.Logs.Log(LogType.Verbose, Messages.OsLinuxDnsResolvConfScript);
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

			string pingPath = LocateExecutable("ping");
			if (pingPath == "")
				Engine.Instance.Logs.Log(LogType.Error, "'ping' " + Messages.NotFound);
			*/

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

		/*
		public override bool OnCheckEnvironmentSession()
		{
			// TOCLEAN
			if (Common.Constants.FeatureIPv6ControlOptions)
			{

			}
			else
			{
				if (Engine.Instance.Storage.GetLower("ipv6.mode") == "disable")
				{
					string keyname = "net.ipv6.conf.all.disable_ipv6";
					string ipV6 = "";
					string sysctlPath = LocateExecutable("sysctl");
					if (sysctlPath != "")
						ipV6 = SystemShell.Shell1(sysctlPath, keyname).Replace(keyname, "").Trim().Trim(new char[] { '=', ' ', '\n', '\r' }); // 2.10.1

					if (ipV6 == "0")
					{
						if (Engine.Instance.OnAskYesNo(Messages.OsLinuxIPv6Warning))
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
						Engine.Instance.Logs.Log(LogType.Verbose, Messages.OsLinuxIPv6WarningUnableToDetect);
					}
				}
			}			

			return true;
		}
		*/

		public override bool OnCheckSingleInstance()
		{
			try
			{
				int currentId = Process.GetCurrentProcess().Id;
				string path = UtilsCore.GetTempPath() + "/" + Constants.Name + "_" + Constants.AppID + ".pid";
				if (File.Exists(path) == false)
				{
				}
				else
				{
					int otherId;
					if (int.TryParse(Platform.Instance.FileContentsReadText(path), out otherId))
					{
						string procFile = "/proc/" + otherId.ToString() + "/cmdline";
						if (File.Exists(procFile))
						{
							return false;
						}
					}
				}

				Platform.Instance.FileContentsWriteText(path, currentId.ToString());

				return true;
			}
			catch (Exception e)
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
				string path = UtilsCore.GetTempPath() + "/" + Constants.Name + "_" + Constants.AppID + ".pid";
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

		public override bool OnDnsSwitchDo(ConnectionActive connectionActive, IpAddresses dns)
		{
			if (GetDnsSwitchMode() == "rename")
			{
				if (FileExists("/etc/resolv.conf.eddie") == false)
				{
					if (FileExists("/etc/resolv.conf"))
					{
						Engine.Instance.Logs.Log(LogType.Verbose, Messages.OsLinuxDnsRenameBackup);
						FileMove("/etc/resolv.conf", "/etc/resolv.conf.eddie");
					}
				}

				Engine.Instance.Logs.Log(LogType.Verbose, Messages.OsLinuxDnsRenameDone);

				string text = "# " + Engine.Instance.GenerateFileHeader() + "\n\n";

				foreach (IpAddress dnsSingle in dns.IPs)
					text += "nameserver " + dnsSingle.Address + "\n";

				FileContentsWriteText("/etc/resolv.conf", text);
				Platform.Instance.FileEnsurePermission("/etc/resolv.conf", "644");
			}

			base.OnDnsSwitchDo(connectionActive, dns);

			return true;
		}

		public override bool OnDnsSwitchRestore()
		{
			// Cleaning rename method if pending
			if (FileExists("/etc/resolv.conf.eddie") == true)
			{
				if (FileExists("/etc/resolv.conf"))
					FileDelete("/etc/resolv.conf");

				Engine.Instance.Logs.Log(LogType.Verbose, Messages.OsLinuxDnsRenameRestored);

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

		public override void OnJsonNetworkInfo(Json jNetworkInfo)
		{
			base.OnJsonNetworkInfo(jNetworkInfo);
			if (Conversions.ToBool(jNetworkInfo["support_ipv6"].Value) == true)
			{
				if (Conversions.ToInt32(GetSysCtl("/net/ipv6/conf/all/disable_ipv6")) != 0)
					jNetworkInfo["support_ipv6"].Value = false;
				if (Conversions.ToInt32(GetSysCtl("/net/ipv6/conf/default/disable_ipv6")) != 0)
					jNetworkInfo["support_ipv6"].Value = false;
			}

			foreach (Json jNetworkInterface in jNetworkInfo["interfaces"].Json.GetArray())
			{
				string id = jNetworkInterface["id"].Value as string;

				// Base virtual always 'false'. Missing Mono implementation?
				jNetworkInterface["support_ipv6"].Value = true;
				{
					if (Conversions.ToInt32(GetSysCtl("/net/ipv6/conf/" + SystemShell.EscapeAlphaNumeric(id) + "/disable_ipv6")) != 0)
						jNetworkInterface["support_ipv6"].Value = false;

					if (Conversions.ToBool(jNetworkInfo["support_ipv6"].Value) == false)
						jNetworkInterface["support_ipv6"].Value = false;
				}
			}
		}

		public override void OnJsonRouteList(Json jRoutesList)
		{
			base.OnJsonRouteList(jRoutesList);

			string o = "";
			o += SystemShell.Shell3(LocateExecutable("ip"), "-4", "route", "show");
			o += SystemShell.Shell3(LocateExecutable("ip"), "-6", "route", "show");

			foreach (string line in o.Trim().Split('\n'))
			{
				string address = "";
				string gateway = "";
				string iface = "";
				string metric = "";
				string[] fields = UtilsString.StringCleanSpace(line.Trim()).Split(' ');
				for (int i = 0; i < fields.Length; i++)
				{
					if (i == 0)
					{
						address = fields[0];
					}
					else if ((fields[i] == "via") && (fields.Length > i))
						gateway = fields[i + 1];
					else if ((fields[i] == "dev") && (fields.Length > i))
						iface = fields[i + 1];
					else if ((fields[i] == "metric") && (fields.Length > i))
						metric = fields[i + 1];
				}
				IpAddress ipGateway = new IpAddress(gateway);
				if (ipGateway.Valid == false)
					continue;
				IpAddress ipAddress = IpAddress.DefaultIPv4;
				if (address == "default")
				{
					if (ipGateway.IsV4)
						ipAddress = IpAddress.DefaultIPv4;
					else if (ipGateway.IsV6)
						ipAddress = IpAddress.DefaultIPv6;
				}
				else
					ipAddress = new IpAddress(address);
				if (ipAddress.Valid == false)
					continue;

				Json jRoute = new Json();
				jRoute["address"].Value = ipAddress.ToCIDR();
				jRoute["gateway"].Value = ipGateway.ToCIDR();
				if (iface != "")
					jRoute["interface"].Value = iface;
				if (metric != "")
					jRoute["metric"].Value = metric;
				jRoutesList.Append(jRoute);
			}
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
			if ((current == "resolvconv") && (Software.FindResource("update-resolv-conf") == ""))
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

		public string GetSysCtl(string name)
		{
			// Don't use sysctl, is deprecated http://man7.org/linux/man-pages/man2/sysctl.2.html

			string path = "/proc/sys/" + name.Trim('/');
			if (FileExists(path))
				return FileContentsReadText(path);
			else
				return "";
		}		
	}
}
