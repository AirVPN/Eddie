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

using System;
using Android.OS;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Eddie.NativeAndroidApp
{
	public class OpenVPNTunnel : IVPNTunnel
	{
		private VPNService vpnService = null;
		private OpenVPNClient openVPNClient = null;
		private NativeMethods.ovpn3_client openVPNClientInterface;
		private object clientSync = new object();

		private Stack<VPNContext> m_contexts = new Stack<VPNContext>();
		private object contextsSync = new object();

		private VPN.Status vpnClientStatus = VPN.Status.UNKNOWN;

        private SettingsManager settingsManager = new SettingsManager();

        public enum VPNAction
        {
            PAUSE = 1,
            RESUME,
            LOCK,
            NETWORK_TYPE_CHANGED
        }

		public OpenVPNTunnel(VPNService service)
		{
			vpnService = service;

            EddieLogger.Init(vpnService);

			// NOTE: do NOT propagate any exception from the following callbacks since they are invoked from C++ code and could cause memory leaks or crashes

			openVPNClientInterface = new NativeMethods.ovpn3_client();
			openVPNClientInterface.socket_protect += OnSocketProtect;
			openVPNClientInterface.on_event += OnEvent;
			openVPNClientInterface.tun_builder_new += OnTunBuilderNew;
			openVPNClientInterface.tun_builder_set_layer += OnTunBuilderSetLayer;
			openVPNClientInterface.tun_builder_set_remote_address += OnTunBuilderSetRemoteAddress;
			openVPNClientInterface.tun_builder_add_address += OnTunBuilderAddAddress;
			openVPNClientInterface.tun_builder_set_route_metric_default += OnTunBuilderSetRouteMetricDefault;
			openVPNClientInterface.tun_builder_reroute_gw += OnTunBuildeRerouteGW;
			openVPNClientInterface.tun_builder_add_route += OnTunBuilderAddRoute;
			openVPNClientInterface.tun_builder_exclude_route += OnTunBuilderExcludeRoute;
			openVPNClientInterface.tun_builder_add_dns_server += OnTunBuilderAddDNSServer;
			openVPNClientInterface.tun_builder_add_search_domain += OnTunBuilderAddSearchDomain;
			openVPNClientInterface.tun_builder_set_mtu += OnTunBuilderSetMTU;
			openVPNClientInterface.tun_builder_set_session_name += OnTunBuilderSetSessionName;
			openVPNClientInterface.tun_builder_add_proxy_bypass += OnTunBuilderAddProxyBypass;
			openVPNClientInterface.tun_builder_set_proxy_auto_config_url += OnTunBuilderSetProxyAutoConfigUrl;
			openVPNClientInterface.tun_builder_set_proxy_http += OnTunBuilderSetProxyHttp;
			openVPNClientInterface.tun_builder_set_proxy_https += OnTunBuilderSetProxyHttps;
			openVPNClientInterface.tun_builder_add_wins_server += OnTunBuilderAddWinsServer;
			openVPNClientInterface.tun_builder_set_block_ipv6 += OnTunBuilderSetBlockIPV6;
			openVPNClientInterface.tun_builder_set_adapter_domain_suffix += OnTunBuilderSetAdapterDomainSuffix;
			openVPNClientInterface.tun_builder_establish += OnTunBuilderEstablish;
			openVPNClientInterface.tun_builder_persist += OnTunBuilderPersist;
			openVPNClientInterface.tun_builder_establish_lite += OnTunBuilderEstablishLite;
			openVPNClientInterface.tun_builder_teardown += OnTunBuilderTeardown;
			openVPNClientInterface.connect_attach += OnConnectAttach;
			openVPNClientInterface.connect_pre_run += OnConnectPreRun;
			openVPNClientInterface.connect_run += OnConnectRun;
			openVPNClientInterface.connect_session_stop += OnConnectSessionStop;
		}

        ~OpenVPNTunnel()
        {
            openVPNClient.Destroy();
        }

		public VPNService Service
		{
			get
			{
				return vpnService;
			}
		}

		public VPNContext ActiveContext
		{
			get
			{
				lock(contextsSync)
				{
					VPNContext context = null;

                    if(!m_contexts.TryPeek(out context))
						throw new Exception("internal error (cannot get a valid context)");

					return context;
				}				
			}
		}

		private int OnSocketProtect(int socket)
		{
			EddieLogger.Debug("OnSocketProtect(socket={0})", socket);			

			try
			{
				return OnSocketProtectImpl(socket);
			}
			catch(Exception e)
			{
				EddieLogger.Error("OnSocketProtect", e);
			}

			return NativeMethods.ERROR;			
		}

		private void OnEvent(ref NativeMethods.EddieLibraryEvent oe)
		{
			try
            {
                switch(oe.type)
                {
                    case NativeMethods.EventType.MESSAGE:
                    case NativeMethods.EventType.INFO:
                    {
                        EddieLogger.Info("{0}: {1}", oe.name, oe.info);
                    }
                    break;

                    case NativeMethods.EventType.WARN:
                    {
                        EddieLogger.Warning("OpenVPN {0}: {1}", oe.name, oe.info);
                    }
                    break;

                    case NativeMethods.EventType.ERROR:
                    case NativeMethods.EventType.FATAL_ERROR:
                    {
                        if(IgnoredError(oe.name))
                        {
                            EddieLogger.Warning("OpenVPN {0}: {1}", oe.name, oe.info);
                        }
                        else
                        {
                            // It seems OpenVPN is having BIG troubles with the connection
                            // In order to prevent worse conditions, try to lock the VPN (in case it is possible)
    
                            EddieLogger.Error("OpenVPN3 Fatal Error {0}: {1}", oe.name, oe.info);
                            
                            NetworkStatusChanged(VPNAction.LOCK);
    
                            vpnService.DoChangeStatus(VPN.Status.LOCKED);
    
                            AlertNotification(vpnService.Resources.GetString(Resource.String.connection_vpn_error));
                        }
                    }
                    break;
                    
                    case NativeMethods.EventType.FORMAL_WARNING:
                    {
                        if(IgnoredError(oe.name))
                        {
                            EddieLogger.Warning("OpenVPN {0}: {1}", oe.name, oe.info);
                        }
                        else
                        {
                            // It seems OpenVPN is having troubles with the connection
                            // In order to prevent worse conditions, lock the VPN
    
                            EddieLogger.Error("OpenVPN3 {0}: {1}", oe.name, oe.info);
                            
                            NetworkStatusChanged(VPNAction.LOCK);
    
                            vpnService.DoChangeStatus(VPN.Status.LOCKED);
    
                            AlertNotification(vpnService.Resources.GetString(Resource.String.connection_vpn_formal_warning));
                        }
                    }
                    break;

                    case NativeMethods.EventType.TUN_ERROR:
                    case NativeMethods.EventType.CLIENT_RESTART:
                    case NativeMethods.EventType.AUTH_FAILED:
                    case NativeMethods.EventType.CERT_VERIFY_FAIL:
                    case NativeMethods.EventType.TLS_VERSION_MIN:
                    case NativeMethods.EventType.CLIENT_HALT:
                    case NativeMethods.EventType.CLIENT_SETUP:
                    case NativeMethods.EventType.CONNECTION_TIMEOUT:
                    case NativeMethods.EventType.INACTIVE_TIMEOUT:
                    case NativeMethods.EventType.DYNAMIC_CHALLENGE:
                    case NativeMethods.EventType.PROXY_NEED_CREDS:
                    case NativeMethods.EventType.PROXY_ERROR:
                    case NativeMethods.EventType.TUN_SETUP_FAILED:
                    case NativeMethods.EventType.TUN_IFACE_CREATE:
                    case NativeMethods.EventType.TUN_IFACE_DISABLED:
                    case NativeMethods.EventType.EPKI_ERROR:
                    case NativeMethods.EventType.EPKI_INVALID_ALIAS:
                    {
                        // These OpenVPN events may cause a fatal error
                        // In order to prevent worse conditions, lock the VPN

                        EddieLogger.Error("OpenVPN {0}: {1}", oe.name, oe.info);
                        
                        NetworkStatusChanged(VPNAction.LOCK);

                        vpnService.DoChangeStatus(VPN.Status.LOCKED);

                        AlertNotification(vpnService.Resources.GetString(Resource.String.connection_vpn_formal_warning));
                    }
                    break;

                    case NativeMethods.EventType.CONNECTED:
                    {
                        if(oe.data.ToInt64() != 0)
                        {
                            NativeMethods.ovpn3_connection_data connectionData = Marshal.PtrToStructure<NativeMethods.ovpn3_connection_data>(oe.data);

                            EddieLogger.Info("CONNECTED: defined={0}, user={1}, serverHost={2}, serverPort={3}, serverProto={4}, serverIp={5}, vpnIp4={6}, vpnIp6={7}, gw4={8}, gw6={9}, clientIp={10}, tunName={11}", connectionData.defined, connectionData.user, connectionData.serverHost, connectionData.serverPort, connectionData.serverProto, connectionData.serverIp, connectionData.vpnIp4, connectionData.vpnIp6, connectionData.gw4, connectionData.gw6, connectionData.clientIp, connectionData.tunName);
                        }
                    }
                    break;

                    case NativeMethods.EventType.TRANSPORT_ERROR:
                    case NativeMethods.EventType.RELAY_ERROR:
                    case NativeMethods.EventType.DISCONNECTED:
                    {
                        EddieLogger.Warning("OpenVPN {0} - {1}: {2}", oe.type, oe.name, oe.info);
                    }
                    break;

                    default:
                    {
                        EddieLogger.Debug("OpenVPN Event: type={0}, name={1}, info={2}, data={3}", oe.type, oe.name, oe.info, oe.data.ToString());
                    }
                    break;
                }
            }
            catch(Exception e)
            {
                EddieLogger.Error("OnEvent", e);
            }           
		}

		private string GetEventContent(ref NativeMethods.EddieLibraryEvent oe)
		{
			string name = oe.name;
			string info = oe.info;

			if(SupportTools.Empty(info))
				return name;

			return name + ": " + info;			
		}

		private int OnSocketProtectImpl(int socket)
		{
			return Service.Protect(socket) ? NativeMethods.SUCCESS : NativeMethods.ERROR;
		}

		private int OnTunBuilderNew()
		{			
			EddieLogger.Debug("OnTunBuilderNew()");

			try
			{
				return OnTunBuilderNewImpl();
			}
			catch(Exception e)
			{
				EddieLogger.Error("OnTunBuilderNew", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuilderNewImpl()
		{
			lock(contextsSync)
			{
				m_contexts.Push(new VPNContext(Service));
			}
						
			return NativeMethods.SUCCESS;
		}

		private int OnTunBuilderSetLayer(int layer)
		{
			EddieLogger.Debug("OnTunBuilderSetLayer(layer={0})", layer);

			try
			{
				return OnTunBuilderSetLayerImpl(layer);
			}
			catch(Exception e)
			{
				EddieLogger.Error("OnTunBuilderSetLayer", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuilderSetLayerImpl(int layer)
		{
			return NativeMethods.SUCCESS;			
		}

		private int OnTunBuilderSetRemoteAddress(string address, /*bool*/ int ipv6)
		{
			EddieLogger.Debug("OnTunBuilderSetRemoteAddress(address={0}, ipv6={1})", address, ipv6);
			
			try
			{
				return OnTunBuilderSetRemoteAddressImpl(address, ipv6);
			}
			catch(Exception e)
			{
				EddieLogger.Error("OnTunBuilderSetRemoteAddress", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuilderSetRemoteAddressImpl(string address, /*bool*/ int ipv6)
		{
			return NativeMethods.SUCCESS;
		}

		private int OnTunBuilderAddAddress(string address, int prefix_length, string gateway, /*bool*/ int ipv6, /*bool*/ int net30)
		{
			EddieLogger.Debug("OnTunBuilderAddAddress(address={0}, prefix_length={1}, gateway={2}, ipv6={3}, net30={4})", address, prefix_length, gateway, ipv6, net30);

			try
			{
				return OnTunBuilderAddAddressImpl(address, prefix_length, gateway, ipv6, net30);
			}
			catch(Exception e)
			{
				EddieLogger.Error("OnTunBuilderAddAddress", e);
			}

			return NativeMethods.ERROR;			
		}

		public VPN.Status HandleScreenChanged(bool active)
		{
            if(vpnClientStatus == VPN.Status.LOCKED)
                return vpnClientStatus;

			lock(clientSync)
			{
				if(openVPNClient == null)
					return vpnClientStatus;

				if(active)
				{
					if(vpnClientStatus == VPN.Status.PAUSED)
					{
						EddieLogger.Info("Screen is now on, trying to resume VPN");

                        NativeMethods.EddieLibraryResult result = openVPNClient.Resume();
                        
						if(result.code == NativeMethods.ResultCode.SUCCESS)
                        {
							EddieLogger.Info("Successfully resumed VPN");
                            
                            vpnClientStatus = VPN.Status.CONNECTED;
                        }
						else
                        {
							EddieLogger.Error(string.Format("Failed to resume VPN. {0}", result.description));
                            
                            vpnClientStatus = VPN.Status.UNKNOWN;
                        }
					}
				}
				else
				{
					if(settingsManager.SystemPauseVpnWhenScreenIsOff)
					{
                        EddieLogger.Info("Screen is now off, trying to pause VPN");
                        
                        NativeMethods.EddieLibraryResult result = openVPNClient.Pause("Screen is off");

						if(result.code == NativeMethods.ResultCode.SUCCESS)
						{
                            EddieLogger.Info("Successfully paused VPN");
                            
                            vpnClientStatus = VPN.Status.PAUSED;
						}
						else
						{
                            EddieLogger.Error(string.Format("Failed to pause VPN. {0}", result.description));

                            vpnClientStatus = VPN.Status.NOT_CONNECTED;
						}
					}
				}
			}
            
            UpdateNotification(vpnClientStatus);

            return vpnClientStatus;
		}

        public VPN.Status NetworkStatusChanged(VPNAction action)
        {
            if(vpnClientStatus == VPN.Status.LOCKED)
                return vpnClientStatus;

            lock(clientSync)
            {
                if(openVPNClient == null)
                    return vpnClientStatus;

                switch(action)
                {
                    case VPNAction.RESUME:
                    {
                        if(vpnClientStatus == VPN.Status.PAUSED)
                        {
                            EddieLogger.Info("Network is now connected, trying to resume VPN");
    
                            NativeMethods.EddieLibraryResult result = openVPNClient.Resume();
                            
                            if(result.code == NativeMethods.ResultCode.SUCCESS)
                            {
                                EddieLogger.Info("Successfully resumed VPN");
                                
                                vpnClientStatus = VPN.Status.CONNECTED;
                            }
                            else
                            {
                                EddieLogger.Error(string.Format("Failed to resume VPN. {0}", result.description));
                                
                                vpnClientStatus = VPN.Status.UNKNOWN;
                            }
                        }
                    }
                    break;

                    case VPNAction.PAUSE:
                    case VPNAction.NETWORK_TYPE_CHANGED:
                    {
                        EddieLogger.Info("Network status has changed, trying to pause VPN");

                        NativeMethods.EddieLibraryResult result = openVPNClient.Pause("Network status changed");

                        if(result.code == NativeMethods.ResultCode.SUCCESS)
                        {
                            EddieLogger.Info("Successfully paused VPN");
                            
                            vpnClientStatus = VPN.Status.PAUSED;
                        }
                        else
                        {
                            EddieLogger.Error(string.Format("Failed to pause VPN. {0}", result.description));
                            
                            vpnClientStatus = VPN.Status.NOT_CONNECTED;
                        }
                    }
                    break;

                    case VPNAction.LOCK:
                    {
                        EddieLogger.Info("VPN error detected. Locking VPN");
    
                        NativeMethods.EddieLibraryResult result = openVPNClient.Pause("Lock VPN");

                        if(result.code == NativeMethods.ResultCode.SUCCESS)
                        {
                            EddieLogger.Info("Successfully locked VPN");
                            
                            vpnClientStatus = VPN.Status.LOCKED;
                        }
                        else
                        {
                            EddieLogger.Error(string.Format("Failed to lock VPN. {0}", result.description));

                            vpnClientStatus = VPN.Status.NOT_CONNECTED;
                        }
                    }
                    break;
                }
            }

            UpdateNotification(vpnClientStatus);

            return vpnClientStatus;
        }

		private int OnTunBuilderAddAddressImpl(string address, int prefix_length, string gateway, /*bool*/ int ipv6, /*bool*/ int net30)
		{
			ActiveContext.Builder.AddAddress(address, prefix_length);

			return NativeMethods.SUCCESS;
		}

		private int OnTunBuilderSetRouteMetricDefault(int metric)
		{
			EddieLogger.Debug("OnTunBuilderSetRouteMetricDefault(metric={0})", metric);

			try
			{
				return OnTunBuilderSetRouteMetricDefaultImpl(metric);
			}
			catch(Exception e)
			{
				EddieLogger.Error("OnTunBuilderSetRouteMetricDefault", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuilderSetRouteMetricDefaultImpl(int metric)
		{
			return NativeMethods.SUCCESS;
		}

		private int OnTunBuildeRerouteGW(/*bool*/ int ipv4, /*bool*/ int ipv6, int flags)
		{
			EddieLogger.Debug("OnTunBuildeRerouteGW(ipv4={0}, ipv6={1}, flags={2})", ipv4, ipv6, flags);

			try
			{
				return OnTunBuildeRerouteGWImpl(ipv4, ipv6, flags);
			}
			catch(Exception e)
			{
				EddieLogger.Error("OnTunBuildeRerouteGW", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuildeRerouteGWImpl(int ipv4, int ipv6, int flags)
		{
			return NativeMethods.SUCCESS;
		}

		private int OnTunBuilderAddRoute(string address, int prefix_length, int metric, /*bool*/ int ipv6)
		{
			EddieLogger.Debug("OnTunBuilderAddRoute(address={0}, prefix_length={1}, metric={2}, ipv6={3})", address, prefix_length, metric, ipv6);

			try
			{
				return OnTunBuilderAddRouteImpl(address, prefix_length, metric, ipv6);
			}
			catch(Exception e)
			{
				EddieLogger.Error("OnTunBuilderAddRoute", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuilderAddRouteImpl(string address, int prefix_length, int metric, /*bool*/ int ipv6)
		{
			ActiveContext.Builder.AddRoute(address, prefix_length);

			return NativeMethods.SUCCESS;
		}

		private int OnTunBuilderExcludeRoute(string address, int prefix_length, int metric, /*bool*/ int ipv6)
		{
			EddieLogger.Debug("OnTunBuilderExcludeRoute(address={0}, prefix_length={1}, metric={2}, ipv6={3})", address, prefix_length, metric, ipv6);

			try
			{
				return OnTunBuilderExcludeRouteImpl(address, prefix_length, metric, ipv6);
			}
			catch(Exception e)
			{
				EddieLogger.Error("OnTunBuilderExcludeRoute", e);
			}

			return NativeMethods.ERROR;			
		}

		private int OnTunBuilderExcludeRouteImpl(string address, int prefix_length, int metric, /*bool*/ int ipv6)
		{
			return NativeMethods.SUCCESS;
		}

		private int OnTunBuilderAddDNSServer(string address, /*bool*/ int ipv6)
		{
			EddieLogger.Debug("OnTunBuilderAddDNSServer(address={0}, ipv6={1})", address, ipv6);

			try
			{
				ActiveContext.AddDNSServer(address, ipv6);

				return NativeMethods.SUCCESS;
			}
			catch(Exception e)
			{
				EddieLogger.Error("OnTunBuilderAddDNSServer", e);
			}

			return NativeMethods.ERROR;
		}	

		private int OnTunBuilderAddSearchDomain(string domain)
		{
			EddieLogger.Debug("OnTunBuilderAddSearchDomain(domain={0})", domain);

			try
			{
				return OnTunBuilderAddSearchDomainImpl(domain);
			}
			catch(Exception e)
			{
				EddieLogger.Error("OnTunBuilderAddSearchDomain", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuilderAddSearchDomainImpl(string domain)
		{
			ActiveContext.Builder.AddSearchDomain(domain);

			return NativeMethods.SUCCESS;
		}

		private int OnTunBuilderSetMTU(int mtu)
		{
			EddieLogger.Debug("OnTunBuilderSetMTU(mtu={0})", mtu);

			try
			{
				ActiveContext.SetMTU(mtu);

				return NativeMethods.SUCCESS;
			}
			catch(Exception e)
			{
				EddieLogger.Error("OnTunBuilderSetMTU", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuilderSetSessionName(string name)
		{
			EddieLogger.Debug("OnTunBuilderSetSessionName(name={0})", name);

			try
			{
				return OnTunBuilderSetSessionNameImpl(name);
			}
			catch(Exception e)
			{
				EddieLogger.Error("OnTunBuilderSetSessionName", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuilderSetSessionNameImpl(string name)
		{
			ActiveContext.Builder.SetSession(name);

			return NativeMethods.SUCCESS;
		}

		private int OnTunBuilderAddProxyBypass(string bypass_host)
		{
			EddieLogger.Debug("OnTunBuilderAddProxyBypass(bypass_host={0})", bypass_host);

			try
			{
				return OnTunBuilderAddProxyBypassImpl(bypass_host);
			}
			catch(Exception e)
			{
				EddieLogger.Error("OnTunBuilderAddProxyBypass", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuilderAddProxyBypassImpl(string bypass_host)
		{
			return NativeMethods.SUCCESS;
		}

		private int OnTunBuilderSetProxyAutoConfigUrl(string url)
		{
			EddieLogger.Debug("OnTunBuilderSetProxyAutoConfigUrl(url={0})", url);

			try
			{
				return OnTunBuilderSetProxyAutoConfigUrlImpl(url);
			}
			catch(Exception e)
			{
				EddieLogger.Error("OnTunBuilderSetProxyAutoConfigUrl", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuilderSetProxyAutoConfigUrlImpl(string url)
		{
			return NativeMethods.SUCCESS;
		}

		private int OnTunBuilderSetProxyHttp(string host, int port)
		{
			EddieLogger.Debug("OnTunBuilderSetProxyHttp(host={0}, port={1})", host, port);

			try
			{
				return OnTunBuilderSetProxyHttpImpl(host, port);
			}
			catch(Exception e)
			{
				EddieLogger.Error("OnTunBuilderSetProxyHttp", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuilderSetProxyHttpImpl(string host, int port)
		{
			return NativeMethods.SUCCESS;
		}

		private int OnTunBuilderSetProxyHttps(string host, int port)
		{
			EddieLogger.Debug("OnTunBuilderSetProxyHttps(host={0}, port={1})", host, port);

			try
			{
				return OnTunBuilderSetProxyHttpsImpl(host, port);
			}
			catch(Exception e)
			{
				EddieLogger.Error("OnTunBuilderSetProxyHttps", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuilderSetProxyHttpsImpl(string host, int port)
		{
			return NativeMethods.SUCCESS;
		}

		private int OnTunBuilderAddWinsServer(string address)
		{
			EddieLogger.Debug("OnTunBuilderAddWinsServer(address={0})", address);

			try
			{
				return OnTunBuilderAddWinsServerImpl(address);
			}
			catch(Exception e)
			{
				EddieLogger.Error("OnTunBuilderAddWinsServer", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuilderAddWinsServerImpl(string address)
		{
			return NativeMethods.SUCCESS;
		}

		private int OnTunBuilderSetBlockIPV6(/*bool*/ int block_ipv6)
		{
			EddieLogger.Debug("OnTunBuilderSetBlockIPV6(block_ipv6={0})", block_ipv6);

			try
			{
				ActiveContext.SetBlockIPV6(SupportTools.BoolCast(block_ipv6));

				return NativeMethods.SUCCESS;
			}
			catch(Exception e)
			{
				EddieLogger.Error("OnTunBuilderSetBlockIPV6", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuilderSetAdapterDomainSuffix(string name)
		{
			EddieLogger.Debug("OnTunBuilderSetAdapterDomainSuffix(name={0})", name);

			try
			{
				return OnTunBuilderSetAdapterDomainSuffixImpl(name);
			}
			catch(Exception e)
			{
				EddieLogger.Error("OnTunBuilderSetAdapterDomainSuffix", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuilderSetAdapterDomainSuffixImpl(string name)
		{
			return NativeMethods.SUCCESS;
		}

		private int OnTunBuilderEstablish()
		{
			EddieLogger.Debug("OnTunBuilderEstablish");

			try
			{
				return OnTunBuilderEstablishImpl();
			}
			catch(Exception e)
			{
				EddieLogger.Error("OnTunBuilderEstablish", e);
			}

			return NativeMethods.ERROR;
		}		

		private int OnTunBuilderEstablishImpl()
		{
			ParcelFileDescriptor fileDescriptor = ActiveContext.Establish();		
			
            if(fileDescriptor == null)
				throw new Exception("VPNService.Builder.Establish failed");

			Service.HandleThreadStarted();
			
			return fileDescriptor.DetachFd();
		}
		 
		private void RunDispatcher()
		{
        /*
			TasksManager.Add((CancellationToken c) =>
			{
				OpenVPNDispatcher dispatcher = new OpenVPNDispatcher(this, m_cancellationToken);

                dispatcher.Run();
			}); */
		}

		private int OnTunBuilderPersist()
		{
			EddieLogger.Debug("OnTunBuilderPersist");

			try
			{
				return OnTunBuilderPersistImpl();
			}
			catch(Exception e)
			{
				EddieLogger.Error("OnTunBuilderPersist", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuilderPersistImpl()
		{
			return NativeMethods.SUCCESS;
		}

		private void OnTunBuilderEstablishLite()
		{
			EddieLogger.Debug("OnTunBuilderEstablishLite");

			try
			{
				OnTunBuilderEstablishLiteImpl();
			}
			catch(Exception e)
			{
				EddieLogger.Error("OnTunBuilderEstablishLite", e);
			}			
		}

		private void OnTunBuilderEstablishLiteImpl()
		{

		}

		private void OnTunBuilderTeardown(/*bool*/ int disconnect)
		{
			EddieLogger.Debug("OnTunBuilderTeardown(disconnect={0})", disconnect);
			
			try
			{
				OnTunBuilderTeardownImpl(disconnect);
			}
			catch(Exception e)
			{
				EddieLogger.Error("OnTunBuilderTeardown", e);
			}			
		}

		private void OnTunBuilderTeardownImpl(int disconnect)
		{

		}

		private void OnConnectAttach()
		{
			EddieLogger.Debug("OnConnectAttach");
			
			try
			{
				OnConnectAttachImpl();
			}
			catch(Exception e)
			{
				EddieLogger.Error("OnConnectAttach", e);
			}			
		}

		private void OnConnectAttachImpl()
		{

		}

		private void OnConnectPreRun()
		{
			EddieLogger.Debug("OnConnectPreRun");
			
			try
			{
				OnConnectPreRunImpl();
			}
			catch(Exception e)
			{
				EddieLogger.Error("OnConnectPreRun", e);
			}			
		}

		private void OnConnectPreRunImpl()
		{

		}

		private void OnConnectRun()
		{
			EddieLogger.Debug("OnConnectRun");

			try
			{
				OnConnectRunImpl();
			}
			catch(Exception e)
			{
				EddieLogger.Error("OnConnectRun", e);
			}			
		}

		private void OnConnectRunImpl()
		{

		}

		private void OnConnectSessionStop()
		{
			EddieLogger.Debug("OnConnectSessionStop");

			try
			{
				OnConnectSessionStopImpl();
			}
			catch(Exception e)
			{
				EddieLogger.Error("OnConnectSessionStop", e);
			}			
		}

		private void OnConnectSessionStopImpl()
		{
		}

		public void Init()
		{
			lock(clientSync)
			{
				if(openVPNClient != null)
					throw new Exception("client already initialized");

				openVPNClient = OpenVPNClient.Create(ref openVPNClientInterface);
				
                if(openVPNClient == null)
					throw new Exception("client creation failed");
			}			
		}

		public NativeMethods.ovpn3_transport_stats GetTransportStats()
		{
			lock(clientSync)
			{
                if(openVPNClient == null)
                {
                    string errMsg = "OpenVPNTunnel::GetTransportStats(): OpenVPN client is not initialized";
                    
                    EddieLogger.Error(errMsg);
                
                    throw new Exception(errMsg);
                }
	
				NativeMethods.ovpn3_transport_stats stats = new NativeMethods.ovpn3_transport_stats();				
				
                NativeMethods.EddieLibraryResult result = openVPNClient.GetTransportStats(ref stats);
                
                if(result.code != NativeMethods.ResultCode.SUCCESS)
                {
                    string errMsg = string.Format("OpenVPNTunnel::GetTransportStats(): Failed to get OpenVPN transport stats. {0}", result.description);
                    
                    EddieLogger.Error(errMsg);
                    
                    throw new Exception(errMsg);
                }
				
				return stats;
			}
		}

		public void LoadProfileFile(string filename)
		{
			LoadProfile(filename, false);
		}

		public void LoadProfileString(string profile)
		{
			LoadProfile(profile, true);
		}

		private void LoadProfile(string profile, bool isString)
		{
            NativeMethods.EddieLibraryResult result;
			
            lock(clientSync)
			{
				if(openVPNClient == null)
					throw new Exception("client not initialized");

				if(isString)
				{
                    result = openVPNClient.LoadProfileString(profile);

                    if(result.code != NativeMethods.ResultCode.SUCCESS)
                    {
                        string errMsg = string.Format("OpenVPNTunnel::LoadProfile(): Failed to load profile string. {0}", result.description);
                        
                        EddieLogger.Error(errMsg);
                        
                        throw new Exception(errMsg);
                    }
				}
				else
				{
                    result = openVPNClient.LoadProfileFile(profile);

					if(result.code != NativeMethods.ResultCode.SUCCESS)
                    {
                        string errMsg = string.Format("OpenVPNTunnel::LoadProfile(): Failed to load profile file. {0}", result.description);
                        
                        EddieLogger.Error(errMsg);
                        
                        throw new Exception(errMsg);
                    }
				}				
			}
		}

		private static string ToOptionValue(bool value)
		{
			return value ? "true" : "false";
		}

		private static string ToOptionValue(int value)
		{
			return value.ToString();
		}

		private void DoSetOption(string option, string value)
		{
            NativeMethods.EddieLibraryResult result = openVPNClient.SetOption(option, value);

            if(result.code != NativeMethods.ResultCode.SUCCESS)
            {
                string errMsg = String.Format("OpenVPNTunnel::DoSetOption(): Failed to set option '{0}' with value '{1}'. {2}", option, value, result.description);
                
                EddieLogger.Error(errMsg);
                
                throw new Exception(errMsg);
            }
		}

		public void BindOptions()
		{
			lock(clientSync)
			{
				if(openVPNClient == null)
					throw new Exception("client not initialized");

                DoSetOption(SettingsManager.OVPN3_OPTION_TLS_MIN_VERSION_NATIVE, settingsManager.Ovpn3TLSMinVersion);
                DoSetOption(SettingsManager.OVPN3_OPTION_PROTOCOL_NATIVE, settingsManager.Ovpn3Protocol);
                DoSetOption(SettingsManager.OVPN3_OPTION_IPV6_NATIVE, settingsManager.Ovpn3IPV6);
                DoSetOption(SettingsManager.OVPN3_OPTION_TIMEOUT_NATIVE, settingsManager.Ovpn3Timeout);
                DoSetOption(SettingsManager.OVPN3_OPTION_TUN_PERSIST_NATIVE, ToOptionValue(settingsManager.Ovpn3TunPersist));
                DoSetOption(SettingsManager.OVPN3_OPTION_COMPRESSION_MODE_NATIVE, settingsManager.Ovpn3CompressionMode);
                DoSetOption(SettingsManager.OVPN3_OPTION_USERNAME_NATIVE, settingsManager.Ovpn3Username);
                DoSetOption(SettingsManager.OVPN3_OPTION_PASSWORD_NATIVE, settingsManager.Ovpn3Password);
                DoSetOption(SettingsManager.OVPN3_OPTION_SYNCHRONOUS_DNS_LOOKUP_NATIVE, ToOptionValue(settingsManager.Ovpn3SynchronousDNSLookup));
                DoSetOption(SettingsManager.OVPN3_OPTION_AUTOLOGIN_SESSIONS_NATIVE, ToOptionValue(settingsManager.Ovpn3AutologinSessions));
                DoSetOption(SettingsManager.OVPN3_OPTION_DISABLE_CLIENT_CERT_NATIVE, ToOptionValue(settingsManager.Ovpn3DisableClientCert));
                DoSetOption(SettingsManager.OVPN3_OPTION_SSL_DEBUG_LEVEL_NATIVE, settingsManager.Ovpn3SSLDebugLevel);
                DoSetOption(SettingsManager.OVPN3_OPTION_PRIVATE_KEY_PASSWORD_NATIVE, settingsManager.Ovpn3PrivateKeyPassword);
                DoSetOption(SettingsManager.OVPN3_OPTION_DEFAULT_KEY_DIRECTION_NATIVE, settingsManager.Ovpn3DefaultKeyDirection);
                DoSetOption(SettingsManager.OVPN3_OPTION_FORCE_AES_CBC_CIPHERSUITES_NATIVE, ToOptionValue(settingsManager.Ovpn3ForceAESCBCCiphersuites));
                DoSetOption(SettingsManager.OVPN3_OPTION_TLS_CERT_PROFILE_NATIVE, settingsManager.Ovpn3TLSCertProfile);
				
                if(settingsManager.SystemProxyEnable)
				{
                    DoSetOption(SettingsManager.OVPN3_OPTION_PROXY_HOST_NATIVE, settingsManager.Ovpn3ProxyHost);
                    DoSetOption(SettingsManager.OVPN3_OPTION_PROXY_PORT_NATIVE, settingsManager.Ovpn3ProxyPort);
                    DoSetOption(SettingsManager.OVPN3_OPTION_PROXY_USERNAME_NATIVE, settingsManager.Ovpn3ProxyUsername);
                    DoSetOption(SettingsManager.OVPN3_OPTION_PROXY_PASSWORD_NATIVE, settingsManager.Ovpn3ProxyPassword);
                    DoSetOption(SettingsManager.OVPN3_OPTION_PROXY_ALLOW_CLEARTEXT_AUTH_NATIVE, ToOptionValue(settingsManager.Ovpn3ProxyAllowCleartextAuth));
				}				
			}
		}

		public void Run()
		{
			try
			{
				DoRun();
			}
			catch(Exception e)
			{
				Service.HandleThreadException(e);
			}			
		}

		private void DoRun()
		{
			OpenVPNClient client = null;

			lock(clientSync)
			{
				if(openVPNClient == null)
                {
                    string errMsg = "OpenVPNTunnel::DoRun(): OpenVPN client is not initialized";
                    
                    EddieLogger.Error(errMsg);
                
					throw new Exception(errMsg);
                }

				client = openVPNClient;

				// Do NOT call m_client.Start under m_clientSync'lock	
			}

			// Dispatcher must be instantiated before starting to allow handling stop requests from outside while the client isn't started yet

            // RunDispatcher();

            NativeMethods.EddieLibraryResult result = client.Start();
            
			if(result.code != NativeMethods.ResultCode.SUCCESS)
            {
                string errMsg = string.Format("OpenVPNTunnel::DoRun(): Failed to start OpenVPN client. {0}", result.description);
                
                EddieLogger.Error(errMsg);
                
				throw new Exception(errMsg);
            }
		}

		public void Cleanup()
		{
			OpenVPNClient client = null;

			lock(clientSync)
			{
                if(openVPNClient == null)
                {
                    string errMsg = "OpenVPNTunnel::Cleanup(): OpenVPN client is not initialized";
                    
                    EddieLogger.Error(errMsg);
                
                    throw new Exception(errMsg);
                }

				client = openVPNClient;
				
                openVPNClient = null;
			}

			try
			{
                NativeMethods.EddieLibraryResult result = client.Stop();

                if(result.code != NativeMethods.ResultCode.SUCCESS)
                {
                    string errMsg = string.Format("OpenVPNTunnel::Cleanup(): Failed to stop OpenVPN client. {0}", result.description);
                    
                    EddieLogger.Error(errMsg);
                    
                    throw new Exception(errMsg);
                }
			}
			finally
			{
				ClearContexts();			
			}	
		}		

		private void ClearContexts()
		{
			EddieLogger.Debug("ClearContexts");

			lock(contextsSync)
			{
				while(m_contexts.Count > 0)
				{
					EddieLogger.Debug("Disposing context");

					SupportTools.SafeDispose(m_contexts.Pop());
				}
			}			
		}

        private void AlertNotification(string message)
        {
            if(message.Equals(""))
                return;

            vpnService.AlertNotification(message);
        }

        private void UpdateNotification(VPN.Status status)
        {
            string text, server = "";

            if(status != VPN.Status.CONNECTED && status != VPN.Status.PAUSED && status != VPN.Status.LOCKED)
                return;

            Dictionary<string, string> pData = settingsManager.SystemLastProfileInfo;
                
            if(pData.Count > 0 && pData.ContainsKey("server"))
                server = pData["server"];

            text = String.Format(vpnService.Resources.GetString(Resource.String.notification_text), server);
            
            if(!NetworkStatusReceiver.GetNetworkDescription().Equals(""))
                text += " " + String.Format(vpnService.Resources.GetString(Resource.String.notification_network), NetworkStatusReceiver.GetNetworkDescription());

            if(status == VPN.Status.PAUSED)
                text += " (" + vpnService.Resources.GetString(Resource.String.vpn_status_paused) + ")";
                
            if(status == VPN.Status.LOCKED)
                text += " (" + vpnService.Resources.GetString(Resource.String.vpn_status_locked) + ")";

            vpnService.UpdateNotification(text);
        }
        
        private void ResumeVPNAfterSeconds(int seconds)
        {
            EddieLogger.Info("VPN will be resumed in {0} seconds", seconds);

            System.Timers.Timer timer = new System.Timers.Timer();

            timer.Elapsed += (sender, args) =>
            {
                EddieLogger.Info("Trying to resume VPN");

                NetworkStatusChanged(VPNAction.RESUME);
                
                timer.Enabled = false;
            };

            timer.Interval = seconds * 1000;
            timer.Enabled = true;
        }
        
        private bool IgnoredError(string s)
        {
            bool ignoreWarning = false;
            string[] ignoredKeys = new string[]{ "NETWORK_RECV_ERROR",
                                                 "PKTID_INVALID",
                                                 "PKTID_BACKTRACK",
                                                 "PKTID_EXPIRE",
                                                 "PKTID_REPLAY",
                                                 "PKTID_TIME_BACKTRACK",
                                                 "TRANSPORT_ERROR"
                                               };

            for(int i = 0; i < ignoredKeys.Length && ignoreWarning == false; i++)
            {
                if(s.Equals(ignoredKeys[i]))
                    ignoreWarning = true;
            }

            return ignoreWarning;
        }
	}
}
