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
using System.Net.NetworkInformation;
using System.Text;
using System.Xml;

namespace Eddie.Core.ConnectionTypes
{
	public class IConnectionType
	{
		public string Id;

		public Session Session;
		public ConnectionInfo Info;

		public List<ConnectionRoute> Routes = new List<ConnectionRoute>();

		public bool ConfigIPv4 = true;
		public bool ConfigIPv6 = true;

		public bool RouteAllIPv4 = true;
		public bool RouteAllIPv6 = true;

		public bool BlockedIPv4 = false;
		public bool BlockedIPv6 = false;

		public DateTime TimeStart = DateTime.MinValue;
		public IpAddress EntryIP = new IpAddress(); // Here because detected (ovpn can contain hostname)
		public int EntryPort = 0;
		public IpAddresses ExitIPs = new IpAddresses(); // Here because computed

		public System.Net.NetworkInformation.NetworkInterface Interface = null;
		public string DataChannel = LanguageManager.GetText("NotAvailable");
		public string ControlChannel = LanguageManager.GetText("NotAvailable");

		public string RealIp = LanguageManager.GetText("NotAvailable");
		public Int64 TimeServer = 0;
		public Int64 TimeClient = 0;

		public Int64 BytesRead = 0;
		public Int64 BytesWrite = 0;
		public Int64 BytesLastDownloadStep = -1;
		public Int64 BytesLastUploadStep = -1;

		public bool RouteEntryIp = false;
		protected bool SkipRouteAll = false;

		public bool IsPreviewMode() // If true, no temporary files are created, for example.
		{
			return (Session == null);
		}

		public void AddRoute(IpAddress destination, string gateway, string notes)
		{
			ConnectionRoute route = new ConnectionRoute();
			route.Destination = destination;
			route.Gateway = gateway;
			route.Notes = notes;
			Routes.Add(route);
		}

		// ---------------------------------------------------------
		// Virtual
		// ---------------------------------------------------------

		public virtual string GetTypeName()
		{
			return "";
		}

		public virtual void Build()
		{
			Id = RandomGenerator.GetRandomToken();
		}

		public virtual void SetupLayers()
		{
			Options options = Engine.Instance.Options;

			// Layers

			if (options.GetLower("network.ipv4.mode") == "in")
			{
				ConfigIPv4 = true;
				RouteAllIPv4 = true;
			}
			else if (options.GetLower("network.ipv4.mode") == "in-out")
			{
				if (Info.SupportIPv4)
				{
					ConfigIPv4 = true;
					RouteAllIPv4 = true;
				}
				else
				{
					ConfigIPv4 = true;
					RouteAllIPv4 = false;
				}
			}
			else if (options.GetLower("network.ipv4.mode") == "in-block")
			{
				if (Info.SupportIPv4)
				{
					ConfigIPv4 = true;
					RouteAllIPv4 = true;
				}
				else
				{
					ConfigIPv4 = false;
					RouteAllIPv4 = false; // Out, but doesn't matter, will be blocked.
				}
			}
			else if (options.GetLower("network.ipv4.mode") == "out")
			{
				ConfigIPv4 = true;
				RouteAllIPv4 = false;
			}
			else if (options.GetLower("network.ipv4.mode") == "block")
			{
				ConfigIPv4 = false;
				RouteAllIPv4 = false; // Out, but doesn't matter, will be blocked.
			}

			if (Engine.Instance.GetNetworkIPv6Mode() == "in")
			{
				ConfigIPv6 = true;
				RouteAllIPv6 = true;
			}
			else if (Engine.Instance.GetNetworkIPv6Mode() == "in-out")
			{
				if (Info.SupportIPv6)
				{
					ConfigIPv6 = true;
					RouteAllIPv6 = true;
				}
				else
				{
					ConfigIPv6 = true;
					RouteAllIPv6 = false;
				}
			}
			else if (Engine.Instance.GetNetworkIPv6Mode() == "in-block")
			{
				if (Info.SupportIPv6)
				{
					ConfigIPv6 = true;
					RouteAllIPv6 = true;
				}
				else
				{
					ConfigIPv6 = false;
					RouteAllIPv6 = false;
				}
			}
			else if (Engine.Instance.GetNetworkIPv6Mode() == "out")
			{
				ConfigIPv6 = true;
				RouteAllIPv6 = false;
			}
			else if (Engine.Instance.GetNetworkIPv6Mode() == "block")
			{
				ConfigIPv6 = false;
				RouteAllIPv6 = false;
			}
		}

		public virtual void SetupRoutes()
		{
			// TOFIX: Remember, if i use a generic ovpn with hostname, EntryIp will be empty here, because valorized after

			// The AddRoute(EntryIP, "net_gateway", "EntryIP"), wg-quick implement it with fwmark in Linux. Here we simply route the EntryIP for simplicity between OS

			if (SkipRouteAll == false)
			{
				string routeCatchAllMode = Engine.Instance.Options.Get("routes.catch_all_mode");
				if (routeCatchAllMode == "auto")
					routeCatchAllMode = "double";
				if (RouteAllIPv4)
				{
					if (routeCatchAllMode == "single")
						AddRoute("0.0.0.0/0", "vpn_gateway", "Route All IPv4");
					else if (routeCatchAllMode == "double")
					{
						AddRoute("0.0.0.0/1", "vpn_gateway", "Route All IPv4 1/2");
						AddRoute("128.0.0.0/1", "vpn_gateway", "Route All IPv4 2/2");
					}
					else if (routeCatchAllMode == "extended")
					{
						AddRoute("0.0.0.0/2", "vpn_gateway", "Route All IPv4 1/4");
						AddRoute("64.0.0.0/2", "vpn_gateway", "Route All IPv4 2/4");
						AddRoute("128.0.0.0/2", "vpn_gateway", "Route All IPv4 3/4");
						AddRoute("192.0.0.0/2", "vpn_gateway", "Route All IPv4 4/4");
					}
					else
						throw new Exception("Unsupported route catch-all mode");
				}

				if (RouteAllIPv6)
				{
					if (routeCatchAllMode == "single")
						AddRoute("::/0", "vpn_gateway", "Route All IPv6");
					else if (routeCatchAllMode == "double")
					{
						AddRoute("::/1", "vpn_gateway", "Route All IPv6 1/2");
						AddRoute("8000::/1", "vpn_gateway", "Route All IPv6 2/2");
					}
					else if (routeCatchAllMode == "extended")
					{
						AddRoute("::/2", "vpn_gateway", "Route All IPv6 1/4");
						AddRoute("4000::/2", "vpn_gateway", "Route All IPv6 2/4");
						AddRoute("8000::/2", "vpn_gateway", "Route All IPv6 3/4");
						AddRoute("C000::/2", "vpn_gateway", "Route All IPv6 4/4");
					}
					else
						throw new Exception("Unsupported route catch-all mode");
				}

				AddRoute(EntryIP, "net_gateway", "EntryIP");
				RouteEntryIp = false;
			}

			if (RouteEntryIp)
				AddRoute(EntryIP, "net_gateway", "Entry IP");

			foreach (IpAddress ip in Info.IpsExit.IPs)
			{
				AddRoute(ip, "vpn_gateway", "For Checking Route");
			}

			if (Platform.Instance.GetUseOpenVpnRoutes() == false)
				AddRoute(EntryIP, "net_gateway", "IP Entry");

			string routes = Engine.Instance.Options.Get("routes.custom");
			string[] routes2 = routes.Split(';');
			foreach (string route in routes2)
			{
				string[] routeEntries = route.Split(',');
				if (routeEntries.Length < 2)
					continue;

				string ipCustomRoute = routeEntries[0];
				IpAddresses ipsCustomRoute = new IpAddresses(ipCustomRoute);

				if (ipsCustomRoute.Count == 0)
				{
					Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("CustomRouteInvalid", ipCustomRoute.ToString()));
				}
				else
				{
					string action = routeEntries[1];
					string notes = "";
					if (routeEntries.Length >= 3)
						notes = routeEntries[2];

					foreach (IpAddress ip in ipsCustomRoute.IPs)
					{
						bool layerAlreadyRouted = false;
						if (ip.IsV4)
							layerAlreadyRouted = RouteAllIPv4;
						else if (ip.IsV6)
							layerAlreadyRouted = RouteAllIPv6;
						string gateway = "";
						if ((layerAlreadyRouted == false) && (action == "in"))
							gateway = "vpn_gateway";
						if ((layerAlreadyRouted == true) && (action == "out"))
							gateway = "net_gateway";
						if (gateway != "")
							AddRoute(ip, gateway, (notes != "") ? notes.Safe() : ipCustomRoute);
					}
				}
			}

			string proxyMode = Engine.Instance.Options.GetLower("proxy.mode");
			string proxyWhen = Engine.Instance.Options.GetLower("proxy.when");
			if ((proxyWhen == "none") || (proxyWhen == "web"))
				proxyMode = "none";

			if (proxyMode == "tor")
			{
				bool forceGuardIPs = false;
				if (IsPreviewMode() == false)
				{
					TorControl.SendNEWNYM();
					forceGuardIPs = true;
				}
				IpAddresses torNodeIps = TorControl.GetGuardIps(forceGuardIPs);
				foreach (IpAddress torNodeIp in torNodeIps.IPs)
				{
					if (((ConfigIPv4) && (torNodeIp.IsV4)) ||
						((ConfigIPv6) && (torNodeIp.IsV6)))
						AddRoute(torNodeIp, "net_gateway", "Tor Guard");
				}
			}
		}

		public virtual void OnInit()
		{

		}

		public virtual void OnStart()
		{
		}

		public virtual void OnStop()
		{

		}

		public virtual void OnCleanAfterStart() // For remove temporary files already read in memory and don't need anymore
		{

		}

		public virtual bool OnWaitingConnection()
		{
			return true;
		}

		public virtual bool OnWaitingDisconnection()
		{
			return true;
		}

		public virtual void OnClose()
		{
			OnCleanAfterStart();

			if (Engine.Instance.NetworkLockManager != null)
				Engine.Instance.NetworkLockManager.DeallowInterface(Interface);
		}

		public virtual void OnLogEvent(string source, string message)
		{
		}

		public virtual bool NeedCredentials()
		{
			return false;
		}

		public virtual void SetCredentialsUserPass(string username, string password)
		{

		}

		public virtual void CheckForWarnings()
		{

		}

		public virtual string AdaptMessage(string str) // Normalize/Adapt/Translate message coming from process
		{
			str = str.TrimStartChars("\t\r\n :-,");
			return str;
		}

		public virtual void OverrideElevatedCommandParameters()
		{
		}

		public virtual void EnsureDriver()
		{

		}

		public virtual IpAddresses GetDns()
		{
			return null;
		}

		public virtual IpAddresses GetVpnIPs()
		{
			return null;
		}

		public virtual bool GetProxyMode()
		{
			return false;
		}

		public virtual string GetProtocolDescription()
		{
			return "";
		}

		public virtual string ExportConfigOriginal()
		{
			if (Info.Path != "")
			{
				if (Platform.Instance.FileExists(Info.Path))
				{
					return Platform.Instance.FileContentsReadText(Info.Path);
				}
			}

			return "";
		}

		public virtual string ExportConfigStartup()
		{
			return "";
		}

		public virtual string ExportConfigPush()
		{
			return ExportConfigStartup();
		}

		// -----------------------------------------
		// Protected
		// -----------------------------------------

		protected void SearchTunNetworkInterfaceByName(string id)
		{
			if (Interface != null)
				return; // Already detected

			// Search NetworkInterface
			NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

			// Search by ID
			foreach (NetworkInterface networkInterface in interfaces)
			{
				if (networkInterface.Id == id)
				{
					SetTunNetworkInterface(networkInterface);
					return;
				}
			}

			// Search by Name
			foreach (NetworkInterface networkInterface in interfaces)
			{
				if (networkInterface.Name == id)
				{
					SetTunNetworkInterface(networkInterface);
					return;
				}
			}

			throw new Exception("Unexpected: Network interface unknown");
		}

		protected void SetTunNetworkInterface(NetworkInterface networkInterface)
		{
			Interface = networkInterface;

			if (Engine.Instance.NetworkLockManager != null)
				Engine.Instance.NetworkLockManager.AllowInterface(networkInterface);

			Json jInfo = Engine.Instance.JsonNetworkInterfaceInfo(networkInterface);

			if ((ConfigIPv4) && (jInfo != null) && (jInfo.HasKey("support_ipv4")) && (Conversions.ToBool(jInfo["support_ipv4"].Value) == false))
			{
				Engine.Instance.Logs.LogWarning(LanguageManager.GetText("IPv4NotSupportedByNetworkAdapter"));
				if ((Engine.Instance.Options.GetBool("network.ipv4.autoswitch")) && (Engine.Instance.Options.Get("network.ipv4.mode") != "block"))
				{
					Engine.Instance.Logs.LogWarning(LanguageManager.GetText("IPv4NotSupportedByNetworkAdapterAutoSwitch"));
					Engine.Instance.Options.Set("network.ipv4.mode", "block");
				}
			}

			if ((ConfigIPv6) && (jInfo != null) && (jInfo.HasKey("support_ipv6")) && (Conversions.ToBool(jInfo["support_ipv6"].Value) == false))
			{
				Engine.Instance.Logs.LogWarning(LanguageManager.GetText("IPv6NotSupportedByNetworkAdapter"));
				if ((Engine.Instance.Options.GetBool("network.ipv6.autoswitch")) && (Engine.Instance.Options.Get("network.ipv6.mode") != "block"))
				{
					Engine.Instance.Logs.LogWarning(LanguageManager.GetText("IPv6NotSupportedByNetworkAdapterAutoSwitch"));
					Engine.Instance.Options.Set("network.ipv6.mode", "block");
				}
			}
		}
	}
}
