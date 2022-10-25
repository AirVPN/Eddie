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
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

#pragma warning disable CA1416 // Windows only

namespace Eddie.Forms.Skin
{
	public class Button : System.Windows.Forms.Button
	{
		private bool m_hover = false;

		public int ImageInflatePerc = 30;
		public Image ImageHover = null;
		public bool DrawBorder = true;

		public Button()
		{
		}

		protected override void OnCreateControl()
		{
			base.OnCreateControl();

			BackgroundImage = null;
			BackgroundImageLayout = ImageLayout.Stretch;
			Cursor = Cursors.Hand;

			FlatStyle = FlatStyle.Flat;
			FlatAppearance.BorderSize = 0;
		}

		public new bool Enabled
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

			if (m_hover)
			{
				m_hover = false;
				Invalidate();
			}
		}

		protected override void OnPaint(PaintEventArgs pevent)
		{
			Rectangle r = ClientRectangle;

			Image imageBackground;
			ImageAttributes imageAttributes = null;

			if (Enabled)
			{
				if (m_hover)
					imageBackground = SkinForm.Skin.ButtonHoverImage;
				else
					imageBackground = SkinForm.Skin.ButtonNormalImage;
			}
			else
			{
				imageBackground = SkinForm.Skin.ButtonDisabledImage;
				ColorMatrix imageColorMatrix = new ColorMatrix();
				imageAttributes = new ImageAttributes();
				imageColorMatrix.Matrix33 = 0.2f;
				imageAttributes.SetColorMatrix(imageColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
			}

			pevent.Graphics.DrawImage(imageBackground, r);

			Brush brushText = SkinForm.Skin.ForeBrush;
			if (Enabled == false)
				brushText = SkinForm.Skin.ForeDisabledBrush;

			if (Image == null)
			{
				pevent.Graphics.DrawString(Text, Font, brushText, r, SkinUtils.StringFormatCenterMiddle);
			}
			else if (Text == "")
			{
				Image mainImage = Image;
				if ((ImageHover != null) && (m_hover))
					mainImage = ImageHover;
				SkinForm.DrawImageContain(pevent.Graphics, mainImage, r, ImageInflatePerc);
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

			if (DrawBorder)
			{
				r.Width--;
				r.Height--;
				pevent.Graphics.DrawRectangle(Pens.Gray, r);
			}

			if (imageAttributes != null)
				imageAttributes.Dispose();
		}
	}
}

#pragma warning restore CA1416 // Windows only
