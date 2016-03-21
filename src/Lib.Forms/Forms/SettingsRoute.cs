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
    public partial class SettingsRoute : AirVPN.Gui.Form
    {
        public String Ip;        
        public String Action;
		public String Notes;

        public SettingsRoute()
        {
            OnPreInitializeComponent();
            InitializeComponent();
            OnInitializeComponent();
        }

        public override void OnInitializeComponent()
        {
            base.OnInitializeComponent();

            txtHost.Font = Skin.FontMono;
        }

        protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			CommonInit(Messages.WindowsSettingsRouteTitle);

			lblHostHelp.Text = Messages.WindowsSettingsRouteEditIp;

			cboAction.Items.Add(Settings.RouteDirectionToDescription("in"));
			cboAction.Items.Add(Settings.RouteDirectionToDescription("out"));

			txtHost.Text = Ip;
			cboAction.Text = Settings.RouteDirectionToDescription(Action);
			txtNotes.Text = Notes;

			EnableIde();
		}
        
		private void EnableIde()
		{
			if (new IpAddressRange(txtHost.Text).Valid == false)
			{
				lblHostHelp.Text = Messages.WindowsSettingsRouteInvalid + "\n" + Messages.WindowsSettingsRouteEditIp;
				cmdOk.Enabled = false;
			}
			else
			{
				lblHostHelp.Text = Messages.WindowsSettingsRouteEditIp;
				cmdOk.Enabled = true;
			}
		}

        private void cmdOk_Click(object sender, EventArgs e)
        {			
			Ip = txtHost.Text;
			Action = Settings.RouteDescriptionToDirection(cboAction.Text);
			Notes = txtNotes.Text;			
        }

		private void txtHost_TextChanged(object sender, EventArgs e)
		{
			EnableIde();
		}
    }
}
