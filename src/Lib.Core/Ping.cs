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

using System.Threading;

namespace Eddie.Core
{
	public class Ping
	{
		public IpAddress Ip;
		public int TimeoutMs = 5000;
		public ConnectionInfo Server;
		public AutoResetEvent Complete = new AutoResetEvent(false);
		public int Result = -1;

		public delegate void CompleteHandler(Ping p);
		public event CompleteHandler CompleteEvent;

		public int DoSync()
		{
			Engine.Instance.PingManager.Add(this);
			if (Complete.WaitOne(TimeoutMs) == false)
				return -1;
			else
				return Result;
		}

		public void End(int result)
		{
			Result = result;
			if (CompleteEvent != null)
				CompleteEvent(this);
			Complete.Set();
		}
	}
}
