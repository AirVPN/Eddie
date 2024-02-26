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

using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Eddie.Core.Crypto
{
	public static class Manager
	{
		public static byte[] WriteBytesEncrypted(byte[] data, string password)
		{
			byte[] encrypted = AESThenHMAC.SimpleEncryptWithPassword(data, password, Constants.NotSecretPayload);
			return encrypted;
		}

		public static byte[] ReadBytesEncrypted(byte[] encrypted, string password)
		{
			byte[] decrypted = AESThenHMAC.SimpleDecryptWithPassword(encrypted, password, Constants.NotSecretPayload.Length);
			return decrypted;
		}

		public static string HashSHA256(string data)
		{
			using (SHA256 sha256 = SHA256.Create())
			{
#if EDDIE_DOTNET
				byte[] bytes = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(data));
#else
				// ComputeHash in dotnet7 raise
				// warning CA1850: Prefer static 'System.Security.Cryptography.SHA256.HashData' method over 'ComputeHash'
				// but System.Security.Cryptography.SHA256.HashData exists only in dotnet 5 and above.
				byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data), 0, Encoding.UTF8.GetByteCount(data));
#endif
				return ExtensionsString.BytesToHex(bytes);
			}
		}

		public static string HashSHA256File(string path)
		{
			using (FileStream stream = File.OpenRead(path))
			{
				using (SHA256 sha = SHA256.Create())
				{
					byte[] bytes = sha.ComputeHash(stream);
					return ExtensionsString.BytesToHex(bytes);
				}
			}
		}

		public static string HashSHA512File(string path)
		{
			using (FileStream stream = File.OpenRead(path))
			{
				using (SHA512 sha = SHA512.Create())
				{
					byte[] bytes = sha.ComputeHash(stream);
					return ExtensionsString.BytesToHex(bytes);
				}
			}
		}
	}
}
