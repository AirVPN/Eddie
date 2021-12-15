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

namespace Eddie.Core
{
	public class PerformanceScope
	{
		private string m_name;
		private long m_timeStart = 0;

		public PerformanceScope(string name)
		{
			Start(name);
		}

		~PerformanceScope()
		{
			End();
		}

		public void Start(string name)
		{
			m_name = name;
			m_timeStart = Utils.UnixTimeStampMs(); // Almost same accuracy as Enviroment.tickCount
		}

		public void End()
		{
			if (m_timeStart != 0)
			{
				long timeElapsed = Utils.UnixTimeStampMs() - m_timeStart;
				m_timeStart = 0;
				Engine.Instance.Logs.LogDebug("PerformanceScope: " + m_name + ", " + timeElapsed.ToString() + " ms");
			}
		}
	}
}
