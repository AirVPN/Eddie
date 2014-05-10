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
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using AirVPN.Core;

namespace AirVPN.Gui.Forms
{
    public partial class About : AirVPN.Gui.Form
    {
        public About()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            lblVersion.ForeColor = Color.White;

            CommonInit(Messages.WindowsAboutTitle);

            lblVersion.Text = Messages.WindowsAboutVersion + " " + Storage.GetVersionDesc();

			txtLicense.Text = Core.UI.Actions.GetAboutLicense();
			txtThirdParty.Text = Core.UI.Actions.GetAboutThirdParty();
        }
                
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Skin.GraphicsCommon(e.Graphics);

			Form.DrawImage(e.Graphics, GuiUtils.GetResourceImage("about_header_bg"), new Rectangle(0, 0, ClientSize.Width + 50, 88 + 2)); // +50 and +2 to avoid GDI+ problem
			Form.DrawImage(e.Graphics, GuiUtils.GetResourceImage("about_logo"), new Rectangle(0, 0, 304, 80));
        }

        private void lnkGPL_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
			Core.UI.Actions.OpenUrlGpl();
        }

		private void lnkWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Core.UI.Actions.OpenUrlWebsite();
		}

		private void lnkManual_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Core.UI.Actions.OpenUrlDocs();
		}

		private void cmdClose_Click(object sender, EventArgs e)
		{
			Close();
		}
    }
}
