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

// Helper for checking sources

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eddie.Core;

namespace Checking
{
	class Program
	{
		static void Main(string[] args)
		{
			BuildCultures();
		}

		static void CheckingLanguages()
		{
			
		}

		static string EncodeCsv(string val)
		{
			return "\"" + val.Replace("\"", "\\\"") + "\"";			
		}

		static void BuildCultures()
		{
			Json json = new Json();
			Csv csv = new Csv();

			csv.WriteField("code");
			csv.WriteField("neutral");
			csv.WriteField("parent");
			csv.WriteField("name_english");
			csv.WriteField("name_native");
			csv.WriteEol();
			foreach(CultureInfo ci in CultureInfo.GetCultures(CultureTypes.AllCultures))
			{
				csv.WriteField(ci.Name);
				csv.WriteField(ci.IsNeutralCulture ? "1" : "0");
				csv.WriteField(ci.Parent.Name);
				csv.WriteField(ci.EnglishName);
				csv.WriteField(ci.NativeName);

				csv.WriteEol();
			}

			csv.Save("../../../../common/cultures.csv"); // For air_languages_cultures
		}
	}

	class Csv
	{
		int iField = 0;
		StringBuilder s = new StringBuilder();

		public void WriteField(string val)
		{
			if (iField > 0)
				s.Append(",");
			s.Append("\"" + val.Replace("\"", "\\\"") + "\"");
			iField++;
		}

		public void WriteEol()
		{
			s.Append("\n");
			iField = 0;
		}

		public void Save(string path)
		{
			File.WriteAllText(path, s.ToString(), Encoding.UTF8);
		}
	}
}
