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
using System.Threading;
using Eddie.Common.Tasks;
using System.Collections.Generic;
using Eddie.Common.Log;

namespace Eddie.NativeAndroidApp
{
	[Service(Name="org.airvpn.eddie.VPNService", Permission="android.permission.BIND_VPN_SERVICE")]
	[IntentFilter(new[] { global::Android.Net.VpnService.ServiceInterface })]
	public class VPNService : global::Android.Net.VpnService, IMessageHandler
	{
		public const int SERVICE_RUNNING_NOTIFICATION_ID = 10000;
		public const string PARAM_START = "START";
		public const string PARAM_STOP = "STOP";
		public const string PARAM_PROFILE = "PROFILE";

		public const string EXTRA_RUN_ARGS = "RUN_ARGS";

		public const int MSG_BIND = 0;
		public const int MSG_BIND_ARG_REMOVE = 0;
		public const int MSG_BIND_ARG_ADD = 1;

		public const int MSG_START = 1;
		public const int MSG_STOP = 2;
		public const int MSG_STATUS = 3;		
		public const string MSG_STATUS_BUNDLE_LAST_ERROR = "LAST_ERROR";

		private object m_dataSync = new object();
		private VPN.Status m_status = VPN.Status.NotConnected;
		private string m_lastError = "";
		private Notification m_notification = null;

		private Messenger m_serviceMessenger = null;
		private TasksManager m_tasksManager = null;
		private TaskEx m_vpnTask = null;
		private object m_vpnTaskSync = new object();
		private OpenVPNTunnel m_tunnel = null;
		private object m_threadsSync = new object();

		private List<Messenger> m_serviceClients = new List<Messenger>();
		private object m_clientsSync = new object();

		private ScreenReceiver m_screenReceiver = null;
        private SettingsManager settingsManager = new SettingsManager();

		private class ScreenReceiver : BroadcastReceiver
		{
			private VPNService m_service = null;

			public ScreenReceiver(VPNService service)
			{
				m_service = service;
			}

			public override void OnReceive(Context context, Intent intent)
			{
				string action = intent.Action;

				LogsManager.Instance.Debug(string.Format("ScreenReceiver.OnReceive (action='{0}')", action));

				if(action == global::Android.Content.Intent.ActionScreenOn)
                    m_service.OnScreenChanged(true);
				else if(action == global::Android.Content.Intent.ActionScreenOff)
                    m_service.OnScreenChanged(false);
				else
                    LogsManager.Instance.Error(string.Format("Unhandled action '{0}' received in ScreenReceiver", action));				
			}
		}

		public TasksManager TasksManager
		{
			get
			{
				return m_tasksManager;
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
					LogsManager.Instance.Debug("VPNService.OnStartCommand: START");
				
					run = true;
				}					
				else if(intent.GetBooleanExtra(PARAM_STOP, false))
				{
					LogsManager.Instance.Debug("VPNService.OnStartCommand: STOP");

					run = false;					
				}					
			}
			else
			{
				LogsManager.Instance.Debug("VPNService.OnStartCommand (no intent)");
			}
		
			if(run != null)
				UpdateService(run.Value, intent.GetBundleExtra(EXTRA_RUN_ARGS));
			
			return result;
		}

		public override IBinder OnBind(Intent intent)
		{
			return m_serviceMessenger != null ? m_serviceMessenger.Binder : null;
		}

		private void Init()
		{
			m_serviceMessenger = new Messenger(new MessageHandler(this));
			
            m_tasksManager = (Application as AndroidApplication).TasksManager;			
		}

		private void Cleanup()
		{
			m_tasksManager = null;

			if(m_serviceMessenger != null)
			{
				m_serviceMessenger.Dispose();
				
                m_serviceMessenger = null;
			}			
		}

		private string LastError
		{
			set
			{
				lock(m_dataSync)
				{
					m_lastError = value;
				}
			}
		}

		public override void OnCreate()
		{
			base.OnCreate();

			Init();
		}
	
		public override void OnDestroy()
		{
			UpdateService(false);
			Cleanup();

			base.OnDestroy();
		}

		public override void OnRevoke()
		{
			LogsManager.Instance.Error("VPNService.OnRevoke");

			// Another VPN may have started here or Android is shutting down the VPN: by now there is no way to "block" the operation and the service must be stopped gracefully

			UpdateService(false);

			base.OnRevoke();
		}

		public void OnMessage(Message msg)
		{
			//LogsManager.Instance.Debug("ServiceHandler.OnMessage");

			switch(msg.What)
			{
    			case VPNService.MSG_BIND:
                {
                	UpdateClients(msg.ReplyTo, msg.Arg1 == MSG_BIND_ARG_ADD ? true : false);
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

				lock(m_dataSync)
				{
					status = m_status;
					lastError = m_lastError;
				}

				SendMessage(msg.ReplyTo, CreateStatusMessage(status, lastError));
			}				
		}

		private void UpdateClients(Messenger client, bool add)
		{
			if(client == null)
				return;

			lock(m_clientsSync)
			{
				if(add)
				{
					m_serviceClients.Add(client);
				}
				else
				{
					// Do NOT compare Messenger by reference (JniIdentityHashCode must be used)

					List<Messenger>.Enumerator iterator = m_serviceClients.GetEnumerator();
					
                    while(iterator.MoveNext())
					{
						Messenger current = iterator.Current;
						
                        if(current.JniIdentityHashCode == client.JniIdentityHashCode)
						{
							m_serviceClients.RemoveAt(m_serviceClients.IndexOf(current));
							break;
						}
					}
				}
			}
		}

		private bool UpdateService(bool run, Bundle data = null)
		{
			lock(m_dataSync)
			{
				bool running = (m_status == VPN.Status.Connected) || (m_status == VPN.Status.Connecting);
				
                if(running == run)
					return true;

				if(m_status == VPN.Status.Disconnecting)
					return false;	// Do not allow start requests while stopping

				// Must be under m_dataSync to ensure "m_status" synchronization
				
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
			LogsManager.Instance.Debug(string.Format("VPNService.OnScreenChanged: active='{0}'", active));

			try
			{
				if(m_tunnel != null)
					m_tunnel.HandleScreenChanged(active);
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnScreenChanged", e);
			}			
		}

		private void EnsureReceivers()
		{
			if(m_screenReceiver != null)
				return;

			IntentFilter intentFilter = new IntentFilter();
			
            intentFilter.AddAction(Intent.ActionScreenOn);
			intentFilter.AddAction(Intent.ActionScreenOff);
			
            m_screenReceiver = new ScreenReceiver(this);
			
            RegisterReceiver(m_screenReceiver, intentFilter);
		}

		private void CleanupReceivers()
		{
			if(m_screenReceiver != null)
			{
				UnregisterReceiver(m_screenReceiver);

				SupportTools.SafeDispose(m_screenReceiver);

                m_screenReceiver = null;
			}
		}

		public void HandleThreadStarted()
		{
			DoChangeStatus(VPN.Status.Connected);

			lock(m_threadsSync)
			{
				DoStartForeground();
				OnServiceStarted();
			}			
		}		

		public void HandleThreadException(Exception e)
		{
			LogsManager.Instance.Error(e);
			
            LastError = e.Message;

			HandleThreadStopped();
		}
	
		public void HandleThreadStopped()
		{
			// OnThreadStopped MUST be called from a separated thread since the current one (the caller) could otherwise wait for himself to exit (because of DoStopVPN's call)

			m_tasksManager.Add((CancellationToken c) => OnThreadStopped());
		}

		private void OnThreadStopped()
		{
			lock(m_threadsSync)
			{				
				DoStopService();
			}
		
			DoStopVPN();					
		}

		private void DoStopService()
		{
			CleanupTunnel();
			DoStopForeground();
			StopSelf();
			DoChangeStatus(VPN.Status.NotConnected);
			OnServiceStopped();
		}

		private void CleanupTunnel()
		{
			try
			{
				if(m_tunnel != null)
					m_tunnel.Cleanup();
			}
			catch(Exception e)
			{
				LastError = "Tunnel stop failed: " + e.Message;
			}
			finally
			{
				m_tunnel = null;
			}			
		}

		private void DoChangeStatus(VPN.Status status)
		{
			string lastError = "";

			lock(m_dataSync)
			{
				if(m_status == status)
					return;

				m_status = status;
				lastError = m_lastError;
			}

			DispatchMessage(CreateStatusMessage(status, lastError));
		}

		private Message CreateStatusMessage(VPN.Status status, string lastError)
		{
			Message message = Message.Obtain(null, VPNService.MSG_STATUS, (int)status, 0);
			
            message.Data.PutString(MSG_STATUS_BUNDLE_LAST_ERROR, lastError);
			
            return message;
		}

		private void DispatchMessage(Message message)
		{
			Messenger[] clients = null;

			lock(m_clientsSync)
			{
				clients = m_serviceClients.ToArray();
			}			

			foreach(Messenger client in clients)
			{
				SendMessage(client, message);
			}
		}

		private void SendMessage(Messenger client, Message message)
		{
			client.Send(message);
		}

		private void DoStart(Bundle data)
		{
			LastError = "";
			
            DoChangeStatus(VPN.Status.Connecting);

			m_tasksManager.Add((CancellationToken c) =>
			{
				lock(m_threadsSync)
				{
					if((Application as AndroidApplication).Initialized)
					{
						try
						{
							DoStartTunnel(data);
						}
						catch(Exception e)
						{
							LastError = "Tunnel start failed: " + e.Message;
							
                            DoStopService();
						}					
					}
					else
					{
						LastError = "Initialization failed";
						
                        DoStopService();
					}
				}
			});
		}
	
		private void DoStop()
		{
			DoChangeStatus(VPN.Status.Disconnecting);

			m_tasksManager.Add((CancellationToken c) =>
			{
				DoStopVPN();
			});
		}
		
		public PendingIntent CreateConfigIntent()
		{
			Intent configIntent = new Intent(this, typeof(SettingsActivity));
			//configIntent.PutExtra("...")
			
            configIntent.AddFlags(ActivityFlags.ReorderToFront);
			
            return PendingIntent.GetActivity(this, 0, configIntent, 0);
		}

		private void DoStartTunnel(Bundle data)
		{
			if(m_tunnel != null)
				throw new Exception("internal error (m_tunnel already initialized)");

			if(data == null)
				throw new Exception("internal error (data bundle is null)");

			m_tunnel = new OpenVPNTunnel(this);
			m_tunnel.Init();
		
			string profile = data.GetString(PARAM_PROFILE, "");
			
            if(profile.Length == 0)
				throw new Exception("no profile defined");

			m_tunnel.LoadProfileString(profile);
			m_tunnel.BindOptions();

			TaskEx vpnTask = m_tasksManager.Add((CancellationToken c) =>
			{
				m_tunnel.Run(c);
			});
			
            lock(m_vpnTaskSync)
			{
				m_vpnTask = vpnTask;
			}
		}

		private void DoStopVPN()
		{
			lock(m_vpnTaskSync)
			{
				if(m_vpnTask != null)
				{
					try
					{
						m_vpnTask.Cancel();
						m_vpnTask.Wait();
					}
					finally
					{
						m_vpnTask = null;
					}									
				}
			}			
		}

		private void DoStartForeground()
		{
			if(settingsManager.SystemShowNotification && (m_notification == null))
			{
                Dictionary<string, string> pData = settingsManager.SystemLastProfileInfo;
                string nText = Resources.GetString(Resource.String.notification_text);
                
                nText += " " + pData["server"];

                m_notification = new Notification.Builder(this)
							.SetContentTitle(Resources.GetString(Resource.String.notification_title))
							.SetContentText(nText)
							.SetSmallIcon(Resource.Drawable.notification_icon)
                            .SetColor(Resource.Color.notificationColor)
							.SetContentIntent(BuildMainActivityIntent())
							.SetOngoing(true)
							.Build();

                StartForeground(SERVICE_RUNNING_NOTIFICATION_ID, m_notification);
			}			
		}

		private PendingIntent BuildMainActivityIntent()
		{
			Intent intent = new Intent(this, typeof(MainActivity));
			
            intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.SingleTop | ActivityFlags.ClearTop);
			
            return PendingIntent.GetActivity(this, 0, intent, 0);
		}

		private void DoStopForeground()
		{
			if(m_notification != null)
			{
				StopForeground(true);

				m_notification.Dispose();
				
                m_notification = null;
			}			
		}
	}
}
