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
    public partial class WindowReport : Eddie.Gui.Form
    {
		public string Body = "";

        public WindowReport()
        {
            OnPreInitializeComponent();
            InitializeComponent();
            OnInitializeComponent();
        }

		public override void OnApplySkin()
		{
			base.OnApplySkin();

			txtBody.Font = Skin.FontMono;
		}

		protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            CommonInit(Messages.WindowsReportTitle);

			CheckEnabled();

			txtBody.Text = "";
		}

		public void SetStep(string step, string text, bool completed)
		{
			txtBody.Text = text;			
		}

        void CheckEnabled()
        {   
        }
		
		private void cmdCopyClipboard_Click(object sender, EventArgs e)
		{
			Application.UseWaitCursor = true;

			Clipboard.SetText(txtBody.Text);

			Application.UseWaitCursor = false;

			Engine.Instance.OnMessageInfo(Messages.LogsCopyClipboardDone);
		}

		private void cmdSave_Click(object sender, EventArgs e)
		{

		}

		private void cmdOk_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}