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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Text;
using Eddie.Core;

namespace Eddie.Forms
{
	public static class GuiUtils
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
			return (Environment.OSVersion.VersionString.IndexOf("Windows") != -1);
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

				string gsettingsPath = Eddie.Core.Platform.Instance.LocateExecutable("gsettings"); // gnome
				if (gsettingsPath != "")
				{
                    string uFontSystem = SystemShell.Shell1(gsettingsPath, "get org.gnome.desktop.interface font-name").Trim('\'');
                    int uFontSystemSep = uFontSystem.LastIndexOf(" ");
					if (uFontSystemSep != -1)
                    {
                        m_unixFontSystemName = uFontSystem.Substring(0, uFontSystemSep).TrimChars(",; \n\r");
                        m_unixFontSystemSize = Conversions.ToInt32(uFontSystem.Substring(uFontSystemSep + 1).TrimChars(",; \n\r"));
                    }

                    string uFontMono = SystemShell.Shell1(gsettingsPath, "get org.gnome.desktop.interface monospace-font-name").Trim('\'');
                    int uFontMonoSep = uFontMono.LastIndexOf(" ");
                    if (uFontMonoSep != -1)
                    {
                        m_unixFontMonoSpaceName = uFontMono.Substring(0, uFontMonoSep).TrimChars(",; \n\r");
                        m_unixFontMonoSpaceSize = Conversions.ToInt32(uFontMono.Substring(uFontMonoSep + 1).TrimChars(",; \n\r"));
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
				return 0 == string.Compare(
				  fontName,
				  testFont.Name,
				  StringComparison.InvariantCultureIgnoreCase);
			}
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
				Bitmap i = (Bitmap)global::Eddie.Forms.Properties.Resources.ResourceManager.GetObject(name);
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

		public static string DirectoryPicker(string description, string startPath)
		{
			using (FolderBrowserDialog dlg = new FolderBrowserDialog())
			{
				dlg.Description = description;
				dlg.SelectedPath = startPath;
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					return dlg.SelectedPath;
				}
				else
					return "";
			}
		}

		public static string FilePicker()
		{
			return FilePicker(LanguageManager.GetText("FilterAllFiles"));
		}

		public static string FilePicker(string filter)
		{
			using (OpenFileDialog sd = new OpenFileDialog())
			{
				sd.Filter = filter;
				if (sd.ShowDialog() == DialogResult.OK)
					return sd.FileName;
				else
					return "";
			}
		}

		public static void MessageBoxInfo(Form parent, string message)
		{
			Forms.WindowMessage dlg = new Forms.WindowMessage();
			dlg.Kind = Forms.WindowMessage.MessageKind.Info;
			dlg.Body = message;
			dlg.ShowDialog(parent);
			//MessageBox.Show(parent, message, Constants.Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		public static void MessageBoxError(Form parent, string message)
		{
			Forms.WindowMessage dlg = new Forms.WindowMessage();
			dlg.Kind = Forms.WindowMessage.MessageKind.Error;
			dlg.Body = message;
			dlg.ShowDialog(parent);			
		}

		public static bool MessageBoxAskYesNo(Form parent, string message)
		{
			Forms.WindowMessage dlg = new Forms.WindowMessage();
			dlg.Kind = Forms.WindowMessage.MessageKind.YesNo;
			dlg.Body = message;			
			dlg.ShowDialog(parent);
			return dlg.YesNoAnswer;
			//return (MessageBox.Show(parent, message, Constants.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes);
		}

		public static void ClipboardSetText(string t)
		{
			try
			{
				Clipboard.SetText(t);
			}
			catch (Exception e)
			{
				Engine.Instance.Logs.Log(e);
			}
		}

		public static void OpenUrl(string url)
		{
			if(IsWindows())
			{
				System.Diagnostics.Process.Start(url);
			}
			else
			{
				// Temporary, TOFIX
				Eddie.Core.Platform.Instance.OpenUrl(url);
			}
		}

		public static string NormalizeString(string val)
		{
			if(IsWindows())
				return val.Replace("\r\n", "\n").Replace("\n", "\r\n");
			else
				return val.Replace("\r\n", "\n");
		}

		// Workaround to Windows 64-chars limit
		public static void SetNotifyIconText(NotifyIcon ni, string text)
		{
			if (text.Length >= 128) throw new ArgumentOutOfRangeException("Text limited to 127 characters");
			Type t = typeof(NotifyIcon);
			BindingFlags hidden = BindingFlags.NonPublic | BindingFlags.Instance;
			t.GetField("text", hidden).SetValue(ni, text);
			if ((bool)t.GetField("added", hidden).GetValue(ni))
				t.GetMethod("UpdateIcon", hidden).Invoke(ni, new object[] { true });
		}
	}
}
