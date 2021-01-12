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
using System.Text;
using Eddie.Core.Threads;

namespace Eddie.Core
{
	public class PingerJob
	{
		//public System.Threading.Thread T;
		public ConnectionInfo Server;

		/*
		public void Start()
		{
			T = new System.Threading.Thread(new ThreadStart(this.Run));
			T.Priority = ThreadPriority.Lowest;
			T.Start();
		}
		*/

		public void Run()
		{			
			RouteScope routeScope = null;
			try
			{				
				IpAddress ip = Server.IpsEntry.FirstPreferIPv4;
				if( (ip == null) || (ip.Valid == false) )
					throw new Exception("Invalid ip");
				routeScope = new RouteScope(ip.ToString());
				Int64 result = Platform.Instance.Ping(ip, Engine.Instance.Options.GetInt("pinger.timeout"));		

                if ( (Engine.Instance == null) || (Engine.Instance.JobsManager == null) || (Engine.Instance.JobsManager.Latency == null) )
					return; // Avoid unidentified crash

				Engine.Instance.JobsManager.Latency.PingResult(Server, result);
			}
			catch (Exception)
			{
				if ((Engine.Instance == null) || (Engine.Instance.JobsManager == null) || (Engine.Instance.JobsManager.Latency == null))
					return; // Avoid unidentified crash

				Engine.Instance.JobsManager.Latency.PingResult(Server, -1);
			}
			finally
			{
				if (routeScope != null)
				{
					routeScope.End();
				}

				if ((Engine.Instance == null) || (Engine.Instance.JobsManager == null) || (Engine.Instance.JobsManager.Latency == null))
				{
					// Avoid unidentified crash
				}
				else
				{
					lock (Engine.Instance.JobsManager.Latency.Jobs)
					{
						Engine.Instance.JobsManager.Latency.Jobs.Remove(this);
					}
				}
			}

		}
	}
}
