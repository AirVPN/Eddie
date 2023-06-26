// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2023 AirVPN (support@airvpn.org) / https://airvpn.org
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

using System.Drawing;
using System.Windows.Forms;

#pragma warning disable CA1416 // Windows only

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

			StringFormat sf = SkinUtils.StringFormatCenterMiddle;
			switch (this.TextAlign)
			{
				case ContentAlignment.TopLeft: sf = SkinUtils.StringFormatLeftTop; break;
				case ContentAlignment.TopCenter: sf = SkinUtils.StringFormatCenterTop; break;
				case ContentAlignment.TopRight: sf = SkinUtils.StringFormatRightTop; break;
				case ContentAlignment.MiddleLeft: sf = SkinUtils.StringFormatLeftMiddle; break;
				case ContentAlignment.MiddleCenter: sf = SkinUtils.StringFormatCenterMiddle; break;
				case ContentAlignment.MiddleRight: sf = SkinUtils.StringFormatRightMiddle; break;
				case ContentAlignment.BottomLeft: sf = SkinUtils.StringFormatLeftBottom; break;
				case ContentAlignment.BottomCenter: sf = SkinUtils.StringFormatCenterBottom; break;
				case ContentAlignment.BottomRight: sf = SkinUtils.StringFormatRightBottom; break;
				default:
					break;
			}

			Rectangle R = ClientRectangle;

			Brush FB = SkinForm.Skin.ForeBrush;
			if (Enabled == false)
				FB = SkinForm.Skin.ForeDisabledBrush;

			if (Image != null)
			{
				if (ImageAlign == ContentAlignment.TopLeft)
					e.Graphics.DrawImageUnscaled(Image, new Point(2, 2));
				else
					e.Graphics.DrawImage(Image, R);
			}

			SkinForm.DrawString(e.Graphics, Text, Font, FB, ClientRectangle, sf);
		}
	}
}

#pragma warning restore CA1416 // Windows only
