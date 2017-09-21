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
    public partial class FrontMessage : Eddie.Forms.Form
	{
		public string Message;

		public FrontMessage()
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

            lblMessage.Font = Skin.FontBig;
        }

        protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			lblMessage.Text = Message;
			cmdClose.Text = Messages.WindowsFrontMessageAccept;
			lnkWebsite.Text = Messages.WindowsFrontMessageMore;

			CommonInit(Messages.WindowsFrontMessageTitle);
		}

		private void lnkWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Engine.Instance.Command("ui.show.website");
        }

		private void cmdClose_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
