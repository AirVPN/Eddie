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
    public class Label : System.Windows.Forms.Label
    {
        public Label()
        {            
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            StringFormat sf = new StringFormat();
            
            Int32 lNum =  (Int32)Math.Log((Double)this.TextAlign, 2);
            sf.LineAlignment = (StringAlignment)(lNum / 4);
            sf.Alignment = (StringAlignment)(lNum % 4);

            Rectangle R = ClientRectangle;

            Brush FB = Form.Skin.ForeBrush;
            			
            if (Image != null)
            {
				if (ImageAlign == ContentAlignment.TopLeft)
					e.Graphics.DrawImageUnscaled(Image, new Point(2, 2));
				else
					e.Graphics.DrawImage(Image, R);				
            }

			Form.DrawString(e.Graphics, Text, Font, FB, ClientRectangle, sf);
        }
    }
}
