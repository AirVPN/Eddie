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
using System.Windows.Forms;
using System.Text;
using AirVPN.Core;

namespace AirVPN.Gui
{
    public class Form : System.Windows.Forms.Form
    {        
        public static Skin.Skins.Light SkinLight = new Skin.Skins.Light();
        public static Skin.Skins.Dark SkinDark = new Skin.Skins.Dark();
        public static Skin.SkinBase Skin = SkinLight;

        public static Gui.Engine Engine
        {
            get
            {
				return Engine.Instance as Gui.Engine;
            }
        }

        public static bool ChangeSkin(string name)
        {
            Skin.SkinBase newSkin = null;

			if (Engine.Instance == null)
                newSkin = SkinLight;
            else if (name == "Light")
                newSkin = SkinLight;
            else if (name == "Dark")
                newSkin = SkinDark;
            else
                newSkin = SkinLight;

            if (Skin != newSkin)
            {
                Skin = newSkin;
                return true;
            }
            else
                return false;
        }
                
        public void CommonInit(string Title)
        {
            String TitleText = Constants.Name;
            if (Title != "")
                TitleText = TitleText + " - " + Title;

            Text = TitleText;

			Icon = WinForms.Properties.Resources.icon;

			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);			
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ApplySkin();
        }

		protected override void OnPaintBackground(PaintEventArgs e)
        {
            Form.DrawImage(e.Graphics, Skin.FormBackgroundImage, this.ClientRectangle);
            //base.OnPaintBackground(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {			
        }

        public virtual void ApplySkin()
        {
            Skin.Apply(this);
        }

        public string GetFormLayout()
        {
            string output = "";
            if (this.Visible)
                output += "1,";
            else
                output += "0,";

            if (this.WindowState == FormWindowState.Minimized)
                output += "m,";
            else if (this.WindowState == FormWindowState.Maximized)
                output += "M,";
            else
                output += "n,";

            System.Drawing.Rectangle R = this.Bounds;
            if (this.WindowState != FormWindowState.Normal)
                R = this.RestoreBounds;

            output += Convert.ToString(R.X) + ",";
            output += Convert.ToString(R.Y) + ",";
			output += Convert.ToString(ClientSize.Width) + ",";
			output += Convert.ToString(ClientSize.Height);

            return output;
        }

        public void SetFormLayout(string v, bool ignoreVisible, bool defaultVisible, Size defaultSize)
        {
			if (v == "")
			{
				v = "";
				v += (defaultVisible ? "1" : "0") + ",";
				v += "n,";
				v += (Screen.PrimaryScreen.WorkingArea.Width / 2 - defaultSize.Width / 2).ToString() + ",";
				v += (Screen.PrimaryScreen.WorkingArea.Height / 2 - defaultSize.Height / 2).ToString() + ",";
				v += defaultSize.Width.ToString() + ",";
				v += defaultSize.Height.ToString();
			}                

            string[] fields = v.Split(',');
            if (fields.Length != 6)
                return;

            Screen scrn = Screen.FromControl(this);
            if (scrn == null)
                scrn = Screen.PrimaryScreen;
            int deskHeight = SystemInformation.VirtualScreen.Height;
            int deskWidth = SystemInformation.VirtualScreen.Width;

            bool vis = ((fields[0] == "0") ? false : true);
            String state = fields[1];
            int l = Convert.ToInt32(fields[2]);
            int t = Convert.ToInt32(fields[3]);
            int w = Convert.ToInt32(fields[4]);
            int h = Convert.ToInt32(fields[5]);

            if (l < 0)
                l = 0;
            if (t < 0)
                t = 0;
            if (w < 50)
                w = 50;
            if (h < 50)
                h = 50;
            if (l + w > deskWidth)
                l = deskWidth - w;
            if (t + h > deskHeight)
                t = deskHeight - h;

            this.Visible = vis;
            this.Left = l;
            this.Top = t;
			//this.Width = w;
			//this.Height = h;

			this.ClientSize = new Size(w, h);
			
			if(ignoreVisible == false)
				if (this.Visible)
					this.Activate();

            if (state == "m")
                this.WindowState = FormWindowState.Minimized;
            else if (state == "M")
                this.WindowState = FormWindowState.Maximized;
            else if (state == "n")
                this.WindowState = FormWindowState.Normal;
        }

        public void SetClientSize(int w, int h)
        {
            //this.SetClientSizeCore(w, h);            
            ClientSize = new Size(w, h);
        }

        public void DrawString(Graphics G, string Text, Font Font, Brush Brush, PointF Point)
        {
            G.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            int Delta = Convert.ToInt32(Font.Size / 7);
            Delta = 1;
            G.DrawString(Text, Font, Brushes.White, new PointF(Point.X - Delta, Point.Y - Delta));
            G.DrawString(Text, Font, Brushes.White, new PointF(Point.X + Delta, Point.Y + Delta));
            G.DrawString(Text, Font, Brushes.White, new PointF(Point.X - Delta, Point.Y + Delta));
            G.DrawString(Text, Font, Brushes.White, new PointF(Point.X + Delta, Point.Y - Delta));
            G.DrawString(Text, Font, Brush, Point);
        }

        public void DrawString(Graphics G, string Text, Font Font, Brush Brush, RectangleF Rect, StringFormat Format)
        {
            G.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            int Delta = Convert.ToInt32(Font.Size / 7);
            Delta = 1;
            RectangleF RD1 = Rect;
            RectangleF RD2 = Rect;
            RectangleF RD3 = Rect;
            RectangleF RD4 = Rect;
            RD1.Offset(-Delta, -Delta);
            RD2.Offset(Delta, Delta);
            RD3.Offset(Delta, -Delta);
            RD4.Offset(-Delta, Delta);
            G.DrawString(Text, Font, Brushes.White, RD1, Format);
            G.DrawString(Text, Font, Brushes.White, RD2, Format);
            G.DrawString(Text, Font, Brushes.White, RD3, Format);
            G.DrawString(Text, Font, Brushes.White, RD4, Format);
            G.DrawString(Text, Font, Brush, Rect, Format);
        }

		public static void DrawImage(Graphics g, Image i, Rectangle r)
		{
			g.DrawImage(i, r);
		}
		
		public static void DrawStringOutline(Graphics g, String t, Font f, Brush b, Rectangle r, StringFormat sf)
		{
			for (int ox = -1; ox <= 1; ox++)
			{
				for (int oy = -1; oy <= 1; oy++)
				{
					DrawString(g, t, f, Brushes.White, r, sf, ox, oy);
				}
			}

			g.DrawString(t, f, b, r, sf);
		}

		public static void DrawString(Graphics g, String t, Font f, Brush b, Rectangle r, StringFormat sf)
		{
			DrawString(g, t, f, b, r, sf, 0, 0);
		}

		public static void DrawString(Graphics g, String t, Font f, Brush b, Rectangle r, StringFormat sf, int ox, int oy)
		{
			Rectangle rt = r;
			rt.Offset(ox, oy);
			g.DrawString(t, f, b, rt, sf);
		}
    }
}
