using System;
using System.Collections;
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
            Serial(ref currentSendNumber);
            bool systemmode = true;
            Serial(ref systemmode); // systemmode
            ++currentSendNumber;
        }

        public void Serial(ref byte obj)
        {
            // TODO use isReading

            byte[] bytes = { obj };
            AddToArray(bytes);
        }

        public void Serial(ref int obj)
        {
            // TODO use isReading

            var bytes = BitConverter.GetBytes(obj);
            AddToArray(bytes);
        }

        public void Serial(ref bool obj)
        {
            // TODO use isReading

            // direct write
            _contentBits[_bitPos] = obj;
            _bitPos++;
        }

        public void Serial(ref string obj)
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
                _contentBits[_bitPos] = newBits[newBits.Count - index - 1];
                _bitPos++;
            }
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
            string ret = "CBMS:";

            foreach (var b in Buffer())
            {
                //ret += Convert.ToString(b, 2).PadLeft(8, '0') + " ";
                ret += Convert.ToString(b, 16).PadLeft(2, '0') + "";
            }

            return ret;
        }
    }
}