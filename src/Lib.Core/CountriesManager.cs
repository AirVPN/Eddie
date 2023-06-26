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
	public class CountriesManager
	{
		public static Dictionary<string, string> Code2Name = new Dictionary<string, string>();
		public static Dictionary<string, string> Name2Code = new Dictionary<string, string>();

		public static void Init()
		{
			string iso3166data = Engine.Instance.LocateResource("iso-3166.json");
			if (iso3166data != "")
			{
				Json jData = null;
				if (Json.TryParse(Platform.Instance.FileContentsReadText(iso3166data), out jData))
				{
					foreach (Json jCountry in jData.GetArray())
					{
						string code = jCountry["alpha-2"].Value as string;
						string name = jCountry["name"].Value as string;

						Add(code, name);
					}
				}
			}
		}

		public static bool IsCountryCode(string code)
		{
			return Code2Name.ContainsKey(code);
		}

		public static string GetNameFromCode(string code)
		{
			if (Code2Name.ContainsKey(code))
				return Code2Name[code];
			else
				return "";
		}

		public static void Add(string code, string name)
		{
			Code2Name[code.ToLowerInvariant()] = name;
			Name2Code[name] = code.ToLowerInvariant();
		}


	}
}
