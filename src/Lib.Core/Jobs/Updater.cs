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
using System.Threading;
using System.Text;
using Eddie.Core;
using Eddie.Common;

namespace Eddie.Core.Jobs
{
	public class Updater : Eddie.Core.Job
	{
		public string m_lastVersionNotification = "";

		public override ThreadPriority GetPriority()
		{
			return ThreadPriority.Lowest;
		}

		public override bool GetSync()
		{
			return false;
		}

		public override void OnRun()
		{
			if (m_lastVersionNotification == "")
				m_lastVersionNotification = Constants.VersionDesc;

			string channel = Engine.Instance.Storage.Get("updater.channel");

			try
			{
				HttpRequest request = new HttpRequest();
				request.Url = Constants.WebSite + "/download/";
				request.Url += "?mode=info";
				request.Url += "&platform=" + Platform.Instance.GetCodeInstaller();
				request.Url += "&arch=" + Platform.Instance.GetArchitecture();
				request.Url += "&ui=" + "ui";
				request.Url += "&format=updater";
				request.Url += "&version=" + channel;
				HttpResponse response = Engine.Instance.FetchUrl(request);

				Json j = null;
				if (Json.TryParse(response.GetBody(), out j))
				{
					string latestVersion = j["version"].Value as string;

					int compare = UtilsCore.CompareVersions(m_lastVersionNotification, latestVersion);

					if (compare == -1)
					{
						Json jUpdaterAvailable = new Json();
						jUpdaterAvailable["command"].Value = "ui.updater.available";

						Engine.Instance.UiManager.Broadcast(jUpdaterAvailable);
					}

					m_lastVersionNotification = latestVersion;

					m_timeEvery = 60 * 60 * 24 * 1000;
				}
				else
				{
					// Error, retry later
					m_timeEvery = 60 * 60 * 3 * 1000;
				}
			}
			catch
			{
				// Error, retry later
				m_timeEvery = 60 * 60 * 3 * 1000;
			}
		}
	}
}
