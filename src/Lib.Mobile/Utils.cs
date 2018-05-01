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

using Java.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Xamarin.Forms;

namespace Eddie
{
    public class Utils
    {
		public const char DEFAULT_SPLIT_SEPARATOR = ',';

		public static void Sleep(int milliseconds)
		{
			System.Threading.Thread.Sleep(milliseconds);
		}

		public static bool BoolCast(int value)
		{
			return value != 0 ? true : false;
		}

		public static bool Empty(string value)
		{
			return string.IsNullOrEmpty(value);
		}

		public static void SafeDispose(IDisposable disposable)
		{
			try
			{
				if(disposable != null)
					disposable.Dispose();
			}
			catch
			{

			}
		}

		public static void SafeClose(ICloseable closable)
		{
			try
			{
				if(closable != null)
					closable.Close();
			}
			catch
			{

			}
		}

		public static String GetExceptionDetails(Exception e)
		{
			String details = e.Message;

			StackTrace stack = new StackTrace(e);
			if(stack.FrameCount > 0)
			{
				StackFrame frame = stack.GetFrame(stack.FrameCount - 1);
				if(frame != null)
				{
					MethodBase method = frame.GetMethod();
					if(method != null)
					{
						string methodName = method.Name;
						if(Utils.Empty(methodName) == false)
						{
							string className = method.ReflectedType != null ? method.ReflectedType.FullName : null;
							if(Utils.Empty(className) == false)
								methodName = className + "." + methodName;

							if(Utils.Empty(details) == false)
								details += " ";

							string fileName = frame.GetFileName();
							int fileLine = frame.GetFileLineNumber();
							int fileColumn = frame.GetFileColumnNumber();

							string fileInfo = "";
							if((Utils.Empty(fileName) == false) || (fileLine != 0) || (fileColumn != 0))
								fileInfo = " at file:line:column " + String.Format("{0}:{1}:{2}", Utils.Empty(fileName) ? "?" : fileName, fileLine != 0 ? Convert.ToString(fileLine) : "?", fileColumn != 0 ? Convert.ToString(fileColumn) : "?");

							details += "(" + methodName + fileInfo + ")";
						}
					}
				}
			}

			return details;
		}

		public static void OpenURL(string url)
		{
			Device.OpenUri(new Uri(url));
		}

		public static string GetAssetURI(string filename)
		{
			// TODO: fixme
			return "file:///android_asset/" + filename;
		}

		public static List<string> SpliValues(string option, char separator = DEFAULT_SPLIT_SEPARATOR)
		{
			// Do not use this form: return new List<string>(option.Split(separator)); (because of empty values)

			List<string> list = new List<string>();

			string[] array = option.Trim().Split(separator);
			foreach(string v in array)
			{
				string current = v.Trim();
				if(Empty(current) == false)
					list.Add(current);
			}

			return list;
		}
	}
}
