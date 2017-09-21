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

		public bool OptionRecursive = true; // Maybe an option in next versions
		public bool OptionRemoveIfFileMissing = true; // Maybe an option in next versions

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

		public override void OnBuildOvpn(ConnectionInfo connection, OvpnBuilder ovpn)
		{
			base.OnBuildOvpn(connection, ovpn);

			if (ovpn.ExistsDirective("auth-retry"))
				ovpn.AppendDirective("auth-retry", "none", "");
		}

		public override string Refresh()
		{
			string pathScan = Path;

			List<ConnectionInfo> connections = new List<ConnectionInfo>();

			// Scan directory
			if (pathScan != "")
			{
				if (Directory.Exists(pathScan))
				{
					ScanDir(pathScan.Trim(), OptionRecursive, connections);
				}
				else
				{
					Engine.Instance.Logs.Log(LogType.Warning, MessagesFormatter.Format(Messages.ProvidersOpenVpnPathNotFound, pathScan, Title));
				}
			}

			// Remove profiles
			for (;;)
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

			// Clean flags
			foreach (XmlElement nodeProfile in Profiles.ChildNodes)
			{
				nodeProfile.Attributes.RemoveNamedItem("checked");
			}

			return "";
		}

		public override void OnBuildConnections()
		{
			base.OnBuildConnections();

			lock (Profiles)
			{
				foreach (XmlElement nodeProfile in Profiles.ChildNodes)
				{
					string code = HashSHA256(Utils.XmlGetAttributeString(nodeProfile, "path", ""));

					ConnectionInfo infoConnection = Engine.Instance.GetConnectionInfo(code, this);

					infoConnection.DisplayName = Utils.XmlGetAttributeString(nodeProfile, "name", "");
					infoConnection.ProviderName = code;
					infoConnection.IpsEntry.Clear();
					infoConnection.IpsEntry.Add(Utils.XmlGetAttributeString(nodeProfile, "remote", ""));
					infoConnection.IpsExit.Clear();
					infoConnection.CountryCode = Utils.XmlGetAttributeString(nodeProfile, "country", "");
					infoConnection.Location = Utils.XmlGetAttributeString(nodeProfile, "location", "");
					infoConnection.Latitude = Utils.XmlGetAttributeFloat(nodeProfile, "latitude", 0);
					infoConnection.Longitude = Utils.XmlGetAttributeFloat(nodeProfile, "longitude", 0);
					infoConnection.ScoreBase = 0;
					infoConnection.Bandwidth = 0;
					infoConnection.BandwidthMax = 0;
					infoConnection.Users = -1;
					infoConnection.WarningOpen = "";
					infoConnection.WarningClosed = "";
					infoConnection.SupportCheck = false;
					infoConnection.SupportIPv4 = (infoConnection.IpsEntry.CountIPv4 != 0); // This is a supposition
					infoConnection.SupportIPv6 = (infoConnection.IpsEntry.CountIPv6 != 0); // This is a supposition
					infoConnection.OvpnDirectives = Utils.XmlGetAttributeString(nodeProfile, "openvpn_directives", "");
					infoConnection.Path = Utils.XmlGetAttributeString(nodeProfile, "path", "");

					if (infoConnection.DisplayName == "")
						infoConnection.DisplayName = ComputeFriendlyNameFromPath(infoConnection.Path);

					infoConnection.LastDiscover = Utils.XmlGetAttributeInt64(nodeProfile, "last-discover", 0);
					infoConnection.NeedDiscover = true;
				}
			}
		}

		public override void OnChangeConnection(ConnectionInfo connection)
		{
			base.OnChangeConnection(connection);

			string code = HashSHA256(connection.Path);

			lock (Profiles)
			{
				foreach (XmlElement nodeProfile in Profiles.ChildNodes)
				{
					string subCode = HashSHA256(Utils.XmlGetAttributeString(nodeProfile, "path", ""));

					if (code == subCode)
					{
						Utils.XmlSetAttributeString(nodeProfile, "country", connection.CountryCode);
						Utils.XmlSetAttributeString(nodeProfile, "location", connection.Location);
						Utils.XmlSetAttributeFloat(nodeProfile, "latitude", connection.Latitude);
						Utils.XmlSetAttributeFloat(nodeProfile, "longitude", connection.Longitude);
						Utils.XmlSetAttributeInt64(nodeProfile, "last-discover", connection.LastDiscover);

						string autoName = ComputeFriendlyNameFromPath(connection.Path);
						if (autoName == connection.DisplayName)
							Utils.XmlSetAttributeString(nodeProfile, "name", ""); // Don't save, will be automatic
						else
							Utils.XmlSetAttributeString(nodeProfile, "name", connection.DisplayName);
					}
				}
			}
		}

		public string Path
		{
			get
			{
				string path = Utils.XmlGetAttributeString(Storage.DocumentElement, "path", "").Trim();
				//c:\Program Files\OpenVPN\config\
				if (path == "")
					path = Platform.Instance.GetDefaultOpenVpnConfigsPath();
				return path;
			}
			set
			{
				Utils.XmlSetAttributeString(Storage.DocumentElement, "path", value);
			}
		}

		public void ScanDir(string path, bool recursive, List<ConnectionInfo> connections)
		{
			if (path == "")
				return;

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

					if (Platform.Instance.FileExists(filePath) == false)
						continue;
						
					// Compute values
					FileInfo file = new FileInfo(filePath);
					string hosts = "";

					try
					{
						string ovpnOriginal = Platform.Instance.FileContentsReadText(file.FullName);

						OvpnBuilder ovpnBuilder = new OvpnBuilder();
						ovpnBuilder.AppendDirectives(ovpnOriginal, "Original");
						//string ovpnNormalized = ovpnBuilder.Get();

						foreach(OvpnBuilder.Directive remoteDirective in ovpnBuilder.GetDirectiveList("remote"))
						{
							string host = remoteDirective.Text;
							int posPort = host.IndexOf(" ");
							if (posPort != -1)
								host = host.Substring(0, posPort).Trim();
							if (hosts != "")
								hosts += ",";
							hosts += host;
						}						

						if (nodeProfile == null)
						{
							nodeProfile = Profiles.OwnerDocument.CreateElement("profile");
							Profiles.AppendChild(nodeProfile);
						}
												
						Utils.XmlSetAttributeString(nodeProfile, "remote", hosts);
						Utils.XmlSetAttributeString(nodeProfile, "path", file.FullName);

						Utils.XmlSetAttributeString(nodeProfile, "checked", "1");
					}
					catch (System.Exception e)
					{
						string message = MessagesFormatter.Format(Messages.ProvidersOpenVpnErrorProfile, file.FullName, this.Title, e.Message); // TOTRANSLATE
						Engine.Instance.Logs.Log(LogType.Warning, message);
					}					
				}

				if (recursive)
				{
					foreach (string dirPath in Directory.GetDirectories(path))
					{
						ScanDir(dirPath, recursive, connections);
					}
				}
			}
			catch (System.Exception e)
			{
				Engine.Instance.Logs.Log(e);
			}
		}

		public string ComputeFriendlyNameFromPath(string path)
		{
			FileInfo file = new FileInfo(path);
			string name = file.FullName;

			if (Path != "")
				name = name.Replace(Path, "").Trim();

			name = Regex.Replace(name, ".tblk", "", RegexOptions.IgnoreCase); // TunnelBlick
			name = Regex.Replace(name, ".ovpn", "", RegexOptions.IgnoreCase); // OpenVPN

			name = name.Trim(" -\\/".ToCharArray());
			return TitleForDisplay + name;
		}		
	}
}
