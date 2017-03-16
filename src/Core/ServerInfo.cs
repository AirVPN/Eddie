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
using System.Text;
using System.Xml;

namespace Eddie.Core
{
    public class ServerInfo : IComparable<ServerInfo>
    {
        public Provider Provider;        
        //public string Name; // Unique name, display name
        //public string ProviderName;

        public string Code; // Unique name across providers
        public string DisplayName; // Display name
        public string ProviderName; // Provider name;
        public string IpEntry;
        public string IpEntry2;
        public string IpExit;
        public string CountryCode;        
        public string Location;
        public Int64 ScoreBase;
        public Int64 Bandwidth;
        public Int64 BandwidthMax;
        public Int64 Users;
		public string WarningOpen;
		public string WarningClosed;
        public bool SupportCheck;
        public string OvpnDirectives;

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
            WhiteList = 1,
            BlackList = 2
        }

        public UserListType UserList = UserListType.None;
        
        public bool Deleted = false;

        public ServerInfo()
        {
        }

        public int CompareTo(ServerInfo other) // Used by Engine.GetServers to order based on Score
        {            
            return Score().CompareTo(other.Score());
        }
        
        public int CompareToEx(ServerInfo other, string field, bool ascending)
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
			if(ascending == false)
				returnVal *= -1;
		
			return returnVal;
		}        

        public bool CanConnect()
        {
            if (Engine.Instance.Storage.GetBool("servers.allow_anyway"))
                return true;

            if (WarningClosed != "")
                return false;

            return true;
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
			if (WarningClosed != "")
				return 99998;
			else if (WarningOpen != "")
				return 99997;			
            else if (Ping == -1)
                return 99995;            
            else
            {
				string scoreType = Engine.Instance.Storage.GetLower("servers.scoretype");
                double ScoreB = ScoreBase;

                if (scoreType == "speed")
					ScoreB = ScoreB / Convert.ToDouble(Provider.GetKeyValue("speed_factor", "1"));
                else if(scoreType == "latency")
                    ScoreB = ScoreB / Convert.ToDouble(Provider.GetKeyValue("latency_factor","500"));

                double PenalityB = Penality * Convert.ToDouble(Provider.GetKeyValue("penality_factor","1000"));
				return Conversions.ToInt32(Ping + Load() + ScoreB + PenalityB);
            }
        }

		public float ScorePerc()
		{
			float scoreF = (Score() - 50);
			scoreF /= 50;

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

			if (WarningClosed != "")
				t += " (Closed: " + WarningClosed + ")";
			if (WarningOpen != "")
				t += " (Warning: " + WarningOpen + ")";

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
    }
}
