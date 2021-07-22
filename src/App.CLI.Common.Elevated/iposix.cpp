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

#include "iposix.h"

#include "signal.h"

#include <cstring>
#include <dirent.h>
#include <errno.h>
#include <fcntl.h>           /* fcntl() */
#include <fstream>
#include <sstream>
#include <stdint.h>
#include <unistd.h>

#include <sys/socket.h>
#include <sys/stat.h>
#include <sys/time.h>
#include <sys/types.h>

#include "pstream.h"
using namespace redi; // related to pstream.h

// Ping
#include <netdb.h>           /* getaddrinfo() */
#include <arpa/inet.h>       /* inet_XtoY() */
#include <netinet/in.h>      /* IPPROTO_ICMP */
#include <netinet/ip.h>
#include <netinet/ip_icmp.h> /* struct icmp */
typedef int socket_t;

#define MIN_IP_HEADER_SIZE 20
#define MAX_IP_HEADER_SIZE 60
#define MAX_IP6_PSEUDO_HEADER_SIZE 40

#ifndef ICMP_ECHO
#define ICMP_ECHO 8
#endif
#ifndef ICMP_ECHO6
#define ICMP6_ECHO 128
#endif
#ifndef ICMP_ECHO_REPLY
#define ICMP_ECHO_REPLY 0
#endif
#ifndef ICMP_ECHO_REPLY6
#define ICMP6_ECHO_REPLY 129
#endif

struct ip6_pseudo_hdr {
	struct in6_addr ip6_src;
	struct in6_addr ip6_dst;
	uint32_t ip6_plen;
	uint8_t ip6_zero[3];
	uint8_t ip6_nxt;
};

// --------------------------
// Virtual
// --------------------------

void IPosix::Idle()
{
	// Scenario: macOS with portable app in /Application, create launchd daemon, replace the app with a new version
	if (GetLaunchMode() == "service")
	{
		time_t start = GetProcessModTimeStart();
		time_t cur = GetProcessModTimeCurrent();
		if (start != cur)
		{
			LogLocal("Executable changed/upgraded, exit");
			kill(GetCurrentProcessId(), SIGTERM);
		}
	}

	IBase::Idle();
}

void IPosix::Do(const std::string& commandId, const std::string& command, std::map<std::string, std::string>& params)
{
	if (command == "openvpn")
	{
		std::string id = params["id"];
		std::string action = params["action"];

		if (action == "stop")
		{
			std::string signal = params["signal"];

			if (m_keypair.find("openvpn_" + id + "_pid") != m_keypair.end())
			{
				pid_t pid = std::atoi(m_keypair["openvpn_" + id + "_pid"].c_str());

				int signalUnix = SIGINT;
				if (signal == "sigint")
					signalUnix = SIGINT;
				else if (signal == "sigterm")
					signalUnix = SIGTERM;

				kill(pid, signalUnix);
			}
		}
		else if (action == "start")
		{
			CheckIfExecutableIsAllowed(params["path"], true);

			std::string checkResult = CheckValidOpenVpnConfigFile(params["config"]);
			if (checkResult != "")
			{
				ThrowException("Not supported OpenVPN config: " + checkResult);
			}
			else
			{
				const pstreams::pmode mode = pstreams::pstdout | pstreams::pstderr;
				pstreams::argv_type argv;
				argv.push_back(params["path"]);

				argv.push_back("--config");
				argv.push_back(params["config"]);

				pstream child(argv, mode);
				char buf[1024 * 32];
				std::streamsize n;

				m_keypair["openvpn_" + id + "_pid"] = std::to_string(child.pid());
				ReplyCommand(commandId, "procid:" + std::to_string(child.pid()));

				bool finished[2] = { false, false };
				while (!finished[0] || !finished[1])
				{
					if (!finished[0])
					{
						while ((n = child.err().readsome(buf, sizeof(buf))) > 0)
						{
							std::string o = std::string(buf, n);
							ReplyCommand(commandId, "stderr:" + o);
						}
						if (child.eof())
						{
							finished[0] = true;
							if (!finished[1])
								child.clear();
						}
					}

					if (!finished[1])
					{
						while ((n = child.out().readsome(buf, sizeof(buf))) > 0)
						{
							std::string o = std::string(buf, n);
							ReplyCommand(commandId, "stdout:" + o);
						}
						if (child.eof())
						{
							finished[1] = true;
							if (!finished[0])
								child.clear();
						}
					}

					Sleep(100); // 0.1 secs. TODO: Search for alternative...
				}

				m_keypair.erase("openvpn_" + id + "_pid");

				child.close();
				int status = child.rdbuf()->status();
				int exitCode = -1;
				if (WIFEXITED(status))
					exitCode = WEXITSTATUS(status);

				std::string exitCodeStr = std::to_string(exitCode);
				ReplyCommand(commandId, "return:" + exitCodeStr);
			}
		}
	}
	else if (command == "hummingbird")
	{
		std::string id = params["id"];
		std::string action = params["action"];

		if (action == "stop")
		{
			std::string signal = params["signal"];

			if (m_keypair.find("hummingbird_" + id + "_pid") != m_keypair.end())
			{
				pid_t pid = std::atoi(m_keypair["hummingbird_" + id + "_pid"].c_str());

				int signalUnix = SIGINT;
				if (signal == "sigint")
					signalUnix = SIGINT;
				else if (signal == "sigterm")
					signalUnix = SIGTERM;

				kill(pid, signalUnix);
			}
		}
		else if (action == "start")
		{
			CheckIfExecutableIsAllowed(params["path"], true);

			// Workaround. In any case, hummingbird called from Eddie don't perform any action that need a recovery.
			if (FsFileExists("/etc/airvpn/hummingbird.lock"))
				FsFileDelete("/etc/airvpn/hummingbird.lock");

			std::string checkResult = CheckValidHummingbirdConfigFile(params["config"]);
			if (checkResult != "")
			{
				ThrowException("Not supported Hummingbird config: " + checkResult);
			}
			else
			{
				const pstreams::pmode mode = pstreams::pstdout | pstreams::pstderr;
				pstreams::argv_type argv;
				argv.push_back(params["path"]);

				if (params.find("dns-ignore") != params.end())
				{
					if (params["dns-ignore"] == "true")
						argv.push_back("--ignore-dns-push"); // TOFIX: must be a generic dns ignore, otherwise HB try to apply any dhcp-option in client-side config?
				}

				// Bypass HB network lock
				argv.push_back("--network-lock");
				argv.push_back("off");

				if (params.find("gui-version") != params.end())
				{
					argv.push_back("--gui-version");
					argv.push_back(params["gui-version"]);
				}

				argv.push_back(params["config"]);

				pstream child(argv, mode);
				char buf[1024 * 32];
				std::streamsize n;

				m_keypair["hummingbird_" + id + "_pid"] = std::to_string(child.pid());
				ReplyCommand(commandId, "procid:" + std::to_string(child.pid()));

				bool finished[2] = { false, false };
				while (!finished[0] || !finished[1])
				{
					if (!finished[0])
					{
						while ((n = child.err().readsome(buf, sizeof(buf))) > 0)
						{
							std::string o = std::string(buf, n);
							ReplyCommand(commandId, "stderr:" + o);
						}
						if (child.eof())
						{
							finished[0] = true;
							if (!finished[1])
								child.clear();
						}
					}

					if (!finished[1])
					{
						while ((n = child.out().readsome(buf, sizeof(buf))) > 0)
						{
							std::string o = std::string(buf, n);
							ReplyCommand(commandId, "stdout:" + o);
						}
						if (child.eof())
						{
							finished[1] = true;
							if (!finished[0])
								child.clear();
						}
					}

					Sleep(100); // 0.1 secs. TOFIX: alternative?
				}

				m_keypair.erase("hummingbird_" + id + "_pid");

				child.close();
				int status = child.rdbuf()->status();
				int exitCode = -1;
				if (WIFEXITED(status))
					exitCode = WEXITSTATUS(status);

				std::string exitCodeStr = std::to_string(exitCode);
				ReplyCommand(commandId, "return:" + exitCodeStr);
			}
		}
	}
	else
	{
		IBase::Do(commandId, command, params);
	}
}

/* // Old exec edition
int IPosix::GetProcessIdMatchingIPEndPoints(std::string sourceAddr, int sourcePort, std::string destAddr, int destPort)
{
	std::vector<std::string> args;
	args.push_back("-F");
	args.push_back("pfn");
	args.push_back("-anPi");
	args.push_back("4tcp@" + destAddr + ":" + std::to_string(destPort));

	std::string lsofPath = LocateExecutable("lsof");
	if(lsofPath != "")
	{
		ExecResult lsResult = ExecEx("lsof", args);
		if(lsResult.exit == 0)
		{
			std::vector<std::string> lines = StringToVector(lsResult.out, '\n');
			int lastPid = 0;
			std::string needle = sourceAddr + ":" + std::to_string(sourcePort) + "->" + destAddr + ":" + std::to_string(destPort);
			for (std::vector<std::string>::const_iterator i = lines.begin(); i != lines.end(); ++i)
			{
				std::string line = StringTrim(StringToLower(*i));

				if(line.length()==0)
					continue;

				char ch = line.at(0);
				if(ch == 'p')
				{
					lastPid = strtol(line.c_str()+1,NULL,10);
				}
				else if(ch == 'n')
				{
					if(StringContain(line, needle))
						return lastPid;
				}
			}
		}
	}

	return 0;
}
*/

void IPosix::AddTorCookiePaths(const std::string& torPath, const std::string& username, std::vector<std::string>& result)
{
	if (torPath != "") // TorBrowser
	{
		std::string path = FsFileGetDirectory(torPath) + "/../Data/Tor/control_auth_cookie";
		result.push_back(path);
	}

	result.push_back("/var/run/tor/control.authcookie");
	result.push_back("/var/lib/tor/control_auth_cookie");
	result.push_back("/run/tor/control.authcookie"); // Default Ubuntu 20.04.1 LTS tor from official repo
	result.push_back("/run/tor/control_auth_cookie"); // Variant of above
}

// --------------------------
// Virtual Pure, OS
// --------------------------

bool IPosix::IsRoot()
{
	uid_t euid = geteuid();
	return (euid == 0);
}

void IPosix::Sleep(int ms)
{
	usleep(ms * 1000);
}

uint64_t IPosix::GetTimestampUnixUsec()
{
	struct timeval now;
	return gettimeofday(&now, NULL) != 0 ? 0 : now.tv_sec * 1000000 + now.tv_usec;
}



// Used by ::Ping below.  RFC 1071 - http://tools.ietf.org/html/rfc1071

/*
static uint16_t compute_checksum(const char* buf, size_t size) {
	size_t i;
	uint64_t sum = 0;

	for (i = 0; i < size; i += 2) {
		sum += *(uint16_t*)buf;
		buf += 2;
	}
	if (size - i > 0) {
		sum += *(uint8_t*)buf;
	}

	while ((sum >> 16) != 0) {
		sum = (sum & 0xffff) + (sum >> 16);
	}

	return (uint16_t)~sum;
}
*/

// Under development, not yet used.
// macos/linux, IPv6 issue (always timeout).
// Based on https://github.com/sryze/ping
// Windows implementation omitted right now, but can be done.
// Also, under Linux,
// // iposix.cpp: warning: Using 'getaddrinfo' in statically linked applications requires at runtime the shared libraries from the glibc version used for linking
// Main objective here is Eddie macOS, because Mono/Xamarin don't support IPv6.
int IPosix::Ping(const std::string& host, const int timeout)
{
	/*

	int gai_error = 0;
	socket_t sockfd = -1;
	struct addrinfo addrinfo_hints;
	struct addrinfo* addrinfo_head = NULL;
	struct addrinfo* addrinfo = NULL;
	void* addr = NULL;
	char addrstr[INET6_ADDRSTRLEN] = "<unknown>";
	struct sockaddr_storage src_addr;
	socklen_t src_addr_len = sizeof(src_addr);
	uint16_t id = (uint16_t)getpid();
	uint16_t seq;
	int result = -1;
	int ntry = 1;

	if (StringIsIPv4(host))
	{
		memset(&addrinfo_hints, 0, sizeof(addrinfo_hints));
		addrinfo_hints.ai_family = AF_INET;
		addrinfo_hints.ai_socktype = SOCK_RAW;
		addrinfo_hints.ai_protocol = IPPROTO_ICMP;
		gai_error = getaddrinfo(host.c_str(), NULL, &addrinfo_hints, &addrinfo_head);
	}
	else if (StringIsIPv6(host))
	{
		memset(&addrinfo_hints, 0, sizeof(addrinfo_hints));
		addrinfo_hints.ai_family = AF_INET6;
		addrinfo_hints.ai_socktype = SOCK_RAW;
		addrinfo_hints.ai_protocol = IPPROTO_ICMPV6;
		gai_error = getaddrinfo(host.c_str(), NULL, &addrinfo_hints, &addrinfo_head);
	}
	else
		ThrowException("Unexpected family");

	if (gai_error != 0) {
		fprintf(stderr, "getaddrinfo: %s\n", gai_strerror(gai_error));
		goto error_exit;
	}

	for (addrinfo = addrinfo_head; addrinfo != NULL; addrinfo = addrinfo->ai_next)
	{
		sockfd = socket(addrinfo->ai_family, addrinfo->ai_socktype, addrinfo->ai_protocol);
		if (sockfd >= 0)
		{
			break;
		}
	}

	if ((int)sockfd < 0)
	{
		goto error_exit;
	}

	switch (addrinfo->ai_family)
	{
	case AF_INET:
		addr = &((struct sockaddr_in*)addrinfo->ai_addr)->sin_addr;
		break;
	case AF_INET6:
		addr = &((struct sockaddr_in6*)addrinfo->ai_addr)->sin6_addr;
		break;
	default:
		ThrowException("Unexpected family");
	}

	inet_ntop(addrinfo->ai_family, addr, addrstr, sizeof(addrstr));

	if (fcntl(sockfd, F_SETFL, O_NONBLOCK) == -1)
	{
		goto error_exit;
	}

	for (seq = 0; seq < ntry; seq++)
	{
		struct icmp request;
		int send_result = 0;
		char recv_buf[MAX_IP_HEADER_SIZE + sizeof(struct icmp)];
		int recv_size = 0;
		int recv_result = 0;
		uint8_t ip_vhl = 0;
		uint8_t ip_header_size = 0;
		struct icmp* response;
		struct icmp response_host;
		uint64_t timeoutus = timeout*10;
		uint64_t start_time;
		uint64_t delay;

		if (seq > 0)
			usleep(1000000); // One sec

		request.icmp_type = addrinfo->ai_family == AF_INET6 ? ICMP6_ECHO : ICMP_ECHO;
		request.icmp_code = 0;
		request.icmp_cksum = 0;
		request.icmp_id = htons(id);
		request.icmp_seq = htons(seq);

		switch (addrinfo->ai_family)
		{
			case AF_INET:
				request.icmp_cksum =
					compute_checksum((const char*)&request, sizeof(request));
				break;
			case AF_INET6: {
				// Checksum is calculated from the ICMPv6 packet prepended with an IPv6 "pseudo-header" (this is different from IPv4).
				// https://tools.ietf.org/html/rfc2463#section-2.3
				// https://tools.ietf.org/html/rfc2460#section-8.1
				struct {
					struct ip6_pseudo_hdr ip6_hdr;
					struct icmp icmp;
				} data = {};

				data.ip6_hdr.ip6_src = in6addr_loopback;
				data.ip6_hdr.ip6_dst = ((struct sockaddr_in6*)&addrinfo->ai_addr)->sin6_addr;
				data.ip6_hdr.ip6_plen = htonl((uint32_t)sizeof(struct icmp));
				data.ip6_hdr.ip6_nxt = IPPROTO_ICMPV6;
				data.icmp = request;

				request.icmp_cksum = compute_checksum((const char*)&data, sizeof(data));
				break;
			}
			default:
				goto error_exit; // Unexpected family
		}

		send_result = sendto(sockfd, (const char*)&request, sizeof(request), 0, addrinfo->ai_addr, (int)addrinfo->ai_addrlen);
		if (send_result < 0)
			goto error_exit;

		switch (addrinfo->ai_family)
		{
			case AF_INET:
				recv_size = (int)(MAX_IP_HEADER_SIZE + sizeof(struct icmp));
				break;
			case AF_INET6:
				// With IPv6 we don't receive IP headers in recvfrom. So the buffer size = ICMP packet size.
				recv_size = (int)sizeof(struct icmp);
				break;
			default:
				goto error_exit; // Unexpected family
		}

		start_time = GetTimestampUnixUsec();

		for (;;)
		{
			delay = GetTimestampUnixUsec() - start_time;

			recv_result = recvfrom(sockfd,
				recv_buf,
				recv_size,
				0,
				(struct sockaddr*)&src_addr,
				&src_addr_len);
			if (recv_result == 0)
			{
				printf("Connection closed\n");
				break;
			}
			if (recv_result < 0)
			{
				if (errno == EAGAIN)
				{
					if (delay > timeoutus)
					{
						printf("Request timed out\n");
						break;
					}
					else
					{
						// No data available yet, try to receive again.
						continue;
					}
				}
				else
				{
					break;
				}
			}

			switch (addrinfo->ai_family)
			{
				case AF_INET:
					// In contrast to IPv6, for IPv4 connections we do receive IP headers in incoming datagrams. VHL = version (4 bits) + header length (lower 4 bits).
					ip_vhl = *(uint8_t*)recv_buf;
					ip_header_size = (ip_vhl & 0x0F) * 4;
					break;
				case AF_INET6:
					ip_header_size = 0;
					break;
				default:
					goto error_exit; // Unexpected family
			}

			response = (struct icmp*)(recv_buf + ip_header_size);
			memcpy(&response_host, response, sizeof(response_host));
			response_host.icmp_id = ntohs(response_host.icmp_id);
			response_host.icmp_seq = ntohs(response_host.icmp_seq);

			if (response_host.icmp_id != id || response_host.icmp_seq != seq)
				continue;

			if ((addrinfo->ai_family == AF_INET && response_host.icmp_type == ICMP_ECHO_REPLY) || (addrinfo->ai_family == AF_INET6 && response_host.icmp_type == ICMP6_ECHO_REPLY))
			{
				uint16_t checksum = response_host.icmp_cksum;
				uint16_t expected_checksum;

				response->icmp_cksum = 0;

				switch (addrinfo->ai_family)
				{
					case AF_INET:
						expected_checksum = compute_checksum((const char*)response, sizeof(*response));
						break;
					case AF_INET6:
					{
						struct {
							struct ip6_pseudo_hdr ip6_hdr;
							struct icmp icmp;
						} data = {};

						data.ip6_hdr.ip6_src =
							((struct sockaddr_in6*)&src_addr)->sin6_addr;
						data.ip6_hdr.ip6_dst = in6addr_loopback;
						data.ip6_hdr.ip6_plen = htonl((uint32_t)sizeof(struct icmp));
						data.ip6_hdr.ip6_nxt = IPPROTO_ICMPV6;
						memcpy(&data.icmp, response, sizeof(struct icmp));

						expected_checksum = compute_checksum((const char*)&data, sizeof(data));
						break;
					}
				}

				if (checksum == expected_checksum)
					break;
			}
		}

		if (recv_result <= 0)
			continue;

		printf("Received ICMP echo reply from %s: seq=%d, time=%.3f ms\n", addrstr, response->icmp_seq, (double)delay / 1000.0);
		result = (double)delay / 1000.0;
	}

	freeaddrinfo(addrinfo_head);

	return result;

error_exit:

	if (addrinfo_head != NULL) {
		freeaddrinfo(addrinfo_head);
	}

	*/
	return -1;
}

pid_t IPosix::GetCurrentProcessId()
{
	return getpid();
}

pid_t IPosix::GetParentProcessId()
{
	return getppid();
}

pid_t IPosix::GetParentProcessId(pid_t pid)
{
	// We don't find a better method
	std::string statusPath = "/proc/" + std::to_string(pid) + "/status";
	if (!FsFileExists(statusPath))
		return 0;

	std::string statusBody = FsFileReadText(statusPath);
	std::string ppidS = StringTrim(StringExtractBetween(statusBody, "PPid:", "\n"));
	return atoi(ppidS.c_str());
}

pid_t IPosix::GetProcessIdOfName(const std::string& name)
{
	// TOFIX - Find a method without exec
	ExecResult pidofResult = ExecEx1("pidof", name);
	if (pidofResult.exit == 0)
		return atoi(pidofResult.out.c_str());
	else
		return 0;
}

std::string IPosix::GetCmdlineOfProcessId(pid_t pid)
{
	std::string path = "/proc/" + std::to_string(pid) + "/cmdline";

	if (FsFileExists(path) == false)
		return "";

	std::string result;
	FILE* f;
	f = fopen(path.c_str(), "rb");
	if (f == NULL)
		return "";
	for (;;)
	{
		int chI = fgetc(f);
		if (chI == EOF)
			break;

		unsigned char ch = (unsigned char)chI;

		if (ch == 0)
			result += " ";
		else
			result += ch;
	}
	fclose(f);
	return result;
}

std::string IPosix::GetWorkingDirOfProcessId(pid_t pid)
{
	std::string procPath = "/proc/" + std::to_string(pid) + "/cwd";
	if (FsFileExists(procPath) == false)
		return "";

	char resolvedPath[PATH_MAX];
	size_t result = readlink(procPath.c_str(), resolvedPath, PATH_MAX - 1);
	resolvedPath[result] = 0;
	return std::string(resolvedPath);
}

void IPosix::SetEnv(const std::string& name, const std::string& value)
{
	setenv(name.c_str(), value.c_str(), 1);
}

int IPosix::Exec(const std::string& path, const std::vector<std::string>& args, const bool stdinWrite, const std::string& stdinBody, std::string& stdOut, std::string& stdErr, const bool log)
{
	const pstreams::pmode mode = pstreams::pstdin | pstreams::pstdout | pstreams::pstderr;
	pstreams::argv_type argv;
	std::string logMessage;

	if (log)
		logMessage += "Exec, path:'" + path + "'";

	argv.push_back(path);
	for (std::vector<std::string>::const_iterator i = args.begin(); i != args.end(); ++i)
	{
		if (log)
			logMessage += ", arg:'" + *i + "'";
		argv.push_back(*i);
	}

	pstream child(argv, mode);
	char buf[1024];
	std::streamsize n;
	bool finished[2] = { false, false };

	if (stdinWrite)
	{
		if (stdinBody.length() != 0)
		{

			child.write(stdinBody.c_str(), stdinBody.length());
		}

		child.peof();
	}

	while (!finished[0] || !finished[1])
	{
		if (!finished[0])
		{
			while ((n = child.err().readsome(buf, sizeof(buf))) > 0)
			{
				stdErr += std::string(buf, n);
			}
			if (child.eof())
			{
				finished[0] = true;
				if (!finished[1])
					child.clear();
			}
		}

		if (!finished[1])
		{
			while ((n = child.out().readsome(buf, sizeof(buf))) > 0)
			{
				stdOut += std::string(buf, n);
			}
			if (child.eof())
			{
				finished[1] = true;
				if (!finished[0])
					child.clear();
			}
		}

		if (child.rdbuf()->exited())
			break;

		Sleep(10); // 0.01 secs. TODO: Search for alternative...
	}

	child.close();
	int status = child.rdbuf()->status();
	int exitCode = -1;
	if (WIFEXITED(status))
		exitCode = WEXITSTATUS(status);

	if (log)
	{
		logMessage += ", exit:" + std::to_string(exitCode);
		if (StringTrim(stdOut) != "")
			logMessage += ", out:'" + StringTrim(stdOut) + "'";
		if (StringTrim(stdErr) != "")
			logMessage += ", err:'" + StringTrim(stdErr) + "'";
		LogDebug(logMessage);
	}

	return exitCode;
}

bool IPosix::FsDirectoryCreate(const std::string& path)
{
	if (FsDirectoryExists(path))
		return true;
	else
		return (mkdir(path.c_str(), 0755) == 0);
}

bool IPosix::FsFileExists(const std::string& path)
{
	struct stat db;
	return (stat(path.c_str(), &db) == 0);
}

bool IPosix::FsDirectoryExists(const std::string& path)
{
	return FsFileExists(path);
}

bool IPosix::FsFileDelete(const std::string& path)
{
	if (FsFileExists(path))
		return (unlink(path.c_str()) == 0);
	else
		return true;
}

bool IPosix::FsDirectoryDelete(const std::string& path, bool recursive)
{
	// Not implemented, never used.
	ThrowException("FsDirectoryCreate - Not implemented");
	return false;
}

bool IPosix::FsFileMove(const std::string& source, const std::string& destination)
{
	return (rename(source.c_str(), destination.c_str()) == 0);
}

std::string IPosix::FsFileReadText(const std::string& path)
{
	std::ifstream f(path);
	if (!f) return "";
	std::stringstream buffer;
	buffer << f.rdbuf();
	return buffer.str();
}

std::vector<char> IPosix::FsFileReadBytes(const std::string& path)
{
	// Note: This don't work with a /proc/x/cmdline

	std::ifstream ifs(path.c_str(), std::ios::binary | std::ios::ate);
	std::ifstream::pos_type pos = ifs.tellg();

	std::vector<char>  result(pos);

	ifs.seekg(0, std::ios::beg);
	ifs.read(&result[0], pos);

	return result;
}

std::vector<std::string> IPosix::FsFilesInPath(const std::string& path)
{
	std::vector<std::string> result;

	DIR* dirp = opendir(path.c_str());
	if (dirp == NULL)
		return result;
	struct dirent* dp;
	while ((dp = readdir(dirp)) != NULL) {
		std::string filename = dp->d_name;
		if (filename == ".") continue;
		if (filename == "..") continue;
		result.push_back(filename);
	}
	closedir(dirp);
	return result;
}

std::string IPosix::FsGetTempPath()
{
	// Same env sequence performed by boost tmpdir
	// First version are the same app dir (elevated is always run with root, so no permission issues).
	// But on macOS, an App in Downloads start with AppTranslocation, and file written here are not accessible (for example pf config).

	const char* path;
	path = getenv("TMP");
	if (path != NULL)
		return path;

	path = getenv("TMPDIR");
	if (path != NULL)
		return path;

	path = getenv("TEMP");
	if (path != NULL)
		return path;

	if (FsFileExists("/tmp"))
		return "/tmp";

	return GetProcessPathCurrentDir();
}

std::vector<std::string> IPosix::FsGetEnvPath()
{
	return StringToVector(getenv("PATH"), ':', false);
}

std::string IPosix::FsGetRealPath(std::string path)
{
	char resolvedPath[PATH_MAX];
	char* resolvedPathResult = realpath(path.c_str(), resolvedPath);
	if (resolvedPathResult == NULL)
		return "";
	else
		return std::string(resolvedPathResult);
}

bool IPosix::SocketIsValid(HSOCKET s)
{
	return (s > 0);
}

void IPosix::SocketMarkReuseAddr(HSOCKET s)
{
	int option = 1;
	setsockopt(s, SOL_SOCKET, SO_REUSEADDR, &option, sizeof(option));
}

void IPosix::SocketBlockMode(HSOCKET s, bool block)
{
	int result = -1;
	if (block == false)
		result = fcntl(s, F_SETFL, fcntl(s, F_GETFL, 0) | O_NONBLOCK);
	else
		result = fcntl(s, F_SETFL, fcntl(s, F_GETFL, 0) & ~O_NONBLOCK);
	if (result == -1) {
		ThrowException("Error on fcntl socket");
	}
}

void IPosix::SocketClose(HSOCKET s)
{
	shutdown(s, 2);
	close(s);
}

int IPosix::SocketGetLastErrorCode()
{
	return errno;
}

bool IPosix::CheckIfExecutableIsAllowed(const std::string& path, const bool& throwException)
{
	std::string issues = "";

	if (CheckIfExecutableIsWhitelisted(path)) // If true, skip other checks.
		return true;

	struct stat st;
	memset(&st, 0, sizeof(struct stat));
	if (stat(path.c_str(), &st) != 0)
	{
		issues += "Not readable;";
	}
	else
	{
		if (st.st_uid != 0)
		{
			issues += "Not owned by root;";
		}
		else if ((st.st_mode & S_ISUID) == 0)
		{
			if ((st.st_mode & S_IXUSR) == 0)
			{
				issues += "Not executable by owner;";
			}

			if ((st.st_mode & S_IWGRP) != 0)
			{
				issues += "Writable by group";
			}

			if ((st.st_mode & S_IWOTH) != 0)
			{
				issues += "Writable by other;";
			}
		}
	}

	if (issues != "")
	{
		if (throwException)
			ThrowException("Executable '" + path + "' not allowed: " + issues);
		else
			return false;
	}
	
	return true;
}

int IPosix::GetProcessIdMatchingIPEndPoints(struct sockaddr_in& addrClient, struct sockaddr_in& addrServer)
{
	if (FsFileExists("/proc") == false)
		return 0;

	// First, locate inode in /proc/net/tcp. After, scan /proc/*/fd/* to search the inode and identify the pid.

	int pidFound = 0;

	std::string procNetTcp = FsFileReadText("/proc/net/tcp");

	std::string sourceAddrHex = StringHexEncode(addrClient.sin_addr.s_addr, 8);
	std::string sourcePortHex = StringHexEncode(ntohs(addrClient.sin_port), 4);
	std::string destAddrHex = StringHexEncode(addrServer.sin_addr.s_addr, 8);
	std::string destPortHex = StringHexEncode(ntohs(addrServer.sin_port), 4);

	std::vector<std::string> procNetTcpLines = StringToVector(procNetTcp, '\n');
	for (std::vector<std::string>::const_iterator l = procNetTcpLines.begin(); l != procNetTcpLines.end(); ++l)
	{
		std::vector<std::string> procNetTcpFields = StringToVector(*l, ' ', true);

		if ((procNetTcpFields.size() > 9) &&
			(procNetTcpFields[1] == sourceAddrHex + ":" + sourcePortHex) &&
			(procNetTcpFields[2] == destAddrHex + ":" + destPortHex)
			)
		{
			long unsigned int inodeSearch = strtoul(procNetTcpFields[9].c_str(), NULL, 0);

			DIR* procDir = opendir("/proc");
			if (procDir != NULL)
			{
				for (;;)
				{
					struct dirent* procDirFile = readdir(procDir);
					if (procDirFile == NULL)
						break;

					std::string procDirFilePath = "/proc/" + std::string(procDirFile->d_name);
					struct stat procDirFileSt; // Don't trust dirent::d_type
					memset(&procDirFileSt, 0, sizeof(struct stat));
					if (stat(procDirFilePath.c_str(), &procDirFileSt) == 0)
					{
						if ((S_ISDIR(procDirFileSt.st_mode)) && (procDirFile->d_name[0] != '.'))
						{
							std::string procFdDirPath = "/proc/" + std::string(procDirFile->d_name) + "/fd";
							DIR* procFdDir = opendir(procFdDirPath.c_str());
							if (procFdDir != NULL)
							{
								for (;;)
								{
									struct dirent* procFdDirFile = readdir(procFdDir);
									if (procFdDirFile == NULL)
										break;

									std::string procFdDirFilePath = "/proc/" + std::string(procDirFile->d_name) + "/fd/" + std::string(procFdDirFile->d_name);
									struct stat procFdDirFileSt; // Don't trust dirent::d_type
									memset(&procFdDirFileSt, 0, sizeof(struct stat));
									if (stat(procFdDirFilePath.c_str(), &procFdDirFileSt) == 0) // Note: lstat
									{
										if ((S_ISSOCK(procFdDirFileSt.st_mode)) && (inodeSearch == procFdDirFileSt.st_ino))
										{
											if (pidFound == 0)
												pidFound = atoi(procDirFile->d_name);
											else
											{
												return 0; // Unexpected
											}
										}
									}
								}
								closedir(procFdDir);
							}
						}
					}
				}
				closedir(procDir);
			}
		}

	}

	return pidFound;
}