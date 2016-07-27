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
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Eddie.Core;
using Eddie.Core.UI;
using Eddie.Gui.Forms; // Temp

namespace Eddie.Gui.Controls
{	
    public class TabNavigatorPage : System.Windows.Forms.Control
    {
        public string Icon = "";
        
        public TabNavigatorPage()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }
    }

    public class TabNavigator : System.Windows.Forms.Control
    {
        public Font TabsFont;
        
        private int m_hoverItem = -1;
        private int m_selectedItem = -1;
        private int m_heightItem = 0;
        private int m_navigatorWidth = 0;
        private int m_distanceIcon = 4;
        private int m_insetNotSelected = 20;
        private int m_insetSelected = 5;
        private int m_paddingLeft = 5;

        public List<TabNavigatorPage> Pages = new List<TabNavigatorPage>();

        public delegate void TabSwitchHandler();
        public event TabSwitchHandler TabSwitch;

        public TabNavigator()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);            
        }

        public int GetTabCount()
        {
            return Pages.Count;
        }

        public Font GetTabsFont()
        {
            if (TabsFont != null)
                return TabsFont;
            else
                return Font;
        }

        public void ImportTabControl(TabControl tabControl)
        {            
            this.SuspendLayout();
            tabControl.SuspendLayout();
            
            Pages.Clear();
            foreach(TabPage tabPage in tabControl.TabPages)
            {
                TabNavigatorPage page = new TabNavigatorPage();
                page.Text = tabPage.Text;                
                page.Visible = false;
                page.Left = 0;
                page.Top = 0;
                page.Width = tabPage.Width;
                page.Height = tabPage.Height;
                page.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
                page.BackColor = Color.Transparent;                
                Controls.Add(page);

                Pages.Add(page);
                
                /*
                List<Control> ctls = new List<Control>();
                foreach (Control c in tabPage.Controls)
                {
                    ctls.Add(c);                    
                }
                tabPage.Controls.Clear();

                foreach (Control c in ctls)
                    page.Controls.Add(c);
                */

            for (;tabPage.Controls.Count > 0;)
                {
                    Control ctlChild = tabPage.Controls[0];
                    
                    tabPage.Controls.Remove(ctlChild);
                    page.Controls.Add(ctlChild);
                } 
                                
            }
            //tabControl.Parent.Controls.Remove(this);
            tabControl.Visible = false;
            //this.Visible = false;

            ComputeSizes();
            
            SelectTab(0);

            this.ResumeLayout();
            tabControl.ResumeLayout();            
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            ComputeSizes();

            CheckSizes();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            if (m_hoverItem != -1)
                SelectTab(m_hoverItem);            
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
                        
            Form.FillRectangle(e.Graphics, Form.Skin.GetBrush("color.tab.tabs.background"), ClientRectangle);
            Form.FillRectangle(e.Graphics, Form.Skin.GetBrush("color.tab.page.background"), GetPageRect());
            Form.DrawImageOpt(e.Graphics, GuiUtils.GetResourceImage("form"), ClientRectangle);

            for (int t=0;t< GetTabCount(); t++)
            {
                Rectangle r = GetItemRect(t);

                string colorName = "normal";

                if (t == m_selectedItem)
                    colorName = "selected";
                else if (t == m_hoverItem)
                    colorName = "hover";

                Form.FillRectangle(e.Graphics, Form.Skin.GetBrush("color.tab." + colorName + ".background"), r);

                //Form.DrawRectangle(e.Graphics, Form.Skin.GetPen("color.tab.tabs.border"), r);

                Form.DrawLine(e.Graphics, Form.Skin.GetPen("color.tab.tabs.border"), new Point(r.Left, r.Top), new Point(r.Right-1, r.Top));
                Form.DrawLine(e.Graphics, Form.Skin.GetPen("color.tab.tabs.border"), new Point(r.Left, r.Top), new Point(r.Left, r.Bottom));
                Form.DrawLine(e.Graphics, Form.Skin.GetPen("color.tab.tabs.border"), new Point(r.Left, r.Bottom), new Point(r.Right-1, r.Bottom));

                Rectangle rText = r;

                rText.X += m_paddingLeft;
                rText.Width -= m_paddingLeft*2;

                if (Pages[t].Icon != "")
                {
                    int imageDx = r.Height;
                    int imageDy = r.Height;
                    Image icon = GuiUtils.GetResourceImage(Pages[t].Icon);
                    Form.DrawImageContain(e.Graphics, icon, new Rectangle(rText.Right - imageDx, rText.Top, imageDx, imageDy), 20);

                    rText.Width -= imageDx;
                    rText.Width -= m_distanceIcon;                    
                }

                Form.DrawString(e.Graphics, Pages[t].Text, GetTabsFont(), Form.Skin.GetBrush("color.tab." + colorName + ".foreground"), rText, GuiUtils.StringFormatRightMiddle);                
            }            
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            int iHoverItem = -1;
            for (int t = 0; t < GetTabCount(); t++)
            {
                Rectangle r = GetItemRect(t);

                if (r.Contains(e.Location))
                { 
                    iHoverItem = t;
                    break;
                }
            }

            if(iHoverItem != m_hoverItem)
            {
                m_hoverItem = iHoverItem;
                Invalidate(GetTabsRect());
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            m_hoverItem = -1;
            Invalidate(GetTabsRect());
        }

        public void ComputeSizes()
        {
            Graphics g = this.CreateGraphics();

            int navDx = -1;
            int itemDy = -1;

            bool withIcons = false;

            foreach (TabNavigatorPage tabPage in Pages)
            {
                Size tabSize = GuiUtils.GetFontSize(g, GetTabsFont(), tabPage.Text);
                if (navDx == -1)
                    navDx = tabSize.Width;
                else if (navDx < tabSize.Width)
                    navDx = tabSize.Width;
                if (itemDy == -1)
                    itemDy = tabSize.Height;
                else if (itemDy < tabSize.Height)
                    itemDy = tabSize.Height;
                if (tabPage.Icon != null)
                    withIcons = true;
            }
            g.Dispose();

            // Extend itemDy
            itemDy += itemDy/2;

            navDx += m_paddingLeft*2;

            if (withIcons)
                navDx += itemDy + m_distanceIcon;
            navDx += m_insetNotSelected;

            m_navigatorWidth = navDx;
            m_heightItem = itemDy;            
        }

        public void CheckSizes()
        {
            if (m_selectedItem == -1) return;

            Control tabPageControl = Pages[m_selectedItem];
            Rectangle tabPageRectangle = GetPageRect();
            if (tabPageControl.Bounds != tabPageRectangle)
            {
                tabPageControl.Bounds = tabPageRectangle;
                /*
                tabPageControl.Left = tabPageRectangle.Left;
                tabPageControl.Top = tabPageRectangle.Top;
                tabPageControl.Width = tabPageRectangle.Width;
                tabPageControl.Height = tabPageRectangle.Height;
                */
            }
        }

        public void SelectTab(int index)
        {
            if (m_selectedItem == index)
                return;

            if (m_selectedItem != -1)
                Pages[m_selectedItem].Visible = false;

            m_selectedItem = index;            

            Control tabPageControl = Pages[m_selectedItem];
            Rectangle tabPageRectangle = GetPageRect();
            CheckSizes();

            tabPageControl.Visible = true;            

            if (TabSwitch != null)
                TabSwitch();

            Invalidate(GetTabsRect());

            if (Platform.IsUnix())
            {
                Skin.ListView.WorkaroundMonoListViewHeadersBug(tabPageControl);                
            }
        }

        public Rectangle GetTabsRect()
        {
            return new Rectangle(0, 0, m_navigatorWidth, ClientSize.Height);
        }
                
        public Rectangle GetPageRect()
        {
            return new Rectangle(m_navigatorWidth, 0, ClientSize.Width - m_navigatorWidth, ClientSize.Height);
        }

        private Rectangle GetItemRect(int index)
        {
            int iDx = m_navigatorWidth;
            int iDy = m_heightItem;

            int startY = 0;
            startY = ClientRectangle.Height / 2 - m_heightItem * Pages.Count / 2;

            int x = 0;
            if (index != m_selectedItem)
                x += m_insetNotSelected;
            else
                x += m_insetSelected;

            return new Rectangle(x, startY + index * iDy, iDx-x, iDy);
        }
    }
}
