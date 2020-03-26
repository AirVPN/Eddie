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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

namespace Eddie.Core
{
	public class UiManager
	{
		List<UiClient> Clients = new List<UiClient>();

		public class Command
		{
			public Json Request;
			public Json Response;
			public UiClient Sender;
			public AutoResetEvent Complete = new AutoResetEvent(false);
		}

		private List<Command> m_commands = new List<Command>();

		public static void Init()
		{

		}

		public void Add(UiClient client)
		{
			Clients.Add(client);
		}

		public void Broadcast(Json data)
		{
			foreach (UiClient client in Clients)
			{
				client.OnReceive(data);
			}
		}

		public Json SendCommand(Json request, UiClient sender)
		{
			Command c = new Command();
			c.Request = request;
			c.Sender = sender;
			lock (m_commands)
				m_commands.Add(c);
			c.Complete.WaitOne();
			return c.Response;
		}

		public Json ProcessCommand(Json data, UiClient sender)
		{
			string cmd = data["command"].Value as string;

			if (cmd == "exit")
			{
				Engine.Instance.Exit();
			}
			else if (cmd == "mainaction.connect")
			{
				Engine.Instance.Connect();
			}
			else if (cmd == "mainaction.disconnect")
			{
				Engine.Instance.Disconnect();
			}			
			else if (cmd == "system.report.start")
			{
				Report report = new Report();

				report.Start(sender);
			}
			// TOCLEAN_OPENVPNMANAGEMENT
			/*
			else if (cmd == "openvpn_management")
			{
				if (Engine.Instance.SendManagementCommand(data["management_command"].Value as string) == false)
					Engine.Instance.Logs.Log(LogType.Warning, LanguageManager.GetText("OpenVpnManagementCommandFail"));
			}
			*/
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
			else if (cmd == "ui.start")
			{
				Json result = new Json();
				result["manifest"].Value = Engine.Instance.Manifest;
				result["logs"].Value = Engine.Instance.Logs.GetJson();
				return result;
			}
			else if (cmd == "man")
			{
				string format = "text";
				if (data.HasKey("format"))
					format = data["format"].Value as string;
				Json result = new Json();
				result["layout"].Value = "text";
				result["title"].Value = "MAN";
				result["body"].Value = Engine.Instance.Storage.GetMan(format);
				return result;
			}
			else if (cmd == "ui.show.os.info")
				return Engine.Instance.GenerateOsInfo().Clone();
			else if (cmd == "tor.guard")
				Engine.Instance.Logs.LogVerbose("Tor Guard IPs:" + TorControl.GetGuardIps(true).ToString());
			else if (cmd == "tor.NEWNYM")
				TorControl.SendNEWNYM();
			else if (cmd == "ip.exit")
				Engine.Instance.Logs.LogVerbose(Engine.Instance.DiscoverExit().ToString());
			else if (cmd == "test.query")
			{
				Json result = new Json();
				result["result"].Value = cmd;
				return result;
			}
			else if (cmd == "test.logs")
			{
				Engine.Instance.Logs.Log(LogType.InfoImportant, "Test log\nInfo");
				Engine.Instance.Logs.Log(LogType.InfoImportant, "Test log\nInfo Important");
				Engine.Instance.Logs.Log(LogType.Warning, "Test log\nWarning\n" + DateTime.Now.ToString());
				Engine.Instance.Logs.Log(LogType.Error, "Test log\nError");
				//Engine.Instance.Logs.Log(LogType.Fatal, "Test log\nFatal");
			}			

			return null;
		}

		public void ProcessOnMainThread()
		{
			lock (m_commands)
			{
				Command c = null;
				if (m_commands.Count > 0)
				{
					c = m_commands[0];
					m_commands.RemoveAt(0);

					c.Response = ProcessCommand(c.Request, c.Sender);
					c.Complete.Set();
				}

			}
		}

		// Helper
		public void Broadcast(string command)
		{
			Json j = new Json();
			j["command"].Value = command;
			Broadcast(j);
		}

		public void Broadcast(string command, string key1, string val1)
		{
			Json j = new Json();
			j["command"].Value = command;
			j[key1].Value = val1;
			Broadcast(j);
		}
	}
}
