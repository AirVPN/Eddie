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
    public class ListViewServers : Skin.ListView
    {
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;

		public bool ShowAll;
        
        public ListViewServers()
        {
			this.DoubleBuffered = true;
			
            columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));

            MultiSelect = true;
            Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader1,
            columnHeader2,
            columnHeader3,
            columnHeader4,
            columnHeader5,
            columnHeader6});
            
            Name = "lstServers";
            UseCompatibleStateImageBehavior = false;
            View = System.Windows.Forms.View.Details;


			columnHeader1.Text = Messages.ServersName;
            columnHeader1.Width = 110;
			columnHeader2.Text = Messages.ServersScore;
            columnHeader2.Width = 66;
			columnHeader3.Text = Messages.ServersLocation;
            columnHeader3.Width = 200;
			columnHeader4.Text = Messages.ServersLatency;
            columnHeader4.Width = 60;
            columnHeader4.TextAlign = HorizontalAlignment.Center;
			columnHeader5.Text = Messages.ServersLoad;
            columnHeader5.Width = 160;
			columnHeader6.Text = Messages.ServersUsers;
            columnHeader6.Width = 50;
            columnHeader6.TextAlign = HorizontalAlignment.Center;


			SmallImageList = (Engine.Instance as Gui.Engine).FormMain.imgCountries;
			LargeImageList = (Engine.Instance as Gui.Engine).FormMain.imgCountries;
			
            //Dock = DockStyle.Fill;
            Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            HeaderStyle = ColumnHeaderStyle.Clickable;

            SetSort(1, SortOrder.Ascending);

			ImageIconResourcePrefix = "flags_";
			ImageStateResourcePrefix = "blacklist_";

            UpdateList();
        }
						
		public override void OnListViewDrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
			if (e.ColumnIndex == 1)
            {
				e.DrawDefault = false;
				DrawSubItemBackground(sender, e);
				                
                int score = Convert.ToInt32(e.SubItem.Text);
                float scoreF = (score - 50);
                scoreF /= 50;

                float part = 1;
                if (scoreF > 5)
                    part = 0;
                else if (scoreF > 1)
                    part /= scoreF;


				Image imageN = GuiUtils.GetResourceImage("stars_n");
				Image imageH = GuiUtils.GetResourceImage("stars_h");

                Rectangle sourceN = new Rectangle(0,0,imageN.Width,imageN.Height);
                Rectangle sourceH = new Rectangle(0,0,Convert.ToInt32(Convert.ToDouble(imageH.Width)*part),imageH.Height);

                int ODX = imageN.Width;
                if(e.Bounds.Width<ODX)
                    ODX = e.Bounds.Width;
                int ODY = imageN.Height;

                int HDX = Convert.ToInt32(Convert.ToDouble(ODX)*part);

                Rectangle destN = new Rectangle(0,0,ODX,ODY);
                destN.Offset(e.Bounds.Width/2,e.Bounds.Height/2);
                destN.Offset(e.Bounds.Left,e.Bounds.Top);
                destN.Offset(-ODX / 2, -ODY / 2);

                Rectangle destH = new Rectangle(0,0,HDX,ODY);
                destH.Offset(e.Bounds.Width/2,e.Bounds.Height/2);
                destH.Offset(e.Bounds.Left,e.Bounds.Top);
                destH.Offset(-ODX / 2, -ODY / 2);

                e.Graphics.DrawImage(imageN, destN);
                e.Graphics.DrawImage(imageH, destH, sourceH, GraphicsUnit.Pixel );				
            }
            else if (e.ColumnIndex == 4)
            {
				e.DrawDefault = false;
				DrawSubItemBackground(sender, e);
				                
                Controls.ListViewItemServer listItemServer = e.Item as Controls.ListViewItemServer;

                Rectangle R1 = e.Bounds;
                R1.Inflate(-2, -2);
                
                Int64 bwCur = 2*(listItemServer.Info.Bandwidth*8)/(1000*1000); // to Mbit/s                
                Int64 bwMax = listItemServer.Info.BandwidthMax;
                
                String label = bwCur.ToString() + " / " + bwMax.ToString() + " Mbit/s";

                int W = Convert.ToInt32((bwCur * R1.Width) / bwMax);
                if (W > R1.Width)
                    W = R1.Width;
				e.Graphics.FillRectangle(Form.Skin.BarBrush, new Rectangle(R1.Left, R1.Top, W, R1.Height));

                R1.Height -= 1;
                //e.Graphics.DrawRectangle(m_loadPen, R1);
				e.Graphics.DrawString(label, e.Item.Font, Form.Skin.ForeBrush, R1, GuiUtils.StringFormatCenterMiddle);                
            }
			else
				base.OnListViewDrawSubItem(sender, e);
        }

        public override int OnSortItem(int col, SortOrder order, ListViewItem pi1, ListViewItem pi2)
        {			
            ListViewItemServer i1 = pi1 as ListViewItemServer;
            ListViewItemServer i2 = pi2 as ListViewItemServer;

            int returnVal = -1;
            switch (col)
            {
                case 1: // Score
                    {
                        int v1 = i1.Info.Score();
                        int v2 = i2.Info.Score();
                        returnVal = v1.CompareTo(v2);						
                    } break;
                case 3: // Latency
                    {
                        long v1 = i1.Info.Ping;
                        if (v1 == -1) 
                            v1 = long.MaxValue;
                        long v2 = i2.Info.Ping;
                        if (v2 == -1)
                            v2 = long.MaxValue;
                        returnVal = v1.CompareTo(v2);
                    } break;
                case 4: // Load
                    {
                        Int64 bwCur1 = 2 * (i1.Info.Bandwidth * 8) / (1000 * 1000);
                        Int64 bwMax1 = i1.Info.BandwidthMax;
                        int v1 = Convert.ToInt32((bwCur1 * 100) / bwMax1);

                        Int64 bwCur2 = 2 * (i2.Info.Bandwidth * 8) / (1000 * 1000);
                        Int64 bwMax2 = i2.Info.BandwidthMax;
                        int v2 = Convert.ToInt32((bwCur2 * 100) / bwMax2);

                        returnVal = v1.CompareTo(v2);

                    } break;
                case 5:
                    {
                        returnVal = i1.Info.Users.CompareTo(i2.Info.Users);
                    } break;
                default:
                    {
						returnVal = base.OnSortItem(col, SortOrder.Ascending, pi1, pi2);
						//order = SortOrder.Ascending;
                    } break;
            }

            if (order == SortOrder.Descending)
                // Invert the value returned by String.Compare.
                returnVal *= -1;

			if (returnVal == 0) // Second order, Name
				returnVal = i1.Info.Name.CompareTo(i2.Info.Name);

            return returnVal;            
        }

        public void UpdateList()
        {
            lock (this)
            {
                //SuspendLayout();

                List<ServerInfo> servers;
                lock (Engine.Instance.Servers)
                {
                    servers = Engine.Instance.GetServers(ShowAll);
                }

                foreach (ServerInfo infoServer in servers)
                {
                    if (Items.ContainsKey(infoServer.Name) == false)
                    {
                        Controls.ListViewItemServer listItemServer = new Controls.ListViewItemServer();
                        listItemServer.Name = infoServer.Name;
                        listItemServer.Info = infoServer;
                        listItemServer.Update();
                        Items.Add(listItemServer);
                    }
                    else
                    {
                        Controls.ListViewItemServer listItemServer = Items[infoServer.Name] as ListViewItemServer;
                        listItemServer.Update();
                    }
                }
								
				List<ListViewItemServer> itemsToRemove = new List<ListViewItemServer>();

                foreach (ListViewItemServer viewItem in Items)
                {
					if (servers.Contains(viewItem.Info) == false)
					{
						itemsToRemove.Add(viewItem);						
					}					
                }

				foreach (ListViewItemServer viewItem in itemsToRemove)
				{
					Items.Remove(viewItem);					
				}
				
                Sort();

                //ResumeLayout();
            }
        }
    }
}
