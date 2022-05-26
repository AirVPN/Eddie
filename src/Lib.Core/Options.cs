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
using System.Text;

namespace Eddie.Core
{
	public class Options
	{
		private Dictionary<string, Option> m_options = new Dictionary<string, Option>();

		public Options()
		{
			EnsureDefaults();
		}

		public Dictionary<string, Option> Dict
		{
			get
			{
				return m_options;
			}
		}

		public string GetReportForSupport()
		{
			string result = "";
			foreach (Option option in Dict.Values)
			{
				if (option.Important == false)
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
					body += "[option_block][option_code]" + kp.Key + "[/option_code]\n\t\t" + kp.Value.Replace("\n", "\n\t") + "[/option_block]\n";
			}

			foreach (Option option in Dict.Values)
			{
				if (option.Man != "")
				{
					body += "[option_block][option_code]" + option.Code + "[/option_code]\n\t\t" + option.Man.Replace("\n", "\n\t");
					if (option.Default != "")
						body += " Default: [i]" + option.Default + "[/i]";
					body += "[/option_block]\n";
				}
			}

			string o = "\n";
			o += "[sh]NAME[/sh]\n";
			o += "\t" + LanguageManager.GetText("ManName").Replace("\n", "[br]");
			o += "\n\n[sh]SYNOPSIS[/sh]\n";
			o += "\t" + LanguageManager.GetText("ManSynopsis").Replace("\n", "[br]");
			o += "\n\n[sh]DESCRIPTION[/sh]\n";
			o += "\t" + LanguageManager.GetText("ManDescription").Replace("\n", "[br]");
			o += "\n\n[sh]OPTIONS[/sh]\n";
			o += "\t" + LanguageManager.GetText("ManHeaderOption1").Replace("\n", "[br]");
			o += "\t" + LanguageManager.GetText("ManHeaderOption2").Replace("\n", "[br]");
			o += "\t" + LanguageManager.GetText("ManHeaderOption3").Replace("\n", "[br]");
			o += "\t" + LanguageManager.GetText("ManHeaderOption4").Replace("\n", "[br]");
			o += "\n\n";
			o += "\t[options_list]" + body.Replace("\n", "\n\t") + "[/options_list]";
			o += "\n\n[sh]COPYRIGHT[/sh]\n";
			o += "\t" + LanguageManager.GetText("ManCopyright").Replace("\n", "[br]");
			o += "\n";

			if (format == "man")
			{
				// Escape dot that can go at beginning of line
				o = o.Replace("].", "]\\[char46]");

				// Header
				o = ".\\\"" + LanguageManager.GetText("ManHeaderComment") + "\n.TH eddie-ui 8 \"" + DateTime.UtcNow.ToString(CultureInfo.InvariantCulture).EscapeQuote() + "\"" + o;

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

		public Option GetOption(string name)
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
					Option option = m_options[name];
					if (option.Value != "")
						return option.Value;
					else
						return option.Default;
				}
			}
			else
			{
				Engine.Instance.Logs.Log(LogType.Error, LanguageManager.GetText("OptionsUnknown", name));
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
			else if (v == "utf-7")
				return Encoding.UTF7;
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
				Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("OptionsUnknown", name));
			else
			{
				string oldValue = "";
				lock (m_options)
				{
					oldValue = m_options[name].Value;
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
			else if (m_options[name].Type.StartsWith("choice:"))
				Set(name, s.ToLowerInv());
			else if (m_options[name].Type.StartsWith("path_file"))
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
			SetDefault(name, type, val, man, true);
		}

		public void SetDefault(string name, string type, string val, string man, bool important)
		{
			Option option = new Option();
			option.Code = name;
			option.Type = type;
			option.Default = val;
			option.Man = man;
			option.Important = important;
			m_options[option.Code] = option;
		}

		public void SetDefaultChoice(string name, string values, string val, string man)
		{
			SetDefault(name, "choice:" + values, val, man);
		}

		public void SetDefaultInt(string name, int val, string man)
		{
			SetDefault(name, "int", val.ToString(CultureInfo.InvariantCulture), man, true);
		}

		public void SetDefaultBool(string name, bool val, string man)
		{
			SetDefault(name, "bool", val.ToString(CultureInfo.InvariantCulture), man, true);
		}

		public void SetDefaultBool(string name, bool val, string man, bool important)
		{
			SetDefault(name, "bool", val.ToString(CultureInfo.InvariantCulture), man, important);
		}

		public void SetDefaultFloat(string name, float val, string man)
		{
			SetDefault(name, "float", val.ToString(CultureInfo.InvariantCulture), man, true);
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
			string NotInMan = ""; // Option not listed in 'man' documentation.
#if DEBUG
			bool devDefault = true;
#else
            bool devDefault = false;
#endif

			SetDefault("login", "text", "", LanguageManager.GetText("ManOptionLogin"));
			SetDefault("password", "password", "", LanguageManager.GetText("ManOptionPassword"));
			SetDefaultBool("remember", false, LanguageManager.GetText("ManOptionRemember"));
			SetDefault("key", "text", "Default", LanguageManager.GetText("ManOptionKey"));
			SetDefault("server", "text", "", LanguageManager.GetText("ManOptionServer"));
			SetDefaultBool("connect", false, LanguageManager.GetText("ManOptionConnect"));
			SetDefaultBool("netlock", false, LanguageManager.GetText("ManOptionNetLock"));

			SetDefaultChoice("updater.channel", "stable,beta,internal,none", "stable", LanguageManager.GetText("ManOptionUpdaterChannel"));

			SetDefault("servers.last", "text", "", NotInMan, false);
			SetDefault("servers.allowlist", "text", "", LanguageManager.GetText("ManOptionServersAllowlist"));
			SetDefault("servers.denylist", "text", "", LanguageManager.GetText("ManOptionServersDenylist"));
			SetDefaultBool("servers.startlast", false, LanguageManager.GetText("ManOptionServersStartLast"));
			SetDefaultBool("servers.locklast", false, LanguageManager.GetText("ManOptionServersLockLast"));
			SetDefaultChoice("servers.scoretype", "Speed,Latency", "Speed", LanguageManager.GetText("ManOptionServersScoreType"));

			SetDefault("areas.allowlist", "text", "", LanguageManager.GetText("ManOptionAreasAllowlist"));
			SetDefault("areas.denylist", "text", "", LanguageManager.GetText("ManOptionAreasDenylist"));

			SetDefault("discover.ip_webservice.list", "text", "https://ipleak.net/json/{@ip};https://freegeoip.net/json/{@ip};http://ip-api.com/json/{@ip}", NotInMan);
			SetDefaultBool("discover.ip_webservice.first", true, NotInMan);
			SetDefaultInt("discover.interval", 60 * 60 * 24, NotInMan); // Delta between refresh discover data (country and other data) for non-service connections.
			SetDefaultBool("discover.exit", true, NotInMan);

			SetDefaultBool("log.file.enabled", false, NotInMan);
			SetDefault("log.file.encoding", "encoding", "utf-8", NotInMan);
			SetDefault("log.file.path", "text", "logs/eddie_%y-%m-%d.log", NotInMan);
			SetDefaultBool("log.level.debug", false, NotInMan);
			SetDefaultBool("log.repeat", false, NotInMan);
			SetDefaultInt("log.limit", 1000, NotInMan);

			SetDefaultInt("checking.ntry", 5, NotInMan); // Number of retry in some action (for example checking tun/dns)

			SetDefault("language.iso", "text", "auto", LanguageManager.GetText("ManOptionLanguageIso"));

			SetDefault("mode.type", "text", "auto", LanguageManager.GetText("ManOptionModeType"));
			SetDefault("mode.protocol", "text", "udp", LanguageManager.GetText("ManOptionModeProtocol"));
			SetDefaultInt("mode.port", 443, LanguageManager.GetText("ManOptionModePort"));
			SetDefaultInt("mode.alt", 0, LanguageManager.GetText("ManOptionModeAlt"));

			SetDefault("proxy.mode", "text", "None", LanguageManager.GetText("ManOptionProxyMode"));
			SetDefaultChoice("proxy.when", "always/web/openvpn/none", "always", NotInMan);
			SetDefault("proxy.host", "ip", "127.0.0.1", LanguageManager.GetText("ManOptionProxyHost"));
			SetDefaultInt("proxy.port", 8080, LanguageManager.GetText("ManOptionProxyPort"));
			SetDefault("proxy.auth", "text", "None", LanguageManager.GetText("ManOptionProxyAuth"));
			SetDefault("proxy.login", "text", "", LanguageManager.GetText("ManOptionProxyLogin"));
			SetDefault("proxy.password", "password", "", LanguageManager.GetText("ManOptionProxyPassword"));
			SetDefaultInt("proxy.tor.control.port", 9151, LanguageManager.GetText("ManOptionProxyTorControlPort"));
			SetDefaultBool("proxy.tor.control.auth", true, LanguageManager.GetText("ManOptionProxyTorControlAuth"));
			SetDefault("proxy.tor.path", "", "", NotInMan);
			SetDefault("proxy.tor.control.cookie.path", "", "", NotInMan);
			SetDefault("proxy.tor.control.password", "password", "", LanguageManager.GetText("ManOptionProxyTorControlPassword"));

			SetDefault("routes.custom", "text", "", LanguageManager.GetText("ManOptionRoutesCustom"));
			SetDefault("routes.catch_all_mode", "text", "auto", NotInMan);

			SetDefault("dns.mode", "text", "auto", LanguageManager.GetText("ManOptionDnsMode"));
			SetDefault("dns.servers", "text", "", LanguageManager.GetText("ManOptionDnsServers"));
			SetDefault("dns.interfaces.names", "text", "", NotInMan);
			SetDefault("dns.interfaces.types", "text", "auto", NotInMan); // "auto", "all", comma-separated // TOFIX: not implemented yet in macOS, always 'all'
			SetDefaultBool("dns.delegate", false, NotInMan);
			SetDefaultBool("dns.check", true, LanguageManager.GetText("ManOptionDnsCheck"));
			SetDefaultInt("dns.cache.ttl", 3600, NotInMan);

			SetDefault("netlock.mode", "text", "auto", LanguageManager.GetText("ManOptionNetLockMode"));
			SetDefaultBool("netlock.allow_private", true, LanguageManager.GetText("ManOptionNetLockAllowPrivate"));
			SetDefaultBool("netlock.allow_dhcp", true, LanguageManager.GetText("ManOptionNetLockAllowDHCP")); // Win only
			SetDefaultBool("netlock.allow_ping", true, LanguageManager.GetText("ManOptionNetLockAllowPing"));
			SetDefaultBool("netlock.allow_dns", false, LanguageManager.GetText("ManOptionNetLockAllowDNS"));
			SetDefaultChoice("netlock.incoming", "allow,block", "block", NotInMan);
			SetDefaultChoice("netlock.outgoing", "allow,block", "block", NotInMan);
			SetDefault("netlock.allowlist.incoming.ips", "text", "", LanguageManager.GetText("ManOptionNetLockAllowlistIncomingIps"));
			SetDefault("netlock.allowlist.outgoing.ips", "text", "", LanguageManager.GetText("ManOptionNetLockAllowlistOutgoingIps"));

			SetDefault("network.entry.iface", "text", "", NotInMan);
			SetDefault("network.entry.iplayer", "text", "ipv4-ipv6", NotInMan); // ipv6-ipv4;ipv4-ipv6;ipv4-only;ipv6-only;
			SetDefaultChoice("network.ipv4.mode", "in,in-out,in-block,out,block", "in", NotInMan);
			SetDefaultChoice("network.ipv6.mode", "in,in-out,in-block,out,block", "in-block", NotInMan);
			SetDefaultBool("network.ipv4.autoswitch", false, NotInMan);
			SetDefaultBool("network.ipv6.autoswitch", true, NotInMan);
			SetDefault("network.gateways.default_skip_types", "text", "Loopback;Tunnel", NotInMan);
			SetDefaultInt("network.mtu", -1, NotInMan);
			SetDefault("network.iface.name", "text", "", LanguageManager.GetText("ManOptionNetworkIfaceName"));

			SetDefault("tools.openvpn.path", "path_file", "", LanguageManager.GetText("ManOptionToolsOpenVpnPath"));
			SetDefaultBool("tools.hummingbird.preferred", Platform.Instance.PreferHummingbirdIfAvailable(), NotInMan);
			SetDefault("tools.hummingbird.path", "path_file", "", NotInMan);
			SetDefault("tools.ssh.path", "path_file", "", LanguageManager.GetText("ManOptionToolsSshPath"));
			SetDefault("tools.ssl.path", "path_file", "", LanguageManager.GetText("ManOptionToolsSslPath"));
			SetDefault("tools.curl.path", "path_file", "", LanguageManager.GetText("ManOptionToolsCurlPath"));

			SetDefaultInt("http.timeout", 20, NotInMan);

			/*
			SetDefaultBool("webui.enabled", true, NotInMan); // WebUI it's a Eddie 3.* feature not yet committed on GitHub.
			SetDefault("webui.ip", "text", "localhost", NotInMan);
			SetDefaultInt("webui.port", 4649, NotInMan);
			*/

			SetDefaultBool("external.rules.recommended", true, NotInMan);
			SetDefault("external.rules", "json", "[]", NotInMan);

			SetDefault("openvpn.custom", "text", "", LanguageManager.GetText("ManOptionOpenVpnCustom"));
			SetDefault("openvpn.dev_node", "text", "", LanguageManager.GetText("ManOptionOpenVpnDevNode"));
			SetDefaultInt("openvpn.sndbuf", -2, LanguageManager.GetText("ManOptionOpenVpnSndBuf")); // 2.11
			SetDefaultInt("openvpn.rcvbuf", -2, LanguageManager.GetText("ManOptionOpenVpnRcvBuf")); // 2.11
			SetDefault("openvpn.directives", "text", "client\r\ndev tun\r\nauth-nocache\r\nresolv-retry infinite\r\nnobind\r\npersist-key\r\npersist-tun\r\nverb 3\r\nconnect-retry-max 1\r\nping 10\r\nping-exit 32\r\nexplicit-exit-notify 5", LanguageManager.GetText("ManOptionOpenVpnDirectives"));
			SetDefault("openvpn.directives.path", "path_file", "", NotInMan);
			SetDefault("openvpn.directives.data-ciphers", "text", "AES-256-GCM:AES-256-CBC:AES-192-GCM:AES-192-CBC:AES-128-GCM:AES-128-CBC", NotInMan);
			SetDefault("openvpn.directives.data-ciphers-fallback", "text", "AES-256-CBC", NotInMan);
			SetDefaultBool("openvpn.directives.chacha20", false, NotInMan); // Temporary
			SetDefaultBool("openvpn.skip_defaults", false, LanguageManager.GetText("ManOptionOpenVpnSkipDefaults"));
			//SetDefaultBool("openvpn.allow.script-security", false, NotInMan);

			SetDefaultBool("wireguard.interface.skip_commands", true, NotInMan); // Anyway are not implemented in Eddie, keep for future.
			SetDefaultInt("wireguard.peer.persistentkeepalive", 15, LanguageManager.GetText("ManOptionWireGuardPeerPersistentKeepalive"));
			SetDefaultInt("wireguard.handshake.timeout.first", 50, NotInMan);
			SetDefaultInt("wireguard.handshake.timeout.connected", 180 + 20, NotInMan); // To maintain the session a client must handshake at least once every 180 seconds

			// Not in Settings			
			SetDefaultInt("ssh.port", 0, LanguageManager.GetText("ManOptionSshPort"));
			SetDefaultInt("ssl.port", 0, LanguageManager.GetText("ManOptionSslPort"));
			SetDefault("ssl.options", "text", "", NotInMan); // "NO_SSLv2" < 2.11.10
			SetDefaultInt("ssl.verify", -1, NotInMan);

			//SetDefaultBool("os.single_instance", true, LanguageManager.GetText("ManOptionOsSingleInstance")); // Removed in 2.21 - Elevated accept only one instance

			SetDefaultBool("advanced.expert", false, LanguageManager.GetText("ManOptionAdvancedExpert"));
			SetDefaultBool("advanced.check.route", true, LanguageManager.GetText("ManOptionAdvancedCheckRoute"));

			SetDefaultInt("advanced.penality_on_error", 30, NotInMan);

			SetDefaultBool("pinger.enabled", true, LanguageManager.GetText("ManOptionAdvancedPingerEnabled"));
			SetDefaultInt("pinger.delay", 0, LanguageManager.GetText("ManOptionAdvancedPingerDelay"));
			SetDefaultInt("pinger.retry", 0, LanguageManager.GetText("ManOptionAdvancedPingerRetry"));
			SetDefaultInt("pinger.jobs", 25, LanguageManager.GetText("ManOptionAdvancedPingerJobs"));
			SetDefaultInt("pinger.valid", 0, LanguageManager.GetText("ManOptionAdvancedPingerValid"));
			SetDefaultInt("pinger.timeout", 3000, NotInMan);

			SetDefaultInt("advanced.manifest.refresh", -1, NotInMan);
			SetDefaultBool("advanced.providers", false, NotInMan);

			SetDefault("bootstrap.urls", "text", "", NotInMan); // WIP: move to provider level

			SetDefaultBool("advanced.skip_tun_detect", false, NotInMan); // Skip TUN driver detection.
			SetDefaultBool("advanced.skip_alreadyrun", false, NotInMan); // Continue even if openvpn is already running.
			SetDefaultBool("connections.allow_anyway", devDefault, NotInMan); // Allow connection even if in 'Not available' status.
			SetDefaultBool("advanced.testonly", false, NotInMan); // Disconnect when connection occur.

			EnsureDefaultsEvent("app.start");
			EnsureDefaultsEvent("app.stop");
			EnsureDefaultsEvent("session.start");
			EnsureDefaultsEvent("session.stop");
			EnsureDefaultsEvent("vpn.pre");
			EnsureDefaultsEvent("vpn.up");
			EnsureDefaultsEvent("vpn.down");

			// Windows only			
			SetDefaultBool("windows.adapters.cleanup", true, NotInMan);
			SetDefault("windows.adapter_service", "text", "tap0901", LanguageManager.GetText("ManOptionWindowsAdapterService"));
			SetDefaultBool("windows.disable_driver_upgrade", false, LanguageManager.GetText("ManOptionWindowsDisableDriverUpgrade"));
			SetDefaultBool("windows.tap_up", true, LanguageManager.GetText("ManOptionWindowsTapUp"));
			//SetDefaultBool("windows.dhcp_disable", false, LanguageManager.GetText("ManOptionWindowsDhcpDisable")); // Deprecated in 2.18
			SetDefaultBool("windows.wfp.enable", true, LanguageManager.GetText("ManOptionWindowsWfp"));
			SetDefaultBool("windows.wfp.dynamic", false, LanguageManager.GetText("ManOptionWindowsWfpDynamic"));
			//SetDefaultBool("windows.ipv6.os_disable", false, NotInMan); // Must be default FALSE if WFP works well // Removed in 2.14, in W10 require reboot
			//SetDefaultBool("windows.dns.force_all_interfaces", false, NotInMan); // Important: With WFP can be false, but users report DNS leak. Maybe not a real DNS Leak, simply request on DNS of other interfaces through VPN tunnel.
			SetDefaultBool("windows.dns.lock", true, LanguageManager.GetText("ManOptionWindowsDnsLock"));
			SetDefaultInt("windows.metrics.tap.ipv4", -2, NotInMan); // 2.13:   0: Windows Automatic, >0 value, -1: Don't change, -2: Automatic
			SetDefaultInt("windows.metrics.tap.ipv6", -2, NotInMan); // 2.13:   0: Windows Automatic, >0 value, -1: Don't change, -2: Automatic
			SetDefaultBool("windows.workarounds", false, NotInMan); // If true, some variants to identify issues
			SetDefaultBool("windows.ipv6.bypass_dns", false, NotInMan); // 2.14: Workaround, skip DNS6.
			SetDefaultBool("windows.ssh.plink.force", true, NotInMan); // Switch to false when stable/tested.
			SetDefaultBool("windows.force_old_driver", false, NotInMan);
			//SetDefaultBool("windows.wintun", true, NotInMan); // 2.21.4 deprecated

			// Linux only
			SetDefault("linux.dns.services", "text", "nscd;dnsmasq;named;bind9;systemd-resolved", NotInMan);

			// General UI
			SetDefault("ui.unit", "text", "", LanguageManager.GetText("ManOptionUiUnit"));
			SetDefaultBool("ui.iec", false, LanguageManager.GetText("ManOptionUiIEC"));
			SetDefaultBool("ui.skip.provider.manifest.failed", false, NotInMan);
			SetDefaultBool("ui.skip.promotional", false, NotInMan);

			// GUI only
			SetDefaultBool("gui.start_minimized", false, NotInMan);
			SetDefaultBool("gui.tray_show", true, NotInMan);
			SetDefaultBool("gui.tray_minimized", (Platform.Instance.IsLinuxSystem() == false), NotInMan); // We can't know if the Linux Desktop Environment will support show tray.
			SetDefaultBool("gui.notifications", true, NotInMan);
			SetDefaultBool("gui.exit_confirm", true, NotInMan, false);
			SetDefault("gui.font.normal.name", "text", "", NotInMan);
			SetDefaultFloat("gui.font.normal.size", 0, NotInMan);

			SetDefault("gui.window.main", "text", "", NotInMan, false);
			SetDefault("gui.list.servers", "text", "", NotInMan, false);
			SetDefault("gui.list.areas", "text", "", NotInMan, false);
			SetDefault("gui.list.logs", "text", "", NotInMan, false);

			// UI - OSX Only
			// SetDefaultBool("gui.osx.dock", false, NotInMan); // See this FAQ: https://airvpn.org/topic/13331-its-possible-to-hide-the-icon-in-dock-bar-under-os-x/
			SetDefaultBool("gui.osx.visible", false, NotInMan);
			SetDefaultBool("gui.osx.sysbar.show_info", false, NotInMan);
			SetDefaultBool("gui.osx.sysbar.show_speed", false, NotInMan); // Menu Status, Window Title, Tray Tooltip
			SetDefaultBool("gui.osx.sysbar.show_server", false, NotInMan);

			// Internal only
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
			SetDefault("event." + name + ".filename", "path_file", "", LanguageManager.GetText("ManOptionEventFileName"));
			SetDefault("event." + name + ".arguments", "text", "", LanguageManager.GetText("ManOptionEventArguments"));
			SetDefaultBool("event." + name + ".waitend", true, LanguageManager.GetText("ManOptionEventWaitEnd"));
		}

		public void ResetAll(bool force)
		{
			foreach (Option option in Dict.Values)
			{
				if ((force == false) && (option.DontUserReset))
					continue;

				option.Value = "";
			}
		}

		public void OnChange(string name)
		{
			if (name == "tools.openvpn.path")
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

			foreach (KeyValuePair<string, Option> kp in m_options)
			{
				Json jOption = kp.Value.GetJson();
				jOption["value"].Value = Get(kp.Key);
				j[kp.Key].Value = jOption;
			}

			return j;
		}
	}
}
