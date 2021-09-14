using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;

namespace RCC.Network
{
    public class CBitMemStream
    {
        private bool _inputStream;
        private bool[] _contentBits;
        private int _bitPos;

        public bool isReading() => _inputStream;

        public int Pos => (int)(_bitPos / 8d);

        public int FreeBits => /*(_contentBits.Length -)*/ 8 - (_bitPos % 8);

        public int Length => (int)((_bitPos - 1) / 8d) + 1;

        public CBitMemStream(bool inputStream = false, int defaultcapacity = 32)
        {
            _inputStream = inputStream;
            _contentBits = new bool[defaultcapacity * 8];
        }

        /// <summary>
        /// Returns the number of bit from the beginning of the buffer (in bit)
        /// </summary>
        public int getPosInBit()
        {
            // return (_BufPos - _Buffer.getPtr() + 1)*8 - _FreeBits;
            return (Pos + 1) * 8 - FreeBits;
        }

        // this shouldn't be here, but it is
        public void BuildSystemHeader(ref int currentSendNumber)
        {
            serial(ref currentSendNumber);
            bool systemmode = true;
            serial(ref systemmode); // systemmode
            ++currentSendNumber;
        }

        public void serial(ref byte obj)
        {
            if (isReading())
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

        public void serial(ref int obj)
        {
            if (isReading())
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

        public void serial(ref uint obj)
        {
            if (isReading())
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

        public void serial(ref short obj, int nbits = 16)
        {
            if (isReading())
            {
                var bits = ReadFromArray(nbits);

                bool[] newBits = new bool[16];
                Array.Copy(bits, 0, newBits, 16 - bits.Length, bits.Length);

                var bytes = ConvertBoolArrayToByteArray(newBits);
                byte[] reversed = bytes.Reverse().ToArray();
                obj = BitConverter.ToInt16(reversed);
            }
            else
            {
                var bytes = BitConverter.GetBytes(obj);
                AddToArray(bytes);
            }
        }

        public void serial(ref long obj)
        {
            if (isReading())
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

        public void serial(ref byte[] obj)
        {
            if (isReading())
            {
                var newBits = ReadFromArray(obj.Length * 8);
                //obj = ConvertBoolArrayToByteArray(newBits);
                obj = ConvertBoolArrayToByteArray(newBits).Reverse().ToArray();
                //obj = BitConverter.ToInt64(reversed);
            }
            else
            {
                //throw new NotImplementedException();
                //var bytes = BitConverter.GetBytes(obj);
                AddToArray(obj.Reverse().ToArray());
            }
        }

        public void serial(ref bool obj)
        {
            if (isReading())
            {
                // direct read
                obj = _contentBits[_bitPos];
                _bitPos++;
            }
            else
            {
                // direct write
                _contentBits[_bitPos] = obj;
                _bitPos++;
            }
        }

        public void serial(ref string obj)
        {
            if (isReading())
            {
                int len = 0;
                serial(ref len);
                byte[] b = new byte[len * 2];

                // Read the string.
                for (uint i = 0; i != len * 2; ++i)
                    serial(ref b[i]);

                obj = System.Text.Encoding.UTF8.GetString(b).Replace("\0", "");
            }
            else
            {
                var str8 = new byte[obj.Length];

                for (var index = 0; index < obj.ToCharArray().Length; index++)
                {
                    var c = obj.ToCharArray()[index];
                    var bytes = BitConverter.GetBytes(c);
                    str8[index] = bytes[0];
                }

                AddToArray(str8);
            }
        }

        /// <summary>
        /// HELPER since this is not c++
        /// </summary>
        public void memcpy(byte[] data)
        {
            if (data.Length * 8 > _contentBits.Length)
                _contentBits = new bool[data.Length * 8];

            AddToArray(data);

            _bitPos = 0;
        }

        private void AddToArray(byte[] bytes)
        {
            var newBits = new BitArray(bytes);

            for (var index = 0; index < newBits.Count; index++)
            {
                if (_bitPos >= _contentBits.Length)
                {
                    Array.Resize<bool>(ref _contentBits, _contentBits.Length + 8);
                }

                _contentBits[_bitPos] = newBits[newBits.Count - index - 1];
                _bitPos++;
            }
        }

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

        public byte[] Buffer()
        {
            return ConvertBoolArrayToByteArray(_contentBits);
        }

        /// Packs a bit array into bytes
        public static byte[] ConvertBoolArrayToByteArray(bool[] boolArr)
        {
            var byteArr = new byte[(boolArr.Length - 1) / 8 + 1];

            for (var i = 0; i < byteArr.Length; i++)
            {
                byteArr[i] = ReadByte(boolArr, 8 * i);
            }

            return byteArr;
        }

        private static byte ReadByte(bool[] boolArr, int index)
        {
            byte result = 0;

            for (var i = 0; i < 8; i++)
            {
                // if the element is 'true' set the bit at that position
                if (boolArr[index + i])
                    result |= (byte)(1 << 7 - i);
            }

            return result;
        }

        public override string ToString()
        {
            string ret = "CBMemStream " + (isReading() ? "in" : "out") + "\r\n";

            var bs = Buffer();
            for (var index = 0; index < bs.Length; index++)
            {
                var b = bs[index];
                ret += Convert.ToString(b, 2).PadLeft(8, '0') + " ";
                //ret += Convert.ToString(b, 16).PadLeft(2, '0') + "";
                if (index % 8 == 7) ret += "\r\n";
            }

            return ret;
        }

        internal void serialAndLog2(ref short index, uint nbBits)
        {
            //short value = -1;
            serial(ref index, (int)nbBits);
            //index = value;
        }

        /// <summary>
        /// Transforms the message from input to output or from output to input
        /// </summary>
        public void invert()
        {
            _inputStream = !_inputStream;

            if (isReading())
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
        /// 
        /// If you are using the stream only in output mode, you can use this method as a faster version
        /// of clear() *if you don't serialize pointers*.
        /// </summary>
        public void resetBufPos()
        {
            _bitPos = 0;
        }

        public void serialBuffer(CBitMemStream other, in int len)
        {
            uint i;
            byte v = 0x00;

            if (isReading())
            {
                //var tmp = new byte[len];

                //for (i = 0; i != len; ++i)
                //{
                //    serial(ref v);
                //    //other.memcpy(new byte[] {v});
                //    //other.Buffer()[i] = (byte)v;
                //
                //    tmp[i] = v;
                //}

                //var reversed = tmp.Reverse().ToArray();

                // TODO: FIX THIS!!!
                other._contentBits = (bool[])_contentBits.Clone();
                other._bitPos = _bitPos;

                //other._inputStream = this._inputStream;

                //other.AddToArray(tmp);
            }
            else
            {
                for (i = 0; i != len; ++i)
                {
                    v = other.Buffer()[i];
                    serial(ref v);
                }
            }
        }

        public int serialVersion(uint currentVersion)
        {
            byte b = 0;
            int v = 0;
            int streamVersion;

            // Open the node
            //xmlPush("VERSION");

            if (isReading())
            {
                serial(ref b);
                if (b == 0xFF)
                    serial(ref v);
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
    }
}