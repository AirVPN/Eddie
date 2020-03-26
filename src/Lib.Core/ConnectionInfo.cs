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
using System.Text;
using System.Xml;

namespace Eddie.Core
{
	public class ConnectionInfo : IComparable<ConnectionInfo>
	{
		public Provider Provider;

		public string Code; // Unique name across providers
		public string DisplayName; // Display name
		public string ProviderName; // Provider name;
		public IpAddresses IpsEntry = new IpAddresses();
		public IpAddresses IpsExit = new IpAddresses();
		public string CountryCode = "";
		public string Location = "";
		public double Latitude = 0;
		public double Longitude = 0;
		public Int64 ScoreBase = 0;
		public Int64 Bandwidth = 0;
		public Int64 BandwidthMax = 0;
		public Int64 Users = 0;
		public string WarningOpen = "";
		public string WarningClosed = "";
		public bool SupportIPv4 = false;
		public bool SupportIPv6 = false;
		public bool SupportCheck = true;
		public string OvpnDirectives;
		public List<string> CiphersTls = new List<string>();
		public List<string> CiphersTlsSuites = new List<string>();
		public List<string> CiphersData = new List<string>();
		public string Path = ""; // External .ovpn config file		

		public List<ConnectionInfoWarning> Warnings = new List<ConnectionInfoWarning>();
		//public Int64 LastPingCheck = 0;
		public Int64 PingTests = 0;
		public Int64 PingFailedConsecutive = 0;
		public Int64 Ping = -1;
		public Int64 LastPingTest = 0;
		public Int64 LastPingResult = 0;
		public Int64 LastPingSuccess = 0;

		public bool NeedDiscover = false; // If true, are updated regulary by Discover thread.
		public Int64 LastDiscover = 0;

		public int Penality = 0;

		public enum UserListType
		{
			None = 0,
			Whitelist = 1,
			Blacklist = 2
		}

		public UserListType UserList = UserListType.None;

		public bool Deleted = false;

		public ConnectionInfo()
		{
		}

		public int CompareTo(ConnectionInfo other) // Used by Engine.GetServers to order based on Score
		{
			return Score().CompareTo(other.Score());
		}

		public int CompareToEx(ConnectionInfo other, string field, bool ascending)
		{
			int returnVal = 0;
			if (field == "Name")
				returnVal = DisplayName.CompareTo(other.DisplayName);
			else if (field == "Score")
			{
				int v1 = this.Score();
				int v2 = other.Score();
				returnVal = v1.CompareTo(v2);
			}
			else if (field == "Location")
			{
				returnVal = GetLocationForOrdering().CompareTo(other.GetLocationForOrdering());
			}
			else if (field == "Latency")
			{
				long v1 = this.Ping;
				if (v1 == -1)
					v1 = long.MaxValue;
				long v2 = other.Ping;
				if (v2 == -1)
					v2 = long.MaxValue;
				returnVal = v1.CompareTo(v2);
			}
			else if (field == "Load")
			{
				int v1 = 0;
				if (this.BandwidthMax != 0)
				{
					Int64 bwCur1 = 2 * (this.Bandwidth * 8) / (1000 * 1000);
					Int64 bwMax1 = this.BandwidthMax;
					v1 = Convert.ToInt32((bwCur1 * 100) / bwMax1);
				}

				int v2 = 0;
				if (other.BandwidthMax != 0)
				{
					Int64 bwCur2 = 2 * (other.Bandwidth * 8) / (1000 * 1000);
					Int64 bwMax2 = other.BandwidthMax;
					v2 = Convert.ToInt32((bwCur2 * 100) / bwMax2);
				}

				returnVal = v1.CompareTo(v2);
			}
			else if (field == "Users")
			{
				returnVal = this.Users.CompareTo(other.Users);
			}

			if (returnVal == 0) // Second order, Name
				returnVal = this.DisplayName.CompareTo(other.DisplayName);

			// Invert the value returned by String.Compare.
			if (ascending == false)
				returnVal *= -1;

			return returnVal;
		}

		public bool HasWarningsErrors()
		{
			lock (Warnings)
			{
				foreach (ConnectionInfoWarning warning in Warnings)
					if (warning.Level == ConnectionInfoWarning.WarningType.Error)
						return true;
			}
			return false;
		}

		public bool CanConnect()
		{
			if (Engine.Instance.Storage.GetBool("connections.allow_anyway"))
				return true;

			if (HasWarningsErrors())
				return false;

			return true;
		}

		public bool CanPing()
		{
			if (IpsEntry.Count == 0)
				return false;
			if (HasWarningsErrors())
				return false;
			return true;
		}

		public void InvalidatePingResults()
		{
			LastPingTest = 0;
			PingTests = 0;
			PingFailedConsecutive = 0;
			Ping = -1;
			LastPingResult = 0;
			LastPingSuccess = 0;
		}

		public int Load()
		{
			if (BandwidthMax == 0)
				return 100;

			Int64 bwCur = 2 * (Bandwidth * 8) / (1000 * 1000); // to Mbit/s                
			Int64 bwMax = BandwidthMax;

			return Conversions.ToInt32((bwCur * 100) / bwMax);
		}

		public int Score()
		{
			lock (Warnings)
			{
				if (HasWarningsErrors())
					return 99998;
				else if (Warnings.Count > 0)
					return 99997;
				else if (Ping == -1)
					return 99995;
				else
				{
					string scoreType = Engine.Instance.Storage.GetLower("servers.scoretype");
					
					double PenalityB = Penality * Convert.ToDouble(Provider.GetKeyValue("penality_factor", "1000"));
					double PingB = Ping * Convert.ToDouble(Provider.GetKeyValue("ping_factor", "1"));
					double LoadB = Load() * Convert.ToDouble(Provider.GetKeyValue("load_factor", "1"));
					double UsersB = Users * Convert.ToDouble(Provider.GetKeyValue("users_factor", "1"));
					double ScoreB = ScoreBase;
					if (scoreType == "speed")
					{
						ScoreB = ScoreB / Convert.ToDouble(Provider.GetKeyValue("speed_factor", "1"));
						LoadB = LoadB / Convert.ToDouble(Provider.GetKeyValue("speed_load_factor", "1")); // 2.18.7
						UsersB = UsersB / Convert.ToDouble(Provider.GetKeyValue("speed_users_factor", "1")); // 2.18.7
					}
					else if (scoreType == "latency")
					{
						ScoreB = ScoreB / Convert.ToDouble(Provider.GetKeyValue("latency_factor", "500"));
						LoadB = LoadB / Convert.ToDouble(Provider.GetKeyValue("latency_load_factor", "10")); // 2.18.7
						UsersB = UsersB / Convert.ToDouble(Provider.GetKeyValue("latency_users_factor", "10")); // 2.18.7
					}					
					return Conversions.ToInt32(PenalityB + PingB + LoadB + ScoreB + UsersB);
				}
			}
		}

		public float ScorePerc()
		{
			float scoreF = (Score() - 70);
			scoreF /= 70;

			float part = 1;
			if (scoreF > 5)
				part = 0;
			else if (scoreF > 1)
				part /= scoreF;

			return part;
		}

		public string GetNameForList()
		{
			string t = DisplayName;

			if (HasWarningsErrors())
			{
				string error = "";
				lock (Warnings)
				{
					foreach (ConnectionInfoWarning w in Warnings)
						if (w.Level == ConnectionInfoWarning.WarningType.Error)
							error += w.Message + ", ";
				}
				t += " (NA: " + error.Trim(new char[] { ',', ' ' }) + ")";
			}
			else if (Warnings.Count > 0)
			{
				string warning = "";
				lock (Warnings)
				{
					foreach (ConnectionInfoWarning w in Warnings)
						if (w.Level == ConnectionInfoWarning.WarningType.Warning)
							warning += w.Message + ", ";
				}
				t += " (Warning: " + warning.Trim(new char[] { ',', ' ' }) + ")";
			}

			return t;
		}

		public string GetNameWithLocation()
		{
			string country = CountriesManager.GetNameFromCode(CountryCode);
			string result = DisplayName;
			if ((country != "") || (Location != ""))
			{
				result += " (";
				if (country != "")
					result += country;
				if (Location != "")
				{
					if (country != "")
						result += ", ";
					result += Location;
				}
				result += ")";
			}
			return result;
		}

		public string GetLatencyForList()
		{
			String text = "";
			if (Ping != -1)
				text = Ping.ToString() + " ms";
			for (int i = 0; i < PingFailedConsecutive; i++)
				text += ".";
			return text;
		}

		public string GetLocationForList()
		{
			string result = Location;
			if (result != "")
				result += " - ";
			result += CountriesManager.GetNameFromCode(CountryCode);
			return result;
		}

		public string GetLocationForOrdering()
		{
			string result = CountriesManager.GetNameFromCode(CountryCode) + " - " + Location;
			return result;
		}

		public string GetUsersForList()
		{
			if (Users == -1)
				return "-";
			else
				return Users.ToString();
		}

		public string GetLoadForList()
		{
			if (BandwidthMax == 0)
				return "-";

			Int64 bwCur = 2 * (Bandwidth * 8) / (1000 * 1000); // to Mbit/s                
			Int64 bwMax = BandwidthMax;

			float p = (float)bwCur / (float)bwMax;
			if (p > 1)
				p = 1;

			String label = Convert.ToInt16(p * 100).ToString() + "%, " + bwCur.ToString() + "/" + bwMax.ToString() + " Mbit/s";

			return label;
		}

		public float GetLoadPercForList()
		{
			if (BandwidthMax == 0)
				return 0;

			Int64 bwCur = 2 * (Bandwidth * 8) / (1000 * 1000); // to Mbit/s                
			Int64 bwMax = BandwidthMax;

			float p = (float)bwCur / (float)bwMax;

			return p;
		}

		public string GetLoadColorForList()
		{
			if (BandwidthMax == 0)
				return "yellow";

			Int64 bwCur = 2 * (Bandwidth * 8) / (1000 * 1000); // to Mbit/s                
			Int64 bwMax = BandwidthMax;

			float p = (float)bwCur / (float)bwMax;

			if (p > 0.9)
				return "red";
			else if (p > 0.5)
				return "yellow";
			else
				return "green";
		}

		public void WarningAdd(string message, ConnectionInfoWarning.WarningType level)
		{
			ConnectionInfoWarning warning = new ConnectionInfoWarning();
			warning.Message = message;
			warning.Level = level;
			lock (Warnings)
			{
				Warnings.Add(warning);
			}
		}

		public ConnectionActive BuildConnectionActive(bool preview)
		{
			// If preview, no physical additional files are created.

			ConnectionActive connectionActive = new ConnectionActive();

			Storage s = Engine.Instance.Storage;

			connectionActive.OpenVpnProfileStartup = new OvpnBuilder();
			OvpnBuilder ovpn = connectionActive.OpenVpnProfileStartup;

			ovpn.AppendDirective("setenv", "IV_GUI_VER " + Constants.Name + Constants.VersionDesc, "Client level");

			if (s.GetBool("openvpn.skip_defaults") == false)
			{
				ovpn.AppendDirectives(Engine.Instance.Storage.Get("openvpn.directives"), "Client level");
				string directivesPath = Engine.Instance.Storage.Get("openvpn.directives.path");
				if (directivesPath.Trim() != "")
				{
					try
					{
						if (Platform.Instance.FileExists(directivesPath))
						{
							string text = Platform.Instance.FileContentsReadText(directivesPath);
							ovpn.AppendDirectives(text, "Client level");
						}
						else
						{
							Engine.Instance.Logs.Log(LogType.Warning, LanguageManager.GetText("FileNotFound", directivesPath));
						}
					}
					catch (Exception ex)
					{
						Engine.Instance.Logs.Log(LogType.Warning, LanguageManager.GetText("FileErrorRead", directivesPath, ex.Message));
					}
				}
				Provider.OnBuildOvpnDefaults(ovpn);

				ovpn.AppendDirectives(OvpnDirectives, "Server level");

				if (Path != "")
				{
					if (Platform.Instance.FileExists(Path))
					{
						string text = Platform.Instance.FileContentsReadText(Path);
						ovpn.AppendDirectives(text, "Config file");

						string dirPath = Platform.Instance.FileGetDirectoryPath(Path);
						ovpn.NormalizeRelativePath(dirPath);
					}
				}
			}

			if (s.Get("openvpn.dev_node") != "")
				ovpn.AppendDirective("dev-node", s.Get("openvpn.dev_node"), "");

			if (s.Get("network.entry.iface") != "")
			{
				ovpn.AppendDirective("local", s.Get("network.entry.iface"), "");
				ovpn.RemoveDirective("nobind");
			}
			else
			{
				ovpn.RemoveDirective("local");
				ovpn.AppendDirective("nobind", "", "");
			}

			int rcvbuf = s.GetInt("openvpn.rcvbuf");
			if (rcvbuf == -2) rcvbuf = Platform.Instance.GetRecommendedRcvBufDirective();
			if (rcvbuf == -2) rcvbuf = -1;
			if (rcvbuf != -1)
				ovpn.AppendDirective("rcvbuf", rcvbuf.ToString(), "");

			int sndbuf = s.GetInt("openvpn.sndbuf");
			if (sndbuf == -2) sndbuf = Platform.Instance.GetRecommendedSndBufDirective();
			if (sndbuf == -2) sndbuf = -1;
			if (sndbuf != -1)
				ovpn.AppendDirective("sndbuf", sndbuf.ToString(), "");

			string proxyDirectiveName = "";
			string proxyDirectiveArgs = "";

			string proxyMode = s.GetLower("proxy.mode");
			string proxyWhen = s.GetLower("proxy.when");
			if ((proxyWhen == "none") || (proxyWhen == "web"))
				proxyMode = "none";
			if (proxyMode == "tor")
			{
				proxyDirectiveName = "socks-proxy";
			}
			else if (proxyMode == "http")
			{
				proxyDirectiveName = "http-proxy";

			}
			else if (proxyMode == "socks")
			{
				proxyDirectiveName = "socks-proxy";
			}

			if (proxyDirectiveName != "")
			{
				proxyDirectiveArgs += s.Get("proxy.host") + " " + s.Get("proxy.port");

				if ((s.GetLower("proxy.mode") != "none") && (s.GetLower("proxy.mode") != "tor"))
				{
					if (s.Get("proxy.auth") != "None")
					{
						string fileNameAuthOvpn = "";
						if (preview)
						{
							fileNameAuthOvpn = "dummy.ppw";
						}
						else
						{
							connectionActive.ProxyAuthFile = new TemporaryFile("ppw");
							fileNameAuthOvpn = connectionActive.ProxyAuthFile.Path;
							string fileNameData = s.Get("proxy.login") + "\n" + s.Get("proxy.password") + "\n";
							Platform.Instance.FileContentsWriteText(connectionActive.ProxyAuthFile.Path, fileNameData, Encoding.Default); // TOFIX: Check if OpenVPN expect UTF-8
							Platform.Instance.FileEnsurePermission(connectionActive.ProxyAuthFile.Path, "600");
						}
						proxyDirectiveArgs += " " + ovpn.EncodePath(fileNameAuthOvpn) + " " + s.Get("proxy.auth").ToLowerInvariant(); // 2.6 Auth Fix
					}
				}

				ovpn.AppendDirective(proxyDirectiveName, proxyDirectiveArgs, "");
			}

			{
				if (s.GetLower("network.ipv4.mode") == "in")
				{
					connectionActive.TunnelIPv4 = true;
				}
				else if (s.GetLower("network.ipv4.mode") == "in-out")
				{
					if (SupportIPv4)
						connectionActive.TunnelIPv4 = true;
					else
						connectionActive.TunnelIPv4 = false;
				}
				else if (s.GetLower("network.ipv4.mode") == "in-block")
				{
					if (SupportIPv4)
						connectionActive.TunnelIPv4 = true;
					else
						connectionActive.TunnelIPv4 = false; // Out, but doesn't matter, will be blocked.
				}
				else if (s.GetLower("network.ipv4.mode") == "out")
				{
					connectionActive.TunnelIPv4 = false;
				}
				else if (s.GetLower("network.ipv4.mode") == "block")
				{
					connectionActive.TunnelIPv4 = false; // Out, but doesn't matter, will be blocked.
				}

				if (Engine.Instance.GetNetworkIPv6Mode() == "in")
				{
					connectionActive.TunnelIPv6 = true;
				}
				else if (Engine.Instance.GetNetworkIPv6Mode() == "in-out")
				{
					if (SupportIPv6)
						connectionActive.TunnelIPv6 = true;
					else
						connectionActive.TunnelIPv6 = false;
				}
				else if (Engine.Instance.GetNetworkIPv6Mode() == "in-block")
				{
					if (SupportIPv6)
						connectionActive.TunnelIPv6 = true;
					else
						connectionActive.TunnelIPv6 = false;
				}
				else if (Engine.Instance.GetNetworkIPv6Mode() == "out")
				{
					connectionActive.TunnelIPv6 = false;
				}
				else if (Engine.Instance.GetNetworkIPv6Mode() == "block")
				{
					connectionActive.TunnelIPv6 = false;
				}

				if (Engine.Instance.GetOpenVpnTool().VersionAboveOrEqual("2.4"))
				{
					ovpn.RemoveDirective("redirect-gateway"); // Remove if exists
					ovpn.AppendDirective("pull-filter", "ignore \"redirect-gateway\"", "Forced at client side");

					if(connectionActive.TunnelIPv6 == false)
					{
						ovpn.AppendDirective("pull-filter", "ignore \"dhcp-option DNS6\"", "Client side");
						ovpn.AppendDirective("pull-filter", "ignore \"tun-ipv6\"", "Client side");
						ovpn.AppendDirective("pull-filter", "ignore \"ifconfig-ipv6\"", "Client side");
					}

					if ((connectionActive.TunnelIPv4 == false) && (connectionActive.TunnelIPv6 == false))
					{
						// no redirect-gateway
					}
					else if ((connectionActive.TunnelIPv4 == true) && (connectionActive.TunnelIPv6 == false))
					{
						ovpn.AppendDirective("redirect-gateway", "def1 bypass-dhcp", "");
						
					}
					else if ((connectionActive.TunnelIPv4 == false) && (connectionActive.TunnelIPv6 == true))
					{
						ovpn.AppendDirective("redirect-gateway", "ipv6 !ipv4 def1 bypass-dhcp", "");
					}
					else
					{
						ovpn.AppendDirective("redirect-gateway", "ipv6 def1 bypass-dhcp", "");
					}
				}
				else
				{
					// OpenVPN <2.4, IPv6 not supported, IPv4 required.
					connectionActive.TunnelIPv4 = true;
					connectionActive.TunnelIPv6 = false;

					if (connectionActive.TunnelIPv4)
					{
						ovpn.AppendDirective("redirect-gateway", "def1 bypass-dhcp", "");
					}
					else
					{
						ovpn.AppendDirective("route-nopull", "", "For Routes Out");

						// 2.9, this is used by Linux resolv-conf DNS method. Need because route-nopull also filter pushed dhcp-option.
						// Incorrect with other provider, but the right-approach (pull-filter based) require OpenVPN >2.4.
						ovpn.AppendDirective("dhcp-option", "DNS " + Constants.DnsVpn, "");
					}
				}
			}
			

			// For Checking
			foreach (IpAddress ip in IpsExit.IPs)
			{
				connectionActive.AddRoute(ip, "vpn_gateway", "For Checking Route");
			}

			string routes = s.Get("routes.custom");
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
                    if(routeEntries.Length >= 3)
					    notes = routeEntries[2];

					foreach (IpAddress ip in ipsCustomRoute.IPs)
					{
						bool layerIn = false;
						if (ip.IsV4)
							layerIn = connectionActive.TunnelIPv4;
						else if (ip.IsV6)
							layerIn = connectionActive.TunnelIPv6;
						string gateway = "";
						if ((layerIn == false) && (action == "in"))
							gateway = "vpn_gateway";
						if ((layerIn == true) && (action == "out"))
							gateway = "net_gateway";
						if (gateway != "")
							connectionActive.AddRoute(ip, gateway, (notes != "") ? notes.Safe() : ipCustomRoute);
					}
				}
			}

			if (proxyMode == "tor")
			{
				if (preview == false)
				{
					TorControl.SendNEWNYM();
				}
				IpAddresses torNodeIps = TorControl.GetGuardIps((preview == false));
				foreach (IpAddress torNodeIp in torNodeIps.IPs)
				{
					if (((connectionActive.TunnelIPv4) && (torNodeIp.IsV4)) ||
						((connectionActive.TunnelIPv6) && (torNodeIp.IsV6)))
						connectionActive.AddRoute(torNodeIp, "net_gateway", "Tor Guard");
				}
			}

			// TOCLEAN
			/*
            if(Engine.Instance.GetUseOpenVpnManagement())
            {
				string managementPasswordFile = "dummy.ppw";
				if(preview == false)
				{
					connectionActive.ManagementPassword = RandomGenerator.GetHash();
					connectionActive.ManagementPasswordFile = new TemporaryFile("ppw");
					managementPasswordFile = connectionActive.ManagementPasswordFile.Path;
                    Platform.Instance.FileContentsWriteText(managementPasswordFile, connectionActive.ManagementPassword, Encoding.ASCII); // UTF8 not recognized by OpenVPN
                    Platform.Instance.FileEnsurePermission(managementPasswordFile, "600");
				}

				ovpn.AppendDirective("management", "127.0.0.1 " + Engine.Instance.Storage.Get("openvpn.management_port") + " " + ovpn.EncodePath(managementPasswordFile), "");
			}
			*/

			// Experimental - Allow identification as Public Network in Windows. Advanced Option?
			// ovpn.Append("route-metric 512");
			// ovpn.Append("route 0.0.0.0 0.0.0.0");

			Provider.OnBuildConnectionActive(this, connectionActive);

			Provider.OnBuildConnectionActiveAuth(connectionActive);

			Platform.Instance.OnBuildOvpn(ovpn);

			ovpn.AppendDirectives(Engine.Instance.Storage.Get("openvpn.custom"), "Custom level");

			foreach (ConnectionActiveRoute route in connectionActive.Routes)
			{
				if ((route.Address.IsV6) || (Constants.FeatureAlwaysBypassOpenvpnRoute))
				{
				}
				else
				{
					// We never find a better method to manage IPv6 route via OpenVPN, at least <2.4.4
					ovpn.AppendDirective("route", route.Address.ToOpenVPN() + " " + route.Gateway, route.Notes.Safe());
				}
			}

			ovpn.Normalize();

			return connectionActive;
		}
	}
}
