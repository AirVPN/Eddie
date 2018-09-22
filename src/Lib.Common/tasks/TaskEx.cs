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

#if EDDIENET4

using System;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eddie.Common.Tasks
{
	public class TaskEx : Task
    {
        private CancellationTokenSource m_cancellationSource = null;
        private List<ITasksListener> m_listeners = new List<ITasksListener>();
        private object m_listenersSync = new object();        

        public TaskEx(CancellationTokenSource cancellationSource, Action action) : base(action)
        {
            m_cancellationSource = cancellationSource;
        }

        public void cancel()
        {
            m_cancellationSource.Cancel();
        }
        
        public bool Canceled
        {
            get
            {
                return m_cancellationSource.IsCancellationRequested;
            }
        }

        public void Cleanup()
        {
            ITasksListener[] listeners = LockListeners();
            if(listeners != null)
            {
                foreach(ITasksListener listener in listeners)
                {
                    listener.onTaskEnded(this);
                }
            }
        }

        private ITasksListener[] LockListeners()
        {
            lock(m_listenersSync)
            {
                return m_listeners.ToArray();
            }
        }

        public void AddListener(ITasksListener listener)
        {
            if(listener == null)
                return;

            lock(m_listenersSync)
            {
                m_listeners.Add(listener);
            }
        }

        public void RemoveListener(ITasksListener listener)
        {
            if(listener == null)
                return;

            lock(m_listenersSync)
            {
                m_listeners.Remove(listener);
            }
        }

		protected override void Dispose(bool disposing)
		{
			if(m_cancellationSource != null)
			{
				m_cancellationSource.Dispose();
				m_cancellationSource = null;
			}

			base.Dispose(disposing);        
		}
	}
}

#endif
