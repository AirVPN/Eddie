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

using System;
using Android.App;
using Android.Content;
using Android.OS;
using System.Collections.Generic;
using Android.Support.V4.App;
using Android.Provider;

namespace Eddie.NativeAndroidApp
{
	[Service(Name="org.airvpn.eddie.VPNService", Permission="android.permission.BIND_VPN_SERVICE")]
	[IntentFilter(new[] { global::Android.Net.VpnService.ServiceInterface })]
	public class VPNService : global::Android.Net.VpnService, IMessageHandler, INetworkStatusReceiverListener
	{
		public const int SERVICE_RUNNING_NOTIFICATION_ID = 10000;
		public const string PARAM_START = "START";
		public const string PARAM_STOP = "STOP";
		public const string PARAM_PROFILE = "PROFILE";

		public const string EXTRA_RUN_ARGS = "RUN_ARGS";
        public const string MSG_STATUS_BUNDLE_LAST_ERROR = "LAST_ERROR";

        public const int MSG_BIND = 0;
		public const int MSG_START = 1;
		public const int MSG_STOP = 2;
		public const int MSG_STATUS = 3;

        public const int MSG_BIND_ARG_REMOVE = 0;
        public const int MSG_BIND_ARG_ADD = 1;

        public const int THREAD_MAX_JOIN_TIME = 8000;  // 8 seconds

		private object dataSync = new object();
		private VPN.Status vpnStatus = VPN.Status.NOT_CONNECTED;
		private string vpnLastError = "";
        private string currentNotificationText = "";

		private NotificationCompat.Builder notification = null;
        private int alertNotificationId = 2000;

		private Messenger serviceMessenger = null;
		private Java.Lang.Thread vpnThread = null;
		private OpenVPNTunnel vpnTunnel = null;

        private Messenger clientMessenger = null;

		private ScreenReceiver screenReceiver = null;
        private SettingsManager settingsManager = new SettingsManager();
        private NetworkStatusReceiver networkStatusReceiver = null;

		private class ScreenReceiver : BroadcastReceiver
		{
			private VPNService vpnService = null;

			public ScreenReceiver(VPNService service)
			{
				vpnService = service;
                
                EddieLogger.Init(service);
			}

			public override void OnReceive(Context context, Intent intent)
			{
				string action = intent.Action;

				EddieLogger.Debug(string.Format("ScreenReceiver.OnReceive (action='{0}')", action));

				if(action == global::Android.Content.Intent.ActionScreenOn)
                    vpnService.OnScreenChanged(true);
				else if(action == global::Android.Content.Intent.ActionScreenOff)
                    vpnService.OnScreenChanged(false);
				else
                    EddieLogger.Error(string.Format("Unhandled action '{0}' received in ScreenReceiver", action));				
			}
		}
		
		public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
		{
			StartCommandResult result = StartCommandResult.Sticky;

			bool? run = null;

			if(intent != null)
			{
				if(intent.GetBooleanExtra(PARAM_START, false))
				{
					EddieLogger.Debug("VPNService.OnStartCommand: START");
				
					run = true;
				}					
				else if(intent.GetBooleanExtra(PARAM_STOP, false))
				{
					EddieLogger.Debug("VPNService.OnStartCommand: STOP");

					run = false;					
				}					
			}
			else
			{
				EddieLogger.Debug("VPNService.OnStartCommand (no intent)");
			}
		
			if(run != null)
				UpdateService(run.Value, intent.GetBundleExtra(EXTRA_RUN_ARGS));
			
			return result;
		}

		public override IBinder OnBind(Intent intent)
		{
			return serviceMessenger != null ? serviceMessenger.Binder : null;
		}

		private void Init()
		{
			serviceMessenger = new Messenger(new MessageHandler(this));
		}

		private void Cleanup()
		{
			if(serviceMessenger != null)
			{
				serviceMessenger.Dispose();
				
                serviceMessenger = null;
			}			
		}

		private string LastError
		{
            get
            {
                return vpnLastError;
            }

			set
			{
				lock(dataSync)
				{
					vpnLastError = value;
				}
			}
		}

		public override void OnCreate()
		{
			base.OnCreate();

            EddieLogger.Init(this);

			Init();

            networkStatusReceiver = new NetworkStatusReceiver();
            networkStatusReceiver.AddListener(this);
            this.RegisterReceiver(networkStatusReceiver, new IntentFilter(Android.Net.ConnectivityManager.ConnectivityAction));
		}
	
		public override void OnDestroy()
		{
			UpdateService(false);
			
            Cleanup();

			base.OnDestroy();

            networkStatusReceiver.RemoveListener(this);
    
            this.UnregisterReceiver(networkStatusReceiver);
		}

		public override void OnRevoke()
		{
			EddieLogger.Error("VPNService.OnRevoke");

			// Another VPN may have started here or Android is shutting down the VPN: by now there is no way to "block" the operation and the service must be stopped gracefully

			UpdateService(false);

			base.OnRevoke();
		}

		public void OnMessage(Message msg)
		{
            if(msg == null)
                return;

			switch(msg.What)
			{
    			case VPNService.MSG_BIND:
                {
                    if(msg.Arg1 == MSG_BIND_ARG_ADD)
                        clientMessenger = msg.ReplyTo;
                    else
                        clientMessenger = null;
                }
    			break;
    
    			case VPNService.MSG_START:
                {
                    UpdateService(true, msg.Data);
                }
    			break;
    
    			case VPNService.MSG_STOP:
                {
                    UpdateService(false);
                }
    			break;
			}

			if(msg.ReplyTo != null)
			{
				VPN.Status status;
				string lastError;

				lock(dataSync)
				{
					status = vpnStatus;
					
                    lastError = vpnLastError;
				}

				SendMessage(msg.ReplyTo, CreateStatusMessage(status, lastError));
			}				
		}

		private bool UpdateService(bool run, Bundle data = null)
		{
			lock(dataSync)
			{
				bool running = (vpnStatus == VPN.Status.CONNECTED) || (vpnStatus == VPN.Status.CONNECTING) || (vpnStatus == VPN.Status.PAUSED) || (vpnStatus == VPN.Status.LOCKED);
				
                if(running == run)
					return true;

				if(vpnStatus == VPN.Status.DISCONNECTING)
					return false;	// Do not allow start requests while stopping

				// Must be under dataSync to ensure "m_status" synchronization
				
                if(run)
					DoStart(data);
				else
					DoStop();
			}

			return true;
		}

		private void OnServiceStarted()
		{
			EnsureReceivers();
		}

		private void OnServiceStopped()
		{
			CleanupReceivers();
		}

		private void OnScreenChanged(bool active)
		{
			EddieLogger.Debug(string.Format("VPNService.OnScreenChanged: active='{0}'", active));

			try
			{
				if(vpnTunnel != null)
                {
					VPN.Status status;

                    status = vpnTunnel.HandleScreenChanged(active);

                    if(status != VPN.Status.UNKNOWN)
                        DoChangeStatus(status);
                }
			}
			catch(Exception e)
			{
				EddieLogger.Error("OnScreenChanged", e);
			}			
		}

        private void NetworkStatusChanged(OpenVPNTunnel.VPNAction action)
        {
            EddieLogger.Debug(string.Format("VPNService.NetworkStatusChanged: action='{0}'", action));

            try
            {
                if(vpnTunnel != null)
                {
                    VPN.Status status;

                    status = vpnTunnel.NetworkStatusChanged(action);

                    if(status != VPN.Status.UNKNOWN)
                        DoChangeStatus(status);
                }
            }
            catch(Exception e)
            {
                EddieLogger.Error("NetworkStatusChanged", e);
            }           
        }

		private void EnsureReceivers()
		{
			if(screenReceiver != null)
				return;

			IntentFilter intentFilter = new IntentFilter();
			
            intentFilter.AddAction(Intent.ActionScreenOn);
			intentFilter.AddAction(Intent.ActionScreenOff);
			
            screenReceiver = new ScreenReceiver(this);
			
            RegisterReceiver(screenReceiver, intentFilter);
		}

		private void CleanupReceivers()
		{
			if(screenReceiver != null)
			{
				UnregisterReceiver(screenReceiver);

				SupportTools.SafeDispose(screenReceiver);

                screenReceiver = null;
			}
		}

		public void HandleThreadStarted()
		{
			DoChangeStatus(VPN.Status.CONNECTED);

			DoStartForeground();
			
            OnServiceStarted();
		}		

		public void HandleThreadException(Exception e)
		{
			EddieLogger.Error(e);
			
            LastError = e.Message;

            DoStop();
		}
	
		private void DoStopService()
		{
			CleanupTunnel();
			
            DoStopForeground();
			
            StopSelf();
			
            DoChangeStatus(VPN.Status.NOT_CONNECTED);
			
            OnServiceStopped();
		}

		private void CleanupTunnel()
		{
			try
			{
				if(vpnTunnel != null)
					vpnTunnel.Cleanup();
			}
			catch(Exception e)
			{
				LastError = "Tunnel stop failed: " + e.Message;
			}
			finally
			{
				vpnTunnel = null;
			}			
		}

		public void DoChangeStatus(VPN.Status status)
		{
			string lastError = "";

			lock(dataSync)
			{
				if(vpnStatus == status)
					return;

				vpnStatus = status;
				
                if(vpnLastError.Equals(""))
                    lastError = vpnLastError;
			}

            if(clientMessenger != null)
                SendMessage(clientMessenger, CreateStatusMessage(status, lastError));
		}

		private Message CreateStatusMessage(VPN.Status status, string lastError)
		{
			Message message = Message.Obtain(null, VPNService.MSG_STATUS, (int)status, 0);
			
            message.Data.PutString(MSG_STATUS_BUNDLE_LAST_ERROR, lastError);
			
            return message;
		}

		private void SendMessage(Messenger client, Message message)
		{
			client.Send(message);
		}

		private void DoStart(Bundle data)
		{
			LastError = "";
			
            DoChangeStatus(VPN.Status.CONNECTING);

			if((Application as AndroidApplication).Initialized)
			{
				try
				{
					TunnelSetup(data);
				}
				catch(Exception e)
				{
					LastError = "Tunnel start failed: " + e.Message;
					
                    DoStopService();
				}					

                Java.Lang.Thread newVpnTask = SupportTools.StartThread(new Java.Lang.Runnable(() =>
                {
                    EddieLogger.Info("Starting VPN thread");

                    vpnTunnel.Run();
                }));

                if(newVpnTask != null)        
                    vpnThread = newVpnTask;
			}
			else
			{
				LastError = "Initialization failed";
				
                DoStopService();
			}
		}

		private void DoStop()
		{
			DoChangeStatus(VPN.Status.DISCONNECTING);

			SupportTools.StartThread(new Java.Lang.Runnable(() =>
			{
                DoStopService();

                currentNotificationText = "";

				WaitForVpnThreadToFinish();
			}));
		}

		public PendingIntent CreateConfigIntent()
		{
			Intent configIntent = new Intent(this, typeof(SettingsActivity));
			//configIntent.PutExtra("...")
			
            configIntent.AddFlags(ActivityFlags.ReorderToFront);
			
            return PendingIntent.GetActivity(this, 0, configIntent, 0);
		}

		private void TunnelSetup(Bundle data)
		{
			if(vpnTunnel != null)
				throw new Exception("internal error (m_tunnel already initialized)");

			if(data == null)
				throw new Exception("internal error (data bundle is null)");

			vpnTunnel = new OpenVPNTunnel(this);
			
            vpnTunnel.Init();
		
			string profile = data.GetString(PARAM_PROFILE, "");
			
            if(profile.Length == 0)
				throw new Exception("no profile defined");

			vpnTunnel.LoadProfileString(profile);
			
            vpnTunnel.BindOptions();
		}

        private void WaitForVpnThreadToFinish()
        {
            if(vpnThread == null)
                return;

            try
            {
                vpnThread.Join(THREAD_MAX_JOIN_TIME);
            }
            catch(Java.Lang.InterruptedException)
            {
                EddieLogger.Error("VPNService.WaitVpnThreadToFinish(): VPN thread has been interrupted");
            }
            finally
            {
                if(vpnThread.IsAlive)
                    EddieLogger.Error("VPNService.WaitVpnThreadToFinish(): VPN thread did not end");
                else
                    EddieLogger.Info("VPN thread execution has completed");
            }
        }

		private void DoStartForeground()
		{
			if(settingsManager.SystemPersistentNotification && (notification == null))
			{
                string text, server = "";
                Dictionary<string, string> pData = settingsManager.SystemLastProfileInfo;

                string channelId = Resources.GetString(Resource.String.notification_channel_id);
                string channelName = Resources.GetString(Resource.String.notification_channel_name);

                if(Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
                {
                    NotificationManager notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);

                    NotificationChannel notificationChannel = new NotificationChannel(channelId, channelName, NotificationImportance.High);

                    notificationManager.CreateNotificationChannel(notificationChannel);
                }

                if(pData.Count > 0 && pData.ContainsKey("server"))
                    server = pData["server"];

                text = String.Format(Resources.GetString(Resource.String.notification_text), server);
                
                if(!NetworkStatusReceiver.GetNetworkDescription().Equals(""))
                    text += " " + String.Format(Resources.GetString(Resource.String.notification_network), NetworkStatusReceiver.GetNetworkDescription());

                notification = new NotificationCompat.Builder(this);
                
				notification.SetContentTitle(Resources.GetString(Resource.String.notification_title))
                            .SetStyle(new NotificationCompat.BigTextStyle().BigText(text))
                            .SetContentText(text)
							.SetSmallIcon(Resource.Drawable.notification_icon)
                            .SetColor(Resource.Color.notificationColor)
							.SetContentIntent(BuildMainActivityIntent())
                            .SetChannelId(channelId)
                            .SetPriority(NotificationCompat.PriorityHigh)
                            .SetOngoing(true);

                if(Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.O)
                {
                    if(settingsManager.SystemNotificationSound)
                        notification.SetSound(Settings.System.DefaultNotificationUri);
                    else
                        notification.SetSound(Android.Net.Uri.Parse("android.resource://" +  ApplicationContext.PackageName + "/" + Resource.Raw.silence));
                }
    
                StartForeground(SERVICE_RUNNING_NOTIFICATION_ID, notification.Build());
                
                currentNotificationText = text;
			}			
		}

        public void UpdateNotification(string text)
        {
            if(settingsManager.SystemPersistentNotification && notification != null && !text.Equals("") && !text.Equals(currentNotificationText))
            {
                string channelId = Resources.GetString(Resource.String.notification_channel_id);
                string channelName = Resources.GetString(Resource.String.notification_channel_name);

                NotificationManager notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
    
                if(Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
                {
                    NotificationChannel notificationChannel = new NotificationChannel(channelId, channelName, NotificationImportance.High);
                    
                    notificationManager.CreateNotificationChannel(notificationChannel);
                }
    
                notification = new NotificationCompat.Builder(this);
                
                notification.SetContentTitle(Resources.GetString(Resource.String.notification_title))
                            .SetStyle(new NotificationCompat.BigTextStyle().BigText(text))
                            .SetContentText(text)
                            .SetSmallIcon(Resource.Drawable.notification_icon)
                            .SetColor(Resource.Color.notificationColor)
                            .SetContentIntent(BuildMainActivityIntent())
                            .SetChannelId(channelId)
                            .SetPriority(NotificationCompat.PriorityHigh)
                            .SetOngoing(true);

                if(Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.O)
                {
                    if(settingsManager.SystemNotificationSound)
                        notification.SetSound(Settings.System.DefaultNotificationUri);
                    else
                        notification.SetSound(Android.Net.Uri.Parse("android.resource://" +  ApplicationContext.PackageName + "/" + Resource.Raw.silence));
                }
                
                notificationManager.Notify(SERVICE_RUNNING_NOTIFICATION_ID, notification.Build());
                
                currentNotificationText = text;
            }
        }

        public void AlertNotification(string message)
        {
            if(message.Equals("") && !message.Equals(currentNotificationText))
                return;

            string channelId = Resources.GetString(Resource.String.notification_channel_id);
            string channelName = Resources.GetString(Resource.String.notification_channel_name);

            NotificationManager notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);

            if(Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                NotificationChannel notificationChannel = new NotificationChannel(channelId, channelName, NotificationImportance.High);

                notificationManager.CreateNotificationChannel(notificationChannel);
            }

            NotificationCompat.Builder alertNotification = new NotificationCompat.Builder(this, channelId);
            
            alertNotification.SetContentTitle(Resources.GetString(Resource.String.notification_title))
                             .SetSmallIcon(Resource.Drawable.notification_icon)
                             .SetColor(Resource.Color.notificationColor)
                             .SetStyle(new NotificationCompat.BigTextStyle().BigText(message))
                             .SetContentText(message)
                             .SetContentIntent(BuildMainActivityIntent())
                             .SetChannelId(channelId)
                             .SetPriority(NotificationCompat.PriorityHigh)
                             .SetAutoCancel(true);

            if(Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.O)
            {
                if(settingsManager.SystemNotificationSound)
                    notification.SetSound(Settings.System.DefaultNotificationUri);
                else
                    notification.SetSound(Android.Net.Uri.Parse("android.resource://" +  ApplicationContext.PackageName + "/" + Resource.Raw.silence));
            }

            notificationManager.Notify(alertNotificationId, alertNotification.Build());

            alertNotificationId++;
            
            currentNotificationText = message;
        }

		private PendingIntent BuildMainActivityIntent()
		{
			Intent intent = new Intent(this, typeof(MainActivity));
			
            intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.SingleTop | ActivityFlags.ClearTop);
			
            return PendingIntent.GetActivity(this, 0, intent, 0);
		}

		private void DoStopForeground()
		{
			if(notification != null)
			{
				StopForeground(true);

				notification.Dispose();
				
                notification = null;
			}			
		}
        
        // NetworkStatusReceiver
        
        public void OnNetworkStatusNotAvailable()
        {
            EddieLogger.Info("Network is not available");

            NetworkStatusChanged(OpenVPNTunnel.VPNAction.PAUSE);
        }

        public void OnNetworkStatusConnected()
        {
            string text, server = "";
            Dictionary<string, string> pData = settingsManager.SystemLastProfileInfo;

            EddieLogger.Info("Network is connected to {0}", NetworkStatusReceiver.GetNetworkDescription());

            NetworkStatusChanged(OpenVPNTunnel.VPNAction.RESUME);

            if(pData.Count > 0 && pData.ContainsKey("server"))
                server = pData["server"];

            text = String.Format(Resources.GetString(Resource.String.notification_text), server);
            
            if(!NetworkStatusReceiver.GetNetworkDescription().Equals(""))
                text += " " + String.Format(Resources.GetString(Resource.String.notification_network), NetworkStatusReceiver.GetNetworkDescription());

            UpdateNotification(text);
        }
    
        public void OnNetworkStatusIsConnecting()
        {
            EddieLogger.Info("Network is connecting");

            NetworkStatusChanged(OpenVPNTunnel.VPNAction.PAUSE);
        }
    
        public void OnNetworkStatusIsDisonnecting()
        {
            EddieLogger.Info("Network is disconnecting");

            NetworkStatusChanged(OpenVPNTunnel.VPNAction.PAUSE);
        }
    
        public void OnNetworkStatusSuspended()
        {
            EddieLogger.Info("Network is suspended");

            NetworkStatusChanged(OpenVPNTunnel.VPNAction.PAUSE);
        }
    
        public void OnNetworkStatusNotConnected()
        {
            EddieLogger.Info("Network is not connected");

            NetworkStatusChanged(OpenVPNTunnel.VPNAction.PAUSE);
        }
    
        public void OnNetworkTypeChanged()
        {
            EddieLogger.Info("Network type has changed to {0}", NetworkStatusReceiver.GetNetworkDescription());

            NetworkStatusChanged(OpenVPNTunnel.VPNAction.NETWORK_TYPE_CHANGED);
        }
	}
}
