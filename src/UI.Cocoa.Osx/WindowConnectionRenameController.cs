
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Eddie.Lib.Common;
using Eddie.Core;

namespace Eddie.UI.Cocoa.Osx
{
    public partial class WindowConnectionRenameController : MonoMac.AppKit.NSWindowController
    {
        public string Body;

        #region Constructors

        // Called when created from unmanaged code
        public WindowConnectionRenameController(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public WindowConnectionRenameController(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        // Call to load from the XIB/NIB file
        public WindowConnectionRenameController() : base("WindowConnectionRename")
        {
            Initialize();
        }

        // Shared initialization code
        void Initialize()
        {
        }

        #endregion

        //strongly typed window accessor
        public new WindowConnectionRename Window
        {
            get
            {
                return (WindowConnectionRename)base.Window;
            }
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            Window.Title = Constants.Name + " - " + Messages.WindowsConnectionRenameTitle;

            TxtBody.StringValue = Body;

			CmdOk.Activated += (object sender, EventArgs e) =>
			{
                Body = TxtBody.StringValue;

				Window.Close();
				NSApplication.SharedApplication.StopModal();
			};

			CmdCancel.Activated += (object sender, EventArgs e) =>
			{
                Body = "";

				Window.Close();
				NSApplication.SharedApplication.StopModal();
			};
        }
    }
}
