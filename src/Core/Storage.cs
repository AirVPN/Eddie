// <airvpn_source_header>
// This file is part of AirVPN Client software.
// Copyright (C)2014-2014 AirVPN (support@airvpn.org) / https://airvpn.org )
//
// AirVPN Client is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// AirVPN Client is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with AirVPN Client. If not, see <http://www.gnu.org/licenses/>.
// </airvpn_source_header>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace AirVPN.Core
{
	public class Storage
    {
		public static string DataPath = "";
		public static bool Simulate = false; // If true, connections not really maded. Useful only during development of UI.

        public XmlNode Manifest;
		public XmlNode User;
		public XmlNode Profiles; // Custom OVPN profiles

        private Dictionary<string, string> m_OptionsDefaults = new Dictionary<string, string>();
		private Dictionary<string, string> m_OptionsMan = new Dictionary<string, string>();
        private Dictionary<string, string> m_Options = new Dictionary<string, string>();

		private Int64 m_lastManifestTimeTry = 0;

        public Storage()
        {

            EnsureDefaults();


						
            XmlDocument DocManifestDefault = new XmlDocument();
			DocManifestDefault.LoadXml(ResourcesFiles.GetString("manifest.xml"));
			Manifest = DocManifestDefault.DocumentElement;



            // Compute profile
			string profile = Get("profile");
			string path = Get("path");

			path = Platform.Instance.NormalizePath(path);

			if (path == "")
				path = Platform.Instance.GetDefaultDataPath();


            if (profile.IndexOf(".") != -1)
            {
                // Profile must not have an extension.
                profile = profile.Substring(0, profile.IndexOf("."));
				CommandLine.SystemEnvironment.Set("profile",profile);
            }

			if (Platform.Instance.IsPath(profile))            
            {
                // Is a path
				FileInfo fi = new FileInfo(Platform.Instance.NormalizePath(profile));
                DataPath = fi.DirectoryName;
                profile = fi.Name;
				CommandLine.SystemEnvironment.Set("profile",profile);

                if (TestDataPath(DataPath, true) == false)
                    DataPath = "";                
            }

            if (DataPath == "")
            {
                if (path == "home")
					path = Platform.Instance.GetUserFolder();
                else if (path == "program")
                    path = Platform.Instance.GetProgramFolder();

                if (path != "")
                {
                    DataPath = path;
                    if (TestDataPath(DataPath, true) == false)
                        DataPath = "";
                }
            }
                        
            if (DataPath == "")
            {
				DataPath = Platform.Instance.GetProgramFolder();
                if (Utils.HasAccessToWrite(DataPath) == false)
                    DataPath = "";
            }

            if (DataPath == "")
				DataPath = Platform.Instance.GetUserFolder();
        }

        public static bool TestDataPath(string path, bool log)
        {
            if (Utils.HasAccessToWrite(path) == false)
            {
                if(log == true)
                    Engine.Instance.Log(Engine.LogType.Info, "Unable to write in path '" + path + "'");
                return false;
            }
            return true;
        }

		/*
		public static string GetVersionDesc()
        {
            return String.Format(CultureInfo.InvariantCulture, "{0:0.0####}", Constants.Version);
        }
		*/

		public string GetMan()
        {
			List<string> options = new List<string>();
			foreach (KeyValuePair<string, string> item in m_OptionsDefaults)
			{
				options.Add(item.Key);
			}

			string body = "";
			foreach (string manKey in options)
            {
				string manDescription = m_OptionsMan[manKey];
				string manDefault = m_OptionsDefaults[manKey];
				string manValue = Get(manKey);

				if (manDescription != "")
				{
					body += "[*][b]" + manKey + "[/b] = " + manDescription.Replace("\n", "\n\t");
					//if (manDefault != "")
					{
						if (manValue != manDefault)
							body += " - Current: '[i]" + manValue + "[/i]'";
						body += " - Default: " + ( (manDefault == "") ? "-Empty-" : "'[i]" + manDefault + "[/i]'");
					}
					body += "[/*]\n";
				}
            }
            return body;
        }

		public bool Exists(string name)
		{
			return m_OptionsDefaults.ContainsKey(name);
		}
                
        public string Get(string name)
        {
			lock (this) {
				if (CommandLine.SystemEnvironment.Exists (name))
					return CommandLine.SystemEnvironment.Get(name,"");
				else if (m_Options.ContainsKey (name))
					return m_Options [name];
				else if (m_OptionsDefaults.ContainsKey (name))
					return m_OptionsDefaults [name];
				else {
					Engine.Instance.Log (Engine.LogType.Error, "Unknown option '" + name + "'");
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

        public Int64 GetInt64(string name)
        {
			return Conversions.ToInt64(Get(name));			
        }

        public List<string> GetList(string name)
        {
            List<string> output = new List<string>();
            string[] va = Get(name).Split(',');
            foreach (string v in va)
            {
                if(v != "")
                    output.Add(v);
            }
            return output;
        }

        public void Set(string name, string val)
        {
            lock (this)
            {
                m_Options[name] = val;
            }
        }

        public void SetInt(string name, int val)
        {
            Set(name,val.ToString(CultureInfo.InvariantCulture));
        }

        public void SetBool(string name, bool val)
        {
            Set(name, val.ToString(CultureInfo.InvariantCulture));
        }

        public void SetList(string name, List<string> val)
        {
            Set(name, String.Join(",", val.ToArray()));
        }

        public void SetDefault(string name, string val, string man)
        {
            m_OptionsDefaults[name] = val;
			m_OptionsMan[name] = man;
        }

		public void SetDefaultInt(string name, int val, string man)
        {
			SetDefault(name, val.ToString(CultureInfo.InvariantCulture), man);
        }

		public void SetDefaultBool(string name, bool val, string man)
        {
			SetDefault(name, val.ToString(CultureInfo.InvariantCulture), man);
        }

        public void Remove(string name)
        {
            lock (this)
            {
                m_Options.Remove(name);
            }
        }

        public string GetPath(string filename)
        {
			return DataPath + Platform.Instance.DirSep + filename;            
        }

        public void EnsureDefaults()
        {
			string NotInMan = ""; // Option not listed in 'man' documentation.

			/*
			bool defaultDnsForceAndCheck = false; // 2.8
			if(Platform.IsUnix())
				defaultDnsForceAndCheck = true;
			*/
			bool defaultDnsForceAndCheck = true; // 2.9

			SetDefaultBool("cli", false, Messages.ManOptionCli);
			SetDefaultBool("help", false, Messages.ManOptionHelp);
			SetDefault("help_format", "text", NotInMan); // Maybe 'text' or 'bbc'.
			SetDefault("login", "", Messages.ManOptionLogin);
            SetDefault("password", "", Messages.ManOptionPassword);
			SetDefaultBool("remember", false, Messages.ManOptionRemember);
			SetDefault("server", "", Messages.ManOptionServer);
			SetDefaultBool("connect", false, Messages.ManOptionConnect);
			SetDefaultBool("netlock", false, NotInMan);

			SetDefault("profile", "AirVPN", Messages.ManOptionProfile); // Not in Settings
			SetDefault("path", "", Messages.ManOptionPath); // Not in Settings // Path. Maybe a full path, or special values 'home' or 'program'.			

			SetDefault("servers.last", "", NotInMan);
			SetDefault("servers.whitelist", "", Messages.ManOptionServersWhiteList);
			SetDefault("servers.blacklist", "", Messages.ManOptionServersBlackList);
			SetDefaultBool("servers.startlast", false, Messages.ManOptionServersStartLast);
			SetDefaultBool("servers.locklast", false, Messages.ManOptionServersLockLast);
			SetDefault("servers.scoretype", "Speed", Messages.ManOptionServersScoreType);

			SetDefault("areas.whitelist", "", Messages.ManOptionAreasWhiteList);
			SetDefault("areas.blacklist", "", Messages.ManOptionAreasBlackList);

			SetDefaultBool("log.file.enabled", false, NotInMan);
			SetDefault("log.file.path", "logs/airvpn_%y-%m-%d.log", NotInMan);
			SetDefaultBool("log.level.debug", false, NotInMan);

			SetDefault("mode.protocol", "AUTO", Messages.ManOptionModeProtocol);
			SetDefaultInt("mode.port", 443, Messages.ManOptionModePort);
			SetDefaultInt("mode.alt", 0, Messages.ManOptionModeAlt);
			SetDefault("mode.tor.host", "127.0.0.1", Messages.ManOptionModeTorHost);
			SetDefaultInt("mode.tor.port", 9150, Messages.ManOptionModeTorPort);
			SetDefaultInt("mode.tor.control.port", 9151, Messages.ManOptionModeTorControlPort);
			SetDefaultBool("mode.tor.control.auth", true, Messages.ManOptionModeTorControlAuth);
			SetDefault("mode.tor.control.password", "", Messages.ManOptionModeTorControlPassword);

			SetDefault("proxy.mode", "None", Messages.ManOptionProxyMode);
			SetDefault("proxy.host", "127.0.0.1", Messages.ManOptionProxyHost);
			SetDefaultInt("proxy.port", 8080, Messages.ManOptionProxyPort);
			SetDefault("proxy.auth", "None", Messages.ManOptionProxyAuth);
			SetDefault("proxy.login", "", Messages.ManOptionProxyLogin);
			SetDefault("proxy.password", "", Messages.ManOptionProxyPassword);

			SetDefault("routes.default", "in", Messages.ManOptionRoutesDefault);
			SetDefault("routes.custom", "", Messages.ManOptionRoutesCustom);
			SetDefaultBool("routes.remove_default", false, NotInMan);

			SetDefault("dns.mode", "auto", Messages.ManOptionDnsMode);
			SetDefault("dns.servers", "", Messages.ManOptionDnsServers);
			SetDefaultBool("dns.check", defaultDnsForceAndCheck, Messages.ManOptionDnsCheck);

			SetDefault("netlock.mode", "auto", NotInMan); // Maybe 'auto' in future			
			SetDefaultBool("netlock.allow_private", true, NotInMan); // Allow private subnet by default
			SetDefaultBool("netlock.allow_ping", true, NotInMan); // Allow ICMP/Ping by default
			
			SetDefault("netlock.allowed_ips", "", NotInMan); // List of IP not blocked			

			SetDefault("ipv6.mode", "disable", NotInMan);

			SetDefault("executables.openvpn", "", Messages.ManOptionExecutablesOpenVpn);
			SetDefault("executables.ssh", "", Messages.ManOptionExecutablesSsh);
			SetDefault("executables.ssl", "", Messages.ManOptionExecutablesSsl);
			SetDefault("executables.curl", "", Messages.ManOptionExecutablesCurl);
			SetDefault("openvpn.custom", "", Messages.ManOptionOpenVpnCustom);
			SetDefault("openvpn.dev_node", "", NotInMan);
			SetDefaultInt("openvpn.rcvbuf", (128 * 1024), NotInMan); // 2.10.1
			SetDefaultInt("openvpn.sndbuf", (128 * 1024), NotInMan);
			SetDefaultBool("openvpn.skip_defaults", false, Messages.ManOptionOpenVpnSkipDefaults);

			SetDefault("profiles.path", "", NotInMan);
			
			// Not in Settings
			SetDefaultInt("openvpn.management_port", 3100, Messages.ManOptionOpenVpnManagementPort);
			SetDefaultInt("ssh.port", 0, Messages.ManOptionSshPort); 
			SetDefaultInt("ssl.port", 0, Messages.ManOptionSslPort);

			SetDefaultBool("advanced.expert", false, Messages.ManOptionAdvancedExpert);			
			SetDefaultBool("advanced.check.route", true, Messages.ManOptionAdvancedCheckRoute);
			
			SetDefaultInt("advanced.penality_on_error", 30, NotInMan);
			SetDefaultBool("advanced.pinger.enabled", true, Messages.ManOptionAdvancedPingerEnabled);
			SetDefaultInt("advanced.pinger.delay", 0, Messages.ManOptionAdvancedPingerDelay);
			SetDefaultInt("advanced.pinger.retry", 0, Messages.ManOptionAdvancedPingerRetry);
			SetDefaultInt("advanced.pinger.jobs", 10, Messages.ManOptionAdvancedPingerJobs);
			SetDefaultInt("advanced.pinger.valid", 0, Messages.ManOptionAdvancedPingerValid);
			SetDefaultInt("advanced.manifest.refresh", -1, NotInMan);
			

			SetDefaultBool("advanced.windows.tap_up", true, Messages.ManOptionAdvancedWindowsTapUp);
			SetDefaultBool("advanced.windows.dhcp_disable", false, Messages.ManOptionAdvancedWindowsDhcpDisable);

			// Not in Settings
			SetDefaultBool("advanced.skip_privileges", false, NotInMan);

			// Not in Settings
			SetDefaultBool("advanced.skip_alreadyrun", false, NotInMan);

			// Not in Settings
			

			// Not in Settings
			SetDefaultBool("advanced.testmode", false, NotInMan);
			

            EnsureDefaultsEvent("app.start");
            EnsureDefaultsEvent("app.stop");
			EnsureDefaultsEvent("session.start");
			EnsureDefaultsEvent("session.stop");
            EnsureDefaultsEvent("vpn.pre");
            EnsureDefaultsEvent("vpn.up");
            EnsureDefaultsEvent("vpn.down");


			// Windows only
			SetDefault("windows.adapter_name", "TAP-Windows Adapter V9", NotInMan);
			SetDefault("windows.adapter_service", "tap0901", NotInMan);
			SetDefaultBool("windows.disable_driver_upgrade", false, NotInMan);

			// GUI only
			SetDefaultBool("gui.exit_confirm", true, NotInMan);
			SetDefault("gui.skin", "Light", NotInMan);						
			SetDefaultBool("gui.tos", false, NotInMan);
			SetDefaultInt("gui.log_limit", 1000, NotInMan);
			SetDefault("forms.main", "", NotInMan);

			// GUI - Windows only
			SetDefaultBool("gui.windows.tray", true, NotInMan);			

			// GUI - OSX Only
			SetDefaultBool("gui.osx.notifications", false, NotInMan);
			// SetDefaultBool("gui.osx.dock", false, NotInMan); // See this FAQ: https://airvpn.org/topic/13331-its-possible-to-hide-the-icon-in-dock-bar-under-os-x/
			SetDefaultBool("gui.osx.visible", false, NotInMan);
			SetDefault ("gui.osx.style", "light", NotInMan);

			// TODO: we need to test params with space in different linux platform, with focus on escaping gksu/kdesu shell to obtain elevated privileges
			SetDefault("paramtest", "", NotInMan); 			
        }

        public void EnsureDefaultsEvent(string name)
        {
			SetDefault("event." + name + ".filename", "", Messages.ManOptionEventFileName);
			SetDefault("event." + name + ".arguments", "", Messages.ManOptionEventArguments);
			SetDefaultBool("event." + name + ".waitend", true, Messages.ManOptionEventWaitEnd);
        }

        public void Save()
        {
			string path = GetPath(Get("profile") + ".xml");

			bool remember = GetBool("remember");
			
            lock (this)
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);

                XmlElement rootNode = xmlDoc.CreateElement("airvpn");
                xmlDoc.InsertBefore(xmlDeclaration, xmlDoc.DocumentElement);

                XmlElement optionsNode = xmlDoc.CreateElement("options");
                rootNode.AppendChild(optionsNode);

                xmlDoc.AppendChild(rootNode);

                foreach (KeyValuePair<string, string> item in m_Options)
                {
					bool skip = false;

					if ((remember == false) && (item.Key == "login"))
						skip = true;
					if ((remember == false) && (item.Key == "password"))
						skip = true;

					if (skip == false)
					{
						if (m_OptionsDefaults.ContainsKey(item.Key))
						{
							if (item.Value != m_OptionsDefaults[item.Key])
							{
								XmlElement itemNode = xmlDoc.CreateElement("option");
								itemNode.SetAttribute("name", item.Key);
								itemNode.SetAttribute("value", item.Value);
								optionsNode.AppendChild(itemNode);
							}
						}
						else
						{
							Debug.Fatal(Messages.Format(Messages.OptionsUnknown, item.Key));
						}
					}
                }

				if (Manifest != null)
				{
					XmlNode manifestNode = xmlDoc.ImportNode(Manifest, true);
					rootNode.AppendChild(manifestNode);
				}

				if (Profiles != null)
				{
					XmlNode profilesNode = xmlDoc.ImportNode(Profiles, true);
					rootNode.AppendChild(profilesNode);
				}

				if ( (remember) && (User != null) )
				{
					XmlNode userNode = xmlDoc.ImportNode(User, true);
					rootNode.AppendChild(userNode);
				}
				
                xmlDoc.Save(path);

				if (Platform.Instance.IsUnixSystem())
					Platform.Instance.ShellCmd("chmod 600 \"" + path + "\"");
            }
			
        }

        public void Load(bool manMode)
        {
            lock (this)
            {
                try
                {
                    XmlDocument xmlDoc = new XmlDocument();

					string profile = Get("profile");
					if (profile.ToLowerInvariant() == "none")
						return;

					string Path = GetPath(profile + ".xml");

					if(manMode == false)
						Engine.Instance.Log(Engine.LogType.Verbose, Messages.Format(Messages.OptionsRead, Path));

					if (File.Exists(Path) == false)
					{
						if (manMode == false)
							Engine.Instance.Log(Engine.LogType.Verbose, Messages.OptionsNotFound);
						return;
					}

                    xmlDoc.Load(Path);

                    m_Options.Clear();

                    XmlNode nodeOptions = xmlDoc.DocumentElement.GetElementsByTagName("options")[0];

                    foreach (XmlElement e in nodeOptions)
                    {
						string name = e.Attributes["name"].Value;
						string value = e.Attributes["value"].Value;

						CompatibilityManager.FixOption(ref name, ref value);

                        Set(name, value);
                    }

					Manifest = Utils.XmlGetFirstElementByTagName(xmlDoc.DocumentElement, "manifest");
					User = Utils.XmlGetFirstElementByTagName(xmlDoc.DocumentElement, "user");
					Profiles = Utils.XmlGetFirstElementByTagName(xmlDoc.DocumentElement, "profiles");

					if (Profiles == null)
						Profiles = xmlDoc.CreateElement("profiles");
                }
                catch (Exception ex)
                {
                    Debug.Trace(ex);
                    Engine.Instance.Log(Engine.LogType.Fatal, Messages.OptionsReverted);
                    m_Options.Clear();
                }
            }
        }

		#region Manifest Management

		public bool UpdateManifestNeed(bool reccomended)
		{
			lock (Manifest)
			{	
				Int64 refreshInterval = 10; // Minutes

				int refreshManifest = GetInt("advanced.manifest.refresh");
				if (refreshManifest < 0)
				{
					if ( (Manifest != null) && (Manifest.Attributes["next_update"] != null) )
					{
						refreshInterval = Conversions.ToInt64(Manifest.Attributes["next_update"].Value);
					}
				}
				else
				{
					refreshInterval = refreshManifest;
				}

				if (refreshInterval == 0)
					return false;
				
				if (m_lastManifestTimeTry + 60 * refreshInterval < Utils.UnixTimeStamp())
					return true;
			}

			return false;
		}

		public string UpdateManifest()
		{
			try
			{
				m_lastManifestTimeTry = Utils.UnixTimeStamp();

				Engine.Instance.Log(Engine.LogType.Verbose, Messages.ManifestUpdate);
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["act"] = "manifest";

				XmlDocument xmlDoc = AirExchange.Fetch(Messages.ManifestUpdate, parameters);
				lock (Manifest)
				{
					if(Manifest == null)
						Manifest = (new XmlDocument()).DocumentElement;
					Manifest = Manifest.OwnerDocument.ImportNode(xmlDoc.DocumentElement, true);					
					//Manifest = xmlDoc.DocumentElement;

					// Update with the local time
					Manifest.Attributes["time"].Value = Utils.UnixTimeStamp().ToString();
				}

				// OvpnManager.Refresh(true); // TOOPEN

				Engine.Instance.PostManifestUpdate();

				Engine.Instance.Log(Engine.LogType.Verbose, Messages.ManifestDone);

				return "";
			}
			catch (Exception e)
			{
				return Messages.Format(Messages.ManifestFailed, e.Message);
			}
		}

		public string GetManifestKeyValue(string key, string def)
		{
			if (Manifest == null)
				return def;
			if (Manifest.Attributes[key] == null)
				return def;
			return Manifest.Attributes[key].Value;
		}

		public string GetDefaultDirectives()
		{
			string result = "# Common:\n" + GetManifestKeyValue("openvpn_directives_common", "") + "\n# UDP only:\n" + GetManifestKeyValue("openvpn_directives_udp", "") + "\n# TCP Only:\n" + GetManifestKeyValue("openvpn_directives_tcp", "");
			result = Platform.Instance.NormalizeString(result);
			return result;
		}

		#endregion

	}
}
