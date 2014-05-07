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
using System.IO;
using System.Text;
using AirVPN.Core;

namespace Deploy
{
	class Program
	{
		static void Main(string[] args)
		{
			Platform.Init();

			Log("AirVPN deployment v1.0, System: " + Platform.Instance.GetCode() + ", Architecture: " + Platform.Instance.GetArchitecture());


			// Checking
			if (Platform.Instance is AirVPN.Core.Platforms.Linux)
			{
				if (Platform.Instance.ShellCmd("mkbundle --help").IndexOf("Usage is: mkbundle") == -1)
				{
					Console.WriteLine("Package mono-complete required.");
					return;
				}

				if (Platform.Instance.ShellCmd("dpkg --version").IndexOf("package management program") == -1)
				{
					Console.WriteLine("dpkg required.");
					return;
				}

				if (Platform.Instance.ShellCmd("tar --version").IndexOf("GNU tar") == -1)
				{
					Console.WriteLine("tar required.");
					return;
				}

				if (File.Exists("/usr/include/zlib.h") == false)
				{
					Console.WriteLine("zlib1g-dev required.");
					return;
				}
			}



			List<string> ListPlatforms = new List<string>();
			ListPlatforms.Add("windows");
			ListPlatforms.Add("windows8");
			ListPlatforms.Add("linux");
			ListPlatforms.Add("osx");

			List<string> ListArch = new List<string>();
			ListArch.Add("x86");
			ListArch.Add("x64");

			List<string> ListPackage = new List<string>();
			ListPackage.Add("portable");
			ListPackage.Add("installer");

			string pathBaseTemp = new DirectoryInfo("../../../../tmp").FullName;
			string pathBaseDeploy = new DirectoryInfo("../../../../deploy").FullName;
			string pathBaseRelease = new DirectoryInfo("../../../../src/bin").FullName;
			string pathBaseRepository = new DirectoryInfo("../../../../repository").FullName;
			string pathBaseResources = new DirectoryInfo("../../../../resources").FullName;
			string pathBaseTools = new DirectoryInfo("../../../../tools").FullName;

			if(Platform.Instance is AirVPN.Core.Platforms.Linux)
				pathBaseTemp = "/tmp/airvpn_deploy";

			foreach (string platform in ListPlatforms)
			{
				if (platform == "windows8")
				{
					Log("Build the solution with .NET Framework 4.");
					Pause();
				}
				else
				{
					Log("Build the solution with .NET Framework 2.");
					Pause();
				}

				foreach (string arch in ListArch)
				{
					foreach (string package in ListPackage)
					{
						// Skip in current OS environment?												
						if ((Platform.Instance is AirVPN.Core.Platforms.Windows) && (platform.StartsWith("windows") == false))
							continue;
						if ((Platform.Instance is AirVPN.Core.Platforms.Linux) && (platform.StartsWith("linux") == false))
							continue;
						if ((Platform.Instance is AirVPN.Core.Platforms.Osx) && (platform.StartsWith("osx") == false))
							continue;

						// Other Skip?
						if ((Platform.Instance is AirVPN.Core.Platforms.Osx) && (arch == "x86")) // OSX 32 bit not supported.
							continue;

						string archiveName = "airvpn_" + platform + "_" + arch + "_" + package;
						string fileName = "airvpn_" + platform + "_" + arch;
						string pathDeploy = pathBaseDeploy + "/" + platform + "_" + arch;
						string pathTemp = pathBaseTemp + "/" + archiveName;
						string pathRelease = pathBaseRelease + "/" + arch + "/Release/";

						// Exceptions
						if (platform == "windows8") // Windows8 use the same common files of Windows
							pathDeploy = pathDeploy.Replace("windows8", "windows");

						// Start
						Log("Building '" + archiveName + "'");

						CreateDirectory(pathTemp);

						CopyAll(pathDeploy, pathTemp);

						if (platform.StartsWith("windows"))
						{							
							CopyFile(pathRelease, "AirVPN.Core.dll", pathTemp);
							CopyFile(pathRelease, "AirVPN.exe", pathTemp);
							CopyFile(pathRelease, "AirVPN_CLI.exe", pathTemp);

							if (package == "portable")
							{
								string pathFinal = NormalizePath(pathBaseRepository + "/" + fileName + ".zip");

								// ZIP
								string command = pathBaseTools + "/windows/7za.exe a -mx9 -tzip";
								command += " \"" + pathFinal + "\" \"" + pathTemp;
								Exec(command);
							}
							else if (package == "installer")
							{
								// NSIS
								string nsis = File.ReadAllText(pathBaseResources + "/nsis/AirVPN.nsi");
								
								nsis = nsis.Replace("{@resources}", NormalizePath(pathBaseResources + "/nsis"));
								nsis = nsis.Replace("{@temp}", NormalizePath(pathTemp));
								nsis = nsis.Replace("{@out}", NormalizePath(pathBaseRepository + "/" + fileName + ".exe"));

								string filesAdd = "";
								string filesDelete = "";
								foreach (string filePath in Directory.GetFiles(pathTemp))
								{
									string name = new FileInfo(filePath).Name;

									filesAdd += "File \"" + name + "\"\r\n";
									filesDelete += "Delete \"$INSTDIR\\" + name + "\"\r\n";
								}

								nsis = nsis.Replace("{@files_add}", filesAdd);
								nsis = nsis.Replace("{@files_delete}", filesDelete);

								if(arch == "x64")
									nsis = nsis.Replace("$PROGRAMFILES", "$PROGRAMFILES64");

								File.WriteAllText(pathTemp + "/AirVPN.nsi", nsis);

								Exec("c:\\Program Files (x86)\\NSIS\\makensisw.exe", "\"" + NormalizePath(pathTemp + "/AirVPN.nsi") + "\"");								
							}
						}
						else if (platform == "linux")
						{
							if (package == "portable")
							{
								// mkbundle
								string command = "mkbundle ";
								command += " \"" + pathRelease + "/AirVPN.exe\"";
								command += " \"" + pathRelease + "/AirVPN.Core.dll\"";
								command += " \"" + pathRelease + "/Microsoft.Win32.TaskScheduler.dll\""; // TOOPTIMIZE: This can be avoided, but mkbundle don't support specific exclude, we need to list manually all depencencies and avoid --deps
								command += " --deps";
								command += " --static";
								command += " -z";
								command += " -o \"" + pathTemp + "/airvpn\"";
								Exec(command);

								string pathFinal = NormalizePath(pathBaseRepository + "/" + fileName + ".tar.gz");

								Exec("chmod 755 \"" + pathTemp + "/airvpn\"");
								Exec("chmod 755 \"" + pathTemp + "/openvpn\"");
								Exec("chmod 755 \"" + pathTemp + "/stunnel\"");

								CreateDirectory(pathTemp + "/" + fileName);
								MoveAll(pathTemp, pathTemp + "/" + fileName);

								// TAR.GZ
								string command2 = "cd \"" + pathTemp + "\" && tar cvfz \"" + pathFinal + "\" " + "*";
								Exec(command2);
							}
							else if (package == "installer")
							{
								CopyFile(pathRelease, "AirVPN.Core.dll", pathTemp);
								CopyFile(pathRelease, "AirVPN.exe", pathTemp);

								string pathFinal = NormalizePath(pathBaseRepository + "/" + fileName + ".deb");

								CreateDirectory(pathTemp + "/usr/lib/AirVPN");
								MoveAll(pathTemp, pathTemp + "/usr/lib/AirVPN");
								CopyDirectory(pathBaseResources + "/debian", pathTemp);

								Exec("chmod 755 \"" + pathTemp + "/DEBIAN/control\"");
								Exec("chmod 755 \"" + pathTemp + "/usr/bin/AirVPN\"");
								//Exec("chmod 755 \"" + pathTemp + "/usr/lib/AirVPN/airvpn\"");
								Exec("chmod 755 \"" + pathTemp + "/usr/lib/AirVPN/openvpn\"");
								Exec("chmod 755 \"" + pathTemp + "/usr/lib/AirVPN/stunnel\"");

								string command = "dpkg -b \"" + pathTemp + "\" \"" + pathFinal + "\"";
								Log(command);
								Exec(command);
							}
						}
						else if (platform == "osx")
						{
							if (package == "portable")
							{
								CopyFile(pathRelease, "AirVPN.Core.dll", pathTemp);
								CopyFile(pathRelease, "AirVPN_CLI.exe", pathTemp, "AirVPN.exe");

								string pathFinal = NormalizePath(pathBaseRepository + "/" + fileName + ".tar.gz");

								Exec("chmod 755 \"" + pathTemp + "/airvpn\"");
								Exec("chmod 755 \"" + pathTemp + "/openvpn\"");
								Exec("chmod 755 \"" + pathTemp + "/stunnel\"");

								CreateDirectory(pathTemp + "/" + fileName);
								MoveAll(pathTemp, pathTemp + "/" + fileName);

								// TAR.GZ
								string command2 = "cd \"" + pathTemp + "\" && tar cvfz \"" + pathFinal + "\" " + "*";
								Exec(command2);
							}
							else if (package == "installer")
							{
								// Not yet implemented.
							}
						}
						
					}
				}
			}

			Log("Done");


			if (Platform.Instance is AirVPN.Core.Platforms.Linux)
			{
				Console.WriteLine("If running from a developing VM, maybe need:");
				Console.WriteLine("cp ../../../../repository/airvpn_linux_* /media/sf_airvpn/repository/");
			}

			Pause();


		}

		static string NormalizePath(string path)
		{
			if (Platform.Instance is AirVPN.Core.Platforms.Windows)
			{
				return path.Replace("/", "\\");
			}
			else
				return path.Replace("\\", "/");
		}

		static void CreateDirectory(string path)
		{
			CreateDirectory(path, true);
		}

		static void CreateDirectory(string path, bool cleanIfExists)
		{
			if (Directory.Exists(path))
			{
				if (cleanIfExists)
				{
					Directory.Delete(NormalizePath(path), true);
					Directory.CreateDirectory(NormalizePath(path));
				}
			}
			else
				Directory.CreateDirectory(NormalizePath(path));
		}

		static void Fatal(string message)
		{
			Log("Fatal error: " + message);
			throw new Exception(message);
		}

		static void NotImplemented()
		{
			Log("Not yet implemented.");
		}

		static void Exec(string command)
		{
			Console.WriteLine("Shell: " + command);
			Platform.Instance.ShellCmd(command);
		}

		static void Exec(string filename, string arguments)
		{
			Console.WriteLine("Shell, " + filename + ", arguments: " + arguments);
			Platform.Instance.Shell(filename, arguments, true);
		}

		static void CopyFile(string fromFilePath, string toFilePath)
		{
			File.Copy(NormalizePath(fromFilePath), NormalizePath(toFilePath), false);
		}

		static void CopyFile(string fromPath, string fromFile, string toPath)
		{
			CopyFile(fromPath + "/" + fromFile, toPath + "/" + fromFile);
		}

		static void CopyFile(string fromPath, string fromFile, string toPath, string toFile)
		{
			CopyFile(fromPath + "/" + fromFile, toPath + "/" + toFile);
		}

		static void CopyAll(string from, string to)
		{
			string[] files = Directory.GetFiles(from);

			foreach (string file in files)
			{
				FileInfo fi = new FileInfo(file);

				CopyFile(fi.FullName, to + "/" + fi.Name);
			}
		}

		static void MoveAll(string from, string to)
		{
			string[] files = Directory.GetFiles(from);

			foreach (string file in files)
			{
				FileInfo fi = new FileInfo(file);

				File.Move(fi.FullName, to + "/" + fi.Name);
			}
		}

		static void CopyDirectory(string fromPath, string toPath)
		{
			//Now Create all of the directories
			foreach (string dirPath in Directory.GetDirectories(fromPath, "*", SearchOption.AllDirectories))
				Directory.CreateDirectory(dirPath.Replace(fromPath, toPath));

			//Copy all the files & Replaces any files with the same name
			foreach (string newPath in Directory.GetFiles(fromPath, "*.*", SearchOption.AllDirectories))
				File.Copy(newPath, newPath.Replace(fromPath, toPath), true);
		}

		static void Log(string message)
		{
			Console.WriteLine(message);
		}

		static void Pause()
		{
			Pause("Press any key to continue.");
		}

		static void Pause(string message)
		{
			Log(message);
			Console.ReadKey();
		}
	}
}
