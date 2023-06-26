// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2023 AirVPN (support@airvpn.org) / https://airvpn.org )
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
using Foundation;
using AppKit;
using Eddie.Core;

namespace Eddie.UI.Cocoa.Osx
{
	public partial class WindowConnectionController : AppKit.NSWindowController
	{
		public ConnectionInfo Connection;

		private Core.ConnectionTypes.IConnectionType m_connection;

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

			Window.Title = Constants.Name + " - " + LanguageManager.GetText(LanguageItems.WindowsConnectionTitle);

			GuiUtils.SetButtonCancel(Window, CmdCancel);
            GuiUtils.SetButtonDefault(Window, CmdOk);
						
            m_connection = Connection.BuildConnection(null);

			TxtOvpnGenerated.Value = Core.Platform.Instance.NormalizeString(m_connection.ExportConfigStartup());

			string configOriginal = m_connection.ExportConfigOriginal();
			if(configOriginal != "")
			{
				TxtOvpnOriginal.Value = Core.Platform.Instance.NormalizeString(configOriginal);
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
