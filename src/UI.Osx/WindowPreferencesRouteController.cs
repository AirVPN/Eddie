
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace AirVPN.UI.Osx
{
	public partial class WindowPreferencesRouteController : MonoMac.AppKit.NSWindowController
	{
		#region Constructors

		// Called when created from unmanaged code
		public WindowPreferencesRouteController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public WindowPreferencesRouteController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		
		// Call to load from the XIB/NIB file
		public WindowPreferencesRouteController () : base ("WindowPreferencesRoute")
		{
			Initialize ();
		}
		
		// Shared initialization code
		void Initialize ()
		{
		}

		#endregion

		//strongly typed window accessor
		public new WindowPreferencesRoute Window {
			get {
				return (WindowPreferencesRoute)base.Window;
			}
		}
	}
}

