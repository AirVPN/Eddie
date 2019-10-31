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
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Eddie.Core;

namespace Eddie.Core.Tools
{
    public class File : Tool
    {
        public string FileName;
		public bool Required;

        public File(string filename, bool required)
        {
            FileName = filename;
			Required = required;
        }

		public File(string filename)
		{
			FileName = filename;
			Required = false;
		}

		public override bool Available()
        {
            return (Path != "");
        }

        public override void OnUpdatePath()
        {
            FindResource(FileName);
        }

        public override void OnUpdateVersion()
        {
            // Do nothing - Don't call base
        }

		public override void ExceptionIfRequired()
		{
			if(Required)
			{
				if (Available() == false)
					throw new Exception(FileName + " - " + LanguageManager.GetText("NotFound"));
			}
		}
	}
}
