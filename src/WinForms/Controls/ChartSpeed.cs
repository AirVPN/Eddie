// <airvpn_source_header>
// This file is part of AirVPN Client software.
// Copyright (C)2014-2014 AirVPN (support@airvpn.org) / https://airvpn.org )
//
// AirVPN Client is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// AirVPN Client is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with AirVPN Client. If not, see <http://www.gnu.org/licenses/>.
// </airvpn_source_header>

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using AirVPN.Core;
using AirVPN.Core.UI;
using AirVPN.Gui.Forms; // Temp

namespace AirVPN.Gui.Controls
{	
    public class ChartSpeed : System.Windows.Forms.Control
    {
		private int m_chartIndex = 0;
		private Chart m_chart;

		public float unitMultiplier = 1;

		private Pen PenGrid;
		private Pen PenMouse;
		private Brush BrushMouse;
		private Pen PenDownloadGraph;
		private Pen PenDownloadLine;
		private Pen PenUploadGraph;
		private Pen PenUploadLine;
		private Brush BrushLegendText;
		private Brush BrushDownloadText;
		private Brush BrushUploadText;
		
		private Font FontLabel;
        private StringFormat formatTopCenter;		
		private StringFormat formatRight;
		private StringFormat formatBottomRight;
		private StringFormat formatBottomLeft;
		private StringFormat formatTopLeft;
		private StringFormat formatTopRight;

		int m_legendDY = 0; // 15
		int m_marginTopY = 15;

		private float chartDX;
		private float chartDY;
		private float chartStartX;
		private float chartStartY;
		        
        public ChartSpeed()
        {
			PenGrid = new Pen(Colors.LightChartGrid, 1);
			PenMouse = new Pen(Colors.LightChartMouse, 1);
			BrushMouse = new SolidBrush(Colors.LightChartMouse);
			PenDownloadGraph = new Pen(Colors.LightChartLineDownload, 1);
			PenDownloadLine = new Pen(Colors.LightChartLineDownload, 1);
			PenUploadGraph = new Pen(Colors.LightChartLineUpload, 1);
			PenUploadLine = new Pen(Colors.LightChartLineUpload, 1);
			BrushLegendText = new SolidBrush(Colors.LightChartLegend);
			BrushDownloadText = new SolidBrush(Colors.LightChartLineDownload);
			BrushUploadText = new SolidBrush(Colors.LightChartLineUpload);
						
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
			Reset();
        }
		
		void Charts_UpdateEvent()
		{
			Invalidate();
		}

		public void Reset()
		{
			
		}

		public string ValToDesc(Int64 v)
		{
			return Utils.FormatBytesEx2(v*8, true) + "/s (" + Utils.FormatBytesEx2(v, false) + "/s)";			
		}
		
		public void Switch(int chartIndex)
		{
			m_chartIndex = chartIndex;
			if( (m_chartIndex<0) || (m_chartIndex >= Engine.Instance.Stats.Charts.ChartsList.Count) )
				m_chartIndex = 0;

			m_chart = Engine.Instance.Stats.Charts.ChartsList[m_chartIndex];

			Invalidate();
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

			Form.Skin.GraphicsCommon(e.Graphics);

            try
			{
				int DX = this.ClientRectangle.Width;
				int DY = this.ClientRectangle.Height;

				//e.Graphics.FillRectangle(BrushBackground, this.ClientRectangle);				
				Form.DrawImage(e.Graphics, GuiUtils.GetResourceImage("tab_l_bg"), new Rectangle(0, 0, ClientSize.Width, ClientSize.Height));

				chartDX = this.ClientRectangle.Width;
				chartDY = this.ClientRectangle.Height - m_legendDY;
				chartStartX = 0;
				chartStartY = chartDY;


				float maxY = m_chart.GetMax();
				if (maxY <= 0)
					maxY = 4096;
				else if (maxY > 1000000000000)
					maxY = 1000000000000;					
				
				Point lastPointDown = new Point(-1, -1);
				Point lastPointUp = new Point(-1, -1);

				float stepX = (chartDX - 0) / m_chart.Resolution;

				// Grid lines				
				for (int g = 0; g < m_chart.Grid; g++)
				{
					float x = ((chartDX - 0) / m_chart.Grid) * g;
					e.Graphics.DrawLine(PenGrid, chartStartX + x, 0, chartStartX + x, chartStartY);
				}

				// Axis line
				e.Graphics.DrawLine(Pens.Gray, 0, chartStartY, chartDX, chartStartY);

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

					float downY = ((m_chart.Download[p]) * (chartDY - m_marginTopY)) / maxY;
					float upY = ((m_chart.Upload[p]) * (chartDY - m_marginTopY)) / maxY;

					Point pointDown = ChartPoint(chartStartX + stepX * i, chartStartY - downY);
					Point pointUp = ChartPoint(chartStartX + stepX * i, chartStartY - upY);

					//e.Graphics.DrawLine(Pens.Green, new Point(0,0), point);

					if (lastPointDown.X != -1)
					{
						e.Graphics.DrawLine(PenDownloadGraph, lastPointDown, pointDown);
						e.Graphics.DrawLine(PenUploadGraph, lastPointUp, pointUp);
					}

					lastPointDown = pointDown;
					lastPointUp = pointUp;
				}

				// Download line
				float downCurY = 0;
				{
					long v = m_chart.GetLastDownload();
					downCurY = ((v) * (chartDY - m_marginTopY)) / maxY;
					e.Graphics.DrawLine(PenDownloadLine, 0, chartStartY - downCurY, chartDX, chartStartY - downCurY);
					Form.DrawStringOutline(e.Graphics, Messages.ChartDownload + ": " + ValToDesc(v), FontLabel, BrushDownloadText, ChartRectangle(0, 0, chartDX, chartStartY - downCurY), formatBottomRight);
				}

				// Upload line
				{
					long v = m_chart.GetLastUpload();
					float y = ((v) * (chartDY - m_marginTopY)) / maxY;
					float dly = 0;
					if (Math.Abs(downCurY - y) < 10) dly = 15; // Download and upload overwrap, distance it.
					e.Graphics.DrawLine(PenUploadLine, 0, chartStartY - y, chartDX, chartStartY - y);
					Form.DrawStringOutline(e.Graphics, Messages.ChartUpload + ": " + ValToDesc(v), FontLabel, BrushUploadText, ChartRectangle(0, 0, chartDX, chartStartY - y - dly), formatBottomRight);
				}

				// Mouse lines
				{
					Point mp = Cursor.Position;
					mp = PointToClient(mp);

					if ((mp.X > 0) && (mp.Y < chartDX) && (mp.Y > 0) && (mp.Y < chartDY))
					{
						e.Graphics.DrawLine(PenMouse, 0, mp.Y, chartDX, mp.Y);
						e.Graphics.DrawLine(PenMouse, mp.X, 0, mp.X, chartDY);

						float i = (chartDX - (mp.X - chartStartX)) / stepX;

						int t = Conversions.ToInt32(i * m_chart.TimeStep);

						//float y = mp.Y * maxY / (chartDY - m_marginTopY);
						float y = (chartStartY - (mp.Y - m_marginTopY)) * maxY / chartDY;

						String label = ValToDesc(Conversions.ToInt64(y)) + ", " + Utils.FormatSeconds(t) + " ago";

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

						Form.DrawStringOutline(e.Graphics, label, FontLabel, BrushMouse, rect, formatAlign);
					}
				}

			}
            catch (Exception ex)
            {
                Debug.Trace(ex);
            }
        }

		private Rectangle ChartRectangle(float x, float y, float w, float h)
		{
			return new Rectangle(Conversions.ToInt32(x), Conversions.ToInt32(y), Conversions.ToInt32(w), Conversions.ToInt32(h));
		}

		private Point ChartPoint(float x, float y)
		{
			return new Point(Conversions.ToInt32(x), Conversions.ToInt32(y));
		}

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ResumeLayout(false);

        }
        
    }
}
