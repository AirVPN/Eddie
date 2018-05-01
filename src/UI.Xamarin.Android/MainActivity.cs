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
using Android.Content.PM;
using Android.OS;
using Android.Content;

namespace Eddie.Droid
{
	[Activity (Label = "Eddie", Icon = "@drawable/icon", Theme="@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
		private AndroidVPNManager m_vpnManager = null;
						
		protected override void OnCreate(Bundle bundle)
		{
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar; 
				
			base.OnCreate(bundle);

			global::Xamarin.Forms.Forms.Init(this, bundle);

			m_vpnManager = new AndroidVPNManager(this);
			LoadApplication(new Eddie.App(m_vpnManager));				
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			if(m_vpnManager != null)
				m_vpnManager.HandleActivityResult(requestCode, resultCode, data);
		}

		protected override void OnStart()
		{
			base.OnStart();

			if(m_vpnManager != null)
				m_vpnManager.HandleActivityStart();
		}
	
		protected override void OnStop()
		{
			base.OnStop();

			if(m_vpnManager != null)
				m_vpnManager.HandleActivityStop();
		}
	}
}
