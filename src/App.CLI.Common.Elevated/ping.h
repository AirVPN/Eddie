// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2016 AirVPN (support@airvpn.org) / https://airvpn.org
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

// TODO: Windows version

#if defined(WIN32) || defined(_WIN32) || defined(__WIN32__) || defined(__NT__)
typedef unsigned int uint;
typedef int socklen_t;
#else
typedef int HSOCKET;

#include <arpa/inet.h>
#include <netinet/in.h>
#include <netinet/ip.h>
#include <netinet/ip_icmp.h>
#include <netinet/icmp6.h>
#include <sys/ioctl.h>
#include <sys/stat.h>
#include <sys/time.h>
//#include <thread>
#include <time.h>
#include <unistd.h>
#endif

#include <fcntl.h>
#include <fstream>
#include <iomanip>
#include <map>
#include <memory>
#include <mutex>

#include <signal.h>
#include <sstream>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <vector>

#include <iostream> 

#define PING_BUFFER_SIZE 256
#define PING_ZEROMEMORY(dest, len) memset((dest), 0, (len))

class PingRequest
{
public:
	std::string m_ip;
	uint64_t m_timeoutMs = 0;
	uint64_t m_startTime = 0;
	uint64_t m_endTime = 0;
	std::string m_notes;
};

class PingEngine
{
public:

	void Start();
	void Stop();
	int Check();

	void Request(const uint16_t& id, const std::string& ip, int timeoutMs, const std::string& notes);

protected:

	uint16_t ChecksumIpAddress(const uint16_t* addr, uint32_t len);
	void RequestEnd(const uint16_t& id, int code);

	// Utils
	uint64_t GetTimestampUnixUsec();
	int SocketCreate(int af, int type, int protocol);
	void SocketClose(int s);
	bool SocketSetNonBlock(int s);
	bool SocketSetHdrIncl(int s);

	virtual void OnResponse(const uint16_t& id, const int& result);

private:
	std::mutex m_mutex;
	std::map<uint16_t, PingRequest*> m_requests;
	std::vector<uint16_t> m_done;
	bool m_debug = false;

	int m_socket4 = -1;
	int m_socket6 = -1;

public:
	void TestDebug();
};


