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
//
// 20 June 2018 - author: promind - initial release. Based on revised code from com.eddie.android. (a tribute to the 1859 Perugia uprising occurred on 20 June 1859 and in memory of those brave inhabitants who fought for the liberty of Perugia)

using Android.App;
using Android.Content;
using Android.OS;
using System;

namespace Eddie.NativeAndroidApp
{
	// Android.Content.Intent.ActionScreenOn, Android.Content.Intent.ActionScreenOff cannot be handled statically but they must be registered dynamically at runtime

	[BroadcastReceiver(Name="org.airvpn.eddie.EddieBroadcastReceiver", Enabled=true, Permission="android.permission.RECEIVE_BOOT_COMPLETED")]
	[IntentFilter(new[] { global::Android.Content.Intent.ActionBootCompleted })]
	public class EddieBroadcastReceiver : BroadcastReceiver
	{
        private SettingsManager settingsManager = new SettingsManager();

		public override void OnReceive(Context context, Intent intent)
		{
			string action = intent.Action;

            EddieLogger.Init(context);

			EddieLogger.Debug(string.Format("EddieBroadcastReceiver.OnReceive (action='{0}')", action));

			if(action == global::Android.Content.Intent.ActionBootCompleted)
			{
                TryRestoreLastProfile(context);
			}
			else
			{
                EddieLogger.Error(string.Format("Unhandled action '{0}' received in EventsReceiver", action));
			}
		}

		private void TryRestoreLastProfile(Context context)
		{
			EddieLogger.Debug("EddieBroadcastReceiver.TryRestoreLastProfile");

			if(VPNService.Prepare(context.ApplicationContext) != null)
			{
				EddieLogger.Debug("VPNService.Prepare requires confirmation");
				
                return;
			}

            if(!settingsManager.SystemRestoreLastProfile)
            {
                EddieLogger.Debug("EddieBroadcastReceiver: SystemRestoreLastProfile disabled");
                
                return;
            }

            if(!settingsManager.SystemLastProfileIsConnected)
            {
                EddieLogger.Debug("EddieBroadcastReceiver: SystemLastProfileIsConnected false");
                
                return;
            }

			string lastProfile = settingsManager.SystemLastProfile;
			if(SupportTools.Empty(lastProfile))
			{
				EddieLogger.Debug("EddieBroadcastReceiver: lastProfile is empty");
				return;
			}

			EddieLogger.Debug("EddieBroadcastReceiver: restoring last profile");

			try
			{
				Bundle args = new Bundle();
				args.PutString(VPNService.PARAM_PROFILE, lastProfile);
				
				Intent intent = new Intent(context, typeof(VPNService));
				intent.PutExtra(VPNService.PARAM_START, true);
				intent.PutExtra(VPNService.EXTRA_RUN_ARGS, args);
			
				context.StartService(intent);				
			}
			catch(Exception e)
			{
				EddieLogger.Error("TryRestoreLastProfile", e);
			}
		}		
	}
}
