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
using System.Text;
using System.Threading;
using Eddie.Lib.Common;
using Eddie.Core;

namespace Eddie.CLI.Windows
{
	class Program
	{
		static void Main(string[] args)
		{
			Platform.Instance = new Eddie.Platforms.Windows();
            
			CommandLine.InitSystem(Environment.CommandLine);

			Core.Engine engine = new Core.Engine();

			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

			if (engine.Initialization(true))
			{
                engine.ConsoleStart();
			}
		}

		static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Engine.Instance.OnUnhandledException(e.ExceptionObject as Exception);			
		}
	}
}
