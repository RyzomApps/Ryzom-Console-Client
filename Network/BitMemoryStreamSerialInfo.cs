using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace RCC.Network
{
    /// <summary>
    /// Item of CBMSDbgInfo
    /// </summary>
    public class BitMemoryStreamSerialInfo
    {
        public int BitPos;
        public int BitSize;
        public SerialType Type;
        public readonly StackTrace Trace;

        public enum SerialType
        {
            Byte,
            Int,
            UInt,
            UShort,
            Short,
            ULong,
            Long,
            ByteArray,
            Bool,
            String,
            BoolArray,
            Buffer
        }

        public BitMemoryStreamSerialInfo(int bitpos, int bitsize, SerialType type, StackTrace trace)
        {
            BitPos = bitpos;
            BitSize = bitsize;
            Type = type;
            Trace = trace;
        }

        public override string ToString()
        {
            return $"{BitPos} {BitSize} {Type} {Trace.GetFrame(1)?.GetMethod()?.Name}";
        }
    }
}
