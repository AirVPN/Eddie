// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2023 AirVPN (support@airvpn.org) / https://airvpn.org
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

namespace Eddie.Core
{
	public class LogEntry
	{
		public string Id = "";
		public DateTime Date = DateTime.UtcNow;
		public LogType Type;
		public string Message;
		public Exception Exception;

		public LogEntry()
		{
			Id = RandomGenerator.GetHash();
		}

		public string GetDateForList()
		{
			return LanguageManager.FormatDateShort(Date.ToLocalTime());
		}

		public static string GetDateForListSample()
		{
			// Used to compute an estimation of Grid cell size
			return LanguageManager.FormatDateShort(DateTime.UtcNow);
		}

		public string GetTypeChar()
		{
			switch (Type)
			{
				case LogType.Realtime: return ".";
				case LogType.Verbose: return ".";
				case LogType.Info: return "I";
				case LogType.InfoImportant: return "!";
				case LogType.Warning: return "W";
				case LogType.Error: return "E";
				case LogType.Fatal: return "F";
				default: return "?";
			}
		}

		public string GetTypeString()
		{
			switch (Type)
			{
				case LogType.Realtime: return "realtime";
				case LogType.Verbose: return "verbose";
				case LogType.Info: return "info";
				case LogType.InfoImportant: return "infoimportant";
				case LogType.Warning: return "warning";
				case LogType.Error: return "error";
				case LogType.Fatal: return "fatal";
				default: return "?";
			}
		}

		public string GetStringLines()
		{
			string result = "";

			string o = "";

			o += GetTypeChar();
			o += " ";
			o += Date.ToLocalTime().ToString("yyyy.MM.dd HH:mm:ss");
			o += " - ";

			string[] lines = Message.Split('\n');
			for (int l = 0; l < lines.Length; l++)
			{
				string line = lines[l];
				if (line.Trim() != "")
				{
					result += o;
					if (l != 0)
						result += "    ";
					result += line.Replace("\r", "").Trim() + "\n";
				}
			}

			return result.Trim();
		}

		public Json GetJson()
		{
			Json j = new Json();
			j["id"].Value = Id;
			j["time"].Value = Conversions.ToUnixTimeMs(Date);
			j["type"].Value = Type.ToString().ToLowerInvariant();
			j["message"].Value = Message;
			return j;
		}
	}

}
