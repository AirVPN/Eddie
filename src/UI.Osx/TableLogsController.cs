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
	public class TableLogsController : NSTableViewDataSource
	{
		public NSTableView tableView;

		private List<LogEntry> m_items = new List<LogEntry>();
		//private Engine m_engine;

		public TableLogsController (NSTableView tableView)
		{
			this.tableView = tableView;

			//m_engine = AirVPN.Core.Engine.Instance as Engine;

			this.tableView.DataSource = this;

		}

		public void AddLog(LogEntry l)
		{
			m_items.Add (l);
			if (m_items.Count >= Engine.Instance.Storage.GetInt ("gui.log_limit"))
				m_items.RemoveAt (0);
			RefreshUI ();
		}

		public void Clear()
		{
			m_items.Clear ();
			RefreshUI ();
		}

		public string GetBody(bool selectedOnly)
		{
			System.Text.StringBuilder buffer = new System.Text.StringBuilder ();

			lock(m_items)
			{
				int i = 0;
				foreach (LogEntry l in m_items) {
					bool skip = false;

					if (selectedOnly) {
						if (tableView.IsRowSelected (i) == false)
							skip = true;
					}

					if (skip == false) {
						buffer.Append (l.GetStringLines () + "\n");
					}

					i++;
				}
			}

			return Platform.Instance.NormalizeString(buffer.ToString());
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
			LogEntry e = m_items [row];

			if (tableColumn.Identifier == "Icon") {
				return NSImage.ImageNamed("log_" + e.Type.ToString().ToLowerInvariant() + ".png");
			}
			else if (tableColumn.Identifier == "Date") {
				return new NSString (e.GetDateForList());
			}
			else if (tableColumn.Identifier == "Message") {
				return new NSString (e.GetMessageForList());
			}
			else 
				throw new NotImplementedException (string.Format ("{0} is not recognized", tableColumn.Identifier));
		}

	}
}

