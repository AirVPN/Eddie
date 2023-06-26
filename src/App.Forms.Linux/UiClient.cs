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
using System.IO;
using System.Threading;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Eddie.Core;

namespace Eddie.Forms.Linux
{
	public class UiClient : Eddie.Forms.UiClient
	{
		public Tray Tray = null;

		public override void OnReceive(Json data)
		{
			string cmd = data["command"].Value as string;

			if (cmd == "engine.shutdown")
			{
				if (Tray != null)
				{
					Tray.CancelRequested = true;
					Tray.SendCommand("action.exit");

					// Tray.Join(); // sometime don't exit...
					if (Tray.Join(2000) == false)
						Tray.Abort();

					Tray = null;
				}
			}
			else if (cmd == "engine.ui")
			{
				if (Eddie.Core.Engine.Instance.ProfileOptions.GetBool("gui.tray_show"))
				{
					Tray = new Tray();
					for (int t = 0; t < 3000; t += 100)
					{
						if (Tray.IsStarted())
							break;
						System.Threading.Thread.Sleep(100);
					}
				}
			}
			else if (cmd == "ui.notification")
			{
				string pathNotifySend = Core.Platform.Instance.LocateExecutable("notify-send");
				if (pathNotifySend != "")
				{
					SystemExec exec = new SystemExec();
					exec.Path = pathNotifySend;
					exec.Arguments.Add("--urgency=low");
					//s.Arguments.Add("--expire-time=2000");
					if (data["level"].Value as string == "infoimportant")
						exec.Arguments.Add("--icon=dialog-information");
					else if (data["level"].Value as string == "warning")
						exec.Arguments.Add("--icon=dialog-warning");
					else if (data["level"].Value as string == "error")
						exec.Arguments.Add("--icon=dialog-error");
					else if (data["level"].Value as string == "fatal")
						exec.Arguments.Add("--icon=dialog-error");
					else
						exec.Arguments.Add("--icon=dialog-information");
					exec.Arguments.Add("\"" + SystemExec.EscapeInsideQuote(Constants.Name) + "\"");
					string message = SystemExec.EscapeInsideQuote(data["message"].Value as string);
					message = message.Trim('-'); // Hack, bad notify-send args parse of quoted string
					exec.Arguments.Add("\"" + message + "\"");
					exec.WaitEnd = false;
					exec.Run();
				}
			}
			else if (cmd == "ui.main-status")
			{
				string appIcon = data["app_icon"].Value as string;
				string appColor = data["app_color"].Value as string;
				string actionIcon = data["action_icon"].Value as string;
				string actionCommand = data["action_command"].Value as string;
				string actionText = data["action_text"].Value as string;
				if (Tray != null)
				{
					if (appColor == "green")
						Tray.SendCommand("tray.active:true");
					else
						Tray.SendCommand("tray.active:false");

					if (appColor == "green")
					{
						Tray.SendCommand("menu.status.icon:stock:gtk-yes");
						Tray.SendCommand("menu.connect.text:" + LanguageManager.GetText(LanguageItems.CommandDisconnect));
					}
					else if (appColor == "yellow")
					{
						Tray.SendCommand("menu.status.icon:stock:gtk-media-play");
					}
					else
					{
						Tray.SendCommand("menu.status.icon:stock:gtk-no");
					}

					Tray.SendCommand("menu.connect.text:" + actionText);
					Tray.SendCommand("menu.connect.enable:" + ((actionCommand != "") ? "true" : "false"));
				}
			}
			else if (cmd == "ui.status")
			{
				string full = data["full"].Value as string;
				if (Tray != null)
				{
					Tray.SendCommand("menu.status.text:> " + full);
				}
			}

			base.OnReceive(data);
		}
	}
}
