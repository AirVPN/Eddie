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

// Titolo
// Descrizione
// Logo?
// Modalità Manifest (url, pkcssalpippo)
// Url/Hosts di default
// Elenco protocollo+porta+ip+cipher
// Supporto al Check Tunnel
// Supporto al Check DNS

namespace Eddie.Core
{
    public class Provider
    {
		public XmlElement Definition;
		public XmlDocument Storage;

		public virtual string GetCode()
		{
			return "";
		}

		public virtual bool AllowMultipleInstance()
		{
			return false;
		}

        public virtual bool GetEnabledByDefault()
        {
            return true;
        }

		public virtual List<IpAddressRange> GetNetworkLockAllowedIps()
		{
			List<IpAddressRange> list = new List<IpAddressRange>();

			return list;
		}

		public virtual string GetFrontMessage()
		{
			return "";
		}

        public virtual bool Enabled
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
                return Utils.XmlGetAttributeString(Storage.DocumentElement, "title", GetCode());                
            }
        }

        public string TitleForDisplay
        {
            get
            {
                // No title if is the only
                if (Engine.Instance.ProvidersManager.Providers.Count == 1)
                    return "";

                string title = Title;
                if (title != "")
                    return title + " - ";
                return "";
            }
        }

        public virtual string SHA256(string str)
        {
            return Utils.SHA256(GetCode() + "-" + Title + "-" + str);
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
				Storage.AppendChild(Storage.CreateElement(GetCode()));				
			}            
		}

		public virtual void OnBuildOvpnDefaults(OvpnBuilder ovpn, string protocol)
		{

		}

		public virtual void OnBuildOvpnAuth(OvpnBuilder ovpn)
		{

		}

		public virtual void OnBuildOvpnPost(ref string ovpn)
		{
			
		}

		public virtual string Refresh()
		{
			return "";
		}

		public virtual void OnBuildServersList()
		{

		}

		public virtual bool IsLogged()
		{
			return false;
		}

        // Used for directive auth-user-pass
        public virtual string GetLogin()
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
