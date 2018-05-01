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
using Eddie.Core;

namespace Eddie.Forms.Linux
{
	public class Engine : Eddie.Forms.Engine
	{
		private static Tray m_tray = null;

		public Engine()
		{
		
		}

		public override bool OnInit2 ()
		{
			bool result = base.OnInit2 ();

			if(Eddie.Core.Engine.Instance.Storage.GetBool("gui.tray_show"))
				m_tray = new Tray ();
		
			return result;
		}

		public override void OnDeInit ()
		{
			if (m_tray != null) {
				m_tray.CancelRequested = true;
				m_tray.SendCommand ("action.exit");
				m_tray.Join ();
				m_tray = null;
			}
			
			base.OnDeInit ();
		}

		public override Json OnCommand (Json data)
		{
			string cmd = data ["command"].Value as string;

			base.OnCommand (data);

			if (cmd == "ui.notification") {
				string pathNotifySend = Core.Platform.Instance.LocateExecutable ("notify-send");
				if (pathNotifySend != "") {
					SystemShell s = new SystemShell ();
					s.Path = pathNotifySend;
					s.Arguments.Add ("--urgency=low");
					s.Arguments.Add ("--expire-time=2000");
					if (data ["level"].Value as string == "infoimportant")
						s.Arguments.Add ("--icon=dialog-information");
					else if (data ["level"].Value as string == "warning")
						s.Arguments.Add ("--icon=dialog-warning");
					else if (data ["level"].Value as string == "error")
						s.Arguments.Add ("--icon=dialog-error");
					else
						s.Arguments.Add ("--icon=dialog-information");
					s.Arguments.Add ("\"" + SystemShell.EscapeInsideQuote (Constants.Name) + "\"");
					string message = SystemShell.EscapeInsideQuote (data ["message"].Value as string);
					message = message.Trim ('-'); // Hack, bad notify-send args parse of quoted string
					s.Arguments.Add ("\"" + message + "\"");
					s.RunAsNormalUser = true;
					s.WaitEnd = false;
					s.Run ();
				}
			} else if (cmd == "ui.color") {
				string color = data ["color"].Value as string;
				if (m_tray != null) {
					if (color == "green") {
						m_tray.SendCommand ("tray.active:true");
						m_tray.SendCommand ("menu.status.icon:stock:gtk-yes");
						m_tray.SendCommand ("menu.connect.text:" + Messages.CommandDisconnect);
					} else {
						m_tray.SendCommand ("tray.active:false");
						if (color == "yellow") {
							m_tray.SendCommand ("menu.status.icon:stock:gtk-media-play");
							m_tray.SendCommand ("menu.connect.text:" + Messages.CommandCancel);
						} else {
							m_tray.SendCommand ("menu.status.icon:stock:gtk-no");
							m_tray.SendCommand ("menu.connect.text:" + Messages.CommandConnect);
						}
					}
				}
			} else if (cmd == "ui.status") {
				string full = data ["full"].Value as string;
				if (m_tray != null) {
					m_tray.SendCommand ("menu.status.text:> " + full);
				}
			} 

			return null;
		}

		public override void OnChangeMainFormVisibility (bool vis)
		{
			base.OnChangeMainFormVisibility (vis);

			if (m_tray != null) {
				if (Engine.Storage.GetBool("gui.tray_minimized"))
				{
					m_tray.SendCommand ("menu.restore.visible:true");

					if (vis)
						m_tray.SendCommand ("menu.restore.text:" + Messages.WindowsMainHide);
					else
						m_tray.SendCommand ("menu.restore.text:" + Messages.WindowsMainShow);
				}
				else
				{
					m_tray.SendCommand ("menu.restore.visible:false");
				}
			}
		}

		public override bool AllowMinimizeInTray()
		{
			if (Engine.Storage != null)
			{
				if (Engine.Storage.GetBool("gui.tray_minimized") == false)
				{
					return false;
				}
			}

			if (m_tray == null)
				return false;
			
			if (m_tray.IsStarted ())
				return true;
			
			return false;
		}
	}
}
