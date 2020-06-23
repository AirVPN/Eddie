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
using System.Threading.Tasks;
using System.ServiceProcess;
using Microsoft.Win32;

using Eddie.Core;

namespace Eddie.Platform.Windows
{
	public class Platform : Core.Platform
	{
		private string ServiceName = "EddieElevationService";
		private string WindowsDriverTapId = "0901";
		private string WindowsDriverTapVersion = "9.24.2";
		private string WindowsDriverWintunId = "wintun";
		private string WindowsDriverWintunVersion = "0.8";
		private string WindowsXpDriverTapVersion = "9.9.2";

		private List<NetworkManagerDnsEntry> m_listOldDns = new List<NetworkManagerDnsEntry>();		
		private string m_oldMetricInterface = "";
		private int m_oldMetricIPv4 = -1;
		private int m_oldMetricIPv6 = -1;
		private Mutex m_mutexSingleInstance = null;
		private NativeMethods.ConsoleCtrlHandlerRoutine m_consoleCtrlHandlerRoutine;

		public static bool IsXpOrNewer()
		{
			OperatingSystem OS = Environment.OSVersion;
			return (OS.Platform == PlatformID.Win32NT) && (OS.Version.Major >= 5);
		}

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
		}

		public override string GetCode()
		{
			return "Windows";
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

		public override bool OnInit()
		{
			base.OnInit();

			try
			{
				// Check VC++ Redistributable

				// Need because our lib include libcurl, and static link of libcurl is not recommended
				// Registry check is not affidable, with vcredistr installed via msm/wix
				//if (Conversions.ToInt32(Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\" + GetOsArchitecture(), "Major", "0"), 0)<14)
				if (File.Exists(Path.Combine(Environment.SystemDirectory, "vcruntime140.dll")) == false)
				{
					Engine.Instance.Logs.LogFatal("This software require some additional files (C++ Redistributable) that are not currently installed on your system. Use the Installer edition, or look https://eddie.website/windows-runtime/");
					return false;
				}				

				bool result = (NativeMethods.Init() == 0);
				if (result == false)
					throw new Exception("fail");

				m_consoleCtrlHandlerRoutine = new NativeMethods.ConsoleCtrlHandlerRoutine(ConsoleCtrlCheck); // Avoid Garbage Collector
				NativeMethods.SetConsoleCtrlHandler(m_consoleCtrlHandlerRoutine, true);
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

		public override string GetOsArchitecture()
		{
			if (GetArchitecture() == "x64")
				return "x64";
			if (NativeMethods.InternalCheckIsWow64())
				return "x64";
			return "x86";
		}

		public override Eddie.Core.Elevated.EleBase StartElevated()
		{
			ElevatedImpl e = new ElevatedImpl();
			e.Start();

			return e;
		}

		public override bool IsAdmin()
		{
			bool isElevated;
			WindowsIdentity identity = WindowsIdentity.GetCurrent();
			WindowsPrincipal principal = new WindowsPrincipal(identity);
			isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);

			return isElevated;
		}

		protected override string GetElevatedHelperPathImpl()
		{
			string path = AppDomain.CurrentDomain.BaseDirectory + "Eddie-CLI-Elevated.exe";
			return FileGetPhysicalPath(path);
		}

		public override bool CheckElevatedSocketAllowed(IPEndPoint localEndpoint, IPEndPoint remoteEndpoint)
		{
			int bufferSize = 0;

			// Getting the size of TCP table, that is returned in 'bufferSize' variable. 
			uint result = NativeMethods.GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, true, NativeMethods.AF_INET, NativeMethods.TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL);

			// Allocating memory from the unmanaged memory of the process by using the 
			// specified number of bytes in 'bufferSize' variable. 
			IntPtr tcpTableRecordsPtr = Marshal.AllocHGlobal(bufferSize);

			try
			{
				// The size of the table returned in 'bufferSize' variable in previous 
				// call must be used in this subsequent call to 'GetExtendedTcpTable' 
				// function in order to successfully retrieve the table. 
				result = NativeMethods.GetExtendedTcpTable(tcpTableRecordsPtr, ref bufferSize, true, NativeMethods.AF_INET, NativeMethods.TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL);

				// Non-zero value represent the function 'GetExtendedTcpTable' failed, 
				// hence empty list is returned to the caller function. 
				if (result != 0)
					return false;

				// Marshals data from an unmanaged block of memory to a newly allocated 
				// managed object 'tcpRecordsTable' of type 'MIB_TCPTABLE_OWNER_PID' 
				// to get number of entries of the specified TCP table structure. 
				NativeMethods.MIB_TCPTABLE_OWNER_PID tcpRecordsTable = (NativeMethods.MIB_TCPTABLE_OWNER_PID)
										Marshal.PtrToStructure(tcpTableRecordsPtr,
										typeof(NativeMethods.MIB_TCPTABLE_OWNER_PID));
				IntPtr tableRowPtr = (IntPtr)((long)tcpTableRecordsPtr +
										Marshal.SizeOf(tcpRecordsTable.dwNumEntries));

				// Reading and parsing the TCP records one by one from the table and 
				// storing them in a list of 'TcpProcessRecord' structure type objects. 
				for (int row = 0; row < tcpRecordsTable.dwNumEntries; row++)
				{
					NativeMethods.MIB_TCPROW_OWNER_PID tcpRow = (NativeMethods.MIB_TCPROW_OWNER_PID)Marshal.
						PtrToStructure(tableRowPtr, typeof(NativeMethods.MIB_TCPROW_OWNER_PID));

					System.Net.IPAddress localAddr = new System.Net.IPAddress(tcpRow.localAddr);
					System.Net.IPAddress remoteAddr = new System.Net.IPAddress(tcpRow.remoteAddr);
					UInt16 localPort = BitConverter.ToUInt16(new byte[2] { tcpRow.localPort[1], tcpRow.localPort[0] }, 0);
					UInt16 remotePort = BitConverter.ToUInt16(new byte[2] { tcpRow.remotePort[1], tcpRow.remotePort[0] }, 0);
					
					if ((localEndpoint.Address.ToString() == localAddr.ToString()) &&
						(localEndpoint.Port == localPort) &&
						(remoteEndpoint.Address.ToString() == remoteAddr.ToString()) &&
						(remoteEndpoint.Port == remotePort))
					{
						int pid = tcpRow.owningPid;

						System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessById(pid);

						return CheckElevatedProcessAllowed(process);
					}

					tableRowPtr = (IntPtr)((long)tableRowPtr + Marshal.SizeOf(tcpRow));
				}
			}
			catch (OutOfMemoryException)
			{
				return false;
			}
			catch (Exception)
			{
				return false;
			}
			finally
			{
				Marshal.FreeHGlobal(tcpTableRecordsPtr);
			}

			return false;
		}

		public override bool CheckElevatedProcessAllowed(System.Diagnostics.Process process)
		{
			string remotePath = "";
			try
			{
				remotePath = process.MainModule.FileName;
			}
			catch
			{
				// Security step: 
				// Standard access path of an elevated process throw Access Denied.
				// QueryFullProcessImageName workaround require Vista, and don't work anyway with Windows Service.
				// In general, if MainModule.FileName fail, it's elevated so trusted.
				return true;
			}

			return CheckElevatedProcessAllowed(remotePath);
		}

		public override bool CheckElevatedProcessAllowed(string remotePath)
		{
			// Security step: if not root, exit
			{
				// Never found a Win good solution that work even if spot-launched or service edition.
				// If spot-launched, WTSQuerySessionInformationW return the current user.
				// If spot-launched or service, GetTokenInformation don't work in any case without elevation.
			}

			// Signature check removed, redundant. // ClodoTemp
			return true;

			bool match = false;

			string localPath = System.Reflection.Assembly.GetEntryAssembly().Location;
			
			// Security step: check match signature
			try
			{
				System.Security.Cryptography.X509Certificates.X509Certificate c1 = System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromSignedFile(localPath);
				System.Security.Cryptography.X509Certificates.X509Certificate c2 = System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromSignedFile(remotePath);

				match = (
					(c1.Issuer == c2.Issuer) &&
					(c1.Subject == c2.Subject) &&
					(c1.GetCertHashString() == c2.GetCertHashString()) &&
					(c1.GetEffectiveDateString() == c2.GetEffectiveDateString()) &&
					(c1.GetPublicKeyString() == c2.GetPublicKeyString()) &&
					(c1.GetRawCertDataString() == c2.GetRawCertDataString()) &&
					(c1.GetSerialNumberString() == c2.GetSerialNumberString())
				);
			}
			catch
			{
			}

#if DEBUG
			// Never official deploy debug edition
			match = true;
#endif

			return match;
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
			/* <2.17.3
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
			*/
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
					if (Engine.Instance.Storage.Get("path") != "")
						arguments = "-path=" + Engine.Instance.Storage.Get("path");
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
			try
			{
				ServiceController sc = new ServiceController();
				sc.ServiceName = ServiceName;

				// Unfortunately i cannot detect is StartType==Disabled, without check registry
				if(sc.Status == ServiceControllerStatus.StartPending)
					sc.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0,0,60));
			}
			catch
			{
			}
		}

		protected override bool GetServiceImpl()
		{
			try
			{
				ServiceController sc = new ServiceController();
				sc.ServiceName = ServiceName;

				if (sc.Status == ServiceControllerStatus.Running)
					return true;
			}
			catch
			{
			}

			return false;
			/*
			ServiceController[] services = ServiceController.GetServices();
			foreach(ServiceController service in services)
			{
				if (service.ServiceName == ServiceName)
					return true;
			}
			return false;
			*/
		}

		protected override bool SetServiceImpl(bool value)
		{
			if (GetServiceImpl() == value)
				return true;

			try
			{
				ProcessStartInfo processStart = new ProcessStartInfo();
				processStart.FileName = GetElevatedHelperPath();
				processStart.Arguments = "";
				processStart.Verb = "runas";
				processStart.CreateNoWindow = true;
				processStart.UseShellExecute = true;
				if (value)
				{
					processStart.Arguments = "service=install service_port=" + Engine.Instance.GetElevatedServicePort();
					System.Diagnostics.Process p = System.Diagnostics.Process.Start(processStart);
					p.WaitForExit();
					return (GetService() == true);
				}
				else
				{
					processStart.Arguments = "service=uninstall";
					System.Diagnostics.Process p = System.Diagnostics.Process.Start(processStart);
					p.WaitForExit();
					return (GetService() == false);
				}
			}
			catch (Exception ex)
			{
				Engine.Instance.Logs.Log(ex);
				return false;
			}
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

		public override string FileGetPhysicalPath(string path)
		{
			string native = NativeMethods.GetFinalPathName(path);
			if (native.StartsWith("\\\\?\\"))
				native = native.Substring(4);
			if (native.StartsWith("UNC\\"))
				native = "\\" + native.Substring(3);
			return native;
		}

		public override bool FileEnsureCurrentUserOnly(string path)
		{
			// Remove Inheritance
			SystemShell.Shell1(Platform.Instance.LocateExecutable("icacls.exe"), "\"" + SystemShell.EscapePath(path) + "\" /c /t /inheritance:d");

			// Set Ownership to Owner
			SystemShell.Shell1(Platform.Instance.LocateExecutable("icacls.exe"), "\"" + SystemShell.EscapePath(path) + "\" /c /t /grant \"" + SystemShell.EscapeInsideQuote(Environment.UserName) + "\":F");

			// Remove Authenticated Users
			SystemShell.Shell1(Platform.Instance.LocateExecutable("icacls.exe"), "\"" + SystemShell.EscapePath(path) + "\" /c /t /remove:g *S-1-5-11");

			// Remove other groups
			SystemShell.Shell1(Platform.Instance.LocateExecutable("icacls.exe"), "\"" + SystemShell.EscapePath(path) + "\" /c /t /remove Administrator \"BUILTIN\\Administrators\" \"NT AUTHORITY\\Authenticated Users\" \"BUILTIN\\Users\" BUILTIN Everyone System Users");

			return true;
		}

		public override string GetExecutablePathEx()
		{
			// It return vshost.exe under VS, better
			string path = Environment.GetCommandLineArgs()[0];
			path = Path.GetFullPath(path); // 2.11.9
			return path;
		}

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

		
		public override Int64 Ping(IpAddress host, int timeoutSec)
		{
			if (host.IsV4)
				return base.Ping(host, timeoutSec);

			// If IPv6, We use a Task<> because otherwise timeout is not honored and hang forever in Win10 with IPv6 issues (Vbox in Nat)
			if (timeoutSec == 5000) Engine.Instance.Logs.LogVerbose("ping 1");

			if ((host == null) || (host.Valid == false))
				return -1;

			Task<long> pingTask = Task.Factory.StartNew(() =>
			{
				try
				{
					using (Ping pingSender = new Ping())
					{
						PingReply reply = pingSender.Send(host.ToString());

						if (reply.Status == IPStatus.Success)
							return reply.RoundtripTime;
						else
							return -1;
					}
				}
				catch
				{
					return -1;
				}
			});

			if (timeoutSec == 5000) Engine.Instance.Logs.LogVerbose("ping 2 W");
			pingTask.Wait(timeoutSec * 1000);
			if (timeoutSec == 5000) Engine.Instance.Logs.LogVerbose("ping 2 WE");
			if (pingTask.IsCompleted)
				return pingTask.Result;
			else
				return -1;

		}

		public override bool ProcessKillSoft(Core.Process process)
		{
			process.CloseMainWindow();
			return true;

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

		public override int GetRecommendedRcvBufDirective()
		{
			return 256 * 1024;
		}

		public override int GetRecommendedSndBufDirective()
		{
			return 256 * 1024;
		}

		public override bool FetchUrlInternal()
		{
			return true;
		}

		public override Json FetchUrl(Json request)
		{
			return NativeMethods.CUrl(request);
		}

		public override void FlushDNS()
		{
			base.FlushDNS();
			
			Engine.Instance.Elevated.DoCommandSync("dns-flush", "mode", (Engine.Instance.Storage.GetBool("windows.workarounds") ? "max" : "normal"));
			
			SystemShell.Shell1(LocateExecutable("ipconfig.exe"), "/flushdns");
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
					throw new Exception(LanguageManager.GetText("NetworkInterfaceNotAvailable"));

				int interfaceIdx = 0;
				if (ip.IsV4)
					interfaceIdx = networkInterface.GetIPProperties().GetIPv4Properties().Index;
				else
					interfaceIdx = networkInterface.GetIPProperties().GetIPv6Properties().Index;
				Core.Elevated.Command c = new Core.Elevated.Command();
				c.Parameters["command"] = "route";
				c.Parameters["action"] = "add";
				c.Parameters["layer"] = (ip.IsV4 ? "ipv4" : "ipv6");
				c.Parameters["cidr"] = ip.ToCIDR(true);
				c.Parameters["address"] = ip.Address;
				c.Parameters["mask"] = ip.Mask;
				c.Parameters["gateway"] = gateway.Address;
				c.Parameters["iface"] = interfaceIdx.ToString();
				if (jRoute.HasKey("metric"))
					c.Parameters["metric"] = jRoute["metric"].Value as string;
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
				NetworkInterface networkInterface = GetNetworkInterfaceFromGuid(jRoute["interface"].Value as string);

				if (networkInterface == null)
					throw new Exception(LanguageManager.GetText("NetworkInterfaceNotAvailable"));

				int interfaceIdx = 0;
				if (ip.IsV4)
					interfaceIdx = networkInterface.GetIPProperties().GetIPv4Properties().Index;
				else
					interfaceIdx = networkInterface.GetIPProperties().GetIPv6Properties().Index;
				Core.Elevated.Command c = new Core.Elevated.Command();
				c.Parameters["command"] = "route";
				c.Parameters["action"] = "remove";
				c.Parameters["layer"] = (ip.IsV4 ? "ipv4" : "ipv6");
				c.Parameters["cidr"] = ip.ToCIDR(true);
				c.Parameters["address"] = ip.Address;
				c.Parameters["mask"] = ip.Mask;
				c.Parameters["gateway"] = gateway.Address;
				c.Parameters["iface"] = interfaceIdx.ToString();
				if (jRoute.HasKey("metric"))
					c.Parameters["metric"] = jRoute["metric"].Value as string;
				Engine.Instance.Elevated.DoCommandSync(c);
				return base.RouteRemove(jRoute);
			}
			catch (Exception e)
			{
				Engine.Instance.Logs.LogVerbose(LanguageManager.GetText("RouteDelFailed", ip.ToCIDR(), gateway.ToCIDR(), e.Message));
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

		public override bool WaitTunReady(ConnectionActive connection)
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

			report.Add("ipconfig /all", SystemShell.Shell1(Platform.Instance.LocateExecutable("ipconfig.exe"), "/all"));
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
			// FlushDNS(); // Removed in 2.11.10



			Recovery.Save();
		}

		public override void OnSessionStop()
		{
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

				Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("IPv6DisabledWpf"));
			}
			else
				throw new Exception("Unexpected IPv6 block requested");

			base.OnIPv6Block();

			return true;
		}

		public override bool OnIPv6Restore()
		{
			if ((Wfp.RemoveItem("ipv6_block_all")) && (Wfp.RemoveItem("ipv6_allow_loopback")))
				Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("IPv6RestoredWpf"));

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

			if (Engine.Instance.GetOpenVpnTool().VersionAboveOrEqual("2.5"))
			{
				if (Engine.Instance.Storage.GetBool("windows.wintun"))
				{
					ovpn.AppendDirectives("windows-driver wintun", "OS");
				}
			}				
		}

		public override void OpenVpnConfigNormalize(OvpnBuilder ovpn)
		{
			if (Engine.Instance.GetOpenVpnTool().VersionUnder("2.5"))
			{
				ovpn.RemoveDirective("windows-driver");
			}

			if(ovpn.ExistsDirective("windows-driver"))
			{
				string driver = ovpn.GetOneDirectiveText("windows-driver");
				if (driver != "wintun")
					ovpn.RemoveDirective("windows-driver");
			}
		}

		public override string GetOvpnDriverRequested(OvpnBuilder ovpn)
		{
			string driver = ovpn.GetOneDirectiveText("windows-driver");
			if (driver == "wintun")
				return WindowsDriverWintunId;
			else
				return WindowsDriverTapId;
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
					XmlIf2.SetAttribute("path", Engine.Instance.GetOpenVpnTool().Path);
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
						XmlIf2.SetAttribute("interface", Engine.Instance.ConnectionActive.Interface.Id);
						Wfp.AddItem("dns_permit_tap", xmlRule);
					}
				}


				Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("DnsLockActivatedWpf"));
			}

			string mode = Engine.Instance.Storage.GetLower("dns.mode");

			if (mode == "auto")
			{
				Json jNetworkInfo = Engine.Instance.JsonNetworkInfo(); // Realtime
				foreach (Json jNetworkInterface in jNetworkInfo["interfaces"].Json.GetArray())
				{	
					string id = jNetworkInterface["id"].Value as string;
					string interfaceName = jNetworkInterface["name"].Value as string;

					bool skip = true;

					if (Engine.Instance.Storage.GetBool("windows.dns.force_all_interfaces"))
						skip = false;
					if ((Engine.Instance.ConnectionActive != null) && (id == Engine.Instance.ConnectionActive.Interface.Id))
						skip = false;
					if (jNetworkInterface["type"].ValueString == "Ethernet")
						skip = false;
					if (jNetworkInterface["type"].ValueString == "Virtual")
						skip = false;
					if (jNetworkInterface["status"].ValueString != "Up")
						skip = true;

					if (skip == false)
					{
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
								Engine.Instance.Elevated.DoCommandSync("windows-dns", "layer", "ipv4", "interface", interfaceName, "mode", "dhcp");
							}

							int nIPv4 = 0;
							foreach (IpAddress ip in dns.OnlyIPv4.IPs)
							{
								Engine.Instance.Elevated.DoCommandSync("windows-dns", "layer", "ipv4", "interface", interfaceName, "ipaddress", ip.Address, "mode", ((nIPv4 == 0) ? "static":"add"));
								nIPv4++;
							}

							m_listOldDns.Add(entry);
							Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("OsWindowsNetworkAdapterDnsDone", "IPv4", entry.Description, (entry.Dns.OnlyIPv4.Count == 0 ? "automatic" : "manual" + " (" + entry.Dns.OnlyIPv4.ToString() + ")"), dns.OnlyIPv4.ToString()));
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
								Engine.Instance.Elevated.DoCommandSync("windows-dns", "layer", "ipv6", "interface", interfaceName, "mode", "dhcp");
							}

							int nIPv6 = 0;
							foreach (IpAddress ip in dns.OnlyIPv6.IPs)
							{
								Engine.Instance.Elevated.DoCommandSync("windows-dns", "layer", "ipv6", "interface", interfaceName, "ipaddress", ip.Address, "mode", ((nIPv6 == 0) ? "static" : "add"));
								nIPv6++;
							}

							m_listOldDns.Add(entry);
							Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("OsWindowsNetworkAdapterDnsDone", "IPv6", entry.Description, (entry.Dns.OnlyIPv6.Count == 0 ? "automatic" : "manual" + " (" + entry.Dns.OnlyIPv6.ToString() + ")"), dns.OnlyIPv6.ToString()));
						}
					}
				}

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
				Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("DnsLockDeactivatedWpf"));

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
				Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("NetworkAdapterMetricSwitch", interfaceMetricIPv4Name, fromStr, toStr, "IPv4"));
				m_oldMetricInterface = id;
				m_oldMetricIPv4 = interfaceMetricIPv4Current;
				Engine.Instance.Elevated.DoCommandSync("set-interface-metric", "idx", interfaceMetricIPv4Idx.ToString(), "layer", "ipv4", "value", interfaceMetricIPv4Value.ToString());
				
				Recovery.Save();
			}

			if ((interfaceMetricIPv6Current != -1) && (interfaceMetricIPv6Current != interfaceMetricIPv6Value))
			{
				string fromStr = (interfaceMetricIPv6Current == 0) ? "Automatic" : interfaceMetricIPv6Current.ToString();
				string toStr = (interfaceMetricIPv6Value == 0) ? "Automatic" : interfaceMetricIPv6Value.ToString();
				Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("NetworkAdapterMetricSwitch", interfaceMetricIPv6Name, fromStr, toStr, "IPv6"));
				m_oldMetricInterface = id;
				m_oldMetricIPv6 = interfaceMetricIPv6Current;
				Engine.Instance.Elevated.DoCommandSync("set-interface-metric", "idx", interfaceMetricIPv6Idx.ToString(), "layer", "ipv6", "value", interfaceMetricIPv6Value.ToString());				

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
								Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("NetworkAdapterMetricRestore", adapter.Name, fromStr, toStr, "IPv4"));
								Engine.Instance.Elevated.DoCommandSync("set-interface-metric", "idx", idx.ToString(), "layer", "ipv4", "value", m_oldMetricIPv4.ToString());								
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
								Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("NetworkAdapterMetricRestore", adapter.Name, fromStr, toStr, "IPv6"));
								Engine.Instance.Elevated.DoCommandSync("set-interface-metric", "idx", idx.ToString(), "layer", "ipv6", "value", m_oldMetricIPv6.ToString());								
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

		public override void OnRecoveryAlways()
		{
			base.OnRecoveryAlways();

			if (IsWin7OrNewer()) // 2.12.2
				if (Wfp.ClearPendingRules())
					Engine.Instance.Logs.Log(LogType.Warning, LanguageManager.GetText("WfpRecovery"));
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
				string result = "";
				if (IsVistaOrNewer() == false) // XP
					result = SystemShell.Shell1(LocateExecutable("route.exe"), "PRINT");
				else
					result = SystemShell.Shell2(LocateExecutable("route.exe"), "-4", "PRINT");

				string[] lines = result.Split('\n');
				foreach (string line in lines)
				{
					string[] fields = line.CleanSpace().Split(' ');

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
				string result = "";
				if (IsVistaOrNewer() == false) // XP
					result = SystemShell.Shell1(LocateExecutable("route.exe"), "PRINT");
				else
					result = SystemShell.Shell2(LocateExecutable("route.exe"), "-6", "PRINT");

				result = result.Replace("\r\n     ", ""); // To avoid parse issue when route print the gateway on a new line.

				string[] lines = result.Split('\n');
				foreach (string line in lines)
				{
					string[] fields = line.CleanSpace().Split(' ');

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

		public override string OsCredentialSystemName()
		{			
			if (IsAdmin()) // Are saved as Admin and not viewer by normal user, will become an issue.
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

		public override string FileGetSignedId(string path)
		{
			// Note 2018-10-24
			// Never found a snippet of code C# or C++ to avoid the fallback shell, required in some digital signature.

			// A
			// System.Security.Cryptography.X509Certificates.X509Certificate2 x509 = new System.Security.Cryptography.X509Certificates.X509Certificate2(System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromSignedFile(path));
			// if (x509.Verify() == false)
			// return false for tap-windows.exe signed by OpenVPN, and in any case don't work with catalog-based signatures like %system32%\route.exe.

			// This crash: https://code.msdn.microsoft.com/windowsdesktop/WinVerifyTrust-signature-a95ab1f6
			// This crash: https://social.technet.microsoft.com/Forums/en-US/cf474fc3-c081-4e30-80c0-edae3f675378/howto-verify-the-digital-signature-of-a-file?forum=windowsdevelopment

			// sigcheck pass x509 signature verification, so we don't need GetTrustedPaths().


			// openvpn.exe:         x509.Verify() true, AuthenticodeTools.IsTrusted() true, sigcheck true.
			// tap-windows.exe:     
			// %system32/route.exe:  

			try
			{
				// First try Authenticode
				System.Security.Cryptography.X509Certificates.X509Certificate2 x509 = new System.Security.Cryptography.X509Certificates.X509Certificate2(System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromSignedFile(path));

				bool x509verify = x509.Verify();
				bool acodeVerify = AuthenticodeTools.IsTrusted(path);

				if ((x509verify) && (acodeVerify))
				{
					return "Subject: " + x509.Subject + " - Issuer: " + x509.Issuer;
				}
			}
			catch
			{
			}

			// Fallback
			string sigcheckPath = Engine.Instance.GetPathTools() + "\\sigcheck.exe";
			string[] sigcheck = SystemShell.Shell3(sigcheckPath, "-c", "-nobanner", "\"" + SystemShell.EscapePath(path) + "\"").Split('\n');

			List<string> list = sigcheck[1].StringToList(",", true, false, false, true);

			if (list[1].Trim('"') == "Signed")
				return "Publisher: " + list[3].Trim('"') + " - Company: " + list[4].Trim('"');

			return "No: Not signed or invalid.";
		}

		public override string GetDriverVersion(string driver)
		{
			try
			{
				if (driver == "0901")
				{
					string sysPath = "";

					object objSysPath = Registry.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\tap0901", "ImagePath", "");
					if (objSysPath != null)
						sysPath = objSysPath as string;

					if (sysPath != "")
					{
						sysPath = NormalizeWinSysPath(sysPath);

						if (Platform.Instance.FileExists(sysPath) == false)
						{
							throw new Exception(LanguageManager.GetText("OsDriverNoPath", sysPath));
						}

						// GetVersionInfo may throw a FileNotFound exception between 32bit/64bit SO/App.
						FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(sysPath);

						string result = versionInfo.ProductVersion;
						if (result == null)
							throw new Exception(LanguageManager.GetText("OsDriverNoVersion", sysPath));
						if (result.IndexOf(" ") != -1)
							result = result.Substring(0, result.IndexOf(" "));

						return result;
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
				else if (driver == "wintun")
				{
					object wintunVersion = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Wintun", "Version", "");
					if (wintunVersion != null)
						return wintunVersion as string;					
				}
				else
					throw new Exception("Unknown driver " + driver);

				
			}
			catch (Exception e)
			{
				Engine.Instance.Logs.Log(e);
			}

			return "";
		}

		public override string GetTunDriverReport()
		{
			string result = "";

			List<string> drivers = new List<string>();
			drivers.Add(WindowsDriverTapId);
			drivers.Add(WindowsDriverWintunId);

			foreach (string driver in drivers)
			{
				string driverResult = GetDriverVersion(driver);
				if (driverResult == "")
					driverResult = "Not found";
				if (result != "")
					result += "; ";
				result += driver + ": " + driverResult;
			}
			return result;
		}

		public override void EnsureDriverAndAdapterAvailable(string driver)
		{
			string version = GetDriverVersion(driver);

			string bundleVersion = WindowsDriverTapVersion;
			if (driver == WindowsDriverTapId)
			{
				bundleVersion = WindowsDriverTapVersion;
				if (IsVistaOrNewer() == false) // XP
					bundleVersion = WindowsXpDriverTapVersion;
			}
			else if (driver == WindowsDriverWintunId)
				bundleVersion = WindowsDriverWintunVersion;

			bool needInstall = false;

			if (version == "")
			{
				needInstall = true;
				Engine.Instance.Logs.Log(LogType.InfoImportant, LanguageManager.GetText("OsDriverInstall", driver));
			}
			else if ((Engine.Instance.Storage.GetBool("windows.disable_driver_upgrade") == false) && (version.VersionCompare(bundleVersion) == -1))
			{
				Engine.Instance.Logs.Log(LogType.Warning, LanguageManager.GetText("OsDriverNeedUpgrade", driver, version, bundleVersion));
				needInstall = true;
			}

			if (needInstall)
			{
				string driverPath = GetDriverInstallerPath(driver);

				if (driverPath == "")
					throw new Exception(LanguageManager.GetText("OsDriverInstallerNotAvailable", driver));

				if (driver == WindowsDriverTapId)
					SystemShell.ShellUserEvent(driverPath, "/S", true);
				else if (driver == WindowsDriverWintunId)
					SystemShell.ShellUserEvent("msiexec", "/i \"" + driverPath + "\" /passive /norestart", true);

				if (GetDriverVersion(driver) == "")
					throw new Exception(LanguageManager.GetText("OsDriverFailed", driver));
			}

			bool adapterFound = false;			
			NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface adapter in interfaces)
			{
				if ((driver == WindowsDriverTapId) && (adapter.Description.ToLowerInvariant().StartsWith("tap-win")))
					adapterFound = true;

				if ((driver == WindowsDriverWintunId) && (adapter.Description.ToLowerInvariant().StartsWith("wintun")))
					adapterFound = true;
			}

			if (adapterFound == false)
			{
				Engine.Instance.Logs.LogVerbose(LanguageManager.GetText("OsDriverNoAdapterFound", driver));
				if (driver == WindowsDriverTapId)
				{
					SystemShell.ShellUserEvent(Software.FindResource("tapctl"), "create --hwid root\\tap0901", true);
				}	
				else if (driver == WindowsDriverWintunId)
				{
					SystemShell.ShellUserEvent(Software.FindResource("tapctl"), "create --hwid wintun", true);
				}
			}

			adapterFound = false;
			interfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface adapter in interfaces)
			{
				if ((driver == WindowsDriverTapId) && (adapter.Description.ToLowerInvariant().StartsWith("tap-win")))
					adapterFound = true;

				if ((driver == WindowsDriverWintunId) && (adapter.Description.ToLowerInvariant().StartsWith("wintun")))
					adapterFound = true;
			}

			if (adapterFound == false)
				throw new Exception(LanguageManager.GetText("OsDriverAdapterNotAvailable", driver));
		}

		public override bool UninstallDriver(string driver)
		{
			return DriverUninstall(driver);
		}

		/*
		public override List<string> GetTrustedPaths()
		{
			List<string> list = base.GetTrustedPaths();
			list.Add(Environment.SystemDirectory);
			return list;
		}
		*/

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
					Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("HackInterfaceUpDone", adapter.Name));

					Engine.Instance.Elevated.DoCommandSync("windows-workaround-interface-up", "name", adapter.Name);					
				}
			}
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
								Engine.Instance.Elevated.DoCommandSync("windows-dns", "layer", "ipv4", "interface", interfaceName, "mode", "dhcp");
							}

							int nIPv4 = 0;
							foreach (IpAddress ip in entry.Dns.OnlyIPv4.IPs)
							{
								Engine.Instance.Elevated.DoCommandSync("windows-dns", "layer", "ipv4", "interface", interfaceName, "ipaddress", ip.Address, "mode", ((nIPv4 == 0) ? "static" : "add"));
								nIPv4++;
							}

							Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("OsWindowsNetworkAdapterDnsRestored", "IPv4", entry.Description, (entry.Dns.OnlyIPv4.Count == 0 ? "automatic" : entry.Dns.OnlyIPv4.ToString())));
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

							Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("OsWindowsNetworkAdapterDnsRestored", "IPv6", entry.Description, (entry.Dns.OnlyIPv6.Count == 0 ? "automatic" : entry.Dns.OnlyIPv6.ToString())));
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
								SystemShell.Shell1(Platform.Instance.LocateExecutable("netsh.exe"), "interface ipv4 set dns name=\"" + interfaceName + "\" source=dhcp register=primary validate=no");								
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
											SystemShell.Shell1(Platform.Instance.LocateExecutable("netsh.exe"), "interface ipv4 set dns name=\"" + interfaceName + "\" source=static address=" + ip.Address + " register=primary validate=no");
										}
										else
										{
											SystemShell.Shell1(Platform.Instance.LocateExecutable("netsh.exe"), "interface ipv4 add dnsserver name=\"" + interfaceName + "\" address=" + ip.Address + " validate=no");
										}
										nIPv4++;
									} else if(ip.IsV6)
									{
										if (nIPv6 == 0)
										{
											SystemShell.Shell1(Platform.Instance.LocateExecutable("netsh.exe"), "interface ipv6 set dns name=\"" + interfaceName + "\" source=static address=" + ip.Address + " register=primary validate=no");
										}
										else
										{
											SystemShell.Shell1(Platform.Instance.LocateExecutable("netsh.exe"), "interface ipv6 add dnsserver name=\"" + interfaceName + "\" address=" + ip.Address + " validate=no");
										}
										nIPv6++;
									}
								}
							}

							string descTo = (entry.AutoDns ? "automatic" : String.Join(",", entry.Dns));
							Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("NetworkAdapterDnsRestored, entry.Description, descTo));
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
								Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("NetworkAdapterDnsRestored, entry.Description, descTo));
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
			

		private string NormalizeWinSysPath(string path)
		{
			string sysPath = path;

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
				// tunnel driver path above is real, but Redirector cause issue.
				// https://msdn.microsoft.com/en-us/library/aa384187(v=vs.85).aspx
				// The Sysnative alias was added starting with Windows Vista.
				sysPath = sysPath.ToLowerInvariant();
				sysPath = sysPath.Replace("\\system32\\", "\\sysnative\\");
			}
			return sysPath;
		}

		private string GetDriverInstallerPath(string driver)
		{
			if (driver == WindowsDriverWintunId)
				return Software.FindResource("eddie-wintun");
			else if (driver == WindowsDriverTapId)
			{
				if (IsVistaOrNewer())
					return Software.FindResource("tap-windows");
				else
					return Software.FindResource("tap-windows-xp");
			}
			else
				return "";
		}

		private string GetDriverUninstallPath(string driver)
		{
			if (driver == WindowsDriverWintunId)
				return Software.FindResource("eddie-wintun");
			else if (driver == WindowsDriverTapId)
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
			}

			return "";
		}
		/*
		private bool DriverInstall(string driver)
		{
			string driverPath = GetDriverInstallerPath(driver);

			if (driverPath == "")
				throw new Exception(LanguageManager.GetText("OsDriverInstallerNotAvailable"));

			if(driver == WindowsDriverTapId)
				SystemShell.ShellUserEvent(driverPath, "/S", true);
			else if(driver == WindowsDriverWintunId)
				SystemShell.ShellUserEvent("msiexec", "/i \"" + driverPath + "\" /quiet /norestart", true);

			System.Threading.Thread.Sleep(3000);

			return (GetDriverVersion(driver) != "");
		}
		*/

		public bool DriverUninstall(string driver)
		{
			string driverPath = GetDriverUninstallPath(driver);
			if (driverPath == "")
				return false;

			if (driver == WindowsDriverTapId)
			{
				SystemShell.ShellUserEvent(driverPath, "/S", true);
			}
			else if (driver == WindowsDriverWintunId)
			{
				//SystemShell.ShellUserEvent("msiexec", "/x \"" + driverPath + "\" /quiet /norestart", true);
				SystemShell.ShellUserEvent("msiexec", "/x \"" + driverPath + "\" /passive", true);
			}
				

			System.Threading.Thread.Sleep(3000);

			return (GetDriverVersion(driver) == "");
		}
		
		private static void SystemEvents_SessionEnding(object sender, Microsoft.Win32.SessionEndingEventArgs e)
		{

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
