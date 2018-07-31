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
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Text;
using Eddie.Common;

namespace Eddie.Forms
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

		private static string m_unixFontSystem;
		private static string m_unixFontMonoSpace;

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

			if(IsUnix())
			{
				m_unixFontSystem = "";
				string gsettingsPath = Eddie.Core.Platform.Instance.LocateExecutable("gsettings"); // gnome
				if (gsettingsPath != "")
				{
					m_unixFontSystem = ShellSync(gsettingsPath, "get org.gnome.desktop.interface font-name").Trim('\'');
					int posSize = m_unixFontSystem.LastIndexOf(" ");
					if (posSize != -1)
						m_unixFontSystem = m_unixFontSystem.Substring(0, posSize) + "," + m_unixFontSystem.Substring(posSize + 1);
				}

				m_unixFontMonoSpace = "";
				if (gsettingsPath != "")
				{
					m_unixFontMonoSpace = ShellSync(gsettingsPath, "get org.gnome.desktop.interface monospace-font-name").Trim('\'');
					int posSize = m_unixFontMonoSpace.LastIndexOf(" ");
					if (posSize != -1)
						m_unixFontMonoSpace = m_unixFontMonoSpace.Substring(0, posSize) + "," + m_unixFontMonoSpace.Substring(posSize + 1);
				}
			}
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

		public static string GetSystemFont()
		{
			if ((IsUnix()) && (m_unixFontSystem != ""))
				return m_unixFontSystem;

			return SystemFonts.MenuFont.Name + "," + SystemFonts.MenuFont.Size;
		}

		public static string GetSystemFontMonospace()
		{
			if ((IsUnix()) && (m_unixFontMonoSpace != ""))
				return m_unixFontMonoSpace;

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
			return FilePicker(Messages.FilterAllFiles);
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

		public static void ShellSync(string path, string[] arguments, out string stdout, out string stderr, out int exitCode)
		{
			try
			{
				using (Process p = new Process())
				{
					p.StartInfo.FileName = path;
					p.StartInfo.Arguments = String.Join(" ", arguments);
					p.StartInfo.WorkingDirectory = "";
					p.StartInfo.CreateNoWindow = true;
					p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
					p.StartInfo.UseShellExecute = false;
					p.StartInfo.RedirectStandardOutput = true;
					p.StartInfo.RedirectStandardError = true;
					p.Start();

					stdout = p.StandardOutput.ReadToEnd().Trim();
					stderr = p.StandardError.ReadToEnd().Trim();

					p.WaitForExit();

					exitCode = p.ExitCode;
				}
			}
			catch (Exception ex)
			{
				stdout = "";
				stderr = "Error: " + ex.Message;
				exitCode = -1;
			}
		}

		public static string ShellSync(string path, string arg1)
		{
			string stdout;
			string stderr;
			int exitCode;
			ShellSync(path, new string[] { arg1 }, out stdout, out stderr, out exitCode);
			return NormalizeString(stdout + "\n" + stderr).Trim();
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
