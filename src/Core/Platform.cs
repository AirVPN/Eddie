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

        // ----------------------------------------
        // Static - Also used before the derivated class is created
        // ----------------------------------------

        public static string ShellPlatformIndipendent(string FileName, string Arguments, string WorkingDirectory, bool WaitEnd, bool ShowWindow, bool noDebugLog)
		{
			if (WaitEnd)
			{
				//lock (Instance) // Removed in 2.11.4
				{
					return ShellPlatformIndipendentEx(FileName, Arguments, WorkingDirectory, WaitEnd, ShowWindow, noDebugLog);
				}
			}
			else
				return ShellPlatformIndipendentEx(FileName, Arguments, WorkingDirectory, WaitEnd, ShowWindow, noDebugLog);
		}

		public static string ShellPlatformIndipendentEx(string FileName, string Arguments, string WorkingDirectory, bool WaitEnd, bool ShowWindow, bool noDebugLog)
        {			
			try
            {
				int startTime = Environment.TickCount;

                Process p = new Process();

                p.StartInfo.Arguments = Arguments;

                if (WorkingDirectory != "")
                    p.StartInfo.WorkingDirectory = WorkingDirectory;

                p.StartInfo.FileName = FileName;

                if (ShowWindow == false)
                {
                    //#do not show DOS window
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                }

                if (WaitEnd)
                {
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
					p.StartInfo.RedirectStandardError = true;
                }

                p.Start();

                if (WaitEnd)
                {
                    string Output = p.StandardOutput.ReadToEnd() + "\n" + p.StandardError.ReadToEnd();
                    p.WaitForExit();

                    Output = Output.Trim();

                    if (noDebugLog == false) // Avoid recursion
                    {
                        if ((Engine.Instance != null) && (Engine.Instance.Storage != null) && (Engine.Instance.Storage.GetBool("log.level.debug")))
                        {
                            int endTime = Environment.TickCount;
                            int deltaTime = endTime - startTime;
                            string message = "Shell of '" + FileName + "','" + Arguments + "' done sync in " + deltaTime.ToString() + " ms, Output: " + Output;
                            message = Utils.RegExReplace(message, "[a-zA-Z0-9+/]{30,}=","{base64-omissis}");
                            Engine.Instance.Logs.Log(LogType.Verbose, message);
                        }
                    }

                    return Output;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception E)
            {
                return "Error:" + E.Message; // 2.8
            }


        }
        
		/*
        public static void Init()
        {   
            if (IsUnix())
            {
				Platform.Instance = new Platforms.Linux();
            }
            else if (IsWindows())
            {
				Platform.Instance = new Platforms.Windows();				             
            }

			Platform.Instance.OnInit();
        }
		*/

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

        public virtual void OnInit()
        {

        }

        public virtual void OnDeInit()
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

		

		public virtual string VersionDescription()
        {
            return Environment.OSVersion.VersionString;
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

        public virtual bool FileImmutableGet(string path)
        {
            return false;
        }

        public virtual void FileImmutableSet(string path, bool value)
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

        public virtual string ShellCmd(string Command)
        {
            return ShellCmd(Command, false);
        }

        public virtual string ShellCmd(string Command, bool noDebugLog)
        {
            NotImplemented();
            return "";
        }

		public virtual string Shell(string FileName, string Arguments)
		{
			return Shell(FileName, Arguments, "", true, false, false);
		}

		public virtual string Shell(string FileName, string Arguments, bool WaitEnd)
        {
            return Shell(FileName, Arguments, "", WaitEnd, false, false);
        }

        public virtual string Shell(string FileName, string Arguments, string WorkingDirectory, bool WaitEnd, bool ShowWindow, bool noDebugLog)
        {
            return ShellPlatformIndipendent(FileName, Arguments, WorkingDirectory, WaitEnd, ShowWindow, noDebugLog);
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

        /*
        public virtual int Ping(string host, int timeout)
        {
            string result = ShellCmd("ping " + host + " -n 1");

            string sMS = Utils.ExtractBetween(result, "Maximum = ", "ms,");
            int iMS;
            if (int.TryParse(sMS, out iMS))
                return iMS;
            else
                return -1;
        }
        */

        public virtual bool SearchTool(string name, string relativePath, ref string path, ref string location)
        {
            return false;
        }

        public virtual Int64 Ping(string host, int timeoutSec)
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();

            // Use the default Ttl value which is 128,
            // but change the fragmentation behavior.
            //options.DontFragment = true;

            // Create a buffer of 32 bytes of data to be transmitted.								
            byte[] buffer = RandomGenerator.GetBuffer(32);
            int timeout = timeoutSec * 1000;
            PingReply reply = pingSender.Send(host, timeout, buffer, options);

            if (reply.Status == IPStatus.Success)
                return reply.RoundtripTime;
            else
                return -1;            
        }

		public virtual void EnsureExecutablePermissions(string path)
		{
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
            NotImplemented();
        }

		public virtual void RouteAdd(RouteEntry r)
		{
			NotImplemented();
		}

		public virtual void RouteRemove(RouteEntry r)
		{
			NotImplemented();
		}

        public virtual void ResolveWithoutAnswer(string host)
        {
            try
            {
                Dns.GetHostEntry(host);
            }
            catch(Exception)
            {

            }
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

			string megareport = "";

			foreach (System.Diagnostics.Process p in processlist)
			{
				megareport += p.SessionId + ":";
				try
				{
                    //result[p.Id] = p.ProcessName.ToLowerInvariant();					
                    if ((p.MainModule != null) && (p.MainModule.FileName != null))
                        result[p.Id] = p.MainModule.FileName;
				}
				catch
                {
					megareport += "ERRORE";
				}

				megareport += "\r\n";
			}

			return result;
		}

		public virtual string GetTunStatsMode()
		{
			return "NetworkInterface";			
		}

        public virtual void LogSystemInfo()
        {
			Engine.Instance.Logs.Log(LogType.Verbose, "Operating System: " + Platform.Instance.VersionDescription());
        }

        public virtual string GenerateEnvironmentReport()
        {
            string t = "";

            t += "Eddie version: " + Constants.VersionDesc + "\n";
            t += "Eddie OS build: " + Platform.Instance.GetSystemCode() + "\n";
            t += "OS type: " + Platform.Instance.GetCode() + "\n";
            t += "OS name: " + Platform.Instance.GetName() + "\n";
            t += "OS description: " + Platform.Instance.VersionDescription() + "\n";
            t += "Mono /.Net Framework: " + Platform.Instance.GetMonoVersion() + "\n";

            t += "OpenVPN driver: " + Software.OpenVpnDriver + "\n";            
            t += "OpenVPN: " + Software.GetTool("openvpn").Version + " (" + Software.GetTool("openvpn").Path + ")\n";
            t += "SSH: " + Software.GetTool("ssh").Version + " (" + Software.GetTool("ssh").Path + ")\n";
            t += "SSL: " + Software.GetTool("ssl").Version + " (" + Software.GetTool("ssl").Path + ")\n";
            t += "curl: " + Software.GetTool("curl").Version + " (" + Software.GetTool("curl").Path + ")\n";

            t += "Profile path: " + Engine.Instance.Storage.GetProfilePath() + "\n";
            t += "Data path: " + Storage.DataPath + "\n";
            t += "Application path: " + Platform.Instance.GetApplicationPath() + "\n";
            t += "Executable path: " + Platform.Instance.GetExecutablePath() + "\n";
            t += "Command line arguments (" + CommandLine.SystemEnvironment.Params.Count.ToString() + "): " + CommandLine.SystemEnvironment.GetFull() + "\n";

            return t;
        }

        public virtual string GenerateSystemReport()
		{
			string t = "";
			t += "Operating System: " + Platform.Instance.VersionDescription() + "\n";

            /*
			if(Platform.Instance.GetSystemFont() != "")
            	t += "System font: " + Platform.Instance.GetSystemFont() + "\n";
			if(Platform.Instance.GetSystemFontMonospace() != "")
            	t += "System monospace font: " + Platform.Instance.GetSystemFontMonospace() + "\n";
            */
            
            try
			{
				NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
				foreach (NetworkInterface adapter in interfaces)
				{
					t += "Network Interface: " + adapter.Name + " (" + adapter.Description + ", ID:" + adapter.Id.ToString() + ") - " + adapter.NetworkInterfaceType.ToString() + " - " + adapter.OperationalStatus.ToString();
					//t += " - Down:" + adapter.GetIPv4Statistics().BytesReceived.ToString();
					//t += " - Up:" + adapter.GetIPv4Statistics().BytesSent.ToString();
					t += "\n";
				}
			}
			catch (Exception)
			{
				t += "Unable to fetch network interfaces.\n";
			}

			t += "\nRouting:\n";
			try
			{
				List<RouteEntry> routeEntries = RouteList();
				foreach (RouteEntry routeEntry in routeEntries)
				{
					t += routeEntry.ToString() + "\n";
				}
			}
			catch (Exception)
			{
				t += "Unable to fetch routes.\n";
			}


			t += "\nDefault gateways:\n";
			List<string> gatewaysList = new List<string>();
			foreach (NetworkInterface f in NetworkInterface.GetAllNetworkInterfaces())
			{
				if (f.OperationalStatus == OperationalStatus.Up)
				{
					foreach (GatewayIPAddressInformation d in f.GetIPProperties().GatewayAddresses)
					{
						string ip = d.Address.ToString();
						if ((IpAddress.IsIP(ip)) && (ip != "0.0.0.0") && (gatewaysList.Contains(ip) == false))
						{
							//gatewaysList.Add(ip);

							t += ip + ", " + f.Description + "\n";
						}
					}
				}
			}

			return NormalizeString(t);
		}

        public virtual bool OnCheckSingleInstance()
        {
            return true;
        }

        public virtual void OnCheckSingleInstanceClear()
        {
        }

        public virtual void OnAppStart()
		{
		}

		public virtual bool OnCheckEnvironment()
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

		public virtual bool OnDnsSwitchDo(string dns)
		{
			return true;
		}

		public virtual bool OnDnsSwitchRestore()
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

        public virtual string GetProjectPath()
        {
            DirectoryInfo di = new DirectoryInfo(GetApplicationPath());

            for (;;)
            {
                if ((FileExists(di.FullName + "/README.md")) && (FileContentsReadText(di.FullName + "/README.md").Contains("Eddie - OpenVPN GUI")))
                    return di.FullName;
                else
                {
                    di = di.Parent;
                    if (di == null)
                        return "";
                }
            }
        }

        public virtual string GetGitDeployPath()
        {
            return GetProjectPath() + "/deploy/" + Platform.Instance.GetSystemCode() + "/";
        }
    }
}
