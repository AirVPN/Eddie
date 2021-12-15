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
using System.Threading;

namespace Eddie.Core.Jobs
{
	public class Discover : Eddie.Core.Job
	{
		public override ThreadPriority GetPriority()
		{
			return ThreadPriority.Lowest;
		}

		public override void OnRun()
		{
			Dictionary<string, ConnectionInfo> servers;

			lock (Engine.Instance.Connections)
				servers = new Dictionary<string, ConnectionInfo>(Engine.Instance.Connections);

			Int64 timeNow = Utils.UnixTimeStamp();

			int interval = Engine.Instance.Options.GetInt("discover.interval");

			foreach (ConnectionInfo infoServer in servers.Values)
			{
				if (m_cancelRequested)
					return;

				if (infoServer.NeedDiscover)
				{
					if (timeNow - infoServer.LastDiscover >= interval)
					{
						DiscoverIp(infoServer);
					}
				}
			}

			m_timeEvery = 60000;
		}

		public string GetStatsString()
		{
			Dictionary<string, ConnectionInfo> servers;
			lock (Engine.Instance.Connections)
				servers = new Dictionary<string, ConnectionInfo>(Engine.Instance.Connections);

			Int64 timeNow = Utils.UnixTimeStamp();
			int interval = Engine.Instance.Options.GetInt("discover.interval");
			int nTotal = 0;
			int nPending = 0;

			foreach (ConnectionInfo infoServer in servers.Values)
			{
				if (infoServer.NeedDiscover)
				{
					nTotal++;
					if (timeNow - infoServer.LastDiscover >= interval)
						nPending++;
				}
			}
			return nPending + " pending / " + nTotal;
		}

		public void InvalidateAll()
		{
			Dictionary<string, ConnectionInfo> servers;

			lock (Engine.Instance.Connections)
				servers = new Dictionary<string, ConnectionInfo>(Engine.Instance.Connections);

			foreach (ConnectionInfo infoServer in servers.Values)
			{
				infoServer.LastDiscover = 0;
			}

			CheckNow();
		}

		public IpAddresses DiscoverExit()
		{
			IpAddresses result = new IpAddresses();

			string[] layers = new string[] { "4", "6" };

			foreach (string layer in layers)
			{
				Json jDoc = DiscoverIpData("", layer);
				if ((jDoc != null) && (jDoc.HasKey("ip")))
				{
					string ip = (jDoc["ip"].Value as string).Trim().ToLowerInvariant();

					result.Add(ip);
				}
			}

			return result;
		}

		private void NormalizeServiceResponse(Json jDoc)
		{
			if (jDoc.HasKey("countryCode"))
				jDoc.RenameKey("countryCode", "country_code");
			if (jDoc.HasKey("CountryCode"))
				jDoc.RenameKey("CountryCode", "country_code");
			if (jDoc.HasKey("CityName"))
				jDoc.RenameKey("CityName", "city_name");
			if (jDoc.HasKey("City"))
				jDoc.RenameKey("City", "city_name");
			if (jDoc.HasKey("city"))
				jDoc.RenameKey("city", "city_name");
			if (jDoc.HasKey("Latitude"))
				jDoc.RenameKey("Latitude", "latitude");
			if (jDoc.HasKey("Longitude"))
				jDoc.RenameKey("Longitude", "longitude");
		}

		private Json DiscoverIpData(string ip, string layer)
		{
			string[] methods = Engine.Instance.Options.Get("discover.ip_webservice.list").Split(';');
			foreach (string method in methods)
			{
				try
				{
					if ((method.StartsWith("http://")) || (method.StartsWith("https://")))
					{
						string url = method;
						url = url.Replace("{@ip}", ip);

						HttpRequest httpRequest = new HttpRequest();
						httpRequest.Url = url;
						httpRequest.IpLayer = layer;

						string json = Engine.Instance.FetchUrl(httpRequest).GetBody();
						Json jDoc = Json.Parse(json);

						NormalizeServiceResponse(jDoc);

						return jDoc;
					}
				}
				catch (Exception)
				{
				}
			}
			return null;
		}

		public void DiscoverIp(ConnectionInfo connection)
		{
			IpAddress ip = connection.IpsEntry.FirstPreferIPv4;
			if (ip != null)
			{
				Json jDoc = DiscoverIpData(ip.Address, "");
				if (jDoc != null)
				{
					// Node parsing
					if (jDoc.HasKey("country_code"))
					{
						string countryCode = Conversions.ToString(jDoc["country_code"].Value).Trim().ToLowerInvariant();
						if (CountriesManager.IsCountryCode(countryCode))
						{
							if (connection.CountryCode != countryCode)
							{
								connection.CountryCode = countryCode;
								Engine.Instance.MarkServersListUpdated();
								Engine.Instance.MarkAreasListUpdated();
							}
						}
					}

					if (jDoc.HasKey("city_name"))
					{
						string cityName = Conversions.ToString(jDoc["city_name"].Value).Trim();
						if (cityName == "N/A")
							cityName = "";
						if (cityName != "")
						{
							connection.Location = cityName;
							Engine.Instance.MarkServersListUpdated();
						}
					}

					if ((jDoc.HasKey("latitude")) && (jDoc.HasKey("longitude")))
					{
						double latitude = Conversions.ToDouble(jDoc["latitude"].Value);
						double longitude = Conversions.ToDouble(jDoc["longitude"].Value);

						connection.Latitude = latitude;
						connection.Longitude = longitude;
						Engine.Instance.MarkServersListUpdated();
					}
				}
			}

			connection.LastDiscover = Utils.UnixTimeStamp();

			connection.Provider.OnChangeConnection(connection);
		}
	}
}
