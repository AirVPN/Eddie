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

#ifndef EDDIE_ANDROID_NATIVE_API_H
#define EDDIE_ANDROID_NATIVE_API_H

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#define OVPN3_EVENT_TYPE_MESSAGE		0
#define OVPN3_EVENT_TYPE_WARNING		1
#define OVPN3_EVENT_TYPE_ERROR			2

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifdef __cplusplus
extern "C" {
#endif

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

typedef struct ovpn3_event_t
{
	int type;				// OVPN3_EVENT_TYPE_*
	const char *name;		// event name
	const char *info;		// additional event infomations
	const void *data;		// event payload (if any)
} ovpn3_event;

typedef struct ovpn3_connection_data_t
{
	int defined;
	const char *user;
	const char *serverHost;
	const char *serverPort;
	const char *serverProto;
	const char *serverIp;
	const char *vpnIp4;
	const char *vpnIp6;
	const char *gw4;
	const char *gw6;
	const char *clientIp;
	const char *tunName;
} ovpn3_connection_data;

typedef struct ovpn3_transport_stats_t
{
	long long bytes_in;
	long long bytes_out;
	long long packets_in;
	long long packets_out;
	int last_packet_received;			// number of binary milliseconds (1/1024th of a second) since last packet was received, or -1 if undefined
} ovpn3_transport_stats;

typedef struct ovpn3_client_t
{
	// Client callbacks

	int (* socket_protect)(int socket);
	void (* on_event)(ovpn3_event *e);

	// TUN callbacks

	int (* tun_builder_new)();
	int (* tun_builder_set_layer)(int layer);	
	int (* tun_builder_set_remote_address)(const char *address, int ipv6);
	int (* tun_builder_add_address)(const char *address, int prefix_length, const char *gateway, int ipv6, int net30);
	int (* tun_builder_set_route_metric_default)(int metric);
	int (* tun_builder_reroute_gw)(int ipv4, int ipv6, unsigned int flags);
	int (* tun_builder_add_route)(const char *address, int prefix_length, int metric, int ipv6);
	int (* tun_builder_exclude_route)(const char *address, int prefix_length, int metric, int ipv6);
	int (* tun_builder_add_dns_server)(const char *address, int ipv6);
	int (* tun_builder_add_search_domain)(const char *domain);
	int (* tun_builder_set_mtu)(int mtu);
	int (* tun_builder_set_session_name)(const char *name);
	int (* tun_builder_add_proxy_bypass)(const char *bypass_host);
	int (* tun_builder_set_proxy_auto_config_url)(const char *url);
	int (* tun_builder_set_proxy_http)(const char *host, int port);
	int (* tun_builder_set_proxy_https)(const char *host, int port);
	int (* tun_builder_add_wins_server)(const char *address);
	int (* tun_builder_set_block_ipv6)(int block_ipv6);
	int (* tun_builder_set_adapter_domain_suffix)(const char *name);
	int (* tun_builder_establish)();
	int (* tun_builder_persist)();
	void (* tun_builder_establish_lite)();
	void (* tun_builder_teardown)(int disconnect);

	// Connection callbacks

	void (* connect_attach)();
	void (* connect_pre_run)();
	void (* connect_run)();
	void (* connect_session_stop)();
} ovpn3_client;

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

int ovpn3_version();

int ovpn3_init();
int ovpn3_cleanup();

int ovpn3_client_create(ovpn3_client *ci);
int ovpn3_client_destroy(int client);
int ovpn3_client_start(int client);
int ovpn3_client_stop(int client);
int ovpn3_client_pause(int client, const char *reason);
int ovpn3_client_resume(int client);
int ovpn3_client_get_transport_stats(int client, ovpn3_transport_stats *stats);
int ovpn3_client_load_profile_file(int client, const char *filename);
int ovpn3_client_load_profile_string(int client, const char *str);
int ovpn3_client_set_option(int client, const char *option, const char *value);

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifdef __cplusplus
}
#endif

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#endif // EDDIE_ANDROID_NATIVE_API_H

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
