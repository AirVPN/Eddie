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
using System.Threading;

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
			else if (cmd == "ui.start")
			{
				Json result = new Json();
				result["manifest"].Value = Engine.Instance.Manifest;
				result["main_status"].Value = Engine.Instance.MainStatusBuild();
				result["logs"].Value = Engine.Instance.Logs.GetJson();
				result["options"].Value = Engine.Instance.ProfileOptions.GetJson();

				Json jNetlockModes = new Json();
				foreach (NetworkLockPlugin lockPlugin in Engine.Instance.NetworkLockManager.Modes)
				{
					Json jNetlockMode = new Json();
					jNetlockMode["code"].Value = lockPlugin.GetCode();
					jNetlockMode["title"].Value = lockPlugin.GetTitleForList();
					jNetlockModes.Append(jNetlockMode);
				}
				result["netlock_modes"].Value = jNetlockModes;

				return result;
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
			else if (cmd == "tor.test")
			{
				Json result = new Json();
				result["result"].Value = TorControl.Test();
				return result;
			}
			else if (cmd == "tor.control")
			{
				string resultC = TorControl.SendCommand(data["command"].Value as string);
				foreach (string line in resultC.Split('\n'))
				{
					string l = line.Trim();
					if (l != "")
						Engine.Instance.Logs.Log(LogType.Verbose, l);
				}
			}
			else if (cmd == "options.set")
			{
				string name = data["name"].Value as string;
				string value = data["value"].Value as string;

				Engine.Instance.ProfileOptions.Set(name, data["value"].Value);
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
				Json result = new Json();
				result["layout"].Value = "text";
				result["title"].Value = "MAN";
				result["body"].Value = Engine.Instance.ProfileOptions.GetMan(format);
				return result;
			}
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
			else if (cmd == "url.open")
			{
				Platform.Instance.OpenUrl(data["uri"].ValueString);
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
