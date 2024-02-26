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
using System;
using System.IO;
using System.Windows.Forms;

namespace Eddie.Forms.Forms
{
	public partial class WindowReport : Eddie.Forms.Skin.SkinForm
	{
		public string Body = "";

		private Controls.ToolTip m_toolTip;

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

			CommonInit(LanguageManager.GetText(LanguageItems.WindowsReportTitle));

			CheckEnabled();

			if (GuiUtils.IsWindows())
			{
				// TOFIX: Under Mono crash...
				m_toolTip = new Controls.ToolTip();
				Controls.Add(m_toolTip);
				m_toolTip.BringToFront();

				m_toolTip.Connect(this.cmdCopyClipboard, LanguageManager.GetText(LanguageItems.TooltipLogsCopy));
				m_toolTip.Connect(this.cmdSave, LanguageManager.GetText(LanguageItems.TooltipLogsSave));
			}

			txtBody.Text = "";
			pgrStep.Minimum = 0;
			pgrStep.Maximum = 100;
		}

		public void SetStep(string step, string text, int perc)
		{
			lblStep.Text = step;
			pgrStep.Value = perc;
			txtBody.Text = text;
			cmdCopyClipboard.Enabled = (perc == 100);
			cmdSave.Enabled = (perc == 100);

			// For refresh, especially Mono-Linux
			Invalidate();
			Update();
			Refresh();
			Application.DoEvents();
		}

		void CheckEnabled()
		{
		}

		private void cmdCopyClipboard_Click(object sender, EventArgs e)
		{
			GuiUtils.ClipboardSetText(this, txtBody.Text);
		}

		private void cmdSave_Click(object sender, EventArgs e)
		{
			string t = txtBody.Text;

			using (SaveFileDialog sd = new SaveFileDialog())
			{
				sd.Filter = LanguageManager.GetText(LanguageItems.FilterTextFiles);

				if (sd.ShowDialog() == DialogResult.OK)
				{
					using (StreamWriter sw = new StreamWriter(sd.FileName))
					{
						sw.Write(t);
						sw.Flush();
						//sw.Close();	// because of "using"
					}

					GuiUtils.MessageBoxInfo(this, LanguageManager.GetText(LanguageItems.LogsSaveToFileDone));
				}
			}
		}

		private void cmdOk_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void cmdUpload_Click(object sender, EventArgs e)
		{
			txtUploadUrl.Text = "Uploading...";
			string url = Engine.Instance.UploadReport(txtBody.Text);
			txtUploadUrl.Text = url;
			if (url == "")
				GuiUtils.MessageBoxError(this, "Failed to upload, please retry");
			else
			{
				GuiUtils.ClipboardSetText(this, url);
			}
		}
	}
}