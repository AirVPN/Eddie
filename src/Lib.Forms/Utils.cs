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
using System.Windows.Forms;
using System.Text;
using Eddie.Core;

namespace Eddie.Gui
{
    public class GuiUtils
    {
        private static Dictionary<String, Bitmap> ImageResourceCache = new Dictionary<String, Bitmap>();

		public static StringFormat StringFormatLeftTop;
		public static StringFormat StringFormatCenterTop;
		public static StringFormat StringFormatRightTop;
		public static StringFormat StringFormatLeftMiddle;
		public static StringFormat StringFormatCenterMiddle;
		public static StringFormat StringFormatRightMiddle;
		public static StringFormat StringFormatLeftBottom;
		public static StringFormat StringFormatCenterBottom;
		public static StringFormat StringFormatRightBottom;
        
        public static void Init()
		{
            StringFormatLeftTop = BuildStringFormat(StringAlignment.Near, StringAlignment.Near);
            StringFormatCenterTop = BuildStringFormat(StringAlignment.Center, StringAlignment.Near);
            StringFormatRightTop = BuildStringFormat(StringAlignment.Far, StringAlignment.Near);
            StringFormatLeftMiddle = BuildStringFormat(StringAlignment.Near, StringAlignment.Center);
            StringFormatCenterMiddle = BuildStringFormat(StringAlignment.Center, StringAlignment.Center);
            StringFormatRightMiddle = BuildStringFormat(StringAlignment.Far, StringAlignment.Center);
            StringFormatLeftBottom = BuildStringFormat(StringAlignment.Near, StringAlignment.Far);
            StringFormatCenterBottom = BuildStringFormat(StringAlignment.Center, StringAlignment.Far);
            StringFormatRightBottom = BuildStringFormat(StringAlignment.Far, StringAlignment.Far);
		}

        public static StringFormat BuildStringFormat(StringAlignment h, StringAlignment v)
        {
            StringFormat sf = new StringFormat();
            sf.Alignment = h;
            sf.LineAlignment = v;
            sf.FormatFlags = StringFormatFlags.NoWrap;
            sf.Trimming = StringTrimming.None;
            return sf;
        }
        
        public static Size GetFontSize(Graphics g, Font f, string text)
        {
            return g.MeasureString(text, f).ToSize();
        }

        public static int GetFontHeight(Graphics g, Font f)
        {
            return GetFontSize(g, f, "SampleStringForFontMeasure123").Height;
        }
		
        public static Image GetResourceImage(string name)
        {
            // Accessing Properties.Resources.xxx is a lot cpu extensive, probabily conversions every time. We cache image resources.
            if (ImageResourceCache.ContainsKey(name))
                return ImageResourceCache[name];
            else
            {						
                Bitmap i = (Bitmap) global::Eddie.Lib.Forms.Properties.Resources.ResourceManager.GetObject(name);
                ImageResourceCache[name] = i;
                return i;
            }
        }

        public static void FixHeightVs(Control controlRef, Control controlFix)
        {
            // ComboBox and TextField have dynamic height. This align vertically the related label.
            if(controlFix.Top != controlRef.Top)
                controlFix.Top = controlRef.Top;
            if(controlFix.Height != controlRef.Height)
                controlFix.Height = controlRef.Height;
        }

        public static string FilePicker()
		{
			return FilePicker(Messages.FilterAllFiles);			
		}

		public static string FilePicker(string filter)
		{
			OpenFileDialog sd = new OpenFileDialog();
			sd.Filter = filter;
			if (sd.ShowDialog() == DialogResult.OK)
			{
				return sd.FileName;
			}
			else
			{
				return "";
			}
		}

    }
}
