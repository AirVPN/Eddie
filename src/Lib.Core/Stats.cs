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

using System;
using System.Collections.Generic;

namespace Eddie.Core
{
	public class Stats
	{
		public Dictionary<string, StatsEntry> Dict = new Dictionary<string, StatsEntry>();
		public List<StatsEntry> List = new List<StatsEntry>();

		public UI.Charts Charts;

		public Stats()
		{
			Charts = new UI.Charts();


			Add("ServerName", LanguageManager.GetText("StatsServerName"), "server");
			Add("ServerLatency", LanguageManager.GetText("StatsServerLatency"), "server");
			Add("ServerLocation", LanguageManager.GetText("StatsServerLocation"), "server");
			Add("ServerLoad", LanguageManager.GetText("StatsServerLoad"), "server");
			Add("ServerUsers", LanguageManager.GetText("StatsServerUsers"), "server");
			Add("AccountLogin", LanguageManager.GetText("StatsLogin"), "session");
			Add("AccountKey", LanguageManager.GetText("StatsKey"), "session");
			Add("VpnSpeedDownload", LanguageManager.GetText("StatsVpnSpeedDownload"), "session");
			Add("VpnSpeedUpload", LanguageManager.GetText("StatsVpnSpeedUpload"), "session");
			Add("VpnStart", LanguageManager.GetText("StatsVpnStart"), "session");
			Add("VpnTotalDownload", LanguageManager.GetText("StatsVpnTotalDownload"), "vpn");
			Add("VpnTotalUpload", LanguageManager.GetText("StatsVpnTotalUpload"), "vpn");
			Add("VpnEntryIP", LanguageManager.GetText("StatsVpnEntryIP"), "vpn");
			Add("VpnExitIPv4", LanguageManager.GetText("StatsVpnExitIPv4"), "vpn");
			Add("VpnExitIPv6", LanguageManager.GetText("StatsVpnExitIPv6"), "vpn");
			Add("VpnType", LanguageManager.GetText("StatsVpnType"), "vpn");
			Add("VpnProtocol", LanguageManager.GetText("StatsVpnProtocol"), "vpn");
			Add("VpnPort", LanguageManager.GetText("StatsVpnPort"), "vpn");
			Add("VpnRealIp", LanguageManager.GetText("StatsVpnRealIp"), "vpn");
			Add("VpnIp", LanguageManager.GetText("StatsVpnIp"), "vpn");
			Add("VpnDns", LanguageManager.GetText("StatsVpnDns"), "vpn");
			Add("VpnInterface", LanguageManager.GetText("StatsVpnInterface"), "vpn");
			Add("VpnDataChannel", LanguageManager.GetText("StatsVpnDataChannel"), "vpn");
			Add("VpnControlChannel", LanguageManager.GetText("StatsVpnControlChannel"), "vpn");
			Add("VpnGeneratedConfig", LanguageManager.GetText("StatsVpnGeneratedConfig"), "vpn", "view");
			Add("VpnGeneratedConfigPush", LanguageManager.GetText("StatsVpnGeneratedConfigPush"), "vpn", "view");
			Add("SessionStart", LanguageManager.GetText("StatsSessionStart"), "session");
			Add("SessionTotalDownload", LanguageManager.GetText("StatsSessionTotalDownload"), "session");
			Add("SessionTotalUpload", LanguageManager.GetText("StatsSessionTotalUpload"), "session");
			Add("Discovery", LanguageManager.GetText("StatsDiscovery"), "system", "Update");
			Add("Pinger", LanguageManager.GetText("StatsPinger"), "system", "Update");
			Add("SystemTimeServerDifference", LanguageManager.GetText("StatsSystemTimeServerDifference"), "system");
			Add("PathProfile", LanguageManager.GetText("StatsSystemPathProfile"), "system", "Open");
			Add("PathData", LanguageManager.GetText("StatsSystemPathData"), "system", "Open");
			Add("PathApp", LanguageManager.GetText("StatsSystemPathApp"), "system", "Open");

			UpdateValue("PathProfile", Engine.Instance.GetProfilePath());
			UpdateValue("PathData", Engine.Instance.GetDataPath());
			UpdateValue("PathApp", Platform.Instance.GetApplicationPath());
		}

		public void Add(string key, string caption, string icon)
		{
			Add(key, caption, icon, "");
		}

		public void Add(string key, string caption, string icon, string clickable)
		{
			StatsEntry entry = new StatsEntry();
			entry.Key = key;
			entry.Caption = caption;
			entry.Icon = icon;
			entry.Clickable = clickable;
			Dict[key] = entry;
			List.Add(entry);
		}

		public StatsEntry Get(string key)
		{
			StatsEntry entry = Dict[key];
			if (entry == null)
				throw new Exception("Unknown stats.");
			return entry;
		}

		public void UpdateValue(string key, string newValue)
		{
			StatsEntry entry = Get(key);

			if (entry.Value != newValue)
			{
				entry.Value = newValue;

				Engine.Instance.OnStatsChange(entry);
			}
		}

		public string GetValue(string key)
		{
			return Get(key).Value;
		}
	}
}
