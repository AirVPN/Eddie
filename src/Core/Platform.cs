// <airvpn_source_header>
// This file is part of AirVPN Client software.
// Copyright (C)2014-2014 AirVPN (support@airvpn.org) / https://airvpn.org )
//
// AirVPN Client is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// AirVPN Client is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with AirVPN Client. If not, see <http://www.gnu.org/licenses/>.
// </airvpn_source_header>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Xml;
using System.Text;

namespace AirVPN.Core
{
    public class Platform
    {
		public static Platform Instance;

		private static string m_uname;
		private static string m_architecture;

        // ----------------------------------------
        // Static - Also used before the derivated class is created
        // ----------------------------------------

		public static string ShellPlatformIndipendent(string FileName, string Arguments, string WorkingDirectory, bool WaitEnd, bool ShowWindow)
        {
            try
            {
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
                    return Output.Trim();
                }
                else
                {
                    return "";
                }
            }
            catch (Exception E)
            {
                return E.Message;
            }


        }
        
        public static void Init()
        {   
            if (IsUnix()) // Linux and OSX
            {
				m_uname = ShellPlatformIndipendent("sh", "-c 'uname'", "", true, false).Trim();
                m_architecture = NormalizeArchitecture(ShellPlatformIndipendent("sh", "-c 'uname -m'", "", true, false).Trim());

                if (IsOSX())
                {
					Platform.Instance = new Platforms.Osx();
                }
                else if(IsLinux())
                {
					Platform.Instance = new Platforms.Linux();
                }
            }
            else if (IsWindows())
            {
				Platform.Instance = new Platforms.Windows();
				m_uname = "Windows";
                m_architecture = NormalizeArchitecture(System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE"));
            }


			if (Platform.Instance == null)
            {
				Platform.Instance = new Platform();
				m_uname = "Unknown";
                m_architecture = "Unknown.";
            }

        }

        public static void DeInit()
        {
        }

		private static bool IsUnix()
        {
            return (Environment.OSVersion.Platform.ToString() == "Unix");
        }

		private static bool IsWindows()
        {
            return (Environment.OSVersion.VersionString.IndexOf("Windows") != -1);
        }

		private static bool IsLinux()
        {
			return m_uname == "Linux";
        }

        private static bool IsOSX()
        {
			return m_uname == "Darwin";
        }

		public string GetSystemCode()
        {
			string t = GetCode() + "_" + m_architecture;
            t = t.Replace(" ", "_");
            t = t.ToLower();
            return t;
        }

		public string GetArchitecture()
		{
			return m_architecture;
		}

		public static string NormalizeArchitecture(string a)
		{
			if (a == "x86") return "x86";
			if (a == "i686") return "x86";
			if (a == "x64") return "x64";
			if (a == "AMD64") return "x64";
			if (a == "x86_64") return "x64";
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
        // Virtual
        // ----------------------------------------

		public virtual string GetCode()
		{
			return "Unknown";
		}

        public virtual void NotImplemented()
        {
            throw new Exception("Not Implemented.");
        }

        public virtual bool IsAdmin()
        {
            return false;            
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
                
        public virtual bool IsPath(string v)
        {
            // Filename or path (absolute or relative) ?
            return (v.IndexOf(DirSep) != -1);
        }

        public virtual string GetProgramFolder()
        {
            //Assembly.GetExecutingAssembly().Location
            //return new FileInfo(ExecutablePath).DirectoryName;
			return Path.GetDirectoryName(GetExecutablePath());
        }

		public virtual string GetExecutablePath()
		{
			return System.Reflection.Assembly.GetEntryAssembly().Location;
		}

        public virtual string GetUserFolder()
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
            NotImplemented();
            return "";
        }

		public virtual string Shell(string FileName, string Arguments, bool WaitEnd)
        {
            return Shell(FileName, Arguments, "", WaitEnd, false);
        }

		public virtual string Shell(string FileName, string Arguments, string WorkingDirectory, bool WaitEnd, bool ShowWindow)
        {
            return ShellPlatformIndipendent(FileName, Arguments, WorkingDirectory, WaitEnd, ShowWindow);
        }
        
        public virtual void FlushDNS()
        {
            NotImplemented();
        }

		public virtual void RouteAdd(string Address, string Mask, string Gateway)
		{
			NotImplemented();
		}

		public virtual void RouteRemove(string Address, string Mask, string Gateway)
		{
			NotImplemented();
		}

		public virtual string RouteList()
		{
			NotImplemented();
			return "";
		}

        public virtual void LogSystemInfo()
        {
			Engine.Instance.Log(Engine.LogType.Verbose, "Operating System: " + Platform.Instance.VersionDescription());
        }

		public virtual string GenerateSystemReport()
		{
			string t = "";
			t += "Operating System: " + Platform.Instance.VersionDescription() + "\n";

			NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface adapter in interfaces)
			{
				t += "Network Interface: " + adapter.Name + " (" + adapter.Description + ", ID:" + adapter.Id.ToString() + ") - " + adapter.NetworkInterfaceType.ToString() + " - " + adapter.OperationalStatus.ToString();
				t += " - Down:" + adapter.GetIPv4Statistics().BytesReceived.ToString();
				t += " - Up:" + adapter.GetIPv4Statistics().BytesSent.ToString();
				t += "\n";
			}

			t += "\nRouting:\n";
			t += RouteList();

			return t;
		}

		public virtual void OnSessionStart()
		{
		}

		public virtual void OnSessionStop()
		{
		}

		public virtual void OnRecoveryLoad(XmlElement root)
		{
		}

		public virtual void OnRecoverySave(XmlElement root)
		{
		}

		public virtual string GetDriverAvailable()
		{
			return Messages.NotImplemented;
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

		public virtual bool IpV6Enabled()
		{
			return System.Net.Sockets.Socket.OSSupportsIPv6;
		}

    }
}
