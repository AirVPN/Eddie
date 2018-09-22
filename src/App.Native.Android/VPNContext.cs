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
//
// 20 June 2018 - author: promind - initial release. Based on revised code from com.eddie.android. (a tribute to the 1859 Perugia uprising occurred on 20 June 1859 and in memory of those brave inhabitants who fought for the liberty of Perugia)

using Android.OS;
using System;
using System.Collections.Generic;

namespace Eddie.NativeAndroidApp
{
	public class VPNContext : IDisposable
	{
		private VPNService.Builder vpnServiceBuilder = null;		
		private ParcelFileDescriptor fileDescriptor = null;
        private SettingsManager settingsManager = new SettingsManager();

		private bool blockIPV4 = false;
		private bool blockIPV6 = false;
		private bool customDNS = false;
		private bool hasDNS = false;
		private bool forceMTU = false;

		public VPNContext(VPNService service)
		{
			vpnServiceBuilder = new VPNService.Builder(service);

			vpnServiceBuilder.SetConfigureIntent(service.CreateConfigIntent());

            EddieLogger.Init(service);

			InitDNS();
			
            InitMTU();
			
            InitApplications();
		}

		~VPNContext()
		{
			Dispose(false);
		}

		public VPNService.Builder Builder
		{
			get
			{
				return vpnServiceBuilder;
			}
		}

		private void InitDNS()
		{
			if(!settingsManager.SystemDNSOverrideEnable)
				return;

            if(settingsManager.SystemDNSOverrideEnable)
            {
    			List<string> dnsCustom = settingsManager.SystemDNSCustomList;
    
                if(dnsCustom.Count == 0)
    				return;
    
    			customDNS = true;
    
    			foreach(string dns in dnsCustom)
    			{
    				DoAddDNS(dns);
    			}
            }
            else
                customDNS = false;
		}

		private void InitMTU()
		{
			String customMtu = settingsManager.SystemCustomMTU.Trim();
			
            if(SupportTools.Empty(customMtu))
				return;

			try
			{
				int mtu = int.Parse(customMtu);
				
                if(mtu > 0)
				{
					vpnServiceBuilder.SetMtu(mtu);
					forceMTU = true;
				}
			}
			catch(Exception e)
			{
				throw new Exception(string.Format("invalid mtu option '{0}': '{1}'", customMtu, e.Message));
			}
		}

		private void InitApplications()
		{
			List<string> applicationsList = settingsManager.SystemApplicationFilterList;
			
            if(applicationsList.Count == 0)
				return;

			string filterType = settingsManager.SystemApplicationFilterType;
			
            if(filterType == SettingsManager.SYSTEM_OPTION_APPLICATION_FILTER_TYPE_NONE)
				return;

			if(filterType == SettingsManager.SYSTEM_OPTION_APPLICATION_FILTER_TYPE_WHITELIST)
			{
				// Only the specified applications will be inside the tunnel

				foreach(string app in applicationsList)
				{
					EddieLogger.Debug(string.Format("Adding '{0}' to whitelisted applications. Traffic and data will be encapsuleted inside the tunnel.", app));
					
                    vpnServiceBuilder.AddAllowedApplication(app);
				}					
			}
			else if(filterType == SettingsManager.SYSTEM_OPTION_APPLICATION_FILTER_TYPE_BLACKLIST)
			{
				// The specified applications will be outside the tunnel

				foreach(string app in applicationsList)
				{
					EddieLogger.Debug(string.Format("Adding '{0}' to blacklisted applications. Traffic and data will be outside of the tunnel control.", app));
					
                    vpnServiceBuilder.AddDisallowedApplication(app);
				}					
			}
			else
			{
				throw new Exception(string.Format("Unknown application's filter type '{0}'", filterType));
			}
		}
		
		public ParcelFileDescriptor Establish()
		{
			EnsureRoutes();
			
            EnsureDNS();

			if(vpnServiceBuilder == null)
				throw new Exception("Internal error (vpnServiceBuilder is null)");

			if(fileDescriptor != null)
				throw new Exception("Internal error (fileDescriptor already initialized)");

			fileDescriptor = vpnServiceBuilder.Establish();

            return fileDescriptor;
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
			SupportTools.SafeClose(fileDescriptor);
			fileDescriptor = null;

			SupportTools.SafeDispose(vpnServiceBuilder);
			vpnServiceBuilder = null;
		}

		private void EnsureDNS()
		{
			if(!settingsManager.SystemDNSOverrideEnable)
				return;

			if(hasDNS)
				return;     // At least one DNS has been added, do not check for alternative

			if(settingsManager.SystemDNSOverrideEnable)
            {
                List<string> dnsAlternate = settingsManager.SystemDNSAlternativeList;
    			
                foreach(string dns in dnsAlternate)
    			{
    				DoAddDNS(dns);
    			}
            }
		}

		private void EnsureRoutes()
		{
			if(blockIPV4 == false)
			{
				vpnServiceBuilder.AllowFamily(global::Android.Systems.OsConstants.AfInet);
				// Routes all IPV4 traffic inside the tunnel
				vpnServiceBuilder.AddRoute("0.0.0.0", 0);
			}

			if(blockIPV6 == false)
			{
				vpnServiceBuilder.AllowFamily(global::Android.Systems.OsConstants.AfInet6);
				// Routes all IPV6 traffic inside the tunnel
				vpnServiceBuilder.AddRoute("::", 0);
			}
		}

		private void DoAddDNS(string address)
		{
			address = address.Trim();
			
            if(SupportTools.Empty(address))
				throw new Exception("Invalid DNS server");

			EddieLogger.Debug("Adding DNS server '{0}'", address);

			vpnServiceBuilder.AddDnsServer(address);
			hasDNS = true;
		}

		public void AddDNSServer(string address, int ipv6)
		{
			if(customDNS)
				EddieLogger.Debug("DNS forced (address '{0}' will be skipped)", address);
			else
				DoAddDNS(address);			
		}

		public void SetBlockIPV6(bool block)
		{
			blockIPV6 = block;			
		}

		public void SetMTU(int mtu)
		{
			if(forceMTU)
				EddieLogger.Debug("MTU forced (value '{0}' will be skipped)", mtu);
			else
				vpnServiceBuilder.SetMtu(mtu);
		}
	}
}
