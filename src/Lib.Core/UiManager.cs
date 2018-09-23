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
using System.Text.RegularExpressions;
using System.Xml;
using Eddie.Common;

namespace Eddie.Core
{	
    public class UiManager
    {
		List<UiClient> Clients = new List<UiClient>();

		public static void Init()
		{
			   
		}

		public void Add(UiClient client)
		{
			Clients.Add(client);
		}

		public void Broadcast(Json data)
		{
			foreach(UiClient client in Clients)
			{
				client.OnReceive(data);
			}
		}

		public Json OnCommand(Json data, UiClient sender)
		{
			string cmd = data["command"].Value as string;

			if (cmd == "test-json")
			{
				Json j = new Json();
				j["result"].Value = "works";
				return j;
			}
			else if (cmd == "exit")
			{
				Engine.Instance.OnExit();
			}
			else if (cmd == "system.report.start")
			{
				Report report = new Report();
				report.Start(sender);
			}
			else if (cmd == "openvpn_management")
			{				
				if (Engine.Instance.SendManagementCommand(data["management_command"].Value as string) == false)
					Engine.Instance.Logs.Log(LogType.Warning, Messages.OpenVpnManagementCommandFail);
			}
			else if (cmd == "tor_control")
			{
				string resultC = TorControl.SendCommand(data["control_command"].Value as string);
				foreach (string line in resultC.Split('\n'))
				{
					string l = line.Trim();
					if (l != "")
						Engine.Instance.Logs.Log(LogType.Verbose, l);
				}
			}
			else if (cmd == "set_option")
			{
				string name = data["name"].Value as string;
				string value = data["value"].Value as string;

				if (Engine.Instance.Storage.Set(name, data["value"].Value))
				{
					if (name == "tools.openvpn.path")
					{
						Software.Checking();
					}
				}
			}
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
			else if (cmd == "ui.show.os.info")
				Engine.Instance.OnShowText("ui.show.os.info", Engine.Instance.GenerateOsInfo().ToJsonPretty());
			else if (cmd == "tor.guard")
				Engine.Instance.Logs.LogVerbose("Tor Guard IPs:" + TorControl.GetGuardIps(true).ToString());
			else if (cmd == "tor.NEWNYM")
				TorControl.SendNEWNYM();
			else if (cmd == "ip.exit")
				Engine.Instance.Logs.LogVerbose(Engine.Instance.DiscoverExit().ToString());			
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
            else if (cmd == "test.netlock.update")
            {
                if (Engine.Instance.NetworkLockManager != null)
                    Engine.Instance.NetworkLockManager.OnUpdateIps();
            }

			return null;
		}

		// Helper
		public void Broadcast(string command)
		{
			Json j = new Json();
			j["command"].Value = command;
			Broadcast(j);
		}
	}
}
