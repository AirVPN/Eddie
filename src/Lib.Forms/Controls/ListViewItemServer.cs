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
        public ServerInfo Info;

        public Engine Engine
        {
            get
            {
				return Engine.Instance as Gui.Engine;
            }
        }

        public void Update()
        {
			Text = Info.GetNameForList();
            ImageKey = Info.CountryCode;
            
            if (SubItems.Count == 1)
            {
                SubItems.Add("");
                SubItems.Add("");
                SubItems.Add("");
                SubItems.Add("");
                SubItems.Add("");
            }

            int score = Info.Score();
            
            SubItems[1].Text = score.ToString();

            

            SubItems[2].Text = Info.GetLocationForList();

            //+" - " + Info.PingTests.ToString();
			SubItems[3].Text = Info.GetLatencyForList();

			SubItems[5].Text = Info.GetUsersForList();

            
                        
            switch(Info.UserList)
            {
                case ServerInfo.UserListType.WhiteList:
                    {
                        ForeColor = Color.DarkGreen;
                        StateImageIndex = 0;
                    } break;
                case ServerInfo.UserListType.BlackList:
                    {
                        ForeColor = Color.DarkRed;
                        StateImageIndex = 1;
                    } break;
                default:
                    {
                        ForeColor = SystemColors.WindowText;
                        StateImageIndex = 2;
                    } break;
            }            
        }
		
    }
}
