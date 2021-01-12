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
using System.Globalization;
using System.Text;

namespace Eddie.Core
{
	public class DnsManager
	{
		private static Dictionary<string, DnsManagerEntry> m_cache = new Dictionary<string, DnsManagerEntry>();

		public static IpAddresses ResolveDNS(string host)
		{
			return ResolveDNS(host, false);
		}

		// Note: If cache is expired, but new query don't return IPs, old cache it's returned.
		public static IpAddresses ResolveDNS(string host, bool nocache)
		{
			if (nocache)
				return Platform.Instance.ResolveDNS(host);
			else
			{
				DnsManagerEntry entry = null;
				lock (m_cache)
				{
					if (m_cache.ContainsKey(host))
						entry = m_cache[host];
					else
						entry = new DnsManagerEntry();
				}
				Int64 now = Utils.UnixTimeStamp();
				Int64 delay = now - entry.TimeStamp;
				Int64 ttl = 3600;
				if ((Engine.Instance != null) && (Engine.Instance.Storage != null))
					ttl = Engine.Instance.Options.GetInt("dns.cache.ttl");
				if (delay >= ttl)
				{
					IpAddresses result = Platform.Instance.ResolveDNS(host);
					if (result.Count != 0)
					{
						entry.Response = result;
						entry.TimeStamp = now;
						lock (m_cache)
						{
							m_cache[host] = entry;
						}
					}
				}

				return entry.Response;
			}
		}

		public static void Invalidate()
		{
			lock (m_cache)
			{
				foreach (DnsManagerEntry entry in m_cache.Values)
					entry.TimeStamp = 0;
			}
		}

		public static void Clear()
		{
			m_cache.Clear();
		}
	}
}
