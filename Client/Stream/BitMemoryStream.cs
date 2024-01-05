///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Client.Stream
{
    /// <summary>
    /// (de)serializes data for server communication
    /// </summary>
    public class BitMemoryStream
    {
        private int _bitPos;
        private bool[] _contentBits;
        private bool _inputStream;
        private readonly List<BitMemoryStreamSerialInfo> _debugData = new List<BitMemoryStreamSerialInfo>();

        /// <summary>
        /// Constructor differentiating between input and output stream and specifying the size of the stream
        /// </summary>
        public BitMemoryStream(byte[] input)
        {
            _contentBits = new bool[0];
            _inputStream = true;
            MemCpy(input);
        }

        /// <summary>
        /// Constructor differentiating between input and output stream and specifying the size of the stream
        /// </summary>
        public BitMemoryStream(bool inputStream = false, int defaultcapacity = 32)
        {
            _inputStream = inputStream;
            _contentBits = new bool[defaultcapacity * 8];
        }

        /// <summary>
        /// Byte Position of the reader/writer in the Stream
        /// </summary>
        public int Pos => (int)(_bitPos / 8d);

        /// <summary>
        /// Free bits in the current byte of the reader/writer
        /// </summary>
        public int FreeBits => 8 - _bitPos % 8;

        /// <summary>
        /// Length in bytes of the overall stream (reader) or length of the written bytes (writer)
        /// </summary>
        public int Length
        {
            get
            {
                if (IsReading())
                    return (int)(_contentBits.Length / 8d); // (_contentBits.Length - 1) / 8 + 1;

                return (int)((_bitPos - 1) / 8d) + 1;
            }
        }

        public string DebugData
        {
            get
            {
                return _debugData.Aggregate("", (current, data) => current + $"{data}\r\n");
            }
        }

        /// <summary>
        /// input or output
        /// </summary>
        public bool IsReading() => _inputStream;

        /// <summary>
        /// Returns the number of bit from the beginning of the buffer (in bit)
        /// </summary>
        public int GetPosInBit()
        {
            return (Pos + 1) * 8 - FreeBits;
        }

        /// <summary>
        /// Builds the header that is needed for the communication with the ryzom server
        /// TODO: this shouldn't be here, but it is - move to a better position
        /// </summary>
        public void BuildSystemHeader(ref int currentSendNumber)
        {
            Serial(ref currentSendNumber);
            var systemmode = true;
            Serial(ref systemmode); // systemmode
            ++currentSendNumber;
        }

        /// <summary>
        /// serializes type byte
        /// </summary>
        public void Serial(ref byte obj)
        {
            if (Constants.BitMemoryStreamDebugEnabled) _debugData.Add(new BitMemoryStreamSerialInfo(_bitPos, 8, BitMemoryStreamSerialInfo.SerialType.Byte, new StackTrace(true)));

            if (IsReading())
            {
                var newBits = ReadFromArray(8);
                obj = ConvertBoolArrayToByteArray(newBits)[0];
            }
            else
            {
                byte[] bytes = { obj };
                AddToArray(bytes);
            }
        }

        /// <summary>
        /// serializes type sint32
        /// </summary>
        public void Serial(ref int obj)
        {
            if (Constants.BitMemoryStreamDebugEnabled) _debugData.Add(new BitMemoryStreamSerialInfo(_bitPos, 32, BitMemoryStreamSerialInfo.SerialType.Int, new StackTrace(true)));

            if (IsReading())
            {
                var newBits = ReadFromArray(32);
                var reversed = ConvertBoolArrayToByteArray(newBits).Reverse().ToArray();
                obj = BitConverter.ToInt32(reversed);
            }
            else
            {
                var bytes = BitConverter.GetBytes(obj);
                AddToArray(bytes);
            }
        }

        /// <summary>
        /// serializes type float
        /// </summary>
        public void Serial(ref float obj)
        {
            if (IsReading())
            {
                throw new NotImplementedException();
            }
            else
            {
                var bytes = BitConverter.GetBytes(obj);
                AddToArray(bytes);
            }
        }

        /// <summary>
        /// serializes type uint32
        /// </summary>
        public void Serial(ref uint obj)
        {
            if (Constants.BitMemoryStreamDebugEnabled) _debugData.Add(new BitMemoryStreamSerialInfo(_bitPos, 32, BitMemoryStreamSerialInfo.SerialType.UInt, new StackTrace(true)));

            if (IsReading())
            {
                var newBits = ReadFromArray(32);
                var reversed = ConvertBoolArrayToByteArray(newBits).Reverse().ToArray();
                obj = BitConverter.ToUInt32(reversed);
            }
            else
            {
                var bytes = BitConverter.GetBytes(obj);
                AddToArray(bytes);
            }
        }

        /// <summary>
        /// serializes type uint16
        /// </summary>
        public void Serial(ref ushort obj)
        {
            if (Constants.BitMemoryStreamDebugEnabled) _debugData.Add(new BitMemoryStreamSerialInfo(_bitPos, 16, BitMemoryStreamSerialInfo.SerialType.UShort, new StackTrace(true)));

            if (IsReading())
            {
                var newBits = ReadFromArray(16);
                var reversed = ConvertBoolArrayToByteArray(newBits).Reverse().ToArray();
                obj = BitConverter.ToUInt16(reversed);
            }
            else
            {
                var bytes = BitConverter.GetBytes(obj);
                AddToArray(bytes);
            }
        }

        /// <summary>
        /// serializes type sint16 with a given bit length
        /// </summary>
        public void Serial(ref short obj, int nbits = 16)
        {
            if (Constants.BitMemoryStreamDebugEnabled) _debugData.Add(new BitMemoryStreamSerialInfo(_bitPos, nbits, BitMemoryStreamSerialInfo.SerialType.Short, new StackTrace(true)));

            if (IsReading())
            {
                var bits = ReadFromArray(nbits);

                var newBits = new bool[16];
                Array.Copy(bits, 0, newBits, 16 - bits.Length, bits.Length);

                var bytes = ConvertBoolArrayToByteArray(newBits);
                var reversed = bytes.Reverse().ToArray();
                obj = BitConverter.ToInt16(reversed);
            }
            else
            {
                var bytes = BitConverter.GetBytes(obj);
                AddToArray(bytes);
            }
        }

        /// <summary>
        /// serializes type uint32 with a given bit length
        /// </summary>
        public void Serial(ref uint obj, int nbits)
        {
            if (Constants.BitMemoryStreamDebugEnabled) _debugData.Add(new BitMemoryStreamSerialInfo(_bitPos, nbits, BitMemoryStreamSerialInfo.SerialType.UInt, new StackTrace(true)));

            if (IsReading())
            {
                var bits = ReadFromArray(nbits);

                var newBits = new bool[32];
                Array.Copy(bits, 0, newBits, 32 - bits.Length, bits.Length);

                var bytes = ConvertBoolArrayToByteArray(newBits);
                var reversed = bytes.Reverse().ToArray();
                obj = BitConverter.ToUInt32(reversed);
            }
            else
            {
                var bytes = BitConverter.GetBytes(obj);
                var bitArr = new bool[nbits];
                var bitArrayIndex = 0;

                foreach (var t in bytes)
                {
                    for (var bitIndex = 0; bitIndex < 8; bitIndex++)
                    {
                        bitArr[bitArrayIndex] = GetBit(t, bitIndex);
                        bitArrayIndex++;

                        if (bitArrayIndex >= nbits) break;
                    }

                    if (bitArrayIndex >= nbits) break;
                }

                bitArr = bitArr.Reverse().ToArray();

                foreach (var bit in bitArr)
                {
                    if (_bitPos >= _contentBits.Length)
                    {
                        Array.Resize(ref _contentBits, _contentBits.Length + 8);
                    }

                    _contentBits[_bitPos] = bit;
                    _bitPos++;
                }
            }
        }

        /// <summary>
        /// serializes type uint64 with a given bit length
        /// </summary>
        public void Serial(ref ulong obj, int nbits)
        {
            if (Constants.BitMemoryStreamDebugEnabled) _debugData.Add(new BitMemoryStreamSerialInfo(_bitPos, nbits, BitMemoryStreamSerialInfo.SerialType.ULong, new StackTrace(true)));

            if (IsReading())
            {
                var bits = ReadFromArray(nbits);

                var newBits = new bool[64];
                Array.Copy(bits, 0, newBits, 64 - bits.Length, bits.Length);

                var bytes = ConvertBoolArrayToByteArray(newBits);
                var reversed = bytes.Reverse().ToArray();
                obj = BitConverter.ToUInt64(reversed);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// serializes type sint64
        /// </summary>
        public void Serial(ref long obj)
        {
            if (Constants.BitMemoryStreamDebugEnabled) _debugData.Add(new BitMemoryStreamSerialInfo(_bitPos, 64, BitMemoryStreamSerialInfo.SerialType.Long, new StackTrace(true)));

            if (IsReading())
            {
                var newBits = ReadFromArray(64);
                byte[] reversed = ConvertBoolArrayToByteArray(newBits).Reverse().ToArray();
                obj = BitConverter.ToInt64(reversed);
            }
            else
            {
                var bytes = BitConverter.GetBytes(obj);
                AddToArray(bytes);
            }
        }

        /// <summary>
        /// serializes type byte[]
        /// </summary>
        public void Serial(ref byte[] obj)
        {
            if (Constants.BitMemoryStreamDebugEnabled) _debugData.Add(new BitMemoryStreamSerialInfo(_bitPos, obj.Length * 8, BitMemoryStreamSerialInfo.SerialType.ByteArray, new StackTrace(true)));

            if (IsReading())
            {
                var newBits = ReadFromArray(obj.Length * 8);

                //obj = ConvertBoolArrayToByteArray(newBits);
                obj = ConvertBoolArrayToByteArray(newBits); //.Reverse().ToArray();
                //obj = BitConverter.ToInt64(reversed);
            }
            else
            {
                //var bytes = BitConverter.GetBytes(obj);
                AddToArray(obj.Reverse().ToArray());
            }
        }

        /// <summary>
        /// serializes type bit
        /// </summary>
        public void Serial(ref bool obj)
        {
            if (Constants.BitMemoryStreamDebugEnabled) _debugData.Add(new BitMemoryStreamSerialInfo(_bitPos, 1, BitMemoryStreamSerialInfo.SerialType.Bool, new StackTrace(true)));

            if (IsReading())
            {
                // direct read
                obj = _contentBits[_bitPos];
                _bitPos++;
            }
            else
            {
                if (_bitPos >= _contentBits.Length)
                    Array.Resize(ref _contentBits, _contentBits.Length + 8); // we need a whole byte

                // direct write
                _contentBits[_bitPos] = obj;
                _bitPos++;
            }
        }

        /// <summary>
        /// serializes type string (see ucstring)
        /// </summary>
        public void Serial(ref string obj, bool isUtf16 = true)
        {
            if (IsReading())
            {
                var len = 0;
                Serial(ref len);

                if (Constants.BitMemoryStreamDebugEnabled) _debugData.Add(new BitMemoryStreamSerialInfo(_bitPos, len * (isUtf16 ? 2 : 1) * 8, BitMemoryStreamSerialInfo.SerialType.String, new StackTrace(true)));

                var b = new byte[len * (isUtf16 ? 2 : 1)];

                // Read the string.
                for (uint i = 0; i != len * (isUtf16 ? 2 : 1); ++i)
                    Serial(ref b[i]);

                obj = isUtf16 ? Encoding.BigEndianUnicode.GetString(b) : Encoding.UTF8.GetString(b);
            }
            else
            {
                obj = new string(obj.Reverse().ToArray());
                var str8 = new byte[obj.Length * (isUtf16 ? 2 : 1)];
                var index2 = 0;

                var len = obj.Length;
                Serial(ref len);

                if (Constants.BitMemoryStreamDebugEnabled) _debugData.Add(new BitMemoryStreamSerialInfo(_bitPos, len * (isUtf16 ? 2 : 1) * 8, BitMemoryStreamSerialInfo.SerialType.BoolArray, new StackTrace(true)));

                for (var i = 0; i < obj.ToCharArray().Length; ++i)
                {
                    var c = obj.ToCharArray()[i];
                    var bytes = BitConverter.GetBytes(c); // TODO use right encoding

                    str8[index2] = bytes[0];
                    index2++;

                    if (!isUtf16) continue;

                    str8[index2] = 0;
                    index2++;
                }

                AddToArray(str8);
            }
        }

        /// <summary>
        /// Serializes a bitArray
        /// </summary>
        public void Serial(ref bool[] obj)
        {
            if (IsReading())
            {
                var len = obj.Length;
                List<byte> bytefield = new List<byte>();

                if (Constants.BitMemoryStreamDebugEnabled) _debugData.Add(new BitMemoryStreamSerialInfo(_bitPos, len, BitMemoryStreamSerialInfo.SerialType.BoolArray, new StackTrace(true)));

                uint v = 0;
                uint i = 0;

                while (len > 32)
                {
                    Serial(ref v);

                    bytefield.AddRange(BitConverter.GetBytes(v));

                    len -= 32;
                    ++i;
                }

                Serial(ref v, len);
                bytefield.AddRange(BitConverter.GetBytes(v));

                bool[] converted = ConvertByteArrayToBoolArray(bytefield.ToArray()).Reverse().ToArray();

                Array.Copy(converted, obj, obj.Length);
            }
            else
            {
                if (Constants.BitMemoryStreamDebugEnabled) _debugData.Add(new BitMemoryStreamSerialInfo(_bitPos, obj.Length, BitMemoryStreamSerialInfo.SerialType.BoolArray, new StackTrace(true)));

                foreach (var ack in obj)
                {
                    var b = ack;
                    Serial(ref b);
                }
            }
        }

        /// <summary>
        /// serializes another stream with a given length
        /// </summary>
        public void SerialBuffer(BitMemoryStream buf, in int len)
        {
            uint i;
            byte v = 0x00;

            if (Constants.BitMemoryStreamDebugEnabled) _debugData.Add(new BitMemoryStreamSerialInfo(_bitPos, len * 8, BitMemoryStreamSerialInfo.SerialType.Buffer, new StackTrace(true)));

            if (IsReading())
            {
                for (i = 0; i != len; ++i)
                {
                    Serial(ref v);

                    buf.AddToArray(new[] { v });
                }
            }
            else
            {
                for (i = 0; i != len; ++i)
                {
                    v = buf.Buffer()[i];
                    Serial(ref v);
                }
            }
        }

        /// <summary>
        /// stream version - serializes the current steam version info (which seems to be always 0)
        /// </summary>
        public uint SerialVersion(uint currentVersion)
        {
            byte b = 0;
            uint v = 0;
            uint streamVersion;

            // Open the node
            //xmlPush("VERSION");

            if (IsReading())
            {
                Serial(ref b);

                if (b == 0xFF)
                    Serial(ref v);
                else
                    v = b;
                streamVersion = v;

                // Exception test.
                //if (_ThrowOnOlder && streamVersion < currentVersion)
                //	throw EOlderStream(*this);
                //if (_ThrowOnNewer && streamVersion > currentVersion)
                //	throw ENewerStream(*this);
            }
            else
            {
                v = streamVersion = currentVersion;

                if (v >= 0xFF)
                {
                    b = 0xFF;
                    Serial(ref b);
                    Serial(ref v);
                }
                else
                {
                    b = (byte)v;
                    Serial(ref b);
                }
            }

            // Close the node
            //xmlPop();

            return streamVersion;
        }

        /// <summary>
        /// double serialisation: serializes type byte[] and serializes length parameter
        /// </summary>
        public void SerialBufferWithSize(byte[] buf, int len)
        {
            Serial(ref len);
            Serial(ref buf);
        }

        /// <summary>
        /// reads a single bit at a given position from a byte
        /// </summary>
        public static bool GetBit(byte b, int bitNumber)
        {
            return (b & (1 << bitNumber)) != 0;
        }

        /// <summary>
        /// HELPER since this is not c++, memcopy (overwrite) byte[] data to the bit[] array of the stream
        /// </summary>
        public void MemCpy(byte[] data)
        {
            if (data.Length * 8 > _contentBits.Length)
                _contentBits = new bool[data.Length * 8];

            AddToArray(data);

            _bitPos = 0;
        }

        /// <summary>
        /// add the bytes to the end of the stream
        /// </summary>
        private void AddToArray(byte[] bytes)
        {
            var newBits = new BitArray(bytes);

            for (var index = 0; index < newBits.Count; index++)
            {
                if (_bitPos >= _contentBits.Length)
                {
                    Array.Resize(ref _contentBits, _contentBits.Length + 8);
                }

                _contentBits[_bitPos] = newBits[newBits.Count - index - 1];
                _bitPos++;
            }
        }

        /// <summary>
        /// reads (length) bit from the stream incrementing the reader pos
        /// </summary>
        private bool[] ReadFromArray(int length)
        {
            bool[] newBits = new bool[length];

            for (var index = 0; index < length; index++)
            {
                newBits[index] = _contentBits[_bitPos];
                _bitPos++;
            }

            return newBits;
        }

        /// <summary>
        /// Returns the complete stream as a byte array
        /// </summary>
        public byte[] Buffer()
        {
            return ConvertBoolArrayToByteArray(_contentBits);
        }

        /// <summary>
        /// Packs a bit array into bytes
        /// </summary>
        public static byte[] ConvertBoolArrayToByteArray(bool[] boolArr)
        {
            var byteArr = new byte[(boolArr.Length - 1) / 8 + 1];

            for (var i = 0; i < byteArr.Length; i++)
            {
                byteArr[i] = ReadByte(boolArr, 8 * i);
            }

            return byteArr;
        }

        /// <summary>
        /// Convert Byte Array To Bool Array
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static bool[] ConvertByteArrayToBoolArray(byte[] bytes)
        {
            BitArray b = new BitArray(bytes);
            bool[] bitValues = new bool[b.Count];
            b.CopyTo(bitValues, 0);
            Array.Reverse(bitValues);
            return bitValues;
        }

        /// <summary>
        /// read a byte at a given offset from the bit (8x) array
        /// </summary>
        private static byte ReadByte(bool[] boolArr, int offset)
        {
            byte result = 0;

            for (var i = 0; i < 8; i++)
            {
                // if the element is 'true' set the bit at that position
                if (boolArr[offset + i])
                    result |= (byte)(1 << (7 - i));
            }

            return result;
        }


        /// <summary>
        /// Transforms the message from input to output or from output to input
        /// </summary>
        public void Invert()
        {
            _inputStream = !_inputStream;

            if (IsReading())
            {
                // Write->Read, the position is set at the beginning of the stream
                _bitPos = 0;
            }
            else
            {
                // read->write: set the position on the last byte, not at the end as in CMemStream::invert()
                _bitPos = _contentBits.Length - 1;
            }

            // Keep the same _FreeBits
        }

        /// <summary>
        /// Set the position at the beginning. In output mode, the method ensures the buffer
        /// contains at least one blank byte to write to.
        /// If you are using the stream only in output mode, you can use this method as a faster version
        /// of clear() *if you don't serialize pointers*.
        /// </summary>
        public void ResetBufPos()
        {
            _bitPos = 0;
        }

        /// <summary>
        /// ToString override for bitwise output
        /// </summary>
        public override string ToString()
        {
            return ToString(false);
        }

        /// <summary>
        /// byte or bitwise output of the stream
        /// </summary>
        public string ToString(bool displayBytes)
        {
            var ret = "CBMemStream " + (IsReading() ? "in" : "out") + "\r\n";

            if (displayBytes)
            {
                var bs = Buffer();
                for (var index = 0; index < bs.Length; index++)
                {
                    var b = bs[index];
                    ret += Convert.ToString(b, 2).PadLeft(8, '0') + " ";
                    if (index % 8 == 7) ret += "\r\n";
                }
            }
            else
            {
                for (var index = 0; index < _contentBits.Length; index++)
                {
                    if (_bitPos == index) ret += "<P>";

                    ret += _contentBits[index] ? "1" : "0";

                    if (index % (8 * 8) == 8 * 8 - 1) ret += "\r\n";
                    else if (index % 8 == 8 - 1) ret += " ";
                }
            }

            return ret;
        }

        /// <summary>
        /// Display the bits of the stream just before the current pos
        /// </summary>
        public string DisplayLastBits(int nbits)
        {
            var bitpos = GetPosInBit();
            return DisplayLastBits(Math.Max(bitpos - nbits, 0), bitpos - 1);
        }

        /// <summary>
        /// Display a part of a bitmemstream
        /// </summary>
        public string DisplayLastBits(int beginbitpos, int endbitpos)
        {
            int beginpos = beginbitpos / 8;
            int endpos = endbitpos / 8;

            string ret = $"BMS: beginpos {beginpos} endpos {endpos} beginbitpos {beginbitpos} endbitpos {endbitpos}\r\n";

            ret += DisplayByteBits(Buffer()[beginpos], 8, 8 - (beginbitpos - beginpos * 8), true) + "\r\n";

            int p;

            for (p = beginpos + 1; p < endpos - 1; ++p)
            {
                ret += DisplayByteBits(Buffer()[p], 8, 0, false) + "\r\n";
            }

            if (endpos > beginpos)
            {
                ret += DisplayByteBits(Buffer()[endpos], 8, 0, false);
            }

            return ret;
        }

        /// <summary>
        ///  Display the bits (with 0 and 1) composing a byte (from right to left)
        /// </summary>
        string DisplayByteBits(byte b, int nbits, int beginpos, bool displayBegin)
        {
            string ret = "";

            string s1 = "", s2 = "\r\n";
            int i;

            for (i = nbits - 1; i != -1; --i)
            {
                s1 += ((b >> i) & 1) == 1 ? "1" : "0";
            }

            ret += s1;

            if (displayBegin)
            {
                for (i = nbits; i > beginpos + 1; --i)
                {
                    s2 += " ";
                }
                s2 += $"^ beginpos={beginpos}";
                ret += s2;
            }

            return ret;
        }
    }
}