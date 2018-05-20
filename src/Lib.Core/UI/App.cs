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

namespace Eddie.Core.UI
{
	// Eddie2 Application Helper
	public class App
	{
		public static Json Manifest;

		public static Json Command(Json data)
		{
			string cmd = data["command"].Value as string;
			if (cmd == "ui.manifest")
				Manifest = data;
			else if (cmd == "ui.stats.pathprofile")
			{
				Platform.Instance.OpenDirectoryInFileManager(Engine.Instance.Stats.Get("PathProfile").Value);
			}
			else if (cmd == "ui.stats.pathdata")
			{
				Platform.Instance.OpenDirectoryInFileManager(Engine.Instance.Stats.Get("PathData").Value);
			}
			else if (cmd == "ui.stats.pathapp")
			{
				Platform.Instance.OpenDirectoryInFileManager(Engine.Instance.Stats.Get("PathApp").Value);
			}
			else if (cmd == "man")
			{
				string format = "text";
				if (data.HasKey("format"))
					format = data["format"].Value as string;
				string body = Engine.Instance.Storage.GetMan(format);
				Engine.Instance.OnShowText("man", body);
			}
			else if (cmd == "ui.show.manifest")
				Engine.Instance.OnShowText("ui.show.manifest", Manifest.ToJsonPretty());
			else if (cmd == "ui.show.os.info")
				Engine.Instance.OnShowText("ui.show.os.info", Engine.Instance.GenerateOsInfo().ToJsonPretty());
			else if (cmd == "tor.guard")
				Engine.Instance.Logs.LogVerbose("Tor Guard IPs:" + TorControl.GetGuardIps(true).ToString());
			else if (cmd == "tor.NEWNYM")
				TorControl.SendNEWNYM();
			else if (cmd == "ip.exit")
				Engine.Instance.Logs.LogVerbose(Engine.Instance.DiscoverExit().ToString());
			else if (cmd == "test.cli-su")
				Engine.Instance.Logs.LogVerbose(TestRunCliAsRoot());
			else if (cmd == "test.log.info")
				Engine.Instance.Logs.Log(LogType.InfoImportant, "Test log\nInfo");
			else if (cmd == "test.log.infoimportant")
				Engine.Instance.Logs.Log(LogType.InfoImportant, "Test log\nInfo Important");
			else if (cmd == "test.log.warning")
				Engine.Instance.Logs.Log(LogType.Warning, "Test log\nWarning");
			else if (cmd == "test.log.error")
				Engine.Instance.Logs.Log(LogType.Error, "Test log\nError");
			else if (cmd == "test.log.fatal")
				Engine.Instance.Logs.Log(LogType.Fatal, "Test log\nFatal");			

			return null;
		}

		public static void RunCommandString(string command)
		{
			command = command.Trim();

			Engine.Instance.Logs.Log(LogType.Verbose, "Running command: " + command);

			Json j = new Json();

			if (command.StartsWith("openvpn "))
			{
				string openvpnManagementCommand = command.Substring(8).Trim();

				j["command"].Value = "openvpn_management";
				j["management_command"].Value = openvpnManagementCommand;
			}
			else if (command.StartsWith("tor "))
			{
				string torControlCommand = command.Substring(4).Trim();

				j["command"].Value = "tor_control";
				j["control_command"].Value = torControlCommand;
			}
			else
			{
				CommandLine cmd = new CommandLine(command, false, true);
				j["command"].Value = cmd.Get("action", "");
				foreach (KeyValuePair<string, string> kp in cmd.Params)
					if (kp.Key != "action")
						j[kp.Key].Value = kp.Value;
			}

			Engine.Instance.Command(j);
		}

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

		public static void OpenUrl(string url)
		{
			Platform.Instance.OpenUrl(url);
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
