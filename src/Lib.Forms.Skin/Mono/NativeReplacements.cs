using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

#pragma warning disable CA1060 // Because it is a P/Invoke method, 'NativeReplacements.WindowsGetFocus()' should be defined in a class named NativeMethods, SafeNativeMethods, or UnsafeNativeMethods.Lib.Forms

namespace Eddie.Forms.Mono
{
	public static class NativeReplacements
	{
		private static Assembly _monoWinFormsAssembly;

		// internal mono WinForms type
		private static Type _xplatUIX11;

		// internal mono WinForms type
		private static Type _xplatUI;

		// internal mono WinForms type
		private static Type _hwnd;

		private static Assembly MonoWinFormsAssembly
		{
			get
			{
				if (_monoWinFormsAssembly == null)
#pragma warning disable 0618 // Using Obsolete method LoadWithPartialName, VS CS0618
#pragma warning disable 0612 // Using Obsolete method LoadWithPartialName.
					_monoWinFormsAssembly = Assembly.LoadWithPartialName("System.Windows.Forms");
#pragma warning restore 0612
#pragma warning restore 0618


				return _monoWinFormsAssembly;
			}
		}

		private static Type XplatUIX11
		{
			get
			{
				if (_xplatUIX11 == null)
					_xplatUIX11 = MonoWinFormsAssembly.GetType("System.Windows.Forms.XplatUIX11");

				return _xplatUIX11;
			}
		}

		private static Type XplatUI
		{
			get
			{
				if (_xplatUI == null)
					_xplatUI = MonoWinFormsAssembly.GetType("System.Windows.Forms.XplatUI");

				return _xplatUI;
			}
		}

		private static Type Hwnd
		{
			get
			{
				if (_hwnd == null)
					_hwnd = MonoWinFormsAssembly.GetType("System.Windows.Forms.Hwnd");

				return _hwnd;
			}
		}

		#region GetFocus
		/// <summary>
		/// Gets the focus.
		/// </summary>
		/// <returns>
		/// The focus.
		/// </returns>
		public static IntPtr GetFocus()
		{
			if (Platform.IsWindows)
				return WindowsGetFocus();

			return MonoGetFocus();
		}

		/// <summary></summary>
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		static extern IntPtr WindowsGetFocus();

		// internal mono WinForms static instance that traces focus
		private static FieldInfo _focusWindow;

		// internal mono Winforms static instance handle to the X server.
		public static FieldInfo _displayHandle;

		// internal mono WinForms Hwnd.whole_window
		internal static FieldInfo _wholeWindow;

		// internal mono WinForms method Hwnd.ObjectFromHandle
		internal static MethodInfo _objectFromHandle;

		/// <summary>
		/// Gets mono's internal Focused Window Ptr/Handle.
		/// </summary>
		public static IntPtr MonoGetFocus()
		{
			if (_focusWindow == null)
				_focusWindow = XplatUIX11.GetField("FocusWindow",
					System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

			// Get static field to determine Focused Window.
			return (IntPtr)_focusWindow.GetValue(null);
		}

		/// <summary>
		/// Get mono's internal display handle to the X server
		/// </summary>		
		public static IntPtr MonoGetDisplayHandle()
		{
			if (_displayHandle == null)
				_displayHandle = XplatUIX11.GetField("DisplayHandle",
					System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

			return (IntPtr)_displayHandle.GetValue(null);
		}

		private static object GetHwnd(IntPtr handle)
		{
			// first call call Hwnd.ObjectFromHandle to get the hwnd.
			if (_objectFromHandle == null)
				_objectFromHandle = Hwnd.GetMethod("ObjectFromHandle", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

			return _objectFromHandle.Invoke(null, new object[] { handle });
		}

		/// <summary>
		/// Get an x11 Window Id from a winforms Control handle
		/// </summary>		
		public static IntPtr MonoGetX11Window(IntPtr handle)
		{
			if (handle == IntPtr.Zero)
				return IntPtr.Zero;

			object hwnd = GetHwnd(handle);

			if (_wholeWindow == null)
				_wholeWindow = Hwnd.GetField("whole_window", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

			return (IntPtr)_wholeWindow.GetValue(hwnd);
		}
		#endregion

		#region SendSetFocusWindowsMessage

		public static void SendSetFocusWindowsMessage(Control control, IntPtr fromHandle)
		{
			if (Platform.IsWindows)
				NativeSendMessage(control.Handle, WM_SETFOCUS, (int)fromHandle, 0);
			else
				control.Focus();
		}

		public const int WM_SETFOCUS = 0x7;

		[DllImport("user32.dll", EntryPoint = "SendMessage")]
		static extern int NativeSendMessage(
			IntPtr hWnd,      // handle to destination window
			uint Msg,     // message
			int wParam,  // first message parameter
			int lParam   // second message parameter
			);

		#endregion

		#region SendMessage

		private static MethodInfo _sendMessage;

		/// <summary>
		/// Please don't use this unless your really have to, and then only if its for sending messages internaly within the application.
		/// For example sending WM_NCPAINT maybe portable but sending WM_USER + N to another application is definitely not poratable.
		/// </summary>
		public static void SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam)
		{
			if (Platform.IsDotNet)
			{
				NativeSendMessage(hWnd, Msg, wParam, lParam);
			}
			else
			{
				if (_sendMessage == null)
					_sendMessage = XplatUI.GetMethod("SendMessage",
						System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static, null, new Type[] { typeof(IntPtr), typeof(int), typeof(IntPtr), typeof(IntPtr) }, null);

				_sendMessage.Invoke(null, new object[] { hWnd, (int)Msg, (IntPtr)wParam, (IntPtr)lParam });
			}
		}

		#endregion

		#region Set WM_CLASS

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

		#endregion
	}
}

