// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2023 AirVPN (support@airvpn.org) / https://airvpn.org
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

namespace Eddie.Core.ConsoleEdition
{
	public class UiApp : Core.UiClient
	{
		public Engine Engine;

		private System.Threading.Thread StandardInThread;
		private List<Json> StandardInInput = new List<Json>();

		public override bool Init(string environmentCommandLine)
		{
			base.Init(environmentCommandLine);

			if (Engine == null)
				Engine = new EngineApp(environmentCommandLine);

			Engine.UiManager.Add(this);

			Engine.Start();

			StandardInThread = new System.Threading.Thread(ProcessStdIn);
			StandardInThread.Start();

			return true;
		}

		public void ProcessStdIn()
		{
			// Note 2023-11-08
			// We run in a lots of issue related to Peek() and in general check if there are data on stdin without blocking,
			// but any method works on Windows but not on Linux.
			// So we opt to a blocking Console.Readline() in a thread, and a OnWork() that dispatch the commands in main thread.
			// UI App will send a "engine.stdin.stop" to close this thread (avoiding Abort or similar)			
			for (; ; )
			{
				if (Engine.Instance.Terminated)
					break;

				string line = Console.ReadLine();
				if (line is null)
				{
					break;
				}
				else
				{
					Json data;
					if (Json.TryParse(line, out data))
					{
						lock (StandardInInput)
						{
							StandardInInput.Add(data);
							if ((data.HasKey("command")) && (data["command"].ValueString == "engine.stdin.stop"))
							{
								break;
							}
						}
					}
				}
			}
		}

		public override void OnReceive(Json data)
		{
			base.OnReceive(data);

			Console.WriteLine(data.ToJson());
		}

		public override void OnWork()
		{
			base.OnWork();

			lock (StandardInInput)
			{
				while (StandardInInput.Count > 0)
				{
					Json data = StandardInInput[0];
					StandardInInput.RemoveAt(0);

					Engine.Instance.UiManager.SendCommandDirect(data, this);
				}
			}
		}

		public void OnUnhandledException(string source, Exception ex)
		{
			if (Engine != null)
				Engine.OnUnhandledException(source, ex);
		}
	}
}
