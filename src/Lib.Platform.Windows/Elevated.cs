// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2026 AirVPN (support@airvpn.org) / https://airvpn.org
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

#define EDDIE_IPC_NAMEDPIPE // hard-enabled; remove to fall back to the TCP (ISocket) transport

using Eddie.Core;
using System;
using System.Globalization;

namespace Eddie.Platform.Windows
{
#if EDDIE_IPC_NAMEDPIPE
	public class ElevatedImpl : IPipe
#else
	public class ElevatedImpl : Eddie.Core.Elevated.ISocket
#endif
	{
		public override void Start()
		{
			base.Start();

			try
			{
				string connectResult = Connect(Engine.Instance.GetElevatedServicePort(), "service");
				if (connectResult != "Ok") // Will work if the service is active
				{
					if (connectResult != "No listening")
						Engine.Instance.Logs.LogVerbose("Elevated in listening, but refuse connection: " + connectResult);

					// No persistent service listening: launch the helper directly as an
					// elevated child (UAC) in spot mode, like Linux/macOS.

					Engine.Instance.UiManager.Broadcast("init.step", "message", LanguageManager.GetText(LanguageItems.InitStepRaiseSystemPrivileges));
					Engine.Instance.Logs.LogVerbose(LanguageManager.GetText(LanguageItems.InitStepRaiseSystemPrivileges));

					int spotPort = RandomGenerator.GetInt(2048 + 128, 256 * 256 - 128);

#if EDDIE_DOTNET
					int currentId = Environment.ProcessId;
#else
					int currentId = System.Diagnostics.Process.GetCurrentProcess().Id;
#endif

					System.Diagnostics.ProcessStartInfo processStart = new System.Diagnostics.ProcessStartInfo();
					processStart.FileName = "\"" + Platform.Instance.GetElevatedHelperPath() + "\"";
					processStart.Arguments = "mode=spot spot_port=" + spotPort.ToString(CultureInfo.InvariantCulture) + " service_port=" + Engine.Instance.GetElevatedServicePort().ToString(CultureInfo.InvariantCulture) + " spot_client_pid=" + currentId.ToString(CultureInfo.InvariantCulture);
					processStart.Verb = "runas";
					processStart.CreateNoWindow = true;
					processStart.UseShellExecute = true;

					System.Diagnostics.Process process = System.Diagnostics.Process.Start(processStart);

					Int64 listeningStartTime = Utils.UnixTimeStamp();
					for (; ; )
					{
						connectResult = Connect(spotPort, "spot", 500);
						if (connectResult == "Ok")
							break;

						if ((process == null) || (process.HasExited))
							throw new Exception("Elevated spot process not running");

						if (Utils.UnixTimeStamp() - listeningStartTime > 60)
							throw new Exception("timeout");

						System.Threading.Thread.Sleep(200);
					}

					// Spot edition: ephemeral elevated child, exits with the session.
					ServiceEdition = false;
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

