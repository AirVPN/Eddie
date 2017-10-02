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

using System;
using System.Runtime.InteropServices;

namespace Eddie.Platform.MacOS
{
	public static class RootLauncher
	{
		const string SecurityFramework = "/System/Library/Frameworks/Security.framework/Versions/Current/Security";

		public static bool LaunchExternalTool(string toolPath, string[] args)
		{
			IntPtr authReference = IntPtr.Zero;
			int result = AuthorizationCreate(IntPtr.Zero, IntPtr.Zero, 0, out authReference);
			if (result != 0)
			{
				Console.WriteLine("Error while creating Auth Reference: {0}", result);
				return false;
			}
			AuthorizationExecuteWithPrivileges(authReference, toolPath, 0, args, IntPtr.Zero);
			return true;
		}

		[DllImport(SecurityFramework)]
		extern static int AuthorizationCreate(IntPtr autorizationRights,
											   IntPtr environment,
											   int authFlags,
											   out IntPtr authRef);

		[DllImport(SecurityFramework)]
		extern static int AuthorizationExecuteWithPrivileges(IntPtr authRef,
															  string pathToTool,
															  int authFlags,
															  string[] args,
															  IntPtr pipe);
	}
}

