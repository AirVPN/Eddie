
using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;

namespace Eddie.UI.Cocoa.Osx
{
    public partial class WindowCredentials : AppKit.NSWindow
    {
        #region Constructors

        // Called when created from unmanaged code
        public WindowCredentials(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public WindowCredentials(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        // Shared initialization code
        void Initialize()
        {
        }

        #endregion
    }
}
