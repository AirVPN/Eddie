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

namespace Eddie.UI.Osx
{
	public class TableRoutingControllerItem
	{
		public string Ip;
		public string Action;
		public string Notes;
		public string Icon;
	}

	public class TableRoutingController : NSTableViewDataSource
	{
		public NSTableView tableView;

		public List<TableRoutingControllerItem> Items = new List<TableRoutingControllerItem>();

		public TableRoutingController (NSTableView tableView)
		{
			this.tableView = tableView;

			this.tableView.DataSource = this;

		}

		public void RefreshUI()
		{
			tableView.ReloadData ();
		}

		public override int GetRowCount (NSTableView tableView)
		{
			return Items.Count;
		}

		public override NSObject GetObjectValue (NSTableView tableView, 
		                                         NSTableColumn tableColumn, 
		                                         int row)
		{
			TableRoutingControllerItem i = Items [row];

			if (tableColumn.Identifier == "Icon") {
				if (i.Icon == "in")
					return NSImage.ImageNamed ("routes_in.png");
				else
					return NSImage.ImageNamed ("routes_out.png");
			}
			else if (tableColumn.Identifier == "Ip") {
				return new NSString (i.Ip);
			}
			else if (tableColumn.Identifier == "Action") {
				return new NSString (WindowPreferencesController.RouteDirectionToDescription(i.Action));
			}
			else if (tableColumn.Identifier == "Notes") {
				return new NSString (i.Notes);
			}
			else 
				throw new NotImplementedException (string.Format ("{0} is not recognized", tableColumn.Identifier));
		}

	}
}

