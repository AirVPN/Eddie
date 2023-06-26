// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2023 AirVPN (support@airvpn.org) / https://airvpn.org
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
	public partial class WindowCredentials : Eddie.Forms.Skin.SkinForm
	{
		public Credentials Credentials;

		public WindowCredentials()
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

			SkinUtils.FixHeightVs(txtUsername, lblUsername);
			SkinUtils.FixHeightVs(txtPassword, lblPassword);
			SkinUtils.FixHeightVs(lblRemember, cboRemember);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			CommonInit(LanguageManager.GetText(LanguageItems.WindowsLoginTitle));

			cboRemember.Items.Add(LanguageManager.GetText(LanguageItems.WindowsCredentialsRememberNo));
			cboRemember.Items.Add(LanguageManager.GetText(LanguageItems.WindowsCredentialsRememberRun));
			cboRemember.Items.Add(LanguageManager.GetText(LanguageItems.WindowsCredentialsRememberPermanent));
			cboRemember.Text = LanguageManager.GetText(LanguageItems.WindowsCredentialsRememberRun);

			EnableIde();
		}

		private void EnableIde()
		{
			bool acceptable = true;
			if (txtUsername.Text.Trim() == "")
				acceptable = false;
			if (txtPassword.Text.Trim() == "")
				acceptable = false;
			cmdOk.Enabled = acceptable;
		}

		private void cmdOk_Click(object sender, EventArgs e)
		{
			Credentials = new Credentials();

			Credentials.UserName = txtUsername.Text;
			Credentials.Password = txtPassword.Text;
			if (cboRemember.Text == LanguageManager.GetText(LanguageItems.WindowsCredentialsRememberNo))
				Credentials.Remember = "no";
			else if (cboRemember.Text == LanguageManager.GetText(LanguageItems.WindowsCredentialsRememberRun))
				Credentials.Remember = "run";
			else if (cboRemember.Text == LanguageManager.GetText(LanguageItems.WindowsCredentialsRememberPermanent))
				Credentials.Remember = "permanent";
			else
				Credentials.Remember = "no";
		}

		private void txtUsername_TextChanged(object sender, EventArgs e)
		{
			EnableIde();
		}

		private void txtPassword_TextChanged(object sender, EventArgs e)
		{
			EnableIde();
		}
	}
}
