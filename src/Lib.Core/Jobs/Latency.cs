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
using System.Net.NetworkInformation;
using System.Threading;
using System.Text;
using Eddie.Core;

namespace Eddie.Core.Jobs
{
	public class Latency : Eddie.Core.Job
	{
		public List<PingerJob> Jobs = new List<PingerJob>();

		public override ThreadPriority GetPriority()
		{
			return ThreadPriority.Normal;
		}

		public override bool GetSync()
		{
			return false;
		}

		public override void OnRun()
		{
			if (GetCanRun() == false)
			{
				m_timeEvery = 1000;
			}
			else
			{
				List<ConnectionInfo> connections = GetConnectionsToPing();

				// Note: If Pinger is not enabled, works like all ping results is 0.						
				bool enabled = GetEnabled();

				int timeNow = Utils.UnixTimeStamp();
				int jobsLimit = Engine.Instance.Storage.GetInt("pinger.jobs");
								
				bool startOne = false;

				foreach (ConnectionInfo connectionInfo in connections)
				{
					if (GetCanRun() == false)
						break;

					int delaySuccess = GetPingerDelaySuccess(connectionInfo);
					int delayRetry = GetPingerDelayRetry(connectionInfo);

					int delay = delaySuccess;
					if (connectionInfo.PingFailedConsecutive > 0)
						delay = delayRetry;

					if (timeNow - connectionInfo.LastPingTest >= delay)
					{
						bool canPingServer = enabled;
						if (connectionInfo.CanPing() == false)
							canPingServer = false;

						if (canPingServer)
						{
							if (Jobs.Count < jobsLimit)
							{
								connectionInfo.LastPingTest = timeNow;

								PingerJob job = new PingerJob();
								job.Server = connectionInfo;
								lock (Jobs)
									Jobs.Add(job);

								ThreadPool.QueueUserWorkItem(new WaitCallback(DoPing), job);
								startOne = true;
							}
						}
						else
						{
							if (connectionInfo.Ping != -1)
							{
								//connectionInfo.LastPingTest = timeNow; // <2.13.4
								connectionInfo.LastPingTest = 0;
								connectionInfo.PingTests = 0;
								connectionInfo.PingFailedConsecutive = 0;
								connectionInfo.Ping = -1;
								connectionInfo.LastPingResult = connectionInfo.LastPingTest;
								connectionInfo.LastPingSuccess = connectionInfo.LastPingTest;
								Engine.Instance.MarkServersListUpdated();
							}
						}
					}

					if (m_cancelRequested)
						return;
				}

				if (startOne)
					m_timeEvery = 100;
				else
					m_timeEvery = 1000;
										
				for(; ;)
				{
					lock (Jobs)
						if (Jobs.Count == 0)
							break;
					Sleep(100);
				}
			}
		}

		public bool GetEnabled()
		{
			return Engine.Instance.Storage.GetBool("pinger.enabled");
		}

		public bool GetCanRun()
		{
			bool canRun = true;

			// Logic: Can't ping when the connection is unstable. Can't ping when connected to server.
			if (Engine.Instance.IsConnected())
			{
				canRun = false;
			}
			else if (Engine.Instance.IsWaiting() && (Engine.Instance.WaitMessage.StartsWithInv(LanguageManager.GetText("WaitingLatencyTestsTitle")) == false))
				canRun = false;

			return canRun;
		}

		public int GetPingerDelaySuccess(ConnectionInfo server) // Delay for already success ping
		{
			int delay = Engine.Instance.Storage.GetInt("pinger.delay");
			if (delay == 0)
				delay = Conversions.ToInt32(server.Provider.GetKeyValue("pinger_delay", "180"));
			return delay;
		}

		public int GetPingerDelayRetry(ConnectionInfo server) // Delay for failed ping
		{
			int delay = Engine.Instance.Storage.GetInt("pinger.retry");
			if (delay == 0)
				delay = Conversions.ToInt32(server.Provider.GetKeyValue("pinger_retry", "5"));
			return delay;
		}

		public int GetPingerDelayValid(ConnectionInfo server) // Delay for consider valid
		{
			int delay = Engine.Instance.Storage.GetInt("pinger.valid");
			if (delay == 0)
				delay = GetPingerDelaySuccess(server) * 5;
			return delay;
		}

		public void InvalidateAll()
		{
			lock (Engine.Instance.Connections)
			{
				foreach (ConnectionInfo infoServer in Engine.Instance.Connections.Values)
					infoServer.InvalidatePingResults();
			}
		}

		private static void DoPing(object o)
		{
			PingerJob job = o as PingerJob;
			job.Run();
		}

		public void PingResult(ConnectionInfo infoServer, PingReply reply)
		{
			Int64 result = 0;

			if (reply.Status == IPStatus.Success)
			{
				result = reply.RoundtripTime;
			}
			else
			{
				result = -1;
			}

			PingResult(infoServer, result);
		}

		public void PingResult(ConnectionInfo infoServer, Int64 result)
		{
			infoServer.PingTests++;
			infoServer.LastPingResult = Utils.UnixTimeStamp();
			if (result == -1)
			{
				infoServer.PingFailedConsecutive++;
				if (infoServer.PingFailedConsecutive >= 5)
					infoServer.Ping = -1;
			}
			else
			{
				infoServer.LastPingSuccess = infoServer.LastPingResult;
				infoServer.PingFailedConsecutive = 0;
				if (infoServer.Ping == -1)
					infoServer.Ping = result;
				else
					infoServer.Ping = (infoServer.Ping + result) / 2;
			}
			Engine.Instance.MarkServersListUpdated();
		}

		void OnPingCompleted(object sender, PingCompletedEventArgs e)
		{
			ConnectionInfo infoServer = e.UserState as ConnectionInfo;

			PingResult(infoServer, e.Reply);
		}

		public PingerStats GetStats()
		{
			PingerStats stats = new PingerStats();

			int timeNow = Utils.UnixTimeStamp();

			int iTotal = 0;

			List<ConnectionInfo> connections = GetConnectionsToPing();

			foreach (ConnectionInfo infoConnection in connections)
			{
				int deltaValid = GetPingerDelayValid(infoConnection);

				if ((stats.OlderCheckDate == 0) || (stats.OlderCheckDate > infoConnection.LastPingResult))
					stats.OlderCheckDate = infoConnection.LastPingResult;

				if ((stats.LatestCheckDate == 0) || (stats.LatestCheckDate < infoConnection.LastPingResult))
					stats.LatestCheckDate = infoConnection.LastPingResult;

				iTotal++;
				if ((infoConnection.CanPing()) && (timeNow - infoConnection.LastPingResult > deltaValid))
					stats.Invalid++;
			}
			
			stats.Valid = (stats.Invalid == 0);

			return stats;
		}

		public bool GetValid()
		{
			return GetStats().Valid;
		}

		public List<ConnectionInfo> GetConnectionsToPing()
		{
			// Old: Engine.Instance.Connections.Values
			List<ConnectionInfo> connections = Engine.Instance.GetConnections(false);
			return connections;
		}
	}
}
