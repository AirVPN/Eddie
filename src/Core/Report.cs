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
using Eddie.Lib.Common;

namespace Eddie.Core
{
	public class Report
	{
		private List<ReportItem> Items = new List<ReportItem>();

		public Report()
		{
		}

		public void Add(string name, string data)
		{
			ReportItem item = new ReportItem();
			item.Name = name;
			if (data == null)
				item.Data = "(null)";
			else
				item.Data = data;
			Items.Add(item);
		}

		public void Start()
		{
			Engine.Instance.OnSystemReport(Messages.ReportStepCollectEnvironmentInfo, Messages.PleaseWait, 0);

			Environment();

			Engine.Instance.OnSystemReport(Messages.ReportStepTests, ToString(), 10);

			Tests();

			Engine.Instance.OnSystemReport(Messages.ReportStepLogs, ToString(), 50);

			Add(Messages.ReportOptions, Engine.Instance.Storage.GetReportForSupport());

			Add(Messages.ReportLogs, Engine.Instance.Logs.ToString());

			Engine.Instance.OnSystemReport(Messages.ReportStepLogs, ToString(), 60);

			NetworkInterfaces();
			DefaultGateways();

			Engine.Instance.OnSystemReport(Messages.ReportStepPlatform, ToString(), 70);

			Platform.Instance.OnReport(this);

			Engine.Instance.OnSystemReport(Messages.ReportStepDone, ToString(), 100);
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

			return Platform.Instance.NormalizeString(result.Trim() + "\n");
		}

		public void Tests()
		{
			IpAddresses dns = new IpAddresses("dnstest.eddie.website");
			Add("Test DNS IPv4", (dns.CountIPv4 == 2) ? Messages.Ok : Messages.Failed);
			Add("Test DNS IPv6", (dns.CountIPv6 == 2) ? Messages.Ok : Messages.Failed);

			Add("Test HTTP", TestUrl("http://" + Constants.Domain + "/test/"));
			Add("Test HTTPS", TestUrl("https://" + Constants.Domain + "/test/"));

			/*
			#if !EDDIENET20
			Add("JsonTest", Newtonsoft.Json.JsonConvert.SerializeObject(new IpAddress("8.8.8.8")));
			#endif
			*/
		}

		public string TestUrl(string url)
		{
			try
			{
				HttpRequest request = new HttpRequest();
				request.Url = url;
				HttpResponse response = Engine.Instance.FetchUrl(request);
				return response.GetLineReport();
			}
			catch (Exception ex)
			{
				return "Error:" + ex.Message;
			}
		}

		public void Environment()
		{
			Add("Eddie version", Lib.Common.Constants.VersionDesc);
			Add("Eddie OS build", Platform.Instance.GetSystemCode());
			Add("Eddie architecture", Platform.Instance.GetArchitecture());
			Add("OS type", Platform.Instance.GetCode());
			Add("OS name", Platform.Instance.GetName());
			Add("OS version", Platform.Instance.GetVersion());
			Add("OS architecture", Platform.Instance.GetOsArchitecture());
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

			Add("Detected DNS", Platform.Instance.DetectDNS().ToString());
			//Add("Detected Exit", Engine.Instance.DiscoverExit().ToString());			
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
