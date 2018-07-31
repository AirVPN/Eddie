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
		public Tray Tray = null;

		public Engine()
		{
		
		}

		public override bool OnInit2 ()
		{
			bool result = base.OnInit2 ();

			if( (Eddie.Core.Engine.Instance.Storage.GetBool("gui.tray_show")) && (Eddie.Core.Platform.Instance.CanShellAsNormalUser()) )
				Tray = new Tray ();
		
			return result;
		}

		public override void OnDeInit ()
		{
			if (Tray != null) {
				Tray.CancelRequested = true;
				Tray.SendCommand ("action.exit");
				Tray.Join ();
				Tray = null;
			}
			
			base.OnDeInit ();
		}
		
		public override void OnChangeMainFormVisibility (bool vis)
		{
			base.OnChangeMainFormVisibility (vis);

			if (Tray != null) 
			{
				if (Engine.Storage.GetBool("gui.tray_minimized"))
				{
					Tray.SendCommand ("menu.restore.visible:true");

					if (vis)
						Tray.SendCommand ("menu.restore.text:" + Messages.WindowsMainHide);
					else
						Tray.SendCommand ("menu.restore.text:" + Messages.WindowsMainShow);
				}
				else
				{
					Tray.SendCommand ("menu.restore.visible:false");
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

			if (Tray == null)
				return false;
			
			if (Tray.IsStarted ())
				return true;
			
			return false;
		}
	}
}
