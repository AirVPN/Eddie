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
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

namespace Eddie.Forms.Controls
{
    public class ToolTip : System.Windows.Forms.Label
    {
		Dictionary<Control, string> Texts = new Dictionary<Control, string>();

		public ToolTip()
        {            
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            BackColor = Color.White;
            BorderStyle = BorderStyle.FixedSingle;
            Visible = false;

            FlatStyle = System.Windows.Forms.FlatStyle.Flat;

            AutoSize = true;
        }
        
        public void Connect(Control ctl, string text)
        {
            ctl.MouseHover += Ctl_MouseHover;
            ctl.MouseLeave += Ctl_MouseLeave;

            Texts[ctl] = text;
        }

        public void Show(Control ctl)
        {
            Text = Texts[ctl];
            Point p = Cursor.Position;
            p = this.Parent.PointToClient(p);
            Left = p.X - this.Width;
            Top = p.Y - this.Height -3;            
            Visible = true;
            Refresh();       
        }

        public void Hide(Control ctl)
        {
            Visible = false;
        }

        private void Ctl_MouseLeave(object sender, EventArgs e)
        {
            Hide(sender as Control);
        }

        private void Ctl_MouseHover(object sender, EventArgs e)
        {
            Show(sender as Control);
        }
    }
}
