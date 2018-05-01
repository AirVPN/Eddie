// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2018 AirVPN (support@airvpn.org) / https://airvpn.org
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

using Android.Net;
using Android.OS;
using Eddie.Common.Log;
using System;
using System.Collections.Generic;

namespace Eddie.Droid
{
	public class AndroidVPNContext : IDisposable
	{
		private VpnService.Builder m_builder = null;		
		private ParcelFileDescriptor m_fileDescriptor = null;

		private bool m_blockIPV4 = false;
		private bool m_blockIPV6 = false;
		private bool m_forceDNS = false;
		private bool m_hasDNS = false;
		private bool m_forceMTU = false;

		public AndroidVPNContext(AndroidVPNService service)
		{
			m_builder = new VpnService.Builder(service);

			m_builder.SetConfigureIntent(service.CreateConfigIntent());

			InitDNS();
			InitMTU();
			InitApplications();
		}

		~AndroidVPNContext()
		{
			Dispose(false);
		}

		public VpnService.Builder Builder
		{
			get
			{
				return m_builder;
			}
		}

		private void InitDNS()
		{
			if(!OptionsManager.SystemDNSOverrideEnable)
				return;

			List<string> dnsForced = OptionsManager.SystemDNSForcedList;
			if(dnsForced.Count == 0)
				return;

			m_forceDNS = true;

			foreach(string dns in dnsForced)
			{
				DoAddDNS(dns);
			}
		}

		private void InitMTU()
		{
			String mtuForced = OptionsManager.SystemMTUForced.Trim();
			if(Utils.Empty(mtuForced))
				return;

			try
			{
				int mtu = int.Parse(mtuForced);
				if(mtu > 0)
				{
					m_builder.SetMtu(mtu);
					m_forceMTU = true;
				}
			}
			catch(Exception e)
			{
				throw new Exception(string.Format("invalid mtu option '{0}': '{1}'", mtuForced, e.Message));
			}
		}

		private void InitApplications()
		{
			List<string> applicationsList = OptionsManager.SystemApplicationsFilterList;
			if(applicationsList.Count == 0)
				return;

			string filterType = OptionsManager.SystemApplicationsFilterType;
			if(filterType == OptionsManager.SYSTEM_OPTION_APPLICATIONS_FILTER_TYPE_NONE)
				return;

			if(filterType == OptionsManager.SYSTEM_OPTION_APPLICATIONS_FILTER_TYPE_WHITELIST)
			{
				// Only the specified applications will be inside the tunnel

				foreach(string app in applicationsList)
				{
					LogsManager.Instance.Debug(string.Format("Application '{0}' will be added to allowed list (inside tunnel)", app));
					m_builder.AddAllowedApplication(app);
				}					
			}
			else if(filterType == OptionsManager.SYSTEM_OPTION_APPLICATIONS_FILTER_TYPE_BLACKLIST)
			{
				// The specified applications will be outside the tunnel

				foreach(string app in applicationsList)
				{
					LogsManager.Instance.Debug(string.Format("Application '{0}' will be added to disallowed list (outside tunnel)", app));
					m_builder.AddDisallowedApplication(app);
				}					
			}
			else
			{
				throw new Exception(string.Format("unknown application's filter type '{0}'", filterType));
			}
		}
		
		public ParcelFileDescriptor Establish()
		{
			EnsureRoutes();
			EnsureDNS();

			if(m_builder == null)
				throw new Exception("internal error (m_builder is null)");

			if(m_fileDescriptor != null)
				throw new Exception("internal error (m_fileDescriptor already initialized)");

			m_fileDescriptor = m_builder.Establish();
			return m_fileDescriptor;
		}

		public void Dispose()
		{
			Dispose(true);
		}

		private void Dispose(bool disposing)
		{
			if(disposing)
				GC.SuppressFinalize(this);

			Cleanup();
		}

		private void Cleanup()
		{
			Utils.SafeClose(m_fileDescriptor);
			m_fileDescriptor = null;

			Utils.SafeDispose(m_builder);
			m_builder = null;
		}

		private void EnsureDNS()
		{
			if(!OptionsManager.SystemDNSOverrideEnable)
				return;

			if(m_hasDNS)
				return;     // At least one DNS has been added, do not check for alternative

			List<string> dnsAlternate = OptionsManager.SystemDNSAlternativeList;
			foreach(string dns in dnsAlternate)
			{
				DoAddDNS(dns);
			}
		}

		private void EnsureRoutes()
		{
			if(m_blockIPV4 == false)
			{
				m_builder.AllowFamily(global::Android.Systems.OsConstants.AfInet);
				// Routes all IPV4 traffic inside the tunnel
				m_builder.AddRoute("0.0.0.0", 0);
			}

			if(m_blockIPV6 == false)
			{
				m_builder.AllowFamily(global::Android.Systems.OsConstants.AfInet6);
				// Routes all IPV6 traffic inside the tunnel
				m_builder.AddRoute("::", 0);
			}
		}

		private void DoAddDNS(string address)
		{
			address = address.Trim();
			if(Utils.Empty(address))
				throw new Exception("invalid DNS server");

			LogsManager.Instance.Debug("Adding DNS server '{0}'", address);

			m_builder.AddDnsServer(address);
			m_hasDNS = true;
		}

		public void AddDNSServer(string address, int ipv6)
		{
			if(m_forceDNS)
				LogsManager.Instance.Debug("DNS forced (address '{0}' will be skipped)", address);
			else
				DoAddDNS(address);			
		}

		public void SetBlockIPV6(bool block)
		{
			m_blockIPV6 = block;			
		}

		public void SetMTU(int mtu)
		{
			if(m_forceMTU)
				LogsManager.Instance.Debug("MTU forced (value '{0}' will be skipped)", mtu);
			else
				m_builder.SetMtu(mtu);
		}
	}
}
