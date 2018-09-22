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
using System.IO;
using System.Threading;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Eddie.Common;

namespace Eddie.Forms
{
	public class UiClient : Eddie.Common.UiClient
	{
		public static UiClient Instance;
		public Eddie.Forms.Forms.Main FormMain;

		public ApplicationContext AppContext;
		public Eddie.Forms.Engine Engine;

		public override bool Init()
		{
			base.Init();

			// ClodoTemp
			/*
			Forms.WindowSplash splash = new Forms.WindowSplash();
			splash.Visible = true;

			for (int i = 0; i < 100; i++)
			{
				splash.SetStatus(i.ToString());
				Thread.Sleep(10);
			}
			*/

			GuiUtils.Init();

			Instance = this;

			if(Engine == null)
				Engine = new Eddie.Forms.Engine();
			Engine.TerminateEvent += Engine_TerminateEvent;

			if (Engine.Initialization(false) == false)
				return false;

			FormMain = new Eddie.Forms.Forms.Main();
			Engine.FormMain = FormMain; // ClodoTemp2 - remove?

			Engine.Instance.UiManager.Add(this);

			Engine.UiStart();

			FormMain.LoadPhase();

			AppContext = new ApplicationContext();


			// ClodoTemp
			/*
			splash.RequestClose();
			*/

			return true;
		}

		private void Engine_TerminateEvent()
		{			
			AppContext.ExitThread();

			if (GuiUtils.IsUnix ()) {
				System.Windows.Forms.Application.Exit();
			}
			//Application.Exit(); // Removed in 2.12, otherwise lock Core thread. Still required in Linux edition.
		}

		public override Json Command(Json data)
		{			
			string cmd = data["command"].Value as string;
			if (cmd == "ui.show.manifest")
			{
				FormMain.OnShowText("Json Data", Data.ToJsonPretty());
				return null;
			}
			else
				return Engine.UiManager.OnCommand(data, this);
		}

		public override void OnReceive(Json data)
		{
			base.OnReceive(data);

			// Engine.Instance.Logs.LogVerbose("OnReceive:" + data.ToJson()); // TOCLEAN

			string cmd = data["command"].Value as string;

			if (cmd == "test2")
			{
				Forms.WindowMan w = new Forms.WindowMan(); // ClodoTemp
				w.ShowDialog();
			}
			else if (cmd == "ui.notification")
			{
				if (FormMain != null)
					FormMain.ShowWindowsNotification(data["level"].Value as string, data["message"].Value as string);
			}
			else if (cmd == "ui.color")
			{
				if (FormMain != null)
					FormMain.SetColor(data["color"].Value as string);
			}
			else if (cmd == "ui.status")
			{
				string textFull = data["full"].Value as string;
				string textShort = textFull;
				if (data.HasKey("short"))
					textShort = data["short"].Value as string;
				if (FormMain != null)
					FormMain.SetStatus(textFull, textShort);
			}
			else if (cmd == "ui.updater.available")
			{
				FormMain.ShowUpdater();
			}
			else if (cmd == "system.report.progress")
			{
				string step = data["step"].Value as string;
				string text = data["body"].Value as string;
				int perc = Conversions.ToInt32(data["perc"].Value, 0);

				if (FormMain != null)
					FormMain.OnSystemReport(step, text, perc);
			}
		}
	}
}
