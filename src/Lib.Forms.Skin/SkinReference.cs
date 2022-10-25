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

using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

#pragma warning disable CA1416 // Windows only

namespace Eddie.Forms.Skin
{
	public class SkinReference
	{
		public static string FontName = "";
		public static float FontSize = 0;
		private static SkinJson m_jsonSkin;

		private static Dictionary<string, Font> GdiCacheFonts = new Dictionary<string, Font>();
		private static Dictionary<string, Brush> GdiCacheBrushes = new Dictionary<string, Brush>();
		private static Dictionary<string, Pen> GdiCachePens = new Dictionary<string, Pen>();
		private static Dictionary<string, Color> GdiCacheColors = new Dictionary<string, Color>();

		public static void Load()
		{
			m_jsonSkin = SkinJson.Parse(System.Text.Encoding.UTF8.GetString(Eddie.Forms.Skin.Properties.Resources.skin));
		}

		public string GetItem(string name)
		{
			if (m_jsonSkin == null) // Designer
				Load();

			return m_jsonSkin[name].ValueString;
		}

		public string GetStyle()
		{
			string value = GetItem("style");
			return value;
		}

		public Font GetFont(string name)
		{
			string value = GetItem(name);
			return GetFontEx(value);
		}

		public Brush GetBrush(string name)
		{
			string value = GetItem(name);
			return GetBrushEx(value);
		}

		public Pen GetPen(string name)
		{
			string value = GetItem(name);
			return GetPenEx(value);
		}

		public Color GetColor(string name)
		{
			string value = GetItem(name);
			return GetColorEx(value);
		}

		public void ClearFontCache()
		{
			GdiCacheFonts.Clear();
		}

		public Font GetFontEx(string name)
		{
			if (GdiCacheFonts.ContainsKey(name))
				return GdiCacheFonts[name];
			else
			{
				string fontName = name;
				string fontSize = "";
				if (name.IndexOf(',') != -1)
				{
					fontName = name.Substring(0, name.IndexOf(',')).Trim();
					fontSize = name.Substring(name.IndexOf(',') + 1).Trim();
				}

				double userBaseSize = FontSize;
				if (userBaseSize == 0)
				{
					string systemFont = SkinUtils.GetSystemFont();
					int posSize = systemFont.IndexOf(",");

					string strSize = systemFont.Substring(posSize + 1);
					if (posSize != -1)
						double.TryParse(strSize, out userBaseSize);

					if (userBaseSize == 0)
						userBaseSize = 10;
				}

				if ((fontName == "System") || (fontName == "SystemMonospace"))
				{
					string systemFont = "";
					if (fontName == "System")
					{
						if (FontName != "")
							systemFont = FontName;
						else
							systemFont = SkinUtils.GetSystemFont();
					}

					else if (fontName == "SystemMonospace")
						systemFont = SkinUtils.GetSystemFontMonospace();
					int posSize = systemFont.IndexOf(",");
					if (posSize != -1)
						systemFont = systemFont.Substring(0, posSize);
					fontName = systemFont;
				}

				if (fontSize == "normal")
					fontSize = userBaseSize.ToString(CultureInfo.InvariantCulture) + "pt";
				else if (fontSize == "big")
					fontSize = (userBaseSize * 1.25).ToString(CultureInfo.InvariantCulture) + "pt";
				else if (fontSize == "small")
					fontSize = (userBaseSize / 1.25).ToString(CultureInfo.InvariantCulture) + "pt";

				string name2 = fontName + "," + fontSize;

				FontConverter fontConverter = new FontConverter();
				Font f = fontConverter.ConvertFromInvariantString(name2) as Font;
				GdiCacheFonts[name] = f;

				return f;
			}
		}



		public Brush GetBrushEx(string value)
		{
			if (GdiCacheBrushes.ContainsKey(value))
				return GdiCacheBrushes[value];
			else
			{
				Brush b = null;
				b = new SolidBrush(GetColorEx(value));
				GdiCacheBrushes[value] = b;
				return b;
			}
		}

		public Pen GetPenEx(string value)
		{
			if (GdiCachePens.ContainsKey(value))
				return GdiCachePens[value];
			else
			{
				Pen p = new Pen(GetColorEx(value));
				GdiCachePens[value] = p;
				return p;
			}
		}

		public Color GetColorEx(string value)
		{
			if (GdiCacheColors.ContainsKey(value))
				return GdiCacheColors[value];
			else
			{
				Color c = System.Drawing.ColorTranslator.FromHtml(value);
				GdiCacheColors[value] = c;
				return c;
			}
		}


		public virtual void Apply(Control c)
		{
			c.Font = FontNormal;

			if (c is Skin.CheckBox)
			{
				Skin.CheckBox c2 = c as Skin.CheckBox;

				c2.BackColor = Color.Transparent;
				c2.ForeColor = ForeColor;

				if (GetStyle() == "flat")
					c2.FlatStyle = FlatStyle.Flat;
				else
					c2.FlatStyle = FlatStyle.Standard;
			}

			if (c is Skin.ComboBox)
			{
				Skin.ComboBox c2 = c as Skin.ComboBox;

				c2.BackColor = BackColor;
				c2.ForeColor = ForeColor;

				if (GetStyle() == "flat")
					c2.FlatStyle = FlatStyle.Flat;
				else
					c2.FlatStyle = FlatStyle.Standard;
			}

			if (c is Skin.TextBox)
			{
				Skin.TextBox c2 = c as Skin.TextBox;

				if (c2.ReadOnly)
					c2.BackColor = ReadOnlyBackColor;
				else
					c2.BackColor = BackColor;
				c2.ForeColor = ForeColor;


				if (GetStyle() == "flat")
					c2.BorderStyle = BorderStyle.FixedSingle;
				else
					c2.BorderStyle = BorderStyle.Fixed3D;
			}

			if (c is Skin.Label)
			{
			}

			if (c is Skin.RadioButton)
			{
				Skin.RadioButton c2 = c as Skin.RadioButton;

				c2.BackColor = Color.Transparent;
				c2.ForeColor = ForeColor;

				if (GetStyle() == "flat")
					c2.FlatStyle = FlatStyle.Flat;
				else
					c2.FlatStyle = FlatStyle.Standard;
			}

			if (c is Skin.LinkLabel)
			{
				Skin.LinkLabel c2 = c as Skin.LinkLabel;

				c2.BackColor = Color.Transparent;
				c2.ForeColor = HyperLinkForeColor;
				//c2.ActiveLinkColor = HyperLinkColor;
				//c2.LinkColor = HyperLinkColor;                
				//c2.VisitedLinkColor = HyperLinkColor;                
			}

			if (c is Skin.TabPage)
			{
				Skin.TabPage c2 = c as Skin.TabPage;

				c2.BackColor = Color.Transparent;
			}

			if (c is Skin.ListView)
			{
				Skin.ListView c2 = c as Skin.ListView;

				c2.BackColor = BackColor;
				c2.ForeColor = ForeColor;

				if (GetStyle() == "flat")
					c2.BorderStyle = BorderStyle.FixedSingle;
				else
					c2.BorderStyle = BorderStyle.Fixed3D;
			}

			if (c is Skin.Button)
			{
				Skin.Button c2 = c as Skin.Button;

				c2.ForeColor = ForeColor;

				//c2.UpdateBackground();
			}

			if (c is System.Windows.Forms.ContextMenuStrip)
			{
				System.Windows.Forms.ContextMenuStrip c2 = c as System.Windows.Forms.ContextMenuStrip;
				foreach (System.Windows.Forms.ToolStripItem i in c2.Items)
				{
					if (SkinUtilsCore.IsUnix())
					{
						// Fix Mono colors issue (for example Arch with dark theme)						
						if (i.ForeColor == SystemColors.ControlText)
							i.ForeColor = Color.Black;
						if (i.BackColor == SystemColors.Control)
							i.BackColor = Color.White;
					}
				}
			}

			foreach (Control sc in c.Controls)
			{
				Apply(sc);
			}

			c.Invalidate();
		}

		public virtual Color ForeColor
		{
			get
			{
				return GetColor("color.foreground");
			}
		}

		public virtual Color BackColor
		{
			get
			{
				return GetColor("color.background");
			}
		}

		public virtual Color ReadOnlyBackColor
		{
			get
			{
				return GetColor("color.readonly.background");
			}
		}

		public virtual Color HyperLinkForeColor
		{
			get
			{
				return GetColor("color.hyperlink.normal.foreground");
			}
		}

		public virtual Color HyperLinkHoverForeColor
		{
			get
			{
				return GetColor("color.hyperlink.hover.foreground");
			}
		}

		public virtual Color HyperLinkBackColor
		{
			get
			{
				return GetColor("color.hyperlink.normal.background");
			}
		}

		public virtual Color HyperLinkHoverBackColor
		{
			get
			{
				return GetColor("color.hyperlink.hover.background");
			}
		}

		public virtual Brush ForeBrush
		{
			get
			{
				return GetBrush("color.foreground");
			}
		}

		public virtual Brush BackDisabledBrush
		{
			get
			{
				return GetBrush("color.disabled.background");
			}
		}

		public virtual Brush ForeDisabledBrush
		{
			get
			{
				return GetBrush("color.disabled.foreground");
			}
		}

		public virtual Brush ListViewDisabledBackBrush
		{
			get
			{
				return GetBrush("color.grid.disabled.background");
			}
		}

		public virtual Brush ListViewNormalBackBrush
		{
			get
			{
				return GetBrush("color.grid.item1.background");
			}
		}

		public virtual Brush ListViewNormal2BackBrush
		{
			get
			{
				return GetBrush("color.grid.item2.background");
			}
		}

		public virtual Brush ListViewSelectedBackBrush
		{
			get
			{
				return GetBrush("color.grid.selected.background");
			}
		}

		public virtual Brush ListViewFocusedBackBrush
		{
			get
			{
				return GetBrush("color.grid.focus.background");
			}
		}

		public virtual Pen ListViewGridPen
		{
			get
			{
				return GetPen("color.grid.border");
			}
		}

		public virtual Image FormBackgroundImage
		{
			get
			{
				return SkinUtils.GetSkinResourceImage("form_l_bg");
			}
		}

		public virtual Image ButtonNormalImage
		{
			get
			{
				return SkinUtils.GetSkinResourceImage("btn_l_n");
			}
		}

		public virtual Image ButtonHoverImage
		{
			get
			{
				return SkinUtils.GetSkinResourceImage("btn_l_h");
			}
		}

		public virtual Image ButtonDisabledImage
		{
			get
			{
				return SkinUtils.GetSkinResourceImage("btn_l_d");
			}
		}

		public virtual Image MainBackImage
		{
			get
			{
				return SkinUtils.GetSkinResourceImage("main_l_bg");
			}
		}

		public virtual Font FontNormal
		{
			get
			{
				return GetFont("font.normal");
			}
		}

		public virtual Font FontBig
		{
			get
			{
				return GetFont("font.big");
			}
		}

		public virtual Font FontMono
		{
			get
			{
				return GetFont("font.monospace.normal");
			}
		}

		public virtual Font FontMonoBig
		{
			get
			{
				return GetFont("font.monospace.big");
			}
		}

		public virtual Size MenuImageSize
		{
			get
			{
				int s = GetFont("font.normal").Height;
				if (s < 16)
					s = 16;

				return new Size(s, s);
			}
		}

		public virtual void GraphicsCommon(Graphics g)
		{
			if (g.PixelOffsetMode != System.Drawing.Drawing2D.PixelOffsetMode.Half)
				g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
			if (g.InterpolationMode != System.Drawing.Drawing2D.InterpolationMode.Default)
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
		}
	}
}

#pragma warning restore CA1416 // Windows only
