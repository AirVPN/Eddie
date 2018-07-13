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

#ifndef EDDIE_ANDROID_NATIVE_MACROS_H
#define EDDIE_ANDROID_NATIVE_MACROS_H

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#define EDDIE_OVPN3_GUI_VERSION				"Eddie.Android"

#define EDDIE_ZEROMEMORY(dest, len)			memset((dest), 0, (len))

#define EDDIE_OVERRIDE						override

#define EDDIE_LOG_TAG						"Eddie.Android.Native"

#define EDDIE_NAMESPACE_NAME				eddie
#define EDDIE_NAMESPACE_BEGIN()				namespace EDDIE_NAMESPACE_NAME {
#define EDDIE_NAMESPACE_END()				}
#define EDDIE_NAMESPACE_USE()				using namespace EDDIE_NAMESPACE_NAME;

#define EDDIE_EXCEPT(message)				throw EDDIE_NAMESPACE_NAME::Exception(message)
#define EDDIE_EXCEPT_IF(cond, message)		if(cond) throw EDDIE_NAMESPACE_NAME::Exception(message)

#define EDDIE_API_VERSION					3

#define EDDIE_API_SUCCESS					0
#define EDDIE_API_ERROR						-1

#define EDDIE_SUCCEEDED(v)					((v) >= 0)
#define EDDIE_FAILED(v)						((v) < 0)

#define EDDIE_BOOL_CAST(v)					((v) ? true : false)
#define EDDIE_FLAG_CAST(v)					((v) ? 1: 0)

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#endif // EDDIE_ANDROID_NATIVE_MACROS_H
