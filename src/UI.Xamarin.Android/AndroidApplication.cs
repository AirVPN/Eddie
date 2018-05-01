// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2018 AirVPN (support@airvpn.org) / https://airvpn.org
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
using Android.App;
using Android.Runtime;
using Eddie.Common.Tasks;
using Android.Widget;
using Android.Content;
using Eddie.Common.Log;

namespace Eddie.Droid
{
	[Application]
	class AndroidApplication : global::Android.App.Application
	{
		public static string LOG_TAG = "Eddie.Android";

		private TasksManager m_tasksManager = new TasksManager();
		private static object m_logSync = new object();
		private bool m_initialized = false;

		public AndroidApplication(IntPtr handle, JniHandleOwnership transfer) : base(handle,transfer)
        {
			
		}

		public TasksManager TasksManager
		{
			get
			{
				return m_tasksManager;
			}
		}

		public bool Initialized
		{
			get
			{
				return m_initialized;
			}
		}
				
		public override void OnCreate()
		{
			base.OnCreate();

			LogsManager.Instance.AddHandler(OnLogEvent);

			if(!m_initialized)
			{
				int ovpn3Version = NativeMethods.OVPN3.Version();

				int result = NativeMethods.OVPN3.Init();
				if(NativeMethods.Succeeded(result))
				{
					m_initialized = true;
					LogsManager.Instance.Debug("NativeMethods.OVPN3.Init succeeded (version={0})", ovpn3Version);
				}					
				else
				{
					LogsManager.Instance.Error("NativeMethods.OVPN3.Init failed with code '{0}' (version={1})", result, ovpn3Version);
				}					
			}				
		}

		public override void OnTerminate()
		{
			if(m_initialized)
			{
				m_initialized = false;

				int result = NativeMethods.OVPN3.Cleanup();
				if(!NativeMethods.Succeeded(result))
					LogsManager.Instance.Error("NativeMethods.OVPN3.Cleanup failed with code '{0}'", result);
			}

			LogsManager.Instance.RemoveHandler(OnLogEvent);

			base.OnTerminate();
		}

		public static void ShowPopup(Context context, string message, ToastLength length = ToastLength.Long)
		{
			Toast.MakeText(context, message, length).Show();
		}

		private void OnLogEvent(LogEntry e)
		{
			string message = e.Message;

			switch(e.Level)
			{
			case LogLevel.error:	global::Android.Util.Log.Error(LOG_TAG, message);
									break;

			case LogLevel.warning:	global::Android.Util.Log.Warn(LOG_TAG, message);
									break;

			default:				global::Android.Util.Log.Debug(LOG_TAG, message);
									break;
			}
		}
	}
}
