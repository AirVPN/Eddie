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

#include "Windows.h"

#include <fstream>
#include <sstream>

#include "iwindows.h"
#include "psapi.h"
#include "tlhelp32.h"

#include <codecvt> // For StringUTF8ToWString

void IWindows::Do(const std::string& commandId, const std::string& command, std::map<std::string, std::string>& params)
{
	if (command == "kill")
	{
		pid_t pid = atoi(params["pid"].c_str());

		if (GetParentProcessId(pid) != GetCurrentProcessId())
			ThrowException("Requested a kill to a non-child elevated process");

		std::string signal = params["signal"];

		if (signal == "sigint")
		{
			if (AttachConsole(pid))
			{
				SetConsoleCtrlHandler(NULL, true);
				try
				{
					GenerateConsoleCtrlEvent(CTRL_C_EVENT, 0);
					Sleep(3000);
				}
				catch (...)
				{
				}
				FreeConsole();
				SetConsoleCtrlHandler(NULL, false);
			}
		}
		else if (signal == "sigterm")
		{
			HANDLE processHandle;
			processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, pid);
			TerminateProcess(processHandle, 1);
			CloseHandle(processHandle);
		}
	}
	else if (command == "process_openvpn")
	{
		std::string path = params["path"];
		std::string config = params["config"];

		CheckIfExecutableIsAllowed(path);

		std::string checkResult = CheckValidOpenVpnConfig(config);
		if (checkResult != "")
		{
			ThrowException("Not supported OpenVPN config: " + checkResult);
		}
		else
		{
			std::vector<std::string> args;

			args.push_back("--config");
			args.push_back(params["config"]);

			t_shellinfo info = ShellStart(path, args);

			if (info.lastErrorCode != 0)
				ThrowException(info.lastError);

			ReplyCommand(commandId, "procid:" + std::to_string(info.pid));

			for (;;) {
				DWORD bytes_read = 0;
				char tBuf[4096];

				bool stderrEnd = false;
				bool stdoutEnd = false;

				{
					// Don't yet know why, but seem hang with stderr (openvpn only). Doesn't seem really matter.
					stderrEnd = true;
					/*
					// stderr
					if (!ReadFile(info.stderrReadHandle, tBuf, 4096 - 1, &bytes_read, NULL))
					{
						//printf("ReadFile: %u %s\n", GetLastError(), GetLastErrorAsString().c_str());
						stderrEnd = true;
					}
					if (bytes_read > 0)
					{
						tBuf[bytes_read] = '\0';
						ReplyCommand(commandId, "stderr:" + std::string(tBuf));
					}
					*/
				}

				{
					// stdout
					if (!ReadFile(info.stdoutReadHandle, tBuf, 4096 - 1, &bytes_read, NULL))
					{
						//printf("ReadFile: %u %s\n", GetLastError(), GetLastErrorAsString().c_str());
						stdoutEnd = true;
					}
					if (bytes_read > 0)
					{
						tBuf[bytes_read] = '\0';
						ReplyCommand(commandId, "stdout:" + std::string(tBuf));
					}

					if ((stderrEnd) && (stdoutEnd))
						break;
				}
			}

			int exitCode = ShellEnd(info);
			ReplyCommand(commandId, "return:" + std::to_string(exitCode));
		}
	}
	else if (command == "hummingbird")
	{
		// At 2020/02/23, Hummingbird Windows is not public available.
	}
	else
	{
		IBase::Do(commandId, command, params);
	}
}

// --------------------------
// Virtual
// --------------------------

void IWindows::AddTorCookiePaths(const std::string& torPath, const std::string& username, std::vector<std::string>& result)
{
	if (torPath != "") // TorBrowser
	{
		std::string path = FsFileGetDirectory(torPath) + "\\..\\Data\\Tor\\control_auth_cookie";
		result.push_back(path);
	}
}

// --------------------------
// Virtual Pure, OS
// --------------------------

bool IWindows::IsRoot()
{
	BOOL fIsElevated = FALSE;
	HANDLE hToken = NULL;
	TOKEN_ELEVATION elevation;
	DWORD dwSize;

	if (!OpenProcessToken(GetCurrentProcess(), TOKEN_QUERY, &hToken))
	{
		// Failed to get Process Token
		goto Cleanup; // if Failed, we treat as False
	}

	if (!GetTokenInformation(hToken, TokenElevation, &elevation, sizeof(elevation), &dwSize))
	{
		// Failed to get Token Information
		goto Cleanup; // if Failed, we treat as False
	}

	fIsElevated = elevation.TokenIsElevated;

Cleanup:
	if (hToken)
	{
		CloseHandle(hToken);
		hToken = NULL;
	}
	return fIsElevated;
}

void IWindows::Sleep(int ms)
{
	::Sleep(ms);
}

pid_t IWindows::GetCurrentProcessId()
{
	return ::GetCurrentProcessId();
}

pid_t IWindows::GetParentProcessId()
{
	return GetParentProcessId(GetCurrentProcessId());
}

pid_t IWindows::GetParentProcessId(pid_t pid)
{
	HANDLE hSnapshot;
	PROCESSENTRY32 pe32;
	DWORD ppid = 0;

	hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
	__try {
		if (hSnapshot == INVALID_HANDLE_VALUE) __leave;

		ZeroMemory(&pe32, sizeof(pe32));
		pe32.dwSize = sizeof(pe32);
		if (!Process32First(hSnapshot, &pe32)) __leave;

		do {
			if (pe32.th32ProcessID == pid) {
				ppid = pe32.th32ParentProcessID;
				break;
			}
		} while (Process32Next(hSnapshot, &pe32));

	}
	__finally {
		if (hSnapshot != INVALID_HANDLE_VALUE) CloseHandle(hSnapshot);
	}
	return ppid;
}

std::string IWindows::GetProcessPathOfId(pid_t pid)
{
	HANDLE processHandle = NULL;
	WCHAR filename[MAX_PATH];

	processHandle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, FALSE, pid);
	if (processHandle != NULL) {
		std::string result;
		if (GetModuleFileNameExW(processHandle, NULL, filename, MAX_PATH) == 0)
		{
			result = ""; // Failed to get module filename.
		}
		else
		{
			result = StringWStringToUTF8(filename);
		}
		CloseHandle(processHandle);
		return result;
	}
	else
	{
		return ""; // Failed to open process.
	}
}

pid_t IWindows::GetProcessIdOfName(const std::string& name)
{
	HANDLE snap = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);

	PROCESSENTRY32W entry;
	entry.dwSize = sizeof entry;

	if (!Process32FirstW(snap, &entry))
		return 0;

	do {
		std::string exeCurrent = StringWStringToUTF8(entry.szExeFile);
		if (exeCurrent == name)
			return entry.th32ProcessID;
		if (exeCurrent == name + ".exe")
			return entry.th32ProcessID;
	} while (Process32NextW(snap, &entry));

	return 0;
}

std::string IWindows::GetCmdlineOfProcessId(pid_t pid)
{
	// Never used in Windows
	return "";
}

int IWindows::Shell(const std::string& path, const std::vector<std::string>& args, const bool stdinWrite, const std::string& stdinBody, std::string& stdOut, std::string& stdErr)
{
	// stdinWrite, stdinBody not yet supported, not need right now

	t_shellinfo info = ShellStart(path, args);

	if (info.lastErrorCode != 0)
		return info.lastErrorCode;

	for (;;) {
		DWORD bytes_read;
		char tBuf[4096];

		bool stderrEnd = false;
		bool stdoutEnd = false;

		{
			// stderr			
			if (!ReadFile(info.stderrReadHandle, tBuf, 4096 - 1, &bytes_read, NULL))
			{
				//printf("ReadFile: %u %s\n", GetLastError(), GetLastErrorAsString().c_str());
				stderrEnd = true;
			}
			if (bytes_read > 0)
			{
				tBuf[bytes_read] = '\0';
				//printf("stderr:-%s-\n", tBuf);

				stdErr += tBuf;
			}
		}

		{
			// stdout
			if (!ReadFile(info.stdoutReadHandle, tBuf, 4096 - 1, &bytes_read, NULL))
			{
				//printf("ReadFile: %u %s\n", GetLastError(), GetLastErrorAsString().c_str());
				stdoutEnd = true;
			}
			if (bytes_read > 0)
			{
				tBuf[bytes_read] = '\0';
				//printf("stdout:-%s-\n", tBuf);

				stdOut += tBuf;
			}

			if ((stderrEnd) && (stdoutEnd))
				break;
		}
	}

	return ShellEnd(info);
}

void IWindows::FsDirectoryCreate(const std::string& path)
{
	if(FsDirectoryExists(path) == false)
		::CreateDirectoryW(StringUTF8ToWString(path).c_str(), NULL);
}

bool IWindows::FsFileExists(const std::string& path)
{
	std::wstring pathW(StringUTF8ToWString(path));
	if (INVALID_FILE_ATTRIBUTES == ::GetFileAttributes(pathW.c_str()) && ::GetLastError() == ERROR_FILE_NOT_FOUND)
		return false;
	else
		return true;
}

bool IWindows::FsDirectoryExists(const std::string& path)
{
	std::wstring pathW(StringUTF8ToWString(path));
	DWORD ftype = GetFileAttributes(pathW.c_str());
	if (ftype == INVALID_FILE_ATTRIBUTES)
		return false;

	if (ftype & FILE_ATTRIBUTE_DIRECTORY)
		return true;

	return false;
}

void IWindows::FsFileDelete(const std::string& path)
{
	if (FsFileExists(path))
	{
		std::wstring pathW(StringUTF8ToWString(path));
		::DeleteFile(pathW.c_str());
	}
}

void IWindows::FsDirectoryDelete(const std::string& path, bool recursive)
{
	if(FsDirectoryExists(path) == false)
		return;

	if (recursive == false)
	{
		::RemoveDirectoryW(StringUTF8ToWString(path).c_str());
	}
	else
	{
		// Not need yet
	}
}

bool IWindows::FsFileMove(const std::string& source, const std::string& destination)
{
	return (MoveFileW(StringUTF8ToWString(source).c_str(), StringUTF8ToWString(destination).c_str()));
}

std::string IWindows::FsFileReadText(const std::string& path)
{
	std::wstring pathW(StringUTF8ToWString(path));
	std::ifstream f(pathW);
	if (!f) return "";
	std::stringstream buffer;
	buffer << f.rdbuf();
	return buffer.str();
}

std::vector<std::string> IWindows::FsFilesInPath(const std::string& path)
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

std::string IWindows::FsGetTempPath()
{
	WCHAR path[MAX_PATH];
	::GetTempPathW(MAX_PATH, path);
	return StringWStringToUTF8(path);
}

std::vector<std::string> IWindows::FsGetEnvPath()
{
	// In Linux/macOS, we use the path. Anyway, we check if root-owned.
	// In Windows, admin-only check is not yet implemented, but we use only system directory below.
	std::vector<std::string> paths;
	/*
	// ENV edition
	char *pValue;
	size_t len;
	errno_t err = _dupenv_s(&pValue, &len, "PATH");
	if (err)
		return paths;

	paths = StringToVector(pValue, ';', false);
	*/
	WCHAR pathSystem[MAX_PATH];
	::GetSystemDirectoryW(pathSystem, MAX_PATH);
	paths.push_back(StringWStringToUTF8(pathSystem));

	return paths;
}

bool IWindows::SocketIsValid(HSOCKET s)
{
	return (s != INVALID_SOCKET);
}

void IWindows::SocketMarkReuseAddr(HSOCKET s)
{
	// TOFIX, for the moment not yet implemented,
	// understand security issues between SO_REUSEADDR and SO_EXCLUSIVEADDRUSE
}

void IWindows::SocketBlockMode(HSOCKET s, bool block)
{
	unsigned long m = (block ? 0:1);
	if (0 != ioctlsocket(s, FIONBIO, &m))
	{
		ThrowException("Error on SocketBlockMode");
	}
}

void IWindows::SocketClose(HSOCKET s)
{
	closesocket(s);
}




// --------------------------
// Protected
// --------------------------

std::string IWindows::GetLastErrorAsString()
{
	//Get the error message, if any.
	DWORD errorMessageID = ::GetLastError();
	if (errorMessageID == 0)
		return std::string(); //No error message has been recorded

	LPSTR messageBuffer = nullptr;
	size_t size = FormatMessageA(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
		NULL, errorMessageID, MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), (LPSTR)&messageBuffer, 0, NULL);

	std::string message(messageBuffer, size);

	//Free the buffer.
	LocalFree(messageBuffer);

	return message;
}

void IWindows::ShellCleanup(IWindows::t_shellinfo info)
{
	CloseHandle(info.stdoutReadHandle);
	CloseHandle(info.stderrReadHandle);
	CloseHandle(info.processInfo.hProcess);
	CloseHandle(info.processInfo.hThread);
}

IWindows::t_shellinfo IWindows::ShellStart(const std::string path, const std::vector<std::string> args)
{
	t_shellinfo info;

	SECURITY_ATTRIBUTES saAttr;
	HANDLE stdoutWriteHandle = NULL;
	HANDLE stderrWriteHandle = NULL;

	std::wstring pathW = StringUTF8ToWString(path);

	std::wstring cmdline = L"";
	for (std::vector<std::string>::const_iterator i = args.begin(); i != args.end(); ++i)
		cmdline += L" " + StringUTF8ToWString(*i);

	const int lMaxCmdLine = 1024;
	TCHAR pcmdline[lMaxCmdLine + 1];

	//strcpy_s(pcmdline, sizeof(pcmdline), cmdline.c_str());
	wcscpy_s(pcmdline, lMaxCmdLine, cmdline.c_str());

	memset(&saAttr, 0, sizeof(saAttr));
	saAttr.nLength = sizeof(SECURITY_ATTRIBUTES);
	saAttr.bInheritHandle = TRUE;
	saAttr.lpSecurityDescriptor = NULL;

	// Create a pipe for the child process's STDOUT. 
	if (!CreatePipe(&info.stdoutReadHandle, &stdoutWriteHandle, &saAttr, 5000))
	{
		info.lastErrorCode = GetLastError();
		info.lastError = GetLastErrorAsString();
		return info;
	}

	// Ensure the read handle to the pipe for STDOUT is not inherited.
	if (!SetHandleInformation(info.stdoutReadHandle, HANDLE_FLAG_INHERIT, 0))
	{
		info.lastErrorCode = GetLastError();
		info.lastError = GetLastErrorAsString();
		return info;
	}

	// Create a pipe for the child process's STDERR. 
	if (!CreatePipe(&info.stderrReadHandle, &stderrWriteHandle, &saAttr, 5000))
	{
		info.lastErrorCode = GetLastError();
		info.lastError = GetLastErrorAsString();
		return info;
	}

	// Ensure the read handle to the pipe for STDOUT is not inherited.
	if (!SetHandleInformation(info.stderrReadHandle, HANDLE_FLAG_INHERIT, 0))
	{
		info.lastErrorCode = GetLastError();
		info.lastError = GetLastErrorAsString();
		return info;
	}

	memset(&info.startupInfo, 0, sizeof(info.startupInfo));
	info.startupInfo.cb = sizeof(info.startupInfo);
	info.startupInfo.hStdError = stderrWriteHandle;
	info.startupInfo.hStdOutput = stdoutWriteHandle;
	//info.startupInfo.hStdInput = GetStdHandle(STD_INPUT_HANDLE);
	info.startupInfo.dwFlags |= STARTF_USESTDHANDLES;
	info.startupInfo.dwFlags |= STARTF_USESHOWWINDOW;
	//info.startupInfo.dwFlags |= CREATE_NEW_PROCESS_GROUP | DETACHED_PROCESS;	
	info.startupInfo.wShowWindow = SW_HIDE;

	memset(&info.processInfo, 0, sizeof(info.processInfo));

	//if (!CreateProcessW(NULL, pcmdline, NULL, NULL, TRUE, CREATE_NO_WINDOW, NULL, 0, &info.startupInfo, &info.processInfo))
	if (!CreateProcessW(pathW.c_str(), pcmdline, NULL, NULL, TRUE, CREATE_NO_WINDOW, NULL, 0, &info.startupInfo, &info.processInfo))
	{
		ShellCleanup(info);
		info.lastErrorCode = GetLastError();
		info.lastError = GetLastErrorAsString();
		return info;
	}

	info.pid = info.processInfo.dwProcessId;

	CloseHandle(stdoutWriteHandle);
	CloseHandle(stderrWriteHandle);

	return info;
}

DWORD IWindows::ShellEnd(t_shellinfo info)
{
	DWORD exitcode;

	if (WaitForSingleObject(info.processInfo.hProcess, INFINITE) != WAIT_OBJECT_0)
	{
		ShellCleanup(info);
		return GetLastError();
	}

	if (!GetExitCodeProcess(info.processInfo.hProcess, &exitcode))
	{
		ShellCleanup(info);
		return GetLastError();
	}

	ShellCleanup(info);

	return exitcode;
}

std::wstring IWindows::StringUTF8ToWString(std::string str)
{
	std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> conv;
	return conv.from_bytes(str);
}

std::string IWindows::StringWStringToUTF8(std::wstring str)
{
	std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> conv;
	return conv.to_bytes(str);
}