using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace RCC
{
    /// <summary>
    /// Contains main settings for Ryzom Console Client
    /// </summary>

    public static class Settings
    {
        // Logging
        public enum FilterModeEnum { Blacklist, Whitelist }
        public static Regex ChatFilter = null;
        public static Regex DebugFilter = null;
        public static FilterModeEnum FilterMode = FilterModeEnum.Blacklist;

        public static string StartupHost = "shard.ryzom.com:40916";
        public static string StartupPage = "/login/r2_login.php";

        /// <summary>
        /// Load settings from the given INI file
        /// </summary>
        /// <param name="file">File to load</param>
        public static void LoadFile(string file)
        {
            ConsoleIO.WriteLogLine("[Settings] Loading Settings from " + Path.GetFullPath(file));
            // TODO: STUB

        }

        /// <summary>
        /// Write an INI file with default settings
        /// </summary>
        /// <param name="settingsfile">File to (over)write</param>
        public static void WriteDefaultSettings(string settingsfile)
        {
            // TODO: STUB
        }
    }
}
