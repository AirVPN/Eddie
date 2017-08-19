﻿// <eddie_source_header>
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
				routeScope = new RouteScope(Server.IpsEntry.ToStringFirstIPv4());

				Int64 result = Platform.Instance.Ping(Server.IpsEntry.ToStringFirstIPv4(), 3);
				Pinger.Instance.PingResult(Server, result);
			}
			catch (Exception)
			{
				Pinger.Instance.PingResult(Server, -1);
			}
			finally
			{
				if (routeScope != null)
				{
					routeScope.End();
				}

				lock (Pinger.Instance.Jobs)
				{
					Pinger.Instance.Jobs.Remove(this);
				}
			}

		}
	}
}