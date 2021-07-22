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
using Foundation;
using AppKit;
using Eddie.Core;

namespace Eddie.UI.Cocoa.Osx
{
	public class TableProtocolsControllerItem
	{
		public string Type = "";
		public string Protocol = "";
		public string Transport = "";
		public int Port = 0;
		public int IP = 0;
		public string Description = "";
		public string Specs = "";

		public TableProtocolsControllerItem()
		{

		}
	}

	public class TableProtocolsController : NSTableViewDataSource
	{
		public NSTableView tableView;

		public List<TableProtocolsControllerItem> Items = new List<TableProtocolsControllerItem>();

		public TableProtocolsController(NSTableView tableView)
		{
			this.tableView = tableView;

			if ((Engine.Instance.AirVPN != null) && (Engine.Instance.AirVPN.Manifest != null))
			{
				foreach (ConnectionMode mode in Engine.Instance.AirVPN.Modes)
				{
					if (mode.Available == false)
						continue;

					if ((mode.Type == "wireguard") && (Core.Platform.Instance.GetSupportWireGuard() == false))
						continue;

					TableProtocolsControllerItem item = new TableProtocolsControllerItem();
					item.Type = mode.Type;
					item.Protocol = mode.Protocol;
					item.Transport = mode.Transport;
					item.Port = mode.Port;
					item.IP = mode.EntryIndex;
					item.Description = mode.Title;
					item.Specs = mode.Specs;
					Items.Add(item);
				}
			}

			this.tableView.DataSource = this;

		}

		public void RefreshUI()
		{
			tableView.ReloadData();
		}

		public override nint GetRowCount(NSTableView tableView)
		{
			return Items.Count;
		}

		public override NSObject GetObjectValue(NSTableView tableView,
												 NSTableColumn tableColumn,
												 nint row)
		{
			TableProtocolsControllerItem i = Items[(int)row];

			if (tableColumn.Identifier == "Type")
			{
				string t = i.Type.ToLowerInvariant();
				if (t == "openvpn") t = "OpenVPN";
				if (t == "wireguard") t = "WireGuard";
				return new NSString(t);
			}
			else if (tableColumn.Identifier == "Protocol")
			{
				string p = i.Protocol.ToUpperInvariant();
				if (i.Transport != "")
					p = i.Transport + ">" + p;
				return new NSString(p);
			}
			else if (tableColumn.Identifier == "Port")
			{
				return new NSString(i.Port.ToString());
			}
			else if (tableColumn.Identifier == "IP")
			{
				return new NSString(Conversions.ToString(i.IP + 1));
			}
			else if (tableColumn.Identifier == "Description")
			{
				return new NSString(i.Description);
			}
			else if (tableColumn.Identifier == "Specs")
			{
				return new NSString(i.Specs);
			}
			else
				throw new NotImplementedException(string.Format("{0} is not recognized", tableColumn.Identifier));
		}

	}
}

