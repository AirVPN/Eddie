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

#ifndef EDDIE_LINUX_NATIVE_API_H
#define EDDIE_LINUX_NATIVE_API_H

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifdef __cplusplus
extern "C" {
#endif

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	EDDIE_LINUX_NATIVE_EXPORT int eddie_init();

	EDDIE_LINUX_NATIVE_EXPORT int eddie_file_get_mode(const char* filename);
	EDDIE_LINUX_NATIVE_EXPORT int eddie_file_set_mode(const char* filename, int mode);
	EDDIE_LINUX_NATIVE_EXPORT int eddie_file_set_mode_str(const char* filename, const char* mode);

	// Returns -1 in case of error and 0 or 1 for the flag value
	EDDIE_LINUX_NATIVE_EXPORT int eddie_file_get_immutable(const char* filename);
	EDDIE_LINUX_NATIVE_EXPORT int eddie_file_set_immutable(const char* filename, int flag);

	EDDIE_LINUX_NATIVE_EXPORT bool eddie_file_get_runasroot(const char* filename);

	EDDIE_LINUX_NATIVE_EXPORT int eddie_pipe_write(const char* filename, const char* data);

	// Ping the specified IP address (no host resolution) with the specified timeout in milliseconds (returns -1 in case of error or the elapsed milliseconds)
	EDDIE_LINUX_NATIVE_EXPORT int eddie_ip_ping(const char* address, int timeout);

	typedef void (*eddie_sighandler_t)(int);
	EDDIE_LINUX_NATIVE_EXPORT void eddie_signal(int signum, eddie_sighandler_t handler);

	EDDIE_LINUX_NATIVE_EXPORT int eddie_kill(int pid, int sig);

	EDDIE_LINUX_NATIVE_EXPORT void eddie_curl(const char* jRequest, unsigned int resultMaxLen, char* jResult);

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifdef __cplusplus
}
#endif

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#endif // EDDIE_LINUX_NATIVE_API_H
