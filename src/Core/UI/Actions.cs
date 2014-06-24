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
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace AirVPN.Core.UI
{
	// Inside UI, we generally try to call this methods, to simplify portability between different UI on different platforms .

	public static class Actions
	{
		public static void OpenUrlAirVPN(string subPath)
		{
			Platform.Instance.OpenUrl(Constants.WebSite + "/" + subPath);
		}

		public static void OpenUrlWebsite()
		{
			OpenUrlAirVPN("");
		}

		public static void OpenUrlDocs()
		{
			OpenUrlAirVPN("software/");            
		}

		public static void OpenUrlDocsProtocols()
		{
			OpenUrlAirVPN("faq/software_protocols/"); 
		}

		public static void OpenUrlDocsAdvanced()
		{
			OpenUrlAirVPN("faq/software_advanced/");
		}

		public static void OpenUrlPorts()
		{
			OpenUrlAirVPN("ports/"); 
		}

		public static void OpenUrlClient()
		{
			OpenUrlAirVPN("client/"); 
		}

		public static void OpenUrlSpeedTest()
		{
			OpenUrlAirVPN("speedtest/"); 
		}

		public static void OpenUrlGpl()
		{
			Platform.Instance.OpenUrl("http://www.gnu.org/licenses/gpl.html");            
		}

		public static void OpenUrlOpenVpnManagement()
		{
			Platform.Instance.OpenUrl("http://openvpn.net/index.php/open-source/documentation/miscellaneous/79-management-interface.html");            
		}
		
		public static void SendOpenVpnManagementCommand(string command)
		{
			Engine.Instance.SendManagementCommand(command);
		}

		public static string GetAboutLicense()
		{
			return Engine.Instance.ResourceGet("license.txt");			
		}

		public static string GetAboutThirdParty()
		{
			return Engine.Instance.ResourceGet("thirdparty.txt");			
		}

		public static string GetTos()
		{
			return Engine.Instance.ResourceGet("tos.txt");			
		}

		public static string GetMan(string format)
		{
			string o = "\n";
			o += "[b]NAME[/b]\n";
			o += "\t" + Messages.ManName.Replace("\n","\n\t");
			o += "\n\n[b]SYNOPSIS[/b]\n";
			o += "\t" + Messages.ManSynopsis.Replace("\n", "\n\t");
			o += "\n\n[b]DESCRIPTION[/b]\n";
			o += "\t" + Messages.ManDescription.Replace("\n", "\n\t");
			o += "\n\n[b]OPTIONS[/b]\n";
			o += "\t[list]" + Engine.Instance.Storage.GetMan().Replace("\n", "\n\t") + "[/list]";
			o += "\n\n[b]COPYRIGHT[/b]\n";
			o += "\t" + Messages.ManCopyright.Replace("\n", "\n\t\t");
			o += "\n";

			if (format != "bbc")
			{
				o = o.Replace("[b]", "");
				o = o.Replace("[/b]", "");
				o = o.Replace("[i]", "");
				o = o.Replace("[/i]", "");
				o = o.Replace("[*]", "");
				o = o.Replace("[/*]", "");
				o = o.Replace("[list]", "");
				o = o.Replace("[/list]", "");
			}

			return o;
		}
	}
}
