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
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Text;
using Eddie.Core;

namespace Eddie.Core.Threads
{
	public class PingerStats
	{
		public int Invalid = 0;
		public long OlderCheckDate = 0;
		public long LatestCheckDate = 0;
		public bool Valid = false;

		public override string ToString()
		{
			if (Engine.Instance.IsConnected())
			{
				return MessagesFormatter.Format(Messages.PingerStatsPending, Utils.FormatTime(LatestCheckDate));
			}
			else
			{
				return MessagesFormatter.Format(Messages.PingerStatsNormal, Invalid.ToString(), Utils.FormatTime(OlderCheckDate), Utils.FormatTime(LatestCheckDate));
			}
		}
	}

	public class PingerJob
	{
		//public System.Threading.Thread T;
		public ConnectionInfo Server;

		/*
		public void Start()
		{
			T = new System.Threading.Thread(new ThreadStart(this.Run));
			T.Priority = ThreadPriority.Lowest;
			T.Start();
		}
		*/

		public void Run()
		{
			RouteScope routeScope = null;
			try
			{
				routeScope = new RouteScope(Server.IpsEntry.ToStringFirstIPv4());

				Int64 result = Platform.Instance.Ping(Server.IpsEntry.ToStringFirstIPv4(), 3);
				Pinger.Instance.PingResult(Server, result);
			}
			catch (Exception)
			{
				Pinger.Instance.PingResult(Server, -1);
			}
			finally
			{
				if (routeScope != null)
				{
					routeScope.End();
				}

				lock (Pinger.Instance.Jobs)
				{
					Pinger.Instance.Jobs.Remove(this);
				}
			}

		}
	}

	public class Pinger : Eddie.Core.Thread
	{
		public static Pinger Instance;

		public List<PingerJob> Jobs = new List<PingerJob>();

		public Pinger()
		{
			Instance = this;
		}

		public override ThreadPriority GetPriority()
		{
			//return ThreadPriority.Lowest;
			return ThreadPriority.Normal;
		}

		public bool GetEnabled()
		{
			return Engine.Instance.Storage.GetBool("pinger.enabled");
		}

		public bool GetCanRun()
		{
			bool canRun = true;
			// bool alwaysRun = Engine.Instance.Storage.GetBool("pinger.always");
			bool alwaysRun = false; // 2.6

			// Logic: Can't ping when the connection is unstable. Can't ping when connected to server.
			if (Engine.IsConnected())
			{
				if (alwaysRun == false)
					canRun = false;
			}
			else if (Engine.IsWaiting() && (Engine.WaitMessage.StartsWith(Messages.WaitingLatencyTestsTitle) == false))
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
			lock (Engine.Connections)
			{
				foreach (ConnectionInfo infoServer in Engine.Connections.Values)
					infoServer.InvalidatePingResults();
			}
		}

		public override void OnRun()
		{
			Dictionary<string, ConnectionInfo> servers;

			for (;;)
			{
				if (GetCanRun() == false)
				{
					Sleep(1000);
				}
				else
				{
					// Note: If Pinger is not enabled, works like all ping results is 0.						
					bool enabled = GetEnabled();

					int timeNow = Utils.UnixTimeStamp();
					int jobsLimit = Engine.Instance.Storage.GetInt("pinger.jobs");

					lock (Engine.Connections)
						servers = new Dictionary<string, ConnectionInfo>(Engine.Connections);

					bool startOne = false;

					foreach (ConnectionInfo infoServer in servers.Values)
					{
						if (GetCanRun() == false)
							break;

						int delaySuccess = GetPingerDelaySuccess(infoServer);
						int delayRetry = GetPingerDelayRetry(infoServer);

						int delay = delaySuccess;
						if (infoServer.PingFailedConsecutive > 0)
							delay = delayRetry;

						if (timeNow - infoServer.LastPingTest >= delay)
						{
							bool canPingServer = enabled;
							if (infoServer.CanPing() == false)
								canPingServer = false;

							if (canPingServer)
							{
								if (Jobs.Count < jobsLimit)
								{
									infoServer.LastPingTest = timeNow;

									PingerJob job = new PingerJob();
									job.Server = infoServer;
									
									ThreadPool.QueueUserWorkItem(new WaitCallback(DoPing), job);
									startOne = true;
								}
							}
							else
							{
								if (infoServer.Ping != -1)
								{
									//infoServer.LastPingTest = timeNow; // <2.13.4
									infoServer.LastPingTest = 0;
									infoServer.PingTests = 0;
									infoServer.PingFailedConsecutive = 0;
									infoServer.Ping = -1;
									infoServer.LastPingResult = infoServer.LastPingTest;
									infoServer.LastPingSuccess = infoServer.LastPingTest;
									Engine.Instance.MarkServersListUpdated();
								}
							}
						}

						if (CancelRequested)
							return;
					}

					if (startOne)
						Sleep(100); // Waiting for a queue slot
					else
						Sleep(1000); // Waiting for a ping need
				}

				if (CancelRequested)
					return;


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

			lock (Engine.Connections)
			{
				foreach (ConnectionInfo infoConnection in Engine.Connections.Values)
				{
					int deltaValid = GetPingerDelayValid(infoConnection);

					if ((stats.OlderCheckDate == 0) || (stats.OlderCheckDate > infoConnection.LastPingResult))
						stats.OlderCheckDate = infoConnection.LastPingResult;

					if ((stats.LatestCheckDate == 0) || (stats.LatestCheckDate < infoConnection.LastPingResult))
						stats.LatestCheckDate = infoConnection.LastPingResult;

					iTotal++;
					if (timeNow - infoConnection.LastPingResult > deltaValid)
						stats.Invalid++;
				}
			}

			stats.Valid = (stats.Invalid == 0);

			return stats;
		}

		public bool GetValid()
		{
			return GetStats().Valid;
		}
	}
}
