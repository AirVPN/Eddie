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
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Xml;

namespace Eddie.Core
{
	public class Platform
	{
		public static Platform Instance;

		protected string m_applicationPath = "";
		protected string m_executablePath = "";
		protected string m_userPath = "";
		protected string m_commonPath = "";
		protected string m_wireGuardVersion = "unknown";
		protected Dictionary<string, string> m_locateExecutableCache = new Dictionary<string, string>();

		protected List<Json> m_routes = new List<Json>();

		public static bool IsUnix()
		{
			return (Environment.OSVersion.Platform.ToString() == "Unix");
		}

		public static bool IsWindows()
		{
			return (Environment.OSVersion.VersionString.IndexOf("Windows", StringComparison.InvariantCulture) != -1);
		}

		public string GetSystemCode()
		{
			string t = GetCode() + "_" + GetOsArchitecture();
			t = t.Replace(" ", "_");
			t = t.ToLower();
			return t;
		}

		public static string NormalizeArchitecture(string a)
		{
			if (a == "i686") return "x86";
			if (a == "AMD64") return "x64";
			if (a == "x86_64") return "x64";
			return a;
		}

		// ----------------------------------------
		// Constructor
		// ----------------------------------------

		public Platform()
		{
			Instance = this;
		}

		// ----------------------------------------
		// Method
		// ----------------------------------------

		public string GetApplicationPath()
		{
			if (m_applicationPath == "")
				m_applicationPath = GetApplicationPathEx();
			return m_applicationPath;
		}

		public string GetExecutablePath()
		{
			if (m_executablePath == "")
				m_executablePath = GetExecutablePathEx();
			return m_executablePath;
		}

		public string GetUserPath()
		{
			if (m_userPath == "")
				m_userPath = GetUserPathEx();
			return m_userPath;
		}

		// ----------------------------------------
		// Virtual
		// ----------------------------------------

		public virtual string GetCode()
		{
			return "Unknown";
		}

		public virtual string GetCodeInstaller()
		{
			return GetCode();
		}

		public virtual string GetName()
		{
			return "Unknown";
		}

		public virtual string GetVersion()
		{
			return Environment.OSVersion.VersionString;
		}

		public virtual string GetArchitecture()
		{
			if (IntPtr.Size == 8)
			{
				return "x64";
			}
			else if (IntPtr.Size == 4)
			{
				return "x86";
			}
			else
				return "?";
		}

		public virtual string GetNetFrameworkVersion()
		{
			return System.Reflection.Assembly.GetExecutingAssembly().ImageRuntimeVersion;
		}

		public virtual string GetWireGuardVersion()
		{
			// Only one time
			if (m_wireGuardVersion == "unknown")
				m_wireGuardVersion = Engine.Instance.Elevated.DoCommandSync("wireguard-version");
			return m_wireGuardVersion;
		}

		public virtual string GetWireGuardVersionShow()
		{
			string v = GetWireGuardVersion();
			if (v == "") v = LanguageManager.GetText("NotAvailable");
			return v;
		}

		public virtual bool OnInit()
		{
			return true;
		}

		public virtual void OnDeInit()
		{

		}

		public virtual void OnStart()
		{

		}

		public virtual string GetOsArchitecture()
		{
			return "Unknown";
		}

		public virtual void NotImplemented()
		{
			throw new Exception("Not Implemented.");
		}

		public virtual string GetDefaultDataPath()
		{
			return "";
		}

		public virtual bool IsElevatedPrivileges()
		{
			return false;
		}

		public virtual bool IsUnixSystem()
		{
			return false;
		}

		public virtual bool IsLinuxSystem()
		{
			return IsUnixSystem();
		}

		public virtual bool IsWindowsSystem()
		{
			return (IsUnixSystem() == false);
		}

		public virtual string GetElevatedHelperPath()
		{
			string path = GetElevatedHelperPathImpl();
			if (CheckElevatedProcessAllowed(path) == false)
				throw new Exception("Elevated executable invalid");
			return path;
		}

		protected virtual string GetElevatedHelperPathImpl()
		{
			return "";
		}

		public virtual bool CheckElevatedSocketAllowed(System.Net.IPEndPoint localEndpoint, System.Net.IPEndPoint remoteEndpoint)
		{
			return false;
		}

		public virtual bool CheckElevatedProcessAllowed(System.Diagnostics.Process process)
		{
			return false;
		}

		public virtual bool CheckElevatedProcessAllowed(string remotePath)
		{
			return false;
		}

		public virtual bool GetAutoStart()
		{
			return false;
		}

		public virtual bool SetAutoStart(bool value)
		{
			return false;
		}

		public virtual bool AllowService()
		{
			return false;
		}

		public virtual string AllowServiceUserDescription()
		{
			return "";
		}

		public virtual bool GetService()
		{
			return GetService(false);
		}

		public virtual bool GetService(bool direct)
		{
			if (direct)
				return GetServiceImpl();

			if ((Engine.Instance.Elevated != null) && (Engine.Instance.Elevated.ServiceEdition))
			{
				if (Engine.Instance.Elevated.ServiceUninstallAtEnd)
					return false;
				else
					return true;
			}
			else
				return GetServiceImpl();
		}

		public virtual bool SetService(bool value, bool direct)
		{
			if (direct)
				return SetServiceImpl(value);

			if ((Engine.Instance.Elevated != null) && (Engine.Instance.Elevated.ServiceEdition))
			{
				Engine.Instance.Elevated.ServiceUninstallAtEnd = !value;

				Engine.Instance.Elevated.DoCommandSync("service-conn-mode", "mode", Engine.Instance.Elevated.ServiceUninstallAtEnd ? "single" : "service");

				return value;
			}
			else
				return SetServiceImpl(value);
		}

		public virtual void WaitService()
		{

		}

		protected virtual bool GetServiceImpl()
		{
			return false;
		}

		protected virtual bool SetServiceImpl(bool value)
		{
			return false;
		}
		public virtual bool GetServiceUninstallSupportRealtime()
		{
			return false;
		}

		public virtual string NormalizeString(string val)
		{
			return val.Replace("\r\n", "\n");
		}

		public virtual string DirSep
		{
			get
			{
				return "/";
			}
		}

		public virtual string EndOfLineSep
		{
			get
			{
				return "\n";
			}
		}

		public virtual string EnvPathSep
		{
			get
			{
				return ";";
			}
		}

		public virtual char[] CharsNotAllowedInPath
		{
			get
			{
				return Path.GetInvalidPathChars();
			}
		}

		public virtual string NormalizePath(string p)
		{
			p = p.Replace("/", DirSep);
			p = p.Replace("\\", DirSep);

			char[] charsToTrimEnd = { '/', '\\' };
			p = p.TrimEnd(charsToTrimEnd);

			return p;
		}

		public virtual bool FileExists(string path)
		{
			if (path == "")
				return false;

			return (File.Exists(path));
		}

		public virtual bool DirectoryExists(string path)
		{
			return (Directory.Exists(path));
		}

		public virtual void DirectoryEnsure(string path)
		{
			Directory.CreateDirectory(path);
		}

		public virtual string DirectoryTemp()
		{
			return NormalizePath(Path.GetTempPath());
		}

		public virtual string FileTempName(string partialName)
		{
			for (int l = 0; ; l++)
			{
				string path = DirectoryTemp() + DirSep + partialName;
				if (l > 0)
					path += "-" + RandomGenerator.GetHash().Substring(0, 6);
				if (FileExists(path) == false)
					return path;
			}
		}

		public virtual string FileGetNameFromPath(string path)
		{
			return new FileInfo(path).Name;
		}

		public virtual void FileDelete(string path)
		{
			FileDelete(path, false);
		}

		public virtual void FileDelete(string path, bool skipImmutableCheck)
		{
			if (File.Exists(path) == false)
				return;

			// TOFIX
			// skipImmutableCheck exists for pending bug: FileImmutableGet lock forever if the file is pipe.
			if (skipImmutableCheck == false)
			{
				if (FileImmutableGet(path))
				{
					FileImmutableSet(path, false);
				}
			}

			File.Delete(path);
		}

		public virtual void FileMove(string from, string to)
		{
			if (FileExists(to))
				FileDelete(to);

			bool immutable = FileImmutableGet(from);

			if (immutable)
			{
				FileImmutableSet(from, false);
			}
			File.Move(from, to);
			if (immutable)
			{
				FileImmutableSet(to, true);
			}
		}

		public virtual string FileContentsReadText(string path)
		{
			return File.ReadAllText(path);
		}

		public virtual bool FileContentsWriteText(string path, string contents, Encoding encoding)
		{
			bool immutable = false;
			if (FileExists(path))
			{
				string current = FileContentsReadText(path);
				if (current == contents)
					return false;
				immutable = FileImmutableGet(path);
				if (immutable)
					FileImmutableSet(path, false);
			}
			File.WriteAllText(path, contents, encoding);
			if (immutable)
				FileImmutableSet(path, true);
			return true;
		}

		public virtual void FileContentsAppendText(string path, string contents, Encoding encoding)
		{
			bool immutable = false;
			if (FileExists(path))
			{
				immutable = FileImmutableGet(path);
				if (immutable)
					FileImmutableSet(path, false);
			}
			File.AppendAllText(path, contents, encoding);
			if (immutable)
				FileImmutableSet(path, true);
		}

		public virtual byte[] FileContentsReadBytes(string path)
		{
			return File.ReadAllBytes(path);
		}

		public virtual void FileContentsWriteBytes(string path, byte[] contents)
		{
			File.WriteAllBytes(path, contents);
		}

		public virtual string FileGetPhysicalPath(string path)
		{
			// For example under Windows convert a path with hardlink under the physical real path.
			return path;
		}

		public virtual string FileGetAbsolutePath(string path, string basePath)
		{
			if ((path.StartsWith("\"", StringComparison.InvariantCulture)) && (path.EndsWith("\"", StringComparison.InvariantCulture)))
				path = path.Substring(1, path.Length - 2);
			if (System.IO.Path.IsPathRooted(path))
				return path;
			else
				return System.IO.Path.Combine(basePath, path);
		}

		public virtual string FileGetDirectoryPath(string path)
		{
			return new FileInfo(path).DirectoryName;
		}

		public virtual bool FileImmutableGet(string path)
		{
			return false;
		}

		public virtual void FileImmutableSet(string path, bool value)
		{
		}

		public virtual bool FileEnsureExecutablePermission(string path)
		{
			return false;
		}

		public virtual bool FileEnsureCurrentUserOnly(string path)
		{
			// Used only under Windows, for ssh.exe key file.			
			return false;
		}

		public virtual bool FileEnsurePermission(string path, string mode)
		{
			return false;
		}

		public virtual string FileAdaptProcessExec(string path)
		{
			return path;
		}

		public virtual bool FileRunAsRoot(string path)
		{
			return false;
		}

		public virtual bool HasAccessToWrite(string path)
		{
			try
			{
				DirectoryInfo di = new DirectoryInfo(path);
				if (di.Exists == false)
					di.Create();

				string tempPath = path + Platform.Instance.DirSep + "test.tmp";

				using (FileStream fs = File.Create(tempPath, 1, FileOptions.DeleteOnClose))
				{
				}
				return true;
			}
			catch
			{
				return false;
			}
		}

		public virtual string GetExecutableReport(string path)
		{
			return "";
		}

		public virtual bool IsPath(string v)
		{
			// Filename or path (absolute or relative) ?
			return (v.IndexOf(DirSep, StringComparison.InvariantCulture) != -1);
		}

		public virtual string GetApplicationPathEx()
		{
			//Assembly.GetExecutingAssembly().Location
			//return new FileInfo(ExecutablePath).DirectoryName;
			return Path.GetDirectoryName(GetExecutablePath());
		}

		public virtual string GetExecutablePathEx()
		{
			return System.Reflection.Assembly.GetEntryAssembly().Location;
		}

		protected virtual string GetUserPathEx()
		{
			NotImplemented();
			return "";
		}

		public virtual string GetDefaultOpenVpnConfigsPath()
		{
			return "";
		}

		public virtual string GetDefaultWireGuardConfigsPath()
		{
			return "";
		}

		public virtual List<string> GetEnvironmentPaths()
		{
			string envPath = Environment.GetEnvironmentVariable("PATH");
			List<string> paths = envPath.StringToList(EnvPathSep, true, true, true, true);
			return paths;
		}

		public virtual string LocateExecutable(string name)
		{
			if (m_locateExecutableCache.ContainsKey(name))
				return m_locateExecutableCache[name];

			return LocateExecutable(name, GetEnvironmentPaths());
		}

		public virtual string LocateExecutable(string name, List<string> paths)
		{
			foreach (string path in paths)
			{
				string fullPath = NormalizePath(path + "/" + name);
				if (FileExists(fullPath))
				{
					m_locateExecutableCache[name] = fullPath;
					return fullPath;
				}
			}
			return "";
		}

		public virtual int StartProcessAsRoot(string path, string[] arguments, bool consoleMode)
		{
			return 0;
		}

		public virtual bool RunProcessAsRoot(string path, string[] arguments, bool consoleMode)
		{
			int pid = StartProcessAsRoot(path, arguments, consoleMode);
			if (pid == -1) // Special case, macOS for example
			{
				System.Threading.Thread.Sleep(1000);
				return true;
			}
			else
			{
				System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessById(pid);
				process.WaitForExit();
				return (process.ExitCode == 0);
			}
		}

		// Must be called only by SystemExec
		public virtual void ExecSyncCore(string path, string[] arguments, string autoWriteStdin, out string stdout, out string stderr, out int exitCode)
		{
			try
			{
				using (Process p = new Process())
				{
					p.StartInfo.FileName = FileAdaptProcessExec(path);
					p.StartInfo.Arguments = String.Join(" ", arguments);
					p.StartInfo.WorkingDirectory = "";
					p.StartInfo.CreateNoWindow = true;
					p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
					p.StartInfo.UseShellExecute = false;
					p.StartInfo.RedirectStandardOutput = true;
					p.StartInfo.RedirectStandardError = true;
					if (autoWriteStdin != "")
						p.StartInfo.RedirectStandardInput = true;
					p.Start();

					if (autoWriteStdin != "")
					{
						p.StandardInput.Write(autoWriteStdin);
						p.StandardInput.Close();
					}

					stdout = p.StandardOutput.ReadToEnd().Trim();
					stderr = p.StandardError.ReadToEnd().Trim();

					p.WaitForExit();

					exitCode = p.ExitCode;
				}
			}
			catch (Exception ex)
			{
				stdout = "";
				stderr = "Error: " + ex.Message;
				exitCode = -1;
			}
		}

		public virtual void ExecASyncCore(string path, string[] arguments)
		{
			try
			{
				using (Process p = new Process())
				{
					p.StartInfo.FileName = FileAdaptProcessExec(path);
					p.StartInfo.Arguments = String.Join(" ", arguments);
					p.StartInfo.WorkingDirectory = "";
					p.StartInfo.CreateNoWindow = true;
					p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
					p.Start();
				}
			}
			catch (Exception ex)
			{
				Engine.Instance.Logs.Log(ex);
			}
		}

		// Must be called only by SystemExec
		// Special direct mode for user events and direct UI exec.
		// Under Windows use ShellExecute, that show UAC (other exec types above doesn't).
		public virtual bool ExecExecuteCore(string filename, string arguments, bool waitEnd)
		{
			try
			{
				System.Diagnostics.Process p = System.Diagnostics.Process.Start(FileAdaptProcessExec(filename), arguments);
				if (waitEnd)
					p.WaitForExit();
				return true;
			}
			catch (Exception ex)
			{
				Engine.Instance.Logs.Log(ex);
				return false;
			}
		}

		public virtual void OpenUrl(string url)
		{
			System.Diagnostics.Process.Start(url);
		}

		public virtual void OpenFolder(string path)
		{
			System.Diagnostics.Process.Start(path);
		}

		public virtual bool OpenDirectoryInFileManager(string path)
		{
			try
			{
				string dirPath = path;
				if (DirectoryExists(dirPath) == false)
					dirPath = Path.GetDirectoryName(dirPath);
				if (DirectoryExists(dirPath))
				{
					OpenFolder(dirPath);
					return true;
				}
				return false;
			}
			catch
			{
				return false;
			}
		}

		public virtual string LocateResource(string relativePath)
		{
			return "";
		}

		public virtual bool IsPortLocalListening(int port)
		{
			foreach (IPEndPoint e in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners())
			{
				if ((e.Address.ToString() == "127.0.0.1") && (e.Port == port))
					return true;
			}
			return false;
		}

		public virtual bool ProcessKillSoft(Process process)
		{
			return false;
		}

		public virtual int GetOpenVpnRecommendedSndBufDirective()
		{
			return -1;
		}

		public virtual int GetOpenVpnRecommendedRcvBufDirective()
		{
			return -1;
		}

		public virtual bool FetchUrlInternal()
		{
			return false;
		}

		public virtual Json FetchUrl(Json request)
		{
			return null;
		}

		public virtual void FlushDNS()
		{
			Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("ConnectionFlushDNS"));

			DnsManager.Invalidate();
		}

		public void Route(Json jRoute, string action)
		{
			string iface = jRoute["interface"].ValueString;			
			string ifaceFriendly = Platform.Instance.GetFriendlyInterfaceName(iface);
			IpAddress ip = jRoute["destination"].ValueString;
			if (ip.Valid == false)
				throw new Exception("Invalid destination " + jRoute["destination"].ValueString);

			// Check if already exists
			Json jRoutes = Engine.Instance.JsonRouteList();
			bool exists = false;
			foreach (Json jRouteCheck in jRoutes.GetArray())
			{
				if (jRoute["destination"].ValueString == jRouteCheck["destination"].ValueString)
				{
					exists = true;
					break;
				}
			}

			if ((action == "add") && (exists))
			{
				Engine.Instance.Logs.LogVerbose(LanguageManager.GetText("RouteAddExists", ip.ToCIDR(true), ifaceFriendly));
			}
			else if ((action == "remove") && (exists == false))
			{
				Engine.Instance.Logs.LogVerbose(LanguageManager.GetText("RouteDelNotExists", ip.ToCIDR(true), ifaceFriendly));
			}
			else
			{
				try
				{
					Platform.Instance.RouteApply(jRoute, action);
					if (action == "add")
						Engine.Instance.Logs.LogVerbose(LanguageManager.GetText("RouteAddDone", ip.ToCIDR(true), ifaceFriendly));
					else if (action == "remove")
						Engine.Instance.Logs.LogVerbose(LanguageManager.GetText("RouteDelDone", ip.ToCIDR(true), ifaceFriendly));
					// Routes, add {1} for interface {2} failed: {3}
				}
				catch (Exception ex)
				{
					// RouteDelFailed
					string msg = "";
					if (action == "add")
						msg = LanguageManager.GetText("RouteAddFailed", ip.ToCIDR(true), ifaceFriendly, ex.Message);
					else
						msg = LanguageManager.GetText("RouteDelFailed", ip.ToCIDR(true), ifaceFriendly, ex.Message);

					if (action == "add") // 2.21.8 - Throw exception only on ADD
						throw new Exception(msg);
				}
			}

			// Old Recovery // WIP
			/*
			lock (m_routes)
			{
				bool inRecoveryList = false;
				for (int i = 0; i < m_routes.Count; i++)
				{
					Json jRouteC = m_routes[i];
					if ((jRouteC["address"].ValueString == jRoute["address"].ValueString) &&
						(jRouteC["interface"].ValueString == jRoute["interface"].ValueString) &&
						(jRouteC["gateway"].ValueString == jRoute["gateway"].ValueString))
					{
						inRecoveryList = true;
					}
				}

				if (action == "add")
				{
					bool known = false;
					for (int i = 0; i < m_routes.Count; i++)
					{
						Json jRouteC = m_routes[i];
						if ((jRouteC["address"].ValueString == jRoute["address"].ValueString) &&
							 (jRouteC["gateway"].ValueString == jRoute["gateway"].ValueString) &&
							 (jRouteC["type"].ValueString == "removed"))
						{
							Engine.Instance.Logs.LogVerbose(LanguageManager.GetText("RouteAddRemoved", new IpAddress(jRoute["address"].ValueString).ToCIDR(), jRoute["interface"].ValueString));
							m_routes.RemoveAt(i);
							known = true;
							break;
						}
					}

					if (known == false)
					{
						Engine.Instance.Logs.LogVerbose(LanguageManager.GetText("RouteAddNew", new IpAddress(jRoute["address"].Value as string).ToCIDR(), jRoute["interface"].ValueString));
						jRoute["type"].Value = "added";
						m_routes.Add(jRoute);
					}
				}
				else if (action == "remove")
				{
					bool known = false;
					for (int i = 0; i < m_routes.Count; i++)
					{
						Json jRouteC = m_routes[i] as Json;
						if ((jRouteC["address"].Value as string == jRoute["address"].Value as string) &&
							 (jRouteC["gateway"].Value as string == jRoute["gateway"].Value as string) &&
							 (jRouteC["type"].Value as string == "added"))
						{
							Engine.Instance.Logs.LogVerbose(LanguageManager.GetText("RouteDelAdded", new IpAddress(jRoute["address"].Value as string).ToCIDR(), jRoute["interface"].ValueString));
							m_routes.RemoveAt(i);
							known = true;
							break;
						}
					}

					if (known == false)
					{
						Engine.Instance.Logs.LogVerbose(LanguageManager.GetText("RouteDelExist", new IpAddress(jRoute["address"].Value as string).ToCIDR(), jRoute["interface"].ValueString));
						jRoute["type"].Value = "removed";
						m_routes.Add(jRoute);
					}
				}				

				Recovery.Save();
			}
			*/
		}

		public virtual void RouteApply(Json jRoute, string action)
		{
		}

		public virtual IpAddresses ResolveDNS(string host)
		{
			IpAddresses result = new IpAddresses();
			try
			{
				IPHostEntry entry = Dns.GetHostEntry(host);
				foreach (IPAddress ip in entry.AddressList)
					result.Add(ip.ToString());
			}
			catch (Exception)
			{
				/*
				 * Fallback to nslookup.exe
				 * 2.14: Occur some cases (for example Check DNS on IPv6 server without IPv6 DNS (pull-ignore))
				 * when GetHostEntry throw "A non-recoverable error occurred during a database lookup" WSANO_RECOVERY
				 * but at the same time nslookup.exe give the correct answer without any error.
				 * 2.14: For the moment is useless this fallback, because the CheckDNS still works (probably parallel DNS)
				 * Search WSANO_RECOVERY in session.cs for more notes.
				 */
				/*
			   if( (e is System.Net.Sockets.SocketException) && ((e as System.Net.Sockets.SocketException).ErrorCode == 11003)) // WSANO_RECOVERY
			   {
				   try
				   {
					   SystemExec s = new SystemExec();
					   s.Path = LocateExecutable("nslookup.exe");
					   s.Arguments.Add(SystemExec.EscapeHost(host));
					   s.NoDebugLogTemp = true;
					   s.Run();

					   if (s.StdOut.StartsWith("DNS request timed out") == false)
					   {
						   int posAnswer = s.StdOut.IndexOf("\r\n\r\n");
						   if (posAnswer != -1)
						   {
							   // Cleanup. Cannot find a better alternative: when WSANO_RECOVERY occur
							   // Dns.GetHostEntry fail, also C getaddrinfo fail, only nslookup.exe works.
							   string d = s.StdOut.Substring(posAnswer + host.Length);
							   d = d.Replace("Name:", "");
							   d = d.Replace("Aliases:", "");
							   d = d.Replace("Address:", "");
							   d = d.Replace("Addresses:", "");
							   d = d.Replace("\t", " ");
							   d = d.Replace("\r", " ");
							   d = d.Replace("\n", " ");
							   d = UtilsString.StringCleanSpace(d);
							   foreach (string ip in d.Split(' '))
							   {
								   if (IpAddress.IsIP(ip))
									   result.Add(ip);
							   }
						   }
					   }
				   }
				   catch (Exception)
				   {

				   }
			   }				
			   */
			}

			return result;
		}

		public virtual IpAddresses DetectDNS()
		{
			return new IpAddresses();
		}

		public virtual bool WaitTunReady(ConnectionTypes.IConnectionType connection)
		{
			return true;
		}

		public virtual Dictionary<string, string> GetProcessesList()
		{
			Dictionary<string, string> result = new Dictionary<string, string>();

			System.Diagnostics.Process[] processlist = Process.GetProcesses();

			foreach (System.Diagnostics.Process p in processlist)
			{
				try
				{
					//result[p.Id] = p.ProcessName.ToLowerInvariant();
					if ((p.HasExited == false) && (p.MainModule != null) && (p.MainModule.FileName != null))
						result[p.Id.ToString()] = p.MainModule.FileName;
				}
				catch
				{
				}
			}

			return result;
		}

		public virtual Json GetRealtimeNetworkStats()
		{
			Json result = new Json();
			result.EnsureArray();
			NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface adapter in interfaces)
			{
				string id = adapter.Id;
				IPv4InterfaceStatistics istats = adapter.GetIPv4Statistics();
				Int64 bytesRcv = istats.BytesReceived;
				Int64 bytesSnd = istats.BytesSent;

				Json jInterface = new Json();
				jInterface["id"].Value = id;
				//jInterface["ts"].Value = Utils.UnixTimeStamp();
				jInterface["rcv"].Value = bytesRcv;
				jInterface["snd"].Value = bytesSnd;
				result.Append(jInterface);
			}
			return result;
		}

		public virtual void OnReport(Report report)
		{
		}

		public virtual bool OnCheckSingleInstance()
		{
			return true;
		}

		public virtual void OnCheckSingleInstanceClear()
		{
		}

		public virtual bool OnCheckEnvironmentApp()
		{
			return true;
		}

		public virtual bool OnCheckEnvironmentSession()
		{
			return true;
		}

		public virtual void OnCheckUpMonitor()
		{

		}

		public virtual void OnElevated()
		{
		}

		public virtual bool NeedExecuteOutsideAppPath(string exePath)
		{
			return false;
		}

		public virtual void OnNetworkLockManagerInit()
		{
		}

		public virtual string OnNetworkLockRecommendedMode()
		{
			return "";
		}

		public virtual void OnSessionStart()
		{
		}

		public virtual void OnSessionStop()
		{
		}

		public virtual void OnSessionLogEvent(string source, string message)
		{
		}

		// This is called every time, the OnRecoveryLoad only if Recovery.xml exists
		public virtual void OnRecoveryAlways()
		{
		}

		public virtual void OnRecoveryLoad(XmlElement root)
		{
			XmlElement nodeRoutes = root.GetFirstElementByTagName("routes");
			if (nodeRoutes != null)
			{
				foreach (XmlElement nodeRoute in nodeRoutes.ChildNodes)
				{
					Json jRoute = new Json();
					ExtensionsXml.XmlToJson(nodeRoute, jRoute);

					if (jRoute["type"].ValueString == "added")
						RouteApply(jRoute, "remove");
					else if (jRoute["type"].ValueString == "removed")
						RouteApply(jRoute, "add");
				}
			}

			OnDnsSwitchRestore();
			OnInterfaceRestore();
			OnIPv6Restore();
		}

		public virtual void OnRecoverySave(XmlElement root)
		{
			XmlDocument doc = root.OwnerDocument;

			if (m_routes.Count > 0)
			{
				XmlElement nodeRoutes = (XmlElement)root.AppendChild(doc.CreateElement("routes"));
				foreach (Json jRoute in m_routes)
				{
					XmlElement nodeRoute = doc.CreateElement("route") as XmlElement;
					nodeRoutes.AppendChild(nodeRoute);

					ExtensionsXml.XmlFromJson(jRoute, nodeRoute);
				}
			}
		}
		public virtual void CompatibilityAfterProfile()
		{

		}

		public virtual void AdaptConfigOpenVpn(ConfigBuilder.OpenVPN config)
		{
			config.RemoveDirective("windows-driver");
		}

		public virtual void AdaptConfigWireGuard(ConfigBuilder.WireGuard config)
		{
		}

		public virtual string GetConnectionTunDriver(ConnectionTypes.IConnectionType connection)
		{
			return "os";
		}

		public virtual bool OnDnsSwitchDo(ConnectionTypes.IConnectionType connection, IpAddresses dns)
		{
			return true;
		}

		public virtual bool OnDnsSwitchRestore()
		{
			return true;
		}

		public virtual bool OnInterfaceDo(NetworkInterface adapter)
		{
			return true;
		}

		public virtual bool OnInterfaceRestore()
		{
			return true;
		}

		public virtual bool OnIPv6Block()
		{
			return true;
		}

		public virtual bool OnIPv6Restore()
		{
			return true;
		}

		public virtual void OnJsonNetworkInfo(Json jNetworkInfo)
		{
		}

		public virtual void OnJsonNetworkInterfaceInfo(NetworkInterface networkInterface, Json jNetworkInterface)
		{
		}

		public virtual void OnJsonRouteList(Json jRoutesList)
		{
			// Notes to remember:
			// - gateway can be a mac-address (macOS)
			// - gateway can be "link#8" (macOS)
			jRoutesList.FromJson(Engine.Instance.Elevated.DoCommandSync("route-list"));
		}

		public virtual string GetFriendlyInterfaceName(string id)
		{
			return id;
		}

		public virtual bool OsCredentialSystemDefault()
		{
			return true;
		}

		public virtual string OsCredentialSystemName()
		{
			return "";
		}

		public virtual string OsCredentialSystemRead(string name)
		{
			return "";
		}

		public virtual bool OsCredentialSystemWrite(string name, string password)
		{
			return false;
		}

		public virtual bool OsCredentialSystemDelete(string name)
		{
			return false;
		}

		public virtual string FileGetSignedId(string path)
		{
			return "No: Unknown";
		}

		public virtual List<string> GetTrustedPaths()
		{
			List<string> result = new List<string>();
			return result;
		}

		public virtual Elevated.IElevated StartElevated()
		{
			return null;
		}

		public virtual bool GetRequireNextHop()
		{
			return false;
		}

		public virtual bool GetUseOpenVpnRoutes()
		{
			return false;
		}

		public virtual bool GetSupportIPv4()
		{
			return true;
		}

		public virtual bool GetSupportIPv6()
		{
			return true;
		}

		public virtual bool GetSupportWireGuard()
		{
			return (GetWireGuardVersion() != "");
		}

		public virtual NetworkInterface SearchAdapter(string driver)
		{
			return null;
		}

		public virtual string GetDriverVersion(string driver)
		{
			return LanguageManager.GetText("NotImplemented");
		}

		public virtual void OpenVpnEnsureDriverAndAdapterAvailable(string driver, string ifaceName)
		{
			if (GetDriverVersion(driver) != "")
				return;

			throw new Exception(LanguageManager.GetText("OsDriverCannotInstall"));
		}

		public virtual bool OpenVpnCanUninstallDriver(string driver)
		{
			return false;
		}

		public virtual bool OpenVpnUninstallDriver(string driver)
		{
			return false;
		}

		public virtual void OpenVpnDeleteOldTapAdapter()
		{
		}

		public virtual bool PreferHummingbirdIfAvailable()
        {
			return false;
        }
	}
}
