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
using Eddie.Core;
using Eddie.Common;

namespace Eddie.Forms.Linux
{
	public class UiClient : Eddie.Forms.UiClient
	{
		public Tray Tray
		{
			get
			{
				return (Engine as Eddie.Forms.Linux.Engine).Tray;
			}
		}

		public override void OnReceive(Json data)
		{
			base.OnReceive(data);

			string cmd = data["command"].Value as string;

			if (cmd == "ui.notification")
			{
				if (Eddie.Core.Platform.Instance.CanShellAsNormalUser())
				{
					string pathNotifySend = Core.Platform.Instance.LocateExecutable("notify-send");
					if (pathNotifySend != "")
					{
						SystemShell s = new SystemShell();
						s.Path = pathNotifySend;
						s.Arguments.Add("--urgency=low");
						s.Arguments.Add("--expire-time=2000");
						if (data["level"].Value as string == "infoimportant")
							s.Arguments.Add("--icon=dialog-information");
						else if (data["level"].Value as string == "warning")
							s.Arguments.Add("--icon=dialog-warning");
						else if (data["level"].Value as string == "error")
							s.Arguments.Add("--icon=dialog-error");
						else
							s.Arguments.Add("--icon=dialog-information");
						s.Arguments.Add("\"" + SystemShell.EscapeInsideQuote(Constants.Name) + "\"");
						string message = SystemShell.EscapeInsideQuote(data["message"].Value as string);
						message = message.Trim('-'); // Hack, bad notify-send args parse of quoted string
						s.Arguments.Add("\"" + message + "\"");
						s.RunAsNormalUser = true;
						s.WaitEnd = false;
						s.Run();
					}
				}
			}
			else if (cmd == "ui.color")
			{
				string color = data["color"].Value as string;
				if (Tray != null)
				{
					if (color == "green")
					{
						Tray.SendCommand("tray.active:true");
						Tray.SendCommand("menu.status.icon:stock:gtk-yes");
						Tray.SendCommand("menu.connect.text:" + Messages.CommandDisconnect);
					}
					else
					{
						Tray.SendCommand("tray.active:false");
						if (color == "yellow")
						{
							Tray.SendCommand("menu.status.icon:stock:gtk-media-play");
							Tray.SendCommand("menu.connect.text:" + Messages.CommandCancel);
						}
						else
						{
							Tray.SendCommand("menu.status.icon:stock:gtk-no");
							Tray.SendCommand("menu.connect.text:" + Messages.CommandConnect);
						}
					}
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
		}
	}
}
