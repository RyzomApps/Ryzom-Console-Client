using System;
using System.IO;
using System.Security.Cryptography;

namespace RCC.Helper
{
    public static class Misc
    {
        public static string ByteArrToString(byte[] arr)
        {
            return BitConverter.ToString(arr).Replace("-", "").ToLowerInvariant();
        }

        public static byte[] GetMD5(string filename)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filename);
            var hash = md5.ComputeHash(stream);
            return hash;
        }

        public static unsafe float Int32BitsToSingle(int value)
        {
            return *(float*) &value;
        }

        /// <summary>
        ///     Return the power of 2 of v.
        /// </summary>
        /// <example>
        ///     getPowerOf2(8) is 3
        ///     getPowerOf2(5) is 3
        /// </example>
        public static uint GetPowerOf2(uint v)
        {
            uint res = 1;
            uint ret = 0;

            while (res < v)
            {
                ret++;
                res <<= 1;
            }

            return ret;
        }
    }
}