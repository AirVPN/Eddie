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
using System;
using System.Drawing;

namespace Eddie.Forms.Forms
{
	public partial class WindowMessage : Eddie.Forms.Skin.SkinForm
	{
		public enum MessageKind
		{
			Info = 0,
			Error = 1,
			YesNo = 2
		}

		public MessageKind Kind = WindowMessage.MessageKind.Info;
		public String Body;
		public bool YesNoAnswer = false;

		public WindowMessage()
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
			CommonInit(LanguageManager.GetText("WindowsMessageTitle"));

			cmdYes.Visible = false;
			cmdNo.Visible = false;
			cmdOk.Visible = false;

			if (Kind == MessageKind.Info)
			{
				cmdOk.Visible = true;
				cmdOk.Text = LanguageManager.GetText("Ok");
				AcceptButton = cmdOk;
				CancelButton = cmdOk;
				lblIcon.Image = Properties.Resources.log_info;
			}
			else if (Kind == MessageKind.Error)
			{
				cmdOk.Visible = true;
				cmdOk.Text = LanguageManager.GetText("Ok");
				AcceptButton = cmdOk;
				CancelButton = cmdOk;
				lblIcon.Image = Properties.Resources.log_error;
			}
			else if (Kind == MessageKind.YesNo)
			{
				cmdYes.Visible = true;
				cmdYes.Text = LanguageManager.GetText("Yes");
				cmdNo.Visible = true;
				cmdNo.Text = LanguageManager.GetText("No");
				CancelButton = cmdNo;
				AcceptButton = cmdYes;
				lblIcon.Image = Properties.Resources.log_warning;
			}

			lblMessage.Text = Body;

			Graphics g = this.CreateGraphics();
			SizeF s = g.MeasureString(Body, this.Font, 600);
			if (s.Width < 300)
				s.Width = 300;
			if (s.Height < 50)
				s.Height = 50;
			this.ClientSize = new Size((int)s.Width + 128 + 30, (int)s.Height + 100);

			CenterToScreen();

			Activate();
		}

		private void cmdOk_Click(object sender, EventArgs e)
		{
		}

		private void cmdYes_Click(object sender, EventArgs e)
		{
			YesNoAnswer = true;
		}

		private void cmdNo_Click(object sender, EventArgs e)
		{
			YesNoAnswer = false;
		}
	}
}
