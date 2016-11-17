// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2016 AirVPN (support@airvpn.org) / https://airvpn.org
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
using System.Globalization;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Eddie.Core;


namespace Eddie.UI.Cocoa.Osx
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

		public static void SelectFile(NSWindow window, NSTextField field)
		{
			NSOpenPanel openPanel = new NSOpenPanel ();
			openPanel.BeginSheet (window, (i) => {

				try {
					if (openPanel.Url != null) {
						string path = openPanel.Url.Path;

						if (!string.IsNullOrEmpty (path))
							field.StringValue = path;
					}
				} finally {
					openPanel.Dispose ();
				}


			});

		}

		public static void MessageBox(string message)
		{
			MessageBox (message, "");
		}

		public static void MessageBox(string message, string title)
		{
			NSAlert alert = new NSAlert();
			alert.MessageText = title;
			alert.InformativeText = message;
			alert.RunModal();
		}

		public static bool MessageYesNo(string message)
		{
			return MessageYesNo (message, "");
		}

		public static bool MessageYesNo(string message, string title)
		{
			NSAlert alert = new NSAlert();
			alert.MessageText = title;
			alert.InformativeText = message;
			alert.AddButton ("Yes");
			alert.AddButton ("No");
			int r = alert.RunModal();

			if (r == 1000)
				return true;

			return false;
		}

		/*
		public static string InterfaceColorMode()
		{
			//string colorMode = NSUserDefaults.StandardUserDefaults.StringForKey ("AppleInterfaceStyle");
			string colorMode = Platform.Instance.ShellCmd ("defaults read -g AppleInterfaceStyle 2>/dev/null");
			if (colorMode == "Dark")
				return "Dark";
			else
				return "Light";
		}
		*/

		public static void ShowWindowWithFocus(MonoMac.AppKit.NSWindowController w, MonoMac.AppKit.NSWindowController parent)
		{
			w.ShowWindow (parent);
			w.Window.Deminiaturize (parent);
			w.Window.MakeKeyAndOrderFront (parent);
			w.Window.MakeMainWindow ();
		}
	}
}

