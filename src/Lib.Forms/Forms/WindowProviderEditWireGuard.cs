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

using Eddie.Core;
using Eddie.Forms.Skin;
using System;

namespace Eddie.Forms.Forms
{
	public partial class WindowProviderEditWireGuard : Eddie.Forms.Skin.SkinForm
	{
		public Core.Providers.WireGuard Provider;

		public WindowProviderEditWireGuard()
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
			SkinUtils.FixHeightVs(lblTitle2, txtTitle2);
			SkinUtils.FixHeightVs(lblPath, txtPath);
			SkinUtils.FixHeightVs(lblPath, cmdPathBrowse);
			SkinUtils.FixHeightVs(lblAuthPassUsername, txtAuthPassUsername);
			SkinUtils.FixHeightVs(lblAuthPassPassword, txtAuthPassPassword);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			CommonInit(LanguageManager.GetText("WindowsProviderEditWireGuardTitle"));

			lblTitle.Text = Provider.DefinitionTitle;
			lblSubtitle.Text = Provider.DefinitionSubTitle;

			chkEnabled.Checked = Provider.Enabled;
			txtTitle2.Text = Provider.Title;
			txtPath.Text = Provider.Path;

			chkSupportIPv6.Text = LanguageManager.GetText("WindowsProviderEditWireGuardSupportIPv6");
			chkSupportIPv6.Checked = Provider.SupportIPv6;

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
			Provider.SupportIPv6 = chkSupportIPv6.Checked;

			Provider.AuthPassUsername = txtAuthPassUsername.Text;
			Provider.AuthPassPassword = txtAuthPassPassword.Text;
		}

		private void cmdPathBrowse_Click(object sender, EventArgs e)
		{
			string result = GuiUtils.DirectoryPicker(LanguageManager.GetText("WindowsProviderEditWireGuardPathBrowse"), txtPath.Text);
			if (result != "")
				txtPath.Text = result;
		}

		private void lblTitle_Click(object sender, EventArgs e)
		{
			GuiUtils.OpenUrl(Provider.DefinitionHref);
		}
	}
}
