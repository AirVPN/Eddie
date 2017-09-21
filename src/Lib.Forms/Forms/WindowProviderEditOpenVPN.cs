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

namespace Eddie.Forms.Forms
{
    public partial class WindowProviderEditOpenVPN : Eddie.Forms.Form
    {
		public Core.Providers.OpenVPN Provider;

		public WindowProviderEditOpenVPN()
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

			lblTitle.Font = Skin.FontBig;
			GuiUtils.FixHeightVs(lblTitle2, txtTitle2);
			GuiUtils.FixHeightVs(lblPath, txtPath);
			GuiUtils.FixHeightVs(lblPath, cmdPathBrowse);
			GuiUtils.FixHeightVs(lblAuthPassUsername, txtAuthPassUsername);
			GuiUtils.FixHeightVs(lblAuthPassPassword, txtAuthPassPassword);
		}

        protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			CommonInit(Messages.WindowsProviderEditOpenVPNTitle);

			lblTitle.Text = Provider.DefinitionTitle;
			lblSubtitle.Text = Provider.DefinitionSubTitle;

			chkEnabled.Checked = Provider.Enabled;
			txtTitle2.Text = Provider.Title;
			txtPath.Text = Provider.Path;

			txtAuthPassUsername.Text = Provider.AuthPassUsername;
			txtAuthPassPassword.Text = Provider.AuthPassPassword;

			EnableIde();
		}
        
		private void EnableIde()
		{			
		}

        private void cmdOk_Click(object sender, EventArgs e)
        {
			Provider.Enabled = chkEnabled.Checked;
			Provider.Title = txtTitle2.Text;
			Provider.Path = txtPath.Text;

			Provider.AuthPassUsername = txtAuthPassUsername.Text;
			Provider.AuthPassPassword = txtAuthPassPassword.Text;
		}

		private void cmdPathBrowse_Click(object sender, EventArgs e)
		{
			string result = GuiUtils.DirectoryPicker(Messages.WindowsProviderEditOpenVPNPathBrowse, txtPath.Text);
			if (result != "")
				txtPath.Text = result;				
		}

		private void lblTitle_Click(object sender, EventArgs e)
		{
			Platform.Instance.OpenUrl(Provider.DefinitionHref);
		}
	}
}
