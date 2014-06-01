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

