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

namespace AirVPN.Core
{
    public class AreaInfo
    {
        public string Code;
        public string PublicName;
        
        public Int64 Bandwidth;
        public Int64 BandwidthMax;
        public Int64 Users;
        public Int64 Servers;

        public enum UserListType
        {
            None = 0,
            WhiteList = 1,
            BlackList = 2
        }

        public UserListType UserList = UserListType.None;

        public bool Deleted = false;

		public int CompareToEx(AreaInfo other, string field, bool ascending)
		{
			int returnVal = 0;
			if (field == "Name")
				returnVal = PublicName.CompareTo(other.PublicName);
			else if (field == "Servers")
			{
				returnVal = this.Servers.CompareTo(other.Servers);
			}			
			else if (field == "Load")
			{
				Int64 bwCur1 = 2 * (this.Bandwidth * 8) / (1000 * 1000);
				Int64 bwMax1 = this.BandwidthMax;
				int v1 = Convert.ToInt32((bwCur1 * 100) / bwMax1);

				Int64 bwCur2 = 2 * (other.Bandwidth * 8) / (1000 * 1000);
				Int64 bwMax2 = other.BandwidthMax;
				int v2 = Convert.ToInt32((bwCur2 * 100) / bwMax2);

				returnVal = v1.CompareTo(v2);
			}
			else if (field == "Users")
			{
				returnVal = this.Users.CompareTo(other.Users);
			}

			if (returnVal == 0) // Second order, Name
				returnVal = this.PublicName.CompareTo(other.PublicName);

			// Invert the value returned by String.Compare.
			if (ascending == false)
				returnVal *= -1;

			return returnVal;
		}

		public string GetLoadForList()
		{
			Int64 bwCur = 2 * (Bandwidth * 8) / (1000 * 1000); // to Mbit/s                
			Int64 bwMax = BandwidthMax;

			float p = (float)bwCur / (float)bwMax;

			String label = Convert.ToInt16(p * 100).ToString() + "%, " + bwCur.ToString() + " / " + bwMax.ToString() + " Mbit/s";
			return label;
		}

		public float GetLoadPercForList()
		{
			Int64 bwCur = 2 * (Bandwidth * 8) / (1000 * 1000); // to Mbit/s                
			Int64 bwMax = BandwidthMax;

			float p = (float)bwCur / (float)bwMax;

			return p;
		}
    }
}
