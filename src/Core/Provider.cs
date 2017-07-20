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

namespace Eddie.Core
{
    public class Provider
    {
		public XmlElement Definition;
		public XmlDocument Storage;

		private Credentials m_runCredentials;		

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
				return Utils.XmlGetAttributeString(Definition, "code", "");
			}
		}

		public string DefinitionTitle
		{
			get
			{
				return Utils.XmlGetAttributeString(Definition, "title", "");
			}
		}

		public string DefinitionSubTitle
		{
			get
			{
				return Utils.XmlGetAttributeString(Definition, "subtitle", "");
			}
		}

		public string DefinitionHref
		{
			get
			{
				return Utils.XmlGetAttributeString(Definition, "href", "");
			}
		}

		public string ID
		{
			get
			{
				return Utils.XmlGetAttributeString(Storage.DocumentElement, "id", "");
			}
			set
			{
				Utils.XmlSetAttributeString(Storage.DocumentElement, "id", value);
			}
		}

		public bool Enabled
        {
            get
            {
                return Utils.XmlGetAttributeBool(Storage.DocumentElement, "enabled", GetEnabledByDefault());                
            }
            set
            {
                Utils.XmlSetAttributeBool(Storage.DocumentElement, "enabled", value);
            }
        }

        public string Title
        {
            get
            {
				string title = Utils.XmlGetAttributeString(Storage.DocumentElement, "title", "");
				if (title == "")
					title = Utils.XmlGetAttributeString(Definition, "title", "");
				return title;
            }
			set
			{
				Utils.XmlSetAttributeString(Storage.DocumentElement, "title", value);
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
				return Utils.XmlGetAttributeString(Storage.DocumentElement, "login", "");
			}
			set
			{
				Utils.XmlSetAttributeString(Storage.DocumentElement, "login", value);
			}
		}

		public string AuthPassPassword
		{
			get
			{
				return Utils.XmlGetAttributeString(Storage.DocumentElement, "password", "");
			}
			set
			{
				Utils.XmlSetAttributeString(Storage.DocumentElement, "password", value);
			}
		}

		public virtual string HashSHA256(string str)
        {
            return Utils.HashSHA256(ID + "-" + str);
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

        public virtual void OnBuildOvpn(ConnectionInfo connection, OvpnBuilder ovpn)
        {
        }

        public virtual void OnBuildOvpnAuth(OvpnBuilder ovpn)
		{

		}

		public virtual void OnBuildOvpnPost(ref string ovpn)
		{
			
		}

		public virtual void OnAuthFailed()
        {
            Engine.Instance.Logs.Log(LogType.Fatal, Messages.AuthFailed);

			ClearCredentials();
        }

		public virtual string Refresh()
		{
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

		public virtual bool IsLogged()
		{
			return false;
		}

		public virtual void ClearCredentials()
		{
			m_runCredentials = null;
			Utils.XmlSetAttributeString(Storage.DocumentElement, "login", "");
			Utils.XmlSetAttributeString(Storage.DocumentElement, "password", "");
		}

		public virtual bool ApplyCredentials(OvpnBuilder ovpn)
		{
			if(ovpn.ExistsDirective("auth-user-pass"))
			{
				if(ovpn.GetOneDirectiveText("auth-user-pass") == "") // If empty
				{
					ovpn.RemoveDirective("auth-user-pass");

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

					ovpn.SetAuthUserPass(username, password);
				}
			}

			return true;
		}

		// Used for directive auth-user-pass
		public virtual string GetUsername()
        {
            return Utils.XmlGetAttributeString(Storage.DocumentElement, "login", "");
        }

        // Used for directive auth-user-pass
        public virtual string GetPassword()
        {
            return Utils.XmlGetAttributeString(Storage.DocumentElement, "password", "");
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
