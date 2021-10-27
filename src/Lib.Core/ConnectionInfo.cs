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
		public Providers.IProvider Provider;

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
		public Int64 UsersMax = 0;
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
			Allowlist = 1,
			Denylist = 2
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
			if (Engine.Instance.Options.GetBool("connections.allow_anyway"))
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

		public int LoadPerc()
		{
			if (BandwidthMax == 0)
				return 100;

			Int64 bwCur = 2 * (Bandwidth * 8) / (1000 * 1000); // to Mbit/s
			Int64 bwMax = BandwidthMax;

			return Conversions.ToInt32((bwCur * 100) / bwMax);
		}

		public int UsersPerc()
		{
			if (UsersMax == 0)
				return 100;

			return Conversions.ToInt32((Users * 100) / UsersMax);
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
					string scoreType = Engine.Instance.Options.GetLower("servers.scoretype");

					double x = Users;
					double x2 = UsersPerc();

					double PenalityB = Penality * Convert.ToDouble(Provider.GetKeyValue("penality_factor", "1000"));
					double PingB = Ping * Convert.ToDouble(Provider.GetKeyValue("ping_factor", "1"));
					double LoadB = LoadPerc() * Convert.ToDouble(Provider.GetKeyValue("load_factor", "1"));
					double UsersB = UsersPerc() * Convert.ToDouble(Provider.GetKeyValue("users_factor", "1"));
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

		public ConnectionTypes.IConnectionType BuildConnection(Session session)
		{
			ConnectionTypes.IConnectionType connection = Provider.BuildConnection(this);
			connection.Info = this;
			connection.Session = session;
			connection.Build();

			return connection;
		}
	}
}
