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

using Eddie.Common.Log;
using Eddie.Common.Tasks;
using System;

namespace Eddie.Common
{
	public abstract class IApplication : IService
	{
		public interface IListener
		{
			void OnApplicationStarting();
			void OnApplicationStarted(bool running);
			void OnApplicationStopping();
			void OnApplicationStopped();
			void OnApplicationUpdate(string message);
			void OnApplicationError(string message);
		}

		private static IApplication g_instance = null;

		private object m_sync = new object();
		private bool m_running = false;
		private IListener m_listener = null;

		private LogsManager m_logsManager = new LogsManager();
		private TasksManager m_tasksManager = new TasksManager();

		protected IApplication()
		{
			if(g_instance != null)
				throw new Exception("internal error (g_instance already initialized)");
			
			g_instance = this;
		}

		public bool Start()
		{
			lock(m_sync)
			{
				if(m_running)
					return false;

				PublishUpdate("Eddie starting...");

				FireOnStarting();

				PublishUpdate("Eddie starting2...");

				PublishUpdate("Super feature loading :<>");

				m_running = OnStart();

				PublishUpdate("Eddie started :D");

				FireOnStarted(m_running);

				return m_running;
			}
		}

		public void Stop()
		{
			lock(m_sync)
			{
				if(m_running == false)
					return;

				FireOnStopping();
					
				OnStop();
				m_running = false;

				m_tasksManager.CancelAll();

				FireOnStopped();
			}
		}

		protected abstract bool OnStart();
		protected abstract void OnStop();

		public static IApplication Instance
		{
			get
			{
				return g_instance;
			}
		}

		public IListener Listener
		{
			get
			{
				lock(this)
				{
					return m_listener;
				}
			}

			set
			{
				lock(this)
				{
					m_listener = value;
				}
			}
		}

		private void FireOnStarting()
		{
			lock(this)
			{
				if(m_listener != null)
					m_listener.OnApplicationStarting();
			}
		}

		private void FireOnStarted(bool running)
		{
			lock(this)
			{
				if(m_listener != null)
					m_listener.OnApplicationStarted(m_running);
			}
		}

		private void FireOnStopping()
		{
			lock(this)
			{
				if(m_listener != null)
					m_listener.OnApplicationStopping();
			}
		}

		private void FireOnStopped()
		{
			lock(this)
			{
				if(m_listener != null)
					m_listener.OnApplicationStopped();
			}
		}

		public LogsManager Logs
		{
			get
			{
				return m_logsManager;
			}
		}

		public TasksManager Tasks
		{
			get
			{
				return m_tasksManager;
			}
		}

		public void PublishUpdate(string message)
		{
			message += " (remove the sleep!)";

			lock(this)
			{
				if(m_listener != null)
					m_listener.OnApplicationUpdate(message);
			}

			Utils.Sleep(1000);
		}

		public void PublishError(string message)
		{
			lock(this)
			{
				if(m_listener != null)
					m_listener.OnApplicationError(message);
			}
		}

		public abstract void ShowNotify(string title, string message);
	}
}

#endif
