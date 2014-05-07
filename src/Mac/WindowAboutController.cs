using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace AirVPN.Mac
{
	public partial class WindowAboutController : MonoMac.AppKit.NSWindowController
	{
		#region Constructors
		// Called when created from unmanaged code
		public WindowAboutController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public WindowAboutController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		// Call to load from the XIB/NIB file
		public WindowAboutController () : base ("WindowAbout")
		{
			Initialize ();
		}
		// Shared initialization code
		void Initialize ()
		{
		}
		#endregion
		//strongly typed window accessor
		public new WindowAbout Window {
			get {
				return (WindowAbout)base.Window;
			}
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib ();

			TxtLicense.Value = AirVPN.Core.Actions.GetAboutLicense ();

			CmdOk.Activated += (object sender, EventArgs e) =>
			{
				this.Close();
				NSApplication.SharedApplication.StopModal();
			};
		}
	}
}

