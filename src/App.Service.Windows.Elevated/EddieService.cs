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
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Lib.Platform.Windows.Elevated;

namespace App.Service.Windows.Elevated
{
	public partial class EddieService : ServiceBase
	{
		private Engine m_engine;

		public EddieService()
		{
			InitializeComponent();

			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
		}

		protected override void OnStart(string[] args)
		{
			if (m_engine != null)
				return;

			try
			{				
				Dictionary<string, string> cmdline = Engine.ParseCommandLine(Environment.GetCommandLineArgs()); // Note: 'args' OnStart params is anyway empty.
				cmdline["mode"] = "service";

				m_engine = new Engine();
				m_engine.Start(cmdline);
			}
			catch (Exception ex)
			{
				Utils.LogDebug(ex.Message);
			}
		}

		protected override void OnStop()
		{
			if (m_engine == null)
				return;

			try
			{
				m_engine.Stop(true);
				m_engine = null;
			}
			catch(Exception ex)
			{
				Utils.LogDebug(ex.Message);
			}
		}

		void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Utils.LogDebug((e.ExceptionObject as Exception).Message);
		}
	}
}
