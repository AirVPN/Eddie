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

		public override void OnBuildConnectionActive(ConnectionInfo connection, ConnectionActive connectionActive)
		{
			base.OnBuildConnectionActive(connection, connectionActive);

			if (connectionActive.OpenVpnProfileStartup.ExistsDirective("auth-retry"))
				connectionActive.OpenVpnProfileStartup.AppendDirective("auth-retry", "none", "");
		}

		public override bool GetNeedRefresh()
		{
			int minInterval = RefreshInterval;
			if (minInterval == -1)
				minInterval = 60 * 60 * 24;
			if (m_lastTryRefresh + minInterval > UtilsCore.UnixTimeStamp())
				return false;

			return true;
		}

		public override string OnRefresh()
		{
			base.OnRefresh();

			string pathScan = Path;

			// Engine.Instance.Logs.LogVerbose(LanguageManager.GetText("ProviderRefreshStart, Title));

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
					Engine.Instance.Logs.Log(LogType.Warning, LanguageManager.GetText("ProvidersOpenVpnPathNotFound", pathScan, Title));
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

			Engine.Instance.Logs.LogVerbose(LanguageManager.GetText("ProviderRefreshDone", Title));

			return "";
		}

		public override void OnBuildConnections()
		{
			base.OnBuildConnections();

			lock (Profiles)
			{
				foreach (XmlElement nodeProfile in Profiles.ChildNodes)
				{
					string code = HashSHA256(nodeProfile.GetAttributeString("path", ""));

					ConnectionInfo infoConnection = Engine.Instance.GetConnectionInfo(code, this);

					infoConnection.DisplayName = nodeProfile.GetAttributeString("name", "");
					infoConnection.ProviderName = code;
					infoConnection.IpsEntry.Clear();
					infoConnection.IpsEntry.Add(nodeProfile.GetAttributeString("remote", ""));
					infoConnection.IpsExit.Clear();
					infoConnection.CountryCode = nodeProfile.GetAttributeString("country", "");
					infoConnection.Location = nodeProfile.GetAttributeString("location", "");
					infoConnection.Latitude = nodeProfile.GetAttributeFloat("latitude", 0);
					infoConnection.Longitude = nodeProfile.GetAttributeFloat("longitude", 0);
					infoConnection.ScoreBase = 0;
					infoConnection.Bandwidth = 0;
					infoConnection.BandwidthMax = 0;
					infoConnection.Users = -1;
					infoConnection.WarningOpen = "";
					infoConnection.WarningClosed = "";
					infoConnection.SupportCheck = false;
					infoConnection.SupportIPv4 = (infoConnection.IpsEntry.CountIPv4 != 0); // This is a supposition
					infoConnection.SupportIPv6 = SupportIPv6;
					infoConnection.OvpnDirectives = nodeProfile.GetAttributeString("openvpn_directives", "");
					infoConnection.Path = nodeProfile.GetAttributeString("path", "");

					if (infoConnection.DisplayName == "")
						infoConnection.DisplayName = ComputeFriendlyNameFromPath(infoConnection.Path);

					infoConnection.LastDiscover = nodeProfile.GetAttributeInt64("last-discover", 0);
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
					string subCode = HashSHA256(nodeProfile.GetAttributeString("path", ""));

					if (code == subCode)
					{
						nodeProfile.SetAttributeString("country", connection.CountryCode);
						nodeProfile.SetAttributeString("location", connection.Location);
						nodeProfile.SetAttributeDouble("latitude", connection.Latitude);
						nodeProfile.SetAttributeDouble("longitude", connection.Longitude);
						nodeProfile.SetAttributeInt64("last-discover", connection.LastDiscover);

						string autoName = ComputeFriendlyNameFromPath(connection.Path);
						if (autoName == connection.DisplayName)
							nodeProfile.SetAttributeString("name", ""); // Don't save, will be automatic
						else
							nodeProfile.SetAttributeString("name", connection.DisplayName);
					}
				}
			}
		}

		public string Path
		{
			get
			{
				string path = Storage.DocumentElement.GetAttributeString("path", "").Trim();
				//c:\Program Files\OpenVPN\config\
				if (path == "")
					path = Platform.Instance.GetDefaultOpenVpnConfigsPath();
				return path;
			}
			set
			{
				Storage.DocumentElement.SetAttributeString("path", value);
			}
		}

		public bool SupportIPv6
		{
			get
			{
				return Storage.DocumentElement.GetAttributeBool("support_ipv6", false);				
			}
			set
			{
				Storage.DocumentElement.SetAttributeBool("support_ipv6", value);
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
						string pathFind = nodeFind.GetAttributeString("path", "");
						if (pathFind == fileInfo.FullName)
						{
							nodeProfile = nodeFind;
							break;
						}
					}

					// Skip if is already checked					
					if ((nodeProfile != null) && (nodeProfile.GetAttributeString("checked", "") != ""))
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

						nodeProfile.SetAttributeString("remote", hosts);
						nodeProfile.SetAttributeString("path", file.FullName);

						nodeProfile.SetAttributeString("checked", "1");
					}
					catch (System.Exception e)
					{
						string message = LanguageManager.GetText("ProvidersOpenVpnErrorProfile", file.FullName, this.Title, e.Message); // TOTRANSLATE
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
