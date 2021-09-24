///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Minecraft Console Client'
// https://github.com/ORelio/Minecraft-Console-Client
// which is released under CDDL-1.0 License
// http://opensource.org/licenses/CDDL-1.0
// Copyright 2021 ORelio and Contributers
///////////////////////////////////////////////////////////////////

using RCC.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace RCC.Config
{
    /// <summary>
    ///     Contains main settings for Ryzom Console Client
    /// </summary>
    public static class ClientConfig
    {
        /// <summary>
        ///     Logging
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
        public static int SelectCharacter = -1;

        public static string UserSheet;

        //public static int SBSPortOffset = 1000;

        // === NON RYZOM STUFF

        public static bool DebugEnabled = false;

        // Custom app variables 
        private static readonly Dictionary<string, object> AppVars = new Dictionary<string, object>();
        public static Regex ChatFilter = null;
        public static Regex DebugFilter = null;
        public static FilterModeEnum FilterMode = FilterModeEnum.Blacklist;
        public static bool LogToFile = false;
        public static string LogFile = "console-log.txt";
        public static bool PrependTimestamp = false;
        public static string OnlinePlayersApi = "";

        //Other Settings
        public static char InternalCmdChar = '/';

        // Automata
        public static bool OnlinePlayersLoggerEnabled = false;
        public static bool AutoJoinTeamEnabled = false;
        public static bool ImpulseDebuggerEnabled = false;

        // Read : "ID", "R G B A MODE [FX]"
        public static Dictionary<string, string> SystemInfoColors = new Dictionary<string, string>
        {
            // OLD STUFF Here for compatibility
            {"RG", "0   0   0   255 normal"}, // Black to see when there is an error
            //{ "BC", "0   0   0   255 normal" }, // Black to see when there is an error
            {"JA", "0   0   0   255 normal"}, // Black to see when there is an error
            {"BL", "0   0   0   255 normal"}, // Black to see when there is an error
            {"VE", "0   0   0   255 normal"}, // Black to see when there is an error
            {"VI", "0   0   0   255 normal"}, // Black to see when there is an error

            // NEW System Info Categories
            {"SYS", "255 255 255 255 normal"}, // Default system messages
            {"BC", "255 255 255 255 centeraround"}, // Broadcast messages
            {
                "TAGBC", "255 255 255 255 centeraround"
            }, // Taged broadcast messages : color should remain white as some word are tagged
            {"XP", "255 255 64  255 over"}, // XP Gain
            {"SP", "255 255 64  255 over"}, // SP Gain
            {"TTL", "255 255 64  255 over"}, // Title
            {"TSK", "255 255 255 255 over"}, // Task
            {"ZON", "255 255 255 255 center"}, // Zone
            {"DG", "255 0   0   255 normal"}, // Damage to me
            {"DMG", "255 0   0   255 normal"}, // Damage to me
            {"DGP", "200 0   0   255 normal"}, // Damage to me from player
            {"DGM", "255 128 64  255 normal"}, // Damage from me
            {"MIS", "150 150 150 255 normal"}, // The opponent misses
            {"MISM", "255 255 255 255 normal"}, // I miss
            {"ITM", "0   200 0   255 over"}, // Item
            {"ITMO", "170 170 255 255 overonly"}, // Item other in group
            {"ITMF", "220 0   220 255 over"}, // Item failed
            {"SPL", "50  50  250 255 normal"}, // Spell to me
            {"SPLM", "50  150 250 255 normal"}, // Spell from me
            {"EMT", "255 150 150 255 normal"}, // Emote
            {"MTD", "255 255 0   255 over"}, // Message Of The Day
            {"FORLD", "64  255 64  255 overonly"}, // Forage Locate Deposit
            {"CHK", "255 120 60  255 center"}, // Tous ce qui ne remplit pas une condition
            {
                "CHKCB", "255 255  0  255 center"
            }, // Tous ce qui ne remplit pas une condition en combat (trop loin, cible invalide, pas assez de mana, etc.)
            {"PVPTM", "255 120 60  255 overonly"}, // PVP timer
            {"THM", "255 255 64  255 over misc_levelup.ps"}, // Thema finished
            {"AMB", "255 255 64  255 center"}, // Ambiance
            {"ISE", "192 208 255 255 normal"}, // Item special effect
            {
                "ISE2", "192 208 255 255 center"
            }, // Item special effect with center text (for effects without flying text)
            {"OSM", "128 160 255 255 center"}, // Outpost state message
            {"AROUND", "255 255 0 255 around"}, // Only in around channel
            {"R2_INVITE", "0 255 0 255 around"} // Ring invitation
        };

        //private static string configPattern = "^(?<parameter>\\w*)[ ]*=[ ]*({(?<value1>\\w*[^={}]*)}|['\"](?<value2>\\w*[^=]*)['\"]|(?<value3>\\w*[^=\"'\\n]*)).*(;|#|\\/\\/)+.*$";

        /// <summary>
        ///     Load settings from the given INI file
        /// </summary>
        /// <param name="file">File to load</param>
        public static void LoadFile(string file)
        {
            ConsoleIO.WriteLogLine($"Loading settings from {Path.GetFullPath(file)}");
            if (!File.Exists(file)) return;

            try
            {
                //var text = File.ReadAllText(file);
                //
                //var match = Regex.Match(text, configPattern, RegexOptions.Multiline & RegexOptions.ECMAScript);
                //
                //while (match.Success)
                //{
                //    Console.WriteLine("'{0}' found in the source code at position {1}.", match.Value, match.Index);
                //
                //    if (match.Groups.ContainsKey("parameter"))
                //    {
                //        if (match.Groups.ContainsKey("value1"))
                //        {
                //            LoadSingleSetting(match.Groups["parameter"].Value, match.Groups["value1"].Value);
                //        }
                //        else if (match.Groups.ContainsKey("value2"))
                //        {
                //            LoadSingleSetting(match.Groups["parameter"].Value, match.Groups["value2"].Value);
                //        }
                //        else if (match.Groups.ContainsKey("value3"))
                //        {
                //            LoadSingleSetting(match.Groups["parameter"].Value, match.Groups["value3"].Value);
                //        }
                //    }
                //
                //    match = match.NextMatch();
                //}

                var lines = File.ReadAllLines(file);

                foreach (var lineRaw in lines)
                {
                    var line = lineRaw.Split('#')[0]/*Split("//")[0]*/.Trim();

                    #region comments after second quotes - TODO Replace by regex
                    var firstQuotationMark = line.IndexOf('"');

                    if (firstQuotationMark > 0)
                    {
                        var secondQuotationMark = line.IndexOf('"', firstQuotationMark + 1);

                        if (secondQuotationMark > 0)
                        {
                            var commentMark = line.IndexOf('/', secondQuotationMark + 1);

                            if (secondQuotationMark < commentMark)
                            {
                                line = line.Substring(0, commentMark);
                            }
                        }
                    }
                    #endregion

                    if (line.Length <= 0) continue;

                    if (line[0] == '[' && line[^1] == ']')
                    {
                        // Sections are not supported atm
                    }
                    else
                    {
                        var argName = line.Split('=')[0];
                        if (line.Length <= argName.Length + 1) continue;

                        var argValue = line[(argName.Length + 1)..];
                        LoadSingleSetting(argName, argValue);
                    }
                }
            }
            catch (IOException e)
            {
                ConsoleIO.WriteLineFormatted("§cError loading Settings: " + e.Message);
            }
        }

        /// <summary>
        ///     Write an INI file with default settings
        /// </summary>
        /// <param name="settingsfile">File to (over)write</param>
        public static void WriteDefaultSettings(string settingsfile)
        {
            // Load embedded default config and adjust line break for the current operating system
            var settingsContents = string.Join(Environment.NewLine,
                Resources.client.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None));

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
        private static void LoadSingleSetting(string argName, string argValue)
        {
            argName = argName.Trim();
            argValue = argValue.Trim();

            argValue = CleanUpArgument(argValue);

            switch (argName.ToLower())
            {
                case "startuphost":
                    StartupHost = argValue;
                    return;

                case "startuppage":
                    StartupPage = argValue;
                    return;

                case "languagecode":
                    LanguageCode = argValue;
                    return;

                case "application":
                    argValue = argValue.Replace("{", "").Replace("}", "").Trim();
                    // config application string <- todo this is not good atm
                    var argValueSplit = argValue.Split();

                    ApplicationServer = CleanUpArgument(argValueSplit[0], true);
                    return;

                case "username":
                    Username = argValue;
                    return;

                case "password":
                    Password = argValue;
                    return;

                case "selectcharacter":
                    SelectCharacter = int.Parse(argValue);
                    return;

                case "onlineplayersapi":
                    OnlinePlayersApi = argValue;
                    return;

                case "onlineplayerslogger":
                    OnlinePlayersLoggerEnabled = bool.Parse(argValue);
                    return;

                case "autojointeam":
                    AutoJoinTeamEnabled = bool.Parse(argValue);
                    return;

                case "impulsedebugger":
                    ImpulseDebuggerEnabled = bool.Parse(argValue);
                    return;

                case "debug":
                    DebugEnabled = bool.Parse(argValue);
                    return;

                default:
                    RyzomClient.GetInstance().GetLogger().Warn($"Could not parse setting {argName} with value '{argValue}'.");
                    return;
            }
        }

        private static string CleanUpArgument(string argValue, bool cleanEndComma = false)
        {
            if (argValue.EndsWith(";"))
            {
                argValue = argValue[..^1].Trim();
            }

            if (cleanEndComma && argValue.EndsWith(","))
            {
                argValue = argValue[..^1].Trim();
            }

            if (argValue.StartsWith("\"") && argValue.EndsWith("\"") ||
                argValue.StartsWith("'") && argValue.EndsWith("'"))
            {
                argValue = argValue[1..^1];
            }

            return argValue;
        }
    }
}