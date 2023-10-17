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
using System.Text;

namespace Eddie.Core
{
	public class ProfileOptions
	{
		private Dictionary<string, ProfileOption> m_options = new Dictionary<string, ProfileOption>();

		public ProfileOptions()
		{
			EnsureDefaults();
		}

		public Dictionary<string, ProfileOption> Dict
		{
			get
			{
				return m_options;
			}
		}

		public string GetReportForSupport()
		{
			string result = "";
			foreach (ProfileOption option in Dict.Values)
			{
				if (option.InternalOnly)
					continue;

				if (option.Value != "")
				{
					if (option.Value != option.Default)
					{
						string v = "";
						if (option.Omissis)
							v = "(omissis)";
						else
							v = option.Value;
						result += option.Code + ": " + v + "\n";
					}
				}
			}
			return Platform.Instance.NormalizeString(result);
		}

		public string GetMan(string format)
		{
			string body = "";

			// Console-only
			foreach (KeyValuePair<string, string> kp in Engine.Instance.StartCommandLine.KnownCommands)
			{
				if (kp.Value != "")
				{
					string code = kp.Key;
					string desc = kp.Value.Replace("\n", "\n\t").Trim();
					if (desc.EndsWithInv(".") == false)
						desc += ".";
					desc += " " + LanguageManager.GetText(LanguageItems.ManCommandLineOnly);

					body += "[option_block][option_code]" + code + "[/option_code]\n\t\t" + desc + "[/option_block]\n";
				}
			}

			foreach (ProfileOption option in Dict.Values)
			{
				if (option.Man != "")
				{
					string code = option.Code;
					string desc = option.Man.Replace("\n", "\n\t").Trim();
					if (desc.EndsWithInv(".") == false)
						desc += ".";
					body += "[option_block][option_code]" + code + "[/option_code]\n\t\t" + desc;
					if (option.Default != "")
						body += " Default: [i]" + option.Default + "[/i]";
					body += "[/option_block]\n";
				}
			}

			string o = "\n";
			o += "[sh]NAME[/sh]\n";
			o += "\t" + LanguageManager.GetText(LanguageItems.ManName).Replace("\n", "[br]");
			o += "\n\n[sh]SYNOPSIS[/sh]\n";
			o += "\t" + LanguageManager.GetText(LanguageItems.ManSynopsis).Replace("\n", "[br]");
			o += "\n\n[sh]DESCRIPTION[/sh]\n";
			o += "\t" + LanguageManager.GetText(LanguageItems.ManDescription).Replace("\n", "[br]");
			o += "\n\n[sh]OPTIONS[/sh]\n";
			o += "\t" + LanguageManager.GetText(LanguageItems.ManHeaderOption1).Replace("\n", "[br]");
			o += "\t" + LanguageManager.GetText(LanguageItems.ManHeaderOption2).Replace("\n", "[br]");
			o += "\t" + LanguageManager.GetText(LanguageItems.ManHeaderOption3).Replace("\n", "[br]");
			o += "\t" + LanguageManager.GetText(LanguageItems.ManHeaderOption4).Replace("\n", "[br]");
			o += "\n\n";
			o += "\t[options_list]" + body.Replace("\n", "\n\t") + "[/options_list]";
			o += "\n\n[sh]COPYRIGHT[/sh]\n";
			o += "\t" + LanguageManager.GetText(LanguageItems.ManCopyright).Replace("\n", "[br]");
			o += "\n";

			if (format == "man")
			{
				// Escape dot that can go at beginning of line
				o = o.Replace("].", "]\\[char46]");

				// Header
				o = ".\\\"" + LanguageManager.GetText(LanguageItems.ManHeaderComment) + "\n.TH eddie-ui 8 \"" + DateTime.UtcNow.ToString(CultureInfo.InvariantCulture).EscapeQuote() + "\"" + o;

				o = o.Replace("[sh]", "\n.SH ");
				o = o.Replace("[/sh]", "\n");
				o = o.Replace("[link]", "\n.I ");
				o = o.Replace("[/link]", "\n");
				o = o.Replace("[option_code]", "\n.B \\-\\-");
				o = o.Replace("[/option_code]", "\n");
				o = o.Replace("[option_block]", "\n.TP\n");
				o = o.Replace("[/option_block]", "\n");
				o = o.Replace("[options_list]", "\n");
				o = o.Replace("[/options_list]", "\n");
				o = o.Replace("[i]", "\n.B ");
				o = o.Replace("[/i]", "\n");
				o = o.Replace("[br]", "\n.PP ");

				o = o.Replace("\t", "");

				// Normalization to avoid man layout break/issue
				for (; ; )
				{
					string orig = o;

					// Remove empty line
					o = o.Replace("\n\n", "\n");
					// Remove space
					o = o.Replace(" \n", "\n");
					// Remove space
					o = o.Replace("\n ", "\n");

					o = o.Trim();

					if (o == orig)
						break;
				}
			}
			else if (format == "bbcode")
			{
				o = o.Replace("[sh]", "[b]");
				o = o.Replace("[/sh]", "[/b]");
				o = o.Replace("[link]", "[url]");
				o = o.Replace("[/link]", "[/url]");
				o = o.Replace("[option_code]", "[b]");
				o = o.Replace("[/option_code]", "[/b]");
				o = o.Replace("[option_block]", "[*]");
				o = o.Replace("[/option_block]", "[/*]");
				o = o.Replace("[options_list]", "[list]");
				o = o.Replace("[/options_list]", "[/list]");
				o = o.Replace("[br]", "\n");
			}
			else if (format == "html")
			{
				o = o.Replace("[sh]", "<h3>");
				o = o.Replace("[/sh]", "</h3>");
				o = System.Text.RegularExpressions.Regex.Replace(o, "\\[link\\](.*?)\\[/link\\]", "<a href='$1'>$1</a>");
				o = o.Replace("[option_code]", "<b>");
				o = o.Replace("[/option_code]", "</b> - ");
				o = o.Replace("[option_block]", "<li>");
				o = o.Replace("[/option_block]", "</li>");
				o = o.Replace("[options_list]", "<ul>");
				o = o.Replace("[/options_list]", "</ul>");
				o = o.Replace("[i]", "<i>");
				o = o.Replace("[/i]", "</i>");
				o = o.Replace("[br]", "<br>");
			}
			else
			{
				// Text
				o = o.Replace("[sh]", "");
				o = o.Replace("[/sh]", "");
				o = o.Replace("[link]", "");
				o = o.Replace("[/link]", "");
				o = o.Replace("[option_code]", "");
				o = o.Replace("[/option_code]", "");
				o = o.Replace("[option_block]", "");
				o = o.Replace("[/option_block]", "");
				o = o.Replace("[options_list]", "");
				o = o.Replace("[/options_list]", "");
				o = o.Replace("[i]", "'");
				o = o.Replace("[/i]", "'");
				o = o.Replace("[br]", "\n\t");
			}

			return Platform.Instance.NormalizeString(o);
		}

		public bool Exists(string name)
		{
			lock (m_options)
			{
				return m_options.ContainsKey(name);
			}
		}

		public ProfileOption GetOption(string name)
		{
			if (Exists(name))
			{
				lock (m_options)
				{
					return m_options[name];
				}
			}

			return null;
		}

		public string Get(string name)
		{
			if (Engine.Instance.StartCommandLine.Exists(name))
			{
				return Engine.Instance.StartCommandLine.Get(name, "");
			}
			else if (Exists(name))
			{
				lock (m_options)
				{
					ProfileOption option = m_options[name];
					if (option.Value != "")
						return option.Value;
					else
						return option.Default;
				}
			}
			else
			{
				Engine.Instance.Logs.Log(LogType.Error, LanguageManager.GetText(LanguageItems.OptionsUnknown, name));
				return "";
			}
		}

		public string GetLower(string name)
		{
			return Get(name).ToLowerInvariant();
		}

		public bool GetBool(string name)
		{
			return Conversions.ToBool(Get(name));
		}

		public int GetInt(string name)
		{
			return Conversions.ToInt32(Get(name));
		}

		public float GetFloat(string name)
		{
			return Conversions.ToFloat(Get(name));
		}

		public Int64 GetInt64(string name)
		{
			return Conversions.ToInt64(Get(name));
		}

		public Encoding GetEncoding(string name)
		{
			string v = Get(name);
			if (v == "utf-8")
				return Encoding.UTF8;
			else if (v == "utf-32")
				return Encoding.UTF32;
			else if (v == "utf-16")
				return Encoding.Unicode;
			else if (v == "ascii")
				return Encoding.ASCII;
			else
				return Encoding.ASCII;
		}

		public List<string> GetList(string name)
		{
			List<string> output = new List<string>();
			string[] va = Get(name).Split(',');
			foreach (string v in va)
			{
				if (v != "")
					output.Add(v.Trim()); // Trim added in 2.11.5
			}
			return output;
		}

		public Json GetJson(string name)
		{
			string t = Get(name);
			Json j = new Json();
			Json.TryParse(t, out j);
			return j;
		}

		public void Set(string name, string val)
		{
			if (Exists(name) == false)
				Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText(LanguageItems.OptionsUnknown, name));
			else
			{
				string oldValue = Get(name);
				lock (m_options)
				{
					m_options[name].Value = val;
				}

				if (oldValue != val)
					OnChange(name);
			}
		}

		public void SetInt(string name, int val)
		{
			Set(name, val.ToString(CultureInfo.InvariantCulture));
		}

		public void SetFloat(string name, float val)
		{
			Set(name, val.ToString(CultureInfo.InvariantCulture));
		}

		public void SetBool(string name, bool val)
		{
			Set(name, val.ToString(CultureInfo.InvariantCulture));
		}

		public bool Set(string name, object val)
		{
			string s = Conversions.ToString(val);
			if (Get(name) == s)
				return false;
			if (m_options[name].Type == "text")
				Set(name, s);
			else if (m_options[name].Type.StartsWithInv("choice:"))
				Set(name, s.ToLowerInv());
			else if (m_options[name].Type.StartsWithInv("path_file"))
				Set(name, s);
			else if (m_options[name].Type == "bool")
				Set(name, Conversions.ToBool(s).ToString(CultureInfo.InvariantCulture));
			else if (m_options[name].Type == "int")
				Set(name, Conversions.ToInt32(s).ToString(CultureInfo.InvariantCulture));
			else if (m_options[name].Type == "float")
				Set(name, Conversions.ToFloat(s).ToString(CultureInfo.InvariantCulture));
			return true;
		}

		public void SetList(string name, List<string> val)
		{
			Set(name, String.Join(",", val.ToArray()));
		}

		public void SetJson(string name, Json val)
		{
			Set(name, val.ToJson());
		}

		public void SetDefault(string name, string type, string val, string man)
		{
			ProfileOption option = new ProfileOption();
			option.Code = name;
			option.Type = type;
			option.Default = val;
			option.Man = man;
			//option.Important = important;
			m_options[option.Code] = option;
		}

		public void SetDefaultChoice(string name, string values, string val, string man)
		{
			SetDefault(name, "choice:" + values, val, man);
		}

		public void SetDefaultInt(string name, int val, string man)
		{
			SetDefault(name, "int", val.ToString(CultureInfo.InvariantCulture), man);
		}

		public void SetDefaultBool(string name, bool val, string man)
		{
			SetDefault(name, "bool", val.ToString(CultureInfo.InvariantCulture), man);
		}

		public void SetDefaultFloat(string name, float val, string man)
		{
			SetDefault(name, "float", val.ToString(CultureInfo.InvariantCulture), man);
		}

		public void Remove(string name)
		{
			lock (this)
			{
				m_options.Remove(name);
			}
		}

		public void EnsureDefaults()
		{
			string NotInManNever = ""; // Option not listed in 'man' documentation.			
			string NotInManYet = ""; // Option that will be added in 'man' documentation.
#if DEBUG
			bool devDefault = true;
#else
            bool devDefault = false;
#endif
			SetDefaultBool("start_os", Platform.Instance.GetAutoStart(), NotInManYet);

			SetDefault("login", "text", "", LanguageManager.GetText(LanguageItems.ManOptionLogin));
			SetDefault("password", "password", "", LanguageManager.GetText(LanguageItems.ManOptionPassword));
			SetDefaultBool("remember", false, LanguageManager.GetText(LanguageItems.ManOptionRemember));
			SetDefault("key", "text", "Default", LanguageManager.GetText(LanguageItems.ManOptionKey));
			SetDefault("server", "text", "", LanguageManager.GetText(LanguageItems.ManOptionServer));
			SetDefaultBool("connect", false, LanguageManager.GetText(LanguageItems.ManOptionConnect));

			SetDefaultChoice("updater.channel", "stable,beta,internal,none", "stable", LanguageManager.GetText(LanguageItems.ManOptionUpdaterChannel));

			SetDefault("servers.allowlist", "text", "", LanguageManager.GetText(LanguageItems.ManOptionServersAllowlist));
			SetDefault("servers.denylist", "text", "", LanguageManager.GetText(LanguageItems.ManOptionServersDenylist));
			SetDefaultBool("servers.startlast", false, LanguageManager.GetText(LanguageItems.ManOptionServersStartLast));
			SetDefaultBool("servers.locklast", false, LanguageManager.GetText(LanguageItems.ManOptionServersLockLast));
			SetDefaultChoice("servers.scoretype", "Speed,Latency", "Speed", LanguageManager.GetText(LanguageItems.ManOptionServersScoreType));

			SetDefault("areas.allowlist", "text", "", LanguageManager.GetText(LanguageItems.ManOptionAreasAllowlist));
			SetDefault("areas.denylist", "text", "", LanguageManager.GetText(LanguageItems.ManOptionAreasDenylist));

			SetDefault("discover.ip_webservice.list", "text", "https://ipleak.net/json/{@ip};https://freegeoip.net/json/{@ip};http://ip-api.com/json/{@ip}", LanguageManager.GetText(LanguageItems.ManOptionDiscoverIpWebserviceList));
			SetDefaultInt("discover.interval", 60 * 60 * 24, LanguageManager.GetText(LanguageItems.ManOptionDiscoverInterval));
			SetDefaultBool("discover.exit", true, LanguageManager.GetText(LanguageItems.ManOptionDiscoverExit));

			SetDefaultBool("log.file.enabled", false, LanguageManager.GetText(LanguageItems.ManOptionLogFileEnabled));
			SetDefault("log.file.encoding", "encoding", "utf-8", LanguageManager.GetText(LanguageItems.ManOptionLogFileEncoding));
			SetDefault("log.file.path", "text", "logs/eddie_%y-%m-%d.log", LanguageManager.GetText(LanguageItems.ManOptionLogFilePath));
			SetDefaultBool("log.level.debug", false, LanguageManager.GetText(LanguageItems.ManOptionLogLevelDebug));
			SetDefaultBool("log.repeat", false, LanguageManager.GetText(LanguageItems.ManOptionLogRepeat));
			SetDefaultInt("log.limit", 1000, LanguageManager.GetText(LanguageItems.ManOptionLogLimit));

			SetDefaultInt("checking.ntry", 5, LanguageManager.GetText(LanguageItems.ManOptionCheckingNTry));

			SetDefault("language.iso", "text", "auto", LanguageManager.GetText(LanguageItems.ManOptionLanguageIso));

			SetDefault("mode.type", "text", "auto", LanguageManager.GetText(LanguageItems.ManOptionModeType));
			SetDefault("mode.protocol", "text", "udp", LanguageManager.GetText(LanguageItems.ManOptionModeProtocol));
			SetDefaultInt("mode.port", 443, LanguageManager.GetText(LanguageItems.ManOptionModePort));
			SetDefaultInt("mode.alt", 0, LanguageManager.GetText(LanguageItems.ManOptionModeAlt));

			SetDefault("proxy.mode", "text", "None", LanguageManager.GetText(LanguageItems.ManOptionProxyMode));
			SetDefaultChoice("proxy.when", "always/web/openvpn/none", "always", LanguageManager.GetText(LanguageItems.ManOptionProxyWhen));
			SetDefault("proxy.host", "ip", "127.0.0.1", LanguageManager.GetText(LanguageItems.ManOptionProxyHost));
			SetDefaultInt("proxy.port", 8080, LanguageManager.GetText(LanguageItems.ManOptionProxyPort));
			SetDefault("proxy.auth", "text", "None", LanguageManager.GetText(LanguageItems.ManOptionProxyAuth));
			SetDefault("proxy.login", "text", "", LanguageManager.GetText(LanguageItems.ManOptionProxyLogin));
			SetDefault("proxy.password", "password", "", LanguageManager.GetText(LanguageItems.ManOptionProxyPassword));
			SetDefaultInt("proxy.tor.control.port", 9151, LanguageManager.GetText(LanguageItems.ManOptionProxyTorControlPort));
			SetDefaultBool("proxy.tor.control.auth", true, LanguageManager.GetText(LanguageItems.ManOptionProxyTorControlAuth));
			SetDefault("proxy.tor.path", "", "", LanguageManager.GetText(LanguageItems.ManOptionProxyTorPath));
			SetDefault("proxy.tor.control.cookie.path", "", "", LanguageManager.GetText(LanguageItems.ManOptionProxyTorControlCookiePath));
			SetDefault("proxy.tor.control.password", "password", "", LanguageManager.GetText(LanguageItems.ManOptionProxyTorControlPassword));

			SetDefault("routes.custom", "text", "", LanguageManager.GetText(LanguageItems.ManOptionRoutesCustom));
			SetDefault("routes.catch_all_mode", "text", "auto", LanguageManager.GetText(LanguageItems.ManOptionRoutesCatchAllMode));

			SetDefault("dns.mode", "text", "auto", LanguageManager.GetText(LanguageItems.ManOptionDnsMode));
			SetDefault("dns.servers", "text", "", LanguageManager.GetText(LanguageItems.ManOptionDnsServers));
			SetDefault("dns.interfaces.names", "text", "", NotInManNever);
			SetDefault("dns.interfaces.types", "text", "auto", NotInManNever); // "auto", "all", comma-separated // not implemented yet in macOS, always 'all'
			SetDefaultBool("dns.delegate", false, LanguageManager.GetText(LanguageItems.ManOptionDnsDelegate));
			SetDefaultBool("dns.check", true, LanguageManager.GetText(LanguageItems.ManOptionDnsCheck));
			SetDefaultInt("dns.cache.ttl", 3600, LanguageManager.GetText(LanguageItems.ManOptionDnsCacheTTL));

			SetDefaultBool("netlock", false, LanguageManager.GetText(LanguageItems.ManOptionNetLock));
			SetDefaultBool("netlock.connection", true, LanguageManager.GetText(LanguageItems.ManOptionNetLockConnection));
			SetDefault("netlock.mode", "text", "auto", LanguageManager.GetText(LanguageItems.ManOptionNetLockMode));
			SetDefaultBool("netlock.allow_private", true, LanguageManager.GetText(LanguageItems.ManOptionNetLockAllowPrivate));
			SetDefaultBool("netlock.allow_dhcp", true, LanguageManager.GetText(LanguageItems.ManOptionNetLockAllowDHCP)); // Win only
			SetDefaultBool("netlock.allow_ping", true, LanguageManager.GetText(LanguageItems.ManOptionNetLockAllowPing));
			SetDefaultBool("netlock.allow_dns", false, LanguageManager.GetText(LanguageItems.ManOptionNetLockAllowDNS));
			SetDefaultChoice("netlock.incoming", "allow,block", "block", LanguageManager.GetText(LanguageItems.ManOptionNetLockIncoming));
			SetDefaultChoice("netlock.outgoing", "allow,block", "block", LanguageManager.GetText(LanguageItems.ManOptionNetLockOutgoing));
			SetDefault("netlock.allowlist.incoming.ips", "text", "", LanguageManager.GetText(LanguageItems.ManOptionNetLockAllowlistIncomingIps));
			SetDefault("netlock.allowlist.outgoing.ips", "text", "", LanguageManager.GetText(LanguageItems.ManOptionNetLockAllowlistOutgoingIps));

			SetDefault("network.entry.iface", "text", "", LanguageManager.GetText(LanguageItems.ManOptionNetworkEntryIFace));
			SetDefault("network.entry.iplayer", "text", "ipv4-ipv6", LanguageManager.GetText(LanguageItems.ManOptionNetworkEntryIpLayer)); // ipv6-ipv4;ipv4-ipv6;ipv4-only;ipv6-only;
			SetDefaultChoice("network.ipv4.mode", "in,in-out,in-block,out,block", "in", LanguageManager.GetText(LanguageItems.ManOptionNetworkIPv4Mode));
			SetDefaultChoice("network.ipv6.mode", "in,in-out,in-block,out,block", "in-block", LanguageManager.GetText(LanguageItems.ManOptionNetworkIPv6Mode));
			SetDefaultBool("network.ipv4.autoswitch", false, LanguageManager.GetText(LanguageItems.ManOptionNetworkIPv4AutoSwitch));
			SetDefaultBool("network.ipv6.autoswitch", true, LanguageManager.GetText(LanguageItems.ManOptionNetworkIPv6AutoSwitch));
			SetDefault("network.iface.name", "text", "", LanguageManager.GetText(LanguageItems.ManOptionNetworkIfaceName));

			SetDefault("tools.openvpn.path", "path_file", "", LanguageManager.GetText(LanguageItems.ManOptionToolsOpenVpnPath));
			SetDefaultBool("tools.hummingbird.preferred", Platform.Instance.PreferHummingbirdIfAvailable(), LanguageManager.GetText(LanguageItems.ManOptionToolsHummingbirdPreferred));
			SetDefault("tools.hummingbird.path", "path_file", "", LanguageManager.GetText(LanguageItems.ManOptionToolsHummingbirdPath));
			SetDefault("tools.ssh.path", "path_file", "", LanguageManager.GetText(LanguageItems.ManOptionToolsSshPath));
			SetDefault("tools.ssl.path", "path_file", "", LanguageManager.GetText(LanguageItems.ManOptionToolsSslPath));
			SetDefault("tools.curl.path", "path_file", "", LanguageManager.GetText(LanguageItems.ManOptionToolsCurlPath));

			SetDefaultInt("http.timeout", 10, LanguageManager.GetText(LanguageItems.ManOptionHttpTimeout));

			/*
			SetDefaultBool("webui.enabled", true, NotInMan); // WebUI it's a Eddie 3.* feature not yet committed on GitHub.
			SetDefault("webui.ip", "text", "localhost", NotInMan);
			SetDefaultInt("webui.port", 4649, NotInMan);
			*/

			SetDefaultBool("external.rules.recommended", true, NotInManNever);
			SetDefault("external.rules", "json", "[]", NotInManNever);

			SetDefault("openvpn.custom", "text", "", LanguageManager.GetText(LanguageItems.ManOptionOpenVpnCustom));
			SetDefault("openvpn.dev_node", "text", "", LanguageManager.GetText(LanguageItems.ManOptionOpenVpnDevNode));
			SetDefaultInt("openvpn.sndbuf", -2, LanguageManager.GetText(LanguageItems.ManOptionOpenVpnSndBuf)); // 2.11
			SetDefaultInt("openvpn.rcvbuf", -2, LanguageManager.GetText(LanguageItems.ManOptionOpenVpnRcvBuf)); // 2.11
			SetDefault("openvpn.directives", "text", "client\r\ndev tun\r\nauth-nocache\r\nresolv-retry infinite\r\nnobind\r\npersist-key\r\npersist-tun\r\nverb 3\r\nconnect-retry-max 1\r\nping 10\r\nping-restart 60\r\nexplicit-exit-notify 5", LanguageManager.GetText(LanguageItems.ManOptionOpenVpnDirectives));
			SetDefault("openvpn.directives.path", "path_file", "", LanguageManager.GetText(LanguageItems.ManOptionOpenVpnDirectivesPath));			
			SetDefault("openvpn.directives.data-ciphers", "text", "AES-256-GCM:AES-192-GCM:AES-128-GCM", LanguageManager.GetText(LanguageItems.ManOptionOpenVpnDirectivesDataCiphers));
			SetDefault("openvpn.directives.data-ciphers-fallback", "text", "AES-256-GCM", LanguageManager.GetText(LanguageItems.ManOptionOpenVpnDirectivesDataCiphersFallback));
			SetDefaultBool("openvpn.directives.chacha20", false, LanguageManager.GetText(LanguageItems.ManOptionOpenVpnDirectivesChacha20)); // Temporary
			SetDefaultBool("openvpn.skip_defaults", false, LanguageManager.GetText(LanguageItems.ManOptionOpenVpnSkipDefaults));
			
			SetDefaultBool("wireguard.interface.skip_commands", true, NotInManNever); // Anyway are not implemented in Eddie, keep for future.
			SetDefaultInt("wireguard.peer.persistentkeepalive", 15, LanguageManager.GetText(LanguageItems.ManOptionWireGuardPeerPersistentKeepalive));
			SetDefaultInt("wireguard.handshake.timeout.first", 50, NotInManNever);
			SetDefaultInt("wireguard.handshake.timeout.connected", 180 + 20, NotInManNever); // To maintain the session a client must handshake at least once every 180 seconds.
			SetDefaultInt("wireguard.interface.mtu", -1, LanguageManager.GetText(LanguageItems.ManOptionWireGuardInterfaceMTU));

			// Not in Settings			
			SetDefaultInt("ssh.port", 0, LanguageManager.GetText(LanguageItems.ManOptionSshPort));
			SetDefaultInt("ssl.port", 0, LanguageManager.GetText(LanguageItems.ManOptionSslPort));
			SetDefault("ssl.options", "text", "", NotInManNever); // "NO_SSLv2" < 2.11.10
			SetDefaultInt("ssl.verify", -1, NotInManNever);

			SetDefaultBool("advanced.expert", false, LanguageManager.GetText(LanguageItems.ManOptionAdvancedExpert));
			SetDefaultBool("advanced.check.route", true, LanguageManager.GetText(LanguageItems.ManOptionAdvancedCheckRoute));

			SetDefaultBool("pinger.enabled", true, LanguageManager.GetText(LanguageItems.ManOptionAdvancedPingerEnabled));
			SetDefaultInt("pinger.delay", 0, LanguageManager.GetText(LanguageItems.ManOptionAdvancedPingerDelay));
			SetDefaultInt("pinger.retry", 0, LanguageManager.GetText(LanguageItems.ManOptionAdvancedPingerRetry));
			SetDefaultInt("pinger.jobs", 25, LanguageManager.GetText(LanguageItems.ManOptionAdvancedPingerJobs));
			SetDefaultInt("pinger.valid", 0, LanguageManager.GetText(LanguageItems.ManOptionAdvancedPingerValid));
			SetDefaultInt("pinger.timeout", 3000, LanguageManager.GetText(LanguageItems.ManOptionAdvancedPingerTimeout));

			SetDefaultInt("advanced.manifest.refresh", -1, LanguageManager.GetText(LanguageItems.ManOptionAdvancedManifestRefresh));
			SetDefaultBool("advanced.providers", false, LanguageManager.GetText(LanguageItems.ManOptionAdvancedProviders));

			SetDefault("bootstrap.urls", "text", "", LanguageManager.GetText(LanguageItems.ManOptionBootstrapUrls)); // WIP: move to provider level

			SetDefaultInt("advanced.penality_on_error", 30, NotInManNever);
			SetDefaultBool("advanced.skip_alreadyrun", false, NotInManNever); // Continue even if openvpn is already running.
			SetDefaultBool("connections.allow_anyway", devDefault, NotInManNever); // Allow connection even if in 'Not available' status.
			SetDefaultBool("advanced.testonly", false, NotInManNever); // Disconnect when connection occur.

			EnsureDefaultsEvent("app.start");
			EnsureDefaultsEvent("app.stop");
			EnsureDefaultsEvent("session.start");
			EnsureDefaultsEvent("session.stop");
			EnsureDefaultsEvent("vpn.pre");
			EnsureDefaultsEvent("vpn.up");
			EnsureDefaultsEvent("vpn.down");

			// Windows only			
			SetDefaultChoice("windows.driver", "auto,ovpn-dco,wintun,tap-windows6,none", "auto", LanguageManager.GetText(LanguageItems.ManOptionWindowsDriver));
			SetDefaultBool("windows.adapters.cleanup", true, LanguageManager.GetText(LanguageItems.ManOptionWindowsDriverCleanup));
			SetDefault("windows.adapter_service", "text", "tap0901", LanguageManager.GetText(LanguageItems.ManOptionWindowsAdapterService));
			SetDefaultBool("windows.disable_driver_upgrade", false, LanguageManager.GetText(LanguageItems.ManOptionWindowsDisableDriverUpgrade));
			SetDefaultBool("windows.tap_up", true, LanguageManager.GetText(LanguageItems.ManOptionWindowsTapUp));
			SetDefaultBool("windows.wfp.enable", true, LanguageManager.GetText(LanguageItems.ManOptionWindowsWfp));
			SetDefaultBool("windows.wfp.dynamic", false, LanguageManager.GetText(LanguageItems.ManOptionWindowsWfpDynamic));
			SetDefaultBool("windows.dns.lock", true, LanguageManager.GetText(LanguageItems.ManOptionWindowsDnsLock));
			SetDefaultInt("windows.metrics.tap.ipv4", -2, LanguageManager.GetText(LanguageItems.ManOptionWindowsMetricsTapIPv4)); // 2.13:   0: Windows Automatic, >0 value, -1: Don't change, -2: Automatic
			SetDefaultInt("windows.metrics.tap.ipv6", -2, LanguageManager.GetText(LanguageItems.ManOptionWindowsMetricsTapIPv6)); // 2.13:   0: Windows Automatic, >0 value, -1: Don't change, -2: Automatic
			SetDefaultBool("windows.ipv6.bypass_dns", false, NotInManNever); // 2.14: Workaround, skip OpenVPN DNS6. Only with dns.delegate, for this not in MAN. Maybe deprecated in future.
			SetDefaultBool("windows.ssh.plink.force", false, NotInManNever); // Switch to false when stable/tested. Not in MAN because need to be deprecated.
			SetDefaultBool("windows.workarounds", false, NotInManNever); // If true, some variants to identify issues. Not in MAN because need to be deprecated.

			// macOS only
			SetDefaultBool("macos.ipv6.dnslookup", true, LanguageManager.GetText(LanguageItems.ManOptionMacosIPv6DnsLookup));

			// Linux only
			SetDefault("linux.dns.services", "text", "nscd;dnsmasq;named;bind9", LanguageManager.GetText(LanguageItems.ManOptionLinuxDnsServices));

			// Internal only
			SetDefault("servers.last", "text", "", NotInManNever);

			// General UI
			SetDefault("ui.unit", "text", "", LanguageManager.GetText(LanguageItems.ManOptionUiUnit));
			SetDefaultBool("ui.iec", false, LanguageManager.GetText(LanguageItems.ManOptionUiIEC));
			SetDefaultBool("ui.skip.provider.manifest.failed", false, LanguageManager.GetText(LanguageItems.ManOptionUiSkipProviderManifestFailed));
			SetDefaultBool("ui.skip.promotional", false, LanguageManager.GetText(LanguageItems.ManOptionUiSkipPromotional));
			SetDefaultBool("ui.skip.netlock.confirm", false, LanguageManager.GetText(LanguageItems.ManOptionUiSkipNetLockConfirm));

			// UI only
			SetDefaultBool("gui.start_minimized", false, NotInManNever);
			SetDefaultBool("gui.tray_show", true, NotInManNever);
			SetDefaultBool("gui.tray_minimized", (Platform.Instance.IsLinuxSystem() == false), NotInManNever); // We can't know if the Linux Desktop Environment will support show tray.
			SetDefaultBool("gui.notifications", true, NotInManNever);
			SetDefaultBool("gui.exit_confirm", true, NotInManNever);
			SetDefault("gui.font.normal.name", "text", "", NotInManNever);
			SetDefaultFloat("gui.font.normal.size", 0, NotInManNever);

			SetDefault("gui.window.main", "text", "", NotInManNever);
			SetDefault("gui.list.servers", "text", "", NotInManNever);
			SetDefault("gui.list.areas", "text", "", NotInManNever);
			SetDefault("gui.list.logs", "text", "", NotInManNever);

			// UI - macOS Only
			// SetDefaultBool("gui.osx.dock", false, NotInMan); // See this FAQ: https://airvpn.org/topic/13331-its-possible-to-hide-the-icon-in-dock-bar-under-os-x/
			SetDefaultBool("gui.osx.visible", false, NotInManNever);
			SetDefaultBool("gui.osx.sysbar.show_info", false, NotInManNever);
			SetDefaultBool("gui.osx.sysbar.show_speed", false, NotInManNever); // Menu Status, Window Title, Tray Tooltip
			SetDefaultBool("gui.osx.sysbar.show_server", false, NotInManNever);

			// Platform-specific
			m_options["start_os"].Platforms = "Windows;";

			// Internal only
			m_options["servers.last"].InternalOnly = true;
			m_options["gui.window.main"].InternalOnly = true;
			m_options["gui.list.servers"].InternalOnly = true;
			m_options["gui.list.areas"].InternalOnly = true;
			m_options["gui.list.logs"].InternalOnly = true;

			// Omissis
			m_options["login"].Omissis = true;
			m_options["password"].Omissis = true;
			m_options["key"].Omissis = true;
			m_options["proxy.login"].Omissis = true;
			m_options["proxy.password"].Omissis = true;
			m_options["proxy.tor.control.password"].Omissis = true;

			// Don't clean with user Reset All
			m_options["login"].DontUserReset = true;
			m_options["password"].DontUserReset = true;
			m_options["remember"].DontUserReset = true;
			m_options["key"].DontUserReset = true;

			// Exceptions
			// TOFIX: Some users report a stuck in 'Checking environment' phase on macOS only, Eddie 2.13.4 and above.
			if (Platform.Instance.GetCode() == "MacOS")
				m_options["advanced.skip_alreadyrun"].Default = "True";
		}

		public void EnsureDefaultsEvent(string name)
		{
			SetDefault("event." + name + ".filename", "path_file", "", LanguageManager.GetText(LanguageItems.ManOptionEventFileName));
			SetDefault("event." + name + ".arguments", "text", "", LanguageManager.GetText(LanguageItems.ManOptionEventArguments));
			SetDefaultBool("event." + name + ".waitend", true, LanguageManager.GetText(LanguageItems.ManOptionEventWaitEnd));
		}

		public void ResetAll(bool force)
		{
			foreach (ProfileOption option in Dict.Values)
			{
				if ((force == false) && (option.DontUserReset))
					continue;

				option.Value = "";
			}
		}

		public void OnChange(string name)
		{
			if (name == "start_os")
			{
				Platform.Instance.SetAutoStart(GetBool("start_os"));
			}
			else if (name == "tools.openvpn.path")
			{
				Software.Checking();
			}

			Json j = new Json();
			j["command"].Value = "option.change";
			j["name"].Value = name;
			j["value"].Value = Get(name);
			Engine.Instance.UiManager.Broadcast(j);
		}

		public Json GetJson()
		{
			Json j = new Json();
			j.EnsureDictionary();

			foreach (KeyValuePair<string, ProfileOption> kp in m_options)
			{
				Json jOption = kp.Value.GetJson();
				jOption["value"].Value = Get(kp.Key);
				j[kp.Key].Value = jOption;
			}

			return j;
		}
	}
}
