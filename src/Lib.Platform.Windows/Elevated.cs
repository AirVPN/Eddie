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

using Eddie.Core;
using System;

namespace Eddie.Platform.Windows
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
					// Here, in spot mode, we install a service that will be uninstalled at exit.
					// The old method above (really spot), still used in other OS, dont work in Windows because WinTun require SYSTEM privileges.

					Engine.Instance.UiManager.Broadcast("init.step", "message", LanguageManager.GetText("InitStepRaiseSystemPrivileges"));
					Engine.Instance.Logs.LogVerbose(LanguageManager.GetText("InitStepRaiseSystemPrivileges"));

					string helperFullPath = Platform.Instance.GetElevatedHelperPath();

					bool serviceAlreadyPresentButFail = Platform.Instance.GetService(true);

					if (Platform.Instance.SetService(true, true) == false)
						throw new Exception("Unable to start (unable to initialize service)");

					Int64 listeningPortStartTime = Utils.UnixTimeStamp();
					for (; ; )
					{
						if (Platform.Instance.IsPortLocalListening(Engine.Instance.GetElevatedServicePort()))
							break;

						if (Utils.UnixTimeStamp() - listeningPortStartTime > 20)
							throw new Exception("Unable to start (timeout)");

						System.Threading.Thread.Sleep(100);
					}
					connectResult = Connect(Engine.Instance.GetElevatedServicePort());
					if (connectResult != "Ok")
						throw new Exception("Unable to start (" + connectResult + ")");

					ServiceEdition = true;

					if (serviceAlreadyPresentButFail == false)
					{
						ServiceUninstallAtEnd = true;
						this.DoCommandSync("service-conn-mode", "mode", "single");
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

				throw new Exception(LanguageManager.GetText("HelperPrivilegesFailed", ex.Message));
			}
		}
	}
}

