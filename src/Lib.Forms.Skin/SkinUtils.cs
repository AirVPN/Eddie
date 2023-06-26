// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2023 AirVPN (support@airvpn.org) / https://airvpn.org
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

#pragma warning disable CA1416 // Windows only

namespace Eddie.Forms.Skin
{
	public static class SkinUtils
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
		public static StringFormat StringFormatLeftMiddleNoWrap;
		public static StringFormat StringFormatCenterMiddleNoWrap;
		public static StringFormat StringFormatRightMiddleNoWrap;

		private static string m_unixFontSystemName = "";
		private static float m_unixFontSystemSize = 11;
		private static string m_unixFontMonoSpaceName = "";
		private static float m_unixFontMonoSpaceSize = 11;

		public static bool IsWindows()
		{
			return (Environment.OSVersion.VersionString.IndexOf("Windows", StringComparison.InvariantCulture) != -1);
		}

		public static bool IsUnix()
		{
			return (Environment.OSVersion.Platform.ToString() == "Unix");
		}

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
			StringFormatLeftMiddleNoWrap = BuildStringFormat(StringAlignment.Near, StringAlignment.Center, StringFormatFlags.NoWrap);
			StringFormatCenterMiddleNoWrap = BuildStringFormat(StringAlignment.Center, StringAlignment.Center, StringFormatFlags.NoWrap);
			StringFormatRightMiddleNoWrap = BuildStringFormat(StringAlignment.Far, StringAlignment.Center, StringFormatFlags.NoWrap);

			if (IsUnix())
			{
				m_unixFontSystemName = SystemFonts.MenuFont.Name;
				m_unixFontSystemSize = SystemFonts.MenuFont.Size;
				m_unixFontMonoSpaceName = "Monospace";
				m_unixFontMonoSpaceSize = SystemFonts.MenuFont.Size;

				string gsettingsPath = SkinUtilsCore.LocateExecutable("gsettings"); // gnome
				if (gsettingsPath != "")
				{
					string uFontSystem = SkinUtilsCore.Exec(gsettingsPath, "get org.gnome.desktop.interface font-name").Trim('\'');
					int uFontSystemSep = uFontSystem.LastIndexOf(" ", StringComparison.InvariantCulture);
					if (uFontSystemSep != -1)
					{
						m_unixFontSystemName = uFontSystem.Substring(0, uFontSystemSep).TrimChars(",; \n\r");
						m_unixFontSystemSize = SkinUtilsCore.ToInt32(uFontSystem.Substring(uFontSystemSep + 1).TrimChars(",; \n\r"));
					}

					string uFontMono = SkinUtilsCore.Exec(gsettingsPath, "get org.gnome.desktop.interface monospace-font-name").Trim('\'');
					int uFontMonoSep = uFontMono.LastIndexOf(" ", StringComparison.InvariantCulture);
					if (uFontMonoSep != -1)
					{
						m_unixFontMonoSpaceName = uFontMono.Substring(0, uFontMonoSep).TrimChars(",; \n\r");
						m_unixFontMonoSpaceSize = SkinUtilsCore.ToInt32(uFontMono.Substring(uFontMonoSep + 1).TrimChars(",; \n\r"));
					}
				}

				if (m_unixFontSystemName == "")
					m_unixFontSystemName = "Cantarell";
				if (m_unixFontSystemSize < 6)
					m_unixFontSystemSize = 6;

				if (m_unixFontMonoSpaceName == "")
					m_unixFontMonoSpaceName = "Monospace";
				if (m_unixFontMonoSpaceSize < 6)
					m_unixFontMonoSpaceSize = 6;
			}
		}

		public static StringFormat BuildStringFormat(StringAlignment h, StringAlignment v)
		{
			StringFormat sf = new StringFormat();
			sf.Alignment = h;
			sf.LineAlignment = v;
			sf.Trimming = StringTrimming.None;
			return sf;
		}

		public static StringFormat BuildStringFormat(StringAlignment h, StringAlignment v, StringFormatFlags f)
		{
			StringFormat sf = new StringFormat();
			sf.Alignment = h;
			sf.LineAlignment = v;
			sf.FormatFlags = f;
			sf.Trimming = StringTrimming.None;
			return sf;
		}

		public static Size GetFontSize(Graphics g, Font f, string text)
		{
			return g.MeasureString(text, f).ToSize();
		}

		public static string GetSystemFont()
		{
			if (IsUnix())
				return m_unixFontSystemName + "," + m_unixFontSystemSize;
			else
				return SystemFonts.MenuFont.Name + "," + SystemFonts.MenuFont.Size;
		}

		public static string GetSystemFontMonospace()
		{
			if (IsUnix())
				return m_unixFontMonoSpaceName + "," + m_unixFontMonoSpaceSize;

			string fontName = "";
			if (IsFontInstalled("Consolas"))
				fontName = "Consolas";
			else if (IsFontInstalled("Monospace"))
				fontName = "Monospace";
			else if (IsFontInstalled("DejaVu Sans Mono"))
				fontName = "DejaVu Sans Mono";
			else if (IsFontInstalled("Courier New"))
				fontName = "Courier New";
			else
				fontName = SystemFonts.MenuFont.Name;
			return fontName + "," + SystemFonts.MenuFont.Size;
		}

		public static bool IsFontInstalled(string fontName)
		{
			using (var testFont = new Font(fontName, 8))
			{
				//return 0 == string.Compare(fontName, testFont.Name, StringComparison.InvariantCultureIgnoreCase); // TOCLEAN
				return string.Equals(fontName, testFont.Name, StringComparison.InvariantCultureIgnoreCase);
			}
		}

		public static int GetFontHeight(Graphics g, Font f)
		{
			return GetFontSize(g, f, "SampleStringForFontMeasure123").Height;
		}

		public static Image GetSkinResourceImage(string name)
		{
			// Accessing Properties.Resources.xxx is a lot cpu extensive, probably conversions every time. We cache image resources.
			if (ImageResourceCache.ContainsKey(name))
				return ImageResourceCache[name];
			else
			{
				Bitmap i = (Bitmap)global::Eddie.Forms.Skin.Properties.Resources.ResourceManager.GetObject(name);
				ImageResourceCache[name] = i;
				return i;
			}
		}

		public static void FixHeightVs(Control controlRef, Control controlFix)
		{
			// ComboBox and TextField have dynamic height. This align vertically the related label.
			if (controlFix.Top != controlRef.Top)
				controlFix.Top = controlRef.Top;
			if (controlFix.Height != controlRef.Height)
				controlFix.Height = controlRef.Height;
		}



	}
}

#pragma warning restore CA1416 // Windows only
