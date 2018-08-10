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
using System.Windows.Forms;
using System.Text;
using Eddie.Common;
using Eddie.Core;
using Eddie.Forms;

namespace Eddie.Forms.Windows
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		/// 

		private static Eddie.Forms.UiClient m_client;
				
		[STAThread]
		static void Main()
		{
			try
			{
				if (Environment.OSVersion.Version.Major >= 6)
					NativeMethods.SetProcessDPIAware();

				//Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

				Core.Platform.Instance = new Eddie.Platform.Windows.Platform();

				CommandLine.InitSystem(Environment.CommandLine);
				
				if (CommandLine.SystemEnvironment.Exists("cli"))
				{
					Core.Engine engine = new Core.Engine();

					if (engine.Initialization(true))
					{
						engine.ConsoleStart();
					}
				}
				else
				{
					m_client = new Eddie.Forms.UiClient();
					m_client.Init();
				}
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message, Constants.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			// Application.Run must be outside the catch above, otherwise it's not unhandled
			if ((m_client != null) && (m_client.AppContext != null))
				Application.Run(m_client.AppContext);
		}
	}
}