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
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Eddie.Core;

namespace Eddie.Gui.Controls
{
    public class ListViewItemServer : ListViewItem
    {
        public ConnectionInfo Info;

        public Engine Engine
        {
            get
            {
				return Engine.Instance as Gui.Engine;
            }
        }
                
        public void Update()
        {
            string nameForList = Info.GetNameForList();
            if (Text != nameForList)
                Text = nameForList;

            string countryCode = Info.CountryCode;
            if (ImageKey != countryCode)
                ImageKey = Info.CountryCode;
            
            if (SubItems.Count == 1)
            {
                SubItems.Add("");
                SubItems.Add("");
                SubItems.Add("");
                SubItems.Add("");
                SubItems.Add("");
            }

            string score = Info.Score().ToString();
            if (SubItems[1].Text != score)
                SubItems[1].Text = score;

            
            string location = Info.GetLocationForList();
            if (SubItems[2].Text != location)
                SubItems[2].Text = location;

            //+" - " + Info.PingTests.ToString();
            string latency = Info.GetLatencyForList();
            if (SubItems[3].Text != latency)
                SubItems[3].Text = latency;

            string users = Info.GetUsersForList();
            if(SubItems[5].Text != users)
                SubItems[5].Text = users;
            

            Color foreColor = SystemColors.WindowText;
            int stateImageIndex = 2;
            switch (Info.UserList)
            {
                case ConnectionInfo.UserListType.WhiteList:
                    {
                        foreColor = Color.DarkGreen;
                        stateImageIndex = 0;
                    } break;
                case ConnectionInfo.UserListType.BlackList:
                    {
                        foreColor = Color.DarkRed;
                        stateImageIndex = 1;
                    } break;                
            }  
            if(ForeColor != foreColor)
            {
                ForeColor = foreColor;
            }
            if(StateImageIndex != stateImageIndex)
            {
                StateImageIndex = stateImageIndex;
            }            
        }		
    }
}
