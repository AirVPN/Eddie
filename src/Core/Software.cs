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
using System.IO;
using System.Text;

namespace Eddie.Core
{
    public class Software
    {		
		public static string OpenVpnDriver = "";
		public static string OpenVpnPath = "";
		public static string OpenVpnVersion = "";

		public static string SshPath = "";
		public static string SshVersion = "";
		public static string SslPath = "";
		public static string SslVersion = "";

		public static string CurlVersion = "";
		public static string CurlPath = "";

		public static void Checking()
		{
			// OpenVPN Driver
			try
			{
				OpenVpnDriver = Platform.Instance.GetDriverAvailable();				
			}
			catch (Exception e)
			{
				Engine.Instance.Logs.Log(LogType.Warning, e);
				OpenVpnDriver = "";
			}

			// OpenVPN Binary
			try
			{
				string executableName = "openvpn";
				
				OpenVpnPath = FindExecutable(executableName);

				if (OpenVpnPath != "")
				{
					OpenVpnVersion = Platform.Instance.Shell(OpenVpnPath, "--version").Trim();
					if( (OpenVpnVersion.StartsWith("Error:")) || (OpenVpnVersion == "") )
					{
						Engine.Instance.Logs.Log(LogType.Verbose, Messages.Format(Messages.BundleExecutableError, executableName, OpenVpnPath));
						Engine.Instance.Logs.Log(LogType.Verbose, "Output: " + OpenVpnVersion);
						Engine.Instance.Logs.Log(LogType.Verbose, Platform.Instance.GetExecutableReport(OpenVpnPath));
						OpenVpnPath = "";
						OpenVpnVersion = "";
					}
					else if (OpenVpnVersion != "")
					{
                        string ver = Utils.ExtractBetween(OpenVpnVersion, "OpenVPN ", " ");
                        string libs = Utils.ExtractBetween(OpenVpnVersion, "library versions:", "\n").Trim();
                        /*
                        int posS = OpenVpnVersion.IndexOf(" ", 8);
                        if (posS > 1)
							OpenVpnVersion = OpenVpnVersion.Substring(0, posS);
                        */
                        OpenVpnVersion = ver + " - " + libs;

                    }
				}
			}
			catch (Exception e)
			{
				Engine.Instance.Logs.Log(LogType.Warning, e);				
				OpenVpnPath = "";
				OpenVpnVersion = "";
			}

			
			// SSH Tunnel Binary
			try
			{
				string executableName = "ssh";
								
				SshPath = FindExecutable(executableName);

				if (SshPath != "")
				{
					string arguments = "-V";
					SshVersion = Platform.Instance.Shell(SshPath, arguments).Trim();
					if( (SshVersion.StartsWith("Error:")) || (SshVersion == ""))
					{
						Engine.Instance.Logs.Log(LogType.Verbose, Messages.Format(Messages.BundleExecutableError, executableName, SshPath));
						Engine.Instance.Logs.Log(LogType.Verbose, "Output: " + SshVersion);
						Engine.Instance.Logs.Log(LogType.Verbose, Platform.Instance.GetExecutableReport(SshPath));						
						SshPath = "";
						SshVersion = "";
					}
					else if (SshVersion != "")
					{
						if (Platform.Instance.IsWindowsSystem()) 
							SshVersion = SshVersion.Replace(": Release", "").Trim();
					}
				}
			}
			catch (Exception e)
			{
				Engine.Instance.Logs.Log(LogType.Warning, e);				
				SshPath = "";
				SshVersion = "";
			}

			// SSL Tunnel Binary
			try
			{
				string executableName = "ssl";
								
				SslPath = FindExecutable(executableName);

				
				if (SslPath != "")
				{
					string arguments = "-version";					
					SslVersion = Platform.Instance.Shell(SslPath, arguments).Trim();
					if ((SslVersion.StartsWith("Error:")) || (SslVersion == ""))
					{
						Engine.Instance.Logs.Log(LogType.Verbose, Messages.Format(Messages.BundleExecutableError, executableName, SslPath));
						Engine.Instance.Logs.Log(LogType.Verbose, "Output: " + SslVersion);
						Engine.Instance.Logs.Log(LogType.Verbose, Platform.Instance.GetExecutableReport(SslPath));
						SslPath = "";
						SslVersion = "";
					}
					else
					{
						int posS = SslVersion.IndexOf(" ", 8);
						if (posS > 1)
							SslVersion = SslVersion.Substring(0, posS);
					}
				}
			}
			catch (Exception e)
			{
				Engine.Instance.Logs.Log(LogType.Warning, e);				
				SslPath = "";
				SslVersion = "";
			}

			// CUrl			
			try
			{
				string executableName = "curl";

				CurlPath = FindExecutable(executableName);

				if (CurlPath != "")
				{
					string arguments = "--version";
					CurlVersion = Platform.Instance.Shell(CurlPath, arguments).Trim();
					if ((CurlVersion.StartsWith("Error:")) || (CurlVersion == ""))
					{
						Engine.Instance.Logs.Log(LogType.Verbose, Messages.Format(Messages.BundleExecutableError, executableName, CurlPath));
						Engine.Instance.Logs.Log(LogType.Verbose, "Output: " + CurlVersion);
						Engine.Instance.Logs.Log(LogType.Verbose, Platform.Instance.GetExecutableReport(CurlPath));
						CurlPath = "";
						CurlVersion = "";
					}
					else
					{
						int posS = CurlVersion.IndexOf("\n");
						if (posS > 1)
							CurlVersion = CurlVersion.Substring(0, posS);
					}
				}
			}
			catch (Exception e)
			{
				Engine.Instance.Logs.Log(LogType.Warning, e);
				CurlPath = "";
				CurlVersion = "";
			}

			// Local Time in the past
			if (DateTime.UtcNow < Constants.dateForPastChecking)
				Engine.Instance.Logs.Log(LogType.Fatal, Messages.WarningLocalTimeInPast);
		}

		public static void Log()
		{
			if(OpenVpnDriver != "")
			{
				Engine.Instance.Logs.Log(LogType.Info, "OpenVPN Driver - " + OpenVpnDriver);
			}
			else
			{
				Engine.Instance.Logs.Log(LogType.Error, "OpenVPN Driver - " + Messages.OsDriverNotAvailable);
			}

			if(OpenVpnPath != "")
			{
				Engine.Instance.Logs.Log(LogType.Info, "OpenVPN - Version: " + OpenVpnVersion + " (" + OpenVpnPath + ")");
			}
			else
			{
				Engine.Instance.Logs.Log(LogType.Error, "OpenVPN - " + Messages.NotAvailable);
			}

			if(SshPath != "")
			{
				Engine.Instance.Logs.Log(LogType.Info, "SSH - Version: " + SshVersion + " (" + SshPath + ")");
			}
			else
			{
				Engine.Instance.Logs.Log(LogType.Warning, "SSH - " + Messages.NotAvailable);
			}

			if(SslPath != "")
			{
				Engine.Instance.Logs.Log(LogType.Info, "SSL - Version: " + SslVersion + " (" + SslPath + ")");
			}
			else
			{
				Engine.Instance.Logs.Log(LogType.Warning, "SSL - " + Messages.NotAvailable);
			}            
        }

        public static bool FileExists(string path)
        {
            if (path == "")
                return false;
            if (Platform.Instance.FileExists(path) == false)
                return false;

            return true;
        }

		public static string FindExecutable(string name)
		{
			string filename = name;

			if (Platform.Instance.IsWindowsSystem())
			{
				if (name == "openvpn")
					filename = "openvpn.exe";
				else if (name == "ssh")
					filename = "plink.exe";
				else if (name == "ssl")
					filename = "stunnel.exe";
				else
					filename = name + ".exe";
			}
			else
			{
				if (name == "ssl")
					filename = "stunnel";
			}

			string path = FindResource(filename, name);
            
			if (path != "") // 2.8
				Platform.Instance.EnsureExecutablePermissions(path);
			
			return path;
		}

		public static string FindResource(string filename)
		{
			return FindResource(filename, "");
		}

		public static string FindResource(string filename, string name)
		{
            // Custom location (2.11, below 'Same path' in 2.10)
            if (name != "")
            {
                string path = Platform.Instance.NormalizePath(Engine.Instance.Storage.Get("executables." + name));
                if (FileExists(path))
                    return path;
            }

            // Same path
            {
				string path = Platform.Instance.NormalizePath(Platform.Instance.GetProgramFolder() + "/" + filename);
				if (FileExists(path))
					return path;
			}
            
			// GIT source tree
			if (Engine.Instance.DevelopmentEnvironment)
			{
				string path = Platform.Instance.NormalizePath(Platform.Instance.GetGitDeployPath() + filename);	
				if (FileExists(path))
					return path;
			}

			// System
			List<string> names = new List<string>();			
			if (filename == "stunnel")
			{				
				// For example, under Ubuntu is 'stunnel4', under Fedora is 'stunnel'.
				names.Add("stunnel5");
				names.Add("stunnel4");				
			}
			names.Add(filename);

			foreach (string fileNameAlt in names)
			{
				// Linux
				if (Platform.Instance.IsUnixSystem())
				{
					string pathBin = "/usr/bin/" + fileNameAlt;
					if (FileExists(pathBin))
						return pathBin;

					string pathSBin = "/usr/sbin/" + fileNameAlt;
					if (FileExists(pathSBin))
						return pathSBin;
				}
			}

			return "";
		}



	}
}
