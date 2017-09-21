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
using MonoMac.AppKit;

namespace Eddie.UI.Cocoa.Osx
{
	public class TableTabsDelegate : NSTableViewDelegate
	{
		NSTableView m_tableView;
		NSTabView m_tabView;

		public TableTabsDelegate (NSTableView tableView, NSTabView tabView)
		{
			m_tableView = tableView;
			m_tabView = tabView;

			SelectionChange ();
		}

		public override void SelectionDidChange (MonoMac.Foundation.NSNotification notification)
		{
			SelectionChange ();
		}

		void SelectionChange()
		{
			if (m_tableView.SelectedRow == -1) {
				m_tableView.SelectRow (0, false);
				m_tabView.SelectAt (0);
			} else {
				m_tabView.SelectAt (m_tableView.SelectedRow);
			}
		}
	}
}

