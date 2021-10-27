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

		public void Start(UiClient client)
		{
			System.Threading.ThreadPool.QueueUserWorkItem(s => Start2(client));
		}

		public void Start2(UiClient client)
		{
			Json jReport = new Json();
			jReport["command"].Value = "system.report.progress";
			jReport["step"].Value = LanguageManager.GetText("ReportStepCollectEnvironmentInfo");
			jReport["body"].Value = LanguageManager.GetText("PleaseWait");
			jReport["perc"].Value = 0;
			client.OnReceive(jReport);

			Environment();

			jReport["step"].Value = LanguageManager.GetText("ReportStepTests");
			jReport["body"].Value = ToString();
			jReport["perc"].Value = 10;
			client.OnReceive(jReport);

			Tests();

			jReport["step"].Value = LanguageManager.GetText("ReportStepLogs");
			jReport["body"].Value = ToString();
			jReport["perc"].Value = 50;
			client.OnReceive(jReport);

			Add(LanguageManager.GetText("ReportOptions"), Engine.Instance.Options.GetReportForSupport().PruneForReport());

			Add(LanguageManager.GetText("ReportLogs"), Engine.Instance.Logs.ToString().PruneForReport());

			jReport["step"].Value = LanguageManager.GetText("ReportStepLogs");
			jReport["body"].Value = ToString();
			jReport["perc"].Value = 60;
			client.OnReceive(jReport);

			jReport["step"].Value = LanguageManager.GetText("ReportStepPlatform");
			jReport["body"].Value = ToString();
			jReport["perc"].Value = 70;
			client.OnReceive(jReport);

			NetworkInfo();

			Platform.Instance.OnReport(this);

			jReport["step"].Value = LanguageManager.GetText("ReportStepDone");
			jReport["body"].Value = ToString();
			jReport["perc"].Value = 100;
			client.OnReceive(jReport);
		}

		public override string ToString()
		{
			string result = "Eddie System/Environment Report - " + LanguageManager.FormatDateShort(DateTime.UtcNow) + " " + "UTC\n";
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

			return Platform.Instance.NormalizeString(result.Trim());
		}

		public void Tests()
		{
			IpAddresses dns = DnsManager.ResolveDNS("dnstest.eddie.website", true);
			Add("Test DNS IPv4", (dns.CountIPv4 == 2) ? LanguageManager.GetText("Ok") : LanguageManager.GetText("Failed"));
			Add("Test DNS IPv6", (dns.CountIPv6 == 2) ? LanguageManager.GetText("Ok") : LanguageManager.GetText("Failed"));

			Add("Test Ping IPv4", TestPing(Constants.WebSiteIPv4));
			Add("Test Ping IPv6", TestPing(Constants.WebSiteIPv6));

			Add("Test HTTP IPv4", TestUrl("http://" + Constants.WebSiteIPv4 + "/test/"));
			Add("Test HTTP IPv6", TestUrl("http://[" + Constants.WebSiteIPv6 + "]/test/"));
			Add("Test HTTPS", TestUrl("https://" + Constants.Domain + "/test/"));
		}

		public string TestPing(string ip)
		{
			try
			{
				Ping p = new Ping();
				p.Ip = ip;
				p.TimeoutMs = 5000;
				int result = p.DoSync();
				if (result != -1)
					return result + " ms";
				else
					return LanguageManager.GetText("Failed");
			}
			catch (Exception ex)
			{
				return "Error: " + ex.Message;
			}
		}

		public string TestUrl(string url)
		{
			try
			{
				HttpRequest request = new HttpRequest();
				request.Url = url;
				HttpResponse response = Engine.Instance.FetchUrl(request);
				if (response.GetBodyAscii().Trim() == "Success.")
					return LanguageManager.GetText("Ok");
				else
					return LanguageManager.GetText("Failed") + " - " + response.GetLineReport();
			}
			catch (Exception ex)
			{
				return "Error: " + ex.Message;
			}
		}

		public void Environment()
		{
			Add("Eddie version", Engine.Instance.GetVersionShow());
			Add("Eddie OS build", Platform.Instance.GetSystemCode());
			Add("Eddie architecture", Platform.Instance.GetArchitecture());
			Add("OS type", Platform.Instance.GetCode());
			Add("OS name", Platform.Instance.GetName());
			Add("OS version", Platform.Instance.GetVersion());
			Add("OS architecture", Platform.Instance.GetOsArchitecture());
			Add("Mono /.Net Framework", Platform.Instance.GetNetFrameworkVersion());

			Add("TUN drivers", Platform.Instance.OpenVpnGetTunDriverReport());
			Add("OpenVPN", Software.GetTool("openvpn").GetVersionDesc());
			Add("Hummingbird", Software.GetTool("hummingbird").GetVersionDesc());
			Add("WireGuard", Platform.Instance.GetWireGuardVersionShow());
			Add("SSH", Software.GetTool("ssh").GetVersionDesc());
			Add("SSL", Software.GetTool("ssl").GetVersionDesc());
			if (Platform.Instance.FetchUrlInternal() == false)
				Add("curl", Software.GetTool("curl").GetVersionDesc());

			Add("Profile path", Engine.Instance.GetProfilePath());
			Add("Data path", Engine.Instance.GetDataPath());
			Add("Application path", Platform.Instance.GetApplicationPath());
			Add("Executable path", Platform.Instance.GetExecutablePath());
			Add("Command line arguments", "(" + Engine.Instance.StartCommandLine.Params.Count.ToString() + " args) " + Engine.Instance.StartCommandLine.GetFull());

			{
				string nl = LanguageManager.GetText("No");
				if (Engine.Instance.NetworkLockManager.IsActive())
					nl = LanguageManager.GetText("Yes") + ", " + Engine.Instance.NetworkLockManager.GetActive().GetName();
				Add("Network Lock Active", nl);
			}

			{
				string cn = LanguageManager.GetText("No");
				if (Engine.Instance.IsConnected())
					cn = LanguageManager.GetText("Yes") + ", " + Engine.Instance.Stats.Get("ServerName").Text;
				Add("Connected to VPN", cn);
			}

			Add("Detected DNS", Platform.Instance.DetectDNS().ToString());
			//Add("Detected Exit", Engine.Instance.DiscoverExit().ToString());
		}

		public void NetworkInfo()
		{
			Add("Network Interfaces and Routes", Engine.Instance.JsonNetworkInfo().ToTextPretty());
		}
	}
}
