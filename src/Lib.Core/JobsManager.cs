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
using System.Drawing;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Xml;
using System.Text;

namespace Eddie.Core
{
	public class JobsManager
	{
		public List<Job> Jobs = new List<Job>();

		public Jobs.Updater Updater;
		public Jobs.Penalities Penalities;
		public Jobs.Latency Latency;
		public Jobs.Discover Discover;
		public Jobs.ProvidersRefresh ProvidersRefresh;

		public JobsManager()
		{
			Jobs.Add(Updater = new Jobs.Updater());
			Jobs.Add(Penalities = new Jobs.Penalities());
			Jobs.Add(Latency = new Jobs.Latency());
			Jobs.Add(Discover = new Jobs.Discover());			
			Jobs.Add(ProvidersRefresh = new Jobs.ProvidersRefresh());
		}

		public void Check()
		{
			foreach (Job j in Jobs)
				j.CheckRun();
		}

		public void RequestStopSync()
		{
			foreach (Job j in Jobs)
			{
				j.CancelRequested();
				j.WaitEnd();
			}
		}
	}
}
