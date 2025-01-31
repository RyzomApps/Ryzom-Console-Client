///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Minecraft Console Client'
// https://github.com/ORelio/Minecraft-Console-Client
// which is released under CDDL-1.0 License
// http://opensource.org/licenses/CDDL-1.0
// Copyright 2021 ORelio and Contributers
///////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Client.Helper;
using static Client.Config.ClientConfig;

namespace Client.Logger
{
    public class FilteredLogger : LoggerBase
    {
        private readonly object _loggerLock = new object();

        protected bool ShouldDisplay(FilterChannel channel, string msg)
        {
            // Determine the current filter mode (positivelist = 0, negativelist = 1)
            bool isNegativelistMode = FilterMode == FilterModeEnum.NegativeList;

            // Select the appropriate regex list based on the channel
            List<Regex> regexesToUse = channel switch
            {
                FilterChannel.Chat => ChatFilters,
                FilterChannel.Debug => [DebugFilter],
                _ => null
            };

            // If there are no regex patterns, return the opposite of the filter mode
            if (regexesToUse == null) return !isNegativelistMode;

            // Check if any regex matches the message
            bool isMatch = regexesToUse.Any(regex => regex.IsMatch(msg));

            // Return the XOR of isMatch and the filter mode
            return isMatch ^ isNegativelistMode;
        }

        public override void Debug(string msg)
        {
            if (!DebugEnabled) return;

            if (ShouldDisplay(FilterChannel.Debug, msg))
            {
                lock (_loggerLock)
                {
                    Log($"\u00a78[DEBUG] {msg}");
                }
            }

            // Don't write debug lines here as it could cause a stack overflow
        }

        public override void Info(string msg)
        {
            if (!InfoEnabled)
                return;

            lock (_loggerLock)
            {
                ConsoleIO.WriteLogLine(msg);
            }
        }

        public override void Warn(string msg)
        {
            if (!WarnEnabled)
                return;

            lock (_loggerLock)
            {
                Log($"\u00a76[WARN] {msg}");
            }
        }

        public override void Error(string msg)
        {
            if (!ErrorEnabled)
                return;

            lock (_loggerLock)
            {
                Log($"\u00a7c[ERROR] {msg}");
            }
        }

        public override void Chat(string msg)
        {
            if (!ChatEnabled)
                return;

            if (ShouldDisplay(FilterChannel.Chat, msg))
            {
                msg = API.Chat.ChatColor.ReplaceRyzomColors(msg);

                lock (_loggerLock)
                {
                    Log(msg);
                }
            }
            else Debug($"[Logger] One Chat message filtered: {msg}");
        }

        protected enum FilterChannel
        {
            Debug,
            Chat
        }
    }
}