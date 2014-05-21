
namespace AirVPN.UI.Osx
{
	// Should subclass MonoMac.AppKit.NSWindow
	[MonoMac.Foundation.Register("WindowFrontMessage")]
	public partial class WindowFrontMessage
	{
	}
	// Should subclass MonoMac.AppKit.NSWindowController
	[MonoMac.Foundation.Register("WindowFrontMessageController")]
	public partial class WindowFrontMessageController
	{
	}
}

