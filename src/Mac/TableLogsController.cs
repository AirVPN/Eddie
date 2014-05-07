using System;
using System.Collections.Generic;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace AirVPN.Mac
{
	public class TableLogsController : NSTableViewDataSource
	{
		public NSTableView tableView;
		public List<string> list;

		public TableLogsController (NSTableView tableView)
		{
			this.tableView = tableView;

			list = new List<string> ();
			list.Add ("alfa");
			list.Add ("beta");

			this.tableView.DataSource = this;

		}

		public override int GetRowCount (NSTableView tableView)
		{
			return list.Count;
		}

		public override NSObject GetObjectValue (NSTableView tableView, 
		                                         NSTableColumn tableColumn, 
		                                         int row)
		{
			if (tableColumn.Identifier == "Icon") {
				return new NSString ("");
			}
			else if (tableColumn.Identifier == "Date") {
				return new NSString (AirVPN.Core.Utils.FormatBytesEx2(41243243, true));
			}
			else if (tableColumn.Identifier == "Message") {
				return new NSString (list [row]);
			}
			else 
				throw new NotImplementedException (string.Format ("{0} is not recognized", tableColumn.Identifier));
		}
	}
}

