using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;

namespace RCC
{
    public class CBitMemStream
    {
        private readonly bool _inputStream;
        private bool[] _contentBits;
        private int _bitPos;

        private bool isReading() => _inputStream;

        public int Pos => (int)(_bitPos / 8d);

        public int FreeBits => _contentBits.Length - _bitPos;

        public int Length => (int)((_bitPos - 1) / 8d) + 1;

        public CBitMemStream(bool inputStream = false, int defaultcapacity = 32)
        {
            _inputStream = inputStream;
            _contentBits = new bool[defaultcapacity * 8];
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
                obj = ReadFromArray(8)[0];
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
                obj = BitConverter.ToInt32(ReadFromArray(32));
            }
            else
            {
                var bytes = BitConverter.GetBytes(obj);
                AddToArray(bytes);
            }
        }

        public void serial(ref short obj)
        {
            if (isReading())
            {
                obj = BitConverter.ToInt16(ReadFromArray(16));
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
                obj = BitConverter.ToInt64(ReadFromArray(64));
            }
            else
            {
                var bytes = BitConverter.GetBytes(obj);
                AddToArray(bytes);
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
            // TODO use isReading

            var str8 = new byte[obj.Length];

            for (var index = 0; index < obj.ToCharArray().Length; index++)
            {
                var c = obj.ToCharArray()[index];
                var bytes = BitConverter.GetBytes(c);
                str8[index] = bytes[0];
            }

            AddToArray(str8);
        }

        /// <summary>
        /// HELPER
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

        private byte[] ReadFromArray(int length)
        {
            bool[] newBits = new bool[length];

            for (var index = 0; index < length; index++)
            {
                newBits[index] = _contentBits[_bitPos];
                _bitPos++;
            }

            //Debug.WriteLine(ToString());

            return ConvertBoolArrayToByteArray(newBits);
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
    }
}