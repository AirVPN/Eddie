#if !EDDIENET2
using Eddie.Common.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Eddie.Common.Log
{
    public sealed class LogsManager
    {
		private const int ENTRIES_LIMIT = 100;

		private List<LogEntry> m_entries = new List<LogEntry>();
		private object m_entriesSync = new object();
		private long m_entriesID = 0;

		public delegate void LogEventHandler(LogEntry e);
		public event LogEventHandler LogEvent;

		public DataLocker<List<LogEntry>> Entries
		{
			get
			{
				return new DataLocker<List<LogEntry>>(m_entries, m_entriesSync);
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
#endif