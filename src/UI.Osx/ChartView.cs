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

namespace AirVPN.UI.Osx
{
	[Register("ChartView")]
	public class ChartView : NSView
	{
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

		public override void DrawRect (System.Drawing.RectangleF dirtyRect)
		{
			var context = NSGraphicsContext.CurrentContext.GraphicsPort;
			context.SetFillColor(new CGColor(1,1,1)); //White
			context.FillRect (dirtyRect);

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
		}
	}
}

