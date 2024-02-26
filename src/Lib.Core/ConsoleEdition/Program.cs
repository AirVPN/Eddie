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
	public class Program
	{
		private static Core.ConsoleEdition.UiConsole m_client;

		public static void Init()
		{
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
		}

		public static void Main()
		{
			CommandLine cmdline = new CommandLine(Environment.CommandLine, true, false);

			if (cmdline.Exists("jsoninout"))
			{
				UiApp client = new UiApp();
				client.Init(Environment.CommandLine);
			}
			else
			{
				m_client = new UiConsole();
				m_client.Init(Environment.CommandLine);
			}
		}

		static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Engine.OnUnhandledException("CurrentDomain", e.ExceptionObject as Exception);
		}
	}
}
