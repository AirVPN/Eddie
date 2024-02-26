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
	public class EngineApp : Core.Engine
	{
		public EngineApp(string environmentCommandLine) : base(environmentCommandLine)
		{
		}

		public override bool IsConsole()
		{
			return true;
		}

		public override bool IsUiApp()
		{
			return true;
		}

		public override bool OnInit()
		{
			Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);

			return base.OnInit();
		}

		private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
		{
			e.Cancel = true;
			ExitStart();
		}
	}

}
