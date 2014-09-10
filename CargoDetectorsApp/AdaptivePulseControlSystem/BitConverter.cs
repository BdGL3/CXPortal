using System;
using Microsoft.SPOT;
using System.Reflection;
using System.Net;
using Microsoft.SPOT.Hardware;

namespace L3.Cargo.Communications.APCS.Common
{
    public static class BitConverter
    {
        public static byte[] GetBytes(uint value)
        {
            return new byte[4] {
                (byte)(value & 0xFF),
                (byte)((value >> 8) & 0xFF),
                (byte)((value >> 16) & 0xFF),
                (byte)((value >> 24) & 0xFF) };
        }

        public static byte[] GetBytes(int value)
        {
            return new byte[4] {
                (byte)(value & 0xFF),
                (byte)((value >> 8) & 0xFF),
                (byte)((value >> 16) & 0xFF),
                (byte)((value >> 24) & 0xFF) };
        }

        public static byte[] GetBytes(short value)
        {
            return new byte[2] {
                (byte)(value & 0xFF),
                (byte)((value >> 8) & 0xFF) };
        }

        public static byte[] GetBytes(ushort value)
        {
            return new byte[2] {
                (byte)(value & 0xFF),
                (byte)((value >> 8) & 0xFF) };
        }

        public static unsafe byte[] GetBytes(float value)
        {
            uint val = *((uint*)&value);
            return GetBytes(val);
        }

        
        public static unsafe byte[] GetBytes(float value, ByteOrder order)
        {
            byte[] bytes = GetBytes(value);
            if (order != ByteOrder.LittleEndian)
            {
                //reserver order
                ReverseArray(bytes);
            }

            return bytes;
        }

        public static uint ToUInt32(byte[] value, int index)
        {
            return (uint)(
                value[0 + index] << 0 |
                value[1 + index] << 8 |
                value[2 + index] << 16 |
                value[3 + index] << 24);
        }

        public static int ToInt32(byte[] value, int index)
        {
            return (int)(
                value[0 + index] << 0 |
                value[1 + index] << 8 |
                value[2 + index] << 16 |
                value[3 + index] << 24);
        }

        public static int ToUInt16(byte[] value, int index)
        {
            return (ushort)(
                value[0 + index] << 0 |
                value[1 + index] << 8 );
        }

        public static int ToInt16(byte[] value, int index)
        {
            return (short)(
                value[0 + index] << 0 |
                value[1 + index] << 8);
        }

        public static unsafe float ToSingle(byte[] value, int index)
        {
            uint i = ToUInt32(value, index);
            return *(((float*)&i));
        }

        public static unsafe float ToSingle(byte[] value, int index, ByteOrder order)
        {
            if (order != ByteOrder.LittleEndian)
            {
                //reserver order
                ReverseArray(value);
            }

            return ToSingle(value, index);
        }

        public enum ByteOrder
        {
            LittleEndian,
            BigEndian
        }

        static public bool IsLittleEndian
        {
            get
            {
                unsafe
                {
                    int i = 1;
                    byte* p = (byte*)&i;

                    return (p[0] == 1);
                }
            }
        }


        private static void ReverseArray(byte[] array)
        {
            byte[] originalArray = new byte[array.Length];

            array.CopyTo(originalArray, 0);

            for (int index = 0; index < array.Length; index++)
            {
                array[array.Length - index - 1] = originalArray[index];
            }

        }
    }
}
