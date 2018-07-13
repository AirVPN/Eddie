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
    public partial class Tos : Eddie.Forms.Form
    {
        public Tos()
        {
            OnPreInitializeComponent();
            InitializeComponent();
            OnInitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            CommonInit(Messages.WindowsTosTitle);

			txtTos.Text = Platform.Instance.NormalizeString(Core.UI.App.GetTos());
			chkAccept1.Text = Messages.WindowsTosCheck1;
			chkAccept2.Text = Messages.WindowsTosCheck2;
			cmdOk.Text = Messages.WindowsTosAccept;
			cmdCancel.Text = Messages.WindowsTosReject;

			bool TosAccepted = Engine.Instance.Storage.GetBool("gui.tos");
            chkAccept1.Checked = TosAccepted;
            chkAccept2.Checked = TosAccepted;
			
            CheckEnabled();

            Visible = true;
            chkAccept1.Focus();
        }

        void CheckEnabled()
        {
            cmdOk.Enabled = (chkAccept1.Checked && chkAccept2.Checked);
        }

        private void chkAccept1_CheckedChanged(object sender, EventArgs e)
        {
            CheckEnabled();
        }

        private void chkAccept2_CheckedChanged(object sender, EventArgs e)
        {
            CheckEnabled();
        }

	

        
    }
}