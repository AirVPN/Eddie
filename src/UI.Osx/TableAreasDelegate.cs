using System;
using MonoMac.AppKit;

namespace AirVPN.UI.Osx
{
	public class TableAreasDelegate : NSTableViewDelegate
	{
		MainWindowController m_main;

		public TableAreasDelegate (MainWindowController main)
		{
			m_main = main;
		}

		public override void SelectionDidChange (MonoMac.Foundation.NSNotification notification)
		{
			m_main.EnabledUI ();
		}
	}
}

