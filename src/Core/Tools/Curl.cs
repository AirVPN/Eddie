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
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Eddie.Core;

namespace Eddie.Core.Tools
{
    public class Curl : Tool
    {
        public string minVersionRequired = "7.21.7";

        public override void OnNormalizeVersion()
        {
            int posS = Version.IndexOf("\n");
            if (posS > 1)
                Version = Version.Substring(0, posS);
            if (Version.StartsWith("curl "))
                Version = Version.Substring(5);

            if (Utils.CompareVersions(Version, minVersionRequired) == -1)
                Engine.Instance.Logs.Log(LogType.Fatal, GetRequiredVersionMessage());
        }

        public override string GetFileName()
        {
            if (Platform.Instance.IsWindowsSystem())
            {
                return "curl.exe";
            }
            else
                return base.GetFileName();
        }

        public override string GetVersionArguments()
        {
            return "--version";
        }

        public string GetRequiredVersionMessage()
        {
            return "curl version " + Version + " installed in your system is too old. Minimum " + minVersionRequired + " required. Please upgrade.";
        }

        public byte[] FetchUrlEx(string url, System.Collections.Specialized.NameValueCollection parameters, string title, bool forceBypassProxy, string resolve)
        {
            if (Available() == false)
                throw new Exception("curl is required.");

            if (Utils.CompareVersions(Version, minVersionRequired) == -1)
                throw new Exception(GetRequiredVersionMessage());

            // Don't use proxy if connected to the VPN, or in special cases (checking) during connection.
            bool bypassProxy = forceBypassProxy;
            if (bypassProxy == false)
                bypassProxy = Engine.Instance.IsConnected();

            string dataParameters = "";
            if (parameters != null)
            {
                foreach (string k in parameters.Keys)
                {
                    if (dataParameters != "")
                        dataParameters += "&";
                    dataParameters += Utils.StringSafeAlphaNumeric(k) + "=" + Uri.EscapeUriString(parameters[k]);
                }
            }

            string args = "";
            if (bypassProxy == false)
            {
                string proxyMode = Engine.Instance.Storage.Get("proxy.mode").ToLowerInvariant();
                string proxyHost = Engine.Instance.Storage.Get("proxy.host");
                int proxyPort = Engine.Instance.Storage.GetInt("proxy.port");
                string proxyAuth = Engine.Instance.Storage.Get("proxy.auth").ToLowerInvariant();
                string proxyLogin = Engine.Instance.Storage.Get("proxy.login");
                string proxyPassword = Engine.Instance.Storage.Get("proxy.password");

                if (proxyMode == "detect")
                    throw new Exception("Proxy mode 'Detect' deprecated, please specify explicit.");

                if (proxyMode == "tor")
                {
                    proxyMode = "socks";
                    proxyAuth = "none";
                    proxyLogin = "";
                    proxyPassword = "";
                }

                if (proxyMode == "http")
                {
                    args += " --proxy http://" + Utils.StringSafeHost(proxyHost) + ":" + proxyPort.ToString();
                }
                else if (proxyMode == "socks")
                {
                    // curl support different types of proxy. OpenVPN not, only socks5. So, it's useless to support other kind of proxy here.
                    args += " --proxy socks5://" + Utils.StringSafeHost(proxyHost) + ":" + proxyPort.ToString();
                }

                if (proxyAuth != "none")
                {
                    if (proxyAuth == "basic")
                        args += " --proxy-basic";
                    else if (proxyAuth == "ntlm")
                        args += " --proxy-ntlm";

                    if ((proxyLogin != "") && (proxyPassword != ""))
                        args += " --proxy-user " + Utils.StringSafeLogin(proxyLogin) + ":" + Utils.StringSafePassword(proxyPassword);
                }
            }

            //
            args += " \"" + Utils.SafeStringUrl(url) + "\"";
            args += " -sS"; // -s Silent mode, -S with errors
            args += " --max-time " + Engine.Instance.Storage.GetInt("tools.curl.max-time").ToString();
            
            //args += " --no-progress-bar";

            Tool cacertTool = Software.GetTool("cacert.pem");
            if (cacertTool.Available())
                args += " --cacert \"" + Utils.SafeStringPath(cacertTool.Path) + "\"";

            if (resolve != "")
                args += " --resolve " + resolve;

            if (dataParameters != "")
                args += " --data \"" + dataParameters + "\"";
            
            string error = "";
            byte[] output = default(byte[]);
            int exitcode = -1;
            try
            {
                Process p = new Process();

                p.StartInfo.FileName = Utils.SafeStringPath(this.GetPath());
                p.StartInfo.Arguments = args;
                p.StartInfo.WorkingDirectory = "";

                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;

                p.Start();
              
                using (var memstream = new System.IO.MemoryStream())
                {
                    p.StandardOutput.BaseStream.CopyTo(memstream);
                    output = memstream.ToArray();
                }
                error = p.StandardError.ReadToEnd();

                p.WaitForExit();

                exitcode = p.ExitCode;
            }
            catch (Exception e)
            {
                error = e.Message;
                output = default(byte[]);
            }

            if (error != "")
                throw new Exception(error);

            return output;
        }
    }
}
