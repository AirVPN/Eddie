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

		public static string HashSHA256(string password)
		{
			using (System.Security.Cryptography.SHA256Managed crypt = new System.Security.Cryptography.SHA256Managed())
			{
				System.Text.StringBuilder hash = new System.Text.StringBuilder();
				byte[] bytes = crypt.ComputeHash(Encoding.UTF8.GetBytes(password), 0, Encoding.UTF8.GetByteCount(password));
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
