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
				string connectResult = Connect("service");

				if (connectResult != "Ok") // Will work if the service is active
				{
					if (connectResult != "No listening")
						Engine.Instance.Logs.LogVerbose("Elevated in listening, but refuse connection: " + connectResult);

					Engine.Instance.UiManager.Broadcast("init.step", "message", LanguageManager.GetText(LanguageItems.InitStepRaiseSystemPrivileges));
					Engine.Instance.Logs.LogVerbose(LanguageManager.GetText(LanguageItems.InitStepRaiseSystemPrivileges));

#if EDDIE_DOTNET
					int currentId = Environment.ProcessId;
#else
					int currentId = System.Diagnostics.Process.GetCurrentProcess().Id;
#endif

					int pid = (Platform.Instance as Eddie.Platform.Linux.Platform).RunElevated(new string[] { "mode=spot", "spot_client_pid=" + currentId.ToString(CultureInfo.InvariantCulture) }, false);

					System.Diagnostics.Process process = null;
					if (pid > 0)
						process = System.Diagnostics.Process.GetProcessById(pid);

					try
					{
						long listeningPortStartTime = Utils.UnixTimeStamp();
						for (; ; )
						{
							System.Threading.Thread.Sleep(100);

							if (System.IO.File.Exists(GetUnixSockPath("spot")))
								break;

							if (process == null)
								throw new Exception("not started");

							if (process.HasExited)
								throw new Exception("already exit" + GetExitCodeDescription(process));

							if (Utils.UnixTimeStamp() - listeningPortStartTime > 60)
								throw new Exception("timeout");
						}

						connectResult = Connect("spot");

						if (connectResult != "Ok")
							throw new Exception(connectResult);
					}
					catch (Exception ex)
					{
						LogHelperReport(process);

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

		// The exit code is not guaranteed to be available for a process that was not started by
		// the Process object itself: return an empty description instead of failing.
		private static string GetExitCodeDescription(System.Diagnostics.Process process)
		{
			try
			{
				return ", exit code " + process.ExitCode.ToString(CultureInfo.InvariantCulture);
			}
			catch (Exception)
			{
				return "";
			}
		}

		// A helper that dies before listening leaves no other trace, so report its dynamic linking
		// status: it covers the case of a missing bundled library. Skipped when the helper is still
		// alive (the failure is then in the handshake, not in the startup). Diagnostics must never
		// replace the original failure, so any error here is ignored.
		private static void LogHelperReport(System.Diagnostics.Process process)
		{
			try
			{
				if ((process != null) && (process.HasExited == false))
					return;

				Engine.Instance.Logs.LogVerbose("Elevated helper report: " + Platform.Instance.GetExecutableReport(Platform.Instance.GetElevatedHelperPath()));
			}
			catch (Exception)
			{
			}
		}
	}
}
