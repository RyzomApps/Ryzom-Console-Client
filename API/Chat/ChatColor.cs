
// ReSharper disable InconsistentNaming

using System;
using System.Drawing;

namespace API.Chat
{
    /// <summary>
    /// All supported color values for chat
    /// </summary>
    public class ChatColor
    {
        /// <summary>
        /// Represents black
        /// </summary>
        public static string BLACK = "§0";

        /// <summary>
        /// Represents dark blue
        /// </summary>
        public static string DARK_BLUE = "§1";

        /// <summary>
        /// Represents dark green
        /// </summary>
        public static string DARK_GREEN = "§2";

        /// <summary>
        /// Represents dark blue (aqua)
        /// </summary>
        public static string DARK_AQUA = "§3";

        /// <summary>
        /// Represents dark red
        /// </summary>
        public static string DARK_RED = "§4";

        /// <summary>
        /// Represents dark purple
        /// </summary>
        public static string DARK_PURPLE = "§5";

        /// <summary>
        /// Represents gold
        /// </summary>
        public static string GOLD = "§6";

        /// <summary>
        /// Represents gray
        /// </summary>
        public static string GRAY = "§7";

        /// <summary>
        /// Represents dark gray
        /// </summary>
        public static string DARK_GRAY = "§8";

        /// <summary>
        /// Represents blue
        /// </summary>
        public static string BLUE = "§9";

        /// <summary>
        /// Represents green
        /// </summary>
        public static string GREEN = "§a";

        /// <summary>
        /// Represents aqua
        /// </summary>
        public static string AQUA = "§b";

        /// <summary>
        /// Represents red
        /// </summary>
        public static string RED = "§c";

        /// <summary>
        /// Represents light purple
        /// </summary>
        public static string LIGHT_PURPLE = "§d";

        /// <summary>
        /// Represents yellow
        /// </summary>
        public static string YELLOW = "§e";

        /// <summary>
        /// Represents white
        /// </summary>
        public static string WHITE = "§f";

        /// <summary>
        /// Represents magical characters that change around randomly
        /// </summary>
        public static string MAGIC = "§k";

        /// <summary>
        /// Makes the text bold.
        /// </summary>
        public static string BOLD = "§l";

        /// <summary>
        /// Makes a line appear through the text.
        /// </summary>
        public static string STRIKETHROUGH = "§m";

        /// <summary>
        /// Makes the text appear underlined.
        /// </summary>
        public static string UNDERLINE = "§n";

        /// <summary>
        /// Makes the text italic.
        /// </summary>
        public static string ITALIC = "§o";

        /// <summary>
        /// Resets all previous chat colors or formats.
        /// </summary>
        public static string RESET = "§r";

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
        /// returns the console color code for a minecraft channel type
        /// </summary>
        public static string GetMinecraftColorForChatGroupType(ChatGroupType mode)
        {
            var color = mode switch
            {
                ChatGroupType.DynChat => AQUA,
                ChatGroupType.Shout => RED,
                ChatGroupType.Team => BLUE,
                ChatGroupType.Guild => GREEN,
                ChatGroupType.Civilization => LIGHT_PURPLE,
                ChatGroupType.Territory => LIGHT_PURPLE,
                ChatGroupType.Universe => GOLD,
                ChatGroupType.Region => GRAY,
                ChatGroupType.Tell => WHITE,
                _ => WHITE
            };

            return color;
        }

        /// <summary>
        /// Remove color codes (e.g. "§c") from a text message received from the server
        /// </summary>
        public static string GetVerbatim(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var idx = 0;
            var data = new char[text.Length];

            for (var i = 0; i < text.Length; i++)
                if (text[i] != '§')
                    data[idx++] = text[i];
                else
                    i++;

            return new string(data, 0, idx);
        }
    }
}
