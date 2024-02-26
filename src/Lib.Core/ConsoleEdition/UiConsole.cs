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

namespace Eddie.Core.ConsoleEdition
{
	public class UiConsole : Core.UiClient
	{
		public Engine Engine;

		public override bool Init(string environmentCommandLine)
		{
			base.Init(environmentCommandLine);

			if (Engine == null)
				Engine = new EngineConsole(environmentCommandLine);

			Engine.UiManager.Add(this);

			Engine.Start();

			return true;
		}

		public override void OnReceive(Json data)
		{
			base.OnReceive(data);

			if (!data.HasKey("command") || data["command"].ValueString != "log")
			{
				// Note: already dumped by Engine.OnLog(LogEntry l)
			}
		}

		public override void OnWork()
		{
			base.OnWork();

			if (Engine.StartCommandLine.Exists("batch"))
				return;

			// try/catch because throw an exception if stdin is redirected.
			try
			{
				if (Console.KeyAvailable)
				{
					// Buttons-style

					char ch = Char.ToLowerInvariant(Console.ReadKey().KeyChar);

					if (ch == 'x')
					{
						Engine.Instance.Logs.Log(LogType.Info, LanguageManager.GetText(LanguageItems.ConsoleKeyCancel));
						if (Engine.IsConnected())
						{
							Engine.Instance.Disconnect();
						}
						else
						{
							Engine.Instance.ExitStart();
						}
					}
					else if (ch == 'n')
					{
						Engine.Instance.Logs.Log(LogType.Info, LanguageManager.GetText(LanguageItems.ConsoleKeySwitch));
						if (Engine.IsConnected())
						{
							Engine.Instance.SwitchServer = true;
						}
						else
						{
							if ((Engine.CanConnect()) && (Engine.IsWaiting() == false))
								Engine.Instance.Connect();
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Console error: " + ex.Message);
			}
		}

		public void OnUnhandledException(string source, Exception ex)
		{
			if (Engine != null)
				Engine.OnUnhandledException(source, ex);
		}
	}
}
