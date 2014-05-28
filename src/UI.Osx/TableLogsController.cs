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

		private Engine m_engine;

		public TableLogsController (NSTableView tableView)
		{
			this.tableView = tableView;

			m_engine = AirVPN.Core.Engine.Instance as Engine;

			this.tableView.DataSource = this;

		}

		public void RefreshUI()
		{
			tableView.ReloadData ();
		}

		public override int GetRowCount (NSTableView tableView)
		{
			return m_engine.LogsEntries.Count;
		}

		public override NSObject GetObjectValue (NSTableView tableView, 
		                                         NSTableColumn tableColumn, 
		                                         int row)
		{
			LogEntry e = m_engine.LogsEntries [row];

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

