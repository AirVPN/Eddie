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
using Eddie.Core;

namespace Eddie.UI.Osx
{
	public class MainWindowDelegate : NSWindowDelegate
	{
		MainWindowController m_main;

		public MainWindowDelegate (MainWindowController main)
		{
			m_main = main;
		}

		public override bool WindowShouldClose (MonoMac.Foundation.NSObject sender)
		{
			return true; // 2.8, in previous versions closing the main window will close the App
			/*
			m_main.Shutdown ();
			return false;
			*/
		}

		public override void DidMiniaturize (MonoMac.Foundation.NSNotification notification)
		{
			m_main.EnabledUI ();
		}
	}
}

