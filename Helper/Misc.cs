using System;
using System.IO;
using System.Security.Cryptography;

namespace RCC.Helper
{
    public static class Misc
    {
        public static string byteArrToString(byte[] arr)
        {
            return BitConverter.ToString(arr).Replace("-", "").ToLowerInvariant();
        }
        public static byte[] getMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return hash;
                }
            }
        }
    }
}
