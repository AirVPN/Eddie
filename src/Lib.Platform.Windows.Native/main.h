// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2016 AirVPN (support@airvpn.org) / https://airvpn.org )
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

#ifndef _MAIN_H_
#define _MAIN_H_

// Standard includes.
#include <Winsock2.h>
#include <conio.h>
#include <strsafe.h>
#include <fwpmu.h>
#include <stdint.h>
#include <vector>

#define XMLBUFSIZE 4096

// ----------------------
// Utils
// ----------------------

extern "C" { __declspec(dllexport) int GetInterfaceMetric(int index, const char* layer); }
extern "C" { __declspec(dllexport) int SetInterfaceMetric(int index, const char* layer, int value); }

// ----------------------
// WFP
// ----------------------

// Firewall sublayer GUID.
GUID m_subLayerGUID;

std::string lastError;
UINT32 lastErrorCode;
std::string subLayerName;
std::wstring subLayerWName;
std::wstring serviceWName;

// Save filter IDs here
std::vector<UINT64> filterids;

UINT16 maxSubLayerWeight = 65535;
UINT64 maxFilterWeight = 18446744073709551615;
UINT64 zeroInterface = 0;

// Firewall engine handle.
HANDLE m_hEngineHandle;

extern "C" { __declspec(dllexport) void LibPocketFirewallInit(const char* name); }

extern "C" { __declspec(dllexport) BOOL LibPocketFirewallStart(const char* xml); }
extern "C" { __declspec(dllexport) BOOL LibPocketFirewallStop(); }

extern "C" { __declspec(dllexport) UINT64 LibPocketFirewallAddRule(const char* xml); }
extern "C" { __declspec(dllexport) BOOL LibPocketFirewallRemoveRule(const UINT64 id); }
extern "C" { __declspec(dllexport) BOOL LibPocketFirewallRemoveRuleDirect(const UINT64 id); }

extern "C" { __declspec(dllexport) const char* LibPocketFirewallGetLastError(); }
extern "C" { __declspec(dllexport) DWORD LibPocketFirewallGetLastErrorCode(); }


#endif