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
