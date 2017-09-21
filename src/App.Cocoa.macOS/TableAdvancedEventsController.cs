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
	public class TableAdvancedEventsControllerItem 
	{
		public string Title = "";
		public string Filename = "";
		public string Arguments = "";
		public bool WaitEnd = true;

		public TableAdvancedEventsControllerItem(string title)
		{
			Title = title;
		}
	}

	public class TableAdvancedEventsController : NSTableViewDataSource
	{
		public NSTableView tableView;

		public List<TableAdvancedEventsControllerItem> Items = new List<TableAdvancedEventsControllerItem>();

		public TableAdvancedEventsController (NSTableView tableView)
		{
			this.tableView = tableView;

			Items.Add(new TableAdvancedEventsControllerItem("App Start"));
			Items.Add(new TableAdvancedEventsControllerItem("App End"));
			Items.Add(new TableAdvancedEventsControllerItem("Session Start"));
			Items.Add(new TableAdvancedEventsControllerItem("Session End"));
			Items.Add(new TableAdvancedEventsControllerItem("VPN Pre"));
			Items.Add(new TableAdvancedEventsControllerItem("VPN Up"));
			Items.Add(new TableAdvancedEventsControllerItem("VPN Down"));

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
			TableAdvancedEventsControllerItem i = Items [row];

			if (tableColumn.Identifier == "Event") {
				return new NSString (i.Title);
			}
			else if (tableColumn.Identifier == "FileName") {
				return new NSString (i.Filename);
			}
			else if (tableColumn.Identifier == "Arguments") {
				return new NSString (i.Arguments);
			}
			else if (tableColumn.Identifier == "WaitEnd") {
				if ((i.Filename.Trim () != "") || (i.Arguments.Trim () != "")) {
					if (i.WaitEnd)
						return NSImage.ImageNamed ("status_green_16.png");
					else
						return NSImage.ImageNamed ("status_red_16.png");
				} else
					return NSImage.ImageNamed ("status_unknown.png");
			}

			else 
				throw new NotImplementedException (string.Format ("{0} is not recognized", tableColumn.Identifier));
		}

	}
}

