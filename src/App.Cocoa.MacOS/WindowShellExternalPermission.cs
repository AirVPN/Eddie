using System;

using Foundation;
using AppKit;

namespace Eddie.UI.Cocoa.Osx
{
    public partial class WindowShellExternalPermission : NSWindow
    {
        public WindowShellExternalPermission(IntPtr handle) : base(handle)
        {
        }

        [Export("initWithCoder:")]
        public WindowShellExternalPermission(NSCoder coder) : base(coder)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
        }
    }
}
