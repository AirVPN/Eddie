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
using System.Windows.Forms;

namespace Eddie.Forms.Controls
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

			if (m_hover)
			{
				m_hover = false;
				Invalidate();
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			//base.OnPaint(pevent);

			Rectangle r = ClientRectangle;

			Image img = null;
			if (m_hover)
				img = GuiUtils.GetResourceImage("topbar_logo_hi");
			else
				img = GuiUtils.GetResourceImage("topbar_logo");

			Skin.SkinForm.DrawImage(e.Graphics, img, r);
		}
	}
}
