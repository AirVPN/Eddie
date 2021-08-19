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
using System.Threading;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Xml;
using System.Text;
using System.Threading.Tasks;

namespace Eddie.Core
{
	public class PingManager
	{
		protected Elevated.Command m_elevated;
		protected Dictionary<int, Ping> m_elevatedPings;

		// DotNet implementation can be cleaned when Elevated implement PingEngine also in Windows platform // WIP
		// DotNet implementation don't work well on other OS (for example, Mono under macOS don't implement IPv6)
		protected bool m_dotnet = false;

		public void Init()
		{
			m_dotnet = Platform.Instance.IsWindowsSystem();

			if (m_dotnet == false)
			{
				m_elevatedPings = new Dictionary<int, Ping>();

				m_elevated = new Elevated.Command();
				m_elevated.Parameters["command"] = "ping-engine";
				m_elevated.ReceiveEvent += delegate (Elevated.Command cmd, string data)
				{
					string[] fields = data.Split(',');
					if (fields.Length == 2)
					{
						int id = Conversions.ToInt32(fields[0]);
						int result = Conversions.ToInt32(fields[1]);
						lock (m_elevatedPings)
						{
							if (m_elevatedPings.ContainsKey(id))
							{
								m_elevatedPings[id].End(result);
								m_elevatedPings.Remove(id);
							}
						}
					}
				};
				m_elevated.DoASync();
			}
		}
		public void Stop()
		{
		}

		public void Add(Ping p)
		{
			if (m_dotnet == false)
			{
				int id = Conversions.ToInt32(Engine.Instance.Elevated.DoCommandSync("ping-request", "ip", p.Ip.ToString(), "timeout", p.TimeoutMs.ToString()));
				if (id > 0)
				{
					lock (m_elevatedPings)
						m_elevatedPings[id] = p;
				}
			}
			else
			{
				DotNetPing(p);
			}
		}

		public void DotNetPing(Ping p)
		{
			// If IPv6, We use a Task<> because otherwise timeout is not honored and hang forever (occur in Win10 with IPv6 issues, Vbox in Nat)
			// Commented for now, because the issue above occur in a previous sync implementation, not yet occur in the async below.
			DotNetPingEx(p);

			/*
			if (p.Ip.IsV4)
				DotNetPing2(p);
			else
			{
				
				Task<int> pingTask = Task.Factory.StartNew(() =>
				{
					DotNetPing2(p);
					p.Complete.WaitOne();
					return p.Result;
				});

				pingTask.Wait(p.TimeoutMs);
				if (pingTask.IsCompleted == false)
					p.End(-1);
			}
			*/
		}

		public void DotNetPingEx(Ping p)
		{
			try
			{
				using (System.Net.NetworkInformation.Ping pingSender = new System.Net.NetworkInformation.Ping())
				{
					pingSender.PingCompleted += delegate (object sender, PingCompletedEventArgs e)
					{
						if (e.Cancelled)
							p.End(-1);
						else if (e.Error != null)
							p.End(-1);
						else if (e.Reply.Status == IPStatus.Success)
							p.End((int)e.Reply.RoundtripTime);
						else
							p.End(-1);
					};

					byte[] buffer = RandomGenerator.GetBuffer(32);
					PingOptions options = new PingOptions(64, true);
					IPAddress ip = IPAddress.Parse(p.Ip.ToString());
					pingSender.SendAsync(ip, p.TimeoutMs, buffer, options, p);
				}
			}
			catch
			{
				p.End(-1);
			}
		}
	}
}
