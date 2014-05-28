using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace AirVPN.UI.Osx
{
	public partial class WindowFrontMessageController : MonoMac.AppKit.NSWindowController
	{
		public string Message;

		#region Constructors
		// Called when created from unmanaged code
		public WindowFrontMessageController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public WindowFrontMessageController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		// Call to load from the XIB/NIB file
		public WindowFrontMessageController () : base ("WindowFrontMessage")
		{
			Initialize ();
		}
		// Shared initialization code
		void Initialize ()
		{
		}
		#endregion
		//strongly typed window accessor
		public new WindowFrontMessage Window {
			get {
				return (WindowFrontMessage)base.Window;
			}
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib ();

			Window.Title = AirVPN.Core.Constants.Name + " - " + Core.Messages.WindowsFrontMessageTitle;

			TxtMessage.StringValue = Message;
			CmdClose.Title = Core.Messages.WindowsFrontMessageAccept;
			CmdMore.Title = Core.Messages.WindowsFrontMessageMore;

			CmdClose.Activated += (object sender, EventArgs e) =>
			{
				Window.Close ();
			};

			CmdMore.Activated += (object sender, EventArgs e) =>
			{
				Core.UI.Actions.OpenUrlWebsite();
			};
		}
	}
}

