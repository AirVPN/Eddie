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
	public class Engine : Eddie.Forms.Engine
	{


		public Engine(string environmentCommandLine) : base(environmentCommandLine)
		{
		}

		public UiClient UiClient
		{
			get
			{
				return Eddie.Forms.UiClient.Instance as UiClient;
			}
		}

		public override void OnChangeMainFormVisibility(bool vis)
		{
			base.OnChangeMainFormVisibility(vis);

			if ((UiClient.Tray != null) && (UiClient.Tray.IsStarted()))
			{
				if (Engine.ProfileOptions.GetBool("gui.tray_show"))
				{
					UiClient.Tray.SendCommand("menu.restore.visible:true");

					if (vis)
						UiClient.Tray.SendCommand("menu.restore.text:" + LanguageManager.GetText(LanguageItems.WindowsMainHide));
					else
						UiClient.Tray.SendCommand("menu.restore.text:" + LanguageManager.GetText(LanguageItems.WindowsMainShow));
				}
				else
				{
					UiClient.Tray.SendCommand("menu.restore.visible:false");
				}
			}
		}

		public override bool AllowMinimizeInTray()
		{
			if (Engine.Storage != null)
			{
				if (Engine.ProfileOptions.GetBool("gui.tray_show") == false)
				{
					return false;
				}
			}

			if (UiClient.Tray == null)
				return false;

			if (UiClient.Tray.IsStarted())
				return true;

			return false;
		}
	}
}
