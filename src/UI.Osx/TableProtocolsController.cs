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
using System.Xml;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Eddie.Core;

namespace Eddie.UI.Osx
{
	public class TableProtocolsControllerItem 
	{
		public string Protocol = "";
		public int Port = 0;
		public int Entry = 0;
		public string Cipher;
		public string Description;

		public TableProtocolsControllerItem()
		{

		}
	}

	public class TableProtocolsController : NSTableViewDataSource
	{
		public NSTableView tableView;

		public List<TableProtocolsControllerItem> Items = new List<TableProtocolsControllerItem>();

		public TableProtocolsController (NSTableView tableView)
		{
			this.tableView = tableView;

			XmlNodeList xmlModes = Engine.Instance.Storage.Manifest.SelectNodes ("//modes/mode");
			foreach (XmlElement xmlMode in xmlModes) {
				TableProtocolsControllerItem item = new TableProtocolsControllerItem ();
				item.Protocol = xmlMode.GetAttribute ("protocol").ToUpper ();
				item.Port = Conversions.ToInt32 (xmlMode.GetAttribute ("port"), 443);
				item.Entry = Conversions.ToInt32 (xmlMode.GetAttribute ("entry_index"), 0);
				item.Cipher = xmlMode.GetAttribute ("cipher");
				item.Description = xmlMode.GetAttribute ("title");
				Items.Add (item);
			}

			this.tableView.DataSource = this;

		}

		public void RefreshUI()
		{
			tableView.ReloadData ();
		}

		public override int GetRowCount (NSTableView tableView)
		{
			return Items.Count;
		}

		public override NSObject GetObjectValue (NSTableView tableView, 
		                                         NSTableColumn tableColumn, 
		                                         int row)
		{
			TableProtocolsControllerItem i = Items [row];

			if (tableColumn.Identifier == "Protocol") {
				return new NSString (i.Protocol.ToUpperInvariant());
			}
			else if (tableColumn.Identifier == "Port") {
				return new NSString (i.Port.ToString());
			}
			else if (tableColumn.Identifier == "Entry") {
				return new NSString ((i.Entry==0) ? "Primary":"Alternative");
			}
			else if (tableColumn.Identifier == "Cipher") {
				return new NSString (i.Cipher);
			}
			else if (tableColumn.Identifier == "Description") {
				return new NSString (i.Description);
			}
			else 
				throw new NotImplementedException (string.Format ("{0} is not recognized", tableColumn.Identifier));
		}

	}
}

