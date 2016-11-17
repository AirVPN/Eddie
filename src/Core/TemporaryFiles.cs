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
using System.Net;
using System.Net.NetworkInformation;
using System.Xml;
using System.Text;

namespace Eddie.Core
{
	public static class TemporaryFiles
	{
		private static List<TemporaryFile> m_files = new List<TemporaryFile>();

		public static void Add(TemporaryFile file)
		{
            m_files.Add(file);
		}

		public static void Remove(TemporaryFile file)
		{
			if(Destroy(file.Path))
				m_files.Remove(file);
		}

		private static bool Destroy(string path)
		{			
			try
			{
				Platform.Instance.FileDelete(path);
			}
			catch (Exception)
			{
				return false;
			}

			return true;
		}

        public static void Clean()
        {
            Clean("");
        }

        private static void Clean(string group)
		{            
            List<TemporaryFile> filesToRemove = new List<TemporaryFile>();
			foreach (TemporaryFile file in m_files)
			{
                if (file.Group == group)
                    filesToRemove.Add(file);                    
                else if(group == "")
                    filesToRemove.Add(file);                
            }

            foreach(TemporaryFile file in filesToRemove)
            {
                m_files.Remove(file);
                Remove(file);
            }

            if (group == "")
            {
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
}
