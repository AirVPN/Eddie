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

#include "stdafx.h"
#include "utils.h"

#include <android/log.h>
#include <boost/lexical_cast.hpp>

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

EDDIE_NAMESPACE_BEGIN()

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

static std::string formatArgs(const char *s, va_list args)
{
	std::string str;
	if(s != nullptr)
	{
		int characters = vsnprintf(nullptr, 0, s, args);
		if(characters > 0)
		{
			characters++;	// terminator

			char *buffer = static_cast<char *>(malloc(characters));

			int result = vsnprintf(buffer, characters, s, args);
			if(result > 0)
				str.assign(buffer, result);

			free(buffer);
		}
	}

	return str;
}

static void log_message(int level, const std::string &message)
{
	__android_log_write(level, EDDIE_LOG_TAG, message.c_str());
}

static inline void left_trim(std::string &str) 
{
	str.erase(str.begin(), std::find_if(str.begin(), str.end(), [](int c) { return !std::isspace(c); }));
}

static inline void right_trim(std::string &str) 
{
	str.erase(std::find_if(str.rbegin(), str.rend(), [](int c) { return !std::isspace(c); }).base(), str.end());
}

template <typename T, typename S>
inline S number_to_string(const T &v)
{
	try
	{
		return boost::lexical_cast<S>(v);
	}
	catch(boost::bad_lexical_cast &)
	{

	}

	return S();
}

template <typename T, typename S>
inline T string_to_number(const S &v)
{
	try
	{
		if(v.empty())
			return 0;

		return boost::lexical_cast<T>(v);
	}
	catch(boost::bad_lexical_cast &)
	{

	}

	return 0;
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

std::string utils::format(const char *s, ...)
{
	va_list args;
	va_start(args, s);

	std::string str = formatArgs(s, args);

	va_end(args);

	return str;
}

void utils::trim(std::string &str)
{
	left_trim(str);
	right_trim(str);
}

std::string utils::trim_copy(const std::string &str)
{
	std::string copy(str);
	trim(copy);
	return copy;
}

void utils::log_debug(const std::string &message)
{
	log_message(ANDROID_LOG_DEBUG, message);
}

void utils::log_info(const std::string &message)
{
	log_message(ANDROID_LOG_INFO, message);
}

void utils::log_error(const std::string &message)
{
	log_message(ANDROID_LOG_ERROR, message);
}

template<>
std::string utils::to_string<bool>(const bool &v)
{
	return v ? "true" : "false";
}

template<>
bool utils::from_string<bool>(const std::string &v)
{
	return v == "true";
}

template<>
std::string utils::to_string<int>(const int &v)
{
	return number_to_string<int, std::string>(v);
}

template<>
int utils::from_string<int>(const std::string &v)
{
	return string_to_number<int, std::string>(v);
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

EDDIE_NAMESPACE_END()

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
