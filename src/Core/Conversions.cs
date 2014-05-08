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
using System.Text;

namespace AirVPN.Core
{
    public static class Conversions
    {
        public static Int32 ToInt32(object v, int def)
        {
            try
            {
                if( (v is String) && (v.ToString() == "") )
                    return def;
                return Convert.ToInt32(v);
            }
            catch (Exception ex)
            {
                Debug.Trace(ex);
                return def;
            }
        }

		public static Int32 ToInt32(string v)
		{
			Int32 vo;
			if (Int32.TryParse(v, out vo))
				return vo;
			else
				return 0;
		}

		public static Int32 ToInt32(float v)
		{
			return Convert.ToInt32(v);
		}

		public static Int32 ToInt32(double v)
		{
			return Convert.ToInt32(v);
		}

		public static Int64 ToInt64(string v)
		{
			Int64 vo;
			if (Int64.TryParse(v, out vo))
				return vo;
			else
				return 0;
		}

		public static Int64 ToInt64(float v)
		{
			return Convert.ToInt64(v);
		}

		public static Int64 ToInt64(double v)
		{
			return Convert.ToInt64(v);
		}

		public static bool ToBool(string v)
		{
			if (v == "1")
				return true;
			if (v.ToLowerInvariant() == "true")
				return true;
			if (v.ToLowerInvariant() == "yes")
				return true;
			return false;
		}

		public static string ToString(object o)
		{
			if (o == null)
				return "null";
			else if (o is bool)
			{
				bool b = (bool) o;
				return (b ? "true" : "false");
			}
			else if (o is string[])
				return string.Join(",", o as string[]);
			else
				return o.ToString();
		}

		public static DateTime ToDateTime(Int64 unixTimeStamp)
		{
			// Unix timestamp is seconds past epoch
			System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
			dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
			return dtDateTime;
		}

		public static int ToUnixTime(DateTime dt)
		{
			return Convert.ToInt32((dt - new DateTime(1970, 1, 1)).TotalSeconds);
		}

    }
}
