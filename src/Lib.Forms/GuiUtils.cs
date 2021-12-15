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

using Eddie.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace Eddie.Forms
{
	public static class GuiUtils
	{
		private static Dictionary<String, Bitmap> ImageResourceCache = new Dictionary<String, Bitmap>();

		public static Image GetResourceImage(string name)
		{
			// Accessing Properties.Resources.xxx is a lot cpu extensive, probably conversions every time. We cache image resources.
			if (ImageResourceCache.ContainsKey(name))
				return ImageResourceCache[name];
			else
			{
				Bitmap i = (Bitmap)global::Eddie.Forms.Properties.Resources.ResourceManager.GetObject(name);
				ImageResourceCache[name] = i;
				return i;
			}
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

		public static void ClipboardSetText(Form parent, string text)
		{
			string error = "";

			Application.UseWaitCursor = true;

			// Loop retry because sometime throw "Requested Clipboard operation did not succeed.", probably concurrency with other apps.
			for (int t = 0; t < 10; t++)
			{
				System.Threading.Thread.Sleep(100);
				try
				{
					Clipboard.SetText(text);
					error = "";
					break;
				}
				catch (Exception ex)
				{
					error = ex.Message;
				}
			}

			Application.UseWaitCursor = false;

			if (error == "")
				MessageBoxInfo(parent, LanguageManager.GetText("LogsCopyClipboardDone"));
			else
				MessageBoxError(parent, error);
		}

		public static void OpenUrl(string url)
		{
			if (Skin.SkinUtils.IsWindows())
			{
				System.Diagnostics.Process.Start(url);
			}
			else
			{
				// Temporary, TOFIX
				Eddie.Core.Platform.Instance.OpenUrl(url);
			}
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
