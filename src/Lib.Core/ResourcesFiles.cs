// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2016 AirVPN (support@airvpn.org) / https://airvpn.org
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
using System.IO;
using System.Reflection;
using System.Text;

namespace Eddie.Core
{
    public static class ResourcesFiles
    {
		private static Dictionary<string, string> m_resources = new Dictionary<string, string>();

		public static int Count()
		{
			return m_resources.Count;
		}

		public static bool Exists(string name)
		{
			return m_resources.ContainsKey(name);
		}

		public static string GetString(string name)
		{
			return m_resources[name];
		}

		public static void SetString(string name, string value)
		{
			m_resources[name] = value;
		}

		public static void LoadString(Assembly assembly, string name, string resource)
		{
			if(Exists(name))
				return;

			string[] names = assembly.GetManifestResourceNames();
			foreach(string currentName in names)
			{
				if(currentName.EndsWith (resource))
				{
					Stream s = null;
					StreamReader sr = null;

					try
					{
						s = assembly.GetManifestResourceStream(currentName);
						sr = new StreamReader(s);
						SetString(name, sr.ReadToEnd());

						return;
					}
					finally
					{
						if(sr != null)
							sr.Dispose();
						else if(s != null)
							s.Dispose();
					}
				}
			}

			throw new Exception ("Resource '" + resource + "' not found.");
		}
    }
}
