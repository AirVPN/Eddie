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
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Eddie.Core
{
    public class Utils
    {

		public static string GetTempPath()
        {
            return Path.GetTempPath();
        }

        public static string GetTempFileName(bool full)
        {
			string hash = Guid.NewGuid().ToString();
			string path = hash + ".tmp";
            if (full)
                path = GetTempPath() + path;
            return path;            
        }

		public static string GetRandomToken()
		{
			Random random = new Random((int)DateTime.Now.Ticks);
			StringBuilder builder = new StringBuilder();
			char ch;
			for (int i = 0; i < 32; i++)
			{
				ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
				builder.Append(ch);
			}

			return builder.ToString().ToLowerInvariant();
		}

		public static string BytesToHex(byte[] bytes) // 2.10.1
		{
			return BitConverter.ToString(bytes).Replace("-", "").ToLower();
		}

        public static string SHA256(string password)
        {
            System.Security.Cryptography.SHA256Managed crypt = new System.Security.Cryptography.SHA256Managed();
            System.Text.StringBuilder hash = new System.Text.StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password), 0, Encoding.UTF8.GetByteCount(password));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }

        public static string GetNameFromPath(string path)
        {
            return new FileInfo(path).Name;
        }

        public static string ExtractBetween(string str, string from, string to)
        {
            int iPos1 = str.IndexOf(from);
            if(iPos1 != -1)
            {
                int iPos2 = str.IndexOf(to, iPos1 + from.Length);
                if(iPos2 != -1)
                {
                    return str.Substring(iPos1 + from.Length, iPos2 - iPos1 - from.Length);
                }
            }

            return "";
        }

		public static XmlElement XmlGetFirstElementByTagName(XmlElement node, string name)
		{
            foreach (XmlElement xmlChild in node.ChildNodes)
                if (xmlChild.Name == name)
                    return xmlChild;
            return null;
            /*
			XmlNodeList list = node.GetElementsByTagName(name);
			if (list.Count == 0)
				return null;
			else
				return list[0] as XmlElement;
            */
		}

		public static bool XmlExistsAttribute(XmlNode node, string name)
		{
			XmlNode nodeAttr = node.Attributes[name];
			if (nodeAttr == null)
				return false;
			else
				return true;
		}

        public static string XmlGetAttributeString(XmlNode node, string name, string def)
        {
            XmlNode nodeAttr = node.Attributes[name];
            if (nodeAttr == null)
                return def;
            else
                return nodeAttr.Value;
        }

		public static string[] XmlGetAttributeStringArray(XmlNode node, string name)
		{
			XmlNode nodeAttr = node.Attributes[name];
			if (nodeAttr == null)
				return new string[0];
			else if(nodeAttr.Value == "") // New in 2.8
				return new string[0];
			else
				return nodeAttr.Value.Split(',');
		}

		public static bool XmlGetAttributeBool(XmlNode node, string name, bool def)
		{
			XmlNode nodeAttr = node.Attributes[name];
			if (nodeAttr == null)
				return def;
			else
				return Conversions.ToBool(nodeAttr.Value);
		}

		public static Int64 XmlGetAttributeInt64(XmlNode node, string name, Int64 def)
		{
			XmlNode nodeAttr = node.Attributes[name];
			if (nodeAttr == null)
				return def;
			else
				return Conversions.ToInt64(nodeAttr.Value);
		}

		public static void XmlSetAttributeString(XmlElement node, string name, string val)
		{
			node.SetAttribute(name, val);
		}

		public static void XmlSetAttributeStringArray(XmlElement node, string name, string[] val)
		{
			if ((val == null) || (val.Length == 0)) // Added in 2.8
				node.SetAttribute(name, "");
			else
				node.SetAttribute(name, String.Join(",", val)); // TODO: Escaping
		}

		public static void XmlSetAttributeBool(XmlElement node, string name, bool val)
		{
			node.SetAttribute(name, Conversions.ToString(val));			
		}

		public static void XmlSetAttributeInt64(XmlElement node, string name, Int64 val)
		{
			node.SetAttribute(name, Conversions.ToString(val));
		}

        public static void XmlRenameTagName(XmlElement parent, string oldName, string newName)
        {
            foreach(XmlElement e in parent.GetElementsByTagName(oldName))
            {
                // TODO
            }
        }

        public static string XmlGetBody(XmlElement node)
        {
            if (node == null)
                return "";
            return node.InnerText;
        }

        public static void XmlCopyElement(XmlElement source, XmlElement parentDestination)
        {
            XmlNode xmlClone = parentDestination.OwnerDocument.ImportNode(source, true);
            parentDestination.AppendChild(xmlClone);
        }

        public static XmlElement XmlCreateElement(string name)
        {
            XmlDocument xmlDoc = new XmlDocument();
            return xmlDoc.CreateElement(name);
        }

        public static string UrlEncode(string url)
		{
			//return HttpUtility.UrlEncode(url); // Require System.Web
			return Uri.EscapeUriString(url);
		}

		public static int UnixTimeStamp()
		{
			return Conversions.ToUnixTime(DateTime.UtcNow);
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

        public static string StringCleanSpace(string v)
        {
            for (; ; )
            {
				string orig = v;

                v = v.Replace("  ", " ");
                v = v.Replace("\t\t", "\t");
                v = v.Trim();

                if (v == orig)
                    break;
            }

            return v;
        }

        public static string StringRemoveEmptyLines(string v)
        {
            for (;;)
            {
                string orig = v;

                v = v.Replace("\n\n", "\n");
                v = v.Trim();

                if (v == orig)
                    break;
            }

            return v;
        }

        public static string StringNormalizePath(string path)
        {
            // Note: Used only in already-quoted path.
            path = path.Replace("'", "");
            path = path.Replace("`", "");            
            return path;
        }
                
        public static string FormatTime(Int64 unix)
		{
			if (unix == 0)
				return "-";

			string o = "";
			Int64 now = UnixTimeStamp();
			if (unix == now)
				o = Messages.TimeJustNow;
			else if (unix < now)
				o = FormatSeconds(now - unix) + " " + Messages.TimeAgo;
			else
				o = FormatSeconds(unix - now) + " " + Messages.TimeRemain;
			return o;
		}

		public static string FormatSeconds(Int64 v)
		{
			TimeSpan ts = new TimeSpan(0, 0, (int) v);
			string o = "";

			if (ts.Days > 1)
				o += ts.Days + " days,";
			else if (ts.Days == 1)
				o += "1 day,";

			if (ts.Hours > 1)
				o += ts.Hours + " hours,";
			else if (ts.Hours == 1)
				o += "1 hour,";

			if (ts.Minutes > 1)
				o += ts.Minutes + " minutes,";
			else if (ts.Minutes == 1)
				o += "1 minute,";

			if (ts.Seconds > 1)
				o += ts.Seconds + " seconds,";
			else if (ts.Seconds == 1)
				o += "1 second,";

			return o.Trim(',');
		}

        public static string FormatBytes(Int64 bytes, bool speedSec, bool showBytes)
        {
            string userUnit = Engine.Instance.Storage.Get("ui.unit");
            bool iec = Engine.Instance.Storage.GetBool("ui.iec");
            return FormatBytes(bytes, speedSec, showBytes, userUnit, iec);
        }

        public static string FormatBytes(Int64 bytes, bool speedSec, bool showBytes, string userUnit, bool iec)
        {
            Int64 v = bytes;

            if (userUnit == "")
            {
                if (speedSec)
                    userUnit = "bits";
            }

            if (userUnit == "bits")
            {
                v *= 8;
            }

            Int64 number = 0;
            string unit = "";
            if (userUnit == "bits")
            {               
                if(iec == false) 
                    FormatBytesEx(v, new string[] { "bit", "kbit", "Mbit", "Gbit", "Tbit", "Pbit" }, 1000, ref number, ref unit);
                else
                    FormatBytesEx(v, new string[] { "bit", "kibit", "Mibit", "Gibit", "Tibit", "Pibit" }, 1024, ref number, ref unit);
            }
            else
            {
                if(iec == false)
                    FormatBytesEx(v, new string[] { "B", "kB", "MB", "GB", "TB", "PB" }, 1000, ref number, ref unit);
                else
                    FormatBytesEx(v, new string[] { "B", "KiB", "MiB", "GiB", "TiB", "PiB" }, 1024, ref number, ref unit);
            }

            string output = number.ToString() + " " + unit;
            if (speedSec)
                output += "/s";
            if ((showBytes) && (bytes >= 0))
            {
                output += " (" + bytes.ToString() + " bytes";
                if (speedSec)
                    output += "/s";
                output += ")";
            }
            return output;
        }

		public static void FormatBytesEx(Int64 val, string[] suf, int logBase, ref Int64 number, ref string unit)
        {
            if (val <= 0)
            {
                number = 0;
                unit = suf[0];
            }
            else
            {
                //string[] suf = { "B", "KB", "MB", "GB", "TB", "PB" };                
                int place = Conversions.ToInt32(Math.Floor(Math.Log(val, logBase)));
                double num = Math.Round(val / Math.Pow(logBase, place), 1);
				number = Conversions.ToInt64(num);
                unit = suf[place];
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

        

		public static string SafeString(string value)
		{
			Regex rgx = new Regex("[^a-zA-Z0-9 -_]");
			value = rgx.Replace(value, "");
			return value;
		}

        public static string SafeStringHost(string value)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9\\.-_]");
            value = rgx.Replace(value, "");
            return value;
        }

        public static string RegExMatchOne(string input, string pattern)
		{
			Match match = Regex.Match(input, pattern, RegexOptions.Multiline);
			if (match.Success)
			{
				return match.Groups[1].Value;
			}
			else
				return "";
		}

		public static List<string> RegExMatchSingle(string input, string pattern)
		{
			Match match = Regex.Match(input, pattern, RegexOptions.Multiline);
			if (match.Success)
			{
				List<string> result = new List<string>();
				for (int i = 1; i < match.Groups.Count; i++)
					result.Add(match.Groups[i].Value);
				return result;
			}
			else
				return null;
		}

		public static List<List<string>> RegExMatchMulti(string input, string pattern)
		{
			List<List<string>> result = new List<List<string>>();

			Regex regex = new Regex(pattern, RegexOptions.Multiline);
			foreach (Match match in regex.Matches(input))
			{
				List<string> result2 = new List<string>();
				result.Add(result2);

				for (int i = 1; i < match.Groups.Count; i++)
					result2.Add(match.Groups[i].Value);
			}
			return result;			
		}

		public static string ListStringToCommaString(List<string> list)
		{
			string result = "";
			foreach (string str in list)
			{
				if (result != "")
					result += ",";
				result += str;
			}
			return result;
		}

		public static string ListStringToCommaString(List<List<string>> list)
		{
			string result = "";
			foreach (List<string> list2 in list)
			{
				foreach (string str in list2)
				{
					if (result != "")
						result += ",";
					result += str;
				}
			}
			return result;
		}

		public static List<string> CommaStringToListString(string lines)
		{
			List<string> result = new List<string> ();

			// Normalization
			lines = lines.Replace ("\n", ",");
			lines = lines.Replace ("\r", ",");
			lines = lines.Replace (";", ",");
			lines = lines.Replace (" ", ",");
			lines = lines.Replace ("\u2028", ","); // OS X

			string[] items = lines.Split (',');
			foreach (string item in items)
            {
				if (item.Trim () != "")
					result.Add (item);
			}

			return result;
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
						if( (IpAddress.IsIP(ip)) && (ip != "0.0.0.0") && (list.Contains(ip) == false) )
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

