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
using AirVPN.Core;

namespace AirVPN.Gui.Controls
{
    public class ListViewAreas : Skin.ListView
    {
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;

		public ListViewAreas()
        {
            // Activate double buffering			
			this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            
            MultiSelect = true;
            Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader1,
            columnHeader2,
            columnHeader3,
            columnHeader4});
            
            Name = "lstAreas";
            UseCompatibleStateImageBehavior = false;
            View = System.Windows.Forms.View.Details;
            //this.lstServers.SelectedIndexChanged += new System.EventHandler(this.lstServers_SelectedIndexChanged);
            
            columnHeader1.Text = Messages.AreasName;
            columnHeader1.Width = 150;
			columnHeader2.Text = Messages.AreasServers;
            columnHeader2.Width = 60;
            columnHeader2.TextAlign = HorizontalAlignment.Center;
			columnHeader3.Text = Messages.AreasLoad;
            columnHeader3.Width = 200;
			columnHeader3.TextAlign = HorizontalAlignment.Center;
			columnHeader4.Text = Messages.AreasUsers;
            columnHeader4.Width = 60;
            columnHeader4.TextAlign = HorizontalAlignment.Center;

			SmallImageList = (Engine.Instance as Gui.Engine).FormMain.imgCountries;
			LargeImageList = (Engine.Instance as Gui.Engine).FormMain.imgCountries;

            Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            HeaderStyle = ColumnHeaderStyle.Clickable;

            SetSort(0, SortOrder.Ascending);

			ImageIconResourcePrefix = "flags_";
			ImageStateResourcePrefix = "blacklist_";

            UpdateList();
        }

		public override void OnListViewDrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {			
			if (e.ColumnIndex == 2)
			{
				e.DrawDefault = false;
				DrawSubItemBackground(sender, e);

				Controls.ListViewItemArea listItemServer = e.Item as Controls.ListViewItemArea;

				Rectangle R1 = e.Bounds;
				R1.Inflate(-2, -2);
				//e.Graphics.FillRectangle(m_loadBrushBg, R1);

				/*
				Int64 bwCur = 2 * (listItemServer.Info.Bandwidth * 8) / (1000 * 1000); // to Mbit/s                
				Int64 bwMax = listItemServer.Info.BandwidthMax;

				float p = (float)bwCur / (float)bwMax;
				*/

				String label = listItemServer.Info.GetLoadForList();
				float p = listItemServer.Info.GetLoadPercForList();
				string color = listItemServer.Info.GetLoadColorForList();
				Brush b = Brushes.LightGreen;
				if (color == "red")
					b = Brushes.LightPink;
				else if (color == "yellow")
					b = Brushes.LightYellow;
				else
					b = Brushes.LightGreen;

				int W = Conversions.ToInt32(p * R1.Width);
				if (W > R1.Width)
					W = R1.Width;
				e.Graphics.FillRectangle(b, new Rectangle(R1.Left, R1.Top, W, R1.Height));

				R1.Height -= 1;
				//e.Graphics.DrawRectangle(m_loadPen, R1);
				e.Graphics.DrawString(label, e.Item.Font, Form.Skin.ForeBrush, R1, GuiUtils.StringFormatCenterMiddle);
			}
			else
				base.OnListViewDrawSubItem(sender, e);
        }

        public override int OnSortItem(int col, SortOrder order, ListViewItem pi1, ListViewItem pi2)
        {
            ListViewItemArea i1 = pi1 as ListViewItemArea;
            ListViewItemArea i2 = pi2 as ListViewItemArea;

			string colName = "";
			if (col == 0)
				colName = "Name";
			else if (col == 1)
				colName = "Servers";
			else if (col == 2)
				colName = "Load";
			else if (col == 3)
				colName = "Users";

			return i1.Info.CompareToEx(i2.Info, colName, order == SortOrder.Ascending);

			/*
            int returnVal = -1;
            switch (col)
            {                
                case 1:
                    {
                        returnVal = i1.Info.Servers.CompareTo(i2.Info.Servers);
                    } break;
				case 2: // Load
					{
						Int64 bwCur1 = 2 * (i1.Info.Bandwidth * 8) / (1000 * 1000);
						Int64 bwMax1 = i1.Info.BandwidthMax;
						int v1 = Conversions.ToInt32((bwCur1 * 100) / bwMax1);

						Int64 bwCur2 = 2 * (i2.Info.Bandwidth * 8) / (1000 * 1000);
						Int64 bwMax2 = i2.Info.BandwidthMax;
						int v2 = Conversions.ToInt32((bwCur2 * 100) / bwMax2);

						returnVal = v1.CompareTo(v2);

					} break;
                case 3:
                    {
                        returnVal = i1.Info.Users.CompareTo(i2.Info.Users);
                    } break;
                default:
                    {
                        return base.OnSortItem(col, order, pi1, pi2);
                    }
            }

            if (order == SortOrder.Descending)
                // Invert the value returned by String.Compare.
                returnVal *= -1;

            return returnVal;
			*/
        }

        public void UpdateList()
        {
            //SuspendLayout();

            List<AreaInfo> areas = new List<AreaInfo>();
            lock (Engine.Instance.Areas)
            {
                foreach (AreaInfo infoArea in Engine.Instance.Areas.Values)
                    areas.Add(infoArea);
            }

            foreach (AreaInfo infoArea in areas)
            {
                if (Items.ContainsKey(infoArea.Code) == false)
                {
                    Controls.ListViewItemArea listItemArea = new Controls.ListViewItemArea();
                    listItemArea.Name = infoArea.Code;
                    listItemArea.Info = infoArea;
                    listItemArea.Update();
                    Items.Add(listItemArea);
                }
                else
                {
                    Controls.ListViewItemArea listItemArea = Items[infoArea.Code] as ListViewItemArea;
                    listItemArea.Update();
                }
            }

            foreach (ListViewItemArea viewItem in Items)
            {
				if (areas.Contains(viewItem.Info) == false)
				{
					Items.Remove(viewItem);					
				}
            }

            //ResumeLayout();
        }
    }
}
