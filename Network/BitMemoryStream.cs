///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace RCC.Network
{
    /// <summary>
    ///     (de)serializes data for server communication
    /// </summary>
    public class BitMemoryStream
    {
        private int _bitPos;
        private bool[] _contentBits;
        private bool _inputStream;

        /// <summary>
        ///     constructor differentiating between input and output stream and specifying the size of the stream
        /// </summary>
        public BitMemoryStream(bool inputStream = false, int defaultcapacity = 32)
        {
            _inputStream = inputStream;
            _contentBits = new bool[defaultcapacity * 8];
        }

        /// <summary>
        ///     Byte Position of the reader/writer in the Stream
        /// </summary>
        public int Pos => (int) (_bitPos / 8d);

        /// <summary>
        ///     Free bits in the current byte of the reader/writer
        /// </summary>
        public int FreeBits => 8 - _bitPos % 8;

        /// <summary>
        ///     Length in bytes of the overall stream (reader) or length of the written bytes (writer)
        /// </summary>
        public int Length
        {
            get
            {
                if (IsReading())
                    return (int) (_contentBits.Length / 8d); // (_contentBits.Length - 1) / 8 + 1;

                return (int) ((_bitPos - 1) / 8d) + 1;
            }
        }

        /// <summary>
        ///     input or output
        /// </summary>
        public bool IsReading() => _inputStream;

        /// <summary>
        ///     Returns the number of bit from the beginning of the buffer (in bit)
        /// </summary>
        public int GetPosInBit()
        {
            // return (_BufPos - _Buffer.getPtr() + 1)*8 - _FreeBits;
            return (Pos + 1) * 8 - FreeBits;
        }

        /// <summary>
        ///     Builds the header that is needed for the communication with the ryzom server
        ///     TODO: this shouldn't be here, but it is - move to a better position
        /// </summary>
        public void BuildSystemHeader(ref int currentSendNumber)
        {
            Serial(ref currentSendNumber);
            bool systemmode = true;
            Serial(ref systemmode); // systemmode
            ++currentSendNumber;
        }

        /// <summary>
        ///     serializes type byte
        /// </summary>
        public void Serial(ref byte obj)
        {
            if (IsReading())
            {
                var newBits = ReadFromArray(8);
                obj = ConvertBoolArrayToByteArray(newBits)[0];
            }
            else
            {
                byte[] bytes = {obj};
                AddToArray(bytes);
            }
        }

        /// <summary>
        ///     serializes type sint32
        /// </summary>
        public void Serial(ref int obj)
        {
            if (IsReading())
            {
                var newBits = ReadFromArray(32);
                byte[] reversed = ConvertBoolArrayToByteArray(newBits).Reverse().ToArray();
                obj = BitConverter.ToInt32(reversed);
            }
            else
            {
                var bytes = BitConverter.GetBytes(obj);
                AddToArray(bytes);
            }
        }

        /// <summary>
        ///     serializes type uint32
        /// </summary>
        public void Serial(ref uint obj)
        {
            if (IsReading())
            {
                var newBits = ReadFromArray(32);
                byte[] reversed = ConvertBoolArrayToByteArray(newBits).Reverse().ToArray();
                obj = BitConverter.ToUInt32(reversed);
            }
            else
            {
                var bytes = BitConverter.GetBytes(obj);
                AddToArray(bytes);
            }
        }

        /// <summary>
        ///     serializes type sint16 with a given bit length
        /// </summary>
        public void Serial(ref short obj, int nbits = 16)
        {
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
                throw new NotImplementedException();
                //var bytes = BitConverter.GetBytes(obj);
                //AddToArray(bytes);
            }
        }

        /// <summary>
        ///     serializes type uint32 with a given bit length
        /// </summary>
        public void Serial(ref uint obj, int nbits = 32)
        {
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
                throw new NotImplementedException();
                //var bytes = BitConverter.GetBytes(obj);
                //AddToArray(bytes);
            }
        }

        /// <summary>
        ///     serializes type sint64
        /// </summary>
        public void Serial(ref long obj)
        {
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
        ///     serializes type byte[]
        /// </summary>
        public void Serial(ref byte[] obj)
        {
            if (IsReading())
            {
                var newBits = ReadFromArray(obj.Length * 8);
                //obj = ConvertBoolArrayToByteArray(newBits);
                obj = ConvertBoolArrayToByteArray(newBits); //.Reverse().ToArray();
                //obj = BitConverter.ToInt64(reversed);
            }
            else
            {
                //throw new NotImplementedException();
                //var bytes = BitConverter.GetBytes(obj);
                AddToArray(obj.Reverse().ToArray());
            }
        }

        /// <summary>
        ///     serializes type bit
        /// </summary>
        public void Serial(ref bool obj)
        {
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
        ///     serializes type string (see ucstring)
        /// </summary>
        public void Serial(ref string obj, bool doubled = true)
        {
            if (IsReading())
            {
                int len = 0;
                Serial(ref len);
                byte[] b = new byte[len * (doubled ? 2 : 1)];

                // Read the string.
                for (uint i = 0; i != len * (doubled ? 2 : 1); ++i)
                    Serial(ref b[i]);

                obj = Encoding.UTF8.GetString(b).Replace("\0", "");
            }
            else
            {
                obj = new string(obj.Reverse().ToArray());
                var str8 = new byte[obj.Length * (doubled ? 2 : 1)];
                int index2 = 0;

                int len = obj.Length;
                Serial(ref len);

                for (var index = 0; index < obj.ToCharArray().Length; index++)
                {
                    var c = obj.ToCharArray()[index];
                    var bytes = BitConverter.GetBytes(c);

                    str8[index2] = bytes[0];
                    index2++;

                    if (doubled)
                    {
                        str8[index2] = 0;
                        index2++;
                    }
                }

                AddToArray(str8);
            }
        }

        /// <summary>
        ///     serializes type uint32 with a given bit length (+debug)
        /// </summary>
        internal void SerialAndLog2(ref uint obj, uint nbBits)
        {
            if (IsReading())
            {
                //short value = -1;
                Serial(ref obj, (int) nbBits);
                //obj = value;
                return;
            }

            var bytes = BitConverter.GetBytes(obj);
            var bitArr = new bool[nbBits];
            var bitArrayIndex = 0;

            foreach (var t in bytes)
            {
                for (var bitIndex = 0; bitIndex < 8; bitIndex++)
                {
                    bitArr[bitArrayIndex] = GetBit(t, bitIndex);
                    bitArrayIndex++;

                    if (bitArrayIndex >= nbBits) break;
                }

                if (bitArrayIndex >= nbBits) break;
            }

            bitArr = bitArr.Reverse().ToArray();

            foreach (var bit in bitArr)
            {
                if (_bitPos >= _contentBits.Length)
                {
                    Array.Resize<bool>(ref _contentBits, _contentBits.Length + 8);
                }

                _contentBits[_bitPos] = bit;
                _bitPos++;
            }
        }

        /// <summary>
        ///     reads a single bit at a given position from a byte
        /// </summary>
        public static bool GetBit(byte b, int bitNumber)
        {
            return (b & (1 << bitNumber)) != 0;
        }

        /// <summary>
        ///     HELPER since this is not c++, memcopy (overwrite) byte[] data to the bit[] array of the stream
        /// </summary>
        public void MemCpy(byte[] data)
        {
            if (data.Length * 8 > _contentBits.Length)
                _contentBits = new bool[data.Length * 8];

            AddToArray(data);

            _bitPos = 0;
        }

        /// <summary>
        ///     add the bytes to the end of the stream
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
        ///     reads (length) bit from the stream incrementing the reader pos
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
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
        ///     Returns the complete stream as a byte array
        /// </summary>
        /// <returns></returns>
        public byte[] Buffer()
        {
            return ConvertBoolArrayToByteArray(_contentBits);
        }

        /// <summary>
        ///     Packs a bit array into bytes
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
        ///     read a byte at a given offset from the bit (8x) array
        /// </summary>
        private static byte ReadByte(bool[] boolArr, int offset)
        {
            byte result = 0;

            for (var i = 0; i < 8; i++)
            {
                // if the element is 'true' set the bit at that position
                if (boolArr[offset + i])
                    result |= (byte) (1 << (7 - i));
            }

            return result;
        }


        /// <summary>
        ///     Transforms the message from input to output or from output to input
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
        ///     Set the position at the beginning. In output mode, the method ensures the buffer
        ///     contains at least one blank byte to write to.
        ///     If you are using the stream only in output mode, you can use this method as a faster version
        ///     of clear() *if you don't serialize pointers*.
        /// </summary>
        public void ResetBufPos()
        {
            _bitPos = 0;
        }

        /// <summary>
        ///     serializes another stream with a given length
        /// </summary>
        public void SerialBuffer(BitMemoryStream buf, in int len)
        {
            uint i;
            byte v = 0x00;

            if (IsReading())
            {
                for (i = 0; i != len; ++i)
                {
                    Serial(ref v);
                    buf.AddToArray(new[] {v});
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
        ///     TODO stream version - serializes the current steam version info (which seems to be always 0)
        /// </summary>
        public int SerialVersion(/*uint currentVersion*/)
        {
            byte b = 0;
            int v = 0;
            int streamVersion;

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
                throw new NotImplementedException();
                //v = streamVersion = currentVersion;
                //if (v >= 0xFF)
                //{
                //	b = 0xFF;
                //	serial(b);
                //	serial(v);
                //}
                //else
                //{
                //	b = (uint8)v;
                //	serial(b);
                //}
            }

            // Close the node
            //xmlPop();

            return streamVersion;
        }

        /// <summary>
        ///     double serialisation: serializes type byte[] and serializes length parameter
        /// </summary>
        public void SerialBufferWithSize(byte[] buf, int len)
        {
            Serial(ref len);
            Serial(ref buf);
        }

        /// <summary>
        ///     ToString override for bitwise output
        /// </summary>
        public override string ToString()
        {
            return ToString(false);
        }

        /// <summary>
        ///     byte or bitwise output of the stream
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
    }
}