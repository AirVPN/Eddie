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
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using Eddie.Common;
using Eddie.Core;

namespace Eddie.Forms.Forms
{
	public partial class About : Eddie.Forms.Form
	{
		public About()
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

			lblDeveloped.Font = Skin.FontBig;
			
			lblVersion.Font = Skin.FontBig;

			lblVersion.ForeColor = Color.White;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			CommonInit(Messages.WindowsAboutTitle);

			lnkWebsite.Text = Core.UI.App.Manifest["links"]["help"]["website"].Value as string;
			lnkManual.Text = Core.UI.App.Manifest["links"]["help"]["general"].Value as string;
			lnkSources.Text = Core.UI.App.Manifest["links"]["github"].Value as string;
			lblVersion.Text = Messages.WindowsAboutVersion + " " + Constants.VersionDesc;
			
			lblThanks.Text = MessagesFormatter.Format(Messages.WindowsAboutThanks, String.Join(", ", Constants.Thanks.Split(';')));
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			Skin.GraphicsCommon(e.Graphics);

			//Form.DrawImage(e.Graphics, GuiUtils.GetResourceImage("about_header_bg"), new Rectangle(0, 0, ClientSize.Width + 50, 88 + 2)); // +50 and +2 to avoid GDI+ problem
			Form.DrawImage(e.Graphics, GuiUtils.GetResourceImage("about_header_bg"), new Rectangle(0, 0, ClientSize.Width + 50, 88)); // +50 and +2 to avoid GDI+ problem
			Form.DrawImage(e.Graphics, GuiUtils.GetResourceImage("about_logo"), new Rectangle(0, 0, 392, 88));
		}

		private void lnkWebsite_LinkClicked(object sender, EventArgs e)
		{
			Core.UI.App.OpenUrl(Core.UI.App.Manifest["links"]["help"]["website"].Value as string);
		}

		private void lnkManual_LinkClicked(object sender, EventArgs e)
		{
			Core.UI.App.OpenUrl(Core.UI.App.Manifest["links"]["help"]["general"].Value as string);
		}

		private void lnkSources_LinkClicked(object sender, EventArgs e)
		{
			Core.UI.App.OpenUrl(Core.UI.App.Manifest["links"]["github"].Value as string);
		}

		private void cmdClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void lnkLicense_LinkClicked(object sender, EventArgs e)
		{
			Engine.FormMain.ShowText(this, "License", Core.UI.App.Manifest["about"]["license"].Value as string);
		}

		private void lnkLibraries_LinkClicked(object sender, EventArgs e)
		{
			Engine.FormMain.ShowText(this, "Libraries and Tools", Core.UI.App.Manifest["about"]["libraries"].Value as string);
		}

		private void cmdSystemReport_Click(object sender, EventArgs e)
		{
			Engine.GenerateSystemReport();
		}

		private void lnkAirVPN_Click(object sender, EventArgs e)
		{
			Core.UI.App.OpenUrl("https://airvpn.org");
		}
	}
}
