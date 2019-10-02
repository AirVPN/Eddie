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
using System.Security.Cryptography;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Eddie.Common;

namespace Eddie.Core
{
	public class UtilsString
	{
		public static string BytesToHex(byte[] bytes) // 2.10.1
		{
			return BitConverter.ToString(bytes).Replace("-", "").ToLower();
		}

		public static string Base64Encode(byte[] data)
		{
			return System.Convert.ToBase64String(data);
		}

		public static byte[] Base64Decode(string data)
		{
			return System.Convert.FromBase64String(data);
		}

		public static byte[] StringToUtf8Bytes(string data)
		{
			return System.Text.Encoding.UTF8.GetBytes(data);
		}

		public static string ExtractBetween(string str, string from, string to)
		{
			int iPos1 = str.IndexOf(from);
			if (iPos1 != -1)
			{
				int iPos2 = str.IndexOf(to, iPos1 + from.Length);
				if (iPos2 != -1)
				{
					return str.Substring(iPos1 + from.Length, iPos2 - iPos1 - from.Length);
				}
			}

			return "";
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
			for (; ; )
			{
				string orig = v;

				v = v.Replace("\n\n", "\n");
				v = v.Trim();

				if (v == orig)
					break;
			}

			return v;
		}

		// StringSafe* set of functions are NOT used to prune/escape values destinated to shell execution. Use SystemShell class instead.
		public static string StringSafe(string value)
		{
			return StringPruneCharsNotIn(value, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 -_");
		}

		public static string StringSafeAlphaNumeric(string value)
		{
			return StringPruneCharsNotIn(value, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");
		}

		public static string StringPruneCharsNotIn(string value, string chars)
		{
			string result = "";
			foreach (char c in value.ToCharArray())
				if (chars.IndexOf(c) != -1)
					result += c;
			return result;
		}

		public static string RegExReplace(string input, string pattern, string replacement)
		{
			return Regex.Replace(input, pattern, replacement);
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

		public static List<string> StringToList(string lines)
		{
			return StringToList(lines, "\n\r; ,", true, true, true, true);
		}

		public static List<string> StringToList(string lines, string separators)
		{
			return StringToList(lines, separators, true, true, true, true);
		}

		public static List<string> StringToList(string str, string separatorsStr, bool checkQuote, bool skipEmpty, bool skipComment, bool trimAuto)
		{
			char[] characters = str.ToCharArray();
			char[] separators = separatorsStr.ToCharArray();
			List<string> result = new List<string>();
			string tempStr = "";
			bool inQuote = false;
			foreach (char ch in characters)
			{
				if (ch == '"')
					inQuote = !inQuote;

				if ((inQuote == false) && (Array.IndexOf(separators, ch) > -1))
				{
					StringToListAddItem(ref result, tempStr, skipEmpty, skipComment, trimAuto);
					tempStr = "";
				}
				else
					tempStr += ch;
			}
			if (tempStr != "")
				StringToListAddItem(ref result, tempStr, skipEmpty, skipComment, trimAuto);
			return result;
		}

		private static void StringToListAddItem(ref List<string> result, string item, bool skipEmpty, bool skipComment, bool trimAuto)
		{
			string v = item;
			if (trimAuto)
				v = v.Trim();
			if ((skipEmpty) && (v == ""))
				return;
			if ((skipComment) && (v.StartsWith("#")))
				return;
			result.Add(item);
		}
	}
}

