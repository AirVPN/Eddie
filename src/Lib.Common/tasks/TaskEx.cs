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
