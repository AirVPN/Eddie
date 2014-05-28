using System;
using MonoMac.AppKit;

namespace AirVPN.UI.Osx
{
	public class TableServersDelegate : NSTableViewDelegate
	{
		MainWindowController m_main;

		public TableServersDelegate (MainWindowController main)
		{
			m_main = main;
		}

		public override void SelectionDidChange (MonoMac.Foundation.NSNotification notification)
		{
			m_main.EnabledUI ();
		}
	}
}

