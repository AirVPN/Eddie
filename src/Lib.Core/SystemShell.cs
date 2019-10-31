﻿// <eddie_source_header>
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
using System.Text;

namespace Eddie.Core
{
	public class SystemShell
	{
		// Request
		public string Path = "";
		public List<string> Arguments = new List<string>();
		public bool WaitEnd = true;
		public bool ExceptionIfFail = false;
		public bool NoDebugLog = false; // Used only under linux
		public string AutoWriteStdin = "";

		// Response
		public string StdOut = "";
		public string StdErr = "";
		public int ExitCode = -1;

		private static int m_id = 0;

		public string Output
		{
			get
			{
				string output = StdOut + Platform.Instance.EndOfLineSep + StdErr;
				output = output.Trim();
				return output;
			}
		}

		public static string EscapeAlphaNumeric(string value)
		{
			return value.PruneCharsNotIn("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");
		}

		public static string EscapeInt(int value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string EscapeInt(string value)
		{
			int v = Conversions.ToInt32(value);
			return v.ToString(CultureInfo.InvariantCulture);
		}

		public static string EscapeHost(string value)
		{
			// Note: RFC 952 with _ exception.
			return value.PruneCharsNotIn("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_");
		}

		public static string EscapeUrl(string value)
		{
			// Note: Used only in already-quoted (double-quote).
			if (Uri.IsWellFormedUriString(value, UriKind.Absolute))
				return value;
			else
				return value;
		}

		public static string EscapePath(string path)
		{
			// Note: Used only in already-quoted (double-quote).
			foreach (char c in Platform.Instance.CharsNotAllowedInPath)
				path = path.Replace(c, '-');
			path = path.Replace("\"", "\\\""); // In Windows " it's already removed above.
			return path;
		}

		public static string EscapeInsideQuote(string value)
		{
			// Note: Used only in already-quoted (double-quote).

			// Note: Must be improved, but at least Windows it's a mess.
			// For the moment, simply remove. Will be an issue only in rare cases.
			// Look for reference https://blogs.msdn.microsoft.com/twistylittlepassagesallalike/2011/04/23/everyone-quotes-command-line-arguments-the-wrong-way/
			// Advise /check these chars where it's used. Look EscapeInsideQuoteAcceptable
			foreach (char c in Platform.Instance.CharsNotAllowedInPath)
				value = value.Replace(c, '-');
			value = value.Replace("'", "-");
			value = value.Replace("`", "-");
			value = value.Replace("\"", "-");
			value = value.Replace("%", "-");
			value = value.Replace("!", "-"); // Delayed variable substitution
			value = value.Replace("$", "-");
			return value;
		}

		public static bool EscapeInsideQuoteAcceptable(string value)
		{
			return (EscapeInsideQuote(value) == value);
		}

		// Called for user events or special UI interaction
		public static bool ShellUserEvent(string filename, string arguments, bool waitEnd)
		{
			/* <2.17.3
			List<string> args = UtilsString.StringToList(arguments, " ", true, true, false, false);
			Shell(path, args.ToArray(), waitEnd);
			*/

			if (ShellExternalManager.CheckAllow(filename) == false)
				return false;

			return Platform.Instance.ShellExecuteCore(filename, arguments, waitEnd);
		}

		public static string Shell0(string path)
		{
			return Shell(path, new string[] { });
		}

		public static string Shell1(string path, string arg1)
		{
			return Shell(path, new string[] { arg1 });
		}

		public static string Shell2(string path, string arg1, string arg2)
		{
			return Shell(path, new string[] { arg1, arg2 });
		}

		public static string Shell3(string path, string arg1, string arg2, string arg3)
		{
			return Shell(path, new string[] { arg1, arg2, arg3 });
		}

		public static string Shell(string path, string[] arguments)
		{
			SystemShell s = new SystemShell();
			s.Path = path;
			s.Arguments.AddRange(arguments);
			s.Run();
			return s.Output;
		}
		
		public bool Run()
		{
			m_id++;

			if (Path == "")
				return false;

			if (ShellExternalManager.CheckAllow(Path) == false)
				return false;


			string path = Path;
			string[] args = Arguments.ToArray();

            if (WaitEnd)
			{
				bool log = ((NoDebugLog == false) && (Engine.Instance != null) && (Engine.Instance.Storage != null) && (Engine.Instance.Storage.GetBool("log.level.debug")));

				if (log)
				{
					string message = "Shell(" + m_id + ") of '" + path + "'";
					if (Arguments.Count > 0)
					{
						message += ", " + Arguments.Count.ToString() + " args: ";
						foreach (string arg in args)
						{
							message += "'" + arg + "';";
						}
					}
					message = message.RegExReplace("[a-zA-Z0-9+/]{30,}=", "{base64-omissis}");
					Engine.Instance.Logs.Log(LogType.Verbose, message);
				}

				int startTime = Environment.TickCount;

                Platform.Instance.ShellSyncCore(path, args, AutoWriteStdin, out StdOut, out StdErr, out ExitCode);
				
				int endTime = Environment.TickCount;

				if (log)
				{
					int deltaTime = endTime - startTime;
					string message = "Shell(" + m_id + ") done in " + deltaTime.ToString() + " ms";
					message += ", exit: " + ExitCode.ToString();
					if (StdOut != "")
						message += ", out: '" + StdOut + "'";
					if (StdErr != "")
						message += ", err: '" + StdErr + "'";
					message = message.RegExReplace("[a-zA-Z0-9+/]{30,}=", "{base64-omissis}");
					Engine.Instance.Logs.Log(LogType.Verbose, message);
				}

				if (ExceptionIfFail)
				{
					if (ExitCode != 0)
						if (StdErr != "")
							throw new Exception(StdErr);
						else
							throw new Exception(LanguageManager.GetText("Failed"));
				}

				return (ExitCode == 0);
			}
			else
			{
				Platform.Instance.ShellASyncCore(path, args);
				return true;
			}
		}
	}
}
