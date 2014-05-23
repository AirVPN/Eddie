using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using AirVPN.Core;

namespace AirVPN.UI.Osx
{
	public partial class WindowTosController : MonoMac.AppKit.NSWindowController
	{
		public bool Accepted = false;
		public string PazzoTest = "uno";

		#region Constructors
		// Called when created from unmanaged code
		public WindowTosController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public WindowTosController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		// Call to load from the XIB/NIB file
		public WindowTosController () : base ("WindowTos")
		{
			Initialize ();
		}
		// Shared initialization code
		void Initialize ()
		{
		}
		#endregion
		//strongly typed window accessor
		public new WindowTos Window {
			get {
				return (WindowTos)base.Window;
			}
		}


		public override void AwakeFromNib()
		{
			base.AwakeFromNib ();

			Accepted = false;

			Window.Title = Messages.WindowsTosTitle;

			TxtTos.Value = Core.UI.Actions.GetTos ();
			ChkTos1.Title = Messages.WindowsTosCheck1;
			ChkTos2.Title = Messages.WindowsTosCheck2;
			CmdAccept.Title = Messages.WindowsTosAccept;
			CmdCancel.StringValue = Messages.WindowsTosReject;

			CmdAccept.Activated += (object sender, EventArgs e) =>
			{
				Accepted = true;
				Window.Close ();
				NSApplication.SharedApplication.StopModal();
			};

			CmdCancel.Activated += (object sender, EventArgs e) =>
			{
				Window.Close ();
				NSApplication.SharedApplication.StopModal();
			};
		}
	}
}

