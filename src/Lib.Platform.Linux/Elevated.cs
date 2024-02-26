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
using System.Globalization;
using Eddie.Core;

namespace Eddie.Platform.Linux
{
	public class ElevatedImpl : Eddie.Core.Elevated.ISocket
	{
		public override void Start()
		{
			base.Start();

			try
			{
				string connectResult = Connect(Engine.Instance.GetElevatedServicePort());

				if (connectResult != "Ok") // Will work if the service is active
				{
					if (connectResult != "No listening")
						Engine.Instance.Logs.LogVerbose("Elevated in listening, but refuse connection: " + connectResult);

					Engine.Instance.UiManager.Broadcast("init.step", "message", LanguageManager.GetText(LanguageItems.InitStepRaiseSystemPrivileges));
					Engine.Instance.Logs.LogVerbose(LanguageManager.GetText(LanguageItems.InitStepRaiseSystemPrivileges));

					int port = GetPortSpot();

					int pid = (Platform.Instance as Eddie.Platform.Linux.Platform).RunElevated(new string[] { "mode=spot", "spot_port=" + port.ToString(CultureInfo.InvariantCulture), "service_port=" + Engine.Instance.GetElevatedServicePort().ToString(CultureInfo.InvariantCulture) }, false);

					System.Diagnostics.Process process = null;
					if (pid > 0)
						process = System.Diagnostics.Process.GetProcessById(pid);

					try
					{
						long listeningPortStartTime = Utils.UnixTimeStamp();
						for (; ; )
						{
							System.Threading.Thread.Sleep(100);

							if (Platform.Instance.IsPortLocalListening(port))
								break;

							if (process == null)
								throw new Exception("not started");

							if (process.HasExited)
								throw new Exception("already exit");

							if (Utils.UnixTimeStamp() - listeningPortStartTime > 60)
								throw new Exception("timeout");
						}

						connectResult = Connect(port);

						if (connectResult != "Ok")
							throw new Exception(connectResult);
					}
					catch (Exception ex)
					{
						throw new Exception("Unable to start (" + ex.Message + "). Look https://eddie.website/support/elevation/ to address this issue.");
					}
				}
				else
				{
					ServiceEdition = true;
				}
			}
			catch (Exception ex)
			{
				Stop();

				throw new Exception(LanguageManager.GetText(LanguageItems.HelperPrivilegesFailed, ex.Message));
			}
		}
	}
}
