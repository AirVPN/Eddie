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
        public override ThreadPriority GetPriority()
        {
            return ThreadPriority.Lowest;
        }

        public override void OnRun()
        {
            Dictionary<string, ServerInfo> servers;

            for (; ; )
            {
                lock (Engine.Servers)
                        servers = new Dictionary<string, ServerInfo>(Engine.Servers);

                int timeNow = Utils.UnixTimeStamp();

                int delay = 60;

                foreach (ServerInfo infoServer in servers.Values)
                {
                    if (infoServer.NeedDiscover)
                    {
                        if (timeNow - infoServer.LastDiscover >= delay)
                        {
                            // TOFIX / TOCONTINUE
                            DiscoverIp(infoServer);

                            infoServer.LastDiscover = timeNow;
                        }
                    }

                    if (CancelRequested)
                        return;                    
                }

                for(int i=0;i<600;i++) // Every minute
                {
                    Sleep(100);

                    if (CancelRequested)
                        return;
                }
            }
        }

        public void DiscoverIp(ServerInfo server)
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
                        url = url.Replace("{@ip}", server.IpEntry);

                        XmlDocument xmlDoc = Engine.Instance.XmlFromUrl(url, "", null, "iptitle", false); // Clodo: Bypass proxy?

                        if (xmlDoc.DocumentElement.HasChildNodes)
                        {
                            // Node renaming
                            Utils.XmlRenameTagName(xmlDoc.DocumentElement, "CountryCode", "country_code");

                            // Node parsing
                            //string countryCode = Utils.XmlGetBody(Utils.XmlGetFirstElementByTagName(xmlDoc.DocumentElement, "country_code")).ToLowerInvariant().Trim();
                            string countryCode = Utils.XmlGetBody(xmlDoc.DocumentElement.SelectSingleNode(".//country_code") as XmlElement).ToLowerInvariant().Trim();
                            if (CountriesManager.IsCountryCode(countryCode))
                            {
                                if(server.CountryCode != countryCode)
                                {
                                    server.CountryCode = countryCode;
                                    Engine.Instance.MarkServersListUpdated();
                                    Engine.Instance.MarkAreasListUpdated();
                                }
                            }
                                    

                            string cityName = Utils.XmlGetBody(xmlDoc.DocumentElement.SelectSingleNode(".//city_name") as XmlElement).Trim();
                            if (cityName == "N/A")
                                cityName = "";
                            if (cityName != "")
                                server.Location = cityName;

                            if (onlyFirstResponse)
                                break;
                        }         
                        else
                        {
                            Engine.Instance.Logs.Log(LogType.Fatal, "Unable to fetch " + url);
                        }               
                    }
                }
                catch (Exception)
                {
                }
            }            
        }
    }
}
