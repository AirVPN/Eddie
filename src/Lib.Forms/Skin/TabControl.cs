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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace AirVPN.Gui.Skin
{
    /// <summary>
    /// Summary description for TabControl.
    /// </summary>
    public class TabControl : System.Windows.Forms.TabControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        //private System.ComponentModel.Container components = null;

        public TabControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            //InitializeComponent();

			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);            
			
        }
		
        protected override void OnPaintBackground(PaintEventArgs e)
        {            
            try
            {
                Form.Skin.GraphicsCommon(e.Graphics);

				//Form.DrawImage(e.Graphics, Form.Skin.FormBackgroundImage, ClientRectangle);
				//Form.DrawImage(e.Graphics, GuiUtils.GetResourceImage("tab_l_bg"), ClientRectangle);
				e.Graphics.FillRectangle(Form.Skin.TabBackgroundBrush, ClientRectangle);
				                
				if (SelectedTab != null)
				{
					Rectangle rPage = new Rectangle(0, GetTabRect(TabPages.IndexOf(SelectedTab)).Bottom, Right, Bottom);

					//e.Graphics.FillRectangle(new System.Drawing.SolidBrush(SelectedTab.BackColor), rPage);
					Form.DrawImage(e.Graphics, GuiUtils.GetResourceImage("tab_l_bg"), rPage);
				}
            }
            catch (Exception ex)
            {
                Core.Debug.Trace(ex);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {   
            //base.OnPaint(e);

            try
            {


                if (TabCount <= 0) return;
				if (SelectedIndex < 0) return;
                
				TabPage tp = TabPages[SelectedIndex] as TabPage;
				
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;

                // Draw Tabs Not Selected
                for (int index = 0; index <= TabCount - 1; index++)
                {
                    tp = TabPages[index] as TabPage;
                    Rectangle r = GetTabRect(index);
                    r.Y += 2;
                    r.Height -= 2;
                    //Utils.DrawImage(e.Graphics, Utils.GetResourceImage("tab_l_n"), r);
                    e.Graphics.FillRectangle(Form.Skin.TabButtonNormalBackBrush, r);

					Rectangle rShadow = r;
					rShadow.X = r.Right;
					rShadow.Width = 5;
					Form.DrawImage(e.Graphics, GuiUtils.GetResourceImage("tab_shadow"), rShadow);
                }

                // Draw Tab selected
                {                    
                    Rectangle r = GetTabRect(TabPages.IndexOf(SelectedTab));
                    r.Y += 2;
                    r.Height -= 2;
                    //Utils.DrawImage(e.Graphics, Utils.GetResourceImage("tab_l_s"), r);
                    e.Graphics.FillRectangle(Form.Skin.TabButtonSelectedBackBrush, r);
                }

                //Draw the Tabs Caption
                for (int index = 0; index <= TabCount - 1; index++)
                {
                    tp = TabPages[index] as TabPage;
                    Rectangle r = GetTabRect(index);
					                
                    

                    //Set up rotation for left and right aligned tabs
                    if (Alignment == TabAlignment.Left || Alignment == TabAlignment.Right)
                    {
                        float RotateAngle = 90;
                        if (Alignment == TabAlignment.Left) RotateAngle = 270;
                        PointF cp = new PointF(r.Left + (r.Width >> 1), r.Top + (r.Height >> 1));
                        e.Graphics.TranslateTransform(cp.X, cp.Y);
                        e.Graphics.RotateTransform(RotateAngle);
                        r = new Rectangle(-(r.Height >> 1), -(r.Width >> 1), r.Height, r.Width);                        
                    }

                    r.Offset(0, 1);

                    //Draw the Tab Text
                    if (tp.Enabled)
                    {
                        Brush FB = Form.Skin.TabButtonNormalForeBrush;
                        if (tp == SelectedTab)
                            FB = Form.Skin.TabButtonSelectedForeBrush;
                        //e.Graphics.DrawString(tp.Text, Font, PaintBrush, r, sf);
                        e.Graphics.DrawString(tp.Text, Font, FB, r, sf);
                    }
                    else
                        ControlPaint.DrawStringDisabled(e.Graphics, tp.Text, Font, tp.BackColor, r, sf);

                    e.Graphics.ResetTransform();
                }

                //PaintBrush.Dispose();
            }
            catch (Exception ex)
            {
                Core.Debug.Trace(ex);
            }
        }
    }

}