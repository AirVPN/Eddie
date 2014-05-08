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
using System.Text;
using System.Xml;

namespace AirVPN.Core
{
    public class ServerInfo : IComparable<ServerInfo>
    {
        public string Name;
        public string PublicName;
        public string IpEntry;
        public string IpEntry2;
        public string IpExit;
        public string CountryCode;
        public string CountryName;
        public string Location;
        public Int64 ScoreBase;
        public Int64 Bandwidth;
        public Int64 BandwidthMax;
        public Int64 Users;
		public string WarningOpen;
		public string WarningClosed;
        public Int64 ServerType;
        public bool Public;        

        public Int64 PingTests = 0;
        public Int64 PingFailedConsecutive = 0;
        public Int64 Ping = -1;
        public Int64 LastPingTest;
		public Int64 LastPingResult;
		public Int64 LastPingSuccess;

        public int Penality = 0;

        public enum UserListType
        {
            None = 0,
            WhiteList = 1,
            BlackList = 2
        }

        public UserListType UserList = UserListType.None;
        
        public bool Deleted = false;

        public int CompareTo(ServerInfo other)
        {            
            return Score().CompareTo(other.Score());
        }

        public int Load()
        {
            Int64 bwCur = 2 * (Bandwidth * 8) / (1000 * 1000); // to Mbit/s                
            Int64 bwMax = BandwidthMax;

			return Conversions.ToInt32((bwCur * 100) / bwMax);
        }

        public int Score()
        {
            if (Ping == -1)
                return 99999;
            else if (ServerType == 2)
                return 99999;
            else if (WarningClosed != "")
                return 99999;
            else if (WarningOpen != "")
                return 99999;            
            else
            {
				string scoreType = Engine.Instance.Storage.Get("servers.scoretype");
                double ScoreB = ScoreBase;
                if(scoreType == "Speed")
					ScoreB = ScoreB / Convert.ToDouble(Engine.Instance.Storage.GetManifestKeyValue("speed_factor", "1"));
                else if(scoreType == "Latency")
                    ScoreB = ScoreB / Convert.ToDouble(Engine.Instance.Storage.GetManifestKeyValue("latency_factor","50"));

                double PenalityB = Penality * Convert.ToDouble(Engine.Instance.Storage.GetManifestKeyValue("penality_factor","1000"));
				return Conversions.ToInt32(Ping + Load() + ScoreB + PenalityB);
            }
        }

        
    }
}
