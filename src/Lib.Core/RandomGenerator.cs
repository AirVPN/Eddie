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

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Eddie.Core
{
    public static class RandomGenerator
    {
		private static Random m_randomSeed = new Random();

		public static Int32 GetInt(int minValue, int maxValue)
		{
			return m_randomSeed.Next(minValue, maxValue);
		}

		public static byte[] GetBuffer(int length)
		{
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			byte[] output = new byte[length];
			rng.GetBytes(output);
			return output;
		}

		public static string GetHash()
		{
			return BitConverter.ToString(GetBuffer(32)).Replace("-", "").ToLower();
		}
    }
}
