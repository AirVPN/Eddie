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
using Eddie.Forms.Skin;
using System;
using System.Drawing;
using System.Windows.Forms;

#pragma warning disable CA1416 // Windows only

namespace Eddie.Forms.Forms
{
	public partial class WindowSplash : Eddie.Forms.Skin.SkinForm
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

			SkinForm.DrawString(e.Graphics, Body, Font, Brushes.White, r, m_sf);
		}

		private delegate void SetStatusDelegate(string t);
		public void SetStatus(string t)
		{
			try
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
			catch (Exception ex)
			{
				Engine.Instance.Logs.LogUnexpected(ex);
			}
		}

		private delegate void OnMessageErrorDelegate(string message);
		public void OnMessageError(string message)
		{
			try
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
			catch (Exception ex)
			{
				Engine.Instance.Logs.LogUnexpected(ex);
			}
		}

		private delegate void RequestCloseDelegate();
		public void RequestClose()
		{
			try
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
			catch (Exception ex)
			{
				Engine.Instance.Logs.LogUnexpected(ex);
			}
		}

		private delegate void RequestCloseForReadyDelegate();
		public void RequestCloseForReady()
		{
			try
			{
				SetStatus(LanguageManager.GetText(LanguageItems.Ready));

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
			catch (Exception ex)
			{
				Engine.Instance.Logs.LogUnexpected(ex);
			}
		}

		// Unlike other platform, this can't be in MainWindow, because if the Forms.Windows it's not showed, this.InvokeRequired works wrong.
		private delegate void RequestMainDelegate();
		public void RequestMain()
		{
			try
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
			catch (Exception ex)
			{
				Engine.Instance.Logs.LogUnexpected(ex);
			}
		}

		private delegate string AskUnlockPasswordDelegate(bool authFailed);
		public string AskUnlockPassword(bool authFailed)
		{
			try
			{
				if (this.InvokeRequired)
				{
					AskUnlockPasswordDelegate inv = new AskUnlockPasswordDelegate(this.AskUnlockPassword);

					return (string)this.Invoke(inv, new object[] { authFailed });
				}
				else
				{
					Forms.WindowUnlock dlg = new Forms.WindowUnlock();
					dlg.AuthFailed = authFailed;
					dlg.ShowDialog(UiClient.Instance.SplashWindow);
					return dlg.Body;
				}
			}
			catch (Exception ex)
			{
				Engine.Instance.Logs.LogUnexpected(ex);
				return "";
			}
		}

		private void tmrTimer_Tick(object sender, EventArgs e)
		{
			Close();
			UiClient.Instance.SplashWindow = null;
		}
	}
}

#pragma warning restore CA1416 // Windows only