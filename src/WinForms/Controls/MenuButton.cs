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
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

namespace AirVPN.Gui.Controls
{
    public class MenuButton : System.Windows.Forms.Label
    {
		private bool m_hover = false;

		public MenuButton()
        {            
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

			BackColor = Color.Transparent;
			BackgroundImage = null;
            BackgroundImageLayout = ImageLayout.Stretch;
            Cursor = Cursors.Hand;

            FlatStyle = System.Windows.Forms.FlatStyle.Flat;            
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

			if (m_hover == false)
			{
				m_hover = true;
				Invalidate();
			}            
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

			if (m_hover == true)
			{
				m_hover = false;
				Invalidate();
			}            
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            //base.OnPaint(pevent);
			
            Rectangle r = ClientRectangle;

			Image img = null;
			if (m_hover)
				img = GuiUtils.GetResourceImage("topbar_logo_hi");				
			else
				img = GuiUtils.GetResourceImage("topbar_logo");

			Form.DrawImage(pevent.Graphics, img, r);
        }
    }
}
