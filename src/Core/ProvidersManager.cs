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
        private Dictionary<string, XmlDocument> Definitions = new Dictionary<string, XmlDocument>();
        private List<Provider> m_providers = new List<Provider>();

        private Int64 m_lastRefreshTry = 0;
        private Int64 m_lastRefreshDone = 0;

        public List<Provider> Providers
        {
            get
            {
                return m_providers;
            }
        }

        public Int64 LastRefreshDone
        {
            get
            {
                return m_lastRefreshDone;
            }
        }

        public string GetProvidersPath() // TOCLEAN, only until 3.0
        {
            string path = "";
            if (Engine.Instance.DevelopmentEnvironment)
                path = Platform.Instance.GetProjectPath();
            else
                path = Platform.Instance.GetProgramFolder();
            path += "//Providers//";

            if (Directory.Exists(path) == false)
                return "";
            else
                return path;
        }

        public void Init()
		{
            string path = GetProvidersPath();

            if (Directory.Exists(path) == false) // TOCLEAN, Compatibility <3.0
            {
                LoadDefinition(ResourcesFiles.GetString("AirVPN.xml"));
                LoadDefinition(ResourcesFiles.GetString("OpenVPN.xml"));
                return;
            }            

            FileInfo[] files = new System.IO.DirectoryInfo(path).GetFiles("*.xml");
            foreach(FileInfo fi in files)
            {
                string xml = Platform.Instance.FileContentsReadText(fi.FullName);
                LoadDefinition(xml);                
            }
		}

        public void Load()
        {
            foreach (XmlElement xmlProvider in Engine.Instance.Storage.Providers)
            {
                string providerCode = xmlProvider.Name;
                AddProvider(providerCode, xmlProvider);
            }

            if (Providers.Count == 0)
                AddProvider("AirVPN", null);
        }

        private void LoadDefinition(string xml)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            string title = Utils.XmlGetAttributeString(xmlDoc.DocumentElement, "title", "");

            Definitions[title] = xmlDoc;
        }

        public Provider AddProvider(string providerCode, XmlElement xmlStorage)
        {
            if (Definitions.ContainsKey(providerCode) == false)
                return null;

            XmlDocument xmlDefiniton = Definitions[providerCode];
            
            string providerClass = Utils.XmlGetAttributeString(xmlDefiniton.DocumentElement, "class", "");

            Provider provider = null;

            if (providerClass == "service")
            {
                provider = new Providers.Service();
            }
            else if (providerClass == "openvpn")
            {
                provider = new Providers.OpenVPN();
            }
            else
                return null;

            if (provider != null)
            {
                provider.Definition = xmlDefiniton.DocumentElement;

                provider.OnInit();

                provider.OnLoad(xmlStorage);

                m_providers.Add(provider);

                if (provider.GetCode() == "AirVPN")
                    Engine.Instance.AirVPN = provider as Providers.Service;
            }

            return provider;
        }

        public bool NeedUpdate(bool recommended)
        {
            Int64 refreshInterval = Engine.Instance.Storage.GetInt("advanced.manifest.refresh");
            if (refreshInterval == 0)
                return false;

            if (refreshInterval < 0)
                refreshInterval = 10; // Default
            
            if (m_lastRefreshTry + 60 * refreshInterval < Utils.UnixTimeStamp())
                return true;
            
            return false;
        }

        public string Refresh()
        {
            m_lastRefreshTry = Utils.UnixTimeStamp();

            Engine.Instance.Logs.Log(LogType.Verbose, Messages.ManifestUpdate);

            // TOOPTIMIZE: Stop at first error
            foreach (Provider provider in Providers)
            {
                if (provider.Enabled)
                {
                    string result = provider.Refresh();
                    if (result != "")
                        return result;
                }
            }

            Engine.Instance.PostManifestUpdate();

            Engine.Instance.Logs.Log(LogType.Verbose, Messages.ManifestDone);

            m_lastRefreshDone = Utils.UnixTimeStamp();

            return "";
        }

    }
}
