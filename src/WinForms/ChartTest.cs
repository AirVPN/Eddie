using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AirVPN.Gui
{
	public partial class ChartTest : Form
	{
		private AirVPN.Gui.Controls.ChartSpeed chart;

		public ChartTest()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			chart = new AirVPN.Gui.Controls.ChartSpeed();
			chart.Dock = DockStyle.Fill;
			Controls.Add(chart);

			chart.Charts.ReadEvent += new AirVPN.Core.Charts.ReadHandler(chart_ReadEvent);			
		}

		protected override void OnClosed(EventArgs e)
		{
			chart.Stop();
		}
		void chart_ReadEvent(ref long download, ref long upload)
		{
			download = Convert.ToInt32(numDown.Value);
			upload = Convert.ToInt32(numUp.Value);
		}
	}
}
