using System;
using System.Collections.Generic;
using RCC.Commands.Internal;
using RCC.Helper;

namespace RCC.Commands
{
    public class PlayedTime : CommandBase
    {
        public override string CmdName => "playedTime";

        public override string CmdUsage => "";

        public override string CmdDesc => "Display character played time";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var msg = "You played with this character for %time.";

            var secondsHumanReadable =
                TimeSpan.FromSeconds(handler.GetNetworkManager().CharPlayedTime).ToHumanReadableString();

            msg = msg.Replace("%time", secondsHumanReadable);

            handler.GetLogger().Info(msg);

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}