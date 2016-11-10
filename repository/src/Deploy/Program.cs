// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2016 AirVPN (support@airvpn.org) / https://airvpn.org
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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Security.Cryptography.X509Certificates;

namespace Deploy
{
	public class Package
	{
		public string Platform;
		public string Architecture;        
        public string UI; // TOFIX: Not yet used
        public bool Bundles; // TOFIX: Not yet used
        public int NetFramework = 0;
		public string Format;

        public string ArchitectureCompile; 

        public Package(string platform, string architecture, string ui, bool bundles, int netFramework, string format)
		{
			Platform = platform;
			Architecture = architecture;
            UI = ui;
            Bundles = bundles;
            NetFramework = netFramework;
            Format = format;
            
            ArchitectureCompile = Architecture;
            if (ArchitectureCompile == "armv7l") // Arm pick x64 executabled (that are anyway CIL).                
                ArchitectureCompile = "x64";            
        }
    }

	class Program
	{
        private static string PathBase = "";
        private static string PathBaseTemp = "";
        private static string PathBaseDeploy = "";
        private static string PathBaseRelease = "";
        private static string PathBaseRepository = "";
        private static string PathBaseResources = "";
        private static string PathBaseTools = "";
        private static string PathBaseSigning = "";

        private static string SO = "";
        private static List<string> Arguments;

		static void Main(string[] args)
		{            
            Log("Eddie deployment v1.4");
            Log("Arguments allowed: 'verbose' (show more logs), 'official' (signing files)");
			
            Arguments = new List<string>();
            foreach(string arg in args)
            {
                string a = arg.ToLowerInvariant().Trim();
                Log("Argument detected: " + a);
                Arguments.Add(a);
            }

            if(IsVerbose())
            {
                Log("PlatformOS: " + Environment.OSVersion.Platform.ToString());
                Log("VersionString: " + Environment.OSVersion.VersionString.ToString());
            }
            
			/* -------------------------------
			   Detect Platform
			------------------------------- */

			if (Environment.OSVersion.VersionString.IndexOf ("Windows") != -1)
				SO = "windows";
			else if ((Environment.OSVersion.Platform.ToString () == "Unix") && (Shell("uname") == "Darwin") ) {
				SO = "osx";
			}
			else if (Environment.OSVersion.Platform.ToString () == "Unix") {
				SO = "linux";
			}
			else 
			{
				Console.WriteLine("Unknown platform.");
				return;
			}
			Log("Platform: " + SO);

            PathBase = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).Directory.Parent.Parent.Parent.Parent.Parent.FullName;

            Log("Path base: " + PathBase);

            PathBaseTemp = new DirectoryInfo(PathBase + "/tmp").FullName;
            PathBaseDeploy = new DirectoryInfo(PathBase + "/deploy").FullName;
            PathBaseRelease = new DirectoryInfo(PathBase + "/src/bin").FullName;
            PathBaseRepository = new DirectoryInfo(PathBase + "/repository/files").FullName;
            PathBaseResources = new DirectoryInfo(PathBase + "/resources").FullName;
            PathBaseTools = new DirectoryInfo(PathBase + "/tools").FullName;
            PathBaseSigning = new DirectoryInfo(PathBase + "/repository/signing").FullName;

            /* --------------------------------------------------------------
			   Checking environment, required
			-------------------------------------------------------------- */

            if (SO == "linux")
			{
				if (Shell("tar --version").IndexOf("GNU tar") == -1)
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

            /* --------------------------------------------------------------
			   Checking environment, optional
			-------------------------------------------------------------- */

            bool AvailableMkBundle = false;
            bool AvailableDpkg = false;
            bool AvailableRPM = false;

            if (SO == "linux")
            {
                if (Shell("mkbundle --help").IndexOf("Usage is: mkbundle") == -1)
                {
                    Console.WriteLine("'mkbundle' not found. Package mono-complete required. Portable edition will be unavailable.");
                }
                else
                    AvailableMkBundle = true;

                if (Shell("dpkg --version").IndexOf("package management program") == -1)
                {
                    Console.WriteLine("'dpkg' required. Debian installer will be unavailable.");
                    return;
                }
                else
                    AvailableDpkg = true;

                if (Shell("rpmbuild --version").IndexOf("RPM version") == -1)
                {
                    Console.WriteLine("'rpmbuild' required. RPM installer will be unavailable.");
                    return;
                }
                else
                    AvailableRPM = true;
            }

            /* -------------------------------
                Pause, to read logs.
            ------------------------------- */

            Pause();

            /* -------------------------------
                Generate Man pages
            ------------------------------- */

            if (SO == "windows")
            {
                Log("Generating manual files");
                string pathExe = new FileInfo(PathBase + "/src/bin/x64/Release/CLI.Windows.exe").FullName;
                WriteTextFile(PathBaseRepository + "/manual.html", Shell(pathExe + " -help -help_format=html"));
                WriteTextFile(PathBaseRepository + "/manual.bb", Shell(pathExe + " -help -help_format=bbc"));
                WriteTextFile(PathBaseRepository + "/manual.txt", Shell(pathExe + " -help -help_format=text"));
                WriteTextFile(PathBaseRepository + "/manual.man", Shell(pathExe + " -help -help_format=man"));
            }
            
            /* -------------------------------
			   Build packages list
			------------------------------- */

            List<Package> ListPackages = new List<Package>();

			if (SO == "windows")
			{
                ListPackages.Add(new Package("windows8", "x86", "ui", true, 4, "portable"));
                ListPackages.Add(new Package("windows8", "x64", "ui", true, 4, "portable"));
                ListPackages.Add(new Package("windows8", "x86", "ui", true, 4, "installer"));
                ListPackages.Add(new Package("windows8", "x64", "ui", true, 4, "installer"));

                ListPackages.Add(new Package("windows", "x86", "ui", true, 2, "portable"));
				ListPackages.Add(new Package("windows", "x64", "ui", true, 2, "portable"));
				ListPackages.Add(new Package("windows", "x86", "ui", true, 2, "installer"));
				ListPackages.Add(new Package("windows", "x64", "ui", true, 2, "installer"));

				ListPackages.Add(new Package("windows_xp", "x86", "ui", true, 2, "portable"));
				ListPackages.Add(new Package("windows_xp", "x64", "ui", true, 2, "portable"));
				ListPackages.Add(new Package("windows_xp", "x86", "ui", true, 2, "installer"));
				ListPackages.Add(new Package("windows_xp", "x64", "ui", true, 2, "installer"));                
			}

			if (SO == "linux")
			{
				string arch = Shell("uname -m");

				if (arch == "x86_64")
					arch = "x64";
				else if(arch == "armv7l")
					arch = "armv7l";
                else
                    arch = "x86";
				ListPackages.Add(new Package("linux", arch, "ui", true, 4, "mono"));
				ListPackages.Add(new Package("linux", arch, "ui", true, 4, "portable"));
                ListPackages.Add(new Package("linux", arch, "ui", false, 4, "debian"));
                ListPackages.Add(new Package("linux", arch, "ui", false, 4, "rpm"));                
            }

			if (SO == "osx") {
				ListPackages.Add(new Package("osx", "x64", "ui", true, 4, "portable"));
				ListPackages.Add(new Package("osx", "x64", "ui", true, 4, "installer"));
			}
			
			string versionString = File.ReadAllText(PathBase + "/version.txt").Trim();

			if(SO == "linux")
				PathBaseTemp = "/tmp/eddie_deploy";

            /* // TOCLEAN
			int netFramework = 0;
			if (SO == "windows") {
				Log ("What .net framework is currently complied? '2' or '4'.");
				ConsoleKeyInfo keyFramework = Console.ReadKey ();
	            
				if (keyFramework.KeyChar == '2')
					netFramework = 2;
				else if (keyFramework.KeyChar == '4')
					netFramework = 4;
			} else if (SO == "linux")
				netFramework = 4;
			else if (SO == "osx")
				netFramework = 2;
            */

            foreach (Package package in ListPackages)
			{
				string platform = package.Platform;
				string arch = package.Architecture;
                string archCompile = package.ArchitectureCompile;
                string ui = package.UI;
                int requiredNetFramework = package.NetFramework;
				string format = package.Format;

                /* // TOCLEAN
				int requiredNetFramework = 4;
				if(platform == "windows")
					requiredNetFramework = 2;
				if (platform == "windows_xp")
					requiredNetFramework = 2;
				else if (platform == "osx")
					requiredNetFramework = 2;
                */

                //string archiveName = "airvpn_" + platform + "_" + arch + "_" + format;
                //string fileName = "airvpn_" + platform + "_" + arch + "_" + format;
                string archiveName = "eddie-" + ui + "_" + versionString + "_" + platform + "_" + arch + "_" + format;
                string fileName = archiveName;
                string pathDeploy = PathBaseDeploy + "/" + platform + "_" + arch;
				string pathTemp = PathBaseTemp + "/" + archiveName;
				string pathRelease = PathBaseRelease + "/" + archCompile + "/Release/";

				// Exceptions
				if (platform == "windows8") // Windows8 use the same common files of Windows
					pathDeploy = pathDeploy.Replace("windows8", "windows");

                /* // TOCLEAN
                if (requiredNetFramework != netFramework)
                {
                    Log("Building '" + archiveName + "' skipped for mismatch .Net framework");
                    continue;
                }
                */

                // Start
                Log("------------------------------");
				Log("Building '" + archiveName + "'");

                bool skipCompile = false;
                if (SO == "osx")
                    skipCompile = true;

                if (skipCompile)
                {
                    Log("Expected already compiled binaries for this platform.");
                }
                else
                {
                    if (Compile(archCompile, requiredNetFramework) == false)
                    {
                        continue;
                    }
                }

                Log("Packaging files");

				CreateDirectory(pathTemp);

				CreateDirectory (PathBaseRepository, false);

				CopyAll(pathDeploy, pathTemp);

				if (platform.StartsWith("windows"))
				{
                    CopyFile(pathRelease, "Lib.Core.dll", pathTemp);					
					CopyFile(pathRelease, "Platforms.Windows.dll", pathTemp);
					
                    if(format == "ui")
                    {
                        CopyFile(pathRelease, "Lib.Forms.dll", pathTemp);
                        CopyFile(pathRelease, "UI.Forms.Windows.exe", pathTemp, "AirVPN.exe"); // TODO Eddie3: "Eddie-UI.exe"
                        CopyFile(pathRelease, "CLI.Windows.exe", pathTemp, "CLI.exe"); // TODO Eddie3: "Eddie-CLI.exe"
                    }
                    else if(format == "cli")
                    {
                        CopyFile(pathRelease, "CLI.Windows.exe", pathTemp, "Eddie-CLI.exe");
                    }                    

                    WindowsSignPath(pathTemp);                    
                    
					if (format == "portable")
					{
						string pathFinal = NormalizePath(PathBaseRepository + "/" + fileName + ".zip");

						if (File.Exists(pathFinal))
							File.Delete(pathFinal);

						// ZIP
						string command = PathBaseTools + "/windows/7za.exe a -mx9 -tzip";
						command += " \"" + pathFinal + "\" \"" + pathTemp;
						Shell(command);
					}
					else if (format == "installer")
					{
                        string nsis = "";

                        if(format == "ui")
                        {
                            nsis = File.ReadAllText(PathBaseResources + "/nsis/Eddie-UI.nsi");
                        }
                        else if(format == "cli")
                        {

                        }

                        if (nsis != "")
                        {
                            string pathExe = NormalizePath(PathBaseRepository + "/" + fileName + ".exe");

                            nsis = nsis.Replace("{@resources}", NormalizePath(PathBaseResources + "/nsis"));
                            nsis = nsis.Replace("{@temp}", NormalizePath(pathTemp));
                            nsis = nsis.Replace("{@out}", pathExe);

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

                            if (arch == "x64")
                                nsis = nsis.Replace("$PROGRAMFILES", "$PROGRAMFILES64");

                            WriteTextFile(pathTemp + "/Eddie.nsi", nsis);

                            //Shell("c:\\Program Files (x86)\\NSIS\\makensisw.exe", "\"" + NormalizePath(pathTemp + "/Eddie.nsi") + "\"");
                            Shell("c:\\Program Files (x86)\\NSIS\\makensis.exe", "\"" + NormalizePath(pathTemp + "/Eddie.nsi") + "\"");

                            WindowsSignFile(pathExe, true);
                        }
                    }
				}
				else if (platform == "linux")
				{
					if (format == "mono")
					{
                        CopyFile(pathRelease, "Lib.Core.dll", pathTemp);
						CopyFile(pathRelease, "Lib.Forms.dll", pathTemp);
						CopyFile(pathRelease, "Platforms.Linux.dll", pathTemp);
						CopyFile(pathRelease, "UI.Forms.Linux.exe", pathTemp, "AirVPN.exe");
                        CopyFile(pathRelease, "CLI.Linux.exe", pathTemp, "CLI.exe");

                        string pathFinal = NormalizePath(PathBaseRepository + "/" + fileName + ".tar.gz");

						if (File.Exists(pathFinal))
							File.Delete(pathFinal);

						Shell("chmod 755 \"" + pathTemp + "/openvpn\"");
						Shell("chmod 755 \"" + pathTemp + "/stunnel\"");

						RemoveFile(pathTemp + "/libgdiplus.so.0");
						RemoveFile(pathTemp + "/libMonoPosixHelper.so");

						CreateDirectory(pathTemp + "/" + fileName);
						MoveAll(pathTemp, pathTemp + "/" + fileName);

						// TAR.GZ
						string command2 = "cd \"" + pathTemp + "\" && tar cvfz \"" + pathFinal + "\" " + "*";
						Shell(command2);
					}
					else if( (format == "portable") && (AvailableMkBundle) )
					{
						//CopyFile(PathBaseResources + "/linux_portable/eddie.config", pathTemp + "/eddie.config");
                        //CopyFile(pathBaseResources + "/linux_portable/eddie.machine.config", pathTemp + "/eddie.machine.config");

                        // mkbundle
                        string command = "mkbundle ";
						command += " \"" + pathRelease + "/UI.Forms.Linux.exe\"";
                        command += " \"" + pathRelease + "/Lib.Core.dll\"";
						command += " \"" + pathRelease + "/Lib.Forms.dll\"";
						command += " \"" + pathRelease + "/Platforms.Linux.dll\"";

						// TOOPTIMIZE: This can be avoided, but mkbundle don't support specific exclude, we need to list manually all depencencies and avoid --deps
						// Otherwise, we need to have two different WinForms project (Windows AND Linux)
						//command += " \"" + pathRelease + "/Windows.dll\"";
						//command += " \"" + pathRelease + "/Microsoft.Win32.TaskScheduler.dll\""; 
								
						command += " --deps";
                        command += " --keeptemp";
                        command += " --static";
                        //command += " --config \"" + pathTemp + "/eddie.config\"";
                        command += " --config \"" + PathBaseResources + "/linux_portable/eddie.config\"";
                        
                        //command += " --machine-config \"" + pathTemp + "/eddie.config\"";
                        command += " --machine-config /etc/mono/4.0/machine.config";
                        command += " -z";
						command += " -o \"" + pathTemp + "/airvpn\"";						
						Shell(command);

						//RemoveFile(pathTemp + "/eddie.config");

						string pathFinal = NormalizePath(PathBaseRepository + "/" + fileName + ".tar.gz");

						if (File.Exists(pathFinal))
							File.Delete(pathFinal);

						Shell("chmod 755 \"" + pathTemp + "/airvpn\"");
						Shell("chmod 755 \"" + pathTemp + "/openvpn\"");
						Shell("chmod 755 \"" + pathTemp + "/stunnel\"");

						CreateDirectory(pathTemp + "/" + fileName);
						MoveAll(pathTemp, pathTemp + "/" + fileName);

						// TAR.GZ
						string command2 = "cd \"" + pathTemp + "\" && tar cvfz \"" + pathFinal + "\" " + "*";
						Shell(command2);
					}
                    else if( (format == "debian") && (AvailableDpkg) )
                    {
                        CopyFile(pathRelease, "Lib.Core.dll", pathTemp);
						CopyFile(pathRelease, "Lib.Forms.dll", pathTemp);
						CopyFile(pathRelease, "Platforms.Linux.dll", pathTemp);
						CopyFile(pathRelease, "UI.Forms.Linux.exe", pathTemp, "AirVPN.exe");
                        CopyFile(pathRelease, "CLI.Linux.exe", pathTemp, "CLI.exe");

                        string pathFinal = NormalizePath(PathBaseRepository + "/" + fileName + ".deb");

						CreateDirectory(pathTemp + "/usr/lib/AirVPN");
						MoveAll(pathTemp, pathTemp + "/usr/lib/AirVPN");
						CopyDirectory(PathBaseResources + "/" + format, pathTemp);

						ReplaceInFile(pathTemp + "/DEBIAN/control", "{@version}", versionString);
						string debianArchitecture = "unknown";
						if(arch == "x86")
							debianArchitecture = "any-i386";
						else if (arch == "x64")
							debianArchitecture = "any-amd64";
                        else if (arch == "armhf")
                            debianArchitecture = "any-armhf";
                        ReplaceInFile(pathTemp + "/DEBIAN/control", "{@architecture}", debianArchitecture);

						RemoveFile(pathTemp + "/usr/lib/AirVPN/openvpn");
						RemoveFile(pathTemp + "/usr/lib/AirVPN/stunnel");
						RemoveFile(pathTemp + "/usr/lib/AirVPN/libgdiplus.so.0");
						RemoveFile(pathTemp + "/usr/lib/AirVPN/libMonoPosixHelper.so");

						Shell("chmod 755 -R \"" + pathTemp + "\"");

                        WriteTextFile(pathTemp + "/usr/share/doc/airvpn/changelog", FetchUrl(Constants.ChangeLogUrl));
						Shell("gzip -9 \"" + pathTemp + "/usr/share/doc/airvpn/changelog\"");
						Shell("chmod 644 \"" + pathTemp + "/usr/share/doc/airvpn/changelog.gz\"");

                        WriteTextFile(pathTemp + "/usr/share/man/man8/airvpn.8", Shell("mono \"" + pathTemp + "/usr/lib/AirVPN/AirVPN.exe\" -cli -help -help_format=man"));
						Shell("gzip -9 \"" + pathTemp + "/usr/share/man/man8/airvpn.8\"");
						Shell("chmod 644 \"" + pathTemp + "/usr/share/man/man8/airvpn.8.gz\"");

                        Shell("chmod 644 \"" + pathTemp + "/usr/lib/AirVPN/Lib.Core.dll\"");
						Shell("chmod 644 \"" + pathTemp + "/usr/lib/AirVPN/Lib.Forms.dll\"");
						Shell("chmod 644 \"" + pathTemp + "/usr/lib/AirVPN/Platforms.Linux.dll\"");
						Shell("chmod 644 \"" + pathTemp + "/usr/share/pixmaps/AirVPN.png\"");
						Shell("chmod 644 \"" + pathTemp + "/usr/share/applications/AirVPN.desktop\"");
								
						Shell("chmod 644 \"" + pathTemp + "/usr/share/doc/airvpn/copyright\"");
								
						string command = "dpkg -b \"" + pathTemp + "\" \"" + pathFinal + "\"";
						Log(command);
						Shell(command);

						Log("Lintian report:");
						Log(Shell("lintian \"" + pathFinal + "\""));
					}
					else if( (format == "rpm") && (AvailableRPM) )
					{
                        string libSubPath = "lib";
						if (arch == "x64")
							libSubPath = "lib64";

                        CopyFile(pathRelease, "Lib.Core.dll", pathTemp);
						CopyFile(pathRelease, "Lib.Forms.dll", pathTemp);
						CopyFile(pathRelease, "Platforms.Linux.dll", pathTemp);
						CopyFile(pathRelease, "UI.Forms.Linux.exe", pathTemp, "AirVPN.exe");
                        CopyFile(pathRelease, "CLI.Linux.exe", pathTemp, "CLI.exe");

                        string pathFinal = NormalizePath(PathBaseRepository + "/" + fileName + ".rpm");

						CreateDirectory(pathTemp + "/usr/" + libSubPath + "/AirVPN");
						MoveAll(pathTemp, pathTemp + "/usr/" + libSubPath + "/AirVPN");
						CopyDirectory(PathBaseResources + "/rpm", pathTemp);

						ReplaceInFile(pathTemp + "/airvpn.spec", "{@version}", versionString);
						ReplaceInFile(pathTemp + "/airvpn.spec", "{@lib}", libSubPath);

						ReplaceInFile(pathTemp + "/usr/bin/airvpn", "{@lib}", libSubPath);						
						
						RemoveFile(pathTemp + "/usr/" + libSubPath + "/AirVPN/openvpn");
						//RemoveFile(pathTemp + "/usr/lib/AirVPN/stunnel"); // OpenSUSE (RPM) don't have stunnel in stable repo
						RemoveFile(pathTemp + "/usr/" + libSubPath + "/AirVPN/libgdiplus.so.0");
						RemoveFile(pathTemp + "/usr/" + libSubPath + "/AirVPN/libMonoPosixHelper.so");

                        WriteTextFile(pathTemp + "/usr/share/man/man8/airvpn.8", Shell("mono \"" + pathTemp + "/usr/" + libSubPath + "/AirVPN/AirVPN.exe\" -cli -help -help_format=man"));
                        Shell("gzip -9 \"" + pathTemp + "/usr/share/man/man8/airvpn.8\"");
                        Shell("chmod 644 \"" + pathTemp + "/usr/share/man/man8/airvpn.8.gz\"");

                        Shell("chmod 755 -R \"" + pathTemp + "\"");
                        Shell("chmod 644 \"" + pathTemp + "/usr/" + libSubPath + "/AirVPN/Lib.Core.dll\"");
						Shell("chmod 644 \"" + pathTemp + "/usr/" + libSubPath + "/AirVPN/Lib.Forms.dll\"");
						Shell("chmod 644 \"" + pathTemp + "/usr/" + libSubPath + "/AirVPN/Platforms.Linux.dll\"");
						Shell("chmod 644 \"" + pathTemp + "/usr/share/pixmaps/AirVPN.png\"");
						Shell("chmod 644 \"" + pathTemp + "/usr/share/applications/AirVPN.desktop\"");

                        string command = "rpmbuild";
                        if(IsOfficial())
                        {
                            Log("Enter AirVPN Staff signing password for RPM build");
                            command += " -sign";
                        }
                        command += " -bb \"" + pathTemp + "/airvpn.spec\" --buildroot \"" + pathTemp + "\"";

                        Log("RPM Build");
						Shell(command);

						Shell("mv ../*.rpm " + pathFinal);
					}
				}
				else if (platform == "osx")
				{
					pathRelease = pathRelease.Replace ("/x64/Release/", "/Release/");

					// Osx bin have a specific subdirectory
					pathRelease = pathRelease.Replace ("/src/bin/", "/src/UI.Cocoa.Osx/bin/");

					if (format == "portable")
					{
						// TAR.GZ
						string pathFinal = NormalizePath(PathBaseRepository + "/" + fileName + ".tar.gz");

						if (File.Exists(pathFinal))
							File.Delete(pathFinal);

						string command2 = "cd \"" + pathRelease + "\" && tar cvfz \"" + pathFinal + "\" " + " AirVPN.app";
						Shell(command2);

					}
					else if (format == "installer")
					{
						string pathFinal = NormalizePath(PathBaseRepository + "/" + fileName + ".pkg");

						Shell ("cp " + pathRelease + "/*.pkg " + pathFinal);
					}
				}
					
			}

            Log("------------------------------");
            Log("Done");

			if (SO == "linux")
			{
                if(IsOfficial())
                {
                    Console.WriteLine("If running from a developing VM, maybe need:");
                    Console.WriteLine("cp files/eddie* /media/sf_airvpn-client/repository/files/");
                }
			}

            if (SO == "windows")
			    Pause();
		}

        static bool IsVerbose()
        {
            return Program.Arguments.Contains("verbose");
        }

        static bool IsOfficial()
        {
            return Program.Arguments.Contains("official");
        }

        static bool Compile(string architecture, int netFramework)
        {
            Log("Compilation, Architecture: " + architecture + ", NetFramework: " + netFramework.ToString());

            string pathCompiler = "";
            if (Environment.OSVersion.VersionString.IndexOf("Windows") != -1)
            {
                pathCompiler = "c:\\Program Files (x86)\\MSBuild\\14.0\\Bin\\msbuild.exe";
            }
            else
            {
                pathCompiler = "/usr/bin/xbuild";
            }

            if(File.Exists(pathCompiler) == false)
            {
                Log("Compiler expected in " + pathCompiler + " but not found, build skipped.");
                return false;
            }

            string arguments = "/p:Configuration=Release /p:Platform=" + architecture + " /p:TargetFrameworkVersion=\"v" + netFramework.ToString() + ".0\" /t:Rebuild \"" + PathBase + "/src/Eddie_VS2015.sln\"";
            string o = Shell(pathCompiler, arguments);

            if (o.IndexOf("0 Error(s)") != -1)
            {
                return true;
            }
            else
            {
                Log("Compilation errors, build skipped.");
                return false;
            }
        }

		static string NormalizePath(string path)
		{
			if (SO == "windows")
			{
				return path.Replace("/", "\\");
			}
			else
				return path.Replace("\\", "/");
		}

		static string FetchUrl(string url)
		{
			// Note: Actually used only under Linux
			return Shell("curl \"" + url + "\"").Trim();
			
			// This version works under Windows, but not under Linux/Mono due RC4 cipher deprecated on airvpn.org
			//WebClient w = new WebClient();
			//w.Proxy = null;
			//return w.DownloadString(url);			
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

		static void CreateSymlink(string path, string origin)
		{
			Shell("ln -s \"" + origin + "\" \"" + path + "\"");
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

		public static string ShellPlatformIndipendent(string FileName, string Arguments, string WorkingDirectory, bool WaitEnd, bool ShowWindow)
		{
			try
			{
				Process p = new Process();

				p.StartInfo.Arguments = Arguments;

				if (WorkingDirectory != "")
					p.StartInfo.WorkingDirectory = WorkingDirectory;

				p.StartInfo.FileName = FileName;

				if (ShowWindow == false)
				{
					//#do not show DOS window
					p.StartInfo.CreateNoWindow = true;
					p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				}

				if (WaitEnd)
				{
					p.StartInfo.UseShellExecute = false;
					p.StartInfo.RedirectStandardOutput = true;
					p.StartInfo.RedirectStandardError = true;
				}

				p.Start();

				if (WaitEnd)
				{
					string Output = p.StandardOutput.ReadToEnd() + "\n" + p.StandardError.ReadToEnd();
					p.WaitForExit();
					return Output.Trim();
				}
				else
				{
					return "";
				}
			}
			catch (Exception E)
			{
				return E.Message;
			}


		}

        /*
        static string Shell(string command)
        {
            return Shell(command, true);
        }
        */

        static string Shell(string command)
		{
            if(IsVerbose())
                Console.WriteLine("Shell: " + command);

			if(SO == "windows")
				return Shell("cmd.exe", String.Format("/c {0}", command));
			else
				return Shell("sh", String.Format("-c '{0}'", command));			
		}

        /*
		static string Shell(string filename, string arguments)
		{
			return Shell(filename, arguments, true);
		}
        */

		static string Shell(string filename, string arguments)
		{
            if(IsVerbose())
				Console.WriteLine("Shell, filename: " + filename + ", arguments: " + arguments);
			string output = ShellPlatformIndipendent(filename, arguments, "", true, false);
			if((IsVerbose()) && (output.Trim() != "") )
				Console.WriteLine("Output: " + output);
			return output;
		}

        static void WindowsSignPath(string path)
        {
            Log("Windows Signing path: " + path);

            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                bool skip = false;

                if (file.EndsWith("tap-windows.exe")) // Already signed by OpenVPN Technologies
                    skip = true;

                if(skip == false)
                    WindowsSignFile(file, false);
            }            
        }

        static void WindowsSignFile(string path, bool log)
        {
            if (Program.Arguments.Contains("official") == false)
                return;

            string pathPfx = NormalizePath(PathBaseSigning + "/eddie.pfx");
            string pathPfxPwd = NormalizePath(PathBaseSigning + "/eddie.pfx.pwd");

            string title = "Eddie - AirVPN Client";

            if( (File.Exists(pathPfx)) && (File.Exists(pathPfxPwd)) )
            {
                if(log)
                    Log("Windows Signing file: " + path);

                string cmd = "";
                cmd += PathBaseTools + "/windows/signtool.exe";
                cmd += " sign";
                cmd += " /p " + File.ReadAllText(pathPfxPwd); // Password
                cmd += " /f " + pathPfx; // Pfx
                cmd += " /t " + Constants.WindowsSigningTimestampUrl; // Timestamp
                cmd += " /d \"" + title + "\""; // Title
                cmd += " \"" + path + "\""; // File
                Shell(cmd);                
            }
            else
            {
                Log("Missing PFX or password for Windows Signatures.");
            }
        }

        static void MoveFile(string fromFilePath, string toFilePath)
        {
            if (IsVerbose())
                Log("Move file from '" + fromFilePath + "' to '" + toFilePath + "'");
            File.Move(NormalizePath(fromFilePath), NormalizePath(toFilePath));
        }

        static void CopyFile(string fromFilePath, string toFilePath)
		{
            if (IsVerbose())
                Log("Copy file from '" + fromFilePath + "' to '" + toFilePath + "'");
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

		static void RemoveFile(string path)
		{
            if (IsVerbose())
                Log("Remove file '" + path + "'");
            File.Delete(path);
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

				MoveFile(fi.FullName, to + "/" + fi.Name);
			}
		}

        static void WriteTextFile(string path, string contents)
        {
            if (IsVerbose())
                Log("Write text in '" + path + "'");
            string dir = Path.GetDirectoryName(path);
            if (Directory.Exists(dir) == false)
                Directory.CreateDirectory(dir);
            File.WriteAllText(path, contents);
        }

		static void ReplaceInFile(string path, string from, string to)
		{
            if (IsVerbose())
                Log("Replace text in '" + path + "'");
            File.WriteAllText(path, File.ReadAllText(path).Replace(from, to));
		}

		static void CopyDirectory(string fromPath, string toPath)
		{
            if (IsVerbose())
                Log("Copy directory from '" + fromPath + "' to '" + toPath + "'");

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
