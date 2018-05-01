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

using Eddie.Common.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Eddie.Common.Log
{
    public sealed class LogsManager
    {
		private const int ENTRIES_LIMIT = 250;

		private object m_sync = new object();

		private List<LogEntry> m_entries = new List<LogEntry>();
		private long m_entriesID = 0;

		public delegate void LogEventHandler(LogEntry e);

		private event LogEventHandler LogEvent;

		public DataLocker<List<LogEntry>> Entries
		{
			get
			{
				return new DataLocker<List<LogEntry>>(m_entries, m_sync);
			}
		}

		private static LogsManager g_instance = new LogsManager();

		public static LogsManager Instance
		{
			get
			{
				return g_instance;
			}
		}

		private LogsManager()
		{

		}

		public void AddHandler(LogEventHandler handler)
		{
			lock(m_sync)
			{
				LogEvent += handler;
			}
		}

		public void RemoveHandler(LogEventHandler handler)
		{
			lock(m_sync)
			{
				LogEvent -= handler;
			}
		}
	
		public void Log(LogLevel level, string message, params object[] args)
        {
			string str = args.Length > 0 ? string.Format(message, args) : message;

#if DEBUG
			Trace.WriteLine(str);
#endif

			/*
			switch(level)
            {
            case LogLevel.debug:    logger.Debug(str);
                                    break;

            case LogLevel.info:     logger.Info(str);
                                    break;

            case LogLevel.warning:  logger.Warn(str);
                                    break;

            case LogLevel.error:    logger.Error(str);
                                    break;

            case LogLevel.fatal:    logger.Fatal(str);
                                    break;
            }
			*/

			using(DataLocker<List<LogEntry>> entries = Entries)
			{
				LogEntry entry = new LogEntry(m_entriesID++, str, level);

				entries.Data.Add(entry);
				if(entries.Data.Count > ENTRIES_LIMIT)
					entries.Data.RemoveAt(0);

				// LogEvent is fired inside the lock to sync the new entry and the prop "Entries"
				if(LogEvent != null)
					LogEvent(entry);
			}
		}

		public void Debug(string message, params object[] args)
        {
            Log(LogLevel.debug, message, args);            
        }
  
        public void Info(string message, params object[] args)
        {
            Log(LogLevel.info, message, args);            
        }

        public void Warning(string message, params object[] args)
        {
            Log(LogLevel.warning, message, args);                        
        }

        public void Error(string message, params object[] args)
        {
            Log(LogLevel.error, message, args);                        
        }

        public void Error(String prefix, Exception e)
        {
            Error(prefix, Utils.GetExceptionDetails(e));
        }

        public void Error(Exception e)
        {
            Error("Exception: {0}", e);
        }

        public void Fatal(string message, params object[] args)
        {
            Log(LogLevel.fatal, message, args);                        
        }

        public void Fatal(Exception e)
        {
            Fatal("Fatal exception: {0}", e);
        }

        public void Fatal(String prefix, Exception e)
        {
			Fatal(prefix, Utils.GetExceptionDetails(e));
        }  
    }
}
