// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2026 AirVPN (support@airvpn.org) / https://airvpn.org
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
		private List<UiClient> Clients = new List<UiClient>();

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

		public UiClient GetContainerClient()
		{
			return Clients[0];
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

		public void OnWork()
		{
			foreach (UiClient client in Clients)
			{
				client.OnWork();
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

		public void SendCommandDirect(Json request, UiClient sender)
		{
			Command c = new Command();
			c.Request = request;
			c.Sender = sender;
			lock (m_commands)
				m_commands.Add(c);
		}

		public Json ProcessCommand(Json data, UiClient sender)
		{
			Json apiResult = ApiManager.Process(data, sender);
			if (apiResult != null)
				return apiResult;

			string cmd = data["command"].Value as string;

			// Only commands wired by the Legacy UIs (Lib.Forms / App.Cocoa.MacOS) live here.
			// Modern-only commands are handled by ApiManager (stubbed out for Legacy source publication).
			if (cmd == "mainaction.connect")
			{
				Engine.Instance.Connect();
			}
			else if (cmd == "mainaction.disconnect")
			{
				Engine.Instance.Disconnect();
			}
			else if (cmd == "report.start")
			{
				Report report = new Report();

				report.Start(sender);
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

					if ((c.Sender != null) && (c.Request.HasKey("callback")))
					{
						Json jReply = new Json();
						jReply["command"].Value = "reply";
						jReply["id"].Value = c.Request["callback"].ValueString;
						jReply["body"].Value = c.Response;
						c.Sender.OnReceive(jReply);
					}
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
