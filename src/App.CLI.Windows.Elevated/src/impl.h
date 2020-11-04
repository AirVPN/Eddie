// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2016 AirVPN (support@airvpn.org) / https://airvpn.org
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

#include "iwindows.h"

class Impl :public IWindows
{
public:
	int Main();
    virtual void Do(const std::string& id, const std::string& command, std::map<std::string, std::string>& params);
	virtual bool IsServiceInstalled();
	virtual bool ServiceInstall();
	virtual bool ServiceUninstall();
    virtual bool ServiceUninstallSupportRealtime();
	
	// Virtual Pure, Other
protected:
	virtual std::string CheckIfClientPathIsAllowed(const std::string& path);
	virtual void CheckIfExecutableIsAllowed(const std::string& path);
	virtual int GetProcessIdMatchingIPEndPoints(struct sockaddr_in& addrClient, struct sockaddr_in& addrServer);

	// Private
private:
	int SetInterfaceMetric(const int index, const std::string layer, const int value);

	// WFP
	HANDLE m_wfpEngineHandle;
	GUID m_wfpSubLayerGUID;
	std::string m_wfpSubLayerName;
	std::wstring m_wfpSubLayerWName;
	std::wstring m_wfpServiceWName;
	std::string m_wfpLastError;
	UINT32 m_wfpLastErrorCode;
	std::vector<UINT64> m_wfpFilters;

	UINT16 m_wfpMaxSubLayerWeight = 65535;
	UINT64 m_wfpMaxFilterWeight = 18446744073709551615;
	UINT64 m_wfpZeroInterface = 0;

	DWORD WfpInterfaceCreate(bool bDynamic);
	DWORD WfpInterfaceDelete();
	DWORD WfpBindInterface(const char* name, std::string weight, bool persistent);
	DWORD WfpUnbindInterface();
	void WfpInit(const std::string& name);
	bool WfpStart(const std::string& xml);
	bool WfpStop();
	UINT64 WfpRuleAdd(const std::string& xml);
	bool WfpRuleRemove(const UINT64 id);
	bool WfpRuleRemoveDirect(const UINT64 id);
	bool WfpRemovePending(const std::string& filterName, const std::string& xml);
	const char* WfpGetLastError();
	DWORD WfpGetLastErrorCode();
};

