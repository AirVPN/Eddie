
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Eddie.Lib.Common;
using Eddie.Core;

namespace Eddie.UI.Cocoa.Osx
{
	public partial class WindowConnectionController : MonoMac.AppKit.NSWindowController
	{
		public ConnectionInfo Connection;

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

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();

			Window.Title = Constants.Name + " - " + Messages.WindowsConnectionTitle;

			TxtOvpnGenerated.Value = Core.Platform.Instance.NormalizeString(Connection.BuildOVPN(true).Get());
			if (Connection.Path != "")
			{
				if (Core.Platform.Instance.FileExists(Connection.Path))
				{
					string original = Core.Platform.Instance.FileContentsReadText(Connection.Path);
					TxtOvpnOriginal.Value = original;
				}
			}
			else
			{
				TabMain.Remove(TabMain.Items[1]);
			}

			CmdOk.Activated += (object sender, EventArgs e) =>
			{
				Window.Close();
				NSApplication.SharedApplication.StopModal();
			};

			CmdCancel.Activated += (object sender, EventArgs e) =>
			{
				Window.Close();
				NSApplication.SharedApplication.StopModal();
			};
		}
	}
}
