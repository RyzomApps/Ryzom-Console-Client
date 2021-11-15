using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace API.Helper
{
    /// <summary>
    /// selection of helper functions that do not fit into any other class
    /// </summary>
    public static class Misc
    {
        /// <summary>
        /// Conerts a byte array to a hexdecimal string
        /// </summary>
        public static string ByteArrToString(byte[] arr)
        {
            return BitConverter.ToString(arr).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// Get MD5 hash of a file
        /// </summary>
        public static byte[] GetFileMD5(string filename)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filename);
            var hash = md5.ComputeHash(stream);
            return hash;
        }

        /// <summary>
        /// usafe conversion from sint32 to single (float) value
        /// </summary>
        public static unsafe float Int32BitsToSingle(int value)
        {
            return *(float*)&value;
        }

        /// <summary>
        /// Return the power of 2 of v.
        /// </summary>
        /// <example>
        /// getPowerOf2(8) is 3
        /// getPowerOf2(5) is 3
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

        /// <summary>
        /// Use input string to calculate MD5 hash
        /// </summary>
        public static string GetMD5(string input)
        {
            using var md5 = MD5.Create();

            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            var sb = new StringBuilder();

            foreach (var t in hashBytes)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Return the local time in milliseconds.
        /// Use it only to measure time difference, the absolute value does not mean anything.
        /// On Unix, getLocalTime() will try to use the Monotonic Clock if available, otherwise
        /// the value can jump backwards if the system time is changed by a user or a NTP time sync process.
        /// The value is different on 2 different computers; use the CUniTime class to get a universal
        /// time that is the same on all computers.
        /// </summary>
        /// <remarks>On Win32, the value is on 32 bits only. It wraps around to 0 every about 49.71 days</remarks>
        public static long GetLocalTime()
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Get a Y-M-D h:m:s timestamp representing the current system date and time
        /// </summary>
        public static string GetTimestamp()
        {
            var time = DateTime.Now;
            return $"{time.Year:0000}-{time.Month:00}-{time.Day:00} {time.Hour:00}:{time.Minute:00}:{time.Second:00}";
        }

        /// <summary>
        /// Resizes a list filling it with a given type
        /// </summary>
        public static void Resize<T>(this List<T> list, int sz, T c)
        {
            var cur = list.Count;
            if (sz < cur)
                list.RemoveRange(sz, cur - sz);
            else if (sz > cur)
            {
                if (sz > list.Capacity) //this bit is purely an optimisation, to avoid multiple automatic capacity changes.
                    list.Capacity = sz;
                list.AddRange(Enumerable.Repeat(c, sz - cur));
            }
        }

        /// <summary>
        /// Resizes a list filling it with a default type
        /// </summary>
        public static void Resize<T>(this List<T> list, int sz) where T : new()
        {
            Resize(list, sz, new T());
        }

        /// <summary>
        /// Convert a timespan to a human readable string (only one unit long)
        /// </summary>
        public static string ToHumanReadableString(this TimeSpan t)
        {
            if (t.TotalSeconds <= 1)
                return $@"{t:s\.ff}s";
            if (t.TotalMinutes <= 1)
                return $@"{t:%s}s";
            if (t.TotalHours <= 1)
                return $@"{t:%m}m";
            if (t.TotalDays <= 1)
                return $@"{t:%h}h";
            return $@"{t:%d}d";
        }

        /// <summary>
        /// Iterate all types within the specified assembly.<br/>
        /// Check whether that's the shortest so far.<br/>
        /// If it's, set it to the ns.
        /// </summary>
        /// <param name="asm">Assembly to check</param>
        /// <returns>Return the shortest namespace of the assembly</returns>
        public static string GetAssemblyNamespace(Assembly asm)
        {
            var ns = "";

            foreach (var tp in asm.Modules.First().GetTypes())
                if (tp.Namespace != null && (ns.Length == 0 || tp.Namespace.Length < ns.Length))
                    ns = tp.Namespace;

            return ns;
        }
    }
}