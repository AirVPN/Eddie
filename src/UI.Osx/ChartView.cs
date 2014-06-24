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
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using AirVPN.Core;
using AirVPN.Core.UI;

namespace AirVPN.UI.Osx
{
	[Register("ChartView")]
	public class ChartView : NSView
	{
		private NSColor m_colorGrid;
		private NSColor m_colorAxis;
		private NSColor m_colorMouse;
		private NSColor m_colorDownloadGraph;
		private NSColor m_colorDownloadLine;
		private NSColor m_colorUploadGraph;
		private NSColor m_colorUploadLine;
		//private NSColor m_colorLegendText;
		private NSColor m_colorDownloadText;
		private NSColor m_colorUploadText;

		private int m_chartIndex = 0;
		private Chart m_chart;

		private int m_legendDY = 0; // 15
		private int m_marginTopY = 15;

		private float m_chartDX;
		private float m_chartDY;
		private float m_chartStartX;
		private float m_chartStartY;

		public ChartView ()
		{
			this.AcceptsTouchEvents = true;
		}

		public ChartView (IntPtr handle) : base (handle)
		{
			Initialize ();
		}

		private void Initialize ()
		{
			NeedsDisplay = true;
		}

		public override bool AcceptsFirstResponder ()
		{
			//return base.AcceptsFirstResponder ();
			return true;
		}
			
		public override void MouseMoved (NSEvent theEvent)
		{
			base.MouseMoved (theEvent);

			NeedsDisplay = true;
		}

		public override void AwakeFromNib ()
		{
			base.AwakeFromNib ();

			m_colorGrid = GuiUtils.ConvertColor(Colors.LightChartGrid);
			m_colorAxis = GuiUtils.ConvertColor (Colors.LightChartAxis);
			m_colorMouse = GuiUtils.ConvertColor(Colors.LightChartMouse);
			m_colorDownloadGraph = GuiUtils.ConvertColor(Colors.LightChartLineDownload);
			m_colorDownloadLine = GuiUtils.ConvertColor(Colors.LightChartLineDownload);
			m_colorUploadGraph = GuiUtils.ConvertColor(Colors.LightChartLineUpload);
			m_colorUploadLine = GuiUtils.ConvertColor(Colors.LightChartLineUpload);
			//m_colorLegendText = GuiUtils.ConvertColor(Colors.LightChartLegend);
			m_colorDownloadText = GuiUtils.ConvertColor(Colors.LightChartLineDownload);
			m_colorUploadText = GuiUtils.ConvertColor(Colors.LightChartLineUpload);


			m_chart = Engine.Instance.Stats.Charts.ChartsList[m_chartIndex];

			Engine.Instance.Stats.Charts.UpdateEvent += new Core.UI.Charts.UpdateHandler(Charts_UpdateEvent);


		}


		void Charts_UpdateEvent()
		{
			new NSObject ().InvokeOnMainThread (() => {
				NeedsDisplay = true;
			});
		}

		public string ValToDesc(Int64 v)
		{
			return Utils.FormatBytesEx2(v * 8, true) + "/s (" + Utils.FormatBytesEx2(v, false) + "/s)";
		}

		public void Switch(int chartIndex)
		{
			m_chartIndex = chartIndex;
			if ((m_chartIndex < 0) || (m_chartIndex >= Engine.Instance.Stats.Charts.ChartsList.Count))
				m_chartIndex = 0;

			m_chart = Engine.Instance.Stats.Charts.ChartsList[m_chartIndex];

			NeedsDisplay = true;

		}

		private Rectangle ChartRectangle(float x, float y, float w, float h)
		{
			return new Rectangle(Conversions.ToInt32(x), Conversions.ToInt32(y), Conversions.ToInt32(w), Conversions.ToInt32(h));
		}

		private Point ChartPoint(float x, float y)
		{
			return new Point(Conversions.ToInt32(x), Conversions.ToInt32(y));
		}

		private PointF Invert(PointF p)
		{
			p.Y = Bounds.Height - p.Y;
			return p;
		}

		private RectangleF Invert(RectangleF r)
		{
			r.Y = Bounds.Height - r.Y;
			return r;
		}

		private void DrawLine(NSColor color, float x1, float y1, float x2, float y2)
		{
			DrawLine(color, new PointF(x1,y1), new PointF(x2,y2));
		}

		private void DrawLine(NSColor color, PointF p1, PointF p2)
		{
			color.Set();
			NSBezierPath.StrokeLine (Invert(p1), Invert(p2));
		}

		private void DrawStringOutline(string text, NSColor color, RectangleF rect, int align)
		{

			NSDictionary attributes = new NSDictionary ();
			NSString nsString = new NSString (text);

			int halign = align % 3;
			int valign = align / 3;


			//nsString = new NSString ("h:" + Conversions.ToString (halign) + ",v:" + Conversions.ToString (valign));


			SizeF size = nsString.StringSize (attributes);

			//NSGraphicsContext.CurrentContext.GraphicsPort.SelectFont ("Arial", 50, CGTextEncoding.MacRoman);

			if (halign == 0) {
			} else if (halign == 1) {
				rect.X = (rect.Left + rect.Right) / 2 - size.Width / 2;
			} else if (halign == 2) {
				rect.X = rect.Right - size.Width;
			}
			rect.Width = size.Width;

			if (valign == 0) {
			} else if (valign == 1) {
				rect.Y = (rect.Top + rect.Bottom) / 2 - size.Height / 2;
			} else if (valign == 2) {
				rect.Y = rect.Bottom - size.Height;
			}
			rect.Height = size.Height;
			
			//this.DrawRect (rect);

			nsString.DrawString(Invert(rect), attributes);
		}
			
		public override void DrawRect (System.Drawing.RectangleF dirtyRect)
		{
			var context = NSGraphicsContext.CurrentContext.GraphicsPort;

			/*
			//context.SetFillColor(new CGColor(1,1,1)); //White
			//context.FillRect (dirtyRect);

			int GridSize = 10;

			for (int i = 1; i < this.Bounds.Size.Height / 10; i++) {
				if (i % 10 == 0) {
					NSColor colorWithSRGBRed = NSColor.FromSrgb (100 / 255.0f, 149 / 255.0f, 237 / 255.0f, 0.3f);
					colorWithSRGBRed.Set ();
				} else if (i % 10 == 0) {
					NSColor colorWithSRGBRed = NSColor.FromSrgb (100 / 255.0f, 149 / 255.0f, 237 / 255.0f, 0.2f);
					colorWithSRGBRed.Set ();
				} else {
					NSColor colorWithSRGBRed = NSColor.FromSrgb (100 / 255.0f, 149 / 255.0f, 237 / 255.0f, 0.1f);
					colorWithSRGBRed.Set ();
				}
				var pointFrom = new PointF (0, (i * GridSize - 0.5f));
				var pointTo = new PointF (this.Bounds.Size.Width, (i * GridSize - 0.5f));

				NSBezierPath.StrokeLine (pointFrom, pointTo);
			}

			for (int i = 1; i < this.Bounds.Size.Width / 10; i++) {
				if (i % 10 == 0) {
					NSColor colorWithSRGBRed = NSColor.FromSrgb (100 / 255.0f, 149 / 255.0f, 237 / 255.0f, 0.3f);
					colorWithSRGBRed.Set ();
				} else if (i % 10 == 0) {
					NSColor colorWithSRGBRed = NSColor.FromSrgb (100 / 255.0f, 149 / 255.0f, 237 / 255.0f, 0.2f);
					colorWithSRGBRed.Set ();
				} else {
					NSColor colorWithSRGBRed = NSColor.FromSrgb (100 / 255.0f, 149 / 255.0f, 237 / 255.0f, 0.1f);
					colorWithSRGBRed.Set ();
				}
				var pointFrom = new PointF ((i * GridSize - 0.5f),0);
				var pointTo = new PointF ((i * GridSize - 0.5f),this.Bounds.Size.Height);

				NSBezierPath.StrokeLine (pointFrom, pointTo);
			}
			*/




			float DX = this.Bounds.Size.Width;
			float DY = this.Bounds.Size.Height;

			//e.Graphics.FillRectangle(BrushBackground, this.ClientRectangle);				
			//Form.DrawImage(e.Graphics, GuiUtils.GetResourceImage("tab_l_bg"), new Rectangle(0, 0, ClientSize.Width, ClientSize.Height));

			m_chartDX = DX;
			m_chartDY = DY - m_legendDY;
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
				DrawLine(m_colorGrid, m_chartStartX + x, 0, m_chartStartX + x, m_chartStartY);
			}

			// Axis line
			DrawLine(m_colorAxis, 0, m_chartStartY, m_chartDX, m_chartStartY);

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
					DrawLine(m_colorDownloadGraph, lastPointDown, pointDown);
					DrawLine(m_colorUploadGraph, lastPointUp, pointUp);
				}

				lastPointDown = pointDown;
				lastPointUp = pointUp;
			}

			// Download line
			float downCurY = 0;
			{
				long v = m_chart.GetLastDownload();
				downCurY = ((v) * (m_chartDY - m_marginTopY)) / maxY;
				DrawLine(m_colorDownloadLine, 0, m_chartStartY - downCurY, m_chartDX, m_chartStartY - downCurY);
				DrawStringOutline(Messages.ChartDownload + ": " + ValToDesc(v), m_colorDownloadText, ChartRectangle(0, 0, m_chartDX, m_chartStartY - downCurY), 8);
			}

			// Upload line
			{
				long v = m_chart.GetLastUpload();
				float y = ((v) * (m_chartDY - m_marginTopY)) / maxY;
				float dly = 0;
				if (Math.Abs(downCurY - y) < 10) dly = 15; // Download and upload overwrap, distance it.
				DrawLine(m_colorUploadLine, 0, m_chartStartY - y, m_chartDX, m_chartStartY - y);
				DrawStringOutline(Messages.ChartUpload + ": " + ValToDesc(v), m_colorUploadText, ChartRectangle(0, 0, m_chartDX, m_chartStartY - y - dly), 8);

			}

			// Mouse lines
			{
				PointF mp = Window.MouseLocationOutsideOfEventStream;
				mp.X -= this.Frame.Left;
				mp.Y -= this.Frame.Top;
				//mp = ParentWindow.ConvertPointToView (mp, this);

				mp = Invert (mp);

				//mp = Window.ConvertScreenToBase (mp);

				if ((mp.X > 0) && (mp.Y < m_chartDX) && (mp.Y > 0) && (mp.Y < m_chartDY))
				{
					DrawLine(m_colorMouse, 0, mp.Y, m_chartDX, mp.Y);
					DrawLine(m_colorMouse, mp.X, 0, mp.X, m_chartDY);

					float i = (m_chartDX - (mp.X - m_chartStartX)) / stepX;

					int t = Conversions.ToInt32(i * m_chart.TimeStep);

					//float y = mp.Y * maxY / (chartDY - m_marginTopY);
					float y = (m_chartStartY - (mp.Y - m_marginTopY)) * maxY / m_chartDY;

					String label = ValToDesc(Conversions.ToInt64(y)) + ", " + Utils.FormatSeconds(t) + " ago";

					int formatAlign = 6;
					RectangleF rect = new RectangleF();
					//if(DX - mp.X > DX / 2)
					if (mp.X < DX - 150)
					{
						//if (DY - mp.Y > DY / 2)
						if (mp.Y < 20)
						{
							formatAlign = 0;
							rect.X = mp.X + 20;
							rect.Y = mp.Y + 5;
							rect.Width = DX;
							rect.Height = DX;
						}
						else
						{
							formatAlign = 6;
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
							formatAlign = 2;
							rect.X = 0;
							rect.Y = mp.Y;
							rect.Width = mp.X - 20;
							rect.Height = DY;
						}
						else
						{
							formatAlign = 8;
							rect.X = 0;
							rect.Y = 0;
							rect.Width = mp.X - 20;
							rect.Height = mp.Y - 5;

						}
					}

					DrawStringOutline(label, m_colorMouse, rect, formatAlign);
				}
			}

		}
	}
}

