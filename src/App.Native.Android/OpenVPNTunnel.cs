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
using Eddie.Common.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using Eddie.Common.Log;
using System.Collections.Generic;

namespace Eddie.NativeAndroidApp
{
	public class OpenVPNTunnel : IVPNTunnel
	{
		private VPNService m_service = null;
		private OpenVPNClient m_client = null;
		private NativeMethods.ovpn3_client m_clientInterface;
		private object m_clientSync = new object();

		private Stack<VPNContext> m_contexts = new Stack<VPNContext>();
		private object m_contextsSync = new object();

		private CancellationToken m_cancellationToken;
		
		private bool m_clientPaused = false;

        private SettingsManager settingsManager = new SettingsManager();

		public OpenVPNTunnel(VPNService service)
		{
			m_service = service;

			// NOTE: do NOT propagate any exception from the following callbacks since they are invoked from C++ code and could cause memory leaks or crashes

			m_clientInterface = new NativeMethods.ovpn3_client();
			m_clientInterface.socket_protect += OnSocketProtect;
			m_clientInterface.on_event += OnEvent;
			m_clientInterface.tun_builder_new += OnTunBuilderNew;
			m_clientInterface.tun_builder_set_layer += OnTunBuilderSetLayer;
			m_clientInterface.tun_builder_set_remote_address += OnTunBuilderSetRemoteAddress;
			m_clientInterface.tun_builder_add_address += OnTunBuilderAddAddress;
			m_clientInterface.tun_builder_set_route_metric_default += OnTunBuilderSetRouteMetricDefault;
			m_clientInterface.tun_builder_reroute_gw += OnTunBuildeRerouteGW;
			m_clientInterface.tun_builder_add_route += OnTunBuilderAddRoute;
			m_clientInterface.tun_builder_exclude_route += OnTunBuilderExcludeRoute;
			m_clientInterface.tun_builder_add_dns_server += OnTunBuilderAddDNSServer;
			m_clientInterface.tun_builder_add_search_domain += OnTunBuilderAddSearchDomain;
			m_clientInterface.tun_builder_set_mtu += OnTunBuilderSetMTU;
			m_clientInterface.tun_builder_set_session_name += OnTunBuilderSetSessionName;
			m_clientInterface.tun_builder_add_proxy_bypass += OnTunBuilderAddProxyBypass;
			m_clientInterface.tun_builder_set_proxy_auto_config_url += OnTunBuilderSetProxyAutoConfigUrl;
			m_clientInterface.tun_builder_set_proxy_http += OnTunBuilderSetProxyHttp;
			m_clientInterface.tun_builder_set_proxy_https += OnTunBuilderSetProxyHttps;
			m_clientInterface.tun_builder_add_wins_server += OnTunBuilderAddWinsServer;
			m_clientInterface.tun_builder_set_block_ipv6 += OnTunBuilderSetBlockIPV6;
			m_clientInterface.tun_builder_set_adapter_domain_suffix += OnTunBuilderSetAdapterDomainSuffix;
			m_clientInterface.tun_builder_establish += OnTunBuilderEstablish;
			m_clientInterface.tun_builder_persist += OnTunBuilderPersist;
			m_clientInterface.tun_builder_establish_lite += OnTunBuilderEstablishLite;
			m_clientInterface.tun_builder_teardown += OnTunBuilderTeardown;
			m_clientInterface.connect_attach += OnConnectAttach;
			m_clientInterface.connect_pre_run += OnConnectPreRun;
			m_clientInterface.connect_run += OnConnectRun;
			m_clientInterface.connect_session_stop += OnConnectSessionStop;
		}

		public VPNService Service
		{
			get
			{
				return m_service;
			}
		}

		public TasksManager TasksManager
		{
			get
			{
				return m_service.TasksManager;
			}
		}

		public VPNContext ActiveContext
		{
			get
			{
				lock(m_contextsSync)
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
			LogsManager.Instance.Debug("OnSocketProtect(socket={0})", socket);			

			try
			{
				return OnSocketProtectImpl(socket);
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnSocketProtect", e);
			}

			return NativeMethods.ERROR;			
		}

		private void OnEvent(ref NativeMethods.ovpn3_event oe)
		{
			LogsManager.Instance.Debug("OnEvent(type={0}, name={1}, info={2}, data={3})", oe.type, oe.name, oe.info, oe.data.ToString());
		
			try
			{
				OnEventImpl(ref oe);
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnEvent", e);
			}			
		}

		private string GetEventContent(ref NativeMethods.ovpn3_event oe)
		{
			string name = oe.name;
			string info = oe.info;

			if(SupportTools.Empty(info))
				return name;

			return name + ": " + info;			
		}
	
		private void OnEventImpl(ref NativeMethods.ovpn3_event oe)
		{
			if(oe.name != "CONNECTED" || (oe.data.ToInt64() == 0))
				return;

			NativeMethods.ovpn3_connection_data connectionData = Marshal.PtrToStructure<NativeMethods.ovpn3_connection_data>(oe.data);
			LogsManager.Instance.Debug("CONNECTED: defined={0}, user={1}, serverHost={2}, serverPort={3}, serverProto={4}, serverIp={5}, vpnIp4={6}, vpnIp6={7}, gw4={8}, gw6={9}, clientIp={10}, tunName={11}", connectionData.defined, connectionData.user, connectionData.serverHost, connectionData.serverPort, connectionData.serverProto, connectionData.serverIp, connectionData.vpnIp4, connectionData.vpnIp6, connectionData.gw4, connectionData.gw6, connectionData.clientIp, connectionData.tunName);			
		}		

		private int OnSocketProtectImpl(int socket)
		{
			return Service.Protect(socket) ? NativeMethods.SUCCESS : NativeMethods.ERROR;
		}

		private int OnTunBuilderNew()
		{			
			LogsManager.Instance.Debug("OnTunBuilderNew()");

			try
			{
				return OnTunBuilderNewImpl();
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnTunBuilderNew", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuilderNewImpl()
		{
			lock(m_contextsSync)
			{
				m_contexts.Push(new VPNContext(Service));
			}
						
			return NativeMethods.SUCCESS;
		}

		private int OnTunBuilderSetLayer(int layer)
		{
			LogsManager.Instance.Debug("OnTunBuilderSetLayer(layer={0})", layer);

			try
			{
				return OnTunBuilderSetLayerImpl(layer);
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnTunBuilderSetLayer", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuilderSetLayerImpl(int layer)
		{
			return NativeMethods.SUCCESS;			
		}

		private int OnTunBuilderSetRemoteAddress(string address, /*bool*/ int ipv6)
		{
			LogsManager.Instance.Debug("OnTunBuilderSetRemoteAddress(address={0}, ipv6={1})", address, ipv6);
			
			try
			{
				return OnTunBuilderSetRemoteAddressImpl(address, ipv6);
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnTunBuilderSetRemoteAddress", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuilderSetRemoteAddressImpl(string address, /*bool*/ int ipv6)
		{
			return NativeMethods.SUCCESS;
		}

		private int OnTunBuilderAddAddress(string address, int prefix_length, string gateway, /*bool*/ int ipv6, /*bool*/ int net30)
		{
			LogsManager.Instance.Debug("OnTunBuilderAddAddress(address={0}, prefix_length={1}, gateway={2}, ipv6={3}, net30={4})", address, prefix_length, gateway, ipv6, net30);

			try
			{
				return OnTunBuilderAddAddressImpl(address, prefix_length, gateway, ipv6, net30);
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnTunBuilderAddAddress", e);
			}

			return NativeMethods.ERROR;			
		}

		public void HandleScreenChanged(bool active)
		{
			lock(m_clientSync)
			{
				if(m_client == null)
					return;

				if(active)
				{
					if(m_clientPaused)
					{
						m_clientPaused = false;

						LogsManager.Instance.Debug("Client has been paused, trying resume VPN...");

						if(m_client.Resume())
							LogsManager.Instance.Debug("Client resumed");
						else
							LogsManager.Instance.Error("Client resume failed");						
					}
				}
				else
				{
					if(settingsManager.SystemPauseVpnWhenScreenIsOff)
					{
                        LogsManager.Instance.Debug("Battery saver is on, trying to pause VPN...");

						if(m_client.Pause("screen_off"))
						{
                            m_clientPaused = true;

                            LogsManager.Instance.Debug("Client paused");
						}
						else
						{
                            LogsManager.Instance.Debug("Client pause failed");
						}
					}
				}
			}
		}

		private int OnTunBuilderAddAddressImpl(string address, int prefix_length, string gateway, /*bool*/ int ipv6, /*bool*/ int net30)
		{
			ActiveContext.Builder.AddAddress(address, prefix_length);

			return NativeMethods.SUCCESS;
		}

		private int OnTunBuilderSetRouteMetricDefault(int metric)
		{
			LogsManager.Instance.Debug("OnTunBuilderSetRouteMetricDefault(metric={0})", metric);

			try
			{
				return OnTunBuilderSetRouteMetricDefaultImpl(metric);
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnTunBuilderSetRouteMetricDefault", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuilderSetRouteMetricDefaultImpl(int metric)
		{
			return NativeMethods.SUCCESS;
		}

		private int OnTunBuildeRerouteGW(/*bool*/ int ipv4, /*bool*/ int ipv6, int flags)
		{
			LogsManager.Instance.Debug("OnTunBuildeRerouteGW(ipv4={0}, ipv6={1}, flags={2})", ipv4, ipv6, flags);

			try
			{
				return OnTunBuildeRerouteGWImpl(ipv4, ipv6, flags);
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnTunBuildeRerouteGW", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuildeRerouteGWImpl(int ipv4, int ipv6, int flags)
		{
			return NativeMethods.SUCCESS;
		}

		private int OnTunBuilderAddRoute(string address, int prefix_length, int metric, /*bool*/ int ipv6)
		{
			LogsManager.Instance.Debug("OnTunBuilderAddRoute(address={0}, prefix_length={1}, metric={2}, ipv6={3})", address, prefix_length, metric, ipv6);

			try
			{
				return OnTunBuilderAddRouteImpl(address, prefix_length, metric, ipv6);
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnTunBuilderAddRoute", e);
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
			LogsManager.Instance.Debug("OnTunBuilderExcludeRoute(address={0}, prefix_length={1}, metric={2}, ipv6={3})", address, prefix_length, metric, ipv6);

			try
			{
				return OnTunBuilderExcludeRouteImpl(address, prefix_length, metric, ipv6);
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnTunBuilderExcludeRoute", e);
			}

			return NativeMethods.ERROR;			
		}

		private int OnTunBuilderExcludeRouteImpl(string address, int prefix_length, int metric, /*bool*/ int ipv6)
		{
			return NativeMethods.SUCCESS;
		}

		private int OnTunBuilderAddDNSServer(string address, /*bool*/ int ipv6)
		{
			LogsManager.Instance.Debug("OnTunBuilderAddDNSServer(address={0}, ipv6={1})", address, ipv6);

			try
			{
				ActiveContext.AddDNSServer(address, ipv6);

				return NativeMethods.SUCCESS;
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnTunBuilderAddDNSServer", e);
			}

			return NativeMethods.ERROR;
		}	

		private int OnTunBuilderAddSearchDomain(string domain)
		{
			LogsManager.Instance.Debug("OnTunBuilderAddSearchDomain(domain={0})", domain);

			try
			{
				return OnTunBuilderAddSearchDomainImpl(domain);
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnTunBuilderAddSearchDomain", e);
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
			LogsManager.Instance.Debug("OnTunBuilderSetMTU(mtu={0})", mtu);

			try
			{
				ActiveContext.SetMTU(mtu);

				return NativeMethods.SUCCESS;
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnTunBuilderSetMTU", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuilderSetSessionName(string name)
		{
			LogsManager.Instance.Debug("OnTunBuilderSetSessionName(name={0})", name);

			try
			{
				return OnTunBuilderSetSessionNameImpl(name);
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnTunBuilderSetSessionName", e);
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
			LogsManager.Instance.Debug("OnTunBuilderAddProxyBypass(bypass_host={0})", bypass_host);

			try
			{
				return OnTunBuilderAddProxyBypassImpl(bypass_host);
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnTunBuilderAddProxyBypass", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuilderAddProxyBypassImpl(string bypass_host)
		{
			return NativeMethods.SUCCESS;
		}

		private int OnTunBuilderSetProxyAutoConfigUrl(string url)
		{
			LogsManager.Instance.Debug("OnTunBuilderSetProxyAutoConfigUrl(url={0})", url);

			try
			{
				return OnTunBuilderSetProxyAutoConfigUrlImpl(url);
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnTunBuilderSetProxyAutoConfigUrl", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuilderSetProxyAutoConfigUrlImpl(string url)
		{
			return NativeMethods.SUCCESS;
		}

		private int OnTunBuilderSetProxyHttp(string host, int port)
		{
			LogsManager.Instance.Debug("OnTunBuilderSetProxyHttp(host={0}, port={1})", host, port);

			try
			{
				return OnTunBuilderSetProxyHttpImpl(host, port);
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnTunBuilderSetProxyHttp", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuilderSetProxyHttpImpl(string host, int port)
		{
			return NativeMethods.SUCCESS;
		}

		private int OnTunBuilderSetProxyHttps(string host, int port)
		{
			LogsManager.Instance.Debug("OnTunBuilderSetProxyHttps(host={0}, port={1})", host, port);

			try
			{
				return OnTunBuilderSetProxyHttpsImpl(host, port);
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnTunBuilderSetProxyHttps", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuilderSetProxyHttpsImpl(string host, int port)
		{
			return NativeMethods.SUCCESS;
		}

		private int OnTunBuilderAddWinsServer(string address)
		{
			LogsManager.Instance.Debug("OnTunBuilderAddWinsServer(address={0})", address);

			try
			{
				return OnTunBuilderAddWinsServerImpl(address);
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnTunBuilderAddWinsServer", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuilderAddWinsServerImpl(string address)
		{
			return NativeMethods.SUCCESS;
		}

		private int OnTunBuilderSetBlockIPV6(/*bool*/ int block_ipv6)
		{
			LogsManager.Instance.Debug("OnTunBuilderSetBlockIPV6(block_ipv6={0})", block_ipv6);

			try
			{
				ActiveContext.SetBlockIPV6(SupportTools.BoolCast(block_ipv6));

				return NativeMethods.SUCCESS;
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnTunBuilderSetBlockIPV6", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuilderSetAdapterDomainSuffix(string name)
		{
			LogsManager.Instance.Debug("OnTunBuilderSetAdapterDomainSuffix(name={0})", name);

			try
			{
				return OnTunBuilderSetAdapterDomainSuffixImpl(name);
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnTunBuilderSetAdapterDomainSuffix", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuilderSetAdapterDomainSuffixImpl(string name)
		{
			return NativeMethods.SUCCESS;
		}

		private int OnTunBuilderEstablish()
		{
			LogsManager.Instance.Debug("OnTunBuilderEstablish");

			try
			{
				return OnTunBuilderEstablishImpl();
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnTunBuilderEstablish", e);
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
			TasksManager.Add((CancellationToken c) =>
			{
				OpenVPNDispatcher dispatcher = new OpenVPNDispatcher(this, m_cancellationToken);
				dispatcher.Run();
			});
		}
		
		private int OnTunBuilderPersist()
		{
			LogsManager.Instance.Debug("OnTunBuilderPersist");

			try
			{
				return OnTunBuilderPersistImpl();
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnTunBuilderPersist", e);
			}

			return NativeMethods.ERROR;
		}

		private int OnTunBuilderPersistImpl()
		{
			return NativeMethods.SUCCESS;
		}

		private void OnTunBuilderEstablishLite()
		{
			LogsManager.Instance.Debug("OnTunBuilderEstablishLite");

			try
			{
				OnTunBuilderEstablishLiteImpl();
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnTunBuilderEstablishLite", e);
			}			
		}

		private void OnTunBuilderEstablishLiteImpl()
		{

		}

		private void OnTunBuilderTeardown(/*bool*/ int disconnect)
		{
			LogsManager.Instance.Debug("OnTunBuilderTeardown(disconnect={0})", disconnect);
			
			try
			{
				OnTunBuilderTeardownImpl(disconnect);
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnTunBuilderTeardown", e);
			}			
		}

		private void OnTunBuilderTeardownImpl(int disconnect)
		{

		}

		private void OnConnectAttach()
		{
			LogsManager.Instance.Debug("OnConnectAttach");
			
			try
			{
				OnConnectAttachImpl();
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnConnectAttach", e);
			}			
		}

		private void OnConnectAttachImpl()
		{

		}

		private void OnConnectPreRun()
		{
			LogsManager.Instance.Debug("OnConnectPreRun");
			
			try
			{
				OnConnectPreRunImpl();
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnConnectPreRun", e);
			}			
		}

		private void OnConnectPreRunImpl()
		{

		}

		private void OnConnectRun()
		{
			LogsManager.Instance.Debug("OnConnectRun");

			try
			{
				OnConnectRunImpl();
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnConnectRun", e);
			}			
		}

		private void OnConnectRunImpl()
		{

		}

		private void OnConnectSessionStop()
		{
			LogsManager.Instance.Debug("OnConnectSessionStop");

			try
			{
				OnConnectSessionStopImpl();
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("OnConnectSessionStop", e);
			}			
		}

		private void OnConnectSessionStopImpl()
		{

		}

		public void Init()
		{
			lock(m_clientSync)
			{
				if(m_client != null)
					throw new Exception("client already initialized");

				m_client = OpenVPNClient.Create(ref m_clientInterface);
				if(m_client == null)
					throw new Exception("client creation failed");
			}			
		}

		public NativeMethods.ovpn3_transport_stats GetTransportStats()
		{
			lock(m_clientSync)
			{
				if(m_client == null)
					throw new Exception("client not initialized");
	
				NativeMethods.ovpn3_transport_stats stats = new NativeMethods.ovpn3_transport_stats();				
				if(!m_client.GetTransportStats(ref stats))
					throw new Exception("failed to get transport stats");
				
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
			lock(m_clientSync)
			{
				if(m_client == null)
					throw new Exception("client not initialized");

				if(isString)
				{
					if(!m_client.LoadProfileString(profile))
						throw new Exception(String.Format("failed to load profile string '{0}'", profile));
				}
				else
				{
					if(!m_client.LoadProfileFile(profile))
						throw new Exception(String.Format("failed to load profile file '{0}'", profile));
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
			if(!m_client.SetOption(option, value))
				throw new Exception(String.Format("failed to set option '{0}' with value '{1}'", option, value));
		}

		public void BindOptions()
		{
			lock(m_clientSync)
			{
				if(m_client == null)
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

		public void Run(CancellationToken c)
		{
			m_cancellationToken = c;

			try
			{
				DoRun();
				Service.HandleThreadStopped();
			}
			catch(Exception e)
			{
				Service.HandleThreadException(e);
			}			
		}

		private void DoRun()
		{
			OpenVPNClient client = null;

			lock(m_clientSync)
			{
				if(m_client == null)
					throw new Exception("client not initialized");

				client = m_client;

				// Do NOT call m_client.Start under m_clientSync'lock	
			}

			// Dispatcher must be instantiated before starting to allow handling stop requests from outside while the client isn't started yet
			RunDispatcher();

			if(!client.Start())
				throw new Exception("client start failed");
		}

		public void Cleanup()
		{
			OpenVPNClient client = null;

			lock(m_clientSync)
			{
				if(m_client == null)
					throw new Exception("client not initialized");

				client = m_client;
				m_client = null;
			}

			try
			{
				if(!client.Stop())
					throw new Exception("client stop failed");
			}
			finally
			{
				ClearContexts();			
			}	
		}		

		private void ClearContexts()
		{
			LogsManager.Instance.Debug("ClearContexts");

			lock(m_contextsSync)
			{
				while(m_contexts.Count > 0)
				{
					LogsManager.Instance.Debug("Disposing context");

					SupportTools.SafeDispose(m_contexts.Pop());
				}
			}			
		}
	}
}
