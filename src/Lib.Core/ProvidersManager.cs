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

		private Dictionary<string, Json> Definitions = new Dictionary<string, Json>();
		private List<Providers.IProvider> m_providers = new List<Providers.IProvider>();

		private Int64 m_lastRefreshDone = 0;

		public List<Providers.IProvider> Providers
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
				foreach (Providers.IProvider provider in Providers)
				{
					if (provider.Enabled)
						n++;
				}
				return n;
			}
		}


		public void Init()
		{
			string path = Engine.Instance.LocateResource("providers");

			FileInfo[] filesJson = new System.IO.DirectoryInfo(path).GetFiles("*.json");
			foreach (FileInfo fi in filesJson)
			{
				LoadDefinitionFromFile(fi.FullName);
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

		private void LoadDefinitionFromFile(string path)
		{
			Json j = Json.Parse(Platform.Instance.FileContentsReadText(path));

			string code = j["code"].ValueString;

			if ((code == "WireGuard") && (Platform.Instance.GetSupportWireGuard() == false))
				return;

			Definitions[code] = j;
		}

		public Json GetDataAddProviders()
		{
			Json jList = new Json();
			jList.EnsureArray();

			foreach (KeyValuePair<string, Json> providerDefinition in Definitions)
			{
				string code = providerDefinition.Key;
				string providerClass = providerDefinition.Value["class"].ValueString;
				if (providerClass == "service") // Only one instance
				{
					if (ExistsProvider(code))
						continue;
				}

				jList.Append(providerDefinition.Value);
			}

			return jList;
		}

		public bool ExistsProvider(string code)
		{
			foreach (Providers.IProvider provider in Providers)
				if (provider.Code == code)
					return true;
			return false;
		}

		public Providers.IProvider AddProvider(string providerCode, XmlElement xmlStorage)
		{
			if (Definitions.ContainsKey(providerCode) == false)
				return null;

			Json jDefinition = Definitions[providerCode] as Json;
			string providerClass = jDefinition["class"].ValueString;

			Providers.IProvider provider = null;

			if (providerClass == "service")
			{
				provider = new Providers.Service();
			}
			else if (providerClass == "openvpn")
			{
				provider = new Providers.OpenVPN();
			}
			else if ((providerClass == "wireguard") && (Platform.Instance.GetSupportWireGuard()))
			{
				provider = new Providers.WireGuard();
			}
			else
				return null;

			if (provider != null)
			{
				provider.Definition = jDefinition;

				provider.OnInit();

				provider.OnLoad(xmlStorage);

				m_providers.Add(provider);
			}

			return provider;
		}

		public void Remove(Providers.IProvider provider)
		{
			Providers.Remove(provider);
		}

		public void DoRefresh(bool force)
		{
			bool postRefreshRecompute = force;
			foreach (Providers.IProvider provider in Providers)
			{
				if (provider.Enabled)
				{
					if ((force) || (provider.GetNeedRefresh()))
					{
						postRefreshRecompute = true;
						string result = provider.OnRefresh();
						if (result != "")
						{
							if (Engine.Instance.Connection == null) // Note: only if not connected, otherwise misunderstanding.
							{
								if (Engine.Instance.Options.GetBool("ui.skip.provider.manifest.failed") == false)
									Engine.Instance.OnProviderManifestFailed(provider);
							}
						}
					}
				}
			}

			if (postRefreshRecompute)
				Engine.Instance.PostManifestUpdate();

			m_lastRefreshDone = Utils.UnixTimeStamp();

			if (InvalidateWithNextRefresh)
			{
				InvalidateWithNextRefresh = false;
				Engine.Instance.InvalidateConnections();
			}
		}

	}
}
