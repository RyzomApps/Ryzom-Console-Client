using System;
using System.Collections.Generic;
using API;
using API.Commands;
using API.Helper;
using Client.Helper;

namespace Client.Commands
{
    public class PlayedTime : CommandBase
    {
        public override string CmdName => "playedTime";

        public override string CmdUsage => "";

        public override string CmdDesc => "Display character played time";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var msg = "You played with this character for %time.";

            var secondsHumanReadable = TimeSpan.FromSeconds(ryzomClient.GetNetworkManager().CharPlayedTime).ToHumanReadableString();

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