// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2023 AirVPN (support@airvpn.org) / https://airvpn.org
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
using System.Xml;

namespace Eddie.Core.Providers
{
	public class IProvider
	{
		public Json Definition;
		public XmlDocument Storage;

		protected Credentials m_runCredentials;

		protected Int64 m_lastTryRefresh = 0;

		public virtual bool GetEnabledByDefault()
		{
			return true;
		}

		public virtual IpAddresses GetNetworkLockAllowlistOutgoingIPs()
		{
			IpAddresses list = new IpAddresses();

			return list;
		}

		public virtual string GetFrontMessage()
		{
			return "";
		}

		public string Code
		{
			get
			{
				return Definition["code"].ValueString;
			}
		}

		public string DefinitionTitle
		{
			get
			{
				return Definition["title"].ValueString;
			}
		}

		public string DefinitionSubTitle
		{
			get
			{
				return Definition["subtitle"].ValueString;
			}
		}

		public string DefinitionHref
		{
			get
			{
				return Definition["href"].ValueString;
			}
		}

		public string ID
		{
			get
			{
				return Storage.DocumentElement.GetAttributeString("id", "");
			}
			set
			{
				Storage.DocumentElement.SetAttributeString("id", value);
			}
		}

		public bool Enabled
		{
			get
			{
				return Storage.DocumentElement.GetAttributeBool("enabled", GetEnabledByDefault());
			}
			set
			{
				Storage.DocumentElement.SetAttributeBool("enabled", value);
			}
		}

		public int RefreshInterval
		{
			get
			{
				return Storage.DocumentElement.GetAttributeInt("refresh_interval", -1);
			}
			set
			{
				Storage.DocumentElement.SetAttributeInt("refresh_interval", value);
			}
		}

		public string Title
		{
			get
			{
				string title = Storage.DocumentElement.GetAttributeString("title", "");
				if (title == "")
					title = Definition["title"].ValueString;
				return title;
			}
			set
			{
				Storage.DocumentElement.SetAttributeString("title", value);
			}
		}

		public string TitleForDisplay
		{
			get
			{
				// No title if is the only
				if (Engine.Instance.ProvidersManager.CountEnabled == 1)
					return "";

				string title = Title;
				if (title != "")
					return title + " - ";
				return "";
			}
		}

		public string AuthPassUsername
		{
			get
			{
				return Storage.DocumentElement.GetAttributeString("login", "");
			}
			set
			{
				Storage.DocumentElement.SetAttributeString("login", value);
			}
		}

		public string AuthPassPassword
		{
			get
			{
				return Storage.DocumentElement.GetAttributeString("password", "");
			}
			set
			{
				Storage.DocumentElement.SetAttributeString("password", value);
			}
		}

		public virtual string HashSHA256(string str)
		{
			return Crypto.Manager.HashSHA256(ID + "-" + str);
		}

		public virtual string GetKeyValue(string key, string def)
		{
			// Option at provider level. If not exists local, try to find in a manifest.

			if (Storage != null)
			{
				lock (Storage)
				{
					if (Storage.DocumentElement != null)
					{
						if (Storage.DocumentElement.Attributes[key] != null)
						{
							return Storage.DocumentElement.Attributes[key].Value;
						}

						XmlElement manifest = Storage.DocumentElement.SelectSingleNode("manifest") as XmlElement;
						if (manifest != null)
						{
							if (manifest.Attributes[key] != null)
							{
								return manifest.Attributes[key].Value;
							}
						}
					}
				}
			}

			return def;
		}
		public virtual ConnectionTypes.IConnectionType BuildConnection(ConnectionInfo info)
		{
			return null;
		}

		public virtual void OnInit()
		{
			Storage = new XmlDocument();
		}

		public virtual void OnLoad(XmlElement xmlStorage)
		{
			if (xmlStorage != null)
			{
				Storage.AppendChild(Storage.ImportNode(xmlStorage, true));
			}
			else
			{
				Storage.AppendChild(Storage.CreateElement(Code));
			}

			if (ID == "")
				ID = RandomGenerator.GetHash();
		}

		public virtual void OnCheck()
		{
		}

		public virtual void OnBuildOvpnDefaults(ConfigBuilder.OpenVPN ovpn)
		{

		}

		public virtual void OnBuildConnection(ConnectionTypes.IConnectionType connection)
		{
		}

		public virtual void OnAuthFailed()
		{
			Engine.Instance.Logs.Log(LogType.Fatal, LanguageManager.GetText(LanguageItems.AuthFailed));

			ClearCredentials();
		}

		public virtual void OnVpnEstablished(Session session)
		{

		}

		public virtual bool GetNeedRefresh()
		{
			return false;
		}

		public virtual string OnRefresh()
		{
			m_lastTryRefresh = Utils.UnixTimeStamp();
			return "";
		}

		public virtual bool OnPreFilterLog(string source, string message)
		{
			return true;
		}

		public virtual void OnBuildConnections()
		{
		}

		public virtual void OnCheckConnections()
		{
		}

		public virtual void OnChangeConnection(ConnectionInfo connection)
		{
		}

		public virtual void ClearCredentials()
		{
			m_runCredentials = null;
			Storage.DocumentElement.SetAttributeString("login", "");
			Storage.DocumentElement.SetAttributeString("password", "");
		}

		public virtual bool ApplyCredentials(ConnectionTypes.IConnectionType connectionActive) // WIP to move
		{
			if (connectionActive.NeedCredentials())
			{
				string username = "";
				string password = "";

				if (m_runCredentials != null)
				{
					username = m_runCredentials.UserName;
					password = m_runCredentials.Password;
				}

				if (username == "")
				{
					username = AuthPassUsername;
					password = AuthPassPassword;
				}

				if (username == "")
				{
					Credentials credentials;

					credentials = Engine.Instance.OnAskCredentials();
					if ((credentials == null) || (credentials.IsFilled == false))
						return false;

					username = credentials.UserName;
					password = credentials.Password;

					if (credentials.Remember == "run")
					{
						m_runCredentials = credentials;
					}
					else if (credentials.Remember == "permanent")
					{
						AuthPassUsername = username;
						AuthPassPassword = password;
					}
				}

				connectionActive.SetCredentialsUserPass(username, password);
			}

			return true;
		}

		// Used for directive auth-user-pass
		public virtual string GetUsername()
		{
			return Storage.DocumentElement.GetAttributeString("login", "");
		}

		// Used for directive auth-user-pass
		public virtual string GetPassword()
		{
			return Storage.DocumentElement.GetAttributeString("password", "");
		}

		public virtual string GetTransportSshKey(string format)
		{
			// 'format' can be 'key' or 'ppk'
			return "";
		}

		public virtual string GetTransportSslCrt()
		{
			return "";
		}
	}
}
