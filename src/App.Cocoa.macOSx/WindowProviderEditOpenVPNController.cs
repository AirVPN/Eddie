
using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;
using Eddie.Lib.Common;
using Eddie.Core;

namespace Eddie.UI.Cocoa.Osx
{
	public partial class WindowProviderEditOpenVPNController : AppKit.NSWindowController
	{
		public Core.Providers.OpenVPN Provider;

		#region Constructors

		// Called when created from unmanaged code
		public WindowProviderEditOpenVPNController(IntPtr handle) : base(handle)
		{
			Initialize();
		}

		// Called when created directly from a XIB file
		[Export("initWithCoder:")]
		public WindowProviderEditOpenVPNController(NSCoder coder) : base(coder)
		{
			Initialize();
		}

		// Call to load from the XIB/NIB file
		public WindowProviderEditOpenVPNController() : base("WindowProviderEditOpenVPN")
		{
			Initialize();
		}

		// Shared initialization code
		void Initialize()
		{
		}

		#endregion

		//strongly typed window accessor
		public new WindowProviderEditOpenVPN Window
		{
			get
			{
				return (WindowProviderEditOpenVPN)base.Window;
			}
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();

			Window.Title = Constants.Name + " - " + Messages.WindowsProviderEditOpenVPNTitle;

			LblTitle.Title = Provider.DefinitionTitle;
			LblSubtitle.StringValue = Provider.DefinitionSubTitle;

			GuiUtils.SetCheck(ChkEnabled, Provider.Enabled);
			TxtTitle.StringValue = Provider.Title;
			TxtPath.StringValue = Provider.Path;

			TxtAuthPassUsername.StringValue = Provider.AuthPassUsername;
			TxtAuthPassPassword.StringValue = Provider.AuthPassPassword;

			LblTitle.Activated += (object sender, EventArgs e) =>
			{
				Core.Platform.Instance.OpenUrl(Provider.DefinitionHref);
			};

			CmdPathBrowse.Activated += (object sender, EventArgs e) =>
			{
				GuiUtils.SelectFile(this.Window, TxtPath);
			};

			CmdOk.Activated += (object sender, EventArgs e) =>
			{
				Provider.Enabled = GuiUtils.GetCheck(ChkEnabled);
				Provider.Title = TxtTitle.StringValue;
				Provider.Path = TxtPath.StringValue;

				Provider.AuthPassUsername = TxtAuthPassUsername.StringValue;
				Provider.AuthPassPassword = TxtAuthPassPassword.StringValue;

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
