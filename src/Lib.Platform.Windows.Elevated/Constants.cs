using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Lib.Platform.Windows.Elevated
{
	public static class Constants
	{
		public static bool Debug = false;
		public static int PortSpot = 9345;
		public static int PortService = 9346;
	}
}