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

		private string m_loadFormat = "";
		private string m_loadPassword = "";

		public Storage()
		{
			Id = RandomGenerator.GetRandomId64();

			if ((Platform.Instance.OsCredentialSystemDefault()) && (Platform.Instance.OsCredentialSystemName() != ""))
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

		public void Save()
		{
			bool remember = Engine.Instance.Options.GetBool("remember");

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

					foreach (Option option in Engine.Instance.Options.Dict.Values)
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
					foreach (Providers.IProvider provider in Engine.Instance.ProvidersManager.Providers)
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
						if ((m_loadFormat != "v2s") || (SavePassword == "") || (SavePassword != m_loadPassword))
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
								Engine.Instance.Options.ResetAll(true);
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
			catch (Exception ex)
			{
				bool ask = Engine.Instance.OnAskYesNo(LanguageManager.GetText("OptionsReadError", ex.Message));
				if (ask)
				{
					Engine.Instance.Options.ResetAll(true);
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

				Engine.Instance.Options.ResetAll(true);

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
					if (name != "")
						options[name] = value;
				}

				CompatibilityManager.FixOptions(options);
				foreach (KeyValuePair<string, string> item in options)
					Engine.Instance.Options.Set(item.Key, item.Value);
				CompatibilityManager.FixOptions(Engine.Instance.Options);

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
			encrypted.CopyTo(n, 3 + 64);

			return n;
		}

		public static void DecodeFormat(byte[] b, out string header, out string id, out byte[] dataEncrypted)
		{
			byte[] bHeader = new byte[3];
			Array.Copy(b, 0, bHeader, 0, 3);
			header = Encoding.ASCII.GetString(bHeader);
			if (header.StartsWithInv("v1"))
			{
				// Compatibility				
				id = RandomGenerator.GetRandomId64();
				dataEncrypted = new byte[b.Length - 3];
				Array.Copy(b, 3, dataEncrypted, 0, b.Length - 3);
			}
			else if (header.StartsWithInv("v2"))
			{
				byte[] bId = new byte[64];
				Array.Copy(b, 3, bId, 0, 64);
				id = Encoding.ASCII.GetString(bId);
				dataEncrypted = new byte[b.Length - 3 - 64];
				Array.Copy(b, 3 + 64, dataEncrypted, 0, b.Length - 3 - 64);
			}
			else
			{
				throw new Exception("Read fail");
			}
		}
	}
}
