﻿// <eddie_source_header>
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
    public partial class SettingsIp : Eddie.Gui.Form
    {
        public String Ip;

		public SettingsIp()
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

            txtIP.Font = Skin.FontMono;

            GuiUtils.FixHeightVs(txtIP, lblIP);
        }

        protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			CommonInit(Messages.WindowsSettingsIpTitle);

			txtIP.Text = Ip;
            
			EnableIde();
		}
        
		private void EnableIde()
		{
			if (new IpAddress(txtIP.Text).Valid == false)
			{
				cmdOk.Enabled = false;
			}
			else
			{
				cmdOk.Enabled = true;
			}
		}

        private void cmdOk_Click(object sender, EventArgs e)
        {			
			Ip = txtIP.Text;			
        }

		private void txtIp_TextChanged(object sender, EventArgs e)
		{
			EnableIde();
		}
    }
}
