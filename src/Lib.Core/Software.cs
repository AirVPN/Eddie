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

namespace Eddie.Core
{
    public class Software
    {		
		public static string TunDriver = "";
        
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
				TunDriver = Platform.Instance.GetTunDriverReport();
            }
			catch (Exception e)
			{
				Engine.Instance.Logs.Log(LogType.Warning, e);
				TunDriver = "";
			}
			
			// Tools
			AddTool("openvpn", new Tools.OpenVPN());
			AddTool("hummingbird", new Tools.Hummingbird());
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

				AddTool("tapctl", new Tools.File("tapctl.exe"));
				AddTool("eddie-wintun", new Tools.File("eddie-wintun-" + Platform.Instance.GetOsArchitecture() + ".msi"));
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
			if(TunDriver != "")
			{
				Engine.Instance.Logs.Log(LogType.Verbose, "Tun Driver - " + TunDriver);
			}
			else 
			{
				Engine.Instance.Logs.Log(LogType.Error, "Tun Driver - " + LanguageManager.GetText("OsDriverNotAvailable"));
			}

            if (Engine.Instance.GetOpenVpnTool().Available())
            {
				Engine.Instance.Logs.Log(LogType.Verbose, "OpenVPN - Version: " + Engine.Instance.GetOpenVpnTool().Version + " (" + Engine.Instance.GetOpenVpnTool().Path + ")");
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
            /*
			string pathCacert = Engine.Instance.LocateResource("cacert.pem");
			if (pathCacert != "")
            {
                Engine.Instance.Logs.Log(LogType.Verbose, "Certification Authorities: " + pathCacert);
            }
            else
            {
                Engine.Instance.Logs.Log(LogType.Warning, "Certification Authorities - " + LanguageManager.GetText("NotAvailable"));

			}
			*/           
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
