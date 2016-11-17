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
using MonoMac.Foundation;
using MonoMac.AppKit;
using Eddie.Core;

namespace Eddie.UI.Cocoa.Osx
{
	public class TableAreasController : NSTableViewDataSource
	{
		public NSTableView tableView;

		public List<AreaInfo> Items = new List<AreaInfo>();

		private bool m_orderAscending = false;
		private string m_orderColumn = "";

		public TableAreasController (NSTableView tableView)
		{
			this.tableView = tableView;

			this.tableView.DataSource = this;

		}

		public void SortByColumn(string col)
		{
			string oldOrderColumn = m_orderColumn;
			m_orderColumn = col;
			if(oldOrderColumn == m_orderColumn)
			{
				m_orderAscending = !m_orderAscending;
			}

			RefreshUI ();
		}

		public void RefreshUI()
		{
			List<AreaInfo> selected = new List<AreaInfo> ();
			for (int r=0; r<Items.Count; r++) {
				if (tableView.IsRowSelected (r))
					selected.Add (Items [r]);
			}

			tableView.DeselectAll (this);

			Items.Clear ();
			lock (Engine.Instance.Areas) {
				foreach (AreaInfo a in Engine.Instance.Areas.Values)
					Items.Add (a);
			}

			// Sorting
			Items.Sort (
				delegate(AreaInfo x, AreaInfo y) {

				return x.CompareToEx (y, m_orderColumn, m_orderAscending);
			});

			int r2 = 0;
			foreach (AreaInfo a in Items) {
				if (selected.Contains (a))
					tableView.SelectRow (r2, true);
				r2++;
			}

			tableView.ReloadData ();
		}

		public AreaInfo GetRelatedItem(int i)
		{
			return Items [i];
		}

		public override int GetRowCount (NSTableView tableView)
		{
			return Items.Count;
		}

		public override NSObject GetObjectValue (NSTableView tableView, 
		                                         NSTableColumn tableColumn, 
		                                         int row)
		{
			AreaInfo a = Items[row];

			if (tableColumn.Identifier == "List") {
				if(a.UserList == AreaInfo.UserListType.WhiteList)
					return NSImage.ImageNamed("blacklist_0.png");
				else if(a.UserList == AreaInfo.UserListType.BlackList)
					return NSImage.ImageNamed("blacklist_1.png");
				else
					return NSImage.ImageNamed("blacklist_2.png");
			} else if (tableColumn.Identifier == "Flag") {
				return NSImage.ImageNamed("flag_" + a.Code.ToLowerInvariant() + ".png");
			} else if (tableColumn.Identifier == "Name") {
				return new NSString (a.Name);
			} else if (tableColumn.Identifier == "Servers") {
				return new NSString (a.Servers.ToString());
			} else if (tableColumn.Identifier == "LoadIcon") {
				return NSImage.ImageNamed("status_" + a.GetLoadColorForList().ToLowerInvariant() + ".png");				
			} else if (tableColumn.Identifier == "Load") {
				return new NSString (a.GetLoadForList());
			} else if (tableColumn.Identifier == "Users") {
				return new NSString (a.Users.ToString());
			} else 
				throw new NotImplementedException (string.Format ("{0} is not recognized", tableColumn.Identifier));
		}

	}
}

