// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2026 AirVPN (support@airvpn.org) / https://airvpn.org
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
using System.Globalization;
using System.Threading;

namespace Eddie.Core.Jobs
{
	public class Discover : Eddie.Core.Job
	{
		private bool m_needBroadcast;

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

			int interval = Engine.Instance.ProfileOptions.GetInt("discover.interval");

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

			if (m_needBroadcast)
			{
				m_needBroadcast = false;
				BroadcastStatus();
			}

			m_timeEvery = 60000;
		}

		public void BroadcastStatus()
		{
			Json j = ToJson();
			j["command"].Value = "ui.discovery.status";
			Engine.Instance.UiManager.Broadcast(j);
		}

		public Json ToJson()
		{
			Dictionary<string, ConnectionInfo> servers;
			lock (Engine.Instance.Connections)
				servers = new Dictionary<string, ConnectionInfo>(Engine.Instance.Connections);

			Int64 timeNow = Utils.UnixTimeStamp();
			int interval = Engine.Instance.ProfileOptions.GetInt("discover.interval");
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
			Json j = new Json();
			j["total"].Value = nTotal;
			j["pending"].Value = nPending;
			return j;
		}

		public string GetStatsString()
		{
			Json j = ToJson();
			return j["pending"].ValueString + " pending / " + j["total"].ValueString + " total";
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
			BroadcastStatus();
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
			// Alias foreign keys onto Eddie's canonical names without overwriting an existing canonical key.

			Json jConnection = jDoc.GetKey("connection") as Json;
			if (jConnection != null)
			{
				if ((!jDoc.HasKey("as_number")) && (jConnection.HasKey("asn")))
					jDoc.SetKey("as_number", jConnection.GetKey("asn"));
				if ((!jDoc.HasKey("isp_name")) && (jConnection.HasKey("isp")))
					jDoc.SetKey("isp_name", jConnection.GetKey("isp"));
				else if ((!jDoc.HasKey("isp_name")) && (jConnection.HasKey("org")))
					jDoc.SetKey("isp_name", jConnection.GetKey("org"));
			}

			if ((jDoc.HasKey("ipAddress")) && (!jDoc.HasKey("ip")))
				jDoc.RenameKey("ipAddress", "ip");
			if ((jDoc.HasKey("query")) && (!jDoc.HasKey("ip")))
				jDoc.RenameKey("query", "ip");

			if ((jDoc.HasKey("country_iso")) && (!jDoc.HasKey("country_code")))
				jDoc.RenameKey("country_iso", "country_code");
			if ((jDoc.HasKey("countryCode")) && (!jDoc.HasKey("country_code")))
				jDoc.RenameKey("countryCode", "country_code");
			if ((jDoc.HasKey("CountryCode")) && (!jDoc.HasKey("country_code")))
				jDoc.RenameKey("CountryCode", "country_code");
			if ((!jDoc.HasKey("country_code")) && (jDoc.HasKey("country")))
			{
				string country = NormalizeServiceResponseValueString(Conversions.ToString(jDoc.GetKey("country")));
				if (country.Length == 2)
					jDoc.SetKey("country_code", country);
			}

			if ((jDoc.HasKey("cityName")) && (!jDoc.HasKey("city_name")))
				jDoc.RenameKey("cityName", "city_name");
			if ((jDoc.HasKey("CityName")) && (!jDoc.HasKey("city_name")))
				jDoc.RenameKey("CityName", "city_name");
			if ((jDoc.HasKey("City")) && (!jDoc.HasKey("city_name")))
				jDoc.RenameKey("City", "city_name");
			if ((jDoc.HasKey("city")) && (!jDoc.HasKey("city_name")))
				jDoc.RenameKey("city", "city_name");

			if ((jDoc.HasKey("regionName")) && (!jDoc.HasKey("region_name")))
				jDoc.RenameKey("regionName", "region_name");
			if ((!jDoc.HasKey("region_name")) && (jDoc.HasKey("region")))
			{
				string region = NormalizeServiceResponseValueString(Conversions.ToString(jDoc.GetKey("region")));
				if (region != "")
					jDoc.SetKey("region_name", region);
			}

			if ((jDoc.HasKey("Latitude")) && (!jDoc.HasKey("latitude")))
				jDoc.RenameKey("Latitude", "latitude");
			if ((jDoc.HasKey("lat")) && (!jDoc.HasKey("latitude")))
				jDoc.RenameKey("lat", "latitude");
			if ((jDoc.HasKey("Longitude")) && (!jDoc.HasKey("longitude")))
				jDoc.RenameKey("Longitude", "longitude");
			if ((jDoc.HasKey("lon")) && (!jDoc.HasKey("longitude")))
				jDoc.RenameKey("lon", "longitude");
			if (((!jDoc.HasKey("latitude")) || (!jDoc.HasKey("longitude"))) && (jDoc.HasKey("loc")))
			{
				string loc = NormalizeServiceResponseValueString(Conversions.ToString(jDoc.GetKey("loc")));
				string[] parts = loc.Split(',');
				if (parts.Length >= 2)
				{
					if (!jDoc.HasKey("latitude"))
						jDoc.SetKey("latitude", parts[0].Trim());
					if (!jDoc.HasKey("longitude"))
						jDoc.SetKey("longitude", parts[1].Trim());
				}
			}

			if ((jDoc.HasKey("asnOrganization")) && (!jDoc.HasKey("isp_name")))
				jDoc.RenameKey("asnOrganization", "isp_name");
			if ((jDoc.HasKey("asn_org")) && (!jDoc.HasKey("isp_name")))
				jDoc.RenameKey("asn_org", "isp_name");
			if ((jDoc.HasKey("organization_name")) && (!jDoc.HasKey("isp_name")))
				jDoc.RenameKey("organization_name", "isp_name");
			if ((jDoc.HasKey("isp")) && (!jDoc.HasKey("isp_name")))
				jDoc.RenameKey("isp", "isp_name");
			if ((!jDoc.HasKey("isp_name")) && (jDoc.HasKey("org")))
				jDoc.SetKey("isp_name", jDoc.GetKey("org"));
			if ((!jDoc.HasKey("isp_name")) && (jDoc.HasKey("organization")))
			{
				string organization = NormalizeServiceResponseValueString(Conversions.ToString(jDoc.GetKey("organization")));
				if (organization.StartsWith("AS", StringComparison.OrdinalIgnoreCase))
				{
					int sp = organization.IndexOf(' ');
					if (sp > 0)
						organization = organization.Substring(sp + 1).Trim();
				}
				if (organization != "")
					jDoc.SetKey("isp_name", organization);
			}

			if ((!jDoc.HasKey("as_number")) && (jDoc.HasKey("asn")))
			{
				string asn = NormalizeServiceResponseValueString(Conversions.ToString(jDoc.GetKey("asn")));
				if (asn.StartsWith("AS", StringComparison.OrdinalIgnoreCase))
					asn = asn.Substring(2).Trim();
				Int64 asnN = 0;
				if ((asn != "") && (Int64.TryParse(asn, NumberStyles.Integer, CultureInfo.InvariantCulture, out asnN)))
					jDoc.SetKey("as_number", asnN);
			}

			if (jDoc.HasKey("ip"))
				jDoc.SetKey("ip", NormalizeServiceResponseValueString(Conversions.ToString(jDoc.GetKey("ip"))).ToLowerInvariant());

			if (jDoc.HasKey("country_code"))
				jDoc.SetKey("country_code", NormalizeServiceResponseValueString(Conversions.ToString(jDoc.GetKey("country_code"))).ToLowerInvariant());

			string regionName = "";
			if (jDoc.HasKey("region_name"))
			{
				regionName = NormalizeServiceResponseValueString(Conversions.ToString(jDoc.GetKey("region_name")));
				jDoc.SetKey("region_name", regionName);
			}

			string cityName = "";
			if (jDoc.HasKey("city_name"))
			{
				cityName = NormalizeServiceResponseValueString(Conversions.ToString(jDoc.GetKey("city_name")));
				jDoc.SetKey("city_name", cityName);
			}

			if (jDoc.HasKey("isp_name"))
				jDoc.SetKey("isp_name", NormalizeServiceResponseValueString(Conversions.ToString(jDoc.GetKey("isp_name"))));

			if (!jDoc.HasKey("location"))
			{
				string location = "";
				if ((regionName != "") && (cityName != "") && (string.Equals(regionName, cityName, StringComparison.OrdinalIgnoreCase) == false))
					location = regionName + ", " + cityName;
				else if (cityName != "")
					location = cityName;
				else if (regionName != "")
					location = regionName;
				if (location != "")
					jDoc.SetKey("location", location);
			}
		}

		private string NormalizeServiceResponseValueString(string s)
		{
			if (s == null)
				return "";
			s = s.Trim();
			if (s == "")
				return "";
			if (string.Equals(s, "N/A", StringComparison.OrdinalIgnoreCase))
				return "";
			if (string.Equals(s, "null", StringComparison.OrdinalIgnoreCase))
				return "";
			return s;
		}

		private Json DiscoverIpData(string ip, string layer)
		{
			string[] methods = Engine.Instance.ProfileOptions.Get("discover.ip_webservice.list").Split(';');
			foreach (string method in methods)
			{
				try
				{
					if ((method.StartsWithInv("http://")) || (method.StartsWithInv("https://")))
					{
						string url = method;
						url = url.Replace("{@ip}", ip);

						HttpRequest httpRequest = new HttpRequest();
						httpRequest.Url = url;
						httpRequest.IpLayer = layer;

						HttpResponse httpResponse = Engine.Instance.FetchUrl(httpRequest);
						string json = httpResponse.GetBody();
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
			m_needBroadcast = true;

			connection.Provider.OnChangeConnection(connection);
		}
	}
}
