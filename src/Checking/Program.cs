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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Checking
{
	class Program
	{
		static void Main(string[] args)
		{
			string repoDir = new DirectoryInfo(new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).Directory + "\\..\\..\\..\\..\\..\\").FullName;

			string srcDir = repoDir + "/src/";
			List<FileInfo> srcFiles = new List<FileInfo>();
			FilesList(srcDir, srcFiles);

			foreach (FileInfo f in srcFiles)
			{
				if (f.Name == "AssemblyInfo.cs")
				{
					string original = File.ReadAllText(f.FullName);
					string content = original;

					Console.WriteLine(content);

					content = content.Replace("AssemblyCopyright(\"Copyright ©  2011-2018\")", "AssemblyCopyright(\"Copyright © 2011 - 2021\")");
					content = content.Replace("AssemblyCopyright(\"Copyright ©  2011 - 2018\")", "AssemblyCopyright(\"Copyright © 2011 - 2021\")");
					content = content.Replace("AssemblyCopyright(\"Copyright ©  2011 - 2019\")", "AssemblyCopyright(\"Copyright © 2011 - 2021\")");
					content = content.Replace("[assembly: AssemblyVersion(\"2.17.0.0\")]", "[assembly: AssemblyVersion(\"2.21.0.0\")]");
					content = content.Replace("[assembly: AssemblyFileVersion(\"2.17.0.0\")]", "[assembly: AssemblyFileVersion(\"2.21.0.0\")]");
					content = content.Replace("[assembly: AssemblyVersion(\"2.19.0.0\")]", "[assembly: AssemblyVersion(\"2.21.0.0\")]");
					content = content.Replace("[assembly: AssemblyFileVersion(\"2.19.0.0\")]", "[assembly: AssemblyFileVersion(\"2.21.0.0\")]");

					if (original != content)
					{
						Console.WriteLine("Updated " + f.FullName);

						File.WriteAllText(f.FullName, content);
					}
					else
					{
						Console.WriteLine("Untouch " + f.FullName);
					}
				}
			}
		}

		static void FilesList(string dir, List<FileInfo> result)
		{
			foreach (string d in Directory.GetDirectories(dir))
			{
				foreach (string f in Directory.GetFiles(d))
				{
					result.Add(new FileInfo(f));
				}
				FilesList(d, result);
			}
		}
	}
}
