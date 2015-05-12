using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mycached.Utilities
{
    public static class Utilities
    {
        public static UInt16 ReadUInt16FromBigEndian(this BinaryReader reader)
        {
            byte[] tempBytes = reader.ReadBytes(2);
            return (UInt16)((tempBytes[0] << 8) | tempBytes[1]);
        }

        public static void WriteBigEndian(this BinaryWriter writer, UInt16 value)
        {
            writer.Write((Byte)((value >> 8) & 0xFF));
            writer.Write((Byte)(value & 0xFF));

        }

        public static UInt32 ReadUInt32FromBigEndian(this BinaryReader reader)
        {
            byte[] tempBytes = reader.ReadBytes(4);
            return (UInt32)((tempBytes[0] << 24) | (tempBytes[1] << 16) | 
                            (tempBytes[2] << 8) | tempBytes[3]);
        }

        public static void WriteBigEndian(this BinaryWriter writer, UInt32 value)
        {
            writer.Write((Byte)((value >> 24) & 0xFF));
            writer.Write((Byte)((value >> 16) & 0xFF));
            writer.Write((Byte)((value >> 8) & 0xFF));
            writer.Write((Byte)(value & 0xFF));
        }

        public static UInt64 ReadUInt64FromBigEndian(this BinaryReader reader)
        {
            byte[] tempBytes = reader.ReadBytes(8);
            return (UInt64)((tempBytes[0] << 56) | (tempBytes[1] << 48) | 
                            (tempBytes[2] << 40) | (tempBytes[3] << 32) |
                            (tempBytes[4] << 24) | (tempBytes[5] << 16) | 
                            (tempBytes[6] << 8) | tempBytes[7]);
        }

        public static void WriteBigEndian(this BinaryWriter writer, UInt64 value)
        {
            writer.Write((Byte)((value >> 56) & 0xFF));
            writer.Write((Byte)((value >> 48) & 0xFF));
            writer.Write((Byte)((value >> 40) & 0xFF));
            writer.Write((Byte)((value >> 32) & 0xFF));
            writer.Write((Byte)((value >> 24) & 0xFF));
            writer.Write((Byte)((value >> 16) & 0xFF));
            writer.Write((Byte)((value >> 8) & 0xFF));
            writer.Write((Byte)(value & 0xFF));
        }

        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }
    }
}
