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
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace Eddie.Core
{
	public class Report
	{
		private List<ReportItem> Items = new List<ReportItem>();

		public Report()
		{
			Build();
		}

		public void Add(string name, string data)
		{
			ReportItem item = new ReportItem();
			item.Name = name;
			item.Data = data;
			Items.Add(item);
		}

		public void Build()
		{
			Environment();

			Tests();

			Add("Important options not at defaults", Engine.Instance.Storage.GetReportForSupport());

			Add("Logs", Engine.Instance.Logs.ToString());
				
			NetworkInterfaces();
			DefaultGateways();

			Platform.Instance.OnReport(this);
		}

		public override string ToString()
		{
			string result = "Eddie System/Environment Report - " + DateTime.UtcNow.ToShortDateString() + " " + DateTime.UtcNow.ToShortTimeString() + " " + "UTC\n";
			bool lastMultiline = false;
			foreach (ReportItem item in Items)
			{
				if ((lastMultiline) || (item.IsMultiline))
					result += "\n----------------------------\n";
				else
					result += "\n";

				if (item.IsMultiline)
					result += item.Name + ":\n\n" + item.Data;
				else
					result += item.Name + ": " + item.Data;

				lastMultiline = item.IsMultiline;
			}

			return result.Trim();
		}

		public void Tests()
		{
			IpAddresses dns = new IpAddresses("dnstest.eddie.website");
			Add("Test DNS IPv4", (dns.CountIPv4 == 2) ? Messages.Yes : Messages.No);
			Add("Test DNS IPv6", (dns.CountIPv6 == 2) ? Messages.Yes : Messages.No);

			IpAddresses list = new IpAddresses(); // ClodoTemp
			list.Add("ipleak.net");
			Add("ipleak.net n. records", list.Count.ToString());
			list.Add("nl.airvpn.org");
			Add("nl.airvpn.org n. records", list.Count.ToString());

			// ClodoTemp
			string json = Newtonsoft.Json.JsonConvert.SerializeObject(dns);
			Add("json test", json);
		}

		public void Environment()
		{
			Add("Eddie version", Lib.Common.Constants.VersionDesc);
			Add("Eddie OS build", Platform.Instance.GetSystemCode());
			Add("OS type", Platform.Instance.GetCode());
			Add("OS name", Platform.Instance.GetName());
			Add("OS description", Platform.Instance.VersionDescription());
			Add("Mono /.Net Framework", Platform.Instance.GetMonoVersion());

			Add("OpenVPN driver", Software.OpenVpnDriver);
			Add("OpenVPN", Software.GetTool("openvpn").Version + " (" + Software.GetTool("openvpn").Path + ")");
			Add("SSH", Software.GetTool("ssh").Version + " (" + Software.GetTool("ssh").Path + ")");
			Add("SSL", Software.GetTool("ssl").Version + " (" + Software.GetTool("ssl").Path + ")");
			Add("curl", Software.GetTool("curl").Version + " (" + Software.GetTool("curl").Path + ")");

			Add("Profile path", Engine.Instance.Storage.GetProfilePath());
			Add("Data path", Storage.DataPath);
			Add("Application path", Platform.Instance.GetApplicationPath());
			Add("Executable path", Platform.Instance.GetExecutablePath());
			Add("Command line arguments", "(" + Lib.Common.CommandLine.SystemEnvironment.Params.Count.ToString() + " args) " + Lib.Common.CommandLine.SystemEnvironment.GetFull());

			Add("DNS", Platform.Instance.DetectDNS().ToString());
		}

		public void NetworkInterfaces()
		{
			string t = "";
			try
			{
				NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
				foreach (NetworkInterface adapter in interfaces)
				{
					t += "Network Interface: " + adapter.Name + " (" + adapter.Description + ", ID:" + adapter.Id.ToString() + ") - " + adapter.NetworkInterfaceType.ToString() + " - " + adapter.OperationalStatus.ToString();
					//t += " - Down:" + adapter.GetIPv4Statistics().BytesReceived.ToString();
					//t += " - Up:" + adapter.GetIPv4Statistics().BytesSent.ToString();
					t += "\n";
				}
			}
			catch (Exception)
			{
				t += "Unable to fetch network interfaces.\n";
			}

			Add("Network Interfaces", t);
		}

		public void DefaultGateways()
		{
			string t = "";
			List<string> gatewaysList = new List<string>();
			foreach (NetworkInterface f in NetworkInterface.GetAllNetworkInterfaces())
			{
				if (f.OperationalStatus == OperationalStatus.Up)
				{
					foreach (GatewayIPAddressInformation d in f.GetIPProperties().GatewayAddresses)
					{
						string ip = d.Address.ToString();
						if ((IpAddress.IsIP(ip)) && (ip != "0.0.0.0") && (gatewaysList.Contains(ip) == false))
						{
							//gatewaysList.Add(ip);

							t += ip + ", " + f.Description + "\n";
						}
					}
				}
			}

			Add("Default gateways", t);
		}
	}
}
