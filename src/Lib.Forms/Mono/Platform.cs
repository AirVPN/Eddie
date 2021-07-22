using System;

namespace Eddie.Forms.Mono
{
	public static class Platform
	{
		static bool? m_isMono;

		public static bool IsLinux
		{
			get { return Environment.OSVersion.Platform == PlatformID.Unix; }
		}

		public static bool IsWindows
		{
			get { return !IsLinux; }
		}

		public static bool IsMono
		{
			get
			{
				if (m_isMono == null)
					m_isMono = Type.GetType("Mono.Runtime") != null;

				return (bool)m_isMono;
			}
		}

		public static bool IsDotNet
		{
			get { return !IsMono; }
		}
	}
}
