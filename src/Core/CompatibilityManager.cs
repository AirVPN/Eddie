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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

/*
 * Official data from ISO 3166-1
 * */

namespace Eddie.Core
{	
    public class CompatibilityManager
    {
		public static void Init()
		{
			if (Platform.IsWindows())
			{
				// < 2.9 - Old Windows Firewall original backup rules path
				string oldPathRulesBackupFirstTime = Storage.DataPath + Platform.Instance.DirSep + "winfirewallrulesorig.wfw";
				string newPathRulesBackupFirstTime = Environment.SystemDirectory + Platform.Instance.DirSep + "winfirewall_rules_original.airvpn";
				if (File.Exists(oldPathRulesBackupFirstTime))
				{
					if (File.Exists(newPathRulesBackupFirstTime))
						File.Delete(oldPathRulesBackupFirstTime);
					else
						File.Move(oldPathRulesBackupFirstTime, newPathRulesBackupFirstTime);
				}

				string oldPathRulesBackupSession = Storage.DataPath + Platform.Instance.DirSep + "winfirewallrules.wfw";
				string newPathRulesBackupSession = Environment.SystemDirectory + Platform.Instance.DirSep + "winfirewall_rules_backup.airvpn";
				if (File.Exists(oldPathRulesBackupFirstTime))
				{
					if (File.Exists(newPathRulesBackupSession))
						File.Delete(oldPathRulesBackupSession);
					else
						File.Move(oldPathRulesBackupSession, newPathRulesBackupSession);
				}
			}

			// < 2.9 - New certificate for SSL connections
			if (Engine.Instance.IsLogged())
			{
                if (Engine.Instance.AirVPN != null)
                {
                    if (Utils.XmlGetAttributeString(Engine.Instance.AirVPN.User, "ssl_crt", "") == "")
                    {
                        Engine.Instance.ReAuth();
                    }
                }
			}            
		}

        public static void FixOptions(Dictionary<string, string> options)
        {
            if(options.ContainsKey("mode.protocol"))
            {
                if(options["mode.protocol"] == "TOR")
                {
                    options["mode.protocol"] = "TCP";
                    options["proxy.mode"] = "Tor";
                    if(options.ContainsKey("mode.tor.host"))
                    {
                        options["proxy.host"] = options["mode.tor.host"];
                        options.Remove("mode.tor.host");
                    }
                    if (options.ContainsKey("mode.tor.port"))
                    {
                        options["proxy.port"] = options["mode.tor.port"];
                        options.Remove("mode.tor.port");
                    }                    
                }
            }
        }

		public static void FixOption(ref string name, ref string value)
		{
            // Eddie <= 2.4 client use  'host,netmask,action' syntax.
            // If detected, convert to new 'iprange,action,notes' syntax.
            if (name == "routes.custom")
            {
                string newValue = "";

                string[] routes2 = value.Split(';');
                foreach (string route in routes2)
                {
                    string[] routeEntries = route.Split(',');
                    if (routeEntries.Length != 3)
                        return;

                    string newRoute = "";
                    if (new IpAddress(routeEntries[1]).Valid)
                    {
                        newRoute = routeEntries[0] + "/" + routeEntries[1] + "," + routeEntries[2];
                    }
                    else
                        newRoute = route;

                    if (newValue != "")
                        newValue += ";";
                    newValue += newRoute;
                }

                value = newValue;
            }
            else if (name == "netlock.active") // < 2.9
            {
                name = "netlock";
            }
            else if (name == "advanced.dns.mode") // < 2.9
            {
                name = "dns.mode";
            }
            else if (name == "advanced.check.dns") // < 2.9
            {
                name = "dns.check";
            }
            else if (name == "mode.tor.control.port") // < 2.11
            {
                name = "proxy.tor.control.port";
            }
            else if (name == "mode.tor.control.password") // < 2.11
            {
                name = "proxy.tor.control.password";
            }
            else if (name == "mode.tor.control.auth") // < 2.11
            {
                name = "proxy.tor.control.auth";
            }
            else if (name == "advanced.windows.tap_up") // < 2.11
            {
                name = "windows.tap_up";
            }
            else if (name == "advanced.windows.dhcp_disable") // < 2.11
            {
                name = "windows.dhcp_disable";
            }
            else if (name == "advanced.pinger.enabled") // < 2.11
            {
                name = "pinger.enabled";
            }
            else if (name == "forms.main") // < 2.11
            {
                name = "gui.window.main";
            }

#if (EDDIE3)            
            if (name == "dns.check") // < 3.0
            {
                name = "providers.AirVPN.dns.check";
            }
            else if (name == "advanced.check.route") // < 3.0
            {
                name = "providers.AirVPN.tunnel.check";
            }
#endif
        }

        public static void FixProviderStorage(XmlDocument e)
        {
            if (e == null)
                return;

            // If not contain urls/url element, it's the manifest <2.11. Need update.
            XmlElement manifest = e.DocumentElement.SelectSingleNode("manifest") as XmlElement;
            if (manifest != null)
            {
                if (manifest.SelectNodes("//urls/url").Count == 0) // Compatibility
                {
                    e.DocumentElement.RemoveChild(manifest);
                }
            }
                
        }
    }
}
