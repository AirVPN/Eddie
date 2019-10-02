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
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Xml;
using System.Text;

namespace Eddie.Core
{
	public class TemporaryFile
	{
        public string Group;
		public string Path;

		public TemporaryFile(string group, string extension)
		{
            Group = group;
            Path = Engine.Instance.GetPathInData(RandomGenerator.GetHash() + ".tmp." + extension);
            TemporaryFiles.Add(this);
		}

        public TemporaryFile(string extension) : this("", extension)
        {

        }


        ~TemporaryFile()
		{
			Close();
		}

		public void Close()
		{
			TemporaryFiles.Remove(this);
		}
	}
}
