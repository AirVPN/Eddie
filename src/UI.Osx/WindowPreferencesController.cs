using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using AirVPN.Core;

namespace AirVPN.UI.Osx
{
	public partial class WindowPreferencesController : MonoMac.AppKit.NSWindowController
	{
		#region Constructors
		// Called when created from unmanaged code
		public WindowPreferencesController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public WindowPreferencesController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		// Call to load from the XIB/NIB file
		public WindowPreferencesController () : base ("WindowPreferences")
		{
			Initialize ();
		}
		// Shared initialization code
		void Initialize ()
		{
		}
		#endregion
		//strongly typed window accessor
		public new WindowPreferences Window {
			get {
				return (WindowPreferences)base.Window;
			}
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib ();

			Window.Title = Constants.Name + " - " + Messages.WindowsSettingsTitle;

			ReadOptions ();

			EnableIde ();



			CmdSave.Activated += (object sender, EventArgs e) => {
				SaveOptions ();
				Close ();
			};

			CmdCancel.Activated += (object sender, EventArgs e) => {
				Close ();
			};

			CmdGeneralTos.Activated += (object sender, EventArgs e) => {
				WindowTosController tos = new WindowTosController ();
				tos.Window.ReleasedWhenClosed = true;
				NSApplication.SharedApplication.RunModalForWindow (tos.Window);
				tos.Window.Close ();
			};
		}

		bool GetCheck(NSButton button)
		{
			return (button.State == NSCellStateValue.On);
		}

		void SetCheck(NSButton button, bool val)
		{
			button.State = val ? NSCellStateValue.On : NSCellStateValue.Off;
		}

		void ReadOptions()
		{
			Storage s = Engine.Instance.Storage;

			SetCheck (ChkAutoStart, s.GetBool ("connect")); 
			SetCheck (ChkGeneralStartLast, s.GetBool("servers.startlast"));
		}

		void SaveOptions()
		{
			Storage s = Engine.Instance.Storage;

			s.SetBool ("connect", GetCheck (ChkAutoStart));
		}

		void EnableIde()
		{
		}
	}
}

