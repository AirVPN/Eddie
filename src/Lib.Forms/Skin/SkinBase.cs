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
using System.Xml;
using Eddie.Core.UI;

namespace Eddie.Gui.Skin
{
    public class SkinBase
    {
        public static Brush ShadowLightBrush = new SolidBrush(Color.FromArgb(77, 167, 253));
		        
        public static Color BlueDarkColor = Color.FromArgb(22,70,141);
        public static Brush BlueDarkBrush = new SolidBrush(BlueDarkColor);
		public static Color BlueMid1Color = Color.FromArgb(159, 207, 255);
		public static Brush BlueMid1Brush = new SolidBrush(BlueMid1Color);
		public static Color BlueMid2Color = Color.FromArgb(210, 233, 255);
		public static Brush BlueMid2Brush = new SolidBrush(BlueMid2Color);
		public static Color BlueMid3Color = Color.FromArgb(9, 105, 236);
		public static Brush BlueMid3Brush = new SolidBrush(BlueMid3Color);
		public static Color BlueMid4Color = Color.FromArgb(190, 210, 255);
		public static Brush BlueMid4Brush = new SolidBrush(BlueMid4Color);
		public static Color BlueLightColor = Color.FromArgb(64, 145, 248);
		public static Brush BlueLightBrush = new SolidBrush(BlueLightColor);

		public static Brush BlueSpecial1 = new SolidBrush(Color.FromArgb(103, 158, 226));

		public static Color BlackColor = Color.FromArgb(0, 0, 0);
		public static Brush BlackBrush = new SolidBrush(BlackColor);
		public static Color Gray50Color = Color.FromArgb(50, 50, 50);
		public static Brush Gray50Brush = new SolidBrush(Gray50Color);
        public static Color Gray220Color = Color.FromArgb(220, 220, 220);
        public static Brush Gray220Brush = new SolidBrush(Gray220Color);
        public static Color Gray248Color = Color.FromArgb(248, 248, 248);
		public static Brush Gray248Brush = new SolidBrush(Gray248Color);
		public static Color WhiteColor = Color.FromArgb(255, 255, 255);
		public static Brush WhiteBrush = new SolidBrush(WhiteColor);

        public static Color BlackLightColor = Color.FromArgb(50, 50, 50);
        public static Brush BlackLightBrush = new SolidBrush(BlackLightColor);
        public static Color WhiteLightColor = Color.FromArgb(230, 230, 230);
        public static Brush WhiteLightBrush = new SolidBrush(WhiteLightColor);
        
        public virtual void Apply(Control c)
        {
            if (c is Skin.CheckBox)
            {
                Skin.CheckBox c2 = c as Skin.CheckBox;

                c2.BackColor = Color.Transparent;
                c2.ForeColor = ForeColor;                
            }

            if (c is Skin.ComboBox)
            {
                Skin.ComboBox c2 = c as Skin.ComboBox;

                c2.BackColor = BackColor;
                c2.ForeColor = ForeColor;
                c2.FlatStyle = FlatStyle.Standard;
            }

            if (c is Skin.TextBox)
            {
                Skin.TextBox c2 = c as Skin.TextBox;

                c2.BackColor = BackColor;
                c2.ForeColor = ForeColor;
                c2.BorderStyle = BorderStyle.Fixed3D;
            }

            if (c is Skin.Label)
            {
				// TOCLEAN

                // Skin.Label c2 = c as Skin.Label;

                //c2.BackColor = Color.Transparent;
                //c2.ForeColor = ForeColor;                                    
            }

            if (c is Skin.RadioButton)
            {
                Skin.RadioButton c2 = c as Skin.RadioButton;

                c2.BackColor = Color.Transparent;
                c2.ForeColor = ForeColor;
            }

            if (c is Skin.LinkLabel)
            {
                Skin.LinkLabel c2 = c as Skin.LinkLabel;

                c2.BackColor = Color.Transparent;
                c2.ForeColor = HyperLinkColor;
                c2.ActiveLinkColor = HyperLinkColor;
                c2.LinkColor = HyperLinkColor;                
                c2.VisitedLinkColor = HyperLinkColor;                
            }

            if (c is Skin.TabPage)
            {
                Skin.TabPage c2 = c as Skin.TabPage;

				c2.BackColor = Color.Transparent;
            }

            if (c is Skin.ListView)
            {
                Skin.ListView c2 = c as Skin.ListView;

                c2.BackColor = BackColor;
                c2.ForeColor = ForeColor;                
            }

            if (c is Skin.Button)
            {
                Skin.Button c2 = c as Skin.Button;

                c2.ForeColor = ForeColor;

                //c2.UpdateBackground();
            }


            foreach (Control sc in c.Controls)
            {
                Apply(sc);
            }

            c.Invalidate();
        }

        public virtual Color ForeColor
        {
            get
            {
                return Color.Black;
            }
        }

		public virtual Color ForeColorAlt
		{
			get
			{
				return Color.White;
			}
		}

        public virtual Color BackColor
        {
            get
            {
                return Color.White;
            }
        }

        public virtual Color HyperLinkColor
        {
            get
            {
                return BlueDarkColor;
            }
        }

        public virtual Brush ForeBrush
        {
            get
            {
                return Brushes.Black;
            }
        }

		public virtual Brush ForeBrushAlt
		{
			get
			{
				return Brushes.White;
			}
		}

		public virtual Brush DisabledBrush
		{
			get
			{
				return Brushes.DarkGray;
			}
		}

        public virtual Brush BackBrush
        {
            get
            {
                return Brushes.White;
            }
        }

        public virtual Brush ShadowBrush
        {
            get
            {
                return ShadowLightBrush;
            }
        }

		public virtual Brush SplitterBrush
		{
			get
			{
				return BlueSpecial1;
			}
		}

        public virtual Brush TabBackgroundBrush
        {
            get
            {
				return BlueMid3Brush;
            }
        }

        public virtual Brush TabButtonNormalBackBrush
        {
            get
            {
                return Gray50Brush;
            }
        }

        public virtual Brush TabButtonSelectedBackBrush
        {
            get
            {
                return WhiteBrush;
            }
        }

        public virtual Brush TabButtonHoverBackBrush
        {
            get
            {
                return Gray220Brush;
            }
        }

        public virtual Brush TabButtonNormalForeBrush
        {
            get
            {
                return Brushes.White;
            }
        }

        public virtual Brush TabButtonSelectedForeBrush
        {
            get
            {
                return Brushes.Black;
            }
        }

        public virtual Brush TabButtonHoverForeBrush
        {
            get
            {
                return Brushes.Black;
            }
        }

        public virtual Brush ListViewNormalBackBrush
		{
			get
			{
				return WhiteBrush;
			}
		}

		public virtual Brush ListViewNormal2BackBrush
		{
			get
			{
				return Gray248Brush;
			}
		}

		public virtual Brush ListViewSelectedBackBrush
		{
			get
			{
				return BlueMid2Brush;
			}
		}

		public virtual Brush ListViewFocusedBackBrush
		{
			get
			{
				return BlueMid1Brush;
			}
		}

		public virtual Pen ListViewGridPen
		{
			get
			{
				return Pens.LightGray;
			}
		}

        public virtual Image FormBackgroundImage
        {
            get
            {
				return GuiUtils.GetResourceImage("form_l_bg");
            }
        }

        public virtual Image ButtonNormalImage
        {
            get
            {
				return GuiUtils.GetResourceImage("btn_l_n");
            }
        }

        public virtual Image ButtonHoverImage
        {
            get
            {
				return GuiUtils.GetResourceImage("btn_l_h");
            }
        }

        public virtual Image ButtonDisabledImage
        {
            get
            {
				return GuiUtils.GetResourceImage("btn_l_d");
            }
        }

        public virtual Image MainBackImage
        {
            get
            {
				return GuiUtils.GetResourceImage("main_l_bg");
            }
        }

		public virtual Brush BarBrush
		{
			get
			{
				return BlueMid4Brush;
			}
		}

        /*
        public virtual Font FontNormal
        {
            get
            {
                //string name = "Segoe UI, 15pt";
                string name = "Automatic, normal";
                return GuiUtils.GetFont(name);
                
            }
        }

        public virtual Font FontBig
        {
            get
            {
                //string name = "Segoe UI, 20pt";
                string name = "Automatic, big";
                return GuiUtils.GetFont(name);
            }
        }

        public virtual Font FontMono
        {
            get
            {
                //string name = "Consolas, 5pt";
                string name = "Automatic, monospace";
                return GuiUtils.GetFont(name);
            }
        }
        */

        public virtual void GraphicsCommon(Graphics g)
        {
			if(g.PixelOffsetMode != System.Drawing.Drawing2D.PixelOffsetMode.Half)
				g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
			if (g.InterpolationMode != System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor)
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
			//g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
			//g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
        }
    }
}
