using System;

using Foundation;
using AppKit;

namespace Eddie.UI.Cocoa.Osx
{
    public partial class WindowSplash : NSWindow
    {
        public WindowSplash(IntPtr handle) : base(handle)
        {
        }

        [Export("initWithCoder:")]
        public WindowSplash(NSCoder coder) : base(coder)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
        }
    }
}
