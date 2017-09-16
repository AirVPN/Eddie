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
using System.Net;
using System.Net.NetworkInformation;
using System.Xml;
using System.Text;

namespace Eddie.Core
{
	// This class manage any OS changes made by the software, to recover them in case of bugs, crash or manual termination.

	public static class Recovery
	{
		public static object Lock = new object();

		public static string RecoveryPath()
		{
			return Engine.Instance.Storage.GetPathInData("Recovery.xml");
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
				if( (root.ChildNodes.Count == 0) && (root.Attributes.Count == 0) )
				{
					if (Platform.Instance.FileExists(path))
						Platform.Instance.FileDelete(path);
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

				// Platform.Instance.OnRecovery();

				if (Platform.Instance.FileExists(path))
				{
					try
					{
						Engine.Instance.Logs.Log(LogType.Warning, Messages.RecoveryDetected);

						XmlDocument doc = new XmlDocument();

						doc.Load(path);

						XmlElement root = doc.ChildNodes[0] as XmlElement;

						Platform.Instance.OnRecoveryLoad(root);

						Engine.Instance.NetworkLockManager.OnRecoveryLoad(root);
					}
					catch (Exception e)
					{
						Engine.Instance.Logs.Log(e);
					}

                    Platform.Instance.FileDelete(path);
				}
			}
		}
	}
}
