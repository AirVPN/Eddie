// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2016 AirVPN (support@airvpn.org) / https://airvpn.org
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

namespace Eddie.Gui.Forms
{
    public partial class WindowConnection : Eddie.Gui.Form
    {
		public ConnectionInfo Connection;

		private Controls.TabNavigator m_tabMain;

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

			txtOvpnGenerated.Font = Skin.FontMono;
			txtOvpnOriginal.Font = Skin.FontMono;
		}

        protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			CommonInit(Messages.WindowsConnectionTitle);

			m_tabMain = new Gui.Controls.TabNavigator();
			m_tabMain.Font = Skin.FontNormal;
			m_tabMain.Top = 0;
			m_tabMain.Left = 0;
			m_tabMain.Height = tabMain.Height;
			m_tabMain.Width = ClientSize.Width;
			m_tabMain.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
			m_tabMain.ImportTabControl(tabMain);
			Controls.Add(m_tabMain);

			txtOvpnGenerated.Text = Platform.Instance.NormalizeString(Connection.BuildOVPN(true).Get());
			if(Connection.Path != "")
			{
				if(Platform.Instance.FileExists(Connection.Path))
				{
					string ovpnOriginal = Platform.Instance.FileContentsReadText(Connection.Path);
					txtOvpnOriginal.Text = Platform.Instance.NormalizeString(ovpnOriginal);
				}
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
