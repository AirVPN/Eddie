// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2019 AirVPN (support@airvpn.org) / https://airvpn.org
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using Microsoft.Win32;

namespace Lib.Platform.Windows.Elevated
{
	public static class NativeMethods
	{
		#region Lib.Platform.Windows.Native

		public const string NativeLibName = "Lib.Platform.Windows.Native.dll";

		[DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern int eddie_init();
		public static int Init()
		{
			return eddie_init();
		}

		[DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern int eddie_get_interface_metric(int idx, string layer);
		public static int GetInterfaceMetric(int idx, string layer)
		{
			return eddie_get_interface_metric(idx, layer);
		}

		[DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern int eddie_set_interface_metric(int idx, string layer, int value);
		public static int SetInterfaceMetric(int idx, string layer, int value)
		{
			return eddie_set_interface_metric(idx, layer, value);
		}

		[DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern void eddie_wfp_init(string name);
		public static void WfpInit(string name)
		{
			eddie_wfp_init(name);
		}


		[DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern bool eddie_wfp_start(string xml);
		public static bool WfpStart(string xml)
		{
			return eddie_wfp_start(xml);
		}

		[DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern bool eddie_wfp_stop();
		public static bool WfpStop()
		{
			return eddie_wfp_stop();
		}

		[DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern UInt64 eddie_wfp_rule_add(string xml);
		public static UInt64 WfpRuleAdd(string xml)
		{
			return eddie_wfp_rule_add(xml);
		}

		[DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern bool eddie_wfp_rule_remove(UInt64 id);
		public static bool WfpRuleRemove(UInt64 id)
		{
			return eddie_wfp_rule_remove(id);
		}

		[DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern bool eddie_wfp_rule_remove_direct(UInt64 id);
		public static bool WfpRuleRemoveDirect(UInt64 id)
		{
			return eddie_wfp_rule_remove_direct(id);
		}

		[DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr eddie_wfp_get_last_error();

		[DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern UInt32 eddie_wfp_get_last_error_code();

		public static string WfpGetLastError()
		{
			IntPtr result = eddie_wfp_get_last_error();
			string s = Marshal.PtrToStringAnsi(result);
			UInt32 code = eddie_wfp_get_last_error_code();
			if (code != 0)
				s += " (0x" + code.ToString("x") + ")";
			return s;
		}

		#endregion

		#region Windows

		public static uint TH32CS_SNAPPROCESS = 2;

		[StructLayout(LayoutKind.Sequential)]
		public struct PROCESSENTRY32
		{
			public uint dwSize;
			public uint cntUsage;
			public uint th32ProcessID;
			public IntPtr th32DefaultHeapID;
			public uint th32ModuleID;
			public uint cntThreads;
			public uint th32ParentProcessID;
			public int pcPriClassBase;
			public uint dwFlags;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szExeFile;
		};

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

		[DllImport("kernel32.dll")]
		public static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

		[DllImport("kernel32.dll")]
		public static extern bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

		// Enum for different possible states of TCP connection 
		public enum MibTcpState
		{
			CLOSED = 1,
			LISTENING = 2,
			SYN_SENT = 3,
			SYN_RCVD = 4,
			ESTABLISHED = 5,
			FIN_WAIT1 = 6,
			FIN_WAIT2 = 7,
			CLOSE_WAIT = 8,
			CLOSING = 9,
			LAST_ACK = 10,
			TIME_WAIT = 11,
			DELETE_TCB = 12,
			NONE = 0
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct MIB_TCPROW_OWNER_PID
		{
			public MibTcpState state;
			public uint localAddr;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
			public byte[] localPort;
			public uint remoteAddr;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
			public byte[] remotePort;
			public int owningPid;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct MIB_TCPTABLE_OWNER_PID
		{
			public uint dwNumEntries;
			[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct,
				SizeConst = 1)]
			public MIB_TCPROW_OWNER_PID[] table;
		}

		public enum TCP_TABLE_CLASS
		{
			TCP_TABLE_BASIC_LISTENER,
			TCP_TABLE_BASIC_CONNECTIONS,
			TCP_TABLE_BASIC_ALL,
			TCP_TABLE_OWNER_PID_LISTENER,
			TCP_TABLE_OWNER_PID_CONNECTIONS,
			TCP_TABLE_OWNER_PID_ALL,
			TCP_TABLE_OWNER_MODULE_LISTENER,
			TCP_TABLE_OWNER_MODULE_CONNECTIONS,
			TCP_TABLE_OWNER_MODULE_ALL
		}

		public const int AF_INET = 2;

		[DllImport("iphlpapi.dll", SetLastError = true)]
		public static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int dwOutBufLen, bool sort, int ipVersion, TCP_TABLE_CLASS tblClass, uint reserved = 0);

		#endregion
	}
}
