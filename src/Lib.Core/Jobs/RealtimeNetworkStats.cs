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
using System.Threading;

namespace Eddie.Core.Jobs
{
	public class RealtimeNetworkStats : Eddie.Core.Job
	{
		public Json m_data = new Json();

		public override ThreadPriority GetPriority()
		{
			return ThreadPriority.Lowest;
		}

		public override void OnRun()
		{
			// Fetch
			Int64 ts = Utils.UnixTimeStamp();

			Json realtimeData = Platform.Instance.GetRealtimeNetworkStats();

			// Mark dead			
			m_data.EnsureDictionary();
			foreach (KeyValuePair<string, object> kp in m_data.GetDictionary())
			{
				m_data[kp.Key]["live"].Value = false;
			}

			// Update
			lock (m_data)
			{
				foreach (Json jDataInterface in realtimeData.GetArray())
				{
					string id = jDataInterface["id"].Value as string;
					Int64 totalBytesRcv = jDataInterface["rcv"].ValueInt64;
					Int64 totalBytesSnd = jDataInterface["snd"].ValueInt64;
					Int64 delta = 0;
					Int64 deltaBytesRcv = 0;
					Int64 deltaBytesSnd = 0;
					if (m_data.HasKey(id) == false)
					{
						m_data[id].Value = new Json();
						m_data[id]["ts"].Value = 0;
					}
					else
					{
						delta = ts - m_data[id]["ts"].ValueInt64;
						if (delta != 0)
						{
							deltaBytesRcv = (totalBytesRcv - m_data[id]["total_rcv"].ValueInt64) / delta;
							deltaBytesSnd = (totalBytesSnd - m_data[id]["total_snd"].ValueInt64) / delta;
						}

						if ((Engine.Instance.Connection != null) && (Engine.Instance.Connection.Interface != null) && (id == Engine.Instance.Connection.Interface.Id) && (Engine.Instance.IsConnected())) // Old UI
						{
							Engine.Instance.Session.StatsRead += deltaBytesRcv;
							Engine.Instance.Session.StatsWrite += deltaBytesSnd;
							Engine.Instance.Connection.BytesLastDownloadStep = deltaBytesRcv;
							Engine.Instance.Connection.BytesLastUploadStep = deltaBytesSnd;
							Engine.Instance.Connection.BytesRead += deltaBytesRcv;
							Engine.Instance.Connection.BytesWrite += deltaBytesSnd;

							Engine.Instance.Stats.Charts.Hit(deltaBytesRcv, deltaBytesSnd);

							Engine.Instance.OnRefreshUi(Core.Engine.RefreshUiMode.Stats);

							Engine.Instance.StatusRaise();
						}
					}

					m_data[id]["live"].Value = true;
					m_data[id]["total_rcv"].Value = totalBytesRcv;
					m_data[id]["total_snd"].Value = totalBytesSnd;
					m_data[id]["ts"].Value = ts;
				}
			}

			foreach (KeyValuePair<string, object> kp in m_data.GetDictionary())
			{
				if (m_data[kp.Key]["live"].ValueBool == false)
				{
					m_data.RemoveKey(kp.Key);
					break; // One at time, to bypass collection modification error.
				}
			}

			m_timeEvery = 1000;
		}

		public void ResetTotal(string interfaceId)
		{

		}

		public Json GetData()
		{
			lock (m_data)
			{
				return m_data.Clone();
			}
		}
	}
}
