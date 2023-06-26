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

using System;
using System.Collections.Generic;
using System.IO;

string repoDir = new DirectoryInfo(new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).Directory + "\\..\\..\\..\\..\\..\\").FullName;

string srcDir = repoDir + "/src/";
List<FileInfo> srcFiles = new List<FileInfo>();
FilesList(srcDir, srcFiles);

List<string> Extensions = new List<string>();
Extensions.Add(".cs");
Extensions.Add(".cpp");
Extensions.Add(".c");
Extensions.Add(".hpp");
Extensions.Add(".h");

foreach (FileInfo f in srcFiles)
{	
	if(Extensions.Contains(f.Extension.ToLowerInvariant()))
	{
		string original = File.ReadAllText(f.FullName);
		string adapted = original;

		//Console.WriteLine("Checking " + f.FullName);

		if (f.Name == "AssemblyInfo.cs")
		{
			//Console.WriteLine(content);

			adapted = adapted.Replace("[assembly: AssemblyCopyright(\"Copyright © 2011 - 2021\")]", "[assembly: AssemblyCopyright(\"Copyright © 2011 - 2023\")]");
			adapted = adapted.Replace("[assembly: AssemblyVersion(\"2.21.0.0\")]", "[assembly: AssemblyVersion(\"2.23.0.0\")]");
			adapted = adapted.Replace("[assembly: AssemblyFileVersion(\"2.21.0.0\")]", "[assembly: AssemblyFileVersion(\"2.23.0.0\")]");
		}

		if(f.Extension == ".cs")
		{
			adapted = adapted.Replace("// Copyright (C)2014-2023 AirVPN", "// Copyright (C)2014-2023 AirVPN");
		}

		if (original != adapted)
		{
			Console.WriteLine("Updated " + f.FullName);

			File.WriteAllText(f.FullName, adapted);
		}
		else
		{
			//Console.WriteLine("Untouch " + f.FullName);
		}
	}
}

Console.WriteLine("Done.");
Console.ReadLine();

void FilesList(string dir, List<FileInfo> result)
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
