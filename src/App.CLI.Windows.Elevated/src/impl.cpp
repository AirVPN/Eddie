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

/******************************************************************************
Original WFP source code by Mahesh S - swatkat_thinkdigit@yahoo.co.in - http://swatrant.blogspot.com/
Some code by ValdikSS
******************************************************************************/

// Note the order: must be included before windows.h
#include <winsock2.h>
#include "impl.h"
#include <Ws2tcpip.h>
#include <ws2ipdef.h>
#include <iphlpapi.h>

#include <fcntl.h>

#include <sys/stat.h>

#include <sys/types.h> // for signal()
#include <signal.h> // for signal();
#include <psapi.h>


// WFP
#define XMLBUFSIZE 4096
#define MALLOC(x) HeapAlloc(GetProcessHeap(), 0, (x)) // WFP
#define FREE(x) HeapFree(GetProcessHeap(), 0, (x)) // WFP
#include <fwpmu.h> // WFP

#pragma comment(lib, "wsock32.lib")
#pragma comment(lib, "Ws2_32.lib")
#pragma comment(lib, "IPHLPAPI.lib")
#pragma comment(lib, "fwpuclnt.lib")
#pragma comment(lib, "rpcrt4.lib")

#include "yxml.h" // WFP utils, maybe converted in JSON in future

std::string serviceName = "EddieElevationService";
std::string serviceDisplayName = "Eddie Elevation Service";
std::string serviceDisplayDesc = "Eddie Elevation Service";

int Impl::Main()
{
	signal(SIGINT, SIG_IGN); // See comment in Linux implementation

	WSADATA	WSAData;
	if (WSAStartup(MAKEWORD(2, 2), &WSAData))
	{
		ThrowException("Error in sockets initialization");
	}

	int result = IBase::Main();

	WSACleanup();

	return result;	
}

void Impl::Do(const std::string& commandId, const std::string& command, std::map<std::string, std::string>& params)
{
	if (command == "compatibility-remove-task")
	{
		// Remove old <2.17.3 task
		ShellEx1(FsLocateExecutable("schtasks.exe"), "/delete /tn AirVPN /f");
		ShellEx1(FsLocateExecutable("schtasks.exe"), "/delete /tn Eddie /f");
	}
	else if (command == "compatibility-profiles")
	{
		std::string dataPath = params["path-data"];
		if (FsDirectoryExists(dataPath) == false)
		{
			std::string appPath = params["path-app"];

			std::vector<std::string> filesToMove;
			filesToMove.push_back("AirVPN.xml");
			filesToMove.push_back("default.xml");
			filesToMove.push_back("Recovery.xml");
			filesToMove.push_back("winfirewall_rules_original.wfw");
			filesToMove.push_back("winfirewall_rules_backup.wfw");
			filesToMove.push_back("winfirewallrules.wfw");
			filesToMove.push_back("winfirewall_rules_original.airvpn");

			FsDirectoryCreate(dataPath);

			if(appPath != dataPath)
			{
				// Old Eddie <2.17.3 save data in C:\Program Files\... . Move now.
				for (std::vector<std::string>::const_iterator i = filesToMove.begin(); i != filesToMove.end(); ++i)
				{
					std::string filename = *i;
					std::string fileOldPath = appPath + FsPathSeparator + filename;
					std::string fileNewPath = dataPath + FsPathSeparator + filename;
					if(FsFileExists(fileOldPath))
					{
						FsFileMove(fileOldPath, fileNewPath);
					}
				}
			}

			std::size_t posLastSlash = dataPath.find_last_of("\\");
			if (posLastSlash != std::string::npos)
			{
				std::string oldDataPath = dataPath.substr(0, posLastSlash) + FsPathSeparator + "AirVPN";
			
				if (FsDirectoryExists(oldDataPath))
				{
					// Old Eddie <2.17.3 save data in AirVPN folder... . Move now.
					for (std::vector<std::string>::const_iterator i = filesToMove.begin(); i != filesToMove.end(); ++i)
					{
						std::string filename = *i;
						std::string fileOldPath = oldDataPath + FsPathSeparator + filename;
						std::string fileNewPath = dataPath + FsPathSeparator + filename;
						if (FsFileExists(fileOldPath))
						{
							FsFileMove(fileOldPath, fileNewPath);
						}
					}

					FsDirectoryDelete(oldDataPath, true);
				}
			}
		}		
	}
	else if (command == "wfp")
	{
		std::string action = params["action"];
		if (action == "init")
		{
			std::string name = params["name"];
			WfpInit(name);
		}
		else if (action == "start")
		{
			std::string xml = params["xml"];
			bool result = WfpStart(xml);
			ReplyCommand(commandId, (result ? "1" : "0"));
		}
		else if (action == "stop")
		{
			WfpStop();
		}
		else if (action == "rule-add")
		{
			std::string xml = params["xml"];
			UINT64 ruleId = WfpRuleAdd(xml.c_str());
			ReplyCommand(commandId, std::to_string(ruleId));
		}
		else if (action == "rule-remove")
		{
			UINT64 ruleId = _atoi64(params["id"].c_str());
			bool result = WfpRuleRemove(ruleId);
			ReplyCommand(commandId, (result ? "1" : "0"));
		}
		else if (action == "pending-remove")
		{
			std::string pathTemp = GetTempPath("wfp_rules.xml");

			ShellResult result = ShellEx1(FsLocateExecutable("netsh.exe"), "WFP Show Filters file=\"" + pathTemp + "\"");
			std::string xml = FsFileReadText(pathTemp);
			FsFileDelete(pathTemp);

			bool found = WfpRemovePending(params["name"], xml);

			ReplyCommand(commandId, (found ? "1" : "0"));
		}
		else if (action == "last-error")
		{
			ReplyCommand(commandId, m_wfpLastError);
		}
	}
	else if (command == "dns-flush")
	{
		std::string mode = params["mode"];

		if (mode == "max")
		{
			ShellEx1(FsLocateExecutable("net.exe"), "stop dnscache");
			ShellEx1(FsLocateExecutable("net.exe"), "start dnscache");
			ShellEx1(FsLocateExecutable("ipconfig.exe"), "/registerdns");
		}

		ShellEx1(FsLocateExecutable("ipconfig.exe"), "/flushdns");
	}
	else if (command == "set-interface-metric")
	{
		int idx = atoi(params["idx"].c_str());
		int value = atoi(params["value"].c_str());
		std::string layer = params["layer"];
		SetInterfaceMetric(idx, layer, value);
	}
	else if (command == "route")
	{		
		ShellResult shellResult;
		if (params["layer"] == "ipv4")
		{
			std::string args = "";
			if (params["action"] == "add")
				args += "add ";
			else if (params["action"] == "remove")
				args += "delete ";
			else
				ThrowException("Unknown action");

			args += StringEnsureIpAddress(params["address"]) + " mask " + StringEnsureIpAddress(params["mask"]) + " " + StringEnsureIpAddress(params["gateway"]);
			args += " if " + StringEnsureNumericInt(params["iface"]);
			// Metric param are ignored or misinterpreted. http://serverfault.com/questions/238695/how-can-i-set-the-metric-of-a-manually-added-route-on-windows
			// if(parameters.ContainsKey("metric"))
			// cmd += " metric " + Utils.EnsureStringNumericInt(parameters["metric"]);
			shellResult = ShellEx1(FsLocateExecutable("route.exe"), args);			
		}
		else if (params["layer"] == "ipv6")
		{
			std::string args = "interface ipv6";
			if (params["action"] == "add")
				args += " add";
			else if (params["action"] == "remove")
				args += " del";
			else
				ThrowException("Unknown action");
			args += " route";
			args += " prefix=\"" + StringEnsureCidr(params["cidr"]) + "\"";
			args += " interface=\"" + StringEnsureNumericInt(params["iface"]) + "\"";
			args += " nexthop=\"" + StringEnsureIpAddress(params["gateway"]) + "\"";
			if (params.find("metric") != params.end())
				args += " metric=" + StringEnsureNumericInt(params["metric"]);
			shellResult = ShellEx1(FsLocateExecutable("netsh.exe"), args);			
		}

		bool accept = false;
		std::string resultStdOut = StringTrim(StringToLower(shellResult.out), "\r\n !.");
		std::string resultBoth = StringTrim(StringToLower(shellResult.output()));
		if (shellResult.exit == 0)
		{	
			if (resultStdOut == "ok")
				accept = true;
			if ((params["action"] == "add") && (StringContain(resultBoth, "the object already exists")))
				accept = true;
			if ((params["action"] == "remove") && (StringContain(resultBoth, "the system cannot find the file specified")))
				accept = true;
			if ((params["action"] == "remove") && (StringContain(resultBoth, "network interface no more available"))) // Win7
				accept = true;			
			if ((params["action"] == "remove") && (StringContain(resultBoth, "element not found.")))
				accept = true;
		}
		else
		{
			// IPv4 route that already exists return exitcode=0 if ipv4, exitcode=1 if ipv6, MS unconsistency.
			if ((params["action"] == "add") && (StringContain(resultBoth, "the object already exists")))
				accept = true;
		}

		if (accept)
			ReplyCommand(commandId, "1");
		else
			ThrowException(shellResult.dump());
	}
	else if (command == "windows-dns")
	{
		std::string interfaceName = params["interface"];
		std::string layer = ((params["layer"] == "ipv4") ? "ipv4" : "ipv6");
		std::string mode = params["mode"];
		if (mode == "dhcp")
		{
			ShellEx1(FsLocateExecutable("netsh.exe"), "interface " + layer + " set dns name=\"" + StringEnsureInterfaceName(interfaceName) + "\" source=dhcp register=primary validate=no");
		}
		else if (mode == "static")
		{
			std::string ipaddress = params["ipaddress"];
			ShellEx1(FsLocateExecutable("netsh.exe"), "interface " + layer + " set dns name=\"" + StringEnsureInterfaceName(interfaceName) + "\" source=static address=" + StringEnsureIpAddress(ipaddress) + " register=primary validate=no");
		}
		else if (mode == "add")
		{
			std::string ipaddress = params["ipaddress"];
			ShellEx1(FsLocateExecutable("netsh.exe"), "interface " + layer + " add dnsserver name=\"" + StringEnsureInterfaceName(interfaceName) + "\" address=" + StringEnsureIpAddress(ipaddress) + " validate=no");
		}
	}
	else if (command == "windows-firewall")
	{
		std::string args = StringTrim(params["args"]);
		ShellResult result = ShellEx1(FsLocateExecutable("netsh.exe"), "advfirewall " + args);
	}
	else if (command == "windows-workaround-25139")
	{
		std::string cidr = StringTrim(params["cidr"]);
		std::string iface = StringTrim(params["iface"]);
		ShellResult result = ShellEx1(FsLocateExecutable("netsh.exe"), "interface ipv6 del route \"" + StringEnsureCidr(cidr) + "\" interface=\"" + StringEnsureNumericInt(iface) + "\"");
	}
	else if (command == "windows-workaround-interface-up")
	{
		std::string name = StringTrim(params["name"]);
		ShellResult result = ShellEx1(FsLocateExecutable("netsh.exe"), "interface set interface \"" + StringEnsureInterfaceName(name) + "\" ENABLED");		
	}
	else
	{
		IWindows::Do(commandId, command, params);
	}
}

bool Impl::IsServiceInstalled()
{
	HKEY hKey;
	LPCTSTR sk = TEXT("SYSTEM\\CurrentControlSet\\Services\\EddieElevationService");
	LONG openRes = RegOpenKeyEx(HKEY_LOCAL_MACHINE, sk, 0, KEY_ALL_ACCESS, &hKey);
	LONG closeOut = RegCloseKey(hKey);

	return (openRes == ERROR_SUCCESS);
}

bool Impl::ServiceInstall()
{
	std::string elevatedPath = GetProcessPathCurrent();

	std::string path = FsFileGetDirectory(elevatedPath) + FsPathSeparator + "Eddie-Service-Elevated.exe";

	std::string elevatedArgs = "mode=service";
	std::string integrity = ComputeIntegrityHash(GetProcessPathCurrent(), "");
	if (m_cmdline.find("service_port") != m_cmdline.end())
		elevatedArgs += " service_port=" + StringEnsureSecure(m_cmdline["service_port"]);
	elevatedArgs += " integrity=" + StringEnsureSecure(integrity);

	// Can be active but old version that don't accept new client
	ShellEx1(FsLocateExecutable("net.exe"), "stop \"" + serviceName + "\"");
	ShellEx1(FsLocateExecutable("sc.exe"), "delete \"" + serviceName + "\"");

	ShellEx1(FsLocateExecutable("sc.exe"), "create \"" + serviceName + "\" binpath= \"" + path + "\" DisplayName= \"" + serviceDisplayName + "\" start= auto");

	// Write description and args
	HKEY hKey;
	LPCTSTR sk = TEXT("SYSTEM\\CurrentControlSet\\Services\\EddieElevationService");

	LONG openRes = RegOpenKeyEx(HKEY_LOCAL_MACHINE, sk, 0, KEY_ALL_ACCESS, &hKey);

	if (openRes != ERROR_SUCCESS)
		ThrowException("Registry update fail");

	std::wstring serviceDisplayDescW = StringUTF8ToWString(serviceDisplayDesc);
	LONG setResD = RegSetValueEx(hKey, TEXT("Description"), 0, REG_SZ, (LPBYTE)serviceDisplayDescW.c_str(), (DWORD)(serviceDisplayDescW.size() + 1) * sizeof(wchar_t));
	if (setResD != ERROR_SUCCESS)
		ThrowException("Registry update fail");

	std::wstring serviceArgs = StringUTF8ToWString(elevatedArgs);
	LONG setResA = RegSetValueEx(hKey, TEXT("EddieArgs"), 0, REG_SZ, (LPBYTE)serviceArgs.c_str(), (DWORD)(serviceArgs.size() + 1) * sizeof(wchar_t));
	if (setResA != ERROR_SUCCESS)
		ThrowException("Registry update fail");

	LONG closeOut = RegCloseKey(hKey);

	ShellEx1(FsLocateExecutable("net.exe"), "start \"" + serviceName + "\"");

	return 0;
}

bool Impl::ServiceUninstall()
{
	ShellResult r1 = ShellEx1(FsLocateExecutable("net.exe"), "stop \"" + serviceName + "\"");
	ShellResult r2 = ShellEx1(FsLocateExecutable("sc.exe"), "delete \"" + serviceName + "\"");

	return 0;
}

std::string Impl::CheckIfClientPathIsAllowed(const std::string& path)
{
#ifdef Debug
	return "ok";
#else
	/*
	// MacOS have equivalent. 
	// Linux don't have any signature check (and can't have, because in distro like Arch, binary are builded client-side from sources
	// This is probably a superfluous check, and can cause issue for who compile from sources.
	// If implemented, need a conversion from C# to C++ of the code below.

	// Check signature (optional)
	{
		try
		{
			System.Security.Cryptography.X509Certificates.X509Certificate c1 = System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromSignedFile(System.Reflection.Assembly.GetEntryAssembly().Location);

			// If above don't throw exception, Elevated it's signed, so it's mandatory that client is signed from same subject.
			try
			{
				System.Security.Cryptography.X509Certificates.X509Certificate c2 = System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromSignedFile(clientPath);

				bool match = (
					(c1.Issuer == c2.Issuer) &&
					(c1.Subject == c2.Subject) &&
					(c1.GetCertHashString() == c2.GetCertHashString()) &&
					(c1.GetEffectiveDateString() == c2.GetEffectiveDateString()) &&
					(c1.GetPublicKeyString() == c2.GetPublicKeyString()) &&
					(c1.GetRawCertDataString() == c2.GetRawCertDataString()) &&
					(c1.GetSerialNumberString() == c2.GetSerialNumberString())
					);

				if (match == false)
					return "Client not allowed: digital signature mismatch";
			}
			catch (Exception)
			{
				return "Client not allowed: digital signature not available";
			}
		}
		catch (Exception)
		{
			// Not signed, but maybe compiled from source, it's an optional check.
		}
	}
	*/

	return "ok";
#endif
}

void Impl::CheckIfExecutableIsAllowed(const std::string& path)
{
	// TOFIX - Missing some check like "path must be writable only by admin"
	
	std::string issues = "";

	if (CheckIfExecutableIsWhitelisted(path)) // If true, skip other checks.
		return;

	/*
	struct stat st;
	memset(&st, 0, sizeof(struct stat));
	if (stat(path.c_str(), &st) != 0)
	{
		issues += "Not readable;";
	}
	else
	{
		if (st.st_uid != 0)
		{
			issues += "Not owned by root;";
		}
		else if ((st.st_mode & S_ISUID) == 0)
		{
			if ((st.st_mode & S_IXUSR) == 0)
			{
				issues += "Not executable by owner;";
			}

			if ((st.st_mode & S_IWGRP) != 0)
			{
				issues += "Writable by group";
			}

			if ((st.st_mode & S_IWOTH) != 0)
			{
				issues += "Writable by other;";
			}
		}
	}
	*/

	if (issues != "")
		ThrowException("Executable '" + path + "' not allowed: " + issues);
}

int Impl::GetProcessIdMatchingIPEndPoints(struct sockaddr_in& addrClient, struct sockaddr_in& addrServer)
{
	std::vector<unsigned char> buffer;
	DWORD dwSize = sizeof(MIB_TCPTABLE_OWNER_PID);
	DWORD dwRetValue = 0;

	do {
		buffer.resize(dwSize, 0);
		dwRetValue = GetExtendedTcpTable(buffer.data(), &dwSize, TRUE, AF_INET, TCP_TABLE_OWNER_PID_ALL, 0);
	} while (dwRetValue == ERROR_INSUFFICIENT_BUFFER);
	if (dwRetValue == ERROR_SUCCESS)
	{
		PMIB_TCPTABLE_OWNER_PID ptTable = reinterpret_cast<PMIB_TCPTABLE_OWNER_PID>(buffer.data());
		for (DWORD i = 0; i < ptTable->dwNumEntries; i++) {
			DWORD pid = ptTable->table[i].dwOwningPid;

			if ((ptTable->table[i].dwLocalAddr == addrClient.sin_addr.s_addr) &&
				(ptTable->table[i].dwRemoteAddr == addrServer.sin_addr.s_addr) &&
				(ptTable->table[i].dwLocalPort == addrClient.sin_port) &&
				(ptTable->table[i].dwRemotePort == addrServer.sin_port)
				)
			{
				return pid;
			}
		}
	}

	return 0;
}

int Impl::SetInterfaceMetric(const int index, const std::string layer, const int value)
{
	DWORD err = 0;

	MIB_IPINTERFACE_ROW ipiface;
	InitializeIpInterfaceEntry(&ipiface);
	if (layer == "ipv4")
		ipiface.Family = AF_INET;
	else if (layer == "ipv6")
		ipiface.Family = AF_INET6;
	else
		return -1;
	ipiface.InterfaceIndex = index;
	err = GetIpInterfaceEntry(&ipiface);
	if (err == NO_ERROR)
	{
		if (ipiface.Family == AF_INET)
			ipiface.SitePrefixLength = 0; // required for IPv4 as per MSDN
		ipiface.Metric = value;
		if (value == 0)
			ipiface.UseAutomaticMetric = TRUE;
		else
			ipiface.UseAutomaticMetric = FALSE;
		err = SetIpInterfaceEntry(&ipiface);
		if (err == NO_ERROR)
			return 0;
	}

	return err;
}

DWORD Impl::WfpInterfaceCreate(bool bDynamic)
{
	DWORD dwFwAPiRetCode = ERROR_BAD_COMMAND;
	try
	{
		FWPM_SESSION0 session = { 0 };
		if (bDynamic)
			session.flags = FWPM_SESSION_FLAG_DYNAMIC;

		// Create packet filter interface.
		dwFwAPiRetCode = ::FwpmEngineOpen0(NULL,
			RPC_C_AUTHN_WINNT,
			NULL,
			&session,
			&m_wfpEngineHandle);
	}
	catch (...)
	{
	}
	return dwFwAPiRetCode;
}

DWORD Impl::WfpInterfaceDelete()
{
	DWORD dwFwAPiRetCode = ERROR_BAD_COMMAND;
	try
	{
		if (NULL != m_wfpEngineHandle)
		{
			// Close packet filter interface.
			dwFwAPiRetCode = ::FwpmEngineClose0(m_wfpEngineHandle);
			m_wfpEngineHandle = NULL;
		}
	}
	catch (...)
	{
	}
	return dwFwAPiRetCode;
}

DWORD Impl::WfpBindInterface(const char* name, std::string weight, bool persistent)
{
	DWORD dwFwAPiRetCode = ERROR_BAD_COMMAND;
	try
	{
		RPC_STATUS rpcStatus = { 0 };
		FWPM_SUBLAYER0 SubLayer = { 0 };

		// Create a GUID for our packet filter layer.
		rpcStatus = ::UuidCreate(&SubLayer.subLayerKey);
		if (NO_ERROR == rpcStatus)
		{
			// Save GUID.
			::CopyMemory(&m_wfpSubLayerGUID,
				&SubLayer.subLayerKey,
				sizeof(SubLayer.subLayerKey));

			// Populate packet filter layer information.
			SubLayer.displayData.name = &m_wfpSubLayerWName[0];
			SubLayer.displayData.description = &m_wfpSubLayerWName[0];

			if (persistent)
				SubLayer.flags = FWPM_SUBLAYER_FLAG_PERSISTENT;

			//SubLayer.flags = 0;
			SubLayer.weight = 0;

			if (weight == "auto")
				SubLayer.weight = 0x100;
			else if (weight == "max")
				SubLayer.weight = m_wfpMaxSubLayerWeight;
			else
			{
				SubLayer.weight = atoi(weight.c_str());
			}

			// Add packet filter to our interface.
			dwFwAPiRetCode = ::FwpmSubLayerAdd0(m_wfpEngineHandle,
				&SubLayer,
				NULL);
		}
	}
	catch (...)
	{
	}
	return dwFwAPiRetCode;
}

DWORD Impl::WfpUnbindInterface()
{
	DWORD dwFwAPiRetCode = ERROR_BAD_COMMAND;
	try
	{
		// Delete packet filter layer from our interface.
		dwFwAPiRetCode = ::FwpmSubLayerDeleteByKey0(m_wfpEngineHandle,
			&m_wfpSubLayerGUID);
		::ZeroMemory(&m_wfpSubLayerGUID, sizeof(GUID));
	}
	catch (...)
	{
	}
	return dwFwAPiRetCode;
}

void Impl::WfpInit(const std::string& name)
{
	// Initialize member variables.
	m_wfpEngineHandle = NULL;
	::ZeroMemory(&m_wfpSubLayerGUID, sizeof(GUID));

	std::string fullName = name;

	std::wstring fullWName(fullName.begin(), fullName.end());

	m_wfpSubLayerName = fullName;
	m_wfpSubLayerWName = fullWName;
	m_wfpServiceWName = fullWName;
}

bool Impl::WfpStart(const std::string& xml)
{
	BOOL bStarted = FALSE;
	std::string description;
	std::string weight;
	bool persistent = false;
	bool dynamic = false;

	std::string attrValue = "";
	void *buf = malloc(XMLBUFSIZE);
	yxml_t x;
	yxml_init(&x, buf, XMLBUFSIZE);
	const char* xmlE = xml.c_str();

	for (; *xmlE; xmlE++)
	{
		yxml_ret_t r = yxml_parse(&x, *xmlE);
		if (r < 0)
		{
			m_wfpLastError = "XML syntax error";
			m_wfpLastErrorCode = 0;
			return 0; // Syntax error
		}

		if (r == YXML_ATTRVAL)
			attrValue.append(x.data);

		switch (r)
		{
		case YXML_ATTREND:
		{
			std::string elemName = x.elem;
			std::string attrName = x.attr;

			if (attrName == "description")
				description = attrValue;
			else if (attrName == "weight")
				weight = attrValue;
			else if (attrName == "persistent")
				persistent = (attrValue == "true");
			else if (attrName == "dynamic")
				dynamic = (attrValue == "true");
			else
			{
				m_wfpLastError = "Unknown attribute '" + attrName + "'";
				m_wfpLastErrorCode = 0;
				return 0;
			}

			attrValue = "";
		}
		}
	}

	try
	{
		m_wfpLastError = "Failed to Create packet filter interface.";
		// Create packet filter interface.
		m_wfpLastErrorCode = WfpInterfaceCreate(dynamic);
		if (m_wfpLastErrorCode == ERROR_SUCCESS)
		{
			m_wfpLastError = "Failed to bind to packet filter interface.";
			// Bind to packet filter interface.
			m_wfpLastErrorCode = WfpBindInterface(description.c_str(), weight.c_str(), persistent);
			if (m_wfpLastErrorCode == ERROR_SUCCESS)
			{
				m_wfpLastError = "";
				m_wfpLastErrorCode = 0;
				bStarted = TRUE;
			}
		}
	}
	catch (...)
	{
		m_wfpLastError = "Unexpected error.";
		m_wfpLastErrorCode = 0;
	}
	return bStarted;
}

bool Impl::WfpStop()
{
	bool bStopped = false;
	try
	{
		DWORD dwFwAPiRetCode = ERROR_BAD_COMMAND;

		// Remove all filters.

		for (unsigned int i = 0; i < m_wfpFilters.size(); i++)
		{
			dwFwAPiRetCode = ::FwpmFilterDeleteById0(m_wfpEngineHandle, m_wfpFilters[i]);
		}
		m_wfpFilters.clear();

		m_wfpLastError = "Failed to unbind from packet filter interface.";
		// Unbind from packet filter interface.
		m_wfpLastErrorCode = WfpUnbindInterface();
		if (m_wfpLastErrorCode == ERROR_SUCCESS)
		{
			m_wfpLastError = "Failed to delete packet filter interface.";
			m_wfpLastErrorCode = WfpInterfaceDelete();
			// Delete packet filter interface.
			if (m_wfpLastErrorCode == ERROR_SUCCESS)
			{
				m_wfpLastError = "";
				m_wfpLastErrorCode = 0;
				bStopped = true;
			}
		}
	}
	catch (...)
	{
		m_wfpLastError = "Unexpected error.";
		m_wfpLastErrorCode = 0;
	}
	return bStopped;
}

struct WfpRuleData
{
	WfpRuleData()
	{
		::ZeroMemory(&ipAddressV4, sizeof(FWP_V4_ADDR_AND_MASK));
		::ZeroMemory(&ipAddressV6, sizeof(FWP_V6_ADDR_AND_MASK));
		::ZeroMemory(&portRange, sizeof(FWP_RANGE0));
		ipAddressV4.mask = ntohl(4294967295); // 255.255.255.255
		ipAddressV6.prefixLength = 128;
		filterWeight = 0;
	}

	FWP_V4_ADDR_AND_MASK ipAddressV4;
	FWP_V6_ADDR_AND_MASK ipAddressV6;
	FWP_RANGE0 portRange;
	UINT64 filterWeight;
};


UINT64 Impl::WfpRuleAdd(const std::string& xml)
{
	DWORD dwFwAPiRetCode = ERROR_BAD_COMMAND;
	UINT64 filterId = 0;

	// Init
	FWPM_FILTER0 Filter;
	FWPM_FILTER_CONDITION0 Condition[100] = { 0 };

	// Prepare filter.
	RtlZeroMemory(&Filter, sizeof(FWPM_FILTER0));

	Filter.subLayerKey = m_wfpSubLayerGUID;
	Filter.displayData.name = &m_wfpServiceWName[0];
	Filter.displayData.description = &m_wfpServiceWName[0];
	Filter.weight.type = FWP_EMPTY;
	Filter.numFilterConditions = 0;
	Filter.filterCondition = Condition;
	Filter.flags = FWPM_FILTER_FLAG_NONE;

	std::string attrValue = "";
	void *buf = malloc(XMLBUFSIZE);
	yxml_t x;
	yxml_init(&x, buf, XMLBUFSIZE);
	const char* xmlE = xml.c_str();

	int conditionIndex = -1;
	std::map<int, WfpRuleData> rulesMap;

	std::wstring ruleWName;

	for (; *xmlE; xmlE++) {
		yxml_ret_t r = yxml_parse(&x, *xmlE);
		if (r < 0)
		{
			m_wfpLastError = "XML syntax error";
			m_wfpLastErrorCode = 0;
			return 0; // Syntax error
		}

		if (r == YXML_ATTRVAL)
			attrValue.append(x.data);

		switch (r)
		{
		case YXML_ATTREND:
		{
			std::string elemName = x.elem;
			std::string attrName = x.attr;

			if (elemName == "rule")
			{
				if (attrName == "name")
				{
					ruleWName = std::wstring(attrValue.begin(), attrValue.end());
					Filter.displayData.description = &ruleWName[0];
				}
				else if (attrName == "persistent")
				{
					if (attrValue == "true")
					{
						Filter.flags = FWPM_FILTER_FLAG_PERSISTENT;
					}
				}
				else if (attrName == "enabled")
				{
					if (attrValue != "true")
					{
						m_wfpLastError = "Don't pass disabled rule.";
						m_wfpLastErrorCode = 0;
						return 0;
					}
				}
				else if (attrName == "layer")
				{
					if (attrValue == "ale_auth_recv_accept_v4")
						Filter.layerKey = FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V4;
					else if (attrValue == "ale_auth_recv_accept_v6")
						Filter.layerKey = FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V6;
					else if (attrValue == "ale_auth_connect_v4")
						Filter.layerKey = FWPM_LAYER_ALE_AUTH_CONNECT_V4;
					else if (attrValue == "ale_auth_connect_v6")
						Filter.layerKey = FWPM_LAYER_ALE_AUTH_CONNECT_V6;
					else if (attrValue == "ale_flow_established_v4")
						Filter.layerKey = FWPM_LAYER_ALE_FLOW_ESTABLISHED_V4;
					else if (attrValue == "ale_flow_established_v6")
						Filter.layerKey = FWPM_LAYER_ALE_FLOW_ESTABLISHED_V6;
					else
					{
						m_wfpLastError = "Unknown layer '" + attrValue + "'";
						m_wfpLastErrorCode = 0;
						return 0;
					}
				}
				else if (attrName == "action")
				{
					if (attrValue == "block")
					{
						Filter.action.type = FWP_ACTION_BLOCK;
					}
					else if (attrValue == "permit")
					{
						Filter.action.type = FWP_ACTION_PERMIT;
					}
					else
					{
						m_wfpLastError = "Unknown action '" + attrValue + "'";
						m_wfpLastErrorCode = 0;
						return 0;
					}
				}
				else if (attrName == "weight")
				{
					if (attrValue == "auto")
						Filter.weight.type = FWP_EMPTY;
					else if (attrValue == "max")
					{
						Filter.weight.type = FWP_UINT64;
						Filter.weight.uint64 = &m_wfpMaxFilterWeight;
					}
					else
					{
						WfpRuleData &ruleData = rulesMap[conditionIndex];

						Filter.weight.type = FWP_UINT64;
						ruleData.filterWeight = atoi(attrValue.c_str());
						Filter.weight.uint64 = &ruleData.filterWeight;
					}
				}
				else
				{
					m_wfpLastError = "Unknown attribute '" + attrName + "'";
					m_wfpLastErrorCode = 0;
					return 0;
				}
			}
			else if (elemName == "if")
			{
				if (attrName == "field")
				{
					conditionIndex++;
					rulesMap[conditionIndex] = WfpRuleData();

					// https://msdn.microsoft.com/en-us/library/windows/desktop/aa363996(v=vs.85).aspx

					if (attrValue == "ale_app_id")
					{
						Condition[conditionIndex].fieldKey = FWPM_CONDITION_ALE_APP_ID;
					}
					else if (attrValue == "ip_local_address")
					{
						Condition[conditionIndex].fieldKey = FWPM_CONDITION_IP_LOCAL_ADDRESS;
					}
					else if (attrValue == "ip_local_interface")
					{
						Condition[conditionIndex].fieldKey = FWPM_CONDITION_IP_LOCAL_INTERFACE;
					}
					else if (attrValue == "ip_local_port")
					{
						Condition[conditionIndex].fieldKey = FWPM_CONDITION_IP_LOCAL_PORT;
					}
					else if (attrValue == "ip_protocol")
					{
						Condition[conditionIndex].fieldKey = FWPM_CONDITION_IP_PROTOCOL;
					}
					else if (attrValue == "ip_remote_address")
					{
						Condition[conditionIndex].fieldKey = FWPM_CONDITION_IP_REMOTE_ADDRESS;
					}
					else if (attrValue == "ip_remote_port")
					{
						Condition[conditionIndex].fieldKey = FWPM_CONDITION_IP_REMOTE_PORT;
					}
					else
					{
						m_wfpLastError = "Unknown field '" + attrValue + "'";
						m_wfpLastErrorCode = 0;
						return 0;
					};
				}
				else if (attrName == "match")
				{
					if (attrValue == "equal")
					{
						Condition[conditionIndex].matchType = FWP_MATCH_EQUAL;
					}
					else if (attrValue == "greater")
					{
						Condition[conditionIndex].matchType = FWP_MATCH_GREATER;
					}
					else if (attrValue == "less")
					{
						Condition[conditionIndex].matchType = FWP_MATCH_LESS;
					}
					else if (attrValue == "greater_or_equal")
					{
						Condition[conditionIndex].matchType = FWP_MATCH_GREATER_OR_EQUAL;
					}
					else if (attrValue == "less_or_equal")
					{
						Condition[conditionIndex].matchType = FWP_MATCH_LESS_OR_EQUAL;
					}
					else if (attrValue == "range")
					{
						Condition[conditionIndex].matchType = FWP_MATCH_RANGE;
					}
					else if (attrValue == "flags_all_set")
					{
						Condition[conditionIndex].matchType = FWP_MATCH_FLAGS_ALL_SET;
					}
					else if (attrValue == "flags_any_set")
					{
						Condition[conditionIndex].matchType = FWP_MATCH_FLAGS_ANY_SET;
					}
					else if (attrValue == "flags_none_set")
					{
						Condition[conditionIndex].matchType = FWP_MATCH_FLAGS_NONE_SET;
					}
					else if (attrValue == "equal_case_insensitive")
					{
						Condition[conditionIndex].matchType = FWP_MATCH_EQUAL_CASE_INSENSITIVE;
					}
					else if (attrValue == "not_equal")
					{
						Condition[conditionIndex].matchType = FWP_MATCH_NOT_EQUAL;
					}
					else if (attrValue == "type_max")
					{
						Condition[conditionIndex].matchType = FWP_MATCH_TYPE_MAX;
					}
					else
					{
						m_wfpLastError = "Unknown match '" + attrValue + "'";
						m_wfpLastErrorCode = 0;
						return 0;
					}
				}
				else if (attrName == "port")
				{
					Condition[conditionIndex].conditionValue.type = FWP_UINT16;
					Condition[conditionIndex].conditionValue.uint16 = atoi(attrValue.c_str());
				}
				else if (attrName == "path")
				{
					if (Condition[conditionIndex].fieldKey == FWPM_CONDITION_ALE_APP_ID)
					{
						std::wstring wAttrValue = std::wstring(attrValue.begin(), attrValue.end());
						FWP_BYTE_BLOB *appblob = NULL;
						if (FwpmGetAppIdFromFileName0(wAttrValue.c_str(), &appblob) != ERROR_SUCCESS)
						{
							m_wfpLastError = "App not found";
							m_wfpLastErrorCode = 0;
							return 0;
						}

						Condition[conditionIndex].conditionValue.type = FWP_BYTE_BLOB_TYPE;
						Condition[conditionIndex].conditionValue.byteBlob = appblob;
					}
					else
					{
						m_wfpLastError = "Unexpected 'path' attribute.";
						m_wfpLastErrorCode = 0;
						return 0;
					}
				}
				else if (attrName == "address")
				{
					if ((Filter.layerKey == FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V4) ||
						(Filter.layerKey == FWPM_LAYER_ALE_AUTH_CONNECT_V4) ||
						(Filter.layerKey == FWPM_LAYER_ALE_FLOW_ESTABLISHED_V4))
					{
						UINT32 IpAddr = 0;
						if (!inet_pton(AF_INET, attrValue.c_str(), &IpAddr))
						{
							m_wfpLastError = "inet_pton ipv4 address failed with '" + attrValue + "'";
							m_wfpLastErrorCode = 0;
							return 0;
						}

						WfpRuleData &ruleData = rulesMap[conditionIndex];
						ruleData.ipAddressV4.addr = ntohl(IpAddr);

						Condition[conditionIndex].conditionValue.type = FWP_V4_ADDR_MASK;
						Condition[conditionIndex].conditionValue.v4AddrMask = &ruleData.ipAddressV4;
					}
					else if ((Filter.layerKey == FWPM_LAYER_ALE_AUTH_CONNECT_V6) ||
						(Filter.layerKey == FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V6) ||
						(Filter.layerKey == FWPM_LAYER_ALE_FLOW_ESTABLISHED_V6))
					{
						in6_addr IpAddr6;
						::ZeroMemory(&IpAddr6, sizeof(in6_addr));

						if (!inet_pton(AF_INET6, attrValue.c_str(), &IpAddr6))
						{
							m_wfpLastError = "inet_pton ipv6 address failed with '" + attrValue + "'";
							m_wfpLastErrorCode = 0;
							return 0;
						}

						WfpRuleData &ruleData = rulesMap[conditionIndex];
						memcpy(&ruleData.ipAddressV6.addr, &IpAddr6.u.Byte, 16);

						Condition[conditionIndex].conditionValue.type = FWP_V6_ADDR_MASK;
						Condition[conditionIndex].conditionValue.v6AddrMask = &ruleData.ipAddressV6;
					}
				}
				else if (attrName == "mask")
				{
					if (attrValue != "")
					{
						if ((Filter.layerKey == FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V4) ||
							(Filter.layerKey == FWPM_LAYER_ALE_AUTH_CONNECT_V4) ||
							(Filter.layerKey == FWPM_LAYER_ALE_FLOW_ESTABLISHED_V4))
						{
							UINT32 Mask;
							if (!inet_pton(AF_INET, attrValue.c_str(), &Mask))
							{
								m_wfpLastError = "inet_pton ipv4 mask failed with '" + attrValue + "'";
								m_wfpLastErrorCode = 0;
								return 0;
							}

							WfpRuleData &ruleData = rulesMap[conditionIndex];
							ruleData.ipAddressV4.mask = ntohl(Mask);

							Condition[conditionIndex].conditionValue.type = FWP_V4_ADDR_MASK;
							Condition[conditionIndex].conditionValue.v4AddrMask = &ruleData.ipAddressV4;
						}
						else if ((Filter.layerKey == FWPM_LAYER_ALE_AUTH_CONNECT_V6) ||
							(Filter.layerKey == FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V6) ||
							(Filter.layerKey == FWPM_LAYER_ALE_FLOW_ESTABLISHED_V6))
						{
							UINT8 prefixLength = atoi(attrValue.c_str());

							WfpRuleData &ruleData = rulesMap[conditionIndex];
							ruleData.ipAddressV6.prefixLength = prefixLength;

							Condition[conditionIndex].conditionValue.type = FWP_V6_ADDR_MASK;
							Condition[conditionIndex].conditionValue.v6AddrMask = &ruleData.ipAddressV6;
						}
					}
				}
				else if (attrName == "port_from")
				{
					WfpRuleData &ruleData = rulesMap[conditionIndex];

					ruleData.portRange.valueLow.type = FWP_UINT16;
					ruleData.portRange.valueLow.uint16 = atoi(attrValue.c_str());

					Condition[conditionIndex].conditionValue.type = FWP_RANGE_TYPE;
					Condition[conditionIndex].conditionValue.rangeValue = &ruleData.portRange;
				}
				else if (attrName == "port_to")
				{
					WfpRuleData &ruleData = rulesMap[conditionIndex];

					ruleData.portRange.valueHigh.type = FWP_UINT16;
					ruleData.portRange.valueHigh.uint16 = atoi(attrValue.c_str());

					Condition[conditionIndex].conditionValue.type = FWP_RANGE_TYPE;
					Condition[conditionIndex].conditionValue.rangeValue = &ruleData.portRange;
				}
				else if (attrName == "protocol")
				{
					Condition[conditionIndex].conditionValue.type = FWP_UINT8;
					if (attrValue == "tcp")
						Condition[conditionIndex].conditionValue.uint8 = IPPROTO_TCP;
					else if (attrValue == "udp")
						Condition[conditionIndex].conditionValue.uint8 = IPPROTO_UDP;
					else if (attrValue == "icmp")
						Condition[conditionIndex].conditionValue.uint8 = IPPROTO_ICMP;
				}
				else if (attrName == "interface")
				{
					if (Condition[conditionIndex].fieldKey == FWPM_CONDITION_IP_LOCAL_INTERFACE)
					{
						Condition[conditionIndex].conditionValue.type = FWP_UINT64;

						BOOL found = FALSE;

						//std::ostringstream log;

						PIP_ADAPTER_ADDRESSES pAddresses = NULL;
						PIP_ADAPTER_ADDRESSES pCurrAddresses = NULL;
						ULONG outBufLen = 0;
						ULONG Iterations = 0;
						DWORD dwRetVal = 0;
						ULONG family = AF_UNSPEC;
						ULONG flags = GAA_FLAG_INCLUDE_PREFIX;

#define WORKING_BUFFER_SIZE 15000
#define MAX_TRIES 3

						// Allocate a 15 KB buffer to start with.
						outBufLen = WORKING_BUFFER_SIZE;

						do {

							pAddresses = (IP_ADAPTER_ADDRESSES *)MALLOC(outBufLen);
							if (pAddresses == NULL) {
								printf
								("Memory allocation failed for IP_ADAPTER_ADDRESSES struct\n");
								exit(1);
							}

							dwRetVal = GetAdaptersAddresses(family, flags, NULL, pAddresses, &outBufLen);

							if (dwRetVal == ERROR_BUFFER_OVERFLOW) {
								FREE(pAddresses);
								pAddresses = NULL;
							}
							else {
								break;
							}

							Iterations++;

						} while ((dwRetVal == ERROR_BUFFER_OVERFLOW) && (Iterations < MAX_TRIES));

						if (dwRetVal == NO_ERROR) {
							// If successful, output some information from the data we received
							pCurrAddresses = pAddresses;
							while (pCurrAddresses) {

								DWORD index = 0;

								if (Filter.layerKey == FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V4)
									index = pCurrAddresses->IfIndex;
								else if (Filter.layerKey == FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V6)
									index = pCurrAddresses->Ipv6IfIndex;
								else if (Filter.layerKey == FWPM_LAYER_ALE_AUTH_CONNECT_V4)
									index = pCurrAddresses->IfIndex;
								else if (Filter.layerKey == FWPM_LAYER_ALE_AUTH_CONNECT_V6)
									index = pCurrAddresses->Ipv6IfIndex;
								else if (Filter.layerKey == FWPM_LAYER_ALE_FLOW_ESTABLISHED_V4)
									index = pCurrAddresses->IfIndex;
								else if (Filter.layerKey == FWPM_LAYER_ALE_FLOW_ESTABLISHED_V6)
									index = pCurrAddresses->Ipv6IfIndex;

								if (index != 0)
								{
									if ((attrValue == "loopback") && (pCurrAddresses->IfType == IF_TYPE_SOFTWARE_LOOPBACK))
										found = true;

									if (strcmp(pCurrAddresses->AdapterName, attrValue.c_str()) == 0)
										found = true;
								}

								if (found)
								{
									NET_LUID tapluid;

									if (ConvertInterfaceIndexToLuid(index, &tapluid) == NO_ERROR)
									{
										Condition[conditionIndex].conditionValue.uint64 = &tapluid.Value;
									}
									else
									{
										m_wfpLastError = "Unable to obtain interface luid";
										m_wfpLastErrorCode = 0;
										return 0;
									}

									break;
								}

								pCurrAddresses = pCurrAddresses->Next;
							}
						}
						else {
						}

						if (pAddresses) {
							FREE(pAddresses);
						}

						if (found == FALSE)
						{
							m_wfpLastError = "Interface ID '" + attrValue + "' unknown or disabled for the layer.";
							m_wfpLastErrorCode = 0;
							return 0;
						}
					}
					else
					{
						m_wfpLastError = "Unexpected 'interface' attribute.";
						m_wfpLastErrorCode = 0;
						return 0;
					}
				}
				else
				{
					m_wfpLastError = "Unknown attribute '" + attrName + "'";
					m_wfpLastErrorCode = 0;
					return 0;
				}
			}
			else
			{
				m_wfpLastError = "Unknown element '" + elemName + "'";
				m_wfpLastErrorCode = 0;
				return 0;
			}

			attrValue.clear();
		} break;
		case YXML_ELEMEND:
		{

		} break;
		}
	}

	yxml_ret_t r = yxml_eof(&x);
	if (r < 0)
	{
		m_wfpLastError = "XML syntax error";
		m_wfpLastErrorCode = 0;
		return 0; // Syntax error
	}

	Filter.numFilterConditions = conditionIndex + 1;

	// Add filter condition to our interface. Save filter id in filterids.
	m_wfpLastErrorCode = ::FwpmFilterAdd0(m_wfpEngineHandle,
		&Filter,
		NULL,
		&filterId);

	if (m_wfpLastErrorCode != ERROR_SUCCESS)
	{
		m_wfpLastError = "WFP Error.";
		return 0;
	}

	m_wfpFilters.push_back(filterId);

	m_wfpLastError = "OK";
	m_wfpLastErrorCode = 0;
	return filterId;
}

bool Impl::WfpRuleRemove(const UINT64 id)
{
	for (unsigned int i = 0; i < m_wfpFilters.size(); i++)
	{
		if (id == m_wfpFilters[i])
		{
			m_wfpFilters.erase(m_wfpFilters.begin() + i);

			return WfpRuleRemoveDirect(id);
		}
	}

	return false;
}

bool Impl::WfpRuleRemoveDirect(const UINT64 id)
{
	m_wfpLastErrorCode = ::FwpmFilterDeleteById0(m_wfpEngineHandle, id);

	return (m_wfpLastErrorCode == ERROR_SUCCESS);
}

bool Impl::WfpRemovePending(const std::string& filterName, const std::string& xml)
{
	int nFound = 0;
	std::string e = xml;
	std::string searchStart = "<name>" + filterName + "</name>";
	std::string searchEnd = "</filterId>";
	for (;;)
	{
		size_t start_pos = e.find(searchStart);
		if (start_pos == std::string::npos)
			break;
		size_t end_pos = e.find(searchEnd, start_pos);
		if (end_pos == std::string::npos)
			break;

		std::string item = e.substr(start_pos, end_pos - start_pos + searchEnd.length());

		std::string filterIdStr = StringExtractBetween(item, "<filterId>", "</filterId>");

		UINT64 filterId = _atoi64(filterIdStr.c_str());
		if (filterId != 0)
		{
			if (WfpRuleRemoveDirect(filterId))
			{
				nFound++;
			}
		}

		e = StringReplaceAll(e, item, "");
	}
	return (nFound != 0);

	void *buf = malloc(XMLBUFSIZE);
	yxml_t x;
	yxml_init(&x, buf, XMLBUFSIZE);
	const char* xmlE = xml.c_str();

	for (; *xmlE; xmlE++)
	{
		yxml_ret_t r = yxml_parse(&x, *xmlE);
		if (r < 0)
		{
			m_wfpLastError = "XML syntax error";
			m_wfpLastErrorCode = 0;
			return false; // Syntax error
		}

		return true;

		/*
		if (r == YXML_ATTRVAL)
			attrValue.append(x.data);

		switch (r)
		{
			case YXML_ATTREND:
			{
				std::string elemName = x.elem;
				std::string attrName = x.attr;

				if (attrName == "description")
					description = attrValue;
				else if (attrName == "weight")
					weight = attrValue;
				else if (attrName == "persistent")
					persistent = (attrValue == "true");
				else if (attrName == "dynamic")
					dynamic = (attrValue == "true");
				else
				{
					m_wfpLastError = "Unknown attribute '" + attrName + "'";
					m_wfpLastErrorCode = 0;
					return 0;
				}

				attrValue = "";
			}
		}
		*/

		/*
		System.Xml.XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.Load(path);
		foreach(XmlElement xmlFilter in xmlDoc.DocumentElement.GetElementsByTagName("filters"))
		{
			foreach(XmlElement xmlItem in xmlFilter.GetElementsByTagName("item"))
			{
				foreach(XmlElement xmlName in xmlItem.SelectNodes("displayData/name"))
				{
					string name = xmlName.InnerText;
					if (name == wfpName)
					{
						foreach(XmlNode xmlFilterId in xmlItem.GetElementsByTagName("filterId"))
						{
							ulong idR;
							if (ulong.TryParse(xmlFilterId.InnerText, out idR))
							{
								NativeMethods.WfpRuleRemoveDirect(idR);
								found = true;
							}
						}
					}
				}
			}
		}
		*/
	}
}

const char* Impl::WfpGetLastError()
{
	return m_wfpLastError.c_str();
}

DWORD Impl::WfpGetLastErrorCode()
{
	return m_wfpLastErrorCode;
}