
using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;

namespace Eddie.UI.Cocoa.Osx
{
    public partial class WindowProviderAdd : AppKit.NSWindow
    {
        #region Constructors

        // Called when created from unmanaged code
        public WindowProviderAdd(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public WindowProviderAdd(NSCoder coder) : base(coder)
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
