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

namespace Eddie.Core.UI
{
	public class Chart
	{
		public int Resolution;
		public long[] Download;
		public long[] Upload;
		public int TimeStep;
		public int Grid;
		public int Pos;
		private Int64 m_lastStepTime;
		private int m_lastStepSamples;

		public Chart(int r, int t, int g)
		{
			Resolution = r / t;
			Download = new long[Resolution];
			Upload = new long[Resolution];
			TimeStep = t;
			Grid = g;
			Pos = 0;
		}

		public long GetMax()
		{
			long m = 0;
			for (int i = 0; i < Resolution; i++)
			{
				long d = Download[i];
				long u = Upload[i];

				if (d > m) m = d;
				if (u > m) m = u;
			}
			return m;
		}

		public long GetLastDownload()
		{
			return Download[Pos];
		}

		public long GetLastUpload()
		{
			return Upload[Pos];
		}

		public void Hit(long d, long u, Int64 ts)
		{
			if (ts - m_lastStepTime > TimeStep)
			{
				// Step
				m_lastStepTime = ts;
				m_lastStepSamples = 0;
				Pos++;
				if (Pos == Resolution)
					Pos = 0;
			}
			else
			{
				m_lastStepSamples++;
			}
			if (m_lastStepSamples == 0)
			{
				Download[Pos] = d;
				Upload[Pos] = u;
			}
			else
			{
				Download[Pos] = (Download[Pos] + d) / 2;
				Upload[Pos] = (Upload[Pos] + u) / 2;
			}
		}
	}
}
