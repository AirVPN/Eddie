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

using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Eddie.Common.Log;
using Eddie.Common.Tasks;
using System;

namespace Eddie.Droid
{
	// Android.Content.Intent.ActionScreenOn, Android.Content.Intent.ActionScreenOff cannot be handled statically but they must be registered dynamically at runtime

	[BroadcastReceiver(Name="com.eddie.android.EventsReceiver", Enabled=true, Permission="android.permission.RECEIVE_BOOT_COMPLETED")]
	[IntentFilter(new[] { Android.Content.Intent.ActionBootCompleted })]
	public class EventsReceiver : BroadcastReceiver
	{
		private TasksManager m_tasksManager = new TasksManager();

		public override void OnReceive(Context context, Intent intent)
		{
			string action = intent.Action;

			LogsManager.Instance.Debug(string.Format("EventsReceiver.OnReceive (action='{0}')", action));

			if(action == Android.Content.Intent.ActionBootCompleted)
			{
				TryRestoreLastProfile(context);
			}
			else
			{
				LogsManager.Instance.Error(string.Format("Unhandled action '{0}' received in EventsReceiver", action));
			}
		}

		private void TryRestoreLastProfile(Context context)
		{
			LogsManager.Instance.Debug("EventsReceiver.TryRestoreLastProfile");

			if(VpnService.Prepare(context.ApplicationContext) != null)
			{
				LogsManager.Instance.Debug("VpnService.Prepare requires confirmation");
				return;
			}

			if(!OptionsManager.SystemLastProfileRestore)
			{
				LogsManager.Instance.Debug("EventsReceiver: SystemRestoreLastProfile disabled");
				return;
			}

			string lastProfile = OptionsManager.SystemLastProfileActivated;
			if(Utils.Empty(lastProfile))
			{
				LogsManager.Instance.Debug("EventsReceiver: lastProfile is empty");
				return;
			}

			LogsManager.Instance.Debug("EventsReceiver: restoring last profile1");

			try
			{
				Bundle args = new Bundle();
				args.PutString(AndroidVPNService.PARAM_PROFILE, lastProfile);
				
				Intent intent = new Intent(context, typeof(AndroidVPNService));
				intent.PutExtra(AndroidVPNService.PARAM_START, true);
				intent.PutExtra(AndroidVPNService.EXTRA_RUN_ARGS, args);
			
				context.StartService(intent);				
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("TryRestoreLastProfile", e);
			}
		}		
	}
}
