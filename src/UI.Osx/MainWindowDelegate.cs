using System;
using MonoMac.AppKit;

namespace AirVPN.UI.Osx
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
			Engine.Instance.RequestStop ();
			return false;
		}
	}
}

