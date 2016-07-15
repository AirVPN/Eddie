// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2016 AirVPN (support@airvpn.org) / https://airvpn.org )
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
using Eddie.Gui;

namespace Eddie.Gui.Skin.Skins
{
    public class Dark : Eddie.Gui.Skin.SkinBase
    {
		private static Brush m_TopBarRed = new SolidBrush(Color.FromArgb(70, 0, 0));
		private static Brush m_TopBarYellow = new SolidBrush(Color.FromArgb(70, 70, 0));
		private static Brush m_TopBarGreen = new SolidBrush(Color.FromArgb(0, 70, 0));

        public override System.Drawing.Color ForeColor
        {
            get
            {
                return Color.White;
            }
        }

        public override System.Drawing.Color BackColor
        {
            get
            {
                return Color.Black;
            }
        }

        public override Color HyperLinkColor
        {
            get
            {
                return BlueLightColor;
            }
        }

        public override Brush ForeBrush
        {
            get
            {
                return Brushes.White;
            }
        }

        public override Brush BackBrush
        {
            get
            {
                return Brushes.Black;
            }
        }

		public override Brush TabBackgroundBrush
        {
            get
            {
				return Brushes.Black;
            }
        }

        public override Brush TabButtonNormalBackBrush
        {
            get
            {
                return BlueDarkBrush;
            }
        }

        public override Brush TabButtonSelectedBackBrush
        {
            get
            {
                return BlackLightBrush;
            }
        }

        public override Brush TabButtonNormalForeBrush
        {
            get
            {
                return Brushes.Gray;
            }
        }

        public override Brush TabButtonSelectedForeBrush
        {
            get
            {
                return Brushes.White;
            }
        }

		public override Brush ListViewNormalBackBrush
		{
			get
			{
				return BlackBrush;
			}
		}

		public override Brush ListViewNormal2BackBrush
		{
			get
			{
				return Gray50Brush;
			}
		}

		public override Brush ListViewSelectedBackBrush
		{
			get
			{
				return BlueMid2Brush;
			}
		}

		public override Brush ListViewFocusedBackBrush
		{
			get
			{
				return BlueMid1Brush;
			}
		}

		public override Pen ListViewGridPen
		{
			get
			{
				return Pens.DarkGray;
			}
		}

        public override Image FormBackgroundImage
        {
            get
            {
				return GuiUtils.GetResourceImage("form_d_bg");
            }
        }

        public override Image ButtonNormalImage
        {
            get
            {
				return GuiUtils.GetResourceImage("btn_d_n");
            }
        }

        public override Image ButtonHoverImage
        {
            get
            {
				return GuiUtils.GetResourceImage("btn_d_h");
            }
        }

        public override Image ButtonDisabledImage
        {
            get
            {
				return GuiUtils.GetResourceImage("btn_d_d");
            }
        }

        public override Image MainBackImage
        {
            get
            {
				return GuiUtils.GetResourceImage("main_d_bg");
            }
        }
		
        public override void Apply(Control c)
        {
            base.Apply(c);

            if (c is Skin.ListView)
            {
                Skin.ListView c2 = c as Skin.ListView;

                c2.BackColor = BlackLightColor;
            }

            if (c is Skin.ComboBox)
            {
                Skin.ComboBox c2 = c as Skin.ComboBox;

                c2.FlatStyle = FlatStyle.Flat;
            }

            if (c is Skin.TextBox)
            {
                Skin.TextBox c2 = c as Skin.TextBox;

                c2.BorderStyle = BorderStyle.FixedSingle;
            }


        }
    }
}
