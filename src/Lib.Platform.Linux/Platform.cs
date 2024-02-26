// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2023 AirVPN (support@airvpn.org) / https://airvpn.org
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
using System.Globalization;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using System.Xml;
using Eddie.Core;

namespace Eddie.Platform.Linux
{
	public class Platform : Core.Platform
	{
		private string m_name = "";
		private string m_version = "";

#if !EDDIE_DOTNET
		private string m_architecture = "";
		private string m_monoVersion = "";
#endif

		private UInt32 m_uid = 9999;

		private string m_elevatedRunPath = "";

		private List<IpV6ModeEntry> m_listIpV6Mode = new List<IpV6ModeEntry>();

		private string m_systemdUnitsPath = "/usr/lib/systemd/system";
		private string m_systemdUnitPath = "/usr/lib/systemd/system/eddie-elevated.service";

		private IpAddresses m_dnsSwitch = new IpAddresses();

		private NetworkLockNftables m_netlockPluginNftables;
		private NetworkLockIptablesLegacy m_netlockPluginIptablesLegacy;
		private NetworkLockIptablesNFT m_netlockPluginIptablesNFT;
		private NetworkLockIptables m_netlockPluginIptables;

		// Override
		public Platform()
		{
#if !EDDIE_DOTNET
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

			m_architecture = Utils.NormalizeArchitecture(SystemExec.Exec1(LocateExecutable("uname"), "-m").Trim());
#endif
			if (Platform.Instance.FileExists("/etc/os-release"))
			{
				string osrelease = FileContentsReadText("/etc/os-release");

				m_name = osrelease.RegExMatchOne("NAME=\\\"(.+?)\\\"");
				m_version = osrelease.RegExMatchOne("VERSION=\\\"(.+?)\\\"");
			}

			if (m_version == "")
				m_version = SystemExec.Exec1(LocateExecutable("uname"), "-a");
		}

		public override string GetCode()
		{
			return "Linux";
		}

		public override string GetName()
		{
			return m_name;
		}

		public override string GetVersion()
		{
			return m_version;
		}

#if !EDDIE_DOTNET
		public override string GetOsArchitecture()
		{
			return m_architecture;
		}

		public override string GetNetFrameworkVersion()
		{
			return m_monoVersion + "; Framework: " + System.Reflection.Assembly.GetExecutingAssembly().ImageRuntimeVersion;
		}
#endif

		public override bool OnInit()
		{
			base.OnInit();

			try
			{
				bool result = (NativeMethods.Init() == 0);
				if (result == false)
					throw new Exception("fail");

			}
			catch
			{
				Engine.Instance.Logs.LogFatal("Unable to initialize native library. Maybe a CPU architecture issue.");
				return false;
			}

			return true;
		}

		/* // TOCLEAN
		public override string GetDefaultDataPath()
		{
			// Maybe the default can be "home" like macOS. 
			// Currently it's String.Empty so Eddie try first to use App directory (for portable editions)
			return String.Empty;
			//return "home";
		}
		*/

		public override string GetDefaultResourcesDirName()
		{
			return "res";
		}

		public override Eddie.Core.Elevated.IElevated StartElevated()
		{
			ElevatedImpl e = new ElevatedImpl();
			e.Start();

			RootExecutionOutsideBundleDelete(m_elevatedRunPath);

			return e;
		}

		public override bool IsElevatedPrivileges()
		{
			if (m_uid == 9999)
			{
				m_uid = NativeMethods.GetUID();
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
				RunElevated(new string[] { "service=install", "service_port=" + Engine.Instance.GetElevatedServicePort() }, true);
				return (GetService() == true);
			}
			else
			{
				RunElevated(new string[] { "service=uninstall" }, true);
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
				return SystemExec.Exec1(lddPath, SystemExec.EscapePath(path));
			else
				return "'ldd' " + LanguageManager.GetText(LanguageItems.NotFound);
		}

#if !EDDIE_DOTNET
		public override string GetExecutablePathEx()
		{
			int pid = Process.GetCurrentProcess().Id;
			string output = SystemExec.Exec1(LocateExecutable("readlink"), "/proc/" + pid.ToString() + "/exe");

			if ((output != "") && (new FileInfo(output).Name.ToLowerInvariant().StartsWith("mono", StringComparison.InvariantCulture)))
			{
				// Exception: Assembly directly load by Mono
				output = base.GetExecutablePathEx();
			}

			return output;
		}
#endif

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
			if (m_locateExecutableCache.ContainsKey(name))
				return m_locateExecutableCache[name];

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

		public override void ExecASyncCore(string path, string[] arguments)
		{
			try
			{
				using (System.Diagnostics.Process p = new System.Diagnostics.Process())
				{
					p.StartInfo.FileName = FileAdaptProcessExec(path);
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
			return (NativeMethods.Kill(process.Id, 15) == 0);
		}

		public override Json FetchUrl(Json request)
		{
			return NativeMethods.CUrl(request);
		}

		public override void FlushDNS()
		{
			base.FlushDNS();

			Engine.Instance.Elevated.DoCommandSync("dns-flush", "services", Engine.Instance.ProfileOptions.Get("linux.dns.services"));
		}

		public override void OpenUrl(string url)
		{
			SystemExec exec = new SystemExec();
			exec.Path = LocateExecutable("xdg-open");
			exec.Arguments.Add(url);
			exec.WaitEnd = false;
			exec.Run();
		}

		public override void OpenFolder(string path)
		{
			SystemExec exec = new SystemExec();
			exec.Path = LocateExecutable("xdg-open");
			exec.Arguments.Add(SystemExec.EscapePath(path));
			exec.WaitEnd = false;
			exec.Run();
		}

		public override void RouteApply(Json jRoute, string action)
		{
			IpAddress destination = jRoute["destination"].ValueString;

			Core.Elevated.Command c = new Core.Elevated.Command();
			c.Parameters["command"] = "route";
			if (destination.IsV4)
				c.Parameters["layer"] = "ipv4";
			else if (destination.IsV6)
				c.Parameters["layer"] = "ipv6";
			c.Parameters["action"] = (action == "add" ? "add" : "delete");
			c.Parameters["destination"] = destination.ToCIDR(true);
			if (jRoute.HasKey("interface"))
			{
				c.Parameters["interface"] = jRoute["interface"].ValueString;
			}
			if (jRoute.HasKey("gateway"))
			{
				IpAddress gateway = jRoute["gateway"].ValueString;
				if (gateway.Valid)
					c.Parameters["gateway"] = gateway.ToCIDR();
			}
			if (jRoute.HasKey("metric"))
				c.Parameters["metric"] = jRoute["metric"].ValueString;
			Engine.Instance.Elevated.DoCommandSync(c);
		}

		public override IpAddresses ResolveDNS(string host)
		{
			IpAddresses result = new IpAddresses();

			string getentPath = LocateExecutable("getent");
			if (getentPath != "")
			{
				// Note: CNAME record are automatically followed.
				SystemExec exec = new SystemExec();
				exec.Path = getentPath;
				exec.Arguments.Add("ahosts");
				exec.Arguments.Add(SystemExec.EscapeHost(host));
				exec.NoDebugLog = true;
				if (exec.Run())
				{
					string o = exec.Output;
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

			report.Add("ip addr show", (LocateExecutable("ip") != "") ? SystemExec.Exec2(LocateExecutable("ip"), "addr", "show") : "'ip' " + LanguageManager.GetText(LanguageItems.NotFound));
			report.Add("ip link show", (LocateExecutable("ip") != "") ? SystemExec.Exec2(LocateExecutable("ip"), "link", "show") : "'ip' " + LanguageManager.GetText(LanguageItems.NotFound));
			/*
			report.Add("ip -4 route show", (LocateExecutable("ip") != "") ? SystemExec.Exec3(LocateExecutable("ip"), "-4", "route", "show") : "'ip' " + LanguageManager.GetText(LanguageItems.NotFound));
			report.Add("ip -6 route show", (LocateExecutable("ip") != "") ? SystemExec.Exec3(LocateExecutable("ip"), "-6", "route", "show") : "'ip' " + LanguageManager.GetText(LanguageItems.NotFound));
			*/
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

		public override void CompatibilityAfterProfile()
		{
			// < 2.11 - Old file name
			if (Platform.Instance.FileExists("/etc/resolv.conf.airvpn"))
				Platform.Instance.FileDelete("/etc/resolv.conf.airvpn");

			// A bug in old experimental 2.11 cause the set of immutable flag in rare cases.
			if (Platform.Instance.FileImmutableGet("/etc/resolv.conf"))
				Platform.Instance.FileImmutableSet("/etc/resolv.conf", false);
		}

		public override void AdaptConfigOpenVpn(Core.ConfigBuilder.OpenVPN config)
		{
			base.AdaptConfigOpenVpn(config);

			// 2.8, to resolve some issue on some distro, ex. Fedora 21
			// 2.22.2, removed, with OpenVPN3 throw a deprecated fatal error.
			//config.AppendDirective("route-delay", "5", ""); 
		}

		public override void OnCheckEnvironmentApp()
		{
			string getentPath = LocateExecutable("getent");
			if (getentPath == "")
				throw new Exception("'getent' " + LanguageManager.GetText(LanguageItems.NotFound));

			string ipPath = LocateExecutable("ip");
			if (ipPath == "")
				throw new Exception("'ip' " + LanguageManager.GetText(LanguageItems.NotFound));
		}

		public override void OnCheckUpMonitor()
		{
			DnsSwitchCheck();
		}

		public override bool OnCheckSingleInstance()
		{
			try
			{
#if EDDIE_DOTNET
				int currentId = Environment.ProcessId;
#else
				int currentId = Process.GetCurrentProcess().Id;
#endif


				string path = DirectoryTemp() + DirSep + Constants.Name + "_" + Constants.AppID + ".pid";
				if (File.Exists(path) == false)
				{
				}
				else
				{
					int otherId;
					if (int.TryParse(Platform.Instance.FileContentsReadText(path), out otherId))
					{
						string procFile = "/proc/" + otherId.ToString(CultureInfo.InvariantCulture) + "/cmdline";
						if (File.Exists(procFile))
						{
							return false;
						}
					}
				}

				Platform.Instance.FileContentsWriteText(path, currentId.ToString(CultureInfo.InvariantCulture), Encoding.ASCII);

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
#if EDDIE_DOTNET
				int currentId = Environment.ProcessId;
#else
				int currentId = Process.GetCurrentProcess().Id;
#endif
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

		public override string RootExecutionOutsideBundleAdapt(string exePath)
		{
			if (Environment.GetEnvironmentVariable("APPIMAGE") == null)
				return exePath;

			if (exePath.StartsWithInv(GetApplicationPath()) == false)
				return exePath;

			// Copy to temporary file, will be deleted by RootExecutionOutsideBundleDelete

			string name = new FileInfo(exePath).Name;
			string newPath = DirectoryTemp() + DirSep + name;
			if (File.Exists(newPath))
				File.Delete(newPath);
			File.Copy(exePath, newPath);
			return newPath;
		}

		public override void RootExecutionOutsideBundleDelete(string exePath)
		{
			if (exePath.StartsWithInv(DirectoryTemp()))
			{
				System.IO.File.Delete(exePath);
			}
		}

		public override void OnNetworkLockManagerInit()
		{
			base.OnNetworkLockManagerInit();

			m_netlockPluginNftables = new NetworkLockNftables();
			m_netlockPluginIptablesLegacy = new NetworkLockIptablesLegacy();
			m_netlockPluginIptablesNFT = new NetworkLockIptablesNFT();
			m_netlockPluginIptables = new NetworkLockIptables();

			Engine.Instance.NetworkLockManager.AddPlugin(m_netlockPluginNftables);
			Engine.Instance.NetworkLockManager.AddPlugin(m_netlockPluginIptablesLegacy);
			Engine.Instance.NetworkLockManager.AddPlugin(m_netlockPluginIptablesNFT);
			Engine.Instance.NetworkLockManager.AddPlugin(m_netlockPluginIptables);
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

					Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText(LanguageItems.OsLinuxNetworkAdapterIPv6Disabled, interfaceName));
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

				Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText(LanguageItems.OsLinuxNetworkAdapterIPv6Restored, entry.Interface));
			}

			m_listIpV6Mode.Clear();

			Recovery.Save();

			base.OnIPv6Restore();

			return true;
		}

		public void DnsSwitchCheck()
		{
			if (GetDnsSwitchMode() == "auto")
			{
				if (m_dnsSwitch.Count > 0)
				{
					Engine.Instance.Elevated.DoCommandSync("dns-switch-do", "dns", m_dnsSwitch.ToString(), "check", "1");
				}
			}
		}

		public override bool OnDnsSwitchDo(Core.ConnectionTypes.IConnectionType connection, IpAddresses dns)
		{
			if (GetDnsSwitchMode() == "auto")
			{
				m_dnsSwitch = dns.Clone();

				if (m_dnsSwitch.Count > 0)
					Engine.Instance.Elevated.DoCommandSync("dns-switch-do", "dns", m_dnsSwitch.ToString());
			}

			base.OnDnsSwitchDo(connection, dns);

			return true;
		}

		public override bool OnDnsSwitchRestore()
		{
			Engine.Instance.Elevated.DoCommandSync("dns-switch-restore");
			m_dnsSwitch.Clear();

			base.OnDnsSwitchRestore();

			return true;
		}

		public override void OnNetworkInterfaceInfoBuild(NetworkInterface networkInterface, Json jNetworkInterface)
		{
			base.OnNetworkInterfaceInfoBuild(networkInterface, jNetworkInterface);

			string id = jNetworkInterface["id"].Value as string;

			// Base virtual always 'false'. Missing Mono implementation?
			jNetworkInterface["support_ipv6"].Value = true;
			{
				if (GetSupportIPv6() == false)
					jNetworkInterface["support_ipv6"].Value = false;
				else if (Conversions.ToInt32(GetSysCtl("/net/ipv6/conf/" + id.SafeAlphaNumeric() + "/disable_ipv6")) != 0)
					jNetworkInterface["support_ipv6"].Value = false;
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
			// See comment in OsCredentialsSystemWrite
			string timeoutPath = LocateExecutable("timeout");
			if (timeoutPath == "")
				return "";
			string secretToolPath = LocateExecutable("secret-tool");
			SystemExec exec = new SystemExec();
			//exec.Path = secretToolPath;
			exec.Path = timeoutPath;
			exec.Arguments.Add("10");
			exec.Arguments.Add(secretToolPath);
			exec.Arguments.Add("lookup");
#if EDDIE_DOTNET
			exec.Arguments.Add("Eddie Profile");
			exec.Arguments.Add(SystemExec.EscapeInsideQuote(name));
#else
			exec.Arguments.Add("'Eddie Profile'");
			exec.Arguments.Add("'" + SystemExec.EscapeInsideQuote(name) + "'");
#endif
			exec.Run();
			int exitCode = exec.ExitCode;
			if (exitCode == 0)
				return exec.Output;
			else
				return "";
		}

		public override bool OsCredentialSystemWrite(string name, string password)
		{
			// Due to unresolved bug (that occur also in fresh Kali installed in 2023-06
			// https://gitlab.gnome.org/GNOME/gnome-keyring/-/issues/116
			// if gnome-keyring is stuck, timeout never occur. Mono timeout don't work.
			// TOFIX: migrate to a better tool.
			string timeoutPath = LocateExecutable("timeout");
			if (timeoutPath == "")
				return false;
			string secretToolPath = LocateExecutable("secret-tool");
			SystemExec exec = new SystemExec();
			//exec.Path = secretToolPath;
			exec.Path = timeoutPath;
			exec.Arguments.Add("10");
			exec.Arguments.Add(secretToolPath);
			exec.Arguments.Add("store");
			exec.Arguments.Add("--label='Eddie, saved password for " + SystemExec.EscapeInsideQuote(name) + " profile'");
#if EDDIE_DOTNET
			exec.Arguments.Add("Eddie Profile");
			exec.Arguments.Add(SystemExec.EscapeInsideQuote(name));
#else
			exec.Arguments.Add("'Eddie Profile'");
			exec.Arguments.Add("'" + SystemExec.EscapeInsideQuote(name) + "'");
#endif
			exec.AutoWriteStdin = password + "\n";
			exec.Run();
			int exitCode = exec.ExitCode;
			return (exitCode == 0);
		}

		public override bool OsCredentialSystemDelete(string name)
		{
			string secretToolPath = LocateExecutable("secret-tool");
			SystemExec exec = new SystemExec();
			exec.Path = secretToolPath;
			exec.Arguments.Add("clear");
#if EDDIE_DOTNET
			exec.Arguments.Add("Eddie Profile");
			exec.Arguments.Add(SystemExec.EscapeInsideQuote(name));
#else
			exec.Arguments.Add("'Eddie Profile'");
			exec.Arguments.Add("'" + SystemExec.EscapeInsideQuote(name) + "'");
#endif
			exec.Run();
			int exitCode = exec.ExitCode;
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

		public override bool GetSupportIPv6()
		{
			if (Conversions.ToInt32(GetSysCtl("/net/ipv6/conf/all/disable_ipv6")) != 0)
				return false;
			else if (Conversions.ToInt32(GetSysCtl("/net/ipv6/conf/default/disable_ipv6")) != 0)
				return false;
			else if (FileExists("/proc/net/if_inet6") == false) // Disabled via Grub for example
				return false;
			else
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

		public string GetDnsSwitchMode()
		{
			string current = Engine.Instance.ProfileOptions.GetLower("dns.mode");

			if (current == "resolvconf") // Compatibility, Deprecated
				current = "auto";
			if (current == "rename") // Compatibility, Deprecated
				current = "auto";

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

		public int RunElevated(string[] arguments, bool waitEnd)
		{
			m_elevatedRunPath = GetElevatedHelperPath();

			m_elevatedRunPath = RootExecutionOutsideBundleAdapt(m_elevatedRunPath);

			string elevationMethod = Engine.Instance.StartCommandLine.Get("elevation", "auto");
			bool redirectStdOut = false;
			if (elevationMethod == "auto")
			{
				elevationMethod = "";

				if ((elevationMethod == "") && (FileRunAsRoot(m_elevatedRunPath)))
				{
					// chown root:root eddie-cli-elevated
					// chmod u+s eddie-cli-elevated
					elevationMethod = "none";
				}

				if ((elevationMethod == "") && (IsElevatedPrivileges()))
				{
					// Already root access
					elevationMethod = "none";
				}

				if ((elevationMethod == "") && (Engine.Instance.IsUiApp()) && (LocateExecutable("pkexec") != ""))
				{
					elevationMethod = "pkexec";
				}

				if ((elevationMethod == "") && (Engine.Instance.IsConsole()) && (LocateExecutable("sudo") != ""))
				{
					List<string> groups = new List<string>(SystemExec.Exec0(LocateExecutable("groups")).ToLowerInv().Split(' '));

					if (groups.Contains("sudo")) // Ubuntu
						elevationMethod = "sudo";
					if (groups.Contains("wheel")) // Manjaro
						elevationMethod = "sudo";
				}

				if ((elevationMethod == "") && (Engine.Instance.IsConsole()) && (LocateExecutable("su") != ""))
				{
					elevationMethod = "su"; // Debian
				}

				if ((elevationMethod == "") && (LocateExecutable("pkexec") != ""))
				{
					elevationMethod = "pkexec";
				}
			}

			System.Diagnostics.Process process = new System.Diagnostics.Process();
			if (elevationMethod == "none")
			{
				process.StartInfo.FileName = FileAdaptProcessExec(m_elevatedRunPath);
#if EDDIE_DOTNET
				foreach (string arg in arguments)
					process.StartInfo.ArgumentList.Add(arg);
#else
				process.StartInfo.Arguments = String.Join(" ", arguments);
#endif
			}
			else if (elevationMethod == "sudo")
			{
				// Under Ubuntu 23 and Manjaro (but not in Debian12), 
				// sudo change terminal endofline, like https://askubuntu.com/questions/1457265/piping-sudo-command-through-another-command-adds-excess-whitespace
				// Redirect the output prevent that.
				redirectStdOut = true;

				process.StartInfo.FileName = LocateExecutable("sudo");
#if EDDIE_DOTNET
				process.StartInfo.ArgumentList.Add(m_elevatedRunPath);
				foreach (string arg in arguments)
					process.StartInfo.ArgumentList.Add(arg);
#else
				process.StartInfo.Arguments = "\"" + m_elevatedRunPath + "\" " + String.Join(" ", arguments);
#endif
			}
			else if (elevationMethod == "su")
			{
				// Under Ubuntu don't work, root disabled.
				// Under Debian12 works.
				process.StartInfo.FileName = "su";
#if EDDIE_DOTNET
				process.StartInfo.ArgumentList.Add("-c");
				process.StartInfo.ArgumentList.Add(m_elevatedRunPath + " " + String.Join(" ", arguments));
#else
				process.StartInfo.Arguments = "-c \"'" + m_elevatedRunPath + "' " + String.Join(" ", arguments) + "\"";
#endif
			}
			else if (elevationMethod == "pkexec")
			{
				process.StartInfo.FileName = LocateExecutable("pkexec");
#if EDDIE_DOTNET
				process.StartInfo.ArgumentList.Add(m_elevatedRunPath);
				foreach (string arg in arguments)
					process.StartInfo.ArgumentList.Add(arg);
#else
				process.StartInfo.Arguments = "\"" + m_elevatedRunPath + "\" " + String.Join(" ", arguments);
#endif
			}

			if (process.StartInfo.FileName == "")
			{
				Engine.Instance.Logs.LogFatal("Unable to find a method to run elevated process.");
				return 0;
			}
			else
			{
				process.StartInfo.WorkingDirectory = "";

				process.StartInfo.Verb = "run";
				process.StartInfo.CreateNoWindow = true;
				process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
				process.StartInfo.UseShellExecute = false;
				if (redirectStdOut)
					process.StartInfo.RedirectStandardOutput = true;

				process.Start();

				if (waitEnd)
				{
					process.WaitForExit();
					return process.ExitCode;
				}
				else
				{
					return process.Id;
				}
			}
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
