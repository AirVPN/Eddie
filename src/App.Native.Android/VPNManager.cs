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

using Android.Content;
using System;
using Android.App;
using Android.OS;
using System.Collections.Generic;
using Eddie.Common.Log;

namespace Eddie.NativeAndroidApp
{
    public class VPN
    {
        public enum Status
        {
            UNKNOWN,
            NOT_CONNECTED,
            DISCONNECTING,
            CONNECTING,
            CONNECTED,
            PAUSED,
            LOCKED
        }

        public static int DescriptionResource(Status s)
        {
            int res = 0;
            
            switch(s)
            {
                case VPN.Status.NOT_CONNECTED:
                {
                    res = Resource.String.vpn_status_not_connected;
                }
                break;

                case VPN.Status.DISCONNECTING:
                {
                    res = Resource.String.vpn_status_disconnecting;
                }
                break;

                case VPN.Status.CONNECTING:
                {
                    res = Resource.String.vpn_status_connecting;
                }
                break;

                case VPN.Status.CONNECTED:
                {
                    res = Resource.String.vpn_status_connected;
                }
                break;

                case VPN.Status.PAUSED:
                {
                    res = Resource.String.vpn_status_paused;
                }
                break;

                case VPN.Status.LOCKED:
                {
                    res = Resource.String.vpn_status_locked;
                }
                break;
                
                default:
                {
                    res = Resource.String.vpn_status_not_connected;
                }
                break;
            }
            
            return res;
        }

        public delegate void OnStatusChanged(bool ready, Status status, string error);       
    }   

    // BindService requires to inherit from Java.LangVPN.Object

	public class VPNManager : Java.Lang.Object, IServiceConnection, IMessageHandler
	{
		public const int VPN_REQUEST_CODE = 123;

		private VPN.Status m_status = VPN.Status.NOT_CONNECTED;
		private string m_lastError = "";
		private Context m_context = null;
		public event VPN.OnStatusChanged StatusChanged;
		private bool m_ready = false;
		private bool m_bound = false;
		private Messenger m_serviceMessenger = null;
		private Messenger m_clientMessenger = null;
		private object m_dataSync = new object();
		private string m_profile = "";
		private List<string> m_profilesStrings = new List<string>();
        private SupportTools supportTools = null;

		public VPNManager(Context context)
		{
			m_context = context;

            supportTools = new SupportTools(m_context);
		}

		public VPN.Status VpnStatus
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

		public void SetProfile(string profile)
		{
			m_profile = profile;
		}

		public void AddProfileString(string str)
		{
			if(SupportTools.Empty(str))
				throw new Exception("invalid profile string");

			m_profilesStrings.Add(str);			
		}

		public void ClearProfiles()
		{
			m_profile = "";
			
            m_profilesStrings.Clear();
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
			return new Intent(m_context, typeof(VPNService));
		}

		private void OnStatusChanged(bool? ready, VPN.Status ? status = null, string error = "")
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

			SendMessage(Message.Obtain(null, VPNService.MSG_START), CreateProfileBundle());
		}

		private Bundle CreateProfileBundle()
		{
			if((m_profile.Equals("")) && (m_profilesStrings.Count == 0))
				return null;

			string profile = "";

            if(m_profilesStrings.Count == 0)
            {
                profile = m_profile + "\n";
            }
            else
            {
                bool directivesAdded = false;

                m_profile = m_profile.Replace("\r\n", "\n");

                string[] lines = m_profile.Split('\n');

    			foreach(string curLine in lines)
    			{
                    if(directivesAdded == false && !curLine.Equals("") && curLine[0] == '<')
                    {
                        foreach(string profileString in m_profilesStrings)
            				profile += profileString + "\n";
                            
                        directivesAdded = true;
                    }

                    profile += curLine + "\n";
                }
            }

			Bundle data = new Bundle();
			
            data.PutString(VPNService.PARAM_PROFILE, profile.Replace("\r\n", "\n"));
			
            return data;
		}
		
		private void SendStopMessage()
		{
			SendMessage(Message.Obtain(null, VPNService.MSG_STOP));		
		}

		private void SendBindMessage(bool bind)
		{
			SendMessage(Message.Obtain(null, VPNService.MSG_BIND, bind ? VPNService.MSG_BIND_ARG_ADD : VPNService.MSG_BIND_ARG_REMOVE, 0));
		}

		private void SendMessage(Message msg, Bundle payload = null)
		{
			if(m_serviceMessenger == null)
            {
                supportTools.InfoDialog(string.Format(m_context.Resources.GetString(Resource.String.conn_cannot_start_vpnservice)));
                
                LogsManager.Instance.Error("VPNManager::SendMessage: {0}", "m_serviceMessenger == null");

                return;
            }

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
			Intent confirmIntent = global::Android.Net.VpnService.Prepare(m_context.ApplicationContext);
			
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
            if(msg == null)
                return;

            // LogsManager.Instance.Debug("VPNManager.OnMessage(What={0})", msg.What);

			switch(msg.What)
			{
			    case VPNService.MSG_STATUS:
                {
                    OnStatusChanged(null, (VPN.Status) msg.Arg1, msg.Data.GetString(VPNService.MSG_STATUS_BUNDLE_LAST_ERROR, ""));
                }
				break;
			}	
		}
	}
}
