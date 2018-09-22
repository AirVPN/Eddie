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
using System.Runtime.InteropServices;

namespace Eddie.NativeAndroidApp
{
	public static class NativeMethods
	{
		public static int SUCCESS = 0;
		public static int ERROR = -1;
		
		public static bool Succeeded(int result)
		{
			return result >= 0;
		}
	
		public static bool Failed(int result)
		{
			return result < 0;
		}

        public enum ResultCode
        {
            ERROR = -1,
            SUCCESS,
            EXCEPTION_ERROR,
            OPENVPN_NOT_INITIALIZED,
            OPENVPN_ALREADY_INITIALIZED,
            BREAKPAD_INITIALIZATION_ERROR,
            OPENVPN_POINTER_IS_NULL,
            FAILED_TO_CREATE_OPENVPN_CLIENT,
            OPENVPN_TRANSPORT_STATS_POINTER_IS_NULL,
            PROFILE_FILENAME_IS_NULL,
            PROFILE_STRING_IS_NULL,
            OPENVPN_OPTION_NAME_IS_NULL,
            OPENVPN_OPTION_VALUE_IS_NULL,
            OPENVPN_UNKNOWN_OPTION,
            OPENVPN_PROFILE_IS_EMPTY,
            OPENVPN_PROFILE_ERROR,
            OPENVPN_CONFIG_EVAL_ERROR,
            OPENVPN_CREDS_ERROR,
            OPENVPN_CONNECTION_ERROR
        }

        public enum EventType
		{
            FATAL_ERROR = -4,
            ERROR = -3,
            FORMAL_WARNING = -2,
            MESSAGE = -1,
            DISCONNECTED = 0,
            CONNECTED,
            RECONNECTING,
            AUTH_PENDING,
            RESOLVE,
            WAIT,
            WAIT_PROXY,
            CONNECTING,
            GET_CONFIG,
            ASSIGN_IP,
            ADD_ROUTES,
            ECHO_OPT,
            INFO,
            WARN,
            PAUSE,
            RESUME,
            RELAY,
            UNSUPPORTED_FEATURE,
            TRANSPORT_ERROR,
            TUN_ERROR,
            CLIENT_RESTART,
            AUTH_FAILED,
            CERT_VERIFY_FAIL,
            TLS_VERSION_MIN,
            CLIENT_HALT,
            CLIENT_SETUP,
            CONNECTION_TIMEOUT,
            INACTIVE_TIMEOUT,
            DYNAMIC_CHALLENGE,
            PROXY_NEED_CREDS,
            PROXY_ERROR,
            TUN_SETUP_FAILED,
            TUN_IFACE_CREATE,
            TUN_IFACE_DISABLED,
            EPKI_ERROR,
            EPKI_INVALID_ALIAS,
            RELAY_ERROR,
            N_TYPES
		}

        public struct EddieLibraryResult
        {
            public NativeMethods.ResultCode code;
            
            [MarshalAs(UnmanagedType.LPStr)]
            public string description;
        }

		public struct EddieLibraryEvent
		{
			public NativeMethods.EventType type;
            
            [MarshalAs(UnmanagedType.LPStr)]
            public string name;      
            
            [MarshalAs(UnmanagedType.LPStr)]
            public string info;
            
            [MarshalAs(UnmanagedType.LPStruct)]
            public IntPtr data;
		}

		public struct ovpn3_connection_data
		{
			public int defined;
			
            [MarshalAs(UnmanagedType.LPStr)]
			public string user;
			
            [MarshalAs(UnmanagedType.LPStr)]
			public string serverHost;
			
            [MarshalAs(UnmanagedType.LPStr)]
			public string serverPort;
			
            [MarshalAs(UnmanagedType.LPStr)]
			public string serverProto;
			
            [MarshalAs(UnmanagedType.LPStr)]
			public string serverIp;
			
            [MarshalAs(UnmanagedType.LPStr)]
			public string vpnIp4;
			
            [MarshalAs(UnmanagedType.LPStr)]
			public string vpnIp6;
			
            [MarshalAs(UnmanagedType.LPStr)]
			public string gw4;
			
            [MarshalAs(UnmanagedType.LPStr)]
			public string gw6;
			
            [MarshalAs(UnmanagedType.LPStr)]
			public string clientIp;
			
            [MarshalAs(UnmanagedType.LPStr)]
			public string tunName;
		}

		public struct ovpn3_transport_stats
		{
			public ulong bytes_in;
			public ulong bytes_out;
			public ulong packets_in;
			public ulong packets_out;
			public int last_packet_received;
		}		

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int socket_protect_ptr(int socket);
		
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void on_event_ptr(ref EddieLibraryEvent e);
		
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int tun_builder_new_ptr();
		
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int tun_builder_set_layer_ptr(int layer);
		
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int tun_builder_set_remote_address_ptr([MarshalAs(UnmanagedType.LPStr)] string address, int ipv6);
		
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int tun_builder_add_address_ptr([MarshalAs(UnmanagedType.LPStr)] string address, int prefix_length, [MarshalAs(UnmanagedType.LPStr)] string gateway, int ipv6, int net30);
		
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int tun_builder_set_route_metric_default_ptr(int metric);
		
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int tun_builder_reroute_gw_ptr(int ipv4, int ipv6, int flags);
		
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int tun_builder_add_route_ptr([MarshalAs(UnmanagedType.LPStr)] string address, int prefix_length, int metric, int ipv6);
		
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int tun_builder_exclude_route_ptr([MarshalAs(UnmanagedType.LPStr)] string address, int prefix_length, int metric, int ipv6);
		
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int tun_builder_add_dns_server_ptr([MarshalAs(UnmanagedType.LPStr)] string address, int ipv6);
		
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int tun_builder_add_search_domain_ptr([MarshalAs(UnmanagedType.LPStr)] string domain);
		
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int tun_builder_set_mtu_ptr(int mtu);
		
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int tun_builder_set_session_name_ptr([MarshalAs(UnmanagedType.LPStr)] string name);
		
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int tun_builder_add_proxy_bypass_ptr([MarshalAs(UnmanagedType.LPStr)] string bypass_host);
		
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int tun_builder_set_proxy_auto_config_url_ptr([MarshalAs(UnmanagedType.LPStr)] string url);
		
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int tun_builder_set_proxy_http_ptr([MarshalAs(UnmanagedType.LPStr)] string host, int port);
		
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int tun_builder_set_proxy_https_ptr([MarshalAs(UnmanagedType.LPStr)] string host, int port);
		
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int tun_builder_add_wins_server_ptr([MarshalAs(UnmanagedType.LPStr)] string address);
		
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int tun_builder_set_block_ipv6_ptr(int block_ipv6);
		
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int tun_builder_set_adapter_domain_suffix_ptr([MarshalAs(UnmanagedType.LPStr)] string name);
		
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int tun_builder_establish_ptr();
		
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int tun_builder_persist_ptr();
		
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void tun_builder_establish_lite_ptr();
		
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void tun_builder_teardown_ptr(int disconnect);
		
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void connect_attach_ptr();
		
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void connect_pre_run_ptr();
		
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void connect_run_ptr();
		
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void connect_session_stop_ptr();
		
        public struct ovpn3_client
		{
			public socket_protect_ptr socket_protect;
			public on_event_ptr on_event;
			public tun_builder_new_ptr tun_builder_new;
			public tun_builder_set_layer_ptr tun_builder_set_layer;
			public tun_builder_set_remote_address_ptr tun_builder_set_remote_address;
			public tun_builder_add_address_ptr tun_builder_add_address;
			public tun_builder_set_route_metric_default_ptr tun_builder_set_route_metric_default;
			public tun_builder_reroute_gw_ptr tun_builder_reroute_gw;
			public tun_builder_add_route_ptr tun_builder_add_route;
			public tun_builder_exclude_route_ptr tun_builder_exclude_route;
			public tun_builder_add_dns_server_ptr tun_builder_add_dns_server;
			public tun_builder_add_search_domain_ptr tun_builder_add_search_domain;
			public tun_builder_set_mtu_ptr tun_builder_set_mtu;
			public tun_builder_set_session_name_ptr tun_builder_set_session_name;
			public tun_builder_add_proxy_bypass_ptr tun_builder_add_proxy_bypass;
			public tun_builder_set_proxy_auto_config_url_ptr tun_builder_set_proxy_auto_config_url;
			public tun_builder_set_proxy_http_ptr tun_builder_set_proxy_http;
			public tun_builder_set_proxy_https_ptr tun_builder_set_proxy_https;
			public tun_builder_add_wins_server_ptr tun_builder_add_wins_server;
			public tun_builder_set_block_ipv6_ptr tun_builder_set_block_ipv6;
			public tun_builder_set_adapter_domain_suffix_ptr tun_builder_set_adapter_domain_suffix;
			public tun_builder_establish_ptr tun_builder_establish;
			public tun_builder_persist_ptr tun_builder_persist;
			public tun_builder_establish_lite_ptr tun_builder_establish_lite;
			public tun_builder_teardown_ptr tun_builder_teardown;
			public connect_attach_ptr connect_attach;
			public connect_pre_run_ptr connect_pre_run;
			public connect_run_ptr connect_run;
			public connect_session_stop_ptr connect_session_stop;
		}

		public const string EddieLibName = "eddie";		

        [DllImport(EddieLibName)]
        private static extern IntPtr Name();
        
        [DllImport(EddieLibName)]
        private static extern IntPtr Version();
        
        [DllImport(EddieLibName)]
        private static extern IntPtr QualifiedName();
        
        [DllImport(EddieLibName)]
        private static extern IntPtr ReleaseDate();
		
        [DllImport(EddieLibName)]
		private static extern int ApiLevel();
		
        [DllImport(EddieLibName)]
		private static extern EddieLibraryResult InitOpenVPN();
		
        [DllImport(EddieLibName)]
		private static extern EddieLibraryResult CleanUpOpenVPN();		
		
        [DllImport(EddieLibName)]
		private static extern EddieLibraryResult CreateOpenVPNClient(ref ovpn3_client ci);
		
        [DllImport(EddieLibName)]
		private static extern EddieLibraryResult DisposeOpenVPNClient();
		
        [DllImport(EddieLibName)]
		private static extern EddieLibraryResult StartOpenVPNClient();
		
        [DllImport(EddieLibName)]
		private static extern EddieLibraryResult StopOpenVPNClient();
		
        [DllImport(EddieLibName)]
		private static extern EddieLibraryResult PauseOpenVPNClient([MarshalAs(UnmanagedType.LPStr)] string reason);
		
        [DllImport(EddieLibName)]
		private static extern EddieLibraryResult ResumeOpenVPNClient();
		
        [DllImport(EddieLibName)]
		private static extern EddieLibraryResult GetOpenVPNClientTransportStatus(ref ovpn3_transport_stats stats);
		
        [DllImport(EddieLibName)]
		private static extern EddieLibraryResult LoadProfileToOpenVPNClient([MarshalAs(UnmanagedType.LPStr)] string filename);
		
        [DllImport(EddieLibName)]
		private static extern EddieLibraryResult LoadStringProfileToOpenVPNClient([MarshalAs(UnmanagedType.LPStr)] string str);
		
        [DllImport(EddieLibName)]
		private static extern EddieLibraryResult SetOpenVPNClientOption([MarshalAs(UnmanagedType.LPStr)] string option, [MarshalAs(UnmanagedType.LPStr)] string value);

		public static class EddieLibrary
		{
            public static string LibraryName()
            {
                return CharStarToString(Name());
            }

            public static string LibraryVersion()
            {
                return CharStarToString(Version());
            }

            public static string LibraryQualifiedName()
            {
                return CharStarToString(QualifiedName());
            }

            public static string LibraryReleaseDate()
            {
                return CharStarToString(ReleaseDate());
            }

            public static int LibraryApiLevel()
            {
                return ApiLevel();
            }

			public static EddieLibraryResult Init()
			{
				return InitOpenVPN();
			}

			public static EddieLibraryResult Cleanup()
			{
				return CleanUpOpenVPN();
			}		
		
			public static EddieLibraryResult CreateClient(ref ovpn3_client ci)
			{
				return CreateOpenVPNClient(ref ci);
			}

			public static EddieLibraryResult DisposeClient()
			{
				return DisposeOpenVPNClient();
			}			

			public static EddieLibraryResult StartClient()
			{
				return StartOpenVPNClient();
			}

			public static EddieLibraryResult StopClient()
			{
				return StopOpenVPNClient();
			}

			public static EddieLibraryResult PauseClient(string reason)
			{
				return PauseOpenVPNClient(reason);
			}

			public static EddieLibraryResult ResumeClient()
			{
				return ResumeOpenVPNClient();
			}
			
			public static EddieLibraryResult GetClientTransportStats(ref ovpn3_transport_stats stats)
			{
				return GetOpenVPNClientTransportStatus(ref stats);
			}

			public static EddieLibraryResult LoadProfileToClient(string filename)
			{
				return LoadProfileToOpenVPNClient(filename);
			}

			public static EddieLibraryResult LoadStringProfileToClient(string str)
			{
				return LoadStringProfileToOpenVPNClient(str);
			}

			public static EddieLibraryResult SetClientOption(string option, string value)
			{
				return SetOpenVPNClientOption(option, value);
			}
            
            private static string CharStarToString(IntPtr charStarPointer)
            {
                byte[] charStarLen = null;
                int charLength = 0;
                string convertedString = "";

                if(charStarPointer == IntPtr.Zero)
                    return convertedString;

                while(System.Runtime.InteropServices.Marshal.ReadByte(charStarPointer, charLength) != 0)
                    charLength++;

                if(charLength > 0)
                {                
                    charStarLen = new byte[charLength];
                    
                    System.Runtime.InteropServices.Marshal.Copy(charStarPointer, charStarLen, 0, charLength);
                
                    convertedString = System.Text.Encoding.UTF8.GetString(charStarLen);
                }
                
                return convertedString;
            }
		}
	}
}
