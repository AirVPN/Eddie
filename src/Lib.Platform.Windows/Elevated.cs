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
using System.Runtime.InteropServices;
using System.Net;
using System.Net.NetworkInformation;
using System.Management;
using System.Security.Principal;
using System.Xml;
using System.Text;
using System.Threading;
using Eddie.Core;
using Microsoft.Win32;
using System.IO.MemoryMappedFiles;
using System.Net.Sockets;

namespace Eddie.Platform.Windows
{
	public class Elevated : ElevatedProcessSocket
	{
		public override void Start()
		{
			ServiceEdition = false;
			ServiceUninstallAtEnd = false;

			try
			{
				if (Connect(Constants.ElevatedServiceTcpPort) == false) // Will work if the service is active
				{
					Engine.Instance.UiManager.Broadcast("init.step", "message", LanguageManager.GetText("InitStepRaiseSystemPrivileges"));
					Engine.Instance.Logs.LogVerbose(LanguageManager.GetText("InitStepRaiseSystemPrivileges"));

					string helperFullPath = Platform.Instance.GetElevatedHelperPath();

					System.Diagnostics.Process process = new System.Diagnostics.Process
					{
						StartInfo =
						{
							FileName = helperFullPath,
							Arguments = "spot",
							Verb = "runas",
							CreateNoWindow = true,
							UseShellExecute = true
						}
					};

					process.Start();

					System.Threading.Thread.Sleep(1000);

					if (process == null)
						throw new Exception("Unable to start (1)");

					if (process.HasExited)
						throw new Exception("Unable to start (2)");

					if (Connect(9345) == false)
						throw new Exception("Unable to start");
				}
				else
				{
					ServiceEdition = true;
				}

				base.Start();
			}
			catch (Exception ex)
			{ 
				Stop();

				throw new Exception(LanguageManager.GetText("HelperPrivilegesFailed", ex.Message));
			}
		}
	}
}

