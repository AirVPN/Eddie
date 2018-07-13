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

#ifndef EDDIE_ANDROID_NATIVE_OPTIONS_H
#define EDDIE_ANDROID_NATIVE_OPTIONS_H

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

/*
Override the minimum TLS version

disabled:	Disabled
default:	Default
tls_1_0:	TLS 1.0
tls_1_1:	TLS 1.1
tls_1_2:	TLS 1.2
*/
#define EDDIE_OVPN3_OPTION_TLS_VERSION_MIN						"tls_version_min"
#define EDDIE_OVPN3_OPTION_TLS_VERSION_MIN_DEFAULT				"tls_1_0"

/*
Force a given transport protocol

"":			Default
adaptive:	Adaptive
tcp:		TCP
udp:		UDP
*/
#define EDDIE_OVPN3_OPTION_PROTOCOL								"protocol"
#define EDDIE_OVPN3_OPTION_PROTOCOL_DEFAULT						""

/*
IPv6 preference

"":			Default (leave decision to server)
no:			disable IPv6, so tunnel will be IPv4-only
yes:		request combined IPv4/IPv6 tunnel
*/
#define EDDIE_OVPN3_OPTION_IPV6									"ipv6"
#define EDDIE_OVPN3_OPTION_IPV6_DEFAULT							""

/*
Connection timeout in seconds

10 sec:		"10"
30 sec:		"30"
1 min:		"60"
2 min:		"120"
Infinite:	"0"
*/
#define EDDIE_OVPN3_OPTION_TIMEOUT								"timeout"
#define EDDIE_OVPN3_OPTION_TIMEOUT_DEFAULT						"60"

// Keep tun interface active during pauses or reconnections
#define EDDIE_OVPN3_OPTION_TUN_PERSIST							"tun_persist"
#define EDDIE_OVPN3_OPTION_TUN_PERSIST_DEFAULT					"true"

/*
Compression mode

yes:		allow compression on both uplink and downlink
asym:		allow compression on downlink only (i.e. server -> client)
no or "":	support compression stubs only
*/
#define EDDIE_OVPN3_OPTION_COMPRESSION_MODE						"compression_mode"
#define EDDIE_OVPN3_OPTION_COMPRESSION_MODE_DEFAULT				"yes"

// Auth username
#define EDDIE_OVPN3_OPTION_USERNAME								"username"
#define EDDIE_OVPN3_OPTION_USERNAME_DEFAULT						""

// Auth password
#define EDDIE_OVPN3_OPTION_PASSWORD								"password"
#define EDDIE_OVPN3_OPTION_PASSWORD_DEFAULT						""

// If true, do synchronous DNS lookup.
#define EDDIE_OVPN3_OPTION_SYNCHRONOUS_DNS_LOOKUP				"synchronous_dns_lookup"
#define EDDIE_OVPN3_OPTION_SYNCHRONOUS_DNS_LOOKUP_DEFAULT		"false"

// Enable autologin sessions
#define EDDIE_OVPN3_OPTION_AUTOLOGIN_SESSIONS					"autologin_sessions"
#define EDDIE_OVPN3_OPTION_AUTOLOGIN_SESSIONS_DEFAULT			"true"

// If true, don't send client cert/key to peer.
#define EDDIE_OVPN3_OPTION_DISABLE_CLIENT_CERT					"disable_client_cert"
#define EDDIE_OVPN3_OPTION_DISABLE_CLIENT_CERT_DEFAULT			"false"

// SSL library debug level
#define EDDIE_OVPN3_OPTION_SSL_DEBUG_LEVEL						"ssl_debug_level"
#define EDDIE_OVPN3_OPTION_SSL_DEBUG_LEVEL_DEFAULT				"0"

// Private key password (optional)
#define EDDIE_OVPN3_OPTION_PRIVATE_KEY_PASSWORD					"private_key_password"
#define EDDIE_OVPN3_OPTION_PRIVATE_KEY_PASSWORD_DEFAULT			""

/*
Default key direction parameter for tls-auth (0, 1, or -1 for bidirectional/default) if no key-direction parameter defined in profile.  
Generally should be -1 (bidirectional) for compatibility with 2.x branch
*/
#define EDDIE_OVPN3_OPTION_DEFAULT_KEY_DIRECTION				"default_key_direction"
#define EDDIE_OVPN3_OPTION_DEFAULT_KEY_DIRECTION_DEFAULT		"-1"

/*
If true, force ciphersuite to be one of:
	1. TLS_DHE_RSA_WITH_AES_256_CBC_SHA, or
	2. TLS_DHE_RSA_WITH_AES_128_CBC_SHA
and disable setting TLS minimum version (this is intended for compatibility with legacy systems).
*/
#define EDDIE_OVPN3_OPTION_FORCE_AES_CBC_CIPHERSUITES			"force_aes_cbc_ciphersuites"
#define EDDIE_OVPN3_OPTION_FORCE_AES_CBC_CIPHERSUITES_DEFAULT	"false"

/*
default or "":		use profile default
legacy:				allow 1024-bit RSA certs signed with SHA1
preferred:			require at least 2048-bit RSA certs signed with SHA256 or higher
suiteb:				require NSA Suite-B
legacy-default:		use legacy as the default if profile doesn't specify tls-cert-profile
preferred-default:	use preferred as the default if profile doesn't specify tls-cert-profile
*/
#define EDDIE_OVPN3_OPTION_TLS_CERT_PROFILE						"tls_cert_profile"
#define EDDIE_OVPN3_OPTION_TLS_CERT_PROFILE_DEFAULT				""

#define EDDIE_OVPN3_OPTION_PROXY_HOST							"proxy_host"
#define EDDIE_OVPN3_OPTION_PROXY_HOST_DEFAULT					""							

#define EDDIE_OVPN3_OPTION_PROXY_PORT							"proxy_port"
#define EDDIE_OVPN3_OPTION_PROXY_PORT_DEFAULT					""				

#define EDDIE_OVPN3_OPTION_PROXY_USERNAME						"proxy_username"
#define EDDIE_OVPN3_OPTION_PROXY_USERNAME_DEFAULT				""

#define EDDIE_OVPN3_OPTION_PROXY_PASSWORD						"proxy_password"
#define EDDIE_OVPN3_OPTION_PROXY_PASSWORD_DEFAULT				""						

/*
enables HTTP Basic auth
*/
#define EDDIE_OVPN3_OPTION_PROXY_ALLOW_CLEARTEXT_AUTH			"proxy_allow_cleartext_auth"
#define EDDIE_OVPN3_OPTION_PROXY_ALLOW_CLEARTEXT_AUTH_DEFAULT	"false"

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#endif // EDDIE_ANDROID_NATIVE_OPTIONS_H
