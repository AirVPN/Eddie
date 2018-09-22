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

namespace Eddie.Common
{
    public static class Converter
    {
        public static UInt16 MakeUint16(byte lo, byte hi)
        {
            byte[] values = { lo, hi };
            return BitConverter.ToUInt16(values, 0);
        }

        public static byte GetUInt16Lo(UInt16 v)
        {
            return BitConverter.GetBytes(v)[0];
        }

        public static byte GetUInt16Hi(UInt16 v)
        {
            return BitConverter.GetBytes(v)[1];
        }

        public static UInt32 MakeUint32(UInt16 lo, UInt16 hi)
        {
            byte[] loValues = BitConverter.GetBytes(lo);
            byte[] hiValues = BitConverter.GetBytes(hi);
            byte[] values = { loValues[0], loValues[1], hiValues[0], hiValues[1] };
            return BitConverter.ToUInt32(values, 0);            
        }

        public static UInt16 GetUInt32Lo(UInt32 v)
        {
            byte[] bytes = BitConverter.GetBytes(v);
            return MakeUint16(bytes[0], bytes[1]);
        }

        public static UInt16 GetUInt32Hi(UInt32 v)
        {
            byte[] bytes = BitConverter.GetBytes(v);
            return MakeUint16(bytes[2], bytes[3]);
        }
    }
}
