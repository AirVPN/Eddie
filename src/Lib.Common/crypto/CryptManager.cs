// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2016 AirVPN (support@airvpn.org) / https://airvpn.org
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

#if !EDDIENET2
using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Eddie.Common.Crypto
{
	public static class CryptManager
	{
		private static byte[] Transform(ICryptoTransform transform, byte[] data)
		{
			// Avoid double buffer.Dispose() call

			/*
			using(MemoryStream buffer = new MemoryStream())
			{
				using(CryptoStream stream = new CryptoStream(buffer, transform, CryptoStreamMode.Write))
				{
					stream.Write(data, 0, data.Length);
					stream.FlushFinalBlock();
				}

				return buffer.ToArray();
			}
			*/

			MemoryStream buffer = null;
			CryptoStream stream = null;

			try
			{
				buffer = new MemoryStream();
				stream = new CryptoStream(buffer, transform, CryptoStreamMode.Write);

				stream.Write(data, 0, data.Length);
				stream.FlushFinalBlock();

				return buffer.ToArray();
			}
			finally
			{
				if(stream != null)
					stream.Dispose();
				else if(buffer != null)
					buffer.Dispose();
			}
		}

		private static T Init<T>(T cipher, byte[] key, byte[] iv, CipherMode mode, PaddingMode padding) where T : SymmetricAlgorithm
		{
			cipher.Key = key;
			cipher.IV = iv;
			cipher.Mode = mode;
			cipher.Padding = padding;
			return cipher;
		}

		public static byte[] AESEncrypt(byte[] plain, byte[] key, byte[] iv, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7)
		{
			using(Aes aes = Aes.Create())
			{
				return Transform(Init(aes, key, iv, mode, padding).CreateEncryptor(), plain);
			}				
		}

		public static byte[] AESDecrypt(byte[] encrypted, byte[] key, byte[] iv, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7)
		{
			using(Aes aes = Aes.Create())
			{
				return Transform(Init(aes, key, iv, mode, padding).CreateDecryptor(), encrypted);
			}				
		}

		public static byte[] ComputeHash<T>(T algorithm, byte[] data) where T : HashAlgorithm
		{
			return algorithm.ComputeHash(data);
		}

		public static byte[] MD5(byte[] data)
		{
			using(System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
			{
				return ComputeHash(md5, data);
			}				
		}

		public static byte[] SHA1(byte[] data)
		{
			using(System.Security.Cryptography.SHA1 sha1 = System.Security.Cryptography.SHA1.Create())
			{
				return ComputeHash(sha1, data);
			}				
		}

		public static byte[] SHA256(byte[] data)
		{
			using(System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
			{
				return ComputeHash(sha256, data);
			}				
		}

		public static byte[] Generate128BitKey(string value)
		{
			// 16bit
			return MD5(System.Text.Encoding.UTF8.GetBytes(value));
		}

		public static byte[] Generate256BitKey(string value)
		{
			// 32bit
			return SHA256(Encoding.UTF8.GetBytes(value));
		}

		public static byte[] GenerateIV(string value)
		{
			// 16bit
			return MD5(Encoding.UTF8.GetBytes(value));
		}

		public static byte[] RandomBlock(int size, int? seed = null)
		{
			if(size <= 0)
				return null;

			byte[] block = new byte[size];
			Random random = seed != null ? new Random(seed.Value) : new Random();
			random.NextBytes(block);

			return block;
		}
	}
}
#endif