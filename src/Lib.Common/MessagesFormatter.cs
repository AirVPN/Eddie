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
    public static class MessagesFormatter
    {        
        public static string Format(string format, string param1)
		{
			return format.Replace("{1}", param1);
		}

		public static string Format(string format, string param1, string param2)
		{
			string o = format;
			o = o.Replace("{1}", param1);
			o = o.Replace("{2}", param2);
			return o;
		}

		public static string Format(string format, string param1, string param2, string param3)
		{
			string o = format;
			o = o.Replace("{1}", param1);
			o = o.Replace("{2}", param2);
			o = o.Replace("{3}", param3);
			return o;
		}

		public static string Format(string format, string param1, string param2, string param3, string param4)
		{
			string o = format;
			o = o.Replace("{1}", param1);
			o = o.Replace("{2}", param2);
			o = o.Replace("{3}", param3);
			o = o.Replace("{4}", param4);
			return o;
		}

		public static string Format(string format, string param1, string param2, string param3, string param4, string param5)
		{
			string o = format;
			o = o.Replace("{1}", param1);
			o = o.Replace("{2}", param2);
			o = o.Replace("{3}", param3);
			o = o.Replace("{4}", param4);
			o = o.Replace("{5}", param5);
			return o;
		}
    }
}
