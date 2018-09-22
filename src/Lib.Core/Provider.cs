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
using System.Text;
using System.Xml;
using Eddie.Common;

namespace Eddie.Core
{
    public class Provider
    {
		public XmlElement Definition;
		public XmlDocument Storage;

		protected Credentials m_runCredentials;

		protected int m_lastTryRefresh = 0;

		public virtual bool GetEnabledByDefault()
        {
            return true;
        }

		public virtual IpAddresses GetNetworkLockAllowedIps()
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
				return UtilsXml.XmlGetAttributeString(Definition, "code", "");
			}
		}

		public string DefinitionTitle
		{
			get
			{
				return UtilsXml.XmlGetAttributeString(Definition, "title", "");
			}
		}

		public string DefinitionSubTitle
		{
			get
			{
				return UtilsXml.XmlGetAttributeString(Definition, "subtitle", "");
			}
		}

		public string DefinitionHref
		{
			get
			{
				return UtilsXml.XmlGetAttributeString(Definition, "href", "");
			}
		}

		public string ID
		{
			get
			{
				return UtilsXml.XmlGetAttributeString(Storage.DocumentElement, "id", "");
			}
			set
			{
				UtilsXml.XmlSetAttributeString(Storage.DocumentElement, "id", value);
			}
		}

		public bool Enabled
        {
            get
            {
                return UtilsXml.XmlGetAttributeBool(Storage.DocumentElement, "enabled", GetEnabledByDefault());                
            }
            set
            {
                UtilsXml.XmlSetAttributeBool(Storage.DocumentElement, "enabled", value);
            }
        }

		public int RefreshInterval
		{
			get
			{
				return UtilsXml.XmlGetAttributeInt(Storage.DocumentElement, "refresh_interval", -1);
			}
			set
			{
				UtilsXml.XmlSetAttributeInt(Storage.DocumentElement, "refresh_interval", -1);
			}
		}

		public string Title
        {
            get
            {
				string title = UtilsXml.XmlGetAttributeString(Storage.DocumentElement, "title", "");
				if (title == "")
					title = UtilsXml.XmlGetAttributeString(Definition, "title", "");
				return title;
            }
			set
			{
				UtilsXml.XmlSetAttributeString(Storage.DocumentElement, "title", value);
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
				return UtilsXml.XmlGetAttributeString(Storage.DocumentElement, "login", "");
			}
			set
			{
				UtilsXml.XmlSetAttributeString(Storage.DocumentElement, "login", value);
			}
		}

		public string AuthPassPassword
		{
			get
			{
				return UtilsXml.XmlGetAttributeString(Storage.DocumentElement, "password", "");
			}
			set
			{
				UtilsXml.XmlSetAttributeString(Storage.DocumentElement, "password", value);
			}
		}

		public virtual string HashSHA256(string str)
        {
            return UtilsCore.HashSHA256(ID + "-" + str);
        }

        public virtual string GetKeyValue(string key, string def)
        {
            // Option at provider level. If not exists local, try to find in a manifest.

            if (Storage != null)
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

            return def;            
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
		
		public virtual void OnBuildOvpnDefaults(OvpnBuilder ovpn)
		{

		}

        public virtual void OnBuildConnectionActive(ConnectionInfo connection, ConnectionActive connectionActive)
        {
        }

        public virtual void OnBuildConnectionActiveAuth(ConnectionActive connectionActive)
		{

		}

		public virtual void OnAuthFailed()
        {
            Engine.Instance.Logs.Log(LogType.Fatal, Messages.AuthFailed);

			ClearCredentials();
        }

		public virtual bool GetNeedRefresh()
		{
			return false;
		}

		public virtual string OnRefresh()
		{
			m_lastTryRefresh = UtilsCore.UnixTimeStamp();
			return "";
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
			UtilsXml.XmlSetAttributeString(Storage.DocumentElement, "login", "");
			UtilsXml.XmlSetAttributeString(Storage.DocumentElement, "password", "");
		}

		public virtual bool ApplyCredentials(ConnectionActive connectionActive)
		{
			if(connectionActive.OpenVpnProfileStartup.ExistsDirective("auth-user-pass"))
			{
				if(connectionActive.OpenVpnProfileStartup.GetOneDirectiveText("auth-user-pass") == "") // If empty
				{
					connectionActive.OpenVpnProfileStartup.RemoveDirective("auth-user-pass");

					string username = "";
					string password = "";

					if (m_runCredentials != null)
					{
						username = m_runCredentials.Username;
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
						if( (credentials == null) || (credentials.IsFilled == false) )
							return false;

						username = credentials.Username;
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

					connectionActive.SetAuthUserPass(username, password);
				}
			}

			return true;
		}

		// Used for directive auth-user-pass
		public virtual string GetUsername()
        {
            return UtilsXml.XmlGetAttributeString(Storage.DocumentElement, "login", "");
        }

        // Used for directive auth-user-pass
        public virtual string GetPassword()
        {
            return UtilsXml.XmlGetAttributeString(Storage.DocumentElement, "password", "");
        }

        public virtual string GetSshKey(string format)
		{
			// 'format' can be 'key' or 'ppk'
			return "";
		}

		public virtual string GetSslCrt()
		{
			return "";
		}        
    }
}
