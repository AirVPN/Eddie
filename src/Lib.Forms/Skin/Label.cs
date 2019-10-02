﻿// <eddie_source_header>
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

namespace Eddie.Forms.Skin
{
	public class Label : System.Windows.Forms.Label
	{
		public Label()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (DesignMode)
				e.Graphics.FillRectangle(Brushes.LightGray, ClientRectangle);

			StringFormat sf = GuiUtils.StringFormatCenterMiddle;
			switch (this.TextAlign)
			{
				case ContentAlignment.TopLeft: sf = GuiUtils.StringFormatLeftTop; break;
				case ContentAlignment.TopCenter: sf = GuiUtils.StringFormatCenterTop; break;
				case ContentAlignment.TopRight: sf = GuiUtils.StringFormatRightTop; break;
				case ContentAlignment.MiddleLeft: sf = GuiUtils.StringFormatLeftMiddle; break;
				case ContentAlignment.MiddleCenter: sf = GuiUtils.StringFormatCenterMiddle; break;
				case ContentAlignment.MiddleRight: sf = GuiUtils.StringFormatRightMiddle; break;
				case ContentAlignment.BottomLeft: sf = GuiUtils.StringFormatLeftBottom; break;
				case ContentAlignment.BottomCenter: sf = GuiUtils.StringFormatCenterBottom; break;
				case ContentAlignment.BottomRight: sf = GuiUtils.StringFormatRightBottom; break;
			}

			Rectangle R = ClientRectangle;

			Brush FB = Form.Skin.ForeBrush;
			if (Enabled == false)
				FB = Form.Skin.ForeDisabledBrush;

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
