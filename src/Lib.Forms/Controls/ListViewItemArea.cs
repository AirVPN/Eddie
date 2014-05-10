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
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AirVPN.Core;

namespace AirVPN.Gui.Controls
{
    public class ListViewItemArea : ListViewItem
    {
        public AreaInfo Info;

        public Engine Engine
        {
            get
            {
				return Engine.Instance as Gui.Engine;
            }
        }

        public void Update()
        {
            Text = Info.PublicName;
            ImageKey = Info.Code;

            if (SubItems.Count == 1)
            {
                SubItems.Add("");
                SubItems.Add("");
                SubItems.Add("");
                SubItems.Add("");                
            }

            String Servers = Info.Servers.ToString();
            SubItems[1].Text = Servers;

            String Users = Info.Users.ToString();
            SubItems[3].Text = Users;

            switch(Info.UserList)
            {
                case AreaInfo.UserListType.WhiteList:
                    {
                        ForeColor = Color.DarkGreen;
                        StateImageIndex = 0;
                    } break;
                case AreaInfo.UserListType.BlackList:
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
