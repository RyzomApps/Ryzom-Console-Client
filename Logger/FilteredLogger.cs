// This code is a modified version of a file from the 'Minecraft Console Client'
// <https://github.com/ORelio/Minecraft-Console-Client>,
// which is released under CDDL-1.0 License.
// <http://opensource.org/licenses/CDDL-1.0>
// Original Copyright 2021 by ORelio and Contributers

using System.Text.RegularExpressions;
using RCC.Helper;
using static RCC.Config.ClientConfig;

namespace RCC.Logger
{
    public class FilteredLogger : LoggerBase
    {
        protected bool ShouldDisplay(FilterChannel channel, string msg)
        {
            Regex regexToUse = null;
            // Convert to bool for XOR later. Whitelist = 0, Blacklist = 1
            var filterMode = FilterMode == FilterModeEnum.Blacklist;
            switch (channel)
            {
                case FilterChannel.Chat:
                    regexToUse = ChatFilter;
                    break;
                case FilterChannel.Debug:
                    regexToUse = DebugFilter;
                    break;
            }

            if (regexToUse != null)
            {
                // IsMatch and white/blacklist result can be represented using XOR
                // e.g.  matched(true) ^ blacklist(true) => shouldn't log(false)
                return regexToUse.IsMatch(msg) ^ filterMode;
            }

            return true;
        }

        public override void Debug(string msg)
        {
            if (!DebugEnabled) return;

            if (ShouldDisplay(FilterChannel.Debug, msg))
            {
                Log("§8[DEBUG] " + msg);
            }

            // Don't write debug lines here as it could cause a stack overflow
        }

        public override void Info(string msg)
        {
            if (InfoEnabled)
                ConsoleIO.WriteLogLine(msg);
        }

        public override void Warn(string msg)
        {
            if (WarnEnabled)
                Log("§6[WARN] " + msg);
        }

        public override void Error(string msg)
        {
            if (ErrorEnabled)
                Log("§c[ERROR] " + msg);
        }

        public override void Chat(string msg)
        {
            if (ChatEnabled)
            {
                if (ShouldDisplay(FilterChannel.Chat, msg))
                {
                    Log(msg);
                }
                else Debug("[Logger] One Chat message filtered: " + msg);
            }
        }

        protected enum FilterChannel
        {
            Debug,
            Chat
        }
    }
}