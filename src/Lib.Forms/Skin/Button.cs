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
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

namespace Eddie.Forms.Skin
{
    public class Button : System.Windows.Forms.Button
    {
		private bool m_hover = false;

        public Button()
        {            
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

			BackgroundImage = null;
            BackgroundImageLayout = ImageLayout.Stretch;
            Cursor = Cursors.Hand;

            FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;            
        }
				
        new public bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                base.Enabled = value;
				Invalidate();        
            }
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

			Image imageBackground = null;
			ImageAttributes imageAttributes = null;

			if (Enabled)
			{
				if (m_hover)
					imageBackground = Form.Skin.ButtonHoverImage;
				else
					imageBackground = Form.Skin.ButtonNormalImage;
			}
			else
			{
				imageBackground = Form.Skin.ButtonDisabledImage;
				ColorMatrix imageColorMatrix = new ColorMatrix();
				imageAttributes = new ImageAttributes();
				imageColorMatrix.Matrix33 = 0.2f;
				imageAttributes.SetColorMatrix(imageColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);    
			}

			pevent.Graphics.DrawImage(imageBackground, r);

			Brush brushText = Form.Skin.ForeBrush;
			if (Enabled == false)
				brushText = Form.Skin.ForeDisabledBrush;

			if (Image == null)
			{
				pevent.Graphics.DrawString(Text, Font, brushText, r, GuiUtils.StringFormatCenterMiddle);
			}
			else if (Text == "")
			{
                Form.DrawImageContain(pevent.Graphics, Image, r, 30);                
            }
			else
			{	
				/*
				 * Not yet used
				SizeF sText = pevent.Graphics.MeasureString(Text, Font);

				int width = Image.Width + 10 + sText.Width;
				int height = Math.Max(Image.Height, sText.Height);
				*/
			}

			r.Width--;
			r.Height--;
			pevent.Graphics.DrawRectangle(Pens.Gray, r);			
        }
    }
}
