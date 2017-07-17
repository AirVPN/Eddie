// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2016 AirVPN (support@airvpn.org) / https://airvpn.org
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
using System.Text;
using Eddie.Lib.Common;

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


			Add("ServerName", Messages.StatsServerName, "server");
			Add("ServerLatency", Messages.StatsServerLatency, "server");
			Add("ServerLocation", Messages.StatsServerLocation, "server");
			Add("ServerLoad", Messages.StatsServerLoad, "server");
			Add("ServerUsers", Messages.StatsServerUsers, "server");
			Add("AccountLogin", Messages.StatsLogin, "session");
			Add("AccountKey", Messages.StatsKey, "session");
			Add("VpnSpeedDownload", Messages.StatsVpnSpeedDownload, "session");
			Add("VpnSpeedUpload", Messages.StatsVpnSpeedUpload, "session");
			Add("VpnConnectionStart", Messages.StatsVpnConnectionStart, "session");            
            Add("VpnTotalDownload", Messages.StatsVpnTotalDownload, "vpn");
			Add("VpnTotalUpload", Messages.StatsVpnTotalUpload, "vpn");
			Add("VpnIpEntry", Messages.StatsVpnIpEntry, "vpn");
			Add("VpnIpExit", Messages.StatsVpnIpExit, "vpn");
			Add("VpnProtocol", Messages.StatsVpnProtocol, "vpn");
			Add("VpnPort", Messages.StatsVpnPort, "vpn");
			Add("VpnRealIp", Messages.StatsVpnRealIp, "vpn");
			Add("VpnIp", Messages.StatsVpnIp, "vpn");
			Add("VpnDns", Messages.StatsVpnDns, "vpn");
			Add("VpnInterface", Messages.StatsVpnInterface, "vpn");
			Add("VpnGateway", Messages.StatsVpnGateway, "vpn");
			Add("VpnCipher", Messages.StatsVpnCipher, "vpn");
			Add("VpnControlChannel", Messages.StatsVpnControlChannel, "vpn");
			Add("VpnGeneratedOVPN", Messages.StatsVpnGeneratedOVPN, "vpn", "view");
			Add("SessionTotalDownload", Messages.StatsSessionTotalDownload, "session");
            Add("SessionTotalUpload", Messages.StatsSessionTotalUpload, "session");            
			Add("ManifestLastUpdate", Messages.StatsManifestLastUpdate, "system", "Update");
			Add("Pinger", Messages.StatsPinger, "system", "Update"); 
			Add("SystemTimeServerDifference", Messages.StatsSystemTimeServerDifference, "system");
            Add("PathProfile", Messages.StatsSystemPathProfile, "system", "Open");
            Add("PathApp", Messages.StatsSystemPathApp, "system", "Open");
            Add("SystemReport", Messages.StatsSystemReport, "system", "View");
            
            UpdateValue("PathProfile", Engine.Instance.Storage.GetProfilePath());
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

                XmlItem xml = new XmlItem("command");
                xml.SetAttribute("action", "ui.stats.change");
                entry.WriteXML(xml);
                Engine.Instance.Command(xml);

                Engine.Instance.OnStatsChange(entry);
			}
		}

		public string GetValue(string key)
		{
			return Get(key).Value;
		}
    }
}
