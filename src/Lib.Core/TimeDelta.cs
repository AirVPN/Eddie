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

namespace Eddie.Core
{
	public class TimeDelta
	{
		int V = 0;

		public TimeDelta()
		{
			Reset();
		}

		public int Reset()
		{
			int o = V;
			int n = Environment.TickCount;

			V = n;
			return n - o;
		}

		public bool Elapsed(int msec)
		{
			int o = V;
			int n = Environment.TickCount;
			int d = n - o;

			if (d >= msec)
			{
				//V = n;
				V += msec;
				return true;
			}

			return false;
		}
	}
}
