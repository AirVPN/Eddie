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
using MonoMac.Foundation;
using MonoMac.AppKit;
using AirVPN.Core;

namespace AirVPN.UI.Osx
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

		private List<TableAdvancedEventsControllerItem> m_items = new List<TableAdvancedEventsControllerItem>();

		public TableAdvancedEventsController (NSTableView tableView)
		{
			this.tableView = tableView;

			m_items.Add(new TableAdvancedEventsControllerItem("App Start"));
			m_items.Add(new TableAdvancedEventsControllerItem("App End"));
			m_items.Add(new TableAdvancedEventsControllerItem("Session Start"));
			m_items.Add(new TableAdvancedEventsControllerItem("Session End"));
			m_items.Add(new TableAdvancedEventsControllerItem("VPN Pre"));
			m_items.Add(new TableAdvancedEventsControllerItem("VPN Up"));
			m_items.Add(new TableAdvancedEventsControllerItem("VPN Down"));

			this.tableView.DataSource = this;

		}

		public void RefreshUI()
		{
			tableView.ReloadData ();
		}

		public override int GetRowCount (NSTableView tableView)
		{
			return m_items.Count;
		}

		public override NSObject GetObjectValue (NSTableView tableView, 
		                                         NSTableColumn tableColumn, 
		                                         int row)
		{
			TableAdvancedEventsControllerItem i = m_items [row];

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
				if (i.WaitEnd)
					return NSImage.ImageNamed ("bool_true.png");
				else
					return NSImage.ImageNamed ("bool_false.png");
			}

			else 
				throw new NotImplementedException (string.Format ("{0} is not recognized", tableColumn.Identifier));
		}

	}
}

