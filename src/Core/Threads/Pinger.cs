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
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Text;
using AirVPN.Core;

namespace AirVPN.Core.Threads
{
	public class PingerJob
	{
		//public System.Threading.Thread T;
		public ServerInfo Server;

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
				bool alwaysRun = Engine.Instance.Storage.GetBool("advanced.pinger.always");
								
				routeScope = new RouteScope(Server.IpEntry, alwaysRun);

				// Ping
				Ping pingSender = new Ping();
				PingOptions options = new PingOptions();

				// Use the default Ttl value which is 128,
				// but change the fragmentation behavior.
				options.DontFragment = true;

				// Create a buffer of 32 bytes of data to be transmitted.								
				byte[] buffer = RandomGenerator.GetBuffer(32);
				int timeout = 5000;
				PingReply reply = pingSender.Send(Server.IpEntry, timeout, buffer, options);
				Pinger.Instance.PingResult(Server, reply);
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

	public class Pinger : AirVPN.Core.Thread
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
			return Engine.Instance.Storage.GetBool("advanced.pinger.enabled");
		}

		public bool GetCanRun()
		{			
			bool canRun = true;
			bool alwaysRun = Engine.Instance.Storage.GetBool("advanced.pinger.always");

			// Logic: Can't ping when the connection is unstable. Can't ping when connected to server.
			if (Engine.IsConnected())
			{
				if (alwaysRun == false)
					canRun = false;
			}
			else if (Engine.IsWaiting() && (Engine.WaitMessage != Messages.WaitingLatencyTests))
				canRun = false;

			return canRun;
		}

        public override void OnRun()
        {            
            Dictionary<string, ServerInfo> servers;

            for (; ; )
            {
				if (GetCanRun() == false)
                {
                    Sleep(1000);
                }
                else
                {
					// Note: If Pinger is not enabled, works like all ping results is 0.						
					bool enabled = GetEnabled();
					//bool alwaysRun = Engine.Instance.Storage.GetBool("advanced.pinger.always");
					int delay = Engine.Instance.Storage.GetInt("advanced.pinger.delay");
					if (delay == 0)
						delay = Conversions.ToInt32(Engine.Instance.Storage.GetManifestKeyValue("pinger_delay", "10"));
					int timeNow = Utils.UnixTimeStamp();
					int jobsLimit = Engine.Instance.Storage.GetInt("advanced.pinger.jobs");

					lock (Engine.Servers)
                        servers = new Dictionary<string, ServerInfo>(Engine.Servers);
					
					bool startOne = false;

                    foreach (ServerInfo infoServer in servers.Values)
                    {
						if (GetCanRun() == false)
							break;

						if (timeNow - infoServer.LastPingTest >= delay)
						{
							if (enabled)
							{
								if (Jobs.Count < jobsLimit)
								{
									infoServer.LastPingTest = timeNow;
									
									PingerJob job = new PingerJob();
									job.Server = infoServer;
									/*
									lock (Jobs)
									{
										Jobs.Add(job);										
									}
									job.Start();
									*/

									ThreadPool.QueueUserWorkItem(new WaitCallback(DoPing), job);
									startOne = true;
								}
							}
							else
							{
								infoServer.LastPingTest = timeNow;
								infoServer.PingTests = 0;
								infoServer.PingFailedConsecutive = 0;
								infoServer.Ping = 0;
								infoServer.LastPingResult = infoServer.LastPingTest;
								infoServer.LastPingSuccess = infoServer.LastPingTest;
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

		public void PingResult(ServerInfo infoServer, PingReply reply)
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

		public void PingResult(ServerInfo infoServer, Int64 result)
		{
			/*
			if(result == -1)
				Console.WriteLine("Ping " + infoServer.IpEntry + " failed");
			else
				Console.WriteLine("Ping " + infoServer.IpEntry + " done, " + result.ToString() + " ms");
			*/
				
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
		}

		void OnPingCompleted(object sender, PingCompletedEventArgs e)
		{
			ServerInfo infoServer = e.UserState as ServerInfo;

			PingResult(infoServer, e.Reply);
		}

		public bool GetValid()
		{
			int deltaValid = Engine.Instance.Storage.GetInt("advanced.pinger.valid");
			int timeNow = Utils.UnixTimeStamp();

			int iTotal = 0;
			int iInvalid = 0;

			lock (Engine.Servers)
			{
				foreach (ServerInfo infoServer in Engine.Servers.Values)
				{
					iTotal++;
					if (timeNow - infoServer.LastPingResult > deltaValid)
						iInvalid++;
				}
			}

			//Console.WriteLine("Ping Total:" + iTotal.ToString() + ", Invalid:" + iInvalid.ToString());

			bool valid = (iInvalid == 0);

			return valid;
		}
    }
}
