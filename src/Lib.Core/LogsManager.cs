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
    public class LogsManager
    {
		private String m_lastLogMessage;
		private int m_logDotCount = 0;
		private string m_logLast = "";
        private int m_logLastCount = 0;

		public List<LogEntry> Entries = new List<LogEntry>();

		public delegate void LogEventHandler(LogEntry e);
		public event LogEventHandler LogEvent;

		public void Log(Exception e)
		{
			Log(LogType.Error, e);
		}

		public void Log(LogType type, Exception e)
		{
			string msg = e.Message;
			if( (Engine.Instance.Storage != null) && (Engine.Instance.Storage.GetBool("log.level.debug")) )
				msg += " - Stack: " + e.StackTrace.ToString();
			Log(type, msg, e);
		}

		public void LogUnexpected(Exception e)
		{
			string msg = "Unexpected: " + e.Message;
			msg += " - Stack: " + e.StackTrace.ToString();
			Log(LogType.Verbose, msg);
		}

		public void LogWarning(string message)
		{
			Log(LogType.Warning, message, null);
		}

		public void LogVerbose(string message)
		{
			Log(LogType.Verbose, message, null);
		}

		public void LogDebug(string message)
		{
			long ts = Utils.UnixTimeStampMs ();
			LogVerbose(ts.ToString() + ":" + message);
		}

		public void LogDebug(Json json)
		{
			LogDebug(json.ToJsonPretty());
		}

		public void LogFatal(string message)
		{
			Log(LogType.Fatal, message, null);
		}

		public void LogFatal(Exception ex)
		{
            Log(LogType.Fatal, ex.Message, ex);
		}

		public void Log(LogType type, string message)
		{
			Log(type, message, null);
		}
		
		public void Log(LogType type, string message, Exception e)
		{
            // Avoid repetition
            if( (type != LogType.Fatal) && (Engine.Instance.Storage != null) && (Engine.Instance.Storage.GetBool("log.repeat") == false) )
            {
                string logRepetitionNormalized = message;
                logRepetitionNormalized = System.Text.RegularExpressions.Regex.Replace(logRepetitionNormalized, "#\\d+", "#n");
                if (logRepetitionNormalized == m_logLast)
                {
                    m_logLastCount++;
                    return;
                }
                else
                {
                    int oldCount = m_logLastCount;
                    m_logLast = logRepetitionNormalized;
                    m_logLastCount = 0;

                    if (oldCount != 0)
                    {
                        Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("LogsLineRepetitionSummary", oldCount.ToString()));
                    }                    
                }
            }

			LogEntry l = new LogEntry();
			l.Type = type;
			l.Message = message;
			l.Exception = e;

			if (l.Type > LogType.Realtime)
			{
				m_lastLogMessage = l.Message;
				m_logDotCount += 1;
				m_logDotCount = m_logDotCount % 10;
			}

			lock (Entries)
			{
				Entries.Add(l);
				if ((Engine.Instance != null) && (Engine.Instance.Storage != null) && (Entries.Count >= Engine.Instance.Storage.GetInt("log.limit")))
					Entries.RemoveAt(0);
			}

			if(LogEvent != null)
				LogEvent(l);
							
			Engine.Instance.OnLog(l);

			Json j = l.GetJson();
			j["command"].Value = "log";
			Engine.Instance.UiManager.Broadcast(j);
		}

		public string GetLogDetailTitle()
		{
			string output = "";
			if ((Engine.Instance.Storage != null) && (Engine.Instance.Storage.GetBool("advanced.expert")))
			{
				if (Engine.Instance.WaitMessage == m_lastLogMessage)
					output = "";
				else
					output = m_lastLogMessage;
			}
			else
			{
				for (int i = 0; i < m_logDotCount; i++)
					output += ".";
			}
			return output;
		}

		public string GetLogSuggestedFileName()
		{
			return "Eddie_" + DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + ".txt";
		}

		public override string ToString()
		{
			string result = "";
			lock(Entries)
			{
				foreach (LogEntry entry in Entries)
					result += entry.GetStringLines() + "\n";
			}
			return result;
		}

		public Json GetJson()
		{
			Json j = new Json();
			j.EnsureArray();
			lock(Entries)
			{
				foreach (LogEntry entry in Entries)
					j.Append(entry.GetJson());
			}			
			return j;
		}

		public List<string> ParseLogFilePath(string paths)
		{
			string logPaths = paths;
			DateTime now = DateTime.Now;
			logPaths = logPaths.Replace("%d", now.ToString("dd"));
			logPaths = logPaths.Replace("%m", now.ToString("MM"));
			logPaths = logPaths.Replace("%y", now.ToString("yyyy"));
			logPaths = logPaths.Replace("%w", now.ToString("dddd"));
			logPaths = logPaths.Replace("%H", now.ToString("HH"));
			logPaths = logPaths.Replace("%M", now.ToString("mm"));
			logPaths = logPaths.Replace("%S", now.ToString("ss"));

			List<string> results = new List<string>();

			string[] logPathsArray = logPaths.Split(';');
			foreach (string path in logPathsArray)
			{
				string logPath = path.Trim();
				if (logPath != "")
				{
					if (System.IO.Path.IsPathRooted(path) == false)
					{
						logPath = Engine.Instance.GetDataPath() + "/" + logPath;
					}
					logPath = Platform.Instance.NormalizePath(logPath).Trim();
					if (logPath != "")
						results.Add(logPath);
				}
			}

			return results;
		}

		public string GetParseLogFilePaths(string paths)
		{
			string output = "";
			List<string> results = ParseLogFilePath(paths);
			foreach (string path in results)
			{
				output += path + "\r\n";
			}
			return output.Trim();
		}
    }

}
