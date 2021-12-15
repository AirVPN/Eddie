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
using System.Globalization;

namespace Eddie.Core
{
	public static class LanguageManager
	{
		public static string MessageInitialization = "Initialization";
		public static CultureInfo Culture;
		public static Dictionary<string, Dictionary<string, string>> Messages = null;

		public static bool Init()
		{
			bool invFound = false;

			Culture = CultureInfo.InvariantCulture;

			Messages = new Dictionary<string, Dictionary<string, string>>();

			foreach (CultureInfo currentCulture in CultureInfo.GetCultures(CultureTypes.AllCultures))
			{
				string code = GetCodeFromCulture(currentCulture);

				// Load
				Dictionary<string, string> items = new Dictionary<string, string>();

				string jsonPath = Engine.Instance.LocateResource("lang/" + code + ".json");
				if (jsonPath != "")
				{
					Json jData = null;
					if (Json.TryParse(Platform.Instance.FileContentsReadText(jsonPath), out jData))
					{
						if (code == "inv")
							invFound = true;
						foreach (KeyValuePair<string, object> kp in jData.GetDictionary())
						{
							string itemId = kp.Key;
							string itemText = kp.Value as string;

							items[itemId] = itemText;
						}
					}
				}

				Messages[code] = items;
			}

			return invFound;
		}

		public static Json GetJsonForManifest()
		{
			Json jData = new Json();

			foreach (CultureInfo currentCulture in CultureInfo.GetCultures(CultureTypes.AllCultures))
			{
				string code = GetCodeFromCulture(currentCulture);

				string parentCulture = "";
				if (currentCulture.Parent != null)
					parentCulture = currentCulture.Parent.Name;

				Json jCulture = new Json();
				jCulture["code"].Value = currentCulture.Name;
				jCulture["parent"].Value = parentCulture;
				jCulture["display_name"].Value = currentCulture.DisplayName;
				jCulture["english_name"].Value = currentCulture.EnglishName;
				jCulture["neutral"].Value = currentCulture.IsNeutralCulture;
				if (Messages[code].Count != 0)
				{
					jCulture["items"].Value = new Json();
					foreach (KeyValuePair<string, string> itemPair in Messages[code])
					{
						jCulture["items"][itemPair.Key].Value = itemPair.Value;
					}
				}
				jData[code].Value = jCulture;
			}

			return jData;
		}

		public static string GetCodeFromCulture(CultureInfo culture)
		{
			if (culture.Name == "")
				return "inv";
			else
				return culture.Name;
		}

		public static void SetIso(string iso)
		{
			try
			{
				if (iso == "auto")
					Culture = CultureInfo.InstalledUICulture;
				else
					Culture = new CultureInfo(iso);
			}
			catch (Exception ex)
			{
				Engine.Instance.Logs.Log(ex);
				Culture = CultureInfo.InvariantCulture;
			}
		}

		public static string GetText(string id)
		{
			string tempString = GetTextTemp(id);
			if (tempString != "")
				return tempString;

			CultureInfo currentCulture = Culture;
			for (; ; )
			{
				string code = GetCodeFromCulture(currentCulture);

				if ((Messages.ContainsKey(code)) && (Messages[code].ContainsKey(id)))
					return Messages[code][id];

				if (code == "inv")
					return "{lang:" + id + "}"; // Not Found

				currentCulture = currentCulture.Parent;
			}
		}

		public static string GetText(string id, string param1)
		{
			string format = GetText(id);
			return format.Replace("{1}", param1);
		}

		public static string GetText(string id, string param1, string param2)
		{
			string format = GetText(id);
			string o = format;
			o = o.Replace("{1}", param1);
			o = o.Replace("{2}", param2);
			return o;
		}

		public static string GetText(string id, string param1, string param2, string param3)
		{
			string format = GetText(id);
			string o = format;
			o = o.Replace("{1}", param1);
			o = o.Replace("{2}", param2);
			o = o.Replace("{3}", param3);
			return o;
		}

		public static string GetText(string id, string param1, string param2, string param3, string param4)
		{
			string format = GetText(id);
			string o = format;
			o = o.Replace("{1}", param1);
			o = o.Replace("{2}", param2);
			o = o.Replace("{3}", param3);
			o = o.Replace("{4}", param4);
			return o;
		}

		public static string GetText(string id, string param1, string param2, string param3, string param4, string param5)
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
				o = GetText("TimeJustNow");
			else if (unix < now)
				o = FormatSeconds(now - unix) + " " + GetText("TimeAgo");
			else
				o = FormatSeconds(unix - now) + " " + GetText("TimeRemain");
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
			string userUnit = Engine.Instance.Options.Get("ui.unit");
			bool iec = Engine.Instance.Options.GetBool("ui.iec");
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
				if (iec == false)
					FormatBytesEx(v, new string[] { "bit", "kbit", "Mbit", "Gbit", "Tbit", "Pbit" }, 1000, ref number, ref unit);
				else
					FormatBytesEx(v, new string[] { "bit", "kibit", "Mibit", "Gibit", "Tibit", "Pibit" }, 1024, ref number, ref unit);
			}
			else
			{
				if (iec == false)
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
			return dt.ToShortDateString() + " - " + dt.ToShortTimeString();
		}

		public static string GetTextTemp(string id)
		{
			// Hardcoded string that need to be moved in language system			            

			return "";
		}
	}
}
