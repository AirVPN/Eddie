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
using System.Net;
using System.Threading;
using System.Text;
using System.IO;
using System.Xml;
using Eddie.Common;

namespace Eddie.Core
{
	public class WebServerClient : Common.UiClient
	{
		public List<Json> Pendings = new List<Json>();

		public override Json Command(Json data)
		{
			return Engine.Instance.UiManager.SendCommand(data, this);
		}

		public override void OnReceive(Json data)
		{
			base.OnReceive(data);

			lock(Pendings)
				Pendings.Add(data);
		}
	}
}