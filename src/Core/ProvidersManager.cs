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
using System.IO;
using System.Text;
using System.Xml;

namespace Eddie.Core
{
    public class ProvidersManager
    {
        private List<Provider> m_providers = new List<Provider>();

        public List<Provider> Providers
        {
            get
            {
                return m_providers;
            }
        }

        public void Init()
		{
            string path = "";
            if (Engine.Instance.DevelopmentEnvironment)
                path = Platform.Instance.GetProjectPath();
            else
                path = Platform.Instance.GetProgramFolder();

            path += "//providers//";

            if(Directory.Exists(path) == false) // TOCLEAN, Compatibility <3.0
            {
                Engine.Instance.AirVPN = Process(ResourcesFiles.GetString("AirVPN.xml")) as Providers.Service;                
                return;
            }

            FileInfo[] files = new System.IO.DirectoryInfo(path).GetFiles("*.xml");
            foreach(FileInfo fi in files)
            {
                string xml = File.ReadAllText(fi.FullName);
                Provider provider = Process(xml);

                if (provider.GetCode() == "AirVPN")
                    Engine.Instance.AirVPN = provider as Providers.Service;
            }
		}

        public void Load()
        {
            foreach (Provider provider in Providers)
            {
                provider.OnLoad();
            }
        }

        private Provider Process(string xml)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);            

            XmlElement xmlProviderDefinition = xmlDoc.DocumentElement;

            string code = xmlProviderDefinition.Name;

            string providerClass = Utils.XmlGetAttributeString(xmlProviderDefinition, "class", "");

            Provider provider = null;
                        
            if (providerClass == "service")
            {
                provider = new Providers.Service();
            }
            else if (providerClass == "openvpn")
            {
                provider = new Providers.OpenVPN();
                Core.Providers.OpenVPN.Singleton = provider as Providers.OpenVPN;
            }
            else
                return null;            

            if (provider != null)
            {
                provider.Definition = xmlProviderDefinition;
                
                provider.OnInit();

                m_providers.Add(provider);
            }

            return provider;
        }
        
    }
}
