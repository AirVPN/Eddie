// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2026 AirVPN (support@airvpn.org) / https://airvpn.org
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

namespace Eddie.Core
{
	public static class LanguageManager
	{
		public static string MessageInitialization = "Initialization";

		private sealed class Locale
		{
			public string Parent = "";
			public Dictionary<string, string> Items = new Dictionary<string, string>();
		}

		private static string m_currentCode = "inv";
		private static Dictionary<string, Locale> m_locales = new Dictionary<string, Locale>();

		public static bool Init()
		{
			m_currentCode = "inv";
			m_locales = new Dictionary<string, Locale>();

			string localesDir = Engine.Instance.LocateResource("locales");
			if (string.IsNullOrEmpty(localesDir) || (Directory.Exists(localesDir) == false))
				return false;

			foreach (string jsonPath in Directory.EnumerateFiles(localesDir, "*.json"))
			{
				string code = Path.GetFileNameWithoutExtension(jsonPath);
				if (string.IsNullOrEmpty(code))
					continue;

				Json jData = null;
				if (Json.TryParse(Platform.Instance.FileContentsReadText(jsonPath), out jData) == false)
					continue;

				Locale locale = new Locale();
				locale.Parent = jData["parent"].ValueString;
				if (string.IsNullOrEmpty(locale.Parent) && code != "inv")
					locale.Parent = "inv";

				Json jItems = jData["items"].Json;
				if (jItems != null)
				{
					foreach (KeyValuePair<string, object> kp in jItems.GetDictionary())
					{
						string itemText = kp.Value as string;
						if (itemText != null)
							locale.Items[kp.Key] = itemText;
					}
				}

				m_locales[code] = locale;
			}

			return m_locales.ContainsKey("inv");
		}

		public static Json GetJson()
		{
			Json jData = new Json();

			foreach (KeyValuePair<string, Locale> kp in m_locales)
			{
				Json jLocale = new Json();
				jLocale["code"].Value = kp.Key;
				jLocale["parent"].Value = kp.Value.Parent;

				Json jItems = new Json();
				foreach (KeyValuePair<string, string> itemPair in kp.Value.Items)
				{
					jItems[itemPair.Key].Value = itemPair.Value;
				}
				jLocale["items"].Value = jItems;

				jData[kp.Key].Value = jLocale;
			}

			return jData;
		}

		public static void SetIso(string iso)
		{
			if (string.IsNullOrEmpty(iso) || iso == "auto")
				m_currentCode = NormalizeCode(Platform.Instance.GetUserLocale());
			else
				m_currentCode = NormalizeCode(iso);
		}

		public static string GetText(LanguageItems id)
		{
			string idStr = id.ToString();
			string code = m_currentCode;

			// Bounded walk: explicit parent chain in m_locales, plus BCP47 fallback (xx-YY -> xx) for codes not present in the shipped set.
			for (int safety = 0; safety < 16; safety++)
			{
				if (m_locales.TryGetValue(code, out Locale locale))
				{
					if (locale.Items.TryGetValue(idStr, out string text))
						return text;

					if (code == "inv")
						break;

					code = string.IsNullOrEmpty(locale.Parent) ? "inv" : locale.Parent;
				}
				else
				{
					int dash = code.LastIndexOf('-');
					if (dash > 0)
						code = code.Substring(0, dash);
					else if (code == "inv")
						break;
					else
						code = "inv";
				}
			}

			return "{lang:" + idStr + "}";
		}

		public static string GetText(LanguageItems id, string param1)
		{
			string format = GetText(id);
			return format.Replace("{1}", param1);
		}

		public static string GetText(LanguageItems id, string param1, string param2)
		{
			string format = GetText(id);
			string o = format;
			o = o.Replace("{1}", param1);
			o = o.Replace("{2}", param2);
			return o;
		}

		public static string GetText(LanguageItems id, string param1, string param2, string param3)
		{
			string format = GetText(id);
			string o = format;
			o = o.Replace("{1}", param1);
			o = o.Replace("{2}", param2);
			o = o.Replace("{3}", param3);
			return o;
		}

		public static string GetText(LanguageItems id, string param1, string param2, string param3, string param4)
		{
			string format = GetText(id);
			string o = format;
			o = o.Replace("{1}", param1);
			o = o.Replace("{2}", param2);
			o = o.Replace("{3}", param3);
			o = o.Replace("{4}", param4);
			return o;
		}

		public static string GetText(LanguageItems id, string param1, string param2, string param3, string param4, string param5)
		{
			string format = GetText(id);
			string o = format;
			o = o.Replace("{1}", param1);
			o = o.Replace("{2}", param2);
			o = o.Replace("{3}", param3);
			o = o.Replace("{4}", param4);
			o = o.Replace("{5}", param5);
			return o;
		}

		// TOCLEAN, TOFIX
		public static string FormatText(string f, string param1)
		{
			string o = f;
			o = o.Replace("{1}", param1);
			return o;
		}

		public static string FormatTime(Int64 unix)
		{
			if (unix == 0)
				return "-";

			string o = "";
			Int64 now = Utils.UnixTimeStamp();
			if (unix == now)
				o = GetText(LanguageItems.TimeJustNow);
			else if (unix < now)
				o = FormatSeconds(now - unix) + " " + GetText(LanguageItems.TimeAgo);
			else
				o = FormatSeconds(unix - now) + " " + GetText(LanguageItems.TimeRemain);
			return o;
		}

		public static string FormatSeconds(Int64 v)
		{
			TimeSpan ts = new TimeSpan(0, 0, (int)v);
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
			string userUnit = Engine.Instance.ProfileOptions.Get("ui.unit");
			bool iec = Engine.Instance.ProfileOptions.GetBool("ui.iec");
			return FormatBytes(bytes, speedSec, showBytes, userUnit, iec);
		}

		public static string FormatBytes(Int64 bytes, bool speedSec, bool showBytes, string userUnit, bool iec)
		{
			Int64 v = bytes;

			if (string.IsNullOrEmpty(userUnit))
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
				if (!iec)
					FormatBytesEx(v, new string[] { "bit", "kbit", "Mbit", "Gbit", "Tbit", "Pbit" }, 1000, ref number, ref unit);
				else
					FormatBytesEx(v, new string[] { "bit", "kibit", "Mibit", "Gibit", "Tibit", "Pibit" }, 1024, ref number, ref unit);
			}
			else
			{
				if (!iec)
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

		public static string FormatDateShort(DateTime dt)
		{
			return dt.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
		}

		private static string NormalizeCode(string raw)
		{
			if (string.IsNullOrEmpty(raw))
				return "inv";

			// Strip POSIX codeset (e.g. ".UTF-8") and modifier (e.g. "@euro").
			int dot = raw.IndexOf('.');
			if (dot > 0) raw = raw.Substring(0, dot);
			int at = raw.IndexOf('@');
			if (at > 0) raw = raw.Substring(0, at);

			raw = raw.Replace('_', '-');

			if (raw.Length == 0 || raw == "C" || raw == "POSIX")
				return "inv";

			return raw;
		}

	}
}
