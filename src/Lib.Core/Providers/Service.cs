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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml;

namespace Eddie.Core.Providers
{
	public class Service : IProvider
	{
		public XmlNode Manifest;
		public XmlNode User;

		public List<ConnectionMode> Modes = new List<ConnectionMode>();

		private Int64 m_lastFetchTime = 0;
		private List<string> m_frontMessages = new List<string>();

		public override ConnectionTypes.IConnectionType BuildConnection(ConnectionInfo info)
		{
			ConnectionMode mode = GetMode();

			ConnectionTypes.IConnectionType c = null;
			if (mode.Type == "wireguard")
				c = new ConnectionTypes.WireGuard();
			else
			{
				if (Engine.Instance.GetOpenVpnTool() is Tools.Hummingbird)
					c = new ConnectionTypes.Hummingbird();
				else
					c = new ConnectionTypes.OpenVPN();
			}

			return c;
		}

		public override void OnInit()
		{
			base.OnInit();

#if (EDDIE3)
            Engine.Instance.Storage.SetDefaultBool("providers." + GetCode() + ".dns.check", true, LanguageManager.GetText(LanguageItems.ManOptionServicesDnsCheck));
            Engine.Instance.Storage.SetDefaultBool("providers." + GetCode() + ".tunnel.check", true, LanguageManager.GetText(LanguageItems.ManOptionServicesTunnelCheck));
#endif
		}

		public override void OnLoad(XmlElement xmlStorage)
		{
			base.OnLoad(xmlStorage);

			CompatibilityManager.FixProviderStorage(Storage);

			Manifest = Storage.DocumentElement.SelectSingleNode("manifest");

			if (Manifest == null)
			{
				Manifest = Storage.ImportNode(ExtensionsXml.XmlFromJson(Definition["manifest"].Json, "manifest"), true);

				Storage.DocumentElement.AppendChild(Manifest);
			}

			User = Storage.DocumentElement.SelectSingleNode("user");
		}

		public override void OnCheck()
		{
			base.OnCheck();

			if (User == null)
				return;

			if (User.Attributes["login"] == null)
				return;

			long ts = User.GetAttributeInt64("ts", 0);

			if (ts < 1685348074) // AirVPN, date of migration of certs SHA1 to SHA512
			{
				Engine.Instance.ReAuth();
			}
		}

		public override void OnBuildOvpnDefaults(ConfigBuilder.OpenVPN ovpn)
		{
			base.OnBuildOvpnDefaults(ovpn);

			ovpn.AppendDirectives(Manifest.Attributes["openvpn_directives"].Value.Replace("\t", "").Trim(), "Provider level");
		}

		public override void OnBuildConnection(ConnectionTypes.IConnectionType connection)
		{
			base.OnBuildConnection(connection);

			ConnectionMode mode = GetMode();

			// Pick the IP
			IpAddress ip = null;
			string entryIpLayer = Engine.Instance.ProfileOptions.Get("network.entry.iplayer");
			if (entryIpLayer == "ipv6-ipv4")
			{
				ip = connection.Info.IpsEntry.GetV6ByIndex(mode.EntryIndex);
				if (ip == null)
					ip = connection.Info.IpsEntry.GetV4ByIndex(mode.EntryIndex);
			}
			else if (entryIpLayer == "ipv4-ipv6")
			{
				ip = connection.Info.IpsEntry.GetV4ByIndex(mode.EntryIndex);
				if (ip == null)
					ip = connection.Info.IpsEntry.GetV6ByIndex(mode.EntryIndex);
			}
			else if (entryIpLayer == "ipv6-only")
				ip = connection.Info.IpsEntry.GetV6ByIndex(mode.EntryIndex);
			else if (entryIpLayer == "ipv4-only")
				ip = connection.Info.IpsEntry.GetV4ByIndex(mode.EntryIndex);

			if (ip == null)
			{
				// This occur only with debug/development servers, never with production servers.
				throw new Exception("IP-Entry not available");
			}

			if (connection is ConnectionTypes.OpenVPN)
			{
				ConnectionTypes.OpenVPN connectionOpenVPN = connection as ConnectionTypes.OpenVPN;
				ConfigBuilder.OpenVPN config = connectionOpenVPN.ConfigStartup;

				if (mode.Transport == "SSH")
				{
					connectionOpenVPN.TransportSshPortDestination = mode.SshPortDestination;
					connectionOpenVPN.TransportSshLocalPort = Engine.Instance.ProfileOptions.GetInt("ssh.port");
					if (connectionOpenVPN.TransportSshLocalPort == 0)
						connectionOpenVPN.TransportSshLocalPort = RandomGenerator.GetInt(1024, 64 * 1024);
				}
				else if (mode.Transport == "SSL")
				{
					connectionOpenVPN.TransportSslLocalPort = Engine.Instance.ProfileOptions.GetInt("ssl.port");
					if (connectionOpenVPN.TransportSslLocalPort == 0)
						connectionOpenVPN.TransportSslLocalPort = RandomGenerator.GetInt(1024, 64 * 1024);
				}

				{
					string modeDirectives = mode.OpenVpnDirectives;
					string paramUserTA = "";
					string paramUserTlsCrypt = "";
					if (User != null)
					{
						paramUserTA = User.GetAttributeString("ta", "");
						paramUserTlsCrypt = User.GetAttributeString("tls_crypt", "");
					}
					modeDirectives = modeDirectives.Replace("{@user-ta}", paramUserTA);
					modeDirectives = modeDirectives.Replace("{@user-tlscrypt}", paramUserTlsCrypt);
					config.AppendDirectives(modeDirectives, "Mode level");
				}

				IpAddress remoteAddress = ip.Clone();
				int remotePort = mode.Port;

				if (mode.Transport == "SSH")
				{
					remoteAddress = "127.0.0.1";
					remotePort = connectionOpenVPN.TransportSshLocalPort;
				}
				else if (mode.Transport == "SSL")
				{
					remoteAddress = "127.0.0.1";
					remotePort = connectionOpenVPN.TransportSslLocalPort;
				}

				config.AppendDirective("remote", remoteAddress.Address + " " + remotePort.ToString(), "");

				// Adjust the protocol
				ConfigBuilder.OpenVPN.Directive dProto = config.GetOneDirective("proto");
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

				connectionOpenVPN.Transport = mode.Transport;
			}
			else if (connection is ConnectionTypes.WireGuard)
			{
				ConnectionTypes.WireGuard connectionWireGuard = connection as ConnectionTypes.WireGuard;
				ConfigBuilder.WireGuard config = connectionWireGuard.ConfigStartup;

				config.PeerEndpointAddress = ip.ToString();
				config.PeerEndpointPort = mode.Port;
			}
			else
				throw new Exception("Unexpected connection type");

			connection.EntryIP = ip.Clone();
			connection.EntryPort = mode.Port;

			if ((mode.Transport == "SSH") || (mode.Transport == "SSL"))
				connection.RouteEntryIp = true; // Entry must routed, because OpenVPN can't route it (see 127.0.0.1)

			// Auth
			string key = Engine.Instance.ProfileOptions.Get("key");

			if (connection.IsPreviewMode() == false)
			{
				// Compatibility: if are logged before the WireGuard activation, don't have keys, reauth to obtain.
				if (connection is ConnectionTypes.WireGuard)
				{
					if (User.GetAttributeString("wg_public_key", "") == "")
					{
						Engine.Instance.ReAuth();
					}
				}
			}

			XmlNode nodeUser = User;
			if ((nodeUser != null) && (nodeUser.Attributes["ca"] != null))
			{
				XmlElement xmlKey = nodeUser.SelectSingleNode("keys/key[@name=\"" + key.Replace("\"", "") + "\"]") as XmlElement;
				if (xmlKey != null)
				{
					if (connection is ConnectionTypes.OpenVPN)
					{
						ConnectionTypes.OpenVPN connectionOpenVPN = connection as ConnectionTypes.OpenVPN;

						connectionOpenVPN.ConfigStartup.AppendDirective("<ca>", nodeUser.Attributes["ca"].Value, "");
						connectionOpenVPN.ConfigStartup.AppendDirective("<cert>", xmlKey.Attributes["crt"].Value, "");
						connectionOpenVPN.ConfigStartup.AppendDirective("<key>", xmlKey.Attributes["key"].Value, "");

					}
					else if (connection is ConnectionTypes.WireGuard)
					{
						ConnectionTypes.WireGuard connectionWireGuard = connection as ConnectionTypes.WireGuard;

						connectionWireGuard.ConfigStartup.InterfacePrivateKey = xmlKey.GetAttributeString("wg_private_key", "");
						connectionWireGuard.ConfigStartup.InterfaceAddresses.Add(xmlKey.GetAttributeString("wg_ipv4", ""));
						connectionWireGuard.ConfigStartup.InterfaceAddresses.Add(xmlKey.GetAttributeString("wg_ipv6", ""));
						connectionWireGuard.ConfigStartup.InterfaceDns.Add(xmlKey.GetAttributeString("wg_dns_ipv4", ""));
						connectionWireGuard.ConfigStartup.InterfaceDns.Add(xmlKey.GetAttributeString("wg_dns_ipv6", ""));
						connectionWireGuard.ConfigStartup.PeerPublicKey = nodeUser.GetAttributeString("wg_public_key", "");
						connectionWireGuard.ConfigStartup.PeerPresharedKey = xmlKey.GetAttributeString("wg_preshared", "");
						connectionWireGuard.ConfigStartup.PeerAllowedIPs.Add("0.0.0.0/0");
						connectionWireGuard.ConfigStartup.PeerAllowedIPs.Add("::/0");
					}
					else
						throw new Exception("Unexpected connection type");
				}
			}
		}

		public override void OnAuthFailed()
		{
			Engine.Instance.Logs.Log(LogType.Warning, LanguageManager.GetText(LanguageItems.AirVpnAuthFailed));
		}

		public override void OnVpnEstablished(Session session)
		{
			base.OnVpnEstablished(session);

			// Remember: when OpenVPN or WireGuard say 'connected', at least under Windows it's not really true, the following code reach try 2.
			int nTry = Engine.Instance.ProfileOptions.GetInt("checking.ntry");

			if (session.Connection.Info.SupportCheck == false)
			{
				Engine.Instance.Logs.Log(LogType.Warning, LanguageManager.GetText(LanguageItems.ConnectionCheckingRouteNotAvailable));
			}
			else
			{
				if ((session.InReset == false) && (CheckTunnel))
				{
					if ((session.InReset == false) && (session.Connection.ConfigIPv4) && (session.Connection.Info.SupportIPv4))
					{
						bool ok = false;
						string lastError = "";
						Engine.Instance.WaitMessageSet(LanguageManager.GetText(LanguageItems.ConnectionCheckingRouteIPv4), true);
						Engine.Instance.Logs.Log(LogType.Info, LanguageManager.GetText(LanguageItems.ConnectionCheckingRouteIPv4));
						for (int t = 0; t < nTry; t++)
						{
							if (session.InReset)
								break;

							System.Threading.Thread.Sleep(t * 1000);

							if (t > 2)
								Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText(LanguageItems.ConnectionCheckingTryRoute, (t + 1).ToString()));

							try
							{
								string checkProtocol = GetKeyValue("check_protocol", "https"); // 2.21.2
								string checkDomain = session.Connection.Info.ProviderName.ToLowerInvariant() + "_exit." + GetKeyValue("check_domain", "");
								string checkUrl = checkProtocol + "://" + checkDomain + "/check/tun/";

								HttpRequest httpRequest = new HttpRequest();
								httpRequest.Url = checkUrl;
								httpRequest.BypassProxy = true;
								httpRequest.IpLayer = "4";
								httpRequest.ForceResolve = checkDomain + ":" + session.Connection.Info.IpsExit.OnlyIPv4.First.Address;

								string ip = "";
								string ts = "";
								Json j = Engine.Instance.FetchUrlJson(httpRequest);
								if ((j != null) && (j.HasKey("ip")))
								{
									ip = j["ip"].ValueString;
									ts = j["ts"].ValueString;
								}

								if (session.Connection.GetVpnIPs().OnlyIPv4.ContainsAddress(ip) == false)
									throw new Exception(LanguageManager.GetText(LanguageItems.ConnectionCheckingTryRouteFail, ip));

								session.Connection.TimeServer = Conversions.ToInt64(ts);
								session.Connection.TimeClient = Utils.UnixTimeStamp();

								ok = true;
								lastError = "";
								break;
							}
							catch (Exception ex)
							{
								lastError = ex.Message;
							}
						}

						if ((session.InReset == false) && (ok == false))
						{
							Engine.Instance.Logs.Log(LogType.Error, LanguageManager.GetText(LanguageItems.ConnectionCheckingRouteIPv4Failed, lastError));
							session.SetReset("ERROR");
						}
					}

					if ((session.InReset == false) && (session.Connection.ConfigIPv6) && (session.Connection.Info.SupportIPv6))
					{
						bool ok = false;
						string lastError = "";
						Engine.Instance.WaitMessageSet(LanguageManager.GetText(LanguageItems.ConnectionCheckingRouteIPv6), true);
						Engine.Instance.Logs.Log(LogType.Info, LanguageManager.GetText(LanguageItems.ConnectionCheckingRouteIPv6));
						for (int t = 0; t < nTry; t++)
						{
							if (session.InReset)
								break;

							System.Threading.Thread.Sleep(t * 1000);

							if (t > 2)
								Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText(LanguageItems.ConnectionCheckingTryRoute, (t + 1).ToString()));

							try
							{
								string checkProtocol = GetKeyValue("check_protocol", "https"); // 2.21.2
								string checkDomain = session.Connection.Info.ProviderName.ToLowerInvariant() + "_exit." + GetKeyValue("check_domain", "");
								string checkUrl = checkProtocol + "://" + checkDomain + "/check/tun/";

								HttpRequest httpRequest = new HttpRequest();
								httpRequest.Url = checkUrl;
								httpRequest.BypassProxy = true;
								httpRequest.IpLayer = "6";
								httpRequest.ForceResolve = checkDomain + ":" + session.Connection.Info.IpsExit.OnlyIPv6.First.Address;

								string ip = "";
								string ts = "";
								Json j = Engine.Instance.FetchUrlJson(httpRequest);
								if ((j != null) && (j.HasKey("ip")))
								{
									ip = j["ip"].ValueString;
									ts = j["ts"].ValueString;
								}

								if (session.Connection.GetVpnIPs().OnlyIPv6.ContainsAddress(ip) == false)
									throw new Exception(LanguageManager.GetText(LanguageItems.ConnectionCheckingTryRouteFail, ip));

								session.Connection.TimeServer = Conversions.ToInt64(ts);
								session.Connection.TimeClient = Utils.UnixTimeStamp();

								ok = true;
								lastError = "";
								break;
							}
							catch (Exception ex)
							{
								lastError = ex.Message;
							}
						}

						if ((session.InReset == false) && (ok == false))
						{
							Engine.Instance.Logs.Log(LogType.Error, LanguageManager.GetText(LanguageItems.ConnectionCheckingRouteIPv6Failed, lastError));
							session.SetReset("ERROR");
						}
					}

					if (session.InReset == false)
					{
						// Real IP are detected with a request over the server entry IP.
						// Normally this is routed outside the tunnel.
						// But if a proxy is active, don't work.
						if (session.Connection.GetProxyMode())
						{
							session.Connection.RealIp = LanguageManager.GetText(LanguageItems.NotAvailable);
							session.Connection.TimeServer = 0;
						}
						else
						{
							try
							{
								string checkProtocol = GetKeyValue("check_protocol", "https"); // 2.21.2
								string checkDomain = session.Connection.Info.ProviderName.ToLowerInvariant() + "." + GetKeyValue("check_domain", "");
								string checkUrl = checkProtocol + "://" + checkDomain + "/check/tun/";
								HttpRequest httpRequest = new HttpRequest();
								httpRequest.Url = checkUrl;
								httpRequest.BypassProxy = true;
								if (session.Connection.EntryIP.IsV4)
									httpRequest.IpLayer = "4";
								else
									httpRequest.IpLayer = "6";
								httpRequest.ForceResolve = checkDomain + ":" + session.Connection.EntryIP;

								string ip = "";
								string ts = "";
								Json j = Engine.Instance.FetchUrlJson(httpRequest);
								if ((j != null) && (j.HasKey("ts")))
								{
									ip = j["ip"].ValueString;
									ts = j["ts"].ValueString;
								}

								session.Connection.TimeServer = Conversions.ToInt64(ts);
								session.Connection.TimeClient = Utils.UnixTimeStamp();
								session.Connection.RealIp = ip;
							}
							catch (Exception ex)
							{
								// Sometime hit under Windows with WireGuard.
								// Don't log exception, are data not important.
								// Engine.Instance.Logs.Log(ex);
								string m = ex.Message;

								session.Connection.RealIp = LanguageManager.GetText(LanguageItems.NotAvailable);
								session.Connection.TimeServer = 0;
							}
						}
					}
				}
				else
				{
					session.Connection.RealIp = "";
					session.Connection.TimeServer = 0;
				}

				// DNS test
				if ((session.InReset == false) && (CheckDns) && (Engine.Instance.ProfileOptions.Get("dns.servers") == ""))
				{
					Engine.Instance.WaitMessageSet(LanguageManager.GetText(LanguageItems.ConnectionCheckingDNS), true);
					Engine.Instance.Logs.Log(LogType.Info, LanguageManager.GetText(LanguageItems.ConnectionCheckingDNS));

					bool ok = false;
					string lastError = "";

					for (int t = 0; t < nTry; t++)
					{
						if (session.InReset)
							break;

						System.Threading.Thread.Sleep(t * 1000);

						if (t > 2)
							Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText(LanguageItems.ConnectionCheckingTryDNS, (t + 1).ToString()));

						try
						{
							// Don't use a real hash, it's too long.
							string hash = RandomGenerator.GetRandomToken();

							// Query a inexistent domain with the hash
							string dnsQuery = GetKeyValue("check_dns_query", "");
							string dnsHost = dnsQuery.Replace("{hash}", hash);
							IpAddresses result = DnsManager.ResolveDNS(dnsHost, true);

							// Note1: AirVPN generation 1 servers don't provide any answer. 
							// So we check SupportIPv6 for generation 2.
							// When all AirVPN servers become gen2, assert(result.Count != 0)
							// Note2: Sometime (with pull-filter DNS6 for example) the server receive the request 
							//        but the DNS fail with WSANO_RECOVERY under Windows.
							//        So result is empty, but CheckDNS below works (parallel DNS execution?)
							//if(result.Count == 0)
							//if ((Engine.CurrentServer.SupportIPv6) && (result.Count == 0))
							//	throw new Exception(LanguageManager.GetText(LanguageItems.ConnectionCheckingTryDNSFail, "No DNS answer"));								

							// Check if the server has received the above DNS query
							string checkProtocol = GetKeyValue("check_protocol", "https"); // 2.21.2
							string checkDomain = session.Connection.Info.ProviderName.ToLowerInvariant() + "_exit." + GetKeyValue("check_domain", "");
							string checkUrl = checkProtocol + "://" + checkDomain + "/check/dns/";
							HttpRequest httpRequest = new HttpRequest();
							httpRequest.Url = checkUrl;
							httpRequest.BypassProxy = true;
							if (result.CountIPv6 != 0) // Note: Use the same IP layer of the dns-result
							{
								httpRequest.IpLayer = "6";
								httpRequest.ForceResolve = checkDomain + ":" + session.Connection.Info.IpsExit.OnlyIPv6.First;
							}
							else
							{
								httpRequest.IpLayer = "4";
								httpRequest.ForceResolve = checkDomain + ":" + session.Connection.Info.IpsExit.OnlyIPv4.First;
							}

							string answer = "";
							Json j = Engine.Instance.FetchUrlJson(httpRequest);
							if ((j != null) && (j.HasKey("dns")))
								answer = j["dns"].ValueString;

							if (hash != answer)
								throw new Exception(LanguageManager.GetText(LanguageItems.ConnectionCheckingTryDNSFail, answer));

							ok = true;
							lastError = "";
							break;
						}
						catch (Exception ex)
						{
							lastError = ex.Message;
						}
					}

					if ((session.InReset == false) && (ok == false))
					{
						Engine.Instance.Logs.Log(LogType.Error, LanguageManager.GetText(LanguageItems.ConnectionCheckingDNSFailed, lastError));
						session.SetReset("ERROR");
					}
				}
			}
		}

		public override bool GetNeedRefresh()
		{
			int minInterval = RefreshInterval;
			{
				// Temp until option migration
				minInterval = Engine.Instance.ProfileOptions.GetInt("advanced.manifest.refresh");
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

			// Engine.Instance.Logs.LogVerbose(LanguageManager.GetText(LanguageItems.ProviderRefreshStart, Title));

			try
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["act"] = "manifest";
				parameters["ts"] = Conversions.ToString(m_lastFetchTime);

				XmlDocument xmlDoc = Fetch(LanguageManager.GetText(LanguageItems.ProviderRefreshStart, Title), parameters);
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

				Engine.Instance.Logs.LogVerbose(LanguageManager.GetText(LanguageItems.ProviderRefreshDone, Title));

				// Show important messages
				foreach (XmlElement xmlMessage in Manifest.SelectNodes("messages/message"))
				{
					string kind = "";
					if (xmlMessage.HasAttribute("kind"))
						kind = xmlMessage.GetAttribute("kind");

					if (kind == "promo")
					{
						bool skipPromo = Engine.Instance.ProfileOptions.GetBool("ui.skip.promotional");
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
			catch (Exception ex)
			{
				Engine.Instance.Logs.LogVerbose(LanguageManager.GetText(LanguageItems.ProviderRefreshFail, Title, ex.Message));

				return LanguageManager.GetText(LanguageItems.ProviderRefreshFail, Title, ex.Message);
			}
		}

		public override bool OnPreFilterLog(string source, string message)
		{
			RegexOptions regexOptions = RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline;

			XmlNodeList xmlRules = Manifest.SelectNodes("logs_skip/rule");
			foreach (XmlElement xmlRule in xmlRules)
			{
				string regexSource = xmlRule.GetAttributeString("source", "");
				Match matchSource = Regex.Match(source, regexSource, regexOptions);
				if (matchSource.Success)
				{
					string regexMessage = xmlRule.GetAttributeString("message", "");
					Match matchMessage = Regex.Match(message, regexMessage, regexOptions);
					if (matchMessage.Success)
					{
						return false;
					}
				}
			}

			return true;
		}
		public override IpAddresses GetNetworkLockAllowlistOutgoingIPs()
		{
			IpAddresses result = base.GetNetworkLockAllowlistOutgoingIPs();

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

				string key = Engine.Instance.ProfileOptions.Get("key");
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
					Engine.Instance.ProfileOptions.Set("key", firstKey);
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
					infoServer.UsersMax = XmlGetServerAttributeInt64(nodeServer, nodeServerGroup, "users_max", 100);
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
						connection.WarningAdd(LanguageManager.GetText(LanguageItems.ConnectionWarningLoginRequired), ConnectionInfoWarning.WarningType.Error);

					if (mode.EntryIndex >= connection.IpsEntry.CountIPv4)
						connection.WarningAdd(LanguageManager.GetText(LanguageItems.ConnectionWarningModeUnsupported), ConnectionInfoWarning.WarningType.Error);
				}
			}
		}

		public override string GetTransportSshKey(string format)
		{
			return User.GetAttributeString("ssh_" + format, "");
		}

		public override string GetTransportSslCrt()
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
                return Engine.Instance.Options.GetBool("providers." + GetCode() + ".dns.check");
#else
				return Engine.Instance.ProfileOptions.GetBool("dns.check");
#endif
			}
		}

		public bool CheckTunnel
		{
			get
			{
#if (EDDIE3)
                return Engine.Instance.Options.GetBool("providers." + GetCode() + ".tunnel.check");
#else
				return Engine.Instance.ProfileOptions.GetBool("advanced.check.route");
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

			// Manual URLs
			foreach (string url in Engine.Instance.ProfileOptions.Get("bootstrap.urls").Split(';'))
			{
				string sUrl = url.Trim();
				if (sUrl != "")
				{
					IpAddress ip = new IpAddress(sUrl);
					if (ip.IsV4)
						sUrl = "http://" + ip.ToString();
					else
						sUrl = "http://[" + ip.ToString() + "]";

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

			return FetchUrls(title, urls, parameters);
		}

		// This is the only method about exchange data between this software and AirVPN infrastructure.
		// We don't use SSL. Useless layer in our case, and we need to fetch hostname and direct IP that don't permit common-name match.

		// 'S' is the AES 256 bit one-time session key, crypted with a RSA 4096 public-key.
		// 'D' is the data from the client to our server, crypted with the AES.
		// The server answer is XML decrypted with the same AES session.
		public XmlDocument FetchUrl(string url, Dictionary<string, string> parameters)
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

				string rsaModulus = "";
				string rsaExponent = "";

				if (Manifest.GetAttributeString("auth_rsa_modulus", "") != "")
				{
					rsaModulus = Manifest.GetAttributeString("auth_rsa_modulus", "");
					rsaExponent = Manifest.GetAttributeString("auth_rsa_exponent", "");
				}
				else // Compatibility <2.21.0
				{
					rsaModulus = Manifest.SelectSingleNode("rsa/RSAParameters/Modulus").InnerText;
					rsaExponent = Manifest.SelectSingleNode("rsa/RSAParameters/Exponent").InnerText;
				}

				RSAParameters publicKey = new RSAParameters();

				publicKey.Modulus = Convert.FromBase64String(rsaModulus);
				publicKey.Exponent = Convert.FromBase64String(rsaExponent);

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
					if (response.HttpCode != 200)
						throw new Exception("Invalid HTTP code " + response.HttpCode.ToString());

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
						message = LanguageManager.GetText(LanguageItems.ProviderRefreshFailUnexpected302, Title, response.GetHeader("location"));
					else
						message = ex.Message + " - " + response.GetLineReport();
					throw new Exception(message);
				}
			}
		}

		public XmlDocument FetchUrls(string title, List<string> urls, Dictionary<string, string> parameters)
		{
			parameters["login"] = Engine.Instance.ProfileOptions.Get("login");
			parameters["password"] = Engine.Instance.ProfileOptions.Get("password");
			parameters["software"] = "EddieDesktop_" + Constants.VersionDesc; // >=2.23.0
			parameters["arch"] = Platform.Instance.GetOsArchitecture(); // >=2.23.0
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
					XmlDocument xmlDoc = FetchUrl(url, parameters) ?? throw new Exception("No answer.");
					if (xmlDoc.DocumentElement.Attributes["error"] != null)
						throw new Exception(xmlDoc.DocumentElement.Attributes["error"].Value);

					return xmlDoc;
				}
				catch (Exception ex)
				{
					string info = ex.Message;
					string proxyMode = Engine.Instance.ProfileOptions.GetLower("proxy.mode");
					string proxyWhen = Engine.Instance.ProfileOptions.GetLower("proxy.when");
					string proxyAuth = Engine.Instance.ProfileOptions.GetLower("proxy.auth");
					if (proxyMode != "none")
						info += " - with '" + proxyMode + "' (" + proxyWhen + ") proxy and '" + proxyAuth + "' auth";

					if (Engine.Instance.ProfileOptions.GetBool("advanced.expert"))
						Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText(LanguageItems.ExchangeTryFailed, title, hostN.ToString(), info));

					if (firstError == "")
						firstError = info;
				}
			}

			throw new Exception(firstError);
		}

		public ConnectionMode GetModeAuto()
		{
			string proxyMode = Engine.Instance.ProfileOptions.GetLower("proxy.mode");

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
			string type = Engine.Instance.ProfileOptions.Get("mode.type").ToLowerInvariant();
			string protocol = Engine.Instance.ProfileOptions.Get("mode.protocol").ToUpperInvariant();
			int port = Engine.Instance.ProfileOptions.GetInt("mode.port");
			int entry = Engine.Instance.ProfileOptions.GetInt("mode.alt");

			if (type == "auto")
				return GetModeAuto();

			// Lap 1: Find exact mode
			// Lap 2: Find first mode with same type+protocol+port
			// Lap 3: Find first mode with same type+protocol
			// Lap 4: Find first mode with same type
			// return Auto

			for (int lap = 1; lap < 4; lap++)
			{
				foreach (ConnectionMode mode in Modes)
				{
					bool accept = true;

					if (mode.Type.ToLowerInvariant() != type.ToLowerInvariant())
						accept = false;

					if ((lap <= 3) && (mode.Protocol.ToLowerInvariant() != protocol.ToLowerInvariant()))
						accept = false;

					if ((lap <= 2) && (mode.Port != port))
						accept = false;

					if ((lap <= 1) && (mode.EntryIndex != entry))
						accept = false;

					if (accept)
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
			if ((nodeAttr == null) && (nodeGroup != null))
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
