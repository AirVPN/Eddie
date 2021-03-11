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
using System.Xml;
using System.Text;
using Eddie.Core;

namespace Eddie.Core.ConnectionTypes
{
	public class Hummingbird : OpenVPN
	{
        public static string TranslateMessage(string str)
        {
            if (str == "ERROR: NETWORK_RECV_ERROR") return "ERROR: errors receiving on network socket";
            else if (str == "ERROR: NETWORK_EOF_ERROR") return "ERROR: EOF received on TCP network socket";
            else if (str == "ERROR: NETWORK_SEND_ERROR") return "ERROR: errors sending on network socket";
            else if (str == "ERROR: NETWORK_UNAVAILABLE") return "ERROR: network unavailable";
            else if (str == "ERROR: DECRYPT_ERROR") return "ERROR: data channel encrypt/decrypt error";
            else if (str == "ERROR: HMAC_ERROR") return "ERROR: HMAC verification failure";
            else if (str == "ERROR: REPLAY_ERROR") return "ERROR: error from PacketIDReceive";
            else if (str == "ERROR: BUFFER_ERROR") return "ERROR: exception thrown in Buffer methods";
            else if (str == "ERROR: CC_ERROR") return "ERROR: general control channel errors";
            else if (str == "ERROR: BAD_SRC_ADDR") return "ERROR: packet from unknown source address";
            else if (str == "ERROR: COMPRESS_ERROR") return "ERROR: compress/decompress errors on data channel";
            else if (str == "ERROR: RESOLVE_ERROR") return "ERROR: DNS resolution error";
            else if (str == "ERROR: SOCKET_PROTECT_ERROR") return "ERROR: Error calling protect() method on socket";
            else if (str == "ERROR: TUN_READ_ERROR") return "ERROR: read errors on tun/tap interface";
            else if (str == "ERROR: TUN_WRITE_ERROR") return "ERROR: write errors on tun/tap interface";
            else if (str == "ERROR: TUN_FRAMING_ERROR") return "ERROR: error with tun PF_INET/PF_INET6 prefix";
            else if (str == "ERROR: TUN_SETUP_FAILED") return "ERROR: error setting up tun/tap interface";
            else if (str == "ERROR: TUN_IFACE_CREATE") return "ERROR: error creating tun/tap interface";
            else if (str == "ERROR: TUN_IFACE_DISABLED") return "ERROR: tun/tap interface is disabled";
            else if (str == "ERROR: TUN_ERROR") return "ERROR: general tun error";
            else if (str == "ERROR: TUN_REGISTER_RINGS_ERROR") return "ERROR: error registering ring buffers with wintun";
            else if (str == "ERROR: TAP_NOT_SUPPORTED") return "ERROR: dev tap is present in profile but not supported";
            else if (str == "ERROR: REROUTE_GW_NO_DNS") return "ERROR: redirect-gateway specified without alt DNS servers";
            else if (str == "ERROR: TRANSPORT_ERROR") return "ERROR: general transport error";
            else if (str == "ERROR: TCP_OVERFLOW") return "ERROR: TCP output queue overflow";
            else if (str == "ERROR: TCP_SIZE_ERROR") return "ERROR: bad embedded uint16_t TCP packet size";
            else if (str == "ERROR: TCP_CONNECT_ERROR") return "ERROR: client error on TCP connect";
            else if (str == "ERROR: UDP_CONNECT_ERROR") return "ERROR: client error on UDP connect";
            else if (str == "ERROR: SSL_ERROR") return "ERROR: errors resulting from read/write on SSL object";
            else if (str == "ERROR: SSL_PARTIAL_WRITE") return "ERROR: SSL object did not process all written cleartext";
            else if (str == "ERROR: SSL_CA_MD_TOO_WEAK") return "ERROR: CA message digest is too weak";
            else if (str == "ERROR: SSL_CA_KEY_TOO_SMALL") return "ERROR: CA key is too small";
            else if (str == "ERROR: SSL_DH_KEY_TOO_SMALL") return "ERROR: DH key is too small";
            else if (str == "ERROR: ENCAPSULATION_ERROR") return "ERROR: exceptions thrown during packet encapsulation";
            else if (str == "ERROR: EPKI_CERT_ERROR") return "ERROR: error obtaining certificate from External PKI provider";
            else if (str == "ERROR: EPKI_SIGN_ERROR") return "ERROR: error obtaining RSA signature from External PKI provider";
            else if (str == "ERROR: HANDSHAKE_TIMEOUT") return "ERROR: handshake failed to complete within given time frame";
            else if (str == "ERROR: KEEPALIVE_TIMEOUT") return "ERROR: lost contact with peer";
            else if (str == "ERROR: INACTIVE_TIMEOUT") return "ERROR: disconnected due to inactive timer";
            else if (str == "ERROR: CONNECTION_TIMEOUT") return "ERROR: connection failed to establish within given time";
            else if (str == "ERROR: PRIMARY_EXPIRE") return "ERROR: primary key context expired";
            else if (str == "ERROR: TLS_VERSION_MIN") return "ERROR: peer cannot handshake at our minimum required TLS version";
            else if (str == "ERROR: TLS_AUTH_FAIL") return "ERROR: tls-auth HMAC verification failed";
            else if (str == "ERROR: TLS_CRYPT_META_FAIL") return "ERROR: tls-crypt-v2 metadata verification failed";
            else if (str == "ERROR: CERT_VERIFY_FAIL") return "ERROR: peer certificate verification failure";
            else if (str == "ERROR: PEM_PASSWORD_FAIL") return "ERROR: incorrect or missing PEM private key decryption password";
            else if (str == "ERROR: AUTH_FAILED") return "ERROR: general authentication failure";
            else if (str == "ERROR: CLIENT_HALT") return "ERROR: HALT message from server received";
            else if (str == "ERROR: CLIENT_RESTART") return "ERROR: RESTART message from server received";
            else if (str == "ERROR: TUN_HALT") return "ERROR: halt command from tun interface";
            else if (str == "ERROR: RELAY") return "ERROR: RELAY message from server received";
            else if (str == "ERROR: RELAY_ERROR") return "ERROR: RELAY error";
            else if (str == "ERROR: N_PAUSE") return "ERROR: Number of transitions to Pause state";
            else if (str == "ERROR: N_RECONNECT") return "ERROR: Number of reconnections";
            else if (str == "ERROR: N_KEY_LIMIT_RENEG") return "ERROR: Number of renegotiations triggered by per-key limits such as data or packet limits";
            else if (str == "ERROR: KEY_STATE_ERROR") return "ERROR: Received packet didn't match expected key state";
            else if (str == "ERROR: PROXY_ERROR") return "ERROR: HTTP proxy error";
            else if (str == "ERROR: PROXY_NEED_CREDS") return "ERROR: HTTP proxy needs credentials";
            else
                return str;
        }
    }
}
