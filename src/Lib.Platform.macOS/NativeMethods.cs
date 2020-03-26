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

using System;
using System.Runtime.InteropServices;

namespace Eddie.Platform.MacOS
{
	public static class NativeMethods
	{
		public const string NativeLibName = "libLib.Platform.macOS.Native.dylib";

		public enum FileMode
		{
			Mode0600 = 33152,
			Mode0644 = 33188
		}

		public enum Signum
		{
			SIGHUP = 1,
			SIGINT = 2,
			SIGQUIT = 3,
			SIGILL = 4,
			SIGTRAP = 5,
			SIGABRT = 6,
			SIGIOT = 6,
			SIGBUS = 7,
			SIGFPE = 8,
			SIGKILL = 9,
			SIGUSR1 = 10,
			SIGSEGV = 11,
			SIGUSR2 = 12,
			SIGPIPE = 13,
			SIGALRM = 14,
			SIGTERM = 15,
			SIGSTKFLT = 16,
			SIGCLD = 17,
			SIGCHLD = 17,
			SIGCONT = 18,
			SIGSTOP = 19,
			SIGTSTP = 20,
			SIGTTIN = 21,
			SIGTTOU = 22,
			SIGURG = 23,
			SIGXCPU = 24,
			SIGXFSZ = 25,
			SIGVTALRM = 26,
			SIGPROF = 27,
			SIGWINCH = 28,
			SIGPOLL = 29,
			SIGIO = 29,
			SIGPWR = 30,
			SIGSYS = 31,
			SIGUNUSED = 31
		}

		/*
		public static string dlerrorMessage()
		{
			IntPtr error = dlerror();
			if (error != IntPtr.Zero)
				return Marshal.PtrToStringAnsi(error);

			return "";
		}

		[DllImport("libdl.so")]
		public static extern IntPtr dlopen(string filename, int flags);

		[DllImport("libdl.so")]
		public static extern IntPtr dlerror(); // [MarshalAs(UnmanagedType.LPStr)] 

		[DllImport("libdl.so")]
		public static extern IntPtr dlsym(IntPtr handle, string symbol);
		*/

		[DllImport(NativeLibName)]
		public static extern int eddie_init();

		[DllImport(NativeLibName)]
		public static extern int eddie_file_get_mode(string pathname);

		[DllImport(NativeLibName)]
		public static extern int eddie_file_set_mode(string pathname, int mode);

		[DllImport(NativeLibName)]
		public static extern int eddie_file_set_mode_str(string pathname, string mode);

		[DllImport(NativeLibName)]
		public static extern int eddie_file_get_immutable(string filename);

		[DllImport(NativeLibName)]
        public static extern bool eddie_file_get_runasroot(string filename);

        [DllImport(NativeLibName)]
		public static extern int eddie_ip_ping(string address, int timeout);

		public delegate void eddie_sighandler(int signum);
		[DllImport(NativeLibName)]
		public static extern void eddie_signal(int signum, eddie_sighandler handler);

        [DllImport(NativeLibName)]
		public static extern int eddie_kill(int pid, int sig);

        [DllImport(NativeLibName)]
        public static extern void eddie_get_realtime_network_stats(byte[] buf, int bufMaxLen);
    }
}
