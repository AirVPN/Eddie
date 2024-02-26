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


// This program is GUI and not a simple Console, to avoid UAC elevation show console when launched with ShellExecute=true.

#include <winsock2.h>
#include "..\include\impl.h"
#include <Ws2tcpip.h>
#include <ws2ipdef.h>
#include <iphlpapi.h>

#include "..\include\main.h"

int APIENTRY wWinMain(_In_ HINSTANCE hInstance,
	_In_opt_ HINSTANCE hPrevInstance,
	_In_ LPWSTR    lpCmdLine,
	_In_ int       nCmdShow)
{
	UNREFERENCED_PARAMETER(hInstance);
	UNREFERENCED_PARAMETER(hPrevInstance);
	UNREFERENCED_PARAMETER(nCmdShow);

	Impl impl;

	std::vector<std::string> args;

	WCHAR path[MAX_PATH];
	GetModuleFileNameW(NULL, path, MAX_PATH);
	args.push_back(impl.StringWStringToUTF8(path));

	std::wstring wArgs = lpCmdLine;
	std::wstring arg;
	bool inQuote = false;
	for (size_t c = 0; c < wArgs.size(); c++)
	{
		if( (wArgs[c] == L' ') && (inQuote == false) )
		{
			args.push_back(impl.StringWStringToUTF8(arg));
			arg = L"";
		}
		else
		{
			arg += wArgs[c];

			if (wArgs[c] == L'\"') inQuote = !inQuote;
		}
	}
	if(arg.size()>0)
		args.push_back(impl.StringWStringToUTF8(arg));

	return impl.AppMain(args);
}

