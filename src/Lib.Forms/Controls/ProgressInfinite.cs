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

namespace Eddie.Gui.Controls
{
    public class ProgressInfinite : System.Windows.Forms.Label
    {
		private int m_step = 1;
		private Timer m_timer;

		public ProgressInfinite()
        {            
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

			BackColor = Color.Transparent;
			BackgroundImage = null;
            BackgroundImageLayout = ImageLayout.Stretch;
            
            FlatStyle = System.Windows.Forms.FlatStyle.Flat;

			m_timer = new Timer();
			m_timer.Interval = 50;			
			m_timer.Tick += new EventHandler(OnTimerTick);
			m_timer.Enabled = true;
			m_timer.Start();
        }

		void OnTimerTick(object sender, EventArgs e)
		{
			if(this.Visible)
				Invalidate();
		}

        protected override void OnPaint(PaintEventArgs pevent)
        {
			m_step++;
			if (m_step == 11)
				m_step = 1;
            //base.OnPaint(pevent);
			
            Rectangle r = ClientRectangle;

			Image img = GuiUtils.GetResourceImage("progress" + String.Format("{0:00}",m_step));

			Form.DrawImage(pevent.Graphics, img, r);
        }
    }
}
