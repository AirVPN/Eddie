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
using System.Runtime.InteropServices;

using Eddie.Core;

namespace Eddie.Platform.Linux
{
	public static class NativeMethods
	{
		public const string NativeLibName = "Lib.Platform.Linux.Native.so";

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

		[DllImport(NativeLibName)]
		private static extern int eddie_init();
		public static int Init()
		{
			return eddie_init();
		}

		[DllImport(NativeLibName)]
		private static extern int eddie_file_get_mode(string pathname);
		public static int GetFileMode(string pathname)
		{
			return eddie_file_get_mode(pathname);
		}

		[DllImport(NativeLibName)]
		private static extern int eddie_file_set_mode(string pathname, int mode);
		public static int SetFileMode(string pathname, int mode)
		{
			return eddie_file_set_mode(pathname, mode);
		}

		[DllImport(NativeLibName)]
		private static extern int eddie_file_set_mode_str(string pathname, string mode);
		public static int SetFileModeStr(string pathname, string mode)
		{
			return eddie_file_set_mode_str(pathname, mode);
		}

		[DllImport(NativeLibName)]
		private static extern int eddie_file_get_immutable(string filename);
		public static int GetFileImmutable(string filename)
		{
			return eddie_file_get_immutable(filename);
		}

		/*// TOCLEAN18
		[DllImport(NativeLibName)]
		private static extern int eddie_file_set_immutable(string filename, int flag);
		public static int SetFileImmutable(string filename, int flag)
		{
			return eddie_file_set_immutable(filename, flag);
		}
		*/

		[DllImport(NativeLibName)]
		private static extern bool eddie_file_get_runasroot(string filename);
		public static bool GetFileRunAsRoot(string filename)
		{
			return eddie_file_get_runasroot(filename);
		}

		[DllImport(NativeLibName)]
		private static extern int eddie_pipe_write(string filename, string data);
		public static int PipeWrite(string filename, string data)
		{
			return eddie_pipe_write(filename, data);
		}

		public delegate void eddie_sighandler(int signum);
		[DllImport(NativeLibName)]
		private static extern void eddie_signal(int signum, eddie_sighandler handler);
		public static void Signal(int signum, eddie_sighandler handler)
		{
			eddie_signal(signum, handler);
		}

		[DllImport(NativeLibName)]
		private static extern int eddie_kill(int pid, int sig);
		public static int Kill(int pid, int sig)
		{
			return eddie_kill(pid, sig);
		}

		[DllImport(NativeLibName)]
		private static extern void eddie_curl(string jRequest, uint resultMaxLen, byte[] jResult);
		public static Json CUrl(Json jRequest)
		{
			uint resultMaxLen = 1000 * 1000 * 5;
			byte[] resultBuf = new byte[resultMaxLen];
			eddie_curl(jRequest.ToJson(), resultMaxLen, resultBuf);
			Json jResult;
			if (Json.TryParse(System.Text.Encoding.ASCII.GetString(resultBuf), out jResult))
				return jResult;
			else
				throw new Exception("curl unexpected json error");
		}

		[DllImport("__Internal", EntryPoint = "mono_get_runtime_build_info")]
		public extern static string GetMonoVersion();

		[DllImport("libc")]
		public static extern uint getuid();
	}
}
