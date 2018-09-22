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
// 20 June 2018 - author: promind - initial release. (a tribute to the 1859 Perugia uprising occurred on 20 June 1859 and in memory of those brave inhabitants who fought for the liberty of Perugia)

using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using Android.Provider;

namespace Eddie.NativeAndroidApp
{
    [Activity(Label = "Settings Activity")]
    public class SettingsActivity : Activity
    {
        private enum SettingEditType
        {
            TEXT = 1,
            PASSWORD,
            NUMERIC,
            IP_PORT,
            IP_ADDRESS_LIST
        };

        private enum SettingEditOptions
        {
            ALLOW_EMPTY_FIELD = 1,
            ALLOW_ZERO_VALUE,
            DO_NOT_ALLOW_EMPTY_FIELD,
            DO_NOT_ALLOW_ZERO_VALUE
        };

        private const int ACTIVITY_RESULT_PACKAGE_CHOOSER = 1000;

        private SupportTools supportTools = null;
        private SettingsManager settingsManager = new SettingsManager();

        private LinearLayout llVpnMinimumTLSVersion = null;
        private LinearLayout llVpnTransportProtocol = null;
        private LinearLayout llVpnIPV6 = null;
        private LinearLayout llVpnTimeout = null;
        private LinearLayout llVpnTunPersist = null;
        private LinearLayout llVpnCompressionMode = null;
        private LinearLayout llDnsOverride = null;
        private LinearLayout llDnsOverrideSettings = null;
        private LinearLayout llDnsCustom = null;
        private LinearLayout llDnsAlternative = null;
        private LinearLayout llVpnUsername = null;
        private LinearLayout llVpnPassword = null;
        private LinearLayout llPauseVpnWhenScreenIsOff = null;
        private LinearLayout llPersistentNotification = null;
        private LinearLayout llNotificationSound = null;
        private LinearLayout llNotificationChannel = null;
        private LinearLayout llRestoreLastProfile = null;
        private LinearLayout llApplicationFilterType = null;
        private LinearLayout llApplicationFilter = null;
        private LinearLayout llProxyEnable = null;
        private LinearLayout llProxySettings = null;
        private LinearLayout llProxyHost = null;
        private LinearLayout llProxyPort = null;
        private LinearLayout llProxyUsername = null;
        private LinearLayout llProxyPassword = null;
        private LinearLayout llProxyAllowClearTextAuth = null;
        private LinearLayout llSynchronousDnsLookup = null;
        private LinearLayout llCustomMtu = null;
        private LinearLayout llAutologinSessions = null;
        private LinearLayout llDisableClientCert = null;
        private LinearLayout llSslDebugLevel = null;
        private LinearLayout llPrivateKeyPassword = null;
        private LinearLayout llDefaultKeyDirection = null;
        private LinearLayout llForceAesCbcCiphers = null;
        private LinearLayout llTlsCertProfile = null;
        private LinearLayout llCustomDirectives = null;

        private Switch swVpnTunPersist = null;
        private Switch swDnsOverride = null;
        private Switch swPauseVpnWhenScreeIsOff = null;
        private Switch swPersistentNotification = null;
        private Switch swNotificationSound = null;
        private Switch swRestoreLastProfile = null;
        private Switch swProxyEnable = null;
        private Switch swProxyAllowClearTextAuth = null;
        private Switch swSynchronousDnsLookup = null;
        private Switch swAutologinSessions = null;
        private Switch swDisableClientCert = null;
        private Switch swForceAesCbcCiphers = null;

        private Button btnResetOptions = null;

        private TextView txtApplicationFilterTitle = null;

        AlertDialog.Builder dialogBuilder = null;
        AlertDialog settingDialog = null;

        private string dialogReturnStringValue = "";
        private Handler dialogHandler = null;

        private Button btnOk = null;
        private Button btnCancel = null;

        private EditText edtKey = null;

        private Result settingsResult = Result.Canceled;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            supportTools = new SupportTools(this);

            SetContentView(Resource.Layout.settings_activity_layout);
            
            llVpnMinimumTLSVersion = FindViewById<LinearLayout>(Resource.Id.setting_ovpn3_tls_min_version);
            llVpnTransportProtocol = FindViewById<LinearLayout>(Resource.Id.setting_ovpn3_protocol);
            llVpnIPV6 = FindViewById<LinearLayout>(Resource.Id.setting_ovpn3_ipv6);
            llVpnTimeout = FindViewById<LinearLayout>(Resource.Id.setting_ovpn3_timeout);
            llVpnTunPersist = FindViewById<LinearLayout>(Resource.Id.setting_ovpn3_tun_persist);
            llVpnCompressionMode = FindViewById<LinearLayout>(Resource.Id.setting_ovpn3_compression_mode);
            llDnsOverride = FindViewById<LinearLayout>(Resource.Id.setting_dns_override);
            llDnsOverrideSettings = FindViewById<LinearLayout>(Resource.Id.dns_override_layout);
            llDnsCustom = FindViewById<LinearLayout>(Resource.Id.setting_dns_custom);
            llDnsAlternative = FindViewById<LinearLayout>(Resource.Id.setting_alternative_dns);
            llVpnUsername = FindViewById<LinearLayout>(Resource.Id.setting_ovpn3_username);
            llVpnPassword = FindViewById<LinearLayout>(Resource.Id.setting_ovpn3_password);
            llPauseVpnWhenScreenIsOff = FindViewById<LinearLayout>(Resource.Id.setting_pause_vpn_when_screen_is_off);
            llPersistentNotification = FindViewById<LinearLayout>(Resource.Id.setting_persistent_notification);
            llNotificationSound = FindViewById<LinearLayout>(Resource.Id.setting_notification_sound);
            llNotificationChannel = FindViewById<LinearLayout>(Resource.Id.setting_notification_channel);
            llRestoreLastProfile = FindViewById<LinearLayout>(Resource.Id.setting_restore_last_profile);
            llApplicationFilterType = FindViewById<LinearLayout>(Resource.Id.setting_application_filter_type);
            llApplicationFilter = FindViewById<LinearLayout>(Resource.Id.setting_application_filter);
            llProxyEnable = FindViewById<LinearLayout>(Resource.Id.setting_proxy_enable);
            llProxySettings = FindViewById<LinearLayout>(Resource.Id.proxy_settings_layout);
            llProxyHost = FindViewById<LinearLayout>(Resource.Id.setting_proxy_host);
            llProxyPort = FindViewById<LinearLayout>(Resource.Id.setting_proxy_port);
            llProxyUsername = FindViewById<LinearLayout>(Resource.Id.setting_proxy_username);
            llProxyPassword = FindViewById<LinearLayout>(Resource.Id.setting_proxy_password);
            llProxyAllowClearTextAuth = FindViewById<LinearLayout>(Resource.Id.setting_proxy_allow_cleartext_auth);
            llSynchronousDnsLookup = FindViewById<LinearLayout>(Resource.Id.setting_synchronous_dns_lookup);
            llCustomMtu = FindViewById<LinearLayout>(Resource.Id.setting_custom_mtu);
            llAutologinSessions = FindViewById<LinearLayout>(Resource.Id.setting_autologin_sessions);
            llDisableClientCert = FindViewById<LinearLayout>(Resource.Id.setting_disable_client_cert);
            llSslDebugLevel = FindViewById<LinearLayout>(Resource.Id.setting_ssl_debug_level);
            llPrivateKeyPassword = FindViewById<LinearLayout>(Resource.Id.setting_private_key_password);
            llDefaultKeyDirection = FindViewById<LinearLayout>(Resource.Id.setting_default_key_direction);
            llForceAesCbcCiphers = FindViewById<LinearLayout>(Resource.Id.setting_force_aes_cbc_ciphers);
            llTlsCertProfile = FindViewById<LinearLayout>(Resource.Id.setting_tls_cert_profile);
            llCustomDirectives = FindViewById<LinearLayout>(Resource.Id.setting_custom_directives);

            swVpnTunPersist = FindViewById<Switch>(Resource.Id.switch_ovpn3_tun_persist);
            swDnsOverride = FindViewById<Switch>(Resource.Id.switch_dns_override);
            swPauseVpnWhenScreeIsOff = FindViewById<Switch>(Resource.Id.switch_pause_vpn_when_screen_is_off);
            swPersistentNotification = FindViewById<Switch>(Resource.Id.switch_persistent_notification);
            swNotificationSound = FindViewById<Switch>(Resource.Id.switch_notification_sound);
            swRestoreLastProfile = FindViewById<Switch>(Resource.Id.switch_restore_last_profile);
            swProxyEnable = FindViewById<Switch>(Resource.Id.switch_proxy_enable);
            swProxyAllowClearTextAuth = FindViewById<Switch>(Resource.Id.switch_proxy_allow_cleartext_auth);
            swSynchronousDnsLookup = FindViewById<Switch>(Resource.Id.switch_synchronous_dns_lookup);
            swAutologinSessions = FindViewById<Switch>(Resource.Id.switch_autologin_sessions);
            swDisableClientCert = FindViewById<Switch>(Resource.Id.switch_disable_client_cert);
            swForceAesCbcCiphers = FindViewById<Switch>(Resource.Id.switch_force_aes_cbc_ciphers);

            btnResetOptions = FindViewById<Button>(Resource.Id.btn_reset_settings);

            txtApplicationFilterTitle = FindViewById<TextView>(Resource.Id.settings_application_filter_title);

            if(Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.O)
            {
                llNotificationSound.Visibility = ViewStates.Visible;

                llNotificationChannel.Visibility = ViewStates.Gone;
            }
            else
            {
                string channelId = Resources.GetString(Resource.String.notification_channel_id);
                NotificationManager notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
                NotificationChannel notificationChannel = notificationManager.GetNotificationChannel(channelId);

                llNotificationSound.Visibility = ViewStates.Gone;

                if(notificationChannel != null)
                    llNotificationChannel.Visibility = ViewStates.Visible;
                else
                    llNotificationChannel.Visibility = ViewStates.Gone;
            }

            llVpnMinimumTLSVersion.Click += delegate
            {
                SelectVpnMinimumTLSVersion();
            };
            
            llVpnTransportProtocol.Click += delegate
            {
                SelectVpnTransportProtocol();
            };
            
            llVpnIPV6.Click += delegate
            {
                SelectVpnIPV6();
            };
            
            llVpnTimeout.Click += delegate
            {
                SelectVpnTimeout();
            };
            
            llVpnTunPersist.Click += delegate
            {
                SelectVpnTunPersist();
            };
            
            llVpnCompressionMode.Click += delegate
            {
                SelectVpnCompressionMode();
            };

            llDnsOverride.Click += delegate
            {
                SelectDnsOverrideSettings();
            };
            
            llDnsCustom.Click += delegate
            {
                SelectDnsCustom();
            };

            llDnsAlternative.Click += delegate
            {
                SelectDnsAlternative();
            };

            llVpnUsername.Click += delegate
            {
                SelectVpnUsername();
            };

            llVpnPassword.Click += delegate
            {
                SelectVpnPassword();
            };

            llPauseVpnWhenScreenIsOff.Click += delegate
            {
                SelectPauseVpnWhenScreenIsOff();
            };
            
            llPersistentNotification.Click += delegate
            {
                SelectPersistentNotification();
            };

            llNotificationSound.Click += delegate
            {
                SelectNotificationSound();
            };

            llNotificationChannel.Click += delegate
            {
                SelectNotificationChannel();
            };
            
            llRestoreLastProfile.Click += delegate
            {
                SelectRestoreLastProfile();
            };
            
            llApplicationFilterType.Click += delegate
            {
                SelectApplicationFilterType();
            };

            llApplicationFilter.Click += delegate
            {
                SelectApplicationFilter();
            };

            llProxyEnable.Click += delegate
            {
                SelectProxyEnable();
            };
            
            llProxyHost.Click += delegate
            {
                SelectProxyHost();
            };

            llProxyPort.Click += delegate
            {
                SelectProxyPort();
            };

            llProxyUsername.Click += delegate
            {
                SelectProxyUsername();
            };

            llProxyPassword.Click += delegate
            {
                SelectProxyPassword();
            };

            llProxyAllowClearTextAuth.Click += delegate
            {
                SelectProxyAllowClearTextAuth();
            };
            
            llSynchronousDnsLookup.Click += delegate
            {
                SelectSynchronousDnsLookup();
            };
            
            llCustomMtu.Click += delegate
            {
                SelectCustomMtu();
            };

            llAutologinSessions.Click += delegate
            {
                SelectAutologinSessions();
            };

            llDisableClientCert.Click += delegate
            {
                SelectDisableClientCert();
            };

            llSslDebugLevel.Click += delegate
            {
                SelectSslDebugLevel();
            };

            llPrivateKeyPassword.Click += delegate
            {
                SelectPrivateKeyPassword();
            };

            llDefaultKeyDirection.Click += delegate
            {
                SelectDefaultKeyDirection();
            };

            llForceAesCbcCiphers.Click += delegate
            {
                SelectForceAesCbcCiphers();
            };

            llTlsCertProfile.Click += delegate
            {
                SelectTlsCertProfile();
            };

            llCustomDirectives.Click += delegate
            {
                SelectCustomDirectives();
            };
            
            btnResetOptions.Click += delegate
            {
                ResetToDefaultOptions();
            };
            
            SetupSettingControls();
        }

        public override void OnBackPressed()
        {
            SetResult(settingsResult, null);

            Finish();
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            switch(requestCode)
            {
                case ACTIVITY_RESULT_PACKAGE_CHOOSER:
                {
                    if(data != null)
                    {
                        settingsManager.SystemApplicationFilter = data.GetStringExtra(PackageChooserActivity.PARAM_PACKAGES);

                        settingsResult = Result.Ok;
                    }
                }
                break;
            }
        }

        private void SetupSettingControls()
        {
            swVpnTunPersist.Checked = settingsManager.Ovpn3TunPersist;
            swDnsOverride.Checked = settingsManager.SystemDNSOverrideEnable;
            swPauseVpnWhenScreeIsOff.Checked = settingsManager.SystemPauseVpnWhenScreenIsOff;
            swPersistentNotification.Checked = settingsManager.SystemPersistentNotification;
            swNotificationSound.Checked = settingsManager.SystemNotificationSound;
            swRestoreLastProfile.Checked = settingsManager.SystemRestoreLastProfile;
            swProxyEnable.Checked = settingsManager.SystemProxyEnable;
            swProxyAllowClearTextAuth.Checked = settingsManager.Ovpn3ProxyAllowCleartextAuth;
            swSynchronousDnsLookup.Checked = settingsManager.Ovpn3SynchronousDNSLookup;
            swAutologinSessions.Checked = settingsManager.Ovpn3AutologinSessions;
            swDisableClientCert.Checked = settingsManager.Ovpn3DisableClientCert;
            swForceAesCbcCiphers.Checked = settingsManager.Ovpn3ForceAESCBCCiphersuites;

            if(settingsManager.SystemDNSOverrideEnable)
                llDnsOverrideSettings.Visibility = ViewStates.Visible;
            else
                llDnsOverrideSettings.Visibility = ViewStates.Gone;

            if(settingsManager.SystemProxyEnable)
                llProxySettings.Visibility = ViewStates.Visible;
            else
                llProxySettings.Visibility = ViewStates.Gone;

            if(settingsManager.SystemApplicationFilterType == SettingsManager.SYSTEM_OPTION_APPLICATION_FILTER_TYPE_NONE)
                llApplicationFilter.Visibility = ViewStates.Gone;
            else
                llApplicationFilter.Visibility = ViewStates.Visible;
                
            if(settingsManager.SystemApplicationFilterType == SettingsManager.SYSTEM_OPTION_APPLICATION_FILTER_TYPE_WHITELIST)
                txtApplicationFilterTitle.Text = Resources.GetString(Resource.String.settings_system_application_whitelist_title);
            else
                txtApplicationFilterTitle.Text = Resources.GetString(Resource.String.settings_system_application_blacklist_title);
        }

        private void SelectVpnMinimumTLSVersion()
        {
            string value = "";

            value = GetOptionFromListDialog(Resource.String.settings_ovpn3_tls_min_version_title,
                                            Resource.Array.settings_ovpn3_tls_min_version_labels,
                                            Resource.Array.settings_ovpn3_tls_min_version_values,
                                            settingsManager.Ovpn3TLSMinVersion);

            if(value != settingsManager.Ovpn3TLSMinVersion)
            {
                settingsManager.Ovpn3TLSMinVersion = value;
                
                settingsResult = Result.Ok;
            }
        }

        private void SelectVpnTransportProtocol()
        {
            string value = "";

            value = GetOptionFromListDialog(Resource.String.settings_ovpn3_protocol_title,
                                            Resource.Array.settings_ovpn3_protocol_labels,
                                            Resource.Array.settings_ovpn3_protocol_values,
                                            settingsManager.Ovpn3Protocol);

            if(value != settingsManager.Ovpn3Protocol)
            {
                settingsManager.Ovpn3Protocol = value;
                
                settingsResult = Result.Ok;
            }
        }

        private void SelectVpnIPV6()
        {
            string value = "";

            value = GetOptionFromListDialog(Resource.String.settings_ovpn3_ipv6_title,
                                            Resource.Array.settings_ovpn3_ipv6_labels,
                                            Resource.Array.settings_ovpn3_ipv6_values,
                                            settingsManager.Ovpn3IPV6);

            if(value != settingsManager.Ovpn3IPV6)
            {
                settingsManager.Ovpn3IPV6 = value;
                
                settingsResult = Result.Ok;
            }
        }

        private void SelectVpnTimeout()
        {
            string value = "";

            value = GetOptionFromListDialog(Resource.String.settings_ovpn3_timeout_title,
                                            Resource.Array.settings_ovpn3_timeout_labels,
                                            Resource.Array.settings_ovpn3_timeout_values,
                                            settingsManager.Ovpn3Timeout);

            if(value != settingsManager.Ovpn3Timeout)
            {
                settingsManager.Ovpn3Timeout = value;
                
                settingsResult = Result.Ok;
            }
        }

        private void SelectVpnTunPersist()
        {
            settingsManager.Ovpn3TunPersist = !settingsManager.Ovpn3TunPersist;
            
            swVpnTunPersist.Checked = settingsManager.Ovpn3TunPersist;
            
            if(!settingsManager.Ovpn3TunPersist)
                supportTools.InfoDialog(Resource.String.settings_tun_persist_warning);

            settingsResult = Result.Ok;
        }

        private void SelectVpnCompressionMode()
        {
            string value = "";

            value = GetOptionFromListDialog(Resource.String.settings_ovpn3_compression_mode_title,
                                            Resource.Array.settings_ovpn3_compression_mode_labels,
                                            Resource.Array.settings_ovpn3_compression_mode_values,
                                            settingsManager.Ovpn3CompressionMode);

            if(value != settingsManager.Ovpn3CompressionMode)
            {
                settingsManager.Ovpn3CompressionMode = value;
                
                settingsResult = Result.Ok;
            }
        }

        private void SelectDnsOverrideSettings()
        {
            settingsManager.SystemDNSOverrideEnable = !settingsManager.SystemDNSOverrideEnable;
            
            swDnsOverride.Checked = settingsManager.SystemDNSOverrideEnable;

            if(settingsManager.SystemDNSOverrideEnable)
                llDnsOverrideSettings.Visibility = ViewStates.Visible;
            else
                llDnsOverrideSettings.Visibility = ViewStates.Gone;

            settingsResult = Result.Ok;
        }

        private void SelectDnsCustom()
        {
            string value = "";

            value = GetIpAddressOptionDialog(Resource.String.settings_system_dns_custom_title,
                                             settingsManager.SystemDNSCustom,
                                             SettingEditOptions.ALLOW_EMPTY_FIELD);

            if(value != settingsManager.SystemDNSCustom)
            {
                settingsManager.SystemDNSCustom = value;
                
                settingsResult = Result.Ok;
            }
        }

        private void SelectDnsAlternative()
        {
            string value = "";

            value = GetIpAddressOptionDialog(Resource.String.settings_system_dns_alternative_title,
                                             settingsManager.SystemDNSAlternative,
                                             SettingEditOptions.ALLOW_EMPTY_FIELD);

            if(value != settingsManager.SystemDNSAlternative)
            {
                settingsManager.SystemDNSAlternative = value;
                
                settingsResult = Result.Ok;
            }
        }

        private void SelectVpnUsername()
        {
            string value = "";

            value = GetTextOptionDialog(Resource.String.settings_ovpn3_username_title,
                                        settingsManager.Ovpn3Username,
                                        SettingEditOptions.ALLOW_EMPTY_FIELD);

            if(value != settingsManager.Ovpn3Username)
            {
                settingsManager.Ovpn3Username = value;
                
                settingsResult = Result.Ok;
            }
        }

        private void SelectVpnPassword()
        {
            string value = "";

            value = GetPasswordOptionDialog(Resource.String.settings_ovpn3_password_title,
                                            settingsManager.Ovpn3Password,
                                            SettingEditOptions.ALLOW_EMPTY_FIELD);

            if(value != settingsManager.Ovpn3Password)
            {
                settingsManager.Ovpn3Password = value;
                
                settingsResult = Result.Ok;
            }
        }

        private void SelectPauseVpnWhenScreenIsOff()
        {
            settingsManager.SystemPauseVpnWhenScreenIsOff = !settingsManager.SystemPauseVpnWhenScreenIsOff;
            
            swPauseVpnWhenScreeIsOff.Checked = settingsManager.SystemPauseVpnWhenScreenIsOff;
            
            if(settingsManager.SystemPauseVpnWhenScreenIsOff)
                supportTools.InfoDialog(Resource.String.settings_pause_vpn_when_screen_is_off_warning);

            settingsResult = Result.Ok;
        }

        private void SelectPersistentNotification()
        {
            settingsManager.SystemPersistentNotification = !settingsManager.SystemPersistentNotification;
            
            swPersistentNotification.Checked = settingsManager.SystemPersistentNotification;
            
            if(!settingsManager.SystemPersistentNotification)
                supportTools.InfoDialog(Resource.String.settings_persistent_notification_warning);
            
            settingsResult = Result.Ok;
        }

        private void SelectNotificationSound()
        {
            settingsManager.SystemNotificationSound = !settingsManager.SystemNotificationSound;
            
            swNotificationSound.Checked = settingsManager.SystemNotificationSound;
        }

        private void SelectNotificationChannel()
        {
            string channelId = Resources.GetString(Resource.String.notification_channel_id);

            Intent channelSettingsIntent = new Intent(Settings.ActionChannelNotificationSettings);
            
            if(channelSettingsIntent != null)
            {
                channelSettingsIntent.PutExtra(Settings.ExtraAppPackage, PackageName);
                channelSettingsIntent.PutExtra(Settings.ExtraChannelId, channelId);
                channelSettingsIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
    
                StartActivity(channelSettingsIntent);
            }
        }

        private void SelectRestoreLastProfile()
        {
            settingsManager.SystemRestoreLastProfile = !settingsManager.SystemRestoreLastProfile;
            
            swRestoreLastProfile.Checked = settingsManager.SystemRestoreLastProfile;
            
            settingsResult = Result.Ok;
        }

        private void SelectApplicationFilterType()
        {
            string value = "";

            value = GetOptionFromListDialog(Resource.String.settings_system_application_filter_type_title,
                                            Resource.Array.settings_system_application_filter_type_labels,
                                            Resource.Array.settings_system_application_filter_type_values,
                                            settingsManager.SystemApplicationFilterType);

            if(value != settingsManager.SystemApplicationFilterType)
            {
                if(settingsManager.SystemApplicationFilter != "")
                {
                    if(supportTools.ConfirmationDialog(Resource.String.settings_application_filter_type_change_warning) == false)
                        return;
                }

                settingsManager.SystemApplicationFilterType = value;

                if(settingsManager.SystemApplicationFilterType == SettingsManager.SYSTEM_OPTION_APPLICATION_FILTER_TYPE_NONE)
                    llApplicationFilter.Visibility = ViewStates.Gone;
                else
                {
                    llApplicationFilter.Visibility = ViewStates.Visible;

                    if(settingsManager.SystemApplicationFilterType == SettingsManager.SYSTEM_OPTION_APPLICATION_FILTER_TYPE_WHITELIST)
                        txtApplicationFilterTitle.Text = Resources.GetString(Resource.String.settings_system_application_whitelist_title);
                    else
                        txtApplicationFilterTitle.Text = Resources.GetString(Resource.String.settings_system_application_blacklist_title);
                }

                settingsManager.SystemApplicationFilter = "";    

                settingsResult = Result.Ok;
            }
        }

        private void SelectApplicationFilter()
        {
            string title = "";

            Intent packageChooserIntent = new Intent(this, typeof(PackageChooserActivity));

            packageChooserIntent.PutExtra(PackageChooserActivity.PARAM_PACKAGES, settingsManager.SystemApplicationFilter);

            if(settingsManager.SystemApplicationFilterType == SettingsManager.SYSTEM_OPTION_APPLICATION_FILTER_TYPE_WHITELIST)
                title = Resources.GetString(Resource.String.settings_application_whitelist_title);
            else
                title = Resources.GetString(Resource.String.settings_application_blacklist_title);

            packageChooserIntent.PutExtra(PackageChooserActivity.CHOOSER_TITLE, title);

            StartActivityForResult(packageChooserIntent, ACTIVITY_RESULT_PACKAGE_CHOOSER);
        }

        private void SelectProxyEnable()
        {
            settingsManager.SystemProxyEnable = !settingsManager.SystemProxyEnable;
            
            swProxyEnable.Checked = settingsManager.SystemProxyEnable;

            if(settingsManager.SystemProxyEnable)
                llProxySettings.Visibility = ViewStates.Visible;
            else
                llProxySettings.Visibility = ViewStates.Gone;

            settingsResult = Result.Ok;
        }

        private void SelectProxyHost()
        {
            string value = "";

            value = GetTextOptionDialog(Resource.String.settings_ovpn3_proxy_host_title,
                                        settingsManager.Ovpn3ProxyHost,
                                        SettingEditOptions.ALLOW_EMPTY_FIELD);

            if(value != settingsManager.Ovpn3ProxyHost)
            {
                settingsManager.Ovpn3ProxyHost = value;
                
                settingsResult = Result.Ok;
            }
        }

        private void SelectProxyPort()
        {
            long value = 0;

            value = GetIpPortOptionDialog(Resource.String.settings_ovpn3_proxy_port_title,
                                          settingsManager.Ovpn3ProxyPortValue,
                                          SettingEditOptions.ALLOW_ZERO_VALUE);

            if(value != settingsManager.Ovpn3ProxyPortValue)
            {
                settingsManager.Ovpn3ProxyPortValue = value;
                
                settingsResult = Result.Ok;
            }
        }

        private void SelectProxyUsername()
        {
            string value = "";

            value = GetTextOptionDialog(Resource.String.settings_ovpn3_proxy_username_title,
                                        settingsManager.Ovpn3ProxyUsername,
                                        SettingEditOptions.ALLOW_EMPTY_FIELD);

            if(value != settingsManager.Ovpn3ProxyUsername)
            {
                settingsManager.Ovpn3ProxyUsername = value;
                
                settingsResult = Result.Ok;
            }
        }

        private void SelectProxyPassword()
        {
            string value = "";

            value = GetPasswordOptionDialog(Resource.String.settings_ovpn3_proxy_password_title,
                                            settingsManager.Ovpn3ProxyPassword,
                                            SettingEditOptions.ALLOW_EMPTY_FIELD);

            if(value != settingsManager.Ovpn3ProxyPassword)
            {
                settingsManager.Ovpn3ProxyPassword = value;
                
                settingsResult = Result.Ok;
            }
        }

        private void SelectProxyAllowClearTextAuth()
        {
            settingsManager.Ovpn3ProxyAllowCleartextAuth = !settingsManager.Ovpn3ProxyAllowCleartextAuth;
            
            swProxyAllowClearTextAuth.Checked = settingsManager.Ovpn3ProxyAllowCleartextAuth;

            settingsResult = Result.Ok;
        }

        private void SelectSynchronousDnsLookup()
        {
            settingsManager.Ovpn3SynchronousDNSLookup = !settingsManager.Ovpn3SynchronousDNSLookup;
            
            swSynchronousDnsLookup.Checked = settingsManager.Ovpn3SynchronousDNSLookup;

            settingsResult = Result.Ok;
        }

        private void SelectCustomMtu()
        {
            long value = 0;

            value = GetNumericOptionDialog(Resource.String.settings_system_custom_mtu_title,
                                           settingsManager.SystemCustomMTUValue,
                                           SettingEditOptions.ALLOW_ZERO_VALUE);

            if(value != settingsManager.SystemCustomMTUValue)
            {
                settingsManager.SystemCustomMTUValue = value;
                
                settingsResult = Result.Ok;
            }
        }

        private void SelectAutologinSessions()
        {
            settingsManager.Ovpn3AutologinSessions = !settingsManager.Ovpn3AutologinSessions;
            
            swAutologinSessions.Checked = settingsManager.Ovpn3AutologinSessions;

            settingsResult = Result.Ok;
        }

        private void SelectDisableClientCert()
        {
            settingsManager.Ovpn3DisableClientCert = !settingsManager.Ovpn3DisableClientCert;
            
            swDisableClientCert.Checked = settingsManager.Ovpn3DisableClientCert;

            settingsResult = Result.Ok;
        }

        private void SelectSslDebugLevel()
        {
            long value = 0;

            value = GetNumericOptionDialog(Resource.String.settings_ovpn3_ssl_debug_level_title,
                                           settingsManager.Ovpn3SSLDebugLevelValue,
                                           SettingEditOptions.ALLOW_ZERO_VALUE);

            if(value != settingsManager.Ovpn3SSLDebugLevelValue)
            {
                settingsManager.Ovpn3SSLDebugLevelValue = value;
                
                settingsResult = Result.Ok;
            }
        }

        private void SelectPrivateKeyPassword()
        {
            string value = "";

            value = GetPasswordOptionDialog(Resource.String.settings_ovpn3_private_key_password_title,
                                            settingsManager.Ovpn3PrivateKeyPassword,
                                            SettingEditOptions.ALLOW_EMPTY_FIELD);

            if(value != settingsManager.Ovpn3PrivateKeyPassword)
            {
                settingsManager.Ovpn3PrivateKeyPassword = value;
                
                settingsResult = Result.Ok;
            }
        }

        private void SelectDefaultKeyDirection()
        {
            string value = "";

            value = GetOptionFromListDialog(Resource.String.settings_ovpn3_default_key_direction_title,
                                            Resource.Array.settings_ovpn3_default_key_direction_labels,
                                            Resource.Array.settings_ovpn3_default_key_direction_values,
                                            settingsManager.Ovpn3DefaultKeyDirection);

            if(value != settingsManager.Ovpn3DefaultKeyDirection)
            {
                settingsManager.Ovpn3DefaultKeyDirection = value;
                
                settingsResult = Result.Ok;
            }
        }

        private void SelectForceAesCbcCiphers()
        {
            settingsManager.Ovpn3ForceAESCBCCiphersuites = !settingsManager.Ovpn3ForceAESCBCCiphersuites;
            
            swForceAesCbcCiphers.Checked = settingsManager.Ovpn3ForceAESCBCCiphersuites;

            settingsResult = Result.Ok;
        }

        private void SelectTlsCertProfile()
        {
            string value = "";

            value = GetTextOptionDialog(Resource.String.settings_ovpn3_tls_cert_profile_title,
                                        settingsManager.Ovpn3TLSCertProfile,
                                        SettingEditOptions.ALLOW_EMPTY_FIELD);

            if(value != settingsManager.Ovpn3TLSCertProfile)
            {
                settingsManager.Ovpn3TLSCertProfile = value;
                
                settingsResult = Result.Ok;
            }
        }

        private void SelectCustomDirectives()
        {
            string value = "";

            value = GetTextOptionDialog(Resource.String.settings_ovpn3_custom_directives_title,
                                        settingsManager.Ovpn3CustomDirectives,
                                        SettingEditOptions.ALLOW_EMPTY_FIELD);

            if(value != settingsManager.Ovpn3CustomDirectives)
            {
                settingsManager.Ovpn3CustomDirectives = value;
                
                settingsResult = Result.Ok;
            }
        }

        private string GetOptionFromListDialog(int resTitle, int resLabel, int resValue, string selectedValue)
        {
            string[] labels = Resources.GetStringArray(resLabel);
            string[] values = Resources.GetStringArray(resValue);

            dialogReturnStringValue = selectedValue;

            dialogHandler = new Handler(m => { throw new TimeoutException(); });
            
            dialogBuilder = new AlertDialog.Builder(this);
            
            dialogBuilder.SetTitle(Resources.GetString(resTitle));
            
            int checkedItem = Array.IndexOf(values, selectedValue);

            dialogBuilder.SetSingleChoiceItems(resLabel, checkedItem, (c, ev) =>
            {
                dialogReturnStringValue = values[ev.Which];
            });
            
            dialogBuilder.SetPositiveButton(Resource.String.ok, (c, ev) =>
            {
                dialogHandler.SendMessage(dialogHandler.ObtainMessage());
            });

            dialogBuilder.SetNegativeButton(Resource.String.cancel, (cw, ev) =>
            {
                dialogReturnStringValue = selectedValue;

                dialogHandler.SendMessage(dialogHandler.ObtainMessage());
            });
            
            settingDialog = dialogBuilder.Create();
            
            settingDialog.Show();

            try
            {
                Looper.Loop();
            }
            catch(TimeoutException)
            {
            }

            return dialogReturnStringValue;
        }

        private string GetTextOptionDialog(int resTitle, string selectedValue, SettingEditOptions editOption = SettingEditOptions.DO_NOT_ALLOW_EMPTY_FIELD)
        {
            return EditOptionDialog(resTitle, selectedValue, SettingEditType.TEXT, editOption);
        }

        private string GetPasswordOptionDialog(int resTitle, string selectedValue, SettingEditOptions editOption = SettingEditOptions.DO_NOT_ALLOW_EMPTY_FIELD)
        {
            return EditOptionDialog(resTitle, selectedValue, SettingEditType.PASSWORD, editOption);
        }

        private long GetNumericOptionDialog(int resTitle, long selectedValue, SettingEditOptions editOption = SettingEditOptions.DO_NOT_ALLOW_ZERO_VALUE)
        {
            string value = "";
            long retVal = 0;

            value = EditOptionDialog(resTitle, selectedValue.ToString(), SettingEditType.NUMERIC, editOption);

            try
            {
                retVal = long.Parse(value);
            }
            catch(System.FormatException)
            {
                retVal = 0;
            }
                
            return retVal;
        }

        private string GetIpAddressOptionDialog(int resTitle, string selectedValue, SettingEditOptions editOption = SettingEditOptions.DO_NOT_ALLOW_EMPTY_FIELD)
        {
            return EditOptionDialog(resTitle, selectedValue, SettingEditType.IP_ADDRESS_LIST, editOption);
        }

        private long GetIpPortOptionDialog(int resTitle, long selectedValue, SettingEditOptions editOption = SettingEditOptions.DO_NOT_ALLOW_ZERO_VALUE)
        {
            string value = "";
            long retVal = 0;

            value = EditOptionDialog(resTitle, selectedValue.ToString(), SettingEditType.IP_PORT, editOption);

            try
            {
                retVal = long.Parse(value);
            }
            catch(System.FormatException)
            {
                retVal = 0;
            }
                
            return retVal;
        }

        private string EditOptionDialog(int resTitle, string selectedValue, SettingEditType editType = SettingEditType.TEXT, SettingEditOptions editOption = SettingEditOptions.DO_NOT_ALLOW_EMPTY_FIELD)
        {
            btnOk = null;
            btnCancel = null;

            dialogHandler = new Handler(m => { throw new Java.Lang.RuntimeException(); });

            dialogBuilder = new AlertDialog.Builder(this);
    
            View content = LayoutInflater.From(this).Inflate(Resource.Layout.edit_option_dialog, null);

            edtKey = content.FindViewById<EditText>(Resource.Id.key);

            edtKey.Text = selectedValue;

            if(editType == SettingEditType.NUMERIC || editType == SettingEditType.IP_PORT)
            {
                edtKey.SetRawInputType(InputTypes.ClassNumber | InputTypes.NumberFlagSigned);

                edtKey.Gravity = GravityFlags.Right;
                
                edtKey.SetSelection(edtKey.Text.Length);
            }

            if(editType == SettingEditType.PASSWORD)
            {
                edtKey.InputType = InputTypes.TextVariationPassword | InputTypes.ClassText;
            } 

            btnOk = content.FindViewById<Button>(Resource.Id.btn_ok);
            btnCancel = content.FindViewById<Button>(Resource.Id.btn_cancel);

            btnOk.Enabled = EditFieldOptionIsValid(editType, selectedValue, editOption);

            btnCancel.Enabled = true;

            btnOk.Click += delegate
            {
                int errMsgResource = 0;

                if(EditFieldIsValid(editType, edtKey.Text, editOption) == false)
                {
                    switch(editType)
                    {
                        case SettingEditType.IP_ADDRESS_LIST:
                        {
                            errMsgResource = Resource.String.settings_ip_address_warning;
                        }
                        break;

                        case SettingEditType.IP_PORT:
                        {
                            errMsgResource = Resource.String.settings_ip_port_warning;
                        }
                        break;

                        default:
                        {
                            errMsgResource = Resource.String.settings_value_warning;
                        }
                        break;
                    }

                    supportTools.InfoDialog(errMsgResource);

                    return;
                }

                if(edtKey.Text.Length > 0 || editOption == SettingEditOptions.ALLOW_EMPTY_FIELD || editOption == SettingEditOptions.ALLOW_ZERO_VALUE)
                    dialogReturnStringValue = edtKey.Text;

                settingDialog.Dismiss();

                dialogHandler.SendMessage(dialogHandler.ObtainMessage());
            };

            btnCancel.Click += delegate
            {
                dialogReturnStringValue = selectedValue;

                settingDialog.Dismiss();

                dialogHandler.SendMessage(dialogHandler.ObtainMessage());
            };

            edtKey.TextChanged += (sender, e) =>
            {
                btnOk.Enabled = EditFieldOptionIsValid(editType, e.Text.ToString(), editOption);
            };

            dialogBuilder.SetTitle(Resources.GetString(resTitle));
            dialogBuilder.SetView(content);

            settingDialog = dialogBuilder.Create();
            settingDialog.Show();
            
            try
            {
                Looper.Loop();
            }
            catch(Java.Lang.RuntimeException)
            {
            }

            return dialogReturnStringValue.Trim();
        }
        
        private bool EditFieldOptionIsValid(SettingEditType editType, string value, SettingEditOptions option)
        {
            bool isValid = false;

            switch(editType)
            {
                case SettingEditType.TEXT:
                case SettingEditType.PASSWORD:
                case SettingEditType.IP_ADDRESS_LIST:
                {
                    if(value.Length > 0 || option == SettingEditOptions.ALLOW_EMPTY_FIELD)
                        isValid = true;
                    else
                        isValid = false;
                }
                break;

                case SettingEditType.NUMERIC:
                case SettingEditType.IP_PORT:
                {
                    long tVal = 0;
                    
                    try
                    {
                        tVal = long.Parse(value);
                    }
                    catch(System.FormatException)
                    {
                        tVal = 0;
                    }

                    if(tVal > 0 || option == SettingEditOptions.ALLOW_ZERO_VALUE)
                        isValid = true;
                    else
                        isValid = false;
                }
                break;
            }
            
            return isValid;
        }
        
        private bool EditFieldIsValid(SettingEditType editType, string value, SettingEditOptions editOption = SettingEditOptions.DO_NOT_ALLOW_EMPTY_FIELD)
        {
            bool isValid = false;
            long tVal = 0;

            switch(editType)
            {
                case SettingEditType.TEXT:
                {
                    isValid = true;
                }
                break;

                case SettingEditType.PASSWORD:
                {
                    isValid = true;
                }
                break;

                case SettingEditType.NUMERIC:
                {
                    isValid = true;
                }
                break;

                case SettingEditType.IP_ADDRESS_LIST:
                {
                    string[] octect = null;
                    isValid = true;

                    string[] ipAdrressArray = value.Split(SettingsManager.DEFAULT_SPLIT_SEPARATOR);

                    foreach(string ipAddress in ipAdrressArray)
                    {
                        octect = ipAddress.Split('.');
                        
                        if(octect.Length != 4)
                            isValid = false;
    
                        foreach(string val in octect)
                        {
                            try
                            {
                                tVal = long.Parse(val);
                            }
                            catch(System.FormatException)
                            {
                                tVal = 0;
                                
                                isValid = false;
                            }
                            
                            if(tVal < 0 || tVal > 255)
                                isValid = false;
                        }
                    }
                    
                    if(value.Length == 0 && editOption == SettingEditOptions.ALLOW_EMPTY_FIELD)
                        isValid = true;
                }
                break;
                
                case SettingEditType.IP_PORT:
                {
                    try
                    {
                        tVal = long.Parse(value);
                    }
                    catch(System.FormatException)
                    {
                        tVal = 0;
                        
                        isValid = false;
                    }

                    if(tVal >= 1 && tVal <= 65535)
                        isValid = true;
                    else
                        isValid = false;
                        
                    if(tVal == 0 && editOption == SettingEditOptions.ALLOW_ZERO_VALUE)
                        isValid = true;
                }
                break;

                default:
                {
                    isValid = false;
                }
                break;
            }

            return isValid;
        }

        private void ResetToDefaultOptions()
        {
            if(supportTools.ConfirmationDialog(Resource.String.settings_reset_to_default))
            {
                settingsManager.Ovpn3TLSMinVersion = SettingsManager.OVPN3_OPTION_TLS_MIN_VERSION_DEFAULT;
                settingsManager.Ovpn3Protocol = SettingsManager.OVPN3_OPTION_PROTOCOL_DEFAULT;
                settingsManager.Ovpn3IPV6 = SettingsManager.OVPN3_OPTION_IPV6_DEFAULT;
                settingsManager.Ovpn3Timeout = SettingsManager.OVPN3_OPTION_TIMEOUT_DEFAULT;
                settingsManager.Ovpn3TunPersist = SettingsManager.OVPN3_OPTION_TUN_PERSIST_DEFAULT;
                settingsManager.Ovpn3CompressionMode = SettingsManager.OVPN3_OPTION_COMPRESSION_MODE_DEFAULT;
                settingsManager.Ovpn3Username = SettingsManager.OVPN3_OPTION_USERNAME_DEFAULT;
                settingsManager.Ovpn3Password = SettingsManager.OVPN3_OPTION_PASSWORD_DEFAULT;
                settingsManager.Ovpn3SynchronousDNSLookup = SettingsManager.OVPN3_OPTION_SYNCHRONOUS_DNS_LOOKUP_DEFAULT;
                settingsManager.Ovpn3AutologinSessions = SettingsManager.OVPN3_OPTION_AUTOLOGIN_SESSIONS_DEFAULT;
                settingsManager.Ovpn3DisableClientCert = SettingsManager.OVPN3_OPTION_DISABLE_CLIENT_CERT_DEFAULT;
                settingsManager.Ovpn3SSLDebugLevel = SettingsManager.OVPN3_OPTION_SSL_DEBUG_LEVEL_DEFAULT;
                settingsManager.Ovpn3PrivateKeyPassword = SettingsManager.OVPN3_OPTION_PRIVATE_KEY_PASSWORD_DEFAULT;
                settingsManager.Ovpn3DefaultKeyDirection = SettingsManager.OVPN3_OPTION_DEFAULT_KEY_DIRECTION_DEFAULT;
                settingsManager.Ovpn3ForceAESCBCCiphersuites = SettingsManager.OVPN3_OPTION_FORCE_AES_CBC_CIPHERSUITES_DEFAULT;
                settingsManager.Ovpn3TLSCertProfile = SettingsManager.OVPN3_OPTION_TLS_CERT_PROFILE_DEFAULT;
                settingsManager.Ovpn3ProxyHost = SettingsManager.OVPN3_OPTION_PROXY_HOST_DEFAULT;
                settingsManager.Ovpn3ProxyPort = SettingsManager.OVPN3_OPTION_PROXY_PORT_DEFAULT;
                settingsManager.Ovpn3ProxyUsername = SettingsManager.OVPN3_OPTION_PROXY_USERNAME_DEFAULT;
                settingsManager.Ovpn3ProxyPassword = SettingsManager.OVPN3_OPTION_PROXY_PASSWORD_DEFAULT;
                settingsManager.Ovpn3ProxyAllowCleartextAuth = SettingsManager.OVPN3_OPTION_PROXY_ALLOW_CLEARTEXT_AUTH_DEFAULT;
                settingsManager.Ovpn3CustomDirectives = SettingsManager.OVPN3_OPTION_CUSTOM_DIRECTIVES_DEFAULT;
                settingsManager.SystemDNSOverrideEnable = SettingsManager.SYSTEM_OPTION_DNS_OVERRIDE_ENABLE_DEFAULT;
                settingsManager.SystemDNSCustom = SettingsManager.SYSTEM_OPTION_DNS_CUSTOM_DEFAULT;
                settingsManager.SystemDNSAlternative = SettingsManager.SYSTEM_OPTION_DNS_ALTERNATIVE_DEFAULT;
                settingsManager.SystemProxyEnable = SettingsManager.SYSTEM_OPTION_PROXY_ENABLE_DEFAULT;
                settingsManager.SystemPersistentNotification = SettingsManager.SYSTEM_OPTION_PERSISTENT_NOTIFICATION_DEFAULT;
                settingsManager.SystemNotificationSound = SettingsManager.SYSTEM_OPTION_NOTIFICATION_SOUND_DEFAULT;
                settingsManager.SystemCustomMTU = SettingsManager.SYSTEM_CUSTOM_MTU_DEFAULT;
                settingsManager.SystemApplicationFilterType = SettingsManager.SYSTEM_OPTION_APPLICATION_FILTER_TYPE_DEFAULT;
                settingsManager.SystemApplicationFilter = SettingsManager.SYSTEM_OPTION_APPLICATION_FILTER_DEFAULT;
                settingsManager.SystemRestoreLastProfile = SettingsManager.SYSTEM_OPTION_RESTORE_LAST_PROFILE_DEFAULT;
                settingsManager.SystemPauseVpnWhenScreenIsOff = SettingsManager.SYSTEM_OPTION_PAUSE_VPN_WHEN_SCREEN_IS_OFF_DEFAULT;

                SetupSettingControls();
                
                supportTools.InfoDialog(Resource.String.settings_reset_to_default_done);
            
                settingsResult = Result.Ok;
            }
        }
    }
}
