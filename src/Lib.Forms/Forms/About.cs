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
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using Eddie.Core;
using Eddie.Forms.Skin;

namespace Eddie.Forms.Forms
{
	public partial class About : Eddie.Forms.Skin.SkinForm
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

			CommonInit(LanguageManager.GetText("WindowsAboutTitle"));

			lnkWebsite.Text = UiClient.Instance.Data["links"]["help"]["website"].Value as string;
			lnkManual.Text = UiClient.Instance.Data["links"]["help"]["general"].Value as string;
			lnkSources.Text = UiClient.Instance.Data["links"]["github"].Value as string;
			lblVersion.Text = LanguageManager.GetText("WindowsAboutVersion", Engine.Instance.GetVersionShow());
			lblThanks.Text = LanguageManager.GetText("WindowsAboutThanks", Constants.Thanks);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			Skin.GraphicsCommon(e.Graphics);

			//Form.DrawImage(e.Graphics, GuiUtils.GetResourceImage("about_header_bg"), new Rectangle(0, 0, ClientSize.Width + 50, 88 + 2)); // +50 and +2 to avoid GDI+ problem
			SkinForm.DrawImage(e.Graphics, GuiUtils.GetResourceImage("about_header_bg"), new Rectangle(0, 0, ClientSize.Width + 50, 88)); // +50 and +2 to avoid GDI+ problem
			SkinForm.DrawImage(e.Graphics, GuiUtils.GetResourceImage("about_logo"), new Rectangle(0, 0, 392, 88));
		}

		private void lnkWebsite_LinkClicked(object sender, EventArgs e)
		{
			GuiUtils.OpenUrl(UiClient.Instance.Data["links"]["help"]["website"].Value as string);
		}

		private void lnkManual_LinkClicked(object sender, EventArgs e)
		{
			GuiUtils.OpenUrl(UiClient.Instance.Data["links"]["help"]["general"].Value as string);
		}

		private void lnkSources_LinkClicked(object sender, EventArgs e)
		{
			GuiUtils.OpenUrl(UiClient.Instance.Data["links"]["github"].Value as string);
		}

		private void cmdClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void lnkLicense_LinkClicked(object sender, EventArgs e)
		{
			UiClient.Instance.MainWindow.ShowText(this, "License", UiClient.Instance.Data["about"]["license"].Value as string);
		}

		private void lnkLibraries_LinkClicked(object sender, EventArgs e)
		{
			UiClient.Instance.MainWindow.ShowText(this, "Libraries and Tools", UiClient.Instance.Data["about"]["libraries"].Value as string);
		}

		private void cmdSystemReport_Click(object sender, EventArgs e)
		{
			UiClient.Instance.Command("system.report.start");
		}

		private void lnkAirVPN_Click(object sender, EventArgs e)
		{
			GuiUtils.OpenUrl("https://airvpn.org");
		}
	}
}
