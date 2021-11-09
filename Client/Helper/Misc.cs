using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Client.Chat;

namespace Client.Helper
{
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
        /// Get the console color from a color specified
        /// </summary>
        public static ConsoleColor FromColor(Color c)
        {
            var index = (c.R > 128) | (c.G > 128) | (c.B > 128) ? 8 : 0; // Bright bit
            index |= c.R > 64 ? 4 : 0; // Red bit
            index |= c.G > 64 ? 2 : 0; // Green bit
            index |= c.B > 64 ? 1 : 0; // Blue bit
            return (ConsoleColor)index;
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
        /// returns the console color code for a minecraft channel type
        /// </summary>
        public static string GetMinecraftColorForChatGroupType(ChatGroupType mode)
        {
            var color = mode switch
            {
                ChatGroupType.DynChat => "§b",
                ChatGroupType.Shout => "§c",
                ChatGroupType.Team => "§9",
                ChatGroupType.Guild => "§a",
                ChatGroupType.Civilization => "§d",
                ChatGroupType.Territory => "§d",
                ChatGroupType.Universe => "§6",
                ChatGroupType.Region => "§7",
                ChatGroupType.Tell => "§f",
                _ => "§f"
            };

            return color;
        }

        /// <summary>
        /// extract a file from an embedded resource and save it to disk
        /// </summary>
        public static void WriteResourceToFile(string resourceName, string fileName)
        {
            var text = (string)Resources.ResourceManager.GetObject(resourceName);
            File.WriteAllText(fileName, text);
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

        public static void Resize<T>(this List<T> list, int sz, T c)
        {
            int cur = list.Count;
            if (sz < cur)
                list.RemoveRange(sz, cur - sz);
            else if (sz > cur)
            {
                if (sz > list.Capacity) //this bit is purely an optimisation, to avoid multiple automatic capacity changes.
                    list.Capacity = sz;
                list.AddRange(Enumerable.Repeat(c, sz - cur));
            }
        }

        public static void Resize<T>(this List<T> list, int sz) where T : new()
        {
            Resize(list, sz, new T());
        }

        public static string ToHumanReadableString(this TimeSpan t)
        {
            if (t.TotalSeconds <= 1)
            {
                return $@"{t:s\.ff}s";
            }
            if (t.TotalMinutes <= 1)
            {
                return $@"{t:%s}s";
            }
            if (t.TotalHours <= 1)
            {
                return $@"{t:%m}m";
            }
            if (t.TotalDays <= 1)
            {
                return $@"{t:%h}h";
            }

            return $@"{t:%d}d";
        }
    }
}