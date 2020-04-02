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

//#include <Windows.h>
#include <tchar.h>


#include <winsock2.h>
#include "..\App.CLI.Windows.Elevated\src\impl.h"
#include <Ws2tcpip.h>
#include <ws2ipdef.h>
#include <iphlpapi.h>

SERVICE_STATUS        g_ServiceStatus = { 0 };
SERVICE_STATUS_HANDLE g_StatusHandle = NULL;
HANDLE                g_ServiceStopEvent = INVALID_HANDLE_VALUE;

VOID WINAPI ServiceMain(DWORD argc, LPTSTR *argv);
VOID WINAPI ServiceCtrlHandler(DWORD);
DWORD WINAPI ServiceWorkerThread(LPVOID lpParam);
void ServiceLogDebug(const std::string& msg);

#define SERVICE_NAME  _T("EddieElevationService")

int _tmain(int argc, TCHAR *argv[])
{
	ServiceLogDebug("EddieElevationService: Main: Entry");

	SERVICE_TABLE_ENTRY ServiceTable[] =
	{
		{ SERVICE_NAME, (LPSERVICE_MAIN_FUNCTION)ServiceMain },
		{ NULL, NULL }
	};

	if (StartServiceCtrlDispatcher(ServiceTable) == FALSE)
	{
		ServiceLogDebug("EddieElevationService: Main: StartServiceCtrlDispatcher returned error");
		return GetLastError();
	}

	ServiceLogDebug("EddieElevationService: Main: Exit");
	return 0;
}


VOID WINAPI ServiceMain(DWORD argc, LPTSTR *argv)
{
	DWORD Status = E_FAIL;

	ServiceLogDebug("EddieElevationService: ServiceMain: Entry");

	g_StatusHandle = RegisterServiceCtrlHandler(SERVICE_NAME, ServiceCtrlHandler);

	if (g_StatusHandle == NULL)
	{
		ServiceLogDebug("EddieElevationService: ServiceMain: RegisterServiceCtrlHandler returned error");
		goto EXIT;
	}

	// Tell the service controller we are starting
	ZeroMemory(&g_ServiceStatus, sizeof(g_ServiceStatus));
	g_ServiceStatus.dwServiceType = SERVICE_WIN32_OWN_PROCESS;
	g_ServiceStatus.dwControlsAccepted = 0;
	g_ServiceStatus.dwCurrentState = SERVICE_START_PENDING;
	g_ServiceStatus.dwWin32ExitCode = 0;
	g_ServiceStatus.dwServiceSpecificExitCode = 0;
	g_ServiceStatus.dwCheckPoint = 0;

	if (SetServiceStatus(g_StatusHandle, &g_ServiceStatus) == FALSE)
	{
		ServiceLogDebug("EddieElevationService: ServiceMain: SetServiceStatus returned error");
	}

	// Perform tasks neccesary to start the service here
	ServiceLogDebug("EddieElevationService: ServiceMain: Performing Service Start Operations");

	// Create stop event to wait on later.
	g_ServiceStopEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
	if (g_ServiceStopEvent == NULL)
	{
		ServiceLogDebug("EddieElevationService: ServiceMain: CreateEvent(g_ServiceStopEvent) returned error");

		g_ServiceStatus.dwControlsAccepted = 0;
		g_ServiceStatus.dwCurrentState = SERVICE_STOPPED;
		g_ServiceStatus.dwWin32ExitCode = GetLastError();
		g_ServiceStatus.dwCheckPoint = 1;

		if (SetServiceStatus(g_StatusHandle, &g_ServiceStatus) == FALSE)
		{
			ServiceLogDebug("EddieElevationService: ServiceMain: SetServiceStatus returned error");
		}
		goto EXIT;
	}

	// Tell the service controller we are started
	g_ServiceStatus.dwControlsAccepted = SERVICE_ACCEPT_STOP;
	g_ServiceStatus.dwCurrentState = SERVICE_RUNNING;
	g_ServiceStatus.dwWin32ExitCode = 0;
	g_ServiceStatus.dwCheckPoint = 0;

	if (SetServiceStatus(g_StatusHandle, &g_ServiceStatus) == FALSE)
	{
		ServiceLogDebug("EddieElevationService: ServiceMain: SetServiceStatus returned error");
	}

	// Start the thread that will perform the main task of the service
	HANDLE hThread = CreateThread(NULL, 0, ServiceWorkerThread, NULL, 0, NULL);

	ServiceLogDebug("EddieElevationService: ServiceMain: Waiting for Worker Thread to complete");

	// Wait until our worker thread exits effectively signaling that the service needs to stop
	WaitForSingleObject(hThread, INFINITE);

	ServiceLogDebug("EddieElevationService: ServiceMain: Worker Thread Stop Event signaled");


	// Perform any cleanup tasks
	ServiceLogDebug("EddieElevationService: ServiceMain: Performing Cleanup Operations");

	CloseHandle(g_ServiceStopEvent);

	g_ServiceStatus.dwControlsAccepted = 0;
	g_ServiceStatus.dwCurrentState = SERVICE_STOPPED;
	g_ServiceStatus.dwWin32ExitCode = 0;
	g_ServiceStatus.dwCheckPoint = 3;

	if (SetServiceStatus(g_StatusHandle, &g_ServiceStatus) == FALSE)
	{
		ServiceLogDebug("EddieElevationService: ServiceMain: SetServiceStatus returned error");
	}

EXIT:
	ServiceLogDebug("EddieElevationService: ServiceMain: Exit");

	return;
}


VOID WINAPI ServiceCtrlHandler(DWORD CtrlCode)
{
	ServiceLogDebug("EddieElevationService: ServiceCtrlHandler: Entry");

	switch (CtrlCode)
	{
	case SERVICE_CONTROL_STOP:

		ServiceLogDebug("EddieElevationService: ServiceCtrlHandler: SERVICE_CONTROL_STOP Request");

		if (g_ServiceStatus.dwCurrentState != SERVICE_RUNNING)
			break;

		// Perform tasks necessary to stop the service here
		
		g_ServiceStatus.dwControlsAccepted = 0;
		g_ServiceStatus.dwCurrentState = SERVICE_STOP_PENDING;
		g_ServiceStatus.dwWin32ExitCode = 0;
		g_ServiceStatus.dwCheckPoint = 4;

		if (SetServiceStatus(g_StatusHandle, &g_ServiceStatus) == FALSE)
		{
			ServiceLogDebug("EddieElevationService: ServiceCtrlHandler: SetServiceStatus returned error");
		}

		// This will signal the worker thread to start shutting down
		SetEvent(g_ServiceStopEvent);

		break;

	default:
		break;
	}

	ServiceLogDebug("EddieElevationService: ServiceCtrlHandler: Exit");
}

class ImplService : public Impl
{
	virtual bool IsStopRequested()
	{
		bool run = (WaitForSingleObject(g_ServiceStopEvent, 0) != WAIT_OBJECT_0);
		if(run == false)
			ServiceLogDebug("IsStopRequested detected");
		return !run;
	}
};

DWORD WINAPI ServiceWorkerThread(LPVOID lpParam)
{
	ServiceLogDebug("EddieElevationService: ServiceWorkerThread: Entry");

	ImplService impl;

	std::vector<std::string> args;
	WCHAR wpath[MAX_PATH];
	GetModuleFileNameW(NULL, wpath, MAX_PATH);
	std::string path = impl.StringWStringToUTF8(wpath);
	args.push_back(path);
	args.push_back("mode=service");
	
	// Obtain args from registry, because Win Service don't support arguments.
	std::wstring regPathW = impl.StringUTF8ToWString("SYSTEM\\CurrentControlSet\\Services\\EddieElevationService");
	std::wstring regKeyW = impl.StringUTF8ToWString("EddieArgs");
	WCHAR szBuffer[4096];
	DWORD dwBufferSize = sizeof(szBuffer);
	auto rc = RegGetValueW(HKEY_LOCAL_MACHINE, regPathW.c_str(), regKeyW.c_str(), RRF_RT_REG_SZ, nullptr, static_cast<void*>(szBuffer), &dwBufferSize);
	if (rc != ERROR_SUCCESS)
		return 1;

	// Quote not need support.
	std::wstring wArgs = szBuffer;
	std::wstring arg;
	for (size_t c = 0; c < wArgs.size(); c++)
	{
		if (wArgs[c] == L' ')
		{
			args.push_back(impl.StringWStringToUTF8(arg));
			arg = L"";
		}
		else
		{
			arg += wArgs[c];
		}
	}
	args.push_back(impl.StringWStringToUTF8(arg));

	impl.AppMain(args);	

	ServiceLogDebug("EddieElevationService: ServiceWorkerThread: Exit");

	return ERROR_SUCCESS;
}

void ServiceLogDebug(const std::string& msg)
{	
	/*
	std::string logPath = "C:\\eddie_service_debug.log";
	FILE* f = fopen(logPath.c_str(), "a");
	fprintf(f, "%s\r\n", msg.c_str());
	fclose(f);
	*/
}