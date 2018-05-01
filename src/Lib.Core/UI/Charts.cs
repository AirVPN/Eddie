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
using System.Threading;
using System.Text;

namespace Eddie.Core.UI
{
	public class Charts
	{
		public List<Chart> ChartsList = new List<Chart>();

		public delegate void UpdateHandler(object sender, EventArgs e);
		public event UpdateHandler UpdateEvent;

		public bool CancelRequested = false;

		public Charts()
		{
			ChartsList.Add(new Chart(60, 1, 6)); // 1 minute, 1 seconds step, 10 seconds grid
			ChartsList.Add(new Chart(60 * 10, 1, 10)); // 10 minute, 1 seconds step, 1 minute grid
			ChartsList.Add(new Chart(60 * 60, 1, 6)); // 1 hour, 1 seconds step, 10 minute grid
			ChartsList.Add(new Chart(60 * 60 * 24, 30, 24)); // 1 day, 30 seconds step, 1 hour grid
			ChartsList.Add(new Chart(60 * 60 * 24 * 30, 600, 30)); // 30 day, 10 minute step, 1 day grid			
		}
				
		public void Hit(long d, long u)
		{
			int ts = UtilsCore.UnixTimeStamp();
			lock (this)
			{
				foreach (Chart c in ChartsList)
				{
					c.Hit(d, u, ts);
				}				
			}
					
			if(UpdateEvent != null)
				UpdateEvent(this, null);			
		}
	}
}
