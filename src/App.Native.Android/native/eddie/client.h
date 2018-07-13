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

#ifndef EDDIE_ANDROID_NATIVE_CLIENT_H
#define EDDIE_ANDROID_NATIVE_CLIENT_H

#include "api.h"

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

EDDIE_NAMESPACE_BEGIN()

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

class Client
{
private:
	class Impl;

// Construction
public:
	Client(ovpn3_client *ci);
	virtual ~Client();

// Attributes
public:
	void getTransportStats(ovpn3_transport_stats &stats) const;
	void setOption(const std::string &name, const std::string &value);

// Operations
public:
	static void init();
	static void cleanup();

public:
	void loadProfileFile(const std::string &filename);	
	void loadProfileString(const std::string &str);

	void start();
	void stop();

	void pause(const std::string &reason);
	void resume();

	void finalize();

private:
	void applyProfiles();

private:
	std::unique_ptr<Impl> m_impl;
	std::list<std::string> m_profiles;
};

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

EDDIE_NAMESPACE_END()

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#endif // EDDIE_ANDROID_NATIVE_CLIENT_H

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
