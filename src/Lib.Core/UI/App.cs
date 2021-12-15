// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2019 AirVPN (support@airvpn.org) / https://airvpn.org
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

namespace Eddie.Core.UI
{
	// Eddie2 Application Helper 
	public class App
	{
		public static void OpenStats(string key)
		{
			if (key == "vpngeneratedconfig")
			{
				if (Engine.Instance.IsConnected())
				{
					Engine.Instance.OnShowText(LanguageManager.GetText("StatsVpnGeneratedConfig"), Engine.Instance.Connection.ExportConfigStartup());
				}
			}
			else if (key == "vpngeneratedconfigpush")
			{
				if (Engine.Instance.IsConnected())
				{
					Engine.Instance.OnShowText(LanguageManager.GetText("StatsVpnGeneratedConfigPush"), Engine.Instance.Connection.ExportConfigPush());
				}
			}
			else if (key == "pinger")
			{
				// ClodoTemp must be InvalidatePinger(), but check Refresh.Full steps
				Engine.Instance.InvalidateConnections();
			}
			else if (key == "discovery")
			{
				Engine.Instance.InvalidateDiscovery();
			}
			else if (key == "pathapp")
			{
				Platform.Instance.OpenDirectoryInFileManager(Engine.Instance.Stats.Get("PathApp").Value);
			}
			else if (key == "pathdata")
			{
				Platform.Instance.OpenDirectoryInFileManager(Engine.Instance.Stats.Get("PathData").Value);
			}
			else if (key == "pathprofile")
			{
				Platform.Instance.OpenDirectoryInFileManager(Engine.Instance.Stats.Get("PathProfile").Value);
			}
		}
	}
}
