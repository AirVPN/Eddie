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

using System.Collections.Generic;

namespace Eddie.Core
{
	public class Software
	{
		public static string XTunDriver = "";

		public static Dictionary<string, Tools.ITool> Tools = new Dictionary<string, Tools.ITool>();

		public static void AddTool(string code, Tools.ITool t)
		{
			t.Code = code;
			Tools[code] = t;
		}

		public static Tools.ITool GetTool(string code)
		{
			return Tools[code];
		}

		public static void Checking()
		{
			Tools.Clear();

			// Tools
			AddTool("openvpn", new Tools.OpenVPN());
			AddTool("hummingbird", new Tools.Hummingbird());
			AddTool("ssh", new Tools.SSH());
			AddTool("ssl", new Tools.SSL());
			if (Platform.Instance.FetchUrlInternal() == false)
				AddTool("curl", new Tools.Curl());
			if (Platform.IsUnix())
			{
				AddTool("update-resolv-conf", new Tools.File("update-resolv-conf"));
			}
			if (Platform.IsWindows())
			{
				AddTool("tap-windows", new Tools.File("tap-windows.exe"));
				AddTool("tapctl", new Tools.File("tapctl.exe"));
			}

			foreach (Tools.ITool tool in Tools.Values)
				tool.UpdatePath();
		}

		public static void ExceptionForRequired()
		{
			foreach (Tools.ITool tool in Tools.Values)
				tool.ExceptionIfRequired();
		}

		public static void Log()
		{
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
				Engine.Instance.Logs.Log(LogType.Info, "SSH - " + LanguageManager.GetText("NotAvailable"));
			}

			if (GetTool("ssl").Available())
			{
				Engine.Instance.Logs.Log(LogType.Verbose, "SSL - Version: " + GetTool("ssl").Version + " (" + GetTool("ssl").Path + ")");
			}
			else
			{
				Engine.Instance.Logs.Log(LogType.Info, "SSL - " + LanguageManager.GetText("NotAvailable"));
			}

			if (Platform.Instance.FetchUrlInternal() == false)
			{
				if (GetTool("curl").Available())
				{
					Engine.Instance.Logs.Log(LogType.Verbose, "curl - Version: " + GetTool("curl").Version + " (" + GetTool("curl").Path + ")");
				}
				else
				{
					Engine.Instance.Logs.Log(LogType.Warning, "curl - " + LanguageManager.GetText("NotAvailable"));
				}
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
			Tools.ITool t = GetTool(tool);
			if (t.Available())
				return t.Path;
			else
				return "";
		}

	}
}
