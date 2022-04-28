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
using Eddie.Core.UI;
using System;
using System.Drawing;
using System.Windows.Forms;

#pragma warning disable CA1416 // Windows only

namespace Eddie.Forms.Controls
{
	public class ChartSpeed : System.Windows.Forms.Control
	{
		public static Color ColorsLightChartBackground = Color.FromArgb(248, 248, 248);
		public static Color ColorsLightChartGrid = Color.FromArgb(211, 211, 211);
		public static Color ColorsLightChartAxis = Color.FromArgb(128, 128, 128);
		public static Color ColorsLightChartMouse = Color.FromArgb(32, 92, 166);
		public static Color ColorsLightChartLegend = Color.FromArgb(64, 145, 255);
		public static Color ColorsLightChartLineDownload = Color.FromArgb(38, 22, 255);
		public static Color ColorsLightChartLineUpload = Color.FromArgb(0, 90, 0);

		private int m_chartIndex = 0;
		private Chart m_chart;

		private Pen m_penGrid;
		private Pen m_penMouse;
		private Brush m_brushMouse;
		private Pen m_penDownloadGraph;
		private Pen m_penDownloadLine;
		private Pen m_penUploadGraph;
		private Pen m_penUploadLine;
		//private Brush m_brushLegendText;
		private Brush m_brushDownloadText;
		private Brush m_brushUploadText;

		private Font FontLabel;
		private StringFormat formatTopCenter;
		private StringFormat formatRight;
		private StringFormat formatBottomRight;
		private StringFormat formatBottomLeft;
		private StringFormat formatTopLeft;
		private StringFormat formatTopRight;

		private int m_legendDY = 0; // 15
		private int m_marginTopY = 15;

		private float m_chartDX;
		private float m_chartDY;
		private float m_chartStartX;
		private float m_chartStartY;

		public ChartSpeed()
		{
			m_penGrid = new Pen(ColorsLightChartGrid, 1);
			m_penMouse = new Pen(ColorsLightChartMouse, 1);
			m_brushMouse = new SolidBrush(ColorsLightChartMouse);
			m_penDownloadGraph = new Pen(ColorsLightChartLineDownload, 1);
			m_penDownloadLine = new Pen(ColorsLightChartLineDownload, 1);
			m_penUploadGraph = new Pen(ColorsLightChartLineUpload, 1);
			m_penUploadLine = new Pen(ColorsLightChartLineUpload, 1);
			//m_brushLegendText = new SolidBrush(Colors.LightChartLegend);
			m_brushDownloadText = new SolidBrush(ColorsLightChartLineDownload);
			m_brushUploadText = new SolidBrush(ColorsLightChartLineUpload);

			FontLabel = new Font("Small Fonts", 7);

			formatRight = new StringFormat();
			formatRight.Alignment = StringAlignment.Far;
			formatRight.LineAlignment = StringAlignment.Center;
			formatBottomRight = new StringFormat();
			formatBottomRight.Alignment = StringAlignment.Far;
			formatBottomRight.LineAlignment = StringAlignment.Far;
			formatBottomLeft = new StringFormat();
			formatBottomLeft.Alignment = StringAlignment.Near;
			formatBottomLeft.LineAlignment = StringAlignment.Far;
			formatTopRight = new StringFormat();
			formatTopRight.Alignment = StringAlignment.Far;
			formatTopRight.LineAlignment = StringAlignment.Near;
			formatTopLeft = new StringFormat();
			formatTopLeft.Alignment = StringAlignment.Near;
			formatTopLeft.LineAlignment = StringAlignment.Near;
			formatTopCenter = new StringFormat();
			formatTopCenter.Alignment = StringAlignment.Center;
			formatTopCenter.LineAlignment = StringAlignment.Near;

			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);

			m_chart = Engine.Instance.Stats.Charts.ChartsList[m_chartIndex];

			Engine.Instance.Stats.Charts.UpdateEvent += new Core.UI.Charts.UpdateHandler(Charts_UpdateEvent);
		}

		void Charts_UpdateEvent(object sender, EventArgs e)
		{
			Invalidate();
		}

		public string ValToDesc(Int64 v)
		{
			string r = LanguageManager.FormatBytes(v, true, true);
			return r;
		}

		public void Switch(int chartIndex)
		{
			m_chartIndex = chartIndex;
			if ((m_chartIndex < 0) || (m_chartIndex >= Engine.Instance.Stats.Charts.ChartsList.Count))
				m_chartIndex = 0;

			m_chart = Engine.Instance.Stats.Charts.ChartsList[m_chartIndex];

			Invalidate();
		}

		private Rectangle ChartRectangle(float x, float y, float w, float h)
		{
			return new Rectangle(Conversions.ToInt32(x), Conversions.ToInt32(y), Conversions.ToInt32(w), Conversions.ToInt32(h));
		}

		private Point ChartPoint(float x, float y)
		{
			return new Point(Conversions.ToInt32(x), Conversions.ToInt32(y));
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			Invalidate();
		}

		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
			//base.OnPaintBackground(pevent);			
		}

		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
		{
			//base.OnPaint(e);            
			Skin.SkinForm.Skin.GraphicsCommon(e.Graphics);

			try
			{
				int DX = this.ClientRectangle.Width;
				int DY = this.ClientRectangle.Height;

				//e.Graphics.FillRectangle(BrushBackground, this.ClientRectangle);				
				Skin.SkinForm.DrawImage(e.Graphics, GuiUtils.GetResourceImage("tab_l_bg"), new Rectangle(0, 0, ClientSize.Width, ClientSize.Height));

				m_chartDX = this.ClientRectangle.Width;
				m_chartDY = this.ClientRectangle.Height - m_legendDY;
				m_chartStartX = 0;
				m_chartStartY = m_chartDY;


				float maxY = m_chart.GetMax();
				if (maxY <= 0)
					maxY = 4096;
				else if (maxY > 1000000000000)
					maxY = 1000000000000;

				Point lastPointDown = new Point(-1, -1);
				Point lastPointUp = new Point(-1, -1);

				float stepX = (m_chartDX - 0) / m_chart.Resolution;

				// Grid lines				
				for (int g = 0; g < m_chart.Grid; g++)
				{
					float x = ((m_chartDX - 0) / m_chart.Grid) * g;
					e.Graphics.DrawLine(m_penGrid, m_chartStartX + x, 0, m_chartStartX + x, m_chartStartY);
				}

				// Axis line
				e.Graphics.DrawLine(Pens.Gray, 0, m_chartStartY, m_chartDX, m_chartStartY);

				// Legend
				/*
				{
					string legend = "";
					legend += Messages.ChartRange + ": " + Utils.FormatSeconds(m_chart.Resolution * m_chart.TimeStep);
					legend += "   ";
					legend += Messages.ChartGrid + ": " + Utils.FormatSeconds(m_chart.Resolution / m_chart.Grid * m_chart.TimeStep);
					legend += "   ";
					legend += Messages.ChartStep + ": " + Utils.FormatSeconds(m_chart.TimeStep);

					Point mp = Cursor.Position;
					mp = PointToClient(mp);
					if ((mp.X > 0) && (mp.Y < chartDX) && (mp.Y > chartDY) && (mp.Y < DY))
						legend += " - " + Messages.ChartClickToChangeResolution;

					e.Graphics.DrawString(legend, FontLabel, BrushLegendText, ChartRectangle(0, chartStartY, chartDX, m_legendDY), formatTopCenter);
				}
				*/

				// Graph
				for (int i = 0; i < m_chart.Resolution; i++)
				{
					int p = i + m_chart.Pos + 1;
					if (p >= m_chart.Resolution)
						p -= m_chart.Resolution;

					float downY = ((m_chart.Download[p]) * (m_chartDY - m_marginTopY)) / maxY;
					float upY = ((m_chart.Upload[p]) * (m_chartDY - m_marginTopY)) / maxY;

					Point pointDown = ChartPoint(m_chartStartX + stepX * i, m_chartStartY - downY);
					Point pointUp = ChartPoint(m_chartStartX + stepX * i, m_chartStartY - upY);

					//e.Graphics.DrawLine(Pens.Green, new Point(0,0), point);

					if (lastPointDown.X != -1)
					{
						e.Graphics.DrawLine(m_penDownloadGraph, lastPointDown, pointDown);
						e.Graphics.DrawLine(m_penUploadGraph, lastPointUp, pointUp);
					}

					lastPointDown = pointDown;
					lastPointUp = pointUp;
				}

				// Download line
				float downCurY = 0;
				{
					long v = m_chart.GetLastDownload();
					downCurY = ((v) * (m_chartDY - m_marginTopY)) / maxY;
					e.Graphics.DrawLine(m_penDownloadLine, 0, m_chartStartY - downCurY, m_chartDX, m_chartStartY - downCurY);
					Skin.SkinForm.DrawStringOutline(e.Graphics, LanguageManager.GetText("ChartDownload") + ": " + ValToDesc(v), FontLabel, m_brushDownloadText, ChartRectangle(0, 0, m_chartDX, m_chartStartY - downCurY), formatBottomRight);
				}

				// Upload line
				{
					long v = m_chart.GetLastUpload();
					float y = ((v) * (m_chartDY - m_marginTopY)) / maxY;
					float dly = 0;
					if (Math.Abs(downCurY - y) < 10) dly = 15; // Download and upload overwrap, distance it.
					e.Graphics.DrawLine(m_penUploadLine, 0, m_chartStartY - y, m_chartDX, m_chartStartY - y);
					Skin.SkinForm.DrawStringOutline(e.Graphics, LanguageManager.GetText("ChartUpload") + ": " + ValToDesc(v), FontLabel, m_brushUploadText, ChartRectangle(0, 0, m_chartDX, m_chartStartY - y - dly), formatBottomRight);
				}

				// Mouse lines
				{
					Point mp = Cursor.Position;
					mp = PointToClient(mp);

					if ((mp.X > 0) && (mp.Y < m_chartDX) && (mp.Y > 0) && (mp.Y < m_chartDY))
					{
						e.Graphics.DrawLine(m_penMouse, 0, mp.Y, m_chartDX, mp.Y);
						e.Graphics.DrawLine(m_penMouse, mp.X, 0, mp.X, m_chartDY);

						float i = (m_chartDX - (mp.X - m_chartStartX)) / stepX;

						int t = Conversions.ToInt32(i * m_chart.TimeStep);

						//float y = mp.Y * maxY / (chartDY - m_marginTopY);
						float y = (m_chartStartY - (mp.Y - m_marginTopY)) * maxY / m_chartDY;

						String label = ValToDesc(Conversions.ToInt64(y)) + ", " + LanguageManager.FormatSeconds(t) + " ago";

						StringFormat formatAlign = formatBottomLeft;
						Rectangle rect = new Rectangle();
						//if(DX - mp.X > DX / 2)
						if (mp.X < DX - 150)
						{
							//if (DY - mp.Y > DY / 2)
							if (mp.Y < 20)
							{
								formatAlign = formatTopLeft;
								rect.X = mp.X + 20;
								rect.Y = mp.Y + 5;
								rect.Width = DX;
								rect.Height = DX;
							}
							else
							{
								formatAlign = formatBottomLeft;
								rect.X = mp.X + 20;
								rect.Y = 0;
								rect.Width = DX;
								rect.Height = mp.Y - 5;
							}
						}
						else
						{
							//if (DY - mp.Y > DY / 2)
							if (mp.Y < 20)
							{
								formatAlign = formatTopRight;
								rect.X = 0;
								rect.Y = mp.Y;
								rect.Width = mp.X - 20;
								rect.Height = DY;
							}
							else
							{
								formatAlign = formatBottomRight;
								rect.X = 0;
								rect.Y = 0;
								rect.Width = mp.X - 20;
								rect.Height = mp.Y - 5;

							}
						}

						Skin.SkinForm.DrawStringOutline(e.Graphics, label, FontLabel, m_brushMouse, rect, formatAlign);
					}
				}

			}
			catch
			{
			}
		}

		private void InitializeComponent()
		{
			this.SuspendLayout();
			this.ResumeLayout(false);

		}

	}
}

#pragma warning restore CA1416 // Windows only
