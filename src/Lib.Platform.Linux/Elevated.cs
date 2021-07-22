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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using Eddie.Core;

namespace Eddie.Platform.Linux
{
	public class ElevatedImpl : Eddie.Core.Elevated.ISocket
	{
		public override void Start()
		{
			base.Start();

			string tempPathToDelete = "";

			try
			{
				string connectResult = Connect(Engine.Instance.GetElevatedServicePort());
				if (connectResult != "Ok") // Will work if the service is active
				{
					Engine.Instance.UiManager.Broadcast("init.step", "message", LanguageManager.GetText("InitStepRaiseSystemPrivileges"));
					Engine.Instance.Logs.LogVerbose(LanguageManager.GetText("InitStepRaiseSystemPrivileges"));

					string helperPath = Platform.Instance.GetElevatedHelperPath();

					// Special environment: AppImage  
					// pkexec/sudo can't see the file. Workaround: Copy, execute, remove.
					bool appImageEnvironment = Platform.Instance.NeedExecuteOutsideAppPath(helperPath);
					if (appImageEnvironment)
					{
						tempPathToDelete = Platform.Instance.FileTempName("eddie-cli-elevated");

						if (File.Exists(tempPathToDelete))
							File.Delete(tempPathToDelete);
						File.Copy(helperPath, tempPathToDelete);

						helperPath = tempPathToDelete;
					}

					int port = GetPortSpot();

					int pid = Platform.Instance.StartProcessAsRoot(helperPath, new string[] { "mode=spot", "spot_port=" + port.ToString(), "service_port=" + Engine.Instance.GetElevatedServicePort().ToString() }, Engine.Instance.ConsoleMode);
					System.Diagnostics.Process process = null;
					if (pid > 0)
						process = System.Diagnostics.Process.GetProcessById(pid);

					long listeningPortStartTime = Utils.UnixTimeStamp();
					for (; ; )
					{
						if (Platform.Instance.IsPortLocalListening(port))
							break;

						if (process == null)
							throw new Exception("Unable to start (null)");

						if (process.HasExited)
							throw new Exception("Unable to start (already exit)");

						if (Utils.UnixTimeStamp() - listeningPortStartTime > 60)
							throw new Exception("Unable to start (timeout)");

						System.Threading.Thread.Sleep(100);
					}

					connectResult = Connect(port);
					if (connectResult != "Ok")
						throw new Exception("Unable to start (" + connectResult + ")");
				}
				else
				{
					ServiceEdition = true;
				}
			}
			catch (Exception ex)
			{
				Stop();

				throw new Exception(LanguageManager.GetText("HelperPrivilegesFailed", ex.Message));
			}
			finally
			{
				if (tempPathToDelete != "")
				{
					Platform.Instance.FileDelete(tempPathToDelete);
				}
			}
		}
	}
}
