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
using System.Text;

namespace Eddie.Core
{
	public class HttpRequest
	{
		public string Url = "";
		public System.Collections.Specialized.NameValueCollection Parameters = new System.Collections.Specialized.NameValueCollection();
		public bool BypassProxy = false;
		public string IpLayer = "";
		public string ForceResolve = "";

		public Json ToJson()
		{
			Json j = new Json();

            j["url"].Value = Url;

			j["postfields"].Value = "";
			foreach (string k in Parameters.Keys)
			{
				if(j["postfields"].ValueString != "")
					j["postfields"].Value += "&";
				j["postfields"].Value += Uri.EscapeUriString(k) + "=" + Uri.EscapeUriString(Parameters[k]);
			}				

			j["iplayer"].Value = IpLayer;
			j["resolve-single"].Value = ForceResolve;
			j["timeout"].Value = Engine.Instance.Options.GetInt("http.timeout");
			j["cacert"].Value = SystemShell.EscapePath(Engine.Instance.LocateResource("cacert.pem"));
            j["useragent"].Value = Constants.Name + "/" + Constants.VersionDesc;

			// Don't use proxy if connected to the VPN, or in special cases (checking) during connection.
			bool bypassProxy = BypassProxy;
			if (bypassProxy == false)
				bypassProxy = Engine.Instance.IsConnected();

			j["proxy"].Value = "";
			j["proxyauth"].Value = "";
			j["proxyuserpwd"].Value = "";
			if (bypassProxy == false)
			{
				string proxyMode = Engine.Instance.Options.GetLower("proxy.mode");
				string proxyWhen = Engine.Instance.Options.GetLower("proxy.when");
				string proxyHost = Engine.Instance.Options.Get("proxy.host");
				int proxyPort = Engine.Instance.Options.GetInt("proxy.port");
				string proxyAuth = Engine.Instance.Options.Get("proxy.auth").ToLowerInvariant();
				string proxyLogin = Engine.Instance.Options.Get("proxy.login");
				string proxyPassword = Engine.Instance.Options.Get("proxy.password");

				if ((proxyWhen == "none") || (proxyWhen == "openvpn"))
					proxyMode = "none";

				if (proxyMode == "detect")
					throw new Exception(LanguageManager.GetText("ProxyDetectDeprecated"));

				if (proxyMode == "tor")
				{
					proxyMode = "socks";
					proxyAuth = "none";
					proxyLogin = "";
					proxyPassword = "";
				}

				if (proxyMode == "http")
				{
					j["proxy"].Value = "http://" + proxyHost + ":" + proxyPort.ToString();
				}
				else if (proxyMode == "socks")
				{
					// curl support different types of proxy. OpenVPN not, only socks5. So, it's useless to support other kind of proxy here.
					j["proxy"].Value = "socks5://" + proxyHost + ":" + proxyPort.ToString();
				}

				if ((proxyMode != "none") && (proxyAuth != "none"))
				{
					j["proxyauth"].Value = proxyAuth;

					if ((proxyLogin != "") && (proxyPassword != ""))
						j["proxyuserpwd"].Value = proxyLogin + ":" + proxyPassword;
				}
			}

			return j;
		}
	}

}
