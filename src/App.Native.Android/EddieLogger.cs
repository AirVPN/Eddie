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
//
// 14 August 2018 - author: ProMIND - initial release.

using System;
using System.Collections.Generic;
using Java.Util.Logging;
using Android.Content;
using System.IO;

namespace Eddie.NativeAndroidApp
{
    public struct LogItem
    {
        public long utcUnixTimestamp;
        public Level logLevel;
        public string message;
    }

    public class EddieLogger
    {
        private static string EddieLogName = "EddieLogger";
        private static string EddieLogFileName = "EddieLogger.log";
        private static Logger logger = Logger.GetLogger(EddieLogName);
        private static Java.IO.File logFile = null;
        private static FileHandler logFileHandler = null;
        private static MemoryHandler logMemoryHandler = null;
        private static int logMaxRecords = 250;
        private static string lineFeedMarker = "**n*n**";

        class EddieLogFormatter : Formatter
        {
            override public string Format(LogRecord record)
            {
                return string.Format("{0}|{1}|{2}\n", DateTimeOffset.UtcNow.ToUnixTimeSeconds(), record.Level, record.Message);
            }
        
            override public String GetHead(Handler h)
            {
                return base.GetHead(h);
            }
        
            override public String GetTail(Handler h)
            {
                return base.GetTail(h);
            }
        }

        public static void Init(Context context)
        {
            if(logFile != null && logFileHandler != null && logMemoryHandler != null)
                return;

            try
            {
                logFile = new Java.IO.File(context.FilesDir, EddieLogFileName);
            }
            catch
            {
                logFile = null;
            }

            if(logFile != null)
            {
                try
                {
                    if(logFile.Exists())
                    {
                        // Remove old log file
    
                        logFile.Delete();
                    }
                }
                catch
                {
                }

                try
                {
                    logFileHandler = new FileHandler(logFile.Path);
                }
                catch
                {
                    logFileHandler = null;
                }

                if(logFileHandler != null)
                {
                    try
                    {
                        logMemoryHandler = new MemoryHandler(logFileHandler, logMaxRecords, Level.Off);
                    }
                    catch
                    {
                        logMemoryHandler = null;
                    }
    
    
                    if(logMemoryHandler != null)
                    {
                        try
                        {
                            logger.AddHandler(logMemoryHandler);
                            
                            logFileHandler.Level = Level.All;
                            logFileHandler.Formatter = new EddieLogFormatter();
                            logger.Level = Level.All;
                        }
                        catch
                        {
                            logFileHandler = null;
                            logMemoryHandler = null;
                            logger = null;
                        }
                    }
                }
            }
        }
        
        private static void Log(Level level, string message, params object[] args)
        {
            if(logger == null || logFileHandler == null || logMemoryHandler == null)
                return;

            string logMessage;
            
            if(args.Length > 0)
                logMessage = string.Format(message, args);
            else
                logMessage = message;

            logger.Log(level, logMessage.Replace("\n", lineFeedMarker));
        }

        public static void Debug(string message, params object[] args)
        {
            Log(Level.Fine, message, args);            
        }
  
        public static void Info(string message, params object[] args)
        {
            Log(Level.Info, message, args);            
        }

        public static void Warning(string message, params object[] args)
        {
            Log(Level.Warning, message, args);                        
        }

        public static void Error(string message, params object[] args)
        {
            Log(Level.Severe, message, args);                        
        }

        public static void Error(String prefix, Exception e)
        {
            Log(Level.Severe, prefix, SupportTools.GetExceptionDetails(e));
        }

        public static void Error(Exception e)
        {
            Log(Level.Severe, "Exception: {0}", e);
        }

        public static void Fatal(string message, params object[] args)
        {
            Log(Level.Severe, message, args);                        
        }

        public static void Fatal(Exception e)
        {
            Log(Level.Severe, "Fatal exception: {0}", e);
        }

        public static void Fatal(String prefix, Exception e)
        {
            Log(Level.Severe, prefix, SupportTools.GetExceptionDetails(e));
        }  

        public static List<LogItem> GetLog()
        {
            if(logger == null || logMemoryHandler == null ||logFileHandler == null || logFile == null)
                return null;

            logMemoryHandler.Push();

            string[] item = null;

            StreamReader reader = null;
            List<LogItem> log = new List<LogItem>();

            try
            {
                string line;
                LogItem logItem;

                reader = new StreamReader(logFile.Path);
    
                while((line = reader.ReadLine()) != null)
                {
                    item = line.Split('|');

                    if(item.Length == 3)
                    {
                        logItem = new LogItem();

                        try
                        {
                            Int64.TryParse(item[0], out logItem.utcUnixTimestamp);
                        }
                        catch
                        {
                            logItem.utcUnixTimestamp = 0;
                        }
                        
                        try
                        {
                            logItem.logLevel = Level.Parse(item[1]);
                        }
                        catch
                        {
                            logItem.logLevel = Level.Off;
                        }
                        
                        logItem.message = item[2].Replace(lineFeedMarker, "\n");

                        log.Add(logItem);
                    }
                }
            }
            catch
            {
                log = null;
            }
            
            if(reader != null)
                reader.Close();

            return log;
        }
    }
}
