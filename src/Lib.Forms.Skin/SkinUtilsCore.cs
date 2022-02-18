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
using System.Diagnostics;
using System.Globalization;

namespace Eddie.Forms.Skin
{
	// These classes exists for removing Lib.Core reference, as workaround to VS bug

	public static class SkinUtilsCore
	{
		public static string LocateExecutable(string path)
		{
			return path;
		}

		public static string Exec(string path, string arguments)
		{
			if (IsUnix() == false) // Used only in Unix
				return "";

			try
			{
				Process p = new Process();
				p.StartInfo.FileName = path;
				p.StartInfo.Arguments = arguments;
				p.StartInfo.WorkingDirectory = "";
				p.StartInfo.CreateNoWindow = true;
				p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				p.StartInfo.UseShellExecute = false;
				p.StartInfo.RedirectStandardOutput = true;
				p.StartInfo.RedirectStandardError = true;
				p.Start();

				string o = p.StandardOutput.ReadToEnd().Trim();
				//string e = p.StandardError.ReadToEnd().Trim();

				p.WaitForExit();
				return o;
			}
			catch (Exception)
			{
				return "";
			}

		}

		public static bool IsUnix()
		{
			return (Environment.OSVersion.Platform.ToString() == "Unix");
		}

		public static string TrimChars(this string str, string chars)
		{
			return str.Trim(chars.ToCharArray());
		}

		public static Int32 ToInt32(string v)
		{
			Int32 vo;
			if (Int32.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out vo))
				return vo;
			else
				return 0;
		}

		public static Int64 ToInt64(object v)
		{
			try
			{
				if ((v is String) && (v.ToString() == ""))
					return 0;
				return Convert.ToInt64(v, CultureInfo.InvariantCulture);
			}
			catch
			{
				return 0;
			}
		}

		public static bool ToBool(object o)
		{
			return (ToString(o) == "true");
		}

		public static string ToString(object o)
		{
			if (o == null)
				return "";
			else if (o is bool)
			{
				bool b = (bool)o;
				return (b ? "true" : "false");
			}
			else if (o is string[])
				return string.Join(",", o as string[]);
			else if (o is float)
			{
				float f = (float)o;
				return f.ToString(CultureInfo.InvariantCulture);
			}
			else
				return o.ToString();

		}
	}

}
