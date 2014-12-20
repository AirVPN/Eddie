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
	public class CommandLine
	{
		public static CommandLine SystemEnvironment;

		public static void InitSystem(string line)
		{
			SystemEnvironment = new CommandLine(line, true, false);
		}
		
		public Dictionary<string, string> Params = new Dictionary<string, string>();

		public CommandLine(string line, bool ignoreFirst, bool firstIsAction)
		{
			Params = ParseCommandLine(line, ignoreFirst, firstIsAction);
		}

		public string GetFull()
		{
			string o = "";
			foreach (KeyValuePair<string, string> item in Params)
				o += item.Key + "=\"" + item.Value + "\" ";
			return o.Trim();
		}

		public bool Exists(string name)
		{
			return Params.ContainsKey(name);
		}

		public void Set(string name, string value)
		{
			Params[name] = value;
		}

		public string Get(string name, string def)
		{
			if (Exists(name))
				return Params[name];
			else
				return def;
		}

		private static Dictionary<string, string> ParseCommandLine(string l, bool ignoreFirst, bool firstIsAction)
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

				if ((i == 1) && (firstIsAction) && (v == ""))
				{
					v = k;
					k = "action";
				}

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
