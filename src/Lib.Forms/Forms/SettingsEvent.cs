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
using Eddie.Forms.Skin;

namespace Eddie.Forms.Forms
{
	public partial class SettingsEvent : Eddie.Forms.Skin.SkinForm
	{
		public String FileName;
		public String Arguments;
		public bool WaitEnd;

		public SettingsEvent()
		{
			OnPreInitializeComponent();
			InitializeComponent();
			OnInitializeComponent();

			Load += new EventHandler(SettingsEvent_Load);
		}

		public override void OnApplySkin()
		{
			base.OnApplySkin();

			SkinUtils.FixHeightVs(txtFileName, lblFileName);
			SkinUtils.FixHeightVs(txtFileName, cmdExeBrowse);
			SkinUtils.FixHeightVs(txtArguments, lblArguments);
			SkinUtils.FixHeightVs(chkWaitEnd, lblWaitEnd);
		}

		void SettingsEvent_Load(object sender, EventArgs e)
		{
			CommonInit(LanguageManager.GetText("WindowsSettingsEventTitle"));

			txtFileName.Text = FileName;
			txtArguments.Text = Arguments;
			chkWaitEnd.Checked = WaitEnd;
		}

		private void cmdOk_Click(object sender, EventArgs e)
		{
			FileName = txtFileName.Text;
			Arguments = txtArguments.Text;
			WaitEnd = chkWaitEnd.Checked;
		}

		private void cmdExeBrowse_Click(object sender, EventArgs e)
		{
			string result = GuiUtils.FilePicker();
			if (result != "")
				txtFileName.Text = result;
		}

	}
}
