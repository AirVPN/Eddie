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
using System.Security.Cryptography;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Eddie.Common;

namespace Eddie.Core
{
	public class UtilsCore
	{
		public static string GetTempPath()
		{
			return Path.GetTempPath();
		}
		
		public static string HashSHA256(string password)
		{
			using (System.Security.Cryptography.SHA256Managed crypt = new System.Security.Cryptography.SHA256Managed())
			{
				System.Text.StringBuilder hash = new System.Text.StringBuilder();
				byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password), 0, Encoding.UTF8.GetByteCount(password));
				foreach (byte theByte in crypto)
				{
					hash.Append(theByte.ToString("x2"));
				}
				return hash.ToString();
			}
		}

		public static byte[] AssocToUtf8Bytes(Dictionary<string, string> assoc)
		{
			string output = "";
			foreach (KeyValuePair<string, string> kp in assoc)
			{
				output += UtilsString.Base64Encode(UtilsString.StringToUtf8Bytes(kp.Key)) + ":" + UtilsString.Base64Encode(UtilsString.StringToUtf8Bytes(kp.Value)) + "\n";
			}
			return System.Text.Encoding.UTF8.GetBytes(output);
		}

		public static byte[] AssocToUtf8Bytes(Dictionary<string, byte[]> assoc)
		{
			string output = "";
			foreach (KeyValuePair<string, byte[]> kp in assoc)
			{
				output += UtilsString.Base64Encode(UtilsString.StringToUtf8Bytes(kp.Key)) + ":" + UtilsString.Base64Encode(kp.Value) + "\n";
			}
			return System.Text.Encoding.UTF8.GetBytes(output);
		}

		public static string GetNameFromPath(string path)
		{
			return new FileInfo(path).Name;
		}

		public static int UnixTimeStamp()
		{
			return Conversions.ToUnixTime(DateTime.UtcNow);
		}

		public static Int64 UnixTimeStampMs()
		{
			return Conversions.ToUnixTimeMs(DateTime.UtcNow);
		}

		public static string HostFromUrl(string url)
		{
			try
			{
				Uri uri = new Uri(url);
				return uri.Host;
			}
			catch (Exception)
			{
				return "";
			}
		}

		public static int CompareVersions(string v1, string v2)
		{
			char[] splitTokens = new char[] { '.', ',' };
			string[] tokens1 = v1.Split(splitTokens, StringSplitOptions.RemoveEmptyEntries);
			string[] tokens2 = v2.Split(splitTokens, StringSplitOptions.RemoveEmptyEntries);

			for (int i = 0; i < Math.Max(tokens1.Length, tokens2.Length); i++)
			{
				int t1 = 0;
				int t2 = 0;
				if (i < tokens1.Length)
				{
					try
					{
						if (Int32.TryParse(tokens1[i], out t1) == false)
							t1 = 0;
					}
					finally
					{
					}
				}
				if (i < tokens2.Length)
				{
					try
					{
						if (Int32.TryParse(tokens2[i], out t2) == false)
							t2 = 0;
					}
					finally
					{
					}
				}

				if (t1 < t2) return -1;
				if (t1 > t2) return 1;
			}

			return 0;
		}

		public static List<string> GetNetworkGateways()
		{
			List<string> list = new List<string>();

			foreach (NetworkInterface f in NetworkInterface.GetAllNetworkInterfaces())
			{
				if (f.OperationalStatus == OperationalStatus.Up)
				{
					foreach (GatewayIPAddressInformation d in f.GetIPProperties().GatewayAddresses)
					{
						string ip = d.Address.ToString();
						if ((IpAddress.IsIP(ip)) && (ip != "0.0.0.0") && (list.Contains(ip) == false))
						{
							list.Add(ip);
						}
					}
				}
			}

			return list;
		}
	}
}

