// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2019 AirVPN (support@airvpn.org) / https://airvpn.org
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
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Eddie.Core;

namespace Eddie.Forms.Forms
{
	public partial class WindowConnection : Eddie.Forms.Form
	{
		public ConnectionInfo Info;

		private Controls.TabNavigator m_tabMain;
		private Core.ConnectionTypes.IConnectionType m_connection;

		public WindowConnection()
		{
			OnPreInitializeComponent();
			InitializeComponent();
			OnInitializeComponent();
		}

		public override void OnInitializeComponent()
		{
			base.OnInitializeComponent();
		}

		public override void OnApplySkin()
		{
			base.OnApplySkin();

			txtConfigGenerated.Font = Skin.FontMono;
			txtConfigOriginal.Font = Skin.FontMono;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			CommonInit(LanguageManager.GetText("WindowsConnectionTitle"));

			m_tabMain = new Eddie.Forms.Controls.TabNavigator();
			m_tabMain.Font = Skin.FontNormal;
			m_tabMain.Top = 0;
			m_tabMain.Left = 0;
			m_tabMain.Height = tabMain.Height;
			m_tabMain.Width = ClientSize.Width;
			m_tabMain.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
			m_tabMain.ImportTabControl(tabMain);
			Controls.Add(m_tabMain);

			m_connection = Info.BuildConnection(null);

			txtConfigGenerated.Text = Platform.Instance.NormalizeString(m_connection.ExportConfigStartup());

			string configOriginal = m_connection.ExportConfigOriginal();
			if (configOriginal != "")
			{
				txtConfigOriginal.Text = Platform.Instance.NormalizeString(configOriginal);
			}
			else
			{
				m_tabMain.Pages.RemoveAt(1);
			}

			EnableIde();
		}

		private void EnableIde()
		{
		}

		private void cmdOk_Click(object sender, EventArgs e)
		{
		}
	}
}
