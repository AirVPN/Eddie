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
using System.Threading;
using System.Text;
using System.Xml;
using Eddie.Core;

namespace Eddie.Core.Threads
{
	public class Discover : Eddie.Core.Thread
    {
		private bool m_checkNow = false;

        public override ThreadPriority GetPriority()
        {
            return ThreadPriority.Lowest;
        }

		public void CheckNow()
		{
			m_checkNow = true;
		}

        public override void OnRun()
        {
            Dictionary<string, ConnectionInfo> servers;

            for (; ; )
            {
				m_checkNow = false;

				lock (Engine.Connections)
                        servers = new Dictionary<string, ConnectionInfo>(Engine.Connections);

                int timeNow = Utils.UnixTimeStamp();

                int interval = Engine.Instance.Storage.GetInt("discover.interval");

				foreach (ConnectionInfo infoServer in servers.Values)
                {
                    if (infoServer.NeedDiscover)
                    {
                        if (timeNow - infoServer.LastDiscover >= interval)
                        {
                            DiscoverIp(infoServer);
                        }
                    }

                    if (CancelRequested)
                        return;                    
                }

                for(int i=0;i<600;i++) // Every minute
                {
                    Sleep(100);

					if (m_checkNow)
						break;

                    if (CancelRequested)
                        return;
                }
            }
        }

        public void DiscoverIp(ConnectionInfo connection)
        {			
			string[] methods = Engine.Instance.Storage.Get("discover.ip_webservice.list").Split(';');
            bool onlyFirstResponse = Engine.Instance.Storage.GetBool("discover.ip_webservice.first");
                        
            foreach (string method in methods)
            {
                try
                {
                    if ((method.StartsWith("http://")) || (method.StartsWith("https://")))
                    {
                        // Fetch a webservice

                        string url = method;
                        url = url.Replace("{@ip}", connection.IpsEntry.ToStringFirstIPv4());

                        XmlDocument xmlDoc = Engine.Instance.FetchUrlXml(url, null);

                        if (xmlDoc.DocumentElement.HasChildNodes)
                        {
                            // Normalizations
                            Utils.XmlRenameTagName(xmlDoc.DocumentElement, "CountryCode", "country_code");
							Utils.XmlRenameTagName(xmlDoc.DocumentElement, "CityName", "city_name");
							Utils.XmlRenameTagName(xmlDoc.DocumentElement, "Latitude", "latitude");
							Utils.XmlRenameTagName(xmlDoc.DocumentElement, "Longitude", "longitude");

							// Node parsing
							string countryCode = Utils.XmlGetBody(xmlDoc.DocumentElement.SelectSingleNode(".//country_code") as XmlElement).ToLowerInvariant().Trim();
                            if (CountriesManager.IsCountryCode(countryCode))
                            {
                                if(connection.CountryCode != countryCode)
                                {
									connection.CountryCode = countryCode;
                                    Engine.Instance.MarkServersListUpdated();
                                    Engine.Instance.MarkAreasListUpdated();
                                }
                            }

							string cityName = Utils.XmlGetBody(xmlDoc.DocumentElement.SelectSingleNode(".//city_name") as XmlElement).Trim();
                            if (cityName == "N/A")
                                cityName = "";
							if (cityName != "")
							{
								connection.Location = cityName;
								Engine.Instance.MarkServersListUpdated();
							}

							float latitude = Conversions.ToFloat(Utils.XmlGetBody(xmlDoc.DocumentElement.SelectSingleNode(".//latitude") as XmlElement).Trim());
							float longitude = Conversions.ToFloat(Utils.XmlGetBody(xmlDoc.DocumentElement.SelectSingleNode(".//longitude") as XmlElement).Trim());
							if( (latitude != 0) && (longitude != 0) )
							{
								connection.Latitude = latitude;
								connection.Longitude = longitude;
								Engine.Instance.MarkServersListUpdated();
							}

							if (onlyFirstResponse)
                                break;
                        }         
                        else
                        {
                            Engine.Instance.Logs.Log(LogType.Verbose, "Discovery, unable to fetch " + url);
                        }               
                    }
                }
                catch (Exception)
                {
                }
            }

			connection.LastDiscover = Utils.UnixTimeStamp();

			connection.Provider.OnChangeConnection(connection);
		}
    }
}
