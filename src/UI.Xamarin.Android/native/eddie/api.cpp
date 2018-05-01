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
#include "api.h"

#include "breakpad.h"
#include "client.h"
#include "utils.h"

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

EDDIE_NAMESPACE_USE()

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

typedef std::unordered_map<int, std::shared_ptr<Client> > ClientsMap;

static bool g_initialized = false;
static std::mutex g_apiCS;
static int g_clientsID = 1;
static std::unique_ptr<ClientsMap> g_clientsPtr;

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

static std::shared_ptr<Client> get_client(int client)
{
	mutex_lock lock(g_apiCS);

	EDDIE_EXCEPT_IF(g_clientsPtr == nullptr, "internal error (g_clientsPtr is null)");

	ClientsMap::const_iterator i = g_clientsPtr->find(client);
	return i != g_clientsPtr->end() ? i->second : nullptr;
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifdef __cplusplus
extern "C" {
#endif

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

int ovpn3_version()
{
	return EDDIE_API_VERSION;
}

int ovpn3_init()
{
	try
	{
		utils::log_debug("ovpn3 init...");

		mutex_lock lock(g_apiCS);

		EDDIE_EXCEPT_IF(g_initialized, "ovpn3 already initialized");

		EDDIE_EXCEPT_IF(breakpad_init() == false, "breakpad initialization failed");

		Client::init();

		g_clientsPtr.reset(new ClientsMap());

		utils::log_debug("ovpn3 init completed");

		g_initialized = true;

		return EDDIE_API_SUCCESS;
	}
	catch(std::exception &e)
	{
		utils::log_error(utils::format("ovpn3_init failed: %s", e.what()));
	}
	catch(...)
	{
		utils::log_error("Unknown error in ovpn3_init");
	}	
	
	return EDDIE_API_ERROR;
}

int ovpn3_cleanup()
{
	try
	{
		utils::log_debug("ovpn3 cleanup...");

		mutex_lock lock(g_apiCS);

		EDDIE_EXCEPT_IF(g_initialized == false, "ovpn3 not initialized");

		if(g_clientsPtr != nullptr)
		{
			for(ClientsMap::const_iterator i = g_clientsPtr->begin(); i != g_clientsPtr->end(); ++i)
			{
				i->second->finalize();
			}

			g_clientsPtr->clear();
			g_clientsPtr.reset();
		}

		Client::cleanup();
		
		breakpad_cleanup();

		utils::log_debug("ovpn3 cleanup completed");

		g_initialized = false;

		return EDDIE_API_SUCCESS;
	}
	catch(std::exception &e)
	{
		utils::log_error(utils::format("ovpn3_cleanup failed: %s", e.what()));
	}
	catch(...)
	{
		utils::log_error("Unknown error in ovpn3_cleanup");
	}

	return EDDIE_API_ERROR;
}

int ovpn3_client_create(ovpn3_client *ci)
{
	try
	{
		utils::log_debug("ovpn3_client_create");
		
		EDDIE_EXCEPT_IF(ci == nullptr, "invalid ovpn3_client pointer");

		mutex_lock lock(g_apiCS);

		EDDIE_EXCEPT_IF(g_initialized == false, "ovpn3 not initialized");

		std::shared_ptr<Client> client(new Client(ci));
		int clientID = g_clientsID++;
		(*g_clientsPtr)[clientID] = client;

		return clientID;		
	}
	catch(std::exception &e)
	{
		utils::log_error(utils::format("ovpn3_client_create failed: %s", e.what()));
	}
	catch(...)
	{
		utils::log_error("Unknown error in ovpn3_client_create");
	}

	return EDDIE_API_ERROR;
}

int ovpn3_client_destroy(int client)
{
	try
	{
		utils::log_debug(utils::format("ovpn3_client_destroy '%d'", client));

		mutex_lock lock(g_apiCS);
		
		EDDIE_EXCEPT_IF(g_initialized == false, "ovpn3 not initialized");

		ClientsMap::iterator i = g_clientsPtr->find(client);
		EDDIE_EXCEPT_IF(i == g_clientsPtr->end(), utils::format("invalid client handle '%d'", client));

		i->second->finalize();
		g_clientsPtr->erase(i);
		
		return EDDIE_API_SUCCESS;
	}
	catch(std::exception &e)
	{
		utils::log_error(utils::format("ovpn3_client_destroy failed: %s", e.what()));
	}
	catch(...)
	{
		utils::log_error("Unknown error in ovpn3_client_destroy");
	}

	return EDDIE_API_ERROR;
}

int ovpn3_client_start(int client)
{
	try
	{
		utils::log_debug(utils::format("ovpn3_client_start '%d'", client));

		std::shared_ptr<Client> c = get_client(client);
		EDDIE_EXCEPT_IF(c == nullptr, utils::format("invalid client handle '%d'", client));
				
		utils::log_debug(utils::format("client '%d' starting", client));

		c->start();

		utils::log_debug(utils::format("client '%d' started", client));
		
		return EDDIE_API_SUCCESS;
	}
	catch(std::exception &e)
	{
		utils::log_error(utils::format("ovpn3_client_start failed: %s", e.what()));
	}
	catch(...)
	{
		utils::log_error("Unknown error in ovpn3_client_start");
	}

	return EDDIE_API_ERROR;
}

int ovpn3_client_stop(int client)
{
	try
	{
		utils::log_debug(utils::format("ovpn3_client_stop '%d'", client));

		std::shared_ptr<Client> c = get_client(client);
		EDDIE_EXCEPT_IF(c == nullptr, utils::format("invalid client handle '%d'", client));
		
		utils::log_debug(utils::format("client '%d' stopping", client));

		c->stop();		

		utils::log_debug(utils::format("client '%d' stopped", client));

		return EDDIE_API_SUCCESS;
	}
	catch(std::exception &e)
	{
		utils::log_error(utils::format("ovpn3_client_stop failed: %s", e.what()));
	}
	catch(...)
	{
		utils::log_error("Unknown error in ovpn3_client_stop");
	}

	return EDDIE_API_ERROR;
}

int ovpn3_client_pause(int client, const char *reason)
{
	try
	{
		utils::log_debug(utils::format("ovpn3_client_pause '%d'", client));

		std::shared_ptr<Client> c = get_client(client);
		EDDIE_EXCEPT_IF(c == nullptr, utils::format("invalid client handle '%d'", client));

		utils::log_debug(utils::format("client '%d' pausing", client));

		c->pause(reason != nullptr ? reason : "");

		utils::log_debug(utils::format("client '%d' paused", client));

		return EDDIE_API_SUCCESS;
	}
	catch(std::exception &e)
	{
		utils::log_error(utils::format("ovpn3_client_pause failed: %s", e.what()));
	}
	catch(...)
	{
		utils::log_error("Unknown error in ovpn3_client_pause");
	}

	return EDDIE_API_ERROR;
}

int ovpn3_client_resume(int client)
{
	try
	{
		utils::log_debug(utils::format("ovpn3_client_resume '%d'", client));

		std::shared_ptr<Client> c = get_client(client);
		EDDIE_EXCEPT_IF(c == nullptr, utils::format("invalid client handle '%d'", client));

		utils::log_debug(utils::format("client '%d' resuming", client));

		c->resume();

		utils::log_debug(utils::format("client '%d' resumed", client));

		return EDDIE_API_SUCCESS;
	}
	catch(std::exception &e)
	{
		utils::log_error(utils::format("ovpn3_client_resume failed: %s", e.what()));
	}
	catch(...)
	{
		utils::log_error("Unknown error in ovpn3_client_resume");
	}

	return EDDIE_API_ERROR;
}

int ovpn3_client_get_transport_stats(int client, ovpn3_transport_stats *stats)
{
	try
	{
		EDDIE_EXCEPT_IF(stats == nullptr, "stats is null");

		utils::log_debug(utils::format("ovpn3_client_transport_stats '%d'", client));

		std::shared_ptr<Client> c = get_client(client);
		EDDIE_EXCEPT_IF(c == nullptr, utils::format("invalid client handle '%d'", client));
		c->getTransportStats(*stats);

		return EDDIE_API_SUCCESS;
	}
	catch(std::exception &e)
	{
		utils::log_error(utils::format("ovpn3_client_transport_stats failed: %s", e.what()));
	}
	catch(...)
	{
		utils::log_error("Unknown error in ovpn3_client_transport_stats");
	}

	return EDDIE_API_ERROR;
}

int ovpn3_client_load_profile_file(int client, const char *filename)
{
	try
	{
		EDDIE_EXCEPT_IF(filename == nullptr, "filename is null");

		utils::log_debug(utils::format("ovpn3_client_load_profile_file '%d' - '%s'", client, filename));

		std::shared_ptr<Client> c = get_client(client);
		EDDIE_EXCEPT_IF(c == nullptr, utils::format("invalid client handle '%d'", client));
		c->loadProfileFile(filename);

		return EDDIE_API_SUCCESS;
	}
	catch(std::exception &e)
	{
		utils::log_error(utils::format("ovpn3_client_load_profile_file failed: %s", e.what()));
	}
	catch(...)
	{
		utils::log_error("Unknown error in ovpn3_client_load_profile_file");
	}

	return EDDIE_API_ERROR;
}

int ovpn3_client_load_profile_string(int client, const char *str)
{
	try
	{
		EDDIE_EXCEPT_IF(str == nullptr, "str is null");

		utils::log_debug(utils::format("ovpn3_client_load_profile_string '%d' - '%s'", client, str));

		std::shared_ptr<Client> c = get_client(client);
		EDDIE_EXCEPT_IF(c == nullptr, utils::format("invalid client handle '%d'", client));
		c->loadProfileString(str);

		return EDDIE_API_SUCCESS;
	}
	catch(std::exception &e)
	{
		utils::log_error(utils::format("ovpn3_client_load_profile_string failed: %s", e.what()));
	}
	catch(...)
	{
		utils::log_error("Unknown error in ovpn3_client_load_profile_string");
	}

	return EDDIE_API_ERROR;
}

int ovpn3_client_set_option(int client, const char *option, const char *value)
{
	try
	{
		EDDIE_EXCEPT_IF(option == nullptr, "option is null");
		EDDIE_EXCEPT_IF(value == nullptr, "value is null");

		utils::log_debug(utils::format("ovpn3_client_set_option '%d' - '%s:%s'", client, option, value));

		std::shared_ptr<Client> c = get_client(client);
		EDDIE_EXCEPT_IF(c == nullptr, utils::format("invalid client handle '%d'", client));
		c->setOption(option, value);

		return EDDIE_API_SUCCESS;
	}
	catch(std::exception &e)
	{
		utils::log_error(utils::format("ovpn3_client_set_option failed: %s", e.what()));
	}
	catch(...)
	{
		utils::log_error("Unknown error in ovpn3_client_set_option");
	}

	return EDDIE_API_ERROR;
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifdef __cplusplus
}
#endif

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
