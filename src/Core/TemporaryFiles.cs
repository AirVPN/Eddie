// <airvpn_source_header>
// This file is part of AirVPN Client software.
// Copyright (C)2014-2014 AirVPN (support@airvpn.org) / https://airvpn.org )
//
// AirVPN Client is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// AirVPN Client is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with AirVPN Client. If not, see <http://www.gnu.org/licenses/>.
// </airvpn_source_header>

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Xml;
using System.Text;

namespace AirVPN.Core
{
	public static class TemporaryFiles
	{
		private static List<string> m_files = new List<string>();

		public static string Add(string extension)
		{
			string path = Storage.DataPath + Platform.Instance.DirSep + RandomGenerator.GetHash() + ".tmp." + extension;

			m_files.Add(path);
			return path;
		}

		public static void Remove(string path)
		{
			if(Destroy(path))
				m_files.Remove(path);
		}

		private static bool Destroy(string path)
		{			
			try
			{
				if (File.Exists(path))
					File.Delete(path);
			}
			catch (Exception)
			{
				return false;
			}

			return true;
		}

		public static void Clean()
		{
			foreach (string path in m_files)
			{
				Destroy(path);
			}

			// Cleaning old zombie temporary files
			string[] files = Directory.GetFiles(Storage.DataPath);
			foreach (string file in files)
			{
				if (file.IndexOf(".tmp.") != -1)
				{
					Destroy(file);
				}
			}
		}
	}
}
