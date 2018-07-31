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
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Xml;
using System.Text;
using Eddie.Common;

namespace Eddie.Core
{
	public class Platform
	{
		public static Platform Instance;

		protected string m_ApplicationPath = "";
		protected string m_ExecutablePath = "";
		protected string m_UserPath = "";
		protected string m_CommonPath = "";
		protected Dictionary<string, string> m_LocateExecutableCache = new Dictionary<string, string>();

		protected List<Json> m_routes = new List<Json>();
		protected List<Json> m_routesDefaultGateway = new List<Json>();

		public static bool IsUnix()
		{
			return (Environment.OSVersion.Platform.ToString() == "Unix");
		}

		public static bool IsWindows()
		{
			return (Environment.OSVersion.VersionString.IndexOf("Windows") != -1);
		}

		private static bool IsLinux()
		{
			return Instance.GetCode() == "Linux";
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
			if (a == "x86") return "x86";
			if (a == "i686") return "x86";
			if (a == "x64") return "x64";
			if (a == "AMD64") return "x64";
			if (a == "x86_64") return "x64";
			if (a == "armv7l") return "armv7l"; // RPI
			return "Unknown";
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
			if (m_ApplicationPath == "")
				m_ApplicationPath = GetApplicationPathEx();
			return m_ApplicationPath;
		}

		public string GetExecutablePath()
		{
			if (m_ExecutablePath == "")
				m_ExecutablePath = GetExecutablePathEx();
			return m_ExecutablePath;
		}

		public string GetUserPath()
		{
			if (m_UserPath == "")
				m_UserPath = GetUserPathEx();
			return m_UserPath;
		}

		// ----------------------------------------
		// Virtual
		// ----------------------------------------

		public virtual string GetCode()
		{
			return "Unknown";
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

		public virtual void OnInit(bool cli)
		{
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

		public virtual bool IsAdminRequired()
		{
			return true;
		}

		public virtual bool IsAdmin()
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

		public virtual void OpenUrl(string url)
		{
			System.Diagnostics.Process.Start(url);
		}

		public virtual bool GetAutoStart()
		{
			return false;
		}

		public virtual bool SetAutoStart(bool value)
		{
			return false;
		}

		public virtual string NormalizeString(string val)
		{
			return val;
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

		public virtual bool NativeInit()
		{
			return true;
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

		public virtual string FileGetPhysicalPath(string path)
		{
			// For example under Windows convert a path with hardlink under the physical real path.
			return path;
		}

		public virtual string FileGetAbsolutePath(string path, string basePath)
		{
			if ((path.StartsWith("\"")) && (path.EndsWith("\"")))
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

		public virtual void FilePermissionsSet(string path, int mod)
		{

		}

		public virtual bool FileEnsureExecutablePermission(string path)
		{
			return false;
		}

		public virtual bool FileEnsureRootOnly(string path)
		{
			// Used only under Windows, for ssh.exe key file.
			return false;
		}

		public virtual bool FileEnsurePermission(string path, string mode)
		{
			return false;
		}

        public virtual bool FileEnsureOwner(string path)
        {
            // Under macOS, a file (for example a .ppw) is created with owner:root. If chmod 600, OpenVPN throw "Permission denied", but not if owned by a normal user.
            // Behiavour never occur under Linux.
            // So, this function is implemented only under macOS and set the owner of the path to the logged user.
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
			return (v.IndexOf(DirSep) != -1);
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

		public virtual string GetUserPathEx()
		{
			NotImplemented();
			return "";
		}

		public virtual string GetDefaultOpenVpnConfigsPath()
		{
			return "";
		}

		public virtual List<string> GetEnvironmentPaths()
		{
			string envPath = Environment.GetEnvironmentVariable("PATH");
			List<string> paths = UtilsString.StringToList(envPath, EnvPathSep, true, true, true, true);
			return paths;
		}

		public virtual string LocateExecutable(string name)
		{
			if (m_LocateExecutableCache.ContainsKey(name))
				return m_LocateExecutableCache[name];

			return LocateExecutable(name, GetEnvironmentPaths ());
		}

		public virtual string LocateExecutable(string name, List<string> paths)
		{
			foreach (string path in paths)
			{
				string fullPath = NormalizePath(path + "/" + name);				
				if (FileExists(fullPath))
				{
					m_LocateExecutableCache[name] = fullPath;
					return fullPath;
				}
			}
			return "";
		}

		public virtual string WaitSignal()
		{
			return "unsupported";
		}

		// Avoid when possible, but for example under Windows sometime commands are not executable in file-system.
		public virtual void ShellCommandDirect(string command, out string path, out string[] arguments)
		{
			path = "";
			arguments = new string[] { };
			NotImplemented();
		}

		// TOCLEAN - Workaround Eddie2, to remove with Eddie3
		public virtual bool CanShellAsNormalUser()
		{
			return true;
		}

		// TOCLEAN - Workaround Eddie2, to remove with Eddie3
		public virtual bool ShellAdaptNormalUser(ref string path, ref string[] arguments)
		{
			return true;
		}

		public virtual void ShellSync(string path, string[] arguments, out string stdout, out string stderr, out int exitCode)
		{
			try
			{
				using (Process p = new Process())
				{
					p.StartInfo.FileName = path;
					p.StartInfo.Arguments = String.Join(" ", arguments);
					p.StartInfo.WorkingDirectory = "";
					p.StartInfo.CreateNoWindow = true;
					p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
					p.StartInfo.UseShellExecute = false;
					p.StartInfo.RedirectStandardOutput = true;
					p.StartInfo.RedirectStandardError = true;					
					p.Start();
					
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

		public virtual void ShellASync(string path, string[] arguments)
		{			
			try
			{
				using (Process p = new Process())
				{
					p.StartInfo.FileName = path;
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

		public virtual bool OpenDirectoryInFileManager(string path)
		{
			try
			{
				string dirPath = path;
				if (DirectoryExists(dirPath) == false)
					dirPath = Path.GetDirectoryName(dirPath);
				if (DirectoryExists(dirPath))
				{
					OpenDirectoryInFileManagerEx(dirPath);
					return true;
				}
				return false;
			}
			catch
			{
				return false;
			}
		}

		protected virtual void OpenDirectoryInFileManagerEx(string path)
		{
			Process.Start(path);
		}

		public virtual string LocateResource(string relativePath)
		{
			return "";
		}

		public virtual Int64 Ping(IpAddress host, int timeoutSec)
		{
			if ((host == null) || (host.Valid == false))
				return -1;

			using (Ping pingSender = new Ping())
			{
				PingOptions options = new PingOptions();

				// Use the default TTL value which is 128, but change the fragmentation behavior.
				// options.DontFragment = true;

				// Create a buffer of 32 bytes of data to be transmitted.
				byte[] buffer = RandomGenerator.GetBuffer(32);
				int timeout = timeoutSec * 1000;
				PingReply reply = pingSender.Send(host.ToString(), timeout, buffer, options);

				if (reply.Status == IPStatus.Success)
					return reply.RoundtripTime;
				else
					return -1;
			}
		}

		public virtual bool ProcessKillSoft(Process process)
		{
			return false;
		}

		public virtual int GetRecommendedSndBufDirective()
		{
			return -1;
		}

		public virtual int GetRecommendedRcvBufDirective()
		{
			return -1;
		}
		
		public virtual void FlushDNS()
		{
			Engine.Instance.Logs.Log(LogType.Verbose, Messages.ConnectionFlushDNS);

			DnsManager.Invalidate();
		}

		public virtual bool RouteAdd(Json jRoute)
		{
			lock (m_routes)
			{
				bool known = false;
				for (int i = 0; i < m_routes.Count; i++)
				{
					Json jRouteC = m_routes[i];
					if ((jRouteC["address"].Value as string == jRoute["address"].Value as string) &&
						 (jRouteC["gateway"].Value as string == jRoute["gateway"].Value as string) &&
						 (jRouteC["type"].Value as string == "removed"))
					{
						Engine.Instance.Logs.LogVerbose(MessagesFormatter.Format(Messages.RouteAddRemoved, new IpAddress(jRoute["address"].Value as string).ToCIDR(), new IpAddress(jRoute["gateway"].Value as string).ToCIDR()));
						m_routes.RemoveAt(i);
						known = true;
						break;
					}
				}

				if (known == false)
				{
					Engine.Instance.Logs.LogVerbose(MessagesFormatter.Format(Messages.RouteAddNew, new IpAddress(jRoute["address"].Value as string).ToCIDR(), new IpAddress(jRoute["gateway"].Value as string).ToCIDR()));
					jRoute["type"].Value = "added";
					m_routes.Add(jRoute);
				}
			}

			Recovery.Save();

			return true;
		}

		public virtual bool RouteRemove(Json jRoute)
		{
			lock (m_routes)
			{
				bool known = false;
				for (int i = 0; i < m_routes.Count; i++)
				{
					Json jRouteC = m_routes[i] as Json;
					if ((jRouteC["address"].Value as string == jRoute["address"].Value as string) &&
						 (jRouteC["gateway"].Value as string == jRoute["gateway"].Value as string) &&
						 (jRouteC["type"].Value as string == "added"))
					{
						Engine.Instance.Logs.LogVerbose(MessagesFormatter.Format(Messages.RouteDelAdded, new IpAddress(jRoute["address"].Value as string).ToCIDR(), new IpAddress(jRoute["gateway"].Value as string).ToCIDR()));
						m_routes.RemoveAt(i);
						known = true;
						break;
					}
				}

				if (known == false)
				{
					Engine.Instance.Logs.LogVerbose(MessagesFormatter.Format(Messages.RouteDelExist, new IpAddress(jRoute["address"].Value as string).ToCIDR(), new IpAddress(jRoute["gateway"].Value as string).ToCIDR()));
					jRoute["type"].Value = "removed";
					m_routes.Add(jRoute);
				}
			}

			Recovery.Save();

			return true;
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
					   SystemShell s = new SystemShell();
					   s.Path = LocateExecutable("nslookup.exe");
					   s.Arguments.Add(SystemShell.EscapeHost(host));
					   s.NoDebugLog = true;
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
		
		public virtual bool WaitTunReady()
		{
			return true;
		}
		
		public virtual bool RestartAsRoot()
		{
			return false;
		}

		public virtual Dictionary<int, string> GetProcessesList()
		{
			Dictionary<int, string> result = new Dictionary<int, string>();

			System.Diagnostics.Process[] processlist = Process.GetProcesses();

			foreach (System.Diagnostics.Process p in processlist)
			{
				try
				{
					//result[p.Id] = p.ProcessName.ToLowerInvariant();
					if ((p.HasExited == false) && (p.MainModule != null) && (p.MainModule.FileName != null))
						result[p.Id] = p.MainModule.FileName;
				}
				catch
				{
				}
			}

			return result;
		}

		public virtual string GetTunStatsMode()
		{
			return "NetworkInterface";
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

		public virtual void OnDaemonOutput(string source, string message)
		{
		}

		// This is called every time, the OnRecoveryLoad only if Recovery.xml exists
		public virtual void OnRecovery()
		{
		}

		public virtual void OnRecoveryLoad(XmlElement root)
		{
			XmlElement nodeRoutes = UtilsXml.XmlGetFirstElementByTagName(root, "routes");
			if (nodeRoutes != null)
			{
				foreach (XmlElement nodeRoute in nodeRoutes.ChildNodes)
				{
					Json jRoute = new Json();
					UtilsXml.XmlToJson(nodeRoute, jRoute);
					m_routes.Add(jRoute);

					if (jRoute["type"].Value as string == "added")
						RouteRemove(jRoute);
					else if (jRoute["type"].Value as string == "removed")
						RouteAdd(jRoute);
				}
			}

			OnRouteDefaultRemoveRestore();
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

					UtilsXml.JsonToXml(jRoute, nodeRoute);
				}
			}
		}

		public virtual void OnBuildOvpn(OvpnBuilder ovpn)
		{
		}

		public virtual bool OnDnsSwitchDo(ConnectionActive connectionActive, IpAddresses dns)
		{
			return true;
		}

		public virtual bool OnDnsSwitchRestore()
		{
			return true;
		}

		public virtual bool OnInterfaceDo(string id)
		{
			return true;
		}

		public virtual bool OnInterfaceRestore()
		{
			return true;
		}

		public virtual bool OnRouteDefaultRemoveDo()
		{
			Json jRoutes = Engine.Instance.JsonRouteList();
			foreach (Json jRoute in jRoutes.GetArray())
			{
				IpAddress address = new IpAddress(jRoute["address"].Value as string);
				if (address.IsDefault)
				{
					if (RouteRemove(jRoute))
						m_routesDefaultGateway.Add(jRoute);
				}
			}

			return true;
		}

		public virtual bool OnRouteDefaultRemoveRestore()
		{
			foreach (Json jRoute in m_routesDefaultGateway)
			{
				RouteAdd(jRoute);
			}
			m_routesDefaultGateway.Clear();

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

		public virtual string GetDriverAvailable()
		{
			return Messages.NotImplemented;
		}

		public virtual bool CanInstallDriver()
		{
			return false;
		}

		public virtual bool CanUnInstallDriver()
		{
			return false;
		}

		public virtual void InstallDriver()
		{
		}

		public virtual void UnInstallDriver()
		{
		}

		public virtual void OnJsonNetworkInfo(Json jNetworkInfo)
		{
		}

		public virtual void OnJsonRouteList(Json jRoutesList)
		{
		}
	}
}
