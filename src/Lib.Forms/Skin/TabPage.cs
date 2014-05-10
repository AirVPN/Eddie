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

namespace AirVPN.Gui.Skin
{
    public class TabPage : System.Windows.Forms.TabPage
    {
        public TabPage()
        {
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
                        
            // Without this, under Linux it call the parent drawing, showing glitch.
            Form.DrawImage(e.Graphics, GuiUtils.GetResourceImage("tab_l_bg"), ClientRectangle);
        }
    }
}
