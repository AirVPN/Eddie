// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2023 AirVPN (support@airvpn.org) / https://airvpn.org
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

#include "../../Lib.CLI.Elevated/include/iposix.h"

extern "C" 
{
	#include "wireguard.h"
}

class Impl : public IPosix
{
	// Virtual
protected:
	virtual int Main();
	virtual void Do(const std::string& id, const std::string& command, std::map<std::string, std::string>& params);
	virtual bool IsServiceInstalled();
	virtual bool ServiceInstall();
	virtual bool ServiceUninstall();

	virtual std::vector<std::string> GetNetworkInterfacesNames();	
	
	virtual std::string CheckIfClientPathIsAllowed(const std::string& path);

	// Virtual Pure, OS
protected:
	virtual std::string GetProcessPathCurrent();
	virtual std::string GetProcessPathOfId(int pid);

private:

	bool m_hasSystemdResolved = false;

	// Private
	int FileImmutableSet(const std::string& path, const int flag);
	std::string IptablesExecutable(const std::string& compatibility, const std::string& layer, const std::string& action);
	std::string IptablesExec(const std::string& path, const std::vector<std::string>& args, const bool stdinWrite, const std::string stdinBody);
	std::string NftablesSearchHandle(const std::string& rulesList, const std::string& comment);
	std::string GetRoutesAsJson();
	std::string GetRoutesAsJsonNew();
	std::string GetRoutesAsJsonHexAddress2string(const std::string& v);
	int GetRoutesAsJsonConvertMaskToCidrNetMask(const std::string& v);
	int GetRoutesAsJsonConvertHexPrefixToCidrNetMask(const std::string& v);
	unsigned long WireGuardLastHandshake(const std::string& interfaceId);
	void WireGuardParseAllowedIPs(const char *allowed_ips, wg_peer *peer);
};

