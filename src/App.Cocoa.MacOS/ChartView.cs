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
//using System.Drawing;
//using Foundation;
//using AppKit;
using Foundation;
using AppKit;
using CoreGraphics;
using Eddie.Core;
using Eddie.Core.UI;
using Eddie.Common;

namespace Eddie.UI.Cocoa.Osx
{
	[Register("ChartView")]
	public class ChartView : NSView
	{
		private CGColor m_colorBackground;
		private CGColor m_colorGrid;
		private CGColor m_colorAxis;
		private CGColor m_colorMouse;
		private CGColor m_colorDownloadGraph;
		private CGColor m_colorDownloadLine;
		private CGColor m_colorUploadGraph;
		private CGColor m_colorUploadLine;
		//private CGColor m_colorLegendText;
		private CGColor m_colorDownloadText;
		private CGColor m_colorUploadText;
		private CGColor m_colorWhite;
		private CoreText.CTFont m_font;

		private int m_chartIndex = 0;
		private Chart m_chart;

		private int m_legendDY = 0; // 15
		private int m_marginTopY = 15;

		private nfloat m_chartDX;
		private nfloat m_chartDY;
		private nfloat m_chartStartX;
		private nfloat m_chartStartY;

		public ChartView()
		{
			this.AcceptsTouchEvents = true;
		}

		public ChartView(IntPtr handle) : base(handle)
		{
			Initialize();
		}

		private void Initialize()
		{
			NeedsDisplay = true;
		}

		public override bool AcceptsFirstResponder()
		{
			//return base.AcceptsFirstResponder ();
			return true;
		}

		public override void MouseMoved(NSEvent theEvent)
		{
			base.MouseMoved(theEvent);

			NeedsDisplay = true;
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();

			CGColor LightChartBackground = ColorFromArgb(248, 248, 248);
			CGColor LightChartGrid = ColorFromArgb(211, 211, 211);
			CGColor LightChartAxis = ColorFromArgb(128, 128, 128);
			CGColor LightChartMouse = ColorFromArgb(32, 92, 166);
			CGColor LightChartLegend = ColorFromArgb(64, 145, 255);
			CGColor LightChartLineDownload = ColorFromArgb(38, 22, 255);
			CGColor LightChartLineUpload = ColorFromArgb(0, 90, 0);
			CGColor LightChartWhite = ColorFromArgb(255, 255, 255);

			m_colorBackground = LightChartBackground;
			m_colorGrid = LightChartGrid;
			m_colorAxis = LightChartAxis;
			m_colorMouse = LightChartMouse;
			m_colorDownloadGraph = LightChartLineDownload;
			m_colorDownloadLine = LightChartLineDownload;
			m_colorUploadGraph = LightChartLineUpload;
			m_colorUploadLine = LightChartLineUpload;
			//m_colorLegendText = LightChartLegend;
			m_colorDownloadText = LightChartLineDownload;
			m_colorUploadText = LightChartLineUpload;
			m_colorWhite = LightChartWhite;

			m_font = new CoreText.CTFont("Menlo", 10);

			m_chart = Engine.Instance.Stats.Charts.ChartsList[m_chartIndex];

			Engine.Instance.Stats.Charts.UpdateEvent += new Core.UI.Charts.UpdateHandler(Charts_UpdateEvent);

		}

		CGColor ColorFromArgb(int r, int g, int b)
		{
			nfloat fR = r;
			nfloat fG = g;
			nfloat fB = b;
			fR = fR / 256;
			fG = fG / 256;
			fB = fB / 256;
			return new CGColor(fR, fG, fB);
		}

		void Charts_UpdateEvent(object sender, EventArgs e)
		{
			new NSObject().InvokeOnMainThread(() =>
			{
				NeedsDisplay = true;
			});
		}

		public string ValToDesc(Int64 v)
		{
			return UtilsString.FormatBytes(v, true, true);
		}

		public void Switch(int chartIndex)
		{
			m_chartIndex = chartIndex;
			if ((m_chartIndex < 0) || (m_chartIndex >= Engine.Instance.Stats.Charts.ChartsList.Count))
				m_chartIndex = 0;

			m_chart = Engine.Instance.Stats.Charts.ChartsList[m_chartIndex];

			NeedsDisplay = true;

		}

		private CGRect ChartRectangle(nfloat x, nfloat y, nfloat w, nfloat h)
		{
			return new CGRect(Conversions.ToInt32(x), Conversions.ToInt32(y), Conversions.ToInt32(w), Conversions.ToInt32(h));
		}

		private CGPoint ChartPoint(nfloat x, nfloat y)
		{
			return new CGPoint(Conversions.ToInt32(x), Conversions.ToInt32(y));
		}

		private CGPoint Invert(CGPoint p)
		{
			p.Y = (float)Bounds.Height - p.Y;
			return p;
		}

		private CGRect Invert(CGRect r)
		{
			r.Y = (float)Bounds.Height - r.Y - r.Height;
			return r;
		}

		private void DrawLine(CGContext context, CGColor color, nfloat x1, nfloat y1, nfloat x2, nfloat y2)
		{
			DrawLine(context, color, new CGPoint(x1, y1), new CGPoint(x2, y2));
		}

		private void DrawLine(CGContext context, CGColor color, CGPoint p1, CGPoint p2)
		{
			context.SetStrokeColor(color);
			//color.Set();
			NSBezierPath.StrokeLine(Invert(p1), Invert(p2));
		}


		private void DrawStringOutline(CGContext context, string text, CGColor color, CGRect rect, int align)
		{
			NSString nsString = new NSString(text);

			int halign = align % 3;
			int valign = align / 3;
			/*
			var attributesText = new NSAttributedString("Outline",
									new CoreText.CTStringAttributes()
									{
										ForegroundColor = color,
										Font = m_font
									});
			var attributesOutline = new NSAttributedString("Outline",
										new CoreText.CTStringAttributes()
										{
											ForegroundColor = m_colorWhite,
											Font = m_font
										});
										*/
			var objectsText = new object[] { m_font, NSColor.FromCGColor(color) };
			var keysText = new object[] { "NSFont", "NSColor" };
			var attributesText = NSDictionary.FromObjectsAndKeys(objectsText, keysText);
			var objectsOutline = new object[] { m_font, NSColor.White };
			var keysOutline = new object[] { "NSFont", "NSColor" };
			var attributesOutline = NSDictionary.FromObjectsAndKeys(objectsOutline, keysOutline);

			CGSize size = nsString.StringSize(attributesText);
			if (halign == 0)
			{
			}
			else if (halign == 1)
			{
				rect.X = (rect.Left + rect.Right) / 2 - size.Width / 2;
			}
			else if (halign == 2)
			{
				rect.X = rect.Right - size.Width;
			}
			rect.Width = size.Width;

			if (valign == 0)
			{
			}
			else if (valign == 1)
			{
				rect.Y = (rect.Top + rect.Bottom) / 2 - size.Height / 2;
			}
			else if (valign == 2)
			{
				rect.Y = rect.Bottom - size.Height;
			}
			rect.Height = size.Height;

			NSColor.Black.Set();
			for (int ox = -1; ox <= 1; ox++)
			{
				for (int oy = -1; oy <= 1; oy++)
				{
					CGRect rectString = rect;
					rectString.Offset(new CGPoint(ox, oy));
					nsString.DrawString(Invert(rectString), attributesOutline);
				}
			}

			nsString.DrawString(Invert(rect), attributesText);
			//nsString.DrawString(Invert(rect), null);
			/*
			using (var textLine = new CoreText.CTLine(attributesText))
			{
				textLine.Draw(context);
			}
			*/

		}

		public override void DrawRect(CGRect dirtyRect)
		{
			var context = NSGraphicsContext.CurrentContext.GraphicsPort;

			// Engine.Instance.Stats.Charts.Hit (RandomGenerator.GetInt (1024, 1024 * 1024), RandomGenerator.GetInt (1024, 1024 * 1024)); // Debugging

			context.SetFillColor(m_colorBackground);
			context.FillRect(dirtyRect);

			NSColor.Gray.Set();
			NSBezierPath.StrokeRect(Bounds);


			nfloat DX = this.Bounds.Size.Width;
			nfloat DY = this.Bounds.Size.Height;

			m_chartDX = DX;
			m_chartDY = DY - m_legendDY;
			m_chartStartX = 0;
			m_chartStartY = m_chartDY;


			float maxY = m_chart.GetMax();
			if (maxY <= 0)
				maxY = 4096;
			else if (maxY > 1000000000000)
				maxY = 1000000000000;

			CGPoint lastPointDown = new CGPoint(-1, -1);
			CGPoint lastPointUp = new CGPoint(-1, -1);

			nfloat stepX = (m_chartDX - 0) / m_chart.Resolution;

			// Grid lines				
			for (int g = 0; g < m_chart.Grid; g++)
			{
				nfloat x = ((m_chartDX - 0) / m_chart.Grid) * g;
				DrawLine(context, m_colorGrid, m_chartStartX + x, 0, m_chartStartX + x, m_chartStartY);
			}

			// Axis line
			DrawLine(context, m_colorAxis, 0, m_chartStartY, m_chartDX, m_chartStartY);

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

				nfloat downY = ((m_chart.Download[p]) * (m_chartDY - m_marginTopY)) / maxY;
				nfloat upY = ((m_chart.Upload[p]) * (m_chartDY - m_marginTopY)) / maxY;

				CGPoint pointDown = ChartPoint(m_chartStartX + stepX * i, m_chartStartY - downY);
				CGPoint pointUp = ChartPoint(m_chartStartX + stepX * i, m_chartStartY - upY);

				//e.Graphics.DrawLine(Pens.Green, new Point(0,0), point);

				if (lastPointDown.X != -1)
				{
					DrawLine(context, m_colorDownloadGraph, lastPointDown, pointDown);
					DrawLine(context, m_colorUploadGraph, lastPointUp, pointUp);

				}

				lastPointDown = pointDown;
				lastPointUp = pointUp;
			}

			// Download line
			nfloat downCurY = 0;
			{
				long v = m_chart.GetLastDownload();
				downCurY = ((v) * (m_chartDY - m_marginTopY)) / maxY;
				DrawLine(context, m_colorDownloadLine, 0, m_chartStartY - downCurY, m_chartDX, m_chartStartY - downCurY);
				DrawStringOutline(context, Messages.ChartDownload + ": " + ValToDesc(v), m_colorDownloadText, ChartRectangle(0, 0, m_chartDX - 10, m_chartStartY - downCurY), 8);
			}

			// Upload line
			{
				long v = m_chart.GetLastUpload();
				nfloat y = ((v) * (m_chartDY - m_marginTopY)) / maxY;
				nfloat dly = 0;
				if (Math.Abs(downCurY - y) < 10) dly = 15; // Download and upload overwrap, distance it.
				DrawLine(context, m_colorUploadLine, 0, m_chartStartY - y, m_chartDX, m_chartStartY - y);
				DrawStringOutline(context, Messages.ChartUpload + ": " + ValToDesc(v), m_colorUploadText, ChartRectangle(0, 0, m_chartDX - 10, m_chartStartY - y - dly), 8);

			}

			// Mouse lines
			{
				CGPoint mp = Window.MouseLocationOutsideOfEventStream;
				mp.X -= this.Frame.Left;
				mp.Y -= this.Frame.Top;
				//mp = ParentWindow.ConvertPointToView (mp, this);

				mp = Invert(mp);

				//mp = Window.ConvertScreenToBase (mp);

				if ((mp.X > 0) && (mp.Y < m_chartDX) && (mp.Y > 0) && (mp.Y < m_chartDY))
				{
					DrawLine(context, m_colorMouse, 0, mp.Y, m_chartDX, mp.Y);
					DrawLine(context, m_colorMouse, mp.X, 0, mp.X, m_chartDY);

					nfloat i = (m_chartDX - (mp.X - m_chartStartX)) / stepX;

					int t = Conversions.ToInt32(i * m_chart.TimeStep);

					//float y = mp.Y * maxY / (chartDY - m_marginTopY);
					nfloat y = (m_chartStartY - (mp.Y - m_marginTopY)) * maxY / m_chartDY;

					String label = ValToDesc(Conversions.ToInt64(y)) + ", " + UtilsString.FormatSeconds(t) + " ago";

					int formatAlign = 6;

					CGRect rect = new CGRect();
					if (DX - mp.X > DX / 2)
					//if (mp.X < DX - 200)
					{
						if (DY - mp.Y > DY / 2)
						//if (mp.Y < 20)
						{
							formatAlign = 0;
							rect.X = mp.X + 5;
							rect.Y = mp.Y + 5;
							rect.Width = DX;
							rect.Height = DX;
						}
						else
						{
							formatAlign = 6;
							rect.X = mp.X + 5;
							rect.Y = 0;
							rect.Width = DX;
							rect.Height = mp.Y - 5;
						}
					}
					else
					{
						if (DY - mp.Y > DY / 2)
						//if (mp.Y < 40)
						{
							formatAlign = 2;
							rect.X = 0;
							rect.Y = mp.Y;
							rect.Width = mp.X - 5;
							rect.Height = DY;
						}
						else
						{
							formatAlign = 8;
							rect.X = 0;
							rect.Y = 0;
							rect.Width = mp.X - 5;
							rect.Height = mp.Y - 5;

						}
					}

					DrawStringOutline(context, label, m_colorMouse, rect, formatAlign);
				}
			}

		}
	}
}

