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
using System.Text;

namespace AirVPN.Core
{
    public class LogEntry
    {
        public DateTime Date = DateTime.Now;
        public Engine.LogType Type;
        public string Message;
        public int BalloonTime = 1000;
		public Exception Exception;

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

		public string GetStringLines()
		{
			string result = "";

			string o = "";

			switch (Type)
			{
				case Engine.LogType.Realtime: o += "."; break;
				case Engine.LogType.Verbose: o += "."; break;
				case Engine.LogType.Info: o += "I"; break;
				case Engine.LogType.InfoImportant: o += "!"; break;
				case Engine.LogType.Warning: o += "W"; break;
				case Engine.LogType.Error: o += "E"; break;
				case Engine.LogType.Fatal: o += "F"; break;
				default: o += "?"; break;
			}
			o += " ";
			o += Date.ToString("yyyy.MM.dd HH:mm:ss");
			o += " - ";

			foreach (string line in Message.Split('\n'))
			{
				if (line.Trim() != "")
					result += o + line.Trim() + "\n";
			}

			return result.Trim();
		}
    }

}
