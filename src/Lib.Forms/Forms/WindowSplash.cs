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
using Eddie.Common;

namespace Eddie.Forms.Forms
{
    public partial class WindowSplash : Eddie.Forms.Form
    {
        public String Body;

		private StringFormat m_sf;

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

			EnableIde();
		}
		
		protected override void OnPaintBackground(PaintEventArgs e)
		{
			base.OnPaintBackground(e);

			Rectangle r = ClientRectangle;
			r.Width -= 10;
			r.Height -= 10;

			Form.DrawString(e.Graphics, Body, Font, Brushes.White, r, m_sf);
		}		

		public void SetStatus(string t)
		{
			Body = t;
			Refresh();
			Invalidate();
		}

		public void RequestClose()
		{
			Close();
		}
        
		private void EnableIde()
		{			
		}

	}
}
