// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2023 AirVPN (support@airvpn.org) / https://airvpn.org )
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

			// Remember: called program still have uid as normal user. Use setuid().

			result = AuthorizationExecuteWithPrivileges(authReference, toolPath, 0, args, IntPtr.Zero);
			AuthorizationFree(authReference, AuthorizationFlags.DestroyRights);
			return (result == 0);
		}

		public enum AuthorizationStatus
		{
			Success = 0,
			InvalidSet = -60001,
			InvalidRef = -60002,
			InvalidTag = -60003,
			InvalidPointer = -60004,
			Denied = -60005,
			Canceled = -60006,
			InteractionNotAllowed = -60007,
			Internal = -60008,
			ExternalizeNotAllowed = -60009,
			InternalizeNotAllowed = -60010,
			InvalidFlags = -60011,
			ToolExecuteFailure = -60031,
			ToolEnvironmentError = -60032,
			BadAddress = -60033,
		}

		[Flags]
		public enum AuthorizationFlags
		{
			Defaults,
			InteractionAllowed = 1 << 0,
			ExtendRights = 1 << 1,
			PartialRights = 1 << 2,
			DestroyRights = 1 << 3,
			PreAuthorize = 1 << 4,
		}

		//
		// For ease of use, we let the user pass the AuthorizationParameters, and we
		// create the structure for them with the proper data
		//
		public class AuthorizationParameters
		{
			public string PathToSystemPrivilegeTool;
			public string Prompt;
			public string IconPath;
		}

		public class AuthorizationEnvironment
		{
			public string Username;
			public string Password;
			public bool AddToSharedCredentialPool;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct AuthorizationItem
		{
			public IntPtr name;
			public IntPtr valueLen;
			public IntPtr value;
			public int flags;  // zero
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

		[DllImport(SecurityFramework)]
		extern static int AuthorizationFree(IntPtr handle, AuthorizationFlags flags);
	}
}

