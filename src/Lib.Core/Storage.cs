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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using Eddie.Common;

namespace Eddie.Core
{
	public class Storage
	{
		public static bool Simulate = false; // If true, connections not really maded. Useful only during development of UI.

		public XmlElement Providers;

		private Dictionary<string, Option> m_options = new Dictionary<string, Option>();
		private string m_pathProfile = "";
		private string m_pathData = "";

		public Storage()
		{
			EnsureDefaults();

			m_pathProfile = Get("profile");
			m_pathData = Get("path");

			string pathApp = Platform.Instance.GetApplicationPath();
			string pathUser = Platform.Instance.GetUserPath();

			// Compute data path
			if (m_pathData == "")
				m_pathData = Platform.Instance.GetDefaultDataPath();

			if (m_pathData == "home")
				m_pathData = Platform.Instance.GetUserPath();
			else if (m_pathData == "program")
				m_pathData = Platform.Instance.GetApplicationPath();
			else if (m_pathData == "") // Detect
			{
				if (Platform.Instance.HasAccessToWrite(pathApp))
					m_pathData = pathApp;
				else
					m_pathData = pathUser;
			}

			// Compute profile
			if (Platform.Instance.IsPath(m_pathProfile))
			{
				// Is a path
				FileInfo fi = new FileInfo(Platform.Instance.NormalizePath(m_pathProfile));
				if (Get("path") == "")
					m_pathData = fi.DirectoryName;
				m_pathProfile = fi.FullName;
			}
			else
			{
				m_pathProfile = m_pathData + Platform.Instance.DirSep + m_pathProfile;
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
			o += "\t" + Messages.ManName.Replace("\n", "[br]");
			o += "\n\n[sh]SYNOPSIS[/sh]\n";
			o += "\t" + Messages.ManSynopsis.Replace("\n", "[br]");
			o += "\n\n[sh]DESCRIPTION[/sh]\n";
			o += "\t" + Messages.ManDescription.Replace("\n", "[br]");
			o += "\n\n[sh]OPTIONS[/sh]\n";
			o += "\t" + Messages.ManHeaderOption1.Replace("\n", "[br]");
			o += "\t" + Messages.ManHeaderOption2.Replace("\n", "[br]");
			o += "\t" + Messages.ManHeaderOption3.Replace("\n", "[br]");
			o += "\t" + Messages.ManHeaderOption4.Replace("\n", "[br]");
			o += "\t[options_list]" + body.Replace("\n", "\n\t") + "[/options_list]";
			o += "\n\n[sh]COPYRIGHT[/sh]\n";
			o += "\t" + Messages.ManCopyright.Replace("\n", "[br]");
			o += "\n";

			if (format == "man")
			{
				// Escape dot that can go at beginning of line
				o = o.Replace("].", "]\\[char46]");

				// Header
				o = ".\\\"" + Messages.ManHeaderComment + "\n.TH eddie-ui 8 \"" + DateTime.UtcNow.ToString(CultureInfo.InvariantCulture) + "\"" + o;

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
				for (;;)
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
				if (CommandLine.SystemEnvironment.Exists(name))
				{
					return CommandLine.SystemEnvironment.Get(name, "");
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
					Engine.Instance.Logs.Log(LogType.Error, MessagesFormatter.Format(Messages.OptionsUnknown, name));
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

		public void Set(string name, string val)
		{
			lock (this)
			{
				if (Exists(name) == false)
					Engine.Instance.Logs.Log(LogType.Warning, MessagesFormatter.Format(Messages.OptionsUnknown, name));
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

		public string GetDataPath()
		{
			return m_pathData;
		}

		public string GetProfilePath()
		{
			return m_pathProfile;
		}

		public string GetPathInData(string filename)
		{
			return m_pathData + Platform.Instance.DirSep + filename;
		}

		public void EnsureDefaults()
		{
			string NotInMan = ""; // Option not listed in 'man' documentation.

			SetDefaultBool("cli", false, Messages.ManOptionCli);
			SetDefaultBool("version", false, NotInMan);
			SetDefaultBool("version.short", false, NotInMan);
			SetDefaultBool("help", false, Messages.ManOptionHelp);
			SetDefaultBool("test.cli-su", false, NotInMan); // ClodoTemp, only for testing
			SetDefault("help.format", "choice:text,bbcode,html,man", "text", Messages.ManOptionHelpFormat); // Maybe 'text' or 'bbcode' or 'html' or 'man'.
			SetDefaultBool("batch", false, NotInMan); // Don't lock interface, exit when connection is closed.            
			SetDefault("login", "text", "", Messages.ManOptionLogin);
			SetDefault("password", "password", "", Messages.ManOptionPassword);
			SetDefaultBool("remember", false, Messages.ManOptionRemember);
			SetDefault("key", "text", "Default", Messages.ManOptionKey);
			SetDefault("server", "text", "", Messages.ManOptionServer);
			SetDefaultBool("connect", false, Messages.ManOptionConnect);
			SetDefaultBool("netlock", false, Messages.ManOptionNetLock);

			SetDefault("console.mode", "choice:none,batch,keys", "keys", NotInMan);

			SetDefault("profile", "text", "default.xml", Messages.ManOptionProfile); // Not in Settings
			SetDefault("path", "text", "", Messages.ManOptionPath); // Not in Settings // Path. Maybe a full path, or special values 'home' or 'program'.			
			SetDefault("path.resources", "text", "res/", NotInMan); // Relative to executable
			SetDefault("path.tools", "text", "", NotInMan); // Relative to executable
			SetDefault("path.exec", "text", "", NotInMan); // Original execution file

			SetDefault("servers.last", "text", "", NotInMan, false);
			SetDefault("servers.whitelist", "text", "", Messages.ManOptionServersWhiteList);
			SetDefault("servers.blacklist", "text", "", Messages.ManOptionServersBlackList);
			SetDefaultBool("servers.startlast", false, Messages.ManOptionServersStartLast);
			SetDefaultBool("servers.locklast", false, Messages.ManOptionServersLockLast);
			SetDefault("servers.scoretype", "choice:Speed,Latency", "Speed", Messages.ManOptionServersScoreType);

			SetDefault("areas.whitelist", "text", "", Messages.ManOptionAreasWhiteList);
			SetDefault("areas.blacklist", "text", "", Messages.ManOptionAreasBlackList);

			SetDefault("discover.ip_webservice.list", "text", "https://ipleak.net/json/{@ip};https://freegeoip.net/json/{@ip};http://ip-api.com/json/{@ip}", NotInMan);
			SetDefaultBool("discover.ip_webservice.first", true, NotInMan);
			SetDefaultInt("discover.interval", 60 * 60 * 24, NotInMan); // Delta between refresh discover data (country and other data) for OpenVPN connections.
			SetDefaultBool("discover.exit", true, NotInMan);

			SetDefaultBool("log.file.enabled", false, NotInMan);
			SetDefault("log.file.encoding", "encoding", "utf-8", NotInMan);
			SetDefault("log.file.path", "text", "logs/eddie_%y-%m-%d.log", NotInMan);
			SetDefaultBool("log.level.debug", false, NotInMan);
			SetDefaultBool("log.repeat", false, NotInMan);

			SetDefault("mode.protocol", "text", "AUTO", Messages.ManOptionModeProtocol);
			SetDefaultInt("mode.port", 443, Messages.ManOptionModePort);
			SetDefaultInt("mode.alt", 0, Messages.ManOptionModeAlt);

			SetDefault("proxy.mode", "text", "None", Messages.ManOptionProxyMode);
			SetDefault("proxy.when", "choice:always/web/openvpn/none", "always", NotInMan);
			SetDefault("proxy.host", "ip", "127.0.0.1", Messages.ManOptionProxyHost);
			SetDefaultInt("proxy.port", 8080, Messages.ManOptionProxyPort);
			SetDefault("proxy.auth", "text", "None", Messages.ManOptionProxyAuth);
			SetDefault("proxy.login", "text", "", Messages.ManOptionProxyLogin);
			SetDefault("proxy.password", "password", "", Messages.ManOptionProxyPassword);
			SetDefaultInt("proxy.tor.control.port", 9151, Messages.ManOptionProxyTorControlPort);
			SetDefaultBool("proxy.tor.control.auth", true, Messages.ManOptionProxyTorControlAuth);
			SetDefault("proxy.tor.control.cookie-path", "", "", NotInMan);
			SetDefault("proxy.tor.control.password", "password", "", Messages.ManOptionProxyTorControlPassword);

			SetDefault("routes.default", "choice:in,out", "in", Messages.ManOptionRoutesDefault);
			SetDefault("routes.custom", "text", "", Messages.ManOptionRoutesCustom);
			SetDefaultBool("routes.remove_default", false, Messages.ManOptionRoutesRemoveDefault); // Will be probably deprecated, issues with DHCP renew.

			SetDefault("dns.mode", "text", "auto", Messages.ManOptionDnsMode);
			SetDefault("dns.servers", "text", "", Messages.ManOptionDnsServers);
			SetDefaultBool("dns.check", true, Messages.ManOptionDnsCheck);
			SetDefaultInt("dns.cache.ttl", 3600, NotInMan);

			SetDefault("netlock.mode", "text", "auto", Messages.ManOptionNetLockMode);
			SetDefaultBool("netlock.allow_private", true, Messages.ManOptionNetLockAllowPrivate);
			SetDefaultBool("netlock.allow_ping", true, Messages.ManOptionNetLockAllowPing);
			SetDefaultBool("netlock.allow_dns", false, Messages.ManOptionNetLockAllowDNS);
			SetDefault("netlock.incoming", "choice:allow,block", "block", NotInMan);
			SetDefault("netlock.outgoing", "choice:allow,block", "block", NotInMan);
			SetDefault("netlock.allowed_ips", "text", "", Messages.ManOptionNetLockAllowedsIps);

			SetDefault("ipv6.mode", "text", "disable", Messages.ManOptionIPv6);

			SetDefault("network.entry.iface", "text", "", NotInMan);
			SetDefault("network.entry.iplayer", "text", "ipv4-ipv6", NotInMan); // ipv6-ipv4;ipv4-ipv6;ipv4-only;ipv6-only;
			SetDefault("network.ipv4.mode", "choice:in,in-out,in-block,out,block", "in", NotInMan);
			SetDefault("network.ipv6.mode", "choice:in,in-out,in-block,out,block", "in-block", NotInMan);
			SetDefaultBool("network.ipv4.autoswitch", false, NotInMan);
			SetDefaultBool("network.ipv6.autoswitch", true, NotInMan);

			SetDefault("tools.openvpn.path", "path_file", "", Messages.ManOptionToolsOpenVpnPath);
			SetDefault("tools.ssh.path", "path_file", "", Messages.ManOptionToolsSshPath);
			SetDefault("tools.ssl.path", "path_file", "", Messages.ManOptionToolsSslPath);
			SetDefault("tools.curl.path", "path_file", "", Messages.ManOptionToolsCurlPath);

			SetDefaultInt("tools.curl.max-time", 20, NotInMan);

			SetDefault("openvpn.custom", "text", "", Messages.ManOptionOpenVpnCustom);
			SetDefault("openvpn.dev_node", "text", "", Messages.ManOptionOpenVpnDevNode);
			SetDefaultInt("openvpn.sndbuf", -2, Messages.ManOptionOpenVpnSndBuf); // 2.11
			SetDefaultInt("openvpn.rcvbuf", -2, Messages.ManOptionOpenVpnRcvBuf); // 2.11
			SetDefault("openvpn.directives", "text", "client\r\ndev tun\r\nauth-nocache\r\nresolv-retry infinite\r\nnobind\r\npersist-key\r\npersist-tun\r\nverb 3\r\nconnect-retry-max 1\r\nping 10\r\nping-exit 32\r\nexplicit-exit-notify 5", Messages.ManOptionOpenVpnDirectives);
			SetDefault("openvpn.directives.path", "path_file", "", NotInMan);
			SetDefaultBool("openvpn.skip_defaults", false, Messages.ManOptionOpenVpnSkipDefaults);

			// Not in Settings
			SetDefaultInt("openvpn.management_port", 3100, Messages.ManOptionOpenVpnManagementPort);
			SetDefaultInt("ssh.port", 0, Messages.ManOptionSshPort);
			SetDefaultInt("ssl.port", 0, Messages.ManOptionSslPort);
			SetDefault("ssl.options", "text", "", NotInMan); // "NO_SSLv2" < 2.11.10
			SetDefaultInt("ssl.verify", -1, NotInMan);

			SetDefaultBool("os.single_instance", true, Messages.ManOptionOsSingleInstance);

			SetDefaultBool("advanced.expert", false, Messages.ManOptionAdvancedExpert);
			SetDefaultBool("advanced.check.route", true, Messages.ManOptionAdvancedCheckRoute);

			SetDefaultInt("advanced.penality_on_error", 30, NotInMan);

			SetDefaultBool("pinger.enabled", true, Messages.ManOptionAdvancedPingerEnabled);
			SetDefaultInt("pinger.delay", 0, Messages.ManOptionAdvancedPingerDelay);
			SetDefaultInt("pinger.retry", 0, Messages.ManOptionAdvancedPingerRetry);
			SetDefaultInt("pinger.jobs", 10, Messages.ManOptionAdvancedPingerJobs);
			SetDefaultInt("pinger.valid", 0, Messages.ManOptionAdvancedPingerValid);

			SetDefaultInt("advanced.manifest.refresh", -1, NotInMan);
			SetDefaultBool("advanced.providers", false, NotInMan);

			SetDefault("bootstrap.urls", "text", "", NotInMan); // ClodoTemp: move to provider level

			SetDefaultBool("advanced.skip_privileges", false, NotInMan); // Skip 'root' detection.
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
			SetDefault("windows.adapter_service", "text", "tap0901", Messages.ManOptionWindowsAdapterService);
			SetDefaultBool("windows.disable_driver_upgrade", false, Messages.ManOptionWindowsDisableDriverUpgrade);
			SetDefaultBool("windows.tap_up", true, Messages.ManOptionWindowsTapUp);
			SetDefaultBool("windows.dhcp_disable", false, Messages.ManOptionWindowsDhcpDisable);
			SetDefaultBool("windows.wfp.enable", true, Messages.ManOptionWindowsWfp);
			SetDefaultBool("windows.wfp.dynamic", false, Messages.ManOptionWindowsWfpDynamic);
			//SetDefaultBool("windows.ipv6.os_disable", false, Messages.ManOptionWindowsIPv6DisableAtOs); // Must be default FALSE if WFP works well // Removed in 2.14, in W10 require reboot
			SetDefaultBool("windows.dns.force_all_interfaces", false, Messages.ManOptionWindowsDnsForceAllInterfaces); // Important: With WFP can be false, but users report DNS leak. Maybe not a real DNS Leak, simply request on DNS of other interfaces through VPN tunnel.
			SetDefaultBool("windows.dns.lock", true, Messages.ManOptionWindowsDnsLock);
			SetDefaultInt("windows.metrics.tap.ipv4", -2, NotInMan); // 2.13:   0: Windows Automatic, >0 value, -1: Don't change, -2: Automatic
			SetDefaultInt("windows.metrics.tap.ipv6", -2, NotInMan); // 2.13:   0: Windows Automatic, >0 value, -1: Don't change, -2: Automatic
			SetDefaultBool("windows.workarounds", false, NotInMan); // If true, some variants to identify issues
			SetDefaultBool("windows.ipv6.bypass_dns", false, NotInMan); // 2.14: Workaround, skip DNS6.
			SetDefaultBool("windows.ssh.plink.force", true, NotInMan); // Switch to false when stable/tested.

			// Linux only
			SetDefaultBool("linux.xhost", false, NotInMan);
			SetDefault("linux.dbus", "text", "", NotInMan);

			// General UI
			SetDefault("ui.unit", "text", "", Messages.ManOptionUiUnit);
			SetDefaultBool("ui.iec", false, Messages.ManOptionUiIEC);
			SetDefaultBool("ui.skip.provider.manifest.failed", false, NotInMan);

			// GUI only
			SetDefaultBool("gui.start_minimized", false, NotInMan);
			SetDefaultBool("gui.tray_show", true, NotInMan);
			SetDefaultBool("gui.tray_minimized", true, NotInMan);
			SetDefaultBool("gui.notifications", true, NotInMan);
			SetDefaultBool("gui.exit_confirm", true, NotInMan, false);
			SetDefault("gui.skin", "text", "Light", NotInMan, false);
			SetDefaultBool("gui.tos", false, NotInMan, false);
			SetDefault("gui.font.normal.name", "text", "", NotInMan);
			SetDefaultFloat("gui.font.normal.size", 0, NotInMan);
			SetDefaultInt("gui.log_limit", 1000, NotInMan);
			SetDefault("gui.window.main", "text", "", NotInMan, false);
			SetDefault("gui.list.servers", "text", "", NotInMan, false);
			SetDefault("gui.list.areas", "text", "", NotInMan, false);
			SetDefault("gui.list.logs", "text", "", NotInMan, false);

			// UI - OSX Only
			// SetDefaultBool("gui.osx.dock", false, NotInMan); // See this FAQ: https://airvpn.org/topic/13331-its-possible-to-hide-the-icon-in-dock-bar-under-os-x/
			SetDefaultBool("gui.osx.visible", false, NotInMan);
			SetDefault("gui.osx.style", "text", "light", NotInMan);
			SetDefaultBool("gui.osx.sysbar.show_info", false, NotInMan);
			SetDefaultBool("gui.osx.sysbar.show_speed", false, NotInMan); // Menu Status, Window Title, Tray Tooltip
			SetDefaultBool("gui.osx.sysbar.show_server", false, NotInMan);

			


			// Command-line only
			Options["cli"].CommandLineOnly = true;
			Options["help"].CommandLineOnly = true;
			Options["help.format"].CommandLineOnly = true;

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
			SetDefault("event." + name + ".filename", "path_file", "", Messages.ManOptionEventFileName);
			SetDefault("event." + name + ".arguments", "text", "", Messages.ManOptionEventArguments);
			SetDefaultBool("event." + name + ".waitend", true, Messages.ManOptionEventWaitEnd);
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
			string path = GetProfilePath();

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

						if (option.CommandLineOnly)
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
							XmlElement xmlAirVPN = UtilsXml.XmlGetFirstElementByTagName(providersNode, "AirVPN");
							if (xmlAirVPN != null)
							{
								foreach (XmlElement xmlChild in xmlAirVPN.ChildNodes)
									UtilsXml.XmlCopyElement(xmlChild, xmlDoc.DocumentElement);
								providersNode.RemoveChild(xmlAirVPN);
							}
							if (providersNode.ChildNodes.Count == 0)
								providersNode.ParentNode.RemoveChild(providersNode);
						}
					}

					xmlDoc.Save(path);

					Platform.Instance.FileEnsurePermission(path, "600");
				}
				catch (Exception ex)
				{
					Engine.Instance.Logs.Log(LogType.Fatal, MessagesFormatter.Format(Messages.OptionsWriteFailed, path, ex.Message));
				}
			}
		}

		public void Load()
		{
			lock (this)
			{
				try
				{
					XmlDocument xmlDoc = new XmlDocument();

					Providers = xmlDoc.CreateElement("providers");

					if (Get("profile").ToLowerInvariant() == "none")
						return;

					string path = GetProfilePath();

					CompatibilityManager.FixOldProfilePath(path); // 2.15

					Engine.Instance.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.OptionsRead, path));

					if (Platform.Instance.FileExists(path) == false)
					{
						Engine.Instance.Logs.Log(LogType.Verbose, Messages.OptionsNotFound);
						return;
					}

					// CompatibilityManager.FixOldProfile(path); // ClodoTemp
					xmlDoc.Load(path);

					ResetAll(true);

					Providers = UtilsXml.XmlGetFirstElementByTagName(xmlDoc.DocumentElement, "providers");
					if (Providers == null)
						Providers = xmlDoc.CreateElement("providers");

					XmlNode nodeOptions = xmlDoc.DocumentElement.GetElementsByTagName("options")[0];
					Dictionary<string, string> options = new Dictionary<string, string>();
					foreach (XmlElement e in nodeOptions)
					{
						string name = e.Attributes["name"].Value;
						string value = e.Attributes["value"].Value;

						CompatibilityManager.FixOption(ref name, ref value);

						options[name] = value;
					}

					CompatibilityManager.FixOptions(options);
					foreach (KeyValuePair<string, string> item in options)
						Set(item.Key, item.Value);

					// For compatibility <3
					XmlElement xmlManifest = UtilsXml.XmlGetFirstElementByTagName(xmlDoc.DocumentElement, "manifest");
					if (xmlManifest != null)
					{
						XmlElement providerAirVpn = xmlDoc.CreateElement("AirVPN");
						Providers.AppendChild(providerAirVpn);

						UtilsXml.XmlCopyElement(xmlManifest, providerAirVpn);

						XmlElement xmlUser = UtilsXml.XmlGetFirstElementByTagName(xmlDoc.DocumentElement, "user");
						if (xmlUser != null) // Compatibility with old manifest < 2.11
						{
							XmlElement oldKeyFormat = xmlUser.SelectSingleNode("keys/key[@id='default']") as XmlElement;
							if (oldKeyFormat != null)
							{
								oldKeyFormat.SetAttribute("name", "Default");
							}
						}
						if (xmlUser != null)
							UtilsXml.XmlCopyElement(xmlUser, providerAirVpn);
					}
				}
				catch (Exception ex)
				{
					Engine.Instance.Logs.Log(LogType.Fatal, MessagesFormatter.Format(Messages.OptionsReverted, ex.Message));
					ResetAll(true);
				}
			}
		}


	}
}
