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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace AirVPN.Core
{
	public class OvpnManager
	{
		public static bool OptionRecursive = true; // Maybe an option
		public static bool OptionRemoveIfFileMissing = true; // Maybe an option
		
		public static void Refresh(bool interactive)
		{
			string optionPath = Engine.Instance.Storage.Get("profiles.path");

			int timeStart = Utils.UnixTimeStamp();

			List<ServerInfo> servers = new List<ServerInfo>();

			// Check current profiles
			foreach (XmlElement nodeProfile in Engine.Instance.Storage.Profiles.ChildNodes)
			{
				string path = Utils.XmlGetAttributeString(nodeProfile, "path", "");
				if (path != "")
				{
					UpdateProfileFromFile(nodeProfile, path);
				}
			}

			// Scan directory
			if (optionPath.Trim() != "")
			{
				ScanDir(optionPath.Trim(), OptionRecursive, servers, interactive);
			}

			// Remove profiles
			for (; ; )
			{
				bool changed = false;
				foreach (XmlElement nodeProfile in Engine.Instance.Storage.Profiles.ChildNodes)
				{
					if (nodeProfile.HasAttribute("checked") == false)
					{
						Engine.Instance.Storage.Profiles.RemoveChild(nodeProfile);
						changed = true;
					}
				}
				if (changed == false)
					break;
			}
			// Rimuovo i profili senza 'checked'

			// Clean flags
			foreach (XmlElement nodeProfile in Engine.Instance.Storage.Profiles.ChildNodes)
			{
				nodeProfile.Attributes.RemoveNamedItem("checked");
			}
			
			
				

			int timeDelta = Utils.UnixTimeStamp() - timeStart;

			Engine.Instance.Log(Engine.LogType.Verbose, Messages.Format("N. {1} ovpn profiles processed in {2} secs", Engine.Instance.Storage.Profiles.ChildNodes.Count.ToString(), timeDelta.ToString())); // TOTRANSLATE
		}

		public static void ScanDir(string path, bool recursive, List<ServerInfo> servers, bool interactive)
		{
			try
			{	
				foreach (string filePath in Directory.GetFiles(path))
				{
					FileInfo fileInfo = new FileInfo(filePath);

					string extension = fileInfo.Extension.ToLowerInvariant().Trim();
					if ((extension != ".ovpn") && (extension != ".conf"))
						continue;

					// Already listed?
					XmlElement nodeProfile = null;
					foreach (XmlElement nodeFind in Engine.Instance.Storage.Profiles.ChildNodes)
					{
						string pathFind = Utils.XmlGetAttributeString(nodeFind, "path", "");
						if (pathFind == fileInfo.FullName)
						{
							nodeProfile = nodeFind;
							break;
						}
					}

					// Skip if is already checked					
					if ((nodeProfile != null) && (Utils.XmlGetAttributeString(nodeProfile, "checked", "") != ""))
						continue;

					UpdateProfileFromFile(nodeProfile, filePath);

				}

				if (recursive)
				{
					foreach (string dirPath in Directory.GetDirectories(path))
					{
						ScanDir(dirPath, recursive, servers, interactive);
					}
				}
			}
			catch (System.Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

		public static void UpdateProfileFromFile(XmlElement nodeProfile, string path)
		{
			Dictionary<string, string> dataProfile = OvpnParse(new FileInfo(path), true);

			if (nodeProfile == null)
			{
				nodeProfile = Engine.Instance.Storage.Profiles.OwnerDocument.CreateElement("profile");
				Engine.Instance.Storage.Profiles.AppendChild(nodeProfile);
			}

			UpdateProfileFromFile(nodeProfile, dataProfile);
		}

		public static void UpdateProfileFromFile(XmlElement nodeProfile, Dictionary<string, string> data)
		{
			Utils.XmlSetAttributeString(nodeProfile, "name", data["name"]);
			Utils.XmlSetAttributeString(nodeProfile, "path", data["path"]);
			Utils.XmlSetAttributeString(nodeProfile, "country", data["country"]);
			Utils.XmlSetAttributeString(nodeProfile, "ovpn", data["ovpn"]);

			Utils.XmlSetAttributeString(nodeProfile, "checked", "1");
		}

		// Parse the OpenVPN configuration file. Check if is valid, import external files, normalize.
		public static Dictionary<string, string> OvpnParse(FileInfo file, bool interactive)
		{
			try
			{
				string optionPath = Engine.Instance.Storage.Get("profiles.path");

				Dictionary<string, string> dictInfo = new Dictionary<string, string>();

				string ovpnOriginal = File.ReadAllText(file.FullName);

				string ovpn = "# " + Engine.Instance.GenerateFileHeader() + "\n";

				List<string> directiveList = new List<string>();

				string[] lines = ovpnOriginal.Split('\n');
				foreach (string line in lines)
				{
					string lineCurrent = line;
					for (; ; )
					{
						string lineOriginal = lineCurrent;

						lineCurrent = lineCurrent.Replace("  ", " ");
						lineCurrent = lineCurrent.Replace("\t", " ");
						if (lineCurrent.IndexOf('#') != -1)
							lineCurrent = lineCurrent.Substring(0, lineCurrent.IndexOf('#'));
						if( (lineCurrent.Length >= 1) && (lineCurrent.Substring(0, 1) == ";") ) // Old comment style
							lineCurrent = "";
						lineCurrent = lineCurrent.Trim();

						if (lineCurrent == lineOriginal)
							break;
					}

					if (lineCurrent != "")
					{
						List<string> directiveParams = new List<string>(lineCurrent.Split(' '));
						if (directiveParams.Count == 0)
							continue;

						string directive = directiveParams[0].ToLowerInvariant().Trim();
						directiveParams.RemoveAt(0);

						directiveList.Add(directive);

						if (directive == "tls-auth")
						{
							if (directiveParams.Count == 0)
								continue;

							string tlsAuthPath = "";
							string tlsAuthDirection = "";
							string newText = "";

							tlsAuthPath = directiveParams[0];
							if (directiveParams.Count >= 2)
								tlsAuthDirection = directiveParams[1];

							if (tlsAuthPath != "")
							{
								string tlsAuthFullPath = tlsAuthPath;
								if (System.IO.Path.IsPathRooted(tlsAuthFullPath) == false)
									tlsAuthFullPath = file.DirectoryName + "\\" + tlsAuthFullPath;
								tlsAuthFullPath = Platform.Instance.NormalizePath(tlsAuthFullPath);
								if (File.Exists(tlsAuthFullPath) == false)
									throw new Exception("TLS-AUTH file not found");
								string tlsAuthData = File.ReadAllText(tlsAuthFullPath);
								newText = "<tls-auth>\n" + tlsAuthData.Trim() + "\n</tls-auth>";
							}

							if (tlsAuthDirection != "")
							{
								ovpn = Regex.Replace(ovpn, "^key-direction (.*?)$", "", RegexOptions.Multiline | RegexOptions.IgnoreCase); // Remove if exists							
								newText += "\nkey-direction " + tlsAuthDirection;
							}

							ovpn += "\n" + newText;
						}
						else if (directive == "ca")
						{
							if (directiveParams.Count == 0)
								continue;

							if (directiveParams[0] == "[inline]")
								continue;

							string caPath = directiveParams[0];
		
							if (caPath != "")
							{
								string caFullPath = caPath;
								if (System.IO.Path.IsPathRooted(caFullPath) == false)
									caFullPath = file.DirectoryName + "\\" + caFullPath;
								caFullPath = Platform.Instance.NormalizePath(caFullPath);
								if (File.Exists(caFullPath) == false)
									throw new Exception("CA file not found");
								string caData = File.ReadAllText(caFullPath);

								ovpn += "\n<ca>\n" + caData.Trim() + "\n</ca>";
							}
						}
						else if (directive == "management")
						{
							// AirVPN client need it's 'management' directive.
							// Ignore.
						}
						else if (
							(directive == "auth-retry") ||
							(directive == "connect-retry-max") ||
							(directive == "ping") ||
							(directive == "ping-exit") ||
							(directive == "explicit-exit-notify")
							)
						{
							// Ignore, added later.
						}
						else
						{
							// Accept directive
							ovpn += "\n" + lineCurrent.Trim();
						}


						if (directive == "remote")
						{
							if (directiveParams.Count >= 1)
								dictInfo["host"] = directiveParams[0];
							if (directiveParams.Count >= 2)
								dictInfo["port"] = directiveParams[1];
						}
						else if (directive == "proto")
						{
							if (directiveParams.Count >= 1)
								dictInfo["protocol"] = directiveParams[0];
						}
					}
				}

				// Adding missing directives
				{
					if (directiveList.Contains("client") == false)
						ovpn += "\nclient";

					if (directiveList.Contains("dev") == false)
						ovpn += "\ndev tun";

					if (directiveList.Contains("verb") == false)
						ovpn += "\nverb 3";

					if (directiveList.Contains("resolv-retry") == false)
						ovpn += "\nresolv-retry infinite";

					if (directiveList.Contains("nobind") == false)
						ovpn += "\nnobind";

					if (directiveList.Contains("persist-key") == false)
						ovpn += "\npersist-key";

					if (directiveList.Contains("persist-tun") == false)
						ovpn += "\npersist-tun";

					if (directiveList.Contains("auth-retry") == false)
						ovpn += "\nauth-retry none";

					if (directiveList.Contains("connect-retry-max") == false)
						ovpn += "\nconnect-retry-max";

					if (directiveList.Contains("ping") == false)
						ovpn += "\nping 10";

					if (directiveList.Contains("ping-exit") == false)
						ovpn += "\nping-exit 32";

					if (directiveList.Contains("explicit-exit-notify") == false)
						ovpn += "\nexplicit-exit-notify 5";
				}

				// Se c'è auth-user-pass
				// sostituire con
				// auth-user-pass <path temp>
				// <path temp>, due righe con login / password.
				// Forse dovrei mettere:
				// auth-nocache
				// ma temo che poi chieda login & password via stdin alla renew-key:  https://community.openvpn.net/openvpn/wiki/Openvpn23ManPage



				/* Le common AirVPN:			
				 * Valutarle una a una, metterle SOLO se solo fondamentali per eddie
	client 
				 // ci deve essere
			 
	dev tun
				 // ci deve essere
			 
	resolv-retry infinite
				 // ci deve essere
			 
	nobind
				 // ci deve essere
			 
	persist-key
				 // ci deve essere

	persist-tun
				 // ci deve essere
			 
	remote-cert-tls server
				 // NON ci deve essere
			 
	cipher AES-256-CBC
				 // NON ci deve essere

	comp-lzo no
				 // NON ci deve essere

	verb 3
				 // ci deve essere

	connect-retry-max 1
				 // ci deve essere

	ping 10
				 // ci deve essere
			 
	ping-exit 32
				 // ci deve essere

	# UDP only:

	explicit-exit-notify 5
				 // ci deve essere

	# TCP Only:
	*/

				ovpn += "\n\n# -------------------------------\n# Original OVPN configuration file\n\n";
				string[] linesOriginal = ovpnOriginal.Split('\n');
				foreach (string line in linesOriginal)
				{
					ovpn += "# " + line + "\n";
				}

				dictInfo["ovpn"] = ovpn;
				dictInfo["path"] = file.FullName;
				dictInfo["country"] = "";

				// Compute user-friendly name
				{
					string name = file.FullName;

					
					if(optionPath != "")
						name = name.Replace(optionPath, "").Trim();

					name = Regex.Replace(name, "udp", "", RegexOptions.IgnoreCase);
					name = Regex.Replace(name, "tcp", "", RegexOptions.IgnoreCase);
					name = Regex.Replace(name, "tblk", "", RegexOptions.IgnoreCase); // TunnelBlick
					name = Regex.Replace(name, "ovpn", "", RegexOptions.IgnoreCase); // OpenVPN

					foreach (string countryName in CountriesManager.Name2Code.Keys)
					{
						if (name.IndexOf(countryName) != -1)
						{
							dictInfo["country"] = CountriesManager.Name2Code[countryName];							
						}
					}

					// Cleaning
					name = name.Replace("-", " - ").Trim();
					name = name.Replace("_", " - ").Trim();
					name = name.Replace(".", " - ").Trim();

					name = name.Replace("\\", " - ").Trim();
					name = name.Replace("/", " - ").Trim();

					for (; ; )
					{
						string orig = name;

						name = name.Replace("  ", " ");
						name = name.Replace("\t", " ");
						name = name.Replace("- -", "-");

						name = name.Trim(" -".ToCharArray());
						
						if (name == orig)
							break;
					}

					if (dictInfo.ContainsKey("protocol"))
						name += " - " + dictInfo["protocol"].ToUpperInvariant();

					dictInfo["name"] = name;
				}

				return dictInfo;
			}
			catch (System.Exception e)
			{
				string message = Messages.Format("Profiles scan, {1} (in profile '{1}')", e.Message, file.FullName); // TOTRANSLATE
				if (interactive)
					Engine.Instance.Log(Engine.LogType.Fatal, message);
				else
					Engine.Instance.Log(Engine.LogType.Warning, message);
				return null;
			}
		}
	}
}
