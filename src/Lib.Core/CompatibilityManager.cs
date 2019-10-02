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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Eddie.Common;

namespace Eddie.Core
{
	public class CompatibilityManager
	{
		public static void Profiles(string pathApp, string pathData)
        {
            if (Engine.Instance.Elevated != null)
            {
                string owner = Environment.UserName;

                ElevatedProcess.Command command = new ElevatedProcess.Command();
                command.Parameters["command"] = "compatibility-profiles";
                command.Parameters["path-app"] = pathApp;
                command.Parameters["path-data"] = pathData;
                command.Parameters["owner"] = owner;
                command.DoSync();
            }
        }

        public static void WindowsRemoveTask()
		{
			ElevatedProcess.Command command = new ElevatedProcess.Command();
			command.Parameters["command"] = "compatibility-remove-task";
			command.DoSync();
		}

		public static string AdaptProfileNameOption(string name)
		{
			// Eddie <2.17 use an old .xml format. If anyone use "profile=my.xml", it will adapted to "profile=my.profile"
			if (name.EndsWith(".xml.profile"))
				name = name.Replace(".xml.", ".");
			return name;
		}

		public static void AfterProfile()
		{
			if (Platform.IsWindows())
			{
				// < 2.9 - Old Windows Firewall original backup rules path
				string oldPathRulesBackupFirstTime = Engine.Instance.GetPathInData("winfirewallrulesorig.wfw");
				string newPathRulesBackupFirstTime = Environment.SystemDirectory + Platform.Instance.DirSep + "winfirewall_rules_original.airvpn";
				if (Platform.Instance.FileExists(oldPathRulesBackupFirstTime))
				{
					if (Platform.Instance.FileExists(newPathRulesBackupFirstTime))
						Platform.Instance.FileDelete(oldPathRulesBackupFirstTime);
					else
						Platform.Instance.FileMove(oldPathRulesBackupFirstTime, newPathRulesBackupFirstTime);
				}

				string oldPathRulesBackupSession = Engine.Instance.GetPathInData("winfirewallrules.wfw");
				string newPathRulesBackupSession = Environment.SystemDirectory + Platform.Instance.DirSep + "winfirewall_rules_backup.airvpn";
				if (Platform.Instance.FileExists(oldPathRulesBackupFirstTime))
				{
					if (Platform.Instance.FileExists(newPathRulesBackupSession))
						Platform.Instance.FileDelete(oldPathRulesBackupSession);
					else
						Platform.Instance.FileMove(oldPathRulesBackupSession, newPathRulesBackupSession);
				}
			}

			if (Platform.Instance.IsLinuxSystem())
			{
				// < 2.11 - Old file name
				if (Platform.Instance.FileExists("/etc/resolv.conf.airvpn"))
					Platform.Instance.FileDelete("/etc/resolv.conf.airvpn");

				// A bug in old experimental 2.11 cause the set of immutable flag in rare cases.
				if (Platform.Instance.FileImmutableGet("/etc/resolv.conf"))
					Platform.Instance.FileImmutableSet("/etc/resolv.conf", false);            
			}

			// < 2.9 - New certificate for SSL connections
			if (Engine.Instance.IsLogged())
			{
				if (Engine.Instance.AirVPN != null)
				{
					if (UtilsXml.XmlGetAttributeString(Engine.Instance.AirVPN.User, "ssl_crt", "") == "")
					{
						Engine.Instance.ReAuth();
					}

					if (UtilsXml.XmlGetAttributeString(Engine.Instance.AirVPN.User, "tls_crypt", "") == "")
					{
						Engine.Instance.ReAuth();
					}
				}
			}
		}

		public static void FixOptions(Dictionary<string, string> options)
		{
			if (options.ContainsKey("mode.protocol"))
			{
				if (options["mode.protocol"] == "TOR")
				{
					options["mode.protocol"] = "TCP";
					options["proxy.mode"] = "Tor";
					if (options.ContainsKey("mode.tor.host"))
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
			else if (name == "advanced.batchmode")
			{
				name = "batch";
			}
			else if (name == "executables.openvpn")
			{
				name = "tools.openvpn.path";
			}
			else if (name == "executables.ssh")
			{
				name = "tools.ssh.path";
			}
			else if (name == "executables.ssl")
			{
				name = "tools.ssl.path";
			}
			else if (name == "executables.curl")
			{
				name = "tools.curl.path";
			}
			else if (name == "gui.windows.notifications")
			{
				name = "gui.notifications";
			}
			else if (name == "gui.osx.notifications")
			{
				name = "gui.notifications";
			}
			else if (name == "gui.windows.start_minimized")
			{
				name = "gui.start_minimized";
			}
			else if (name == "gui.windows.tray")
			{
				name = "gui.tray_minimized";
			}
			else if (name == "netlock.allowed_ips")
			{
				name = "netlock.whitelist.outgoing.ips";
			}		
            else if (name == "gui.tos")
            {
                name = "";
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

		// Rename AirVPN.xml (if exists) to Default.xml
		public static void FixOldProfilePath(string newPath)
		{
			if (Platform.Instance.FileExists(newPath))
				return;			

			if( (newPath.EndsWith("default.profile")) && (Platform.Instance.FileExists(newPath.Replace("default.profile","AirVPN.xml"))) )
				Platform.Instance.FileMove(newPath.Replace("default.profile", "AirVPN.xml"), newPath.Replace("default.profile", "default.xml"));

			if ((newPath.EndsWith("default.profile")) && (Platform.Instance.FileExists(newPath.Replace("default.profile", "default.xml"))))
			{				
				byte[] content = Platform.Instance.FileContentsReadBytes(newPath.Replace("default.profile", "default.xml"));
				Platform.Instance.FileContentsWriteBytes(newPath, Storage.EncodeFormat("v2n", RandomGenerator.GetRandomId64(), content, Constants.PasswordIfEmpty));
				Platform.Instance.FileDelete(newPath.Replace("default.profile", "default.xml"));
			}
		}

		public static string FixOldProfile(string originalPath)
		{
			FileInfo originalFile = new FileInfo(originalPath);
			string newPath = "";
			if (originalFile.Name == "AirVPN.xml")
				newPath = originalFile.DirectoryName + "/default.json";
			else
				newPath = originalFile.DirectoryName + "/" + originalFile.Name.Replace(".xml", ".json");

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(originalPath);

			// Fix
			{
				XmlNodeList xmlList = xmlDoc.DocumentElement.GetElementsByTagName("providers");
				if (xmlList.Count == 1)
				{
					XmlElement xmlProviders = xmlList[0] as XmlElement;

					foreach (XmlElement xmlProvider in xmlProviders.ChildNodes)
					{
						xmlProvider.SetAttribute("type", xmlProvider.Name);
						xmlProvider.SetAttribute("json-convert-name", "provider");						
					}
				}
			}
			/*
			Json j = Conversions.ToJson(xmlDoc.DocumentElement);
			Platform.Instance.FileContentsWriteText(newPath, j.ToJsonPretty());
			*/

			return newPath;
		}
	}
}
