using System;
using System.Runtime.InteropServices;

namespace AirVPN.UI.Osx
{
	public static class RootLauncher
	{
		const string SecurityFramework = "/System/Library/Frameworks/Security.framework/Versions/Current/Security";

		public static bool LaunchExternalTool (string toolPath)
		{
			IntPtr authReference = IntPtr.Zero;
			int result = AuthorizationCreate (IntPtr.Zero, IntPtr.Zero, 0, out authReference);
			if (result != 0) {
				Console.WriteLine ("Error while creating Auth Reference: {0}", result);
				return false;
			}
			AuthorizationExecuteWithPrivileges (authReference, toolPath, 0, new string[] { null }, IntPtr.Zero);
			return true;
		}

		[DllImport (SecurityFramework)]
		extern static int AuthorizationCreate (IntPtr autorizationRights,
		                                       IntPtr environment,
		                                       int authFlags,
		                                       out IntPtr authRef);

		[DllImport (SecurityFramework)]
		extern static int AuthorizationExecuteWithPrivileges (IntPtr authRef,
		                                                      string pathToTool,
		                                                      int authFlags,
		                                                      string[] args,
		                                                      IntPtr pipe);
	}
}

