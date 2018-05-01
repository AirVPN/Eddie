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
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;
using Eddie.Core;

namespace Eddie.Core.Threads
{
	public enum RefreshType
	{
		None = 0,
		Refresh = 1,
		RefreshInvalidate = 2,
	}

	public class Manifest : Eddie.Core.Thread
	{
		public static Manifest Instance;

		private string m_LastResult;
		public RefreshType Refresh = RefreshType.None;

		public AutoResetEvent Updated = new AutoResetEvent(false); // ClodoTemp Disposable issue

		public Manifest()
		{
			Instance = this;
		}

		public string GetLastResult()
		{
			lock (this)
			{
				return m_LastResult;
			}
		}

		public override ThreadPriority GetPriority()
		{
			return ThreadPriority.Lowest;
		}

		public override void OnRun()
		{
			for (;;)
			{
				if ((Refresh != RefreshType.None) || (Engine.ProvidersManager.NeedUpdate(true)))
				{
					m_LastResult = Engine.ProvidersManager.Refresh();
					if (m_LastResult != "")
					{
						//Engine.Instance.Log(Engine.LogType.Warning, Messages.Format(Messages.ManifestUpdate, m_LastResult)); // < 2.9, Warning
						Engine.Instance.Logs.Log(LogType.Verbose, m_LastResult); // >= 2.9, Verbose
					}
					if (Refresh == RefreshType.RefreshInvalidate)
						Engine.Instance.InvalidateConnections();

					Refresh = RefreshType.None;
					Updated.Set();
				}

				for (int i = 0; i < 600; i++) // Every minute
				{
					Sleep(100);
										
					if (CancelRequested)
						return;

					if (Refresh != RefreshType.None)
						break;
				}
			}

		}

		public override void OnStop()
		{
			base.OnStop();

			// Cast as IDisposable to avoid compilation errors on .net2
			(Updated as IDisposable).Dispose();
		}
	}
}