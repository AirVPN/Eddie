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
using System.Windows.Forms;
using Eddie.Core;
using Eddie.Common;

namespace Eddie.Forms.Controls
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

		// We use a separate dictionary, because ListView have poor performance internal dictionary.
		Dictionary<string, Controls.ListViewItemServer> ItemsServers = new Dictionary<string, ListViewItemServer>();

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
			columnHeader2.TextAlign = HorizontalAlignment.Center;
			columnHeader3.Text = Messages.ServersLocation;
			columnHeader3.Width = 200;
			columnHeader4.Text = Messages.ServersLatency;
			columnHeader4.Width = 60;
			columnHeader4.TextAlign = HorizontalAlignment.Center;
			columnHeader5.Text = Messages.ServersLoad;
			columnHeader5.Width = 160;
			columnHeader5.TextAlign = HorizontalAlignment.Center;
			columnHeader6.Text = Messages.ServersUsers;
			columnHeader6.Width = 50;
			columnHeader6.TextAlign = HorizontalAlignment.Center;

			ImageListIcon = (Engine.Instance as Eddie.Forms.Engine).FormMain.imgCountries;
			ImageListState = (Engine.Instance as Eddie.Forms.Engine).FormMain.imgCountries;
			//SmallImageList = (Engine.Instance as Gui.Engine).FormMain.imgCountries;
			//LargeImageList = (Engine.Instance as Gui.Engine).FormMain.imgCountries;

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

		public override bool GetDrawSubItemFull(int columnIndex)
		{
			if (columnIndex == 1)
				return false;
			else if (columnIndex == 4)
				return false;
			else
				return true;
		}

		public override Brush OnSubItemBrush(DrawListViewSubItemEventArgs e)
		{
			Controls.ListViewItemServer listItemServer = e.Item as Controls.ListViewItemServer;

			if (listItemServer.Info.CanConnect() == false)
				return Form.Skin.ListViewDisabledBackBrush;

			return null;
		}

		public override void OnListViewDrawSubItem(object sender, DrawListViewSubItemEventArgs e)
		{
			base.OnListViewDrawSubItem(sender, e);

			if (Visible == false)
				return;

			if (e.ColumnIndex == 1)
			{
				Controls.ListViewItemServer listItemServer = e.Item as Controls.ListViewItemServer;

				float part = listItemServer.Info.ScorePerc();

				Image imageN = GuiUtils.GetResourceImage("stars_n");
				Image imageH = GuiUtils.GetResourceImage("stars_h");

				Rectangle sourceH = new Rectangle(0, 0, Convert.ToInt32(Convert.ToDouble(imageH.Width) * part), imageH.Height);

				Form.DrawImageContain(e.Graphics, imageN, e.Bounds, 0);
				Form.DrawImageContain(e.Graphics, imageH, e.Bounds, 0, sourceH);
			}
			else if (e.ColumnIndex == 4)
			{
				Controls.ListViewItemServer listItemServer = e.Item as Controls.ListViewItemServer;

				Rectangle R1 = e.Bounds;
				R1.Inflate(-2, -2);

				/*
				Int64 bwCur = 2*(listItemServer.Info.Bandwidth*8)/(1000*1000); // to Mbit/s                
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



				int W = Convert.ToInt32(p * R1.Width);
				if (W > R1.Width)
					W = R1.Width;
				//e.Graphics.FillRectangle(Form.Skin.BarBrush, new Rectangle(R1.Left, R1.Top, W, R1.Height));
				Form.FillRectangle(e.Graphics, b, new Rectangle(R1.Left, R1.Top, W, R1.Height));

				R1.Height -= 1;
				//e.Graphics.DrawRectangle(m_loadPen, R1);
				Form.DrawString(e.Graphics, label, e.Item.Font, Form.Skin.ForeBrush, R1, GuiUtils.StringFormatCenterMiddle);
			}
		}

		public override int OnSortItem(int col, SortOrder order, ListViewItem pi1, ListViewItem pi2)
		{
			ListViewItemServer i1 = pi1 as ListViewItemServer;
			ListViewItemServer i2 = pi2 as ListViewItemServer;

			string colName = "";
			if (col == 0)
				colName = "Name";
			else if (col == 1)
				colName = "Score";
			else if (col == 2)
				colName = "Location";
			else if (col == 3)
				colName = "Latency";
			else if (col == 4)
				colName = "Load";
			else if (col == 5)
				colName = "Users";

			int r = i1.Info.CompareToEx(i2.Info, colName, order == SortOrder.Ascending);
			return r;

			/*
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
            return returnVal;   
			*/
		}

		public void UpdateList()
		{
			lock (this)
			{
				//SuspendLayout();

				List<ConnectionInfo> serversList;
				lock (Engine.Instance.Connections)
				{
					serversList = Engine.Instance.GetConnections(ShowAll);
				}

				Dictionary<string, ConnectionInfo> servers = new Dictionary<string, ConnectionInfo>();
				foreach (ConnectionInfo infoServer in serversList)
				{
					servers[infoServer.Code] = infoServer;
				}

				foreach (ConnectionInfo infoServer in servers.Values)
				{
					if (ItemsServers.ContainsKey(infoServer.Code) == false)
					{
						Controls.ListViewItemServer listItemServer = new Controls.ListViewItemServer();
						listItemServer.Info = infoServer;
						listItemServer.Update();
						ItemsServers.Add(infoServer.Code, listItemServer);
						Items.Add(listItemServer); // TOFIX: Mono impressive slow. 'Show all' took around 5 seconds to add 150 items.                        
					}
					else
					{
						Controls.ListViewItemServer listItemServer = ItemsServers[infoServer.Code] as ListViewItemServer;
						listItemServer.Update();
					}
				}

				List<ListViewItemServer> itemsToRemove = new List<ListViewItemServer>();

				foreach (ListViewItemServer viewItem in ItemsServers.Values)
				{
					if (servers.ContainsKey(viewItem.Info.Code) == false)
					{
						itemsToRemove.Add(viewItem);
					}
				}

				if (itemsToRemove.Count > 0)
				{
					if (Platform.IsWindows())
					{
						foreach (ListViewItemServer viewItem in itemsToRemove)
						{
							Items.Remove(viewItem);
							ItemsServers.Remove(viewItem.Info.Code);
						}
					}
					else
					{
						// Mono workaround to avoid a crash, like this: http://sourceforge.net/p/keepass/bugs/1314/
						// Reproduce the crash by whitelist some server and switch "Show all" continuosly.
						List<ListViewItemServer> items = new List<ListViewItemServer>();
						foreach (ListViewItemServer itemCurrent in Items)
						{
							if (itemsToRemove.Contains(itemCurrent) == false)
								items.Add(itemCurrent);
						}
						Items.Clear();
						ItemsServers.Clear();
						foreach (ListViewItemServer itemCurrent in items)
						{
							ItemsServers.Add(itemCurrent.Info.Code, itemCurrent);
							Items.Add(itemCurrent);
						}
					}
				}

				Sort();

				//ResumeLayout();
			}
		}
	}
}
