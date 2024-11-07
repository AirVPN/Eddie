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
using System.Xml;

using Eddie.Core;

#if !EDDIE_DOTNET
	// If errors occur here, probably Xamarin update cause trouble. Remove Xamarin.Mac reference and re-add by browsing to path
	// /Library/Frameworks/Xamarin.Mac.framework/Versions/Current/lib/mono/4.5/Xamarin.Mac.dll
	using AppKit;
	using Foundation;
	using System.Net;
#endif

namespace Eddie.Platform.MacOS
{
	public class Platform : Core.Platform
	{
		private string m_version = "";
		private string m_launchdPath = "/Library/LaunchDaemons/org.airvpn.eddie.ui.elevated.plist";

		private List<DnsSwitchEntry> m_listDnsSwitch = new List<DnsSwitchEntry>();
		private List<IpV6ModeEntry> m_listIpV6Mode = new List<IpV6ModeEntry>();

		private string m_workaroundIpv6DnsLookupEnableInterface = "";

#if !EDDIE_DOTNET
		private string m_osArchitecture = "";
#endif

		// Override
		public Platform()
		{
		}

		public override string GetCode()
		{
			return "MacOS";
		}

		public override string GetName()
		{
			return "macOS " + GetVersion();
		}

		public override string GetVersion()
		{
			if (m_version == "")
				m_version = SystemExec.Exec1(LocateExecutable("sw_vers"), "-productVersion").Trim(); // Example output: '10.14.3'
			return m_version;
		}

#if !EDDIE_DOTNET
		public override string GetOsArchitecture()
		{
			if(m_osArchitecture == "")
			{
				// Note: uname -m still return "x64" also in arm64,
				// because net4 still run with Rosetta
				string cpuBrand = SystemExec.Exec2(LocateExecutable("sysctl"), "-n", "machdep.cpu.brand_string").Trim();
				if (cpuBrand.Contains("Apple"))
					m_osArchitecture = "arm64";
				else
					m_osArchitecture = "x64";
			}
			return m_osArchitecture;
		}
#endif

		public override bool OnInit()
		{
			base.OnInit();

#if !EDDIE_DOTNET
			// Clean mono_crash files, otherwise Elevated can't run (fail codesign bundle verification)
			if(true)
			{
				string resPath = "../Resources";
				resPath = FileGetAbsolutePath(resPath, GetApplicationPath());
				resPath = FileGetPhysicalPath(resPath);
				if (DirectoryExists(resPath))
				{
					foreach (string file in Directory.GetFiles(resPath, "mono_crash*"))
						FileDelete(file);
				}
			}
#endif

			/*
#if !EDDIE_DOTNET
			
			// TOCLEAN, Removed in 2.24.0, read NSPipe code comment
			// Need remove because otherwise not compilable with .Net7
			if (Engine.Instance.IsConsole())
			{
				NSApplication.Init(); // Requested in CLI edition to call NSPipe, NSTask etc.			
			}			
#endif
			*/

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
			// Only in OSX, always save in 'home' path also with portable edition.
			return "home";
		}
		*/

		public override Eddie.Core.Elevated.IElevated StartElevated()
		{
			ElevatedImpl e = new ElevatedImpl();
			e.Start();
			return e;
		}

		public override bool IsElevatedPrivileges()
		{
			// With root privileges with AuthorizationExecuteWithPrivileges, Environment.UserName still return the normal username, 'whoami' return 'root'.
			string u = SystemExec.Exec(LocateExecutable("whoami"), new string[] { }).ToLowerInvariant().Trim();
			return (u == "root");
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
			return true;
		}

		public override bool CheckElevatedProcessAllowed(string remotePath)
		{
			// TOFIX
			return true;
		}

		public override bool GetAutoStart()
		{
			return false;
		}

		public override bool SetAutoStart(bool value)
		{
			return false;
		}

		public override bool AllowService()
		{
			return true;
		}

		public override string AllowServiceUserDescription()
		{
			return "If checked, install a Launchd daemon";
		}

		protected override bool GetServiceImpl()
		{
			return FileExists(m_launchdPath);
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

			int result = NativeMethods.FileGetImmutable(path);
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
			if ((path == "") || (Platform.Instance.FileExists(path) == false))
				return false;

			int result = NativeMethods.FileGetMode(path);
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
				result = NativeMethods.FileSetMode(path, newResult);
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

			int result = NativeMethods.FileGetMode(path);
			if (result == -1)
			{
				Engine.Instance.Logs.Log(LogType.Warning, "Failed to detect if '" + path + "' is executable");
				return false;
			}

			int newResult = result | 73; // +x :<> (S_IXUSR | S_IXGRP | S_IXOTH) 

			if (newResult != result)
			{
				result = NativeMethods.FileSetMode(path, newResult);
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
			return NativeMethods.FileGetRunAsRoot(path);
		}

		public override string GetExecutableReport(string path)
		{
			string otoolPath = LocateExecutable("otool");
			if (otoolPath != "")
				return SystemExec.Exec2(otoolPath, "-L", SystemExec.EscapePath(path));
			else
				return "'otool' " + LanguageManager.GetText(LanguageItems.NotFound);
		}

#if !EDDIE_DOTNET
		public override string GetExecutablePathEx()
		{
			string currentPath = System.Reflection.Assembly.GetEntryAssembly().Location;
			if (new FileInfo(currentPath).Directory.Name == "MonoBundle")
			{
				// OSX Bundle detected, use the launcher executable
				currentPath = currentPath.Replace("/MonoBundle/", "/MacOS/").Replace(".exe", "");
			}
			else if (Process.GetCurrentProcess().ProcessName.StartsWith("mono", StringComparison.InvariantCultureIgnoreCase))
			{
				// mono <app>, Entry Assembly path it's ok
			}
			else
			{
				currentPath = Process.GetCurrentProcess().MainModule.FileName;
			}
			return currentPath;
		}
#endif

		protected override string GetUserPathEx()
		{
			string basePath = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
			if (basePath == null)
				basePath = "";
			if (basePath == "")
				basePath = Environment.GetEnvironmentVariable("HOME") + DirSep + ".config";

			return basePath + DirSep + "eddie";
		}

		public override bool ProcessKillSoft(Process process)
		{
			return (NativeMethods.Kill(process.Id, 15) == 0);
		}

		public override int GetOpenVpnRecommendedRcvBufDirective()
		{
			return 256 * 1024;
		}

		public override int GetOpenVpnRecommendedSndBufDirective()
		{
			return 256 * 1024;
		}

		public override Json FetchUrl(Json request)
		{
			return NativeMethods.eddie_curl(request);
		}

		public override void FlushDNS()
		{
			base.FlushDNS();

			Engine.Instance.Elevated.DoCommandSync("dns-flush", "services", Engine.Instance.ProfileOptions.Get("linux.dns.services"));
		}

		// TOCLEAN, NSPipe edition.
		// We don't honestly know why exists this NSPipe edition, .Net base works.
		// Disabled in 2.24.0 (2023-11-23), keep here for comparison
		/*
		public override void ExecSyncCore(string path, string[] arguments, string autoWriteStdin, out string stdout, out string stderr, out int exitCode)
		{
			if (autoWriteStdin != "")
				throw new Exception("ExecSyncCore::AutoWriteStdin not supported in macOS"); // Never need yet, used only in Linux

			try
			{
				var pipeOut = new NSPipe();
				var pipeErr = new NSPipe();

				var t = new NSTask();

				t.LaunchPath = path;
				t.Arguments = arguments;
				t.StandardOutput = pipeOut;
				t.StandardError = pipeErr;

				t.Launch();
				t.WaitUntilExit();
				//t.Release();
				t.Dispose();

				NSFileHandle fileOut = pipeOut.ReadHandle;
				stdout = fileOut.ReadDataToEndOfFile().ToString().Trim();
				fileOut.CloseFile();

				NSFileHandle fileErr = pipeErr.ReadHandle;
				stderr = fileErr.ReadDataToEndOfFile().ToString().Trim();
				fileErr.CloseFile();

				exitCode = t.TerminationStatus; // Note 2023-11-23: wrong, it's not exitCode
			}
			catch (Exception ex)
			{
				stdout = "";
				stderr = "Error: " + ex.Message;
				exitCode = -1;
			}
					
			if(false)
			{
				// Used to compare output between base edition and NSPipe edition
				string stdout2;
				string stderr2;
				int exitcode2;

				base.ExecSyncCore(path, arguments, autoWriteStdin, out stdout2, out stderr2, out exitcode2);

				if(stdout != stdout2)
				{
					Console.WriteLine("stdout diff");
				}
				if (stderr != stderr2)
				{
					Console.WriteLine("stderr diff");
				}
				if (exitCode != exitcode2)
				{
					// Occur, but because NSPipe edition is wrong.
					Console.WriteLine("exitcode diff");
				}
			}
		}
		*/

		public override string LocateResource(string relativePath)
		{
			string resPath = "../Resources/" + relativePath;
			resPath = FileGetAbsolutePath(resPath, GetApplicationPath());
			resPath = FileGetPhysicalPath(resPath);
			if ((File.Exists(resPath)) || (Directory.Exists(resPath)))
			{
				return resPath;
			}

			return base.LocateResource(relativePath);
		}

		public override bool IsPortLocalListening(int port)
		{
			// TOFIX, need a better implementation

			string stdout = "";
			string stderr = "";
			int exitcode = 0;
			ExecSyncCore(LocateExecutable("netstat"), new string[] { "-an", "-ptcp" }, "", out stdout, out stderr, out exitcode);

			foreach (string line in stdout.Split('\n'))
			{
				string[] fields = line.CleanSpace().Split(' ');
				if (fields.Length > 2)
				{
					if (fields[3] == "127.0.0.1." + port.ToString(CultureInfo.InvariantCulture))
					{
						if (fields[5] == "LISTEN")
						{
							return true;
						}
					}
				}
			}
			return false;
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
				c.Parameters["interface"] = jRoute["interface"].ValueString;
			if (jRoute.HasKey("gateway"))
				c.Parameters["gateway"] = new IpAddress(jRoute["gateway"].ValueString).Address;
			if (jRoute.HasKey("metric"))
				c.Parameters["metric"] = jRoute["metric"].ValueString;
			string result = Engine.Instance.Elevated.DoCommandSync(c);
		}

		public override IpAddresses ResolveDNS(string host)
		{
			// Base method with Dns.GetHostEntry have cache issue, for example on Fedora. OS X it's based on Mono.
			// Also, base methods with Dns.GetHostEntry sometime don't fetch AAAA IPv6 addresses.

			IpAddresses result = new IpAddresses();

			string hostPath = LocateExecutable("host");
			if (hostPath != "")
			{
				// Note: CNAME record are automatically followed.
				SystemExec exec = new SystemExec();
				exec.Path = LocateExecutable("host");
				exec.Arguments.Add("-W 5");
				exec.Arguments.Add(SystemExec.EscapeHost(host));
				exec.NoDebugLog = true;
				if (exec.Run())
				{
					string hostout = exec.Output;
					foreach (string line in hostout.Split('\n'))
					{
						string ipv4 = line.RegExMatchOne("^.*? has address (.*?)$");
						if (ipv4 != "")
							result.Add(ipv4.Trim());

						string ipv6 = line.RegExMatchOne("^.*? has IPv6 address (.*?)$");
						if (ipv6 != "")
							result.Add(ipv6.Trim());
					}
				}
			}

			return result;
		}

		public override IpAddresses DetectDNS()
		{
			IpAddresses list = new IpAddresses();

			// Method1: Don't return DHCP DNS
			string networksetupPath = LocateExecutable("networksetup");
			if (networksetupPath != "")
			{
				string[] interfaces = GetInterfaces();
				foreach (string i in interfaces)
				{
					string i2 = i.Trim();

					string current = SystemExec.Exec(networksetupPath, new string[] { "-getdnsservers", SystemExec.EscapeInsideQuote(i2) });

					foreach (string line in current.Split('\n'))
					{
						string field = line.Trim();
						list.Add(field);
					}

				}
			}

			// Method2 - More info about DHCP DNS
			string scutilPath = LocateExecutable("scutil");
			if (scutilPath != "")
			{
				string scutilOut = SystemExec.Exec1(scutilPath, "--dns");
				List<List<string>> result = scutilOut.Replace(" ", "").RegExMatchMulti("nameserver\\[[0-9]+\\]:([0-9:\\.]+)");
				foreach (List<string> match in result)
				{
					foreach (string field in match)
					{
						list.Add(field);
					}
				}
			}

			// Method3 - Compatibility
			try
			{
				if (FileExists("/etc/resolv.conf"))
				{
					string o = FileContentsReadText("/etc/resolv.conf");
					foreach (string line in o.Split('\n'))
					{
						if (line.Trim().StartsWith("#", StringComparison.InvariantCulture))
							continue;
						if (line.Trim().StartsWith("nameserver", StringComparison.InvariantCulture))
						{
							string field = line.Substring(11).Trim();
							list.Add(field);
						}
					}
				}
			}
			catch
			{
				// Can be unreadable (root 600), ignore
			}

			return list;
		}

		public override void OnReport(Report report)
		{
			base.OnReport(report);

			report.Add("ifconfig", (LocateExecutable("ifconfig") != "") ? SystemExec.Exec0(LocateExecutable("ifconfig")) : "'ifconfig' " + LanguageManager.GetText(LanguageItems.NotFound));
		}

		public override Dictionary<string, string> GetProcessesList()
		{
			// We experience some crash under OSX with the base method.
			Dictionary<string, string> result = new Dictionary<string, string>();
			string psPath = LocateExecutable("ps");
			if (psPath != "")
			{
				string resultS = SystemExec.Exec2(psPath, "-eo", "pid,command");
				string[] resultA = resultS.Split('\n');
				foreach (string pS in resultA)
				{
					int posS = pS.IndexOf(' ');
					if (posS != -1)
					{
						string pid = pS.Substring(0, posS).Trim();
						string name = pS.Substring(posS).Trim();
						result[pid] = name;
					}
				}
			}

			return result;
		}

		public override void OnCheckEnvironmentApp()
		{
			string networksetupPath = LocateExecutable("networksetup");
			if (networksetupPath == "")
				throw new Exception("'networksetup' " + LanguageManager.GetText(LanguageItems.NotFound));

			string pfctlPath = LocateExecutable("pfctl");
			if (pfctlPath == "")
				Engine.Instance.Logs.Log(LogType.Warning, "'pfctl' " + LanguageManager.GetText(LanguageItems.NotFound));

			string hostPath = LocateExecutable("host");
			if (hostPath == "")
				Engine.Instance.Logs.Log(LogType.Warning, "'host' " + LanguageManager.GetText(LanguageItems.NotFound));

			string psPath = LocateExecutable("ps");
			if (psPath == "")
				Engine.Instance.Logs.Log(LogType.Warning, "'ps' " + LanguageManager.GetText(LanguageItems.NotFound));
		}

		public override bool OnCheckEnvironmentSession()
		{
			return true;
		}

		public override void OnNetworkLockManagerInit()
		{
			base.OnNetworkLockManagerInit();

			Engine.Instance.NetworkLockManager.AddPlugin(new NetworkLockOsxPf());
		}

		public override void OnRecoveryLoad(XmlElement root)
		{
			XmlElement nodeDns = root.GetFirstElementByTagName("DnsSwitch");
			if (nodeDns != null)
			{
				foreach (XmlElement nodeEntry in nodeDns.ChildNodes)
				{
					DnsSwitchEntry entry = new DnsSwitchEntry();
					entry.ReadXML(nodeEntry);
					m_listDnsSwitch.Add(entry);
				}
			}

			XmlElement nodeIpV6 = root.GetFirstElementByTagName("IpV6");
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

			if (m_listDnsSwitch.Count != 0)
			{
				XmlElement nodeDns = (XmlElement)root.AppendChild(doc.CreateElement("DnsSwitch"));
				foreach (DnsSwitchEntry entry in m_listDnsSwitch)
				{
					XmlElement nodeEntry = nodeDns.AppendChild(doc.CreateElement("entry")) as XmlElement;
					entry.WriteXML(nodeEntry);
				}
			}

			if (m_listIpV6Mode.Count != 0)
			{
				XmlElement nodeDns = (XmlElement)root.AppendChild(doc.CreateElement("IpV6"));
				foreach (IpV6ModeEntry entry in m_listIpV6Mode)
				{
					XmlElement nodeEntry = nodeDns.AppendChild(doc.CreateElement("entry")) as XmlElement;
					entry.WriteXML(nodeEntry);
				}
			}
		}

		public override bool OnIPv6Block()
		{
			string result = Engine.Instance.Elevated.DoCommandSync("ipv6-block");
			if (result != "")
			{
				foreach (string resultItem in result.Split('\n'))
				{
					string[] fields = resultItem.Split(';');
					if (fields.Length != 6)
						continue;
					if (fields[0] == "SwitchIPv6")
					{
						string interfaceName = fields[1];

						IpV6ModeEntry entry = new IpV6ModeEntry();
						entry.Interface = interfaceName;
						entry.Mode = fields[2];
						entry.Address = fields[3];
						entry.Router = fields[4];
						entry.PrefixLength = fields[5];
						m_listIpV6Mode.Add(entry);

						Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText(LanguageItems.OsMacNetworkAdapterIPv6Disabled, interfaceName));
					}
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
				c.Parameters["mode"] = entry.Mode;
				c.Parameters["address"] = entry.Address;
				c.Parameters["router"] = entry.Router;
				c.Parameters["prefix"] = entry.PrefixLength;
				Engine.Instance.Elevated.DoCommandSync(c);

				Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText(LanguageItems.OsMacNetworkAdapterIPv6Restored, entry.Interface));
			}

			m_listIpV6Mode.Clear();

			Recovery.Save();

			base.OnIPv6Restore();

			return true;
		}

		public override bool OnDnsSwitchDo(Core.ConnectionTypes.IConnectionType connection, IpAddresses dns)
		{
			string mode = Engine.Instance.ProfileOptions.GetLower("dns.mode");

			if (mode == "auto")
			{
				string result = Engine.Instance.Elevated.DoCommandSync("dns-switch-do", "dns", dns.ToString());
				if (result != "")
				{
					foreach (string resultItem in result.Split('\n'))
					{
						string[] fields = resultItem.Split(';');
						if (fields.Length != 3)
							continue;
						if (fields[0] == "SwitchDNS")
						{
							string interfaceName = fields[1];
							IpAddresses oldIPs = new IpAddresses(fields[2]);

							DnsSwitchEntry e = new DnsSwitchEntry();
							e.Name = interfaceName;
							e.Dns = oldIPs.Addresses;
							m_listDnsSwitch.Add(e);

							Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText(LanguageItems.OsMacNetworkAdapterDnsDone, interfaceName, ((oldIPs.Count == 0) ? "Automatic" : oldIPs.Addresses), dns.Addresses));
						}
					}
				}

				Recovery.Save();
			}

			base.OnDnsSwitchDo(connection, dns);

			return true;
		}

		public override bool OnDnsSwitchRestore()
		{
			foreach (DnsSwitchEntry e in m_listDnsSwitch)
			{
				IpAddresses dns = new IpAddresses();
				dns.Add(e.Dns);

				string result = Engine.Instance.Elevated.DoCommandSync("dns-switch-restore", "interface", e.Name, "dns", dns.ToString());

				Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText(LanguageItems.OsMacNetworkAdapterDnsRestored, e.Name, ((e.Dns == "") ? "Automatic" : e.Dns)));
			}

			m_listDnsSwitch.Clear();

			Recovery.Save();

			base.OnDnsSwitchRestore();

			return true;
		}

		public override bool OnInterfaceDo(Core.ConnectionTypes.IConnectionType connection)
		{
			if (Engine.Instance.ProfileOptions.GetBool("macos.ipv6.dnslookup"))
			{
				Core.Elevated.Command c = new Core.Elevated.Command();
				c.Parameters["command"] = "workaround-ipv6-dns-lookup-enable";
				c.Parameters["iface"] = connection.Interface.Name;
				string result = Engine.Instance.Elevated.DoCommandSync(c);

				string msg = "Workaround to enable IPv6 DNS lookups";
				if (result.StartsWithInv("err:"))
				{
					// No exception, it's only a workaround.
					Engine.Instance.Logs.LogVerbose(msg + " failed: " + result.Substring(4));
				}
				else if (result == "not_need_already")
				{
					Engine.Instance.Logs.LogVerbose(msg + " not needed, already looked AAAA records up");
				}
				else if (result == "not_need_noipv6")
				{
					Engine.Instance.Logs.LogVerbose(msg + " not needed, not IPv6");
				}
				else if (result == "ok")
				{
					Engine.Instance.Logs.LogVerbose(msg + " activated");
					m_workaroundIpv6DnsLookupEnableInterface = connection.Interface.Name;
				}
				else
				{
					Engine.Instance.Logs.LogVerbose(msg + " unexpected result");
				}
			}
			return true;
		}

		public override bool OnInterfaceRestore()
		{
			if (m_workaroundIpv6DnsLookupEnableInterface != "")
			{
				Core.Elevated.Command c = new Core.Elevated.Command();
				c.Parameters["command"] = "workaround-ipv6-dns-lookup-disable";
				c.Parameters["iface"] = m_workaroundIpv6DnsLookupEnableInterface;
				string result = Engine.Instance.Elevated.DoCommandSync(c);

				string msg = "Workaround for enabling IPv6 DNS lookups";
				if (result.StartsWithInv("err:"))
				{
					// No exception, it's only a workaround.
					Engine.Instance.Logs.LogVerbose(msg + " failed: " + result.Substring(4));
				}
				else if (result == "ok")
				{
					Engine.Instance.Logs.LogVerbose(msg + " deactivated");
				}
				else
				{
					Engine.Instance.Logs.LogVerbose(msg + " unexpected result");
				}
			}
			return true;
		}

		public override Json GetRealtimeNetworkStats()
		{
			// Mono NetworkInterface::GetIPv4Statistics().BytesReceived always return 0 under OSX.

			Json result = new Json();
			result.EnsureArray();

			int maxLen = 1024;
			byte[] buf = new byte[maxLen];
			NativeMethods.GetRealtimeNetworkStats(buf, maxLen);
			string jNativeStr = System.Text.Encoding.ASCII.GetString(buf);

			Json jNative = Json.Parse(jNativeStr);

			// Expect the sequence is the same.
			// C++ edition don't detect interface name right now, otherwise NetworkInterface here can be removed.
			NetworkInterface[] interfaces = Engine.Instance.GetNetworkInterfaces();
			lock (interfaces)
			{
				for (int i = 0; i < interfaces.Length; i++)
				{
					if (i < jNative.GetArray().Count)
					{
						Json jNativeIf = jNative.GetIndex(i) as Json;

						Int64 rcv = jNativeIf["rcv"].ValueInt64;
						Int64 snd = jNativeIf["snd"].ValueInt64;
						Json jInterface = new Json();
						jInterface["id"].Value = interfaces[i].Id;
						jInterface["rcv"].Value = rcv;
						jInterface["snd"].Value = snd;
						result.Append(jInterface);
					}
				}
			}

			return result;
		}

		public override void OnNetworkInterfaceInfoBuild(NetworkInterface networkInterface, Json jNetworkInterface)
		{
			Json j = Json.Parse(Engine.Instance.Elevated.DoCommandSync("network-interface-info", "id", jNetworkInterface["id"].ValueString));
			if (j.HasKey("friendly"))
				jNetworkInterface["friendly"].Value = j["friendly"].ValueString;
			if (j.HasKey("support_ipv4"))
				jNetworkInterface["support_ipv4"].Value = (j["support_ipv4"].ValueString == "true");
			if (j.HasKey("support_ipv6"))
				jNetworkInterface["support_ipv6"].Value = (j["support_ipv6"].ValueString == "true");
		}

		public string[] GetInterfaces()
		{
			List<string> result = new List<string>();
			foreach (string line in SystemExec.Exec(LocateExecutable("networksetup"), new string[] { "-listallnetworkservices" }).Split('\n'))
			{
				if (line.StartsWith("An asterisk", StringComparison.InvariantCultureIgnoreCase))
					continue;
				if (line.Trim() == "")
					continue;
				result.Add(line.Trim());
			}

			return result.ToArray();
		}

		public override string OsCredentialSystemName()
		{
			return "macOS Keychain";
		}

		public override string OsCredentialSystemRead(string name)
		{
			return NativeMethods.CredentialSystemRead(Constants.Name + " - " + name, name);
			/*
			// Xamarin edition
			byte[] b = null;
			var code = Security.SecKeyChain.FindGenericPassword(Constants.Name + " - " + name, name, out b);
			if (code == Security.SecStatusCode.Success)
			return Encoding.UTF8.GetString(b);
			else
			return "";
			*/
		}

		public override bool OsCredentialSystemWrite(string name, string password)
		{
			if (OsCredentialSystemDelete(name) == false) // Otherwise is not overwritten
				return false;

			return NativeMethods.CredentialSystemWrite(Constants.Name + " - " + name, name, password);
			/*
			// Xamarin edition
			var b = Encoding.UTF8.GetBytes(password);

			Security.SecStatusCode ssc;
			ssc = Security.SecKeyChain.AddGenericPassword(Constants.Name + " - " + name, name, b);
			return (ssc == Security.SecStatusCode.Success);
			*/
		}

		public override bool OsCredentialSystemDelete(string name)
		{
			return NativeMethods.CredentialSystemDelete(Constants.Name + " - " + name, name);
			/*
			// Xamarin edition
			Security.SecRecord sr = new Security.SecRecord(Security.SecKind.GenericPassword);
			sr.Service = Constants.Name + " - " + name;
			sr.Account = name;
			Security.SecStatusCode ssc;
			Security.SecRecord find = Security.SecKeyChain.QueryAsRecord(sr, out ssc);
			if (ssc == Security.SecStatusCode.Success)
			{
				sr.ValueData = find.ValueData;
				Security.SecStatusCode ssc2 = Security.SecKeyChain.Remove(sr);
				if (ssc2 == Security.SecStatusCode.Success)
					return true;
				else
					return false;
			}
			else if (ssc == Security.SecStatusCode.ItemNotFound)
				return true;
			else
				return false;
			*/
		}

		public override List<string> GetTrustedPaths()
		{
			List<string> list = base.GetTrustedPaths();
			list.Add(LocateExecutable("codesign"));
			list.Add(LocateExecutable("whoami"));

			return list;
		}

		public override string FileGetSignedId(string path)
		{
			string codesignPath = LocateExecutable("codesign");
			SystemExec cmdV = new SystemExec();
			cmdV.Path = codesignPath;
			cmdV.Arguments.Add("-v");
			cmdV.Arguments.Add(SystemExec.EscapePath(path));
			cmdV.Run();
			if (cmdV.Output != "") // ExitCode always 0
				return "No: Invalid signature (tampered?)";

			SystemExec cmdS = new SystemExec();
			cmdS.Path = codesignPath;
			cmdS.Arguments.Clear();
			cmdS.Arguments.Add("-dv");
			cmdS.Arguments.Add("--verbose=4");
			cmdS.Arguments.Add(SystemExec.EscapePath(path));
			cmdS.Run();

			string codesignResult = cmdS.Output;
			string o = "";
			foreach (string line in codesignResult.Split('\n'))
			{
				int posE = line.IndexOf("=", StringComparison.InvariantCulture);
				if (posE != -1)
				{
					string k = line.Substring(0, posE);
					if (k == "Authority")
					{
						if (o != "")
							o += " - ";
						o += line.Substring(posE + 1);
					}
				}
			}

			if (o != "")
				return o;
			else
				return base.FileGetSignedId(path);
		}

		public override string GetDriverVersion(string driver)
		{
			return "Expected";
		}

		public override bool PreferHummingbirdIfAvailable()
		{
			return false;
		}

		public int RunElevated(string[] arguments, bool waitEnd)
		{
			string path = GetElevatedHelperPath();

			string elevationMethod = Engine.Instance.StartCommandLine.Get("elevation", "auto");
			if (elevationMethod == "auto")
			{
				if (FileRunAsRoot(path))
					elevationMethod = "none";
				else if (IsElevatedPrivileges())
					elevationMethod = "none";
				else if (Engine.Instance.IsUiApp())
					elevationMethod = "ui";
				else if (Engine.Instance.IsConsole())
					elevationMethod = "sudo";
				else
					elevationMethod = "none";
			}
			if ((elevationMethod == "sudo") && (LocateExecutable("sudo") == ""))
				elevationMethod = "none";

			System.Diagnostics.Process process = null;
			bool processDirectResult = false;

			if (elevationMethod == "none")
			{
				process = new System.Diagnostics.Process();
				process.StartInfo.FileName = path;
				process.StartInfo.Arguments = string.Join(" ", arguments);
			}
			else if (elevationMethod == "sudo")
			{
				process = new System.Diagnostics.Process();
				process.StartInfo.FileName = "sudo";
				process.StartInfo.Arguments = "\"" + path + "\" " + string.Join(" ", arguments);
			}
			/*
			else if (elevationMethod == "osascript")
			{
				// Method not really working,
				// elevated throw "Client not allowed: Connection not from parent process (spot mode)"
				// because is launched under a top-level process called "/System/Library/Frameworks/Security.framework/authtrampoline"
				process = new System.Diagnostics.Process();
				process.StartInfo.FileName = "osascript";
#if EDDIE_DOTNET
				process.StartInfo.ArgumentList.Add("-e");
				process.StartInfo.ArgumentList.Add("do shell script \"" + path + " " + string.Join(" ", arguments) + "\" with prompt \"" + LanguageManager.GetText(LanguageItems.HelperPrivilegesPrompt).Safe() + "\" with administrator privileges");
#else
				process.StartInfo.Arguments = " -e 'do shell script \"" + path + " " + string.Join(" ", arguments) + "\" with prompt \"" + LanguageManager.GetText(LanguageItems.HelperPrivilegesPrompt).Safe() + "\" with administrator privileges'";
#endif
			}
			*/
			else if (elevationMethod == "ui")
			{
				Json jArgs = new Json();
				jArgs.EnsureArray();
				foreach (string arg in arguments)
					jArgs.Append(arg);

				Json j = new Json();
				j["command"].Value = "elevated.start";
				j["args"].Value = jArgs;
				Engine.Instance.UiManager.Broadcast(j);

				// TOFIX: this work because Broadcast above is sync.
				// can be removed when return value.
				System.Threading.Thread.Sleep(500);
				int pid2 = Conversions.ToInt32(SystemExec.Exec2(LocateExecutable("pgrep"), "-f", path));				
				if (pid2 > 0)
					processDirectResult = true;
			}

			if (process != null)
			{
				process.StartInfo.WorkingDirectory = "";
				process.StartInfo.Verb = "run";
				process.StartInfo.CreateNoWindow = true;
				process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
				process.StartInfo.UseShellExecute = false;
				process.Start();

				if (waitEnd)
				{
					process.WaitForExit();
					return process.ExitCode;
				}
				else
					return process.Id;
			}
			else
			{
				// Note: waitEnd ignored:
				// we can't access here in macOS to elevated process with clean C#,
				// we need some kind of loop and pgrep exec,
				// but don't really need to wait, we don't need the exitcode, and maybe deprecated in future
				if (waitEnd)
				{
					System.Threading.Thread.Sleep(1000);
					return 0;
				}
				else
				{
					return processDirectResult ? -1 : 0;
				}
			}
		}
	}

	public class DnsSwitchEntry
	{
		public string Name;
		public string Dns;

		public void ReadXML(XmlElement node)
		{
			Name = node.GetAttributeString("name", "");
			Dns = node.GetAttributeString("dns", "");
		}

		public void WriteXML(XmlElement node)
		{
			node.SetAttributeString("name", Name);
			node.SetAttributeString("dns", Dns);
		}
	}

	public class IpV6ModeEntry
	{
		public string Interface;
		public string Mode;
		public string Address;
		public string Router;
		public string PrefixLength;

		public void ReadXML(XmlElement node)
		{
			Interface = node.GetAttributeString("interface", "");
			Mode = node.GetAttributeString("mode", "");
			Address = node.GetAttributeString("address", "");
			Router = node.GetAttributeString("router", "");
			PrefixLength = node.GetAttributeString("prefix_length", "");
		}

		public void WriteXML(XmlElement node)
		{
			node.SetAttributeString("interface", Interface);
			node.SetAttributeString("mode", Mode);
			node.SetAttributeString("address", Address);
			node.SetAttributeString("router", Router);
			node.SetAttributeString("prefix_length", PrefixLength);
		}
	}
}

