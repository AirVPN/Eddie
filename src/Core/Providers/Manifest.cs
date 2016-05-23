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
using System.Text.RegularExpressions;
using System.Xml;

namespace Eddie.Core.Providers
{
	public class Service : Core.Provider
    {		
		public XmlNode Manifest;
		public XmlNode User;

		public override string GetCode()
		{
			return "AirVPN";
		}
        
        public override void OnInit()
        {
            base.OnInit();

#if (EDDIE3)
            Engine.Instance.Storage.SetDefaultBool("providers." + GetCode() + ".dns.check", true, Messages.ManOptionServicesDnsCheck);
            Engine.Instance.Storage.SetDefaultBool("providers." + GetCode() + ".tunnel.check", true, Messages.ManOptionServicesTunnelCheck);
#endif
        }

        public override void OnLoad()
		{
			base.OnLoad();

            CompatibilityManager.FixProviderStorage(Storage);

			Manifest = Storage.DocumentElement.SelectSingleNode("manifest");
            
			if (Manifest == null)
			{
				XmlNode nodeDefinitionDefaultManifest = Definition.SelectSingleNode("manifest");
				if (nodeDefinitionDefaultManifest == null)
					throw new Exception(Messages.ProvidersInvalid);

				Manifest = Storage.ImportNode(nodeDefinitionDefaultManifest, true);
				Storage.DocumentElement.AppendChild(Manifest);
			}

			User = Storage.DocumentElement.SelectSingleNode("user");
		}

		public override void OnBuildOvpnDefaults(OvpnBuilder ovpn, string protocol)
		{
			base.OnBuildOvpnDefaults(ovpn, protocol);

			ovpn.AppendDirectives(Manifest.Attributes["openvpn_directives"].Value.Replace("\t", "").Trim(), "Provider level");
		}

		public override void OnBuildOvpnAuth(OvpnBuilder ovpn)
		{
			base.OnBuildOvpnAuth(ovpn);

            string key = Engine.Instance.Storage.Get("key");

            XmlNode nodeUser = User;            
			ovpn.AppendDirective("<ca>", nodeUser.Attributes["ca"].Value);
			XmlElement xmlKey = nodeUser.SelectSingleNode("keys/key[@name='" + key + "']") as XmlElement;
            if (xmlKey == null)
                throw new Exception("Key not found.");
            ovpn.AppendDirective("<cert>", xmlKey.Attributes["crt"].Value);
            ovpn.AppendDirective("<key>", xmlKey.Attributes["key"].Value);            
			ovpn.AppendDirective("key-direction","1");
			ovpn.AppendDirective("<tls-auth>", nodeUser.Attributes["ta"].Value);
		}

		public override void OnBuildOvpnPost(ref string ovpn)
		{
			base.OnBuildOvpnPost(ref ovpn);

			// Custom replacement, useful to final adjustment of generated OVPN by server-side rules.
			// Never used yet, available for urgent maintenance.
			int iCustomReplaces = 0;
			for (; ; )
			{
				if (Manifest.Attributes["openvpn_replace" + iCustomReplaces.ToString() + "_pattern"] == null)
					break;

				string pattern = Manifest.Attributes["openvpn_replace" + iCustomReplaces.ToString() + "_pattern"].Value;
				string replacement = Manifest.Attributes["openvpn_replace" + iCustomReplaces.ToString() + "_replacement"].Value;

				ovpn = Regex.Replace(ovpn, pattern, replacement);

				iCustomReplaces++;
			}
		}

		public override string Refresh()
		{
			base.Refresh();

			try
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["act"] = "manifest";

				XmlDocument xmlDoc = Fetch(Messages.ManifestUpdate, parameters);
				lock (Storage)
				{
					if(Manifest != null)
						Storage.DocumentElement.RemoveChild(Manifest);

					Manifest = Storage.ImportNode(xmlDoc.DocumentElement, true);
					Storage.DocumentElement.AppendChild(Manifest);

					// Update with the local time
					Manifest.Attributes["time"].Value = Utils.UnixTimeStamp().ToString();
				}

				return "";
			}
			catch (Exception e)
			{
				return Messages.Format(Messages.ManifestFailed, e.Message);
			}
		}

        public override List<IpAddressRange> GetNetworkLockAllowedIps()
        {
            List<IpAddressRange> result = base.GetNetworkLockAllowedIps();

            // Hosts
            XmlNodeList nodesUrls = Storage.DocumentElement.SelectNodes("//urls/url");
            foreach (XmlNode nodeUrl in nodesUrls)
            {
                string url = nodeUrl.Attributes["address"].Value;
                string host = Utils.HostFromUrl(url);                
                IpAddressRange ip = new IpAddressRange(host); // TOFIX: Presume it's an IP address, but can be hostname.
                result.Add(ip);
            }

            return result;
        }

        public override string GetFrontMessage()
        {
            if (Manifest.Attributes["front_message"] != null)
            {
                string msg = Manifest.Attributes["front_message"].Value;
                return msg;
            }

            return base.GetFrontMessage();
        }

        public void Auth(XmlNode node)
		{
			lock (Storage)
			{
				if (User != null)
					Storage.DocumentElement.RemoveChild(User);

				User = Storage.ImportNode(node, true);
				Storage.DocumentElement.AppendChild(User);
			}
		}

		public void DeAuth()
		{
			lock (Storage)
			{
				if (User != null)
				{
					Storage.DocumentElement.RemoveChild(User);
					User = null;
				}

			}
		}

		public override void OnBuildServersList()
		{
			base.OnBuildServersList();

            /*
			if (IsLogged() == false)
				return;
            */

			lock (Manifest)
			{
				foreach (XmlNode nodeServer in Manifest.SelectNodes("//servers/server"))
				{
					string name = nodeServer.Attributes["name"].Value;

					ServerInfo infoServer = Engine.Instance.GetServerInfo(name, this);
					
					// Update info
					infoServer.IpEntry = Utils.XmlGetAttributeString(nodeServer, "ip_entry", ""); ;
					infoServer.IpEntry2 = Utils.XmlGetAttributeString(nodeServer, "ip_entry2", "");
					infoServer.IpExit = Utils.XmlGetAttributeString(nodeServer, "ip_exit", "");
					infoServer.CountryCode = Utils.XmlGetAttributeString(nodeServer, "country_code", "");
					infoServer.Location = Utils.XmlGetAttributeString(nodeServer, "location", "");
					infoServer.ScoreBase = Utils.XmlGetAttributeInt64(nodeServer, "scorebase", 0);
					infoServer.Bandwidth = Utils.XmlGetAttributeInt64(nodeServer, "bw", 0);
					infoServer.BandwidthMax = Utils.XmlGetAttributeInt64(nodeServer, "bw_max", 1);
					infoServer.Users = Utils.XmlGetAttributeInt64(nodeServer, "users", 0);
					infoServer.WarningOpen = Utils.XmlGetAttributeString(nodeServer, "warning_open", "");
					infoServer.WarningClosed = Utils.XmlGetAttributeString(nodeServer, "warning_closed", "");
                    infoServer.OvpnDirectives = Utils.XmlGetAttributeString(nodeServer, "openvpn_directives", "");                    
                }
			}
		}

		public override bool IsLogged()
		{
			return ((User != null) && (User.Attributes["login"] != null));			
		}

		public override string GetSshKey(string format)
		{
			return Utils.XmlGetAttributeString(User, "ssh_" + format, "");
		}

		public override string GetSslCrt()
		{
			return Utils.XmlGetAttributeString(User, "ssl_crt", "");
		}

        public bool SupportConnect
        {
            get
            {
                return true;
            }
        }

        public bool CheckDns
        {
            get
            {
#if (EDDIE3)
                return Engine.Instance.Storage.GetBool("providers." + GetCode() + ".dns.check");
#else
                return Engine.Instance.Storage.GetBool("dns.check");
#endif
            }
        }

        public bool CheckTunnel
        {
            get
            {
#if (EDDIE3)
                return Engine.Instance.Storage.GetBool("providers." + GetCode() + ".tunnel.check");
#else
                return Engine.Instance.Storage.GetBool("advanced.check.route");
#endif
            }
        }

        public XmlDocument Fetch(string title, Dictionary<string, string> parameters)
		{
			List<string> urls = new List<string>();

			if (Manifest != null)
			{
				XmlNodeList nodesUrls = Manifest.SelectNodes("//urls/url");
				foreach (XmlNode nodeUrl in nodesUrls)
				{
					urls.Add(nodeUrl.Attributes["address"].Value);
				}
			}
            
			string authPublicKey = Manifest.SelectSingleNode("rsa").InnerXml;

            return AirExchange.FetchUrls(title, authPublicKey, urls, parameters);            
		}

        public string GetManifestKeyValue(string key, string def)
        {
            if (Manifest == null)
                return def;
            if (Manifest.Attributes[key] == null)
                return def;
            return Manifest.Attributes[key].Value;
        }
    }
}
