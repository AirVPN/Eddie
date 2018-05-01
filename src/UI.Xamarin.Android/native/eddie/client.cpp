// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2017 AirVPN (support@airvpn.org) / https://airvpn.org
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

#include "stdafx.h"
#include "client.h"

#include "client/ovpncli.hpp"
#include "constants.h"
#include "openvpn/client/cliconstants.hpp"
#include "openvpn/options/merge.hpp"
#include "options.h"
#include "utils.h"

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

EDDIE_NAMESPACE_BEGIN()

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

class Client::Impl : public openvpn::ClientAPI::OpenVPNClient
{
	typedef openvpn::ClientAPI::OpenVPNClient BaseClass;

	typedef std::unordered_map<std::string, std::string> OptionsMap;

// Construction
public:
	Impl(ovpn3_client *ci);
	virtual ~Impl();

// Attributes
public:
	const std::string & getOption(const std::string &name, const std::string &defValue = constants::empty_string) const;
	void setOption(const std::string &name, const std::string &value, bool check = false);

	template <typename T>
	T getOptionValue(const std::string &name) const;
	template <typename T>
	T getOptionValue(const std::string &name, const T &defValue) const;

// Operations
public:
	void initOptions();
	
	void applyConfig(openvpn::ClientAPI::Config &config);
	void applyCreds(openvpn::ClientAPI::ProvideCreds &creds);

private:
	void dumpConfig(openvpn::ClientAPI::Config &config);
	void dumpCreds(openvpn::ClientAPI::ProvideCreds &creds);

// OpenVPNClient interface
public:
	virtual void log(const openvpn::ClientAPI::LogInfo &li) EDDIE_OVERRIDE;
	virtual bool socket_protect(int socket) EDDIE_OVERRIDE;
	virtual bool pause_on_connection_timeout() EDDIE_OVERRIDE;
	virtual void event(const openvpn::ClientAPI::Event &e) EDDIE_OVERRIDE;
	virtual void external_pki_cert_request(openvpn::ClientAPI::ExternalPKICertRequest&) EDDIE_OVERRIDE;
	virtual void external_pki_sign_request(openvpn::ClientAPI::ExternalPKISignRequest&) EDDIE_OVERRIDE;

// OpenVPNClient overrides
protected:
	virtual void connect_attach() EDDIE_OVERRIDE;
	virtual void connect_pre_run() EDDIE_OVERRIDE;
	virtual void connect_run() EDDIE_OVERRIDE;
	virtual void connect_session_stop() EDDIE_OVERRIDE;

// TunBuilderBase overrides
public:
	// Tun builder methods, loosely based on the Android VpnService.Builder
	// abstraction.  These methods comprise an abstraction layer that
	// allows the OpenVPN C++ core to call out to external methods for
	// establishing the tunnel, adding routes, etc.

	// All methods returning bool use the return
	// value to indicate success (true) or fail (false).
	// tun_builder_new() should be called first, then arbitrary setter methods,
	// and finally tun_builder_establish to return the socket descriptor
	// for the session.  IP addresses are pre-validated before being passed to
	// these methods.
	// This interface is based on Android's VpnService.Builder.

	// Callback to construct a new tun builder
	// Should be called first.
	virtual bool tun_builder_new() EDDIE_OVERRIDE;

	// Optional callback that indicates OSI layer, should be 2 or 3.
	// Defaults to 3.
	virtual bool tun_builder_set_layer(int layer) EDDIE_OVERRIDE;

	// Callback to set address of remote server
	// Never called more than once per tun_builder session.
	virtual bool tun_builder_set_remote_address(const std::string &address, bool ipv6) EDDIE_OVERRIDE;

	// Callback to add network address to VPN interface
	// May be called more than once per tun_builder session
	virtual bool tun_builder_add_address(const std::string &address, int prefix_length, const std::string &gateway, bool ipv6, bool net30) EDDIE_OVERRIDE;

	// Optional callback to set default value for route metric.
	// Guaranteed to be called before other methods that deal
	// with routes such as tun_builder_add_route and
	// tun_builder_reroute_gw.  Route metric is ignored
	// if < 0.
	virtual bool tun_builder_set_route_metric_default(int metric) EDDIE_OVERRIDE;

	// Callback to reroute default gateway to VPN interface.
	// ipv4 is true if the default route to be added should be IPv4.
	// ipv6 is true if the default route to be added should be IPv6.
	// flags are defined in RGWFlags (rgwflags.hpp).
	// Never called more than once per tun_builder session.
	virtual bool tun_builder_reroute_gw(bool ipv4, bool ipv6, unsigned int flags) EDDIE_OVERRIDE;

	// Callback to add route to VPN interface
	// May be called more than once per tun_builder session
	// metric is optional and should be ignored if < 0
	virtual bool tun_builder_add_route(const std::string &address, int prefix_length, int metric, bool ipv6) EDDIE_OVERRIDE;

	// Callback to exclude route from VPN interface
	// May be called more than once per tun_builder session
	// metric is optional and should be ignored if < 0
	virtual bool tun_builder_exclude_route(const std::string &address, int prefix_length, int metric, bool ipv6) EDDIE_OVERRIDE;

	// Callback to add DNS server to VPN interface
	// May be called more than once per tun_builder session
	// If reroute_dns is true, all DNS traffic should be routed over the
	// tunnel, while if false, only DNS traffic that matches an added search
	// domain should be routed.
	// Guaranteed to be called after tun_builder_reroute_gw.
	virtual bool tun_builder_add_dns_server(const std::string &address, bool ipv6) EDDIE_OVERRIDE;

	// Callback to add search domain to DNS resolver
	// May be called more than once per tun_builder session
	// See tun_builder_add_dns_server above for description of
	// reroute_dns parameter.
	// Guaranteed to be called after tun_builder_reroute_gw.
	virtual bool tun_builder_add_search_domain(const std::string &domain) EDDIE_OVERRIDE;

	// Callback to set MTU of the VPN interface
	// Never called more than once per tun_builder session.
	virtual bool tun_builder_set_mtu(int mtu) EDDIE_OVERRIDE;

	// Callback to set the session name
	// Never called more than once per tun_builder session.
	virtual bool tun_builder_set_session_name(const std::string &name) EDDIE_OVERRIDE;

	// Callback to add a host which should bypass the proxy
	// May be called more than once per tun_builder session
	virtual bool tun_builder_add_proxy_bypass(const std::string &bypass_host) EDDIE_OVERRIDE;

	// Callback to set the proxy "Auto Config URL"
	// Never called more than once per tun_builder session.
	virtual bool tun_builder_set_proxy_auto_config_url(const std::string &url) EDDIE_OVERRIDE;

	// Callback to set the HTTP proxy
	// Never called more than once per tun_builder session.
	virtual bool tun_builder_set_proxy_http(const std::string &host, int port) EDDIE_OVERRIDE;

	// Callback to set the HTTPS proxy
	// Never called more than once per tun_builder session.
	virtual bool tun_builder_set_proxy_https(const std::string &host, int port) EDDIE_OVERRIDE;

	// Callback to add Windows WINS server to VPN interface.
	// WINS server addresses are always IPv4.
	// May be called more than once per tun_builder session.
	// Guaranteed to be called after tun_builder_reroute_gw.
	virtual bool tun_builder_add_wins_server(const std::string &address) EDDIE_OVERRIDE;

	// Optional callback that indicates whether IPv6 traffic should be
	// blocked, to prevent unencrypted IPv6 packet leakage when the
	// tunnel is IPv4-only, but the local machine has IPv6 connectivity
	// to the internet.  Enabled by "block-ipv6" config var.
	virtual bool tun_builder_set_block_ipv6(bool block_ipv6) EDDIE_OVERRIDE;

	// Optional callback to set a DNS suffix on tun/tap adapter.
	// Currently only implemented on Windows, where it will
	// set the "Connection-specific DNS Suffix" property on
	// the TAP driver.
	virtual bool tun_builder_set_adapter_domain_suffix(const std::string &name) EDDIE_OVERRIDE;

	// Callback to establish the VPN tunnel, returning a file descriptor
	// to the tunnel, which the caller will henceforth own.  Returns -1
	// if the tunnel could not be established.
	// Always called last after tun_builder session has been configured.
	virtual int tun_builder_establish() EDDIE_OVERRIDE;

	// Return true if tun interface may be persisted, i.e. rolled
	// into a new session with properties untouched.  This method
	// is only called after all other tests of persistence
	// allowability succeed, therefore it can veto persistence.
	// If persistence is ultimately enabled,
	// tun_builder_establish_lite() will be called.  Otherwise,
	// tun_builder_establish() will be called.
	virtual bool tun_builder_persist() EDDIE_OVERRIDE;

	// Indicates a reconnection with persisted tun state.
	virtual void tun_builder_establish_lite() EDDIE_OVERRIDE;

	// Indicates that tunnel is being torn down.
	// If disconnect == true, then the teardown is occurring
	// prior to final disconnect.
	virtual void tun_builder_teardown(bool disconnect) EDDIE_OVERRIDE;

public: 
	static void eddie_log_debug(const std::string &message);
	static void eddie_log_error(const std::string &message);
	static void ovpn3_log_debug(const std::string &message);
	static void ovpn3_log_error(const std::string &message);

private:
	ovpn3_client m_ci;
	OptionsMap m_options;
};

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

Client::Impl::Impl(ovpn3_client *ci)
{
	// IMPORTANT: ovpn3_client structure MUST be copied (the caller could deallocate after constructing a Client object)
	memcpy(&m_ci, ci, sizeof(ovpn3_client));

	initOptions();
}

Client::Impl::~Impl()
{
	
}

const std::string & Client::Impl::getOption(const std::string &name, const std::string &defValue) const
{
	OptionsMap::const_iterator i = m_options.find(name);
	if(i != m_options.end())
		return i->second;

	return defValue;
}

void Client::Impl::setOption(const std::string &name, const std::string &value, bool check)
{
	EDDIE_EXCEPT_IF(check && m_options.find(name) == m_options.end(), utils::format("unknown option '%s'", name.c_str()));
	m_options[name] = value;
}

template <typename T>
T Client::Impl::getOptionValue(const std::string &name) const
{
	return utils::from_string<T>(getOption(name));
}

template <typename T>
T Client::Impl::getOptionValue(const std::string &name, const T &defValue) const
{
	// Do not check for empty strings here since it could be a "good" value, only returns defValue when a key doesn't exist

	OptionsMap::const_iterator i = m_options.find(name);
	if(i != m_options.end())
		return utils::from_string<T>(i->second);

	return defValue;
}

void Client::Impl::initOptions()
{
	setOption(EDDIE_OVPN3_OPTION_TLS_VERSION_MIN, EDDIE_OVPN3_OPTION_TLS_VERSION_MIN_DEFAULT);
	setOption(EDDIE_OVPN3_OPTION_PROTOCOL, EDDIE_OVPN3_OPTION_PROTOCOL_DEFAULT);
	setOption(EDDIE_OVPN3_OPTION_IPV6, EDDIE_OVPN3_OPTION_IPV6_DEFAULT);
	setOption(EDDIE_OVPN3_OPTION_TIMEOUT, EDDIE_OVPN3_OPTION_TIMEOUT_DEFAULT);
	setOption(EDDIE_OVPN3_OPTION_TUN_PERSIST, EDDIE_OVPN3_OPTION_TUN_PERSIST_DEFAULT);
	setOption(EDDIE_OVPN3_OPTION_COMPRESSION_MODE, EDDIE_OVPN3_OPTION_COMPRESSION_MODE_DEFAULT);
	setOption(EDDIE_OVPN3_OPTION_USERNAME, EDDIE_OVPN3_OPTION_USERNAME_DEFAULT);
	setOption(EDDIE_OVPN3_OPTION_PASSWORD, EDDIE_OVPN3_OPTION_PASSWORD_DEFAULT);
	setOption(EDDIE_OVPN3_OPTION_SYNCHRONOUS_DNS_LOOKUP, EDDIE_OVPN3_OPTION_SYNCHRONOUS_DNS_LOOKUP_DEFAULT);
	setOption(EDDIE_OVPN3_OPTION_AUTOLOGIN_SESSIONS, EDDIE_OVPN3_OPTION_AUTOLOGIN_SESSIONS_DEFAULT);	
	setOption(EDDIE_OVPN3_OPTION_DISABLE_CLIENT_CERT, EDDIE_OVPN3_OPTION_DISABLE_CLIENT_CERT_DEFAULT);
	setOption(EDDIE_OVPN3_OPTION_SSL_DEBUG_LEVEL, EDDIE_OVPN3_OPTION_SSL_DEBUG_LEVEL_DEFAULT);
	setOption(EDDIE_OVPN3_OPTION_PRIVATE_KEY_PASSWORD, EDDIE_OVPN3_OPTION_PRIVATE_KEY_PASSWORD_DEFAULT);
	setOption(EDDIE_OVPN3_OPTION_DEFAULT_KEY_DIRECTION, EDDIE_OVPN3_OPTION_DEFAULT_KEY_DIRECTION_DEFAULT);
	setOption(EDDIE_OVPN3_OPTION_FORCE_AES_CBC_CIPHERSUITES, EDDIE_OVPN3_OPTION_FORCE_AES_CBC_CIPHERSUITES_DEFAULT);
	setOption(EDDIE_OVPN3_OPTION_TLS_CERT_PROFILE, EDDIE_OVPN3_OPTION_TLS_CERT_PROFILE_DEFAULT);
	setOption(EDDIE_OVPN3_OPTION_PROXY_HOST, EDDIE_OVPN3_OPTION_PROXY_HOST_DEFAULT);
	setOption(EDDIE_OVPN3_OPTION_PROXY_PORT, EDDIE_OVPN3_OPTION_PROXY_PORT_DEFAULT);
	setOption(EDDIE_OVPN3_OPTION_PROXY_USERNAME, EDDIE_OVPN3_OPTION_PROXY_USERNAME_DEFAULT);
	setOption(EDDIE_OVPN3_OPTION_PROXY_PASSWORD, EDDIE_OVPN3_OPTION_PROXY_PASSWORD_DEFAULT);
	setOption(EDDIE_OVPN3_OPTION_PROXY_ALLOW_CLEARTEXT_AUTH, EDDIE_OVPN3_OPTION_PROXY_ALLOW_CLEARTEXT_AUTH_DEFAULT);
}

void Client::Impl::applyConfig(openvpn::ClientAPI::Config &config)
{
	config.protoOverride = getOption(EDDIE_OVPN3_OPTION_PROTOCOL);
	config.connTimeout = getOptionValue<int>(EDDIE_OVPN3_OPTION_TIMEOUT);
	config.compressionMode = getOption(EDDIE_OVPN3_OPTION_COMPRESSION_MODE);
	config.ipv6 = getOption(EDDIE_OVPN3_OPTION_IPV6);
	config.privateKeyPassword = getOption(EDDIE_OVPN3_OPTION_PRIVATE_KEY_PASSWORD);
	config.tlsVersionMinOverride = getOption(EDDIE_OVPN3_OPTION_TLS_VERSION_MIN);
	config.tlsCertProfileOverride = getOption(EDDIE_OVPN3_OPTION_TLS_CERT_PROFILE);
	config.disableClientCert = getOptionValue<bool>(EDDIE_OVPN3_OPTION_DISABLE_CLIENT_CERT);
	config.proxyHost = getOption(EDDIE_OVPN3_OPTION_PROXY_HOST);
	config.proxyPort = getOption(EDDIE_OVPN3_OPTION_PROXY_PORT);
	config.proxyUsername = getOption(EDDIE_OVPN3_OPTION_PROXY_USERNAME);
	config.proxyPassword = getOption(EDDIE_OVPN3_OPTION_PROXY_PASSWORD);
	config.proxyAllowCleartextAuth = getOptionValue<bool>(EDDIE_OVPN3_OPTION_PROXY_ALLOW_CLEARTEXT_AUTH);
	config.defaultKeyDirection = getOptionValue<int>(EDDIE_OVPN3_OPTION_DEFAULT_KEY_DIRECTION);
	config.forceAesCbcCiphersuites = getOptionValue<bool>(EDDIE_OVPN3_OPTION_FORCE_AES_CBC_CIPHERSUITES);
	config.sslDebugLevel = getOptionValue<int>(EDDIE_OVPN3_OPTION_SSL_DEBUG_LEVEL);
	config.autologinSessions = getOptionValue<bool>(EDDIE_OVPN3_OPTION_AUTOLOGIN_SESSIONS);
	config.tunPersist = getOptionValue<bool>(EDDIE_OVPN3_OPTION_TUN_PERSIST);
	config.synchronousDnsLookup = getOptionValue<bool>(EDDIE_OVPN3_OPTION_SYNCHRONOUS_DNS_LOOKUP);
	
	dumpConfig(config);
}

void Client::Impl::applyCreds(openvpn::ClientAPI::ProvideCreds &creds)
{
	creds.username = getOption(EDDIE_OVPN3_OPTION_USERNAME);
	creds.password = getOption(EDDIE_OVPN3_OPTION_PASSWORD);

	dumpCreds(creds);
}

void Client::Impl::dumpConfig(openvpn::ClientAPI::Config &config)
{
	eddie_log_debug("config.protoOverride: " + config.protoOverride);
	eddie_log_debug("config.connTimeout: " + utils::to_string(config.connTimeout));
	eddie_log_debug("config.compressionMode: " + config.compressionMode);
	eddie_log_debug("config.ipv6: " + config.ipv6);
	eddie_log_debug("config.privateKeyPassword: " + config.privateKeyPassword);
	eddie_log_debug("config.tlsVersionMinOverride: " + config.tlsVersionMinOverride);
	eddie_log_debug("config.tlsCertProfileOverride: " + config.tlsCertProfileOverride);
	eddie_log_debug("config.disableClientCert: " + utils::to_string(config.disableClientCert));
	eddie_log_debug("config.proxyHost: " + config.proxyHost);
	eddie_log_debug("config.proxyPort: " + config.proxyPort);
	eddie_log_debug("config.proxyUsername: " + config.proxyUsername);
	eddie_log_debug("config.proxyPassword: " + config.proxyPassword);	 
	eddie_log_debug("config.proxyAllowCleartextAuth: " + utils::to_string(config.proxyAllowCleartextAuth));	
	eddie_log_debug("config.defaultKeyDirection: " + utils::to_string(config.defaultKeyDirection));
	eddie_log_debug("config.forceAesCbcCiphersuites: " + utils::to_string(config.forceAesCbcCiphersuites));
	eddie_log_debug("config.sslDebugLevel: " + utils::to_string(config.sslDebugLevel));
	eddie_log_debug("config.autologinSessions: " + utils::to_string(config.autologinSessions));	 
	eddie_log_debug("config.tunPersist: " + utils::to_string(config.tunPersist));
	eddie_log_debug("config.synchronousDnsLookup: " + utils::to_string(config.synchronousDnsLookup));	 
}

void Client::Impl::dumpCreds(openvpn::ClientAPI::ProvideCreds &creds)
{
	eddie_log_debug("creds.username: " + creds.username);
	eddie_log_debug("creds.password: " + creds.password);
}

void Client::Impl::log(const openvpn::ClientAPI::LogInfo &li)
{
	ovpn3_log_debug(li.text);
}

bool Client::Impl::socket_protect(int socket)
{
	if(!m_ci.socket_protect)
	{
		eddie_log_error("socket_protect unimplemented");
		return false;
	}

	eddie_log_debug(utils::format("socket_protect(socket=%d)", socket));
	return EDDIE_SUCCEEDED(m_ci.socket_protect(socket));
}

bool Client::Impl::pause_on_connection_timeout()
{
	return false;
}

void Client::Impl::event(const openvpn::ClientAPI::Event &e)
{
	std::string message = "EVENT: " + e.name;
	if(!e.info.empty())
		message += " " + e.info;

	if(e.fatal)
		message += " [FATAL-ERR]";
	else if(e.error)
		message += " [ERR]";

	if(e.fatal || e.error)
		ovpn3_log_error(message);
	else
		ovpn3_log_debug(message);
	
	if(m_ci.on_event)
	{		
		ovpn3_event ce;
		EDDIE_ZEROMEMORY(&ce, sizeof(ovpn3_event));

		if(e.fatal)
			ce.type = OVPN3_EVENT_TYPE_ERROR;
		else if(e.error)
			ce.type = OVPN3_EVENT_TYPE_WARNING;
		else
			ce.type = OVPN3_EVENT_TYPE_MESSAGE;

		ce.name = e.name.c_str();
		ce.info = e.info.c_str();
		ce.data = nullptr;

		if(e.name == "CONNECTED")
		{
			openvpn::ClientAPI::ConnectionInfo connectionInfo = connection_info();
			
			ovpn3_connection_data connection_data;
			EDDIE_ZEROMEMORY(&connection_data, sizeof(ovpn3_connection_data));
			connection_data.defined = connectionInfo.defined;
			connection_data.user = connectionInfo.user.c_str();
			connection_data.serverHost = connectionInfo.serverHost.c_str();
			connection_data.serverPort = connectionInfo.serverPort.c_str();
			connection_data.serverProto = connectionInfo.serverProto.c_str();
			connection_data.serverIp = connectionInfo.serverIp.c_str();
			connection_data.vpnIp4 = connectionInfo.vpnIp4.c_str();
			connection_data.vpnIp6 = connectionInfo.vpnIp6.c_str();
			connection_data.gw4 = connectionInfo.gw4.c_str();
			connection_data.gw6 = connectionInfo.gw6.c_str();
			connection_data.clientIp = connectionInfo.clientIp.c_str();
			connection_data.tunName = connectionInfo.tunName.c_str();

			ce.data = &connection_data;
	
			m_ci.on_event(&ce);
		}
		else
		{
			m_ci.on_event(&ce);
		}			
	}	
}

void Client::Impl::external_pki_cert_request(openvpn::ClientAPI::ExternalPKICertRequest &req)
{
	eddie_log_debug("external_pki_cert_request");
}

void Client::Impl::external_pki_sign_request(openvpn::ClientAPI::ExternalPKISignRequest &req)
{
	eddie_log_debug("external_pki_sign_request");
}

void Client::Impl::connect_attach()
{
	eddie_log_debug("connect_attach");

	BaseClass::connect_attach();

	if(m_ci.connect_attach)
		m_ci.connect_attach();
}

void Client::Impl::connect_pre_run()
{
	eddie_log_debug("connect_pre_run");

	BaseClass::connect_pre_run();

	if(m_ci.connect_pre_run)
		m_ci.connect_pre_run();
}

void Client::Impl::connect_run()
{
	eddie_log_debug("connect_run");

	BaseClass::connect_run();

	if(m_ci.connect_run)
		m_ci.connect_run();
}

void Client::Impl::connect_session_stop()
{
	eddie_log_debug("connect_session_stop");

	BaseClass::connect_session_stop();

	if(m_ci.connect_session_stop)
		m_ci.connect_session_stop();
}

bool Client::Impl::tun_builder_new()
{
	if(!m_ci.tun_builder_new)
	{
		eddie_log_error("tun_builder_new unimplemented");
		return false;
	}

	eddie_log_debug("tun_builder_new");	
	return EDDIE_SUCCEEDED(m_ci.tun_builder_new());
}

bool Client::Impl::tun_builder_set_layer(int layer)
{
	if(!m_ci.tun_builder_set_layer)
	{
		eddie_log_error("tun_builder_set_layer unimplemented");
		return false;
	}

	eddie_log_debug(utils::format("tun_builder_set_layer(layer=%d)", layer));
	return EDDIE_SUCCEEDED(m_ci.tun_builder_set_layer(layer));
}

bool Client::Impl::tun_builder_set_remote_address(const std::string &address, bool ipv6)
{
	if(!m_ci.tun_builder_set_remote_address)
	{
		eddie_log_error("tun_builder_set_remote_address unimplemented");
		return false;
	}
	
	eddie_log_debug(utils::format("tun_builder_set_remote_address(address=%s, ipv6=%s)", address.c_str(), utils::to_string(ipv6).c_str()));
	return EDDIE_SUCCEEDED(m_ci.tun_builder_set_remote_address(address.c_str(), EDDIE_FLAG_CAST(ipv6)));
}

bool Client::Impl::tun_builder_add_address(const std::string &address, int prefix_length, const std::string &gateway, /*optional*/ bool ipv6, bool net30)
{
	if(!m_ci.tun_builder_add_address)
	{
		eddie_log_error("tun_builder_add_address unimplemented");
		return false;
	}

	eddie_log_debug(utils::format("tun_builder_add_address(address=%s, prefix_length=%d, gateway=%s, ipv6=%s, net30=%s)", address.c_str(), prefix_length, gateway.c_str(), utils::to_string(ipv6).c_str(), utils::to_string(net30).c_str()));
	return EDDIE_SUCCEEDED(m_ci.tun_builder_add_address(address.c_str(), prefix_length, gateway.c_str(), EDDIE_FLAG_CAST(ipv6), EDDIE_FLAG_CAST(net30)));
}

bool Client::Impl::tun_builder_set_route_metric_default(int metric)
{
	if(!m_ci.tun_builder_set_route_metric_default)
	{
		eddie_log_error("tun_builder_set_route_metric_default unimplemented");
		return false;
	}

	eddie_log_debug(utils::format("tun_builder_set_route_metric_default(metric=%d)", metric));
	return EDDIE_SUCCEEDED(m_ci.tun_builder_set_route_metric_default(metric));
}

bool Client::Impl::tun_builder_reroute_gw(bool ipv4, bool ipv6, unsigned int flags)
{
	if(!m_ci.tun_builder_reroute_gw)
	{
		eddie_log_error("tun_builder_reroute_gw unimplemented");
		return false;
	}

	eddie_log_debug(utils::format("tun_builder_reroute_gw(ipv4=%s, ipv6=%s, flags=%d)", utils::to_string(ipv4).c_str(), utils::to_string(ipv6).c_str(), flags));
	return EDDIE_SUCCEEDED(m_ci.tun_builder_reroute_gw(EDDIE_FLAG_CAST(ipv4), EDDIE_FLAG_CAST(ipv6), flags));
}

bool Client::Impl::tun_builder_add_route(const std::string &address, int prefix_length, int metric, bool ipv6)
{
	if(!m_ci.tun_builder_add_route)
	{
		eddie_log_error("tun_builder_add_route unimplemented");
		return false;
	}	

	eddie_log_debug(utils::format("tun_builder_add_route(address=%s, prefix_length=%d, metric=%d, ipv6=%s)", address.c_str(), prefix_length, metric, utils::to_string(ipv6).c_str()));
	return EDDIE_SUCCEEDED(m_ci.tun_builder_add_route(address.c_str(), prefix_length, metric, EDDIE_FLAG_CAST(ipv6)));
}

bool Client::Impl::tun_builder_exclude_route(const std::string &address, int prefix_length, int metric, bool ipv6)
{
	if(!m_ci.tun_builder_exclude_route)
	{
		eddie_log_error("tun_builder_exclude_route unimplemented");
		return false;
	}

	eddie_log_debug(utils::format("tun_builder_exclude_route(address=%s, prefix_length=%d, metric=%d, ipv6=%s)", address.c_str(), prefix_length, metric, utils::to_string(ipv6).c_str()));
	return EDDIE_SUCCEEDED(m_ci.tun_builder_exclude_route(address.c_str(), prefix_length, metric, EDDIE_FLAG_CAST(ipv6)));
}

bool Client::Impl::tun_builder_add_dns_server(const std::string &address, bool ipv6)
{
	if(!m_ci.tun_builder_add_dns_server)
	{
		eddie_log_error("tun_builder_add_dns_server unimplemented");
		return false;
	}

	eddie_log_debug(utils::format("tun_builder_add_dns_server(address=%s, ipv6=%s)", address.c_str(), utils::to_string(ipv6).c_str()));
	return EDDIE_SUCCEEDED(m_ci.tun_builder_add_dns_server(address.c_str(), EDDIE_FLAG_CAST(ipv6)));
}

bool Client::Impl::tun_builder_add_search_domain(const std::string &domain)
{
	if(!m_ci.tun_builder_add_search_domain)
	{
		eddie_log_error("tun_builder_add_search_domain unimplemented");
		return false;
	}

	eddie_log_debug(utils::format("tun_builder_add_search_domain(domain=%s)", domain.c_str()));
	return EDDIE_SUCCEEDED(m_ci.tun_builder_add_search_domain(domain.c_str()));
}

bool Client::Impl::tun_builder_set_mtu(int mtu)
{
	if(!m_ci.tun_builder_set_mtu)
	{
		eddie_log_error("tun_builder_set_mtu unimplemented");
		return false;
	}

	eddie_log_debug(utils::format("tun_builder_set_mtu(mtu=%d)", mtu));
	return EDDIE_SUCCEEDED(m_ci.tun_builder_set_mtu(mtu));
}

bool Client::Impl::tun_builder_set_session_name(const std::string &name)
{
	if(!m_ci.tun_builder_set_session_name)
	{
		eddie_log_error("tun_builder_set_session_name unimplemented");
		return false;
	}

	eddie_log_debug(utils::format("tun_builder_set_session_name(name=%s)", name.c_str()));
	return EDDIE_SUCCEEDED(m_ci.tun_builder_set_session_name(name.c_str()));
}

bool Client::Impl::tun_builder_add_proxy_bypass(const std::string &bypass_host)
{
	if(!m_ci.tun_builder_add_proxy_bypass)
	{
		eddie_log_error("tun_builder_add_proxy_bypass unimplemented");
		return false;
	}

	eddie_log_debug(utils::format("tun_builder_add_proxy_bypass(bypass_host=%s)", bypass_host.c_str()));
	return EDDIE_SUCCEEDED(m_ci.tun_builder_add_proxy_bypass(bypass_host.c_str()));
}

bool Client::Impl::tun_builder_set_proxy_auto_config_url(const std::string &url)
{
	if(!m_ci.tun_builder_set_proxy_auto_config_url)
	{
		eddie_log_error("tun_builder_set_proxy_auto_config_url unimplemented");
		return false;
	}

	eddie_log_debug(utils::format("tun_builder_set_proxy_auto_config_url(url=%s)", url.c_str()));
	return EDDIE_SUCCEEDED(m_ci.tun_builder_set_proxy_auto_config_url(url.c_str()));
}

bool Client::Impl::tun_builder_set_proxy_http(const std::string &host, int port)
{
	if(!m_ci.tun_builder_set_proxy_http)
	{
		eddie_log_error("tun_builder_set_proxy_http unimplemented");
		return false;
	}

	eddie_log_debug(utils::format("tun_builder_set_proxy_http(host=%s, port=%d)", host.c_str(), port));
	return EDDIE_SUCCEEDED(m_ci.tun_builder_set_proxy_http(host.c_str(), port));
}

bool Client::Impl::tun_builder_set_proxy_https(const std::string &host, int port)
{
	if(!m_ci.tun_builder_set_proxy_https)
	{
		eddie_log_error("tun_builder_set_proxy_https unimplemented");
		return false;
	}

	eddie_log_debug(utils::format("tun_builder_set_proxy_https(host=%s, port=%d)", host.c_str(), port));
	return EDDIE_SUCCEEDED(m_ci.tun_builder_set_proxy_https(host.c_str(), port));
}

bool Client::Impl::tun_builder_add_wins_server(const std::string &address)
{
	if(!m_ci.tun_builder_add_wins_server)
	{
		eddie_log_error("tun_builder_add_wins_server unimplemented");
		return false;
	}

	eddie_log_debug(utils::format("tun_builder_add_wins_server(address=%s)", address.c_str()));
	return EDDIE_SUCCEEDED(m_ci.tun_builder_add_wins_server(address.c_str()));
}

bool Client::Impl::tun_builder_set_block_ipv6(bool block_ipv6)
{
	if(!m_ci.tun_builder_set_block_ipv6)
	{
		eddie_log_error("tun_builder_set_block_ipv6 unimplemented");
		return false;
	}

	eddie_log_debug(utils::format("tun_builder_set_block_ipv6(block_ipv6=%s)", utils::to_string(block_ipv6).c_str()));
	return EDDIE_SUCCEEDED(m_ci.tun_builder_set_block_ipv6(EDDIE_FLAG_CAST(block_ipv6)));
}

bool Client::Impl::tun_builder_set_adapter_domain_suffix(const std::string &name)
{
	if(!m_ci.tun_builder_set_adapter_domain_suffix)
	{
		eddie_log_error("tun_builder_set_adapter_domain_suffix unimplemented");
		return false;
	}

	eddie_log_debug(utils::format("tun_builder_set_adapter_domain_suffix(name=%s)", name.c_str()));
	return EDDIE_SUCCEEDED(m_ci.tun_builder_set_adapter_domain_suffix(name.c_str()));
}

int Client::Impl::tun_builder_establish()
{
	if(!m_ci.tun_builder_establish)
	{
		eddie_log_error("tun_builder_establish unimplemented");
		return false;
	}

	eddie_log_debug("tun_builder_establish");
	return m_ci.tun_builder_establish();
}

bool Client::Impl::tun_builder_persist()
{
	if(!m_ci.tun_builder_persist)
	{
		eddie_log_error("tun_builder_persist unimplemented");
		return false;
	}

	eddie_log_debug("tun_builder_persist");
	return EDDIE_SUCCEEDED(m_ci.tun_builder_persist());
}

void Client::Impl::tun_builder_establish_lite()
{
	if(!m_ci.tun_builder_establish_lite)
	{
		eddie_log_error("tun_builder_establish_lite unimplemented");
		return;
	}

	eddie_log_debug("tun_builder_establish_lite");
	m_ci.tun_builder_establish_lite();
}

void Client::Impl::tun_builder_teardown(bool disconnect) 
{
	if(!m_ci.tun_builder_teardown)
	{
		eddie_log_error("tun_builder_teardown unimplemented");
		return;
	}

	eddie_log_debug(utils::format("tun_builder_teardown(disconnect=%s)", utils::to_string(disconnect).c_str()));
	m_ci.tun_builder_teardown(EDDIE_FLAG_CAST(disconnect));
}

void Client::Impl::eddie_log_debug(const std::string &message)
{
	utils::log_debug("EDDIE: " + message);
}

void Client::Impl::eddie_log_error(const std::string &message)
{
	utils::log_error("EDDIE: " + message);
}

void Client::Impl::ovpn3_log_debug(const std::string &message)
{
	utils::log_debug("OVPN3: " + message);
}

void Client::Impl::ovpn3_log_error(const std::string &message)
{
	utils::log_error("OVPN3: " + message);
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

Client::Client(ovpn3_client *ci) : m_impl(new Impl(ci))
{

}

Client::~Client()
{

}

void Client::getTransportStats(ovpn3_transport_stats &stats) const
{
	EDDIE_ZEROMEMORY(&stats, sizeof(ovpn3_transport_stats));

	openvpn::ClientAPI::TransportStats ts = m_impl->transport_stats();
	stats.bytes_in = ts.bytesIn;
	stats.bytes_out = ts.bytesOut;
	stats.packets_in = ts.packetsIn;
	stats.packets_out = ts.packetsOut;
	stats.last_packet_received = ts.lastPacketReceived;
}

void Client::setOption(const std::string &name, const std::string &value)
{
	m_impl->setOption(name, value, true);
}

void Client::init()
{
	Impl::init_process();
}

void Client::cleanup()
{
	Impl::uninit_process();
}

void Client::loadProfileFile(const std::string &filename)
{
	openvpn::ProfileMerge pm(filename.c_str(), "ovpn", "", openvpn::ProfileMerge::FOLLOW_FULL, openvpn::ProfileParseLimits::MAX_LINE_SIZE, openvpn::ProfileParseLimits::MAX_PROFILE_SIZE);
	EDDIE_EXCEPT_IF(pm.status() != openvpn::ProfileMerge::MERGE_SUCCESS, utils::format("merge config error: '%s' ('%s')", pm.status_string(), pm.error().c_str()));
	loadProfileString(pm.profile_content());	
}

void Client::loadProfileString(const std::string &str)
{
	std::string profile = utils::trim_copy(str);

	EDDIE_EXCEPT_IF(profile.empty(), "empty profile");

	//Impl::eddie_log_debug("profile: " + profile);

	m_profiles.push_back(profile);
}

void Client::start()
{
	Impl::eddie_log_debug("loading profile(s)...");

	applyProfiles();

	Impl::eddie_log_debug("profile(s) loaded, connecting...");

	openvpn::ClientAPI::Status connectStatus = m_impl->connect();
	if(connectStatus.error)
	{
		std::string message = "connect error";
		if(!connectStatus.status.empty())
			message += " (" + connectStatus.status + ")";

		EDDIE_EXCEPT(message);
	}
	else
	{
		Impl::eddie_log_debug("ovpn3 client started...");
	}
}

void Client::stop()
{
	m_impl->stop();
}

void Client::pause(const std::string &reason)
{
	m_impl->pause(reason);
}

void Client::resume()
{
	m_impl->resume();
}

void Client::finalize()
{

}

void Client::applyProfiles()
{
	EDDIE_EXCEPT_IF(m_profiles.empty(), "no profile loaded");

	// Default params

	//std::string peer_info;
	//bool eval = false;
	//bool self_test = false;
	//bool merge = false;
	//bool version = false;
	std::string epki_cert_fn;
	//std::string epki_ca_fn;
	//std::string epki_key_fn;

	// Create the config from the loaded profiles

	openvpn::ClientAPI::Config config;
	config.guiVersion = EDDIE_OVPN3_GUI_VERSION;
	config.info = true;
	config.altProxy = false;
	config.dco = false;
	config.googleDnsFallback = false;
	//config.serverOverride = server;
	//config.gremlinConfig = gremlin;

	for(std::list<std::string>::const_iterator i = m_profiles.begin(); i != m_profiles.end(); ++i)
	{
		if(i != m_profiles.begin())
			config.content += "\n";

		config.content += *i;
	}			
	
	m_impl->applyConfig(config);

	if(!epki_cert_fn.empty())
		config.externalPkiAlias = "epki"; // dummy	

	openvpn::ClientAPI::EvalConfig eval = m_impl->eval_config(config);
	EDDIE_EXCEPT_IF(eval.error, "eval config error (" + eval.message + ")");

	if(!eval.autologin)
	{
		Impl::eddie_log_debug("loading creds");

		openvpn::ClientAPI::ProvideCreds creds;
		/*		
		if(password.empty() && dynamicChallengeCookie.empty())
			password = get_password("Password:");
		creds.username = username;
		creds.password = password;
		*/		
		creds.response = "";
		creds.dynamicChallengeCookie = "";
		creds.replacePasswordWithSessionID = true;
		creds.cachePassword = false;

		m_impl->applyCreds(creds);

		EDDIE_EXCEPT_IF(creds.username.empty() || creds.password.empty(), "creds required");

		openvpn::ClientAPI::Status creds_status = m_impl->provide_creds(creds);
		EDDIE_EXCEPT_IF(creds_status.error, "creds error (" + creds_status.message + ")");
	}

	// external PKI
	if(!epki_cert_fn.empty())
	{
		/*
		client.epki_cert = read_text_utf8(epki_cert_fn);
		if(!epki_ca_fn.empty())
			client.epki_ca = read_text_utf8(epki_ca_fn);
#if defined(USE_MBEDTLS)
		if(!epki_key_fn.empty())
		{
			const std::string epki_key_txt = read_text_utf8(epki_key_fn);
			client.epki_ctx.parse(epki_key_txt, "EPKI", privateKeyPassword);
		}
		else
			OPENVPN_THROW_EXCEPTION("--epki-key must be specified");
#endif
		*/
	}
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

EDDIE_NAMESPACE_END()

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
