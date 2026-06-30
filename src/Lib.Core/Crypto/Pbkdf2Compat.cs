// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2026 AirVPN (support@airvpn.org) / https://airvpn.org
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
using System.Security.Cryptography;
using System.Text;

namespace Eddie.Core.Crypto
{
	/// <summary>
	/// PBKDF2 helpers: SHA-256 for new format, SHA-1 only for decrypt of legacy blobs.
	/// Legacy-specific code is under EDDIEMONO4LINUX so it can be removed when Mono build is dropped.
	/// </summary>
	internal static class Pbkdf2Compat
	{
		internal static byte[] DeriveKeySha256(string password, byte[] salt, int iterations, int outputByteCount)
		{
			byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
#if EDDIEMONO4LINUX
			return DeriveKeySha256Manual(passwordBytes, salt, iterations, outputByteCount);
#elif NET5_0_OR_GREATER
			return Rfc2898DeriveBytes.Pbkdf2(passwordBytes, salt, iterations, HashAlgorithmName.SHA256, outputByteCount);
#else
			using (var generator = new Rfc2898DeriveBytes(passwordBytes, salt, iterations, HashAlgorithmName.SHA256))
				return generator.GetBytes(outputByteCount);
#endif
		}

#if EDDIEMONO4LINUX
		static byte[] DeriveKeySha256Manual(byte[] password, byte[] salt, int iterations, int outputByteCount)
		{
			using (var hmac = new HMACSHA256(password))
			{
				int blockCount = (outputByteCount + 31) / 32;
				var result = new byte[blockCount * 32];
				int offset = 0;
				for (int i = 1; i <= blockCount; i++)
				{
					byte[] block = Pbkdf2Block(hmac, salt, iterations, i);
					int toCopy = Math.Min(32, outputByteCount - offset);
					Buffer.BlockCopy(block, 0, result, offset, toCopy);
					offset += toCopy;
				}
				return result.Length == outputByteCount ? result : CopyOf(result, outputByteCount);
			}
		}

		static byte[] Pbkdf2Block(HMACSHA256 hmac, byte[] salt, int iterations, int blockIndex)
		{
			var u = new byte[salt.Length + 4];
			Buffer.BlockCopy(salt, 0, u, 0, salt.Length);
			u[salt.Length + 0] = (byte)(blockIndex >> 24);
			u[salt.Length + 1] = (byte)(blockIndex >> 16);
			u[salt.Length + 2] = (byte)(blockIndex >> 8);
			u[salt.Length + 3] = (byte)blockIndex;
			byte[] block = hmac.ComputeHash(u);
			byte[] prev = block;
			for (int i = 1; i < iterations; i++)
			{
				prev = hmac.ComputeHash(prev);
				for (int j = 0; j < 32; j++)
					block[j] ^= prev[j];
			}
			return block;
		}

		static byte[] CopyOf(byte[] src, int length)
		{
			var d = new byte[length];
			Buffer.BlockCopy(src, 0, d, 0, length);
			return d;
		}
#endif

		internal static void DeriveKeysLegacy(string password, byte[] cryptSalt, byte[] authSalt, int iterations, int keyBytes,
			out byte[] cryptKey, out byte[] authKey)
		{
#if NET5_0_OR_GREATER
			byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
			cryptKey = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, cryptSalt, iterations, HashAlgorithmName.SHA1, keyBytes);
			authKey = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, authSalt, iterations, HashAlgorithmName.SHA1, keyBytes);
#else
#pragma warning disable CA5379 // SHA-1 only for legacy decrypt
#pragma warning disable SYSLIB0041 // 3-param constructor obsolete; intentional for legacy blob compatibility
			using (var generator = new Rfc2898DeriveBytes(password, cryptSalt, iterations))
				cryptKey = generator.GetBytes(keyBytes);
			using (var generator = new Rfc2898DeriveBytes(password, authSalt, iterations))
				authKey = generator.GetBytes(keyBytes);
#pragma warning restore SYSLIB0041
#pragma warning restore CA5379
#endif
		}
	}
}
