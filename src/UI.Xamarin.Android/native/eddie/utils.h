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

#ifndef EDDIE_ANDROID_NATIVE_UTILS_H
#define EDDIE_ANDROID_NATIVE_UTILS_H

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

EDDIE_NAMESPACE_BEGIN()

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

class utils
{
public:
	static std::string format(const char *s, ...);
	
	static void trim(std::string &str);
	static std::string trim_copy(const std::string &str);

	static void log_debug(const std::string &message);
	static void log_info(const std::string &message);
	static void log_error(const std::string &message);

	template<typename T>
	static std::string to_string(const T &v);

	template<typename T>
	static T from_string(const std::string &v);

	template<typename T>
	static T from_string(const std::string &v, const T &defaultValue);
};

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

template<typename T>
T utils::from_string(const std::string &v, const T &defaultValue)
{
	if(v.empty())
		return defaultValue;

	return from_string<T>(v);
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

EDDIE_NAMESPACE_END()

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#endif // EDDIE_ANDROID_NATIVE_UTILS_H
