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
// 19 July 2018 - author: ProMIND - initial release.

using System.Collections.Generic;
using Android.Content;
using Android.Net;

namespace Eddie.NativeAndroidApp
{
    [BroadcastReceiver(Name="org.airvpn.eddie.NetworkStatusReceiver")]

    public class NetworkStatusReceiver : BroadcastReceiver
    {
        public enum Status
        {
            UNKNOWN = 1,
            NOT_AVAILABLE,
            CONNECTED,
            IS_CONNECTING,
            IS_DISCONNECTING,
            SUSPENDED,
            NOT_CONNECTED
        };

        private static Status NetworkStatus = Status.UNKNOWN;
        private static ConnectivityType NetworkType = ConnectivityType.Dummy;
        private static ConnectivityType CurrentNetworkType = ConnectivityType.Dummy;
        private static string NetworkDescription = "";
        private static string CurrentNetworkDescription = "";

        private static HashSet<INetworkStatusReceiverListener> registeredListeners = null;

        public NetworkStatusReceiver()
        {
            if(registeredListeners == null)
                registeredListeners = new HashSet<INetworkStatusReceiverListener>();
        }

        public override void OnReceive(Context context, Intent intent)
        {
            if(intent == null || intent.Extras == null)
                return;
    
            ConnectivityManager manager = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
    
            NetworkInfo networkInfo = manager.ActiveNetworkInfo;

            if(networkInfo != null)
            {
                NetworkInfo.State connectionStatus = networkInfo.GetState();
    
                if(connectionStatus == NetworkInfo.State.Connected)
                {
                    NetworkStatus = Status.CONNECTED;
        
                    NetworkType = networkInfo.Type;
        
                    NetworkDescription = networkInfo.TypeName;
                }
                else if(connectionStatus == NetworkInfo.State.Disconnected)
                {
                    NetworkStatus = Status.NOT_CONNECTED;
        
                    NetworkType = ConnectivityType.Dummy;
        
                    NetworkDescription = "";
                }
                else if(connectionStatus == NetworkInfo.State.Connecting)
                {
                    NetworkStatus = Status.IS_CONNECTING;
        
                    NetworkType = ConnectivityType.Dummy;
        
                    NetworkDescription = "";
                }
                else if(connectionStatus == NetworkInfo.State.Disconnecting)
                {
                    NetworkStatus = Status.IS_DISCONNECTING;
        
                    NetworkType = ConnectivityType.Dummy;
        
                    NetworkDescription = "";
                }
                else if(connectionStatus == NetworkInfo.State.Suspended)
                {
                    NetworkStatus = Status.SUSPENDED;
        
                    NetworkType = ConnectivityType.Dummy;
        
                    NetworkDescription = "";
                }
                else if(connectionStatus == NetworkInfo.State.Unknown)
                {
                    NetworkStatus = Status.UNKNOWN;
        
                    NetworkType = ConnectivityType.Dummy;
        
                    NetworkDescription = "";
                }
            }
            else
            {
                NetworkStatus = Status.NOT_AVAILABLE;
    
                NetworkType = ConnectivityType.Dummy;
    
                NetworkDescription = "";
            }
    
            NotifyStateToAll();
            
            CurrentNetworkType = NetworkType;
            CurrentNetworkDescription = NetworkDescription;
        }

        private void NotifyStateToAll()
        {
            foreach(INetworkStatusReceiverListener listener in registeredListeners)
                NotifyState(listener);
        }

        private void NotifyState(INetworkStatusReceiverListener listener)
        {
            if(listener == null)
                return;

            if(CurrentNetworkType != NetworkType)
                listener.OnNetworkTypeChanged();

            switch(NetworkStatus)
            {
                case Status.NOT_AVAILABLE:
                {
                    listener.OnNetworkStatusNotAvailable();
                }
                break;

                case Status.CONNECTED:
                {
                    listener.OnNetworkStatusConnected();
                }
                break;

                case Status.IS_CONNECTING:
                {
                    listener.OnNetworkStatusIsConnecting();
                }
                break;

                case Status.IS_DISCONNECTING:
                {
                    listener.OnNetworkStatusIsDisonnecting();
                }
                break;

                case Status.SUSPENDED:
                {
                    listener.OnNetworkStatusSuspended();
                }
                break;

                case Status.NOT_CONNECTED:
                {
                    listener.OnNetworkStatusNotConnected();
                }
                break;

                default:
                {
                    listener.OnNetworkStatusNotAvailable();
                }
                break;
            }
        }

        public void AddListener(INetworkStatusReceiverListener l)
        {
            if(l == null || registeredListeners == null)
                return;

            registeredListeners.Add(l);
    
            NotifyState(l);
        }

        public void RemoveListener(INetworkStatusReceiverListener l)
        {
            if(l == null || registeredListeners == null)
                return;

            registeredListeners.Remove(l);
        }

        public static Status GetNetworkStatus()
        {
            return NetworkStatus;
        }

        public static bool IsNetworkConnected()
        {
            return (NetworkStatus == Status.CONNECTED);
        }

        public static ConnectivityType GetNetworkType()
        {
            return CurrentNetworkType;
        }

        public static string GetNetworkDescription()
        {
            return CurrentNetworkDescription;
        }
    }
}
