// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2023 AirVPN (support@airvpn.org) / https://airvpn.org
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

#include "ping.h"

// Windows implementation incomplete. Eddie still use the Net Framework Ping.

#if defined(WIN32) || defined(_WIN32) || defined(__WIN32__) || defined(__NT__)

#define EDDIE_PING_PLATFORM_WINDOWS
#include <winsock2.h>
#include <iphlpapi.h>
#include <icmpapi.h>
#include <stdio.h>
#include "Windows.h"
#include "ws2tcpip.h"
#include <mswsock.h>  /* WSARecvMsg() */

struct icmp {
	uint8_t icmp_type;
	uint8_t icmp_code;
	uint16_t icmp_cksum;
	uint16_t icmp_id;
	uint16_t icmp_seq;
};
#define ICMP_ECHO 8
static LPFN_WSARECVMSG WSARecvMsg;

#else

#define EDDIE_PING_PLATFORM_UNIX

#endif

void PingEngine::Start()
{
	// Init v4
	m_socket4 = SocketCreate(AF_INET, SOCK_RAW, IPPROTO_ICMP);
	if (m_socket4 != -1)
	{
		if (SocketSetHdrIncl(m_socket4) == false)
		{
			SocketClose(m_socket4);
			m_socket4 = -1;
		}
	}
	if (m_socket4 != -1)
	{
		if (SocketSetNonBlock(m_socket4) == false)
		{
			SocketClose(m_socket4);
			m_socket4 = -1;
		}
	}

	// Init v6
	m_socket6 = SocketCreate(AF_INET6, SOCK_RAW, IPPROTO_ICMPV6);
	if (m_socket6 != -1)
	{
		if (SocketSetNonBlock(m_socket6) == false)
		{
			SocketClose(m_socket6);
			m_socket6 = -1;
		}
	}

#ifdef EDDIE_PING_PLATFORM_WINDOWS
	int error;
	GUID recvmsg_id = WSAID_WSARECVMSG;
	DWORD size;

	// Obtain a pointer to the WSARecvMsg (recvmsg) function.
	error = WSAIoctl(m_socket4,
		SIO_GET_EXTENSION_FUNCTION_POINTER,
		&recvmsg_id,
		sizeof(recvmsg_id),
		&WSARecvMsg,
		sizeof(WSARecvMsg),
		&size,
		NULL,
		NULL);
	if (error == SOCKET_ERROR) {
		exit(EXIT_FAILURE);
	}
#endif
}

void PingEngine::Stop()
{
	m_mutex.lock();

	for (std::map<uint16_t, PingRequest*>::iterator it = m_requests.begin(); it != m_requests.end(); it++)
		delete it->second;
	m_requests.clear();

	if (m_socket4 != -1)
	{
		SocketClose(m_socket4);
		m_socket4 = -1;
	}

	if (m_socket6 != -1)
	{
		SocketClose(m_socket6);
		m_socket6 = -1;
	}

	m_mutex.unlock();
}

int PingEngine::Check()
{
	int nPending = 0;

	m_mutex.lock();

	char responseBuffer[PING_BUFFER_SIZE];
	memset(responseBuffer, 0, PING_BUFFER_SIZE);

	uint64_t now = GetTimestampUnixUsec();

	for (std::map<uint16_t, PingRequest*>::iterator it = m_requests.begin(); it != m_requests.end(); it++)
	{
		nPending++;

		uint16_t requestId = it->first;
		PingRequest* pR = it->second;

		if (pR->m_startTime == 0)
		{
			bool isV4 = pR->m_ip.find('.') != std::string::npos;
			bool isV6 = pR->m_ip.find(':') != std::string::npos;

			pR->m_startTime = now;

			if (isV4)
			{
				struct sockaddr_in requestAddress;
				memset(&requestAddress, 0, sizeof(requestAddress));
				requestAddress.sin_family = AF_INET;
				requestAddress.sin_port = 0;

				if (inet_pton(AF_INET, pR->m_ip.c_str(), &requestAddress.sin_addr) > 0)
				{
#ifdef EDDIE_PING_PLATFORM_WINDOWS
					// TODO
					/*
					struct icmp requestHeader;
					memset(&requestHeader, 0, sizeof(struct icmp));
					requestHeader.icmp_type = ICMP_ECHO;
					requestHeader.icmp_cksum = ChecksumIpAddress((const uint16_t*)(&requestHeader), sizeof(struct icmp));
					requestHeader.icmp_id = htons(requestId);
					requestHeader.icmp_seq = 0;

					sendto(m_socket4, (const char*)&requestHeader, sizeof(requestHeader), 0, (const struct sockaddr*)&requestAddress, (socklen_t)sizeof(requestAddress));
					*/
#endif

#ifdef EDDIE_PING_PLATFORM_UNIX								
					struct icmp requestHeader;
					PING_ZEROMEMORY(&requestHeader, sizeof(struct icmp));
					requestHeader.icmp_hun.ih_idseq.icd_id = htons(requestId);
					requestHeader.icmp_hun.ih_idseq.icd_seq = 0;
					requestHeader.icmp_type = ICMP_ECHO;
					requestHeader.icmp_cksum = ChecksumIpAddress((const uint16_t*)(&requestHeader), sizeof(struct icmp));

					sendto(m_socket4, &requestHeader, sizeof(requestHeader), 0, (const struct sockaddr*)&requestAddress, (socklen_t)sizeof(requestAddress));
#endif
				}
			}
			else if (isV6)
			{
				struct sockaddr_in6 requestAddress;
				memset(&requestAddress, 0, sizeof(requestAddress));
				requestAddress.sin6_family = AF_INET6;
				requestAddress.sin6_port = 0;

				if (inet_pton(AF_INET6, pR->m_ip.c_str(), &requestAddress.sin6_addr) > 0)
				{
#ifdef EDDIE_PING_PLATFORM_WINDOWS
					// TODO
#endif
#ifdef EDDIE_PING_PLATFORM_UNIX


					struct icmp6_hdr requestHeader;
					PING_ZEROMEMORY(&requestHeader, sizeof(struct icmp6_hdr));
					requestHeader.icmp6_id = htons(requestId);
					requestHeader.icmp6_seq = 0;
					requestHeader.icmp6_type = ICMP6_ECHO_REQUEST;
					requestHeader.icmp6_code = 0;
					requestHeader.icmp6_cksum = ChecksumIpAddress((const uint16_t*)(&requestHeader), sizeof(struct icmp6_hdr));

					sendto(m_socket6, &requestHeader, sizeof(requestHeader), 0, (const struct sockaddr*)&requestAddress, (socklen_t)sizeof(requestAddress));
#endif
				}
			}
		}
		else if ((pR->m_endTime == 0) && (now > pR->m_startTime + (pR->m_timeoutMs * 1000)))
		{
			// Timeout
			RequestEnd(requestId, -1);
		}
	}

	// Check v4
	if (m_socket4 != -1)
	{
#ifdef EDDIE_PING_PLATFORM_WINDOWS
		// TODO		
#endif
#ifdef EDDIE_PING_PLATFORM_UNIX
		int response4size = (int)recvfrom(m_socket4, responseBuffer, PING_BUFFER_SIZE, 0, NULL, NULL);
		if (response4size > 0)
		{
			if (response4size == int(sizeof(struct ip) + sizeof(struct icmp)))
			{
				struct icmp* responseHeader = (struct icmp*)(responseBuffer + sizeof(struct ip));

				if (responseHeader->icmp_type == ICMP_ECHOREPLY)
				{
					// TODO: checksum

					uint16_t responseId = ntohs(responseHeader->icmp_hun.ih_idseq.icd_id);
					RequestEnd(responseId, 0);
				}
			}
		}
#endif
	}

	// Check v6
	if (m_socket6 != -1)
	{
#ifdef EDDIE_PING_PLATFORM_WINDOWS
		// TODO
#endif
#ifdef EDDIE_PING_PLATFORM_UNIX
		int response6size = (int)recvfrom(m_socket6, responseBuffer, PING_BUFFER_SIZE, 0, NULL, NULL);
		if (response6size > 0)
		{
			// here the buffer doesn't seem to include the ip6_hdr while in ipv4 is included (is there an issue with IP_HDRINCL?)
			if (response6size == int(sizeof(struct icmp6_hdr)))
			{
				struct icmp6_hdr* responseHeader = (struct icmp6_hdr*)(responseBuffer);

				if (responseHeader->icmp6_type == ICMP6_ECHO_REPLY)
				{
					// TODO: checksum

					uint16_t responseId = ntohs(responseHeader->icmp6_id);
					RequestEnd(responseId, 0);
				}
			}
		}
#endif
	}

	for (std::vector<uint16_t>::const_iterator iDone = m_done.begin(); iDone != m_done.end(); ++iDone)
	{
		uint16_t requestId = *iDone;
		PingRequest* pR = m_requests[requestId];
		m_requests.erase(requestId);
		delete pR;
	}
	m_done.clear();

	m_mutex.unlock();

	return nPending;
}

uint16_t PingEngine::ChecksumIpAddress(const uint16_t* addr, uint32_t len)
{
	uint32_t sum = 0;

	while (len > 1)
	{
		sum += *addr++;
		len -= 2;
	}

	if (len > 0)
		sum += *(unsigned char*)addr;

	sum = (sum >> 16) + (sum & 0xffff);
	sum += (sum >> 16);

	return ~sum;
}

// Generate a request
void PingEngine::Request(const uint16_t& id, const std::string& ip, int timeoutMs, const std::string& notes)
{
	m_mutex.lock();

	PingRequest* pR = new PingRequest();
	pR->m_ip = ip;
	pR->m_timeoutMs = timeoutMs;
	pR->m_notes = notes;
	m_requests[id] = pR;

	m_mutex.unlock();
}

void PingEngine::RequestEnd(const uint16_t& id, int code)
{
	if (m_requests.find(id) != m_requests.end())
	{
		PingRequest* pR = m_requests[id];
		if (pR->m_endTime == 0)
		{
			pR->m_endTime = GetTimestampUnixUsec();

			int result = code;
			if (code == 0)
				result = int((pR->m_endTime - pR->m_startTime) / 1000);

			OnResponse(id, result);

			if (m_debug)
				std::cout << "IP '" << pR->m_ip << "'" << "(" << pR->m_notes << ") done in " << result << "\n";

			m_done.push_back(id);
		}
	}
}

uint64_t PingEngine::GetTimestampUnixUsec()
{
#if defined(WIN32) || defined(_WIN32) || defined(__WIN32__) || defined(__NT__)
	uint64_t EPOCH = ((uint64_t)116444736000000000ULL);
	SYSTEMTIME  system_time;
	FILETIME    file_time;
	uint64_t    time;
	GetSystemTime(&system_time);
	SystemTimeToFileTime(&system_time, &file_time);
	time = ((uint64_t)file_time.dwLowDateTime);
	time += ((uint64_t)file_time.dwHighDateTime) << 32;
	return (time - EPOCH) / 10 + uint64_t(system_time.wMilliseconds) * 1000;
#else
	struct timeval now;
	return gettimeofday(&now, NULL) != 0 ? 0 : now.tv_sec * 1000000 + now.tv_usec;
#endif
}

int PingEngine::SocketCreate(int af, int type, int protocol)
{
#ifdef EDDIE_PING_PLATFORM_WINDOWS
	SOCKET s = WSASocketW(af, type, protocol, NULL, 0, 0);
	if (s == INVALID_SOCKET)
		return -1;
	else
		return (int)s;
#endif

#ifdef EDDIE_PING_PLATFORM_UNIX
	return socket(af, type, protocol);
#endif

	return -1;
}

void PingEngine::SocketClose(int s)
{
#ifdef EDDIE_PING_PLATFORM_WINDOWS
	closesocket(s);
#endif

#ifdef EDDIE_PING_PLATFORM_UNIX
	close(s);
#endif
}

bool PingEngine::SocketSetNonBlock(int s)
{
#ifdef EDDIE_PING_PLATFORM_WINDOWS
	unsigned long m = 1;
	if (ioctlsocket(m_socket4, FIONBIO, &m) != 0)
		return false;
#endif

#ifdef EDDIE_PING_PLATFORM_UNIX
	if (fcntl(s, F_SETFL, O_NONBLOCK) == -1)
		return false;
#endif
	return true;
}

bool PingEngine::SocketSetHdrIncl(int s)
{
#ifdef EDDIE_PING_PLATFORM_UNIX
	int hdrincl = 0;
	if (setsockopt(s, IPPROTO_IP, IP_HDRINCL, &hdrincl, sizeof(hdrincl)) == -1)
		return false;
#endif
	return true;
}

// Call from Impl::Run a PingEngine p; p.TestDebug() to debug
void PingEngine::TestDebug()
{
	std::cout << "Ping Test Init\n";

	m_debug = true;

	Start();

	std::cout << "Ping Test Samples IPv4\n";

	Request(1,"193.148.16.210", 5000, "expected:230");
	Request(2,"8.8.8.9", 5000, "expected:timeout");
	Request(3,"188.166.41.48", 5000, "expected:52");
	Request(4,"184.75.223.210", 5000, "expected:127");

	std::cout << "Ping Test Samples IPv6\n";

	Request(5,"2606:6080:1002:6:ddb0:6fe6:c526:9e8a", 5000, "expected-v6:127");
	Request(6,"2a03:b0c0:2:d0::11b4:6001", 5000, "expected-v6:52");
	Request(7,"2a03:b0c0:2:d0::11b4:6002", 5000, "expected-v6:timeout");

	std::cout << "Ping Test Start\n";

	uint64_t timeStart = GetTimestampUnixUsec();	
	for (;;)
	{
		int nPending = Check();

		int sleepMs = 1000; // 1s
		if (nPending > 0)
			sleepMs = 1;

#ifdef EDDIE_PING_PLATFORM_WINDOWS
		::Sleep(sleepMs);
#endif
#ifdef EDDIE_PING_PLATFORM_UNIX
		usleep(sleepMs * 1000);
#endif

		if (GetTimestampUnixUsec() - timeStart > 10 * 1000 * 1000) // Run for 10 seconds and exit
			break;
	}

	std::cout << "Ping Test Stopping\n";

	Stop();

	std::cout << "Ping Test End\n";
}

void PingEngine::OnResponse(const uint16_t& id, const int& result)
{
}