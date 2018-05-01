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

using Eddie;
using Eddie.Common.Log;
using Xamarin.Forms;

namespace Eddie
{
	public partial class App : Application
	{
		public NavigationPage NavigationPage { get; private set; }
		public RootPage RootPage { get; private set; }

		public IVPNManager VPNManager { get; private set; }

		public static App Instance
		{
			get
			{
				return Current as App;
			}
		}

		public App(IVPNManager manager)
		{
			InitializeComponent();

			VPNManager = manager;

			if(OptionsManager.SystemFirstRun)
			{
				// TODO: display welcome screen

				OptionsManager.SystemFirstRun = false;
			}

			/*
			if(Device.RuntimePlatform == Device.iOS)
				MainPage = new MainPage(m_vpnManager);
			else
				MainPage = new NavigationPage(new MainPage(m_vpnManager));
			*/
			
			NavigationPage = new NavigationPage(new ServicePage());
			RootPage = new RootPage();
			RootPage.Master = new MenuPage();
			RootPage.Detail = NavigationPage;
			MainPage = RootPage;			
		}
	}
}
