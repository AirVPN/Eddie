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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.NetworkInformation;
using System.Management;
using System.Security.Principal;
using System.Xml;
using System.Text;
using System.Threading;
using Eddie.Common;
using Eddie.Core;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;

namespace Eddie.Platform.Windows
{
	public class Platform : Core.Platform
	{
		private List<NetworkManagerDhcpEntry> m_listOldDhcp = new List<NetworkManagerDhcpEntry>();
		private List<NetworkManagerDnsEntry> m_listOldDns = new List<NetworkManagerDnsEntry>();
		private object m_oldIPv6 = null;
		private string m_oldMetricInterface = "";
		private int m_oldMetricIPv4 = -1;
		private int m_oldMetricIPv6 = -1;
		private Mutex m_mutexSingleInstance = null;
		private NativeMethods.ConsoleCtrlHandlerRoutine m_consoleCtrlHandlerRoutine;

		public static bool IsVistaOrNewer()
		{
			OperatingSystem OS = Environment.OSVersion;
			return (OS.Platform == PlatformID.Win32NT) && (OS.Version.Major >= 6);
		}

		public static bool IsWin7OrNewer()
		{
			OperatingSystem OS = Environment.OSVersion;
			return OS.Platform == PlatformID.Win32NT && (OS.Version.Major > 6 || (OS.Version.Major == 6 && OS.Version.Minor >= 1));
		}

		public static bool IsWin8OrNewer()
		{
			OperatingSystem OS = Environment.OSVersion;
			return OS.Platform == PlatformID.Win32NT && (OS.Version.Major > 6 || (OS.Version.Major == 6 && OS.Version.Minor >= 2));
		}

		public static bool IsWin10OrNewer()
		{
			OperatingSystem OS = Environment.OSVersion;
			return (OS.Platform == PlatformID.Win32NT) && (OS.Version.Major >= 10);
		}

		// Override
		public Platform()
		{
			/*
#if EDDIENET20
			// Look the comment in TrustCertificatePolicy.cs
			TrustCertificatePolicy.Activate();
#endif
			*/
		}

		public override string GetCode()
		{
			return "Windows";
		}

		public override string GetName()
		{
			return Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "ProductName", "").ToString();
		}

		public override void OnInit(bool cli)
		{
			base.OnInit(cli);

			m_consoleCtrlHandlerRoutine = new NativeMethods.ConsoleCtrlHandlerRoutine(ConsoleCtrlCheck); // Avoid Garbage Collector
			NativeMethods.SetConsoleCtrlHandler(m_consoleCtrlHandlerRoutine, true);
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

		public override string GetOsArchitecture()
		{
			if (GetArchitecture() == "x64")
				return "x64";
			if (NativeMethods.InternalCheckIsWow64())
				return "x64";
			return "x86";
		}

		public override bool IsAdmin()
		{
			//return true; // Manifest ensure that

			// 2.10.1
			bool isElevated;
			WindowsIdentity identity = WindowsIdentity.GetCurrent();
			WindowsPrincipal principal = new WindowsPrincipal(identity);
			isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);

			return isElevated;
		}

		public override bool GetAutoStart()
		{
			TaskService ts = null;
			try
			{
				ts = new TaskService();
				if (ts.RootFolder.Tasks.Exists("AirVPN"))
					return true;
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

			return false;
		}
		public override bool SetAutoStart(bool value)
		{
			TaskService ts = null;
			try
			{
				ts = new TaskService();
				if (value == false)
				{
					if (ts.RootFolder.Tasks.Exists("AirVPN"))
						ts.RootFolder.DeleteTask("AirVPN");
				}
				else
				{
					if (ts.RootFolder.Tasks.Exists("AirVPN"))
						ts.RootFolder.DeleteTask("AirVPN");

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

					td.RegistrationInfo.Description = "AirVPN Client";
					td.Triggers.Add(new LogonTrigger());
					string command = "\"" + GetExecutablePath() + "\"";
					string arguments = "";
					if (Engine.Instance.Storage.Get("path") != "")
						arguments = "-path=" + Engine.Instance.Storage.Get("path");
					td.Actions.Add(new ExecAction(command, (arguments == "") ? null : arguments, null));

					// Register the task in the root folder
					ts.RootFolder.RegisterTaskDefinition(@"AirVPN", td);
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
		}

		public override string NormalizeString(string val)
		{
			return val.Replace("\r\n", "\n").Replace("\n", "\r\n");
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

		public override bool NativeInit()
		{
			return (NativeMethods.Init() == 0);
		}

		public override string FileGetPhysicalPath(string path)
		{
			string native = NativeMethods.GetFinalPathName(path);
			if (native.StartsWith("\\\\?\\"))
				native = native.Substring(4);
			if (native.StartsWith("UNC\\"))
				native = "\\" + native.Substring(3);
			return native;
		}

		public override string GetExecutablePathEx()
		{
			// It return vshost.exe under VS, better
			string path = Environment.GetCommandLineArgs()[0];
			path = Path.GetFullPath(path); // 2.11.9
			return path;
		}

		public override string GetUserPathEx()
		{
			return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\AirVPN";
		}

		public override string GetDefaultOpenVpnConfigsPath()
		{
			object v = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\OpenVPN", "config_dir", "");
			if (v == null)
				return "";
			else
				return Conversions.ToString(v);
		}

		private bool ConsoleCtrlCheck(NativeMethods.CtrlTypes ctrlType)
		{
			switch (ctrlType)
			{
				case NativeMethods.CtrlTypes.CTRL_C_EVENT:
					Engine.Instance.OnSignal("C");
					break;
				case NativeMethods.CtrlTypes.CTRL_BREAK_EVENT:
					Engine.Instance.OnSignal("BREAK");
					break;
				case NativeMethods.CtrlTypes.CTRL_CLOSE_EVENT:
					Engine.Instance.OnSignal("CLOSE");
					break;
				case NativeMethods.CtrlTypes.CTRL_LOGOFF_EVENT:
					Engine.Instance.OnSignal("LOGOFF");
					break;
				case NativeMethods.CtrlTypes.CTRL_SHUTDOWN_EVENT:
					Engine.Instance.OnSignal("SHUTDOWN");
					break;
				default:
					break;
			}

			return true;
		}

		public override void ShellCommandDirect(string command, out string path, out string[] arguments)
		{
			path = "cmd.exe";
			arguments = new string[] { "/c", command };
		}

		public override bool ProcessKillSoft(Core.Process process)
		{
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
		}

		public override int GetRecommendedRcvBufDirective()
		{
			return 256 * 1024;
		}

		public override int GetRecommendedSndBufDirective()
		{
			return 256 * 1024;
		}

		public override void FlushDNS()
		{
			base.FlushDNS();

			// <2.11.8
			//ShellCmd("ipconfig /flushdns");

			// 2.11.10
			if (Engine.Instance.Storage.GetBool("windows.workarounds"))
			{
				SystemShell.ShellCmd("net stop dnscache");
				SystemShell.ShellCmd("net start dnscache");
			}

			SystemShell.ShellCmd("ipconfig /flushdns");
			SystemShell.ShellCmd("ipconfig /registerdns");
		}

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
				NetworkInterface networkInterface = GetNetworkInterfaceFromGuid(jRoute["interface"].Value as string);

				if (networkInterface == null)
					throw new Exception(Messages.NetworkInterfaceNotAvailable);

				string cmd = "";
				if (ip.IsV4)
				{
					cmd += "route add " + ip.Address + " mask " + ip.Mask + " " + gateway.Address;
					int interfaceIdx = networkInterface.GetIPProperties().GetIPv4Properties().Index;
					cmd += " if " + SystemShell.EscapeInt(interfaceIdx);
					/*
					 * Metric param are ignored or misinterpreted. http://serverfault.com/questions/238695/how-can-i-set-the-metric-of-a-manually-added-route-on-windows
					if(r.Metrics != "")
						cmd += " metric " + r.Metrics;
					*/
				}
				else if (ip.IsV6)
				{
					cmd = "netsh interface ipv6 add route";
					cmd += " prefix=\"" + SystemShell.EscapeInsideQuote(ip.ToCIDR(true)) + "\"";
					int interfaceIdx = networkInterface.GetIPProperties().GetIPv6Properties().Index;
					cmd += " interface=\"" + SystemShell.EscapeInt(interfaceIdx) + "\"";
					cmd += " nexthop=\"" + SystemShell.EscapeInsideQuote(gateway.Address) + "\"";
					if (jRoute.HasKey("metric"))
						cmd += " metric=" + SystemShell.EscapeInt(jRoute["metric"].Value as string);
				}

				string result = SystemShell.ShellCmd(cmd);
				result = result.Trim().Trim(new char[] { '!', '.' });
				if (result.ToLowerInvariant() == "ok")
				{
					return base.RouteAdd(jRoute);
				}
				else
				{
					throw new Exception(result);
				}
			}
			catch(Exception e)
			{
				Engine.Instance.Logs.LogWarning(MessagesFormatter.Format(Messages.RouteAddFailed, ip.ToCIDR(), gateway.ToCIDR(), e.Message));
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
				NetworkInterface networkInterface = GetNetworkInterfaceFromGuid(jRoute["interface"].Value as string);

				if (networkInterface == null)
					throw new Exception(Messages.NetworkInterfaceNotAvailable);

				string cmd = "";
				if (ip.IsV4)
				{
					cmd = "route delete " + ip.Address + " mask " + ip.Mask + " " + gateway.Address;
					int interfaceIdx = networkInterface.GetIPProperties().GetIPv4Properties().Index;
					cmd += " if " + SystemShell.EscapeInt(interfaceIdx);
				}
				else if (ip.IsV6)
				{
					cmd = "netsh interface ipv6 del route";
					cmd += " prefix=\"" + SystemShell.EscapeInsideQuote(ip.ToCIDR(true)) + "\"";
					int interfaceIdx = networkInterface.GetIPProperties().GetIPv6Properties().Index;
					cmd += " interface=\"" + SystemShell.EscapeInt(interfaceIdx) + "\"";
					cmd += " nexthop=\"" + SystemShell.EscapeInsideQuote(gateway.Address) + "\"";
				}

				string result = SystemShell.ShellCmd(cmd);
				result = result.Trim().Trim(new char[] { '!', '.' });
				if (result.ToLowerInvariant() == "ok")
				{
					return base.RouteRemove(jRoute);
				}
				else
				{
					// Remember: Route deletion can occur in a second moment (for example a Recovery phase).

					// Still accepted: The device are not available anymore, so the route are already deleted.
					if (result.ToLowerInvariant().Contains("the system cannot find the file specified"))
						return base.RouteRemove(jRoute);

					// Still accepted: Already deleted.
					if (result.ToLowerInvariant().Contains("element not found."))
						return base.RouteRemove(jRoute);

					// Unexpected/unknown error.
					throw new Exception(result);
				}
			}
			catch(Exception e)
			{
				Engine.Instance.Logs.LogVerbose(MessagesFormatter.Format(Messages.RouteDelFailed, ip.ToCIDR(), gateway.ToCIDR(), e.Message));
				return false;
			}
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
		
		public override bool WaitTunReady()
		{
			int tickStart = Environment.TickCount;
			string lastStatus = "";

			for (; ; )
			{
				NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
				foreach (NetworkInterface adapter in interfaces)
				{
					if (adapter.Description.ToLowerInvariant().StartsWith("tap-win"))
					{
						if (adapter.OperationalStatus == OperationalStatus.Up)
							return true;
						else
						{
							lastStatus = adapter.OperationalStatus.ToString();
						}
					}
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

			report.Add("ipconfig /all", SystemShell.ShellCmd("ipconfig /all"));
			//report.Add("route print", SystemShell.ShellCmd("route print"));

			/* // Too much data, generally useless
			t += "\n-- NetworkAdapterConfiguration\n";

			ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
			ManagementObjectCollection objMOC = objMC.GetInstances();

			foreach (ManagementObject objMO in objMOC)
			{	
				t += "\n";
				t += "Network Adapter: " + Conversions.ToString(objMO.Properties["Caption"].Value) + "\n";
				t += "DNS: " + Conversions.ToString(objMO.Properties["DNSServerSearchOrder"].Value) + "\n";
				
				t += "Details:\n";
				foreach (PropertyData prop in objMO.Properties)
				{
					t += "\t" + prop.Name + ": " + Conversions.ToString(prop.Value) + "\n";					
				}				
			}
			*/
		}

		public override bool OnCheckSingleInstance()
		{
			m_mutexSingleInstance = new Mutex(false, "Global\\" + "b57887e0-65d0-4d18-b57f-106de6e0f1b6");
			if (m_mutexSingleInstance.WaitOne(0, false) == false)
				return false;
			else
				return true;
		}
		
		public override void OnNetworkLockManagerInit()
		{
			base.OnNetworkLockManagerInit();

			if (IsVistaOrNewer()) // 2.10.1
			{
				Engine.Instance.NetworkLockManager.AddPlugin(new NetworkLockWfp());
				Engine.Instance.NetworkLockManager.AddPlugin(new NetworkLockWindowsFirewall());
			}
		}

		public override string OnNetworkLockRecommendedMode()
		{
			if (IsVistaOrNewer() == false)
				return "none";

			if (Engine.Instance.Storage.GetBool("windows.wfp.enable"))
				return "windows_wfp";
			else
				return "windows_firewall";
		}

		public override void OnSessionStart()
		{
			// https://airvpn.org/topic/11162-airvpn-client-advanced-features/ -> Switch DHCP to Static			
			if (Engine.Instance.Storage.GetBool("windows.dhcp_disable"))
				SwitchToStaticDo();

			// FlushDNS(); // Removed in 2.11.10

			Recovery.Save();
		}

		public override void OnSessionStop()
		{
			SwitchToStaticRestore();

			// FlushDNS(); // Moved in 2.13.3 in ::session.cs, but only if a connection occur.

			Recovery.Save();
		}

		public override bool OnIPv6Block()
		{
			if ((IsVistaOrNewer()) && (Engine.Instance.Storage.GetBool("windows.wfp.enable")))
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

				// <2.12.1
				/*
				XmlDocument xmlDocRule = new XmlDocument();
				XmlElement xmlRule = xmlDocRule.CreateElement("rule");
				xmlRule.SetAttribute("name", "IPv6 - Block");
				xmlRule.SetAttribute("layer", "ipv6");
				xmlRule.SetAttribute("action", "block");
				xmlRule.SetAttribute("weight", "3000"); 
				XmlElement XmlIf1 = xmlDocRule.CreateElement("if");
				xmlRule.AppendChild(XmlIf1);
				XmlIf1.SetAttribute("field", "ip_local_interface");
				XmlIf1.SetAttribute("match", "not_equal");
				XmlIf1.SetAttribute("interface", "loopback");
				Wfp.AddItem("ipv6_block_all", xmlRule);
				*/

				Engine.Instance.Logs.Log(LogType.Verbose, Messages.IPv6DisabledWpf);
			}

			// Removed in 2.14, in W10 require reboot
			/*
			if (Engine.Instance.Storage.GetBool("windows.ipv6.os_disable"))
			{
				// Based on: http://support.microsoft.com/kb/929852
				// Based on: https://www.wincert.net/networking/ipv6-breaking-down-the-disabledcomponents-registry-value/
				object reg = Registry.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\services\\TCPIP6\\Parameters", "DisabledComponents", "");
				UInt32 v = Conversions.ToUInt32(reg, 0);
				bool available = true;
				if ((v & (1 << 0)) != 0) // Bit 0 controls ALL of the IPv6 tunnel interfaces. If 1 is disabled
					available = false;
				if ((v & (1 << 4)) != 0) // Bit 4 controls IPv6 for non-tunnel interfaces. If 1 is disabled
					available = false;

				if(available == false)
				{
					m_oldIPv6 = null;
				}
				else
				{
					m_oldIPv6 = v;

					UInt32 newValue = 17;
					Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\services\\TCPIP6\\Parameters", "DisabledComponents", newValue, RegistryValueKind.DWord);

					Engine.Instance.Logs.Log(LogType.Verbose, Messages.IPv6DisabledOs);

					Recovery.Save();
				}
			}
			*/

			base.OnIPv6Block();

			return true;
		}

		public override bool OnIPv6Restore()
		{
			if (m_oldIPv6 != null)
			{
				Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\services\\TCPIP6\\Parameters", "DisabledComponents", m_oldIPv6, RegistryValueKind.DWord);
				m_oldIPv6 = null;

				Engine.Instance.Logs.Log(LogType.Verbose, Messages.IPv6RestoredOs);
			}

			if ((Wfp.RemoveItem("ipv6_block_all")) && (Wfp.RemoveItem("ipv6_allow_loopback")))
				Engine.Instance.Logs.Log(LogType.Verbose, Messages.IPv6RestoredWpf);

			base.OnIPv6Restore();

			return true;
		}

		public override void OnBuildOvpn(OvpnBuilder ovpn)
		{
			base.OnBuildOvpn(ovpn);

			if (Engine.Instance.Storage.GetBool("windows.ipv6.bypass_dns"))
			{
				ovpn.AppendDirectives("pull-filter ignore \"dhcp-option DNS6\"", "OS");
			}
		}

		public override bool OnDnsSwitchDo(ConnectionActive connectionActive, IpAddresses dns)
		{
			if ((Engine.Instance.Storage.GetBool("windows.dns.lock")) && (IsVistaOrNewer()) && (Engine.Instance.Storage.GetBool("windows.wfp.enable")))
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

				// This is required if OpenVPN itself perform DNS resolution
				{
					XmlDocument xmlDocRule = new XmlDocument();
					XmlElement xmlRule = xmlDocRule.CreateElement("rule");
					xmlRule.SetAttribute("name", "Dns - Allow port 53 of OpenVPN");
					xmlRule.SetAttribute("layer", "all-out");
					xmlRule.SetAttribute("action", "permit");
					xmlRule.SetAttribute("weight", "2000");
					XmlElement XmlIf1 = xmlDocRule.CreateElement("if");
					xmlRule.AppendChild(XmlIf1);
					XmlIf1.SetAttribute("field", "ip_remote_port");
					XmlIf1.SetAttribute("match", "equal");
					XmlIf1.SetAttribute("port", "53");
					XmlElement XmlIf2 = xmlDocRule.CreateElement("if");
					xmlRule.AppendChild(XmlIf2);
					XmlIf2.SetAttribute("field", "ale_app_id");
					XmlIf2.SetAttribute("match", "equal");
					XmlIf2.SetAttribute("path", Software.GetTool("openvpn").Path);
					Wfp.AddItem("dns_permit_openvpn", xmlRule);
				}

				{
					// Remember: This because may fail at WFP side with a "Unknown interface" because network interface with IPv4/IPv6 disabled have Ipv6IfIndex == 0 and don't match the requested interface.
					string layer = "all-out";
					if ((connectionActive.TunnelIPv6 == false) && (layer == "all-out"))
						layer = "ipv4-out";
					if ((connectionActive.BlockedIPv6) && (layer == "all-out"))
						layer = "ipv4-out";
					if ((connectionActive.TunnelIPv4 == false) && (layer == "all-out"))
						layer = "ipv6-out";
					if ((connectionActive.BlockedIPv4) && (layer == "all-out"))
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
						XmlIf2.SetAttribute("interface", Engine.Instance.ConnectionActive.InterfaceId);
						Wfp.AddItem("dns_permit_tap", xmlRule);
					}
				}


				Engine.Instance.Logs.Log(LogType.Verbose, Messages.DnsLockActivatedWpf);
			}

			string mode = Engine.Instance.Storage.GetLower("dns.mode");

			if (mode == "auto")
			{
				Json jNetworkInfo = Engine.Instance.JsonNetworkInfo(); // Realtime
				foreach (Json jNetworkInterface in jNetworkInfo["interfaces"].Json.GetArray())
				{
					if (jNetworkInterface["type"].Value as string != "Ethernet")
						continue;

					string id = jNetworkInterface["id"].Value as string;
					string interfaceName = jNetworkInterface["name"].Value as string;

					bool skip = true;

					// <2.14.0
					//if ((Engine.Instance.Storage.GetBool("windows.dns.lock")) && (Engine.Instance.Storage.GetBool("windows.dns.force_all_interfaces")))
					if (Engine.Instance.Storage.GetBool("windows.dns.force_all_interfaces"))
						skip = false;
					if ((Engine.Instance.ConnectionActive != null) && (id == Engine.Instance.ConnectionActive.InterfaceId))
						skip = false;

					if (skip == false)
					{
						/*
						NetworkManagerDnsEntry entry = new NetworkManagerDnsEntry();

						entry.Guid = id;
						entry.Description = interfaceName;
						entry.Dns.Add(jNetworkInterface["dns4"].Value as string);
						entry.Dns.Add(jNetworkInterface["dns6"].Value as string);

						if (entry.Dns.Equals(dns))
							continue;
						*/

						// TOCLEAN
						//entry.AutoDns = ((Registry.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces\\" + entry.Guid, "NameServer", "") as string) == "");

						/*
						if (entry.AutoDns == false) // Added 2.11
						{
							if (dns.Equals(dnsStatic))
								continue;
						}
						*/

						// IPv4
						IpAddresses dnsIPv4 = new IpAddresses(jNetworkInterface["dns4"].Value as string);
						if (dnsIPv4.OnlyIPv4.Equals(dns.OnlyIPv4) == false)
						{
							NetworkManagerDnsEntry entry = new NetworkManagerDnsEntry();
							entry.Guid = id;
							entry.Description = interfaceName;
							entry.Layer = "IPv4";
							entry.Dns = dnsIPv4;

							if (dns.OnlyIPv4.Count == 0)
							{
								SystemShell.ShellCmd("netsh interface ipv4 set dns name=\"" + SystemShell.EscapeInsideQuote(interfaceName) + "\" source=dhcp register=primary validate=no");
							}

							int nIPv4 = 0;
							foreach (IpAddress ip in dns.OnlyIPv4.IPs)
							{
								if (nIPv4 == 0)
								{
									SystemShell.ShellCmd("netsh interface ipv4 set dns name=\"" + SystemShell.EscapeInsideQuote(interfaceName) + "\" source=static address=" + ip.Address + " register=primary validate=no");
								}
								else
								{
									SystemShell.ShellCmd("netsh interface ipv4 add dnsserver name=\"" + SystemShell.EscapeInsideQuote(interfaceName) + "\" address=" + ip.Address + " validate=no");
								}
								nIPv4++;
							}

							m_listOldDns.Add(entry);
							Engine.Instance.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.OsWindowsNetworkAdapterDnsDone, "IPv4", entry.Description, (entry.Dns.OnlyIPv4.Count == 0 ? "automatic" : "manual" + " (" + entry.Dns.OnlyIPv4.ToString() + ")"), dns.OnlyIPv4.ToString()));
						}
												
						IpAddresses dnsIPv6 = new IpAddresses(jNetworkInterface["dns6"].Value as string);
						if (dnsIPv6.OnlyIPv6.Equals(dns.OnlyIPv6) == false)
						{
							NetworkManagerDnsEntry entry = new NetworkManagerDnsEntry();
							entry.Guid = id;
							entry.Description = interfaceName;
							entry.Layer = "IPv6";
							entry.Dns = dnsIPv6;

							if (dns.OnlyIPv6.Count == 0)
							{
								SystemShell.ShellCmd("netsh interface ipv6 set dns name=\"" + SystemShell.EscapeInsideQuote(interfaceName) + "\" source=dhcp register=primary validate=no");
							}

							int nIPv6 = 0;
							foreach (IpAddress ip in dns.OnlyIPv6.IPs)
							{
								if (nIPv6 == 0)
								{
									SystemShell.ShellCmd("netsh interface ipv6 set dns name=\"" + SystemShell.EscapeInsideQuote(interfaceName) + "\" source=static address=" + ip.Address + " register=primary validate=no");
								}
								else
								{
									SystemShell.ShellCmd("netsh interface ipv6 add dnsserver name=\"" + SystemShell.EscapeInsideQuote(interfaceName) + "\" address=" + ip.Address + " validate=no");
								}
								nIPv6++;
							}

							m_listOldDns.Add(entry);
							Engine.Instance.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.OsWindowsNetworkAdapterDnsDone, "IPv6", entry.Description, (entry.Dns.OnlyIPv6.Count == 0 ? "automatic" : "manual" + " (" + entry.Dns.OnlyIPv6.ToString() + ")"), dns.OnlyIPv6.ToString()));
						}
					}
				}

				/* <2.14.0
				try
				{
					ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
					ManagementObjectCollection objMOC = objMC.GetInstances();

					foreach (ManagementObject objMO in objMOC)
					{
						string guid = objMO["SettingID"] as string;

						bool skip = true;

						if((Engine.Instance.Storage.GetBool("windows.dns.lock")) && (Engine.Instance.Storage.GetBool("windows.dns.force_all_interfaces")) )
							skip = false;
						if( (Engine.Instance.ConnectionActive != null) && (guid == Engine.Instance.ConnectionActive.InterfaceId) )
							skip = false;

						if (skip == false)
						{
							bool ipEnabled = (bool)objMO["IPEnabled"];

							NetworkManagerDnsEntry entry = new NetworkManagerDnsEntry();

							entry.Guid = guid;
							entry.Description = objMO["Description"] as string;
							entry.Dns = objMO["DNSServerSearchOrder"] as string[];

							entry.AutoDns = ((Registry.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces\\" + entry.Guid, "NameServer", "") as string) == "");

							if (entry.Dns == null)
							{
								continue;
							}

							if (entry.AutoDns == false) // Added 2.11
							{
								if (dns.Equals(new IpAddresses(entry.Dns)))
									continue;
							}

							bool done = false;
							
							// 2.13.3 : Win10 with ManagementObject & SetDNSServerSearchOrder sometime return errcode 84 "IP not enabled on adapter" if we try to set DNS on Tap in state "Network cable unplugged", typical in end of session.
							if (IsWin10OrNewer())
							{
								NetworkInterface inet = GetNetworkInterfaceFromGuid(guid);
								if(inet != null)
								{
									string interfaceName = inet.Name;									
									int nIPv4 = 0;
									int nIPv6 = 0;
									foreach (IpAddress ip in dns.IPs)
									{
										if (ip.IsV4)
										{
											if(nIPv4 == 0)
											{
												SystemShell.ShellCmd("netsh interface ipv4 set dns name=\"" + SystemShell.EscapeInsideQuote(interfaceName) + "\" source=static address=" + ip.Address + " register=primary validate=no");
											}
											else
											{
												SystemShell.ShellCmd("netsh interface ipv4 add dnsserver name=\"" + SystemShell.EscapeInsideQuote(interfaceName) + "\" address=" + ip.Address + " validate=no");
											}
											nIPv4++;
										} else if(ip.IsV6)
										{
											if (nIPv6 == 0)
											{
												SystemShell.ShellCmd("netsh interface ipv6 set dns name=\"" + SystemShell.EscapeInsideQuote(interfaceName) + "\" source=static address=" + ip.Address + " register=primary validate=no");
											}
											else
											{
												SystemShell.ShellCmd("netsh interface ipv6 add dnsserver name=\"" + SystemShell.EscapeInsideQuote(interfaceName) + "\" address=" + ip.Address + " validate=no");
											}
											nIPv6++;
										}
									}
									done = true;
								}
							}
							else
							{
								ManagementBaseObject objSetDNSServerSearchOrder = objMO.GetMethodParameters("SetDNSServerSearchOrder");
								objSetDNSServerSearchOrder["DNSServerSearchOrder"] = dns.AddressesToStringArray();
								objMO.InvokeMethod("SetDNSServerSearchOrder", objSetDNSServerSearchOrder, null);
								done = true;
							}

							if (done)
							{
								m_listOldDns.Add(entry);

								string descFrom = (entry.AutoDns ? "automatic" : "manual") + " (" + String.Join(",", entry.Dns) + ")";
								Engine.Instance.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.NetworkAdapterDnsDone, entry.Description, descFrom, dns.Addresses));
							}							
						}
					}
				}
				catch (Exception e)
				{
					Engine.Instance.Logs.Log(e);
				}
				*/


				Recovery.Save();
			}

			base.OnDnsSwitchDo(connectionActive, dns);

			return true;
		}

		public override bool OnDnsSwitchRestore()
		{
			DnsForceRestore();

			bool DnsPermitExists = false;
			DnsPermitExists = DnsPermitExists | Wfp.RemoveItem("dns_permit_openvpn");
			DnsPermitExists = DnsPermitExists | Wfp.RemoveItem("dns_permit_tap");
			DnsPermitExists = DnsPermitExists | Wfp.RemoveItem("dns_block_all");
			if (DnsPermitExists)
				Engine.Instance.Logs.Log(LogType.Verbose, Messages.DnsLockDeactivatedWpf);

			base.OnDnsSwitchRestore();

			return true;
		}

		public override bool OnInterfaceDo(string id)
		{
			int interfaceMetricIPv4Value = Engine.Instance.Storage.GetInt("windows.metrics.tap.ipv4");
			int interfaceMetricIPv6Value = Engine.Instance.Storage.GetInt("windows.metrics.tap.ipv6");
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
			foreach (NetworkInterface adapter in interfaces)
			{
				if (adapter.Id == id)
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
				string fromStr = (interfaceMetricIPv4Current == 0) ? "Automatic" : interfaceMetricIPv4Current.ToString();
				string toStr = (interfaceMetricIPv4Value == 0) ? "Automatic" : interfaceMetricIPv4Value.ToString();
				Engine.Instance.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.NetworkAdapterMetricSwitch, interfaceMetricIPv4Name, fromStr, toStr, "IPv4"));
				m_oldMetricInterface = id;
				m_oldMetricIPv4 = interfaceMetricIPv4Current;
				NativeMethods.SetInterfaceMetric(interfaceMetricIPv4Idx, "ipv4", interfaceMetricIPv4Value);

				Recovery.Save();
			}

			if ((interfaceMetricIPv6Current != -1) && (interfaceMetricIPv6Current != interfaceMetricIPv6Value))
			{
				string fromStr = (interfaceMetricIPv6Current == 0) ? "Automatic" : interfaceMetricIPv6Current.ToString();
				string toStr = (interfaceMetricIPv6Value == 0) ? "Automatic" : interfaceMetricIPv6Value.ToString();
				Engine.Instance.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.NetworkAdapterMetricSwitch, interfaceMetricIPv6Name, fromStr, toStr, "IPv6"));
				m_oldMetricInterface = id;
				m_oldMetricIPv6 = interfaceMetricIPv6Current;
				NativeMethods.SetInterfaceMetric(interfaceMetricIPv6Idx, "ipv6", interfaceMetricIPv6Value);

				Recovery.Save();
			}

			return base.OnInterfaceDo(id);
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
								string fromStr = (current == 0) ? "Automatic" : current.ToString();
								string toStr = (m_oldMetricIPv4 == 0) ? "Automatic" : m_oldMetricIPv4.ToString();
								Engine.Instance.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.NetworkAdapterMetricRestore, adapter.Name, fromStr, toStr, "IPv4"));
								NativeMethods.SetInterfaceMetric(idx, "ipv4", m_oldMetricIPv4);
							}
							m_oldMetricIPv4 = -1;
						}

						if (m_oldMetricIPv6 != -1)
						{
							int idx = adapter.GetIPProperties().GetIPv6Properties().Index;
							int current = NativeMethods.GetInterfaceMetric(idx, "ipv6");
							if (current != m_oldMetricIPv6)
							{
								string fromStr = (current == 0) ? "Automatic" : current.ToString();
								string toStr = (m_oldMetricIPv6 == 0) ? "Automatic" : m_oldMetricIPv6.ToString();
								Engine.Instance.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.NetworkAdapterMetricRestore, adapter.Name, fromStr, toStr, "IPv6"));
								NativeMethods.SetInterfaceMetric(idx, "ipv6", m_oldMetricIPv6);
							}
							m_oldMetricIPv6 = -1;
						}

						m_oldMetricInterface = "";
						break;
					}
				}
			}
			return base.OnInterfaceRestore();
		}

		public override void OnDaemonOutput(string source, string message)
		{
			if (message.IndexOf("Waiting for TUN/TAP interface to come up") != -1)
			{
				if (Engine.Instance.Storage.GetBool("windows.tap_up"))
				{
					HackWindowsInterfaceUp();
				}
			}
		}

		public override void OnRecovery()
		{
			base.OnRecovery();

			if (IsWin7OrNewer()) // 2.12.2
				if (Wfp.ClearPendingRules())
					Engine.Instance.Logs.Log(LogType.Warning, Messages.WfpRecovery);
		}

		public override void OnRecoveryLoad(XmlElement root)
		{
			XmlElement nodeDhcp = UtilsXml.XmlGetFirstElementByTagName(root, "DhcpSwitch");
			if (nodeDhcp != null)
			{
				foreach (XmlElement nodeEntry in nodeDhcp.ChildNodes)
				{
					NetworkManagerDhcpEntry entry = new NetworkManagerDhcpEntry();
					entry.ReadXML(nodeEntry);
					m_listOldDhcp.Add(entry);
				}
			}

			XmlElement nodeDns = UtilsXml.XmlGetFirstElementByTagName(root, "DnsSwitch");
			if (nodeDns != null)
			{
				foreach (XmlElement nodeEntry in nodeDns.ChildNodes)
				{
					NetworkManagerDnsEntry entry = new NetworkManagerDnsEntry();
					entry.ReadXML(nodeEntry);
					m_listOldDns.Add(entry);
				}
			}

			if (UtilsXml.XmlExistsAttribute(root, "IPv6"))
			{
				m_oldIPv6 = Conversions.ToUInt32(UtilsXml.XmlGetAttributeInt64(root, "IPv6", 0), 0);
			}

			if (UtilsXml.XmlExistsAttribute(root, "interface-metric-id"))
			{
				m_oldMetricInterface = UtilsXml.XmlGetAttributeString(root, "interface-metric-id", "");
				m_oldMetricIPv4 = UtilsXml.XmlGetAttributeInt(root, "interface-metric-ipv4", -1);
				m_oldMetricIPv6 = UtilsXml.XmlGetAttributeInt(root, "interface-metric-ipv6", -1);
			}

			SwitchToStaticRestore();

			base.OnRecoveryLoad(root);
		}

		public override void OnRecoverySave(XmlElement root)
		{
			base.OnRecoverySave(root);

			XmlDocument doc = root.OwnerDocument;

			if (m_listOldDhcp.Count != 0)
			{
				XmlElement nodeDhcp = (XmlElement)root.AppendChild(doc.CreateElement("DhcpSwitch"));
				foreach (NetworkManagerDhcpEntry entry in m_listOldDhcp)
				{
					XmlElement nodeEntry = nodeDhcp.AppendChild(doc.CreateElement("entry")) as XmlElement;
					entry.WriteXML(nodeEntry);
				}
			}

			if (m_listOldDns.Count != 0)
			{
				XmlElement nodeDns = (XmlElement)root.AppendChild(doc.CreateElement("DnsSwitch"));
				foreach (NetworkManagerDnsEntry entry in m_listOldDns)
				{
					XmlElement nodeEntry = nodeDns.AppendChild(doc.CreateElement("entry")) as XmlElement;
					entry.WriteXML(nodeEntry);
				}
			}

			if (m_oldIPv6 != null)
				UtilsXml.XmlSetAttributeInt64(root, "IPv6", Conversions.ToInt64(m_oldIPv6));

			if (m_oldMetricInterface != "")
			{
				UtilsXml.XmlSetAttributeString(root, "interface-metric-id", m_oldMetricInterface);
				UtilsXml.XmlSetAttributeInt(root, "interface-metric-ipv4", m_oldMetricIPv4);
				UtilsXml.XmlSetAttributeInt(root, "interface-metric-ipv6", m_oldMetricIPv6);
			}
		}

		private string GetDriverUninstallPath()
		{
			// Note: 32 bit uninstaller can't be viewed by 64 bit app and viceversa.
			// http://www.rhyous.com/2011/01/24/how-read-the-64-bit-registry-from-a-32-bit-application-or-vice-versa/

			object objUninstallPath = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\TAP-Windows", "UninstallString", "");
			if (objUninstallPath != null)
			{
				string uninstallPath = objUninstallPath as string;

				if (Platform.Instance.FileExists(uninstallPath))
					return uninstallPath;
			}

			return "";
		}

		private string GetDriverVersion()
		{
			try
			{
				string regPath = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\services\\" + Engine.Instance.Storage.Get("windows.adapter_service");

				object objSysPath = Registry.GetValue(regPath, "ImagePath", "");
				if (objSysPath != null)
				{
					string sysPath = objSysPath as string;

					if (sysPath.StartsWith("\\SystemRoot\\")) // Win 8 and above
					{
						sysPath = Platform.Instance.NormalizePath(sysPath.Replace("\\SystemRoot\\", Environment.GetEnvironmentVariable("windir") + Platform.Instance.DirSep));
					}
					else // Relative path, Win 7 and below
					{
						sysPath = Platform.Instance.NormalizePath(Environment.GetEnvironmentVariable("windir") + Platform.Instance.DirSep + sysPath);
					}

					if ((GetArchitecture() == "x86") && (GetOsArchitecture() == "x64") && (IsVistaOrNewer()))
					{
						// If Eddie is compiled for 32 bit, and architecture is 64 bit, 
						// tunnel driver path above is real, but Redirector
						// https://msdn.microsoft.com/en-us/library/aa384187(v=vs.85).aspx
						// cause issue.
						// The Sysnative alias was added starting with Windows Vista.
						sysPath = sysPath.ToLowerInvariant();
						sysPath = sysPath.Replace("\\system32\\", "\\sysnative\\");
					}

					if (Platform.Instance.FileExists(sysPath) == false)
					{
						throw new Exception(MessagesFormatter.Format(Messages.OsDriverNoPath, sysPath));
					}

					// GetVersionInfo may throw a FileNotFound exception between 32bit/64bit SO/App.
					FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(sysPath);

					string result = versionInfo.ProductVersion;
					if(result == null)
						throw new Exception(MessagesFormatter.Format(Messages.OsDriverNoVersion, sysPath));
					if (result.IndexOf(" ") != -1)
						result = result.Substring(0, result.IndexOf(" "));

					return result;
				}
				else
				{
					throw new Exception(Messages.OsDriverNoRegPath);
				}

				/* // Not a realtime solution
				ManagementObjectSearcher objSearcher = new ManagementObjectSearcher("Select * from Win32_PnPSignedDriver where DeviceName='" + Engine.Instance.Storage.Get("windows.adapter_name") + "'");

				ManagementObjectCollection objCollection = objSearcher.Get();

				foreach (ManagementObject obj in objCollection)
				{
					object objVersion = obj["DriverVersion"];
					if(objVersion != null)
					{
						string version = objVersion as string;
						return version;
					}				
				}
				*/
			}
			catch (Exception e)
			{
				Engine.Instance.Logs.Log(e);
			}

			return "";
		}

		public override string GetDriverAvailable()
		{
			string result = "";
			NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface adapter in interfaces)
			{
				// Changed in 2.10.4

				// TAP-Win32 Adapter V9
				// or
				// TAP-Windows Adapter V9
				// or something that start with TAP-Win under XP
				if (adapter.Description.ToLowerInvariant().StartsWith("tap-win"))
				{
					result = adapter.Description;
					break;
				}
			}

			// Remember: uninstalling OpenVPN doesn't remove tap0901.sys, so finding an adapter is mandatory.
			if (result == "")
			{
				Engine.Instance.Logs.Log(LogType.Verbose, Messages.OsDriverNoAdapterFound);
				return "";
			}

			string version = GetDriverVersion();

			if (version == "")
			{
				Engine.Instance.Logs.Log(LogType.Verbose, Messages.OsDriverNoVersionFound);
				return "";
			}

			string bundleVersion = Constants.WindowsDriverVersion;
			if (IsVistaOrNewer() == false) // XP
				bundleVersion = Constants.WindowsXpDriverVersion;

			bool needReinstall = false;

			if (Engine.Instance.Storage.GetBool("windows.disable_driver_upgrade") == false)
				needReinstall = (UtilsCore.CompareVersions(version, bundleVersion) == -1);

			if (needReinstall)
			{
				Engine.Instance.Logs.Log(LogType.Warning, MessagesFormatter.Format(Messages.OsDriverNeedUpgrade, version, bundleVersion));
				return "";
			}

			if (result != "")
				result += ", version ";
			result += version;

			return result;
		}

		public override bool CanInstallDriver()
		{
			string driverPath = GetDriverInstallerPath();

			return Platform.Instance.FileExists(driverPath);
		}

		public override bool CanUnInstallDriver()
		{
			if (GetDriverUninstallPath() != "")
				return true;

			return false;
		}

		public string GetDriverInstallerPath()
		{
			if (IsVistaOrNewer())
				return Software.FindResource("tap-windows");
			else
				return Software.FindResource("tap-windows-xp");
		}

		public override void InstallDriver()
		{
			string driverPath = GetDriverInstallerPath();

			if (driverPath == "")
				throw new Exception(Messages.OsDriverInstallerNotAvailable);

			SystemShell.Shell1(driverPath, "/S");

			System.Threading.Thread.Sleep(3000);
		}

		public override void UnInstallDriver()
		{
			string uninstallPath = GetDriverUninstallPath();
			if (uninstallPath == "")
				return;

			SystemShell.Shell1(uninstallPath, "/S");

			System.Threading.Thread.Sleep(3000);
		}

		public override void OnJsonNetworkInfo(Json jNetworkInfo)
		{
			base.OnJsonNetworkInfo(jNetworkInfo);

			foreach (Json jNetworkInterface in jNetworkInfo["interfaces"].Json.GetArray())
			{
				string id = jNetworkInterface["id"].Value as string;

				jNetworkInterface["dns4"].Value = Registry.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces\\" + id.ToLowerInvariant(), "NameServer", "") as string;
				jNetworkInterface["dns6"].Value = Registry.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\Tcpip6\\Parameters\\Interfaces\\" + id.ToLowerInvariant(), "NameServer", "") as string;
				jNetworkInterface["dns6"].Value = null;
			}

			jNetworkInfo["support_ipv4"].Value = OsSupportIPv4();
			jNetworkInfo["support_ipv6"].Value = OsSupportIPv6();
		}

		public override void OnJsonRouteList(Json jRoutesList)
		{
			base.OnJsonRouteList(jRoutesList);

			// Windows 'route' show IPv4 address in IPv4 Interface fields, Adapter Index in IPv6 Interface fields.
			Dictionary<string, string> InterfacesIPv4IpToGuid = new Dictionary<string, string>();
			Dictionary<int, string> InterfacesIPv6IndexToGuid = new Dictionary<int, string>();

			NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface adapter in interfaces)
			{
				string guid = adapter.Id.ToString();
				foreach (UnicastIPAddressInformation ip2 in adapter.GetIPProperties().UnicastAddresses)
				{
					IpAddress ip = new IpAddress(ip2.Address.ToString());
					if (ip.Valid)
					{
						if (ip.IsV4)
						{
							InterfacesIPv4IpToGuid[ip.Address] = guid;
						}
						else if (ip.IsV6)
						{
							int interfaceIPv6Index = -1;
							try
							{
								interfaceIPv6Index = adapter.GetIPProperties().GetIPv6Properties().Index;
							}
							catch
							{
							}
							InterfacesIPv6IndexToGuid[interfaceIPv6Index] = guid;
						}
					}
				}
			}

			// IPv4
			if (true)
			{
				string cmd = "route -4 PRINT";
				if (IsVistaOrNewer() == false) // XP
					cmd = "route PRINT";
				string result = SystemShell.ShellCmd(cmd);

				string[] lines = result.Split('\n');
				foreach (string line in lines)
				{
					string[] fields = UtilsString.StringCleanSpace(line).Split(' ');

					if (fields.Length == 5)
					{
						if (fields[2].ToLowerInvariant().Trim() == "on-link")
							fields[2] = "link";
						Json jRoute = new Json();
						IpAddress address = new IpAddress();
						address.Parse(fields[0] + " " + fields[1]);
						if (address.Valid == false)
							continue;
						jRoute["address"].Value = address.ToCIDR();

						if (InterfacesIPv4IpToGuid.ContainsKey(fields[3]))
							jRoute["interface"].Value = InterfacesIPv4IpToGuid[fields[3]];
						jRoute["gateway"].Value = fields[2];
						jRoute["metric"].Value = fields[4];
						jRoutesList.Append(jRoute);
					}
				}
			}

			// IPv6
			if (IsVistaOrNewer())
			{
				string cmd = "route -6 PRINT";
				string result = SystemShell.ShellCmd(cmd);

				result = result.Replace("\r\n     ", ""); // To avoid parse issue when route print the gateway on a new line.

				string[] lines = result.Split('\n');
				foreach (string line in lines)
				{
					string[] fields = UtilsString.StringCleanSpace(line).Split(' ');

					if (fields.Length == 4)
					{
						if (fields[3].ToLowerInvariant().Trim() == "on-link")
							fields[3] = "link";
						Json jRoute = new Json();
						IpAddress address = new IpAddress();
						address.Parse(fields[2]);
						if (address.Valid == false)
							continue;
						jRoute["address"].Value = address.ToCIDR();
						int interfaceIndex = Convert.ToInt32(fields[0]);
						if (InterfacesIPv6IndexToGuid.ContainsKey(interfaceIndex))
							jRoute["interface"].Value = InterfacesIPv6IndexToGuid[interfaceIndex];
						jRoute["gateway"].Value = fields[3];
						jRoute["metric"].Value = fields[1];
						jRoutesList.Append(jRoute);
					}
				}
			}
		}

		// Specific
		public bool IsWindows8()
		{
			if ((Environment.OSVersion.Version.Major == 6) && (Environment.OSVersion.Version.Minor == 2))
				return true;

			return false;
		}

		public void HackWindowsInterfaceUp()
		{
			NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface adapter in interfaces)
			{
				if (adapter.Description.ToLowerInvariant().StartsWith("tap-win"))
				{
					Engine.Instance.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.HackInterfaceUpDone, adapter.Name));
					SystemShell.ShellCmd("netsh interface set interface \"" + SystemShell.EscapeInsideQuote(adapter.Name) + "\" ENABLED"); // IJTF2 // TOCHECK
				}
			}
		}

		/*
		private void WaitUntilEnabled(ManagementObject objMOC)
		{		
			NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface adapter in interfaces)
			{
				if (objMOC["SettingID"] as string == adapter.Id)
				{
					OperationalStatus status = adapter.OperationalStatus;
					if (status == OperationalStatus.Up)
					{
						// TODO: It's always up...
					}
				}
			}
		}
		*/

		private bool SwitchToStaticDo()
		{
			try
			{
				ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
				ManagementObjectCollection objMOC = objMC.GetInstances();

				foreach (ManagementObject objMO in objMOC)
				{
					if (!((bool)objMO["IPEnabled"]))
						continue;
					if (!((bool)objMO["DHCPEnabled"]))
						continue;

					NetworkManagerDhcpEntry entry = new NetworkManagerDhcpEntry();

					entry.Guid = objMO["SettingID"] as string;
					entry.Description = objMO["Description"] as string;
					entry.IpAddress = (objMO["IPAddress"] as string[]);
					entry.SubnetMask = (objMO["IPSubnet"] as string[]);
					entry.Dns = objMO["DNSServerSearchOrder"] as string[];
					entry.Gateway = objMO["DefaultIPGateway"] as string[];

					entry.AutoDns = ((Registry.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces\\" + entry.Guid, "NameServer", "") as string) == "");

					Engine.Instance.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.NetworkAdapterDhcpDone, entry.Description));

					ManagementBaseObject objEnableStatic = objMO.GetMethodParameters("EnableStatic");
					//objNewIP["IPAddress"] = new string[] { ipAddress };
					//objNewIP["SubnetMask"] = new string[] { subnetMask };
					objEnableStatic["IPAddress"] = new string[] { entry.IpAddress[0] };
					objEnableStatic["SubnetMask"] = new string[] { entry.SubnetMask[0] };
					objMO.InvokeMethod("EnableStatic", objEnableStatic, null);

					ManagementBaseObject objSetDNSServerSearchOrder = objMO.GetMethodParameters("SetDNSServerSearchOrder");
					objSetDNSServerSearchOrder["DNSServerSearchOrder"] = entry.Dns;
					objMO.InvokeMethod("SetDNSServerSearchOrder", objSetDNSServerSearchOrder, null);

					ManagementBaseObject objSetGateways = objMO.GetMethodParameters("SetGateways");
					objSetGateways["DefaultIPGateway"] = new string[] { entry.Gateway[0] };
					objMO.InvokeMethod("SetGateways", objSetGateways, null);

					m_listOldDhcp.Add(entry);

					// TOOPTIMIZE: Need to wait until the interface return UP...
					//WaitUntilEnabled(objMO); // Checking the OperationalStatus changes are not effective.
					System.Threading.Thread.Sleep(3000);
				}

				return true;
			}
			catch (Exception e)
			{
				Engine.Instance.Logs.Log(e);
				return false;
			}
		}

		private void SwitchToStaticRestore()
		{
			try
			{
				ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
				ManagementObjectCollection objMOC = objMC.GetInstances();

				foreach (ManagementObject objMO in objMOC)
				{
					string guid = objMO["SettingID"] as string;
					foreach (NetworkManagerDhcpEntry entry in m_listOldDhcp)
					{
						if (entry.Guid == guid)
						{
							ManagementBaseObject objEnableDHCP = objMO.GetMethodParameters("EnableDHCP");
							objMO.InvokeMethod("EnableDHCP", objEnableDHCP, null);

							ManagementBaseObject objSetDNSServerSearchOrder = objMO.GetMethodParameters("SetDNSServerSearchOrder");
							if (entry.AutoDns == false)
							{
								objSetDNSServerSearchOrder["DNSServerSearchOrder"] = entry.Dns;
							}
							objMO.InvokeMethod("SetDNSServerSearchOrder", objSetDNSServerSearchOrder, null);

							Engine.Instance.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.NetworkAdapterDhcpRestored, entry.Description));
						}
					}
				}
			}
			catch (Exception e)
			{
				Engine.Instance.Logs.Log(e);
			}

			m_listOldDhcp.Clear();
		}

		private void DnsForceRestore()
		{
			try
			{
				// 2.13.3 : Win10 with ManagementObject & SetDNSServerSearchOrder sometime return errcode 84 "IP not enabled on adapter" if we try to set DNS on Tap in state "Network cable unplugged", typical in end of session.
				// 2.14.0 : Deprecated
				//if (IsWin10OrNewer())
				{
					foreach (NetworkManagerDnsEntry entry in m_listOldDns)
					{
						string interfaceName = entry.Description;

						// IPv4
						if (entry.Layer == "IPv4")
						{
							if (entry.Dns.OnlyIPv4.Count == 0)
							{
								SystemShell.ShellCmd("netsh interface ipv4 set dns name=\"" + SystemShell.EscapeInsideQuote(interfaceName) + "\" source=dhcp register=primary validate=no");
							}

							int nIPv4 = 0;
							foreach (IpAddress ip in entry.Dns.OnlyIPv4.IPs)
							{
								if (nIPv4 == 0)
								{
									SystemShell.ShellCmd("netsh interface ipv4 set dns name=\"" + SystemShell.EscapeInsideQuote(interfaceName) + "\" source=static address=" + ip.Address + " register=primary validate=no");
								}
								else
								{
									SystemShell.ShellCmd("netsh interface ipv4 add dnsserver name=\"" + SystemShell.EscapeInsideQuote(interfaceName) + "\" address=" + ip.Address + " validate=no");
								}
								nIPv4++;
							}

							Engine.Instance.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.OsWindowsNetworkAdapterDnsRestored, "IPv4", entry.Description, (entry.Dns.OnlyIPv4.Count == 0 ? "automatic" : entry.Dns.OnlyIPv4.ToString())));
						}

						// IPv6
						if (entry.Layer == "IPv6")
						{
							if (entry.Dns.OnlyIPv6.Count == 0)
							{
								SystemShell.ShellCmd("netsh interface ipv6 set dns name=\"" + SystemShell.EscapeInsideQuote(interfaceName) + "\" source=dhcp register=primary validate=no");
							}

							int nIPv6 = 0;
							foreach (IpAddress ip in entry.Dns.OnlyIPv6.IPs)
							{
								if (nIPv6 == 0)
								{
									SystemShell.ShellCmd("netsh interface ipv6 set dns name=\"" + SystemShell.EscapeInsideQuote(interfaceName) + "\" source=static address=" + ip.Address + " register=primary validate=no");
								}
								else
								{
									SystemShell.ShellCmd("netsh interface ipv6 add dnsserver name=\"" + SystemShell.EscapeInsideQuote(interfaceName) + "\" address=" + ip.Address + " validate=no");
								}
								nIPv6++;
							}

							Engine.Instance.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.OsWindowsNetworkAdapterDnsRestored, "IPv6", entry.Description, (entry.Dns.OnlyIPv6.Count == 0 ? "automatic" : entry.Dns.OnlyIPv6.ToString())));
						}
					}
				}

				/* <2.14.0
				// 2.13.3 : Win10 with ManagementObject & SetDNSServerSearchOrder sometime return errcode 84 "IP not enabled on adapter" if we try to set DNS on Tap in state "Network cable unplugged", typical in end of session.
				if (IsWin10OrNewer())
				{
					foreach (NetworkManagerDnsEntry entry in m_listOldDns)
					{
						NetworkInterface inet = GetNetworkInterfaceFromGuid(entry.Guid);
						if (inet != null)
						{
							string interfaceName = SystemShell.EscapeInsideQuote(inet.Name);

							if (entry.AutoDns == true)
							{
								SystemShell.ShellCmd("netsh interface ipv4 set dns name=\"" + interfaceName + "\" source=dhcp register=primary validate=no");								
							}
							else
							{
								int nIPv4 = 0;
								int nIPv6 = 0;
								foreach (IpAddress ip in entry.Dns)
								{
									if (ip.IsV4)
									{
										if(nIPv4 == 0)
										{
											SystemShell.ShellCmd("netsh interface ipv4 set dns name=\"" + interfaceName + "\" source=static address=" + ip.Address + " register=primary validate=no");
										}
										else
										{
											SystemShell.ShellCmd("netsh interface ipv4 add dnsserver name=\"" + interfaceName + "\" address=" + ip.Address + " validate=no");
										}
										nIPv4++;
									} else if(ip.IsV6)
									{
										if (nIPv6 == 0)
										{
											SystemShell.ShellCmd("netsh interface ipv6 set dns name=\"" + interfaceName + "\" source=static address=" + ip.Address + " register=primary validate=no");
										}
										else
										{
											SystemShell.ShellCmd("netsh interface ipv6 add dnsserver name=\"" + interfaceName + "\" address=" + ip.Address + " validate=no");
										}
										nIPv6++;
									}
								}
							}

							string descTo = (entry.AutoDns ? "automatic" : String.Join(",", entry.Dns));
							Engine.Instance.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.NetworkAdapterDnsRestored, entry.Description, descTo));
						}
					}
				}
				else
				{
					ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
					ManagementObjectCollection objMOC = objMC.GetInstances();

					foreach (ManagementObject objMO in objMOC)
					{
						string guid = objMO["SettingID"] as string;
						foreach (NetworkManagerDnsEntry entry in m_listOldDns)
						{
							if (entry.Guid == guid)
							{
								ManagementBaseObject objSetDNSServerSearchOrder = objMO.GetMethodParameters("SetDNSServerSearchOrder");
								if (entry.AutoDns == false)
								{
									objSetDNSServerSearchOrder["DNSServerSearchOrder"] = entry.Dns;
								}
								else
								{
									//objSetDNSServerSearchOrder["DNSServerSearchOrder"] = new string[] { };
									objSetDNSServerSearchOrder["DNSServerSearchOrder"] = null;
								}

								objMO.InvokeMethod("SetDNSServerSearchOrder", objSetDNSServerSearchOrder, null);

								if (entry.AutoDns == true)
								{
									// Sometime, under Windows 10, the above method don't set it to automatic. So, registry write.
									Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces\\" + entry.Guid, "NameServer", "");
								}

								string descTo = (entry.AutoDns ? "automatic" : String.Join(",", entry.Dns));
								Engine.Instance.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.NetworkAdapterDnsRestored, entry.Description, descTo));
							}
						}
					}
				}
				*/
			}
			catch (Exception e)
			{
				Engine.Instance.Logs.Log(e);
			}

			m_listOldDns.Clear();
		}

		private bool OsSupportIPv4()
		{
			object v = Registry.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\services\\TCPIP\\Parameters", "DisabledComponents", "");
			if (Conversions.ToUInt32(v, 0) == 0) // 0 is the Windows default if the key doesn't exist.
				v = 0;

			return (Conversions.ToUInt32(v, 0) == 0);
		}

		private bool OsSupportIPv6()
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
	}

	public class NetworkManagerDhcpEntry
	{
		//public ManagementObject Obj;
		public string Description;
		public string[] IpAddress;
		public string[] SubnetMask;
		public string[] Dns;
		public string[] Gateway;
		public string Guid;
		public bool AutoDns;

		public void ReadXML(XmlElement node)
		{
			Description = UtilsXml.XmlGetAttributeString(node, "description", "");
			IpAddress = UtilsXml.XmlGetAttributeStringArray(node, "ip_address");
			SubnetMask = UtilsXml.XmlGetAttributeStringArray(node, "subnet_mask");
			Dns = UtilsXml.XmlGetAttributeStringArray(node, "dns");
			Gateway = UtilsXml.XmlGetAttributeStringArray(node, "gateway");
			Guid = UtilsXml.XmlGetAttributeString(node, "guid", "");
			AutoDns = UtilsXml.XmlGetAttributeBool(node, "auto_def", false);
		}

		public void WriteXML(XmlElement node)
		{
			UtilsXml.XmlSetAttributeString(node, "description", Description);
			UtilsXml.XmlSetAttributeStringArray(node, "ip_address", IpAddress);
			UtilsXml.XmlSetAttributeStringArray(node, "subnet_mask", SubnetMask);
			UtilsXml.XmlSetAttributeStringArray(node, "dns", Dns);
			UtilsXml.XmlSetAttributeStringArray(node, "gateway", Gateway);
			UtilsXml.XmlSetAttributeString(node, "guid", Guid);
			UtilsXml.XmlSetAttributeBool(node, "auto_dns", AutoDns);
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
			Guid = UtilsXml.XmlGetAttributeString(node, "guid", "");
			Description = UtilsXml.XmlGetAttributeString(node, "description", "");
			Layer = UtilsXml.XmlGetAttributeString(node, "layer", "");
			Dns.Set(UtilsXml.XmlGetAttributeString(node, "dns", ""));
		}

		public void WriteXML(XmlElement node)
		{
			UtilsXml.XmlSetAttributeString(node, "guid", Guid);
			UtilsXml.XmlSetAttributeString(node, "description", Description);
			UtilsXml.XmlSetAttributeString(node, "layer", Layer);
			UtilsXml.XmlSetAttributeString(node, "dns", Dns.ToString());
		}
	}
}
