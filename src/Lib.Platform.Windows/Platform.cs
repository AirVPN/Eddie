// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2026 AirVPN (support@airvpn.org) / https://airvpn.org
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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Xml;
using System.Threading;
using Microsoft.Win32;

using Eddie.Core;
using System.Globalization;

// Avoid useless warning "This call site is reachable on all platforms. 'x' is only supported on: 'windows'."
#pragma warning disable CA1416

namespace Eddie.Platform.Windows
{
	public class Platform : Core.Platform
	{
		private string ServiceName = "EddieElevationService";
		
		private List<NetworkManagerDnsEntry> m_listOldDns = new List<NetworkManagerDnsEntry>();
		private string m_oldMetricInterface = "";
		private int m_oldMetricIPv4 = -1;
		private int m_oldMetricIPv6 = -1;
		private Mutex m_mutexSingleInstance = null;
		
		public bool IsXpOrNewer()
		{
			OperatingSystem OS = Environment.OSVersion;
			return (OS.Platform == PlatformID.Win32NT) && (OS.Version.Major >= 5);
		}

		public bool IsVistaOrNewer()
		{
			OperatingSystem OS = Environment.OSVersion;
			return (OS.Platform == PlatformID.Win32NT) && (OS.Version.Major >= 6);
		}

		public bool IsWin7OrNewer()
		{
			OperatingSystem OS = Environment.OSVersion;
			return OS.Platform == PlatformID.Win32NT && (OS.Version.Major > 6 || (OS.Version.Major == 6 && OS.Version.Minor >= 1));
		}

		public bool IsWin8OrNewer()
		{
			OperatingSystem OS = Environment.OSVersion;
			return OS.Platform == PlatformID.Win32NT && (OS.Version.Major > 6 || (OS.Version.Major == 6 && OS.Version.Minor >= 2));
		}

		public bool IsWin10OrNewer()
		{
			OperatingSystem OS = Environment.OSVersion;
			return (OS.Platform == PlatformID.Win32NT) && (OS.Version.Major >= 10);
		}

		public bool IsWin7()
		{
			return (IsWin7OrNewer()) && (IsWin8OrNewer() == false);
		}

		// Override
		public Platform()
		{
		}

		public override string GetCode()
		{
			return "Windows";
		}

		public override string GetUserLocale()
		{
			string osLocale = NativeMethods.GetUserDefaultLocale();
			if (string.IsNullOrEmpty(osLocale) == false)
				return osLocale;

			// Fallback to env vars (useful under WSL / MSYS / Cygwin shells).
			return base.GetUserLocale();
		}

		public override string GetCodeInstaller()
		{
			if (IsWin10OrNewer())
				return "Windows-10";
			else if (IsWin7OrNewer())
				return "Windows-7";
			else if (IsVistaOrNewer())
				return "Windows-Vista";
			else if (IsXpOrNewer())
				return "Windows-XP";
			else
				return GetCode();
		}

		public override string GetName()
		{
			string name = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "ProductName", "").ToString();
			return name;
		}

		public override string GetVersion()
		{
			string v = base.GetVersion();
			v = v.Replace("Microsoft Windows NT", "");
			v = v.Replace("Microsoft Windows", "");
			return v.Trim();
		}

#if !EDDIE_DOTNET
		public override string GetOsArchitecture()
		{
			if (GetProcessArchitecture() == "x64")
				return "x64";
			if (NativeMethods.InternalCheckIsWow64())
				return "x64";
			return "x86";
		}
#endif

		public override bool OnInit()
		{
			base.OnInit();

			try
			{
				// Check VC++ Redistributable
				// Need because our lib include libcurl, and static link of libcurl is not recommended
				bool vcRuntimeAvailable = false;

				// <2.20
				/*
				if (File.Exists(Path.Combine(Environment.SystemDirectory, "vcruntime140_1.dll")))
					vcRuntimeAvailable = true;
				*/

				if (GetProcessArchitecture() == "x86")
				{
					int currentVersion = Conversions.ToInt32(Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\" + GetProcessArchitecture().ToUpperInvariant(), "Major", "0"), 0);
					if (currentVersion >= 14)
						vcRuntimeAvailable = true;
					if (File.Exists(Path.Combine(Environment.SystemDirectory, "vcruntime140.dll")) == false)
						vcRuntimeAvailable = false;
				}
				else
				{
					int currentVersion = Conversions.ToInt32(Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\" + GetProcessArchitecture(), "Major", "0"), 0);
					if (currentVersion >= 14)
						vcRuntimeAvailable = true;

					// Registry check is not affidable, with vcredistr installed via msm/wix
					if ((currentVersion == 14) && (File.Exists(Path.Combine(Environment.SystemDirectory, "vcruntime140_1.dll")) == false))
						vcRuntimeAvailable = false;
				}

				if (vcRuntimeAvailable == false)
				{
					Engine.Instance.Logs.LogFatal("This software require some additional files (C++ Redistributable) that are not currently installed on your system. Use the Installer edition, or look https://eddie.website/windows-runtime/");
					return false;
				}

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

		public override void OnDeInit()
		{
			base.OnDeInit();

			if (IsVistaOrNewer())
				Wfp.Stop();
		}

		public override void OnStart()
		{
			base.OnStart();

			if (IsVistaOrNewer())
				Wfp.Start();
		}

		public override Eddie.Core.Elevated.IElevated StartElevated()
		{
			ElevatedImpl e = new ElevatedImpl();
			e.Start();

			return e;
		}

		public override bool IsElevatedPrivileges()
		{
			return NativeMethods.IsProcessElevated();

			/* Old C# edition
			bool isElevated;
			WindowsIdentity identity = WindowsIdentity.GetCurrent();
			WindowsPrincipal principal = new WindowsPrincipal(identity);
			isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);

			return isElevated;
			*/
		}

		protected override string GetElevatedHelperPathImpl()
		{
			string path = AppDomain.CurrentDomain.BaseDirectory + "Eddie-CLI-Elevated.exe";
			return FileGetPhysicalPath(path);
		}

		public override bool GetAutoStart()
		{
			try
			{
				RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

				if (registryKey.GetValue("Eddie") == null)
					return false;
				else
					return true;
			}
			catch
			{
				return false;
			}
		}

		public override bool SetAutoStart(bool value)
		{
			try
			{
				RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

				if (value)
				{
					registryKey.SetValue("Eddie", GetExecutablePath());
					return true;
				}
				else
				{
					registryKey.DeleteValue("Eddie", false);
					return true;
				}
			}
			catch
			{
				return false;
			}

			/* <2.17.3			
			TaskService ts = null;
			try
			{
				ts = new TaskService();
				if (value == false)
				{
					if (ts.RootFolder.Tasks.Exists("AirVPN"))
						ts.RootFolder.DeleteTask("AirVPN");
					if (ts.RootFolder.Tasks.Exists("Eddie"))
						ts.RootFolder.DeleteTask("Eddie");
				}
				else
				{
					if (ts.RootFolder.Tasks.Exists("AirVPN"))
						ts.RootFolder.DeleteTask("AirVPN");
					if (ts.RootFolder.Tasks.Exists("Eddie"))
						ts.RootFolder.DeleteTask("Eddie");

					// Create a new task definition and assign properties
					TaskDefinition td = ts.NewTask();
					td.Principal.RunLevel = TaskRunLevel.Highest;
					//td.Settings.RunOnlyIfLoggedOn = true;
					td.Settings.DisallowStartIfOnBatteries = false;
					td.Settings.StopIfGoingOnBatteries = false;
					td.Settings.RunOnlyIfNetworkAvailable = false;
					td.Settings.MultipleInstances = TaskInstancesPolicy.IgnoreNew;
					td.Settings.DeleteExpiredTaskAfter = TimeSpan.Zero;
					td.Settings.ExecutionTimeLimit = TimeSpan.Zero;

					td.RegistrationInfo.Description = "Eddie - OpenVPN Client";
					td.Triggers.Add(new LogonTrigger());
					string command = "\"" + GetExecutablePath() + "\"";
					string arguments = "";
					if (Engine.Instance.Options.Get("path") != "")
						arguments = "-path=" + Engine.Instance.Options.Get("path");
					td.Actions.Add(new ExecAction(command, (arguments == "") ? null : arguments, null));

					// Register the task in the root folder
					ts.RootFolder.RegisterTaskDefinition(@"Eddie", td);
				}
			}
			catch (NotV1SupportedException)
			{
				//Ignore, not supported on XP
			}
			finally
			{
				if (ts != null)
					ts.Dispose();
			}

			return true;
			*/
		}

		public override bool AllowService()
		{
			return true;
		}

		public override string AllowServiceUserDescription()
		{
			return "If checked, install a Windows Service";
		}

		public override void WaitService()
		{
			for (int t = 0; t < 100; t++)
			{
				UInt32 status = NativeMethods.GetServiceStatus(ServiceName);
				if (status != 2) // Start Pending
					break;
				/*
				if (status == 4) // Running
					break;
				*/

				System.Threading.Thread.Sleep(100);
			}
		}

		protected override bool GetServiceImpl()
		{
			UInt32 status = NativeMethods.GetServiceStatus(ServiceName);

			/*
			if (status == 4) // Running
				return true;
			*/

			if (status > 0) // Return 0 if not exists
				return true;

			return false;
		}

		protected override bool SetServiceImpl(bool value)
		{
			try
			{
				ProcessStartInfo processStart = new ProcessStartInfo();
				processStart.FileName = "\"" + GetElevatedHelperPath() + "\"";
				processStart.Arguments = "";
				processStart.Verb = "runas";
				processStart.CreateNoWindow = true;
				processStart.UseShellExecute = true;
				if (value)
				{
					processStart.Arguments = "service=install service_port=" + Engine.Instance.GetElevatedServicePort();
					System.Diagnostics.Process p = System.Diagnostics.Process.Start(processStart);
					p.WaitForExit();
					return (GetService(true) == true);
				}
				else
				{
					processStart.Arguments = "service=uninstall";
					System.Diagnostics.Process p = System.Diagnostics.Process.Start(processStart);
					p.WaitForExit();
					return (GetService(true) == false);
				}
			}
			catch (Exception ex)
			{
				Engine.Instance.Logs.Log(ex);
				return false;
			}
		}
		public override bool GetServiceUninstallSupportRealtime()
		{
			return true;
		}

		public override string NormalizeString(string val)
		{
			val = base.NormalizeString(val);
			return val.Replace("\n", "\r\n");
		}

		public override string DirSep
		{
			get
			{
				return "\\";
			}
		}

		public override string EndOfLineSep
		{
			get
			{
				return "\r\n";
			}
		}

		public override string FileGetPhysicalPath(string path)
		{
			string native = NativeMethods.GetFinalPathName(path);
			if (native.StartsWithInv("\\\\?\\"))
				native = native.Substring(4);
			if (native.StartsWithInv("UNC\\"))
				native = "\\" + native.Substring(3);
			return native;
		}

		public override bool FileEnsureCurrentUserOnly(string path)
		{
			/* <2.25.0
			// Remove Inheritance
			SystemExec.Exec1(Platform.Instance.LocateExecutable("icacls.exe"), "\"" + SystemExec.EscapePath(path).EscapeQuote() + "\" /c /t /inheritance:d");

			// Set Ownership to Owner
			SystemExec.Exec1(Platform.Instance.LocateExecutable("icacls.exe"), "\"" + SystemExec.EscapePath(path).EscapeQuote() + "\" /c /t /grant \"" + SystemExec.EscapeInsideQuote(Environment.UserName) + "\":F");

			// Remove Authenticated Users
			SystemExec.Exec1(Platform.Instance.LocateExecutable("icacls.exe"), "\"" + SystemExec.EscapePath(path).EscapeQuote() + "\" /c /t /remove:g *S-1-5-11");

			// Remove other groups
			SystemExec.Exec1(Platform.Instance.LocateExecutable("icacls.exe"), "\"" + SystemExec.EscapePath(path).EscapeQuote() + "\" /c /t /remove Administrator \"BUILTIN\\Administrators\" \"NT AUTHORITY\\Authenticated Users\" \"BUILTIN\\Users\" BUILTIN Everyone System Users");
			*/

			string exe = Platform.Instance.LocateExecutable("icacls.exe");
			string p = SystemExec.EscapePath(path).EscapeQuote();
			string qpath = $"\"{p}\"";

			// Best-effort: reset ACL completely, then re-apply minimal secure ACL.
			// Key points:
			// - /reset wipes all explicit ACEs (including orphaned UNKNOWN SIDs)
			// - /inheritance:r disables inheritance AND removes inherited ACEs
			// - /grant:r replaces (not adds) the DACL with only the specified entries
			// - add SYSTEM + Administrators to avoid edge cases (services, elevated contexts)
			SystemExec.Exec1(exe, $"{qpath} /c /t /reset");
			SystemExec.Exec1(exe, $"{qpath} /c /t /inheritance:r");

			// Use the actual current identity (domain\user or machine\user), not just Environment.UserName
			string me = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

			// Replace ACL with minimal set
			SystemExec.Exec1(exe, $"{qpath} /c /t /grant:r \"{SystemExec.EscapeInsideQuote(me)}\":F \"SYSTEM\":F \"BUILTIN\\Administrators\":F");

			// Set owner explicitly to current identity (some setups care)
			SystemExec.Exec1(exe, $"{qpath} /c /t /setowner \"{SystemExec.EscapeInsideQuote(me)}\"");

			return true;
		}

		public override string FileAdaptProcessExec(string path)
		{
			// Under Windows, better to escape a specific path. Under other OS don't work.
			return "\"" + base.FileAdaptProcessExec(path) + "\"";
		}

#if !EDDIE_DOTNET
		public override string GetExecutablePathEx()
		{
			// It return vshost.exe under VS, better
			string path = Environment.GetCommandLineArgs()[0];
			path = Path.GetFullPath(path); // 2.11.9
			return path;
		}
#endif

		protected override string GetUserPathEx()
		{
			return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Eddie";
		}

		public override string GetDefaultOpenVpnConfigsPath()
		{
			object v = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\OpenVPN", "config_dir", "");
			if (v == null)
				return "";
			else
				return Conversions.ToString(v);
		}	

		public override bool ProcessKillSoft(Core.Process process)
		{
			// This is called only for close SSL or SSH process (running not elevated)
			return false; // Will be a fallback to SIGTERM

			/*
			// Don't work on Win10.
			process.CloseMainWindow();
			return true;
			*/

			/* >=2.16.3
			StandardInput.Close() or StandardInput.WriteLine("\x3") don't work. CloseMainWindow seem the best solution for now.
			*/

			// The below method was used <=2.16.2, but cause a crash with SSL/SSH tunnel at exit, 
			// a generic 'The parameter is incorrect.' in GC Finalizer.
			/*

			bool result = false;

			if (process != null)
			{
				if (NativeMethods.AttachConsole((uint)process.Id))
				{
					NativeMethods.SetConsoleCtrlHandler(null, true);

					try
					{
						if (NativeMethods.GenerateConsoleCtrlEvent((uint)NativeMethods.CtrlTypes.CTRL_C_EVENT, 0))
						{
							process.WaitForExit();
							result = true;
						}
					}
					finally
					{
						NativeMethods.FreeConsole();
						NativeMethods.SetConsoleCtrlHandler(null, false);
					}
				}
			}

			return result;
			*/
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
			return NativeMethods.CUrl(request);
		}

		public override void FlushDNS()
		{
			base.FlushDNS();

			Engine.Instance.Elevated.DoCommandSync("dns-flush");

			SystemExec.Exec1(LocateExecutable("ipconfig.exe"), "/flushdns");
		}

		public override void RouteApply(Json jRoute, string action)
		{
			IpAddress destination = jRoute["destination"].ValueString;
			string iface = jRoute["interface"].ValueString;

			NetworkInterface networkInterface = GetNetworkInterfaceFromGuid(iface) ?? throw new Exception(LanguageManager.GetText(LanguageItems.NetworkInterfaceNotAvailable));
			iface = networkInterface.Name;

			int interfaceIdx = 0;
			if (destination.IsV4)
				interfaceIdx = networkInterface.GetIPProperties().GetIPv4Properties().Index;
			else
				interfaceIdx = networkInterface.GetIPProperties().GetIPv6Properties().Index;

			Core.Elevated.Command c = new Core.Elevated.Command();
			c.Parameters["command"] = "route";
			c.Parameters["action"] = action;
			c.Parameters["destination"] = destination.ToCIDR(true);
			c.Parameters["iface"] = interfaceIdx.ToString(CultureInfo.InvariantCulture);
			if (jRoute.HasKey("gateway"))
			{
				IpAddress gateway = jRoute["gateway"].ValueString;
				c.Parameters["gateway"] = gateway.Address;
			}
			if (jRoute.HasKey("metric"))
				c.Parameters["metric"] = jRoute["metric"].ValueString;
			Engine.Instance.Elevated.DoCommandSync(c);
		}

		public override IpAddresses DetectDNS()
		{
			IpAddresses list = new IpAddresses();

			NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

			foreach (NetworkInterface networkInterface in networkInterfaces)
			{
				if (networkInterface.OperationalStatus == OperationalStatus.Up)
				{
					IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();
					IPAddressCollection dnsAddresses = ipProperties.DnsAddresses;

					foreach (IPAddress dnsAddress in dnsAddresses)
					{
						if (dnsAddress.IsIPv6SiteLocal == false)
							list.Add(dnsAddress.ToString());
					}
				}
			}

			return list;
		}

		public override bool WaitTunReady(Core.ConnectionTypes.IConnectionType connection)
		{
			int tickStart = Environment.TickCount;
			string lastStatus = "";

			for (; ; )
			{
				if (connection.Interface.OperationalStatus == OperationalStatus.Up)
					return true;
				else
				{
					lastStatus = connection.Interface.OperationalStatus.ToString();
				}

				if (Environment.TickCount - tickStart > 10000)
				{
					Engine.Instance.Logs.Log(LogType.Warning, "Tunnel not ready in 10 seconds, contact our support. Last interface status: " + lastStatus);
					return false;
				}

				Engine.Instance.Logs.Log(LogType.Warning, "Waiting TUN interface");

				System.Threading.Thread.Sleep(2000);
			}
		}

		public override void OnReport(Report report)
		{
			base.OnReport(report);

			report.Add("ovpn-dco version", GetDriverVersion("ovpn-dco"));
			report.Add("tap-windows6 version", GetDriverVersion("tap-windows6"));

			report.Add("ipconfig /all", SystemExec.Exec1(Platform.Instance.LocateExecutable("ipconfig.exe"), "/all"));
		}

		public override bool OnCheckSingleInstance()
		{
			m_mutexSingleInstance = new Mutex(false, "Global\\" + "b57887e0-65d0-4d18-b57f-106de6e0f1b6");
			if (m_mutexSingleInstance.WaitOne(0, false) == false)
				return false;
			else
				return true;
		}

		public override void OnElevated()
		{
			base.OnElevated();

			CompatibilityManager.WindowsRemoveTask();
		}

		public override void OnNetworkLockManagerInit()
		{
			base.OnNetworkLockManagerInit();

			if (IsVistaOrNewer()) // 2.10.1
			{
				Engine.Instance.NetworkLockManager.AddPlugin(new NetworkLockWfp());
			}
		}

		public override string OnNetworkLockRecommendedMode()
		{
			if (IsVistaOrNewer() == false)
				return "none";

			return "windows_wfp";
		}

		public override void OnSessionStart()
		{
			// FlushDNS(); // Removed in 2.11.10



			Recovery.Save();
		}

		public override void OnSessionStop()
		{
			// FlushDNS(); // Moved in 2.13.3 in ::session.cs, but only if a connection occur.

			if (Engine.Instance.ProfileOptions.GetBool("windows.adapters.cleanup"))
				Engine.Instance.Elevated.DoCommandSync("network-adapter-clear-all");

			Recovery.Save();
		}

		public override bool OnIPv6Block()
		{
			if (IsVistaOrNewer())
			{
				{
					XmlDocument xmlDocRule = new XmlDocument();
					XmlElement xmlRule = xmlDocRule.CreateElement("rule");
					xmlRule.SetAttribute("name", "IPv6 - Block");
					xmlRule.SetAttribute("layer", "ipv6");
					xmlRule.SetAttribute("action", "block");
					xmlRule.SetAttribute("weight", "3000");
					Wfp.AddItem("ipv6_block_all", xmlRule);
				}

				{
					XmlDocument xmlDocRule = new XmlDocument();
					XmlElement xmlRule = xmlDocRule.CreateElement("rule");
					xmlRule.SetAttribute("name", "IPv6 - Allow loopback");
					xmlRule.SetAttribute("layer", "ipv6");
					xmlRule.SetAttribute("action", "permit");
					xmlRule.SetAttribute("weight", "3001");
					XmlElement XmlIf1 = xmlDocRule.CreateElement("if");
					xmlRule.AppendChild(XmlIf1);
					XmlIf1.SetAttribute("field", "ip_local_interface");
					XmlIf1.SetAttribute("match", "equal");
					XmlIf1.SetAttribute("interface", "loopback");
					Wfp.AddItem("ipv6_allow_loopback", xmlRule);
				}

				Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText(LanguageItems.IPv6DisabledWpf));
			}
			else
				throw new Exception("Unexpected IPv6 block requested");

			base.OnIPv6Block();

			return true;
		}

		public override bool OnIPv6Restore()
		{
			if ((Wfp.RemoveItem("ipv6_block_all")) && (Wfp.RemoveItem("ipv6_allow_loopback")))
				Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText(LanguageItems.IPv6RestoredWpf));

			base.OnIPv6Restore();

			return true;
		}

		public override void AdaptConfigOpenVpn(Core.ConfigBuilder.OpenVPN config)
		{
			base.AdaptConfigOpenVpn(config);

			if ((Engine.Instance.ProfileOptions.GetBool("windows.ipv6.bypass_dns")) && (Engine.Instance.ProfileOptions.GetBool("dns.delegate")))
			{
				config.AppendDirectives("pull-filter ignore \"dhcp-option DNS6\"", "OS");
			}			
		}

		public override string GetConnectionTunDriver(Core.ConnectionTypes.IConnectionType connection)
		{
			if (connection is Core.ConnectionTypes.OpenVPN)
			{
				Core.ConnectionTypes.OpenVPN connectionOpenVPN = connection as Core.ConnectionTypes.OpenVPN;
				if (connectionOpenVPN.ConfigStartup.ExistsDirective("comp-lzo") == false)
					return "ovpn-dco";
				else
					return "tap-windows6";
			}
			else if (connection is Core.ConnectionTypes.WireGuard)
			{
				return "";
			}
			else
				throw new Exception("Unexpected connection type");
		}
		
		private XmlElement CreateDnsResolverRule(string title, string layer)
		{
			XmlDocument xmlDocRule = new XmlDocument();
			XmlElement xmlRule = xmlDocRule.CreateElement("rule");
			xmlRule.SetAttribute("name", title);
			xmlRule.SetAttribute("layer", layer);
			xmlRule.SetAttribute("action", "permit");
			xmlRule.SetAttribute("weight", "2001");
			XmlElement xmlIfPort = xmlDocRule.CreateElement("if");
			xmlRule.AppendChild(xmlIfPort);
			xmlIfPort.SetAttribute("field", "ip_remote_port");
			xmlIfPort.SetAttribute("match", "equal");
			xmlIfPort.SetAttribute("port", "53");
			return xmlRule;
		}

		public override bool OnDnsSwitchDo(Core.ConnectionTypes.IConnectionType connection, IpAddresses dns)
		{
			if ((Engine.Instance.ProfileOptions.GetBool("windows.dns.lock")) && (IsVistaOrNewer()))
			{
				// Order is important! IPv6 block use weight 3000, DNS-Lock 2000, WFP 1000. All within a parent filter of max priority.
				// Otherwise the netlock allow-private rule can allow DNS outside the tunnel in some configuration.
				{
					XmlDocument xmlDocRule = new XmlDocument();
					XmlElement xmlRule = xmlDocRule.CreateElement("rule");
					xmlRule.SetAttribute("name", "Dns - Block port 53");
					xmlRule.SetAttribute("layer", "all-out");
					xmlRule.SetAttribute("action", "block");
					xmlRule.SetAttribute("weight", "2000");
					XmlElement XmlIf1 = xmlDocRule.CreateElement("if");
					xmlRule.AppendChild(XmlIf1);
					XmlIf1.SetAttribute("field", "ip_remote_port");
					XmlIf1.SetAttribute("match", "equal");
					XmlIf1.SetAttribute("port", "53");
					Wfp.AddItem("dns_block_all", xmlRule);
				}

				// Allow port 53 only toward the detected system resolvers, so hostname
				// resolution keeps working while the DNS lock blocks every other port 53
				// flow. Weight 2001 wins over the block (2000) but stays below the IPv6
				// block (3000), so resolvers are still blocked when IPv6 is blocked.
				{
					XmlElement xmlRuleV4 = null;
					XmlElement xmlRuleV6 = null;

					foreach (IpAddress dnsResolver in DetectDNS().IPs)
					{
						if (dnsResolver.Valid == false)
							continue;

						XmlElement xmlRule = null;
						if (dnsResolver.IsV4)
						{
							if (xmlRuleV4 == null)
								xmlRuleV4 = CreateDnsResolverRule("Dns - Allow port 53 toward system resolvers - IPv4", "ipv4-out");
							xmlRule = xmlRuleV4;
						}
						else if (dnsResolver.IsV6)
						{
							if (xmlRuleV6 == null)
								xmlRuleV6 = CreateDnsResolverRule("Dns - Allow port 53 toward system resolvers - IPv6", "ipv6-out");
							xmlRule = xmlRuleV6;
						}

						if (xmlRule == null)
							continue;

						XmlElement xmlIfAddress = xmlRule.OwnerDocument.CreateElement("if");
						xmlRule.AppendChild(xmlIfAddress);
						xmlIfAddress.SetAttribute("field", "ip_remote_address");
						xmlIfAddress.SetAttribute("match", "equal");
						xmlIfAddress.SetAttribute("address", dnsResolver.Address);
						xmlIfAddress.SetAttribute("mask", dnsResolver.Mask);
					}

					if (xmlRuleV4 != null)
						Wfp.AddItem("dns_permit_resolver_v4", xmlRuleV4);
					if (xmlRuleV6 != null)
						Wfp.AddItem("dns_permit_resolver_v6", xmlRuleV6);
				}

				{
					// Remember: This because may fail at WFP side with a "Unknown interface" because network interface with IPv4/IPv6 disabled have Ipv6IfIndex == 0 and don't match the requested interface.
					string layer = "all-out";
					if ((connection.ConfigIPv6 == false) && (layer == "all-out"))
						layer = "ipv4-out";
					if ((connection.BlockedIPv6) && (layer == "all-out"))
						layer = "ipv4-out";
					if ((connection.ConfigIPv4 == false) && (layer == "all-out"))
						layer = "ipv6-out";
					if ((connection.BlockedIPv4) && (layer == "all-out"))
						layer = "ipv6-out";

					if (layer != "")
					{
						XmlDocument xmlDocRule = new XmlDocument();
						XmlElement xmlRule = xmlDocRule.CreateElement("rule");
						xmlRule.SetAttribute("name", "Dns - Allow port 53 on TAP");
						xmlRule.SetAttribute("layer", layer);
						xmlRule.SetAttribute("action", "permit");
						xmlRule.SetAttribute("weight", "2000");
						XmlElement XmlIf1 = xmlDocRule.CreateElement("if");
						xmlRule.AppendChild(XmlIf1);
						XmlIf1.SetAttribute("field", "ip_remote_port");
						XmlIf1.SetAttribute("match", "equal");
						XmlIf1.SetAttribute("port", "53");
						XmlElement XmlIf2 = xmlDocRule.CreateElement("if");
						xmlRule.AppendChild(XmlIf2);
						XmlIf2.SetAttribute("field", "ip_local_interface");
						XmlIf2.SetAttribute("match", "equal");
						XmlIf2.SetAttribute("interface", Engine.Instance.Connection.Interface.Id);
						Wfp.AddItem("dns_permit_tap", xmlRule);
					}
				}


				Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText(LanguageItems.DnsLockActivatedWpf));
			}

			string mode = Engine.Instance.ProfileOptions.GetLower("dns.mode");

			if (mode == "auto")
			{
				List<string> dnsInterfacesNamesList = Engine.Instance.ProfileOptions.Get("dns.interfaces.names").ToLowerInvariant().StringToList();

				string dnsInterfacesTypes = Engine.Instance.ProfileOptions.Get("dns.interfaces.types").ToLowerInvariant();
				if (dnsInterfacesTypes == "auto")
					dnsInterfacesTypes = "ethernet, virtual";

				List<string> dnsInterfacesTypesList = dnsInterfacesTypes.StringToList();

				Json jNetworkInfo = Engine.Instance.NetworkInfoBuild();
				foreach (Json jNetworkInterface in jNetworkInfo["interfaces"].Json.GetArray())
				{
					string interfaceId = jNetworkInterface["id"].ValueString;
					string interfaceName = jNetworkInterface["name"].ValueString;
					string interfaceType = jNetworkInterface["type"].ValueString;

					bool skip = true;

					if ((Engine.Instance.Connection != null) && (interfaceId == Engine.Instance.Connection.Interface.Id))
						skip = false;

					if (dnsInterfacesTypes.Contains("all"))
						skip = false;
					if (dnsInterfacesNamesList.Contains(interfaceId.ToLowerInvariant()))
						skip = false;
					if (dnsInterfacesNamesList.Contains(interfaceName.ToLowerInvariant()))
						skip = false;
					if (dnsInterfacesTypesList.Contains(jNetworkInterface["type"].ValueString.ToLowerInvariant()))
						skip = false;

					if (jNetworkInterface["status"].ValueString != "Up")
						skip = true;

					if (skip == false)
					{
						// IPv4
						IpAddresses dnsIPv4 = new IpAddresses(jNetworkInterface["dns4"].Value as string);
						if (!dnsIPv4.OnlyIPv4.Equals(dns.OnlyIPv4))
						{
							NetworkManagerDnsEntry entry = new NetworkManagerDnsEntry();
							entry.Guid = interfaceId;
							entry.Description = interfaceName;
							entry.Layer = "IPv4";
							entry.Dns = dnsIPv4;

							if (dns.OnlyIPv4.Count == 0)
							{
								Engine.Instance.Elevated.DoCommandSync("windows-dns", "layer", "ipv4", "interface", interfaceName, "mode", "dhcp");
							}

							int nIPv4 = 0;
							foreach (IpAddress ip in dns.OnlyIPv4.IPs)
							{
								Engine.Instance.Elevated.DoCommandSync("windows-dns", "layer", "ipv4", "interface", interfaceName, "ipaddress", ip.Address, "mode", ((nIPv4 == 0) ? "static" : "add"));
								nIPv4++;
							}

							m_listOldDns.Add(entry);
							Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText(LanguageItems.OsWindowsNetworkAdapterDnsDone, "IPv4", entry.Description, (entry.Dns.OnlyIPv4.Count == 0 ? "automatic" : "manual" + " (" + entry.Dns.OnlyIPv4.ToString() + ")"), dns.OnlyIPv4.ToString()));
						}

						IpAddresses dnsIPv6 = new IpAddresses(jNetworkInterface["dns6"].Value as string);
						if (!dnsIPv6.OnlyIPv6.Equals(dns.OnlyIPv6))
						{
							NetworkManagerDnsEntry entry = new NetworkManagerDnsEntry();
							entry.Guid = interfaceId;
							entry.Description = interfaceName;
							entry.Layer = "IPv6";
							entry.Dns = dnsIPv6;

							if (dns.OnlyIPv6.Count == 0)
							{
								Engine.Instance.Elevated.DoCommandSync("windows-dns", "layer", "ipv6", "interface", interfaceName, "mode", "dhcp");
							}

							int nIPv6 = 0;
							foreach (IpAddress ip in dns.OnlyIPv6.IPs)
							{
								Engine.Instance.Elevated.DoCommandSync("windows-dns", "layer", "ipv6", "interface", interfaceName, "ipaddress", ip.Address, "mode", ((nIPv6 == 0) ? "static" : "add"));
								nIPv6++;
							}

							m_listOldDns.Add(entry);
							Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText(LanguageItems.OsWindowsNetworkAdapterDnsDone, "IPv6", entry.Description, (entry.Dns.OnlyIPv6.Count == 0 ? "automatic" : "manual" + " (" + entry.Dns.OnlyIPv6.ToString() + ")"), dns.OnlyIPv6.ToString()));
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
			DnsForceRestore();

			bool DnsPermitExists = false;
			DnsPermitExists |= Wfp.RemoveItem("dns_permit_resolver_v4");
			DnsPermitExists |= Wfp.RemoveItem("dns_permit_resolver_v6");
			DnsPermitExists |= Wfp.RemoveItem("dns_permit_tap");
			DnsPermitExists |= Wfp.RemoveItem("dns_block_all");
			if (DnsPermitExists)
				Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText(LanguageItems.DnsLockDeactivatedWpf));

			base.OnDnsSwitchRestore();
			return true;
		}

		public override bool OnInterfaceDo(Core.ConnectionTypes.IConnectionType connection)
		{
			NetworkInterface adapter = connection.Interface;
			string id = adapter.Id;

			int interfaceMetricIPv4Value = Engine.Instance.ProfileOptions.GetInt("windows.metrics.tap.ipv4");
			int interfaceMetricIPv6Value = Engine.Instance.ProfileOptions.GetInt("windows.metrics.tap.ipv6");
			if ((interfaceMetricIPv4Value == -1) || (interfaceMetricIPv6Value == -1))
				return true;

			if (interfaceMetricIPv4Value == -2) // Automatic/Recommended
				interfaceMetricIPv4Value = 3;
			if (interfaceMetricIPv6Value == -2) // Automatic/Recommended
				interfaceMetricIPv6Value = 3;

			int interfaceMetricIPv4Idx = -1;
			string interfaceMetricIPv4Name = "";
			int interfaceMetricIPv6Idx = -1;
			string interfaceMetricIPv6Name = "";

			NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface adapter2 in interfaces)
			{
				if (adapter2.Id == id)
				{
					if (interfaceMetricIPv4Value != -1)
					{
						try
						{
							interfaceMetricIPv4Idx = adapter.GetIPProperties().GetIPv4Properties().Index;
							interfaceMetricIPv4Name = adapter.Name;
						}
						catch
						{
							// Throw System.Net.NetworkInformation.NetworkInformationException: 
							// 'The requested protocol has not been configured into the system, or no implementation for it exists'
							// if IPv4 it's disabled in the adapter.
						}
					}
					if (interfaceMetricIPv6Value != -1)
					{
						try
						{
							interfaceMetricIPv6Idx = adapter.GetIPProperties().GetIPv6Properties().Index;
							interfaceMetricIPv6Name = adapter.Name;
						}
						catch
						{
							// Throw System.Net.NetworkInformation.NetworkInformationException: 
							// 'The requested protocol has not been configured into the system, or no implementation for it exists'
							// if IPv6 it's disabled in the adapter.
						}
					}
					break;
				}
			}

			int interfaceMetricIPv4Current = -1;
			int interfaceMetricIPv6Current = -1;

			if (interfaceMetricIPv4Idx != -1)
				interfaceMetricIPv4Current = NativeMethods.GetInterfaceMetric(interfaceMetricIPv4Idx, "ipv4");
			if (interfaceMetricIPv6Idx != -1)
				interfaceMetricIPv6Current = NativeMethods.GetInterfaceMetric(interfaceMetricIPv6Idx, "ipv6");

			if ((interfaceMetricIPv4Current != -1) && (interfaceMetricIPv4Current != interfaceMetricIPv4Value))
			{
				string fromStr = (interfaceMetricIPv4Current == 0) ? "Automatic" : interfaceMetricIPv4Current.ToString(CultureInfo.InvariantCulture);
				string toStr = (interfaceMetricIPv4Value == 0) ? "Automatic" : interfaceMetricIPv4Value.ToString(CultureInfo.InvariantCulture);
				Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText(LanguageItems.NetworkAdapterMetricSwitch, interfaceMetricIPv4Name, fromStr, toStr, "IPv4"));
				m_oldMetricInterface = id;
				m_oldMetricIPv4 = interfaceMetricIPv4Current;
				Engine.Instance.Elevated.DoCommandSync("set-interface-metric", "idx", interfaceMetricIPv4Idx.ToString(CultureInfo.InvariantCulture), "layer", "ipv4", "value", interfaceMetricIPv4Value.ToString(CultureInfo.InvariantCulture));

				Recovery.Save();
			}

			if ((interfaceMetricIPv6Current != -1) && (interfaceMetricIPv6Current != interfaceMetricIPv6Value))
			{
				string fromStr = (interfaceMetricIPv6Current == 0) ? "Automatic" : interfaceMetricIPv6Current.ToString(CultureInfo.InvariantCulture);
				string toStr = (interfaceMetricIPv6Value == 0) ? "Automatic" : interfaceMetricIPv6Value.ToString(CultureInfo.InvariantCulture);
				Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText(LanguageItems.NetworkAdapterMetricSwitch, interfaceMetricIPv6Name, fromStr, toStr, "IPv6"));
				m_oldMetricInterface = id;
				m_oldMetricIPv6 = interfaceMetricIPv6Current;
				Engine.Instance.Elevated.DoCommandSync("set-interface-metric", "idx", interfaceMetricIPv6Idx.ToString(CultureInfo.InvariantCulture), "layer", "ipv6", "value", interfaceMetricIPv6Value.ToString(CultureInfo.InvariantCulture));

				Recovery.Save();
			}

			return base.OnInterfaceDo(connection);
		}

		public override bool OnInterfaceRestore()
		{
			if (m_oldMetricInterface != "")
			{
				NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
				foreach (NetworkInterface adapter in interfaces)
				{
					if (adapter.Id == m_oldMetricInterface)
					{
						if (m_oldMetricIPv4 != -1)
						{
							int idx = adapter.GetIPProperties().GetIPv4Properties().Index;
							int current = NativeMethods.GetInterfaceMetric(idx, "ipv4");
							if (current != m_oldMetricIPv4)
							{
								string fromStr = (current == 0) ? "Automatic" : current.ToString(CultureInfo.InvariantCulture);
								string toStr = (m_oldMetricIPv4 == 0) ? "Automatic" : m_oldMetricIPv4.ToString(CultureInfo.InvariantCulture);
								Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText(LanguageItems.NetworkAdapterMetricRestore, adapter.Name, fromStr, toStr, "IPv4"));
								Engine.Instance.Elevated.DoCommandSync("set-interface-metric", "idx", idx.ToString(CultureInfo.InvariantCulture), "layer", "ipv4", "value", m_oldMetricIPv4.ToString(CultureInfo.InvariantCulture));
							}
							m_oldMetricIPv4 = -1;
						}

						if (m_oldMetricIPv6 != -1)
						{
							int idx = adapter.GetIPProperties().GetIPv6Properties().Index;
							int current = NativeMethods.GetInterfaceMetric(idx, "ipv6");
							if (current != m_oldMetricIPv6)
							{
								string fromStr = (current == 0) ? "Automatic" : current.ToString(CultureInfo.InvariantCulture);
								string toStr = (m_oldMetricIPv6 == 0) ? "Automatic" : m_oldMetricIPv6.ToString(CultureInfo.InvariantCulture);
								Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText(LanguageItems.NetworkAdapterMetricRestore, adapter.Name, fromStr, toStr, "IPv6"));
								Engine.Instance.Elevated.DoCommandSync("set-interface-metric", "idx", idx.ToString(CultureInfo.InvariantCulture), "layer", "ipv6", "value", m_oldMetricIPv6.ToString(CultureInfo.InvariantCulture));
							}
							m_oldMetricIPv6 = -1;
						}
						break;
					}
				}

				m_oldMetricInterface = ""; // Do anyway, even if interface not found
			}
			return base.OnInterfaceRestore();
		}

		public override void OnSessionLogEvent(string source, string message)
		{
			if (message.IndexOfInv("Waiting for TUN/TAP interface to come up") != -1)
			{
				if (Engine.Instance.ProfileOptions.GetBool("windows.tap_up"))
				{
					HackWindowsInterfaceUp();
				}
			}
		}

		public override void OnRecoveryAlways()
		{
			base.OnRecoveryAlways();

			if (IsWin7OrNewer()) // 2.12.2
				if (Wfp.ClearPendingRules())
					Engine.Instance.Logs.Log(LogType.Warning, LanguageManager.GetText(LanguageItems.WfpRecovery));

			if (Engine.Instance.ProfileOptions.GetBool("windows.adapters.cleanup"))
				Engine.Instance.Elevated.DoCommandSync("network-adapter-clear-all");
		}

		public override void OnRecoveryLoad(XmlElement root)
		{
			XmlElement nodeDns = root.GetFirstElementByTagName("DnsSwitch");
			if (nodeDns != null)
			{
				foreach (XmlElement nodeEntry in nodeDns.ChildNodes)
				{
					NetworkManagerDnsEntry entry = new NetworkManagerDnsEntry();
					entry.ReadXML(nodeEntry);
					m_listOldDns.Add(entry);
				}
			}

			if (root.ExistsAttribute("interface-metric-id"))
			{
				m_oldMetricInterface = root.GetAttributeString("interface-metric-id", "");
				m_oldMetricIPv4 = root.GetAttributeInt("interface-metric-ipv4", -1);
				m_oldMetricIPv6 = root.GetAttributeInt("interface-metric-ipv6", -1);
			}

			base.OnRecoveryLoad(root);
		}

		public override void OnRecoverySave(XmlElement root)
		{
			base.OnRecoverySave(root);

			XmlDocument doc = root.OwnerDocument;

			if (m_listOldDns.Count != 0)
			{
				XmlElement nodeDns = (XmlElement)root.AppendChild(doc.CreateElement("DnsSwitch"));
				foreach (NetworkManagerDnsEntry entry in m_listOldDns)
				{
					XmlElement nodeEntry = nodeDns.AppendChild(doc.CreateElement("entry")) as XmlElement;
					entry.WriteXML(nodeEntry);
				}
			}

			if (m_oldMetricInterface != "")
			{
				root.SetAttributeString("interface-metric-id", m_oldMetricInterface);
				root.SetAttributeInt("interface-metric-ipv4", m_oldMetricIPv4);
				root.SetAttributeInt("interface-metric-ipv6", m_oldMetricIPv6);
			}
		}

		public override void OnNetworkInfoBuild(Json jNetworkInfo)
		{
			base.OnNetworkInfoBuild(jNetworkInfo);

			// Try to discover "device_id", used to avoid to reuse proprietary VPN driver when discovering the interface to use			
			String registryKeyNetworkInterfacesPath = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}";
			using (Microsoft.Win32.RegistryKey registryKeyNetworkInterfaces = Registry.LocalMachine.OpenSubKey(registryKeyNetworkInterfacesPath))
			{
				foreach (String subkeyName in registryKeyNetworkInterfaces.GetSubKeyNames())
				{
					try
					{
						RegistryKey registryKeyNetworkInterface = registryKeyNetworkInterfaces.OpenSubKey(subkeyName);

						string id = registryKeyNetworkInterface.GetValue("NetCfgInstanceId") as string;
						foreach (Json jNetworkInterface in jNetworkInfo["interfaces"].Json.GetArray())
						{
							if (jNetworkInterface.GetKey("id") as string == id)
							{
								string deviceID = registryKeyNetworkInterface.GetValue("MatchingDeviceId") as string;
								if (deviceID != null)
									jNetworkInterface["device_id"].Value = deviceID;
							}
						}
					}
					catch
					{
						// Can throw access denied, ignore	
					}
				}
			}
		}

		public override void OnNetworkInterfaceInfoBuild(NetworkInterface networkInterface, Json jNetworkInterface)
		{
			base.OnNetworkInterfaceInfoBuild(networkInterface, jNetworkInterface);

			string id = jNetworkInterface["id"].ValueString;

			string dns4 = Registry.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces\\" + id.ToLowerInvariant(), "NameServer", "") as string;
			string dns6 = Registry.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\Tcpip6\\Parameters\\Interfaces\\" + id.ToLowerInvariant(), "NameServer", "") as string;
			jNetworkInterface["dns4"].Value = dns4;
			jNetworkInterface["dns6"].Value = dns6;
		}

		public override void OnNetworkRouteListBuild(Json jRoutesList)
		{
			// C++ code return an interface index, here converted to interface ID/Name
			Dictionary<int, string> InterfacesIndexToGuid = new Dictionary<int, string>();
			NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface adapter in interfaces)
			{
				string guid = adapter.Id.ToString();

				try
				{
					int interfaceIPv4Index = adapter.GetIPProperties().GetIPv4Properties().Index;
					InterfacesIndexToGuid[interfaceIPv4Index] = guid;
				}
				catch
				{
				}

				try
				{
					int interfaceIPv6Index = adapter.GetIPProperties().GetIPv6Properties().Index;
					InterfacesIndexToGuid[interfaceIPv6Index] = guid;
				}
				catch
				{
				}
			}

			jRoutesList.FromJson(Engine.Instance.Elevated.DoCommandSync("route-list"));
			foreach (Json jRoute in jRoutesList.GetArray())
			{
				int interfaceIndex = Conversions.ToInt32(jRoute["interface_index"].ValueString);
				if (InterfacesIndexToGuid.ContainsKey(interfaceIndex))
					jRoute["interface"].Value = InterfacesIndexToGuid[interfaceIndex];
			}
		}

		public override string GetFriendlyInterfaceName(string id)
		{
			NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface adapter in interfaces)
			{
				if (adapter.Id == id)
					return adapter.Name + " (" + adapter.Description + ")";
			}
			return id;
		}

		public override string OsCredentialSystemName()
		{
			if (IsElevatedPrivileges()) // Are saved as Admin and not viewer by normal user, will become an issue.
				return "";
			else
				return "Windows Credential";
		}

		public override string OsCredentialSystemRead(string name)
		{
			return CredentialManager.Read(Constants.Name, name);
		}

		public override bool OsCredentialSystemWrite(string name, string password)
		{
			return CredentialManager.Write(Constants.Name, name, password);
		}

		public override bool OsCredentialSystemDelete(string name)
		{
			return CredentialManager.Delete(Constants.Name, name);
		}

		public override bool GetRequireRouteGateway()
		{
			return true;
		}

		public override bool GetUseOpenVpnRoutes()
		{
			// In 2.21, we implement routes in Eddie,
			// and use our system also in OpenVPN.
			// But in Win7 don't work well.
			if ((Constants.OurRoutesInWin7) && (IsWin7()))
				return true;
			return false;
		}

		public override bool GetSupportIPv4()
		{
			object v = Registry.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\services\\TCPIP\\Parameters", "DisabledComponents", "");
			if (Conversions.ToUInt32(v, 0) == 0) // 0 is the Windows default if the key doesn't exist.
				v = 0;

			return (Conversions.ToUInt32(v, 0) == 0);
		}

		public override bool GetSupportIPv6()
		{
			bool available = true;

			// Based on: http://support.microsoft.com/kb/929852
			// Based on: https://www.wincert.net/networking/ipv6-breaking-down-the-disabledcomponents-registry-value/
			object reg = Registry.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\services\\TCPIP6\\Parameters", "DisabledComponents", "");
			UInt32 v = Conversions.ToUInt32(reg, 0);

			if ((v & (1 << 0)) != 0) // Bit 0 controls ALL of the IPv6 tunnel interfaces. If 1 is disabled
				available = false;
			if ((v & (1 << 4)) != 0) // Bit 4 controls IPv6 for non-tunnel interfaces. If 1 is disabled
				available = false;

			return available;
		}

		public override string GetDriverVersion(string driver)
		{
			try
			{
				if (driver == "ovpn-dco")
				{
					return GetDriverSysServiceVersion("ovpn-dco");
				}
			else if (driver == "tap-windows6")
			{
				return GetDriverSysServiceVersion("tap0901");
			}
				else
					throw new Exception("Unknown driver " + driver);
			}
			catch (Exception ex)
			{
				Engine.Instance.Logs.Log(ex);
			}

			return "";
		}

		public override string OpenVpnEnsureInterface(string driver, string ifaceName)
		{
			if (GetDriverVersion(driver) == "")
				Core.Engine.Instance.Elevated.DoCommandSync("driver-install", "driver", driver);

			Json jNetworkInfo = Engine.Instance.NetworkInfoUpdate();
			Json jNetworkInterfaceSelected = FindDriverInterface(driver, ifaceName, jNetworkInfo);

			if (jNetworkInterfaceSelected == null)
			{
				// If a conflicting adapter was removed, give Windows time to release the name,
				// otherwise the rename step of tapctl create fails (error 0x1) on the just-freed name.
				if (RemoveConflictingDriverInterface(driver, ifaceName, jNetworkInfo))
					System.Threading.Thread.Sleep(3000);

				Core.Engine.Instance.Elevated.DoCommandSync("network-adapter-create", "driver", driver, "name", ifaceName);
				Engine.Instance.Logs.LogVerbose("Created new " + driver + " network interface \"" + ifaceName + "\"");

				jNetworkInterfaceSelected = FindDriverInterface(driver, ifaceName);
			}

			if (jNetworkInterfaceSelected == null)
				throw new Exception(LanguageManager.GetText(LanguageItems.OsDriverAdapterNotAvailable, driver));

			Engine.Instance.Logs.LogVerbose("Using " + driver + " network interface \"" + jNetworkInterfaceSelected["name"].ValueString + " (" + jNetworkInterfaceSelected["description"].ValueString + ")\"");

			return jNetworkInterfaceSelected["id"].ValueString;
		}

		private Json FindDriverInterface(string driver, string ifaceName)
		{
			return FindDriverInterface(driver, ifaceName, Engine.Instance.NetworkInfoUpdate());
		}

		private Json FindDriverInterface(string driver, string ifaceName, Json jNetworkInfo)
		{
			string expectedDeviceId = "";
			if (driver == "tap-windows6")
				expectedDeviceId = "tap0901";
			else if (driver == "ovpn-dco")
				expectedDeviceId = "ovpn-dco";

			foreach (Json jNetworkInterface in jNetworkInfo["interfaces"].Json.GetArray())
			{
				if (jNetworkInterface.HasKey("device_id") == false)
					continue;
				if (jNetworkInterface["type"].ValueString != "Virtual")
					continue;
				if (jNetworkInterface["status"].ValueString != "Down")
					continue;
				if (jNetworkInterface["name"].ValueString != ifaceName)
					continue;
				if (jNetworkInterface["device_id"].ValueString.ToLowerInvariant().Contains(expectedDeviceId) == false)
					continue;

				return jNetworkInterface;
			}

			return null;
		}

		// Safety net for the case where an adapter with the requested name already exists but belongs
		// to the other OpenVPN driver (tap-windows6 vs ovpn-dco): tapctl create would fail on the name
		// clash. Remove it (only Eddie-manageable tap/dco adapters) so the following create succeeds.
		// With per-driver adapter names this should not happen anymore, hence the warning log.
		private bool RemoveConflictingDriverInterface(string driver, string ifaceName, Json jNetworkInfo)
		{
			string expectedDeviceId = "";
			if (driver == "tap-windows6")
				expectedDeviceId = "tap0901";
			else if (driver == "ovpn-dco")
				expectedDeviceId = "ovpn-dco";

			bool removed = false;

			foreach (Json jNetworkInterface in jNetworkInfo["interfaces"].Json.GetArray())
			{
				if (jNetworkInterface.HasKey("device_id") == false)
					continue;
				if (jNetworkInterface["type"].ValueString != "Virtual")
					continue;
				if (jNetworkInterface["name"].ValueString != ifaceName)
					continue;

				string deviceId = jNetworkInterface["device_id"].ValueString.ToLowerInvariant();

				bool isOpenVpnAdapter = deviceId.Contains("tap0901") || deviceId.Contains("ovpn-dco");
				if (isOpenVpnAdapter == false || deviceId.Contains(expectedDeviceId))
					continue;

				Engine.Instance.Logs.Log(LogType.Warning, "Removing pre-existing \"" + ifaceName + "\" interface of a different driver (" + jNetworkInterface["device_id"].ValueString + ") before creating the " + driver + " adapter.");
				Core.Engine.Instance.Elevated.DoCommandSync("network-adapter-delete", "id", jNetworkInterface["id"].ValueString);
				removed = true;
			}

			return removed;
		}

		public override bool OpenVpnCanUninstallDriver(string driver)
		{
			if (driver == "ovpn-dco" || driver == "tap-windows6")
				return GetDriverVersion(driver) != "";
			return false;
		}

		public override bool OpenVpnUninstallDriver(string driver)
		{
			if (driver != "ovpn-dco" && driver != "tap-windows6")
				return false;

			Core.Engine.Instance.Elevated.DoCommandSync("driver-uninstall", "driver", driver);
			System.Threading.Thread.Sleep(3000);
			return (GetDriverVersion(driver) == "");
		}

		public override int OpenVpnDeleteAllAdapters()
		{
			int nRemoved = 0;
			NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface adapter in adapters)
			{
				string description = adapter.Description.ToLowerInvariant();
				bool isOpenVpnAdapter = description.StartsWithInv("tap-win") || description.Contains("data channel offload");
				if (isOpenVpnAdapter)
				{
					Core.Engine.Instance.Elevated.DoCommandSync("network-adapter-delete", "id", adapter.Id);
					nRemoved++;
				}
			}
			return nRemoved;
		}

		// Specific

		public void HackWindowsInterfaceUp()
		{
			NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface adapter in interfaces)
			{
				if (adapter.Description.ToLowerInvariant().StartsWithInv("tap-win"))
				{
					Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText(LanguageItems.HackInterfaceUpDone, adapter.Name));

					Engine.Instance.Elevated.DoCommandSync("windows-workaround-interface-up", "name", adapter.Name);
				}
			}
		}

		private void DnsForceRestore()
		{
			try
			{
				// 2.13.3 : Win10 with ManagementObject & SetDNSServerSearchOrder sometime return errcode 84 "IP not enabled on adapter" if we try to set DNS on Tap in state "Network cable unplugged", typical in end of session.

				foreach (NetworkManagerDnsEntry entry in m_listOldDns)
				{
					string interfaceName = entry.Description;

					// IPv4
					if (entry.Layer == "IPv4")
					{
						if (entry.Dns.OnlyIPv4.Count == 0)
						{
							Engine.Instance.Elevated.DoCommandSync("windows-dns", "layer", "ipv4", "interface", interfaceName, "mode", "dhcp");
						}

						int nIPv4 = 0;
						foreach (IpAddress ip in entry.Dns.OnlyIPv4.IPs)
						{
							Engine.Instance.Elevated.DoCommandSync("windows-dns", "layer", "ipv4", "interface", interfaceName, "ipaddress", ip.Address, "mode", ((nIPv4 == 0) ? "static" : "add"));
							nIPv4++;
						}

						Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText(LanguageItems.OsWindowsNetworkAdapterDnsRestored, "IPv4", entry.Description, (entry.Dns.OnlyIPv4.Count == 0 ? "automatic" : entry.Dns.OnlyIPv4.ToString())));
					}

					// IPv6
					if (entry.Layer == "IPv6")
					{
						if (entry.Dns.OnlyIPv6.Count == 0)
						{
							Engine.Instance.Elevated.DoCommandSync("windows-dns", "layer", "ipv6", "interface", interfaceName, "mode", "dhcp");
						}

						int nIPv6 = 0;
						foreach (IpAddress ip in entry.Dns.OnlyIPv6.IPs)
						{
							Engine.Instance.Elevated.DoCommandSync("windows-dns", "layer", "ipv6", "interface", interfaceName, "ipaddress", ip.Address, "mode", ((nIPv6 == 0) ? "static" : "add"));
							nIPv6++;
						}

						Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText(LanguageItems.OsWindowsNetworkAdapterDnsRestored, "IPv6", entry.Description, (entry.Dns.OnlyIPv6.Count == 0 ? "automatic" : entry.Dns.OnlyIPv6.ToString())));
					}
				}
			}
			catch (Exception ex)
			{
				Engine.Instance.Logs.Log(ex);
			}

			m_listOldDns.Clear();
		}

		private NetworkInterface GetNetworkInterfaceFromGuid(string guid)
		{
			NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface i in interfaces)
			{
				if (i.Id == guid)
					return i;
			}
			return null;
		}

		private string NormalizeWinSysPath(string path)
		{
			string sysPath = path;

			if (sysPath.StartsWithInv("\\SystemRoot\\")) // Win 8 and above
			{
				sysPath = Platform.Instance.NormalizePath(sysPath.Replace("\\SystemRoot\\", Environment.GetEnvironmentVariable("windir") + Platform.Instance.DirSep));
			}
			else // Relative path, Win 7 and below
			{
				sysPath = Platform.Instance.NormalizePath(Environment.GetEnvironmentVariable("windir") + Platform.Instance.DirSep + sysPath);
			}

			if ((GetProcessArchitecture() == "x86") && (GetOsArchitecture() == "x64") && (IsVistaOrNewer()))
			{
				// If Eddie is compiled for 32 bit, and architecture is 64 bit, 
				// tunnel driver path above is real, but Redirector cause issue.
				// https://msdn.microsoft.com/en-us/library/aa384187(v=vs.85).aspx
				// The Sysnative alias was added starting with Windows Vista.
				sysPath = sysPath.ToLowerInvariant();
				sysPath = sysPath.Replace("\\system32\\", "\\sysnative\\");
			}
			return sysPath;
		}

		private string GetDriverSysServiceVersion(string driver)
		{
			string sysPath = "";

			object objSysPath = Registry.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\" + driver, "ImagePath", "");
			if (objSysPath != null)
				sysPath = objSysPath as string;

			if (sysPath != "")
			{
				sysPath = NormalizeWinSysPath(sysPath);

				if (Platform.Instance.FileExists(sysPath))
				{
					// GetVersionInfo may throw a FileNotFound exception between 32bit/64bit SO/App.
					FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(sysPath);

					string result = versionInfo.ProductVersion;
					if (result != null)
					{
						if (result.IndexOfInv(" ") != -1)
							result = result.Substring(0, result.IndexOfInv(" "));

						return result;
					}
				}
			}

			return "";
		}
		
	}

	public class NetworkManagerDnsEntry
	{
		public string Guid;
		public string Description;
		public string Layer;
		public IpAddresses Dns = new IpAddresses();

		public void ReadXML(XmlElement node)
		{
			Guid = node.GetAttributeString("guid", "");
			Description = node.GetAttributeString("description", "");
			Layer = node.GetAttributeString("layer", "");
			Dns.Set(node.GetAttributeString("dns", ""));
		}

		public void WriteXML(XmlElement node)
		{
			node.SetAttributeString("guid", Guid);
			node.SetAttributeString("description", Description);
			node.SetAttributeString("layer", Layer);
			node.SetAttributeString("dns", Dns.ToString());
		}
	}
}

#pragma warning restore CA1416

