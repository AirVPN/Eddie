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
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace AirVPN.Core
{
	public class AirExchange
	{
		// This is the only method about exchange data between this software and AirVPN infrastructure.
		// We don't use SSL. Useless layer in our case, and we need to fetch hostname and direct IP that don't permit common-name match.
		
		// 'S' is the AES 256 bit one-time session key, crypted with a RSA 4096 public-key.
		// 'D' is the data from the client to our server, crypted with the AES.
		// The server answer is XML decrypted with the same AES session.
		public static XmlDocument Fetch(string host, Dictionary<string, string> parameters)
		{
			// AES				
			RijndaelManaged rijAlg = new RijndaelManaged();
			rijAlg.KeySize = 256;
			rijAlg.GenerateKey();
			rijAlg.GenerateIV();

			// Generate S

			string airAuthPublicKey = ResourcesFiles.GetString("auth.xml");
			StringReader sr = new System.IO.StringReader(airAuthPublicKey);
			System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
			RSAParameters publicKey = (RSAParameters)xs.Deserialize(sr);

			RSACryptoServiceProvider csp = new RSACryptoServiceProvider();
			csp.ImportParameters(publicKey);

			Dictionary<string, byte[]> assocParamS = new Dictionary<string, byte[]>();
			assocParamS["key"] = rijAlg.Key;
			assocParamS["iv"] = rijAlg.IV;
				
			byte[] bytesParamS = csp.Encrypt(AssocToBytes(assocParamS), false);

			// Generate D

			byte[] aesDataIn = AssocToBytes(parameters);
			MemoryStream aesCryptStream = new MemoryStream();
			ICryptoTransform aesEncryptor = rijAlg.CreateEncryptor();
			CryptoStream aesCryptStream2 = new CryptoStream(aesCryptStream, aesEncryptor, CryptoStreamMode.Write);
			aesCryptStream2.Write(aesDataIn, 0, aesDataIn.Length);
			aesCryptStream2.FlushFinalBlock();
			byte[] bytesParamD = aesCryptStream.ToArray();

			// HTTP Fetch
			string url = "http://" + host + "?s=" + Uri.EscapeUriString(Base64Encode(bytesParamS)) + "&d=" + Uri.EscapeUriString(Base64Encode(bytesParamD));
			byte[] fetchResponse = Engine.Instance.FetchUrl(url);

			// Decrypt answer

			MemoryStream aesDecryptStream = new MemoryStream();
			ICryptoTransform aesDecryptor = rijAlg.CreateDecryptor();
			CryptoStream aesDecryptStream2 = new CryptoStream(aesDecryptStream, aesDecryptor, CryptoStreamMode.Write);
			aesDecryptStream2.Write(fetchResponse, 0, fetchResponse.Length);
			aesDecryptStream2.FlushFinalBlock();
			byte[] fetchResponsePlain = aesDecryptStream.ToArray();

			string finalData = System.Text.Encoding.UTF8.GetString(fetchResponsePlain);

			XmlDocument doc = new XmlDocument();
			doc.LoadXml(finalData);
			return doc;			
		}

		public static XmlDocument Fetch(Dictionary<string, string> parameters)
		{
			parameters["login"] = Engine.Instance.Storage.Get("login");
			parameters["password"] = Engine.Instance.Storage.Get("password");
			parameters["system"] = Platform.Instance.GetSystemCode();
			parameters["version"] = Constants.VersionInt.ToString(CultureInfo.InvariantCulture);

			List<string> hosts = new List<string>();
			
			if (Engine.Instance.Storage.Manifest != null)
			{
				XmlNodeList nodesHosts = Engine.Instance.Storage.Manifest.SelectNodes("//hosts/host");
				foreach (XmlNode nodeHost in nodesHosts)
				{
					hosts.Add(nodeHost.Attributes["address"].Value);
				}
			}

			string firstError = "";

			foreach (string host in hosts)
			{
				if (NetworkLocking.Instance.GetActive())
				{
					// If locked network are enabled, skip the hostname and try only by IP.
					// To avoid DNS issue (generally, to avoid losing time).
					if (Utils.IsIP(host) == false)
						continue;
				}

				try
				{
					RouteScope routeScope = new RouteScope(host);
					XmlDocument xmlDoc = AirExchange.Fetch(host, parameters);
					routeScope.End();
					if (xmlDoc == null)
						throw new Exception("No answer.");

					if (xmlDoc.DocumentElement.Attributes["error"] != null)
						throw new Exception(xmlDoc.DocumentElement.Attributes["error"].Value);

					return xmlDoc;
				}
				catch (Exception e)
				{
					if (firstError == "")
						firstError = e.Message;
				}
			}

			throw new Exception(firstError);			
		}

		/* --------------------------------------
		Utils
		-------------------------------------- */

		private static string Base64Encode(byte[] data)
		{
			return System.Convert.ToBase64String(data);
		}

		private static byte[] Base64Decode(string data)
		{
			return System.Convert.FromBase64String(data);
		}

		private static byte[] StringToBytes(string data)
		{
			return System.Text.Encoding.UTF8.GetBytes(data);
		}

		private static byte[] AssocToBytes(Dictionary<string, string> assoc)
		{
			string output = "";
			foreach (KeyValuePair<string, string> kp in assoc)
			{
				output += Base64Encode(StringToBytes(kp.Key)) + ":" + Base64Encode(StringToBytes(kp.Value)) + "\n";
			}
			return System.Text.Encoding.UTF8.GetBytes(output);			
		}

		private static byte[] AssocToBytes(Dictionary<string, byte[]> assoc)
		{
			string output = "";
			foreach (KeyValuePair<string, byte[]> kp in assoc)
			{
				output += Base64Encode(StringToBytes(kp.Key)) + ":" + Base64Encode(kp.Value) + "\n";
			}
			return System.Text.Encoding.UTF8.GetBytes(output);			
		}
	}
}
