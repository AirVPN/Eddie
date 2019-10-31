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
	public partial class WindowUnlock : Eddie.Forms.Form
	{
		public bool AuthFailed = false;
		public string Body = "";

		public WindowUnlock()
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
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			CommonInit(LanguageManager.GetText("WindowsUnlockTitle"));

			if (AuthFailed)
			{
				lblMessage.Text = LanguageManager.GetText("WindowsUnlockFailedAuth");
				lblMessage.ForeColor = Color.Red;
			}
			else
			{
				lblMessage.Text = LanguageManager.GetText("WindowsUnlockFirstAuth");
				lblMessage.ForeColor = Color.Black;
			}

			EnableIde();
		}

		private void EnableIde()
		{
			cmdOk.Enabled = (txtText.Text != "");
		}

		private void cmdOk_Click(object sender, EventArgs e)
		{
			Body = txtText.Text;
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			Body = "";
		}

		private void txtText_TextChanged(object sender, EventArgs e)
		{
			EnableIde();
		}
	}
}
