// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2016 AirVPN (support@airvpn.org) / https://airvpn.org )
//
// Eddie is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Eddie is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Eddie. If not, see <http://www.gnu.org/licenses/>.
// </eddie_source_header>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Foundation;
using AppKit;
using Eddie.Common;
using Eddie.Core;

namespace Eddie.UI.Cocoa.Osx
{
	public partial class WindowProviderAddController : AppKit.NSWindowController
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
				string code = UtilsXml.XmlGetAttributeString(xmlProvider, "code", "");
				string t = UtilsXml.XmlGetAttributeString(xmlProvider, "title", "");
				t += " - " + UtilsXml.XmlGetAttributeString(xmlProvider, "subtitle", "");
				t += " - " + UtilsXml.XmlGetAttributeString(xmlProvider, "href", "");
				CboProvider.AddItem(t);
				m_choices.Add(code);
			}

			CmdOk.Activated += (object sender, EventArgs e) =>
			{
				Provider = m_choices[(int)CboProvider.IndexOfSelectedItem];

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
