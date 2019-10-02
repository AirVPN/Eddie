using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;

namespace Lib.Platform.Windows.Elevated
{
	public class Conversions
	{
		public static string StringToBase64(string s)
		{
			var bytes = System.Text.Encoding.UTF8.GetBytes(s);
			return System.Convert.ToBase64String(bytes);
		}

		public static string Base64ToString(string s)
		{
			return System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(s));
		}
	}
}