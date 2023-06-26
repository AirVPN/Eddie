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

namespace Eddie.Core.Jobs
{
	public class Penalities : Eddie.Core.Job
	{
		public string m_lastVersionNotification = "";

		public override ThreadPriority GetPriority()
		{
			return ThreadPriority.Lowest;
		}

		public override bool GetSync()
		{
			return true;
		}

		public override void OnRun()
		{
			lock (Engine.Instance.Connections)
			{
				foreach (ConnectionInfo infoServer in Engine.Instance.Connections.Values)
				{
					if (infoServer.Penality > 0)
						infoServer.Penality--;
				}
			}

			m_timeEvery = 60000;
		}
	}
}
