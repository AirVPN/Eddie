// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2016 AirVPN (support@airvpn.org) / https://airvpn.org )
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

#ifndef _MAIN_H_
#define _MAIN_H_

#include <Winsock2.h>
#include <conio.h>
#include <strsafe.h>
#include <fwpmu.h>
#include <stdint.h>
#include <vector>

// Some function are here basically to avoid dependencies in C# to assembly only for spot things,
// almost to avoid complexity in different dependencies between .Net Framework vs .Net5

extern "C" { __declspec(dllexport) int eddie_init(); }

extern "C" { __declspec(dllexport) int eddie_get_interface_metric(int index, const char* layer); }

extern "C" { __declspec(dllexport) DWORD eddie_service_status(const char* serviceId); }

extern "C" { __declspec(dllexport) BOOL eddie_is_process_elevated(); }

extern "C" { __declspec(dllexport) void eddie_curl(const char* jRequest, unsigned int resultMaxLen, char* jResult); }

#endif