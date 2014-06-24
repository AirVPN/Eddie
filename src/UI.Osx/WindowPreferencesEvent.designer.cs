
namespace AirVPN.UI.Osx
{
	
	// Should subclass MonoMac.AppKit.NSWindow
	[MonoMac.Foundation.Register ("WindowPreferencesEvent")]
	public partial class WindowPreferencesEvent
	{
	}
	
	// Should subclass MonoMac.AppKit.NSWindowController
	[MonoMac.Foundation.Register ("WindowPreferencesEventController")]
	public partial class WindowPreferencesEventController
	{
	}
}

