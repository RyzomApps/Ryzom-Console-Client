using System;
using System.Collections.Generic;
using API;
using API.Commands;
using API.Helper;

namespace Client.Commands
{
    /// <summary>
    /// Display character played time
    /// </summary>
    public class PlayedTime : CommandBase
    {
        public override string CmdName => "playedTime";

        public override string CmdUsage => "";

        public override string CmdDesc => "Display the characters overall time played";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            responseMsg = "You played with this character for %time.";

            var secondsHumanReadable = TimeSpan.FromSeconds(ryzomClient.GetNetworkManager().CharPlayedTime).ToHumanReadableString();

            responseMsg = responseMsg.Replace("%time", secondsHumanReadable);

            return true;
        }
    }
}