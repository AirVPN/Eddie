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

using Android.Content;
using Android.Net;
using System;
using Android.App;
using Android.OS;
using System.Collections.Generic;
using System.IO;
using Eddie.Common.Log;

namespace Eddie.Droid
{
	// BindService requires to inherit from Java.Lang.Object
	public class AndroidVPNManager : Java.Lang.Object, IServiceConnection, IVPNManager, IMessageHandler
	{
		private static int VPN_REQUEST_CODE = 123;

		private VPN.ServiceStatus m_status = VPN.ServiceStatus.Stopped;
		private string m_lastError = "";
		private Context m_context = null;
		public event VPN.OnStatusChanged StatusChanged;
		private bool m_ready = false;
		private bool m_bound = false;
		private Messenger m_serviceMessenger = null;
		private Messenger m_clientMessenger = null;
		private object m_dataSync = new object();
		private List<string> m_profilesFiles = new List<string>();
		private List<string> m_profilesStrings = new List<string>();

		public AndroidVPNManager(Context context)
		{
			m_context = context;
		}

		public VPN.ServiceStatus Status
		{
			get
			{
				lock(m_dataSync)
				{
					return m_status;
				}					
			}
		}

		public bool Ready
		{
			get
			{
				lock(m_dataSync)
				{
					return m_ready;
				}
			}
		}

		public string LastError
		{
			get
			{
				lock(m_dataSync)
				{
					return m_lastError;
				}					
			}
		}

		public void AddProfileFile(string filename)
		{
			if(Utils.Empty(filename))
				throw new Exception("invalid profile filename");

			m_profilesFiles.Add(filename);			
		}

		public void AddProfileString(string str)
		{
			if(Utils.Empty(str))
				throw new Exception("invalid profile string");

			m_profilesStrings.Add(str);			
		}

		public void ClearProfiles()
		{
			m_profilesFiles.Clear();
			m_profilesStrings.Clear();
		}

		public void ShareContent(string title, string data)
		{
			Intent sharingIntent = new Intent(Android.Content.Intent.ActionSend);
			sharingIntent.SetType("text/plain");
			sharingIntent.PutExtra(Android.Content.Intent.ExtraTitle, title);
			sharingIntent.PutExtra(Android.Content.Intent.ExtraSubject, title);
			sharingIntent.PutExtra(Android.Content.Intent.ExtraText, data);
			m_context.StartActivity(Intent.CreateChooser(sharingIntent, "Share using:"));
		}

		public string GetDownloadDirectory()
		{
			return "/sdcard/Download";
		}

		public void EditOptions()
		{
			m_context.StartActivity(typeof(OptionsActivity));
		}

		public bool Start()
		{
			try
			{
				SendStartMessage();

				return true;
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error(e);

				OnStatusChanged(null, null, e.Message);
			}

			return false;				
		}

		public void Stop()
		{
			try
			{
				SendStopMessage();
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error(e);

				OnStatusChanged(null, null, e.Message);
			}			
		}
		/*
		private bool UpdateService(VPN.ServiceStatus status)
		{
			lock(m_dataSync)
			{
				if(m_status == status)
					return true;

				switch(m_status)
				{
				case VPN.ServiceStatus.Starting:
				case VPN.ServiceStatus.Stopping:
													return false;
				}

				bool done = false;				
				switch(status)
				{
				case VPN.ServiceStatus.Started:		done = PrepareStart();
													break;

				case VPN.ServiceStatus.Stopped:		done = PrepareStop();
													break;
				}

				return done;
			}
		}
		

		private bool PrepareStart()
		{
			if(m_status != VPN.ServiceStatus.Stopped)
				return false;

			OnStatusChanged(VPN.ServiceStatus.Starting);

			m_tasksManager.Add((CancellationToken c) =>
			{
				m_cancelRequest = c;
				OnStart();				
			});

			return true;
		}
		
		private bool PrepareStop()
		{
			if(m_status != VPN.ServiceStatus.Started)
				return false;

			OnStatusChanged(VPN.ServiceStatus.Stopping);

			m_tasksManager.Add((CancellationToken c) =>
			{
				m_cancelRequest = c;
				OnStop();				
			});			

			return true;
		}
		*/
		/*
		private void OnStart()
		{
			// The first time Android requires a confirmation from the user to start the VPN service			

			//Intent confirmIntent = VpnService.Prepare(m_context.ApplicationContext);
			//if(confirmIntent != null)
			//	m_context.StartActivityForResult(confirmIntent, VPN_REQUEST_CODE);
			//else
			//	HandleActivityResult(VPN_REQUEST_CODE, Result.Ok, null);

			//SendStartMessage();
		}
		*/
		public void HandleActivityResult(int requestCode, Result resultCode, Intent data)
		{
			if(requestCode != VPN_REQUEST_CODE)
				return;

			//if(HandleCancellationRequest())
				//return;			

			if(resultCode == Result.Ok)
				DoBindService();
		}

		public void HandleActivityStart()
		{
			BindService();
		}

		public void HandleActivityStop()
		{
			UnbindService();
		}

		private Intent CreateServiceIntent()
		{
			return new Intent(m_context, typeof(AndroidVPNService));
		}

		/*
		private void OnStop()
		{
			try
			{
				SendStopMessage();
				OnStatusChanged(VPN.ServiceStatus.Stopped);
			}
			catch(Exception e)
			{
				OnStatusChanged(VPN.ServiceStatus.Stopped, e.Message);
			}			
		}
		*/
			
		private void OnStatusChanged(bool? ready, VPN.ServiceStatus? status = null, string error = "")
		{
			lock(m_dataSync)
			{
				if(ready != null)
					m_ready = ready.Value;
				else
					ready = m_ready;

				if(status != null)
					m_status = status.Value;
				else
					status = m_status;

				m_lastError = error;
			}

			if(StatusChanged != null)
				StatusChanged(ready.Value, status.Value, error);
		}

		public void OnServiceConnected(ComponentName name, IBinder service)
		{
			m_bound = true;

			m_clientMessenger = new Messenger(new MessageHandler(this));
			m_serviceMessenger = new Messenger(service);

			//OnStatusChanged(true);
			lock(m_dataSync)
			{
				m_ready = true; // OnStatusChanged will be raised by the next SendBindMessage
			}
			SendBindMessage(true);
		}

		public void OnServiceDisconnected(ComponentName name)
		{
			// This is called when the connection with the service has been unexpectedly disconnected -- that is, its process crashed.
			UnbindService(false);
		}

		private void SendStartMessage()
		{
			m_context.StartService(CreateServiceIntent());

			SendMessage(Message.Obtain(null, AndroidVPNService.MSG_START), CreateProfileBundle());
			
			/*
			Message message = Message.Obtain ();
			Bundle b = new Bundle ();
			b.PutString ("InputText", "text from client");
			message.Data = b;
			*/
		}

		private Bundle CreateProfileBundle()
		{
			if((m_profilesFiles.Count == 0) && (m_profilesStrings.Count == 0))
				return null;

			string profile = "";

			foreach(string profileFile in m_profilesFiles)
			{
				if(profile.Length > 0)
					profile += "\n";

				profile += File.ReadAllText(profileFile);
			}
			foreach(string profileString in m_profilesStrings)
			{
				if(profile.Length > 0)
					profile += "\n";

				profile += profileString;
			}
	
			Bundle data = new Bundle();
			data.PutString(AndroidVPNService.PARAM_PROFILE, profile.Replace("\r\n", "\n"));
			return data;
		}
		
		private void SendStopMessage()
		{
			SendMessage(Message.Obtain(null, AndroidVPNService.MSG_STOP));		
		}

		private void SendBindMessage(bool bind)
		{
			SendMessage(Message.Obtain(null, AndroidVPNService.MSG_BIND, bind ? AndroidVPNService.MSG_BIND_ARG_ADD : AndroidVPNService.MSG_BIND_ARG_REMOVE, 0));
		}

		private void SendMessage(Message msg, Bundle payload = null)
		{
			if(m_serviceMessenger == null)
				throw new Exception("internal error (m_serviceMessenger is null)");

			msg.ReplyTo = m_clientMessenger;
			if(payload != null)     // Allows to define msg.Data from outside
				msg.Data = payload;

			m_serviceMessenger.Send(msg);
		}

		protected override void Dispose(bool disposing)
		{
			UnbindService();

			base.Dispose(disposing);
		}

		private void BindService()
		{
			Intent confirmIntent = VpnService.Prepare(m_context.ApplicationContext);
			if(confirmIntent != null)
			{
				// The only case when m_context is not an Activity is from the BootReceiver but in that case the VPN confirmation should already been granted and the confirmIntent will be null

				Activity activity = m_context as Activity;
				if(activity != null)
					activity.StartActivityForResult(confirmIntent, VPN_REQUEST_CODE);
				else
					LogsManager.Instance.Error("Failed to cast context to Activity");
			}
			else
			{
				HandleActivityResult(VPN_REQUEST_CODE, Result.Ok, null);
			}
		}

		private void DoBindService()
		{
			try
			{
				if(!m_context.BindService(CreateServiceIntent(), this, Bind.AutoCreate))
					throw new Exception("BindService failed");
			}
			catch(Exception e)
			{
				OnStatusChanged(null, null, e.Message);
			}
		}

		private void UnbindService(bool unbind = true)
		{
			if(m_clientMessenger != null)
			{
				if(unbind)
					SendBindMessage(false);

				m_clientMessenger.Dispose();
				m_clientMessenger = null;
			}

			if(m_bound)
			{
				if(unbind)
					m_context.UnbindService(this);

				m_bound = false;
			}

			if(m_serviceMessenger != null)
			{
				m_serviceMessenger.Dispose();
				m_serviceMessenger = null;
			}

			OnStatusChanged(false);
		}

		public void OnMessage(Message msg)
		{
			//LogsManager.Instance.Debug("AndroidVPNManager.OnMessage(What={0})", msg.What);

			switch(msg.What)
			{
			case AndroidVPNService.MSG_STATUS:		OnStatusChanged(null, (VPN.ServiceStatus) msg.Arg1, msg.Data.GetString(AndroidVPNService.MSG_STATUS_BUNDLE_LAST_ERROR, ""));
													break;
			}	
		}
	}
}
