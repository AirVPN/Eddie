// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2018 AirVPN (support@airvpn.org) / https://airvpn.org
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
using System.Linq;
using System.Text;

namespace Eddie.Common.Log
{
	public class LogEntry
	{
		private long m_id;
		private string m_message;
		private LogLevel m_level;
		private DateTime m_timestamp;

		public LogEntry(long id, string message, LogLevel level, DateTime? timestamp = null)
		{
			m_id = id;
			m_message = message;
			m_level = level;
			m_timestamp = timestamp != null ? timestamp.Value : DateTime.UtcNow;
		}

		public long ID
		{
			get
			{
				return m_id;
			}
		}

		public string Message
		{
			get
			{
				return m_message;
			}
		}

		public LogLevel Level
		{
			get
			{
				return m_level;
			}
		}

		public DateTime Timestamp
		{
			get
			{
				return m_timestamp;
			}
		}
	}
}
