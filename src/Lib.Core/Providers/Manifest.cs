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
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Eddie.Core.Providers
{
	public class Service : Core.Provider
	{
		public XmlNode Manifest;
		public XmlNode User;

		public List<ConnectionMode> Modes = new List<ConnectionMode>();

		private Int64 m_lastFetchTime = 0;
		private List<string> m_frontMessages = new List<string>();

		public override void OnInit()
		{
			base.OnInit();

#if (EDDIE3)
            Engine.Instance.Storage.SetDefaultBool("providers." + GetCode() + ".dns.check", true, LanguageManager.GetText("ManOptionServicesDnsCheck"));
            Engine.Instance.Storage.SetDefaultBool("providers." + GetCode() + ".tunnel.check", true, LanguageManager.GetText("ManOptionServicesTunnelCheck"));
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
					throw new Exception(LanguageManager.GetText("ProvidersInvalid"));

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

		public override void OnBuildConnectionActive(ConnectionInfo connection, ConnectionActive connectionActive)
		{
			base.OnBuildConnectionActive(connection, connectionActive);

			OvpnBuilder ovpn = connectionActive.OpenVpnProfileStartup;
			ConnectionMode mode = GetMode();

			if (mode.Protocol == "SSH")
			{
				connectionActive.SshLocalPort = Engine.Instance.Storage.GetInt("ssh.port");
				connectionActive.SshRemotePort = mode.Port;
				connectionActive.SshPortDestination = mode.SshPortDestination;
				if (connectionActive.SshLocalPort == 0)
					connectionActive.SshLocalPort = RandomGenerator.GetInt(1024, 64 * 1024);
			}
			else if (mode.Protocol == "SSL")
			{
				connectionActive.SslLocalPort = Engine.Instance.Storage.GetInt("ssl.port");
				connectionActive.SslRemotePort = mode.Port;
				if (connectionActive.SslLocalPort == 0)
					connectionActive.SslLocalPort = RandomGenerator.GetInt(1024, 64 * 1024);
			}

			{
				string modeDirectives = mode.Directives;
				string paramUserTA = "";
				string paramUserTlsCrypt = "";
				if (User != null)
				{
					paramUserTA = User.GetAttributeString("ta", "");
					paramUserTlsCrypt = User.GetAttributeString("tls_crypt", "");
				}
				modeDirectives = modeDirectives.Replace("{@user-ta}", paramUserTA);
				modeDirectives = modeDirectives.Replace("{@user-tlscrypt}", paramUserTlsCrypt);
				ovpn.AppendDirectives(modeDirectives, "Mode level");
			}

			// Pick the IP
			IpAddress ip = null;
			string entryIpLayer = Engine.Instance.Storage.Get("network.entry.iplayer");
			if (entryIpLayer == "ipv6-ipv4")
			{
				ip = connection.IpsEntry.GetV6ByIndex(mode.EntryIndex);
				if (ip == null)
					ip = connection.IpsEntry.GetV4ByIndex(mode.EntryIndex);
			}
			else if (entryIpLayer == "ipv4-ipv6")
			{
				ip = connection.IpsEntry.GetV4ByIndex(mode.EntryIndex);
				if (ip == null)
					ip = connection.IpsEntry.GetV6ByIndex(mode.EntryIndex);
			}
			else if (entryIpLayer == "ipv6-only")
				ip = connection.IpsEntry.GetV6ByIndex(mode.EntryIndex);
			else if (entryIpLayer == "ipv4-only")
				ip = connection.IpsEntry.GetV4ByIndex(mode.EntryIndex);

			if (ip != null)
			{
				IpAddress remoteAddress = ip.Clone();
				int remotePort = mode.Port;

				if (mode.Protocol == "SSH")
				{
					remoteAddress = "127.0.0.1";
					remotePort = connectionActive.SshLocalPort;
				}
				else if (mode.Protocol == "SSL")
				{
					remoteAddress = "127.0.0.1";
					remotePort = connectionActive.SslLocalPort;
				}

				ovpn.AppendDirective("remote", remoteAddress.Address + " " + remotePort.ToString(), "");

				// Adjust the protocol
				OvpnBuilder.Directive dProto = ovpn.GetOneDirective("proto");
				if (dProto != null)
				{
					dProto.Text = dProto.Text.ToLowerInvariant();
					if (dProto.Text == "tcp")
					{
						if (remoteAddress.IsV6)
							dProto.Text = "tcp6";
					}
					else if (dProto.Text == "udp")
					{
						if (remoteAddress.IsV6)
							dProto.Text = "udp6";
					}
				}

				if ((mode.Protocol == "SSH") || (mode.Protocol == "SSL"))
				{
					if (((ip.IsV4) && (connectionActive.TunnelIPv4)) ||
						((ip.IsV6) && (connectionActive.TunnelIPv6)))
							connectionActive.AddRoute(ip, "net_gateway", "VPN Entry IP");					
				}
			}

			connectionActive.Protocol = mode.Protocol;
			if (ip != null)
				connectionActive.Address = ip.Clone();
		}

		public override void OnBuildConnectionActiveAuth(ConnectionActive connectionActive)
		{
			base.OnBuildConnectionActiveAuth(connectionActive);

			string key = Engine.Instance.Storage.Get("key");

			XmlNode nodeUser = User;
			if( (nodeUser != null) && (nodeUser.Attributes["ca"] != null) )
			{
				connectionActive.OpenVpnProfileStartup.AppendDirective("<ca>", nodeUser.Attributes["ca"].Value, "");
				XmlElement xmlKey = nodeUser.SelectSingleNode("keys/key[@name=\"" + key.Replace("\"","") + "\"]") as XmlElement;
				if (xmlKey != null)
				{
					connectionActive.OpenVpnProfileStartup.AppendDirective("<cert>", xmlKey.Attributes["crt"].Value, "");
					connectionActive.OpenVpnProfileStartup.AppendDirective("<key>", xmlKey.Attributes["key"].Value, "");
				}
			}
		}

		public override void OnAuthFailed()
		{
			Engine.Instance.Logs.Log(LogType.Warning, LanguageManager.GetText("AirVpnAuthFailed"));
		}

		public override bool GetNeedRefresh()
		{
			int minInterval = RefreshInterval;
			{
				// Temp until option migration
				minInterval = Engine.Instance.Storage.GetInt("advanced.manifest.refresh");
				if (minInterval == 0)
					return false;
				if (minInterval != -1)
					minInterval *= 60;
			}
			if ((Manifest != null) && (minInterval == -1)) // Pick server recommended
			{
				minInterval = Manifest.GetAttributeInt("next_update", -1);
				if (minInterval != -1)
					minInterval *= 60;
			}

			if (minInterval == -1)
				minInterval = 60 * 60 * 24;
			if (m_lastTryRefresh + minInterval > Utils.UnixTimeStamp())
				return false;

			return true;
		}

		public override string OnRefresh()
		{
			base.OnRefresh();

            // Engine.Instance.Logs.LogVerbose(LanguageManager.GetText("ProviderRefreshStart, Title));

			try
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["act"] = "manifest";
				parameters["ts"] = Conversions.ToString(m_lastFetchTime);
								
				XmlDocument xmlDoc = Fetch(LanguageManager.GetText("ProviderRefreshStart", Title), parameters);
				lock (Storage)
				{
					if (Manifest != null)
						Storage.DocumentElement.RemoveChild(Manifest);

					Manifest = Storage.ImportNode(xmlDoc.DocumentElement, true);
					Storage.DocumentElement.AppendChild(Manifest);

					// Update with the local time
					Manifest.Attributes["time"].Value = Utils.UnixTimeStamp().ToString();

					m_lastFetchTime = Utils.UnixTimeStamp();
				}

				Engine.Instance.Logs.LogVerbose(LanguageManager.GetText("ProviderRefreshDone", Title));

				// Show important messages
				foreach (XmlElement xmlMessage in Manifest.SelectNodes("messages/message"))
				{
                    string kind = "";
                    if (xmlMessage.HasAttribute("kind"))
                        kind = xmlMessage.GetAttribute("kind");

                    if(kind == "promo")
                    {
                        bool skipPromo = Engine.Instance.Storage.GetBool("ui.skip.promotional");
                        if (skipPromo)
                            continue;
                    }

					if ((xmlMessage.HasAttribute("from_time")) && (Utils.UnixTimeStamp() < Conversions.ToInt64(xmlMessage.GetAttribute("from_time"))))
						continue;
					if ((xmlMessage.HasAttribute("to_time")) && (Utils.UnixTimeStamp() > Conversions.ToInt64(xmlMessage.GetAttribute("to_time"))))
						continue;

					Json jMessage = new Json();
					jMessage["text"].Value = xmlMessage.GetAttribute("text");
					jMessage["url"].Value = xmlMessage.GetAttribute("url");
					jMessage["link"].Value = xmlMessage.GetAttribute("link");
					jMessage["html"].Value = xmlMessage.GetAttribute("html");

					string text = jMessage["text"].Value as string;
					if (m_frontMessages.Contains(text) == false)
					{
						Json jCommand = new Json();
						jCommand["command"].Value = "ui.frontmessage";
						jCommand["message"].Value = jMessage;
						Engine.Instance.UiManager.Broadcast(jCommand);
						m_frontMessages.Add(text);
					}
				}				

				return "";
			}
			catch (Exception e)
			{
				Engine.Instance.Logs.LogVerbose(LanguageManager.GetText("ProviderRefreshFail", Title, e.Message));

				return LanguageManager.GetText("ProviderRefreshFail", Title, e.Message);
			}
		}

		public override IpAddresses GetNetworkLockWhiteListOutgoingIPs()
		{
			IpAddresses result = base.GetNetworkLockWhiteListOutgoingIPs();

			List<string> urls = GetBootstrapUrls();
			foreach (string url in urls)
			{
				string host = Utils.HostFromUrl(url);
				if (host != "")
					result.Add(host);
			}

			return result;
		}
		
		public void Auth(XmlNode node)
		{
			lock (Storage)
			{
				if (User != null)
					Storage.DocumentElement.RemoveChild(User);

				User = Storage.ImportNode(node, true);
				Storage.DocumentElement.AppendChild(User);
								
				string key = Engine.Instance.Storage.Get("key");
				string firstKey = "";
				bool found = false;
				foreach (XmlElement xmlKey in User.SelectNodes("keys/key"))
				{
					if (key == xmlKey.GetAttribute("name"))
					{
						found = true;
						break;
					}
					if (firstKey == "")
						firstKey = xmlKey.GetAttribute("name");
				}
				if (found == false)
					Engine.Instance.Storage.Set("key", firstKey);
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
					string code = Crypto.Manager.HashSHA256(nodeServer.Attributes["name"].Value);

					string group = nodeServer.GetAttributeString("group", "");

					XmlNode nodeServerGroup = Manifest.SelectSingleNode("//servers_groups/servers_group[@group=\"" + group + "\"]");

					ConnectionInfo infoServer = Engine.Instance.GetConnectionInfo(code, this);

					// Update info
					infoServer.DisplayName = TitleForDisplay + nodeServer.Attributes["name"].Value;
					infoServer.ProviderName = nodeServer.Attributes["name"].Value;
					infoServer.IpsEntry.Set(XmlGetServerAttributeString(nodeServer, nodeServerGroup, "ips_entry", ""));
					infoServer.IpsExit.Set(XmlGetServerAttributeString(nodeServer, nodeServerGroup, "ips_exit", ""));
					infoServer.CountryCode = XmlGetServerAttributeString(nodeServer, nodeServerGroup, "country_code", "");
					infoServer.Location = XmlGetServerAttributeString(nodeServer, nodeServerGroup, "location", "");
					infoServer.ScoreBase = XmlGetServerAttributeInt64(nodeServer, nodeServerGroup, "scorebase", 0);
					infoServer.Bandwidth = XmlGetServerAttributeInt64(nodeServer, nodeServerGroup, "bw", 0);
					infoServer.BandwidthMax = XmlGetServerAttributeInt64(nodeServer, nodeServerGroup, "bw_max", 1);
					infoServer.Users = XmlGetServerAttributeInt64(nodeServer, nodeServerGroup, "users", 0);
					infoServer.WarningOpen = XmlGetServerAttributeString(nodeServer, nodeServerGroup, "warning_open", "");
					infoServer.WarningClosed = XmlGetServerAttributeString(nodeServer, nodeServerGroup, "warning_closed", "");
					infoServer.SupportIPv4 = XmlGetServerAttributeBool(nodeServer, nodeServerGroup, "support_ipv4", false);
					infoServer.SupportIPv6 = XmlGetServerAttributeBool(nodeServer, nodeServerGroup, "support_ipv6", false);
					infoServer.SupportCheck = XmlGetServerAttributeBool(nodeServer, nodeServerGroup, "support_check", false);
					infoServer.OvpnDirectives = XmlGetServerAttributeString(nodeServer, nodeServerGroup, "openvpn_directives", "");
					infoServer.CiphersTls = XmlGetServerAttributeString(nodeServer, nodeServerGroup, "ciphers_tls", "").StringToList(":");
					infoServer.CiphersTlsSuites = XmlGetServerAttributeString(nodeServer, nodeServerGroup, "ciphers_tlssuites", "").StringToList(":");
					infoServer.CiphersData = XmlGetServerAttributeString(nodeServer, nodeServerGroup, "ciphers_data", "").StringToList(":");
				}
			}

			RefreshModes();
		}

		public override void OnCheckConnections()
		{
			base.OnCheckConnections();

			ConnectionMode mode = GetMode();

			lock (Engine.Instance.Connections)
			{
				foreach (ConnectionInfo connection in Engine.Instance.Connections.Values)
				{
					if (connection.Provider != this)
						continue;

					if (User == null)
						connection.WarningAdd(LanguageManager.GetText("ConnectionWarningLoginRequired"), ConnectionInfoWarning.WarningType.Error);

					if (mode.EntryIndex >= connection.IpsEntry.CountIPv4)
						connection.WarningAdd(LanguageManager.GetText("ConnectionWarningModeUnsupported"), ConnectionInfoWarning.WarningType.Error);
				}
			}
		}

		public override string GetSshKey(string format)
		{
			return User.GetAttributeString("ssh_" + format, "");
		}

		public override string GetSslCrt()
		{
			return User.GetAttributeString("ssl_crt", "");
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

		public List<string> GetBootstrapUrls()
		{
			List<string> urls = new List<string>();

			// Manual Urls
			foreach (string url in Engine.Instance.Storage.Get("bootstrap.urls").Split(';'))
			{
				string sUrl = url.Trim();
				if (sUrl != "")
				{
					if (IpAddress.IsIP(sUrl))
						sUrl = "http://" + sUrl;
					string host = Utils.HostFromUrl(sUrl);
					if (host != "")
						urls.Add(sUrl);
				}
			}

			// Manifest Urls
			if (Manifest != null)
			{
				XmlNodeList nodesUrls = Manifest.SelectNodes("//urls/url");
				foreach (XmlNode nodeUrl in nodesUrls)
				{
					urls.Add(nodeUrl.Attributes["address"].Value);
				}
			}

			return urls;
		}

		public XmlDocument Fetch(string title, Dictionary<string, string> parameters)
		{
			List<string> urls = GetBootstrapUrls();

            string authPublicKey = Manifest.SelectSingleNode("rsa").InnerXml;

			return FetchUrls(title, authPublicKey, urls, parameters);
		}

		// This is the only method about exchange data between this software and AirVPN infrastructure.
		// We don't use SSL. Useless layer in our case, and we need to fetch hostname and direct IP that don't permit common-name match.

		// 'S' is the AES 256 bit one-time session key, crypted with a RSA 4096 public-key.
		// 'D' is the data from the client to our server, crypted with the AES.
		// The server answer is XML decrypted with the same AES session.
		public XmlDocument FetchUrl(string authPublicKey, string url, Dictionary<string, string> parameters)
		{
			// AES				
			using (RijndaelManaged rijAlg = new RijndaelManaged())
			{
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

				Dictionary<string, byte[]> assocParamS = new Dictionary<string, byte[]>();
				assocParamS["key"] = rijAlg.Key;
				assocParamS["iv"] = rijAlg.IV;

				byte[] bytesParamS = null;
				using (RSACryptoServiceProvider csp = new RSACryptoServiceProvider())
				{
					csp.ImportParameters(publicKey);
					bytesParamS = csp.Encrypt(AssocToUtf8Bytes(assocParamS), false);
				}

				// Generate D

				byte[] aesDataIn = AssocToUtf8Bytes(parameters);
				byte[] bytesParamD = null;

				{
					MemoryStream aesCryptStream = null;
					CryptoStream aesCryptStream2 = null;

					try
					{
						aesCryptStream = new MemoryStream();
						using (ICryptoTransform aesEncryptor = rijAlg.CreateEncryptor())
						{
							aesCryptStream2 = new CryptoStream(aesCryptStream, aesEncryptor, CryptoStreamMode.Write);
							aesCryptStream2.Write(aesDataIn, 0, aesDataIn.Length);
							aesCryptStream2.FlushFinalBlock();

							bytesParamD = aesCryptStream.ToArray();
						}
					}
					finally
					{
						if (aesCryptStream2 != null)
							aesCryptStream2.Dispose();
						else if (aesCryptStream != null)
							aesCryptStream.Dispose();
					}
				}
								
				// HTTP Fetch
				HttpRequest request = new HttpRequest();
				request.Url = url;
				request.Parameters["s"] = ExtensionsString.Base64Encode(bytesParamS);
				request.Parameters["d"] = ExtensionsString.Base64Encode(bytesParamD);

                HttpResponse response = Engine.Instance.FetchUrl(request);

				try
				{
					byte[] fetchResponse = response.BufferData;
					byte[] fetchResponsePlain = null;

					MemoryStream aesDecryptStream = null;
					CryptoStream aesDecryptStream2 = null;

					// Decrypt answer

					try
					{
						aesDecryptStream = new MemoryStream();
						using (ICryptoTransform aesDecryptor = rijAlg.CreateDecryptor())
						{
							aesDecryptStream2 = new CryptoStream(aesDecryptStream, aesDecryptor, CryptoStreamMode.Write);
							aesDecryptStream2.Write(fetchResponse, 0, fetchResponse.Length);
							aesDecryptStream2.FlushFinalBlock();

							fetchResponsePlain = aesDecryptStream.ToArray();
						}
					}
					finally
					{
						if (aesDecryptStream2 != null)
							aesDecryptStream2.Dispose();
						else if (aesDecryptStream != null)
							aesDecryptStream.Dispose();
					}

					string finalData = System.Text.Encoding.UTF8.GetString(fetchResponsePlain);

					XmlDocument doc = new XmlDocument();
					doc.LoadXml(finalData);
					return doc;
				}
				catch (Exception ex)
				{
					string message = "";
					if (response.GetHeader("location") != "")
						message = LanguageManager.GetText("ProviderRefreshFailUnexpected302", Title, response.GetHeader("location"));
					else
						message = ex.Message + " - " + response.GetLineReport();
					throw new Exception(message);
				}
			}
		}

		public XmlDocument FetchUrls(string title, string authPublicKey, List<string> urls, Dictionary<string, string> parameters)
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
					string proxyWhen = Engine.Instance.Storage.Get("proxy.when").ToLowerInvariant();
					string proxyAuth = Engine.Instance.Storage.Get("proxy.auth").ToLowerInvariant();
					if (proxyMode != "none")
						info += " - with '" + proxyMode + "' (" + proxyWhen + ") proxy and '" + proxyAuth + "' auth";

					if (Engine.Instance.Storage.GetBool("advanced.expert"))
						Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("ExchangeTryFailed", title, hostN.ToString(), info));

					if (firstError == "")
						firstError = info;
				}
			}

			throw new Exception(firstError);
		}

		public ConnectionMode GetModeAuto()
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

			return null;
		}

		public ConnectionMode GetMode()
		{
			String protocol = Engine.Instance.Storage.Get("mode.protocol").ToUpperInvariant();
			int port = Engine.Instance.Storage.GetInt("mode.port");
			int entry = Engine.Instance.Storage.GetInt("mode.alt");

			if (protocol == "AUTO")
			{
				return GetModeAuto();
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

			return GetModeAuto();
		}

		public static XmlNode XmlGetServerAttribute(XmlNode nodeServer, XmlNode nodeGroup, string name)
		{
			if (nodeServer == null)
				return null;
			XmlNode nodeAttr = nodeServer.Attributes[name];
			if( (nodeAttr == null) && (nodeGroup != null) )
				nodeAttr = nodeGroup.Attributes[name];
			return nodeAttr;
		}

		public static string XmlGetServerAttributeString(XmlNode nodeServer, XmlNode nodeGroup, string name, string def)
		{
			XmlNode nodeAttr = XmlGetServerAttribute(nodeServer, nodeGroup, name);
			if (nodeAttr == null)
				return def;
			else
				return nodeAttr.Value;
		}

		public static Int64 XmlGetServerAttributeInt64(XmlNode nodeServer, XmlNode nodeGroup, string name, Int64 def)
		{
			XmlNode nodeAttr = XmlGetServerAttribute(nodeServer, nodeGroup, name);
			if (nodeAttr == null)
				return def;
			else
				return Conversions.ToInt64(nodeAttr.Value);
		}

		public static bool XmlGetServerAttributeBool(XmlNode nodeServer, XmlNode nodeGroup, string name, bool def)
		{
			XmlNode nodeAttr = XmlGetServerAttribute(nodeServer, nodeGroup, name);
			if (nodeAttr == null)
				return def;
			else
				return Conversions.ToBool(nodeAttr.Value);
		}

		public static byte[] AssocToUtf8Bytes(Dictionary<string, string> assoc)
		{
			string output = "";
			foreach (KeyValuePair<string, string> kp in assoc)
			{
				output += ExtensionsString.Base64Encode(kp.Key.GetUtf8Bytes()) + ":" + ExtensionsString.Base64Encode(kp.Value.GetUtf8Bytes()) + "\n";
			}
			return System.Text.Encoding.UTF8.GetBytes(output);
		}

		public static byte[] AssocToUtf8Bytes(Dictionary<string, byte[]> assoc)
		{
			string output = "";
			foreach (KeyValuePair<string, byte[]> kp in assoc)
			{
				output += ExtensionsString.Base64Encode(kp.Key.GetUtf8Bytes()) + ":" + ExtensionsString.Base64Encode(kp.Value) + "\n";
			}
			return System.Text.Encoding.UTF8.GetBytes(output);
		}
	}
}
