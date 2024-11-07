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

#include <tchar.h>
#include <windows.h>

#include <string>
#include <vector>
#include <fstream>
#include <sstream>
#include <algorithm>

#include "../../../dependencies/sha256/sha256.h"

SERVICE_STATUS        g_ServiceStatus = { 0 };
SERVICE_STATUS_HANDLE g_StatusHandle = NULL;

PROCESS_INFORMATION g_ProcessInfo;
STARTUPINFO g_StartupInfo = { sizeof(g_StartupInfo) };

VOID WINAPI ServiceMain(DWORD argc, LPTSTR* argv);
VOID WINAPI ServiceCtrlHandler(DWORD);

#define SERVICE_NAME  _T("EddieElevationService")

void LogDebug(const std::wstring& msg)
{
	/*
	std::wstring logPath = _T("C:\\eddie_vpn_windows_service_debug.log");
	FILE* f = nullptr;
	errno_t err = _wfopen_s(&f, logPath.c_str(), L"a");

	if (err == 0 && f != nullptr)
	{
		fwprintf(f, _T("%s\r\n"), msg.c_str());
		fclose(f);
	}
	*/
}

const std::string FsPathSeparator = "\\";

std::string StringToLower(const std::string& s)
{
	std::string result = s;
	std::transform(result.begin(), result.end(), result.begin(), ::tolower);
	return result;
}

bool StringStartsWith(const std::string& s, const std::string& f)
{
	size_t pos = s.find(f);
	return (pos == 0);
}

bool StringEndsWith(const std::string& s, const std::string& f)
{
	return(s.size() >= f.size() && s.compare(s.size() - f.size(), f.size(), f) == 0);
}

bool StringContain(const std::string& s, const std::string& f)
{
	return (s.find(f) != std::string::npos);
}

std::wstring StringUTF8ToWString(const std::string& str)
{
	if (str.empty()) return std::wstring();
	int sizeNeeded = MultiByteToWideChar(CP_UTF8, 0, &str[0], (int)str.size(), NULL, 0);
	std::wstring wstrTo(sizeNeeded, 0);
	MultiByteToWideChar(CP_UTF8, 0, &str[0], (int)str.size(), &wstrTo[0], sizeNeeded);
	return wstrTo;
}

std::string StringWStringToUTF8(const std::wstring& wstr)
{
	if (wstr.empty()) return std::string();
	int sizeNeeded = WideCharToMultiByte(CP_UTF8, 0, &wstr[0], (int)wstr.size(), NULL, 0, NULL, NULL);
	std::string strTo(sizeNeeded, 0);
	WideCharToMultiByte(CP_UTF8, 0, &wstr[0], (int)wstr.size(), &strTo[0], sizeNeeded, NULL, NULL);
	return strTo;
}

bool FsFileExists(const std::string& path)
{
	if (path == "")
		return false;

	std::wstring pathW(StringUTF8ToWString(path));
	DWORD fileAttributes = GetFileAttributes(pathW.c_str());
	return (fileAttributes != INVALID_FILE_ATTRIBUTES) && !(fileAttributes & FILE_ATTRIBUTE_DIRECTORY);
}

std::string FsFileGetExtension(const std::string& path)
{
	std::string ext = "";
	std::string::size_type extPos = path.rfind('.');
	if (extPos != std::string::npos)
		ext = StringToLower(path.substr(extPos + 1));
	return ext;
}

bool FsFileIsExecutable(std::string path)
{
	if (FsFileExists(path) == false)
		return false;
	std::string ext = FsFileGetExtension(path);
	return (ext == "exe" || ext == "msi" || ext == "bat" || ext == "ps1" || ext == "cmd" || ext == "com");
}

std::string FsFileGetDirectory(const std::string& path)
{
	return path.substr(0, path.find_last_of("/\\"));
}

std::vector<std::string> FsFilesInPath(const std::string& path)
{
	std::vector<std::string> files;

	std::string pathU = path;

	pathU += FsPathSeparator + "*.*";

	std::wstring pathW(StringUTF8ToWString(pathU));

	WIN32_FIND_DATAW ffd;
	HANDLE hFind = INVALID_HANDLE_VALUE;
	hFind = FindFirstFileW(pathW.c_str(), &ffd);
	if (INVALID_HANDLE_VALUE == hFind)
		return files;

	do
	{
		std::wstring fileW = ffd.cFileName;
		std::string file = StringWStringToUTF8(fileW);
		if (ffd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
			continue;
		files.push_back(file);

	} while (FindNextFile(hFind, &ffd) != 0);

	return files;
}

std::string FsFileSHA256Sum(const std::string& path)
{
	std::ifstream ifs(path, std::ios::binary | std::ios::ate);
	std::ifstream::pos_type pos = ifs.tellg();

	std::vector<char> buf((const unsigned int)pos);

	ifs.seekg(0, std::ios::beg);
	if (pos > 0)
		ifs.read(&buf[0], pos);

	sha256_context ctx;
	sha256_starts(&ctx);
	sha256_update(&ctx, (unsigned char*)buf.data(), (unsigned long)buf.size());
	unsigned char sha256sum[32];
	sha256_finish(&ctx, sha256sum);

	const char hex_chars[] = "0123456789abcdef";
	std::string result;
	for (size_t i = 0; i < sizeof(sha256sum); ++i) {
		result.push_back(hex_chars[(sha256sum[i] >> 4) & 0x0F]);
		result.push_back(hex_chars[sha256sum[i] & 0x0F]);
	}

	return result;
}

std::string SystemWideDataGet(const std::string& key, const std::string& def)
{
	std::wstring keyW = StringUTF8ToWString(key);
	HKEY hKey;
	wchar_t buffer[256*256];
	DWORD bufferSize = sizeof(buffer);

	if (RegOpenKeyEx(HKEY_LOCAL_MACHINE, L"SOFTWARE\\Eddie-VPN", 0, KEY_READ, &hKey) != ERROR_SUCCESS)
		return def;
	bool result = (RegQueryValueEx(hKey, keyW.c_str(), NULL, NULL, reinterpret_cast<BYTE*>(buffer), &bufferSize) == ERROR_SUCCESS);
	RegCloseKey(hKey);
	if (result)
		return StringWStringToUTF8(std::wstring(buffer));
	else
		return def;
}

std::string IntegrityCheckBuild(const std::string elevatedPath)
{
	std::string output;
	std::string checkPath = FsFileGetDirectory(elevatedPath);
	std::vector<std::string> files = FsFilesInPath(checkPath);
	std::sort(files.begin(), files.end());
	for (std::vector<std::string>::const_iterator i = files.begin(); i != files.end(); ++i)
	{
		std::string file = *i;
		std::string filePathFull = checkPath + FsPathSeparator + file;

		std::string ext = "";
		std::string::size_type extPos = file.rfind('.');

		if (extPos != std::string::npos)
			ext = StringToLower(file.substr(extPos + 1));

		bool include = ((FsFileIsExecutable(filePathFull)) || (StringEndsWith(file, ".dll")) || (StringEndsWith(file, ".dylib")) || (StringEndsWith(file, ".so")) || (StringContain(file, ".so.")));

		if (StringStartsWith(file, "System.")) include = false; // Hack, dotnet7, avoid a lots of intermediary files

		if (include)
		{
			std::string sha256 = FsFileSHA256Sum(filePathFull);
			output += sha256 + ";" + filePathFull + "\n";
		}
	}
	return output;
}

std::string IntegrityCheckRead(const std::string mode)
{	
	return SystemWideDataGet("integrity_" + mode, "");
}

std::wstring ReadRegistryArg(const std::wstring& name)
{
	WCHAR szBuffer[4096];
	DWORD dwBufferSize = sizeof(szBuffer);
	auto rc = RegGetValueW(HKEY_LOCAL_MACHINE, _T("SYSTEM\\CurrentControlSet\\Services\\EddieElevationService"), name.c_str(), RRF_RT_REG_SZ, nullptr, static_cast<void*>(szBuffer), &dwBufferSize);
	if (rc != ERROR_SUCCESS)
		return _T("");
	return szBuffer;
}

void ServiceDelete()
{
	LogDebug(_T("ServiceDelete: Enter"));

	SC_HANDLE serviceControlManager = OpenSCManager(0, 0, SC_MANAGER_ALL_ACCESS); // GENERIC_WRITE is not enough

	if (serviceControlManager)
	{
		SC_HANDLE service = OpenService(serviceControlManager, L"EddieElevationService", SC_MANAGER_ALL_ACCESS);
		if (service)
		{
			LogDebug(_T("ServiceDelete: Do"));
			DeleteService(service);
			CloseServiceHandle(service);
		}

		CloseServiceHandle(serviceControlManager);
	}

	/* WIP, don't work, cannot delete current .exe */
	wchar_t systemDirectory[MAX_PATH];
	UINT size = GetSystemDirectoryW(systemDirectory, MAX_PATH);
	std::wstring servicePathSecure = std::wstring(systemDirectory) + L"\\Eddie-VPN-Elevated-Service.exe";
	DeleteFileW(servicePathSecure.c_str());

	LogDebug(_T("ServiceDelete: Exit"));
}

int _tmain(int argc, TCHAR* argv[])
{
	LogDebug(_T("Main: Enter"));

	SetDefaultDllDirectories(LOAD_LIBRARY_SEARCH_SYSTEM32);
	SetDllDirectory(L"");

	SERVICE_TABLE_ENTRY ServiceTable[] =
	{
		{ SERVICE_NAME, (LPSERVICE_MAIN_FUNCTION)ServiceMain },
		{ NULL, NULL }
	};

	if (StartServiceCtrlDispatcher(ServiceTable) == FALSE)
	{
		LogDebug(_T("Main: StartServiceCtrlDispatcher returned error"));
		return GetLastError();
	}

	LogDebug(_T("Main: Exit"));
	return 0;
}

VOID WINAPI ServiceMain(DWORD argc, LPTSTR* argv)
{
	DWORD Status = E_FAIL;

	LogDebug(_T("ServiceMain: Enter"));

	g_StatusHandle = RegisterServiceCtrlHandler(SERVICE_NAME, ServiceCtrlHandler);

	if (g_StatusHandle == NULL)
	{
		LogDebug(_T("ServiceMain: RegisterServiceCtrlHandler returned error"));
	}
	else
	{
		ZeroMemory(&g_ServiceStatus, sizeof(g_ServiceStatus));
		g_ServiceStatus.dwServiceType = SERVICE_WIN32_OWN_PROCESS;
		g_ServiceStatus.dwControlsAccepted = 0;
		g_ServiceStatus.dwCurrentState = SERVICE_START_PENDING;
		g_ServiceStatus.dwWin32ExitCode = 0;
		g_ServiceStatus.dwServiceSpecificExitCode = 0;
		g_ServiceStatus.dwCheckPoint = 0;

		if (SetServiceStatus(g_StatusHandle, &g_ServiceStatus) == FALSE)
		{
			LogDebug(_T("ServiceMain: SetServiceStatus returned error"));
		}

		LogDebug(_T("ServiceMain: Performing Service Start Operations"));

		g_ServiceStatus.dwControlsAccepted = SERVICE_ACCEPT_STOP;
		g_ServiceStatus.dwCurrentState = SERVICE_RUNNING;
		g_ServiceStatus.dwWin32ExitCode = 0;
		g_ServiceStatus.dwCheckPoint = 0;

		if (SetServiceStatus(g_StatusHandle, &g_ServiceStatus) == FALSE)
		{
			LogDebug(_T("ServiceMain: SetServiceStatus returned error"));
		}

		if (true)
		{
			LogDebug(_T("ServiceMain: Prepare Process"));

			std::wstring args = ReadRegistryArg(_T("EddieArgs"));
			std::wstring elevatedPath = ReadRegistryArg(_T("EddieElevatedPath"));
			
			LogDebug(std::wstring(_T("ServiceMain: ReadArg::EddieArgs = ")) + args);
			LogDebug(std::wstring(_T("ServiceMain: ReadArg::EddieElevatedPath = ")) + elevatedPath);
			
			std::string securityHashesStored = IntegrityCheckRead("service");
			std::string securityHashesComputed = IntegrityCheckBuild(StringWStringToUTF8(elevatedPath));

			if (securityHashesStored == securityHashesComputed)
			{
				LogDebug(_T("ServiceMain: Create Process"));

				// Convert the command line to mutable LPWSTR
				std::wstring cmdline = std::wstring(_T("\"")) + elevatedPath + std::wstring(_T("\" ")) + args;
				std::unique_ptr<wchar_t[]> cmdlineM(new wchar_t[cmdline.length() + 1]);
				wcscpy_s(cmdlineM.get(), cmdline.length() + 1, cmdline.c_str());

				ZeroMemory(&g_ProcessInfo, sizeof(g_ProcessInfo));

				if (CreateProcessW(
					NULL,
					cmdlineM.get(),
					nullptr,
					nullptr,
					FALSE,
					CREATE_NO_WINDOW,
					nullptr,
					nullptr,
					&g_StartupInfo,
					&g_ProcessInfo
				))
				{
					LogDebug(_T("ServiceMain: Wait Process"));

					WaitForSingleObject(g_ProcessInfo.hProcess, INFINITE);
					CloseHandle(g_ProcessInfo.hProcess);
					CloseHandle(g_ProcessInfo.hThread);

					LogDebug(_T("ServiceMain: End Process"));
				}
			}
			else
			{
				LogDebug(_T("ServiceMain: Check don't match."));
				ServiceDelete();
			}
		}

		g_ServiceStatus.dwControlsAccepted = 0;
		g_ServiceStatus.dwCurrentState = SERVICE_STOPPED;
		g_ServiceStatus.dwWin32ExitCode = 0;
		g_ServiceStatus.dwCheckPoint = 3;

		if (SetServiceStatus(g_StatusHandle, &g_ServiceStatus) == FALSE)
		{
			LogDebug(_T("ServiceMain: SetServiceStatus returned error"));
		}
	}

	LogDebug(_T("ServiceMain: Exit"));
}


VOID WINAPI ServiceCtrlHandler(DWORD CtrlCode)
{
	LogDebug(_T("ServiceCtrlHandler: Enter"));

	switch (CtrlCode)
	{
	case SERVICE_CONTROL_STOP:

		LogDebug(_T("ServiceCtrlHandler: SERVICE_CONTROL_STOP Request"));

		if (g_ServiceStatus.dwCurrentState != SERVICE_RUNNING)
			break;

		g_ServiceStatus.dwControlsAccepted = 0;
		g_ServiceStatus.dwCurrentState = SERVICE_STOP_PENDING;
		g_ServiceStatus.dwWin32ExitCode = 0;
		g_ServiceStatus.dwCheckPoint = 4;

		if (SetServiceStatus(g_StatusHandle, &g_ServiceStatus) == FALSE)
		{
			LogDebug(_T("ServiceCtrlHandler: SetServiceStatus returned error"));
		}

		if(g_ProcessInfo.hProcess)
			TerminateProcess(g_ProcessInfo.hProcess, 0);

		break;

	default:
		break;
	}

	LogDebug(_T("ServiceCtrlHandler: Exit"));
}
