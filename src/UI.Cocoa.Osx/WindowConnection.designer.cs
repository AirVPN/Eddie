
namespace Eddie.UI.Cocoa.Osx
{

    // Should subclass MonoMac.AppKit.NSWindow
    [MonoMac.Foundation.Register("WindowConnection")]
    public partial class WindowConnection
    {
    }

    // Should subclass MonoMac.AppKit.NSWindowController
    [MonoMac.Foundation.Register("WindowConnectionController")]
    public partial class WindowConnectionController
    {
    }
}
