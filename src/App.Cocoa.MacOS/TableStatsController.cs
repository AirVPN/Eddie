// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2023 AirVPN (support@airvpn.org) / https://airvpn.org )
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
//using Foundation;
//using AppKit;
using Foundation;
using AppKit;
using Eddie.Core;

namespace Eddie.UI.Cocoa.Osx
{
	public class TableStatsController : NSTableViewDataSource
	{
		public NSTableView tableView;

		public TableStatsController(NSTableView tableView)
		{
			this.tableView = tableView;

			this.tableView.DataSource = this;

		}

		public void RefreshUI()
		{
			tableView.ReloadData();
		}

		public void DoubleClickItem()
		{
			nint i = tableView.SelectedRow;
			if ((i >= 0) && (i < Engine.Instance.Stats.List.Count))
			{
				StatsEntry e = Engine.Instance.Stats.List[(int)tableView.SelectedRow];

				Core.UI.App.OpenStats(e.Key.ToLowerInvariant());
			}
		}

		public override nint GetRowCount(NSTableView tableView)
		{
			return Engine.Instance.Stats.List.Count;
		}

		public override NSObject GetObjectValue(NSTableView tableView,
												 NSTableColumn tableColumn,
												 nint row)
		{
			StatsEntry e = Engine.Instance.Stats.List[(int)row];

			if (tableColumn.Identifier == "Icon")
			{
				return NSImage.ImageNamed("stats_" + e.Icon.ToLowerInvariant() + ".png");
			}
			else if (tableColumn.Identifier == "Key")
			{
				return new NSString(e.Caption);
			}
			else if (tableColumn.Identifier == "Value")
			{
				return new NSString(e.Text);
			}
			else
				throw new NotImplementedException(string.Format("{0} is not recognized", tableColumn.Identifier));
		}

	}
}

