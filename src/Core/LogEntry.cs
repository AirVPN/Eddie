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
using System.Xml;
using System.Text;
using Eddie.Lib.Common;

namespace Eddie.Core
{
    public class LogEntry
    {
        public DateTime Date = DateTime.Now;
        public LogType Type;
        public string Message;
        public int BalloonTime = 1000;
		public Exception Exception;

        public void WriteXML(XmlItem item)
		{
            item.SetAttributeInt64("timestamp", Conversions.ToUnixTime(Date));
            item.SetAttribute("level", GetTypeString());
            item.SetAttribute("message", Message);			
		}
                
		public string GetMessageForList()
		{
			return Message.Replace("\r", "").Replace("\n", " | ");			
		}

		public string GetDateForList()
		{
			return Date.ToShortDateString() + " - " + Date.ToShortTimeString();
		}

        public static string GetDateForListSample()
        {
            // Used to compute an estimation of Grid cell size
            return DateTime.UtcNow.ToShortDateString() + " - " + DateTime.UtcNow.ToShortTimeString();
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
			o += Date.ToString("yyyy.MM.dd HH:mm:ss");
			o += " - ";

			foreach (string line in Message.Split('\n'))
			{
				if (line.Trim() != "")
					result += o + line.Replace("\r","").Trim() + "\n";
			}

			return result.Trim();
		}
    }

}
