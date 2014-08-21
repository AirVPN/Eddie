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
using System.Globalization;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using AirVPN.Core;


namespace AirVPN.UI.Osx
{
	public static class GuiUtils
	{
		public static NSColor ConvertColor(System.Drawing.Color c)
		{
			return NSColor.FromSrgb(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f, c.A / 255.0f);
		}
		public static void SetSelected(NSPopUpButton control, string value)
		{
			control.SelectItem (value);
		}

		public static string GetSelected(NSPopUpButton control)
		{
			return control.SelectedItem.Title;
		}

		public static bool GetCheck(NSButton button)
		{
			return (button.State == NSCellStateValue.On);
		}

		public static void SetCheck(NSButton button, bool val)
		{
			button.State = val ? NSCellStateValue.On : NSCellStateValue.Off;
		}
	}
}

