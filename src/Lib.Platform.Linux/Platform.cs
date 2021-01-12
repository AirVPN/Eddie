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
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Xml;
using Eddie.Core;
// using Mono.Unix.Native; // Removed in 2.11
// using Mono.Unix; // Removed in 2.14.4

namespace Eddie.Platform.Linux
{
	public class Platform : Core.Platform
	{
		private string m_version = "";
		private string m_architecture = "";

		private string m_monoVersion = "2-generic";

		private UInt32 m_uid = 9999;

		private List<IpV6ModeEntry> m_listIpV6Mode = new List<IpV6ModeEntry>();

		private string m_systemdUnitsPath = "/usr/lib/systemd/system";
		private string m_systemdUnitPath = "/usr/lib/systemd/system/eddie-elevated.service";

        private NetworkLockIptables m_netlockPluginIptables;
        private NetworkLockNftables m_netlockPluginNftables;

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
			if (m_monoVersion.VersionUnder("5.10.1.45"))
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

		public override bool OnInit()
		{
			base.OnInit(); 

            m_version = SystemShell.Shell1(LocateExecutable("uname"), "-a");
            m_architecture = NormalizeArchitecture(SystemShell.Shell1(LocateExecutable("uname"), "-m").Trim());

            try
            {
                bool result = (NativeMethods.Init() == 0);
                if (result == false)
                    throw new Exception("fail");

                NativeMethods.Signal((int)NativeMethods.Signum.SIGHUP, SignalCallback);
                NativeMethods.Signal((int)NativeMethods.Signum.SIGINT, SignalCallback);
                NativeMethods.Signal((int)NativeMethods.Signum.SIGTERM, SignalCallback);
                NativeMethods.Signal((int)NativeMethods.Signum.SIGUSR1, SignalCallback);
                NativeMethods.Signal((int)NativeMethods.Signum.SIGUSR2, SignalCallback);             
            }
            catch
            {
                Console.WriteLine("Unable to initialize native library. Maybe a CPU architecture issue.");
                return false;
            }

            return true;
        }

		private static void SignalCallback(int signum)
		{
			NativeMethods.Signum sig = (NativeMethods.Signum)signum;
			if (sig == NativeMethods.Signum.SIGHUP)
				Engine.Instance.OnSignal("SIGHUP");
			else if (sig == NativeMethods.Signum.SIGINT)
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

		public override string GetDefaultDataPath()
		{
			// Maybe the default can be "home" like macOS. 
			// Currently it's String.Empty so Eddie try first to use App directory (for portable editions)
			return String.Empty;
			//return "home";
		}

		public override Eddie.Core.Elevated.EleBase StartElevated()
		{
			ElevatedImpl e = new ElevatedImpl();
			e.Start();
			return e;
		}

		public override bool IsAdmin()
		{
			// return true; // Decomment for debugging

			if (m_uid == 9999)
			{
				m_uid = NativeMethods.getuid();
			}
			return (m_uid == 0);
		}

		public override bool IsUnixSystem()
		{
			return true;
		}

		protected override string GetElevatedHelperPathImpl()
		{
			return FileGetPhysicalPath(GetApplicationPath() + "/eddie-cli-elevated");
		}

		public override bool CheckElevatedSocketAllowed(System.Net.IPEndPoint localEndpoint, System.Net.IPEndPoint remoteEndpoint)
		{
			// Security: Don't yet find a method, because in some platform (ex. Arch) Eddie are compiled from sources.
			return true;
		}

		public override bool CheckElevatedProcessAllowed(string remotePath)
		{
			// Security: Don't yet find a method, because in some platform (ex. Arch) Eddie are compiled from sources.
			return true;
		}

		public override bool AllowService()
		{
			if (DirectoryExists(m_systemdUnitsPath))
				return true;

			return false;
		}

		public override string AllowServiceUserDescription()
		{
			if (DirectoryExists(m_systemdUnitsPath))
				return "If checked, install a systemd service";

			return "";
		}

		protected override bool GetServiceImpl()
		{
			return FileExists(m_systemdUnitPath);
		}

		protected override bool SetServiceImpl(bool value)
		{
			if (GetServiceImpl() == value)
				return true;

			if (value)
			{
				RunProcessAsRoot(GetElevatedHelperPath(), new string[] { "service=install", "service_port=" + Engine.Instance.GetElevatedServicePort() }, Engine.Instance.ConsoleMode);
				return (GetService() == true);
			}
			else
			{
				RunProcessAsRoot(GetElevatedHelperPath(), new string[] { "service=uninstall" }, Engine.Instance.ConsoleMode);
				return (GetService() == false);
			}
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

			Engine.Instance.Elevated.DoCommandSync("file-immutable-set", "path", path, "flag", (value ? "1" : "0"));
		}

		public override bool FileEnsurePermission(string path, string mode)
		{
			if ((path == "") || (FileExists(path) == false))
				return false;

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
		}

		public override bool FileRunAsRoot(string path)
		{
			return NativeMethods.GetFileRunAsRoot(path);
		}

		public override string GetExecutableReport(string path)
		{
			string lddPath = LocateExecutable("ldd");
			if (lddPath != "")
				return SystemShell.Shell1(lddPath, SystemShell.EscapePath(path));
			else
				return "'ldd' " + LanguageManager.GetText("NotFound");
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

			if ((output != "") && (new FileInfo(output).Name.ToLowerInvariant().StartsWith("mono", StringComparison.InvariantCulture)))
			{
				// Exception: Assembly directly load by Mono
				output = base.GetExecutablePathEx();
			}

			return output;
		}

		protected override string GetUserPathEx()
		{
			// return Environment.GetEnvironmentVariable("HOME") + DirSep + ".eddie";

			string basePath = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
			if (basePath == null)
				basePath = "";
			if (basePath == "")
				basePath = Environment.GetEnvironmentVariable("HOME") + DirSep + ".config";

			return basePath + DirSep + "eddie";
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

		public override int StartProcessAsRoot(string path, string[] arguments, bool consoleMode)
		{
			System.Diagnostics.Process process = new System.Diagnostics.Process();

            bool canRunAsRoot = FileRunAsRoot(path);

            process = new System.Diagnostics.Process();
			if (canRunAsRoot)
			{
				process.StartInfo.FileName = path;
				process.StartInfo.Arguments = String.Join(" ", arguments);
			}
			else
			{				
				if (consoleMode)
				{					
					if (IsAdmin())
					{
						process.StartInfo.FileName = path;
						process.StartInfo.Arguments = String.Join(" ", arguments);
					}
					else
					{
                        string sudoPath = LocateExecutable("sudo");
						if (sudoPath != "")
                        {
                            List<string> groups = new List<string>(SystemShell.Shell0(LocateExecutable("groups")).ToLowerInv().Split(' '));
                            if(groups.Contains("sudo"))
                            {
								process.StartInfo.FileName = "sudo";
                                process.StartInfo.Arguments = "\"" + path + "\" " + String.Join(" ", arguments);
                            }
                            else
                            {
								// Ask for password, but for unknown reason, password mismatch (endofline at first character), stdin issue, probably related to Mono.
								//process.StartInfo.FileName = "su";
								//process.StartInfo.Arguments = "-c \"'" + path + "' " + String.Join(" ", arguments) + "\"";

								// This works in Manjaro
								process.StartInfo.FileName = "sudo";
								process.StartInfo.Arguments = "\"" + path + "\" " + String.Join(" ", arguments);
							}
                        }
					}
				}
				else
				{
					process.StartInfo.FileName = "pkexec";
					process.StartInfo.Arguments = "\"" + path + "\" " + String.Join(" ", arguments);
				}
			}

            if(process.StartInfo.FileName == "")
            {
                Engine.Instance.Logs.LogFatal("Unable to find a method to run elevated process. Install 'sudo' and ensure user is in sudo group, or run chown root:root eddie-cli-elevated;chmod u+s eddie-cli-elevated");
                return 0;
            }
            else
            {
                process.StartInfo.WorkingDirectory = "";

                process.StartInfo.Verb = "run";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                process.StartInfo.UseShellExecute = false;

                process.Start();

                return process.Id;
            }
		}

		public override void ShellASyncCore(string path, string[] arguments)
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

        public override bool FetchUrlInternal()
        {
            return false;  // See comment in Lib.Platform.Native/build.sh
        }

        public override Json FetchUrl(Json request)
        {
            return NativeMethods.CUrl(request);
        }

        public override void FlushDNS()
		{
			base.FlushDNS();

			Engine.Instance.Elevated.DoCommandSync("dns-flush", "services", Engine.Instance.Options.Get("linux.dns.services"));
		}

		public override void OpenUrl(string url)
		{
			SystemShell s = new SystemShell();
			s.Path = LocateExecutable("xdg-open");
			s.Arguments.Add(url);
			s.WaitEnd = false;
			s.Run();
		}

		public override void OpenFolder(string path)
		{
			SystemShell s = new SystemShell();
			s.Path = LocateExecutable("xdg-open");
			s.Arguments.Add(SystemShell.EscapePath(path));
			s.WaitEnd = false;
			s.Run();
		}

		// This works, but we use base to avoid shell
		/*
		public override long Ping(IpAddress host, int timeoutSec)
		{
			if ((host == null) || (host.Valid == false))
				return -1;

			// <2.17.3, require root
			//return NativeMethods.PingIP(host.ToString(), timeoutSec * 1000);

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
					s.Arguments.Add(host.Address);
					s.NoDebugLog = true;

					if (s.Run())
					{
						string result = s.Output;
						string sMS = UtilsString.ExtractBetween(result.ToLowerInvariant(), "min/avg/max/mdev = ", "/");
						if (float.TryParse(sMS, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out iMS) == false)
							iMS = -1;
					}
				}

				return (long)iMS;
			}
		}
		*/

		public override bool RouteAdd(Json jRoute)
		{
			IpAddress ip = jRoute["address"].Value as string;
			if (ip.Valid == false)
				return false;
			IpAddress gateway = jRoute["gateway"].Value as string;
			if (gateway.Valid == false)
				return false;

			try
			{

                Core.Elevated.Command c = new Core.Elevated.Command();
				c.Parameters["command"] = "route";
				if (ip.IsV4)
					c.Parameters["layer"] = "ipv4";
				else if (ip.IsV6)
					c.Parameters["layer"] = "ipv6";
				c.Parameters["action"] = "add";
				c.Parameters["cidr"] = ip.ToCIDR();
				c.Parameters["gateway"] = gateway.ToCIDR();
				if (jRoute.HasKey("interface"))
					c.Parameters["interface"] = jRoute["interface"].Value as string;
				else
					c.Parameters["interface"] = "";
				if (jRoute.HasKey("metric"))
					c.Parameters["metric"] = jRoute["metric"].Value as string;
				else
					c.Parameters["metric"] = "";
				Engine.Instance.Elevated.DoCommandSync(c);
				return base.RouteAdd(jRoute);
			}
			catch (Exception e)
			{
				Engine.Instance.Logs.LogWarning(LanguageManager.GetText("RouteAddFailed", ip.ToCIDR(), gateway.ToCIDR(), e.Message));

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

			try
			{
                Core.Elevated.Command c = new Core.Elevated.Command();
				c.Parameters["command"] = "route";
				if (ip.IsV4)
					c.Parameters["layer"] = "ipv4";
				else if (ip.IsV6)
					c.Parameters["layer"] = "ipv6";
				c.Parameters["action"] = "delete";
				c.Parameters["cidr"] = ip.ToCIDR();
				c.Parameters["gateway"] = gateway.ToCIDR();
				if (jRoute.HasKey("interface"))
					c.Parameters["interface"] = jRoute["interface"].Value as string;
				else
					c.Parameters["interface"] = "";
				c.Parameters["metric"] = "";
				Engine.Instance.Elevated.DoCommandSync(c);
				return base.RouteRemove(jRoute);
			}
			catch (Exception e)
			{
				Engine.Instance.Logs.LogWarning(LanguageManager.GetText("RouteAddFailed", ip.ToCIDR(), gateway.ToCIDR(), e.Message));
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
					o = o.CleanSpace();
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
					if (line.Trim().StartsWith("#", StringComparison.InvariantCulture))
						continue;
					if (line.Trim().StartsWith("nameserver", StringComparison.InvariantCulture))
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

			report.Add("ip addr show", (LocateExecutable("ip") != "") ? SystemShell.Shell2(LocateExecutable("ip"), "addr", "show") : "'ip' " + LanguageManager.GetText("NotFound"));
			report.Add("ip link show", (LocateExecutable("ip") != "") ? SystemShell.Shell2(LocateExecutable("ip"), "link", "show") : "'ip' " + LanguageManager.GetText("NotFound"));
			report.Add("ip -4 route show", (LocateExecutable("ip") != "") ? SystemShell.Shell3(LocateExecutable("ip"), "-4", "route", "show") : "'ip' " + LanguageManager.GetText("NotFound"));
			report.Add("ip -6 route show", (LocateExecutable("ip") != "") ? SystemShell.Shell3(LocateExecutable("ip"), "-6", "route", "show") : "'ip' " + LanguageManager.GetText("NotFound"));
		}

		public override Dictionary<string, string> GetProcessesList()
		{
			Dictionary<string, string> result = new Dictionary<string, string>();

			foreach (string fi in Directory.GetDirectories("/proc/"))
			{
				string pid = fi.Substring(6);
				string cmdline = "";
				if (FileExists(fi + "/cmdline"))
					cmdline = File.ReadAllText(fi + "/cmdline");
				result[pid] = cmdline;
			}

			return result;
		}

		public override void OnRecoveryAlways()
		{
			base.OnRecoveryAlways();

			OnDnsSwitchRestore();
		}

		public override void OnRecoveryLoad(XmlElement root)
		{
			XmlElement nodeIpV6 = root.GetFirstElementByTagName("IPv6");
			if (nodeIpV6 != null)
			{
				foreach (XmlElement nodeEntry in nodeIpV6.ChildNodes)
				{
					IpV6ModeEntry entry = new IpV6ModeEntry();
					entry.ReadXML(nodeEntry);
					m_listIpV6Mode.Add(entry);
				}
			}

			base.OnRecoveryLoad(root);
		}

		public override void OnRecoverySave(XmlElement root)
		{
			base.OnRecoverySave(root);

			XmlDocument doc = root.OwnerDocument;

			if (m_listIpV6Mode.Count != 0)
			{
				XmlElement nodeDns = (XmlElement)root.AppendChild(doc.CreateElement("IPv6"));
				foreach (IpV6ModeEntry entry in m_listIpV6Mode)
				{
					XmlElement nodeEntry = nodeDns.AppendChild(doc.CreateElement("entry")) as XmlElement;
					entry.WriteXML(nodeEntry);
				}
			}
		}

		public override void OnBuildOvpn(OvpnBuilder ovpn)
		{
			base.OnBuildOvpn(ovpn);

			ovpn.AppendDirective("route-delay", "5", ""); // 2.8, to resolve some issue on some distro, ex. Fedora 21
		}

		public override bool OnCheckEnvironmentApp()
		{
			string getentPath = LocateExecutable("getent");
			if (getentPath == "")
				Engine.Instance.Logs.Log(LogType.Error, "'getent' " + LanguageManager.GetText("NotFound"));

			string ipPath = LocateExecutable("ip");
			if (ipPath == "")
				Engine.Instance.Logs.Log(LogType.Error, "'ip' " + LanguageManager.GetText("NotFound"));

			return true;
		}

		public override bool OnCheckSingleInstance()
		{
			try
			{
				int currentId = Process.GetCurrentProcess().Id;
				string path = DirectoryTemp() + DirSep + Constants.Name + "_" + Constants.AppID + ".pid";
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

				Platform.Instance.FileContentsWriteText(path, currentId.ToString(), Encoding.ASCII);

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
				string path = DirectoryTemp() + DirSep + Constants.Name + "_" + Constants.AppID + ".pid";
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

		public override bool NeedExecuteOutsideAppPath(string exePath)
		{
			if (IsAppImageAndPathWithin(exePath))
				return true;
			return base.NeedExecuteOutsideAppPath(exePath);
		}

		public override void OnNetworkLockManagerInit()
		{
			base.OnNetworkLockManagerInit();

            m_netlockPluginIptables = new NetworkLockIptables();
            m_netlockPluginNftables = new NetworkLockNftables();

            Engine.Instance.NetworkLockManager.AddPlugin(m_netlockPluginNftables);
            Engine.Instance.NetworkLockManager.AddPlugin(m_netlockPluginIptables);
        }

		public override string OnNetworkLockRecommendedMode()
		{
            if (m_netlockPluginNftables.GetSupport())
                return m_netlockPluginNftables.GetCode();
            else
                return m_netlockPluginIptables.GetCode();
		}

		public override bool OnIPv6Block()
		{
			string result = Engine.Instance.Elevated.DoCommandSync("ipv6-block");
			if (result != "")
			{
				foreach (string resultItem in result.Split('\n'))
				{
					string interfaceName = resultItem;

					IpV6ModeEntry entry = new IpV6ModeEntry();
					entry.Interface = interfaceName;
					m_listIpV6Mode.Add(entry);

					Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("OsLinuxNetworkAdapterIPv6Disabled", interfaceName));
				}
			}

			Recovery.Save();

			base.OnIPv6Block();

			return true;
		}

		public override bool OnIPv6Restore()
		{
			foreach (IpV6ModeEntry entry in m_listIpV6Mode)
			{
                Core.Elevated.Command c = new Core.Elevated.Command();
				c.Parameters["command"] = "ipv6-restore";
				c.Parameters["interface"] = entry.Interface;
				Engine.Instance.Elevated.DoCommandSync(c);

				Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("OsLinuxNetworkAdapterIPv6Restored", entry.Interface));
			}

			m_listIpV6Mode.Clear();

			Recovery.Save();

			base.OnIPv6Restore();

			return true;
		}

		public override bool OnDnsSwitchDo(ConnectionActive connectionActive, IpAddresses dns)
		{
			if (GetDnsSwitchMode() == "rename")
			{
				string text = "# " + Engine.Instance.GenerateFileHeader() + "\n\n";

				foreach (IpAddress dnsSingle in dns.IPs)
					text += "nameserver " + dnsSingle.Address + "\n";

				Engine.Instance.Elevated.DoCommandSync("dns-switch-rename-do", "text", text);

				Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("OsLinuxDnsRenameDone"));
			}

			base.OnDnsSwitchDo(connectionActive, dns);

			return true;
		}

		public override bool OnDnsSwitchRestore()
		{
			// Cleaning rename method if pending
			if (FileExists("/etc/resolv.conf.eddie") == true)
				Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("OsLinuxDnsRenameRestored"));

			Engine.Instance.Elevated.DoCommandSync("dns-switch-rename-restore");

			base.OnDnsSwitchRestore();

			return true;
		}

		public override string GetDriverVersion(string driver)
		{
			if (Platform.Instance.FileExists("/dev/net/tun"))
				return "/dev/net/tun";
			else if (Platform.Instance.FileExists("/dev/tun"))
				return "/dev/tun";

			return "";
		}

		public override void OnJsonNetworkInfo(Json jNetworkInfo)
		{
			base.OnJsonNetworkInfo(jNetworkInfo);
			if (Conversions.ToBool(jNetworkInfo["support_ipv6"].Value) == true)
			{
				if (Conversions.ToInt32(GetSysCtl("/net/ipv6/conf/all/disable_ipv6")) != 0)
					jNetworkInfo["support_ipv6"].Value = false;
				else if (Conversions.ToInt32(GetSysCtl("/net/ipv6/conf/default/disable_ipv6")) != 0)
					jNetworkInfo["support_ipv6"].Value = false;
				else if (FileExists("/proc/net/if_inet6") == false) // Disabled via Grub for example
					jNetworkInfo["support_ipv6"].Value = false;
			}

			foreach (Json jNetworkInterface in jNetworkInfo["interfaces"].Json.GetArray())
			{
				string id = jNetworkInterface["id"].Value as string;

				// Base virtual always 'false'. Missing Mono implementation?
				jNetworkInterface["support_ipv6"].Value = true;
				{
					if (Conversions.ToBool(jNetworkInfo["support_ipv6"].Value) == false)
						jNetworkInterface["support_ipv6"].Value = false;
					else if (Conversions.ToInt32(GetSysCtl("/net/ipv6/conf/" + SystemShell.EscapeAlphaNumeric(id) + "/disable_ipv6")) != 0)
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
				string[] fields = line.Trim().CleanSpace().Split(' ');
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

		public override string OsCredentialSystemName()
		{
			string secretToolPath = LocateExecutable("secret-tool");
			if (secretToolPath != "")
				return "Linux secret-tool";
			else
				return "";
		}

		public override string OsCredentialSystemRead(string name)
		{
			string secretToolPath = LocateExecutable("secret-tool");
			SystemShell shell = new SystemShell();
			shell.Path = secretToolPath;
			shell.Arguments.Add("lookup");
			shell.Arguments.Add("'Eddie Profile'");
			shell.Arguments.Add("'" + SystemShell.EscapeInsideQuote(name) + "'");
			shell.Run();
			int exitCode = shell.ExitCode;
			if (exitCode == 0)
				return shell.Output;
			else
				return "";
		}

		public override bool OsCredentialSystemWrite(string name, string password)
		{
			string secretToolPath = LocateExecutable("secret-tool");
			SystemShell shell = new SystemShell();
			shell.Path = secretToolPath;
			shell.Arguments.Add("store");
			shell.Arguments.Add("--label='Eddie, saved password for " + SystemShell.EscapeInsideQuote(name) + " profile'");
			shell.Arguments.Add("'Eddie Profile'");
			shell.Arguments.Add("'" + SystemShell.EscapeInsideQuote(name) + "'");
			shell.AutoWriteStdin = password + "\n";
			shell.Run();
			int exitCode = shell.ExitCode;
			return (exitCode == 0);
		}

		public override bool OsCredentialSystemDelete(string name)
		{
			string secretToolPath = LocateExecutable("secret-tool");
			SystemShell shell = new SystemShell();
			shell.Path = secretToolPath;
			shell.Arguments.Add("clear");
			shell.Arguments.Add("'Eddie Profile'");
			shell.Arguments.Add("'" + SystemShell.EscapeInsideQuote(name) + "'");
			shell.Run();
			int exitCode = shell.ExitCode;
			return (exitCode == 0);
		}

		public override List<string> GetTrustedPaths()
		{
			List<string> list = base.GetTrustedPaths();

			list.Add("/bin");
			list.Add("/usr/bin");
			list.Add("/sbin");
			list.Add("/usr/sbin");
			list.Add("/usr/local/sbin");
			list.Add("/root/bin");

			return list;
		}

		public string GetDnsSwitchMode()
		{
			string current = Engine.Instance.Options.GetLower("dns.mode");

			if (current == "auto")
				current = "rename";

			if (current == "resolvconf")
				current = "rename";

			/*
			// Fallback
			if ((current == "resolvconv") && (Software.FindResource("update-resolv-conf") == ""))
				current = "rename";

			if ((current == "resolvconv") && (Platform.Instance.FileExists("/sbin/resolvconf") == false))
				current = "rename";
            */

			return current;
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

		public bool IsAppImageAndPathWithin(string path)
		{
			// This occur for example when elevated run openvpn exe in bundle: a root process (elevated) can't read an user volume (AppImage/fuse)
			if (Environment.GetEnvironmentVariable("APPIMAGE") == null)
				return false;

			return path.StartsWith(GetApplicationPath(), StringComparison.InvariantCulture);
		}
	}

	public class IpV6ModeEntry
	{
		public string Interface;

		public void ReadXML(XmlElement node)
		{
			Interface = node.GetAttributeString("interface", "");
		}

		public void WriteXML(XmlElement node)
		{
			node.SetAttributeString("interface", Interface);
		}
	}
}
