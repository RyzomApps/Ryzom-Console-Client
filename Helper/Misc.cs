using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using RCC.Chat;

namespace RCC.Helper
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

        /// <summary>
        /// Get the console color from a color specified
        /// </summary>
        public static ConsoleColor FromColor(Color c)
        {
            var index = (c.R > 128) | (c.G > 128) | (c.B > 128) ? 8 : 0; // Bright bit
            index |= c.R > 64 ? 4 : 0; // Red bit
            index |= c.G > 64 ? 2 : 0; // Green bit
            index |= c.B > 64 ? 1 : 0; // Blue bit
            return (ConsoleColor) index;
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
    }
}