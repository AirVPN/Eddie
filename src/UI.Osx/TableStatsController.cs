using System;
using System.Collections.Generic;
using MonoMac.Foundation;
using MonoMac.AppKit;
using AirVPN.Core;

namespace AirVPN.UI.Osx
{
	public class TableStatsController : NSTableViewDataSource
	{
		public NSTableView tableView;

		private Engine m_engine;

		public TableStatsController (NSTableView tableView)
		{
			this.tableView = tableView;

			m_engine = AirVPN.Core.Engine.Instance as Engine;

			this.tableView.DataSource = this;

		}

		public void RefreshUI()
		{
			tableView.ReloadData ();
		}

		public void DoubleClickItem()
		{
			int i = tableView.SelectedRow;
			if ((i >= 0) && (i < Engine.Instance.Stats.List.Count)) {
				StatsEntry e = Engine.Instance.Stats.List [tableView.SelectedRow];
				if (e.Key == "VpnGeneratedOVPN") {
				} else if (e.Key == "SystemReport") {
					WindowTextViewerController textViewer = new WindowTextViewerController ();
					textViewer.Title = e.Caption;
					textViewer.Body = Platform.Instance.GenerateSystemReport ();
					textViewer.ShowWindow (this);
				}
			}
		}

		public override int GetRowCount (NSTableView tableView)
		{
			return Engine.Instance.Stats.List.Count;
		}

		public override NSObject GetObjectValue (NSTableView tableView, 
		                                         NSTableColumn tableColumn, 
		                                         int row)
		{
			StatsEntry e = Engine.Instance.Stats.List [row];

			if (tableColumn.Identifier == "Icon") {
				return NSImage.ImageNamed("stats_" + e.Icon.ToLowerInvariant() + ".png");
			}
			else if (tableColumn.Identifier == "Key") {
				return new NSString (e.Caption);
			}
			else if (tableColumn.Identifier == "Value") {
				return new NSString (e.Value);
			}
			else 
				throw new NotImplementedException (string.Format ("{0} is not recognized", tableColumn.Identifier));
		}

	}
}

