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
    public partial class OpenVpnManagementCommand : Eddie.Gui.Form
    {
        public String Command;
        
		public OpenVpnManagementCommand()
        {
            OnPreInitializeComponent();
            InitializeComponent();
            OnInitializeComponent();
        }

        private void SettingsRoute_Load(object sender, EventArgs e)
        {
			CommonInit(Messages.WindowsOpenVpnManagementCommandTitle);
						
			Command = "";

			txtCommand.Text = "help";
			txtCommand.SelectAll();			
			txtCommand.Focus();

            GuiUtils.FixHeightVs(txtCommand, lblCommand);
        }

        private void cmdOk_Click(object sender, EventArgs e)
        {
			Command = txtCommand.Text;            
        }

        private void lnkHelp_LinkClicked(object sender, EventArgs e)
		{
			Engine.Instance.Command("ui.show.openvpn.management");
        }
    }
}
