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

namespace Eddie.Core.Providers
{
    public class OpenVPN : Core.Provider
    {
		public XmlElement Profiles;
        
		public override string GetCode()
		{
			return "OpenVPN";
		}

		public override bool AllowMultipleInstance()
		{
			return true;
		}

        public override bool GetEnabledByDefault()
        {
            return true;
        }

        public override void OnInit()
		{
			base.OnInit();
        }

		public override void OnLoad(XmlElement xmlStorage)
		{
            base.OnLoad(xmlStorage);

			Profiles = Storage.DocumentElement.SelectSingleNode("profiles") as XmlElement;
			if (Profiles == null)
			{
				Profiles = Storage.CreateElement("profiles") as XmlElement;
				Storage.DocumentElement.AppendChild(Profiles);
			}
		}

        public override void OnBuildOvpn(OvpnBuilder ovpn)
        {
            base.OnBuildOvpn(ovpn);            
        }

        public override string Refresh()
		{
			base.Refresh();

			Refresh(true);

			return "";
		}

		public override void OnBuildServersList()
		{
			base.OnBuildServersList();

            lock (Profiles)
			{
				foreach (XmlElement nodeProfile in Profiles.ChildNodes)
				{
                    string code = HashSHA256(Utils.XmlGetAttributeString(nodeProfile, "path", ""));
                    
					ServerInfo infoServer = Engine.Instance.GetServerInfo(code, this);

                    infoServer.DisplayName = Utils.XmlGetAttributeString(nodeProfile, "name", "");
                    infoServer.ProviderName = code;
                    infoServer.IpEntry = Utils.XmlGetAttributeString(nodeProfile, "remote", "");
                    infoServer.IpEntry2 = "";
					infoServer.IpExit = "";
					infoServer.CountryCode = Utils.XmlGetAttributeString(nodeProfile, "country", "");
					infoServer.Location = "";
					infoServer.ScoreBase = 0;
					infoServer.Bandwidth = 0;
					infoServer.BandwidthMax = 0;
					infoServer.Users = -1;
					infoServer.WarningOpen = "";
					infoServer.WarningClosed = "";
                    infoServer.SupportCheck = false;
                    infoServer.OvpnDirectives = Utils.XmlGetAttributeString(nodeProfile, "ovpn", "");

                    infoServer.NeedDiscover = true;
				}
			}
		}



		public bool OptionRecursive = true; // Maybe an option
		public bool OptionRemoveIfFileMissing = true; // Maybe an option

        public string GetPathScan()
        {
            return Utils.XmlGetAttributeString(Storage.DocumentElement, "path", "").Trim();
        }

		public void Refresh(bool interactive)
		{
            string pathScan = GetPathScan();
            
            int timeStart = Utils.UnixTimeStamp();

			List<ServerInfo> servers = new List<ServerInfo>();

			// Check current profiles
			foreach (XmlElement nodeProfile in Profiles.ChildNodes)
			{
				string path = Utils.XmlGetAttributeString(nodeProfile, "path", "");
				if (path != "")
				{
                    try
                    {
                        UpdateProfileFromFile(nodeProfile, path);
                    }
                    catch(Exception e)
                    {
                        Engine.Instance.Logs.Log(e);
                    }
				}
			}

			// Scan directory
			if (Directory.Exists(pathScan))
			{
				ScanDir(pathScan.Trim(), OptionRecursive, servers, interactive);
			}
            else
            {
                Engine.Instance.Logs.Log(LogType.Warning, Messages.Format(Messages.ProvidersOpenVpnPathNotFound, pathScan, Title));
            }

			// Remove profiles
			for (; ; )
			{
				bool changed = false;
				foreach (XmlElement nodeProfile in Profiles.ChildNodes)
				{
					if (nodeProfile.HasAttribute("checked") == false)
					{
						Profiles.RemoveChild(nodeProfile);
						changed = true;
					}
				}
				if (changed == false)
					break;
			}
			// Rimuovo i profili senza 'checked'

			// Clean flags
			foreach (XmlElement nodeProfile in Profiles.ChildNodes)
			{
				nodeProfile.Attributes.RemoveNamedItem("checked");
			}




			int timeDelta = Utils.UnixTimeStamp() - timeStart;

			Engine.Instance.Logs.Log(LogType.Verbose, Messages.Format("{1} - N. {2} ovpn profiles processed in {3} secs", Title, Profiles.ChildNodes.Count.ToString(), timeDelta.ToString())); // TOTRANSLATE
		}

		public void ScanDir(string path, bool recursive, List<ServerInfo> servers, bool interactive)
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
					foreach (XmlElement nodeFind in Profiles.ChildNodes)
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

		public void UpdateProfileFromFile(XmlElement nodeProfile, string path)
		{
            if (Platform.Instance.FileExists(path) == false)
                return;

            Dictionary<string, string> dataProfile = OvpnParse(new FileInfo(path), true);

			if (nodeProfile == null)
			{
				nodeProfile = Profiles.OwnerDocument.CreateElement("profile");
				Profiles.AppendChild(nodeProfile);
			}

			UpdateProfileFromFile(nodeProfile, dataProfile);            
        }

        public string ComputeFriendlyNameFromPath(string path, OvpnBuilder ovpn)
        {
            return path;
        }

		public void UpdateProfileFromFile(XmlElement nodeProfile, Dictionary<string, string> data)
		{
			Utils.XmlSetAttributeString(nodeProfile, "name", data["name"]);
            Utils.XmlSetAttributeString(nodeProfile, "remote", data["remote"]);
            Utils.XmlSetAttributeString(nodeProfile, "path", data["path"]);
			Utils.XmlSetAttributeString(nodeProfile, "country", data["country"]);
			Utils.XmlSetAttributeString(nodeProfile, "ovpn", data["ovpn"]);

			Utils.XmlSetAttributeString(nodeProfile, "checked", "1");
		}

		// Parse the OpenVPN configuration file. Check if is valid, import external files, normalize.
		public Dictionary<string, string> OvpnParse(FileInfo file, bool interactive)
		{
			try
			{
                Dictionary<string, string> dictInfo = new Dictionary<string, string>();

                string ovpnOriginal = Platform.Instance.FileContentsReadText(file.FullName);                
                

                OvpnBuilder ovpnBuilder = new OvpnBuilder();
                ovpnBuilder.AppendDirectives(ovpnOriginal, "Original");
                string ovpnNormalized = ovpnBuilder.Get();

                OvpnBuilder.Directive directiveRemote = ovpnBuilder.GetOneDirective("remote");
                if(directiveRemote != null)
                {
                    string host = directiveRemote.Text;
                    int posPort = host.IndexOf(" ");
                    if (posPort != -1)
                        host = host.Substring(0, posPort).Trim();
                    dictInfo["remote"] = host;
                }
                else
                    dictInfo["remote"] = "";

                dictInfo["ovpn"] = ovpnNormalized;
				dictInfo["path"] = file.FullName;
				dictInfo["country"] = "";

				// Compute user-friendly name
				{
					string name = TitleForDisplay + file.FullName;

					name = name.Replace(GetPathScan(), "").Trim();

					//name = Regex.Replace(name, "udp", "", RegexOptions.IgnoreCase);
					//name = Regex.Replace(name, "tcp", "", RegexOptions.IgnoreCase);
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
					Engine.Instance.Logs.Log(LogType.Fatal, message);
				else
					Engine.Instance.Logs.Log(LogType.Warning, message);
				return null;
			}
		}
        
    }
}
