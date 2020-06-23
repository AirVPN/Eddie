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

#if !EDDIENET2
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Eddie.Core
{
    public static class Utils
    {
		public static int UnixTimeStamp()
		{
			return Conversions.ToUnixTime(DateTime.UtcNow);
		}

		public static Int64 UnixTimeStampMs()
		{
			return Conversions.ToUnixTimeMs(DateTime.UtcNow);
		}

		public static void Sleep(int milliseconds)
		{
			System.Threading.Thread.Sleep(milliseconds);
		}
				
		public static string[] GetAssemblyResources(Assembly assembly = null)
		{
			if(assembly == null)
				assembly = Assembly.GetExecutingAssembly();

			return assembly.GetManifestResourceNames();
		}

		public static string GetResourceText(string name, Assembly assembly = null)
		{
			if(assembly == null)
				assembly = Assembly.GetExecutingAssembly();

			Stream stream = null;
			StreamReader reader = null;

			try
			{
				stream = assembly.GetManifestResourceStream(name);
				reader = new StreamReader(stream);

				return reader.ReadToEnd();
			}
			finally
			{
				if(reader != null)
					reader.Dispose();       // stream.Dispose() is called here
				else if(stream != null)
					stream.Dispose();       // StreamReader raised an exception and stream.Dispose() won't be called here
			}
		}
		
		public static string HostFromUrl(string url)
		{
			try
			{
				Uri uri = new Uri(url);
				return uri.Host;
			}
			catch (Exception)
			{
				return "";
			}
		}
	}
}
#endif
