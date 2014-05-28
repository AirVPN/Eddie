using System;
using System.Collections.Generic;
using MonoMac.Foundation;
using MonoMac.AppKit;
using AirVPN.Core;

namespace AirVPN.UI.Osx
{
	public class TableAreasController : NSTableViewDataSource
	{
		public NSTableView tableView;

		public List<AreaInfo> Items = new List<AreaInfo>();

		public TableAreasController (NSTableView tableView)
		{
			this.tableView = tableView;

			this.tableView.DataSource = this;

		}

		public void RefreshUI()
		{
			Items.Clear ();
			lock (Engine.Instance.Areas) {
				foreach (AreaInfo a in Engine.Instance.Areas.Values)
					Items.Add (a);
			}
			tableView.ReloadData ();
		}

		public AreaInfo GetRelatedItem(int i)
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
			AreaInfo a = Items[row];

			if (tableColumn.Identifier == "List") {
				if(a.UserList == AreaInfo.UserListType.WhiteList)
					return NSImage.ImageNamed("blacklist_0.png");
				else if(a.UserList == AreaInfo.UserListType.BlackList)
					return NSImage.ImageNamed("blacklist_1.png");
				else
					return NSImage.ImageNamed("blacklist_2.png");
			} else if (tableColumn.Identifier == "Flag") {
				return NSImage.ImageNamed("flag_" + a.Code.ToLowerInvariant() + ".png");
			} else if (tableColumn.Identifier == "Name") {
				return new NSString (a.PublicName);
			} else if (tableColumn.Identifier == "Servers") {
				return new NSString (a.Servers.ToString());
			} else if (tableColumn.Identifier == "Load") {
				return new NSString (a.Bandwidth.ToString ());
			} else if (tableColumn.Identifier == "Users") {
				return new NSString (a.Users.ToString());
			} else 
				throw new NotImplementedException (string.Format ("{0} is not recognized", tableColumn.Identifier));
		}

	}
}

