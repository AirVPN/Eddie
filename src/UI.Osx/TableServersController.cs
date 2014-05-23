using System;
using System.Collections.Generic;
using MonoMac.Foundation;
using MonoMac.AppKit;
using AirVPN.Core;

namespace AirVPN.UI.Osx
{
	public class TableServersController : NSTableViewDataSource
	{
		public NSTableView tableView;

		private Engine m_engine;

		public TableServersController (NSTableView tableView)
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
			return Engine.Instance.Stats.List.Count;
		}

		public override NSObject GetObjectValue (NSTableView tableView, 
		                                         NSTableColumn tableColumn, 
		                                         int row)
		{
			ServerInfo s = Engine.Instance.Servers ["Castor"];

			if (tableColumn.Identifier == "List") {
				return NSImage.ImageNamed("blacklist_2.png");
			} else if (tableColumn.Identifier == "Flag") {
				return NSImage.ImageNamed("flag_" + s.CountryCode.ToLowerInvariant() + ".png");
			} else if (tableColumn.Identifier == "Name") {
				return new NSString (s.PublicName);
			} else if (tableColumn.Identifier == "Score") {
				return new NSString (s.Score().ToString());
			} else if (tableColumn.Identifier == "Location") {
				return new NSString (s.GetLocationForList());
			} else if (tableColumn.Identifier == "Latenct") {
				return new NSString (s.GetLatencyForList());
			} else if (tableColumn.Identifier == "Load") {
				return new NSString (s.Bandwidth.ToString ());
			} else if (tableColumn.Identifier == "Users") {
				return new NSString (s.GetUsersForList());
			} else 
				throw new NotImplementedException (string.Format ("{0} is not recognized", tableColumn.Identifier));
		}

	}
}

