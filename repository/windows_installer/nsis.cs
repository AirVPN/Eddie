using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

class Program
{
	static int Main(string[] args)
	{
		try
		{
			string scriptPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			//string csPath = scriptPath + "\\..\\..\\src\\Lib.Common\\Constants.cs";
			//string body = System.IO.File.ReadAllText(csPath);

			//string version = System.Text.RegularExpressions.Regex.Match(body, "VersionDesc = \"([0-9\\.]+)\"").Groups[1].Value;

			//Console.WriteLine(version);

			Console.WriteLine(args.Length);
			for (int i = 0; i < args.Length; i++)
				Console.WriteLine("A" + i + ":" + args[i]);

			string arch = args[0];
			string pathTemp = args[1];
			string pathDeploy = args[2];
			

			string nsis = System.IO.File.ReadAllText(scriptPath + "\\nsis\\Eddie-UI.nsi");

			nsis = nsis.Replace("{@arch}", arch);
			nsis = nsis.Replace("{@resources}", scriptPath + "\\nsis");
			//nsis = nsis.Replace("{@temp}", NormalizePath(pathTemp));
			nsis = nsis.Replace("{@out}", pathDeploy);

			List<string> filesList = GetFilesRecursive(pathTemp);

			string filesAdd = "";
			string filesDelete = "";
			string filesAddLastPath = "";
			List<string> pathsToDelete = new List<string>();
			foreach (string filePath in filesList)
			{
				string name = filePath.Substring(pathTemp.Length + 1);

				FileInfo fi = new FileInfo(filePath);

				if (fi.Directory.FullName != filesAddLastPath)
				{
					filesAddLastPath = fi.Directory.FullName;
					string pathName = "$INSTDIR" + filesAddLastPath.Substring(pathTemp.Length);
					filesAdd += "SetOutPath \"" + pathName + "\"\r\n";
					if (pathName != "$INSTDIR")
						pathsToDelete.Add(pathName);
				}

				filesAdd += "File \"" + name + "\"\r\n";
				filesDelete += "Delete \"$INSTDIR\\" + name + "\"\r\n";
			}

			foreach (string pathToDelete in pathsToDelete)
			{
				filesDelete += "RMDIR \"" + pathToDelete + "\"\r\n";
			}

			nsis = nsis.Replace("{@files_add}", filesAdd);
			nsis = nsis.Replace("{@files_delete}", filesDelete);

			if (arch == "x64")
				nsis = nsis.Replace("$PROGRAMFILES", "$PROGRAMFILES64");

			File.WriteAllText(pathTemp + "/Eddie.nsi", nsis);
						
			return 0;
		}
		catch(Exception ex)
		{
			Console.WriteLine(ex.Message);
			return 1;
		}
	}

	static List<string> GetFilesRecursive(string path)
	{
		List<string> result = new List<string>();

		foreach (string filePath in Directory.GetFiles(path))
		{
			result.Add(filePath);
		}

		foreach (string dirPath in Directory.GetDirectories(path))
		{
			result.AddRange(GetFilesRecursive(dirPath));
		}

		return result;
	}
}

