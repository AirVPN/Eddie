// <airvpn_source_header>
// This file is part of AirVPN Client software.
// Copyright (C)2014-2014 AirVPN (support@airvpn.org) / https://airvpn.org )
//
// AirVPN Client is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// AirVPN Client is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with AirVPN Client. If not, see <http://www.gnu.org/licenses/>.
// </airvpn_source_header>

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Xml;
using System.Text;

namespace AirVPN.Core
{
	// This class manage any OS changes made by the software, to recover them in case of bugs, crash or manual termination.

	public static class Recovery
	{
		public static object Lock = new object();

		public static string RecoveryPath()
		{
			return Engine.Instance.Storage.GetPath("Recovery.xml");
		}

		public static void Save()
		{
			lock (Lock)
			{
				XmlDocument doc = new XmlDocument();
				XmlElement root = (XmlElement)doc.AppendChild(doc.CreateElement("Recovery"));

				Platform.Instance.OnRecoverySave(root);
				Engine.Instance.NetworkLockManager.OnRecoverySave(root);

				string path = RecoveryPath();
				if (root.ChildNodes.Count == 0)
				{
					if (File.Exists(path))
						File.Delete(path);
				}
				else
					doc.Save(RecoveryPath());
			}
		}

		public static void Load()
		{
			lock (Lock)
			{
				string path = RecoveryPath();

				Platform.Instance.OnRecovery();

				if (File.Exists(path))
				{
					try
					{
						Engine.Instance.Log(Engine.LogType.Warning, Messages.RecoveryDetected);

						XmlDocument doc = new XmlDocument();

						doc.Load(path);

						XmlElement root = doc.ChildNodes[0] as XmlElement;

						Platform.Instance.OnRecoveryLoad(root);

						Engine.Instance.NetworkLockManager.OnRecoveryLoad(root);
					}
					catch (Exception e)
					{
						Engine.Instance.Log(e);
					}

					File.Delete(path);
				}
			}
		}
	}
}
