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
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Eddie.Core
{
    public class Tool
    {
        public string Code = "";
        public string Path = ""; // TOFIX: must be private, use GetPath everywhere
        public string Version = "";
        public string Location = "missing";
        public string Hash = "";

        public void UpdatePath()
        {
            try
            {
                OnUpdatePath();
                OnUpdateVersion();
                OnNormalizeVersion();                
            }
            catch (Exception e)
            {
                Engine.Instance.Logs.Log(LogType.Verbose, MessagesFormatter.Format(Messages.BundleExecutableError, Code, Path));
                Engine.Instance.Logs.Log(LogType.Verbose, e.Message);
                Engine.Instance.Logs.Log(LogType.Verbose, Platform.Instance.GetExecutableReport(Path));

                Path = "";
                Version = "";
                Location = "missing";
            }
        }

        public virtual bool Available()
        {
            return (Version != "");
        }

        public virtual void OnUpdatePath()
        {
            string filename = GetFileName();
            FindResource(filename);
        }

        public virtual void OnUpdateVersion()
        {
            string finalPath = GetPath();
            if (finalPath != "")
            {
                string arguments = GetVersionArguments();
                Version = Platform.Instance.Shell(finalPath, arguments).Trim();
                if ((Version.StartsWith("Error:")) || (Version == ""))
                    throw new Exception(Version);
            }
        }

        public virtual void OnNormalizeVersion()
        {
        }

        public virtual string GetFileName()
        {
            return Code;
        }

        public virtual string GetVersionArguments()
        {
            return "";
        }

        public string GetPath()
        {
            if (Location == "custom")
                return Path;
            if (Location == "system")
                return Path;

            /*
            string realHash = ComputeHash();
            if (realHash != Hash)
            {
                //Engine.Instance.Logs.Log(LogType.Error, MessagesFormatter.Format("Unexpected hash of executable '{1}': {2} vs {3}", Path, realHash, Hash));
                return "";
            }
            */

            if (Platform.Instance.FileExists(Path))
                Platform.Instance.EnsureExecutablePermissions(Path);

            return Path;
        }

        public string ComputeHash()
        {
            if (Path == "")
                return "";

            // TOFIX: No SHA512CryptoServiceProvider in .Net 2.0
            return "";
            /*
            try
            {
                HashAlgorithm algo = new SHA512CryptoServiceProvider();

                using (var fs = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var hash = algo.ComputeHash(fs);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }                
            }
            catch(Exception e)
            {
                Engine.Instance.Logs.Log(LogType.Warning, e);
                return "";
            }
            */
        }

        public void FindResource(string filename)
        {
            string searchLocation = "";
            string customLocationOption = "tools." + Code + ".location"; // "" (auto), "bundle", "system", "custom";
            if (Engine.Instance.Storage.Exists(customLocationOption))
                searchLocation = Engine.Instance.Storage.Get(customLocationOption);

            string customPathOption = "tools." + Code + ".path";
            if (Engine.Instance.Storage.Exists(customPathOption))
            {
                string path = Platform.Instance.NormalizePath(customPathOption);
                if (Platform.Instance.FileExists(path))
                {
                    Path = path;
                    Location = "custom";
                    return;
                }
            }
            
            
                        
            // Same path
            {
                string path = Platform.Instance.NormalizePath(Platform.Instance.GetApplicationPath() + "/" + filename);
                if (Platform.Instance.FileExists(path))
                {
                    Path = path;
                    Location = "bundle";
                    return;
                }
            }

            // GIT source tree
            if (Engine.Instance.DevelopmentEnvironment)
            {
                string path = Platform.Instance.NormalizePath(Platform.Instance.GetGitDeployPath() + filename);
                if (Platform.Instance.FileExists(path))
                {
                    Path = path;
                    Location = "bundle";
                    return;
                }
            }

            // System
            List<string> names = new List<string>();
            if (filename == "stunnel")
            {
                // For example, under Ubuntu is 'stunnel4', under Fedora is 'stunnel'.
                names.Add("stunnel5");
                names.Add("stunnel4");
            }
            names.Add(filename);

            foreach (string fileNameAlt in names)
            {
                // Linux
                if (Platform.Instance.IsUnixSystem())
                {
                    string pathBin = "/usr/bin/" + fileNameAlt;
                    if (Platform.Instance.FileExists(pathBin))
                    {
                        Path = pathBin;
                        Location = "system";
                        return;
                    }

                    string pathSBin = "/usr/sbin/" + fileNameAlt;
                    if (Platform.Instance.FileExists(pathSBin))
                    {
                        Path = pathSBin;
                        Location = "system";
                        return;
                    }
                }
            }
        }
    }
}
