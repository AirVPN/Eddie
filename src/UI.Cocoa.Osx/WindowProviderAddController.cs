
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Eddie.Lib.Common;
using Eddie.Core;

namespace Eddie.UI.Cocoa.Osx
{
	public partial class WindowProviderAddController : MonoMac.AppKit.NSWindowController
	{
		public string Provider;

		private List<string> m_choices = new List<string>();

		#region Constructors

		// Called when created from unmanaged code
		public WindowProviderAddController(IntPtr handle) : base(handle)
		{
			Initialize();
		}

		// Called when created directly from a XIB file
		[Export("initWithCoder:")]
		public WindowProviderAddController(NSCoder coder) : base(coder)
		{
			Initialize();
		}

		// Call to load from the XIB/NIB file
		public WindowProviderAddController() : base("WindowProviderAdd")
		{
			Initialize();
		}

		// Shared initialization code
		void Initialize()
		{
		}

		#endregion

		//strongly typed window accessor
		public new WindowProviderAdd Window
		{
			get
			{
				return (WindowProviderAdd)base.Window;
			}
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();

			Window.Title = Constants.Name + " - " + Messages.WindowsProviderAddTitle;

			XmlElement xmlData = Engine.Instance.ProvidersManager.GetDataAddProviders();

			CboProvider.RemoveAllItems();
			foreach (XmlElement xmlProvider in xmlData.ChildNodes)
			{
				string code = Utils.XmlGetAttributeString(xmlProvider, "code", "");
				string t = Utils.XmlGetAttributeString(xmlProvider, "title", "");
				t += " - " + Utils.XmlGetAttributeString(xmlProvider, "subtitle", "");
				t += " - " + Utils.XmlGetAttributeString(xmlProvider, "href", "");
				CboProvider.AddItem(t);
				m_choices.Add(code);
			}

			CmdOk.Activated += (object sender, EventArgs e) =>
			{
				Provider = m_choices[CboProvider.IndexOfSelectedItem];

				Window.Close();
				NSApplication.SharedApplication.StopModal();
			};

			CmdCancel.Activated += (object sender, EventArgs e) =>
			{
				Provider = "";

				Window.Close();
				NSApplication.SharedApplication.StopModal();
			};

			EnableIde();
		}

		public void EnableIde()
		{
			CmdOk.Enabled = (CboProvider.SelectedItem != null);
		}
	}
}
