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

using Android.App;
using Android.OS;
using Android.Preferences;
using Android;
using Android.Content;
using System;
using System.Threading;

namespace Eddie.Droid
{
	[Activity(Label = "Options")]
	public class OptionsActivity : PreferenceActivity, ISharedPreferencesOnSharedPreferenceChangeListener
	{
		private ISharedPreferences m_sharedPreferences = null;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			PreferenceManager.SharedPreferencesName = OptionsManager.OPTIONS_FILE;
			
			AddPreferencesFromResource(Resource.Xml.options);
		
			m_sharedPreferences = PreferenceManager.SharedPreferences;
			m_sharedPreferences.RegisterOnSharedPreferenceChangeListener(this);

			InitUI();
			RefreshUI();			
		}

		protected override void OnDestroy()
		{
			// TODO: fixme (actually this causes a crash in Xamarin)
			/*
			if(m_sharedPreferences != null)
			{
				m_sharedPreferences.UnregisterOnSharedPreferenceChangeListener(this);
				m_sharedPreferences = null;
			}
			*/

			base.OnDestroy();
		}
		
		public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
		{
			RefreshUI();

			if(Utils.Equals(key, OptionsManager.SYSTEM_OPTION_SHOW_NOTIFICATION))
			{
				if(!OptionsManager.SystemShowNotification)
					AndroidApplication.ShowPopup(this, "Warning: Android could suddenly stop the VPN service if this option is disabled");				
			}
			else if(Utils.Equals(key, OptionsManager.SYSTEM_OPTION_BATTERY_SAVER))
			{
				if(OptionsManager.SystemBatterySaver)
					AndroidApplication.ShowPopup(this, "Warning: apps running in background could not work as expected when this option is enabled");
			}
		}

		private void InitUI()
		{
			FindPreference(OptionsManager.SYSTEM_OPTION_APPLICATIONS_FILTER).PreferenceClick += OnApplicationsFilterClick;
		}

		private void OnApplicationsFilterClick(object sender, Preference.PreferenceClickEventArgs e)
		{
			Intent intent = new Intent(this, typeof(PackagesPickerActivity));
			intent.PutExtra(PackagesPickerActivity.PARAM_PACKAGES, OptionsManager.SystemApplicationsFilter);
			StartActivity(intent);
		}

		private void RefreshUI()
		{
			bool ProxyEnable = OptionsManager.SystemProxyEnable;			
			FindPreference(OptionsManager.OVPN3_OPTION_PROXY_HOST).Enabled = ProxyEnable;
			FindPreference(OptionsManager.OVPN3_OPTION_PROXY_PORT).Enabled = ProxyEnable;
			FindPreference(OptionsManager.OVPN3_OPTION_PROXY_USERNAME).Enabled = ProxyEnable;
			FindPreference(OptionsManager.OVPN3_OPTION_PROXY_PASSWORD).Enabled = ProxyEnable;
			FindPreference(OptionsManager.OVPN3_OPTION_PROXY_ALLOW_CLEARTEXT_AUTH).Enabled = ProxyEnable;

			bool DNSOverrideEnable = OptionsManager.SystemDNSOverrideEnable;
			FindPreference(OptionsManager.SYSTEM_OPTION_DNS_FORCED).Enabled = DNSOverrideEnable;
			FindPreference(OptionsManager.SYSTEM_OPTION_DNS_ALTERNATIVE).Enabled = DNSOverrideEnable;

			FindPreference(OptionsManager.SYSTEM_OPTION_APPLICATIONS_FILTER).Enabled = OptionsManager.SystemApplicationsFilterType != OptionsManager.SYSTEM_OPTION_APPLICATIONS_FILTER_TYPE_NONE;
		}
	}
}
