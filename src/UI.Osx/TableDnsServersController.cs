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
	public class TableDnsServersController : NSTableViewDataSource
	{
		public NSTableView tableView;

		private List<string> m_items = new List<string>();
		//private Engine m_engine;

		public TableDnsServersController (NSTableView tableView)
		{
			this.tableView = tableView;

			//m_engine = AirVPN.Core.Engine.Instance as Engine;

			this.tableView.DataSource = this;

		}

		public void Add(string ip)
		{
			m_items.Add (ip);
			RefreshUI ();
		}

		public void RemoveAt(int i)
		{
			m_items.RemoveAt (i);
		}

		public string Get(int i)
		{
			return m_items [i];
		}

		public void Set(int i, string ip)
		{
			m_items [i] = ip;
		}

		public int GetCount()
		{
			return m_items.Count;
		}

		public void Clear()
		{
			m_items.Clear ();
			RefreshUI ();
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
			string e = m_items [row];

			if (tableColumn.Identifier == "IP") {
				return new NSString (e);
			}
			else 
				throw new NotImplementedException (string.Format ("{0} is not recognized", tableColumn.Identifier));
		}

	}
}

