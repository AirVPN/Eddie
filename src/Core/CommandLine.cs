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
	public static class CommandLine
	{
		public static Dictionary<string, string> Params = new Dictionary<string, string>();

		public static void Init(string line)
		{			
			Params = ParseCommandLine(line, true);

			string x = Lib.Core.Properties.Resources.Manifest; // pazzo
		}

		public static string Get()
		{
			string o = "";
			foreach (KeyValuePair<string, string> item in Params)
				o += item.Key + "=\"" + item.Value + "\" ";
			return o.Trim();
		}

		private static Dictionary<string, string> ParseCommandLine(string l, bool ignoreFirst)
		{
			Dictionary<string, string> result = new Dictionary<string, string>();

			string regexSpliter = @"(?<=^(?:[^""]*""[^""]*"")*[^""]*) ";

			string[] substrings = System.Text.RegularExpressions.Regex.Split(l, regexSpliter);

			int i = 0;
			foreach (string ssub in substrings)
			{
				// IgnoreFirst, typical to ignore the executable if the argument is a command-line.
				i++;
				if ((i == 1) && (ignoreFirst))
					continue;


				string k = "";
				string v = "";
				int posE = ssub.IndexOf('=');
				if (posE == -1)
				{
					k = ssub;
					v = "";
				}
				else
				{
					k = ssub.Substring(0, posE);
					v = ssub.Substring(posE + 1);
				}

				string trimCharsK = " /\\-.\"'\n\r\t";
				string trimCharsV = " -.\"'\n\r\t";
				k = k.Trim(trimCharsK.ToCharArray());
				v = v.Trim(trimCharsV.ToCharArray());

				if (v == "") // For example, "... -help ..." is equivalent of "... -help=True ..."
					v = "True";

				if (k != "")
				{
					result[k] = v;
				}
			}

			return result;


		}

	}
}
