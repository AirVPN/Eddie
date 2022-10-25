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

namespace Eddie.Forms.Mono
{
	public static class NativeX11Methods
	{
#pragma warning disable CA1049

		#region Native datatypes
		public enum RevertTo
		{
			None = 0,
			PointerRoot = 1,
			Parent = 2
		}

		// Managed struct of XSetClassHint classHint.
		public struct XClassHint
		{
			public IntPtr res_name;
			public IntPtr res_class;
		}
		#endregion

		#region Linux/Mono only pinvokes
		[DllImport("libX11", EntryPoint = "XOpenDisplay")]
		public static extern IntPtr XOpenDisplay(IntPtr displayName);

		[DllImport("libX11", EntryPoint = "XCloseDisplay")]
		public static extern uint XCloseDisplay(IntPtr display);

		[DllImport("libX11", EntryPoint = "XQueryKeymap")]
		public static extern void XQueryKeyMap(IntPtr display, [MarshalAs(UnmanagedType.LPArray, SizeConst = 32)] byte[] keys_return);

		[DllImport("libX11", EntryPoint = "XKeysymToKeycode")]
		public static extern uint XKeysymToKeycode(IntPtr display, uint keysym);

		[DllImport("libX11", EntryPoint = "XSetInputFocus")]
		public static extern int XSetInputFocus(IntPtr display, IntPtr window, RevertTo revert_to, IntPtr time);

		[DllImport("libX11", EntryPoint = "XSetClassHint", CharSet = CharSet.Ansi)]
		public static extern int XSetClassHint(IntPtr display, IntPtr window, IntPtr classHint);

		#endregion
	}
}