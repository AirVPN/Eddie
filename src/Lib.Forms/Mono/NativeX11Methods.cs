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
		[DllImport ("libX11", EntryPoint="XOpenDisplay")]
		public static extern IntPtr XOpenDisplay(IntPtr displayName);
	
		[DllImport ("libX11", EntryPoint="XCloseDisplay")]
		public static extern uint XCloseDisplay (IntPtr display);
	
		[DllImport ("libX11", EntryPoint="XQueryKeymap")]
		public static extern void XQueryKeyMap(IntPtr display, [MarshalAs(UnmanagedType.LPArray, SizeConst = 32)] byte[] keys_return);
	
		[DllImport ("libX11", EntryPoint="XKeysymToKeycode")]
		public static extern uint XKeysymToKeycode(IntPtr display, uint keysym);
		
		[DllImport ("libX11", EntryPoint="XSetInputFocus")]
		public extern static int XSetInputFocus(IntPtr display, IntPtr window, RevertTo revert_to, IntPtr time);		
		
		[DllImport ("libX11", EntryPoint="XSetClassHint", CharSet=CharSet.Ansi)]
		public extern static int XSetClassHint(IntPtr display, IntPtr window, IntPtr classHint);
		
		#endregion
	}
}