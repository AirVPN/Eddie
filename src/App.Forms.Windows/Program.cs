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

using Eddie.Core;
using System;
using System.Threading;
using System.Windows.Forms;

namespace Eddie.Forms.Windows
{
	public static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		/// 

		private static Eddie.Forms.UiClient m_client;

		[STAThread]
		public static void Main()
		{
			try
			{
				if (Environment.OSVersion.Version.Major >= 6)
				{
					NativeMethods.SetProcessDPIAware();
				}

				//Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

				Application.ThreadException += new ThreadExceptionEventHandler(ApplicationThreadException);
				Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
				AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

				Core.Platform.Instance = new Platform.Windows.Platform();

				if (new CommandLine(Environment.CommandLine, true, false).Exists("cli")) // TOFIX, not need anymore when every OS have a CLI executable.
				{
					Core.ConsoleEdition.UiClient client = new Core.ConsoleEdition.UiClient();
					client.Init(Environment.CommandLine);
				}
				else
				{
					m_client = new UiClient();
					m_client.Init(Environment.CommandLine);
				}
			}
			catch (Exception ex)
			{
				_ = MessageBox.Show(ex.Message, Constants.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			// Application.Run must be outside the catch above, otherwise it's not unhandled
			if ((m_client != null) && (m_client.AppContext != null))
			{
				Application.Run(m_client.AppContext);
			}
		}

		public static void ApplicationThreadException(object sender, ThreadExceptionEventArgs e)
		{
			if (m_client != null)
			{
				m_client.OnUnhandledException("ApplicationThread", e.Exception);
			}
		}

		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			if (m_client != null)
			{
				Exception ex = (Exception)e.ExceptionObject;
				m_client.OnUnhandledException("CurrentDomain", ex);
			}
		}
	}
}