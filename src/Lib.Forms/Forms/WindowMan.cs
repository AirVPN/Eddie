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
using System.IO;
using System.Text;
using System.Windows.Forms;
using Eddie.Core;

namespace Eddie.Forms.Forms
{
    public partial class WindowMan : Eddie.Forms.Form
    {
		public WindowMan()
        {
            OnPreInitializeComponent();
            InitializeComponent();
            OnInitializeComponent();
        }

		public override void OnApplySkin()
		{
			base.OnApplySkin();

			//txtBody.Font = Skin.FontMono;
		}

		protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            CommonInit(LanguageManager.GetText("WindowsManTitle"));

			CheckEnabled();

			foreach (Option option in Engine.Instance.Options.Dict.Values)
			{
				Controls.ListViewItemOption item = new Controls.ListViewItemOption();
				item.Text = option.Code;
				item.SubItems.Add(option.Man);
				item.SubItems.Add(option.Default);
				item.SubItems.Add(option.Value);
				item.BackColor = Color.Red;
				lstOptions.Items.Add(item);
			}

			/*
			txtBody.Text = "";
			pgrStep.Minimum = 0;
			pgrStep.Maximum = 100;
			*/

			pnlProviders.Controls.Add(new Controls.Provider());
			pnlProviders.Controls.Add(new Controls.Provider());
			pnlProviders.Controls.Add(new Controls.Provider());
			pnlProviders.Controls.Add(new Controls.Provider());
			pnlProviders.Controls.Add(new Controls.Provider());
			pnlProviders.Controls.Add(new Controls.Provider());
			pnlProviders.Controls.Add(new Controls.Provider());

			pnlProviders.Controls[1].Top = pnlProviders.Controls[0].Height;
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
			Refresh();			
		}

        void CheckEnabled()
        {   
        }
		
		private void cmdCopyClipboard_Click(object sender, EventArgs e)
		{
			Application.UseWaitCursor = true;

			GuiUtils.ClipboardSetText(txtBody.Text);

			Application.UseWaitCursor = false;

			ShowMessageInfo(LanguageManager.GetText("LogsCopyClipboardDone"));
		}

		private void cmdSave_Click(object sender, EventArgs e)
		{
			string t = txtBody.Text;

			using(SaveFileDialog sd = new SaveFileDialog())
			{
				sd.Filter = LanguageManager.GetText("FilterTextFiles");

				if (sd.ShowDialog() == DialogResult.OK)
				{
					using(StreamWriter sw = new StreamWriter(sd.FileName))
					{
						sw.Write(t);
						sw.Flush();
						//sw.Close();	// because of "using"
					}

					ShowMessageInfo(LanguageManager.GetText("LogsSaveToFileDone"));
				}
			}
		}

		private void cmdOk_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}