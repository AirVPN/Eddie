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
using System.IO;
using System.Text;

namespace AirVPN.Core
{
    public class Software
    {		
		public static bool IPV6 = false;

		public static string OpenVpnDriver = "";
		public static string OpenVpnPath = "";
		public static string OpenVpnVersion = "";

		public static string SshPath = "";
		public static string SshVersion = "";
		public static string SslPath = "";
		public static string SslVersion = "";

		public static void Checking()
		{
			// OpenVPN Driver
			try
			{
				OpenVpnDriver = Platform.Instance.GetDriverAvailable();				
			}
			catch (Exception e)
			{
				Engine.Instance.Log(Engine.LogType.Warning, e);
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
					if (OpenVpnVersion != "")
					{
						int posS = OpenVpnVersion.IndexOf(" ", 8);
						if (posS > 1)
							OpenVpnVersion = OpenVpnVersion.Substring(0, posS);
					}
				}
			}
			catch (Exception e)
			{
				Engine.Instance.Log(Engine.LogType.Warning, e);
				OpenVpnPath = "";
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
					if (SshVersion != "")
					{
						if (Platform.Instance.IsWindowsSystem()) 
							SshVersion = SshVersion.Replace(": Release", "").Trim();
					}
				}
			}
			catch (Exception e)
			{
				Engine.Instance.Log(Engine.LogType.Warning, e);
				SshPath = "";
			}

			
			// SSL Tunnel Binary
			try
			{
				string executableName = "ssl";
								
				SslPath = FindExecutable(executableName);

				
				if (SslPath != "")
				{
					string arguments = "-version";					
					SslVersion = Platform.Instance.Shell(SslPath, arguments);
					int posS = SslVersion.IndexOf(" ", 8);					
					if (posS > 1)
						SslVersion = SslVersion.Substring(0, posS);
				}
			}
			catch (Exception e)
			{
				Engine.Instance.Log(Engine.LogType.Warning, e);
				SshPath = "";
			}

			// IPV6
			try
			{
				IPV6 = Platform.Instance.IpV6Enabled();				
			}
			catch (Exception)
			{
				IPV6 = false;
			}		
	
			// Local Time in the past
			if (DateTime.UtcNow < Constants.dateForPastChecking)
				Engine.Instance.Log(Engine.LogType.Fatal, Messages.WarningLocalTimeInPast);
		}

		public static void Log()
		{
			if(OpenVpnDriver != "")
			{
				Engine.Instance.Log(Engine.LogType.Info, "OpenVPN Driver - " + OpenVpnDriver);
			}
			else
			{
				Engine.Instance.Log(Engine.LogType.Error, "OpenVPN Driver - " + Messages.NotAvailable);
			}

			if(OpenVpnPath != "")
			{
				Engine.Instance.Log(Engine.LogType.Info, "OpenVPN - Version: " + OpenVpnVersion + " (" + OpenVpnPath + ")");
			}
			else
			{
				Engine.Instance.Log(Engine.LogType.Error, "OpenVPN - " + Messages.NotAvailable);
			}

			if(SshPath != "")
			{
				Engine.Instance.Log(Engine.LogType.Info, "SSH - Version: " + SshVersion + " (" + SshPath + ")");
			}
			else
			{
				Engine.Instance.Log(Engine.LogType.Warning, "SSH - " + Messages.NotAvailable);
			}

			if(SslPath != "")
			{
				Engine.Instance.Log(Engine.LogType.Info, "SSL - Version: " + SslVersion + " (" + SslPath + ")");
			}
			else
			{
				Engine.Instance.Log(Engine.LogType.Warning, "SSL - " + Messages.NotAvailable);
			}

			Engine.Instance.Log(Engine.LogType.Info, "IPV6: " + (IPV6 ? Messages.Available : Messages.NotAvailable));
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
			}
			else
			{
				if (name == "ssl")
					filename = "stunnel";
			}

			string path = FindResource(filename, name);
			
			return path;
		}

		public static string FindResource(string filename)
		{
			return FindResource(filename, "");
		}

		public static string FindResource(string filename, string name)
		{	
			// Same path
			{
				string path = Platform.Instance.NormalizePath(Platform.Instance.GetProgramFolder() + "/" + filename);
				if (File.Exists(path))
					return path;
			}

			// Custom location
			if(name != "")
			{
				string path = Platform.Instance.NormalizePath(Engine.Instance.Storage.Get("executables." + name));
				if (File.Exists(path))
					return path;
			}

			// GIT source tree
			if (Engine.Instance.DevelopmentEnvironment)
			{
				string path = Platform.Instance.NormalizePath(Platform.Instance.GetGitDeployPath() + filename);	
				if (File.Exists(path))
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
					if (File.Exists(pathBin))
						return pathBin;

					string pathSBin = "/usr/sbin/" + fileNameAlt;
					if (File.Exists(pathSBin))
						return pathSBin;
				}
			}

			return "";
		}



	}
}
