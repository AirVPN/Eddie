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

		public List<ServerInfo> Items = new List<ServerInfo>();

		public bool ShowAll = false;

		public TableServersController (NSTableView tableView)
		{
			this.tableView = tableView;

			this.tableView.DataSource = this;

		}

		public void RefreshUI()
		{
			Items.Clear ();

			Items = Engine.Instance.GetServers (ShowAll);
			tableView.ReloadData ();
		}

		public ServerInfo GetRelatedItem(int i)
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
			ServerInfo s = Items [row];

			if (tableColumn.Identifier == "List") {
				if(s.UserList == ServerInfo.UserListType.WhiteList)
					return NSImage.ImageNamed("blacklist_0.png");
				else if(s.UserList == ServerInfo.UserListType.BlackList)
					return NSImage.ImageNamed("blacklist_1.png");
				else
					return NSImage.ImageNamed("blacklist_2.png");
			} else if (tableColumn.Identifier == "Flag") {
				return NSImage.ImageNamed("flag_" + s.CountryCode.ToLowerInvariant() + ".png");
			} else if (tableColumn.Identifier == "Name") {
				return new NSString (s.PublicName);
			} else if (tableColumn.Identifier == "Score") {
				return NSImage.ImageNamed ("stars_h.png");
			} else if (tableColumn.Identifier == "Location") {
				return new NSString (s.GetLocationForList());
			} else if (tableColumn.Identifier == "Latency") {
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

