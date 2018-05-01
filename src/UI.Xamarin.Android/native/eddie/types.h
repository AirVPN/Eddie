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

#ifndef EDDIE_ANDROID_NATIVE_TYPES_H
#define EDDIE_ANDROID_NATIVE_TYPES_H

#include <stdint.h>

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

EDDIE_NAMESPACE_BEGIN()

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

typedef int8_t			e_int8;
typedef int16_t		 	e_int16;
typedef int32_t			e_int32;
typedef int64_t			e_int64;
typedef uint8_t		 	e_uint8;
typedef uint16_t		e_uint16;
typedef uint32_t		e_uint32;
typedef uint64_t		e_uint64;

typedef e_uint8			e_byte;

typedef std::lock_guard<std::mutex> mutex_lock;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

EDDIE_NAMESPACE_END()

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#endif // EDDIE_ANDROID_NATIVE_TYPES_H
