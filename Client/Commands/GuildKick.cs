using API;
using API.Commands;
using Client.Stream;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class GuildKick : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Guild;
        public override string CmdName => "GuildKick";

        public override string CmdUsage => "<playerName>";

        public override string CmdDesc =>
            "Kick a guild member (requires officer rank or higher over the member).\n"
            + "NOTE: the official client sends the member's index plus the guild counter; this command sends the player name, which the server may reject. Left in for compatibility - needs rework.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 1)
            {
                responseMsg = "Please specify a parameter.";
                return false;
            }

            const string msgName = "GUILD:KICK_MEMBER";

            // TODO GUKick right arguments
            var out2 = new BitMemoryStream();

            if (!ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                responseMsg = $"Unknown message named '{msgName}'.";
                return false;
            }

            var buf = args[0];
            out2.Serial(ref buf);
            ryzomClient.GetNetworkManager().Push(out2);

            return true;
        }
    }
}
