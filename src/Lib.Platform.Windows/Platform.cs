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
using System.Xml;
using System.Threading;
#if NETSTANDARD
#else
#endif
using Microsoft.Win32;

using Eddie.Core;

namespace Eddie.Platform.Windows
{
	public class Platform : Core.Platform
	{
		private string ServiceName = "EddieElevationService";
		private string OpenVpnDriverTapId = "0901";
		private string OpenVpnDriverTapVersion = "9.24.2";
		private string OpenVpnDriverTapWin7Version = "9.24.2";
		private string OpenVpnDriverWintunId = "wintun";

		private List<NetworkManagerDnsEntry> m_listOldDns = new List<NetworkManagerDnsEntry>();
		private string m_oldMetricInterface = "";
		private int m_oldMetricIPv4 = -1;
		private int m_oldMetricIPv6 = -1;
		private Mutex m_mutexSingleInstance = null;
		private NativeMethods.ConsoleCtrlHandlerRoutine m_consoleCtrlHandlerRoutine;

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
				bool vcRuntimeAvailable = false;

				// <2.20
				/*
				if (File.Exists(Path.Combine(Environment.SystemDirectory, "vcruntime140_1.dll")))
					vcRuntimeAvailable = true;
				*/

				if (GetArchitecture() == "x86")
				{
					int currentVersion = Conversions.ToInt32(Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\" + GetArchitecture().ToUpperInvariant(), "Major", "0"), 0);
					if (currentVersion >= 14)
						vcRuntimeAvailable = true;
					if (File.Exists(Path.Combine(Environment.SystemDirectory, "vcruntime140.dll")) == false)
						vcRuntimeAvailable = false;
				}
				else
				{
					int currentVersion = Conversions.ToInt32(Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\" + GetArchitecture(), "Major", "0"), 0);
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

		public override Eddie.Core.Elevated.IElevated StartElevated()
		{
			ElevatedImpl e = new ElevatedImpl();
			e.Start();

			return e;
		}

		public override bool IsElevatedPrivileges()
		{
			return NativeMethods.IsProcessElevated();

			/* Old C# code			
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

			// Signature check removed, redundant.
			return true;

			/*
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
			*/
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
			if (native.StartsWith("\\\\?\\"))
				native = native.Substring(4);
			if (native.StartsWith("UNC\\"))
				native = "\\" + native.Substring(3);
			return native;
		}

		public override bool FileEnsureCurrentUserOnly(string path)
		{
			// Remove Inheritance
			SystemExec.Exec1(Platform.Instance.LocateExecutable("icacls.exe"), "\"" + SystemExec.EscapePath(path).EscapeQuote() + "\" /c /t /inheritance:d");

			// Set Ownership to Owner
			SystemExec.Exec1(Platform.Instance.LocateExecutable("icacls.exe"), "\"" + SystemExec.EscapePath(path).EscapeQuote() + "\" /c /t /grant \"" + SystemExec.EscapeInsideQuote(Environment.UserName) + "\":F");

			// Remove Authenticated Users
			SystemExec.Exec1(Platform.Instance.LocateExecutable("icacls.exe"), "\"" + SystemExec.EscapePath(path).EscapeQuote() + "\" /c /t /remove:g *S-1-5-11");

			// Remove other groups
			SystemExec.Exec1(Platform.Instance.LocateExecutable("icacls.exe"), "\"" + SystemExec.EscapePath(path).EscapeQuote() + "\" /c /t /remove Administrator \"BUILTIN\\Administrators\" \"NT AUTHORITY\\Authenticated Users\" \"BUILTIN\\Users\" BUILTIN Everyone System Users");

			return true;
		}

		public override string FileAdaptProcessExec(string path)
		{
			// Under Windows, better to escape a specific path. Under other OS don't work.
			return "\"" + base.FileAdaptProcessExec(path) + "\"";
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

			Engine.Instance.Elevated.DoCommandSync("dns-flush", "mode", (Engine.Instance.Options.GetBool("windows.workarounds") ? "max" : "normal"));

			SystemExec.Exec1(LocateExecutable("ipconfig.exe"), "/flushdns");
		}

		public override void RouteApply(Json jRoute, string action)
		{
			IpAddress destination = jRoute["destination"].ValueString;
			string iface = jRoute["interface"].ValueString;

			NetworkInterface networkInterface = GetNetworkInterfaceFromGuid(jRoute["interface"].ValueString);

			if (networkInterface == null)
				throw new Exception(LanguageManager.GetText("NetworkInterfaceNotAvailable"));

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
			c.Parameters["iface"] = interfaceIdx.ToString();
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
				Engine.Instance.NetworkLockManager.AddPlugin(new NetworkLockWindowsFirewall());
			}
		}

		public override string OnNetworkLockRecommendedMode()
		{
			if (IsVistaOrNewer() == false)
				return "none";

			if (Engine.Instance.Options.GetBool("windows.wfp.enable"))
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

			if (Engine.Instance.Options.GetBool("windows.adapters.cleanup"))
				Engine.Instance.Elevated.DoCommandSync("wintun-adapter-removepool", "pool", Constants.WintunPool);

			Recovery.Save();
		}

		public override bool OnIPv6Block()
		{
			if ((IsVistaOrNewer()) && (Engine.Instance.Options.GetBool("windows.wfp.enable")))
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

		public override void AdaptConfigOpenVpn(Core.ConfigBuilder.OpenVPN config)
		{
			base.AdaptConfigOpenVpn(config);

			if ((Engine.Instance.Options.GetBool("windows.ipv6.bypass_dns")) && (Engine.Instance.Options.GetBool("dns.delegate")))
			{
				config.AppendDirectives("pull-filter ignore \"dhcp-option DNS6\"", "OS");
			}

			if (IsWin8OrNewer()) // Win7 throw "WintunCreateAdapter fail"
			{
				if (Engine.Instance.GetOpenVpnTool().VersionAboveOrEqual("2.5"))
				{
					if (Engine.Instance.Options.GetBool("windows.force_old_driver") == false)
					{
						config.AppendDirectives("windows-driver wintun", "OS");
						if (Core.Engine.Instance.Options.Get("network.iface.name") != "")
							config.AppendDirectives("dev-node \"" + Core.Engine.Instance.Options.Get("network.iface.name").EscapeQuote() + "\"", "OS");
					}
				}
			}

			if (Engine.Instance.GetOpenVpnTool().VersionUnder("2.5"))
			{
				config.RemoveDirective("windows-driver");
			}

			if (config.ExistsDirective("windows-driver"))
			{
				string driver = config.GetOneDirectiveText("windows-driver");
				if (driver != "wintun")
					config.RemoveDirective("windows-driver");
			}
		}

		public override string GetConnectionTunDriver(Core.ConnectionTypes.IConnectionType connection)
		{
			if (connection is Core.ConnectionTypes.OpenVPN)
			{
				Core.ConnectionTypes.OpenVPN connectionOpenVPN = connection as Core.ConnectionTypes.OpenVPN;
				string driver = connectionOpenVPN.ConfigStartup.GetOneDirectiveText("windows-driver");
				if (driver == "wintun")
					return OpenVpnDriverWintunId;
				else
					return OpenVpnDriverTapId;
			}
			else if (connection is Core.ConnectionTypes.WireGuard)
			{
				return OpenVpnDriverWintunId;
			}
			else
				throw new Exception("Unexpected connection type");
		}

		public override bool OnDnsSwitchDo(Core.ConnectionTypes.IConnectionType connectionActive, IpAddresses dns)
		{
			if ((Engine.Instance.Options.GetBool("windows.dns.lock")) && (IsVistaOrNewer()) && (Engine.Instance.Options.GetBool("windows.wfp.enable")))
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
					if ((connectionActive.ConfigIPv6 == false) && (layer == "all-out"))
						layer = "ipv4-out";
					if ((connectionActive.BlockedIPv6) && (layer == "all-out"))
						layer = "ipv4-out";
					if ((connectionActive.ConfigIPv4 == false) && (layer == "all-out"))
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
						XmlIf2.SetAttribute("interface", Engine.Instance.Connection.Interface.Id);
						Wfp.AddItem("dns_permit_tap", xmlRule);
					}
				}


				Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("DnsLockActivatedWpf"));
			}

			string mode = Engine.Instance.Options.GetLower("dns.mode");

			if (mode == "auto")
			{
				List<string> dnsInterfacesNamesList = Engine.Instance.Options.Get("dns.interfaces.names").ToLowerInvariant().StringToList();

				string dnsInterfacesTypes = Engine.Instance.Options.Get("dns.interfaces.types").ToLowerInvariant();
				if (dnsInterfacesTypes == "auto")
					dnsInterfacesTypes = "ethernet, virtual";

				List<string> dnsInterfacesTypesList = dnsInterfacesTypes.StringToList();

				Json jNetworkInfo = Engine.Instance.JsonNetworkInfo(); // Realtime
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
						if (dnsIPv4.OnlyIPv4.Equals(dns.OnlyIPv4) == false)
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
							Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("OsWindowsNetworkAdapterDnsDone", "IPv4", entry.Description, (entry.Dns.OnlyIPv4.Count == 0 ? "automatic" : "manual" + " (" + entry.Dns.OnlyIPv4.ToString() + ")"), dns.OnlyIPv4.ToString()));
						}

						IpAddresses dnsIPv6 = new IpAddresses(jNetworkInterface["dns6"].Value as string);
						if (dnsIPv6.OnlyIPv6.Equals(dns.OnlyIPv6) == false)
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

		public override bool OnInterfaceDo(NetworkInterface adapter)
		{
			string id = adapter.Id;

			int interfaceMetricIPv4Value = Engine.Instance.Options.GetInt("windows.metrics.tap.ipv4");
			int interfaceMetricIPv6Value = Engine.Instance.Options.GetInt("windows.metrics.tap.ipv6");
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

			return base.OnInterfaceDo(adapter);
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
						break;
					}
				}

				m_oldMetricInterface = ""; // Do anyway, even if interface not found
			}
			return base.OnInterfaceRestore();
		}

		public override void OnSessionLogEvent(string source, string message)
		{
			if (message.IndexOf("Waiting for TUN/TAP interface to come up") != -1)
			{
				if (Engine.Instance.Options.GetBool("windows.tap_up"))
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

			if (Engine.Instance.Options.GetBool("windows.adapters.cleanup"))
				Engine.Instance.Elevated.DoCommandSync("wintun-adapter-removepool", "pool", Constants.WintunPool);
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

		public override void CompatibilityAfterProfile()
		{
			// < 2.9 - Old Windows Firewall original backup rules path
			string oldPathRulesBackupFirstTime = Engine.Instance.GetPathInData("winfirewallrulesorig.wfw");
			string newPathRulesBackupFirstTime = Environment.SystemDirectory + Platform.Instance.DirSep + "winfirewall_rules_original.airvpn";
			if (Platform.Instance.FileExists(oldPathRulesBackupFirstTime))
			{
				if (Platform.Instance.FileExists(newPathRulesBackupFirstTime))
					Platform.Instance.FileDelete(oldPathRulesBackupFirstTime);
				else
					Platform.Instance.FileMove(oldPathRulesBackupFirstTime, newPathRulesBackupFirstTime);
			}

			string oldPathRulesBackupSession = Engine.Instance.GetPathInData("winfirewallrules.wfw");
			string newPathRulesBackupSession = Environment.SystemDirectory + Platform.Instance.DirSep + "winfirewall_rules_backup.airvpn";
			if (Platform.Instance.FileExists(oldPathRulesBackupFirstTime))
			{
				if (Platform.Instance.FileExists(newPathRulesBackupSession))
					Platform.Instance.FileDelete(oldPathRulesBackupSession);
				else
					Platform.Instance.FileMove(oldPathRulesBackupSession, newPathRulesBackupSession);
			}
		}

		public override void OnJsonNetworkInterfaceInfo(NetworkInterface networkInterface, Json jNetworkInterface)
		{
			base.OnJsonNetworkInterfaceInfo(networkInterface, jNetworkInterface);

			string id = jNetworkInterface["id"].ValueString;

			jNetworkInterface["dns4"].Value = Registry.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces\\" + id.ToLowerInvariant(), "NameServer", "") as string;
			jNetworkInterface["dns6"].Value = Registry.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\Tcpip6\\Parameters\\Interfaces\\" + id.ToLowerInvariant(), "NameServer", "") as string;
			jNetworkInterface["dns6"].Value = null; // CLODOTEMP, re-check
		}

		public override void OnJsonRouteList(Json jRoutesList)
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
			string[] sigcheck = SystemExec.Exec3(sigcheckPath, "-c", "-nobanner", "\"" + SystemExec.EscapePath(path).EscapeQuote() + "\"").Split('\n');

			List<string> list = sigcheck[1].StringToList(",", true, false, false, true);

			if (list[1].Trim('"') == "Signed")
				return "Publisher: " + list[3].Trim('"') + " - Company: " + list[4].Trim('"');

			return "No: Not signed or invalid.";
		}

		public override bool GetRequireNextHop()
		{
			return IsWin7();
		}

		public override bool GetUseOpenVpnRoutes()
		{
			// In 2.21, we implement routes in Eddie,
			// and use our system also in OpenVPN.
			// But in Win7 don't work well.
			return IsWin7();
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

		public override NetworkInterface SearchAdapter(string driver)
		{
			NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface adapter in interfaces)
			{
				if ((int)adapter.NetworkInterfaceType != 53) // Not virtual
					continue;
				if (adapter.OperationalStatus != OperationalStatus.Down)
					continue;

				if ((driver == OpenVpnDriverTapId) && (adapter.Description.ToLowerInvariant().StartsWith("tap-win")))
				{
					return adapter;
				}

				// There is nothing to identify if is a wintun interface, Description maybe simple "Eddie Tunnel" is some Win10...
				if ((driver == OpenVpnDriverWintunId) && (adapter.Description.ToLowerInvariant().StartsWith("tap-win") == false))
				{
					return adapter;
				}
			}

			return null;
		}

		/*
		public override List<string> GetTrustedPaths()
		{
			List<string> list = base.GetTrustedPaths();
			list.Add(Environment.SystemDirectory);
			return list;
		}
		*/

		public override string GetDriverVersion(string driver)
		{
			try
			{
				if (driver == "wintun")
				{
					return "Automatic";
				}
				else if (driver == "0901")
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

		public override void OpenVpnEnsureDriverAndAdapterAvailable(string driver, string ifaceName)
		{
			if (driver == OpenVpnDriverTapId)
			{
				if (Engine.Instance.GetOpenVpnTool().VersionAboveOrEqual("2.5"))
				{
					Engine.Instance.Logs.LogWarning("Deprecated: please uncheck Preferences > Advanced > Force usage of old Tap driver");
				}
			}

			// Need driver install?
			if (driver == OpenVpnDriverTapId)
			{
				string version = GetDriverVersion(OpenVpnDriverTapId);

				string bundleVersion = OpenVpnDriverTapVersion;
				if (IsWin8OrNewer() == false) // Win7
					bundleVersion = OpenVpnDriverTapWin7Version;

				bool needInstall = false;

				if (version == "")
				{
					needInstall = true;
					Engine.Instance.Logs.Log(LogType.InfoImportant, LanguageManager.GetText("OsDriverInstall", driver));
				}
				else if ((Engine.Instance.Options.GetBool("windows.disable_driver_upgrade") == false) && (version.VersionCompare(bundleVersion) == -1))
				{
					Engine.Instance.Logs.Log(LogType.Warning, LanguageManager.GetText("OsDriverNeedUpgrade", driver, version, bundleVersion));
					needInstall = true;
				}

				if (needInstall)
				{
					string driverPath = GetDriverInstallerPath(driver);

					if (driverPath == "")
						throw new Exception(LanguageManager.GetText("OsDriverInstallerNotAvailable", driver));

					if (driver == OpenVpnDriverTapId)
						ExecWithUAC(driverPath, "/S");

					if (GetDriverVersion(OpenVpnDriverTapId) == "")
						throw new Exception(LanguageManager.GetText("OsDriverFailed", driver));
				}
			}


			bool adapterFound = false;
			for (int lap = 0; lap < 2; lap++)
			{
				NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

				foreach (NetworkInterface adapter in interfaces)
				{
					if ((int)adapter.NetworkInterfaceType != 53) // Not virtual
						continue;
					if (adapter.OperationalStatus != OperationalStatus.Down)
						continue;

					// There is nothing to identify if is a wintun interface, Description maybe simple "Eddie Tunnel" is some Win10...
					if ((driver == OpenVpnDriverWintunId) && (adapter.Description.ToLowerInvariant().StartsWith("tap-win") == false) && (adapter.Name == ifaceName))
					{
						Engine.Instance.Logs.LogVerbose("Using WinTun network interface \"" + adapter.Name + " (" + adapter.Description + ")\"");
						adapterFound = true;
						break;
					}

					if ((driver == OpenVpnDriverTapId) && (adapter.Description.ToLowerInvariant().StartsWith("tap-win")) && (adapter.Name == ifaceName))
					{
						Engine.Instance.Logs.LogVerbose("Using Tap0901 network interface \"" + adapter.Name + " (" + adapter.Description + ")\"");
						adapterFound = true;
						break;
					}
				}

				if (adapterFound == false)
				{
					if (lap == 0)
					{
						// Engine.Instance.Logs.LogVerbose(LanguageManager.GetText("OsDriverNoAdapterFound", driver));

						if (driver == OpenVpnDriverWintunId)
						{
							string wintunVersion = Core.Engine.Instance.Elevated.DoCommandSync("wintun-adapter-ensure", "pool", Constants.WintunPool, "name", ifaceName);
							float wintunVersionN = Conversions.ToFloat(wintunVersion) / 100;
							Engine.Instance.Logs.LogVerbose("Added new network interface \"" + ifaceName + "\", Wintun version " + Conversions.ToString(wintunVersionN));
						}
						else if (driver == OpenVpnDriverTapId)
						{
							ExecWithUAC(Software.FindResource("tapctl"), "create --hwid \"root\\tap0901\" --name \"" + ifaceName + "\"");
							Engine.Instance.Logs.LogVerbose("Added new network interface \"" + ifaceName + "\", Tap0901");
						}
					}
				}
				else
				{
					break;
				}
			}

			if (adapterFound == false)
				throw new Exception(LanguageManager.GetText("OsDriverAdapterNotAvailable", driver));
		}

		public override bool OpenVpnCanUninstallDriver(string driver)
		{
			string driverPath = GetDriverUninstallPath(driver);
			if (driverPath == "")
				return false;

			return true;
		}
		public override bool OpenVpnUninstallDriver(string driver)
		{
			string driverPath = GetDriverUninstallPath(driver);
			if (driverPath == "")
				return false;

			if (driver == OpenVpnDriverWintunId)
			{
				// Latest version automatically uninstall driver when latest adapter deleted.				
			}
			else if (driver == OpenVpnDriverTapId)
			{
				ExecWithUAC(driverPath, "/S");
			}

			System.Threading.Thread.Sleep(3000);

			return (GetDriverVersion(driver) == "");
		}

		public override void OpenVpnDeleteOldTapAdapter()
		{
			NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface adapter in adapters)
			{
				if (adapter.Description.ToLowerInvariant().StartsWith("tap-win"))
				{
					ExecWithUAC(Software.FindResource("tapctl"), "delete \"" + adapter.Id + "\"");
				}
			}
		}

		// Specific

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
			if (driver == OpenVpnDriverTapId)
			{
				return Software.FindResource("tap-windows");
			}
			else
				return "";
		}

		private string GetDriverUninstallPath(string driver)
		{
			if (driver == OpenVpnDriverTapId)
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

		public bool ExecWithUAC(string filename, string arguments)
		{
			return Platform.Instance.ExecExecuteCore(filename, arguments, true);
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
