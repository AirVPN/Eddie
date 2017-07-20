
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Eddie.UI.Cocoa.Osx
{
    public partial class WindowConnectionController : MonoMac.AppKit.NSWindowController
    {
        #region Constructors

        // Called when created from unmanaged code
        public WindowConnectionController(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public WindowConnectionController(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        // Call to load from the XIB/NIB file
        public WindowConnectionController() : base("WindowConnection")
        {
            Initialize();
        }

        // Shared initialization code
        void Initialize()
        {
        }

        #endregion

        //strongly typed window accessor
        public new WindowConnection Window
        {
            get
            {
                return (WindowConnection)base.Window;
            }
        }
    }
}
