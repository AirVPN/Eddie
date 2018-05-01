#if !EDDIENET2
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
#endif