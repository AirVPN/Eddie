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
    public partial class WindowSplash : Eddie.Forms.Form
    {		
        public String Body;

		private StringFormat m_sf;
		private bool m_closePending = false;

		public WindowSplash()
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

			m_sf = new StringFormat();
			m_sf.Alignment = StringAlignment.Far;
			m_sf.LineAlignment = StringAlignment.Far;
			m_sf.FormatFlags = StringFormatFlags.NoWrap;
			m_sf.Trimming = StringTrimming.None;

			CommonInit("");

			SetStatus(LanguageManager.MessageInitialization);
		}
		
		protected override void OnPaintBackground(PaintEventArgs e)
		{
			base.OnPaintBackground(e);

			Rectangle r = ClientRectangle;
			r.Width -= 10;
			r.Height -= 10;

			Form.DrawString(e.Graphics, Body, Font, Brushes.White, r, m_sf);
		}		

		private delegate void SetStatusDelegate(string t);
		public void SetStatus(string t)
		{
			if (this.InvokeRequired)
			{
				SetStatusDelegate inv = new SetStatusDelegate(this.SetStatus);

				this.Invoke(inv, new object[] { t });
			}
			else
			{
				Body = t;
				Refresh();
				Invalidate();
				//TopMost = true;

				BringToFront();
			}
		}

		private delegate void OnMessageErrorDelegate(string message);
		public void OnMessageError(string message)
		{
			if (this.InvokeRequired)
			{
				OnMessageErrorDelegate inv = new OnMessageErrorDelegate(this.OnMessageError);
				this.Invoke(inv, new object[] { message });
			}
			else
			{
				GuiUtils.MessageBoxError(this, message);
			}
		}

		private delegate void RequestCloseDelegate();
		public void RequestClose()
		{
			if (m_closePending)
				return;

			if (this.InvokeRequired)
			{
				RequestCloseDelegate inv = new RequestCloseDelegate(this.RequestClose);

				this.Invoke(inv, new object[] { });
			}
			else
			{
				Close();
				UiClient.Instance.SplashWindow = null;
			}
		}

		private delegate void RequestCloseForReadyDelegate();
		public void RequestCloseForReady()
		{
			SetStatus(LanguageManager.GetText("Ready"));

			if (m_closePending)
				return;

			if (this.InvokeRequired)
			{
				RequestCloseForReadyDelegate inv = new RequestCloseForReadyDelegate(this.RequestCloseForReady);

				this.Invoke(inv, new object[] { });
			}
			else
			{
				m_closePending = true;

				tmrTimer.Enabled = true;
			}
		}

		// Unlike other platform, this can't be in MainWindow, because if the Forms.Windows it's not showed, this.InvokeRequired works wrong.
		private delegate void RequestMainDelegate();
		public void RequestMain()
		{
			if (this.InvokeRequired)
			{
				RequestMainDelegate inv = new RequestMainDelegate(this.RequestMain);

				this.Invoke(inv, new object[] { });
			}
			else
			{
				UiClient.Instance.MainWindow = new Forms.Main();
				UiClient.Instance.MainWindow.RequestShow();
			}
		}

		private delegate string AskUnlockPasswordDelegate(bool authFailed);
		public string AskUnlockPassword(bool authFailed)
		{
			if (this.InvokeRequired)
			{
				AskUnlockPasswordDelegate inv = new AskUnlockPasswordDelegate(this.AskUnlockPassword);

				return (string) this.Invoke(inv, new object[] { authFailed  });
			}
			else
			{
				Forms.WindowUnlock dlg = new Forms.WindowUnlock();
				dlg.AuthFailed = authFailed;				
				dlg.ShowDialog(UiClient.Instance.SplashWindow);
				return dlg.Body;
			}
		}

		private void tmrTimer_Tick(object sender, EventArgs e)
		{
			Close();
			UiClient.Instance.SplashWindow = null;
		}
	}
}
