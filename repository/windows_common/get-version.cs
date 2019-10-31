using System;
using System.Text;

class Program
{
	static int Main(string[] args)
	{
		try
		{
			string scriptPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			string csPath = scriptPath + "\\..\\..\\src\\Lib.Core\\Constants.cs";
			string body = System.IO.File.ReadAllText(csPath);

			string version = System.Text.RegularExpressions.Regex.Match(body, "VersionDesc = \"([0-9\\.]+)\"").Groups[1].Value;

			Console.WriteLine(version);

			return 0;
		}
		catch(Exception)
		{
			return 1;
		}
	}
}

