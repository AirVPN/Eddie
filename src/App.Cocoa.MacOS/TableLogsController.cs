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
using Foundation;
using AppKit;
using Eddie.Core;

namespace Eddie.UI.Cocoa.Osx
{
	public class TableLogsController : NSTableViewDataSource
	{
		public NSTableView tableView;

		private List<LogEntry> m_items = new List<LogEntry>();
		//private Engine m_engine;

		public TableLogsController(NSTableView tableView)
		{
			this.tableView = tableView;

			//m_engine = Eddie.Core.Engine.Instance as Engine;

			this.tableView.DataSource = this;

		}

		public void AddLog(LogEntry l)
		{
			m_items.Add(l);
			if (m_items.Count >= Engine.Instance.Options.GetInt("log.limit"))
				m_items.RemoveAt(0);
			RefreshUI();
		}

		public void Clear()
		{
			m_items.Clear();
			RefreshUI();
		}

		public string GetBody(bool selectedOnly)
		{
			System.Text.StringBuilder buffer = new System.Text.StringBuilder();

			lock (m_items)
			{
				int i = 0;
				foreach (LogEntry l in m_items)
				{
					bool skip = false;

					if (selectedOnly)
					{
						if (tableView.IsRowSelected(i) == false)
							skip = true;
					}

					if (skip == false)
					{
						buffer.Append(l.GetStringLines() + "\n");
					}

					i++;
				}
			}

			return Core.Platform.Instance.NormalizeString(buffer.ToString());
		}

		public void RefreshUI()
		{
			tableView.ReloadData();
		}

		public override nint GetRowCount(NSTableView tableView)
		{
			return m_items.Count;
		}

		public override NSObject GetObjectValue(NSTableView tableView,
												 NSTableColumn tableColumn,
												 nint row)
		{
			LogEntry e = m_items[(int)row];

			if (tableColumn.Identifier == "Icon")
			{
				return NSImage.ImageNamed("log_" + e.Type.ToString().ToLowerInvariant() + ".png");
			}
			else if (tableColumn.Identifier == "Date")
			{
				return new NSString(e.GetDateForList());
			}
			else if (tableColumn.Identifier == "Message")
			{
                string line = e.Message.Replace("\r", "").Replace("\n", " | ");
				return new NSString(line);
			}
			else
				throw new NotImplementedException(string.Format("{0} is not recognized", tableColumn.Identifier));
		}

	}
}

