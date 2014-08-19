// <airvpn_source_header>
// This file is part of AirVPN Client software.
// Copyright (C)2014-2014 AirVPN (support@airvpn.org) / https://airvpn.org )
//
// AirVPN Client is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// AirVPN Client is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with AirVPN Client. If not, see <http://www.gnu.org/licenses/>.
// </airvpn_source_header>

using System;
using System.Collections.Generic;
using System.Text;

namespace AirVPN.Core
{
	public class Process : System.Diagnostics.Process
	{	
		public bool ReallyExited
		{
			get
			{
				bool check1 = false;
				try
				{
					check1 = HasExited;
				}
				catch (Exception e)
				{
					Engine.Instance.Log(e);
				}

				/*
				bool check2 = true;
				
				Dictionary<int, string> processes = Platform.Instance.GetProcessesList();
				System.Diagnostics.Process[] processlist = Process.GetProcesses();
				foreach (int id in processes.Keys)
				{
					if (id == this.Id)
						check2 = false;					
				}

				if ((check1 == true) && (check2 == false))
				{
					Console.WriteLine("Unexpected: process marked as exited but still running.");
				}
				*/
				
				return check1;
			}
		}
	}
}
