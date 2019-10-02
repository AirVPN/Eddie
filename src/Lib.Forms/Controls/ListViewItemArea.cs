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
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Eddie.Core;

namespace Eddie.Forms.Controls
{
    public class ListViewItemArea : ListViewItem
    {
        public AreaInfo Info;

        public Engine Engine
        {
            get
            {
				return Engine.Instance as Eddie.Forms.Engine;
            }
        }

        public void Update()
        {
            string nameForList = Info.GetNameForList();
            if (Text != nameForList)
                Text = nameForList;

            string code = Info.Code;
            if(ImageKey != code)
                ImageKey = code;

            if (SubItems.Count == 1)
            {
                SubItems.Add("");
                SubItems.Add("");
                SubItems.Add("");
                SubItems.Add("");                
            }

            string servers = Info.GetServersForList();
            if (SubItems[1].Text != servers)
                SubItems[1].Text = servers;
                        
            string users = Info.GetUsersForList();
            if(SubItems[3].Text != users)
                SubItems[3].Text = users;

            Color foreColor = SystemColors.WindowText;
            int stateImageIndex = 2;
            switch(Info.UserList)
            {
                case AreaInfo.UserListType.WhiteList:
                    {
                        foreColor = Color.DarkGreen;
                        stateImageIndex = 0;
                    } break;
                case AreaInfo.UserListType.BlackList:
                    {
                        foreColor = Color.DarkRed;
                        stateImageIndex = 1;
                    } break;                
            }
            if (ForeColor != foreColor)
                ForeColor = foreColor;
            if (StateImageIndex != stateImageIndex)
                StateImageIndex = stateImageIndex;
        }
    }
}
