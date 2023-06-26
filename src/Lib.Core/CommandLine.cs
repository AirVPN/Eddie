// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2023 AirVPN (support@airvpn.org) / https://airvpn.org
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

using System.Collections.Generic;

namespace Eddie.Core
{
	public class CommandLine
	{
		public static CommandLine InitSystem(string line)
		{
			return new CommandLine(line, true, false);
		}

		public Dictionary<string, string> Params = new Dictionary<string, string>();

		// ------------
		// Costructors
		// ------------

		public CommandLine()
		{
		}

		public CommandLine(string line, bool ignoreFirst, bool firstIsAction)
		{
			Params = ParseCommandLine(line, ignoreFirst, firstIsAction);
		}

		public CommandLine(CommandLine commandLine)
		{
			foreach (KeyValuePair<string, string> item in Params)
				Params[item.Key] = item.Value;
		}

		public CommandLine(string action, string key1, string val1)
		{
			Set("action", action);
			Set(key1, val1);
		}

		public CommandLine(string action, string key1, string val1, string key2, string val2)
		{
			Set("action", action);
			Set(key1, val1);
			Set(key2, val2);
		}

		// ------------
		// Import / Export
		// ------------

		public string GetFull()
		{
			string o = "";
			foreach (KeyValuePair<string, string> item in Params)
				o += item.Key + "=\"" + item.Value + "\" ";
			return o.Trim();
		}

		public List<string> GetFullArray()
		{
			List<string> result = new List<string>();
			foreach (KeyValuePair<string, string> item in Params)
				result.Add(item.Key + "=\"" + item.Value + "\"");
			return result;
		}

		public override string ToString()
		{
			return GetFull();
		}

		// ------------
		// Management
		// ------------

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

		public string Get(string name)
		{
			return Get(name, "");
		}

		public void Set(string name, int value)
		{
			Set(name, value.ToString());
		}

		public void Set(string name, bool value)
		{
			Set(name, value ? "1" : "0");
		}

		public void SetPos(int pos, string name)
		{
			int p = 0;
			foreach (KeyValuePair<string, string> item in Params)
			{
				if (p == pos)
				{
					Params.Remove(item.Key);
					Params[name] = item.Key;
					break;
				}
				p++;
			}
		}



		// ------------
		// Misc
		// ------------

		private static Dictionary<string, string> ParseCommandLine(string l, bool ignoreFirst, bool firstIsAction)
		{
			Dictionary<string, string> result = new Dictionary<string, string>();

			char currentQuoteChar = ' ';
			int nParams = 0;
			string current = "";
			for (int pC = 0; pC < l.Length + 1; pC++)
			{
				bool add = false;

				if (pC == l.Length)
					add = true;
				else
				{
					char ch = l[pC];

					if ((ch == ' ') && (currentQuoteChar == ' '))
						add = true;
					else
					{
						bool chIsQuote = false;
						if ((ch == '\'') || (ch == '\"'))
							chIsQuote = true;

						if ((chIsQuote) && (pC > 0) && (l[pC - 1] == '\\'))
							chIsQuote = false;

						if ((chIsQuote) && (ch == currentQuoteChar))
							currentQuoteChar = ' ';
						else if ((chIsQuote) && (currentQuoteChar == ' '))
							currentQuoteChar = ch;
						else
						{
							current += ch;
						}
					}
				}

				if (add)
				{
					if (current.Trim() != "")
					{
						string k = "";
						string v = "";

						if ((firstIsAction) && (nParams == 0))
						{
							result.Add("action", current.Trim());
						}
						else if ((ignoreFirst == false) || (nParams > 0))
						{
							int posEq = current.IndexOf("=");
							if (posEq != -1)
							{
								k = current.Substring(0, posEq);
								v = current.Substring(posEq + 1);
							}
							else
							{
								k = current;
								v = "True";
							}

							k = k.Trim().TrimStart(" /-".ToCharArray());
							v = v.Trim();

							if (k != "")
								result[k] = v;
						}

						nParams++;
					}

					current = "";
				}
			}

			return result;
		}
	}
}
