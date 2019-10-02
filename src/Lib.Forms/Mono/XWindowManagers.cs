using System;
using System.Runtime.InteropServices;

namespace Eddie.Forms.Mono
{
	public class XWindowManagers
	{
		public static void SetWmClass(string name, string @class, IntPtr handle)
		{			
			var a = new NativeX11Methods.XClassHint { res_name = Marshal.StringToCoTaskMemAnsi(name), res_class = Marshal.StringToCoTaskMemAnsi(@class) };
			IntPtr classHints = Marshal.AllocCoTaskMem(Marshal.SizeOf(a));
			Marshal.StructureToPtr(a, classHints, true);
						
			NativeX11Methods.XSetClassHint(NativeReplacements.MonoGetDisplayHandle(), NativeReplacements.MonoGetX11Window(handle), classHints);			
			
			Marshal.FreeCoTaskMem(a.res_name);
			Marshal.FreeCoTaskMem(a.res_class);
			
			Marshal.FreeCoTaskMem(classHints);
		}	
	}
}

