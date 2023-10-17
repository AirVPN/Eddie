// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2023 AirVPN (support@airvpn.org) / https://airvpn.org
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.Win32.SafeHandles;

namespace Eddie.Platform.Windows
{
	public static class CredentialManager
	{
		public static string Read(string applicationName, string userName)
		{
			try
			{
				Credential c = ReadCredential2(applicationName + " - " + userName);
				if (c != null)
					return c.Password;
				else
					return "";
			}
			catch
			{
				return "";
			}
		}

		public static bool Delete(string applicationName, string userName)
		{
			try
			{
				return CredDelete(applicationName + " - " + userName, CredentialType.Generic, 0);
			}
			catch
			{
				return false;
			}
		}

		public static bool Write(string applicationName, string userName, string password)
		{
			try
			{
				return WriteCredential(applicationName + " - " + userName, userName, password);
			}
			catch
			{
				return false;
			}
		}

		public static Credential ReadCredential2(string applicationName)
		{
			IntPtr nCredPtr;
			bool read = CredRead(applicationName, CredentialType.Generic, 0, out nCredPtr);
			if (read)
			{
				using (CriticalCredentialHandle critCred = new CriticalCredentialHandle(nCredPtr))
				{
					CREDENTIAL cred = critCred.GetCredential();
					return ReadCredential(cred);
				}
			}

			return null;
		}

		private static Credential ReadCredential(CREDENTIAL credential)
		{
			string applicationName = Marshal.PtrToStringUni(credential.TargetName);
			string userName = Marshal.PtrToStringUni(credential.UserName);
			string secret = null;
			if (credential.CredentialBlob != IntPtr.Zero)
			{
				secret = Marshal.PtrToStringUni(credential.CredentialBlob, (int)credential.CredentialBlobSize / 2);
			}

			return new Credential(credential.Type, applicationName, userName, secret);
		}

		public static bool WriteCredential(string applicationName, string userName, string secret)
		{
			byte[] byteArray = Encoding.Unicode.GetBytes(secret);
			if (byteArray.Length > 512)
				throw new ArgumentOutOfRangeException("secret", "The secret message has exceeded 512 bytes.");

			CREDENTIAL credential = new CREDENTIAL();
			credential.AttributeCount = 0;
			credential.Attributes = IntPtr.Zero;
			credential.Comment = IntPtr.Zero;
			credential.TargetAlias = IntPtr.Zero;
			credential.Type = CredentialType.Generic;
			credential.Persist = (UInt32)CredentialPersistence.LocalMachine;
			credential.CredentialBlobSize = (UInt32)Encoding.Unicode.GetBytes(secret).Length;
			credential.TargetName = Marshal.StringToCoTaskMemUni(applicationName);
			credential.CredentialBlob = Marshal.StringToCoTaskMemUni(secret);
			credential.UserName = Marshal.StringToCoTaskMemUni(userName ?? Environment.UserName);

			bool written = CredWrite(ref credential, 0);
			// int lastError = Marshal.GetLastWin32Error();

			Marshal.FreeCoTaskMem(credential.TargetName);
			Marshal.FreeCoTaskMem(credential.CredentialBlob);
			Marshal.FreeCoTaskMem(credential.UserName);

			if (written)
				return true;
			else
				return false;
		}

		public static List<Credential> EnumerateCrendentials()
		{
			List<Credential> result = new List<Credential>();

			int count;
			IntPtr pCredentials;
			bool ret = CredEnumerate(null, 0, out count, out pCredentials);
			if (ret)
			{
				for (int n = 0; n < count; n++)
				{
					IntPtr credential = Marshal.ReadIntPtr(pCredentials, n * Marshal.SizeOf(typeof(IntPtr)));
					result.Add(ReadCredential((CREDENTIAL)Marshal.PtrToStructure(credential, typeof(CREDENTIAL))));
				}
			}
			else
			{
				int lastError = Marshal.GetLastWin32Error();
				throw new Win32Exception(lastError);
			}

			return result;
		}

		[DllImport("Advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
		static extern bool CredRead(string target, CredentialType type, int reservedFlag, out IntPtr credentialPtr);

		[DllImport("Advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
		static extern bool CredWrite([In] ref CREDENTIAL userCredential, [In] UInt32 flags);

		[DllImport("advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode)]
		private static extern bool CredDelete(string target, CredentialType type, int flags);

		[DllImport("advapi32", SetLastError = true, CharSet = CharSet.Unicode)]
		static extern bool CredEnumerate(string filter, int flag, out int count, out IntPtr pCredentials);

		[DllImport("Advapi32.dll", EntryPoint = "CredFree", SetLastError = true)]
		static extern bool CredFree([In] IntPtr cred);



		private enum CredentialPersistence : uint
		{
			Session = 1,
			LocalMachine,
			Enterprise
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct CREDENTIAL
		{
			public UInt32 Flags;
			public CredentialType Type;
			public IntPtr TargetName;
			public IntPtr Comment;
			public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
			public UInt32 CredentialBlobSize;
			public IntPtr CredentialBlob;
			public UInt32 Persist;
			public UInt32 AttributeCount;
			public IntPtr Attributes;
			public IntPtr TargetAlias;
			public IntPtr UserName;
		}

		sealed class CriticalCredentialHandle : CriticalHandleZeroOrMinusOneIsInvalid
		{
			public CriticalCredentialHandle(IntPtr preexistingHandle)
			{
				SetHandle(preexistingHandle);
			}

			public CREDENTIAL GetCredential()
			{
				if (!IsInvalid)
				{
					CREDENTIAL credential = (CREDENTIAL)Marshal.PtrToStructure(handle, typeof(CREDENTIAL));
					return credential;
				}

				throw new InvalidOperationException("Invalid CriticalHandle!");
			}

			protected override bool ReleaseHandle()
			{
				if (!IsInvalid)
				{
					CredFree(handle);
					SetHandleAsInvalid();
					return true;
				}

				return false;
			}
		}
	}

	public enum CredentialType
	{
		Generic = 1,
		DomainPassword,
		DomainCertificate,
		DomainVisiblePassword,
		GenericCertificate,
		DomainExtended,
		Maximum,
		MaximumEx = Maximum + 1000,
	}

	public class Credential
	{
		private readonly string _applicationName;
		private readonly string _userName;
		private readonly string _password;
		private readonly CredentialType _credentialType;

		public CredentialType CredentialType
		{
			get { return _credentialType; }
		}

		public string ApplicationName
		{
			get { return _applicationName; }
		}

		public string UserName
		{
			get { return _userName; }
		}

		public string Password
		{
			get { return _password; }
		}

		public Credential(CredentialType credentialType, string applicationName, string userName, string password)
		{
			_applicationName = applicationName;
			_userName = userName;
			_password = password;
			_credentialType = credentialType;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "CredentialType: {0}, ApplicationName: {1}, UserName: {2}, Password: {3}", CredentialType, ApplicationName, UserName, Password);
		}
	}
}