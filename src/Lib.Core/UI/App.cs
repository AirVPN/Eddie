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
using System.Text;
using Eddie.Common;
using System.Diagnostics;

namespace Eddie.Core.UI // ClodoTemp2 - remove?
{
	// Eddie2 Application Helper 
	public class App
	{
		public static void OpenStats(string key)
		{
			if (key == "vpngeneratedovpn")
			{
				if ((Engine.Instance.IsConnected()) && (Engine.Instance.ConnectionActive != null))
				{
					Engine.Instance.OnShowText(Messages.StatsVpnGeneratedOVPN, Engine.Instance.ConnectionActive.OpenVpnProfileStartup.Get());
				}
			}
			else if (key == "vpngeneratedovpnpush")
			{
				if ((Engine.Instance.IsConnected()) && (Engine.Instance.ConnectionActive != null))
				{
					Engine.Instance.OnShowText(Messages.StatsVpnGeneratedOVPN, Engine.Instance.ConnectionActive.OpenVpnProfileWithPush.Get());
				}
			}
			else if (key == "pinger")
			{
				// ClodoTemp must be InvalidatePinger(), but check Refresh.Full steps
				Engine.Instance.InvalidateConnections();
			}
			else if (key == "discovery")
			{
				Engine.Instance.InvalidateDiscovery();
			}
			else if (key == "manifestlastupdate")
			{
				Engine.Instance.RefreshConnections();
			}
			else if (key == "pathapp")
			{
				Platform.Instance.OpenDirectoryInFileManager(Engine.Instance.Stats.Get("PathApp").Value);
			}
			else if (key == "pathdata")
			{
				Platform.Instance.OpenDirectoryInFileManager(Engine.Instance.Stats.Get("PathData").Value);
			}
			else if (key == "pathprofile")
			{
				Platform.Instance.OpenDirectoryInFileManager(Engine.Instance.Stats.Get("PathProfile").Value);
			}
		}

		public static string GetTos()
		{
			return ResourcesFiles.GetString("tos.txt");
		}
		
		public static string TestRunCliAsRoot()
		{
			if (Platform.Instance.GetCode() == "Windows")
			{
				using (Process p = new Process())
				{
					p.StartInfo.Verb = "runas";
					p.StartInfo.WorkingDirectory = "";
					p.StartInfo.CreateNoWindow = true;
					p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
					p.StartInfo.UseShellExecute = true;
					p.StartInfo.FileName = Platform.Instance.GetApplicationPath() + "\\eddie-cli.exe";
					p.StartInfo.Arguments = "test.cli-su a=b";

					// Without "runas" throw "The requested operation requires elevation"
					// "runas" require "UseShellExecute = true"
					// "UseShellExecute = true" it's incompatible with stdout redirection.

					/*
					p.StartInfo.RedirectStandardOutput = true;
					p.StartInfo.RedirectStandardError = true;
					p.Start();

					string stdout = p.StandardOutput.ReadToEnd().Trim();
					string stderr = p.StandardError.ReadToEnd().Trim();

					p.WaitForExit();

					return stdout + stderr;
					*/

					p.Start();
					p.WaitForExit();
					return "";
				}
			}
			else if (Platform.Instance.GetCode() == "MacOS")
			{
				// Probabilmente NO, usare NSTask
				using (Process p = new Process())
				{
					p.StartInfo.Verb = "runas";
					p.StartInfo.WorkingDirectory = "";
					p.StartInfo.CreateNoWindow = true;
					p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
					p.StartInfo.UseShellExecute = true;
					p.StartInfo.FileName = Platform.Instance.GetApplicationPath() + "/eddie-cli";
					p.StartInfo.Arguments = "test.cli-su a=b";

					p.StartInfo.RedirectStandardOutput = true;
					p.StartInfo.RedirectStandardError = true;
					p.Start();

					string stdout = p.StandardOutput.ReadToEnd().Trim();
					string stderr = p.StandardError.ReadToEnd().Trim();

					p.WaitForExit();

					return stdout + stderr;
				}
			}
			else // Linux	
			{
				using (Process p = new Process())
				{
					p.StartInfo.Verb = "runas";
					p.StartInfo.WorkingDirectory = "";
					p.StartInfo.CreateNoWindow = true;
					p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
					p.StartInfo.UseShellExecute = true;
					p.StartInfo.FileName = Platform.Instance.GetApplicationPath() + "\\eddie-cli.exe";
					p.StartInfo.Arguments = "test.cli-su a=b";

					if (Platform.Instance.FileExists(Platform.Instance.GetApplicationPath() + "/eddie-cli.exe"))
					{
						p.StartInfo.FileName = "mono";
						p.StartInfo.Arguments = Platform.Instance.GetApplicationPath() + "/eddie-cli.exe test.cli-su a=b";
					}
					else
					{
						p.StartInfo.FileName = Platform.Instance.GetApplicationPath() + "/eddie-cli";
						p.StartInfo.Arguments = Platform.Instance.GetApplicationPath() + "test.cli-su a=b";
					}

					p.StartInfo.RedirectStandardOutput = true;
					p.StartInfo.RedirectStandardError = true;
					p.Start();

					string stdout = p.StandardOutput.ReadToEnd().Trim();
					string stderr = p.StandardError.ReadToEnd().Trim();

					p.WaitForExit();

					return stdout + stderr;
				}

			}
		}
	}
}
