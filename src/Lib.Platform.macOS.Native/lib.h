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

#ifndef EDDIE_MACOS_NATIVE_API_H
#define EDDIE_MACOS_NATIVE_API_H

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#define EDDIE_MACOS_NATIVE_EXPORT

#define EDDIE_ZEROMEMORY(dest, len)			memset((dest), 0, (len))

#ifdef __cplusplus
extern "C" {
#endif

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	EDDIE_MACOS_NATIVE_EXPORT int eddie_init();

	EDDIE_MACOS_NATIVE_EXPORT int eddie_file_get_mode(const char* filename);
	EDDIE_MACOS_NATIVE_EXPORT int eddie_file_set_mode(const char* filename, int mode);
	EDDIE_MACOS_NATIVE_EXPORT int eddie_file_set_mode_str(const char* filename, const char* mode);

	// Returns -1 in case of error and 0 or 1 for the flag value
	EDDIE_MACOS_NATIVE_EXPORT int eddie_file_get_immutable(const char* filename);
	EDDIE_MACOS_NATIVE_EXPORT int eddie_file_set_immutable(const char* filename, int flag);

	EDDIE_MACOS_NATIVE_EXPORT bool eddie_file_get_runasroot(const char* filename);

	typedef void (*eddie_sighandler_t)(int);
	EDDIE_MACOS_NATIVE_EXPORT void eddie_signal(int signum, eddie_sighandler_t handler);

	EDDIE_MACOS_NATIVE_EXPORT int eddie_kill(int pid, int sig);

	EDDIE_MACOS_NATIVE_EXPORT void eddie_get_realtime_network_stats(char* buf, int bufMaxLen);

	EDDIE_MACOS_NATIVE_EXPORT void eddie_curl(const char* jRequest, unsigned int resultMaxLen, char* jResult);

	EDDIE_MACOS_NATIVE_EXPORT void eddie_credential_system_read(const char* serviceName, const char* accountName, unsigned int outputMax, char* output);
	EDDIE_MACOS_NATIVE_EXPORT bool eddie_credential_system_write(const char* serviceName, const char* accountName, const char* value);
	EDDIE_MACOS_NATIVE_EXPORT bool eddie_credential_system_delete(const char* serviceName, const char* accountName);

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifdef __cplusplus
}
#endif

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#endif // EDDIE_MACOS_NATIVE_API_H
