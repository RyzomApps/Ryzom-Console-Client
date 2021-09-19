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

        public static ConsoleColor FromColor(System.Drawing.Color c)
        {
            int index = (c.R > 128 | c.G > 128 | c.B > 128) ? 8 : 0; // Bright bit
            index |= (c.R > 64) ? 4 : 0; // Red bit
            index |= (c.G > 64) ? 2 : 0; // Green bit
            index |= (c.B > 64) ? 1 : 0; // Blue bit
            return (ConsoleColor)index;
        }
    }
}