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

using Eddie.Common.Log;
using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;

namespace Eddie
{
	public partial class ServicePage : ContentPage
	{
		public ServicePage()
		{
			InitializeComponent();

			Title = "Eddie";

			IVPNManager manager = App.Instance.VPNManager;

			InitUI();
			RefreshUI(manager.Ready, manager.Status, manager.LastError);

			manager.StatusChanged += OnServiceStatusChanged;
		}

		private void OnServiceStatusChanged(bool ready, VPN.ServiceStatus status, string error)
		{
			Device.BeginInvokeOnMainThread(() => RefreshUI(ready, status, error));
		}

		private void InitUI()
		{
			m_buttonStart.Clicked += OnStartService;
			m_buttonStop.Clicked += OnStopService;
			m_buttonPickFile.Clicked += OnPickFile;
			
			m_profileFile.Text = OptionsManager.SystemLastProfileFile;
		}

		private async void OnPickFile(object sender, EventArgs args)
		{
			try
			{
				string downloadDirectory = App.Instance.VPNManager.GetDownloadDirectory();

				List<string> files = new List<string>();

				IEnumerator<string> filesEnumerator = Directory.EnumerateFiles(downloadDirectory, "*.ovpn").GetEnumerator();
				while(filesEnumerator.MoveNext())
				{
					files.Add(filesEnumerator.Current);
				}

				string profileFile = await DisplayActionSheet("Select a profile file", "Cancel", null, files.ToArray());
				if(profileFile != "Cancel")		// :<>
					m_profileFile.Text = profileFile;
			}
			catch(Exception e)
			{
				UpdateLastError(e.Message);
			}
		}

		private void StartProfileFile(string filename)
		{
			OptionsManager.SystemLastProfileFile = filename;

			App.Instance.VPNManager.ClearProfiles();

			String profileFile = filename.Trim();
			if(profileFile.Length > 0)
				App.Instance.VPNManager.AddProfileFile(profileFile);

			String profileString = OptionsManager.Ovpn3CustomDirectives.Trim();
			if(profileString.Length > 0)
				App.Instance.VPNManager.AddProfileString(profileString);

			App.Instance.VPNManager.Start();
		}

		private void OnStartService(object sender, EventArgs args)
		{
			try
			{
				StartProfileFile(m_profileFile.Text != null ? m_profileFile.Text : "");
			}			
			catch(Exception e)
			{
				UpdateLastError(e.Message);
			}
		}

		private void OnStopService(object sender, EventArgs args)
		{
			try
			{
				App.Instance.VPNManager.Stop();
			}
			catch(Exception e)
			{
				UpdateLastError(e.Message);
			}
		}

		private void RefreshUI(bool ready, VPN.ServiceStatus status, string error)
		{
			if(ready)
				m_status.Text = "Status: " + status.ToString();
			else
				m_status.Text = "Initializing...";

			m_buttonStart.IsEnabled = ready && (status == VPN.ServiceStatus.Stopped);
			m_buttonStop.IsEnabled = (status == VPN.ServiceStatus.Starting) || (status == VPN.ServiceStatus.Started);

			UpdateLastError(error);
		}

		private void UpdateLastError(string error)
		{
			m_error.IsVisible = !Utils.Empty(error);
			m_error.Text = "Error: " + error;
		}
	}
}
