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
#include "breakpad.h"

#include "client/linux/handler/exception_handler.h"
#include "client/linux/handler/minidump_descriptor.h"
#include "utils.h"

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

EDDIE_NAMESPACE_BEGIN()

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

static std::unique_ptr<google_breakpad::MinidumpDescriptor> g_breakpadMinidumpDescriptor = nullptr;
static std::unique_ptr<google_breakpad::ExceptionHandler> g_breakpadExceptionHandler = nullptr;

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

static bool breakpad_dump_callback(const google_breakpad::MinidumpDescriptor &descriptor, void *context, bool succeeded) 
{
	utils::log_error(utils::format("breakpad_dump_callback (dump path:' %s')", descriptor.path()));	

	return succeeded;
}

bool breakpad_init()
{
	utils::log_debug("breakpad_init");

	EDDIE_EXCEPT_IF(g_breakpadMinidumpDescriptor != nullptr, "breakpad already initialized");

	//g_breakpadMinidumpDescriptor.reset(new google_breakpad::MinidumpDescriptor("/data/data/com.eddie.android/cache"));	
	g_breakpadMinidumpDescriptor.reset(new google_breakpad::MinidumpDescriptor("/sdcard"));
	g_breakpadExceptionHandler.reset(new google_breakpad::ExceptionHandler(*g_breakpadMinidumpDescriptor.get(), nullptr, breakpad_dump_callback, nullptr, true, -1));
		
	return true;
}

void breakpad_cleanup()
{
	utils::log_debug("breakpad_cleanup");

	g_breakpadExceptionHandler.reset();
	g_breakpadMinidumpDescriptor.reset();
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

EDDIE_NAMESPACE_END()

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
