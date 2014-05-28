using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace AirVPN.UI.Osx
{
	public partial class WindowOpenVpnManagementCommandController : MonoMac.AppKit.NSWindowController
	{
		public string Command = "";

		#region Constructors
		// Called when created from unmanaged code
		public WindowOpenVpnManagementCommandController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public WindowOpenVpnManagementCommandController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		// Call to load from the XIB/NIB file
		public WindowOpenVpnManagementCommandController () : base ("WindowOpenVpnManagementCommand")
		{
			Initialize ();
		}
		// Shared initialization code
		void Initialize ()
		{
		}
		#endregion
		//strongly typed window accessor
		public new WindowOpenVpnManagementCommand Window {
			get {
				return (WindowOpenVpnManagementCommand)base.Window;
			}
		}
	}
}

