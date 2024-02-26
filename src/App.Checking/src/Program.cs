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

// Note: run on Windows, not tested on other OS.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

string repoDir = new DirectoryInfo(System.Environment.ProcessPath + "\\..\\..\\..\\..\\..\\..\\").FullName;

string constantsCsPath = repoDir + "\\src\\Lib.Core\\Constants.cs";
string constantsBody = File.ReadAllText(constantsCsPath);
string version = Regex.Match(constantsBody, "VersionDesc = \"([0-9\\.]+)\"").Groups[1].Value;
string year = DateTime.Now.Year.ToString(System.Globalization.CultureInfo.InvariantCulture);

Console.WriteLine("Processing...");

string srcDir = repoDir + "/src/";
List<FileInfo> srcFiles = new();
FilesList(srcDir, srcFiles);

List<string> Extensions = new();
Extensions.Add(".cs");
Extensions.Add(".cpp");
Extensions.Add(".c");
Extensions.Add(".hpp");
Extensions.Add(".h");
Extensions.Add(".rc");
Extensions.Add(".plist");

foreach (FileInfo f in srcFiles)
{
	if (Extensions.Contains(f.Extension.ToLowerInvariant()))
	{
		string original = File.ReadAllText(f.FullName);
		string adapted = original;

		//Console.WriteLine("Checking " + f.FullName);

		// We know we can share an unique AssemblyInfo.cs between project, but we prefer keep distinct.

		if (f.Name == "AssemblyInfo.cs")
		{
			adapted = Regex.Replace(adapted, "\\[assembly: AssemblyVersion\\(\".+?\"\\)\\]", "[assembly: AssemblyVersion(\"" + version + ".0\")]");
			adapted = Regex.Replace(adapted, "\\[assembly: AssemblyFileVersion\\(\".+?\"\\)\\]", "[assembly: AssemblyFileVersion(\"" + version + ".0\")]");
			adapted = Regex.Replace(adapted, "\\[assembly: AssemblyCopyright\\(\".+?\"\\)\\]", "[assembly: AssemblyCopyright(\"Copyright (C) 2011 - " + year + " AirVPN\")]");
		}

		if (f.Extension == ".rc")
		{
			adapted = Regex.Replace(adapted, "FILEVERSION .+", "FILEVERSION " + version.Replace(".", ",") + ",0");
			adapted = Regex.Replace(adapted, "PRODUCTVERSION .+", "PRODUCTVERSION " + version.Replace(".", ",") + ",0");
			adapted = Regex.Replace(adapted, "VALUE \"FileVersion\", \".+?\"", "VALUE \"FileVersion\", \"" + version + ".0\"");
			adapted = Regex.Replace(adapted, "VALUE \"ProductVersion\", \".+?\"", "VALUE \"ProductVersion\", \"" + version + ".0\"");
			adapted = Regex.Replace(adapted, "VALUE \"LegalCopyright\", \".+?\"", "VALUE \"LegalCopyright\", \"Copyright (C) 2011 - " + year + " AirVPN\"");
		}

		if (f.Extension == ".cs")
		{
			adapted = Regex.Replace(adapted, "^// Copyright \\(C\\)2014-.+ AirVPN", "// Copyright (C)2014-" + year + " AirVPN", RegexOptions.Multiline);
		}

		if (f.Name == "Info.plist")
		{
			adapted = Regex.Replace(adapted, "\\<key\\>CFBundleShortVersionString\\</key\\>\\s+\\<string\\>(.*?)\\</string\\>", "<key>CFBundleShortVersionString</key>\r\n\t<string>" + version + "</string>", RegexOptions.Multiline);
		}

		if (original != adapted)
		{
			Console.WriteLine("Updated " + f.FullName);

			File.WriteAllText(f.FullName, adapted);
		}
		else
		{
			// Console.WriteLine("Untouch " + f.FullName);
		}
	}
}

Console.WriteLine("Done.");
Console.ReadLine();

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
