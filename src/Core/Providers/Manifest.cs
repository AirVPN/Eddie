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
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Eddie.Lib.Common;

namespace Eddie.Core.Providers
{
	public class Service : Core.Provider
    {		
		public XmlNode Manifest;
		public XmlNode User;

		public List<ConnectionMode> Modes = new List<ConnectionMode>();

		private Int64 m_lastFetchTime = 0;

		public override void OnInit()
        {
            base.OnInit();

#if (EDDIE3)
            Engine.Instance.Storage.SetDefaultBool("providers." + GetCode() + ".dns.check", true, Messages.ManOptionServicesDnsCheck);
            Engine.Instance.Storage.SetDefaultBool("providers." + GetCode() + ".tunnel.check", true, Messages.ManOptionServicesTunnelCheck);
#endif
        }

        public override void OnLoad(XmlElement xmlStorage)
		{
			base.OnLoad(xmlStorage);

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

		public override void OnBuildOvpnDefaults(OvpnBuilder ovpn)
		{
			base.OnBuildOvpnDefaults(ovpn);

			ovpn.AppendDirectives(Manifest.Attributes["openvpn_directives"].Value.Replace("\t", "").Trim(), "Provider level");
		}

        public override void OnBuildOvpn(ConnectionInfo connection, OvpnBuilder ovpn)
        {
            base.OnBuildOvpn(connection, ovpn);

			ConnectionMode mode = GetMode();

            int proxyPort = 0;

            if (mode.Protocol == "SSH")
            {
                proxyPort = Engine.Instance.Storage.GetInt("ssh.port");
                if (proxyPort == 0)
                    proxyPort = RandomGenerator.GetInt(1024, 64 * 1024);
            }
            else if (mode.Protocol == "SSL")
            {
                proxyPort = Engine.Instance.Storage.GetInt("ssl.port");
                if (proxyPort == 0)
                    proxyPort = RandomGenerator.GetInt(1024, 64 * 1024);
            }
            else
            {
                proxyPort = 0;
            }

			{
				string modeDirectives = mode.Directives;
				string paramUserTA = "";
				if (User != null)
					paramUserTA = Utils.XmlGetAttributeString(User, "ta", "");
				modeDirectives = modeDirectives.Replace("{@user-ta}", paramUserTA);
				ovpn.AppendDirectives(modeDirectives, "Mode level");
			}

			// Pick the IP
			IpAddress ip = null; 
			string protocolEntry = Engine.Instance.Storage.Get("protocol.ip.entry");
			if (protocolEntry == "ipv6-ipv4")
			{
				ip = connection.IpsEntry.GetV6ByIndex(mode.EntryIndex);
				if (ip == null)
					ip = connection.IpsEntry.GetV4ByIndex(mode.EntryIndex);
			}
			else if (protocolEntry == "ipv4-ipv6")
			{
				ip = connection.IpsEntry.GetV4ByIndex(mode.EntryIndex);
				if (ip == null)
					ip = connection.IpsEntry.GetV6ByIndex(mode.EntryIndex);
			}
			else if (protocolEntry == "ipv6-only")
				ip = connection.IpsEntry.GetV6ByIndex(mode.EntryIndex);
			else if (protocolEntry == "ipv4-only")
				ip = connection.IpsEntry.GetV4ByIndex(mode.EntryIndex);

			if (ip != null)
			{
				if (mode.Protocol == "SSH")
					ovpn.AppendDirective("remote", "127.0.0.1 " + Conversions.ToString(proxyPort), "");
				else if (mode.Protocol == "SSL")
					ovpn.AppendDirective("remote", "127.0.0.1 " + Conversions.ToString(proxyPort), "");
				else
					ovpn.AppendDirective("remote", ip.AddressQ + " " + mode.Port.ToString(), "");

				string routesDefault = Engine.Instance.Storage.Get("routes.default");
				if (routesDefault == "in")
				{
					if ((mode.Protocol == "SSH") || (mode.Protocol == "SSL"))
					{
						ovpn.AppendDirective("route", ip.ToOpenVPN() + " net_gateway", "VPN Entry IP"); // ClodoIPv6 // ToFix
					}
				}
			}
            
            ovpn.Protocol = mode.Protocol; // TOCLEAN
            ovpn.Address = ip;
            ovpn.Port = mode.Port;
            ovpn.ProxyPort = proxyPort;
        }

        public override void OnBuildOvpnAuth(OvpnBuilder ovpn)
		{
			base.OnBuildOvpnAuth(ovpn);

            string key = Engine.Instance.Storage.Get("key");

            XmlNode nodeUser = User;            
			if(nodeUser != null)
			{
				ovpn.AppendDirective("<ca>", nodeUser.Attributes["ca"].Value, "");
				XmlElement xmlKey = nodeUser.SelectSingleNode("keys/key[@name='" + key + "']") as XmlElement;
				if (xmlKey == null)
					throw new Exception(MessagesFormatter.Format(Messages.KeyNotFound, key));
				ovpn.AppendDirective("<cert>", xmlKey.Attributes["crt"].Value, "");
				ovpn.AppendDirective("<key>", xmlKey.Attributes["key"].Value, "");
			}		
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

        public override void OnAuthFailed()
        {
            Engine.Instance.Logs.Log(LogType.Warning, Messages.AirVpnAuthFailed);
        }

        public override string Refresh()
		{
			base.Refresh();

			try
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["act"] = "manifest";
				parameters["ts"] = Conversions.ToString(m_lastFetchTime);

				XmlDocument xmlDoc = Fetch(Messages.ManifestUpdate, parameters);
				lock (Storage)
				{
					if(Manifest != null)
						Storage.DocumentElement.RemoveChild(Manifest);

					Manifest = Storage.ImportNode(xmlDoc.DocumentElement, true);
					Storage.DocumentElement.AppendChild(Manifest);

					// Update with the local time
					Manifest.Attributes["time"].Value = Utils.UnixTimeStamp().ToString();

					m_lastFetchTime = Utils.UnixTimeStamp();
				}

				return "";
			}
			catch (Exception e)
			{
				return MessagesFormatter.Format(Messages.ManifestFailed, e.Message);
			}
		}

        public override IpAddresses GetNetworkLockAllowedIps()
        {
			IpAddresses result = base.GetNetworkLockAllowedIps();

            // Hosts
            XmlNodeList nodesUrls = Storage.DocumentElement.SelectNodes("//urls/url");
            foreach (XmlNode nodeUrl in nodesUrls)
            {
                string url = nodeUrl.Attributes["address"].Value;
                string host = Utils.HostFromUrl(url);                
                result.Add(host);
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

		public override void OnBuildConnections()
		{
			base.OnBuildConnections();

            lock (Manifest)
			{
				foreach (XmlNode nodeServer in Manifest.SelectNodes("//servers/server"))
				{
                    string code = Utils.HashSHA256(nodeServer.Attributes["name"].Value);

					ConnectionInfo infoServer = Engine.Instance.GetConnectionInfo(code, this);

                    // Update info
                    infoServer.DisplayName = TitleForDisplay + nodeServer.Attributes["name"].Value;
                    infoServer.ProviderName = nodeServer.Attributes["name"].Value;
					infoServer.IpsEntry.Set(Utils.XmlGetAttributeString(nodeServer, "ips_entry", ""));
					infoServer.IpsExit.Set(Utils.XmlGetAttributeString(nodeServer, "ips_exit", ""));
					infoServer.CountryCode = Utils.XmlGetAttributeString(nodeServer, "country_code", "");
					infoServer.Location = Utils.XmlGetAttributeString(nodeServer, "location", "");
					infoServer.ScoreBase = Utils.XmlGetAttributeInt64(nodeServer, "scorebase", 0);
					infoServer.Bandwidth = Utils.XmlGetAttributeInt64(nodeServer, "bw", 0);
					infoServer.BandwidthMax = Utils.XmlGetAttributeInt64(nodeServer, "bw_max", 1);
					infoServer.Users = Utils.XmlGetAttributeInt64(nodeServer, "users", 0);
					infoServer.WarningOpen = Utils.XmlGetAttributeString(nodeServer, "warning_open", "");
					infoServer.WarningClosed = Utils.XmlGetAttributeString(nodeServer, "warning_closed", "");
					infoServer.SupportIPv4 = Utils.XmlGetAttributeBool(nodeServer, "support_ipv4", false);
					infoServer.SupportIPv6 = Utils.XmlGetAttributeBool(nodeServer, "support_ipv6", false);
					infoServer.SupportCheck = Utils.XmlGetAttributeBool(nodeServer, "support_check", false);
                    infoServer.OvpnDirectives = Utils.XmlGetAttributeString(nodeServer, "openvpn_directives", "");					
				}
			}

			RefreshModes();
		}

		public override void OnCheckConnections()
		{
			base.OnCheckConnections();

			ConnectionMode mode = GetMode();

			lock(Engine.Instance.Connections)
			{
				foreach(ConnectionInfo connection in Engine.Instance.Connections.Values)
				{
					if (connection.Provider != this)
						continue;

					if (User == null)
						connection.WarningAdd(Messages.ConnectionWarningLoginRequired, ConnectionInfoWarning.WarningType.Error);

					if (mode.EntryIndex >= connection.IpsEntry.CountIPv4)
						connection.WarningAdd(Messages.ConnectionWarningModeUnsupported, ConnectionInfoWarning.WarningType.Error);					
				}
			}
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

		public void RefreshModes()
		{
			// Update modes
			lock (Modes)
			{
				Modes.Clear();
				foreach (XmlNode xmlMode in Manifest.SelectNodes("//modes/mode"))
				{
					ConnectionMode mode = new ConnectionMode();
					mode.ReadXML(xmlMode as XmlElement);
					Modes.Add(mode);
				}
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

            return FetchUrls(title, authPublicKey, urls, parameters);            
		}

		// This is the only method about exchange data between this software and AirVPN infrastructure.
		// We don't use SSL. Useless layer in our case, and we need to fetch hostname and direct IP that don't permit common-name match.

		// 'S' is the AES 256 bit one-time session key, crypted with a RSA 4096 public-key.
		// 'D' is the data from the client to our server, crypted with the AES.
		// The server answer is XML decrypted with the same AES session.
		public static XmlDocument FetchUrl(string authPublicKey, string url, Dictionary<string, string> parameters)
		{
			// AES				
			RijndaelManaged rijAlg = new RijndaelManaged();
			rijAlg.KeySize = 256;
			rijAlg.GenerateKey();
			rijAlg.GenerateIV();

			// Generate S

			// Bug workaround: Xamarin 6.1.2 macOS throw an 'Default constructor not found for type System.Diagnostics.FilterElement' error.
			// in 'new System.Xml.Serialization.XmlSerializer', so i avoid that.
			/*
            StringReader sr = new System.IO.StringReader(authPublicKey);
			System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
			RSAParameters publicKey = (RSAParameters)xs.Deserialize(sr);
            */
			RSAParameters publicKey = new RSAParameters();
			XmlDocument docAuthPublicKey = new XmlDocument();
			docAuthPublicKey.LoadXml(authPublicKey);
			publicKey.Modulus = Convert.FromBase64String(docAuthPublicKey.DocumentElement["Modulus"].InnerText);
			publicKey.Exponent = Convert.FromBase64String(docAuthPublicKey.DocumentElement["Exponent"].InnerText);

			RSACryptoServiceProvider csp = new RSACryptoServiceProvider();
			csp.ImportParameters(publicKey);

			Dictionary<string, byte[]> assocParamS = new Dictionary<string, byte[]>();
			assocParamS["key"] = rijAlg.Key;
			assocParamS["iv"] = rijAlg.IV;

			byte[] bytesParamS = csp.Encrypt(Utils.AssocToUtf8Bytes(assocParamS), false);

			// Generate D

			byte[] aesDataIn = Utils.AssocToUtf8Bytes(parameters);
			MemoryStream aesCryptStream = new MemoryStream();
			ICryptoTransform aesEncryptor = rijAlg.CreateEncryptor();
			CryptoStream aesCryptStream2 = new CryptoStream(aesCryptStream, aesEncryptor, CryptoStreamMode.Write);
			aesCryptStream2.Write(aesDataIn, 0, aesDataIn.Length);
			aesCryptStream2.FlushFinalBlock();
			byte[] bytesParamD = aesCryptStream.ToArray();

			// HTTP Fetch
			System.Collections.Specialized.NameValueCollection fetchParameters = new System.Collections.Specialized.NameValueCollection();
			fetchParameters["s"] = Utils.Base64Encode(bytesParamS);
			fetchParameters["d"] = Utils.Base64Encode(bytesParamD);

			// 'GET' Edition - < 2.9			
			// string url = "http://" + host + "?s=" + Uri.EscapeUriString(Base64Encode(bytesParamS)) + "&d=" + Uri.EscapeUriString(Base64Encode(bytesParamD));
			// byte[] fetchResponse = Engine.Instance.FetchUrlEx(url, null, "", 1, Engine.Instance.IsConnected());

			// 'POST' Edition - >= 2.9			
			// Debug with an url direct to backend service client debugging page
			url = "https://airvpn.org/services/client/err.php";
			Tools.CurlResponse response = Engine.Instance.FetchUrlEx(url, fetchParameters, false, "", "");
			byte[] fetchResponse = response.Buffer;

			try
			{
				// Decrypt answer
				MemoryStream aesDecryptStream = new MemoryStream();
				ICryptoTransform aesDecryptor = rijAlg.CreateDecryptor();
				CryptoStream aesDecryptStream2 = new CryptoStream(aesDecryptStream, aesDecryptor, CryptoStreamMode.Write);
				aesDecryptStream2.Write(fetchResponse, 0, fetchResponse.Length);
				aesDecryptStream2.FlushFinalBlock();
				byte[] fetchResponsePlain = aesDecryptStream.ToArray();

				string finalData = System.Text.Encoding.UTF8.GetString(fetchResponsePlain);

				XmlDocument doc = new XmlDocument();
				doc.LoadXml(finalData);
				return doc;
			}
			catch(Exception ex)
			{
				// ClodoTemp, every fetch inside try/catch?
				string message = ex.Message + " - " + response.GetLineReport();
				throw new Exception(message);
			}
		}

		public static XmlDocument FetchUrls(string title, string authPublicKey, List<string> urls, Dictionary<string, string> parameters)
		{
			parameters["login"] = Engine.Instance.Storage.Get("login");
			parameters["password"] = Engine.Instance.Storage.Get("password");
			parameters["system"] = Platform.Instance.GetSystemCode();
			parameters["version"] = Constants.VersionInt.ToString(CultureInfo.InvariantCulture);

			string firstError = "";
			int hostN = 0;
			foreach (string url in urls)
			{
				string host = Utils.HostFromUrl(url);

				hostN++;
				if (IpAddress.IsIP(host) == false)
				{
					// If locked network are enabled, skip the hostname and try only by IP.
					// To avoid DNS issue (generally, to avoid losing time).
					if (Engine.Instance.NetworkLockManager.IsDnsResolutionAvailable(host) == false)
						continue;
				}

				try
				{
					RouteScope routeScope = new RouteScope(host);
					XmlDocument xmlDoc = FetchUrl(authPublicKey, url, parameters);
					routeScope.End();
					if (xmlDoc == null)
						throw new Exception("No answer.");

					if (xmlDoc.DocumentElement.Attributes["error"] != null)
						throw new Exception(xmlDoc.DocumentElement.Attributes["error"].Value);

					return xmlDoc;
				}
				catch (Exception e)
				{
					string info = e.Message;
					string proxyMode = Engine.Instance.Storage.Get("proxy.mode").ToLowerInvariant();
					string proxyAuth = Engine.Instance.Storage.Get("proxy.auth").ToLowerInvariant();
					if (proxyMode != "none")
						info += " - with '" + proxyMode + "' proxy and '" + proxyAuth + "' auth";

					if (Engine.Instance.Storage.GetBool("advanced.expert"))
						Engine.Instance.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.ExchangeTryFailed, title, hostN.ToString(), info));

					if (firstError == "")
						firstError = info;
				}
			}

			throw new Exception(firstError);
		}

		public ConnectionMode GetMode()
		{
			String protocol = Engine.Instance.Storage.Get("mode.protocol").ToUpperInvariant();
			int port = Engine.Instance.Storage.GetInt("mode.port");
			int entry = Engine.Instance.Storage.GetInt("mode.alt");

			if (protocol == "AUTO")
			{
				string proxyMode = Engine.Instance.Storage.GetLower("proxy.mode");

				foreach (ConnectionMode mode in Modes)
				{
					if (mode.Available == false)
						continue;
					if ((proxyMode != "none") && (mode.Protocol != "TCP"))
						continue;

					return mode;
				}				
			}
			else
			{
				foreach (ConnectionMode mode in Modes)
				{
					if ((mode.Protocol.ToLowerInvariant() == protocol.ToLowerInvariant()) &&
						(mode.Port == port) &&
						(mode.EntryIndex == entry))
						return mode;
				}
			}

			return null;
		}
    }
}
