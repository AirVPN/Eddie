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
using System.Text;
using System.Windows.Forms;

namespace AirVPN.Gui.Skin
{
    public class ListView : System.Windows.Forms.ListView
    {
		public int ImageSpace = 2;
		public int TextSpace = 2;

		public String ImageIconResourcePrefix = "";
		public String ImageStateResourcePrefix = "";
        public ImageList ImageListIcon = null;
        public ImageList ImageListState = null;

        private int m_sortColumn = -1;
				
        public ListView()
        {
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
			
			FullRowSelect = true;
			GridLines = false;
			HideSelection = false;

			ColumnClick += new ColumnClickEventHandler(OnListViewColumnClick);

            OwnerDraw = true;
			this.DrawColumnHeader += new DrawListViewColumnHeaderEventHandler(OnListViewDrawColumnHeader);
			this.DrawSubItem += new DrawListViewSubItemEventHandler(OnListViewDrawSubItem);
		}
        
        public void ResizeColumnsAuto()
        {
            for(int c=0;c<Columns.Count;c++)
            {
                ResizeColumnAuto(c);
            }
        }

        public void ResizeColumnAuto(int column)
        {
            // It ignore Image if present, .Net bug.
            //AutoResizeColumn(column, ColumnHeaderAutoResizeStyle.ColumnContent);

            Graphics g = this.CreateGraphics();

            int minWidth = GuiUtils.GetFontSize(g, Font, Columns[column].Text).Width;

            foreach (ListViewItem item in Items)
            {
                if (column >= item.SubItems.Count)
                    continue;

                string t = "";
                if (column == 0)
                    t = item.Text;
                else
                    t = item.SubItems[column].Text;
                Size s = GuiUtils.GetFontSize(g, Font, t);
                if (minWidth == -1)
                    minWidth = s.Width;
                else if (minWidth < s.Width)
                    minWidth = s.Width;                    
            }
            g.Dispose();

            if ((ImageIconResourcePrefix != "") && (Items.Count > 0))
                minWidth += GetItemRect(0).Height*2;
            
            minWidth += Convert.ToInt32(minWidth * 0.2); // 20% margin

            Columns[column].Width = minWidth;
        }

        public void ResizeColumnMax(int column)
        {
            Columns[column].Width = 3000;
        }

        public void ResizeColumnString(int column, int charsLen)
        {
            ResizeColumnString(column, new string('W', charsLen));
        }

        public void ResizeColumnString(int column, string text)
        {
            Graphics g = this.CreateGraphics();
            Size s = GuiUtils.GetFontSize(g, Font, text);
            g.Dispose();

            s.Width += Convert.ToInt32(s.Width * 0.1);
            
            Columns[column].Width = s.Width;
        }
        				
		public void DrawSubItemBackground(object sender, DrawListViewSubItemEventArgs e)
		{
			Rectangle r = e.Bounds;

            Brush b = null;
            if (this.Enabled == false)
                b = Form.Skin.BackDisabledBrush;
            else
            {
                if (e.Item.Focused)
                    b = Form.Skin.ListViewFocusedBackBrush;
                else if (e.Item.Selected)
                    b = Form.Skin.ListViewSelectedBackBrush;
                else if (e.ItemIndex % 2 == 0)
                    b = Form.Skin.ListViewNormalBackBrush;
                else
                    b = Form.Skin.ListViewNormal2BackBrush;
            }

            e.Graphics.FillRectangle(b, r);

			e.Graphics.DrawRectangle(Form.Skin.ListViewGridPen, r);
		}
		
		public virtual void OnListViewDrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
		{
			e.DrawDefault = true;
		}

		public virtual void OnListViewDrawSubItem(object sender, DrawListViewSubItemEventArgs e)
		{
			e.DrawDefault = false;

			Form.Skin.GraphicsCommon(e.Graphics);
			
			DrawSubItemBackground(sender, e);
			
			if (e.ColumnIndex == 0)
			{
                // int middleY = (e.Bounds.Top + e.Bounds.Bottom) / 2; // TOCLEAN

				Rectangle r = e.Bounds;

				Image imageState = GuiUtils.GetResourceImage(ImageStateResourcePrefix + e.Item.StateImageIndex.ToString());
				if (imageState != null)
				{
					int imageWidth = r.Height;

					Rectangle rImage = r;
					rImage.X += ImageSpace;
					rImage.Width = imageWidth;

					e.Graphics.DrawImage(imageState, rImage);

					r.X += (imageWidth + ImageSpace);
					r.Width -= (imageWidth + ImageSpace);
				}
				
				Image imageIcon = GuiUtils.GetResourceImage(ImageIconResourcePrefix + e.Item.ImageKey);
                if((imageIcon == null) && (ImageListIcon != null) && (ImageListIcon.Images.ContainsKey(e.Item.ImageKey)))
                    imageIcon = ImageListIcon.Images[e.Item.ImageKey];
                else if ((imageIcon == null) && (e.Item.ListView.LargeImageList != null) && (e.Item.ListView.LargeImageList.Images.ContainsKey(e.Item.ImageKey)))
					imageIcon = e.Item.ListView.LargeImageList.Images[e.Item.ImageKey];
                else if ( (imageIcon == null) && (e.Item.ListView.SmallImageList != null) && (e.Item.ListView.SmallImageList.Images.ContainsKey(e.Item.ImageKey)) )
					imageIcon = e.Item.ListView.SmallImageList.Images[e.Item.ImageKey];
				if (imageIcon != null)
				{
                    //int imageWidth = r.Height;
                    int imageHeight = r.Height;
                    int imageWidth = (imageHeight * imageIcon.Width) / imageIcon.Height;

                    Rectangle rImage = r;
					rImage.X += ImageSpace;
					rImage.Width = imageWidth;

					e.Graphics.DrawImage(imageIcon, rImage);

					r.X += (imageWidth + ImageSpace);
					r.Width -= (imageWidth + ImageSpace);                    
                }

				r.X += TextSpace;
				r.Width -= TextSpace;

				e.Graphics.DrawString(e.Item.Text, e.Item.Font, Form.Skin.ForeBrush, r, GuiUtils.StringFormatLeftMiddle);
			}
			else				
			{
				Rectangle r = e.Bounds;
				r.X += TextSpace;
				r.Width -= TextSpace;
				
				StringFormat stringFormat = GuiUtils.StringFormatLeftMiddle;				
				if(e.Item.ListView.Columns[e.ColumnIndex].TextAlign == HorizontalAlignment.Center)
					stringFormat = GuiUtils.StringFormatCenterMiddle;
				else if(e.Item.ListView.Columns[e.ColumnIndex].TextAlign == HorizontalAlignment.Right)
					stringFormat = GuiUtils.StringFormatRightMiddle;				
				e.Graphics.DrawString(e.Item.SubItems[e.ColumnIndex].Text, e.Item.Font, Form.Skin.ForeBrush, r, stringFormat);
				
			}
		}
        
        public virtual void OnListViewColumnClick(object sender, ColumnClickEventArgs e)
		{
			if (e.Column == m_sortColumn)
			{
				// Determine what the last sort order was and change it.
				if (Sorting == SortOrder.Ascending)
					Sorting = SortOrder.Descending;
				else
					Sorting = SortOrder.Ascending;
			}

			SetSort(e.Column, Sorting);
		}

		public void SetSort(int col, SortOrder order)
		{
			m_sortColumn = col;
			Sorting = order;

			ListViewItemSorter = new ListViewItemComparer(m_sortColumn, Sorting);

			Sort();
		}

		public virtual int OnSortItem(int col, SortOrder order, ListViewItem i1, ListViewItem i2)
		{
			int returnVal = -1;
			returnVal = String.Compare(i1.SubItems[col].Text, i2.SubItems[col].Text);			
			// Determine whether the sort order is descending.
			if (order == SortOrder.Descending)
				// Invert the value returned by String.Compare.
				returnVal *= -1;
			return returnVal;
		}
    }
}