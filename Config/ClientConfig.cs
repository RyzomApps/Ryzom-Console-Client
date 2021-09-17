// This code is a modified version of a file from the 'Minecraft Console Client'
// <https://github.com/ORelio/Minecraft-Console-Client>,
// which is released under CDDL-1.0 License.
// <http://opensource.org/licenses/CDDL-1.0>
// Original Copyright 2021 by ORelio and Contributers

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using RCC.Helper;

namespace RCC.Config
{
    /// <summary>
    ///     Contains main settings for Ryzom Console Client
    /// </summary>
    public static class ClientConfig
    {
        /// <summary>
        /// Logging
        /// </summary>
        public enum FilterModeEnum
        {
            Blacklist,
            Whitelist
        }

        // === RYZOM STUFF
        public static string StartupHost = "shard.ryzom.com:40916";
        public static string StartupPage = "/login/r2_login.php";
        public static string LanguageCode = "en";

        // ryzom_live -> atys
        // ryzom_dev -> server error: You don't have sufficient privilege to connect to YUBO now, please try later (3014)
        // ryzom_test -> server error: Your account needs a proper subscription to connect (3011)
        public static string ApplicationServer = "ryzom_live";

        public static string Username = "";
        public static string Password = "";
        public static int SelectCharacter = 0;

        public static string UserSheet;

        //public static int SBSPortOffset = 1000;

        // === NON RYZOM STUFF

        // Custom app variables 
        private static readonly Dictionary<string, object> AppVars = new Dictionary<string, object>();
        public static Regex ChatFilter = null;
        public static Regex DebugFilter = null;
        public static FilterModeEnum FilterMode = FilterModeEnum.Blacklist;
        public static bool LogToFile = false;
        public static string LogFile = "console-log.txt";
        public static bool PrependTimestamp = false;

        //Other Settings
        public static char InternalCmdChar = '/';

        /// <summary>
        ///     Load settings from the given INI file
        /// </summary>
        /// <param name="file">File to load</param>
        public static void LoadFile(string file)
        {
            ConsoleIO.WriteLogLine("[Settings] Loading Settings from " + Path.GetFullPath(file));
            if (!File.Exists(file)) return;

            try
            {
                var lines = File.ReadAllLines(file);

                foreach (var lineRaw in lines)
                {
                    var line = lineRaw.Split('#')[0].Split("//")[0].Trim();

                    if (line.Length <= 0) continue;

                    if (line[0] == '[' && line[^1] == ']')
                    {
                        // Sections are not supported atm
                    }
                    else
                    {
                        var argName = line.Split('=')[0];
                        if (line.Length <= argName.Length + 1) continue;

                        var argValue = line.Substring(argName.Length + 1);
                        LoadSingleSetting(argName, argValue);
                    }
                }
            }
            catch (IOException)
            {
            }
        }

        /// <summary>
        ///     Write an INI file with default settings
        /// </summary>
        /// <param name="settingsfile">File to (over)write</param>
        public static void WriteDefaultSettings(string settingsfile)
        {
            // Load embedded default config and adjust line break for the current operating system
            string settingsContents = string.Join(Environment.NewLine,
                Resources.client.Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.None));

            // Write configuration file with current version number
            File.WriteAllText(settingsfile,
                "# Ryzom Console Client v"
                + Program.Version
                + Environment.NewLine
                + settingsContents, Encoding.UTF8);
        }

        /// <summary>
        ///     Load a single setting from INI file or command-line argument
        /// </summary>
        /// <param name="argName">Setting name</param>
        /// <param name="argValue">Setting value</param>
        /// <returns>TRUE if setting was valid</returns>
        private static bool LoadSingleSetting(string argName, string argValue)
        {
            argName = argName.Trim();
            argValue = argValue.Trim();

            argValue = CleanUpArgument(argValue);

            switch (argName.ToLower())
            {
                case "startuphost":
                    StartupHost = argValue;
                    return true;

                case "startuppage":
                    StartupPage = argValue;
                    return true;

                case "languagecode":
                    LanguageCode = argValue;
                    return true;

                case "application":
                    argValue = argValue.Replace("{", "").Replace("}", "").Trim();
                    var argValueSplit = argValue.Split();

                    ApplicationServer = CleanUpArgument(argValueSplit[0], true);
                    return true;

                case "username":
                    Username = argValue;
                    return true;

                case "password":
                    Password = argValue;
                    return true;

                case "selectcharacter":
                    SelectCharacter = int.Parse(argValue);
                    return true;

                default:
                    ConsoleIO.WriteLineFormatted("§cCould not parse setting " + argName + " with value '" + argValue +
                                                 "'.");
                    return false;
            }
        }

        private static string CleanUpArgument(string argValue, bool cleanEndComma = false)
        {
            if (argValue.EndsWith(";"))
            {
                argValue = argValue.Substring(0, argValue.Length - 1).Trim();
            }

            if (cleanEndComma && argValue.EndsWith(","))
            {
                argValue = argValue.Substring(0, argValue.Length - 1).Trim();
            }

            if (argValue.StartsWith("\"") && argValue.EndsWith("\"") ||
                argValue.StartsWith("'") && argValue.EndsWith("'"))
            {
                argValue = argValue.Substring(1, argValue.Length - 2);
            }

            return argValue;
        }
    }
}