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
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

namespace Eddie.Core
{
	public class ProvidersManager
	{
		public bool InvalidateWithNextRefresh = false;

		private Dictionary<string, XmlDocument> Definitions = new Dictionary<string, XmlDocument>();
		private List<Provider> m_providers = new List<Provider>();

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
		
		public int CountEnabled
		{
			get
			{
				int n = 0;
				foreach (Provider provider in Providers)
				{
					if (provider.Enabled)
						n++;
				}
				return n;
			}
		}


		public void Init()
		{
			string path = Platform.Instance.NormalizePath(Engine.Instance.GetPathResources() + "/providers");

			if (Platform.Instance.DirectoryExists(path) == false) // TOCLEAN, Compatibility <3.0
			{
				LoadDefinition(ResourcesFiles.GetString("AirVPN.xml"));
				LoadDefinition(ResourcesFiles.GetString("OpenVPN.xml"));
				return;
			}

			FileInfo[] files = new System.IO.DirectoryInfo(path).GetFiles("*.xml");
			foreach (FileInfo fi in files)
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

			// Hack Eddie 2.x - Removed in 2.13.4
			/*
            {
                string specialOvpnDirectory = Engine.Instance.Storage.GetPath("ovpn");

                // First, find OpenVPN provider if already exists and match {data}/ovpn
                Core.Providers.OpenVPN providerSpecialOpenVPN = null;
                foreach (Provider p in Providers)
                {
                    if (p is Providers.OpenVPN)
                    {
                        if ((p as Providers.OpenVPN).Path == specialOvpnDirectory)
                        {
                            providerSpecialOpenVPN = p as Providers.OpenVPN;
                        }
                    }
                }

                // Remove if directory don't exists
                if (providerSpecialOpenVPN != null)
                {
                    if (Platform.Instance.DirectoryExists(specialOvpnDirectory) == false)
                    {
                        Providers.Remove(providerSpecialOpenVPN);
                    }
                }

                // Add if directory exists
                if (providerSpecialOpenVPN == null)
                {
                    if (Platform.Instance.DirectoryExists(specialOvpnDirectory))
                    {
                        providerSpecialOpenVPN = AddProvider("OpenVPN", null) as Providers.OpenVPN;
                        Utils.XmlSetAttributeString(providerSpecialOpenVPN.Storage.DocumentElement, "path", specialOvpnDirectory);
                    }
                }
            }
			*/
		}

		private void LoadDefinition(string xml)
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xml);

			string code = xmlDoc.DocumentElement.GetAttributeString("code", "");

			Definitions[code] = xmlDoc;
		}

		public XmlElement GetDataAddProviders()
		{
			XmlElement xmlData = ExtensionsXml.XmlCreateElement("data");

			foreach (KeyValuePair<string, XmlDocument> providerDefinition in Definitions)
			{
				string code = providerDefinition.Key;
				string providerClass = providerDefinition.Value.DocumentElement.GetAttributeString("class", "");
				if (providerClass == "service") // Only one instance
				{
					if (ExistsProvider(code))
						continue;
				}

				xmlData.AppendChild(xmlData.OwnerDocument.ImportNode(providerDefinition.Value.DocumentElement, true));
			}

			return xmlData;
		}

		public bool ExistsProvider(string code)
		{
			foreach (Provider provider in Providers)
				if (provider.Code == code)
					return true;
			return false;
		}

		public Provider AddProvider(string providerCode, XmlElement xmlStorage)
		{
			if (Definitions.ContainsKey(providerCode) == false)
				return null;

			XmlDocument xmlDefiniton = Definitions[providerCode];

			string providerClass = xmlDefiniton.DocumentElement.GetAttributeString("class", "");

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
			}

			return provider;
		}
		
		public void Remove(Provider provider)
		{
			Providers.Remove(provider);
		}

		public void DoRefresh(bool force)
		{
			bool postRefreshRecompute = force;
			foreach (Provider provider in Providers)
			{
				if (provider.Enabled)
				{
					if( (force) || (provider.GetNeedRefresh()) )
					{
						postRefreshRecompute = true;
						string result = provider.OnRefresh();
						if (result != "")
						{
							if (Engine.Instance.ConnectionActive == null) // Note: only if not connected, otherwise misunderstanding.
							{
								if (Engine.Instance.Options.GetBool("ui.skip.provider.manifest.failed") == false)
									Engine.Instance.OnProviderManifestFailed(provider);
							}
						}							
					}
				}
			}
			
			if(postRefreshRecompute)
				Engine.Instance.PostManifestUpdate();

			m_lastRefreshDone = Utils.UnixTimeStamp();

			if(InvalidateWithNextRefresh)
			{
				InvalidateWithNextRefresh = false;
				Engine.Instance.InvalidateConnections();
			}
		}

	}
}
