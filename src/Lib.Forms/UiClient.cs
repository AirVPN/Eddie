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
using System.IO;
using System.Threading;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Eddie.Core;

namespace Eddie.Forms
{
	public class UiClient : Eddie.Core.UiClient
	{
		public static UiClient Instance;
		public Forms.Main MainWindow;
		public Forms.WindowSplash SplashWindow;

		public ApplicationContext AppContext;
		public Eddie.Forms.Engine Engine;

		public override bool Init(string environmentCommandLine)
		{
			Instance = this;

			AppContext = new ApplicationContext();

			base.Init(environmentCommandLine);

			try
			{
				Skin.SkinUtils.Init();

				SplashWindow = new Forms.WindowSplash();
				SplashWindow.Show();
			}
			catch (Exception ex)
			{
				Engine.Instance.Logs.LogFatal("Cannot initialize UI. Probably a DISPLAY issue, ensure your are not running as root. Error:" + ex.Message);
				return false;
			}


			if (Engine == null)
				Engine = new Eddie.Forms.Engine(environmentCommandLine);
			Engine.TerminateEvent += Engine_TerminateEvent;
			Engine.UiManager.Add(this);

			Engine.Start();

			return true;
		}

		public void OnUnhandledException(string source, Exception ex)
		{
			if (Engine != null)
				Engine.OnUnhandledException(source, ex);
		}

		private void Engine_TerminateEvent()
		{
			if (SplashWindow != null)
				SplashWindow.RequestClose();

			if (MainWindow != null)
				MainWindow.RequestClose();

			if (AppContext != null)
				AppContext.ExitThread();

			if (Platform.Instance.IsUnixSystem())
			{
				System.Windows.Forms.Application.Exit();
			}
			//Application.Exit(); // Removed in 2.12, otherwise lock Core thread. Still required in Linux edition.
		}

		public override Json Command(Json data)
		{
			string cmd = data["command"].Value as string;
			if (cmd == "ui.show.manifest")
			{
				MainWindow.OnShowText("Json Data", Data.ToJsonPretty());
				return null;
			}
			else
				return Engine.UiManager.SendCommand(data, this);
		}

		public override void OnReceive(Json data)
		{
			base.OnReceive(data);

			string cmd = data["command"].Value as string;

			if (cmd == "log")
			{
				if (data["type"].Value as string == "fatal")
				{
					if (SplashWindow != null)
						SplashWindow.OnMessageError(data["message"].Value as string);
					else if (MainWindow != null)
						MainWindow.OnMessageError(data["message"].Value as string);
					else
						GuiUtils.MessageBoxError(null, data["message"].Value as string);
				}
			}
			else if (cmd == "init.step")
			{
				if (SplashWindow != null)
					SplashWindow.SetStatus(data["message"].Value as string);
			}
			else if (cmd == "engine.ui")
			{
				Skin.SkinForm.Skin.ClearFontCache(); // Splash loaded before options

				SplashWindow.RequestMain();
			}
			else if (cmd == "ui.restarted")
			{
				// Hide Splash when waiting elevated subprocess 
				SplashWindow.RequestClose();
			}
			else if (cmd == "ui.notification")
			{
				if (MainWindow != null)
					MainWindow.ShowWindowsNotification(data["level"].Value as string, data["message"].Value as string);
			}
			else if (cmd == "ui.main-status")
			{
				string appIcon = data["app_icon"].Value as string;
				string appColor = data["app_color"].Value as string;
				string actionIcon = data["action_icon"].Value as string;
				string actionCommand = data["action_command"].Value as string;
				string actionText = data["action_text"].Value as string;
				if (MainWindow != null)
					MainWindow.SetMainStatus(appIcon, appColor, actionIcon, actionCommand, actionText);
			}
			else if (cmd == "ui.status")
			{
				string textFull = data["full"].Value as string;
				string textShort = textFull;
				if (data.HasKey("short"))
					textShort = data["short"].Value as string;
				if (MainWindow != null)
					MainWindow.SetStatus(textFull, textShort);
			}
			else if (cmd == "ui.updater.available")
			{
				MainWindow.ShowUpdater();
			}
			else if (cmd == "ui.frontmessage")
			{
				if (UiClient.Instance.MainWindow != null)
					UiClient.Instance.MainWindow.OnFrontMessage(data["message"].Value as Json);
			}
			else if (cmd == "system.report.progress")
			{
				string step = data["step"].Value as string;
				string text = data["body"].Value as string;
				int perc = Conversions.ToInt32(data["perc"].Value, 0);

				if (MainWindow != null)
					MainWindow.OnSystemReport(step, text, perc);
			}
		}
	}
}
