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
using System.Collections.Generic;
using System.IO;
using System.Text;
using Eddie.Common;

namespace Eddie.Core
{
    public class Software
    {		
		public static string OpenVpnDriver = "";
        
        public static Dictionary<string, Tool> Tools = new Dictionary<string, Tool>();
        
        public static void AddTool(string code, Tool t)
        {
            t.Code = code;
            Tools[code] = t;
        }

        public static Tool GetTool(string code)
        {
            return Tools[code];
        }

        public static void Checking()
		{
            Tools.Clear();

			// OpenVPN Driver
			try
			{
				OpenVpnDriver = Platform.Instance.GetDriverAvailable();
            }
			catch (Exception e)
			{
				Engine.Instance.Logs.Log(LogType.Warning, e);
				OpenVpnDriver = "";
			}
			
			// Tools
			AddTool("openvpn", new Tools.OpenVPN());
            AddTool("ssh", new Tools.SSH());
            AddTool("ssl", new Tools.SSL());
            AddTool("curl", new Tools.Curl());            
            if (Platform.IsUnix())
            {
                AddTool("update-resolv-conf", new Tools.File("update-resolv-conf"));
            }
            if (Platform.IsWindows())
            {
				AddTool("tap-windows", new Tools.File("tap-windows.exe"));
                AddTool("tap-windows-xp", new Tools.File("tap-windows-xp.exe"));
            }

            foreach (Tool tool in Tools.Values)
                tool.UpdatePath();            
		}

		public static void ExceptionForRequired()
		{
			foreach (Tool tool in Tools.Values)
				tool.ExceptionIfRequired();
		}

		public static void Log()
		{
			if(OpenVpnDriver != "")
			{
				Engine.Instance.Logs.Log(LogType.Verbose, "OpenVPN Driver - " + OpenVpnDriver);
			}
			else
			{
				Engine.Instance.Logs.Log(LogType.Error, "OpenVPN Driver - " + LanguageManager.GetText("OsDriverNotAvailable"));
			}

            if (GetTool("openvpn").Available())
            {
				Engine.Instance.Logs.Log(LogType.Verbose, "OpenVPN - Version: " + GetTool("openvpn").Version + " (" + GetTool("openvpn").Path + ")");
			}
			else
			{
				Engine.Instance.Logs.Log(LogType.Error, "OpenVPN - " + LanguageManager.GetText("NotAvailable"));
			}

            if (GetTool("ssh").Available())
            {
				Engine.Instance.Logs.Log(LogType.Verbose, "SSH - Version: " + GetTool("ssh").Version + " (" + GetTool("ssh").Path + ")");
			}
			else
			{
				Engine.Instance.Logs.Log(LogType.Warning, "SSH - " + LanguageManager.GetText("NotAvailable"));
			}

            if (GetTool("ssl").Available())
            {
				Engine.Instance.Logs.Log(LogType.Verbose, "SSL - Version: " + GetTool("ssl").Version + " (" + GetTool("ssl").Path + ")");
			}
			else
			{
				Engine.Instance.Logs.Log(LogType.Warning, "SSL - " + LanguageManager.GetText("NotAvailable"));
			}

            if (GetTool("curl").Available())
            {
                Engine.Instance.Logs.Log(LogType.Verbose, "curl - Version: " + GetTool("curl").Version + " (" + GetTool("curl").Path + ")");
            }
            else
            {
                Engine.Instance.Logs.Log(LogType.Warning, "curl - " + LanguageManager.GetText("NotAvailable"));

			}

			string pathCacert = Engine.Instance.LocateResource("cacert.pem");
			if (pathCacert != "")
            {
                Engine.Instance.Logs.Log(LogType.Verbose, "Certification Authorities: " + pathCacert);
            }
            else
            {
                Engine.Instance.Logs.Log(LogType.Warning, "Certification Authorities - " + LanguageManager.GetText("NotAvailable"));

			}
        }        

        public static string FindResource(string tool)
        {
            Tool t = GetTool(tool);
            if (t.Available())
                return t.Path;
            else
                return "";
        }

    }
}
