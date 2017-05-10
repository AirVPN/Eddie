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
using System.Globalization;
using System.Text;

namespace Eddie.Core
{
    public class SystemShell
    {
        public static string EscapeAlphaNumeric(string value)
        {
            return Utils.StringPruneCharsNotIn(value, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");
        }

		public static string EscapeInt(int value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

        public static string EscapeHost(string value)
        {
            return Utils.StringPruneCharsNotIn(value, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_");
        }

        public static string EscapeUrl(string value)
        {
            // Note: Used only in already-quoted (double-quote).
            if (Uri.IsWellFormedUriString(value, UriKind.Absolute))
                return value;
            else
                return value;
        }

        public static string EscapePath(string path)
        {
            // Note: Used only in already-quoted (double-quote).
            return EscapeInsideQuote(path);
        }

        public static string EscapeInsideQuote(string value)
        {
            // Note: Used only in already-quoted (double-quote).
            
            // Note: Must be improved, but at least Windows it's a mess. 
            // For the moment, simply remove. Will be an issue only in rare cases.
            // Look for reference https://blogs.msdn.microsoft.com/twistylittlepassagesallalike/2011/04/23/everyone-quotes-command-line-arguments-the-wrong-way/
            // Advise /check these chars where it's used. Look EscapeInsideQuoteAcceptable
            value = value.Replace("'", "");
            value = value.Replace("`", "");
            value = value.Replace("\"", "");
            value = value.Replace("%", "");
            value = value.Replace("!", ""); // Delayed variable substitution
            value = value.Replace("$", "");
            return value;
        }

        public static bool EscapeInsideQuoteAcceptable(string value)
        {
            return (EscapeInsideQuote(value) == value);
        }
    }
}
