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
	public class Thread
	{
		public volatile bool CancelRequested = false;
		protected System.Threading.Thread m_Thread;

		public Thread() : this(true)
		{
		}

		public Thread(bool start)
		{
			m_Thread = new System.Threading.Thread(this.DoRun);
			/* // TOCLEAN
			if (NeedApartmentState())
				m_Thread.SetApartmentState(ApartmentState.STA);
			*/
			m_Thread.Priority = GetPriority();
			if (start)
				m_Thread.Start();
		}

		public static Core.Engine Engine
		{
			get
			{
				return Engine.Instance;
			}
		}

		public virtual void Start()
		{
			m_Thread.Start();
		}

		public virtual void Join()
		{
			m_Thread.Join();
		}

		public virtual bool Join(int millisecondsTimeout)
		{
			return m_Thread.Join(millisecondsTimeout);
		}

		// TOCLEAN
		/*
		public virtual void Abort()
		{
			m_Thread.Abort();
		}
		*/

		public virtual void RequestStop()
		{
			CancelRequested = true;
		}

		public void RequestStopSync()
		{
			RequestStop();
			m_Thread.Join();
		}

		public void Sleep(int msec)
		{
			System.Threading.Thread.Sleep(msec);
		}

		public void DoRun()
		{
			OnRun();
			OnStop();
		}

		public bool IsAlive()
		{
			return m_Thread.IsAlive;
		}

		public virtual ThreadPriority GetPriority()
		{
			return ThreadPriority.Normal;
		}

		/* // TOCLEAN
		public virtual bool NeedApartmentState()
		{
			return false;
		}
		*/

		public virtual void OnRun()
		{
		}

		public virtual void OnStop()
		{
		}
	}
}
