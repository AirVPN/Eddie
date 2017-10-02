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
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Eddie.Core;

namespace Eddie.Core.Tools
{
    public class SSH : Tool
    {
        public override void OnNormalizeVersion()
        {
            if (Platform.Instance.IsWindowsSystem())
                Version = Version.Replace(": Release", "").Trim();
        }

        public override string GetFileName()
        {
			// TOFIX: Microsoft it's working on a ssh.exe (on GitHub, PowerShell). To be tested, to remove plink.exe dependencies.
            if (Platform.Instance.IsWindowsSystem())
            {
				return "plink.exe";
            }
            else
                return base.GetFileName();
        }

        public override string GetVersionArgument()
        {
            return "-V";
        }
    }
}
