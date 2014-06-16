
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace AirVPN.UI.Osx
{
	public partial class WindowPreferencesEventController : MonoMac.AppKit.NSWindowController
	{
		#region Constructors

		// Called when created from unmanaged code
		public WindowPreferencesEventController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public WindowPreferencesEventController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		
		// Call to load from the XIB/NIB file
		public WindowPreferencesEventController () : base ("WindowPreferencesEvent")
		{
			Initialize ();
		}
		
		// Shared initialization code
		void Initialize ()
		{
		}

		#endregion

		//strongly typed window accessor
		public new WindowPreferencesEvent Window {
			get {
				return (WindowPreferencesEvent)base.Window;
			}
		}
	}
}

