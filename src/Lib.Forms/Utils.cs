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
using System.Windows.Forms;
using System.Text;
using AirVPN.Core;

namespace AirVPN.Gui
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
			StringFormatLeftTop = new StringFormat();
			StringFormatLeftTop.Alignment = StringAlignment.Near;
			StringFormatLeftTop.LineAlignment = StringAlignment.Near;

			StringFormatCenterTop = new StringFormat();
			StringFormatCenterTop.Alignment = StringAlignment.Center;
			StringFormatCenterTop.LineAlignment = StringAlignment.Near;

			StringFormatRightTop = new StringFormat();
			StringFormatRightTop.Alignment = StringAlignment.Far;
			StringFormatRightTop.LineAlignment = StringAlignment.Near;

			StringFormatLeftMiddle = new StringFormat();
			StringFormatLeftMiddle.Alignment = StringAlignment.Near;
			StringFormatLeftMiddle.LineAlignment = StringAlignment.Center;
			StringFormatLeftMiddle.FormatFlags = StringFormatFlags.NoWrap;

			StringFormatCenterMiddle = new StringFormat();
			StringFormatCenterMiddle.Alignment = StringAlignment.Center;
			StringFormatCenterMiddle.LineAlignment = StringAlignment.Center;
			StringFormatCenterMiddle.FormatFlags = StringFormatFlags.NoWrap;

			StringFormatRightMiddle = new StringFormat();
			StringFormatRightMiddle.Alignment = StringAlignment.Far;
			StringFormatRightMiddle.LineAlignment = StringAlignment.Center;
			StringFormatRightMiddle.FormatFlags = StringFormatFlags.NoWrap;

			StringFormatLeftBottom = new StringFormat();
			StringFormatLeftBottom.Alignment = StringAlignment.Near;
			StringFormatLeftBottom.LineAlignment = StringAlignment.Far;

			StringFormatCenterBottom = new StringFormat();
			StringFormatCenterBottom.Alignment = StringAlignment.Center;
			StringFormatCenterBottom.LineAlignment = StringAlignment.Far;

			StringFormatRightBottom = new StringFormat();
			StringFormatRightBottom.Alignment = StringAlignment.Far;
			StringFormatRightBottom.LineAlignment = StringAlignment.Far;
		}
		
        public static Image GetResourceImage(string name)
        {
            // Accessing Properties.Resources.xxx is a lot cpu extensive, probabily conversions every time. We cache image resources.
            if (ImageResourceCache.ContainsKey(name))
                return ImageResourceCache[name];
            else
            {						
                Bitmap i = (Bitmap) global::AirVPN.Lib.Forms.Properties.Resources.ResourceManager.GetObject(name);
                ImageResourceCache[name] = i;
                return i;
            }
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
