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

namespace Eddie.Core
{
    public class StatsEntry
    {
		public string Key = "";
		public string Caption = "";
		public string Icon = "";
		public string Value = "";
        public string Clickable = "";
		public bool Listed = false; // Not really used

        public void WriteXML(XmlItem item)
        {
            item.SetAttribute("key", Key);
            item.SetAttribute("value", Value);
            item.SetAttribute("text", Text);
        }

        public string Text
        {
            get
            {
                string t = Value;
                if(Clickable != "")
                {
                    if (t != "")
                        t += " ";
                    t += Messages.Format(Messages.DoubleClickToAction, Clickable);
                }
                return t;
            }
        }
    }
}
