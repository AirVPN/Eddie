
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Eddie.Lib.Common;
using Eddie.Core;

namespace Eddie.UI.Cocoa.Osx
{
	public partial class WindowProviderEditManifestController : MonoMac.AppKit.NSWindowController
	{
		public Provider Provider;

		#region Constructors

		// Called when created from unmanaged code
		public WindowProviderEditManifestController(IntPtr handle) : base(handle)
		{
			Initialize();
		}

		// Called when created directly from a XIB file
		[Export("initWithCoder:")]
		public WindowProviderEditManifestController(NSCoder coder) : base(coder)
		{
			Initialize();
		}

		// Call to load from the XIB/NIB file
		public WindowProviderEditManifestController() : base("WindowProviderEditManifest")
		{
			Initialize();
		}

		// Shared initialization code
		void Initialize()
		{
		}

		#endregion

		//strongly typed window accessor
		public new WindowProviderEditManifest Window
		{
			get
			{
				return (WindowProviderEditManifest)base.Window;
			}
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();

			Window.Title = Constants.Name + " - " + Messages.WindowsProviderEditManifestTitle;

			LblTitle.Title = Provider.DefinitionTitle;
			LblSubtitle.StringValue = Provider.DefinitionSubTitle;

			GuiUtils.SetCheck(ChkEnabled, Provider.Enabled);

			LblTitle.Activated += (object sender, EventArgs e) =>
			{
				Core.Platform.Instance.OpenUrl(Provider.DefinitionHref);
			};

			CmdOk.Activated += (object sender, EventArgs e) =>
			{
				Provider.Enabled = GuiUtils.GetCheck(ChkEnabled);

				Window.Close();
				NSApplication.SharedApplication.StopModal();
			};

			CmdCancel.Activated += (object sender, EventArgs e) =>
			{
				Provider = null;

				Window.Close();
				NSApplication.SharedApplication.StopModal();
			};
		}
	}
}
