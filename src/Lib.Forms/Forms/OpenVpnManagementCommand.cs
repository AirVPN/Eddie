// <airvpn_source_header>
// This file is part of AirVPN Client software.
// Copyright (C)2014-2014 AirVPN (support@airvpn.org) / https://airvpn.org )
//
// AirVPN Client is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// AirVPN Client is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with AirVPN Client. If not, see <http://www.gnu.org/licenses/>.
// </airvpn_source_header>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AirVPN.Core;

namespace AirVPN.Gui.Forms
{
    public partial class OpenVpnManagementCommand : AirVPN.Gui.Form
    {
        public String Command;
        
		public OpenVpnManagementCommand()
        {
            InitializeComponent();
        }

        private void SettingsRoute_Load(object sender, EventArgs e)
        {
			CommonInit(Messages.WindowsOpenVpnManagementCommandTitle);
						
			Command = "";

			txtCommand.Text = "help";
			txtCommand.SelectAll();			
			txtCommand.Focus();
        }

        private void cmdOk_Click(object sender, EventArgs e)
        {
			Command = txtCommand.Text;            
        }

        private void lnkHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Core.UI.Actions.OpenUrlOpenVpnManagement();
		}
    }
}
