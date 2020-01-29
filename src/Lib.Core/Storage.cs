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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace Eddie.Core
{
	public class Storage
	{
		public string SavePath = "";        
        public string SaveFormat = "v1n";
		public string SavePassword = "";

		public string Id = "";

		public XmlElement Providers;

		private Dictionary<string, Option> m_options = new Dictionary<string, Option>();

		private string m_loadFormat = "";
		private string m_loadPassword = "";

		public Storage()
		{
			Id = RandomGenerator.GetRandomId64();

			EnsureDefaults();

			if( (Platform.Instance.OsCredentialSystemDefault()) && (Platform.Instance.OsCredentialSystemName() != "") )
			{
				SaveFormat = "v2s"; // Os
			}
			else
			{
				SaveFormat = "v2n"; // None
			}
						
			XmlDocument xmlDoc = new XmlDocument();
			Providers = xmlDoc.CreateElement("providers");
		}

		public string LoadPassword
		{
			get
			{
				return m_loadPassword;
			}
		}

		public Dictionary<string, Option> Options
		{
			get
			{
				return m_options;
			}
		}

		public string GetReportForSupport()
		{
			string result = "";
			foreach (Option option in Options.Values)
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

			foreach (Option option in Options.Values)
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
				o = ".\\\"" + LanguageManager.GetText("ManHeaderComment") + "\n.TH eddie-ui 8 \"" + DateTime.UtcNow.ToString(CultureInfo.InvariantCulture) + "\"" + o;

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
			return m_options.ContainsKey(name);
		}

		public string Get(string name)
		{
			lock (Options)
			{
				if (Engine.Instance.StartCommandLine.Exists(name))
				{
					return Engine.Instance.StartCommandLine.Get(name, "");
				}
				else if (Exists(name))
				{
					Option option = Options[name];
					if (option.Value != "")
						return option.Value;
					else
						return option.Default;
				}
				else
				{
					Engine.Instance.Logs.Log(LogType.Error, LanguageManager.GetText("OptionsUnknown", name));
					return "";
				}
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
			lock (this)
			{
				if (Exists(name) == false)
					Engine.Instance.Logs.Log(LogType.Warning, LanguageManager.GetText("OptionsUnknown", name));
				else
					Options[name].Value = val;
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
			if (Options[name].Type == "text")
				Set(name, s);
			else if (Options[name].Type.StartsWith("choice:"))
				Set(name, s);
			else if (Options[name].Type.StartsWith("path_file"))
				Set(name, s);
			else if (Options[name].Type == "bool")
				Set(name, Conversions.ToBool(s).ToString(CultureInfo.InvariantCulture));
			else if (Options[name].Type == "int")
				Set(name, Conversions.ToInt32(s).ToString(CultureInfo.InvariantCulture));
			else if (Options[name].Type == "float")
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

			SetDefault("login", "text", "", LanguageManager.GetText("ManOptionLogin"));
			SetDefault("password", "password", "", LanguageManager.GetText("ManOptionPassword"));
			SetDefaultBool("remember", false, LanguageManager.GetText("ManOptionRemember"));
			SetDefault("key", "text", "Default", LanguageManager.GetText("ManOptionKey"));
			SetDefault("server", "text", "", LanguageManager.GetText("ManOptionServer"));
			SetDefaultBool("connect", false, LanguageManager.GetText("ManOptionConnect"));
			SetDefaultBool("netlock", false, LanguageManager.GetText("ManOptionNetLock"));

			SetDefault("updater.channel", "choice:stable,beta,internal,none", "stable", LanguageManager.GetText("ManOptionUpdaterChannel"));

			SetDefault("servers.last", "text", "", NotInMan, false);
			SetDefault("servers.whitelist", "text", "", LanguageManager.GetText("ManOptionServersWhiteList"));
			SetDefault("servers.blacklist", "text", "", LanguageManager.GetText("ManOptionServersBlackList"));
			SetDefaultBool("servers.startlast", false, LanguageManager.GetText("ManOptionServersStartLast"));
			SetDefaultBool("servers.locklast", false, LanguageManager.GetText("ManOptionServersLockLast"));
			SetDefault("servers.scoretype", "choice:Speed,Latency", "Speed", LanguageManager.GetText("ManOptionServersScoreType"));

			SetDefault("areas.whitelist", "text", "", LanguageManager.GetText("ManOptionAreasWhiteList"));
			SetDefault("areas.blacklist", "text", "", LanguageManager.GetText("ManOptionAreasBlackList"));

			SetDefault("discover.ip_webservice.list", "text", "https://ipleak.net/json/{@ip};https://freegeoip.net/json/{@ip};http://ip-api.com/json/{@ip}", NotInMan);
			SetDefaultBool("discover.ip_webservice.first", true, NotInMan);
			SetDefaultInt("discover.interval", 60 * 60 * 24, NotInMan); // Delta between refresh discover data (country and other data) for OpenVPN connections.
			SetDefaultBool("discover.exit", true, NotInMan);

			SetDefaultBool("log.file.enabled", false, NotInMan);
			SetDefault("log.file.encoding", "encoding", "utf-8", NotInMan);
			SetDefault("log.file.path", "text", "logs/eddie_%y-%m-%d.log", NotInMan);
			SetDefaultBool("log.level.debug", false, NotInMan);
			SetDefaultBool("log.repeat", false, NotInMan);
			SetDefaultInt("log.limit", 1000, NotInMan);

			SetDefault("language.iso", "text", "auto", LanguageManager.GetText("ManOptionLanguageIso"));

			SetDefault("mode.protocol", "text", "AUTO", LanguageManager.GetText("ManOptionModeProtocol"));
			SetDefaultInt("mode.port", 443, LanguageManager.GetText("ManOptionModePort"));
			SetDefaultInt("mode.alt", 0, LanguageManager.GetText("ManOptionModeAlt"));

			SetDefault("proxy.mode", "text", "None", LanguageManager.GetText("ManOptionProxyMode"));
			SetDefault("proxy.when", "choice:always/web/openvpn/none", "always", NotInMan);
			SetDefault("proxy.host", "ip", "127.0.0.1", LanguageManager.GetText("ManOptionProxyHost"));
			SetDefaultInt("proxy.port", 8080, LanguageManager.GetText("ManOptionProxyPort"));
			SetDefault("proxy.auth", "text", "None", LanguageManager.GetText("ManOptionProxyAuth"));
			SetDefault("proxy.login", "text", "", LanguageManager.GetText("ManOptionProxyLogin"));
			SetDefault("proxy.password", "password", "", LanguageManager.GetText("ManOptionProxyPassword"));
			SetDefaultInt("proxy.tor.control.port", 9151, LanguageManager.GetText("ManOptionProxyTorControlPort"));
			SetDefaultBool("proxy.tor.control.auth", true, LanguageManager.GetText("ManOptionProxyTorControlAuth"));
			SetDefault("proxy.tor.path", "", "", NotInMan);
			SetDefault("proxy.tor.control.password", "password", "", LanguageManager.GetText("ManOptionProxyTorControlPassword"));

			SetDefault("routes.custom", "text", "", LanguageManager.GetText("ManOptionRoutesCustom"));
			// SetDefaultBool("routes.remove_default", false, LanguageManager.GetText("ManOptionRoutesRemoveDefault")); // Deprecated in 2.18, issues with DHCP renew.

			SetDefault("dns.mode", "text", "auto", LanguageManager.GetText("ManOptionDnsMode"));
			SetDefault("dns.servers", "text", "", LanguageManager.GetText("ManOptionDnsServers"));
			SetDefaultBool("dns.check", true, LanguageManager.GetText("ManOptionDnsCheck"));
			SetDefaultInt("dns.cache.ttl", 3600, NotInMan);

			SetDefault("netlock.mode", "text", "auto", LanguageManager.GetText("ManOptionNetLockMode"));
			SetDefaultBool("netlock.allow_private", true, LanguageManager.GetText("ManOptionNetLockAllowPrivate"));
			SetDefaultBool("netlock.allow_dhcp", true, LanguageManager.GetText("ManOptionNetLockAllowDHCP")); // Win only
			SetDefaultBool("netlock.allow_ping", true, LanguageManager.GetText("ManOptionNetLockAllowPing"));
			SetDefaultBool("netlock.allow_dns", false, LanguageManager.GetText("ManOptionNetLockAllowDNS"));
			SetDefault("netlock.incoming", "choice:allow,block", "block", NotInMan);
			SetDefault("netlock.outgoing", "choice:allow,block", "block", NotInMan);
			SetDefault("netlock.whitelist.incoming.ips", "text", "", LanguageManager.GetText("ManOptionNetLockWhitelistIncomingIps"));
			SetDefault("netlock.whitelist.outgoing.ips", "text", "", LanguageManager.GetText("ManOptionNetLockWhitelistOutgoingIps"));

			SetDefault("network.entry.iface", "text", "", NotInMan);
			SetDefault("network.entry.iplayer", "text", "ipv4-ipv6", NotInMan); // ipv6-ipv4;ipv4-ipv6;ipv4-only;ipv6-only;
			SetDefault("network.ipv4.mode", "choice:in,in-out,in-block,out,block", "in", NotInMan);
			SetDefault("network.ipv6.mode", "choice:in,in-out,in-block,out,block", "in-block", NotInMan);
			SetDefaultBool("network.ipv4.autoswitch", false, NotInMan);
			SetDefaultBool("network.ipv6.autoswitch", true, NotInMan);
			SetDefault("network.gateways.default_skip_types", "text", "Loopback;Tunnel", NotInMan);

			SetDefault("tools.openvpn.path", "path_file", "", LanguageManager.GetText("ManOptionToolsOpenVpnPath"));
			SetDefaultBool("tools.hummingbird.preferred", false, NotInMan);
			SetDefault("tools.hummingbird.path", "path_file", "", NotInMan);
            SetDefault("tools.ssh.path", "path_file", "", LanguageManager.GetText("ManOptionToolsSshPath"));
			SetDefault("tools.ssl.path", "path_file", "", LanguageManager.GetText("ManOptionToolsSslPath"));
			SetDefault("tools.curl.path", "path_file", "", LanguageManager.GetText("ManOptionToolsCurlPath"));

			SetDefaultInt("tools.curl.max-time", 20, NotInMan);

			SetDefaultBool("webui.enabled", true, NotInMan); // WebUI it's a Eddie 3.* feature not yet committed on GitHub.
			SetDefault("webui.ip", "text", "127.0.0.1", NotInMan); // Messages.ManOptionWebUiAddress
			SetDefaultInt("webui.port", 4649, NotInMan); // Messages.ManOptionWebUiPort

			SetDefaultBool("external.rules.recommended", true, NotInMan);
			SetDefault("external.rules", "json", "[]", NotInMan);

			SetDefault("openvpn.custom", "text", "", LanguageManager.GetText("ManOptionOpenVpnCustom"));
			SetDefault("openvpn.dev_node", "text", "", LanguageManager.GetText("ManOptionOpenVpnDevNode"));
			SetDefaultInt("openvpn.sndbuf", -2, LanguageManager.GetText("ManOptionOpenVpnSndBuf")); // 2.11
			SetDefaultInt("openvpn.rcvbuf", -2, LanguageManager.GetText("ManOptionOpenVpnRcvBuf")); // 2.11
			SetDefault("openvpn.directives", "text", "client\r\ndev tun\r\nauth-nocache\r\nresolv-retry infinite\r\nnobind\r\npersist-key\r\npersist-tun\r\nverb 3\r\nconnect-retry-max 1\r\nping 10\r\nping-exit 32\r\nexplicit-exit-notify 5", LanguageManager.GetText("ManOptionOpenVpnDirectives"));
			SetDefault("openvpn.directives.path", "path_file", "", NotInMan);
			//SetDefaultBool("openvpn.allow.script-security", false, NotInMan);
			SetDefaultBool("openvpn.skip_defaults", false, LanguageManager.GetText("ManOptionOpenVpnSkipDefaults"));

			// Not in Settings
			SetDefaultInt("openvpn.management_port", 3100, LanguageManager.GetText("ManOptionOpenVpnManagementPort"));
			SetDefaultInt("ssh.port", 0, LanguageManager.GetText("ManOptionSshPort"));
			SetDefaultInt("ssl.port", 0, LanguageManager.GetText("ManOptionSslPort"));
			SetDefault("ssl.options", "text", "", NotInMan); // "NO_SSLv2" < 2.11.10
			SetDefaultInt("ssl.verify", -1, NotInMan);

			SetDefaultBool("os.single_instance", true, LanguageManager.GetText("ManOptionOsSingleInstance"));

			SetDefaultBool("advanced.expert", false, LanguageManager.GetText("ManOptionAdvancedExpert"));
			SetDefaultBool("advanced.check.route", true, LanguageManager.GetText("ManOptionAdvancedCheckRoute"));

			SetDefaultInt("advanced.penality_on_error", 30, NotInMan);

			SetDefaultBool("pinger.enabled", true, LanguageManager.GetText("ManOptionAdvancedPingerEnabled"));
			SetDefaultInt("pinger.delay", 0, LanguageManager.GetText("ManOptionAdvancedPingerDelay"));
			SetDefaultInt("pinger.retry", 0, LanguageManager.GetText("ManOptionAdvancedPingerRetry"));
			SetDefaultInt("pinger.jobs", 10, LanguageManager.GetText("ManOptionAdvancedPingerJobs"));
			SetDefaultInt("pinger.valid", 0, LanguageManager.GetText("ManOptionAdvancedPingerValid"));

			SetDefaultInt("advanced.manifest.refresh", -1, NotInMan);
			SetDefaultBool("advanced.providers", false, NotInMan);

			SetDefault("bootstrap.urls", "text", "", NotInMan); // ClodoTemp: move to provider level

			SetDefaultBool("advanced.skip_tun_detect", false, NotInMan); // Skip TUN driver detection.
			SetDefaultBool("advanced.skip_alreadyrun", false, NotInMan); // Continue even if openvpn is already running.
			SetDefaultBool("connections.allow_anyway", false, NotInMan); // Allow connection even if in 'Not available' status.
			SetDefaultBool("advanced.testonly", false, NotInMan); // Disconnect when connection occur.

			EnsureDefaultsEvent("app.start");
			EnsureDefaultsEvent("app.stop");
			EnsureDefaultsEvent("session.start");
			EnsureDefaultsEvent("session.stop");
			EnsureDefaultsEvent("vpn.pre");
			EnsureDefaultsEvent("vpn.up");
			EnsureDefaultsEvent("vpn.down");

			// Windows only			
			SetDefault("windows.adapter_service", "text", "tap0901", LanguageManager.GetText("ManOptionWindowsAdapterService"));
			SetDefaultBool("windows.disable_driver_upgrade", false, LanguageManager.GetText("ManOptionWindowsDisableDriverUpgrade"));
			SetDefaultBool("windows.tap_up", true, LanguageManager.GetText("ManOptionWindowsTapUp"));
			//SetDefaultBool("windows.dhcp_disable", false, LanguageManager.GetText("ManOptionWindowsDhcpDisable")); // Deprecated in 2.18
			SetDefaultBool("windows.wfp.enable", true, LanguageManager.GetText("ManOptionWindowsWfp"));
			SetDefaultBool("windows.wfp.dynamic", false, LanguageManager.GetText("ManOptionWindowsWfpDynamic"));
			//SetDefaultBool("windows.ipv6.os_disable", false, Messages.ManOptionWindowsIPv6DisableAtOs); // Must be default FALSE if WFP works well // Removed in 2.14, in W10 require reboot
			SetDefaultBool("windows.dns.force_all_interfaces", false, LanguageManager.GetText("ManOptionWindowsDnsForceAllInterfaces")); // Important: With WFP can be false, but users report DNS leak. Maybe not a real DNS Leak, simply request on DNS of other interfaces through VPN tunnel.
			SetDefaultBool("windows.dns.lock", true, LanguageManager.GetText("ManOptionWindowsDnsLock"));
			SetDefaultInt("windows.metrics.tap.ipv4", -2, NotInMan); // 2.13:   0: Windows Automatic, >0 value, -1: Don't change, -2: Automatic
			SetDefaultInt("windows.metrics.tap.ipv6", -2, NotInMan); // 2.13:   0: Windows Automatic, >0 value, -1: Don't change, -2: Automatic
			SetDefaultBool("windows.workarounds", false, NotInMan); // If true, some variants to identify issues
			SetDefaultBool("windows.ipv6.bypass_dns", false, NotInMan); // 2.14: Workaround, skip DNS6.
			SetDefaultBool("windows.ssh.plink.force", true, NotInMan); // Switch to false when stable/tested.

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
			Options["gui.window.main"].InternalOnly = true;
			Options["gui.list.servers"].InternalOnly = true;
			Options["gui.list.areas"].InternalOnly = true;
			Options["gui.list.logs"].InternalOnly = true;

			// Omissis
			Options["login"].Omissis = true;
			Options["password"].Omissis = true;
			Options["key"].Omissis = true;
			Options["proxy.login"].Omissis = true;
			Options["proxy.password"].Omissis = true;
			Options["proxy.tor.control.password"].Omissis = true;

			// Don't clean with user Reset All
			Options["login"].DontUserReset = true;
			Options["password"].DontUserReset = true;
			Options["remember"].DontUserReset = true;
			Options["key"].DontUserReset = true;

			// Exceptions
			// TOFIX: Some users report a stuck in 'Checking environment' phase on macOS only, Eddie 2.13.4 and above.
			if (Platform.Instance.GetCode() == "MacOS")
				Options["advanced.skip_alreadyrun"].Default = "True";
		}

		public void EnsureDefaultsEvent(string name)
		{
			SetDefault("event." + name + ".filename", "path_file", "", LanguageManager.GetText("ManOptionEventFileName"));
			SetDefault("event." + name + ".arguments", "text", "", LanguageManager.GetText("ManOptionEventArguments"));
			SetDefaultBool("event." + name + ".waitend", true, LanguageManager.GetText("ManOptionEventWaitEnd"));
		}

		public void ResetAll(bool force)
		{
			foreach (Option option in Options.Values)
			{
				if ((force == false) && (option.DontUserReset))
					continue;

				option.Value = "";
			}
		}

		public void Save()
		{
			bool remember = GetBool("remember");

			lock (this)
			{
				try
				{
					XmlDocument xmlDoc = new XmlDocument();
					XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);

					XmlElement rootNode = xmlDoc.CreateElement("eddie");
					xmlDoc.InsertBefore(xmlDeclaration, xmlDoc.DocumentElement);

					XmlElement optionsNode = xmlDoc.CreateElement("options");
					rootNode.AppendChild(optionsNode);

					xmlDoc.AppendChild(rootNode);

					foreach (Option option in Options.Values)
					{
						bool skip = false;

						if ((remember == false) && (option.Code == "login"))
							skip = true;
						if ((remember == false) && (option.Code == "password"))
							skip = true;

						if ((option.Value == "") || (option.Value == option.Default))
							skip = true;

						if (skip == false)
						{
							XmlElement itemNode = xmlDoc.CreateElement("option");
							itemNode.SetAttribute("name", option.Code);
							itemNode.SetAttribute("value", option.Value);
							optionsNode.AppendChild(itemNode);
						}
					}

					XmlElement providersNode = xmlDoc.CreateElement("providers");
					rootNode.AppendChild(providersNode);
					foreach (Provider provider in Engine.Instance.ProvidersManager.Providers)
					{
						XmlNode providerNode = xmlDoc.ImportNode(provider.Storage.DocumentElement, true);
						providersNode.AppendChild(providerNode);
					}

					if (Engine.Instance.ProvidersManager.Providers.Count == 1)
					{
						if (Engine.Instance.ProvidersManager.Providers[0].Code == "AirVPN")
						{
							// Move providers->AirVPN to root.
							XmlElement xmlAirVPN = providersNode.GetFirstElementByTagName("AirVPN");
							if (xmlAirVPN != null)
							{
								foreach (XmlElement xmlChild in xmlAirVPN.ChildNodes)
									ExtensionsXml.XmlCopyElement(xmlChild, xmlDoc.DocumentElement);
								providersNode.RemoveChild(xmlAirVPN);
							}
							if (providersNode.ChildNodes.Count == 0)
								providersNode.ParentNode.RemoveChild(providersNode);
						}
					}

					// Compute password
					if ((SaveFormat == "v2s") && (Platform.Instance.OsCredentialSystemName() == ""))
						SaveFormat = "v2n";

					if ((Platform.Instance.OsCredentialSystemName() != "") && (m_loadFormat == "v2s") && (SaveFormat != "v2s"))
						Platform.Instance.OsCredentialSystemDelete(Id);

					if (SaveFormat == "v2n")
						SavePassword = Constants.PasswordIfEmpty;
					else if (SaveFormat == "v2s")
					{
						if( (m_loadFormat != "v2s") || (SavePassword == "") || (SavePassword != m_loadPassword) )
						{
							SavePassword = RandomGenerator.GetRandomPassword();
							if (Platform.Instance.OsCredentialSystemWrite(Id, SavePassword) == false)
							{
								// User not authorize the OS keychain, or fail. Revert to plain mode.
								SaveFormat = "v2n";
								SavePassword = Constants.PasswordIfEmpty;
							}
						}
					}

					byte[] plainData = Encoding.UTF8.GetBytes(xmlDoc.OuterXml);
					byte[] encrypted = Storage.EncodeFormat(SaveFormat, Id, plainData, SavePassword);
					Platform.Instance.FileContentsWriteBytes(SavePath, encrypted);
					Platform.Instance.FileEnsurePermission(SavePath, "600");

					m_loadFormat = SaveFormat;
					m_loadPassword = SavePassword;
				}
				catch (Exception ex)
				{
					Engine.Instance.Logs.Log(LogType.Fatal, LanguageManager.GetText("OptionsWriteFailed", SavePath, ex.Message));
				}
			}
		}

		public bool Load()
		{
			try
			{
				byte[] profileDataEncrypted;
				Storage.DecodeFormat(Platform.Instance.FileContentsReadBytes(SavePath), out m_loadFormat, out Id, out profileDataEncrypted);

				if (m_loadFormat == "v1n")
				{
					// Compatibility format, exists only in version 2.18.1 and 2.18.2, fixed in 2.18.3
					m_loadPassword = Constants.PasswordIfEmpty;					
				}
				else if (m_loadFormat == "v1s")
				{
					// Compatibility format, exists only in version 2.18.1 and 2.18.2, fixed in 2.18.3
					m_loadPassword = Platform.Instance.OsCredentialSystemRead(new FileInfo(SavePath).Name);
					if (m_loadPassword == null)
						m_loadPassword = ""; // Will fail after the decryption					
				}				
				else if (m_loadFormat == "v1p")
				{
					// Compatibility format, exists only in version 2.18.1 and 2.18.2, fixed in 2.18.3
					m_loadPassword = Engine.Instance.OnAskProfilePassword(false);
					if ((m_loadPassword == null) || (m_loadPassword == ""))
						return false;					
				}
				else if (m_loadFormat == "v2n")
				{
					m_loadPassword = Constants.PasswordIfEmpty;
				}
				else if (m_loadFormat == "v2s")
				{
					m_loadPassword = Platform.Instance.OsCredentialSystemRead(Id);
					if (m_loadPassword == null)
						m_loadPassword = ""; // Will fail after the decryption
				}
				else if (m_loadFormat == "v2p")
				{
					m_loadPassword = Engine.Instance.OnAskProfilePassword(false);
					if ((m_loadPassword == null) || (m_loadPassword == ""))
						return false;					
				}

				byte[] decrypted = null;
				for (; ; )
				{
					decrypted = Core.Crypto.Manager.ReadBytesEncrypted(profileDataEncrypted, m_loadPassword);
					if (decrypted == null)
					{
						if ((m_loadFormat == "v1s") || (m_loadFormat == "v2s"))
						{
							// Loses, ask what to do
							bool ask = Engine.Instance.OnAskYesNo(LanguageManager.GetText("OptionsReadNoKeyring"));
							if (ask)
							{
								ResetAll(true);
								return true;
							}
							else
								return false;
						}
						m_loadPassword = Engine.Instance.OnAskProfilePassword(true);
						if ((m_loadPassword == null) || (m_loadPassword == ""))
							return false;
					}
					else
						break;
				}

				SavePassword = m_loadPassword;
				SaveFormat = m_loadFormat;

				// Compatibility
				if (m_loadFormat == "v1n")
					SaveFormat = "v2n";
				else if (m_loadFormat == "v1p")
					SaveFormat = "v2p";
				else if (m_loadFormat == "v1s")
				{
					SaveFormat = "v2s";
					SavePassword = ""; // Will be generated
				}				
						
				LoadInternal(decrypted);
				return true;				
			}
			catch(Exception ex)
			{
				bool ask = Engine.Instance.OnAskYesNo(LanguageManager.GetText("OptionsReadError", ex.Message));
				if (ask)
				{
					ResetAll(true);
					return true;
				}
				else
					return false;
			}
		}

		private void LoadInternal(byte[] plainData)
		{
			lock (this)
			{				
				if (plainData == null)
					throw new Exception("Unknown format");

				XmlDocument xmlDoc = new XmlDocument();

				Providers = xmlDoc.CreateElement("providers");

				// Put the byte array into a stream, rewind it to the beginning and read
				MemoryStream ms = new MemoryStream(plainData);
				ms.Flush();
				ms.Position = 0;
				xmlDoc.Load(ms);

				ResetAll(true);

				Providers = xmlDoc.DocumentElement.GetFirstElementByTagName("providers");
				if (Providers == null)
					Providers = xmlDoc.CreateElement("providers");

				XmlNode nodeOptions = xmlDoc.DocumentElement.GetElementsByTagName("options")[0];
				Dictionary<string, string> options = new Dictionary<string, string>();
				foreach (XmlElement e in nodeOptions)
				{
					string name = e.Attributes["name"].Value;
					string value = e.Attributes["value"].Value;

					CompatibilityManager.FixOption(ref name, ref value);
                    if(name != "")
						options[name] = value;
				}

				CompatibilityManager.FixOptions(options);
				foreach (KeyValuePair<string, string> item in options)
					Set(item.Key, item.Value);

				// For compatibility <3
				XmlElement xmlManifest = xmlDoc.DocumentElement.GetFirstElementByTagName("manifest");
				if (xmlManifest != null)
				{
					XmlElement providerAirVpn = xmlDoc.CreateElement("AirVPN");
					Providers.AppendChild(providerAirVpn);

					ExtensionsXml.XmlCopyElement(xmlManifest, providerAirVpn);

					XmlElement xmlUser = xmlDoc.DocumentElement.GetFirstElementByTagName("user");
					if (xmlUser != null) // Compatibility with old manifest < 2.11
					{
						XmlElement oldKeyFormat = xmlUser.SelectSingleNode("keys/key[@id='default']") as XmlElement;
						if (oldKeyFormat != null)
						{
							oldKeyFormat.SetAttribute("name", "Default");
						}
					}
					if (xmlUser != null)
						ExtensionsXml.XmlCopyElement(xmlUser, providerAirVpn);
				}
			}
		}

		public static byte[] EncodeFormat(string header, string id, byte[] dataPlain, string password)
		{
			if (header.Length != 3)
				throw new Exception("Unexpected");
			if (header.StartsWithInv("v1"))
				throw new Exception("Unexpected");

			byte[] encrypted = Core.Crypto.Manager.WriteBytesEncrypted(dataPlain, password);

			byte[] n = null;			
			n = new byte[3 + 64 + encrypted.Length];
			Encoding.ASCII.GetBytes(header).CopyTo(n, 0);
			Encoding.ASCII.GetBytes(id).CopyTo(n, 3);
			encrypted.CopyTo(n, 3+64);
			
			return n;
		}

		public static void DecodeFormat(byte[] b, out string header, out string id, out byte[] dataEncrypted)
		{
			byte[] bHeader = new byte[3];			
			Array.Copy(b, 0, bHeader, 0, 3);
			header = Encoding.ASCII.GetString(bHeader);
			if(header.StartsWithInv("v1"))
			{
				// Compatibility				
				id = RandomGenerator.GetRandomId64();
				dataEncrypted = new byte[b.Length - 3];
				Array.Copy(b, 3, dataEncrypted, 0, b.Length - 3);
			}
			else if (header.StartsWith("v2"))
			{
				byte[] bId = new byte[64];
				Array.Copy(b, 3, bId, 0, 64);
				id = Encoding.ASCII.GetString(bId);
				dataEncrypted = new byte[b.Length - 3 - 64];
				Array.Copy(b, 3+64, dataEncrypted, 0, b.Length - 3 - 64);				
			}
			else
			{
				throw new Exception("Read fail");
			}
		}
	}
}
