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

using System.Collections.Generic;
using System.Text;
using Android.Content;

namespace Eddie.NativeAndroidApp
{
    public class SettingsManager
    {
        public const string OVPN3_OPTION_TLS_MIN_VERSION = "ovpn3_tls_version_min";
        public const string OVPN3_OPTION_TLS_MIN_VERSION_NATIVE = "tls_version_min";
        public static readonly string OVPN3_OPTION_TLS_MIN_VERSION_DEFAULT = "tls_1_0";

        public const string OVPN3_OPTION_PROTOCOL = "ovpn3_protocol";
        public const string OVPN3_OPTION_PROTOCOL_NATIVE = "protocol";
        public static readonly string OVPN3_OPTION_PROTOCOL_DEFAULT = "";

        public const string OVPN3_OPTION_IPV6 = "ovpn3_ipv6";
        public const string OVPN3_OPTION_IPV6_NATIVE = "ipv6";
        public static readonly string OVPN3_OPTION_IPV6_DEFAULT = "";

        public const string OVPN3_OPTION_TIMEOUT = "ovpn3_timeout";
        public const string OVPN3_OPTION_TIMEOUT_NATIVE = "timeout";
        public static readonly string OVPN3_OPTION_TIMEOUT_DEFAULT = "60";

        public const string OVPN3_OPTION_TUN_PERSIST = "ovpn3_tun_persist";
        public const string OVPN3_OPTION_TUN_PERSIST_NATIVE = "tun_persist";
        public static readonly bool OVPN3_OPTION_TUN_PERSIST_DEFAULT = true;

        public const string OVPN3_OPTION_COMPRESSION_MODE = "ovpn3_compression_mode";
        public const string OVPN3_OPTION_COMPRESSION_MODE_NATIVE = "compression_mode";
        public static readonly string OVPN3_OPTION_COMPRESSION_MODE_DEFAULT = "yes";

        public const string OVPN3_OPTION_USERNAME = "ovpn3_username";
        public const string OVPN3_OPTION_USERNAME_NATIVE = "username";
        public static readonly string OVPN3_OPTION_USERNAME_DEFAULT = "";

        public const string OVPN3_OPTION_PASSWORD = "ovpn3_password";
        public const string OVPN3_OPTION_PASSWORD_NATIVE = "password";
        public static readonly string OVPN3_OPTION_PASSWORD_DEFAULT = "";

        public const string OVPN3_OPTION_SYNCHRONOUS_DNS_LOOKUP = "ovpn3_synchronous_dns_lookup";
        public const string OVPN3_OPTION_SYNCHRONOUS_DNS_LOOKUP_NATIVE = "synchronous_dns_lookup";
        public static readonly bool OVPN3_OPTION_SYNCHRONOUS_DNS_LOOKUP_DEFAULT = false;

        public const string OVPN3_OPTION_AUTOLOGIN_SESSIONS = "ovpn3_autologin_sessions";
        public const string OVPN3_OPTION_AUTOLOGIN_SESSIONS_NATIVE = "autologin_sessions";
        public static readonly bool OVPN3_OPTION_AUTOLOGIN_SESSIONS_DEFAULT = true;

        public const string OVPN3_OPTION_DISABLE_CLIENT_CERT = "ovpn3_disable_client_cert";
        public const string OVPN3_OPTION_DISABLE_CLIENT_CERT_NATIVE = "disable_client_cert";
        public static readonly bool OVPN3_OPTION_DISABLE_CLIENT_CERT_DEFAULT = false;

        public const string OVPN3_OPTION_SSL_DEBUG_LEVEL = "ovpn3_ssl_debug_level";
        public const string OVPN3_OPTION_SSL_DEBUG_LEVEL_NATIVE = "ssl_debug_level";
        public static readonly string OVPN3_OPTION_SSL_DEBUG_LEVEL_DEFAULT = "0";

        public const string OVPN3_OPTION_PRIVATE_KEY_PASSWORD = "ovpn3_private_key_password";
        public const string OVPN3_OPTION_PRIVATE_KEY_PASSWORD_NATIVE = "private_key_password";
        public static readonly string OVPN3_OPTION_PRIVATE_KEY_PASSWORD_DEFAULT = "";

        public const string OVPN3_OPTION_DEFAULT_KEY_DIRECTION = "ovpn3_default_key_direction";
        public const string OVPN3_OPTION_DEFAULT_KEY_DIRECTION_NATIVE = "default_key_direction";
        public static readonly string OVPN3_OPTION_DEFAULT_KEY_DIRECTION_DEFAULT = "-1";

        public const string OVPN3_OPTION_FORCE_AES_CBC_CIPHERSUITES = "ovpn3_force_aes_cbc_ciphersuites";
        public const string OVPN3_OPTION_FORCE_AES_CBC_CIPHERSUITES_NATIVE = "force_aes_cbc_ciphersuites";
        public static readonly bool OVPN3_OPTION_FORCE_AES_CBC_CIPHERSUITES_DEFAULT = false;

        public const string OVPN3_OPTION_TLS_CERT_PROFILE = "ovpn3_tls_cert_profile";
        public const string OVPN3_OPTION_TLS_CERT_PROFILE_NATIVE = "tls_cert_profile";
        public static readonly string OVPN3_OPTION_TLS_CERT_PROFILE_DEFAULT = "";

        public const string OVPN3_OPTION_PROXY_HOST = "ovpn3_proxy_host";
        public const string OVPN3_OPTION_PROXY_HOST_NATIVE = "proxy_host";
        public static readonly string OVPN3_OPTION_PROXY_HOST_DEFAULT = "";

        public const string OVPN3_OPTION_PROXY_PORT = "ovpn3_proxy_port";
        public const string OVPN3_OPTION_PROXY_PORT_NATIVE = "proxy_port";
        public static readonly string OVPN3_OPTION_PROXY_PORT_DEFAULT = "";

        public const string OVPN3_OPTION_PROXY_USERNAME = "ovpn3_proxy_username";
        public const string OVPN3_OPTION_PROXY_USERNAME_NATIVE = "proxy_username";
        public static readonly string OVPN3_OPTION_PROXY_USERNAME_DEFAULT = "";

        public const string OVPN3_OPTION_PROXY_PASSWORD = "ovpn3_proxy_password";
        public const string OVPN3_OPTION_PROXY_PASSWORD_NATIVE = "proxy_password";
        public static readonly string OVPN3_OPTION_PROXY_PASSWORD_DEFAULT = "";

        public const string OVPN3_OPTION_PROXY_ALLOW_CLEARTEXT_AUTH = "ovpn3_proxy_allow_cleartext_auth";
        public const string OVPN3_OPTION_PROXY_ALLOW_CLEARTEXT_AUTH_NATIVE = "proxy_allow_cleartext_auth";
        public static readonly bool OVPN3_OPTION_PROXY_ALLOW_CLEARTEXT_AUTH_DEFAULT = false;

        public const string OVPN3_OPTION_CUSTOM_DIRECTIVES = "ovpn3_custom_directives";
        public static readonly string OVPN3_OPTION_CUSTOM_DIRECTIVES_DEFAULT = "";

        public const string SYSTEM_OPTION_DNS_OVERRIDE_ENABLE = "system_dns_override_enable";
        public static readonly bool SYSTEM_OPTION_DNS_OVERRIDE_ENABLE_DEFAULT = false;

        public const string SYSTEM_OPTION_DNS_CUSTOM = "system_dns_custom";
        public static readonly string SYSTEM_OPTION_DNS_CUSTOM_DEFAULT = "";

        public const string SYSTEM_OPTION_DNS_ALTERNATIVE = "system_dns_alternative";
        public static readonly string SYSTEM_OPTION_DNS_ALTERNATIVE_DEFAULT = "";

        public const string SYSTEM_OPTION_PROXY_ENABLE = "system_proxy_enable";
        public static readonly bool SYSTEM_OPTION_PROXY_ENABLE_DEFAULT = false;

        public const string SYSTEM_OPTION_SHOW_NOTIFICATION = "system_show_notification";
        public static readonly bool SYSTEM_OPTION_SHOW_NOTIFICATION_DEFAULT = true;

        public const string SYSTEM_CUSTOM_MTU = "system_forced_mtu";
        public static readonly string SYSTEM_CUSTOM_MTU_DEFAULT = "";

        public const string SYSTEM_OPTION_APPLICATION_FILTER_TYPE = "system_application_filter_type";
        public static readonly string SYSTEM_OPTION_APPLICATION_FILTER_TYPE_NONE = "0";
        public static readonly string SYSTEM_OPTION_APPLICATION_FILTER_TYPE_WHITELIST = "1";
        public static readonly string SYSTEM_OPTION_APPLICATION_FILTER_TYPE_BLACKLIST = "2";
        public static readonly string SYSTEM_OPTION_APPLICATION_FILTER_TYPE_DEFAULT = SYSTEM_OPTION_APPLICATION_FILTER_TYPE_NONE;

        public const string SYSTEM_OPTION_APPLICATION_FILTER = "system_application_filter";
        public static readonly string SYSTEM_OPTION_APPLICATION_FILTER_DEFAULT = "";

        public const string SYSTEM_OPTION_FIRST_RUN = "system_first_run";
        public static readonly bool SYSTEM_OPTION_FIRST_RUN_DEFAULT = true;

        public const string SYSTEM_OPTION_RESTORE_LAST_PROFILE = "system_restore_last_profile";
        public static readonly bool SYSTEM_OPTION_RESTORE_LAST_PROFILE_DEFAULT = true;

        public const string SYSTEM_OPTION_LAST_PROFILE_IS_CONNECTED = "system_last_profile_is_connected";
        public static readonly bool SYSTEM_OPTION_LAST_PROFILE_DEFAULT_IS_CONNECTED = false;

        public const string SYSTEM_OPTION_LAST_PROFILE = "system_last_profile";

        public const string SYSTEM_OPTION_LAST_PROFILE_INFO = "system_last_profile_info";

        public const string SYSTEM_OPTION_PAUSE_VPN_WHEN_SCREEN_IS_OFF = "system_pause_vpn_when_screen_is_off";
        public static readonly bool SYSTEM_OPTION_PAUSE_VPN_WHEN_SCREEN_IS_OFF_DEFAULT = false;

        public const char DEFAULT_SPLIT_SEPARATOR = ',';

        private ISharedPreferences appPrefs = null;
        private ISharedPreferencesEditor prefsEditor = null;
    
        public SettingsManager()
        {
            appPrefs = Android.Preferences.PreferenceManager.GetDefaultSharedPreferences(Android.App.Application.Context);
            
            prefsEditor = appPrefs.Edit();
        }

        public string Ovpn3TLSMinVersion
        {
            get
            {
                return GetString(OVPN3_OPTION_TLS_MIN_VERSION, OVPN3_OPTION_TLS_MIN_VERSION_DEFAULT);
            }

            set
            {
                SaveString(OVPN3_OPTION_TLS_MIN_VERSION, value);
            }
        }

        public string Ovpn3Protocol
        {
            get
            {
                return GetString(OVPN3_OPTION_PROTOCOL, OVPN3_OPTION_PROTOCOL_DEFAULT);
            }

            set
            {
                SaveString(OVPN3_OPTION_PROTOCOL, value);
            }
        }

        public string Ovpn3IPV6
        {
            get
            {
                return GetString(OVPN3_OPTION_IPV6, OVPN3_OPTION_IPV6_DEFAULT);
            }

            set
            {
                SaveString(OVPN3_OPTION_IPV6, value);
            }
        }

        public string Ovpn3Timeout
        {
            get
            {
                return GetString(OVPN3_OPTION_TIMEOUT, OVPN3_OPTION_TIMEOUT_DEFAULT);
            }

            set
            {
                SaveString(OVPN3_OPTION_TIMEOUT, value);
            }
        }


        public bool Ovpn3TunPersist
        {
            get
            {
                return GetBool(OVPN3_OPTION_TUN_PERSIST, OVPN3_OPTION_TUN_PERSIST_DEFAULT);
            }

            set
            {
                SaveBool(OVPN3_OPTION_TUN_PERSIST, value);
            }
        }

        public string Ovpn3CompressionMode
        {
            get
            {
                return GetString(OVPN3_OPTION_COMPRESSION_MODE, OVPN3_OPTION_COMPRESSION_MODE_DEFAULT);
            }

            set
            {
                SaveString(OVPN3_OPTION_COMPRESSION_MODE, value);
            }
        }

        public string Ovpn3Username
        {
            get
            {
                return GetString(OVPN3_OPTION_USERNAME, OVPN3_OPTION_USERNAME_DEFAULT);
            }

            set
            {
                SaveString(OVPN3_OPTION_USERNAME, value);
            }
        }
        
        public string Ovpn3Password
        {
            get
            {
                return GetString(OVPN3_OPTION_PASSWORD, OVPN3_OPTION_PASSWORD_DEFAULT);
            }

            set
            {
                SaveString(OVPN3_OPTION_PASSWORD, value);
            }
        }

        public bool Ovpn3SynchronousDNSLookup
        {
            get
            {
                return GetBool(OVPN3_OPTION_SYNCHRONOUS_DNS_LOOKUP, OVPN3_OPTION_SYNCHRONOUS_DNS_LOOKUP_DEFAULT);
            }

            set
            {
                SaveBool(OVPN3_OPTION_SYNCHRONOUS_DNS_LOOKUP, value);
            }
        }

        public bool Ovpn3AutologinSessions
        {
            get
            {
                return GetBool(OVPN3_OPTION_AUTOLOGIN_SESSIONS, OVPN3_OPTION_AUTOLOGIN_SESSIONS_DEFAULT);
            }

            set
            {
                SaveBool(OVPN3_OPTION_AUTOLOGIN_SESSIONS, value);
            }
        }
        
        public bool Ovpn3DisableClientCert
        {
            get
            {
                return GetBool(OVPN3_OPTION_DISABLE_CLIENT_CERT, OVPN3_OPTION_DISABLE_CLIENT_CERT_DEFAULT);
            }

            set
            {
                SaveBool(OVPN3_OPTION_DISABLE_CLIENT_CERT, value);
            }
        }

        public string Ovpn3SSLDebugLevel
        {
            get
            {
                return GetString(OVPN3_OPTION_SSL_DEBUG_LEVEL, OVPN3_OPTION_SSL_DEBUG_LEVEL_DEFAULT);
            }

            set
            {
                SaveString(OVPN3_OPTION_SSL_DEBUG_LEVEL, value);
            }
        }

        public long Ovpn3SSLDebugLevelValue
        {
            get
            {
                long defVal = 0;
                
                try
                {
                    defVal = long.Parse(OVPN3_OPTION_SSL_DEBUG_LEVEL_DEFAULT);
                }
                catch(System.FormatException)
                {
                    defVal = 0;
                }

                return GetLong(OVPN3_OPTION_SSL_DEBUG_LEVEL, defVal);
            }

            set
            {
                string prefVal = "";

                if(value <= 0)
                    prefVal = OVPN3_OPTION_SSL_DEBUG_LEVEL_DEFAULT;
                else
                    prefVal = value.ToString();

                SaveString(OVPN3_OPTION_SSL_DEBUG_LEVEL, prefVal);
            }
        }

        public string Ovpn3PrivateKeyPassword
        {
            get
            {
                return GetString(OVPN3_OPTION_PRIVATE_KEY_PASSWORD, OVPN3_OPTION_PRIVATE_KEY_PASSWORD_DEFAULT);
            }

            set
            {
                SaveString(OVPN3_OPTION_PRIVATE_KEY_PASSWORD, value);
            }
        }

        public string Ovpn3DefaultKeyDirection
        {
            get
            {
                return GetString(OVPN3_OPTION_DEFAULT_KEY_DIRECTION, OVPN3_OPTION_DEFAULT_KEY_DIRECTION_DEFAULT);
            }

            set
            {
                SaveString(OVPN3_OPTION_DEFAULT_KEY_DIRECTION, value);
            }
        }

        public bool Ovpn3ForceAESCBCCiphersuites
        {
            get
            {
                return GetBool(OVPN3_OPTION_FORCE_AES_CBC_CIPHERSUITES, OVPN3_OPTION_FORCE_AES_CBC_CIPHERSUITES_DEFAULT);
            }

            set
            {
                SaveBool(OVPN3_OPTION_FORCE_AES_CBC_CIPHERSUITES, value);
            }
        }

        public string Ovpn3TLSCertProfile
        {
            get
            {
                return GetString(OVPN3_OPTION_TLS_CERT_PROFILE, OVPN3_OPTION_TLS_CERT_PROFILE_DEFAULT);
            }

            set
            {
                SaveString(OVPN3_OPTION_TLS_CERT_PROFILE, value);
            }
        }

        public string Ovpn3ProxyHost
        {
            get
            {
                return GetString(OVPN3_OPTION_PROXY_HOST, OVPN3_OPTION_PROXY_HOST_DEFAULT);
            }
        
            set
            {
                SaveString(OVPN3_OPTION_PROXY_HOST, value);
            }
        }
    
        public string Ovpn3ProxyPort
        {
            get
            {
                return GetString(OVPN3_OPTION_PROXY_PORT, OVPN3_OPTION_PROXY_PORT_DEFAULT);
            }

            set
            {
                SaveString(OVPN3_OPTION_PROXY_PORT, value);
            }
        }

        public long Ovpn3ProxyPortValue
        {
            get
            {
                long defVal = 0;
                
                try
                {
                    defVal = long.Parse(OVPN3_OPTION_PROXY_PORT_DEFAULT);
                }
                catch(System.FormatException)
                {
                    defVal = 0;
                }

                return GetLong(OVPN3_OPTION_PROXY_PORT, defVal);
            }

            set
            {
                string prefVal = "";

                if(value <= 0)
                    prefVal = OVPN3_OPTION_PROXY_PORT_DEFAULT;
                else
                    prefVal = value.ToString();

                SaveString(OVPN3_OPTION_PROXY_PORT, prefVal);
            }
        }

        public string Ovpn3ProxyUsername
        {
            get
            {
                return GetString(OVPN3_OPTION_PROXY_USERNAME, OVPN3_OPTION_PROXY_USERNAME_DEFAULT);
            }

            set
            {
                SaveString(OVPN3_OPTION_PROXY_USERNAME, value);
            }
        }

        public string Ovpn3ProxyPassword
        {
            get
            {
                return GetString(OVPN3_OPTION_PROXY_PASSWORD, OVPN3_OPTION_PROXY_PASSWORD_DEFAULT);
            }

            set
            {
                SaveString(OVPN3_OPTION_PROXY_PASSWORD, value);
            }
        }

        public bool Ovpn3ProxyAllowCleartextAuth
        {
            get
            {
                return GetBool(OVPN3_OPTION_PROXY_ALLOW_CLEARTEXT_AUTH, OVPN3_OPTION_PROXY_ALLOW_CLEARTEXT_AUTH_DEFAULT);
            }

            set
            {
                SaveBool(OVPN3_OPTION_PROXY_ALLOW_CLEARTEXT_AUTH, value);
            }
        }

        public string Ovpn3CustomDirectives
        {
            get
            {
                return GetString(OVPN3_OPTION_CUSTOM_DIRECTIVES, OVPN3_OPTION_CUSTOM_DIRECTIVES_DEFAULT);
            }

            set
            {
                SaveString(OVPN3_OPTION_CUSTOM_DIRECTIVES, value);
            }
        }   

        public bool SystemDNSOverrideEnable
        {
            get
            {
                return GetBool(SYSTEM_OPTION_DNS_OVERRIDE_ENABLE, SYSTEM_OPTION_DNS_OVERRIDE_ENABLE_DEFAULT);
            }

            set
            {
                SaveBool(SYSTEM_OPTION_DNS_OVERRIDE_ENABLE, value);
            }
        }

        public string SystemDNSCustom
        {
            get
            {
                return GetString(SYSTEM_OPTION_DNS_CUSTOM, SYSTEM_OPTION_DNS_CUSTOM_DEFAULT);
            }

            set
            {
                SaveString(SYSTEM_OPTION_DNS_CUSTOM, value);
            }
        }

        public List<string> SystemDNSCustomList
        {
            get
            {
                List<string> list = new List<string>();
                string[] valArray = null;
                string prefVal = "";

                prefVal = GetString(SYSTEM_OPTION_DNS_CUSTOM, SYSTEM_OPTION_DNS_CUSTOM_DEFAULT);

                valArray = prefVal.Split(DEFAULT_SPLIT_SEPARATOR);

                foreach(string item in valArray)
                    list.Add(item);

                return list;
            }
            
            set
            {
                string prefVal = "";
                
                foreach(string item in value)
                {
                    if(!prefVal.Equals(""))
                        prefVal += DEFAULT_SPLIT_SEPARATOR;
                        
                    prefVal += item;
                }

                if(prefVal != null)
                    SaveString(SYSTEM_OPTION_DNS_CUSTOM, prefVal);
            }
        }

        public string SystemDNSAlternative
        {
            get
            {
                return GetString(SYSTEM_OPTION_DNS_ALTERNATIVE, SYSTEM_OPTION_DNS_ALTERNATIVE_DEFAULT);
            }

            set
            {
                SaveString(SYSTEM_OPTION_DNS_ALTERNATIVE, value);
            }
        }

        public List<string> SystemDNSAlternativeList
        {
            get
            {
                List<string> list = new List<string>();
                string[] valArray = null;
                string prefVal = "";

                prefVal = GetString(SYSTEM_OPTION_DNS_ALTERNATIVE, SYSTEM_OPTION_DNS_ALTERNATIVE_DEFAULT);

                valArray = prefVal.Split(DEFAULT_SPLIT_SEPARATOR);

                foreach(string item in valArray)
                    list.Add(item);

                return list;
            }
            
            set
            {
                string prefVal = "";
                
                foreach(string item in value)
                {
                    if(!prefVal.Equals(""))
                        prefVal += DEFAULT_SPLIT_SEPARATOR;
                        
                    prefVal += item;
                }

                if(prefVal != null)
                    SaveString(SYSTEM_OPTION_DNS_ALTERNATIVE, prefVal);
            }
        }

        public bool SystemProxyEnable
        {
            get
            {
                return GetBool(SYSTEM_OPTION_PROXY_ENABLE, SYSTEM_OPTION_PROXY_ENABLE_DEFAULT);
            }

            set
            {
                SaveBool(SYSTEM_OPTION_PROXY_ENABLE, value);
            }
        }       

        public bool SystemShowNotification
        {
            get
            {
                return GetBool(SYSTEM_OPTION_SHOW_NOTIFICATION, SYSTEM_OPTION_SHOW_NOTIFICATION_DEFAULT);
            }

            set
            {
                SaveBool(SYSTEM_OPTION_SHOW_NOTIFICATION, value);
            }
        }

        public string SystemCustomMTU
        {
            get
            {
                return GetString(SYSTEM_CUSTOM_MTU, SYSTEM_CUSTOM_MTU_DEFAULT);
            }

            set
            {
                SaveString(SYSTEM_CUSTOM_MTU, value);
            }
        }

        public long SystemCustomMTUValue
        {
            get
            {
                long defVal = 0;
                
                try
                {
                    defVal = long.Parse(SYSTEM_CUSTOM_MTU_DEFAULT);
                }
                catch(System.FormatException)
                {
                    defVal = 0;
                }

                return GetLong(SYSTEM_CUSTOM_MTU, defVal);
            }

            set
            {
                string prefVal = "";

                if(value <= 0)
                    prefVal = SYSTEM_CUSTOM_MTU_DEFAULT;
                else
                    prefVal = value.ToString();

                SaveString(SYSTEM_CUSTOM_MTU, prefVal);
            }
        }

        public string SystemApplicationFilterType
        {
            get
            {
                return GetString(SYSTEM_OPTION_APPLICATION_FILTER_TYPE, SYSTEM_OPTION_APPLICATION_FILTER_TYPE_DEFAULT);
            }

            set
            {
                SaveString(SYSTEM_OPTION_APPLICATION_FILTER_TYPE, value);
            }
        }

        public string SystemApplicationFilter
        {
            get
            {
                return GetString(SYSTEM_OPTION_APPLICATION_FILTER, SYSTEM_OPTION_APPLICATION_FILTER_DEFAULT);
            }

            set
            {
                SaveString(SYSTEM_OPTION_APPLICATION_FILTER, value);
            }
        }

        public List<string> SystemApplicationFilterList
        {
            get
            {
                List<string> list = new List<string>();
                string[] valArray = null;
                string prefVal = "";

                prefVal = GetString(SYSTEM_OPTION_APPLICATION_FILTER, SYSTEM_OPTION_APPLICATION_FILTER_DEFAULT);

                valArray = prefVal.Split(DEFAULT_SPLIT_SEPARATOR);

                foreach(string item in valArray)
                    list.Add(item);

                return list;
            }
            
            set
            {
                string prefVal = "";
                
                foreach(string item in value)
                {
                    if(!prefVal.Equals(""))
                        prefVal += DEFAULT_SPLIT_SEPARATOR;
                        
                    prefVal += item;
                }

                if(prefVal != null)
                    SaveString(SYSTEM_OPTION_APPLICATION_FILTER, prefVal);
            }
        }

        public bool SystemFirstRun
        {
            get
            {
                return GetBool(SYSTEM_OPTION_FIRST_RUN, SYSTEM_OPTION_FIRST_RUN_DEFAULT);
            }

            set
            {
                SaveBool(SYSTEM_OPTION_FIRST_RUN, value);
            }
        }       

        public bool SystemRestoreLastProfile
        {
            get
            {
                return GetBool(SYSTEM_OPTION_RESTORE_LAST_PROFILE, SYSTEM_OPTION_RESTORE_LAST_PROFILE_DEFAULT);
            }

            set
            {
                SaveBool(SYSTEM_OPTION_RESTORE_LAST_PROFILE, value);
            }
        }

        public bool SystemLastProfileIsConnected
        {
            get
            {
                return GetBool(SYSTEM_OPTION_LAST_PROFILE_IS_CONNECTED, SYSTEM_OPTION_LAST_PROFILE_DEFAULT_IS_CONNECTED);
            }

            set
            {
                SaveBool(SYSTEM_OPTION_LAST_PROFILE_IS_CONNECTED, value);
            }
        }

        public string SystemLastProfile
        {
            get
            {
                return Encoding.UTF8.GetString(System.Convert.FromBase64String(GetString(SYSTEM_OPTION_LAST_PROFILE, "")));
            }

            set
            {
                if(value != null)
                    SaveString(SYSTEM_OPTION_LAST_PROFILE, System.Convert.ToBase64String(Encoding.UTF8.GetBytes(value)));
            }
        }

        public Dictionary<string, string> SystemLastProfileInfo
        {
            get
            {
                Dictionary<string, string> pData = new Dictionary<string, string>();
                string info = "";
                string[] items = null, entry = null;

                info = Encoding.UTF8.GetString(System.Convert.FromBase64String(GetString(SYSTEM_OPTION_LAST_PROFILE_INFO, "")));
                
                if(!info.Equals(""))
                {
                    items = info.Split('|');

                    foreach(string item in items)
                    {
                        entry = item.Split(':');
                        
                        pData.Add(entry[0], entry[1]);
                    }
                }
                
                return pData;
            }

            set
            {
                string info = "";
                
                foreach(KeyValuePair<string, string> item in value)
                {
                    if(!info.Equals(""))
                        info += "|";
                        
                    info += item.Key + ":" + item.Value;
                }

                if(info != null)
                    SaveString(SYSTEM_OPTION_LAST_PROFILE_INFO, System.Convert.ToBase64String(Encoding.UTF8.GetBytes(info)));
            }
        }

        public bool SystemPauseVpnWhenScreenIsOff
        {
            get
            {
                return GetBool(SYSTEM_OPTION_PAUSE_VPN_WHEN_SCREEN_IS_OFF, SYSTEM_OPTION_PAUSE_VPN_WHEN_SCREEN_IS_OFF_DEFAULT);
            }

            set
            {
                SaveBool(SYSTEM_OPTION_PAUSE_VPN_WHEN_SCREEN_IS_OFF, value);
            }
       }

        public string GetString(string key, string defValue)
        {
            return appPrefs.GetString(key, defValue);
        }
    
        public void SaveString(string key, string value)
        {
            prefsEditor.PutString(key, value);
    
            prefsEditor.Apply();
        }

        public long GetLong(string key, long defValue)
        {
            long returnValue = 0;
            string value = "";
    
            value = appPrefs.GetString(key, "");
    
            if(value.Equals(""))
                returnValue = defValue;
            else
            {
                try
                {
                    returnValue = long.Parse(value);
                }
                catch(System.FormatException)
                {
                    returnValue = 0;
                }
            }
    
            return returnValue;
        }
    
        public void SaveLong(string key, long value)
        {
            prefsEditor.PutString(key, value.ToString());
    
            prefsEditor.Apply();
        }

        public bool GetBool(string key, bool defValue)
        {
            bool returnValue = false;
            string value = "";
    
            value = appPrefs.GetString(key, "");
    
            if(value.Equals(""))
                returnValue = defValue;
            else if(value.Equals("true"))
                returnValue = true;
            else
                returnValue = false;
    
            return returnValue;
        }
    
        public void SaveBool(string key, bool value)
        {
            string val = "";

            if(value)
                val = "true";
            else
                val = "false";
    
            prefsEditor.PutString(key, val);
    
            prefsEditor.Apply();
        }
    }
}
