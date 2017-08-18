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
using Eddie.Lib.Common;

namespace Eddie.Core
{
	public class Platform
	{
		public static Platform Instance;

		public RouteEntry m_routeDefaultRemove;

		protected string m_ApplicationPath = "";
		protected string m_ExecutablePath = "";
		protected string m_UserPath = "";
		protected string m_CommonPath = "";
		protected Dictionary<string, string> m_LocateExecutableCache = new Dictionary<string, string>();

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
			if(m_ApplicationPath == "")
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

		public string GetCommonPath()
		{
			if (m_CommonPath == "")
			{
				m_CommonPath = GetApplicationPath();

				if (Engine.Instance.DevelopmentEnvironment)
				{
					DirectoryInfo di = new DirectoryInfo(GetApplicationPath());
					
					for(;di != null;)
					{						
						if(DirectoryExists(di.FullName + "/common"))
						{
							m_CommonPath = di.FullName + "/common";
							break;
						}
						else
							di = di.Parent;
					}
				}
			}
			return m_CommonPath;
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

		public virtual string GetMonoVersion()
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
		
		public virtual bool IsTraySupported()
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
			if (File.Exists(path) == false)
				return;

			if (FileImmutableGet(path))
			{
				FileImmutableSet(path, false);
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

		public virtual bool FileContentsWriteText(string path, string contents)
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
			File.WriteAllText(path, contents);
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

		public virtual void FileEnsureExecutablePermission(string path)
		{
		}

		public virtual void FileEnsurePermission(string path, string mode)
		{
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

		public virtual void OpenUrl(string url)
		{
			System.Diagnostics.Process.Start(url);
		}

		public virtual string GetDefaultOpenVpnConfigsPath()
		{
			return "";
		}

		public virtual List<string> GetEnvironmentPaths()
		{
			string envPath = Environment.GetEnvironmentVariable("PATH");
			List<string> paths = Utils.StringToList(envPath, EnvPathSep, true, true, true, true);
			return paths;
		}

		public virtual string LocateExecutable(string name)
		{
			if (m_LocateExecutableCache.ContainsKey(name))
				return m_LocateExecutableCache[name];
			
			return LocateExecutable(name, GetEnvironmentPaths());			
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

		// Avoid when possible, but for example under Windows sometime commands are not executable in file-system.
		public virtual void ShellCommandDirect(string command, out string path, out string[] arguments)
		{
			path = "";
			arguments = new string[] { };
			NotImplemented();
		}

		public virtual void ShellSync(string path, string[] arguments, out string stdout, out string stderr, out int exitCode)
		{
			try
			{
				Process p = new Process();

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
			catch(Exception ex)
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
				Process p = new Process();
				p.StartInfo.FileName = path;
				p.StartInfo.Arguments = String.Join(" ", arguments);
				p.StartInfo.WorkingDirectory = "";
				p.StartInfo.CreateNoWindow = true;
				p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				p.Start();
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
		
		public virtual bool SearchTool(string name, string relativePath, ref string path, ref string location)
		{
			return false;
		}

		public virtual Int64 Ping(string host, int timeoutSec)
		{
			Ping pingSender = new Ping();
			PingOptions options = new PingOptions();

			// Use the default TTL value which is 128, but change the fragmentation behavior.
			// options.DontFragment = true;

			// Create a buffer of 32 bytes of data to be transmitted.
			byte[] buffer = RandomGenerator.GetBuffer(32);
			int timeout = timeoutSec * 1000;
			PingReply reply = pingSender.Send(host, timeout, buffer, options);

			if (reply.Status == IPStatus.Success)
				return reply.RoundtripTime;
			else
				return -1;
		}

		public virtual int GetRecommendedSndBufDirective()
		{
			return -1;
		}

		public virtual int GetRecommendedRcvBufDirective()
		{
			return -1;
		}

		public virtual string GetSystemFont()
		{
			return SystemFonts.MenuFont.Name + "," + SystemFonts.MenuFont.Size;
		}

		public virtual string GetSystemFontMonospace()
		{
			string fontName = "";
			if (IsFontInstalled("Consolas"))
				fontName = "Consolas";
			else if (IsFontInstalled("Monospace"))
				fontName = "Monospace";
			else if (IsFontInstalled("DejaVu Sans Mono"))
				fontName = "DejaVu Sans Mono";
			else if (IsFontInstalled("Courier New"))
				fontName = "Courier New";
			else
				fontName = SystemFonts.MenuFont.Name;
			return fontName + "," + SystemFonts.MenuFont.Size;
		}

		public virtual bool IsFontInstalled(string fontName)
		{
			using (var testFont = new Font(fontName, 8))
			{
				return 0 == string.Compare(
				  fontName,
				  testFont.Name,
				  StringComparison.InvariantCultureIgnoreCase);
			}
		}

		public virtual void FlushDNS()
		{
			Engine.Instance.Logs.Log(LogType.Verbose, Messages.ConnectionFlushDNS);

			DnsManager.Invalidate();
		}

		public virtual void RouteAdd(RouteEntry r)
		{
			NotImplemented();
		}

		public virtual void RouteRemove(RouteEntry r)
		{
			NotImplemented();
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
			catch(Exception)
			{

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

		public virtual List<RouteEntry> RouteList()
		{
			NotImplemented();
			return null;
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
					if ((p.MainModule != null) && (p.MainModule.FileName != null))
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
			XmlElement nodeRouteDefaultRemoved = Utils.XmlGetFirstElementByTagName(root, "RouteDefaultRemoved");
			if (nodeRouteDefaultRemoved != null)
			{
				m_routeDefaultRemove = new RouteEntry();
				m_routeDefaultRemove.ReadXML(nodeRouteDefaultRemoved);
			}

			OnRouteDefaultRemoveRestore();
			OnDnsSwitchRestore();
			OnInterfaceRestore();
			OnIpV6Restore();
		}

		public virtual void OnRecoverySave(XmlElement root)
		{
			if (m_routeDefaultRemove != null)
			{
				XmlDocument doc = root.OwnerDocument;

				XmlElement nodeRouteDefaultRemoved = (XmlElement)root.AppendChild(doc.CreateElement("RouteDefaultRemoved"));

				m_routeDefaultRemove.WriteXML(nodeRouteDefaultRemoved);
			}
		}

		public virtual void OnBuildOvpn(OvpnBuilder ovpn)
		{
		}

		public virtual bool OnDnsSwitchDo(IpAddresses dns)
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
			List<RouteEntry> routeEntries = RouteList();
			foreach (RouteEntry routeEntry in routeEntries)
			{
				if (routeEntry.Mask.ToString() == "0.0.0.0")
				{
					m_routeDefaultRemove = routeEntry;

					routeEntry.Remove();

					Recovery.Save();
				}
			}

			return true;
		}

		public virtual bool OnRouteDefaultRemoveRestore()
		{
			if (m_routeDefaultRemove != null)
			{
				m_routeDefaultRemove.Add();
				m_routeDefaultRemove = null;

				Recovery.Save();
			}

			return true;
		}

		public virtual bool OnIpV6Do()
		{
			return true;
		}

		public virtual bool OnIpV6Restore()
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
			NotImplemented();
		}

		public virtual void UnInstallDriver()
		{
			NotImplemented();
		}		
	}
}
