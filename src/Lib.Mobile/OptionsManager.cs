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

using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System.Collections.Generic;

namespace Eddie
{
	public static class OptionsManager
	{
		// This param will be binded to:
		// Android: SharedPreferences Name https://developer.android.com/reference/android/content/Context.html#getSharedPreferences(java.lang.String, int)
		// iOS: SuiteName https://developer.xamarin.com/guides/ios/watchos/application-fundamentals/settings/
		public const string OPTIONS_FILE = "options";

		public const string OVPN3_OPTION_TLS_VERSION_MIN = "ovpn3_tls_version_min";
		public const string OVPN3_OPTION_TLS_VERSION_MIN_NATIVE = "tls_version_min";
		public static readonly string OVPN3_OPTION_TLS_VERSION_MIN_DEFAULT = "tls_1_0";

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

		public const string SYSTEM_OPTION_DNS_FORCED = "system_dns_forced";
		public static readonly string SYSTEM_OPTION_DNS_FORCED_DEFAULT = "";

		public const string SYSTEM_OPTION_DNS_ALTERNATIVE = "system_dns_alternative";
		public static readonly string SYSTEM_OPTION_DNS_ALTERNATIVE_DEFAULT = "";

		public const string SYSTEM_OPTION_PROXY_ENABLE = "system_proxy_enable";
		public static readonly bool SYSTEM_OPTION_PROXY_ENABLE_DEFAULT = false;

		public const string SYSTEM_OPTION_SHOW_NOTIFICATION = "system_show_notification";
		public static readonly bool SYSTEM_OPTION_SHOW_NOTIFICATION_DEFAULT = true;

		public const string SYSTEM_MTU_FORCED = "system_mtu_forced";
		public static readonly string SYSTEM_MTU_FORCED_DEFAULT = "";

		public const string SYSTEM_OPTION_APPLICATIONS_FILTER_TYPE = "system_applications_filter_type";
		public static readonly string SYSTEM_OPTION_APPLICATIONS_FILTER_TYPE_NONE = "0";
		public static readonly string SYSTEM_OPTION_APPLICATIONS_FILTER_TYPE_WHITELIST = "1";
		public static readonly string SYSTEM_OPTION_APPLICATIONS_FILTER_TYPE_BLACKLIST = "2";
		public static readonly string SYSTEM_OPTION_APPLICATIONS_FILTER_TYPE_DEFAULT = SYSTEM_OPTION_APPLICATIONS_FILTER_TYPE_NONE;

		public const string SYSTEM_OPTION_APPLICATIONS_FILTER = "system_applications_filter";
		public static readonly string SYSTEM_OPTION_APPLICATIONS_FILTER_DEFAULT = "";

		public const string SYSTEM_OPTION_FIRST_RUN = "system_first_run";
		public static readonly bool SYSTEM_OPTION_FIRST_RUN_DEFAULT = true;

		public const string SYSTEM_OPTION_LAST_PROFILE_ACTIVATED = "system_last_profile_activated";
		public static readonly string SYSTEM_OPTION_LAST_PROFILE_ACTIVATED_DEFAULT = "";

		public const string SYSTEM_OPTION_LAST_PROFILE_RESTORE = "system_last_profile_restore";
		public static readonly bool SYSTEM_OPTION_LAST_PROFILE_DEFAULT_RESTORE = true;

		public const string SYSTEM_OPTION_LAST_PROFILE_FILE = "system_last_profile_file";
		public static readonly string SYSTEM_OPTION_LAST_PROFILE_FILE_DEFAULT = "/sdcard/Download/profile.ovpn";

		public const string SYSTEM_OPTION_BATTERY_SAVER = "system_battery_saver";
		public static readonly bool SYSTEM_OPTION_BATTERY_SAVER_DEFAULT = false;

		private static ISettings AppSettings
		{
			get
			{
				return CrossSettings.Current;
			}
		}

		public static string Ovpn3TLSVersionMin
		{
			get
			{
				return AppSettings.GetValueOrDefault(OVPN3_OPTION_TLS_VERSION_MIN, OVPN3_OPTION_TLS_VERSION_MIN_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(OVPN3_OPTION_TLS_VERSION_MIN, value, OPTIONS_FILE);
			}
		}

		public static string Ovpn3Protocol
		{
			get
			{
				return AppSettings.GetValueOrDefault(OVPN3_OPTION_PROTOCOL, OVPN3_OPTION_PROTOCOL_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(OVPN3_OPTION_PROTOCOL, value, OPTIONS_FILE);
			}
		}

		public static string Ovpn3IPV6
		{
			get
			{
				return AppSettings.GetValueOrDefault(OVPN3_OPTION_IPV6, OVPN3_OPTION_IPV6_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(OVPN3_OPTION_IPV6, value, OPTIONS_FILE);
			}
		}

		public static string Ovpn3Timeout
		{
			get
			{
				return AppSettings.GetValueOrDefault(OVPN3_OPTION_TIMEOUT, OVPN3_OPTION_TIMEOUT_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(OVPN3_OPTION_TIMEOUT, value, OPTIONS_FILE);
			}
		}


		public static bool Ovpn3TunPersist
		{
			get
			{
				return AppSettings.GetValueOrDefault(OVPN3_OPTION_TUN_PERSIST, OVPN3_OPTION_TUN_PERSIST_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(OVPN3_OPTION_TUN_PERSIST, value, OPTIONS_FILE);
			}
		}

		public static string Ovpn3CompressionMode
		{
			get
			{
				return AppSettings.GetValueOrDefault(OVPN3_OPTION_COMPRESSION_MODE, OVPN3_OPTION_COMPRESSION_MODE_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(OVPN3_OPTION_COMPRESSION_MODE, value, OPTIONS_FILE);
			}
		}

		public static string Ovpn3Username
		{
			get
			{
				return AppSettings.GetValueOrDefault(OVPN3_OPTION_USERNAME, OVPN3_OPTION_USERNAME_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(OVPN3_OPTION_USERNAME, value, OPTIONS_FILE);
			}
		}
		
		public static string Ovpn3Password
		{
			get
			{
				return AppSettings.GetValueOrDefault(OVPN3_OPTION_PASSWORD, OVPN3_OPTION_PASSWORD_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(OVPN3_OPTION_PASSWORD, value, OPTIONS_FILE);
			}
		}

		public static bool Ovpn3SynchronousDNSLookup
		{
			get
			{
				return AppSettings.GetValueOrDefault(OVPN3_OPTION_SYNCHRONOUS_DNS_LOOKUP, OVPN3_OPTION_SYNCHRONOUS_DNS_LOOKUP_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(OVPN3_OPTION_SYNCHRONOUS_DNS_LOOKUP, value, OPTIONS_FILE);
			}
		}

		public static bool Ovpn3AutologinSessions
		{
			get
			{
				return AppSettings.GetValueOrDefault(OVPN3_OPTION_AUTOLOGIN_SESSIONS, OVPN3_OPTION_AUTOLOGIN_SESSIONS_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(OVPN3_OPTION_AUTOLOGIN_SESSIONS, value, OPTIONS_FILE);
			}
		}
		
		public static bool Ovpn3DisableClientCert
		{
			get
			{
				return AppSettings.GetValueOrDefault(OVPN3_OPTION_DISABLE_CLIENT_CERT, OVPN3_OPTION_DISABLE_CLIENT_CERT_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(OVPN3_OPTION_DISABLE_CLIENT_CERT, value, OPTIONS_FILE);
			}
		}

		public static string Ovpn3SSLDebugLevel
		{
			get
			{
				return AppSettings.GetValueOrDefault(OVPN3_OPTION_SSL_DEBUG_LEVEL, OVPN3_OPTION_SSL_DEBUG_LEVEL_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(OVPN3_OPTION_SSL_DEBUG_LEVEL, value, OPTIONS_FILE);
			}
		}

		public static string Ovpn3PrivateKeyPassword
		{
			get
			{
				return AppSettings.GetValueOrDefault(OVPN3_OPTION_PRIVATE_KEY_PASSWORD, OVPN3_OPTION_PRIVATE_KEY_PASSWORD_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(OVPN3_OPTION_PRIVATE_KEY_PASSWORD, value, OPTIONS_FILE);
			}
		}

		public static string Ovpn3DefaultKeyDirection
		{
			get
			{
				return AppSettings.GetValueOrDefault(OVPN3_OPTION_DEFAULT_KEY_DIRECTION, OVPN3_OPTION_DEFAULT_KEY_DIRECTION_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(OVPN3_OPTION_DEFAULT_KEY_DIRECTION, value, OPTIONS_FILE);
			}
		}

		public static bool Ovpn3ForceAESCBCCiphersuites
		{
			get
			{
				return AppSettings.GetValueOrDefault(OVPN3_OPTION_FORCE_AES_CBC_CIPHERSUITES, OVPN3_OPTION_FORCE_AES_CBC_CIPHERSUITES_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(OVPN3_OPTION_FORCE_AES_CBC_CIPHERSUITES, value, OPTIONS_FILE);
			}
		}

		public static string Ovpn3TLSCertProfile
		{
			get
			{
				return AppSettings.GetValueOrDefault(OVPN3_OPTION_TLS_CERT_PROFILE, OVPN3_OPTION_TLS_CERT_PROFILE_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(OVPN3_OPTION_TLS_CERT_PROFILE, value, OPTIONS_FILE);
			}
		}

		public static string Ovpn3ProxyHost
		{
			get
			{
				return AppSettings.GetValueOrDefault(OVPN3_OPTION_PROXY_HOST, OVPN3_OPTION_PROXY_HOST_DEFAULT, OPTIONS_FILE);
			}
		
			set
			{
				AppSettings.AddOrUpdateValue(OVPN3_OPTION_PROXY_HOST, value, OPTIONS_FILE);
			}
		}
	
		public static string Ovpn3ProxyPort
		{
			get
			{
				return AppSettings.GetValueOrDefault(OVPN3_OPTION_PROXY_PORT, OVPN3_OPTION_PROXY_PORT_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(OVPN3_OPTION_PROXY_PORT, value, OPTIONS_FILE);
			}
		}
		
		public static string Ovpn3ProxyUsername
		{
			get
			{
				return AppSettings.GetValueOrDefault(OVPN3_OPTION_PROXY_USERNAME, OVPN3_OPTION_PROXY_USERNAME_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(OVPN3_OPTION_PROXY_USERNAME, value, OPTIONS_FILE);
			}
		}

		public static string Ovpn3ProxyPassword
		{
			get
			{
				return AppSettings.GetValueOrDefault(OVPN3_OPTION_PROXY_PASSWORD, OVPN3_OPTION_PROXY_PASSWORD_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(OVPN3_OPTION_PROXY_PASSWORD, value, OPTIONS_FILE);
			}
		}

		public static bool Ovpn3ProxyAllowCleartextAuth
		{
			get
			{
				return AppSettings.GetValueOrDefault(OVPN3_OPTION_PROXY_ALLOW_CLEARTEXT_AUTH, OVPN3_OPTION_PROXY_ALLOW_CLEARTEXT_AUTH_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(OVPN3_OPTION_PROXY_ALLOW_CLEARTEXT_AUTH, value, OPTIONS_FILE);
			}
		}

		public static string Ovpn3CustomDirectives
		{
			get
			{
				return AppSettings.GetValueOrDefault(OVPN3_OPTION_CUSTOM_DIRECTIVES, OVPN3_OPTION_CUSTOM_DIRECTIVES_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(OVPN3_OPTION_CUSTOM_DIRECTIVES, value, OPTIONS_FILE);
			}
		}	

		public static bool SystemDNSOverrideEnable
		{
			get
			{
				return AppSettings.GetValueOrDefault(SYSTEM_OPTION_DNS_OVERRIDE_ENABLE, SYSTEM_OPTION_DNS_OVERRIDE_ENABLE_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(SYSTEM_OPTION_DNS_OVERRIDE_ENABLE, value, OPTIONS_FILE);
			}
		}

		public static string System3DNSForced
		{
			get
			{
				return AppSettings.GetValueOrDefault(SYSTEM_OPTION_DNS_FORCED, SYSTEM_OPTION_DNS_FORCED_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(SYSTEM_OPTION_DNS_FORCED, value, OPTIONS_FILE);
			}
		}

		public static List<string> SystemDNSForcedList
		{
			get
			{
				return Utils.SpliValues(System3DNSForced);
			}
		}

		public static string SystemDNSAlternative
		{
			get
			{
				return AppSettings.GetValueOrDefault(SYSTEM_OPTION_DNS_ALTERNATIVE, SYSTEM_OPTION_DNS_ALTERNATIVE_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(SYSTEM_OPTION_DNS_ALTERNATIVE, value, OPTIONS_FILE);
			}
		}

		public static List<string> SystemDNSAlternativeList
		{
			get
			{
				return Utils.SpliValues(SystemDNSAlternative);
			}
		}

		public static bool SystemProxyEnable
		{
			get
			{
				return AppSettings.GetValueOrDefault(SYSTEM_OPTION_PROXY_ENABLE, SYSTEM_OPTION_PROXY_ENABLE_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(SYSTEM_OPTION_PROXY_ENABLE, value, OPTIONS_FILE);
			}
		}		

		public static bool SystemShowNotification
		{
			get
			{
				return AppSettings.GetValueOrDefault(SYSTEM_OPTION_SHOW_NOTIFICATION, SYSTEM_OPTION_SHOW_NOTIFICATION_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(SYSTEM_OPTION_SHOW_NOTIFICATION, value, OPTIONS_FILE);
			}
		}

		public static string SystemMTUForced
		{
			get
			{
				return AppSettings.GetValueOrDefault(SYSTEM_MTU_FORCED, SYSTEM_MTU_FORCED_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(SYSTEM_MTU_FORCED, value, OPTIONS_FILE);
			}
		}

		public static string SystemApplicationsFilterType
		{
			get
			{
				return AppSettings.GetValueOrDefault(SYSTEM_OPTION_APPLICATIONS_FILTER_TYPE, SYSTEM_OPTION_APPLICATIONS_FILTER_TYPE_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(SYSTEM_OPTION_APPLICATIONS_FILTER_TYPE, value, OPTIONS_FILE);
			}
		}

		public static string SystemApplicationsFilter
		{
			get
			{
				return AppSettings.GetValueOrDefault(SYSTEM_OPTION_APPLICATIONS_FILTER, SYSTEM_OPTION_APPLICATIONS_FILTER_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(SYSTEM_OPTION_APPLICATIONS_FILTER, value, OPTIONS_FILE);
			}
		}

		public static List<string> SystemApplicationsFilterList
		{
			get
			{
				return Utils.SpliValues(SystemApplicationsFilter);
			}
		}

		public static bool SystemFirstRun
		{
			get
			{
				return AppSettings.GetValueOrDefault(SYSTEM_OPTION_FIRST_RUN, SYSTEM_OPTION_FIRST_RUN_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(SYSTEM_OPTION_FIRST_RUN, value, OPTIONS_FILE);
			}
		}		

		public static string SystemLastProfileActivated
		{
			get
			{
				return AppSettings.GetValueOrDefault(SYSTEM_OPTION_LAST_PROFILE_ACTIVATED, SYSTEM_OPTION_LAST_PROFILE_ACTIVATED_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(SYSTEM_OPTION_LAST_PROFILE_ACTIVATED, value, OPTIONS_FILE);
			}
		}

		public static bool SystemLastProfileRestore
		{
			get
			{
				return AppSettings.GetValueOrDefault(SYSTEM_OPTION_LAST_PROFILE_RESTORE, SYSTEM_OPTION_LAST_PROFILE_DEFAULT_RESTORE, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(SYSTEM_OPTION_LAST_PROFILE_RESTORE, value, OPTIONS_FILE);
			}
		}

		public static string SystemLastProfileFile
		{
			get
			{
				return AppSettings.GetValueOrDefault(SYSTEM_OPTION_LAST_PROFILE_FILE, SYSTEM_OPTION_LAST_PROFILE_FILE_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(SYSTEM_OPTION_LAST_PROFILE_FILE, value, OPTIONS_FILE);
			}
		}

		public static bool SystemBatterySaver
		{
			get
			{
				return AppSettings.GetValueOrDefault(SYSTEM_OPTION_BATTERY_SAVER, SYSTEM_OPTION_BATTERY_SAVER_DEFAULT, OPTIONS_FILE);
			}

			set
			{
				AppSettings.AddOrUpdateValue(SYSTEM_OPTION_BATTERY_SAVER, value, OPTIONS_FILE);
			}
		}		
	}
}
