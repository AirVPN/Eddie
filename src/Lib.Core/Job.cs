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
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Xml;
using System.Text;
using Eddie.Common;

namespace Eddie.Core
{
	public class Job
	{
		protected Int64 m_timeLastStart = 0;
		protected Int64 m_timeLastEnd = 0;
		protected Int64 m_timeEvery = 0;
		protected bool m_cancelRequested = false;

		private System.Threading.Thread m_thread;

		public virtual ThreadPriority GetPriority()
		{
			return ThreadPriority.Lowest;
		}

		public virtual bool GetSync()
		{
			return true;
		}

		public virtual void OnRun()
		{	
		}

		public void CancelRequested()
		{
			m_cancelRequested = true;
		}

		public void WaitEnd()
		{
			if (m_thread != null)
				m_thread.Join();
		}

		public void CheckNow()
		{
			m_timeEvery = 0;
		}

		public void CheckRun()
		{
			if (m_cancelRequested)
				return;

			Int64 now = UtilsCore.UnixTimeStampMs();

			if(m_thread != null)
			{
				// Already running
			}
			else if( (m_timeLastStart != 0) && (m_timeLastEnd< m_timeLastStart) )
			{
				// Already running
			}
			else if(now- m_timeLastEnd<m_timeEvery)
			{ 
				// Not need
			}
			else
			{
				//Engine.Instance.Logs.LogVerbose("Run job " + this.GetType().FullName);
				if (GetSync())
				{					
					Run();
				}
				else
				{
					m_thread = new System.Threading.Thread(this.Run);
					m_thread.Priority = GetPriority();
					m_thread.Start();
				}
			}
		}

		public void Run()
		{
			Int64 now = UtilsCore.UnixTimeStampMs();

			m_timeLastStart = now;
			OnRun();
			m_timeLastEnd = now;

			if(m_thread != null)
				m_thread = null;
		}
	}
}
